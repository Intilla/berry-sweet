using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer), typeof(AudioSource))]
public class PlayerCharacter : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction moveAction;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    [Header("Movement")]
    public float speed = 3f;
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
    public AudioClip loseBasketSound; 

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>(); // 🎵
    }

    void Update()
    {
        move = moveAction.ReadValue<Vector2>();
        bool isMoving = move.sqrMagnitude > 0.01f;

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

        float slowFactor = 0.02f;
        float targetSpeed = Mathf.Max(1.5f, speed - berriesCarried * slowFactor);
        float currentSpeed = Mathf.Lerp(speed, targetSpeed, 0.5f);
        transform.position += (Vector3)(move * currentSpeed * Time.deltaTime);

        if (Mathf.Abs(move.x) > 0.01f)
            spriteRenderer.flipX = move.x > 0;
    }

    public void AddBerry(int amount = 1)
    {
        berriesCarried += amount;
        OnBerriesChanged.Invoke(berriesCarried);

        if (berryPickupSound && audioSource)
            audioSource.PlayOneShot(berryPickupSound);
    }

    public void BankBerries()
    {
        if (berriesCarried > 0 && BasketManager.I)
        {
            BasketManager.I.BankBasket(berriesCarried);
            berriesCarried = 0;
            OnBerriesChanged.Invoke(berriesCarried);

            if (bankBasketSound && audioSource)
                audioSource.PlayOneShot(bankBasketSound);
        }
    }

    public void LoseBasket()
    {
        basketsFilled = Mathf.Max(basketsFilled - 1, 0);
        berriesCarried = 0;
        OnBerriesChanged.Invoke(berriesCarried);
        BasketManager.I.LoseBasket();

        if (loseBasketSound && audioSource)
            audioSource.PlayOneShot(loseBasketSound);
    }
}
