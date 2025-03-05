
using UnityEngine;
using UnityEditor;

namespace ProjectCore.Module.Editor
{
    [CustomEditor(typeof(MInitializerSO))]
    internal class MInitializerSOEditor : UnityEditor.Editor
    {
        private const string RESOURCE_PATH = "Initializer";
        private SerializedProperty _initializeScopeProp;
        private SerializedProperty _autoInitializeProp;

        private void OnEnable()
        {
            _initializeScopeProp = serializedObject.FindProperty("_initializeScope");
            _autoInitializeProp = serializedObject.FindProperty("_autoInitialize");
            
            if (!_initializeScopeProp.objectReferenceValue)
            {
                LoadInitializerPrefab();
            }
        }

        private void LoadInitializerPrefab()
        {
            var prefab = Resources.Load<ProjectInitializeLifetimeScope>(RESOURCE_PATH);
            if (prefab)
            {
                _initializeScopeProp.objectReferenceValue = prefab;
                serializedObject.ApplyModifiedProperties();
                
                Debug.Log($"Automatically assigned Initializer prefab from Resources: {RESOURCE_PATH}");
            }
            else
            {
                Debug.LogError($"Failed to find Initializer prefab in Resources folder at path: {RESOURCE_PATH}");
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_initializeScopeProp);
            
            if (!_initializeScopeProp.objectReferenceValue)
            {
                EditorGUILayout.HelpBox("Initializer prefab is not assigned. Press the button below to load it from Resources.", MessageType.Warning);
                
                if (GUILayout.Button("Load Initializer Prefab"))
                {
                    LoadInitializerPrefab();
                }
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_autoInitializeProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}