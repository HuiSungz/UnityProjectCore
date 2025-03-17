
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using ProjectCore.Monetize;

namespace ProjectCore.Editor
{
    [CustomPropertyDrawer(typeof(IAPSkuAttribute))]
    public class IAPSkuAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var iapSettings = FindIAPSettings();
            if (!iapSettings)
            {
                EditorGUI.PropertyField(position, property, label);
                EditorGUILayout.HelpBox("IAPSettingSO가 프로젝트에 존재하지 않습니다.", MessageType.Warning);
                return;
            }
            
            var skuList = iapSettings.IAPCatalog.Select(item => item.Sku).ToList();
            var options = new List<string> { "(None)" };
            options.AddRange(skuList);
            
            // 현재 선택된 값의 인덱스 찾기
            var currentValue = property.stringValue;
            var selectedIndex = string.IsNullOrEmpty(currentValue) ? 0 : options.IndexOf(currentValue);
            if (selectedIndex == -1)
            {
                selectedIndex = 0;
            }
            
            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, options.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = selectedIndex == 0 ? "" : options[selectedIndex];
            }
        }
        
        private IAPSettingSO FindIAPSettings()
        {
            var guids = AssetDatabase.FindAssets("t:IAPSettingSO");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<IAPSettingSO>(path);
            }
            
            guids = AssetDatabase.FindAssets("t:MonetizationSettingSO");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var monetizationSettings = AssetDatabase.LoadAssetAtPath<MonetizationSettingSO>(path);
                if (monetizationSettings)
                {
                    return monetizationSettings.IAPSetting;
                }
            }
            
            return null;
        }
    }
}