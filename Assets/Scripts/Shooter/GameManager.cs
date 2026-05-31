using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int RoomsCleared { get; private set; }
    public int TotalRooms { get; private set; }

    bool won;
    PlatformerController pc;

    void Awake() { Instance = this; }

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) pc = p.GetComponent<PlatformerController>();
    }

    public void SetTotalRooms(int n) { TotalRooms = n; }

    public void OnEnemyKilled() { }
    public void OnPlayerDeath() { }

    public void OnRoomCleared()
    {
        if (won) return;
        RoomsCleared++;
        if (TotalRooms > 0 && RoomsCleared >= TotalRooms) won = true;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (won && kb.rKey.wasPressedThisFrame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // (old shooter-prototype debug HUD removed — the level HUD is now hearts + credits)
}
