using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace Tidy
{
    public class TidyConfigWindow : EditorWindow
    {
        private Vector2 _scrollPosition; // For scrollable area
        private SerializedObject _serializedObject;
        private SerializedProperty _serializedRegexConfigItems;
        private SerializedProperty _serializedIgnoredDirectoryPath;

        [SerializeField]
        private List<RegexConfig> _regexConfigItems = new List<RegexConfig>();
        [SerializeField]
        private string _ignoredDirectoryPath = "";
        private GUIStyle _headerStyle;

        [MenuItem("Toolbox/Tidy/Tidy Config")]
        public static void ShowWindow()
        {
            GetWindow<TidyConfigWindow>("Tidy Config");
        }

        void OnEnable()
        {
            _headerStyle = new GUIStyle(EditorStyles.boldLabel);
            _headerStyle.fontSize = 16;
            _serializedObject = new SerializedObject(this);
            _serializedRegexConfigItems = _serializedObject.FindProperty("_regexConfigItems");
            _serializedIgnoredDirectoryPath = _serializedObject.FindProperty("_ignoredDirectoryPath");

            // Load data from JSON file
            LoadDataFromJson();
        }

        void OnGUI()
        {
            _serializedObject.Update();

            EditorGUILayout.BeginVertical();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (GUILayout.Button("Save New Config"))
                CreateNewConfigFile();

            EditorGUILayout.PropertyField(_serializedIgnoredDirectoryPath, true);

            EditorGUILayout.PropertyField(_serializedRegexConfigItems, true);

            _serializedObject.ApplyModifiedProperties();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void CreateNewConfigFile()
        {
            // Convert the list to JSON
            string jsonString = JsonUtility.ToJson(new Wrapper() { 
                IgnoredDirectoryPath = _ignoredDirectoryPath,
                List = _regexConfigItems 
            }, true);

            // Get the path of the script
            string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));

            // Write the JSON to the file
            File.WriteAllText($"{path}/TidyConfig.json", jsonString);
        }

        private void LoadDataFromJson()
        {
            // Get the path of the script
            string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));

            // Read the JSON from the file
            if (File.Exists($"{path}/TidyConfig.json"))
            {
                string jsonString = File.ReadAllText($"{path}/TidyConfig.json");

                // Convert the JSON to the string
                _ignoredDirectoryPath = JsonUtility.FromJson<Wrapper>(jsonString).IgnoredDirectoryPath;

                // Convert the JSON to the list
                _regexConfigItems = JsonUtility.FromJson<Wrapper>(jsonString).List;

                // Update the serialized property
                _serializedRegexConfigItems.ClearArray();
                foreach (RegexConfig item in _regexConfigItems)
                {
                    _serializedRegexConfigItems.arraySize++;
                    SerializedProperty serializedItem = _serializedRegexConfigItems.GetArrayElementAtIndex(_serializedRegexConfigItems.arraySize - 1);
                    serializedItem.FindPropertyRelative("fileType").stringValue = item.fileType;
                    serializedItem.FindPropertyRelative("prefix").stringValue = item.prefix;
                    serializedItem.FindPropertyRelative("CaseOptions").enumValueIndex = (int)item.CaseOptions;
                    serializedItem.FindPropertyRelative("suffix").stringValue = item.suffix;
                    serializedItem.FindPropertyRelative("assetDirectory").stringValue = item.assetDirectory;
                }
            }
        }
    }
}
