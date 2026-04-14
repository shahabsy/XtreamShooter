using UnityEngine;
using System.Collections.Generic;
using System;

public class BackgroundTilePool : MonoBehaviour
{
    public static BackgroundTilePool Instance { get; private set; }

    public GameObject tilePrefab;
    public int poolSize = 5;
    [SerializeField] private int expandChunk = 5;

    private Queue<BackgroundTile> pool = new Queue<BackgroundTile>();
    private HashSet<BackgroundTile> allTiles = new HashSet<BackgroundTile>();
    private bool initialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(GameObject prefab, int size)
    {
        if (initialized) return;
        tilePrefab = prefab;
        poolSize = Mathf.Max(1, size);
        CreatePool();
        initialized = true;
    }

    private void CreatePool()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("BackgroundTilePool.CreatePool called with null tilePrefab.");
            return;
        }
        for(int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(tilePrefab, transform);
            obj.SetActive(false);
            var tile = obj.GetComponent<BackgroundTile>();
            if (tile != null)
            {
                pool.Enqueue(tile);
                allTiles.Add(tile);
            }
            else
            {
                Debug.LogWarning("BackgroundTilePool: instantiated prefab has no BackgroundTile component.");
            }
        }
    }

    public BackgroundTile GetTile()
    {
        if (!initialized)
        {
            Debug.LogWarning("BackgroundTilePool.GetTile called before Initialize().");
            if (tilePrefab != null && poolSize > 0)
            {
                CreatePool();
                initialized = true;
            }
            else return null;
        }

        if ( pool.Count == 0)
        {
            
            int minChunk = Mathf.Max(3, poolSize / 2);
            int chunk = Mathf.Max(expandChunk, minChunk);
            if (tilePrefab == null)
            {
                Debug.LogWarning("BackgroundTilePool.GetTile is null, cannot expand pool");
                return null;
            }
            else
            {
                Debug.LogWarning($"BackgroundTilePool exhausted: expanding pool by {chunk}.");
                ExpandPool(chunk);
            }
        }
        
        BackgroundTile tile = (pool.Count > 0) ? pool.Dequeue() : null;
        if (tile != null)
        {
            tile.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("BackgroundTilePool.GetTile: failed to obtain a tile even after expansion.");
        }
        return tile;
    }

    public void ReturnTile(BackgroundTile tile)
    {
        if (tile == null) return;

        tile.transform.SetParent(transform, worldPositionStays: false);
        tile.transform.localPosition = Vector3.zero;
        tile.transform.localScale = Vector3.one;
        tile.gameObject.SetActive(false);

        if (allTiles.Contains(tile) && !pool.Contains(tile))
        {
            pool.Enqueue(tile);
        }
    }

    public void ExpandPool(int count = 5)
    {
        if (tilePrefab == null) return;
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(tilePrefab, transform);
            obj.SetActive(false);
            var t = obj.GetComponent<BackgroundTile>();
            if (t != null)
            {
                allTiles.Add(t);
                pool.Enqueue(t);
            }
        }
    }

    public int TotalCount => allTiles.Count;
    public int AvailableCount => pool.Count;
    public int InUseCount => TotalCount - AvailableCount;
    public bool IsInitialized => initialized;
}
