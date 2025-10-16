using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Walk To Pile",
    story: "Move towards the target berry pile",
    category: "BearAI",
    id: "bear_walk_to_pile"
)]
public partial class WalkToPileAction : Action
{
    // 👇 These must be PUBLIC and SerializeReference
    [SerializeReference] public BlackboardVariable<BabyBearBehaviour> bearRef;
    [SerializeReference] public BlackboardVariable<BerryPile> targetPile;

    [SerializeField] private float stopDistance = 0.2f;

    protected override Status OnStart()
    {
        if (bearRef?.Value == null || targetPile?.Value == null)
            return Status.Failure;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (bearRef?.Value == null || targetPile?.Value == null)
            return Status.Failure;

        Vector3 target = targetPile.Value.transform.position;
        Vector3 pos = bearRef.Value.transform.position;
        Vector3 dir = (target - pos).normalized;

        bearRef.Value.transform.position += dir * bearRef.Value.moveSpeed * Time.deltaTime;

        if (bearRef.Value.TryGetComponent<SpriteRenderer>(out var sr))
            sr.flipX = dir.x > 0;

        float dist = Vector2.Distance(pos, target);
        return dist <= stopDistance ? Status.Success : Status.Running;
    }
}
