using UnityEngine;

public class RoomController : MonoBehaviour
{
    public Vector3 center;
    public float interiorHalf = 7f;
    public int tier;
    public bool bossRoom;

    bool cleared;
    Transform player;
    GameObject goal;

    public void Configure(Vector3 c, float half, int t, bool boss)
    {
        center = c;
        interiorHalf = half;
        tier = t;
        bossRoom = boss;
        transform.position = c;
    }

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
        SpawnGoal();
    }

    void SpawnGoal()
    {
        Vector3 pos = center + new Vector3(interiorHalf - 1.5f, 0.6f, interiorHalf - 1.5f);
        goal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        goal.name = "GoalZone";
        goal.transform.position = pos;
        goal.transform.localScale = new Vector3(1.4f, 0.6f, 1.4f);
        Color c = bossRoom ? new Color(0.95f, 0.45f, 1f) : new Color(1f, 0.85f, 0.2f);
        ShooterUtil.SetColor(goal, c);
        var col = goal.GetComponent<Collider>();
        col.isTrigger = true;
        var zone = goal.AddComponent<GoalZoneTrigger>();
        zone.room = this;
    }

    public void OnGoalReached()
    {
        if (cleared) return;
        cleared = true;
        if (goal != null) Destroy(goal);
        if (GameManager.Instance != null) GameManager.Instance.OnRoomCleared();
    }

    public void OnEnemyKilled() { }
}
