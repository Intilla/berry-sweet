using UnityEngine;
using System.Collections;

public class BaseZoneSpawner : MonoBehaviour
{
    [Header("Zone Settings")]
    public GameObject baseZonePrefab;
    public float zoneRadius = 1.5f;

    [Header("Spawn Area")]
    public Vector2 minSpawnPos = new Vector2(-20f, -20f);
    public Vector2 maxSpawnPos = new Vector2(20f, 20f);

    [Header("Timing")]
    public float minSpawnDelay = 5f;
    public float maxSpawnDelay = 15f;
    public float minDisappearDelay = 3f;
    public float maxDisappearDelay = 8f;

    [Header("Obstacles")]
    public LayerMask obstacleLayer;

    private GameObject currentZone;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnZone();

            float visibleTime = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(visibleTime);

            if (currentZone != null)
            {
                Destroy(currentZone);
                currentZone = null;
            }

            float disappearTime = Random.Range(minDisappearDelay, maxDisappearDelay);
            yield return new WaitForSeconds(disappearTime);
        }
    }

    void SpawnZone()
    {
        Vector2 spawnPos = FindValidSpawnPosition();
        currentZone = Instantiate(baseZonePrefab, spawnPos, Quaternion.identity);
    }

    Vector2 FindValidSpawnPosition()
    {
        int maxAttempts = 50;
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(minSpawnPos.x, maxSpawnPos.x),
                Random.Range(minSpawnPos.y, maxSpawnPos.y)
            );

            Collider2D hit = Physics2D.OverlapCircle(candidate, zoneRadius, obstacleLayer);
            if (hit == null)
            {
                return candidate;
            }
        }

        return Vector2.zero;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            new Vector3((minSpawnPos.x + maxSpawnPos.x) / 2, (minSpawnPos.y + maxSpawnPos.y) / 2, 0),
            new Vector3(maxSpawnPos.x - minSpawnPos.x, maxSpawnPos.y - minSpawnPos.y, 0)
        );
    }
}
