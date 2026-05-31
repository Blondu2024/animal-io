using UnityEngine;

// A full-corridor wall that ROLLS toward the player (like the barrel). Its gap
// jumps to a new RANDOM position every few seconds — align with it or get crushed.
public class RandomGapWall : MonoBehaviour
{
    public Transform leftWall;
    public Transform rightWall;
    public float corridorHalf = 3f;
    public float gapHalf = 0.9f;
    public float moveSpeed = 3.5f;
    public float shiftInterval = 2f;
    public float startDelay = 2.5f;

    Transform player;
    Vector3 startPos;
    float gapX;
    float shiftTimer;
    float delayTimer;

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
        startPos = transform.position;
        delayTimer = startDelay;
        NewGap();
    }

    void NewGap()
    {
        gapX = Random.Range(-corridorHalf + gapHalf + 0.4f, corridorHalf - gapHalf - 0.4f);
        shiftTimer = shiftInterval;
        Place(leftWall, -corridorHalf, gapX - gapHalf);
        Place(rightWall, gapX + gapHalf, corridorHalf);
    }

    void Update()
    {
        if (player == null) return;

        if (delayTimer > 0f) { delayTimer -= Time.deltaTime; return; }

        transform.position += Vector3.back * moveSpeed * Time.deltaTime;

        shiftTimer -= Time.deltaTime;
        if (shiftTimer <= 0f) NewGap();

        // passed the player -> reset far and come again with a fresh gap
        if (transform.position.z < player.position.z - 4f)
        {
            transform.position = startPos;
            delayTimer = 0.4f;
            NewGap();
        }
    }

    void Place(Transform t, float xMin, float xMax)
    {
        float w = Mathf.Max(0.02f, xMax - xMin);
        var s = t.localScale; s.x = w; t.localScale = s;
        var lp = t.localPosition; lp.x = (xMin + xMax) * 0.5f; t.localPosition = lp;
    }
}
