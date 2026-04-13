using UnityEngine;
using System.Collections.Generic;
using System;

public class BackgroundSpawner : MonoBehaviour
{
    [SerializeField] private List<BackgroundLayerData> layers;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private int poolSize = 5;

    private void Awake()
    {
        int requiredPoolSize = CalculateRequiredPoolSize();

        BackgroundTilePool pool = FindAnyObjectByType<BackgroundTilePool>();
        if (pool == null)
        {
            GameObject poolObj = new GameObject("BackgroundTilePool");
            pool = poolObj.AddComponent<BackgroundTilePool>();
            int initialSize = Mathf.Max(poolSize, requiredPoolSize);
            pool.Initialize(tilePrefab, initialSize);
            Debug.Log($"BackgroundSpawner: Initialized BackgroundTilePool with size {initialSize}");
        }
        else
        {
            if (!pool.IsInitialized && tilePrefab != null)
            {
                int initialSize = Mathf.Max(poolSize, requiredPoolSize);
                pool.Initialize(tilePrefab, initialSize);
                Debug.Log($"BackgroundSpawner: Initialized BackgroundTilePool with size {initialSize}");
            }
            else if (pool.IsInitialized)
            {
                if (pool.TotalCount < requiredPoolSize)
                {
                    int expandBy = requiredPoolSize - pool.TotalCount;
                    pool.ExpandPool(expandBy);
                    Debug.Log($"BackgroundSpawner: Expanded backgroundtilepool by {expandBy} to reach required size {requiredPoolSize}");
                }
            }
        }
    }

    void Start()
    {
        if (layers == null || layers.Count == 0)
        {
            Debug.LogWarning("BackgroundSpawner: 'Layers' is empty. No background layers will be created.");
            return;
        }

        foreach(var layerData in layers)
        {
            if (layerData == null)
            {
                Debug.LogError("BackgroundSpawner: one of the 'layers' entries is null. Skipping.");
                continue;
            }
            GameObject layerObj = new GameObject($"Layer_{layerData.name}");
            layerObj.transform.SetParent(transform);
            BackgroundLayer layer = layerObj.AddComponent<BackgroundLayer>();

            SetLayerData(layer, layerData);
        }
    }

    public void SetLayerData(BackgroundLayer layer, BackgroundLayerData data)
    {
        layer.Init(data);
    }

    private int CalculateRequiredPoolSize()
    {
        if (layers == null || layers.Count == 0) return poolSize;

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return poolSize;

        Vector3 leftEdgeWorld = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 rightEdgeWorld = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, mainCamera.nearClipPlane));
        float screenWidth = Mathf.Abs(rightEdgeWorld.x  - leftEdgeWorld.x);

        int totalNeeded = 0;
        foreach (var ld in layers)
        {
            if (ld == null) continue;

            float avgWidth = 2f;
            if (ld.possibleTiles != null && ld.possibleTiles.Count > 0)
            {
                float sum = 0f;
                int cout = 0;
                foreach (var pd in ld.possibleTiles)
                {
                    if (pd != null && pd.sprite != null)
                    {
                        sum += pd.sprite.bounds.size.x;
                        cout++;
                    }
                }
                if (cout > 0) avgWidth = sum / cout;
            }
            int neededForLayer = Mathf.CeilToInt(screenWidth / Mathf.Max(0.01f, avgWidth)) + 2;
            if (!ld.continuousSpwning) neededForLayer = Mathf.Min(3, neededForLayer);
            totalNeeded += neededForLayer;
        }
        return Mathf.Max(poolSize, totalNeeded);
    }
}
