using System.Collections.Generic;
using UnityEngine;

// Builds a small map of 10 connected rooms (a ring on a 4x3 grid). Each room has walls
// with doorways toward its neighbours, a floor, and a RoomController. Open doors: you
// roam freely; a room activates (spawns enemies) the first time you enter it.
public class MapBuilder : MonoBehaviour
{
    public float roomSize = 16f;
    public float wallThick = 1f;
    public float wallHeight = 2.5f;
    public float doorWidth = 4f;

    // Ring of 10 rooms on a 4x3 grid (cols 0-3, rows 0-2); middle of the ring is hollow.
    static readonly Vector2Int[] cells =
    {
        new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(3,0),
        new Vector2Int(0,1),                                            new Vector2Int(3,1),
        new Vector2Int(0,2), new Vector2Int(1,2), new Vector2Int(2,2), new Vector2Int(3,2),
    };

    HashSet<Vector2Int> set;

    void Awake()
    {
        set = new HashSet<Vector2Int>(cells);
        Build();
    }

    bool Has(int c, int r) => set.Contains(new Vector2Int(c, r));
    Vector3 CenterOf(Vector2Int cell) => new Vector3(cell.x * roomSize, 0f, -cell.y * roomSize);

    void Build()
    {
        float half = roomSize * 0.5f;
        float interiorHalf = half - wallThick;
        var parent = new GameObject("Rooms").transform;

        for (int idx = 0; idx < cells.Length; idx++)
        {
            Vector2Int cell = cells[idx];
            Vector3 center = CenterOf(cell);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor_" + idx;
            floor.transform.SetParent(parent, false);
            floor.transform.position = center + new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(roomSize, 1f, roomSize);
            ShooterUtil.SetColor(floor, new Color(0.32f, 0.30f, 0.27f));

            BuildWall(parent, center, half, Has(cell.x, cell.y - 1), true,  1f);  // North (+Z)
            BuildWall(parent, center, half, Has(cell.x, cell.y + 1), true, -1f);  // South (-Z)
            BuildWall(parent, center, half, Has(cell.x + 1, cell.y), false, 1f);  // East  (+X)
            BuildWall(parent, center, half, Has(cell.x - 1, cell.y), false,-1f);  // West  (-X)

            var ro = new GameObject("Room_" + idx);
            ro.transform.SetParent(parent, false);
            var rc = ro.AddComponent<RoomController>();
            rc.Configure(center, interiorHalf, idx, idx == cells.Length - 1); // last room = boss

        }

        // Build the room graph for enemy navigation (orthogonal neighbours are connected).
        var centersArr = new Vector3[cells.Length];
        var adjList = new List<int>[cells.Length];
        for (int i = 0; i < cells.Length; i++) { centersArr[i] = CenterOf(cells[i]); adjList[i] = new List<int>(); }
        for (int i = 0; i < cells.Length; i++)
            for (int j = 0; j < cells.Length; j++)
            {
                if (i == j) continue;
                int md = Mathf.Abs(cells[i].x - cells[j].x) + Mathf.Abs(cells[i].y - cells[j].y);
                if (md == 1) adjList[i].Add(j);
            }
        MapNav.Init(centersArr, adjList, roomSize * 0.5f);

        var p = GameObject.FindWithTag("Player");
        if (p != null) p.transform.position = CenterOf(cells[0]) + Vector3.up * 1f;
    }

    void Start()
    {
        // Set after all Awakes so GameManager.Instance exists.
        if (GameManager.Instance != null) GameManager.Instance.SetTotalRooms(cells.Length);
    }

    // horizontal = wall runs along X (north/south side); axisSign places it +/- along its axis.
    void BuildWall(Transform parent, Vector3 center, float half, bool hasNeighbor, bool horizontal, float axisSign)
    {
        Vector3 wallCenter = horizontal
            ? center + new Vector3(0f, wallHeight * 0.5f, axisSign * half)
            : center + new Vector3(axisSign * half, wallHeight * 0.5f, 0f);

        if (!hasNeighbor)
        {
            Vector3 scale = horizontal
                ? new Vector3(roomSize, wallHeight, wallThick)
                : new Vector3(wallThick, wallHeight, roomSize);
            CreateSeg(parent, wallCenter, scale);
            return;
        }

        // Two segments leaving a central doorway gap.
        float segLen = (roomSize - doorWidth) * 0.5f;
        float off = (doorWidth + segLen) * 0.5f;
        if (horizontal)
        {
            CreateSeg(parent, wallCenter + new Vector3(-off, 0f, 0f), new Vector3(segLen, wallHeight, wallThick));
            CreateSeg(parent, wallCenter + new Vector3( off, 0f, 0f), new Vector3(segLen, wallHeight, wallThick));
        }
        else
        {
            CreateSeg(parent, wallCenter + new Vector3(0f, 0f, -off), new Vector3(wallThick, wallHeight, segLen));
            CreateSeg(parent, wallCenter + new Vector3(0f, 0f,  off), new Vector3(wallThick, wallHeight, segLen));
        }
    }

    void CreateSeg(Transform parent, Vector3 pos, Vector3 scale)
    {
        var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
        w.name = "Wall";
        w.transform.SetParent(parent, false);
        w.transform.position = pos;
        w.transform.localScale = scale;
        ShooterUtil.SetColor(w, new Color(0.20f, 0.18f, 0.16f));
    }

    void BuildBossGate(Transform parent, Vector3 center, float half, bool horizontal, float axisSign)
    {
        Vector3 pos = horizontal
            ? center + new Vector3(0f, wallHeight * 0.5f, axisSign * half)
            : center + new Vector3(axisSign * half, wallHeight * 0.5f, 0f);
        Vector3 scale = horizontal
            ? new Vector3(doorWidth, wallHeight, wallThick)
            : new Vector3(wallThick, wallHeight, doorWidth);

        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = "BossGate";
        g.transform.SetParent(parent, false);
        g.transform.position = pos;
        g.transform.localScale = scale;
        ShooterUtil.SetColor(g, new Color(0.8f, 0.65f, 0.1f)); // golden locked gate
        g.AddComponent<BossGate>();
    }
}
