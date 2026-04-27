using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;

public class BackgroundSpawner : MonoBehaviour
{
    public static BackgroundSpawner Instance { get; private set; }

    
    private MissionTileSet currentTileSet;
    private float globalScrollSpeed;
    private bool scrollingEnabled = true;
    private float scrollSpeedMultiplier = 1f;

    [SerializeField] private List<BackgroundLayer> activeLayers = new List<BackgroundLayer>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Initialize(MissionTileSet tileSet)
    {
        if (tileSet == null) return;

        currentTileSet = tileSet;
        globalScrollSpeed = tileSet.baseScrollSpeed;


        ClearAllLayer();
        
        currentTileSet = tileSet;
        globalScrollSpeed = tileSet.baseScrollSpeed;

        SpawnAllLayers();
    }

    private void ClearAllLayer()
    {
        foreach (Transform child in transform)
        {
            //Debug.Log(child.gameObject.name);
            Destroy(child.gameObject);   
        }
        activeLayers.Clear();
        //Debug.Log("Cleared all background layers.");
    }

    private void SpawnAllLayers()
    {
        for(int i = 0; i < currentTileSet.layers.Length; i++)
        {
            BackgroundLayerData layerData = currentTileSet.layers[i];
            if(layerData == null) continue;

            GameObject layerObj = new GameObject($"Layer_{layerData.name}_{i}");
            layerObj.transform.SetParent(transform);

            BackgroundLayer layer = layerObj.AddComponent<BackgroundLayer>();
            layer.Initialize(layerData, i);
            activeLayers.Add(layer);
        }
    }

    private void Update()
    {
        if(!scrollingEnabled) return;
        float scrollDelta = globalScrollSpeed * scrollSpeedMultiplier * Time.deltaTime;
        foreach(var layer in activeLayers)
        {
            layer.Scroll(scrollDelta);
        }
    }

    public void SetScrolling(bool enabled)
    {
        scrollingEnabled = enabled;
    }


    public void SetScrollingMultiplier(float multiplier)
    {
        scrollSpeedMultiplier = multiplier;
        Debug.Log($"Background Speed Multiplier set to {multiplier}");
    }

    public void ResetSpawner()
    {
        ClearAllLayer();
        scrollingEnabled = true;
        scrollSpeedMultiplier = 1f;
        globalScrollSpeed = 0f;
        //Debug.Log("BackgroundSpawner reset.");
    }
}
