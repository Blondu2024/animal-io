using UnityEditor;
using UnityEngine;

public static class SetMixamoLoops
{
    // clips that must loop while a key is held
    static readonly string[] LoopPaths =
    {
        "Assets/Mixamo/Running Crawl.fbx",
        "Assets/Mixamo/Climbing Ladder.fbx",
        "Assets/Mixamo/Running.fbx",
        "Assets/Mixamo/Pulling A Rope.fbx",
    };

    [MenuItem("MCP/Set Mixamo Loops")]
    public static void Apply()
    {
        foreach (var path in LoopPaths)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) { Debug.LogWarning("[Loops] no importer: " + path); continue; }

            var clips = importer.defaultClipAnimations;
            for (int i = 0; i < clips.Length; i++) clips[i].loopTime = true;
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
            Debug.Log("[Loops] loop=true -> " + path + " (" + clips.Length + " clip)");
        }
        Debug.Log("[Loops] DONE");
    }
}
