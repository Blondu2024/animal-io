using System.IO;
using UnityEditor;
using UnityEngine;

public static class ImportStarterAssets
{
    [MenuItem("MCP/Import Starter Assets ThirdPerson")]
    public static void Import()
    {
        string path = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
            "Unity", "Asset Store-5.x", "Unity Technologies", "Unity Essentials",
            "Starter Assets - ThirdPerson URP.unitypackage");

        if (!File.Exists(path))
        {
            Debug.LogError("[ImportStarterAssets] Package not found at: " + path);
            return;
        }

        Debug.Log("[ImportStarterAssets] Importing: " + path);
        AssetDatabase.ImportPackage(path, true);
    }
}
