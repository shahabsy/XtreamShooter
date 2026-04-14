using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;

public class BackgroundSpawner : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private int poolSize = 5;

    private List<BackgroundLayerData> layers = new List<BackgroundLayerData>();
    [SerializeField] private List<BackgroundLayer> activeLayers = new List<BackgroundLayer>();


    private void Awake()
    {
        int requiredPoolSize = CalculateRequiredPoolSize();

        BackgroundTilePool pool = FindAnyObjectByType<BackgroundTilePool>();

        if (pool != null && pool.tilePrefab != tilePrefab)
        {
            Debug.LogWarning("Existing BackgroundTilePool uses a different tilePrefab. Destroying and recreating.");
            Destroy(pool.gameObject);
            pool = null;
        }

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

        for (int i = 0; i < layers.Count; ++i)
        {
            var layerData = layers[i];
            if (layerData == null)
            {
                Debug.LogError("BackgroundSpawner: one of the 'layers' entries is null. Skipping.");
                continue;
            }

            GameObject layerObj = new GameObject($"Layer_{i:00}_{layerData.name}");
            layerObj.transform.SetParent(transform);

            var sg = layerObj.AddComponent<SortingGroup>();
            sg.sortingLayerName = layerData.sortingLayerName;

            int layerOrder = layerData.baseOrderInLayer + (i * layerData.OrderGap);
            sg.sortingOrder = layerOrder;

            BackgroundLayer layer = layerObj.AddComponent<BackgroundLayer>();

            layer.Init(layerData, i, layerOrder);
            // store the runtime component for later control
            activeLayers.Add(layer);
        }
    }

    public void SetScrolling(bool enabled)
    {
        foreach (var layer in activeLayers)
        {
            layer.SetScrolling(enabled);
        }
    }

    public void SetSpawning(bool enabled)
    {
        foreach (var layer in activeLayers)
        {
            layer.SetSpawning(enabled);
        }
    }

    public void SetScrollingMultiplier(float multiplier)
    {
        foreach (var layer in activeLayers)
        {
            if (layer != null)
            {
                layer.SetScrollSpeedMultiplier(multiplier);
            }
        }
    }

    public void ResetBackground()
    {
        foreach(var layer in activeLayers)
        {
            if (layer != null)
            {
                Destroy(layer.gameObject);
            }
        }
        activeLayers.Clear();
        Start();
    }

    private int CalculateRequiredPoolSize()
    {
        if (layers == null || layers.Count == 0) return poolSize;

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return poolSize;

        float zDistance = Mathf.Abs(Camera.main.transform.position.z - 0f);
        Vector3 leftEdgeWorld = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, zDistance));
        Vector3 rightEdgeWorld = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, zDistance));
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
            if (!ld.continuousSpawning) neededForLayer = Mathf.Min(3, neededForLayer);
            totalNeeded += neededForLayer;
        }
        return Mathf.Max(poolSize, totalNeeded);
    }
}
