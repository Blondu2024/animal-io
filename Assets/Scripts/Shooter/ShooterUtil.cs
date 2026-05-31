using UnityEngine;

// Small helpers shared across the shooter prototype.
public static class ShooterUtil
{
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    // URP's Lit shader uses _BaseColor, not the legacy .color. Set both to be safe.
    public static void SetColor(GameObject go, Color c)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;
        var mat = r.material;
        if (mat.HasProperty(BaseColorId)) mat.SetColor(BaseColorId, c);
        mat.color = c;
    }

    // True if a wall/obstacle blocks the straight line from a to b (ignores enemies & the hero).
    public static bool WallBetween(Vector3 a, Vector3 b)
    {
        a.y = 1f; b.y = 1f;
        Vector3 d = b - a;
        float dist = d.magnitude;
        if (dist < 0.6f) return false;

        Vector3 dirn = d / dist;
        Vector3 origin = a + dirn * 0.6f; // step out of the caster's own collider
        float len = dist - 0.6f;
        // Ignore trigger colliders so in-flight arrows/projectiles don't count as walls.
        if (Physics.Raycast(origin, dirn, out RaycastHit hit, len, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.GetComponentInParent<Enemy>() == null &&
                hit.collider.GetComponentInParent<HeroController>() == null)
                return true; // first thing hit is a wall/obstacle
        }
        return false;
    }
}
