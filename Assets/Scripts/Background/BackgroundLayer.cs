using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BackgroundLayer : MonoBehaviour
{
    private BackgroundLayerData data;
    private float tileWidth;
    private int sortingOrderBase;
    private float scrollTimer;
    private float currentScrollSpeed;

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

        scrollTimer = 0f;
        currentScrollSpeed = data.scrollSpeed;
        SpawnInitialTiles();
    }

    private float GetTileWidth()
    {
        if (data.possibleTiles == null || data.possibleTiles.Count == 0) return 0f;
        
        BackgroundTileData firstTile = data.possibleTiles[0];
        if (firstTile == null || firstTile.sprite == null)
        {
            Debug.LogWarning("First tile or its sprite is null for layer: " + name);
            return 1f;
        }
        float width = firstTile.sprite.rect.width / firstTile.sprite.pixelsPerUnit;
        return width;
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
            Debug.LogWarning("No possible tiles defined for layer: " + name);
            return null;
        }
        BackgroundTileData tileData = data.possibleTiles[Random.Range(0, data.possibleTiles.Count)];
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

        scrollTimer += delta;
        currentScrollSpeed = CalculateCurrentSpeed(delta);

        foreach (var tile in activeTiles)
        {
            if (tile == null) continue;
            
            Vector3 pos = tile.transform.position;
            pos.x -= currentScrollSpeed * delta;
            tile.transform.position = pos;

            Camera cam = Camera.main;
            if(cam != null)
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

    private float CalculateCurrentSpeed(float delta)
    {
        switch(data.scrollingMode)
        {
            case ScrollingMode.Constant:
                return data.scrollSpeed;
            
            case ScrollingMode.EaseInEaseOut:
                scrollTimer %= data.easeDuration * 2f;
                if(scrollTimer < data.easeDuration)
                {
                    float t = scrollTimer / data.easeDuration;
                    return Mathf.Lerp(data.scrollSpeedRange.x, data.scrollSpeedRange.y, t);
                }
                else
                {
                    float t = (scrollTimer - data.easeDuration) / data.easeDuration;
                    return Mathf.Lerp(data.scrollSpeedRange.y, data.scrollSpeedRange.x, t);
                }
            
            case ScrollingMode.SineWave:
                float sineValue = Mathf.Sin(scrollTimer * data.sineFrequency * Mathf.PI * 2f);
                return data.scrollSpeed + sineValue * data.sineAmplitude;
            
            default:
                return data.scrollSpeed;
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
}
