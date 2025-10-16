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
    private PlayerCharacter player;

    void Awake()
    {
        move = playerInput.actions[moveActionName];
        pick = playerInput.actions[pickActionName];
        player = GetComponent<PlayerCharacter>();
    }

    void Update()
    {
        if (!pick.WasPressedThisFrame()) return;
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
            if (player != null)
                player.AddBerry(1);

            BerryFX.PopAt(hit.transform);
        }
    }
}
