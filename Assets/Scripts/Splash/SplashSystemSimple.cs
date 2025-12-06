using UnityEngine;
using System.Collections;

public class SplashSystemSimple : MonoBehaviour
{
    public static SplashSystemSimple Instance;

    [Header("Visual splash prefab (optional)")]
    public GameObject fxSplashPrefab;      // 你的 FX_SplashUnit.prefab (用于视觉粒子)
    public int spawnCount = 10;
    public float spreadRadius = 1.2f;
    public float lifetime = 1.0f;
    public float visualScale = 1.0f;

    [Header("Paint options")]
    public Sprite paintSprite;             // 默认用于绘制到 mask 的 sprite（如果 fxSplashPrefab 自带 sprite 可不填）
    public float paintSpriteScale = 1.0f;  // per-splash scale

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Call this on explosion
    /// </summary>
    public void StartExplosion(Vector2 worldCenter)
    {
        // spawn some visual particles and schedule painting when they "land"
        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            if (dir == Vector2.zero) dir = Vector2.up;
            float r = Random.Range(0f, spreadRadius);
            Vector2 pos = worldCenter + dir * r;

            // spawn visual
            if (fxSplashPrefab)
            {
                GameObject g = Instantiate(fxSplashPrefab, pos, Quaternion.identity);
                g.transform.localScale = Vector3.one * (visualScale * Random.Range(0.8f, 1.2f));
                Destroy(g, lifetime); // cheap way; for production use pooling
            }

            // paint directly (we can introduce small random delay if desired)
            Sprite s = paintSprite;
            // if prefab has a sprite, prefer that
            if (!s && fxSplashPrefab)
            {
                var sr = fxSplashPrefab.GetComponent<SpriteRenderer>();
                if (sr) s = sr.sprite;
            }
            // call mask painter
            if (MaskPainter.Instance && s)
            {
                MaskPainter.Instance.PaintSplash(pos, s, paintSpriteScale * Random.Range(0.8f, 1.3f));
            }
        }
    }
}
