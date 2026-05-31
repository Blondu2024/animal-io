using UnityEngine;

public class GoalZoneTrigger : MonoBehaviour
{
    public RoomController room;

    void OnTriggerEnter(Collider other)
    {
        if (room == null) return;
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
            room.OnGoalReached();
    }
}
