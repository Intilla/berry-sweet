using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer), typeof(AudioSource))]
public class PlayerCharacter : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction runAction;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private float staminaCooldownTimer;

    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    private Vector2 move;
    private Vector2 lastMoveDir = Vector2.down;

    [Header("Inventory")]
    public int berriesCarried = 0;
    public int basketsFilled = 0;
    public int maxBaskets = 3;
    public UnityEvent<int> OnBerriesChanged = new UnityEvent<int>();

    [Header("Sounds")]
    public AudioClip berryPickupSound;
    public AudioClip bankBasketSound;
    public AudioClip sellSound;
    public AudioClip loseBasketSound;
    public AudioClip hurtSound;
    public AudioClip healSound;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaDrainPerSecond = 20f;
    public float staminaRegenPerSecond = 10f;
    public float staminaRegenDelay = 2f;
    public Slider staminaBar;

    private float currentStamina;
    private bool isRunning;

    [Header("Economy")]
    public int totalCoins = 0;
    public int berryBaseValue = 10;
    public CoinPopupUI coinPopup;

    private int pendingBerriesToSell = 0;
    private Coroutine sellingCoroutine;

    [Header("Lives ❤️")]
    public int maxLives = 3;
    public int lives = 3;
    public UnityEvent<int> OnLivesChanged = new UnityEvent<int>();

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        runAction = playerInput.actions["Run"];
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        currentStamina = maxStamina;
        UpdateStaminaUI();



        lives = maxLives;
        PlayerPrefs.SetInt("PlayerLives", lives);
        PlayerPrefs.Save();
        
        OnLivesChanged.Invoke(lives);
    }

    void Update()
    {
        move = moveAction.ReadValue<Vector2>();
        bool isMoving = move.sqrMagnitude > 0.01f;
        isRunning = runAction.IsPressed() && currentStamina > 0f;

        if (isMoving)
        {
            lastMoveDir = move.normalized;
            animator.SetFloat("MoveX", move.x);
            animator.SetFloat("MoveY", move.y);
        }
        else
        {
            animator.SetFloat("MoveX", lastMoveDir.x);
            animator.SetFloat("MoveY", lastMoveDir.y);
        }

        animator.SetFloat("LastMoveX", lastMoveDir.x);
        animator.SetFloat("LastMoveY", lastMoveDir.y);
        animator.SetBool("IsMoving", isMoving);

        HandleStamina(isMoving);

        float baseSpeed = isRunning ? runSpeed : walkSpeed;
        float slowFactor = 0.02f;
        float targetSpeed = Mathf.Max(1.5f, baseSpeed - berriesCarried * slowFactor);
        float currentSpeed = Mathf.Lerp(baseSpeed, targetSpeed, 0.5f);
        transform.position += (Vector3)(move * currentSpeed * Time.deltaTime);

        if (Mathf.Abs(move.x) > 0.01f)
            spriteRenderer.flipX = move.x > 0;
    }

    public void TakeDamage(int amount = 1)
    {
        lives = Mathf.Max(0, lives - amount);
        OnLivesChanged.Invoke(lives);
        PlayerPrefs.SetInt("PlayerLives", lives);

        if (hurtSound && audioSource)
            audioSource.PlayOneShot(hurtSound);

        Debug.Log($"Player took damage! Lives left: {lives}");

        if (lives <= 0)
            OnPlayerDeath();
    }

    public void GainLife(int amount = 1)
    {
        lives = Mathf.Min(maxLives, lives + amount);
        OnLivesChanged.Invoke(lives);
        PlayerPrefs.SetInt("PlayerLives", lives);

        if (healSound && audioSource)
            audioSource.PlayOneShot(healSound);

        Debug.Log($"Gained a life! Lives: {lives}/{maxLives}");
    }

    void OnPlayerDeath()
    {
        Debug.Log("Player has died!");
        // TODO: Add respawn, game over screen, etc.
    }

    void HandleStamina(bool isMoving)
    {
        if (isRunning && isMoving)
        {
            currentStamina -= staminaDrainPerSecond * Time.deltaTime;
            staminaCooldownTimer = staminaRegenDelay;
        }
        else
        {
            if (staminaCooldownTimer > 0f)
                staminaCooldownTimer -= Time.deltaTime;
            else
                currentStamina += staminaRegenPerSecond * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        UpdateStaminaUI();
    }

    void UpdateStaminaUI()
    {
        if (!staminaBar) return;

        float normalized = currentStamina / maxStamina;
        staminaBar.value = normalized;

        bool showBar = isRunning && normalized > 0f;

        CanvasGroup cg = staminaBar.GetComponentInParent<CanvasGroup>();
        if (cg)
        {
            float targetAlpha = showBar ? 1f : 0f;
            cg.alpha = Mathf.MoveTowards(cg.alpha, targetAlpha, Time.deltaTime * 6f);
        }
        else
        {
            staminaBar.gameObject.SetActive(showBar);
        }
    }

    public void AddBerry(int amount = 1)
    {
        berriesCarried += amount;
        OnBerriesChanged.Invoke(berriesCarried);

        if (berryPickupSound && audioSource)
            audioSource.PlayOneShot(berryPickupSound);
    }

    public void SellBerries()
    {
        if (berriesCarried <= 0) return;

        if (bankBasketSound && audioSource)
            audioSource.PlayOneShot(bankBasketSound);

        pendingBerriesToSell += berriesCarried;
        OnBerriesChanged.Invoke(0);
        berriesCarried = 0;

        Debug.Log($"Added {pendingBerriesToSell} berries to pending sale");

        if (sellingCoroutine == null)
            sellingCoroutine = StartCoroutine(SellBerriesDelayed());
    }

    private IEnumerator SellBerriesDelayed()
    {
        while (pendingBerriesToSell > 0)
        {
            int initialCount = pendingBerriesToSell;
            float delay = Mathf.Clamp(initialCount * 0.4f, 2f, 6f);
            float elapsed = 0f;

            while (elapsed < delay)
            {
                if (pendingBerriesToSell > initialCount)
                {
                    initialCount = pendingBerriesToSell;
                    delay = Mathf.Clamp(initialCount * 0.4f, 2f, 6f);
                    elapsed = 0f;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            int berriesToSell = pendingBerriesToSell;
            pendingBerriesToSell = 0;

            float multiplier = Mathf.Lerp(1f, 2f, Mathf.Clamp01((berriesToSell - 1) / 10f));
            int earned = Mathf.RoundToInt(berriesToSell * berryBaseValue * multiplier);
            totalCoins += earned;

            PlayerPrefs.SetInt("LastMoney", totalCoins);
            PlayerPrefs.Save();

            if (sellSound && audioSource)
                audioSource.PlayOneShot(sellSound);

            if (coinPopup)
            {
                string message = $"+{earned} coins! (x{multiplier:F1})";
                coinPopup.ShowPopup(message);
            }

            yield return new WaitForSeconds(0.2f);
        }

        sellingCoroutine = null;
    }
}