
using UnityEngine;
using UnityEditor;
using System.IO;
using ProjectCore.Monetize;

namespace ProjectCore.Editor
{
    [CustomEditor(typeof(MonetizationSettingSO))]
    public class MonetizationSettingsEditor : UnityEditor.Editor
    {
        #region Fields

        private const string MainAssetPath = GlobalAccess.MainAssetPath + "Monetization/";
        private const string MenuItemPath = GlobalAccess.MenuItemCreateMenuRoot + "Monetization Setting";
        private const float TAB_HEIGHT = 30f;

        private SerializedProperty _adsSettingsProperty;
        private SerializedProperty _iapSettingsProperty;
        private SerializedProperty _useNetworkValidateProperty;
        private SerializedProperty _isActivateProperty;

        private enum SettingsTab
        {
            Ads,
            IAP
        }

        private SettingsTab _currentTab = SettingsTab.Ads;
        private UnityEditor.Editor _adsSettingsEditor;
        private UnityEditor.Editor _iapSettingsEditor;
        
        // 아이콘 텍스쳐
        private Texture2D _adsIcon;
        private Texture2D _iapIcon;
        
        // 탭 스타일
        private GUIStyle _tabButtonStyle;

        #endregion

        #region Menu

        [MenuItem("ActionFit/Monetization Setting", priority = 14)]
        public static void SelectMonetizationSettings()
        {
            var selectedObject = GetMonetizationSettings();
            if (selectedObject)
            {
                Selection.activeObject = selectedObject;
            }
            else
            {
                if (!Directory.Exists(MainAssetPath))
                {
                    Directory.CreateDirectory(MainAssetPath);
                    AssetDatabase.Refresh();
                }
                
                CreateAsset(MainAssetPath, true);
            }
        }

        [MenuItem(MenuItemPath)]
        public static void CreateAssetFromMenu()
        {
            var monetizationSettings = GetMonetizationSettings();
            if (monetizationSettings)
            {
                Debug.Log("MonetizationSettings 파일이 이미 존재합니다!");
                EditorGUIUtility.PingObject(monetizationSettings);
                return;
            }
            
            var selectionPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            selectionPath = Path.GetDirectoryName(selectionPath);

            if (string.IsNullOrEmpty(selectionPath) || !Directory.Exists(selectionPath))
            {
                selectionPath = MainAssetPath;
            }

            CreateAsset(selectionPath, true);
        }
        
        private static MonetizationSettingSO GetMonetizationSettings()
        {
            var guids = AssetDatabase.FindAssets("t:MonetizationSettingSO");
            if (guids.Length <= 0)
            {
                return null;
            }
            
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<MonetizationSettingSO>(path);
        }

        #endregion

        #region Lifecycle

        private void OnEnable()
        {
            // Properties 초기화
            _adsSettingsProperty = serializedObject.FindProperty("_adsSetting");
            _iapSettingsProperty = serializedObject.FindProperty("_iapSetting");
            _useNetworkValidateProperty = serializedObject.FindProperty("_useNetworkValidate");
            _isActivateProperty = serializedObject.FindProperty("_isActivate");

            // 아이콘 로드
            _adsIcon = LoadIconTexture("icon_ads");
            _iapIcon = LoadIconTexture("icon_iap");
            
            // 서브 에디터 생성
            CreateSubEditors();
            
            // 탭 스타일 초기화
            InitTabButtonStyle();
        }
        
        private Texture2D LoadIconTexture(string iconName)
        {
            Texture2D originalIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Editor/Resources/{iconName}.png") ?? 
                                    Resources.Load<Texture2D>(iconName);
            
            if (originalIcon == null)
            {
                Debug.LogWarning($"아이콘을 찾을 수 없습니다: {iconName}");
                return null;
            }
            
            return originalIcon;
        }
        
        private void InitTabButtonStyle()
        {
            _tabButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
            {
                fixedHeight = TAB_HEIGHT,
                fontStyle = FontStyle.Bold,
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };
        }

        private void OnDisable()
        {
            DestroySubEditors();
        }

