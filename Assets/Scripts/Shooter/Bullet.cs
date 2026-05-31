using System.Collections.Generic;
using UnityEngine;

// Arrow projectile: ballistic arc, tilts along its velocity, reliably hits enemies within
// hitRadius, and sticks in walls/ground. Supports upgrade skills: pierce, ricochet, fire (burn).
public class Bullet : MonoBehaviour
{
    public float speed = 16f;
    public float damage = 10f;
    public float lifetime = 3f;
    public float hitRadius = 0.55f;

    const float Gravity = -16f;

    Vector3 dir;
    float vy;
    float age;
    bool resting;

    int pierce;
    int bounce;
    float burnDps;
    float burnDur;
    readonly List<Enemy> hits = new List<Enemy>();

    public void Launch(Vector3 direction, float bulletSpeed, float bulletDamage)
    {
        direction.y = 0f;
        dir = direction.normalized;
        speed = bulletSpeed;
        damage = bulletDamage;
        vy = 3.6f;
        Orient();
    }

    public void Configure(int pierceCount, int bounceCount, float burnDamagePerSec, float burnDuration)
    {
        pierce = pierceCount;
        bounce = bounceCount;
        burnDps = burnDamagePerSec;
        burnDur = burnDuration;
    }

    void Update()
    {
        if (resting)
        {
            age += Time.deltaTime;
            if (age >= lifetime) Destroy(gameObject);
            return;
        }

        transform.position += dir * speed * Time.deltaTime;

        vy += Gravity * Time.deltaTime;
        Vector3 p = transform.position;
        p.y += vy * Time.deltaTime;
        if (p.y <= 0.18f)
        {
            p.y = 0.18f;
            transform.position = p;
            resting = true;
            lifetime = age + 0.6f;
            return;
        }
        transform.position = p;
        Orient();

        // At most one new enemy hit per frame.
        float r2 = hitRadius * hitRadius;
        var list = Enemy.All;
        for (int i = 0; i < list.Count; i++)
        {
            Enemy e = list[i];
            if (e == null || hits.Contains(e)) continue;
            Vector3 d = e.transform.position - transform.position;
            d.y = 0f;
            if (d.sqrMagnitude <= r2)
            {
                e.TakeDamage(damage);
                if (burnDps > 0f) ApplyBurn(e);
                hits.Add(e);

                if (pierce > 0) pierce--;            // keep flying through
                else if (bounce > 0) { bounce--; Retarget(); }
                else Destroy(gameObject);
                return;
            }
        }

        age += Time.deltaTime;
        if (age >= lifetime) Destroy(gameObject);
    }

    void ApplyBurn(Enemy e)
    {
        var burn = e.GetComponent<Burn>();
        if (burn == null) burn = e.gameObject.AddComponent<Burn>();
        burn.Apply(burnDps, burnDur);
    }

    void Retarget()
    {
        Enemy best = null;
        float bestSqr = float.MaxValue;
        var list = Enemy.All;
        for (int i = 0; i < list.Count; i++)
        {
            Enemy e = list[i];
            if (e == null || hits.Contains(e)) continue;
            float ds = (e.transform.position - transform.position).sqrMagnitude;
            if (ds < bestSqr) { bestSqr = ds; best = e; }
        }
        if (best == null) { Destroy(gameObject); return; }

        Vector3 nd = best.transform.position - transform.position;
        nd.y = 0f;
        if (nd.sqrMagnitude > 0.0001f)
        {
            dir = nd.normalized;
            vy = 2.5f; // small hop on bounce
            Orient();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (resting) return;
        if (other.GetComponent<Enemy>() != null) return;          // enemies handled by proximity check
        if (other.GetComponent<HeroController>() != null) return;  // don't collide with the archer
        if (other.GetComponent<EnemyProjectile>() != null) return;
        if (other.GetComponent<Bullet>() != null) return;
        Destroy(gameObject); // wall / obstacle
    }

    void Orient()
    {
        Vector3 vel = dir * speed + Vector3.up * vy;
        if (vel.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(vel);
    }
}
