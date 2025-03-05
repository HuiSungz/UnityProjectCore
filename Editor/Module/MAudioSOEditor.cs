
using System.IO;
using ProjectCore.Editor;
using UnityEditor;
using UnityEngine;

namespace ProjectCore.Module.Editor
{
    [CustomEditor(typeof(MAudioSO))]
    internal class MAudioSOEditor : UnityEditor.Editor
    {
        private const string JSAM_PACKAGE_URL = "https://github.com/jackyyang09/Simple-Unity-Audio-Manager.git";
        private const string JSAM_SETTINGS_PATH = "Assets/Settings/Resources/JSAMSettings.asset";
        private const string JSAM_MENU_EXECUTE = "Window/JSAM/Audio Library";
        
        private GUIStyle _installedStyle;
        private GUIStyle _notInstalledStyle;
        private GUIStyle _hintStyle;
        
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
            
            _hintStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal =
                {
                    textColor = Color.yellow
                },
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };

            _isInit = true;
        }

        private void OnEnable()
        {
            _isInstalled = GitPackageValidator.IsInstallValidation(JSAM_PACKAGE_URL);
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
                
                EditorGUILayout.LabelField("If the settings file is not created yet,\ngo to Window/JSAM/AudioLibrary to create settings.", _hintStyle);
                EditorGUILayout.Space(5);
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                var buttonHeight = GUILayout.Height(30);
                var buttonWidth = GUILayout.Width(EditorGUIUtility.currentViewWidth / 2 - 20);

                if (GUILayout.Button("Find JSAM Settings Asset", buttonWidth, buttonHeight))
                {
                    FindAndSelectJSAMSettings();
                }
                
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Open AudioLibrary Window", buttonWidth, buttonHeight))
                {
                    EditorApplication.ExecuteMenuItem(JSAM_MENU_EXECUTE);
                }
                
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("Status: Not Installed", _notInstalledStyle);
                EditorGUILayout.Space(5);

                if (_isInstalling)
                {
                    EditorGUILayout.HelpBox("Installing JSAM Audio Manager...", MessageType.Info);
                    Rect rect = EditorGUILayout.GetControlRect(false, 20);
                    EditorGUI.ProgressBar(rect, 0.5f, "Installing...");
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    
                    var buttonHeight = GUILayout.Height(30);
                    var buttonWidth = GUILayout.Width(EditorGUIUtility.currentViewWidth / 2 - 20);
                    
                    if (GUILayout.Button("Install JSAM Audio Manager", buttonWidth, buttonHeight))
                    {
                        InstallJSAMAudioManager();
                    }
                    
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUILayout.Space(3);
            EditorGUILayout.EndVertical();
        }
        
        private void InstallJSAMAudioManager()
        {
            _isInstalling = true;

            Repaint();
            
            GitPackageInstaller.InstallPackage(JSAM_PACKAGE_URL, success => 
            {
                if (success)
                {
                    _isInstalled = true;
                    Debug.Log("JSAM Audio Manager 설치 완료");
                    EditorUtility.DisplayDialog("Installation Complete", 
                        "JSAM Audio Manager has been installed successfully. Please go to Window/JSAM/AudioLibrary to create and configure the settings.", 
                        "OK");
                }
                else
                {
                    _isInstalled = false;
                }
                
                _isInstalling = false;
                Repaint();
            });
        }
        
        private void FindAndSelectJSAMSettings()
        {
            bool actualFileExists = File.Exists(JSAM_SETTINGS_PATH);
            var guids = AssetDatabase.FindAssets("t:Object JSAMSettings");
            bool foundInDatabase = guids.Length > 0;
            
            // 실제 파일이 없고 에셋 데이터베이스에서도 찾을 수 없는 경우
            if (!actualFileExists && !foundInDatabase)
            {
                // 파일을 찾을 수 없을 때 AudioLibrary를 직접 열도록 변경
                EditorUtility.DisplayDialog("Settings File Not Found", 
                    "JSAMSettings.asset file could not be found. Opening AudioLibrary window to create settings.", 
                    "OK");
                
                EditorApplication.ExecuteMenuItem(JSAM_MENU_EXECUTE);
                return;
            }
            
            // 에셋 데이터베이스에서는 찾을 수 있지만 실제 파일은 없는 경우
            if (!actualFileExists)
            {
                EditorUtility.DisplayDialog("Settings Issue Detected", 
                    "The settings file reference exists in the database, but the actual file may not exist. " +
                    "Please open the AudioLibrary window to properly create the settings.", 
                    "OK");
                
                EditorApplication.ExecuteMenuItem(JSAM_MENU_EXECUTE);
                return;
            }

            SelectAssetAtPath(JSAM_SETTINGS_PATH);
        }
        
        private void SelectAssetAtPath(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (!asset)
            {
                EditorApplication.ExecuteMenuItem(JSAM_MENU_EXECUTE);
                return;
            }
            
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}