
using UnityEngine;
using UnityEditor;
using ProjectCore.Editor;

namespace ProjectCore.Module.Editor
{
    [CustomEditor(typeof(MNetworkSO))]
    internal class MNetworkSOEditor : UnityEditor.Editor
    {
        private const string PACKAGE_NAME = "https://github.com/HuiSungz/Unity-NetworkConnection-Validator.git";
        private const string CONFIG_PATH = "Assets/Network/Resources/NetworkConfig.asset";
        
        private GUIStyle _installedStyle;
        private GUIStyle _notInstalledStyle;
        private bool _isInit;

        private void InitializeStyles()
        {
            if (_isInit)
            {
                return;
            }
            
            _installedStyle = new GUIStyle(EditorStyles.label)
            {
                normal =
                {
                    textColor = Color.green
                },
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            _notInstalledStyle = new GUIStyle(EditorStyles.label)
            {
                normal =
                {
                    textColor = Color.red
                },
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            _isInit = true;
        }
        
        public override void OnInspectorGUI()
        {
            InitializeStyles();
            
            var isPackageInstalled = PackageValidator.IsInstallValidation(PACKAGE_NAME);
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (isPackageInstalled)
            {
                EditorGUILayout.LabelField("Status: Installed", _installedStyle);
                if (GUILayout.Button("Network Config"))
                {
                    FindAndSelectNetworkConfig();
                }
            }
            else
            {
                EditorGUILayout.LabelField("Status : Non Install", _notInstalledStyle);
                EditorGUILayout.HelpBox("Network Validator 패키지가 설치되어 있지 않습니다. GitHub에서 설치해주세요.", MessageType.Warning);
                
                if (GUILayout.Button("GitHub 페이지 열기"))
                {
                    Application.OpenURL("https://github.com/HuiSungz/Unity-NetworkConnection-Validator.git");
                }
            }
            EditorGUILayout.Space(3);
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// NetworkConfig 애셋을 찾아 선택하고 핑합니다.
        /// </summary>
        private void FindAndSelectNetworkConfig()
        {
            Object configAsset = AssetDatabase.LoadAssetAtPath<Object>(CONFIG_PATH);
            
            if (configAsset != null)
            {
                Selection.activeObject = configAsset;
                EditorGUIUtility.PingObject(configAsset);
            }
            else
            {
                EditorUtility.DisplayDialog("애셋을 찾을 수 없음", 
                    "NetworkConfig.asset을 찾을 수 없습니다. 경로가 올바른지 확인해주세요: " + CONFIG_PATH, 
                    "확인");
            }
        }
    }
}