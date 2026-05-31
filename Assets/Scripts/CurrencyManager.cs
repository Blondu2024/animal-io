using UnityEngine;

// Persistent wallet. Credits survive between play sessions via PlayerPrefs.
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;
    public int coins;

    const string KEY = "credits";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(this); return; }
        coins = PlayerPrefs.GetInt(KEY, 0);
    }

    public void Add(int n)
    {
        coins += n;
        PlayerPrefs.SetInt(KEY, coins);
        PlayerPrefs.Save();
    }

    public void ResetWallet()
    {
        coins = 0;
        PlayerPrefs.SetInt(KEY, 0);
        PlayerPrefs.Save();
    }

    void OnGUI()
    {
        // dark plate top-right so the counter is always readable
        const float w = 200f, h = 40f;
        var r = new Rect(Screen.width - w - 12, 12, w, h);
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = Color.white;

        var s = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        s.normal.textColor = new Color(1f, 0.85f, 0.1f);
        GUI.Label(r, "◆ " + coins, s);
    }
}
