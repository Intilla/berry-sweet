using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class BabyBearBehaviour : MonoBehaviour
{
    [Header("General Settings")]
    public float moveSpeed = 1.5f;
    public float playerDetectRadius = 3f;
    public string playerTag = "Player";

    [Header("Berry Picking")]
    public float searchRadius = 6f;
    public float pickDelay = 0.7f;
    public float afterPickWait = 2f;

    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite[] walkSprites;
    public Sprite alertSprite;
    public Sprite[] eatSprites;

    [Header("Animation Settings")]
    public float walkFrameRate = 0.15f;
    public float eatFrameRate = 0.25f;

    [Header("Mother Bear Settings")]
    public GameObject motherBearPrefab;
    public float motherBearSpawnDelay = 5f;
    public Transform spawnPoint;

    [HideInInspector] public string pickingMode = "Nearest";


    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Transform player;
    private bool playerInSight;
    private Coroutine mainRoutine;
    private Coroutine motherBearTimerRoutine;
    private float motherBearTimerRemaining;

    private Coroutine walkAnimRoutine;
    private Coroutine eatAnimRoutine;
    private bool isMoving;

    private Vector2 lastClearDirection = Vector2.right; 

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        motherBearTimerRemaining = motherBearSpawnDelay;

        var playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj)
            player = playerObj.transform;
    }

IEnumerator Start()
{
    yield return new WaitForSeconds(Random.Range(0f, 0.25f));
    mainRoutine = StartCoroutine(MainRoutine());
}

    void Update() => HandlePlayerDetection();


    IEnumerator MainRoutine()
{

    if (walkAnimRoutine == null)
{
    isMoving = true;
    walkAnimRoutine = StartCoroutine(AnimateWalk());
}


    while (true)
    {
        if (playerInSight)
        {
            StopWalkAnimation();
            yield return null;
            continue;
        }

        BerryPile targetPile = null;
        while (targetPile == null)
        {
            targetPile = FindRandomPile();

            if (targetPile == null)
            {
                yield return new WaitForSeconds(Random.Range(0.2f, 2.0f));
                yield return Wander();
            }
        }

if (targetPile)
{
    yield return WalkToSafe(targetPile.transform.position);

    if (!targetPile || !targetPile.IsAvailable(name))
        continue;
}


        StopWalkAnimation();
        sr.sprite = idleSprite;
        isMoving = false;

        if (targetPile == null)
            continue;

        if (targetPile && targetPile.berries > 0)
        {
            yield return PickFromPile(targetPile);
        }

if (targetPile)
    targetPile.reservedBy = null;


        yield return new WaitForSeconds(afterPickWait);
    }
}




    IEnumerator WalkToSafe(Vector3 destination)
{
    StopWalkAnimation();
    isMoving = true;
    walkAnimRoutine = StartCoroutine(AnimateWalk());

    Vector3 lastPos = transform.position;
    float stuckTimer = 0f;

    while (Vector2.Distance(transform.position, destination) > 0.3f)
    {
        if (playerInSight) yield break;

        Vector2 dir = (destination - transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 0.6f, LayerMask.GetMask("Bush", "Obstacle"));
        if (hit.collider)
        {
            // choose a new clear direction
            dir = FindClearDirection(dir);
            destination = transform.position + (Vector3)dir * 2f;
        }

        dir = AvoidOtherBears(dir);

        rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
        sr.flipX = dir.x > 0;

        float distMoved = Vector2.Distance(transform.position, lastPos);
        if (distMoved < 0.01f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 1f)
            {
                dir = FindClearDirection(lastClearDirection);
                destination = transform.position + (Vector3)dir * 2f;
                stuckTimer = 0f;
            }
        }
        else stuckTimer = 0f;

        lastClearDirection = dir;
        lastPos = transform.position;

        yield return new WaitForFixedUpdate();
    }

    StopWalkAnimation();
    sr.sprite = idleSprite;
    isMoving = false;
}


    IEnumerator Wander()
    {
        StopWalkAnimation();
        yield return new WaitForSeconds(Random.Range(0.5f, 2f));

        Vector2 dir = FindClearDirection(lastClearDirection);
        Vector3 destination = transform.position + (Vector3)dir * Random.Range(2f, 4f);

        yield return WalkToSafe(destination);
    }

    Vector2 FindClearDirection(Vector2 baseDir)
    {
        for (int i = 0; i < 18; i++)
        {
            float angle = i * 20f;
            Vector2 testDir = Quaternion.Euler(0, 0, angle) * baseDir;
            if (!Physics2D.Raycast(transform.position, testDir, 0.6f, LayerMask.GetMask("Bush", "Obstacle")))
                return testDir.normalized;

            Vector2 testDir2 = Quaternion.Euler(0, 0, -angle) * baseDir;
            if (!Physics2D.Raycast(transform.position, testDir2, 0.6f, LayerMask.GetMask("Bush", "Obstacle")))
                return testDir2.normalized;
        }

        // fallback to last known clear direction or random
        return lastClearDirection != Vector2.zero ? lastClearDirection : Random.insideUnitCircle.normalized;
    }

 IEnumerator PickFromPile(BerryPile pile)
{
    rb.linearVelocity = Vector2.zero;
    isMoving = false;
    StopWalkAnimation();

    if (eatAnimRoutine != null) StopCoroutine(eatAnimRoutine);
    eatAnimRoutine = StartCoroutine(AnimateEat());
    yield return null;

    while (pile && pile.berries > 0)
    {
        if (playerInSight) yield break;
        if (pile.TryPick())
        {
            yield return new WaitForSeconds(pickDelay);
        }
        else break;
    }

    StopEatAnimation();
    sr.sprite = idleSprite;
}


    IEnumerator AnimateWalk()
    {
        int frame = 0;
        while (isMoving)
        {
            if (walkSprites.Length > 0)
            {
                sr.sprite = walkSprites[frame];
                frame = (frame + 1) % walkSprites.Length;
            }
            yield return new WaitForSeconds(walkFrameRate);
        }
        sr.sprite = idleSprite;
    }

    IEnumerator AnimateEat()
    {
        int frame = 0;
        while (true)
        {
            if (eatSprites.Length > 0)
            {
                sr.sprite = eatSprites[frame];
                frame = (frame + 1) % eatSprites.Length;
            }
            yield return new WaitForSeconds(eatFrameRate);
        }
    }

    void StopWalkAnimation()
    {
        if (walkAnimRoutine != null)
        {
            StopCoroutine(walkAnimRoutine);
            walkAnimRoutine = null;
        }
    }

    void StopEatAnimation()
    {
        if (eatAnimRoutine != null)
        {
            StopCoroutine(eatAnimRoutine);
            eatAnimRoutine = null;
        }
    }

