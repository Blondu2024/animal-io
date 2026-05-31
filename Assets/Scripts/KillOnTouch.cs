using UnityEngine;

// Put on a trigger collider. Touching the player respawns them.
public class KillOnTouch : MonoBehaviour
{
    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var ex = other.GetComponent<PlatformerExtras>();
        if (ex != null) ex.Respawn();
    }
}
