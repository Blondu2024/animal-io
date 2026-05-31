using UnityEngine;

// Third-person follow camera: sits behind and above the player at a configurable tilt,
// keeping a fixed orientation (doesn't spin as the archer re-aims) so combat stays readable.
public class ShooterCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 11f, -12f);
    public float tilt = 42f;     // pitch in degrees (lower = more behind-the-back)
    public float follow = 8f;

    void Start()
    {
        if (target == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) target = p.transform;
        }
        transform.rotation = Quaternion.Euler(tilt, 0f, 0f);
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 want = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, want, 1f - Mathf.Exp(-follow * Time.deltaTime));
        transform.rotation = Quaternion.Euler(tilt, 0f, 0f);
    }
}
