using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using UnityEngine;
using UnityEngine.Tilemaps;
public class PriorityQueue<TElement, TPriority>
{
    private readonly List<(TElement Element, TPriority Priority)> _heap;
    private readonly IComparer<TPriority> _comparer;
    private readonly Dictionary<TElement, int> _elementIndices;

    public PriorityQueue() : this(0, null) { }

    public PriorityQueue(int initialCapacity) : this(initialCapacity, null) { }

    public PriorityQueue(IComparer<TPriority> comparer) : this(0, comparer) { }

    public PriorityQueue(int initialCapacity, IComparer<TPriority> comparer)
    {
        _heap = new List<(TElement, TPriority)>(initialCapacity);
        _comparer = comparer ?? Comparer<TPriority>.Default;
        _elementIndices = new Dictionary<TElement, int>();
    }
    public int Count => _heap.Count;

    public void Enqueue(TElement element, TPriority priority)
    {
        if (_elementIndices.TryGetValue(element, out int existingIndex))
        {
            _heap[existingIndex] = (element, priority);
            HeapifyUp(existingIndex);
            HeapifyDown(existingIndex);
        }
        else
        {
            int newIndex = _heap.Count;
            _heap.Add((element, priority));
            _elementIndices[element] = newIndex;
            HeapifyUp(newIndex);
        }
    }
    public TElement Dequeue()
    {
        if (_heap.Count == 0)
            throw new InvalidOperationException("Queue is empty");
        var result = _heap[0].Element;
        _elementIndices.Remove(result);
        int lastIndex = _heap.Count - 1;
        if (lastIndex > 0)
        {
            _heap[0] = _heap[lastIndex];
            _elementIndices[_heap[0].Element] = 0;
        }
        
        _heap.RemoveAt(lastIndex);

        if (_heap.Count > 0)
            HeapifyDown(0);

        return result;
    }

    public TElement Peek()
    {
        if (_heap.Count == 0)
            throw new InvalidOperationException("Queue is empty");

        return _heap[0].Element;
    }

    public void Clear()
    {
        _heap.Clear();
        _elementIndices.Clear();
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            if (_comparer.Compare(_heap[index].Priority, _heap[parentIndex].Priority) >= 0)
                break;

            Swap(index, parentIndex);
            index = parentIndex;
        }
    }

    private void HeapifyDown(int index)
    {
        int count = _heap.Count;
        while (true)
        {
            int leftChild = 2 * index + 1;
            int rightChild = 2 * index + 2;
            int smallest = index;

            if (leftChild < count && _comparer.Compare(_heap[leftChild].Priority, _heap[smallest].Priority) < 0)
                smallest = leftChild;

            if (rightChild < count && _comparer.Compare(_heap[rightChild].Priority, _heap[smallest].Priority) < 0)
                smallest = rightChild;

            if (smallest == index)
                break;

            Swap(index, smallest);
            index = smallest;
        }
    }

    private void Swap(int index1, int index2)
    {
        var temp = _heap[index1];
        _heap[index1] = _heap[index2];
        _heap[index2] = temp;
        _elementIndices[_heap[index1].Element] = index1;
        _elementIndices[_heap[index2].Element] = index2;
    }
}
public class AStarManager:MonoBehaviour
{
    public static AStarManager Instance;
    private Dictionary<Vector2Int,bool> obstacleCache = new Dictionary<Vector2Int,bool>();
    void Awake()
    {
        Instance = this;
    }
    public List<Vector2Int> FindPath(Grid grid, Tilemap[] obstacles, Vector2 startPosition, Vector2 endPosition)
    {
        Vector3Int startCell = grid.WorldToCell(startPosition);
        Vector3Int endCell = grid.WorldToCell(endPosition);
        Vector2Int startGrid = new Vector2Int(startCell.x, startCell.y);
        Vector2Int endGrid = new Vector2Int(endCell.x, endCell.y);
        return FindPath(obstacles, startGrid, endGrid);
    }
    private List<Vector2Int> FindPath(Tilemap[] obstacles,Vector2Int startPosition,Vector2Int endPosition)
    {
        if(startPosition == endPosition)
        {
            return new List<Vector2Int>();
        }
        var openList = new PriorityQueue<Vector2Int, float>();  
        var closeList = new HashSet<Vector2Int>();
        var fatherNode = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float>();
        Vector2Int neighborPosition = default;
        Vector2Int currentPosition = startPosition;
        float neighborGScore;
        obstacleCache[currentPosition] = false;
        gScore[currentPosition] = 0;
        openList.Enqueue(currentPosition,1.5f * GetManhattanDistance(currentPosition, endPosition));
        while (openList.Count > 0)
        {
            currentPosition = openList.Dequeue();
            if(currentPosition == endPosition)
            {
                return BuildPath(fatherNode, startPosition, endPosition);
            }
            closeList.Add(currentPosition);
            for(int n = -1;n <= 1;n++)
            {
                for (int i = -1; i <= 1; i++)
                {
                    if (n == 0 && i == 0) continue;
                    neighborPosition.x = currentPosition.x + n;
                    neighborPosition.y = currentPosition.y + i;
                    if (!obstacleCache.TryGetValue(neighborPosition, out bool isObstacle))
                    {
                        isObstacle = IsObstacle(neighborPosition, obstacles);
                        obstacleCache[neighborPosition] = isObstacle;
                    }
                    if (isObstacle || closeList.Contains(neighborPosition)) continue;
                    if (n != 0 && i != 0) neighborGScore = gScore[currentPosition] + 1.4f;
                    else neighborGScore = gScore[currentPosition] + 1.0f;
                    if(!gScore.ContainsKey(neighborPosition) || neighborGScore < gScore[neighborPosition])
                    {
                        gScore[neighborPosition] = neighborGScore;
                        fatherNode[neighborPosition] = currentPosition;
                        openList.Enqueue(neighborPosition, neighborGScore + 1.5f * GetManhattanDistance(neighborPosition, endPosition));
                    }
                }
            }
        }
        return new List<Vector2Int>();
    }
    private bool IsObstacle(Vector2Int cellPos, Tilemap[] tilemaps)
    {
        Vector3Int tilemapCellPos = new Vector3Int(cellPos.x, cellPos.y, 0);
        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap == null || tilemap.HasTile(tilemapCellPos))
            {
                return true;
            }
        }
        return false;
    }
    private float GetManhattanDistance(Vector2Int firstPosition,Vector2Int secondPosition)
    {
        return Mathf.Abs(firstPosition.x - secondPosition.x) + Mathf.Abs(firstPosition.y - secondPosition.y);
    }
    private List<Vector2Int> BuildPath(Dictionary<Vector2Int,Vector2Int> father, Vector2Int start,Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = end;
        while(current != start)
        {
            path.Add(current);
            current = father[current];
        }
        path.Reverse();
        return path;
    }
}

