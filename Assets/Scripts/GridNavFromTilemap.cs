using UnityEngine;
using UnityEngine.Tilemaps;

public class GridNavFromTilemap : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap groundTilemap;
    public Tilemap bushesTilemap;

    public Vector3Int cellMin;
    public int width;
    public int height; 

    public bool[,] blocked;

    void Awake()
    {
        BuildBoardFromTilemaps();
    }

    public void BuildBoardFromTilemaps()
    {
       
        var b = groundTilemap.cellBounds;
        cellMin = new Vector3Int(b.xMin, b.yMin, 0);
        width   = b.size.x;
        height  = b.size.y;

        blocked = new bool[width, height];

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            var cell = new Vector3Int(cellMin.x + x, cellMin.y + y, 0);
            blocked[x, y] = bushesTilemap.HasTile(cell);
        }
    }


    public Vector2Int WorldToIndex(Vector3 world)
    {
        var cell = groundTilemap.WorldToCell(world);
        int ix = Mathf.Clamp(cell.x - cellMin.x, 0, width  - 1);
        int iy = Mathf.Clamp(cell.y - cellMin.y, 0, height - 1);
        return new Vector2Int(ix, iy);
    }

    public Vector3 IndexToWorldCenter(Vector2Int idx)
    {
        var cell = new Vector3Int(cellMin.x + idx.x, cellMin.y + idx.y, 0);
        return groundTilemap.GetCellCenterWorld(cell);
    }

    public bool InBounds(Vector2Int idx)
        => idx.x >= 0 && idx.x < width && idx.y >= 0 && idx.y < height;

    public bool Walkable(Vector2Int idx)
        => InBounds(idx) && !blocked[idx.x, idx.y];

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!groundTilemap) return;
        Gizmos.color = new Color(0,1,0,0.15f);
        var b = groundTilemap.cellBounds;
        var bl = groundTilemap.GetCellCenterWorld(new Vector3Int(b.xMin, b.yMin, 0));
        var tr = groundTilemap.GetCellCenterWorld(new Vector3Int(b.xMax, b.yMax, 0));
        var size = tr - bl;
        Gizmos.DrawCube(bl + size * 0.5f, new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), 0.01f));
    }
#endif
}
