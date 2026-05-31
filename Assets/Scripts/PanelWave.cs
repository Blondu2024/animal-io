using UnityEngine;

// Launches all panels at once (they start spread out down the corridor so the
// player sees them coming in the distance). Resets when the player respawns.
public class PanelWave : MonoBehaviour
{
    public MovingPanel[] panels;
    public float startDelay = 3f;

    Transform player;
    float delay;
    float prevZ;
    bool launched;

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
        delay = startDelay;
    }

    void Update()
    {
        if (player == null) return;

        if (player.position.z < 2f && prevZ > 5f) ResetWave();
        prevZ = player.position.z;

        if (!launched)
        {
            delay -= Time.deltaTime;
            if (delay <= 0f)
            {
                launched = true;
                foreach (var p in panels) if (p != null) p.Launch();
            }
        }
    }

    void ResetWave()
    {
        foreach (var p in panels) if (p != null) p.ResetPanel();
        launched = false; delay = startDelay;
    }
}
