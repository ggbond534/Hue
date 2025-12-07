using UnityEngine;

namespace Splash
{
    [ExecuteAlways]
    public class MaskPainter : MonoBehaviour
    {
        public static MaskPainter Instance;
        private static readonly int MaskTexID = Shader.PropertyToID("_MaskTex");
        private static readonly int MaskWorldBoundsID = Shader.PropertyToID("_MaskWorldBounds");

        [Header("Mask texture")]
        public int textureSize = 1024;
        public TextureFormat textureFormat = TextureFormat.RGBA32;
        public bool linear;

        [Header("World bounds covered by mask texture")]
        public Vector2 worldMin = new Vector2(-16, -9);
        public Vector2 worldMax = new Vector2(16, 9);

        [Header("Material to update")]
        public Material targetMaterial;

        [Header("Paint settings")]
        [Range(0.1f, 4f)]
        public float globalSpriteScale = 1.0f;
        public float paintStrength = 1.0f;
        public bool additive;

        private Texture2D _maskTex;
        private Color32[] _clearColors;

        private void Awake()
        {
            Instance = this;
            var tilemap = FindObjectOfType<UnityEngine.Tilemaps.Tilemap>();
            if (tilemap != null)
            {
                Bounds bounds = tilemap.localBounds;
                worldMin = tilemap.transform.TransformPoint(bounds.min);
                worldMax = tilemap.transform.TransformPoint(bounds.max);
            }
            InitTexture();
            UpdateMaterialProperties();
        }
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                InitTexture();
                UpdateMaterialProperties();
            }
        }
        private void InitTexture()
        {
            if (_maskTex && _maskTex.width == textureSize && _maskTex.height == textureSize) return;
            if (_maskTex)
            {
#if UNITY_EDITOR
                DestroyImmediate(_maskTex);
#else
            Destroy(maskTex);
#endif
                _maskTex = null;
            }
            try
            {
                _maskTex = new Texture2D(textureSize, textureSize, textureFormat, false, linear);
            }
            catch
            {
                _maskTex = new Texture2D(textureSize, textureSize, textureFormat, false);
            }
            _maskTex.filterMode = FilterMode.Point;
            _maskTex.wrapMode = TextureWrapMode.Clamp;
            _clearColors = new Color32[textureSize * textureSize];
            for (int i = 0; i < _clearColors.Length; i++) _clearColors[i] = new Color32(0, 0, 0, 0);
            _maskTex.SetPixels32(_clearColors);
            _maskTex.Apply(false);
            UpdateMaterialProperties();
        }
        public void ClearMaskImmediate()
        {
            if (_maskTex == null) InitTexture();
            if (_maskTex == null) return;

            _maskTex.SetPixels32(_clearColors);
            _maskTex.Apply(false);
            ApplyToMaterial();
        }
        private void UpdateMaterialProperties()
        {
            if (!targetMaterial || !_maskTex) return;

            targetMaterial.SetTexture(MaskTexID, _maskTex);
            Vector4 bounds = new Vector4(worldMin.x, worldMin.y, worldMax.x, worldMax.y);
            targetMaterial.SetVector(MaskWorldBoundsID, bounds);
        }
        private void ApplyToMaterial()
        {
            if (targetMaterial && _maskTex)
            {
                targetMaterial.SetTexture(MaskTexID, _maskTex);
            }
        }
        public void PaintSplash(Vector2 worldPos, Sprite sprite, float scale = 1.0f)
        {
            if (!sprite) return;
            if (!_maskTex) InitTexture();
            if (!_maskTex) return;
            float u = Mathf.InverseLerp(worldMin.x, worldMax.x, worldPos.x);
            float v = Mathf.InverseLerp(worldMin.y, worldMax.y, worldPos.y);
            if (u < 0f || u > 1f || v < 0f || v > 1f)
            {
                return;
            }
            int centerX = Mathf.RoundToInt(u * (_maskTex.width - 1));
            int centerY = Mathf.RoundToInt(v * (_maskTex.height - 1));
            Texture2D srcTex = sprite.texture;
            Rect rect = sprite.textureRect;
            bool srcReadable = true;
            try
            {

                srcTex.GetPixel(0, 0);
            }
            catch (UnityException)
            {
                srcReadable = false;
            }
            if (!srcReadable) return;
            int srcW = Mathf.Max(1, (int)rect.width);
            int srcH = Mathf.Max(1, (int)rect.height);
            float spriteWorldWidth = srcW / sprite.pixelsPerUnit;
            float spriteWorldHeight = srcH / sprite.pixelsPerUnit;
            float finalWorldW = spriteWorldWidth * scale * globalSpriteScale;
            float finalWorldH = spriteWorldHeight * scale * globalSpriteScale;
            float worldSpanX = Mathf.Max(0.0001f, worldMax.x - worldMin.x);
            float worldSpanY = Mathf.Max(0.0001f, worldMax.y - worldMin.y);
            int destW = Mathf.Max(1, Mathf.RoundToInt((finalWorldW / worldSpanX) * _maskTex.width));
            int destH = Mathf.Max(1, Mathf.RoundToInt((finalWorldH / worldSpanY) * _maskTex.height));
            destW = Mathf.Min(destW, _maskTex.width);
            destH = Mathf.Min(destH, _maskTex.height);
            Color32[] destPixels = _maskTex.GetPixels32();
            int halfW = destW / 2;
            int halfH = destH / 2;

            for (int dx = -halfW; dx <= halfW; dx++)
            {
                int px = centerX + dx;
                if (px < 0 || px >= _maskTex.width) continue;
                float sx = (dx + halfW) / (float)destW;

                for (int dy = -halfH; dy <= halfH; dy++)
                {
                    int py = centerY + dy;
                    if (py < 0 || py >= _maskTex.height) continue;
                    float sy = (dy + halfH) / (float)destH;
                    float srcXf = rect.x + sx * rect.width;
                    float srcYf = rect.y + sy * rect.height;
                    Color srcC;
                    try
                    {
                        srcC = srcTex.GetPixelBilinear(srcXf / srcTex.width, srcYf / srcTex.height);
                    }
                    catch (UnityException)
                    {
                        continue;
                    }

                    float srcA = srcC.a * paintStrength;


                    if (srcA < 0.3f)
                        continue;

                    int idx = py * _maskTex.width + px;
                    Color32 existing = destPixels[idx];

                    float existingA = existing.a / 255f;


                    float newA = additive ? Mathf.Max(existingA, srcA) : srcA;

                    destPixels[idx].a = (byte)(newA * 255f);
                    destPixels[idx].r = 255;
                    destPixels[idx].g = 255;
                    destPixels[idx].b = 255;

                }
            }
            _maskTex.SetPixels32(destPixels);
            _maskTex.Apply(false);
            ApplyToMaterial();
        }
    }
}
