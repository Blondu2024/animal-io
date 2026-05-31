using UnityEngine;

// God-of-War-style boss. Armored and unhittable while guarding. It telegraphs a heavy axe slam
// (raise weapon ~0.85s -> SLAM AoE you must dodge), then is STUNNED with its weak point exposed —
// shoot the glowing core during that window. Hitting the body any other time just clinks (no damage).
public class Boss : MonoBehaviour, IDamageable
{
    public int maxHealth = 5;
    public float moveSpeed = 2.6f;
    public float attackRange = 3.6f;
    public float windupTime = 0.85f;
    public float slamRadius = 4.2f;
    public float exposedTime = 2.6f;
    public float recoverTime = 0.7f;

    [HideInInspector] public int health;

    public bool IsExposed => state == State.Exposed;
    public float HealthFrac => (float)health / Mathf.Max(1, maxHealth);

    enum State { Approach, Windup, Slam, Exposed, Recover }
    State state = State.Approach;
    float timer, flash, roarTimer;
    Transform player; PlatformerExtras pe;
    Transform weapon; Material bodyMat, weakMat;
    Animator anim; bool dead;

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) { player = p.transform; pe = p.GetComponent<PlatformerExtras>(); }
        health = maxHealth;
        var smr = GetComponentInChildren<SkinnedMeshRenderer>(); if (smr != null) bodyMat = smr.material;
        anim = GetComponentInChildren<Animator>();
        BuildWeakPoint();           // real orangutan model slams with fists -> no code-built axe
        GameAudio.Play("boss_roar", transform.position, 1f); // entrance presence
        roarTimer = 3.5f;
    }

    void BuildWeapon()
    {
        var sh = Shader.Find("Universal Render Pipeline/Lit"); if (sh == null) sh = Shader.Find("Standard");
        weapon = new GameObject("Weapon").transform;
        weapon.SetParent(transform, false);
        weapon.localPosition = new Vector3(0.5f, 0.1f, 0.3f);

        var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        if (handle.GetComponent<Collider>() != null) Destroy(handle.GetComponent<Collider>());
        handle.transform.SetParent(weapon, false);
        handle.transform.localScale = new Vector3(0.07f, 0.07f, 1.5f);
        handle.transform.localPosition = new Vector3(0f, 0f, 0.45f);
        handle.GetComponent<Renderer>().sharedMaterial = new Material(sh) { color = new Color(0.25f, 0.17f, 0.1f) };

        var blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
        if (blade.GetComponent<Collider>() != null) Destroy(blade.GetComponent<Collider>());
        blade.transform.SetParent(weapon, false);
        blade.transform.localScale = new Vector3(0.55f, 0.5f, 0.18f);
        blade.transform.localPosition = new Vector3(0.18f, 0f, 1.05f);
        blade.GetComponent<Renderer>().sharedMaterial = new Material(sh) { color = new Color(0.62f, 0.62f, 0.68f) };
    }

    void BuildWeakPoint()
    {
        var sh = Shader.Find("Universal Render Pipeline/Lit"); if (sh == null) sh = Shader.Find("Standard");
        var wp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        wp.name = "WeakPoint";
        wp.transform.SetParent(transform, false);
        wp.transform.localScale = Vector3.one * 0.5f;
        wp.transform.localPosition = new Vector3(0f, -0.45f, 0.5f); // belly-front, ~player arrow height after scaling
        var col = wp.GetComponent<Collider>(); if (col != null) col.isTrigger = true;
        weakMat = wp.GetComponent<Renderer>().material;
        wp.AddComponent<BossWeakPoint>().boss = this;
    }

    void Update()
    {
        if (dead || player == null) return;
        Vector3 to = player.position - transform.position; to.y = 0f;
        float dist = to.magnitude;
        if (state != State.Slam && to.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(to), 3.5f * Time.deltaTime);

        timer -= Time.deltaTime;
        switch (state)
        {
            case State.Approach:
                roarTimer -= Time.deltaTime;
                if (roarTimer <= 0f) { roarTimer = Random.Range(4f, 6f); GameAudio.Play("boss_roar", transform.position, 0.85f); }
                if (dist > attackRange) transform.position += to.normalized * moveSpeed * Time.deltaTime;
                else { state = State.Windup; timer = windupTime; GameAudio.Play("boss_windup", transform.position); if (anim != null) anim.SetTrigger("Attack"); }
                break;
            case State.Windup:
                SetWeapon(1f - Mathf.Clamp01(timer / windupTime)); // raise overhead = telegraph
                if (timer <= 0f) { DoSlam(); state = State.Slam; timer = 0.25f; }
                break;
            case State.Slam:
                SetWeapon(0f); // crashed down
                if (timer <= 0f) { state = State.Exposed; timer = exposedTime; }
                break;
            case State.Exposed:
                SetWeapon(0f);
                if (timer <= 0f) { state = State.Recover; timer = recoverTime; }
                break;
            case State.Recover:
                if (timer <= 0f) state = State.Approach;
                break;
        }
        UpdateGlow();
    }

    void DoSlam()
    {
        if (player == null) return;
        GameAudio.Play("boss_slam", transform.position);
        Vector3 d = player.position - transform.position; d.y = 0f;
        if (d.magnitude > slamRadius) return;
        var hp = player.GetComponent<PlayerHealth>();
        if (hp != null) hp.Damage(2);
        else if (pe != null) pe.Respawn();
    }

    void SetWeapon(float raise) // 0 = down, 1 = overhead
    {
        if (weapon != null) weapon.localRotation = Quaternion.Euler(-115f * raise, 0f, 0f);
    }

    void UpdateGlow()
    {
        if (flash > 0f) flash -= Time.deltaTime * 3f;
        bool ex = state == State.Exposed;
        if (weakMat != null)
        {
            float pulse = 0.6f + 0.4f * Mathf.Sin(Time.time * 9f);
            weakMat.color = ex ? new Color(1f, 0.9f, 0.12f) : new Color(0.16f, 0.12f, 0.05f);
            weakMat.EnableKeyword("_EMISSION");
            weakMat.SetColor("_EmissionColor", ex ? new Color(1.7f, 1.4f, 0.12f) * pulse : Color.black);
        }
        if (bodyMat != null)
            bodyMat.color = Color.Lerp(Color.white, new Color(1f, 0.4f, 0.4f), Mathf.Clamp01(flash)); // flash red on hit, else show fur
    }

    // Body hit: armored, never damages — just a clink flash so the player learns it's protected.
    public void TakeHit(int dmg) { flash = 1f; GameAudio.Play("armor_clink", transform.position); }

    // Weak point hit (via BossWeakPoint relay): only damages while exposed.
    public void HitWeakPoint(int dmg)
    {
        flash = 1f;
        if (state != State.Exposed) return;
        health -= dmg;
        if (health <= 0) { dead = true; GameAudio.Play("boss_die", transform.position); if (anim != null) anim.SetTrigger("Die"); Destroy(gameObject, 1.4f); }
    }
}
