using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapReplacer : MonoBehaviour
{
    [Header("대상 Tilemap")]
    public Tilemap tilemap;

    [Header("교체할 타일들")]
    public TileBase fromTile; // 기존 타일
    public TileBase toTile;   // 새 타일

    [ContextMenu("Replace Tiles")]
    void ReplaceTiles()
    {
        if (tilemap == null)
        {
            Debug.LogError("TilemapReplacer: tilemap 이 비어있습니다.");
            return;
        }

        if (fromTile == null || toTile == null)
        {
            Debug.LogError("TilemapReplacer: fromTile 또는 toTile 이 비어있습니다.");
            return;
        }

        var bounds = tilemap.cellBounds;
        int count = 0;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase current = tilemap.GetTile(pos);

                if (current == fromTile)
                {
                    tilemap.SetTile(pos, toTile);
                    count++;
                }
            }
        }

        Debug.Log($"TilemapReplacer: {count} 개 타일을 교체했습니다.");
    }
}
