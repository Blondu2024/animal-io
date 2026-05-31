using System.IO;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;

public static class ExportArcherFbx
{
    [MenuItem("MCP/Export Archer FBX (for Mixamo)")]
    public static void Export()
    {
        const string src = "Assets/Models/archer_v2.glb";
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(src);
        if (go == null)
        {
            Debug.LogError("[ExportArcherFbx] Could not load " + src);
            return;
        }

        var inst = (GameObject)PrefabUtility.InstantiatePrefab(go);
        if (inst == null)
        {
            Debug.LogError("[ExportArcherFbx] Could not instantiate.");
            return;
        }

        string outDir = @"C:\Users\Shadow\Desktop\archer_for_mixamo";
        Directory.CreateDirectory(outDir);
        string outPath = Path.Combine(outDir, "archer.fbx");

        string result = ModelExporter.ExportObject(outPath, inst);

        Object.DestroyImmediate(inst);

        if (string.IsNullOrEmpty(result))
            Debug.LogError("[ExportArcherFbx] Export FAILED.");
        else
            Debug.Log("[ExportArcherFbx] EXPORTED -> " + result);
    }
}
