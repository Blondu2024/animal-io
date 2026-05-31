using UnityEngine;

// A wall made of chunks. Break() turns each chunk into physics debris that blasts apart
// (used by the intro cinematic when the barrel smashes through).
public class BreakableWall : MonoBehaviour
{
    public Transform[] chunks;
    public float force = 7f;
    bool broken;

    public bool Broken => broken;

    public void Break(Vector3 from)
    {
        if (broken) return;
        broken = true;
        foreach (var ch in chunks)
        {
            if (ch == null) continue;
            var rb = ch.GetComponent<Rigidbody>(); if (rb == null) rb = ch.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = false; rb.useGravity = true;
            rb.AddExplosionForce(force * 55f, from, 12f, 2.5f);
            rb.AddTorque(Random.insideUnitSphere * 25f);
            Destroy(ch.gameObject, 4f); // clear debris a few seconds later
        }
    }
}
