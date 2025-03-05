
using UnityEngine;
using UnityEditor;
using VContainer.Unity;

namespace ProjectCore.Editor
{
    [CustomEditor(typeof(VContainerSettings))]
    public class VContainerSettingEditor : UnityEditor.Editor
    {
        private const string GLOBAL_LIFETIME_SCOPE_PATH = "GlobalLifetimeScope";
        private SerializedProperty _rootLifetimeScopeProp;
        
        private void OnEnable()
        {
            if (!target)
            {
                return;
            }
            
            _rootLifetimeScopeProp = serializedObject.FindProperty("RootLifetimeScope");
            
            if (_rootLifetimeScopeProp != null && _rootLifetimeScopeProp.objectReferenceValue == null)
            {
                TryAssignGlobalLifetimeScope();
            }
        }
        
        private void TryAssignGlobalLifetimeScope()
        {
            var globalLifetimeScope = Resources.Load<LifetimeScope>(GLOBAL_LIFETIME_SCOPE_PATH);
            if (globalLifetimeScope)
            {
                _rootLifetimeScopeProp.objectReferenceValue = globalLifetimeScope;
                serializedObject.ApplyModifiedProperties();
                
                Debug.Log($"자동으로 GlobalLifetimeScope 프리팹을 VContainerSettings의 RootLifetimeScope로 할당했습니다.");
            }
            else
            {
                Debug.LogWarning($"Resources 폴더에서 GlobalLifetimeScope 프리팹을 찾을 수 없습니다. 경로: {GLOBAL_LIFETIME_SCOPE_PATH}");
            }
        }
    }
}