        private void CreateSubEditors()
        {
            // Ads 설정 에디터 생성
            if (_adsSettingsProperty.objectReferenceValue != null)
            {
                _adsSettingsEditor = CreateEditor(_adsSettingsProperty.objectReferenceValue);
            }

            // IAP 설정 에디터 생성
            if (_iapSettingsProperty.objectReferenceValue != null)
            {
                _iapSettingsEditor = CreateEditor(_iapSettingsProperty.objectReferenceValue);
            }
        }

        private void DestroySubEditors()
        {
            if (_adsSettingsEditor != null)
            {
                DestroyImmediate(_adsSettingsEditor);
                _adsSettingsEditor = null;
            }

            if (_iapSettingsEditor != null)
            {
                DestroyImmediate(_iapSettingsEditor);
                _iapSettingsEditor = null;
            }
        }

        #endregion

        #region Inspector GUI

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUICustomLayout.BeginBoxGroup("Monetization Settings");

            // 기본 속성 표시
            EditorGUILayout.PropertyField(_useNetworkValidateProperty);
            EditorGUILayout.PropertyField(_isActivateProperty);
            EditorGUILayout.Space(5);
            
            // 참조 필드 표시
            EditorGUILayout.PropertyField(_adsSettingsProperty);
            EditorGUILayout.PropertyField(_iapSettingsProperty);
            
            EditorGUILayout.Space(5);

            // 구분선 추가
            EditorGUILayout.Space();
            DrawSeparator();
            EditorGUILayout.Space();

            // 탭 그리기
            DrawTabSection();

            GUICustomLayout.EndBoxGroup();

            serializedObject.ApplyModifiedProperties();

            // 변경사항이 있으면 저장
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void DrawSeparator()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawTabSection()
        {
            // 탭 섹션을 위한 박스 스타일
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            // 탭 버튼
            DrawTabs();
            
            EditorGUILayout.Space(5);
            
            // 선택된 탭에 따라 세부 설정 표시
            DrawTabContent();
            
            GUILayout.EndVertical();
        }

        private void DrawTabs()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var normalColor = new Color(0.2f, 0.2f, 0.2f);
            var selectedColor = new Color(0.3f, 0.7f, 0.3f);
            var oldColor = GUI.backgroundColor;
        
            // Ads 탭
            GUI.backgroundColor = _currentTab == SettingsTab.Ads ? selectedColor : normalColor;
        
            // Ads 아이콘 크기 설정 (원본 비율 유지)
            if (_adsIcon != null)
            {
                float adsIconHeight = 16f * ((float)_adsIcon.height / _adsIcon.width);
                EditorGUIUtility.SetIconSize(new Vector2(16f, adsIconHeight));
            }
        
            GUIContent adsContent = new GUIContent(" Ads Settings", _adsIcon);
            if (GUILayout.Button(adsContent, _tabButtonStyle, GUILayout.ExpandWidth(true)))
            {
                _currentTab = SettingsTab.Ads;
            }

            // 아이콘 크기 리셋 (다음 아이콘에 영향을 주지 않도록)
            EditorGUIUtility.SetIconSize(Vector2.zero);
            // IAP 탭
            GUI.backgroundColor = _currentTab == SettingsTab.IAP ? selectedColor : normalColor;
            
            // IAP 아이콘 크기 설정 (원본 비율 유지)
            if (_iapIcon != null)
            {
                float iapIconWidth = 16f * ((float)_iapIcon.width / _iapIcon.height);
                EditorGUIUtility.SetIconSize(new Vector2(iapIconWidth, 16f));
            }
            
            GUIContent iapContent = new GUIContent(" IAP Settings", _iapIcon);
            if (GUILayout.Button(iapContent, _tabButtonStyle, GUILayout.ExpandWidth(true)))
            {
                _currentTab = SettingsTab.IAP;
            }
        
            // 아이콘 크기 복원
            EditorGUIUtility.SetIconSize(Vector2.zero);
        
