using System.Collections.Generic;
using UnityEngine;

// Simple room-graph navigation for the map. Enemies use it to path around walls toward the
// player by stepping through doorways (midpoints between adjacent room centers) via BFS.
public static class MapNav
{
    static Vector3[] centers;
    static List<int>[] adj;
    static float half;

    public static void Init(Vector3[] roomCenters, List<int>[] adjacency, float roomHalf)
    {
        centers = roomCenters;
        adj = adjacency;
        half = roomHalf;
    }

    public static bool Ready => centers != null && centers.Length > 0;

    public static int RoomAt(Vector3 p)
    {
        for (int i = 0; i < centers.Length; i++)
            if (Mathf.Abs(p.x - centers[i].x) <= half && Mathf.Abs(p.z - centers[i].z) <= half)
                return i;

        int best = 0;
        float bd = float.MaxValue;
        for (int i = 0; i < centers.Length; i++)
        {
            float d = (p - centers[i]).sqrMagnitude;
            if (d < bd) { bd = d; best = i; }
        }
        return best;
    }

    // World point to move toward to progress from 'from' to 'to' around the walls.
    public static Vector3 NextWaypoint(Vector3 from, Vector3 to)
    {
        if (!Ready) return to;
        int a = RoomAt(from), b = RoomAt(to);
        if (a == b) return to; // same room: go straight to the target

        int next = NextRoomOnPath(a, b);
        if (next < 0) return to;

        // Aim a bit PAST the doorway (into the next room) so the enemy walks through it
        // instead of stopping on the boundary line.
        Vector3 door = (centers[a] + centers[next]) * 0.5f;
        Vector3 axis = centers[next] - centers[a];
        axis.y = 0f;
        if (axis.sqrMagnitude > 0.0001f) axis.Normalize();
        return door + axis * 3f;
    }

    static int NextRoomOnPath(int start, int goal)
    {
        var prev = new int[centers.Length];
        for (int i = 0; i < prev.Length; i++) prev[i] = -2;

        var q = new Queue<int>();
        q.Enqueue(start);
        prev[start] = -1;
        while (q.Count > 0)
        {
            int c = q.Dequeue();
            if (c == goal) break;
            foreach (int n in adj[c])
                if (prev[n] == -2) { prev[n] = c; q.Enqueue(n); }
        }

        if (prev[goal] == -2) return -1; // unreachable

        int cur = goal;
        while (prev[cur] != start && prev[cur] != -1) cur = prev[cur];
        return cur;
    }
}
