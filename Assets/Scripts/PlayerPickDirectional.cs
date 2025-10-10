using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickDirectional : MonoBehaviour
{
    [Header("Setup")]
    public GridNavFromTilemap grid;
    public PlayerInput playerInput;
    public string moveActionName = "Move";
    public string pickActionName = "Pick";
    public LayerMask berryMask;
    [Range(0f, 1f)] public float dirDeadzone = 0.4f;

    private InputAction move;
    private InputAction pick;
    private PlayerCharacter player; // 👈 reference to your inventory

    void Awake()
    {
        move = playerInput.actions[moveActionName];
        pick = playerInput.actions[pickActionName];
        player = GetComponent<PlayerCharacter>(); // 👈 get reference
    }

    void Update()
    {
        // only trigger if pick button pressed this frame
        if (!pick.WasPressedThisFrame()) return;

        // use current movement direction to decide which grid cell to check
        Vector2 i = move.ReadValue<Vector2>();
        if (i.magnitude < dirDeadzone) return;

        Vector2Int dir = Mathf.Abs(i.x) >= Mathf.Abs(i.y)
            ? (i.x > 0 ? Vector2Int.right : Vector2Int.left)
            : (i.y > 0 ? Vector2Int.up : Vector2Int.down);

        var p = grid.WorldToIndex(transform.position);
        var target = p + dir;
        if (!grid.InBounds(target)) return;

        var world = grid.IndexToWorldCenter(target);
        var hit = Physics2D.OverlapPoint(world, berryMask);

        if (hit && hit.TryGetComponent(out BerryPile pile) && pile.TryPick())
        {
            // 🫐 Add to player's carried berries instead of global score
            if (player != null)
                player.AddBerry(1);

            // 💥 Optional effects / feedback
            BerryFX.PopAt(hit.transform);
            Debug.Log($"Picked 1 {pile.typeId}, left {pile.berries}, carrying {player.berriesCarried}");
        }
    }
}
