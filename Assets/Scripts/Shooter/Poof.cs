using UnityEngine;

// One fragment of a death burst: flies outward, shrinks, and self-destructs.
public class PoofBit : MonoBehaviour
{
    public Vector3 vel;
    float life = 0.4f;
    float age;

    void Update()
    {
        transform.position += vel * Time.deltaTime;
        vel *= 0.9f;
        transform.localScale *= 0.86f;
        age += Time.deltaTime;
        if (age >= life) Destroy(gameObject);
    }

    // Spawn a small burst of fragments at a position, tinted like the thing that died.
    public static void Spawn(Vector3 pos, Color color, int count = 6)
    {
        for (int i = 0; i < count; i++)
        {
            var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.name = "Poof";
            var col = s.GetComponent<Collider>();
            if (col != null) Destroy(col);

            s.transform.position = pos + Vector3.up * 0.5f;
            s.transform.localScale = Vector3.one * 0.28f;
            ShooterUtil.SetColor(s, color);

            float a = Random.Range(0f, Mathf.PI * 2f);
            Vector3 dir = new Vector3(Mathf.Cos(a), Random.Range(0.4f, 1.2f), Mathf.Sin(a));
            var bit = s.AddComponent<PoofBit>();
            bit.vel = dir * Random.Range(3f, 6f);
        }
    }
}
