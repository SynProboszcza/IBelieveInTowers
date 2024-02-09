using UnityEngine;
using UnityEditor;
using System.IO;

namespace Melon
{

    [ExecuteInEditMode]
    public class PathPopup : EditorWindow
    {
        public static PathPopup Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
                DestroyImmediate(this);
            else
                Instance = this;
        }

        public string path;

        [MenuItem("Tools/Icon Maker/Select Path")]
        public static void ShowWindow()
        {
            GetWindow<PathPopup>(false, "Path Selector", true);
        }

        void OnGUI()
        {
            var headerStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16, fontStyle = FontStyle.Bold };
            var textStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12, fontStyle = FontStyle.Bold };
            var buttonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter, fontSize = 12 };

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Final file destination", headerStyle, GUILayout.ExpandWidth(false));

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField("Please choose a file destination", textStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(25);
            EditorGUILayout.LabelField("Path in project folder + /SavedIcons", textStyle, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Default Path", buttonStyle))
            {
                path = Application.dataPath + "/SavedIcons/";
                if (path.Length != 0)
                    PlayerPrefs.SetString("path", path);
                this.Close();
            }
            EditorGUILayout.LabelField("Path destination outside Unity Project", textStyle, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("AppData Path", buttonStyle))
            {
                //path = EditorUtility.OpenFolderPanel("Choose a file destination", "", "png");
                path = Application.persistentDataPath + "/SavedIcons/";
                if (path.Length != 0)
                    PlayerPrefs.SetString("path", path);
                this.Close();
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Done !", buttonStyle)) this.Close();
        }
    }
}