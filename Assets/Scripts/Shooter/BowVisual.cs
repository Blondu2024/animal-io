using UnityEngine;

// A simple procedural bow drawn with LineRenderers, attached to the hero.
// The bowstring pulls back and a nocked arrow appears while the hero is drawing.
// Lives as a child set under the Player; inherits the player's facing (+Z = aim).
[RequireComponent(typeof(HeroController))]
public class BowVisual : MonoBehaviour
{
    const float BowHeight = 1.1f;   // tip-to-tip
    const float BowDepth = 0.35f;   // how far limbs bow forward
    const float MaxPull = 0.5f;     // how far the string pulls back at full draw
    static readonly Vector3 Offset = new Vector3(0f, 1.0f, 0.6f); // bow center in front of hero

    HeroController hero;
    Transform root;
    LineRenderer limbs;
    LineRenderer bowString;
    Transform nockArrow;

    void Start()
    {
        hero = GetComponent<HeroController>();

        root = new GameObject("Bow").transform;
        root.SetParent(transform, false);

        var mat = new Material(Shader.Find("Sprites/Default"));

        limbs = NewLine("Limbs", mat, 0.05f, new Color(0.40f, 0.26f, 0.12f));   // wood
        bowString = NewLine("String", mat, 0.02f, new Color(0.9f, 0.9f, 0.82f)); // string

        BuildLimbs();

        var a = GameObject.CreatePrimitive(PrimitiveType.Cube);
        a.name = "NockArrow";
        var col = a.GetComponent<Collider>();
        if (col != null) Destroy(col);
        a.transform.SetParent(root, false);
        a.transform.localScale = new Vector3(0.06f, 0.06f, 0.7f);
        ShooterUtil.SetColor(a, new Color(0.85f, 0.7f, 0.45f));
        nockArrow = a.transform;
        nockArrow.gameObject.SetActive(false);
    }

    LineRenderer NewLine(string lname, Material mat, float width, Color c)
    {
        var go = new GameObject(lname);
        go.transform.SetParent(root, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.material = mat;
        lr.widthMultiplier = width;
        lr.numCapVertices = 2;
        lr.startColor = lr.endColor = c;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return lr;
    }

    void BuildLimbs()
    {
        const int seg = 9;
        limbs.positionCount = seg;
        for (int i = 0; i < seg; i++)
        {
            float t = (float)i / (seg - 1);               // 0=bottom .. 1=top
            float y = Offset.y + Mathf.Lerp(-BowHeight * 0.5f, BowHeight * 0.5f, t);
            float z = Offset.z + Mathf.Sin(t * Mathf.PI) * BowDepth; // bows forward in the middle
            limbs.SetPosition(i, new Vector3(Offset.x, y, z));
        }
    }

    void Update()
    {
        if (hero == null) return;

        Vector3 top = new Vector3(Offset.x, Offset.y + BowHeight * 0.5f, Offset.z);
        Vector3 bot = new Vector3(Offset.x, Offset.y - BowHeight * 0.5f, Offset.z);
        Vector3 mid = (top + bot) * 0.5f;

        bool drawing = hero.IsDrawing;
        float pull = drawing ? hero.DrawNormalized * MaxPull : 0f;
        Vector3 nock = mid + new Vector3(0f, 0f, -pull); // pulled back toward the hero

        bowString.positionCount = 3;
        bowString.SetPosition(0, top);
        bowString.SetPosition(1, nock);
        bowString.SetPosition(2, bot);

        nockArrow.gameObject.SetActive(drawing);
        if (drawing)
        {
            // Arrow rests on the string, pointing forward (+Z).
            nockArrow.localPosition = nock + new Vector3(0f, 0f, 0.35f);
            nockArrow.localRotation = Quaternion.identity;
        }
    }
}
