using UnityEngine;

// Triggers AFTER the player passes the barrel (advances past triggerZ).
// Spawns zombies behind + a barrier ahead. Kill all zombies -> barrier drops -> panel starts.
public class CombatGate : MonoBehaviour
{
    public float triggerZ = 12f;
    public MovingPanel panel;
    public int zombieCount = 3;
    public float barrierAhead = 10f;
    public float panelAhead = 25f;

    Transform player;
    Transform barrier;
    bool activated;
    bool opened;

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        if (!activated)
        {
            if (player.position.z > triggerZ) Activate();
            return;
        }

        if (opened) return;

        if (Object.FindObjectsByType<Zombie>(FindObjectsSortMode.None).Length == 0)
        {
            opened = true;
            if (barrier != null) barrier.position += Vector3.down * 4f;
            if (panel != null) panel.LaunchFrom(new Vector3(0f, 0f, player.position.z + panelAhead));
        }
    }

    void Activate()
    {
        activated = true;
        float pz = player.position.z;

        barrier = Spawn("GateBarrier", PrimitiveType.Cube,
            new Vector3(0f, 1.7f, pz + barrierAhead), new Vector3(6f, 3.4f, 0.5f),
            new Color(0.5f, 0.15f, 0.15f)).transform;

        for (int i = 0; i < zombieCount; i++)
            Zombie.SpawnMonkey(new Vector3((i - 1) * 1.4f, 1f, pz - 5f - i * 2f), transform); // real hook-monkey model + screech
    }

    GameObject Spawn(string name, PrimitiveType t, Vector3 pos, Vector3 scale, Color col)
    {
        var go = GameObject.CreatePrimitive(t);
        go.name = name; go.transform.SetParent(transform);
        go.transform.position = pos; go.transform.localScale = scale;
        var sh = Shader.Find("Universal Render Pipeline/Lit"); if (sh == null) sh = Shader.Find("Standard");
        go.GetComponent<Renderer>().sharedMaterial = new Material(sh) { color = col };
        return go;
    }
}
