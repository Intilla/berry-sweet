// using System;
// using Unity.Behavior;
// using UnityEngine;
// using Action = Unity.Behavior.Action;
// using Unity.Properties;

// [Serializable, GeneratePropertyBag]
// [NodeDescription(name: "Find Closest BerryPile", story: "Find Closest Available BerryPile", category: "BearAI", id: "77a6b4a403b8154d4a407e9d2e5eed9d")]
// public partial class FindClosestBerryPileAction : Action
// {
//     [SerializeReference] public BlackboardVariable<BerryPile> targetPile;
//     [SerializeReference] public BlackboardVariable<BabyBearBehaviour> bearRef;

//     protected override Status OnStart()
//     {
//         if (bearRef.Value == null)
// bearRef.Value = UnityEngine.Object.FindFirstObjectByType<BabyBearBehaviour>();


//         if (bearRef.Value == null)
//             return Status.Failure;

//         var found = bearRef.Value.FindNearestAvailablePile();
//         if (found == null)
//             return Status.Failure;

//         targetPile.Value = found;
//         return Status.Success;
//     }
// }