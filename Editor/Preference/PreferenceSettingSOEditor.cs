
#if UNITY_EDITOR
using System.Collections.Generic;
using ProjectCore.Preference;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ProjectCore.Editor
{
    [CustomEditor(typeof(PreferenceSettingSO))]
    public class PreferenceSettingSOEditor : UnityEditor.Editor
    {
        private SerializedProperty _saveUseThreadProperty;
        private SerializedProperty _clearOnSavesProperty;
        private SerializedProperty _autoLoadProperty;
        private SerializedProperty _autoSaveIntervalProperty;
        private SerializedProperty _preferenceFileInfosProperty;
        
        private ReorderableList _fileInfoList;
        private GUIStyle _headerStyle;
        
        private void OnEnable()
        {
            _saveUseThreadProperty = serializedObject.FindProperty("_saveUseThread");
            _clearOnSavesProperty = serializedObject.FindProperty("_clearOnSaves");
            _autoLoadProperty = serializedObject.FindProperty("_autoLoad");
            _autoSaveIntervalProperty = serializedObject.FindProperty("_autoSaveInterval");
            _preferenceFileInfosProperty = serializedObject.FindProperty("_preferenceFileInfos");
            
            InitializeReorderableList();
        }
        
        private void InitializeReorderableList()
        {
            _fileInfoList = new ReorderableList(
                serializedObject,
                _preferenceFileInfosProperty,
                true, true, true, true);
            
            _fileInfoList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Preference Files");
            };
            
            _fileInfoList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = _preferenceFileInfosProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;
                
                var fileNameProperty = element.FindPropertyRelative("PreferenceFileName");
                var isEncryptProperty = element.FindPropertyRelative("IsEncrypt");
                
                // File name field
                var fileNameRect = new Rect(rect.x, rect.y, rect.width * 0.7f - 5, rect.height);
                EditorGUI.PropertyField(fileNameRect, fileNameProperty, GUIContent.none);
                
                // Encryption toggle
                var encryptLabelRect = new Rect(rect.x + rect.width * 0.7f, rect.y, rect.width * 0.15f, rect.height);
                EditorGUI.LabelField(encryptLabelRect, "Encrypt");
                
                var encryptToggleRect = new Rect(rect.x + rect.width * 0.85f, rect.y, rect.width * 0.15f, rect.height);
                EditorGUI.PropertyField(encryptToggleRect, isEncryptProperty, GUIContent.none);
            };
            
            _fileInfoList.onAddCallback = list =>
            {
                int index = list.serializedProperty.arraySize;
                list.serializedProperty.arraySize++;
                list.index = index;
                
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("PreferenceFileName").stringValue = "NewPreference.dat";
                element.FindPropertyRelative("IsEncrypt").boolValue = false;
                
                serializedObject.ApplyModifiedProperties();
            };
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawGeneralSettings();
            EditorGUILayout.Space(10);
            DrawFileSettings();
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGeneralSettings()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Reset to Default"), false, ResetGeneralSettings);
            
            GUICustomLayout.BeginMenuBoxGroup("General Settings", menu);
            
            EditorGUILayout.PropertyField(_saveUseThreadProperty, new GUIContent("Save Using Thread", "Use threading for preference saving to avoid blocking the main thread"));
            EditorGUILayout.PropertyField(_clearOnSavesProperty, new GUIContent("Clear On Saves", "Clear all preference data on save"));
            EditorGUILayout.PropertyField(_autoLoadProperty, new GUIContent("Auto Load", "Automatically load preferences on initialization"));
            
            EditorGUI.BeginChangeCheck();
            float newAutoSaveInterval = EditorGUILayout.Slider(
                new GUIContent("Auto Save Interval", "Time in seconds between auto-saves (0 = disabled)"), 
                _autoSaveIntervalProperty.floatValue, 
                0f, 
                300f);
                
            if (EditorGUI.EndChangeCheck())
            {
                _autoSaveIntervalProperty.floatValue = newAutoSaveInterval;
            }
            
            if (_autoSaveIntervalProperty.floatValue > 0)
            {
                EditorGUILayout.HelpBox($"Auto-save will occur every {_autoSaveIntervalProperty.floatValue:0.0} seconds.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Auto-save is disabled.", MessageType.Info);
            }
            
            GUICustomLayout.EndBoxGroup();
        }

        private void DrawFileSettings()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add Default Files"), false, AddDefaultFiles);
            menu.AddItem(new GUIContent("Clear All Files"), false, ClearAllFiles);
            
            GUICustomLayout.BeginMenuBoxGroup("Preference Files", menu);
            
            EditorGUILayout.Space(5);
            
            // 헬프박스 추가 - 설명
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox("PreferenceFileName\n" +
                                    "데이터를 저장할 파일명 (Ex: UserPrefs.dat)", MessageType.Info);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox("Encrypt\n" +
                                    "파일이 암호화되어 저장여부 (중요 데이터는 체크 권장)", MessageType.Info);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            _fileInfoList.DoLayoutList();
            
            if (_preferenceFileInfosProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No preference files defined. Use the '+' button to add a file, or the menu for more options.", MessageType.Warning);
            }
            
            GUICustomLayout.EndBoxGroup();
        }
        
        private void ResetGeneralSettings()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Reset General Settings", 
                "Are you sure you want to reset all general settings to their default values?", 
                "Reset", "Cancel");
                
            if (confirmed)
            {
                _saveUseThreadProperty.boolValue = true;
                _clearOnSavesProperty.boolValue = false;
                _autoLoadProperty.boolValue = false;
                _autoSaveIntervalProperty.floatValue = 0f;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void AddDefaultFiles()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Add Default Files", 
                "This will add default preference files (UserPrefs.dat, GameSettings.dat, PlayerData.dat). Continue?", 
                "Add", "Cancel");
                
            if (confirmed)
            {
                var defaultFiles = new List<(string name, bool encrypt)>
                {
                    ("PlayerData.dat", true),
                    ("GameSettings.dat", false)
                };
                
                foreach (var file in defaultFiles)
                {
                    // Check if the file already exists
                    bool exists = false;
                    for (int i = 0; i < _preferenceFileInfosProperty.arraySize; i++)
                    {
                        var element = _preferenceFileInfosProperty.GetArrayElementAtIndex(i);
                        if (element.FindPropertyRelative("PreferenceFileName").stringValue == file.name)
                        {
                            exists = true;
                            break;
                        }
                    }
                    
                    if (!exists)
                    {
                        int index = _preferenceFileInfosProperty.arraySize;
                        _preferenceFileInfosProperty.arraySize++;
                        
                        var element = _preferenceFileInfosProperty.GetArrayElementAtIndex(index);
                        element.FindPropertyRelative("PreferenceFileName").stringValue = file.name;
                        element.FindPropertyRelative("IsEncrypt").boolValue = file.encrypt;
                    }
                }
                
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void ClearAllFiles()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Clear All Files", 
                "Are you sure you want to remove all preference file definitions?", 
                "Clear", "Cancel");
                
            if (confirmed)
            {
                _preferenceFileInfosProperty.ClearArray();
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif