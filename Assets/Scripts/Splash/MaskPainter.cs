using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// MaskPainter (singleton)
/// - Maintains a Texture2D mask (RGBA) where alpha = 1 means "hole" (top tilemap hidden)
/// - Exposes PaintSplash(worldPos, sprite, scale) to draw a sprite's alpha into mask at world position
/// - You must assign the material on your top Tilemap (Material must use MaskCutout shader below)
/// - Set worldBounds to the rectangular world-space area that the mask texture covers (xmin,ymin)-(xmax,ymax)
/// </summary>
[ExecuteAlways]
public class MaskPainter : MonoBehaviour
{
    public static MaskPainter Instance;
    private static readonly int MaskTex = Shader.PropertyToID("_MaskTex");
    private static readonly int MaskWorldBounds = Shader.PropertyToID("_MaskWorldBounds");

    [Header("Mask texture")]
    public int textureSize = 1024;                // mask resolution (square)
    public TextureFormat textureFormat = TextureFormat.RGBA32;
    public bool linear = false;

    [Header("World bounds covered by mask texture")]
    public Vector2 worldMin = new Vector2(-16, -9);
    public Vector2 worldMax = new Vector2(16, 9);

    [Header("Material to update")]
    public Material targetMaterial;               // material using MaskCutout.shader

    [Header("Paint settings")]
    [Range(0.1f, 4f)]
    public float globalSpriteScale = 1.0f;        // multiplier for painted sprite size
    public float paintStrength = 1.0f;            // how strong alpha is written (0..1)
    public bool additive = false;                 // true: max alpha, false: overwrite with paintStrength

    private Texture2D maskTex;
    private Color32[] clearColors;
    private bool initialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) DestroyImmediate(gameObject);
        Instance = this;
        InitTexture();
    }

    private void OnValidate()
    {
        // keep updated in editor
        if (!Application.isPlaying) InitTexture();
        UpdateMaterialProperties();
    }

    private void InitTexture()
    {
        if (maskTex)
        {
            if (maskTex.width == textureSize && maskTex.height == textureSize) return;
        }

        maskTex = new Texture2D(textureSize, textureSize, textureFormat, false, linear);
        maskTex.filterMode = FilterMode.Bilinear;
        maskTex.wrapMode = TextureWrapMode.Clamp;
        clearColors = new Color32[textureSize * textureSize];
        for (int i = 0; i < clearColors.Length; i++) clearColors[i] = new Color32(0,0,0,0);

        ClearMaskImmediate();

        UpdateMaterialProperties();
        initialized = true;
    }

    /// <summary>
    /// Clear mask to fully transparent (no holes)
    /// </summary>
    public void ClearMaskImmediate()
    {
        if (!maskTex) InitTexture();
        maskTex.SetPixels32(clearColors);
        maskTex.Apply();
        ApplyToMaterial();
    }

    private void UpdateMaterialProperties()
    {
        if (targetMaterial && maskTex)
        {
            targetMaterial.SetTexture(MaskTex, maskTex);
            // Set world bounds to map world->uv in shader
            Vector4 bounds = new Vector4(worldMin.x, worldMin.y, worldMax.x, worldMax.y);
            targetMaterial.SetVector(MaskWorldBounds, bounds);
        }
    }

    private void ApplyToMaterial()
    {
        if (targetMaterial && maskTex)
        {
            targetMaterial.SetTexture("_MaskTex", maskTex);
            // ensure shader sees updated texture
        }
    }

    /// <summary>
    /// Paints a sprite's alpha into the mask at worldPos.
    /// sprite: the sprite asset whose texture/rect will be used for alpha.
    /// scale: optional multiplier relative to sprite's native size in world units.
    /// </summary>
    public void PaintSplash(Vector2 worldPos, Sprite sprite, float scale = 1.0f)
    {
        if (!maskTex) InitTexture();
        if (!sprite) return;

        // convert worldPos into pixel coords on maskTex
        float u = Mathf.InverseLerp(worldMin.x, worldMax.x, worldPos.x);
        float v = Mathf.InverseLerp(worldMin.y, worldMax.y, worldPos.y);

        int centerX = Mathf.RoundToInt(u * (maskTex.width - 1));
        int centerY = Mathf.RoundToInt(v * (maskTex.height - 1));

        // get sprite texture and rect (support atlas)
        Texture2D srcTex = sprite.texture;
        Rect rect = sprite.textureRect;
        int srcW = (int)rect.width;
        int srcH = (int)rect.height;

        // Determine how many mask pixels correspond to the sprite's current world size.
        // sprite.pixelsPerUnit gives sprite pixel->world mapping. world size of sprite in units:
        float spriteWorldWidth = (float)srcW / sprite.pixelsPerUnit;
        float spriteWorldHeight = (float)srcH / sprite.pixelsPerUnit;

        // apply requested scales
        float finalWorldW = spriteWorldWidth * scale * globalSpriteScale;
        float finalWorldH = spriteWorldHeight * scale * globalSpriteScale;

        // convert final world size to mask pixels
        float worldSpanX = worldMax.x - worldMin.x;
        float worldSpanY = worldMax.y - worldMin.y;
        int destW = Mathf.Max(1, Mathf.RoundToInt((finalWorldW / worldSpanX) * maskTex.width));
        int destH = Mathf.Max(1, Mathf.RoundToInt((finalWorldH / worldSpanY) * maskTex.height));

        // If dest smaller/larger than src, sample src at appropriate stride.
        // We'll iterate dest pixels and sample corresponding src pixel via bilinear sampling.
        Color32[] destPixels = maskTex.GetPixels32(); // full copy (could be optimized to partial region)
        int halfW = destW / 2;
        int halfH = destH / 2;

        for (int dx = -halfW; dx <= halfW; dx++)
        {
            int px = centerX + dx;
            if (px < 0 || px >= maskTex.width) continue;
            float sx = (dx + halfW) / (float)destW; // 0..1 across sprite

            for (int dy = -halfH; dy <= halfH; dy++)
            {
                int py = centerY + dy;
                if (py < 0 || py >= maskTex.height) continue;
                float sy = (dy + halfH) / (float)destH;

                // sample src texture using sy,sx and sprite.textureRect
                float srcXF = rect.x + sx * rect.width;
                float srcYF = rect.y + sy * rect.height;

                Color srcC = srcTex.GetPixelBilinear(srcXF / srcTex.width, srcYF / srcTex.height);
                float srcA = srcC.a * paintStrength;

                int idx = py * maskTex.width + px;
                Color32 existing = destPixels[idx];

                if (additive)
                {
                    // write max alpha
                    float newA = Mathf.Clamp01(Mathf.Max(existing.a / 255f, srcA));
                    byte aByte = (byte)Mathf.RoundToInt(newA * 255f);
                    destPixels[idx].a = aByte;
                }
                else
                {
                    // overwrite with srcA (composite by max)
                    float newA = Mathf.Clamp01(Mathf.Max(existing.a / 255f, srcA));
                    destPixels[idx].a = (byte)Mathf.RoundToInt(newA * 255f);
                }
            }
        }

        // write back partial? we write full array for simplicity
        maskTex.SetPixels32(destPixels);
        maskTex.Apply(false); // no mipmaps

        ApplyToMaterial();
    }
}
