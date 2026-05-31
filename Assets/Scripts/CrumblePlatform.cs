using UnityEngine;

// A platform over a pit. Step on it and it shakes, then falls ~1s later.
// Keep moving / jump to the next one. Resets to its home spot on respawn.
[RequireComponent(typeof(BoxCollider))]
public class CrumblePlatform : LevelHazard
{
    public float crumbleDelay = 0.9f;
    public float shakeAmp = 0.07f;

    BoxCollider solid;
    Rigidbody rb;
    Vector3 homePos;
    Quaternion homeRot;
    bool armed;
    float timer;

    protected override void Start()
    {
        solid = GetComponent<BoxCollider>();
        homePos = transform.position;
        homeRot = transform.rotation;
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true; rb.useGravity = false;
        base.Start();
    }

    protected override void OnReset()
    {
        armed = false; timer = 0f;
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
        transform.position = homePos;
        transform.rotation = homeRot;
        if (solid != null) solid.enabled = true;
    }

    protected override void Tick()
    {
        if (!armed)
        {
            if (PlayerOnTop()) { armed = true; timer = crumbleDelay; }
            return;
        }

        timer -= Time.deltaTime;
        if (timer > 0f)
        {
            float jx = (Mathf.PerlinNoise(Time.time * 45f, 0.3f) - 0.5f) * shakeAmp;
            float jz = (Mathf.PerlinNoise(0.7f, Time.time * 45f) - 0.5f) * shakeAmp;
            transform.position = homePos + new Vector3(jx, 0f, jz);
        }
        else if (rb != null && rb.isKinematic)
        {
            // drop: let gravity take it and stop blocking the player so they fall too if too slow
            rb.isKinematic = false; rb.useGravity = true;
            if (solid != null) solid.enabled = false;
        }
    }

    bool PlayerOnTop()
    {
        if (player == null || solid == null) return false;
        Bounds b = solid.bounds;
        Vector3 pp = player.position;
        return pp.x > b.min.x - 0.3f && pp.x < b.max.x + 0.3f
            && pp.z > b.min.z - 0.3f && pp.z < b.max.z + 0.3f
            && pp.y > b.max.y - 0.4f && pp.y < b.max.y + 2.0f;
    }
}
