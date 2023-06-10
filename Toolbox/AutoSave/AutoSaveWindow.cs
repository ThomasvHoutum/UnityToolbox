using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;

[InitializeOnLoad]
public class AutoSaveProject
{
    public static float saveInterval;
    public static DateTime lastSaveTime;
    public static bool isAutoSaveEnabled;
    public static bool isConsoleOutputEnabled;

    private static double _nextSaveTime;

    static AutoSaveProject()
    {
        saveInterval = EditorPrefs.GetFloat("AutoSaveInterval", 300);
        isAutoSaveEnabled = EditorPrefs.GetBool("AutoSaveEnabled", true);
        isConsoleOutputEnabled = EditorPrefs.GetBool("ConsoleOutputEnabled", true);

        EditorApplication.update += OnUpdate;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;

        ResetSaveCountdown();
    }

    static void OnUpdate()
    {
        if (!isAutoSaveEnabled || EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        if (EditorApplication.timeSinceStartup > _nextSaveTime || (DateTime.Now - lastSaveTime).TotalSeconds > saveInterval)
        {
            ResetSaveCountdown();
            SaveAll();
        }
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
            ResetSaveCountdown();
    }

    static void ResetSaveCountdown() => _nextSaveTime = EditorApplication.timeSinceStartup + saveInterval;

    static void SaveAll()
    {
        if (isConsoleOutputEnabled)
            Debug.Log("AutoSave : " + System.DateTime.Now);

        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();
        lastSaveTime = DateTime.Now;
    }

    public static void SetSaveInterval(float interval)
    {
        saveInterval = interval;
        EditorPrefs.SetFloat("AutoSaveInterval", saveInterval);
        ResetSaveCountdown();
    }

    public static void SetAutoSaveEnabled(bool enabled)
    {
        isAutoSaveEnabled = enabled;
        EditorPrefs.SetBool("AutoSaveEnabled", enabled);
    }

    public static void SetConsoleOutputEnabled(bool enabled)
    {
        isConsoleOutputEnabled = enabled;
        EditorPrefs.SetBool("ConsoleOutputEnabled", enabled);
    }
}

public class AutoSaveWindow : EditorWindow
{
    [MenuItem("Toolbox/AutoSave")]
    public static void ShowWindow() => GetWindow<AutoSaveWindow>("AutoSave");

    void OnGUI()
    {
        GUILayout.Label("AutoSave Settings", EditorStyles.boldLabel);

        bool newEnabled = EditorGUILayout.Toggle("Enable Auto Save", AutoSaveProject.isAutoSaveEnabled);
        if (newEnabled != AutoSaveProject.isAutoSaveEnabled)
            AutoSaveProject.SetAutoSaveEnabled(newEnabled);


        bool newConsoleOutput = EditorGUILayout.Toggle("Enable Console Output", AutoSaveProject.isConsoleOutputEnabled);
        if (newConsoleOutput != AutoSaveProject.isConsoleOutputEnabled)
            AutoSaveProject.SetConsoleOutputEnabled(newConsoleOutput);

        float newInterval = EditorGUILayout.FloatField("Save Interval (Seconds)", AutoSaveProject.saveInterval);
        if (newInterval != AutoSaveProject.saveInterval)
            AutoSaveProject.SetSaveInterval(newInterval);

        if (GUILayout.Button("Close"))
            Close();
    }
}

public class SaveTimePostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) => AutoSaveProject.lastSaveTime = DateTime.Now;
}