BerryPile FindRandomPile()
    {
        BerryPile[] piles = Object.FindObjectsByType<BerryPile>(FindObjectsSortMode.None);
        if (piles.Length == 0) return null;

        List<BerryPile> validPiles = new List<BerryPile>();
        foreach (var pile in piles)
        {
            if (pile && pile.berries > 0 && Vector2.Distance(transform.position, pile.transform.position) <= searchRadius)
                validPiles.Add(pile);
        }

        if (validPiles.Count == 0) return null;

        BerryPile chosen = validPiles[Random.Range(0, validPiles.Count)];
        chosen.reservedBy = name;
        return chosen;
    }





    void HandlePlayerDetection()
{
    if (!player) return;

    float dist = Vector2.Distance(transform.position, player.position);
    bool wasInSight = playerInSight;
    playerInSight = false;

    if (dist <= playerDetectRadius)
    {
        Vector2 dir = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, LayerMask.GetMask("Bush", "Obstacle"));

        if (!hit.collider)
            playerInSight = true;
    }

    if (playerInSight && !wasInSight)
    {
        StopWalkAnimation();
        StopEatAnimation();
        sr.sprite = alertSprite;
        if (!MotherBearBehaviour.IsActive)
            PauseBehaviourAndStartMotherTimer();
    }
    else if (!playerInSight && wasInSight)
    {
        sr.sprite = idleSprite;
        ResumeBehaviourAndPauseMotherTimer();
    }

    if (playerInSight)
    {
        Vector3 direction = (player.position - transform.position).normalized;
        sr.flipX = direction.x > 0;
    }
}


    void PauseBehaviourAndStartMotherTimer()
    {
        if (mainRoutine != null)
        {
            StopCoroutine(mainRoutine);
            mainRoutine = null;
        }
        if (motherBearTimerRoutine == null)
            motherBearTimerRoutine = StartCoroutine(MotherBearCountdown());
    }

    void ResumeBehaviourAndPauseMotherTimer()
    {
        if (motherBearTimerRoutine != null)
        {
            StopCoroutine(motherBearTimerRoutine);
            motherBearTimerRoutine = null;
        }
        if (mainRoutine == null)
            mainRoutine = StartCoroutine(MainRoutine());
    }

    IEnumerator MotherBearCountdown()
    {
        while (motherBearTimerRemaining > 0f)
        {
            if (!playerInSight) yield break;
            motherBearTimerRemaining -= Time.deltaTime;
            yield return null;
        }
        SpawnMotherBearAndDie();
    }

    void SpawnMotherBearAndDie()
    {
        if (!MotherBearBehaviour.IsActive)
        {
            Vector3 spawnPos = spawnPoint ? spawnPoint.position : transform.position;
            Instantiate(motherBearPrefab, spawnPos, Quaternion.identity);
            Destroy(gameObject);
        }
    }

void OnDestroy()
{
    var spawner = FindFirstObjectByType<BearSpawner>();
    if (spawner)
        spawner.OnBearDestroyed(this);
}


    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0.3f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, playerDetectRadius);

        Gizmos.color = new Color(0.5f, 1f, 0.5f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }

    Vector2 AvoidOtherBears(Vector2 desiredDir)
{
    BabyBearBehaviour[] allBears = Object.FindObjectsByType<BabyBearBehaviour>(FindObjectsSortMode.None);
    Vector2 avoidDir = Vector2.zero;

    foreach (var bear in allBears)
    {
        if (bear == this) continue;
        float dist = Vector2.Distance(transform.position, bear.transform.position);
        if (dist < 1.0f) 
        {
            Vector2 away = (Vector2)(transform.position - bear.transform.position).normalized / dist;
            avoidDir += away;
        }
    }

    if (avoidDir != Vector2.zero)
    {
        desiredDir += avoidDir * 0.5f; 
        desiredDir.Normalize();
    }

    return desiredDir;
}


}
