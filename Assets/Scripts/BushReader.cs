using UnityEngine;
using UnityEngine.Tilemaps;

public class BushReader : MonoBehaviour
{
    public Tilemap bushTilemap;
    public TileBase bushBlueberryTile; 
    public TileBase bushLingonTile;    

    public bool IsBlueberry(Vector3Int cell)
        => bushTilemap.GetTile(cell) == bushBlueberryTile;

    public bool IsLingon(Vector3Int cell)
        => bushTilemap.GetTile(cell) == bushLingonTile;
}
