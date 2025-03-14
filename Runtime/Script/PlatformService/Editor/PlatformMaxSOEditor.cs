
#if UNITY_EDITOR && SDK_INSTALLED_APPLOVINMAX
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace ProjectCore.PlatformService
{
    [CustomEditor(typeof(PlatformMaxSO))]
    public class PlatformMaxSOEditor : Editor
    {
        private SerializedProperty _isVerboseLogProperty;
        private SerializedProperty _isInitGdprProperty;
        private SerializedProperty _testDeviceMapProperty;
        private SerializedProperty _testDeviceCsvProperty;
        
        private bool _showMappingInfo = false;
        
        private void OnEnable()
        {
            _isVerboseLogProperty = serializedObject.FindProperty("_isVerboseLog");
            _isInitGdprProperty = serializedObject.FindProperty("_isInitGdpr");
            _testDeviceMapProperty = serializedObject.FindProperty("_testDeviceMap");
            _testDeviceCsvProperty = serializedObject.FindProperty("_testDeviceCsv");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // 기본 프로퍼티 표시
            EditorGUILayout.PropertyField(_isVerboseLogProperty);
            EditorGUILayout.PropertyField(_isInitGdprProperty);
            
            EditorGUILayout.Space();
            
            // 정보 표시
            _showMappingInfo = EditorGUILayout.Foldout(_showMappingInfo, "CSV 매핑 정보");
            if (_showMappingInfo)
            {
                EditorGUILayout.HelpBox(
                    "CSV 파일은 다음 형식이어야 합니다:\n" +
                    "1. 첫 번째 행은 'Owner, TestId'와 같은 헤더\n" +
                    "2. 2행부터 각 행은 Key, Value 쌍\n" +
                    "3. 쉼표(,)로 구분된 값", 
                    MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // CSV 파일 필드
            EditorGUILayout.PropertyField(_testDeviceCsvProperty);
            
            // CSV 파일이 지정된 경우 매핑 버튼 표시
            if (_testDeviceCsvProperty.objectReferenceValue != null)
            {
                if (GUILayout.Button("CSV에서 테스트 기기 매핑하기", GUILayout.Height(30)))
                {
                    MapCsvToDeviceDictionary();
                }
            }
            
            EditorGUILayout.Space();
            
            // 테스트 기기 매핑 표시
            EditorGUILayout.PropertyField(_testDeviceMapProperty);
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void MapCsvToDeviceDictionary()
        {
            TextAsset csvFile = _testDeviceCsvProperty.objectReferenceValue as TextAsset;
            if (csvFile == null)
            {
                EditorUtility.DisplayDialog("오류", "CSV 파일이 유효하지 않습니다.", "확인");
                return;
            }
            
            try
            {
                // CSV 내용 읽기
                string csvText = csvFile.text;
                string[] lines = csvText.Split('\n');
                
                if (lines.Length < 2)
                {
                    EditorUtility.DisplayDialog("오류", "CSV 파일이 비어있거나 헤더만 있습니다.", "확인");
                    return;
                }
                
                // 엔트리 준비
                var deviceEntries = new Dictionary<string, string>();
                
                // 첫 번째 행(헤더) 건너뛰고 2행부터 처리
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line))
                        continue;
                    
                    string[] values = line.Split(',');
                    if (values.Length < 2)
                        continue;
                    
                    string owner = values[0].Trim();
                    string deviceId = values[1].Trim();
                    
                    if (!string.IsNullOrEmpty(owner) && !string.IsNullOrEmpty(deviceId))
                    {
                        deviceEntries[owner] = deviceId;
                    }
                }
                
                if (deviceEntries.Count == 0)
                {
                    EditorUtility.DisplayDialog("결과", "매핑할 유효한 엔트리가 없습니다.", "확인");
                    return;
                }
                
                // 타겟 객체에서 직접 SerializedDictionary에 접근
                PlatformMaxSO targetObject = (PlatformMaxSO)target;
                
                // 리플렉션을 사용하여 private 필드에 접근
                var fieldInfo = typeof(PlatformMaxSO).GetField("_testDeviceMap", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                
                if (fieldInfo != null)
                {
                    var testDeviceMap = fieldInfo.GetValue(targetObject) as SerializedDictionary<string, string>;
                    
                    // 사전이 null이면 새로 생성
                    if (testDeviceMap == null)
                    {
                        testDeviceMap = new SerializedDictionary<string, string>();
                        fieldInfo.SetValue(targetObject, testDeviceMap);
                    }
                    else
                    {
                        // 기존 사전 비우기
                        testDeviceMap.Clear();
                    }
                    
                    // CSV에서 읽은 데이터 추가
                    foreach (var entry in deviceEntries)
                    {
                        testDeviceMap[entry.Key] = entry.Value;
                    }
                    
                    // 변경 사항 저장
                    EditorUtility.SetDirty(targetObject);
                    serializedObject.Update();
                    
                    EditorUtility.DisplayDialog("성공", $"{deviceEntries.Count}개의 테스트 기기가 매핑되었습니다.", "확인");
                }
                else
                {
                    EditorUtility.DisplayDialog("오류", "_testDeviceMap 필드를 찾을 수 없습니다.", "확인");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("CSV 매핑 중 오류 발생: " + ex.Message);
                EditorUtility.DisplayDialog("오류", "CSV 매핑 중 오류가 발생했습니다: " + ex.Message, "확인");
            }
        }
    }
}
#endif