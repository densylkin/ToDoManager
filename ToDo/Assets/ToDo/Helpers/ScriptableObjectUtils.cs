using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;

public static class ScriptableObjectUtils
{
    public static T CreateAsset<T>(string path) where T : ScriptableObject
    {
        var temp = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(temp, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return temp;
    }

    public static T LoadAsset<T>(string path) where T : ScriptableObject
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    public static T LoadOrCreateAsset<T>(string path) where T : ScriptableObject
    {
        var temp = LoadAsset<T>(path);
        return temp ?? CreateAsset<T>(path);
    }

    public static bool DeleteAsset(string path)
    {
        var file = new FileInfo(path);
        if (file.Exists)
        {
            file.Delete();
            return true;
        }
        else
            return false;
    }
}
