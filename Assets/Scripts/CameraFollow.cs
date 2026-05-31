using UnityEngine;

/// <summary>
/// Smoothly follows a target from a fixed top-down-ish offset.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 14f, -8f);
    public float smoothTime = 0.15f;

    private Vector3 _velocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desired = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
        transform.LookAt(target.position);
    }
}
