using UnityEngine;

// A panel that rolls toward the player. The SAFE spot shifts live every shiftInterval.
//  Gap   - full wall with a gap that jumps left/right (run through it)
//  Jump  - low wall whose height changes tall/short (jump over it)
//  Slide - top bar forces you low + bottom blocks one shifting side (slide under, open side)
public class MovingPanel : MonoBehaviour
{
    public enum Mode { Gap, Jump, Slide }
    public Mode mode = Mode.Gap;
    public Transform a;
    public Transform b;
    public float corridorHalf = 3f;
    public float fullHeight = 3.4f;
    public float speed = 4f;
    public float shiftInterval = 1.4f;

    [HideInInspector] public bool done = true;

    Vector3 startPos;
    Transform player;
    bool moving;
    float shiftTimer;

    void Awake() { startPos = transform.position; }

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
        ResetPanel();
    }

    public void ResetPanel()
    {
        moving = false; done = true;
        if (a) a.gameObject.SetActive(false);
        if (b) b.gameObject.SetActive(false);
    }

    public void Launch()
    {
        transform.position = startPos;
        shiftTimer = shiftInterval;
        Configure();
        moving = true; done = false;
    }

    public void LaunchFrom(Vector3 pos)
    {
        startPos = pos;
        transform.position = pos;
        shiftTimer = shiftInterval;
        Configure();
        moving = true; done = false;
    }

    void Configure()
    {
        if (mode == Mode.Gap)
        {
            float gx = Random.Range(-corridorHalf + 1.3f, corridorHalf - 1.3f);
            SetBlock(a, -corridorHalf, gx - 0.9f, fullHeight, 1.7f);
            SetBlock(b, gx + 0.9f, corridorHalf, fullHeight, 1.7f);
        }
        else if (mode == Mode.Jump)
        {
            float h = Random.Range(0.6f, 1.5f);
            SetBlock(a, -corridorHalf, corridorHalf, h, h * 0.5f);
            if (b) b.gameObject.SetActive(false);
        }
        else // Slide: a top bar -> duck/slide under (gap below ~1.4 clears a crouch)
        {
            SetBlock(a, -corridorHalf, corridorHalf, 2.0f, 2.4f);
            if (b) b.gameObject.SetActive(false);
        }
    }

    void SetBlock(Transform t, float xMin, float xMax, float h, float cy)
    {
        if (t == null) return;
        t.gameObject.SetActive(true);
        float w = Mathf.Max(0.05f, xMax - xMin);
        t.localScale = new Vector3(w, h, 0.5f);
        t.localPosition = new Vector3((xMin + xMax) * 0.5f, cy, 0f);
    }

    void Update()
    {
        if (!moving || player == null) return;

        transform.position += Vector3.back * speed * Time.deltaTime;

        shiftTimer -= Time.deltaTime;
        if (shiftTimer <= 0f) { Configure(); shiftTimer = shiftInterval; }

        if (transform.position.z < player.position.z - 4f)
        {
            moving = false; done = true;
            if (a) a.gameObject.SetActive(false);
            if (b) b.gameObject.SetActive(false);
        }
    }
}
