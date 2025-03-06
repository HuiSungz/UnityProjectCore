
#if UNITY_EDITOR
using ProjectCore.Preference;
using ProjectCore.Module;
using UnityEditor;
using UnityEngine;

namespace ProjectCore.Editor
{
    [CustomEditor(typeof(MPreferenceServiceSO))]
    public class MPreferenceServiceSOEditor : UnityEditor.Editor
    {
        private SerializedProperty _settingProperty;
        private const string PREFERENCE_SETTING_PATH = "Assets/ProjectSettings/PreferenceSettings.asset";
        private const string PREFERENCE_SETTING_MENU_PATH = "Assets/ActionFit Create/PreferenceSetting";

        private void OnEnable()
        {
            _settingProperty = serializedObject.FindProperty("_setting");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawDefaultInspector();

            if (!_settingProperty.objectReferenceValue)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("PreferenceSettingSO가 설정되지 않았습니다.", MessageType.Warning);
                
                var existingSetting = AssetDatabase.LoadAssetAtPath<PreferenceSettingSO>(PREFERENCE_SETTING_PATH);
                if (existingSetting)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("기존 설정 파일이 발견되었습니다:");
                    
                    if (GUILayout.Button("자동 설정", GUILayout.Width(100)))
                    {
                        _settingProperty.objectReferenceValue = existingSetting;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(target);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("설정 파일을 생성하시겠습니까?");
                    
                    if (GUILayout.Button("생성 및 설정", GUILayout.Width(100)))
                    {
                        // 메뉴 명령 실행
                        EditorApplication.ExecuteMenuItem(PREFERENCE_SETTING_MENU_PATH);
                        
                        // 약간의 딜레이 후 에셋 로드 시도 (에셋 생성 후 바로 참조하기 위해)
                        EditorApplication.delayCall += () =>
                        {
                            PreferenceSettingSO newSetting = AssetDatabase.LoadAssetAtPath<PreferenceSettingSO>(PREFERENCE_SETTING_PATH);
                            if (newSetting != null)
                            {
                                SerializedObject serializedObj = new SerializedObject(target);
                                SerializedProperty settingProp = serializedObj.FindProperty("_setting");
                                settingProp.objectReferenceValue = newSetting;
                                serializedObj.ApplyModifiedProperties();
                                EditorUtility.SetDirty(target);
                            }
                        };
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif