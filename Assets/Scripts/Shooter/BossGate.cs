using System.Collections.Generic;
using UnityEngine;

// A locked gate blocking a doorway into the boss room. Opens (disables) once every other
// room has been cleared, so the boss is always the finale.
public class BossGate : MonoBehaviour
{
    public static readonly List<BossGate> All = new List<BossGate>();

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    public static void OpenAll()
    {
        for (int i = All.Count - 1; i >= 0; i--)
            if (All[i] != null) All[i].gameObject.SetActive(false);
    }
}
