using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tidy
{
    public class MassSelectWindow : EditorWindow
    {
        private Vector2 _scrollPosition; // For scrollable area

        private GameObject _searchedGameObject;
        [SerializeField] private GameObject _toSearchGameObject;
        [SerializeField] private GameObject[] _foundGameObjects;

        private string _selectedTag = "Untagged";
        private string _selectedLayer = "Default";

        private Transform _transformDelta;


        private SerializedObject _serializedObject;
        private SerializedProperty _serializedToSearchGameObject;
        private SerializedProperty _serializedFoundGameObjects;

        [MenuItem("Toolbox/Mass Select")]
        public static void ShowWindow() => GetWindow<MassSelectWindow>("Mass Select");

        void OnEnable()
        {
            _serializedObject = new SerializedObject(this);
            _serializedToSearchGameObject = _serializedObject.FindProperty("_toSearchGameObject");
            _serializedFoundGameObjects = _serializedObject.FindProperty("_foundGameObjects");
            _foundGameObjects = new GameObject[0];
        }


        void OnDisable()
        {
            if (_transformDelta != null)
            {
                DestroyImmediate(_transformDelta.gameObject);
                _transformDelta = null;
            }
        }


        void OnGUI()
        {
            _serializedObject.Update();

            EditorGUILayout.BeginVertical();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.PropertyField(_serializedToSearchGameObject, true);
            EditorGUILayout.PropertyField(_serializedFoundGameObjects, true);

            _selectedTag = EditorGUILayout.TagField("Filter by Tag", _selectedTag);
            _selectedLayer = LayerMask.LayerToName(EditorGUILayout.LayerField("Filter by Layer", LayerMask.NameToLayer(_selectedLayer)));


            if (_toSearchGameObject != null && _toSearchGameObject != _searchedGameObject)
            {
                _searchedGameObject = _toSearchGameObject;
                SearchOnGameObject();
            }
               

            _serializedObject.ApplyModifiedProperties();

            if (_foundGameObjects.Length > 0)
            {
                if (_transformDelta == null)
                {
                    var go = new GameObject("Temp");
                    _transformDelta = go.transform;
                    _transformDelta.position = Vector3.zero;
                    _transformDelta.rotation = Quaternion.identity;
                    _transformDelta.localScale = Vector3.zero;
                }

                _transformDelta.position = EditorGUILayout.Vector3Field("Position Delta", _transformDelta.position);
                _transformDelta.rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation Delta", _transformDelta.rotation.eulerAngles));
                _transformDelta.localScale = EditorGUILayout.Vector3Field("Scale Delta", _transformDelta.localScale);

                if (GUILayout.Button("Apply Transform Delta"))
                {
                    foreach (GameObject obj in _foundGameObjects)
                    {
                        obj.transform.position += _transformDelta.position;
                        obj.transform.rotation *= _transformDelta.rotation;
                        obj.transform.localScale += _transformDelta.localScale;
                    }

                    // Reset the transform delta
                    _transformDelta.position = Vector3.zero;
                    _transformDelta.rotation = Quaternion.identity;
                    _transformDelta.localScale = Vector3.zero;
                }
            }


            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void SearchOnGameObject()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            Array allObjects = activeScene.GetRootGameObjects();
            List<GameObject> tempFoundGameObjects = new List<GameObject>();
            string searchName = CleanName(_toSearchGameObject.name);
            foreach (GameObject obj in allObjects)
            {
                string objName = CleanName(obj.name);
                if (objName.StartsWith(searchName))
                    tempFoundGameObjects.Add(obj);

                CheckChildren(obj.transform, searchName, tempFoundGameObjects);
            }

            _foundGameObjects = tempFoundGameObjects.ToArray();
        }

        private void CheckChildren(Transform parent, string searchName, List<GameObject> foundGameObjects)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                string childName = CleanName(child.gameObject.name);

                // Check tag and layer
                bool matchesTag = child.gameObject.tag == _selectedTag;
                bool matchesLayer = LayerMask.LayerToName(child.gameObject.layer) == _selectedLayer;

                if (childName.StartsWith(searchName) && matchesTag && matchesLayer)
                    foundGameObjects.Add(child.gameObject);

                // Recursively check the children of this child
                CheckChildren(child, searchName, foundGameObjects);
            }
        }

        private string CleanName(string originalName)
        {
            return Regex.Replace(originalName, @" \(\d+\)$", "");
        }
    }
}
