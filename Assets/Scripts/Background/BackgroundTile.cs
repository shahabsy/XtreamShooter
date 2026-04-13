using UnityEngine;

public class BackgroundTile : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer { get; private set; }
    public float Width { get; private set; }

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Width = SpriteRenderer.bounds.size.x;
    }

    public void SetTile(BackgroundTileData data)
    {
        SpriteRenderer.sprite = data.sprite;
        Width = SpriteRenderer.bounds.size.x;
    }
}
