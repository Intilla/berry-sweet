using UnityEngine;

public class BaseZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerCharacter player = other.GetComponent<PlayerCharacter>();
            if (player != null)
                player.SellBerries();

            MotherBearBehaviour[] bears = Object.FindObjectsByType<MotherBearBehaviour>(FindObjectsSortMode.None);
            foreach (var bear in bears)
            {
                if (bear != null && bear.isActiveAndEnabled)
                    bear.StartCoroutine(bear.CalmDown());
            }
        }
    }
}
