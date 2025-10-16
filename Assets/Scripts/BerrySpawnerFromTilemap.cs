using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BerrySpawnerFromTilemap : MonoBehaviour
{
    [Header("Refs")]
    public Tilemap bushesTilemap;
    public GameObject berryPilePrefab;

    [Header("Allowed bush tiles (paint these)")]
    public TileBase bushBlueberryTile;
    public TileBase bushLingonTile;

    [Header("Max active piles")]
    public int blueberryCount = 5;
    public int lingonCount    = 7;

    [Header("Per-pile berry ranges")]
    public Vector2Int blueberryRange = new Vector2Int(3, 5);
    public Vector2Int lingonRange    = new Vector2Int(1, 3);

    [Header("Respawn times (seconds)")]
    public Vector2 blueberryRespawn = new Vector2(10f, 18f);
    public Vector2 lingonRespawn    = new Vector2(6f, 12f);

[Header("Spawn offset")]
public float yOffset = 0.1f;
    public float zOffset = -0.02f;
    public int randomSeed = 0;

    readonly List<Vector3Int> bbCells = new List<Vector3Int>();
    readonly List<Vector3Int> lgCells = new List<Vector3Int>();
    readonly HashSet<Vector3Int> occupied = new HashSet<Vector3Int>();

    void Start()
    {
        if (randomSeed != 0) Random.InitState(randomSeed);
        ScanAllowedCells();
        InitialSpawn();
    }

    void ScanAllowedCells()
    {
        bbCells.Clear(); lgCells.Clear();
        var b = bushesTilemap.cellBounds;
        for (int y = b.yMin; y < b.yMax; y++)
        for (int x = b.xMin; x < b.xMax; x++)
        {
            var c = new Vector3Int(x, y, 0);
            var t = bushesTilemap.GetTile(c);
            if (t == null) continue;
            if (t == bushBlueberryTile) bbCells.Add(c);
            else if (t == bushLingonTile) lgCells.Add(c);
        }
    }

    void InitialSpawn()
    {
        for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
        occupied.Clear();

        Shuffle(bbCells); Shuffle(lgCells);

        for (int i = 0; i < Mathf.Min(blueberryCount, bbCells.Count); i++)
            CreatePileAt(bbCells[i], "blueberry", blueberryRange);

        for (int i = 0; i < Mathf.Min(lingonCount, lgCells.Count); i++)
            CreatePileAt(lgCells[i], "lingon", lingonRange);
    }

    void CreatePileAt(Vector3Int cell, string typeId, Vector2Int range)
    {
        if (occupied.Contains(cell)) return;
        occupied.Add(cell);

        Vector3 world = bushesTilemap.GetCellCenterWorld(cell);
        world.y += yOffset;
        world.z += zOffset;

        var go = Instantiate(berryPilePrefab, world, Quaternion.identity, transform);
        var pile = go.GetComponent<BerryPile>();
        pile.Init(typeId, range.x, range.y);
        pile.OnDepleted = OnPileDepleted;                   

        var tag = go.AddComponent<PileCellTag>();
        tag.cell = cell;
    }

    void OnPileDepleted(BerryPile pile)
    {
        var tag = pile.GetComponent<PileCellTag>();
        if (tag != null) occupied.Remove(tag.cell);

        StartCoroutine(RespawnElsewhere(pile));
    }

    IEnumerator RespawnElsewhere(BerryPile pile)
    {
        bool isBlue = pile.typeId == "blueberry";
        float wait = Random.Range(isBlue ? blueberryRespawn.x : lingonRespawn.x,
                                  isBlue ? blueberryRespawn.y : lingonRespawn.y);

        float end = Time.realtimeSinceStartup + wait;
        while (Time.realtimeSinceStartup < end) yield return null;

        var pool = isBlue ? bbCells : lgCells;
        var range = isBlue ? blueberryRange : lingonRange;

        for (int tries = 0; tries < 200; tries++)
        {
            var c = pool[Random.Range(0, pool.Count)];
            if (occupied.Contains(c)) continue;

            occupied.Add(c);

            pile.transform.position = bushesTilemap.GetCellCenterWorld(c) + new Vector3(0, 0, zOffset);
            var tag = pile.GetComponent<PileCellTag>() ?? pile.gameObject.AddComponent<PileCellTag>();
            tag.cell = c;

            pile.Init(pile.typeId, range.x, range.y);
            yield break;
        }

        StartCoroutine(RespawnElsewhere(pile));
    }

    static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        { int j = Random.Range(0, i + 1); (list[i], list[j]) = (list[j], list[i]); }
    }

    class PileCellTag : MonoBehaviour { public Vector3Int cell; }
}
