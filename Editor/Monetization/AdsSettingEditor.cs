
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ProjectCore.Attributes;
using ProjectCore.Monetize;

namespace ProjectCore.Editor
{
    [CustomEditor(typeof(AdsSettingSO))]
    public class AdsSettingsEditor : UnityEditor.Editor
    {
        private Dictionary<string, List<SerializedProperty>> _groupedProperties;
        private Dictionary<string, bool> _groupFoldouts;
        
        private void OnEnable()
        {
            // 그룹별 프로퍼티 맵 초기화
            _groupedProperties = new Dictionary<string, List<SerializedProperty>>();
            _groupFoldouts = new Dictionary<string, bool>();
            
            // SerializedObject의 모든 프로퍼티 순회
            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;
            
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                
                // m_Script 프로퍼티는 건너뜀
                if (property.name == "m_Script") continue;
                
                // 프로퍼티에서 BoxGroup 어트리뷰트 찾기
                string groupName = "Default";
                
                foreach (var attr in GetTargetAttributes(property))
                {
                    if (attr is BoxGroupAttribute boxAttr)
                    {
                        groupName = boxAttr.GroupName;
                        break;
                    }
                }
                
                // 그룹에 프로퍼티 추가
                if (!_groupedProperties.ContainsKey(groupName))
                {
                    _groupedProperties[groupName] = new List<SerializedProperty>();
                    _groupFoldouts[groupName] = true; // 기본적으로 펼쳐진 상태로 시작
                }
                
                _groupedProperties[groupName].Add(property.Copy());
            }
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // 각 그룹별로 프로퍼티 표시
            foreach (var groupName in _groupedProperties.Keys)
            {
                // Default 그룹은 특별히 처리
                if (groupName == "Default")
                {
                    foreach (var property in _groupedProperties[groupName])
                    {
                        EditorGUILayout.PropertyField(property, true);
                    }
                    continue;
                }
                
                EditorGUILayout.Space();
                
                // 그룹 시작
                GUICustomLayout.BeginBoxGroup(groupName);
                
                // 그룹 내 프로퍼티 표시
                foreach (var property in _groupedProperties[groupName])
                {
                    EditorGUILayout.PropertyField(property, true);
                }
                
                // 그룹 종료
                GUICustomLayout.EndBoxGroup();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private IEnumerable<PropertyAttribute> GetTargetAttributes(SerializedProperty property)
        {
            var obj = serializedObject.targetObject;
            var objType = obj.GetType();
            
            var field = objType.GetField(property.name, System.Reflection.BindingFlags.Instance | 
                                                   System.Reflection.BindingFlags.Public | 
                                                   System.Reflection.BindingFlags.NonPublic);
            
            if (field == null) yield break;
            
            foreach (var attr in field.GetCustomAttributes(typeof(PropertyAttribute), true))
            {
                yield return attr as PropertyAttribute;
            }
        }
    }
}