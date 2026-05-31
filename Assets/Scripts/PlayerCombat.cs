using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public float attackCooldown = 0.55f;
    public float releaseDelay = 0.3f;

    public static int ArrowDamage = 1;   // arrows read this; super arrow raises it

    Animator anim;
    float cd;
    float superTimer;

    void Start() { anim = GetComponent<Animator>(); }

    public void ActivateSuperArrow(float dur) { superTimer = Mathf.Max(superTimer, dur); }

    void Update()
    {
        if (DeathManager.IsPaused) return;

        if (superTimer > 0f) { superTimer -= Time.deltaTime; ArrowDamage = 5; } else ArrowDamage = 1;
        if (cd > 0f) cd -= Time.deltaTime;
        if (Input.GetMouseButtonDown(0) && cd <= 0f)
        {
            cd = attackCooldown;

            // Aim horizontally where the player is looking (mouse-controlled camera), chest level
            Vector3 aimDir = transform.forward;
            if (Camera.main != null)
            {
                aimDir = Camera.main.transform.forward;
                aimDir.y = 0f;
                if (aimDir.sqrMagnitude < 0.001f) aimDir = transform.forward;
                aimDir.Normalize();
            }
            // face where we shoot
            transform.rotation = Quaternion.LookRotation(aimDir);

            if (anim != null) anim.SetTrigger("AttackTrig");
            Invoke(nameof(Shoot), releaseDelay);
        }
    }

    void Shoot()
    {
        Vector3 aimDir = transform.forward; aimDir.y = 0f;
        if (aimDir.sqrMagnitude < 0.001f) aimDir = Vector3.forward;
        aimDir.Normalize();

        bool super = ArrowDamage > 1;
        var arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arrow.name = "Arrow";
        arrow.transform.localScale = super ? new Vector3(0.13f, 0.13f, 0.85f) : new Vector3(0.07f, 0.07f, 0.6f);
        Vector3 origin = transform.position + Vector3.up * 1.2f + aimDir * 0.6f;
        arrow.transform.position = origin;
        arrow.transform.forward = aimDir;

        var sh = Shader.Find("Universal Render Pipeline/Lit"); if (sh == null) sh = Shader.Find("Standard");
        var amat = new Material(sh) { color = super ? new Color(1f, 0.8f, 0.1f) : new Color(0.18f, 0.13f, 0.08f) };
        if (super) { amat.EnableKeyword("_EMISSION"); amat.SetColor("_EmissionColor", new Color(1.4f, 1f, 0.1f)); }
        arrow.GetComponent<Renderer>().sharedMaterial = amat;

        arrow.AddComponent<Arrow>();

        GameAudio.Play("bow_shoot", origin, super ? 1f : 0.9f, super ? 0.92f : 1f);
    }
}
