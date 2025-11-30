using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class Monster : MonoBehaviour
{
    [SerializeField] private Tilemap[] obstacles;
    [SerializeField] private Grid grid;
    [SerializeField] private float speed = 0.5f; 

    private List<Vector2Int> _path;
    private Vector2 _playerPosition;
    private Vector2 _monsterPosition;

    private void Start()
    {
        _playerPosition = Player.Instance.transform.position;
        _monsterPosition = transform.position;
        _path = AStarManager.Instance.FindPath(grid, obstacles, _monsterPosition, _playerPosition);

        if (_path != null && _path.Count > 0)
        {
            StartCoroutine(MoveToNode());
        }
    }
    private IEnumerator MoveToNode()
    {
        foreach (Vector2Int node in _path)
        {
            Vector3 worldPosition = grid.CellToWorld(new Vector3Int(node.x, node.y, 0));
            worldPosition.x += grid.cellSize.x / 2;
            worldPosition.y += grid.cellSize.y / 2;
            transform.position = worldPosition;
            yield return new WaitForSeconds(1/speed);
        }
    }
}