using UnityEngine;
using UnityEditor;
using System.IO;
using VContainer.Unity;

namespace ProjectCore.Module.Editor
{
    [CustomEditor(typeof(MInitializerSO))]
    internal class MInitializerSOEditor : UnityEditor.Editor
    {
        private const string RESOURCE_PATH = "Initializer";
        private SerializedProperty _initializeScopeProp;
        private SerializedProperty _autoInitializeProp;
        
        private bool _initialized;

        private void OnEnable()
        {
            if (!target)
            {
                return;
            }
            
            CheckAndCreateVContainerSettings();
            
            _initialized = true;
        }
        
        private void SafeInitializeProperties()
        {
            if (!target)
            {
                return;
            }
            
            _initializeScopeProp = serializedObject.FindProperty("_initializeScope");
            _autoInitializeProp = serializedObject.FindProperty("_autoInitialize");
                
            if (_initializeScopeProp != null && !_initializeScopeProp.objectReferenceValue)
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
        
        private void CheckAndCreateVContainerSettings()
        {
            var existingSettings = Resources.Load<VContainerSettings>("VContainerSettings");

            var folderPath = ProjectCore.Editor.GlobalAccess.MainAssetPath;
            var assetPath = Path.Combine(folderPath, "VContainerSettings.asset");
            if (!existingSettings)
            {
                existingSettings = AssetDatabase.LoadAssetAtPath<VContainerSettings>(assetPath);
            }

            if (existingSettings)
            {
                return;
            }
            
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
            }
            
            var settings = CreateInstance<VContainerSettings>();
            
            AssetDatabase.CreateAsset(settings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"VContainerSettings가 '{assetPath}'에 자동으로 생성되었습니다.");
        }

        public override void OnInspectorGUI()
        {
            if (!_initialized)
            {
                SafeInitializeProperties();
                _initialized = true;
            }
            
            serializedObject.Update();
            
            if (_initializeScopeProp != null)
            {
                EditorGUILayout.PropertyField(_initializeScopeProp);
                
                if (!_initializeScopeProp.objectReferenceValue)
                {
                    EditorGUILayout.HelpBox("Initializer prefab is not assigned. Press the button below to load it from Resources.", MessageType.Warning);
                    
                    if (GUILayout.Button("Load Initializer Prefab"))
                    {
                        LoadInitializerPrefab();
                    }
                }
            }
            
            EditorGUILayout.Space();
            
            if (_autoInitializeProp != null)
            {
                EditorGUILayout.PropertyField(_autoInitializeProp);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}