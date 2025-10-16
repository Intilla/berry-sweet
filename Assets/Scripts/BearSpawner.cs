using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BearSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject babyBearPrefab;

    [Header("Spawn Area")]
    public float spawnMargin = 2f;

    [Header("Difficulty Settings")]
    public int startBearCount = 2;
    public int maxBearCount = 10;
    public float difficultyIncreaseInterval = 30f;
    public int bearsPerIncrease = 1;

    [Header("Respawn Timing")]
    public float minRespawnDelay = 1f;
    public float maxRespawnDelay = 3f;

    [Header("Bear Stat Scaling")]
    public float baseMoveSpeed = 1.5f;
    public float speedIncreasePerWave = 0.15f;

    private readonly List<BabyBearBehaviour> activeBears = new();
    private int currentBearLimit;
    private int currentWave = 0;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        currentBearLimit = startBearCount;

        // Spawn initial bears
        for (int i = 0; i < currentBearLimit; i++)
            SpawnBear();

        // Start difficulty progression
        StartCoroutine(DifficultyRamp());
    }

    IEnumerator DifficultyRamp()
    {
        while (true)
        {
            yield return new WaitForSeconds(difficultyIncreaseInterval);

            if (currentBearLimit < maxBearCount)
            {
                currentWave++;
                currentBearLimit += bearsPerIncrease;

                while (activeBears.Count < currentBearLimit)
                    SpawnBear();
            }
        }
    }

    public void OnBearDestroyed(BabyBearBehaviour deadBear)
    {
        activeBears.RemoveAll(b => b == null);

        if (activeBears.Count < currentBearLimit)
        {
            float randomDelay = Random.Range(minRespawnDelay, maxRespawnDelay);
            StartCoroutine(RespawnAfterDelay(randomDelay));
        }
    }

    IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnBear();
    }

    void SpawnBear()
    {
        if (!babyBearPrefab) return;
        if (activeBears.Count >= currentBearLimit) return;
        if (!mainCam) mainCam = Camera.main;

        Vector3 spawnPos = GetOffScreenSpawnPosition();
        spawnPos.z = 0;
        GameObject bearObj = Instantiate(babyBearPrefab, spawnPos, Quaternion.identity);

        var behaviour = bearObj.GetComponent<BabyBearBehaviour>();
        if (behaviour)
        {
            behaviour.moveSpeed = baseMoveSpeed + (speedIncreasePerWave * currentWave);
            StartCoroutine(StartBearAfterDelay(behaviour, Random.Range(0f, 1.5f)));
        }

        activeBears.Add(behaviour);
    }

    Vector3 GetOffScreenSpawnPosition()
    {
        Vector3 camPos = mainCam.transform.position;
        float camHeight = 2f * mainCam.orthographicSize;
        float camWidth = camHeight * mainCam.aspect;

        int side = Random.Range(0, 4);
        Vector3 spawnPos = camPos;

        switch (side)
        {
            case 0:
                spawnPos.x -= camWidth / 2 + spawnMargin;
                spawnPos.y += Random.Range(-camHeight / 2, camHeight / 2);
                break;
            case 1:
                spawnPos.x += camWidth / 2 + spawnMargin;
                spawnPos.y += Random.Range(-camHeight / 2, camHeight / 2);
                break;
            case 2:
                spawnPos.y += camHeight / 2 + spawnMargin;
                spawnPos.x += Random.Range(-camWidth / 2, camWidth / 2);
                break;
            case 3:
                spawnPos.y -= camHeight / 2 + spawnMargin;
                spawnPos.x += Random.Range(-camWidth / 2, camWidth / 2);
                break;
        }

        return spawnPos;
    }

    IEnumerator StartBearAfterDelay(BabyBearBehaviour bear, float delay)
    {
        bear.enabled = false;
        yield return new WaitForSeconds(delay);
        bear.enabled = true;
    }

    void OnDrawGizmosSelected()
    {
        if (!mainCam) mainCam = Camera.main;
        if (!mainCam) return;

        Vector3 camPos = mainCam.transform.position;
        float camHeight = 2f * mainCam.orthographicSize;
        float camWidth = camHeight * mainCam.aspect;

        Gizmos.color = new Color(0.9f, 0.6f, 0.3f, 0.2f);
        Gizmos.DrawWireCube(camPos, new Vector3(camWidth, camHeight, 0));

        Gizmos.color = new Color(1f, 0.3f, 0.1f, 0.3f);
        Gizmos.DrawWireCube(camPos, new Vector3(camWidth + spawnMargin * 2, camHeight + spawnMargin * 2, 0));
    }
}
