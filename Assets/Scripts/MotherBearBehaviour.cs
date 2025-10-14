using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer), typeof(AudioSource))]
public class MotherBearBehaviour : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite roarSprite;
    public Sprite walkSprite;
    public Sprite attackSprite;
    public Sprite afterAttackSprite;

    [Header("Audio")]
    public AudioClip roarSound;
    public AudioClip stepSound;
    public AudioClip attackSound;
    public float stepSoundInterval = 0.5f;

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

    [HideInInspector] public bool playerInBaseZone = false;

    void Awake()
    {
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
        Debug.Log("🐻‍❄️ Mother Bear ROARS!");

        yield return new WaitForSeconds(roarDuration);

        isChasing = true;
        sr.sprite = walkSprite;
    }

    void Update()
    {
        // ✅ Stop chasing if player enters BaseZone
        if (playerInBaseZone)
        {
            if (isChasing)
            {
                isChasing = false;
                sr.sprite = idleSprite;
                Debug.Log("🐻‍❄️ Mother Bear stopped chasing — player is in BaseZone!");
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

    IEnumerator AttackPlayer()
    {
        hasAttacked = true;
        isChasing = false;

        sr.sprite = attackSprite ? attackSprite : roarSprite;
        if (attackSound) audioSource.PlayOneShot(attackSound);
        Debug.Log("🐻‍❄️ Mother Bear attacks!");

        if (!playerInBaseZone)
        {
            PlayerCharacter pc = player.GetComponent<PlayerCharacter>();
            if (pc)
                pc.TakeDamage(1);
        }

        yield return new WaitForSeconds(attackCooldown);

        sr.sprite = afterAttackSprite ? afterAttackSprite : idleSprite;
        Debug.Log("🐻‍❄️ Mother Bear calms down...");

        yield return new WaitForSeconds(afterAttackDuration);

        Debug.Log("🐻‍❄️ Mother Bear disappears.");
        Destroy(gameObject);
    }

    // ✅ Separate coroutine for calm down
    public IEnumerator CalmDown()
    {
        StopAllCoroutines(); // stop any roar/chase/attack coroutine
        isChasing = false;
        hasAttacked = false;

        sr.sprite = afterAttackSprite ? afterAttackSprite : idleSprite;
        Debug.Log("🐻‍❄️ Mother Bear calms down (triggered by BaseZone).");

        yield return new WaitForSeconds(afterCalmDownDuration);

        Debug.Log("🐻‍❄️ Mother Bear disappears (after calm).");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (!player) return;
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawLine(transform.position, player.position);
        Gizmos.DrawWireSphere(transform.position, stopChaseDistance);
    }
}
