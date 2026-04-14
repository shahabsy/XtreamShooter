using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BackgroundLayer : MonoBehaviour
{
    [SerializeField] private BackgroundLayerData layerData;

    private List<BackgroundTile> activeTiles = new List<BackgroundTile>();
    private Camera mainCamera;
    private float leftBoundary;
    private float rightSpawnX;

    private int layerIndex = 0;
    private int layerSortingOrder = 0;
    private float layerZ = 0f;
    private const float layerZSpacing = 0.05f; // very small z space for parallax only
    
    private float fallbackWidth = 10f;

    public void Init(BackgroundLayerData data, int index, int sortingOrder)
    {
        layerData = data;
        layerIndex = index;
        layerSortingOrder = sortingOrder;
        mainCamera = Camera.main;

        layerZ = layerData.zOffset - (layerIndex * layerZSpacing);
        var p = transform.position;
        p.z = layerZ;
        transform.position = p;
    }

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        ComputeBoundaries();

        if (layerData == null) return;
        
        if (layerData.continuousSpwning)
        {
            float currentX = rightSpawnX;
            while (currentX > leftBoundary)
            {
                SpawnTileAt(currentX);
                currentX -= GetAverageTileWidth();
            }
        }
        else
        {
            StartCoroutine(SpawnRoutine());
        }
    }

    private void SpawnTileAt(float x)
    {
        if (BackgroundTilePool.Instance == null) return;

        BackgroundTile tile = BackgroundTilePool.Instance.GetTile();
        if (tile == null) return;

        BackgroundTileData data = GetRandomTile();
        if (data == null || data.sprite == null)
        {
            BackgroundTilePool.Instance.ReturnTile(tile);
            return;
        }
        tile.SetTile(data);

        tile.transform.SetParent(transform, worldPositionStays: false);
        tile.transform.localScale = Vector3.one;
        tile.transform.position = new Vector3(x, 0, layerZ);

        var sr = tile.SpriteRenderer;
        if (sr != null)
        {
            sr.sortingLayerName = layerData.sortingLayerName;
            sr.sortingOrder = 0;
        }

        activeTiles.Add(tile);
    }

    // Update is called once per frame
    void Update()
    {
        ComputeBoundaries();

        if (layerData == null) return;

        float delta = layerData.scrollSpeed * Time.deltaTime;
        foreach (var tile in activeTiles)
        {
            if (tile != null)
                tile.transform.position += Vector3.left * delta;
        }

        for (int i = activeTiles.Count -1; i >=0; i--)
        {
            var t = activeTiles[i];
            if (t == null)
            {
                activeTiles.RemoveAt(i);
                continue;
            }
            if (t.transform.position.x + t.Width / 2 < leftBoundary)
            {
                BackgroundTilePool.Instance.ReturnTile(t);
                activeTiles.RemoveAt(i);
            }
        }

        if (layerData.continuousSpwning)
        {
            float rightmostX = GetRightmostTileX();
            float avgWidth = GetAverageTileWidth();

            while(rightmostX < rightSpawnX)
            {
                float spawnX = rightmostX + avgWidth / 2f;
                SpawnTileAt(spawnX);
                rightmostX = GetRightmostTileX();
            }
        }
    }

    private void ComputeBoundaries()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        float zDistance = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        Vector3 leftEdgeWorld = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, zDistance));
        leftBoundary = leftEdgeWorld.x - 1f;

        Vector3 rightEdgeWorld = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, zDistance));
        rightSpawnX = rightEdgeWorld.x + 2f;
    }

    private BackgroundTileData GetRandomTile()
    {
        if (layerData == null || layerData.possibleTiles == null || layerData.possibleTiles.Count == 0) return null;
        
        int totalWeight = 0;
        foreach (var t in layerData.possibleTiles)
        {
            if (t != null) totalWeight += Mathf.Max(0, t.weight);
        }
        if (totalWeight <= 0) return layerData.possibleTiles[0];

        int r = UnityEngine.Random.Range(0, totalWeight);
        foreach (var t in layerData.possibleTiles)
        {
            if (t == null) continue;
            if (r < t.weight) return t;
            r -= t.weight;
        }
        return layerData.possibleTiles[0];
    }

    private float GetAverageTileWidth()
    {
        if (layerData == null || layerData.possibleTiles == null || layerData.possibleTiles.Count == 0) return fallbackWidth;
        float sum = 0f;
        int count = 0;
        foreach(var pd in layerData.possibleTiles)
        {
            if (pd != null && pd.sprite != null)
            {
                sum += pd.sprite.bounds.size.x;
                count++;
            }
        }
        if (count == 0) return fallbackWidth;
        return sum / count;
    }

    private float GetRightmostTileX()
    {
        if (activeTiles.Count == 0 || activeTiles == null)
        {
            return rightSpawnX - GetAverageTileWidth();
        }

        float max = -Mathf.Infinity;
        foreach (var t in activeTiles)
        {
            if (t == null) continue;
            float rightEdge = t.transform.position.x + t.Width / 2;
            if (rightEdge > max) max = rightEdge;
        }
        return max;
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(layerData != null ? layerData.spawnInterval : 1f);
            SpawnTileAt(rightSpawnX);
        }
    }
}
