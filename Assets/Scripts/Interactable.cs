using UnityEngine;

public class Interactable : MonoBehaviour
{
    public enum Kind { Chest, Loot, Rope, Button, Generic }

    public Kind kind = Kind.Chest;
    public string playerAnimTrigger = "OpenTrig";
    public string prompt = "Open";
    public int coinReward = 5;
    public bool grantSuperArrow;   // opening this also powers up your arrows for a while
    public bool destroyOnUse;
    public Transform doorToOpen;
    public float doorDropY = 3.2f;
    public float actionDuration = 1.1f; // movement locked this long while the action plays (one-shot)

    [Header("Hold-to-operate (e.g. rope)")]
    public bool holdToOperate = false;
    public string holdAnimBool = "Pulling";
    public float operateSpeed = 2f; // how fast the door opens while held
    [System.NonSerialized] public float opened;

    bool used;

    public void Interact()
    {
        if (used) return;

        switch (kind)
        {
            case Kind.Chest:
            case Kind.Loot:
                used = true;
                if (CurrencyManager.Instance != null) CurrencyManager.Instance.Add(coinReward);
                if (grantSuperArrow)
                {
                    var pc = Object.FindFirstObjectByType<PlayerCombat>();
                    if (pc != null) pc.ActivateSuperArrow(8f);
                }
                GameAudio.Play("chest_open", transform.position);
                if (destroyOnUse) Destroy(gameObject);
                break;
            case Kind.Rope:
            case Kind.Button:
                used = true;
                if (doorToOpen != null) doorToOpen.position += Vector3.down * doorDropY;
                break;
        }
    }
}
