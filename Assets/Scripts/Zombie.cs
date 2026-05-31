using UnityEngine;

public class Zombie : MonoBehaviour, IDamageable
{
    public float speed = 1.6f;
    public int health = 2;            // a couple arrow hits (so weapon upgrades still matter)
    public float reachDist = 1.2f;
    public int killReward = 10;
    public float attackInterval = 0.8f;

    Transform player;
    float atkCd;
    Animator anim;

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
        anim = GetComponentInChildren<Animator>();   // monkey model: Move loops by default, Attack on hit
        GameAudio.Play("monkey_screech", transform.position, 0.6f, Random.Range(0.9f, 1.25f)); // screech on spawn
    }

    // Spawn a hook-monkey enemy from the Resources prefab (falls back to a capsule). Shared by
    // LevelSequencer / CombatGate / BossRoom so every corridor + arena enemy is the real model.
    public static Zombie SpawnMonkey(Vector3 pos, Transform parent, float scale = 0.9f)
    {
        var prefab = Resources.Load<GameObject>("Monkey");
        GameObject z = prefab != null ? Instantiate(prefab) : GameObject.CreatePrimitive(PrimitiveType.Capsule);
        z.name = "Monkey";
        if (parent != null) z.transform.SetParent(parent);
        z.transform.position = pos;
        z.transform.localScale = Vector3.one * (prefab != null ? scale : 1f);
        var col = z.GetComponent<CapsuleCollider>(); if (col == null) col = z.AddComponent<CapsuleCollider>();
        col.height = 1.6f; col.radius = 0.4f; col.center = new Vector3(0f, 0.8f, 0f);
        var zc = z.GetComponent<Zombie>(); if (zc == null) zc = z.AddComponent<Zombie>();
        return zc;
    }

    void Update()
    {
        if (player == null) return;
        if (atkCd > 0f) atkCd -= Time.deltaTime;

        Vector3 to = player.position - transform.position; to.y = 0f;
        float d = to.magnitude;
        if (d > reachDist)
        {
            transform.position += to.normalized * speed * Time.deltaTime;
            if (to.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(to), 5f * Time.deltaTime);
        }
        else if (atkCd <= 0f)
        {
            // reached the player -> chip a heart (not instant death anymore)
            if (anim != null) anim.SetTrigger("Attack");
            GameAudio.Play("monkey_screech", transform.position, 0.5f, Random.Range(1.0f, 1.3f));
            var hp = player.GetComponent<PlayerHealth>();
            if (hp != null) hp.Damage(1);
            else { var ex = player.GetComponent<PlatformerExtras>(); if (ex != null) ex.Respawn(); }
            atkCd = attackInterval;
        }
    }

    public void TakeHit(int dmg)
    {
        health -= dmg;
        if (health <= 0)
        {
            GameAudio.Play("zombie_die", transform.position);
            if (CurrencyManager.Instance != null) CurrencyManager.Instance.Add(killReward);
            Destroy(gameObject);
        }
    }
}
