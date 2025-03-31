
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using ProjectCore.Monetize;

[CustomPropertyDrawer(typeof(IAPSkuAttribute))]
public class IAPSkuAttributeDrawer : PropertyDrawer
{
    // 캐시된 설정 참조
    private static IAPSettingSO _cachedSettings;
    private static Dictionary<string, int> _cachedSkuIndices;
    private static string[] _cachedOptions;
    private static bool _cacheInitialized = false;
    private static double _lastCacheTime;
    
    // 캐시 갱신 간격 (초)
    private const double CACHE_REFRESH_INTERVAL = 5.0;
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 주기적으로 캐시 초기화 또는 필요시 초기화
        if (!_cacheInitialized || (EditorApplication.timeSinceStartup - _lastCacheTime) > CACHE_REFRESH_INTERVAL)
        {
            InitializeCache();
        }
        
        if (_cachedOptions == null || _cachedOptions.Length <= 1)
        {
            EditorGUI.PropertyField(position, property, label);
            EditorGUILayout.HelpBox("IAPSettingSO가 프로젝트에 존재하지 않거나 SKU 목록이 비어있습니다.", MessageType.Warning);
            return;
        }
        
        // 현재 선택된 값의 인덱스 찾기
        var currentValue = property.stringValue;
        int selectedIndex = 0;
        
        if (!string.IsNullOrEmpty(currentValue) && _cachedSkuIndices.ContainsKey(currentValue))
        {
            selectedIndex = _cachedSkuIndices[currentValue];
        }
        
        EditorGUI.BeginChangeCheck();
        selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, _cachedOptions);
        if (EditorGUI.EndChangeCheck())
        {
            property.stringValue = selectedIndex == 0 ? "" : _cachedOptions[selectedIndex];
        }
    }
    
    private void InitializeCache()
    {
        _cachedSettings = FindIAPSettings();
        
        List<string> options = new List<string> { "(None)" };
        _cachedSkuIndices = new Dictionary<string, int>();
        
        if (_cachedSettings != null && _cachedSettings.IAPCatalog != null)
        {
            int index = 1; // 0은 "(None)"을 위해 예약
            foreach (var item in _cachedSettings.IAPCatalog)
            {
                if (!string.IsNullOrEmpty(item.Sku))
                {
                    options.Add(item.Sku);
                    _cachedSkuIndices[item.Sku] = index++;
                }
            }
        }
        
        _cachedOptions = options.ToArray();
        _cacheInitialized = true;
        _lastCacheTime = EditorApplication.timeSinceStartup;
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