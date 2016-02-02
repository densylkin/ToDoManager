using UnityEngine;
using System.Collections;
using Todo.Utils;
using UnityEditor;

namespace Todo.Editor
{
    public static class TodoPreferences
    {

        private static string _dataPath;
        public static string DataPath
        {
            get
            {
                if (string.IsNullOrEmpty(_dataPath))
                    _dataPath = EditorPrefs.GetString("todo_path", @"Assets/ToDo/todo.asset");
                return _dataPath;
            }
            set
            {
                _dataPath = value;
                EditorPrefs.SetString("data_path", value);
            }
        }

        private static string _tempPath = "";

        [PreferenceItem("Todo")]
        public static void OnGUI()
        {
            using (new HorizontalBlock())
            {
                _tempPath = EditorGUILayout.TextField(_tempPath);
                if (GUILayout.Button("Set", EditorStyles.miniButton))
                    DataPath = _tempPath;
            }
        }
    } 
}
