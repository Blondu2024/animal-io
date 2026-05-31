using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ArcherController : MonoBehaviour
{
    [Header("Movement")]
    public float runSpeed = 4.5f;
    public float crawlSpeed = 1.8f;
    public float rotationSpeed = 12f;
    public float jumpHeight = 1.3f;
    public float gravity = -20f;

    [Header("Slide / Dash")]
    public float slideBurst = 8f;
    public float slideTime = 0.45f;
    public float slideCooldown = 0.6f;

    CharacterController cc;
    Animator anim;
    Transform cam;
    float vy;
    float slideTimer;
    float slideCd;
    Vector3 slideDir;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        if (Camera.main != null) cam = Camera.main.transform;
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camF = cam ? cam.forward : Vector3.forward;
        Vector3 camR = cam ? cam.right : Vector3.right;
        camF.y = 0; camR.y = 0; camF.Normalize(); camR.Normalize();
        Vector3 move = camF * v + camR * h;
        if (move.sqrMagnitude > 1f) move.Normalize();

        bool crawl = Input.GetKey(KeyCode.C);
        bool climb = Input.GetKey(KeyCode.E);
        anim.SetBool("Crawl", crawl);
        anim.SetBool("Climb", climb);

        float speed = crawl ? crawlSpeed : runSpeed;
        Vector3 horiz = move * speed;

        // Slide / dash on Left Shift
        if (slideCd > 0f) slideCd -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.LeftShift) && slideCd <= 0f && move.sqrMagnitude > 0.1f)
        {
            slideTimer = slideTime;
            slideCd = slideCooldown;
            slideDir = move.normalized;
            anim.SetTrigger("Slide");
        }
        if (slideTimer > 0f)
        {
            slideTimer -= Time.deltaTime;
            horiz = slideDir * slideBurst;
        }

        // Face movement direction
        if (move.sqrMagnitude > 0.01f)
        {
            var target = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.deltaTime);
        }

        // Climb overrides gravity; otherwise jump + gravity
        if (climb)
        {
            vy = 2.5f;
        }
        else
        {
            if (cc.isGrounded)
            {
                if (vy < 0f) vy = -2f;
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    vy = Mathf.Sqrt(jumpHeight * -2f * gravity);
                    anim.SetTrigger("JumpTrig");
                }
            }
            vy += gravity * Time.deltaTime;
        }

        cc.Move((horiz + Vector3.up * vy) * Time.deltaTime);

        Vector3 flat = horiz; flat.y = 0;
        anim.SetFloat("Speed", flat.magnitude);
    }
}
