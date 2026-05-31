using System.IO;
using UnityEditor;
using UnityEngine;

public static class ImportInvectorFree
{
    [MenuItem("MCP/Import Invector Basic Locomotion FREE")]
    public static void Import()
    {
        string path = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
            "Unity", "Asset Store-5.x", "Invector", "Editor ExtensionsGame Toolkits",
            "Third Person Controller - Basic Locomotion FREE.unitypackage");

        if (!File.Exists(path))
        {
            Debug.LogError("[ImportInvectorFree] Package not found at: " + path);
            return;
        }

        Debug.Log("[ImportInvectorFree] Importing: " + path);
        AssetDatabase.ImportPackage(path, true);
    }
}
