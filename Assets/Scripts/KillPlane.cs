using UnityEngine;

// A flat trigger under the pits. Falling into it respawns the player.
[RequireComponent(typeof(Collider))]
public class KillPlane : LevelHazard
{
    protected override void Start()
    {
        var c = GetComponent<Collider>(); if (c != null) c.isTrigger = true;
        base.Start();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) Kill();
    }
}
