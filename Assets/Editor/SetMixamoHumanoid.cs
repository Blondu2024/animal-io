using UnityEditor;
using UnityEngine;

public static class SetMixamoHumanoid
{
    [MenuItem("MCP/Set Mixamo FBX to Humanoid")]
    public static void Apply()
    {
        var guids = AssetDatabase.FindAssets("t:Model", new[] { "Assets/Mixamo" });
        int done = 0;
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) continue;

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.SaveAndReimport();
            done++;
            Debug.Log("[SetMixamoHumanoid] Humanoid set -> " + path);
        }
        Debug.Log("[SetMixamoHumanoid] DONE. Configured " + done + " models.");
    }
}
