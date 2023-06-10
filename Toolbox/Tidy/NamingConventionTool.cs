using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;

namespace Tidy
{
    public class NamingConventionTool : EditorWindow
    {
        private static string _ignoredDirectoryPath = "";
        private static List<RegexConfig> _regexConfig = new List<RegexConfig>();

        [MenuItem("Toolbox/Tidy/Check Naming")]
        public static void ShowWindow() => CheckNamingConventions();

        private static void CheckNamingConventions()
        {

            string[] allAssets = AssetDatabase.FindAssets("", new[] { "Assets" });

            LoadDataFromJson();

            List<Regex> regexList = new List<Regex>();
            foreach (var item in _regexConfig)
            {
                switch (item.CaseOptions)
                {
                    case RegexConfig.CaseOptionsEnum.PascalCase:
                        regexList.Add(new Regex($"^{item.prefix}[A-Z][a-zA-Z0-9]*{item.suffix}$"));
                        break;
                    case RegexConfig.CaseOptionsEnum.CamelCase:
                        regexList.Add(new Regex($"^{item.prefix}[a-z]+(?:[A-Z][a-z]+)*{item.suffix}$"));
                        break;
                    case RegexConfig.CaseOptionsEnum.KebabCase:
                        regexList.Add(new Regex($"^{item.prefix}[a-z]+(?:-[a-z]+)*{item.suffix}$"));
                        break;
                    case RegexConfig.CaseOptionsEnum.SnakeCase:
                        regexList.Add(new Regex($"^{item.prefix}[a-z]+(?:_[a-z]+)*{item.suffix}$"));
                        break;
                }
            }

            foreach (string guid in allAssets)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = Path.GetFileNameWithoutExtension(path);

                // Ignore assets in this directory
                if (path.StartsWith(_ignoredDirectoryPath) || path.StartsWith("Assets/Editor"))
                    continue;

                foreach (RegexConfig config in _regexConfig)
                {
                    config.fileType = config.fileType[0] == '.'
                        ? config.fileType
                        : '.' + config.fileType;

                    if (path.EndsWith(config.fileType))
                    {
                        if (config.assetDirectory != "" && !path.StartsWith(config.assetDirectory))
                            Debug.LogError($"File is not in correct directory: {path}");

                        Regex regex = regexList[(int)config.CaseOptions];

                        
                        if (!regex.IsMatch(name))
                            Debug.LogError($"File is not following correct naming: {path}");
                    }
                }
            }
        }

        private static void LoadDataFromJson()
        {
            string jsonFilePath = "";
            var assetGUIDs = AssetDatabase.FindAssets("TidyConfig t:json");
            if (assetGUIDs.Length > 0)
                jsonFilePath = AssetDatabase.GUIDToAssetPath(assetGUIDs[0]);
            else
                Debug.LogWarning("No saved TidyConfig found. Please create one with the Tidy Config Window");

            // Read the JSON from the file
            if (File.Exists($"{jsonFilePath}"))
            {
                Debug.Log(jsonFilePath);

                string jsonString = File.ReadAllText($"{jsonFilePath}");

                // Convert the JSON to the string
                _ignoredDirectoryPath = JsonUtility.FromJson<Wrapper>(jsonString).IgnoredDirectoryPath;

                // Convert the JSON to the list
                _regexConfig = JsonUtility.FromJson<Wrapper>(jsonString).List;
            }
        }
    }
}
