using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer), typeof(AudioSource))]
public class MotherBearBehaviour : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite roarSprite;
    public Sprite[] walkSprites;
    public Sprite attackSprite;
    public Sprite afterAttackSprite;

    [Header("Audio")]
    public AudioClip roarSound;
    public AudioClip stepSound;
    public AudioClip attackSound;
    public float stepSoundInterval = 0.5f;

    [Header("Animation")]
    public float walkFrameRate = 0.15f;

    [Header("Settings")]
    public float roarDuration = 1.5f;
    public float chaseSpeed = 3.5f;
    public float stopChaseDistance = 0.8f;
    public float attackCooldown = 1.2f;
    public float afterAttackDuration = 2f;
    public float afterCalmDownDuration = 2f;

    [Header("Target")]
    public string playerTag = "Player";

    private SpriteRenderer sr;
    private AudioSource audioSource;
    private Transform player;
    private bool isChasing;
    private bool hasAttacked;
    private float stepTimer;
    public static bool IsActive = false;

    [HideInInspector] public bool playerInBaseZone = false;

    private Coroutine walkAnimRoutine;

    void Awake()
    {
        if (IsActive)
        {
            Destroy(gameObject);
            return;
        }

        IsActive = true;
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (!audioSource)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        var playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj)
            player = playerObj.transform;
    }

    void Start()
    {
        StartCoroutine(BehaviorRoutine());
    }

    IEnumerator BehaviorRoutine()
    {
        sr.sprite = roarSprite;
        if (roarSound) audioSource.PlayOneShot(roarSound);

        yield return new WaitForSeconds(roarDuration);

        isChasing = true;
        StartWalkAnimation();
    }

    void Update()
    {
        if (playerInBaseZone)
        {
            if (isChasing)
            {
                isChasing = false;
                StopWalkAnimation();
                sr.sprite = idleSprite;
                StartCoroutine(CalmDown());
            }
            return;
        }

        if (!isChasing || !player) return;

        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * chaseSpeed * Time.deltaTime;
        sr.flipX = dir.x > 0;

        stepTimer += Time.deltaTime;
        if (stepTimer >= stepSoundInterval)
        {
            stepTimer = 0f;
            if (stepSound) audioSource.PlayOneShot(stepSound);
        }

        if (Vector2.Distance(transform.position, player.position) <= stopChaseDistance && !hasAttacked)
        {
            StartCoroutine(AttackPlayer());
        }
    }

    void OnDestroy()
    {
        if (IsActive)
            IsActive = false;
    }

    IEnumerator AttackPlayer()
    {
        hasAttacked = true;
        isChasing = false;
        StopWalkAnimation();

        sr.sprite = attackSprite ? attackSprite : roarSprite;
        if (attackSound) audioSource.PlayOneShot(attackSound);

        if (!playerInBaseZone)
        {
            PlayerCharacter pc = player.GetComponent<PlayerCharacter>();
            if (pc)
                pc.TakeDamage(1);
        }

        yield return new WaitForSeconds(attackCooldown);

        sr.sprite = afterAttackSprite ? afterAttackSprite : idleSprite;

        yield return new WaitForSeconds(afterAttackDuration);

        Destroy(gameObject);
    }

    public IEnumerator CalmDown()
    {
        isChasing = false;
        hasAttacked = true;
        StopWalkAnimation();

        sr.sprite = afterAttackSprite ? afterAttackSprite : idleSprite;

        yield return new WaitForSeconds(afterCalmDownDuration);

        Destroy(gameObject);
    }

    IEnumerator AnimateWalk()
    {
        int frame = 0;
        while (isChasing && walkSprites.Length > 0)
        {
            sr.sprite = walkSprites[frame];
            frame = (frame + 1) % walkSprites.Length;
            yield return new WaitForSeconds(walkFrameRate);
        }
    }

    void StartWalkAnimation()
    {
        StopWalkAnimation();
        if (walkSprites != null && walkSprites.Length > 0)
            walkAnimRoutine = StartCoroutine(AnimateWalk());
        else
            sr.sprite = walkSprites.Length > 0 ? walkSprites[0] : idleSprite;
    }

    void StopWalkAnimation()
    {
        if (walkAnimRoutine != null)
        {
            StopCoroutine(walkAnimRoutine);
            walkAnimRoutine = null;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!player) return;
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawLine(transform.position, player.position);
        Gizmos.DrawWireSphere(transform.position, stopChaseDistance);
    }
}
