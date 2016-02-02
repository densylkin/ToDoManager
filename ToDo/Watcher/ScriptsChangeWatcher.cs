using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;

[InitializeOnLoad]
public class ScriptsChangeWatcher
{
    private static FileSystemWatcher _watcher;

    static ScriptsChangeWatcher()
    {
        _watcher = new FileSystemWatcher(Application.dataPath, "*.cs");
        _watcher.Changed += OnChanged;
        _watcher.EnableRaisingEvents = true;
        _watcher.IncludeSubdirectories = true;
        Debug.Log("Atatat");
    }

    private static void OnChanged(object obj, FileSystemEventArgs e)
    {
        Debug.Log(e.ChangeType.ToString());
    }
}
