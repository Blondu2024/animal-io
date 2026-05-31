using UnityEngine;

// Projectile fired by ranged enemies. Damages the hero on contact, then dies.
public class EnemyProjectile : MonoBehaviour
{
    float speed = 9f;
    float damage = 14f;
    float lifetime = 4f;
    float age;
    Vector3 dir;

    public void Launch(Vector3 direction, float projSpeed, float projDamage)
    {
        dir = direction.normalized;
        speed = projSpeed;
        damage = projDamage;
    }

    void Update()
    {
        transform.position += dir * speed * Time.deltaTime;
        age += Time.deltaTime;
        if (age >= lifetime) Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        var hero = other.GetComponent<HeroController>();
        if (hero != null)
        {
            hero.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
        if (other.GetComponent<Enemy>() != null) return;           // pass through enemies
        if (other.GetComponent<EnemyProjectile>() != null) return; // ignore other shots
        if (other.GetComponent<Bullet>() != null) return;
        Destroy(gameObject); // wall / obstacle -> stopped by walls
    }
}
