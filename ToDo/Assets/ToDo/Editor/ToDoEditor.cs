using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Todo.Utils;

namespace Todo.Editor
{
    public class ToDoEditor : EditorWindow
    {
        private string _dataPath = @"Assets/ToDo/todo.asset";

        private FileSystemWatcher _watcher;
        private FileInfo[] _files;
        private TodoData _data;

        private string _searchString = "";
        public string SearchString
        {
            get { return _searchString; }
            set
            {
                if (value != _searchString)
                {
                    _searchString = value;
                    RefreshEntriesToShow();
                }
            }
        }

        private string _newTagName = "";

        private Vector2 _sidebarScroll;
        private Vector2 _mainAreaScroll;

        private int _currentTag = -1;
        private TodoEntry[] _entriesToShow;

        private float SidebarWidth
        {
            get { return position.width / 3f; }
        }

        private string[] Tags
        {
            get
            {
                if (_data != null && _data.TagsList.Count > 0)
                    return _data.TagsList.ToArray();
                else
                    return new string[] { "TODO", "BUG" };
            }
        }

        [MenuItem("Tools/Todo")]
        public static void Init()
        {
            var window = GetWindow<ToDoEditor>();
            window.minSize = new Vector2(400, 250);
            window.titleContent = new GUIContent("Todo");
            window.Show();
        }

        private void OnEnable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            RefreshFiles();

            Debug.Log("Atatat");

            _data = ScriptableObjectUtils.LoadOrCreateAsset<TodoData>(_dataPath);
            RefreshEntriesToShow();

            _watcher = new FileSystemWatcher(Application.dataPath, "*.cs");
            _watcher.Changed += OnChanged;
            _watcher.Deleted += OnDeleted;
            _watcher.Renamed += OnRenamed;
            _watcher.Created += OnCreated;

            _watcher.EnableRaisingEvents = true;
            _watcher.IncludeSubdirectories = true;
        }

