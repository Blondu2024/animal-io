using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Cheap, high-impact visual pass: a global post-processing Volume (bloom + filmic tonemap +
// color grading + vignette) plus atmospheric fog. No Meshy credits — turns gray boxes into "a game".
public static class BuildVisualPass
{
    const string ProfilePath = "Assets/Settings/Level1PostFX.asset";

    [MenuItem("MCP/Build Visual Pass")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Visual] stop play first"); return; }

        // ---- Volume profile (recreated each run) ----
        var folder = "Assets/Settings";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "Settings");
        var existing = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
        if (existing != null) AssetDatabase.DeleteAsset(ProfilePath);

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(profile, ProfilePath);

        var bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(1.4f);
        bloom.threshold.Override(0.85f);
        bloom.scatter.Override(0.7f);
        bloom.tint.Override(new Color(1f, 0.92f, 0.8f));
        AssetDatabase.AddObjectToAsset(bloom, profile);

        var tone = profile.Add<Tonemapping>(true);
        tone.mode.Override(TonemappingMode.ACES);
        AssetDatabase.AddObjectToAsset(tone, profile);

        var grade = profile.Add<ColorAdjustments>(true);
        grade.postExposure.Override(0.15f);
        grade.contrast.Override(18f);
        grade.saturation.Override(8f);
        grade.colorFilter.Override(new Color(1f, 0.97f, 0.92f));
        AssetDatabase.AddObjectToAsset(grade, profile);

        var vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.34f);
        vignette.smoothness.Override(0.45f);
        AssetDatabase.AddObjectToAsset(vignette, profile);

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();

        // ---- Global Volume in the scene ----
        var old = GameObject.Find("PostFX");
        if (old != null) Object.DestroyImmediate(old);
        var go = new GameObject("PostFX");
        var vol = go.AddComponent<Volume>();
        vol.isGlobal = true;
        vol.priority = 1f;
        vol.sharedProfile = profile;

        // ---- Enable post-processing on the main camera ----
        var cam = Camera.main;
        if (cam != null)
        {
            var data = cam.GetUniversalAdditionalCameraData();
            if (data != null) data.renderPostProcessing = true;
        }

        // ---- Atmospheric fog (depth + mystery; traps emerge from the dark ahead) ----
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.05f, 0.06f, 0.09f);
        RenderSettings.fogDensity = 0.011f;

        EditorSceneManager.MarkSceneDirty(cam != null ? cam.gameObject.scene : EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("[Visual] DONE. PostFX volume (bloom+ACES+grade+vignette) + fog. Post-processing on " + (cam != null ? cam.name : "NO CAMERA"));
    }
}
