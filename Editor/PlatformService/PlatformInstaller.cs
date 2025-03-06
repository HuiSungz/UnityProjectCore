
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectCore.Editor
{
    public class PlatformInstaller : EditorWindow
    {
        #region Fields

        private PlatformInstallerViewModel _viewModel;
        
        private GUIStyle _headerStyle;
        private GUIStyle _packageBoxStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _descriptionStyle;
        
        private Vector2 _scrollPosition;
        private bool _isInitialized;
        private bool _isInstalling;
        private string _searchFilter = "";
        
        // 색상 정의
        private readonly Color _installedColor = new Color(0.4f, 0.8f, 0.4f, 1f);
        private readonly Color _notInstalledColor = new Color(0.8f, 0.4f, 0.4f, 1f);
        private readonly Color _installButtonColor = new Color(0.3f, 0.6f, 1f, 1f);
        
        #endregion

        #region Unity Event Methods

        [MenuItem("ActionFit/Platform(SDK) Installer", priority = 11)]
        public static void ShowWindow()
        {
            var window = GetWindow<PlatformInstaller>("Platform(SDK) Installer");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }
        
        private void OnEnable()
        {
            _viewModel = new PlatformInstallerViewModel();
            _viewModel.OnPackagesChanged += Repaint;
            
            _isInitialized = true;
        }
        
        private void OnDisable()
        {
            if (_viewModel != null)
            {
                _viewModel.OnPackagesChanged -= Repaint;
            }
        }
        
        private void OnGUI()
        {
            if (!_isInitialized)
            {
                return;
            }
            
            InitStyles();
            DrawHeader();
            
            EditorGUILayout.Space(10);
            DrawSearchBar();
            EditorGUILayout.Space(10);
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DrawPackageList();
            EditorGUILayout.EndScrollView();
        }
        
        #endregion

        #region UI Methods
        
        private void InitStyles()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter,
                    margin = new RectOffset(10, 10, 15, 15)
                };
            }

            if (_packageBoxStyle == null)
            {
                _packageBoxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(15, 15, 10, 10),
                    margin = new RectOffset(5, 5, 5, 5)
                };
            }

            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    margin = new RectOffset(0, 0, 5, 5)
                };
            }
            
            if (_descriptionStyle == null)
            {
                _descriptionStyle = new GUIStyle(EditorStyles.label)
                {
                    wordWrap = true,
                    margin = new RectOffset(0, 0, 3, 7)
                };
            }
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("ActionFit Platform(SDK) Installer", _headerStyle);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Refresh", GUILayout.Width(80), GUILayout.Height(25)))
            {
                _viewModel.RefreshPackageStatus();
            }
            
            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawSearchBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("검색:", GUILayout.Width(50));
            
            var newFilter = EditorGUILayout.TextField(_searchFilter);
            if (newFilter != _searchFilter)
            {
                _searchFilter = newFilter;
                Repaint();
            }
            
            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                _searchFilter = "";
                GUI.FocusControl(null);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawPackageList()
        {
            if (_viewModel.Packages == null || _viewModel.Packages.Count == 0)
            {
                EditorGUILayout.HelpBox("패키지 정보를 불러올 수 없습니다.", MessageType.Info);
                return;
            }

            var filteredPackages = FilterPackages(_viewModel.Packages);
            
            if (filteredPackages.Count == 0)
            {
                EditorGUILayout.HelpBox($"검색어 '{_searchFilter}'에 해당하는 패키지가 없습니다.", MessageType.Info);
                return;
            }

            for (var i = 0; i < filteredPackages.Count; i++)
            {
                DrawPackageItem(filteredPackages[i], GetOriginalIndex(_viewModel.Packages, filteredPackages[i]));
            }
        }
        
        private List<PlatformPackageInfo> FilterPackages(List<PlatformPackageInfo> packages)
        {
            if (string.IsNullOrEmpty(_searchFilter))
            {
                return packages;
            }
            
            var result = new List<PlatformPackageInfo>();
            var lowerFilter = _searchFilter.ToLower();
            
            foreach (var package in packages)
            {
                if (package.Name.ToLower().Contains(lowerFilter) || 
                    package.Description.ToLower().Contains(lowerFilter))
                {
                    result.Add(package);
                }
            }
            
            return result;
        }
        
        private int GetOriginalIndex(List<PlatformPackageInfo> originalList, PlatformPackageInfo item)
        {
            for (var i = 0; i < originalList.Count; i++)
            {
                if (originalList[i] == item)
                {
                    return i;
                }
            }
            
            return -1;
        }
        
        private void DrawPackageItem(PlatformPackageInfo package, int index)
        {
            EditorGUILayout.BeginVertical(_packageBoxStyle);
            
            // 패키지 이름과 상태 표시
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(package.Name, _titleStyle);
            GUILayout.FlexibleSpace();
            
            var statusColor = package.IsInstalled ? _installedColor : _notInstalledColor;
            var oldColor = GUI.color;
            GUI.color = statusColor;
            GUILayout.Label(package.IsInstalled ? "설치됨" : "미설치", EditorStyles.boldLabel);
            GUI.color = oldColor;
            
            EditorGUILayout.EndHorizontal();
            
            // 설명
            EditorGUILayout.LabelField(package.Description, _descriptionStyle);
            
            // 패키지 타입 표시
            var packageTypeText = package.Type switch
            {
                PackageType.Git => "Git Repository",
                PackageType.OpenUPM => "OpenUPM Registry",
                PackageType.Web => "Web Documentation",
                _ => "Unknown"
            };
            
            EditorGUILayout.LabelField($"타입: {packageTypeText}", EditorStyles.miniLabel);
            
            // 패키지 ID 또는 URL 표시
            if (!string.IsNullOrEmpty(package.PackageId))
            {
                EditorGUILayout.LabelField($"ID: {package.PackageId}", EditorStyles.miniLabel);
            }
            else if (!string.IsNullOrEmpty(package.Url))
            {
                EditorGUILayout.LabelField($"URL: {package.Url}", EditorStyles.miniLabel);
            }
            
            // 설치 버튼
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUI.enabled = !_isInstalling && !package.IsInstalled;
            
            Color oldBgColor = GUI.backgroundColor;
            
            if (package.Type == PackageType.Web)
            {
                if (GUILayout.Button("웹사이트 열기", GUILayout.Width(100), GUILayout.Height(25)))
                {
                    package.Install();
                }
            }
            else if (!package.IsInstalled)
            {
                GUI.backgroundColor = _installButtonColor;
                
                if (GUILayout.Button("설치", GUILayout.Width(80), GUILayout.Height(25)))
                {
                    _isInstalling = true;
                    EditorUtility.DisplayProgressBar("패키지 설치 중", $"{package.Name} 설치 중...", 0.5f);
                    
                    _viewModel.InstallPackage(index, success =>
                    {
                        _isInstalling = false;
                        EditorUtility.ClearProgressBar();
                        
                        if (success)
                        {
                            EditorUtility.DisplayDialog("설치 완료", $"{package.Name} 패키지가 성공적으로 설치되었습니다.", "확인");
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("설치 실패", $"{package.Name} 패키지 설치 중 오류가 발생했습니다.", "확인");
                        }
                    });
                }
                
                GUI.backgroundColor = oldBgColor;
            }
            
            GUI.enabled = true;
            GUI.backgroundColor = oldBgColor;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        
        #endregion
    }
}