using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Coin : MonoBehaviour
{
    public int value = 1;
    public float spin = 120f;

    void Update()
    {
        transform.Rotate(Vector3.up * spin * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (CurrencyManager.Instance != null) CurrencyManager.Instance.Add(value);
        GameAudio.Play("coin_pickup", transform.position);
        Destroy(gameObject);
    }
}