        private void OnGUI()
        {
            if (_data == null)
            {
                GUILayout.Label("No data loaded", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            Undo.RecordObject(_data, "tododata");

            Toolbar();
            using (new HorizontalBlock())
            {
                Sidebar();
                MainArea();
            }

            EditorUtility.SetDirty(_data);
        }

        #region GUI
        private void Toolbar()
        {
            using (new HorizontalBlock(EditorStyles.toolbar))
            {
                GUILayout.Label("ToDo");
                if (GUILayout.Button("Force scan", EditorStyles.toolbarButton))
                    ScanAllFiles();
                GUILayout.FlexibleSpace();
                SearchString = SearchField(SearchString, GUILayout.Width(250));
            }
        }

        private void Sidebar()
        {
            using (new VerticalBlock(GUI.skin.box, GUILayout.Width(SidebarWidth), GUILayout.ExpandHeight(true)))
            {
                using (new ScrollviewBlock(ref _sidebarScroll))
                {
                    TagField(-1);
                    for (var i = 0; i < _data.TagsCount; i++)
                        TagField(i);
                }
                AddTagField();
            }
        }

        private void MainArea()
        {
            using (new VerticalBlock(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
            {
                using (new ScrollviewBlock(ref _mainAreaScroll))
                    for (var i = 0; i < _entriesToShow.Length; i++)
                        EntryField(i);
            }
        }

        private void TagField(int index)
        {
            Event e = Event.current;
            var tag = index == -1 ? "ALL" : _data.TagsList[index];
            using (new HorizontalBlock(EditorStyles.helpBox))
            {
                using (new ColoredBlock(index == _currentTag ? Color.green : Color.white))
                {
                    GUILayout.Label(tag);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("(" + _data.GetCountByTag(index) + ")");
                }
                if (index != -1 && index != 0 && index != 1)
                {
                    if (GUILayout.Button("x", EditorStyles.miniButton))
                        EditorApplication.delayCall += () =>
                        {
                            _data.RemoveTag(index);
                            Repaint();
                        };
                }
            }
            var rect = GUILayoutUtility.GetLastRect();
            if (e.isMouse && e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
                SetCurrentTag(index);
        }

        private void AddTagField()
        {
            using (new HorizontalBlock(EditorStyles.helpBox))
            {
                _newTagName = EditorGUILayout.TextField(_newTagName);
                if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                {
                    _data.AddTag(_newTagName);
                    _newTagName = "";
                    GUI.FocusControl(null);
                }
            }
        }

        private void EntryField(int index)
        {
            var entry = _entriesToShow[index];
            using (new VerticalBlock(EditorStyles.helpBox))
            {
                using (new HorizontalBlock())
                {
                    GUILayout.Label(entry.Tag, EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(entry.PathToShow, EditorStyles.miniBoldLabel);
                }
                GUILayout.Space(5f);
                GUILayout.Label(entry.Text, EditorStyles.largeLabel);
            }
            Event e = Event.current;
            var rect = GUILayoutUtility.GetLastRect();
            if (e.isMouse && e.type == EventType.MouseDown && rect.Contains(e.mousePosition) && e.clickCount == 2)
                EditorApplication.delayCall += () =>
                {
                    UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(entry.File, entry.Line);
                };
        } 
        #endregion

        #region Files envents handlers
        private void OnChanged(object obj, FileSystemEventArgs e)
        {
            EditorApplication.delayCall += () => ScanFile(e.FullPath);
        }

        private void OnCreated(object obj, FileSystemEventArgs e)
        {
            EditorApplication.delayCall += () => ScanFile(e.FullPath);
        }

        private void OnDeleted(object obj, FileSystemEventArgs e)
        {
            EditorApplication.delayCall += () => _data.Entries.RemoveAll(en => en.File == e.FullPath);
        }

        private void OnRenamed(object obj, FileSystemEventArgs e)
        {
            EditorApplication.delayCall += () => ScanFile(e.FullPath);
        } 
        #endregion

        #region Files Helpers

        private void ScanAllFiles()
        {
            RefreshFiles();
            foreach (var file in _files.Where(file => file.Exists))
            {
                ScanFile(file.FullName);
            }
        }

        private void ScanFile(string filePath)
        {
            var file = new FileInfo(filePath);
            if (!file.Exists)
                return;

            var entries = new List<TodoEntry>();
            _data.Entries.RemoveAll(e => e.File == filePath);

            var parser = new ScriptsParser(filePath, Tags);

            entries.AddRange(parser.Parse());
            var temp = entries.Except(_data.Entries);
            _data.Entries.AddRange(temp);
        }

        private void RefreshFiles()
        {
            var assetsDir = new DirectoryInfo(Application.dataPath);

            _files =
                assetsDir.GetFiles("*.cs", SearchOption.AllDirectories)
                    .Concat(assetsDir.GetFiles("*.js", SearchOption.AllDirectories))
                    .ToArray();
        }

        #endregion

        #region UI helpers

        private void RefreshEntriesToShow()
        {
            if (_currentTag == -1)
                _entriesToShow = _data.Entries.ToArray();
            else if (_currentTag >= 0)
                _entriesToShow = _data.Entries.Where(e => e.Tag == _data.TagsList[_currentTag]).ToArray();
            if (!string.IsNullOrEmpty(SearchString))
            {
                var etmp = _entriesToShow;
                _entriesToShow = etmp.Where(e => e.Text.Contains(_searchString)).ToArray();
            }
        }

        private void SetCurrentTag(int index)
        {
            EditorApplication.delayCall += () =>
            {
                _currentTag = index;
                RefreshEntriesToShow();
                Repaint();
            };
        }

        public static string AssetsRelativePath(string absolutePath)
        {
            if (absolutePath.StartsWith(Application.dataPath))
                return "Assets" + absolutePath.Substring(Application.dataPath.Length);
            else
                throw new System.ArgumentException("Full path does not contain the current project's Assets folder",
                    "absolutePath");
        }

        private string SearchField(string searchStr, params GUILayoutOption[] options)
        {
            searchStr = GUILayout.TextField(searchStr, "ToolbarSeachTextField", options);
            if (GUILayout.Button("", "ToolbarSeachCancelButton"))
            {
                searchStr = "";
                GUI.FocusControl(null);
            }
            return searchStr;
        }

        #endregion
    }

    
}