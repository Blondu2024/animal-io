using UnityEngine;

public class Spikes : MonoBehaviour
{
    void OnTriggerEnter(Collider other) => TryHit(other);
    void OnCollisionEnter(Collision col) => TryHit(col.collider);

    void TryHit(Collider c)
    {
        var root = c.transform.root;
        if (!root.CompareTag("Player")) return;
        var pc = root.GetComponent<PlatformerController>();
        if (pc != null) pc.Respawn();
    }
}
