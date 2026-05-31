using UnityEditor;
using UnityEngine;

public static class SyntyImporter
{
    [MenuItem("MCP/Import Synty Starter")]
    public static void ImportSynty()
    {
        string pkg = @"C:\Users\Shadow\Downloads\POLYGON_Starter_Unity_2022_3_v1_2_0.unitypackage";
        if (!System.IO.File.Exists(pkg)) { Debug.LogError("Synty package missing: " + pkg); return; }
        AssetDatabase.ImportPackage(pkg, false); // false = silent, no dialog
        Debug.Log("Synty ImportPackage started (silent).");
    }
}
