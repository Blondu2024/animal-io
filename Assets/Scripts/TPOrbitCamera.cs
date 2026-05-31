using UnityEngine;

public class TPOrbitCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 4.5f;
    public float height = 1.5f;
    public float sensitivity = 3f;
    float yaw;
    float pitch = 12f;

    void LateUpdate()
    {
        if (target == null) return;
        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        pitch = Mathf.Clamp(pitch, -10f, 60f);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + Vector3.up * height;
        transform.position = focus - rot * Vector3.forward * distance;
        transform.LookAt(focus);
    }
}
