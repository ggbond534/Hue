using UnityEngine;
using UnityEngine.Tilemaps;

namespace Map
{
    public class GameObject : MonoBehaviour
    {
        Tilemap _map;
        public void Start()
        {
            _map = GetComponent<Tilemap>();
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.CompareTag("Player"))
            {
                _map.color = new Color(1, 1, 1, 0.99f);
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            _map.color = new Color(1, 1, 1, 1);
        }
    }
}