            GUI.backgroundColor = oldColor;
        
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTabContent()
        {
            // 참조가 없는 경우 생성 버튼 표시
            switch (_currentTab)
            {
                case SettingsTab.Ads:
                    if (_adsSettingsProperty.objectReferenceValue == null)
                    {
                        EditorGUILayout.HelpBox("Ads Settings 에셋이 설정되지 않았습니다.", MessageType.Warning);
                        
                        if (GUILayout.Button("Create Ads Settings"))
                        {
                            CreateAdsSettings();
                        }
                        return;
                    }
                    break;
                    
                case SettingsTab.IAP:
                    if (_iapSettingsProperty.objectReferenceValue == null)
                    {
                        EditorGUILayout.HelpBox("IAP Settings 에셋이 설정되지 않았습니다.", MessageType.Warning);
                        
                        if (GUILayout.Button("Create IAP Settings"))
                        {
                            CreateIAPSettings();
                        }
                        return;
                    }
                    break;
            }

            // 설정 내용 표시
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            switch (_currentTab)
            {
                case SettingsTab.Ads:
                    if (_adsSettingsEditor != null)
                    {
                        _adsSettingsEditor.OnInspectorGUI();
                    }
                    else if (_adsSettingsProperty.objectReferenceValue != null)
                    {
                        _adsSettingsEditor = CreateEditor(_adsSettingsProperty.objectReferenceValue);
                    }
                    break;
                    
                case SettingsTab.IAP:
                    if (_iapSettingsEditor != null)
                    {
                        _iapSettingsEditor.OnInspectorGUI();
                    }
                    else if (_iapSettingsProperty.objectReferenceValue != null)
                    {
                        _iapSettingsEditor = CreateEditor(_iapSettingsProperty.objectReferenceValue);
                    }
                    break;
            }
            
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Asset Creation

        private static void CreateAsset(string folderPath, bool ping)
        {
            var monetizationSettings = CreateInstance<MonetizationSettingSO>();
            monetizationSettings.name = "MonetizationSetting";
            
            var assetPath = Path.Combine(folderPath, monetizationSettings.name + ".asset");
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            
            AssetDatabase.CreateAsset(monetizationSettings, assetPath);

            // Ads Settings 생성
            var adsSettings = CreateInstance<AdsSettingSO>();
            adsSettings.name = "AdsSetting";
            AssetDatabase.AddObjectToAsset(adsSettings, monetizationSettings);

            // IAP Settings 생성
            var iapSettings = CreateInstance<IAPSettingSO>();
            iapSettings.name = "IAPSetting";
            AssetDatabase.AddObjectToAsset(iapSettings, monetizationSettings);

            // Monetization Settings 참조 설정
            var serializedObject = new SerializedObject(monetizationSettings);
            serializedObject.Update();
            
            var adsSettingsProperty = serializedObject.FindProperty("_adsSetting");
            var iapSettingsProperty = serializedObject.FindProperty("_iapSetting");
            
            adsSettingsProperty.objectReferenceValue = adsSettings;
            iapSettingsProperty.objectReferenceValue = iapSettings;
            
            serializedObject.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();

            if (ping)
            {
                EditorGUIUtility.PingObject(monetizationSettings);
            }
        }

        private void CreateAdsSettings()
        {
            var monetizationSettings = (MonetizationSettingSO)target;

            // Ads Settings 생성
            var adsSettings = CreateInstance<AdsSettingSO>();
            adsSettings.name = "AdsSetting";
            AssetDatabase.AddObjectToAsset(adsSettings, monetizationSettings);

            // 참조 설정
            serializedObject.Update();
            _adsSettingsProperty.objectReferenceValue = adsSettings;
            serializedObject.ApplyModifiedProperties();

            // 에디터 업데이트
            DestroySubEditors();
            CreateSubEditors();

            AssetDatabase.SaveAssets();
        }

        private void CreateIAPSettings()
        {
            var monetizationSettings = (MonetizationSettingSO)target;

            // IAP Settings 생성
            var iapSettings = CreateInstance<IAPSettingSO>();
            iapSettings.name = "IAPSetting";
            AssetDatabase.AddObjectToAsset(iapSettings, monetizationSettings);

            // 참조 설정
            serializedObject.Update();
            _iapSettingsProperty.objectReferenceValue = iapSettings;
            serializedObject.ApplyModifiedProperties();

            // 에디터 업데이트
            DestroySubEditors();
            CreateSubEditors();

            AssetDatabase.SaveAssets();
        }

        #endregion
    }
}