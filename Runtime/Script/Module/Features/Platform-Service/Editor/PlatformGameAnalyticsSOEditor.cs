
#if UNITY_EDITOR && SDK_INSTALLED_GAMEANALYTICS
using UnityEditor;
using UnityEngine;

namespace ProjectCore.Module.Editor
{
    [CustomEditor(typeof(PlatformGameAnalyticsSO))]
    public class PlatformGameAnalyticsSOEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Open GameAnalytics Settings", GUILayout.Height(30)))
            {
                EditorApplication.ExecuteMenuItem("Window/GameAnalytics/Select Settings");
            }
        }
    }
}
#endif