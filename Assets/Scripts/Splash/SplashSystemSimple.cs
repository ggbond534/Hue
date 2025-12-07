using UnityEngine;

namespace Splash
{
    public class SplashSystemSimple : MonoBehaviour
    {
        public static SplashSystemSimple Instance;

        [Header("Visual splash prefab (optional)")]
        public GameObject fxSplashPrefab;
        public int spawnCount = 10;
        public float spreadRadius = 1.2f;
        public float lifetime = 1.0f;
        public float visualScale = 1.0f;
        
        [Header("Paint options")]
        public Sprite paintSprite;     
        public float paintSpriteScale = 1.0f;
        private void Awake()
        {
            Instance = this;
        }
        public void StartExplosion(Vector2 worldCenter)
        {
            for (int i = 0; i < spawnCount; i++)
            {
                Vector2 dir = Random.insideUnitCircle.normalized;
                if (dir == Vector2.zero) dir = Vector2.up;
                float r = Random.Range(0f, spreadRadius);
                Vector2 pos = worldCenter + dir * r;
                if (fxSplashPrefab)
                {
                    GameObject g = Instantiate(fxSplashPrefab, pos, Quaternion.identity);
                    g.transform.localScale = Vector3.one * (visualScale * Random.Range(0.8f, 1.2f));
                    Destroy(g, lifetime);
                }
                Sprite s = paintSprite;
                if (!s && fxSplashPrefab)
                {
                    var sr = fxSplashPrefab.GetComponent<SpriteRenderer>();
                    if (sr) s = sr.sprite;
                }
                if (MaskPainter.Instance && s)
                {
                    MaskPainter.Instance.PaintSplash(pos, s, paintSpriteScale * Random.Range(0.8f, 1.3f));
                }
            }
        }
    }
}
