using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class BabyBearBehaviour : MonoBehaviour
{
    [Header("General Settings")]
    public float moveSpeed = 1.5f;
    public float roamRadius = 5f;
    public float pauseTimeMin = 1.5f;
    public float pauseTimeMax = 4f;
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

    private SpriteRenderer sr;
    private Transform player;
    private Vector3 homePos;
    private Vector3 targetPos;
    private bool playerInSight;
    private Coroutine mainRoutine;
    private Coroutine motherBearTimerRoutine;
    private float motherBearTimerRemaining;

    private Coroutine walkAnimRoutine;
    private Coroutine eatAnimRoutine;

    // ✅ animation loops that actually keep running until stopped
    IEnumerator AnimateWalk()
    {
        int frame = 0;
        while (true)
        {
            if (walkSprites != null && walkSprites.Length > 0)
            {
                sr.sprite = walkSprites[frame];
                frame = (frame + 1) % walkSprites.Length;
            }
            yield return new WaitForSeconds(walkFrameRate);
        }
    }

    IEnumerator AnimateEat()
    {
        int frame = 0;
        while (true)
        {
            if (eatSprites != null && eatSprites.Length > 0)
            {
                sr.sprite = eatSprites[frame];
                frame = (frame + 1) % eatSprites.Length;
            }
            yield return new WaitForSeconds(eatFrameRate);
        }
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        homePos = transform.position;
        motherBearTimerRemaining = motherBearSpawnDelay;

        var playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj)
            player = playerObj.transform;
    }

    void Start() => mainRoutine = StartCoroutine(MainRoutine());

    void Update() => HandlePlayerDetection();

    IEnumerator MainRoutine()
    {
        while (true)
        {
            if (playerInSight)
            {
                yield return null;
                continue;
            }

            var pile = FindClosestAvailablePile();
            if (pile)
            {
                pile.TryReserve(name);
                yield return WalkTo(pile.transform.position);
                yield return PickFromPile(pile);
                pile.Unreserve(name);
                yield return new WaitForSeconds(afterPickWait);
            }
            else
            {
                yield return Wander();
            }
        }
    }

    IEnumerator Wander()
    {
        Vector2 randomOffset = Random.insideUnitCircle * roamRadius;
        targetPos = homePos + new Vector3(randomOffset.x, randomOffset.y, 0f);

        // ✅ start animation properly
        if (walkAnimRoutine != null) StopCoroutine(walkAnimRoutine);
        walkAnimRoutine = StartCoroutine(AnimateWalk());
        yield return null; // let animation start

        while (Vector2.Distance(transform.position, targetPos) > 0.1f)
        {
            if (playerInSight) yield break;
            Vector2 dir = (targetPos - transform.position).normalized;
            transform.position += (Vector3)dir * moveSpeed * Time.deltaTime;
            sr.flipX = dir.x > 0;
            yield return null;
        }

        StopWalkAnimation();
        sr.sprite = idleSprite;
        yield return new WaitForSeconds(Random.Range(pauseTimeMin, pauseTimeMax));
    }

    IEnumerator WalkTo(Vector3 destination)
    {
        if (walkAnimRoutine != null) StopCoroutine(walkAnimRoutine);
        walkAnimRoutine = StartCoroutine(AnimateWalk());
        yield return null; // ensure it shows first frame

        while (Vector2.Distance(transform.position, destination) > 0.2f)
        {
            if (playerInSight) yield break;
            Vector2 dir = (destination - transform.position).normalized;
            transform.position += (Vector3)dir * moveSpeed * Time.deltaTime;
            sr.flipX = dir.x > 0;
            yield return null;
        }

        StopWalkAnimation();
        sr.sprite = idleSprite;
    }

    IEnumerator PickFromPile(BerryPile pile)
    {
        if (eatAnimRoutine != null) StopCoroutine(eatAnimRoutine);
        eatAnimRoutine = StartCoroutine(AnimateEat());
        yield return null;

        while (pile && pile.berries > 0)
        {
            if (playerInSight) yield break;
            if (pile.TryPick())
            {
                Debug.Log($"🐻 {name} picked a berry from {pile.name}");
                yield return new WaitForSeconds(pickDelay);
            }
            else break;
        }

        StopEatAnimation();
        sr.sprite = idleSprite;
    }

    // 🧹 helper functions to stop coroutines cleanly
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

    BerryPile FindClosestAvailablePile()
    {
        var all = FindObjectsByType<BerryPile>(FindObjectsSortMode.None);
        BerryPile best = null;
        float bestDist = float.MaxValue;
        foreach (var pile in all)
        {
            if (!pile.IsAvailable(name)) continue;
            float dist = Vector2.Distance(transform.position, pile.transform.position);
            if (dist < searchRadius && dist < bestDist)
            {
                best = pile;
                bestDist = dist;
            }
        }
        return best;
    }

    void HandlePlayerDetection()
    {
        if (!player) return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool wasInSight = playerInSight;
        playerInSight = dist <= playerDetectRadius;

        if (playerInSight && !wasInSight)
        {
            StopWalkAnimation();
            StopEatAnimation();
            sr.sprite = alertSprite;
            Debug.Log("🐻 Baby bear spots the player!");
            PauseBehaviourAndStartMotherTimer();
        }
        else if (!playerInSight && wasInSight)
        {
            sr.sprite = idleSprite;
            Debug.Log("🐻 Player left, bear calms down!");
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
        Vector3 spawnPos = spawnPoint ? spawnPoint.position : transform.position;
        Instantiate(motherBearPrefab, spawnPos, Quaternion.identity);
        Debug.Log("🐻‍❄️ Mother Bear spawned!");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0.8f, 1f, 0.3f);
        Gizmos.DrawWireSphere(Application.isPlaying ? homePos : transform.position, roamRadius);

        Gizmos.color = new Color(1f, 0.4f, 0.3f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, playerDetectRadius);

        Gizmos.color = new Color(0.5f, 1f, 0.5f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}
