
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ProjectCore.Monetize;
using UnityEngine.Purchasing;

namespace ProjectCore.Editor
{
    [CustomEditor(typeof(IAPSettingSO))]
    public class IAPSettingsEditor : UnityEditor.Editor
    {
        private SerializedProperty _iapCatalogProperty;
        private SerializedProperty _autoInitializeProperty;
        
        private bool _isAddingNewProduct;
        private string _newProductSku = "";
        private string _errorMessage;
        
        private readonly Dictionary<int, bool> _foldoutStates = new();
        private readonly Dictionary<string, bool> _duplicateSkus = new();

        // 스타일 정의
        private GUIStyle _folderHeaderStyle;
        private GUIStyle _foldoutStyle;
        private GUIStyle _headerLabelStyle;
        
        private void OnEnable()
        {
            _iapCatalogProperty = serializedObject.FindProperty("_iapCatalog");
            _autoInitializeProperty = serializedObject.FindProperty("_autoInitialize");
            _foldoutStates.Clear();

            CheckDuplicateSkus();
            InitStyles();
        }
        
        private void InitStyles()
        {
            _folderHeaderStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(5, 5, 5, 5),
                margin = new RectOffset(0, 0, 2, 2)
            };
            
            _foldoutStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };
            
            _headerLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.white },
                margin = new RectOffset(0, 0, 2, 0)
            };
        }
        
        private void CheckDuplicateSkus()
        {
            _duplicateSkus.Clear();
            
            if (_iapCatalogProperty?.arraySize > 0)
            {
                Dictionary<string, int> skuCount = new Dictionary<string, int>();
                
                for (int i = 0; i < _iapCatalogProperty.arraySize; i++)
                {
                    SerializedProperty catalogItem = _iapCatalogProperty.GetArrayElementAtIndex(i);
                    string sku = catalogItem.FindPropertyRelative("_productSku").stringValue;
                    
                    if (!string.IsNullOrEmpty(sku))
                    {
                        if (skuCount.ContainsKey(sku))
                        {
                            skuCount[sku]++;
                            _duplicateSkus[sku] = true;
                        }
                        else
                        {
                            skuCount[sku] = 1;
                        }
                    }
                }
            }
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // 기본 설정
            EditorGUILayout.PropertyField(_autoInitializeProperty);
            
            EditorGUILayout.Space(10);
            
            // 제품 목록 헤더
            GUICustomLayout.BeginBoxGroup("In-App Products");
            
            if (_iapCatalogProperty.arraySize > 0)
            {
                DrawIAPCatalogItems();
            }
            else
            {
                EditorGUILayout.HelpBox("제품 목록이 비어있습니다. 새로운 제품을 추가하세요.", MessageType.Info);
            }
            
            // 신규 제품 추가 UI
            DrawAddProductUI();
            
            GUICustomLayout.EndBoxGroup();
            
            // 오류 메시지 표시
            if (!string.IsNullOrEmpty(_errorMessage))
            {
                EditorGUILayout.HelpBox(_errorMessage, MessageType.Error);
            }
            
            // 중복 SKU 경고
            if (_duplicateSkus.Count > 0)
            {
                string duplicateList = string.Join(", ", _duplicateSkus.Keys);
                EditorGUILayout.HelpBox($"중복된 SKU가 발견되었습니다: {duplicateList}", MessageType.Warning);
            }
            
            serializedObject.ApplyModifiedProperties();
            
            // 변경사항이 있으면 중복 검사 다시 실행
            if (GUI.changed)
            {
                CheckDuplicateSkus();
            }
        }
        
        private void DrawIAPCatalogItems()
        {
            for (int i = 0; i < _iapCatalogProperty.arraySize; i++)
            {
                SerializedProperty catalogItem = _iapCatalogProperty.GetArrayElementAtIndex(i);
                SerializedProperty skuProperty = catalogItem.FindPropertyRelative("_productSku");
                SerializedProperty androidIDProperty = catalogItem.FindPropertyRelative("_androidID");
                SerializedProperty iosIDProperty = catalogItem.FindPropertyRelative("_iosID");
                SerializedProperty productTypeProperty = catalogItem.FindPropertyRelative("_productType");
                
                string sku = skuProperty.stringValue;
                ProductType productType = (ProductType)productTypeProperty.enumValueIndex;
                
                string title = !string.IsNullOrEmpty(sku) ? sku : "제품 정보";
                
                // 중복 표시 아이콘 추가
                if (_duplicateSkus.ContainsKey(sku))
                {
                    title += " (중복)";
                }
                
                // 타입 정보 추가
                string typeLabel = $" ({productType})";
                
                // 항목 컨테이너 시작
                EditorGUILayout.BeginVertical(_folderHeaderStyle);
                
                // 헤더 배경 (다크 테마)
                Rect headerRect = EditorGUILayout.BeginHorizontal();
                EditorGUI.DrawRect(headerRect, EditorGUIUtility.isProSkin ? new Color(0.12f, 0.12f, 0.12f) : new Color(0.2f, 0.2f, 0.2f));
                
                // 제목 레이블
                EditorGUILayout.LabelField(title + typeLabel, _headerLabelStyle);
                
                EditorGUILayout.EndHorizontal();
                
                // 항목 내용 (항상 표시)
                EditorGUILayout.Space(5);
                
                // SKU ID
                EditorGUILayout.PropertyField(skuProperty, new GUIContent("SKU ID"));
                
                // Product IDs
                EditorGUI.BeginChangeCheck();
                
                // Android ID - 비어있으면 빨간색 표시
                GUI.color = string.IsNullOrEmpty(androidIDProperty.stringValue) ? new Color(1, 0.7f, 0.7f) : GUI.color;
                EditorGUILayout.PropertyField(androidIDProperty, new GUIContent("Android ID"));
                GUI.color = Color.white;
                
                // iOS ID - 비어있으면 빨간색 표시
                GUI.color = string.IsNullOrEmpty(iosIDProperty.stringValue) ? new Color(1, 0.7f, 0.7f) : GUI.color;
                EditorGUILayout.PropertyField(iosIDProperty, new GUIContent("iOS ID"));
                GUI.color = Color.white;
                
                if (EditorGUI.EndChangeCheck())
                {
                    androidIDProperty.stringValue = androidIDProperty.stringValue.Trim();
                    iosIDProperty.stringValue = iosIDProperty.stringValue.Trim();
                }
                
                // 제품 유형
                EditorGUILayout.PropertyField(productTypeProperty, new GUIContent("제품 유형"));
                EditorGUILayout.Space(5);
                
                // 제품 삭제 버튼
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("제품 삭제", GUILayout.Width(120)))
                {
                    if (EditorUtility.DisplayDialog("제품 삭제", $"'{sku}' 제품을 정말 삭제하시겠습니까?", "삭제", "취소"))
                    {
                        _iapCatalogProperty.DeleteArrayElementAtIndex(i);
                        serializedObject.ApplyModifiedProperties();
                        return;
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(2);
            }
        }
        
        private void DrawAddProductUI()
        {
            EditorGUILayout.Space(5);
            
            // 신규 제품 추가 UI 토글
            EditorGUILayout.BeginHorizontal();
            
            if (!_isAddingNewProduct)
            {
                if (GUILayout.Button("새 제품 추가", GUILayout.Height(24)))
                {
                    _isAddingNewProduct = true;
                    _newProductSku = "";
                    _errorMessage = null;
                }
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.LabelField("새 제품 추가", EditorStyles.boldLabel);
                
                _newProductSku = EditorGUILayout.TextField("SKU ID", _newProductSku);
                
                EditorGUILayout.Space(2);
                
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("취소"))
                {
                    _isAddingNewProduct = false;
                    _errorMessage = null;
                }
                
                if (GUILayout.Button("추가"))
                {
                    if (string.IsNullOrWhiteSpace(_newProductSku))
                    {
                        _errorMessage = "SKU ID는 필수 항목입니다.";
                    }
                    else if (_duplicateSkus.ContainsKey(_newProductSku) || IsSkuExist(_newProductSku))
                    {
                        _errorMessage = $"'{_newProductSku}' SKU ID가 이미 존재합니다.";
                    }
                    else
                    {
                        // 새 제품 추가
                        AddNewProduct(_newProductSku);
                        _isAddingNewProduct = false;
                        _errorMessage = null;
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private bool IsSkuExist(string sku)
        {
            for (int i = 0; i < _iapCatalogProperty.arraySize; i++)
            {
                SerializedProperty catalogItem = _iapCatalogProperty.GetArrayElementAtIndex(i);
                string existingSku = catalogItem.FindPropertyRelative("_productSku").stringValue;
                
                if (existingSku == sku)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        private void AddNewProduct(string sku)
        {
            int index = _iapCatalogProperty.arraySize;
            _iapCatalogProperty.arraySize++;
            
            SerializedProperty newItem = _iapCatalogProperty.GetArrayElementAtIndex(index);
            newItem.FindPropertyRelative("_productSku").stringValue = sku;
            newItem.FindPropertyRelative("_androidID").stringValue = "";
            newItem.FindPropertyRelative("_iosID").stringValue = "";
            newItem.FindPropertyRelative("_productType").enumValueIndex = (int)ProductType.Consumable;
            
            // 새 항목을 펼쳐진 상태로 설정
            _foldoutStates[index] = true;
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}