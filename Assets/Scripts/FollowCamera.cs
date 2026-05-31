using UnityEngine;

// Mobile-friendly third-person camera.
// - Auto-swings behind the player's facing direction (no input needed).
// - Hold RIGHT mouse button to look around freely (secondary camera); releasing
//   eases back behind the player.
// - Double-click LEFT mouse to toggle a 2X zoom-in (to inspect something).
public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 5f;
    public float lookHeight = 1.4f;
    public float yawDamp = 4f;
    public float pitch = 14f;

    [Header("Mouse look (secondary)")]
    public float mouseSensitivity = 3f;
    public float returnDelay = 1.2f;

    [Header("Combat")]
    public float combatRange = 7f; // when a zombie is this close, frame it

    public static bool Locked; // frozen (no yaw/pitch change) during interactions

    float yaw;
    float curPitch;
    float manualTimer;
    float curDistance;
    bool zoomed;
    float lastClickTime = -1f;
    CharacterController targetCC;

    void Start()
    {
        if (target != null)
        {
            yaw = target.eulerAngles.y;
            targetCC = target.GetComponent<CharacterController>();
        }
        curPitch = pitch;
        curDistance = distance;
    }

    void LateUpdate()
    {
        if (target == null) return;
        float dt = Time.deltaTime;

        // Frozen during interactions (no left/right swing)
        if (!Locked)
        {
            // Mouse look: move the mouse to track zombies; auto-returns when you stop
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");
            if (Mathf.Abs(mx) > 0.01f || Mathf.Abs(my) > 0.01f)
            {
                yaw += mx * mouseSensitivity;
                curPitch = Mathf.Clamp(curPitch - my * mouseSensitivity, -20f, 70f);
                manualTimer = returnDelay;
            }

            if (manualTimer > 0f) manualTimer -= dt;
            else
            {
                // Look down the corridor (+Z) by default; the player aims with the mouse
                yaw = Mathf.LerpAngle(yaw, 0f, yawDamp * dt);
                curPitch = Mathf.Lerp(curPitch, pitch, yawDamp * dt);
            }
        }

        // (left mouse is the attack button now — zoom removed to avoid conflict)
        curDistance = Mathf.Lerp(curDistance, distance, 8f * dt);

        Quaternion rot = Quaternion.Euler(curPitch, yaw, 0f);
        Vector3 focus = target.position + Vector3.up * lookHeight;
        Vector3 desired = focus - (rot * Vector3.forward) * curDistance;

        // Don't let walls block the view: if something is between the player and the
        // camera, pull the camera in front of it.
        int mask = ~(1 << target.gameObject.layer);
        if (Physics.Linecast(focus, desired, out RaycastHit hit, mask, QueryTriggerInteraction.Ignore))
            desired = hit.point + (focus - desired).normalized * 0.3f;

        transform.position = desired;
        transform.LookAt(focus);
    }

    Zombie NearestZombie()
    {
        Zombie best = null;
        float bd = combatRange;
        foreach (var z in Object.FindObjectsByType<Zombie>(FindObjectsSortMode.None))
        {
            float d = Vector3.Distance(target.position, z.transform.position);
            if (d < bd) { bd = d; best = z; }
        }
        return best;
    }
}
