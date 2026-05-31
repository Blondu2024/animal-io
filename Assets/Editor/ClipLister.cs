using UnityEditor;
using UnityEngine;

public static class ClipLister
{
    [MenuItem("MCP/List Character Clips")]
    public static void List()
    {
        var objs = AssetDatabase.LoadAllAssetsAtPath("Assets/Models/character.glb");
        int n = 0;
        foreach (var o in objs)
        {
            if (o is AnimationClip c)
            {
                n++;
                Debug.Log("CLIP[" + n + "]: <" + c.name + "> loop=" + c.isLooping + " len=" + c.length);
            }
        }
        Debug.Log("TOTAL CLIPS: " + n);
    }
}
