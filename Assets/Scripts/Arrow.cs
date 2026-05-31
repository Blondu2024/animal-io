using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Arrow : MonoBehaviour
{
    public float speed = 32f;
    public float life = 2.5f;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        Destroy(gameObject, life);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        int dmg = PlayerCombat.ArrowDamage;
        var hits = Physics.OverlapSphere(transform.position, 0.9f);
        // prioritize the boss weak point so a hit there always lands before a body/armor hit
        foreach (var c in hits)
        {
            var wp = c.GetComponent<BossWeakPoint>();
            if (wp != null) { GameAudio.Play("weakpoint_hit", transform.position); wp.TakeHit(dmg); Destroy(gameObject); return; }
        }
        foreach (var c in hits)
        {
            var d = c.GetComponentInParent<IDamageable>();
            if (d != null) { GameAudio.Play("arrow_hit", transform.position); d.TakeHit(dmg); Destroy(gameObject); return; }
        }
    }
}
