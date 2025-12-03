using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
using Character.Player;

public class Monster : MonoBehaviour
{
    [SerializeField] private Tilemap[] obstacles;   //存储障碍物的 Tilemap 数组
    [SerializeField] private Grid grid;     //用于将世界坐标转换为网格坐标或反之的 Grid 组件
    [SerializeField] private float speed = 0.5f;    //怪物移动的速度
    [SerializeField] private float pathRecalculationInterval = 1.0f;  // 路径重计算间隔时间

    private List<Vector2Int> _path;     //从怪物当前位置到玩家位置的路径，存储为 Vector2Int 列表
    private Vector2 _playerPosition;    //存储玩家的世界坐标。
    private Vector2 _monsterPosition;   //存储怪物的世界坐标。
    private int _currentPathIndex = 0;  //当前路径索引

    private void Start()
    {
        _playerPosition = Player.Instance.transform.position;       //获取玩家的初始位置
        _monsterPosition = transform.position;      //获取怪物的初始位置
        RecalculatePath();  // 计算初始路径
        StartCoroutine(RecalculatePathCoroutine());  // 启动路径重计算协程
        StartCoroutine(MoveToNodeCoroutine());  // 启动移动协程
    }

    private IEnumerator RecalculatePathCoroutine()
    {
        while (true)
        {
            RecalculatePath();
            yield return new WaitForSeconds(pathRecalculationInterval);
        }
    }

    private void RecalculatePath()
    {
        _playerPosition = Player.Instance.transform.position;  // 更新玩家位置
        _monsterPosition = transform.position;  // 更新怪物位置
        _path = AStarManager.Instance.FindPath(grid, obstacles, _monsterPosition, _playerPosition);  // 重新计算路径
        _currentPathIndex = 0;  // 重置路径索引
    }

    private IEnumerator MoveToNodeCoroutine()
    {
        while (true)
        {
            if (_path == null || _path.Count == 0)
            {
                yield return null;
                continue;
            }

            if (_currentPathIndex >= _path.Count)
            {
                // 如果已经到达最后一个节点，停止移动
                yield return null;
                continue;
            }

            Vector2Int currentNode = _path[_currentPathIndex];
            Vector3 worldPosition = grid.CellToWorld(new Vector3Int(currentNode.x, currentNode.y, 0));
            worldPosition.x += grid.cellSize.x / 2;
            worldPosition.y += grid.cellSize.y / 2;

            // 使用 MoveTowards 实现平滑移动
            while (Vector3.Distance(transform.position, worldPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, worldPosition, speed * Time.deltaTime);
                yield return null;
            }

            // 检查是否已经到达目标位置
            _currentPathIndex++;
        }
    }
}