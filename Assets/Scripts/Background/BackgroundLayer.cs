using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.NetworkInformation;

public class BackgroundLayer : MonoBehaviour
{
    private BackgroundLayerData data;
    private float tileWidth;
    private int sortingOrderBase;

    private List<GameObject> activeTiles = new List<GameObject>();

    private bool isInitialized = false;

    public void Initialize(BackgroundLayerData layerData, int layerIndex)
    {
        if (isInitialized)
        {
            Debug.LogWarning("BackgroundLayer is already initialized.");
            return;
        }
        isInitialized = true;
        data = layerData;
        sortingOrderBase = data.baseOrderInLayer + layerIndex * data.OrderGap;
        tileWidth = GetTileWidth();
        if (tileWidth <= 0f) return;

        SpawnInitialTiles();
    }
    private float GetTileWidth()
    {
        if (data.possibleTiles == null || data.possibleTiles.Count == 0) return 0f;
        
        BackgroundTileData firstTile = data.possibleTiles[0];
        if (firstTile == null || firstTile.sprite == null)
        {
            Debug.LogWarning("First tile or its sprite is null for layer: " + data.name);
            return 1f; // Default width
        }
        float width = firstTile.sprite.rect.width / firstTile.sprite.pixelsPerUnit;
        return width;
    }
    private float GetTileWidth0()
    {
        if(data.possibleTiles == null || data.possibleTiles.Count == 0)
        {
            Debug.LogWarning("No possible tiles defined for layer: " + data.name);
            return 1f; // Default width
        }
        return data.possibleTiles[0].sprite.bounds.size.x;
    }

    private void SpawnInitialTiles()
    {
        Camera cam = Camera.main;
        if(cam == null) return;

        float screenRight = cam.ViewportToWorldPoint(new Vector3(1f, 0, 0)).x;
        float screenLeft = cam.ViewportToWorldPoint(new Vector3(0f, 0, 0)).x;
        float screenWidth = screenRight - screenLeft;

        int tilesNeeded = Mathf.CeilToInt(screenWidth / tileWidth) + 2;

        for (int i = -1; i < tilesNeeded - 1; i++)
        {
            float xPos = screenLeft + i * tileWidth;
            GameObject tile = SpawnTile(xPos, sortingOrderBase);
            activeTiles.Add(tile);
        }
    }

    private GameObject SpawnTile(float xPos, int sortingOrder)
    {
        if(data.possibleTiles == null || data.possibleTiles.Count == 0)
        {
            Debug.LogWarning("No possible tiles defined for layer: " + data.name);
            return null;
        }
        BackgroundTileData tileData = data.possibleTiles[UnityEngine.Random.Range(0, data.possibleTiles.Count)];
        GameObject tileObj = new GameObject($"Tile_{tileData.name}");
        tileObj.transform.SetParent(transform);
        tileObj.transform.position = new Vector3(xPos, 0, data.zOffset);

        SpriteRenderer sr = tileObj.AddComponent<SpriteRenderer>();
        sr.sprite = tileData.sprite;
        sr.sortingLayerName = data.sortingLayerName;
        sr.sortingOrder = sortingOrder;

        return tileObj;
    }

    public void Scroll(float delta)
    {
        if(activeTiles.Count == 0) return;

        foreach (var tile in activeTiles)
        {
            if (tile == null) continue;
            
            Vector3 pos = tile.transform.position;
            pos.x -= delta * data.scrollSpeed;
            tile.transform.position = pos;

            Camera cam = Camera.main;
            if( cam != null)
            {
                float leftEdge = cam.ViewportToWorldPoint(Vector3.zero).x - tileWidth;

                if(pos.x < leftEdge)
                {
                    float maxX = GetRightmostTileX();
                    tile.transform.position = new Vector3(maxX + tileWidth, pos.y, pos.z);
                }
            }
        }
    }

    private float GetRightmostTileX()
    {
        float max = -Mathf.Infinity;
        foreach (var tile in activeTiles)
        {
            if (tile != null && tile.transform.position.x > max)
                max = tile.transform.position.x;
        }
        return max;
    }

    private void ChangeTileSprite(GameObject tile)
    {
        if(data.possibleTiles.Count <= 1) return;
        var newTileData = data.possibleTiles[UnityEngine.Random.Range(0, data.possibleTiles.Count)];
        if (newTileData != null && newTileData.sprite != null)
        {
            var sr = tile.GetComponent<SpriteRenderer>();
            if(sr != null)
            {
                sr.sprite = newTileData.sprite;
            }
        }
    }
}
