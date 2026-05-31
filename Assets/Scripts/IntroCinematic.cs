using UnityEngine;
using StarterAssets;

// Opening cinematic: the barrel rolls down the corridor smashing breakable walls; zombies are
// revealed at each smash; the camera flies with the barrel. When it finishes, control + camera
// return to the player and normal gameplay (LevelSequencer) begins.
public class IntroCinematic : MonoBehaviour
{
    public Transform barrel;
    public BreakableWall[] walls;     // ordered by Z
    public GameObject[] reveals;      // per wall: a group of zombies, shown when that wall breaks
    public Light mainLight;
    public float barrelSpeed = 13f;
    public float endZ = 90f;

    Transform player; ThirdPersonController tpc; PlatformerExtras pe; PlayerCombat pc;
    FollowCamera follow; Camera cam; LevelSequencer seq;
    int nextWall;
    bool running, returning, done;
    float returnT;

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) { player = p.transform; tpc = p.GetComponent<ThirdPersonController>(); pe = p.GetComponent<PlatformerExtras>(); pc = p.GetComponent<PlayerCombat>(); }
        cam = Camera.main;
        if (cam != null) follow = cam.GetComponent<FollowCamera>();
        seq = Object.FindFirstObjectByType<LevelSequencer>();

        SetControl(false);
        if (follow != null) follow.enabled = false;
        FollowCamera.Locked = true;
        if (seq != null) seq.heldForIntro = true;
        GameAudio.StartLoop("barrel_roll", 1.5f); // the cinematic barrel rumbles down the corridor
        HideSeqBarrel(); // the gameplay dodge barrel must not be visible during the show
        if (reveals != null) foreach (var r in reveals) if (r != null) r.SetActive(false);

        // make sure the scene is lit for the show (override the sequencer's dark start)
        if (mainLight != null) mainLight.intensity = 1.1f;
        RenderSettings.ambientLight = new Color(0.42f, 0.42f, 0.48f);

        running = true;
    }

    void Update()
    {
        if (done) return;
        if ((running || returning) && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))) { Finish(); return; }
        if (mainLight != null && mainLight.intensity < 1f) mainLight.intensity = 1.1f;

        if (running)
        {
            HideSeqBarrel(); // re-hide in case the sequencer's Start ran after ours and re-showed it
            barrel.position += Vector3.forward * barrelSpeed * Time.deltaTime;
            barrel.Rotate(Vector3.right * 300f * Time.deltaTime, Space.World);

            DriveCam(barrel.position + new Vector3(2.6f, 4f, -7.5f), barrel.position + new Vector3(0f, 1f, 9f));

            while (walls != null && nextWall < walls.Length && barrel.position.z >= walls[nextWall].transform.position.z)
            {
                walls[nextWall].Break(barrel.position);
                GameAudio.Play("wall_smash", walls[nextWall].transform.position);
                if (reveals != null && nextWall < reveals.Length && reveals[nextWall] != null) reveals[nextWall].SetActive(true);
                nextWall++;
            }

            if (barrel.position.z >= endZ) { running = false; returning = true; returnT = 0f; }
        }
        else if (returning)
        {
            returnT += Time.deltaTime * 1.1f;
            DriveCam(player.position + new Vector3(0f, 2.5f, -5f), player.position + new Vector3(0f, 1.5f, 6f));
            if (returnT >= 1f) Finish();
        }
    }

    void HideSeqBarrel()
    {
        if (seq != null && seq.barrel != null) seq.barrel.Hide();
    }

    void DriveCam(Vector3 pos, Vector3 look)
    {
        if (cam == null) return;
        cam.transform.position = Vector3.Lerp(cam.transform.position, pos, 5f * Time.deltaTime);
        var dir = look - cam.transform.position;
        if (dir.sqrMagnitude > 0.001f)
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.LookRotation(dir), 5f * Time.deltaTime);
    }

    void Finish()
    {
        done = true;
        // clear any walls still standing + show all reveals (covers the skip case)
        if (walls != null) for (int i = 0; i < walls.Length; i++) if (walls[i] != null && !walls[i].Broken) walls[i].Break(walls[i].transform.position);
        if (reveals != null) foreach (var r in reveals) if (r != null) r.SetActive(true);
        if (barrel != null) barrel.gameObject.SetActive(false);
        GameAudio.StopLoop("barrel_roll");
        if (follow != null) follow.enabled = true;
        FollowCamera.Locked = false;
        SetControl(true);
        if (seq != null) seq.heldForIntro = false;
    }

    void SetControl(bool on)
    {
        if (tpc != null) tpc.enabled = on;
        if (pe != null) pe.enabled = on;
        if (pc != null) pc.enabled = on;
    }
}
