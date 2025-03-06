
using ProjectCore.Editor;
using UnityEditor;
using UnityEngine;

namespace ProjectCore.Module.Editor
{
    [CustomEditor(typeof(MHapticSO))]
    internal class MHapticSOEditor : UnityEditor.Editor
    {
        private const string VIBRATION_PACKAGE_URL = "https://github.com/BenoitFreslon/Vibration.git";
        
        private GUIStyle _installedStyle;
        private GUIStyle _notInstalledStyle;
        private GUIStyle _codeStyle;
        
        private bool _isInstalling;
        private bool _isInit;
        private bool _isInstalled;

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
            
            _codeStyle = new GUIStyle(EditorStyles.textField)
            {
                normal =
                {
                    textColor = new Color(0.3f, 0.65f, 1f),
                    background = EditorStyles.textField.normal.background
                },
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(5, 5, 0, 0)
            };

            _isInit = true;
        }

        private void OnEnable()
        {
            _isInstalled = PackageValidator.IsInstallValidation(VIBRATION_PACKAGE_URL);
        }
        
        public override void OnInspectorGUI()
        {
            InitializeStyles();
            
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (_isInstalled)
            {
                EditorGUILayout.LabelField("Status: Installed", _installedStyle);
                EditorGUILayout.Space(5);
                
                EditorGUILayout.HelpBox("Mobile Vibration package is installed and ready to use.", MessageType.Info);

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Usage Example:", EditorStyles.boldLabel);

                GUI.enabled = false; // 컨트롤을 비활성화
                EditorGUILayout.TextField("Haptic.Using - bool Property", _codeStyle);
                EditorGUILayout.TextField("Haptic.Weak();", _codeStyle);
                EditorGUILayout.TextField("Haptic.Soft();", _codeStyle);
                EditorGUILayout.TextField("Haptic.Medium();", _codeStyle);
                EditorGUILayout.TextField("Haptic.Hard();", _codeStyle);
                GUI.enabled = true; // 다시 활성화
            }
            else
            {
                EditorGUILayout.LabelField("Status: Not Installed", _notInstalledStyle);
                EditorGUILayout.Space(5);

                if (_isInstalling)
                {
                    EditorGUILayout.HelpBox("Installing Mobile Vibration Package...", MessageType.Info);
                    Rect rect = EditorGUILayout.GetControlRect(false, 20);
                    EditorGUI.ProgressBar(rect, 0.5f, "Installing...");
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    
                    var buttonHeight = GUILayout.Height(30);
                    var buttonWidth = GUILayout.Width(EditorGUIUtility.currentViewWidth / 2 - 20);
                    
                    if (GUILayout.Button("Install Mobile Vibration", buttonWidth, buttonHeight))
                    {
                        InstallVibrationPackage();
                    }
                    
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUILayout.Space(3);
            EditorGUILayout.EndVertical();
        }
        
        private void InstallVibrationPackage()
        {
            _isInstalling = true;

            Repaint();
            
            GitPackageInstaller.InstallPackage(VIBRATION_PACKAGE_URL, success => 
            {
                if (success)
                {
                    _isInstalled = true;
                    if (!SymbolUtility.HasSymbol(GlobalAccess.SYMBOL_INSTALLED_HAPTIC))
                    {
                        EditorApplication.delayCall += () =>
                        {
                            SymbolUtility.AddSymbolForTarget(GlobalAccess.SYMBOL_INSTALLED_HAPTIC, BuildTargetGroup.iOS);
                            SymbolUtility.AddSymbolForTarget(GlobalAccess.SYMBOL_INSTALLED_HAPTIC, BuildTargetGroup.Android);
                        };
                    }
                    Debug.Log("Mobile Vibration 패키지 설치 완료");
                    
                    EditorUtility.DisplayDialog("Installation Complete", 
                        "Mobile Vibration package has been installed successfully. You can now use vibration features in your mobile apps.", 
                        "OK");
                }
                else
                {
                    _isInstalled = false;
                    
                    EditorUtility.DisplayDialog("Installation Failed", 
                        "Failed to install Mobile Vibration package. Please try again or install manually.", 
                        "OK");
                }
                
                _isInstalling = false;
                Repaint();
            });
        }
    }
}