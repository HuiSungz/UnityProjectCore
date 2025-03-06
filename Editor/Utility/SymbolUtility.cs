using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace ProjectCore.Editor
{
    public class SymbolUtility : EditorWindow
    {
        #region Fields

        private GUIStyle _headerStyle;
        private GUIStyle _symbolBoxStyle;
        private GUIStyle _searchBoxStyle;
        private GUIStyle _titleStyle;
        
        private Texture2D _addIconTexture;
        private Texture2D _removeIconTexture;
        private Texture2D _refreshIconTexture;
        
        private Color _defaultBackgroundColor;
        
        private string _newSymbol = "";
        private string _searchFilter = "";
        private string _symbolToRemove;
        
        private List<string> _currentSymbols = new();
        private BuildTargetGroup _selectedTargetGroup;
        private Vector2 _scrollPosition;
        private bool _showAllSymbols = true;        

        #endregion

        #region Unity Event Methods

        [MenuItem("ActionFit/Utility/Symbol Editor", priority = 998)]
        public static void ShowWindow()
        {
            var window = GetWindow<SymbolUtility>("Symbol Editor");
            window.minSize = new Vector2(450, 350);
            window.Show();
        }
        
        private void OnEnable()
        {
            _selectedTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            RefreshSymbols();
            LoadIcons();
        }
        
        private void OnGUI()
        {
            InitStyles();

            EditorGUILayout.Space(15);
            GUILayout.Label("<color=#4E7FFF>Scripting Define Symbols Editor</color>", _titleStyle);

            EditorGUILayout.BeginVertical();
            
            DrawPlatformSelector();
            EditorGUILayout.Space(15);
            GUICustomLayout.BeginMenuBoxGroup("Add New Symbol", CreateSymbolOptionsMenu());
            DrawAddSymbolSection();
            GUICustomLayout.EndBoxGroup();
            EditorGUILayout.Space(20);
            GUICustomLayout.BeginMenuBoxGroup("Current Symbols", CreateSymbolListOptionsMenu());
            DrawSymbolsList();
            GUICustomLayout.EndBoxGroup();
            
            EditorGUILayout.EndVertical();

            if (_symbolToRemove == null)
            {
                return;
            }
            
            RemoveSymbol(_symbolToRemove);
            _symbolToRemove = null;
            Repaint();
        }
        
        private void InitStyles()
        {
            _headerStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(10, 10, 10, 10)
            };

            _symbolBoxStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 8, 8),
                margin = new RectOffset(5, 5, 2, 2)
            };

            _searchBoxStyle ??= new GUIStyle(EditorStyles.toolbarSearchField)
            {
                fixedHeight = 22,
                margin = new RectOffset(5, 5, 5, 5)
            };

            _titleStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 10, 15),
                margin = new RectOffset(0, 0, 5, 10),
                richText = true
            };

            _defaultBackgroundColor = GUI.backgroundColor;
        }
        
        private void LoadIcons()
        {
            _addIconTexture = EditorGUIUtility.FindTexture("d_Toolbar Plus");
            _removeIconTexture = EditorGUIUtility.FindTexture("d_Toolbar Minus");
            _refreshIconTexture = EditorGUIUtility.FindTexture("d_Refresh");
        }

        #endregion
        
        private void DrawPlatformSelector()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            
            GUILayout.Label("Target Platform:", GUILayout.Width(100));
            
            EditorGUI.BeginChangeCheck();
            _selectedTargetGroup = (BuildTargetGroup)EditorGUILayout.EnumPopup(_selectedTargetGroup);
            if (EditorGUI.EndChangeCheck())
            {
                RefreshSymbols();
            }
            
            if (GUILayout.Button(new GUIContent(_refreshIconTexture, "Refresh Symbols"), GUILayout.Width(30), GUILayout.Height(18)))
            {
                RefreshSymbols();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawAddSymbolSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Enter Symbol Name:", GUILayout.Width(120));
            
            _newSymbol = EditorGUILayout.TextField(_newSymbol);
            
            GUI.enabled = !string.IsNullOrEmpty(_newSymbol) && !_currentSymbols.Contains(_newSymbol);
            if (GUILayout.Button(new GUIContent(_addIconTexture, "Add Symbol"), GUILayout.Width(30), GUILayout.Height(18)))
            {
                AddSymbol(_newSymbol);
                _newSymbol = "";
                GUI.FocusControl(null);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Commonly Used Symbols:", EditorStyles.miniLabel);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("UNITY_PRO_LICENSE", EditorStyles.miniButton))
            {
                _newSymbol = "UNITY_PRO_LICENSE";
            }
            
            if (GUILayout.Button("DEVELOPMENT_BUILD", EditorStyles.miniButton))
            {
                _newSymbol = "DEVELOPMENT_BUILD";
            }
            
            if (GUILayout.Button("ACTIONFIT_DEBUG", EditorStyles.miniButton))
            {
                _newSymbol = "ACTIONFIT_DEBUG";
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawSymbolsList()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Filter:", GUILayout.Width(40));
            _searchFilter = EditorGUILayout.TextField(_searchFilter, _searchBoxStyle);
            if (GUILayout.Button("×", GUILayout.Width(20)))
            {
                _searchFilter = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            if (_currentSymbols.Count == 0)
            {
                EditorGUILayout.HelpBox("No symbols defined for this platform.", MessageType.Info);
            }
            else
            {
                var filteredSymbols = _currentSymbols;
                if (!string.IsNullOrEmpty(_searchFilter))
                {
                    filteredSymbols = _currentSymbols.Where(s => s.ToLower().Contains(_searchFilter.ToLower())).ToList();
                }

                if (filteredSymbols.Count == 0)
                {
                    EditorGUILayout.HelpBox($"No symbols match the filter: '{_searchFilter}'", MessageType.Info);
                }
                else
                {
                    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MinHeight(200));

                    foreach (var symbol in filteredSymbols.ToArray()) // ToArray로 복사본 사용
                    {
                        DrawSymbolItem(symbol);
                    }

                    EditorGUILayout.EndScrollView();
                }
            }

            EditorGUILayout.EndVertical();
        }
        
        private void DrawSymbolItem(string symbol)
        {
            EditorGUILayout.BeginHorizontal(_symbolBoxStyle);
            EditorGUILayout.LabelField(symbol, EditorStyles.boldLabel, GUILayout.ExpandWidth(true));

            GUI.backgroundColor = new Color(1f, 0.7f, 0.7f, 1f);
            if (GUILayout.Button(new GUIContent(_removeIconTexture, "Remove Symbol"), GUILayout.Width(30), GUILayout.Height(18)))
            {
                if (EditorUtility.DisplayDialog("Remove Symbol", 
                        $"Are you sure you want to remove the symbol '{symbol}'?", 
                        "Yes", "Cancel"))
                {
                    _symbolToRemove = symbol;
                }
            }
            GUI.backgroundColor = _defaultBackgroundColor;
            
            EditorGUILayout.EndHorizontal();
        }

        private GenericMenu CreateSymbolOptionsMenu()
        {
            var menu = new GenericMenu();
            
            menu.AddItem(new GUIContent("Predefined Symbols"), false, () => {
                PredefinedSymbolsWindow.ShowWindow(this);
            });
            
            menu.AddSeparator("");
            
            menu.AddItem(new GUIContent("Clear Input"), false, () => {
                _newSymbol = "";
            });
            
            return menu;
        }

        private GenericMenu CreateSymbolListOptionsMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Copy All To Clipboard"), false, () => {
                EditorGUIUtility.systemCopyBuffer = string.Join(";", _currentSymbols);
                Debug.Log("All symbols copied to clipboard.");
            });
            
            menu.AddItem(new GUIContent("Clear All Symbols"), false, () =>
            {
                if (!EditorUtility.DisplayDialog("Clear All Symbols",
                        "Are you sure you want to remove all scripting define symbols?",
                        "Yes", "Cancel"))
                {
                    return;
                }
                
                _currentSymbols.Clear();
                SaveSymbols();
            });
            
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Show All Symbols"), _showAllSymbols, () => {
                _showAllSymbols = !_showAllSymbols;
                RefreshSymbols();
            });
            
            return menu;
        }

        public void AddPredefinedSymbol(string symbol)
        {
            _newSymbol = symbol;
        }
        
        private void RefreshSymbols()
        {
            _currentSymbols = GetCurrentSymbols();
        }

        private List<string> GetCurrentSymbols()
        {
            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(_selectedTargetGroup);
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbols(namedTarget);
            
            return string.IsNullOrEmpty(defineSymbols) 
                ? new List<string>() 
                : defineSymbols.Split(';').ToList();
        }

        #region Add & Remove

        private void AddSymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol) || _currentSymbols.Contains(symbol))
            {
                return;
            }
                
            _currentSymbols.Add(symbol);
            SaveSymbols();
        }
        
        private void RemoveSymbol(string symbol)
        {
            if (!_currentSymbols.Contains(symbol))
            {
                return;
            }
            
            _currentSymbols.Remove(symbol);
            SaveSymbols();
        }
        
        private void SaveSymbols()
        {
            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(_selectedTargetGroup);
            var defineSymbols = string.Join(";", _currentSymbols);
            PlayerSettings.SetScriptingDefineSymbols(namedTarget, defineSymbols);
            
            AssetDatabase.SaveAssets();
            RefreshSymbols();
        }
        
        public static void AddSymbolStatic(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                return;
            }

            var currentTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbols(currentTarget);
            var symbols = string.IsNullOrEmpty(defineSymbols) 
                ? new List<string>() 
                : defineSymbols.Split(';').ToList();

            if (symbols.Contains(symbol))
            {
                return;
            }
            
            symbols.Add(symbol);
            var newDefines = string.Join(";", symbols);
            PlayerSettings.SetScriptingDefineSymbols(currentTarget, newDefines);
            AssetDatabase.SaveAssets();
        }
        
        public static void RemoveSymbolStatic(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                return;
            }

            var currentTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbols(currentTarget);
            var symbols = string.IsNullOrEmpty(defineSymbols) 
                ? new List<string>() 
                : defineSymbols.Split(';').ToList();

            if (!symbols.Contains(symbol))
            {
                return;
            }
            
            symbols.Remove(symbol);
            var newDefines = string.Join(";", symbols);
            PlayerSettings.SetScriptingDefineSymbols(currentTarget, newDefines);
            AssetDatabase.SaveAssets();
        }

        #endregion
        
        public static bool HasSymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                return false;
            }

            var currentTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbols(currentTarget);
            var symbols = string.IsNullOrEmpty(defineSymbols) 
                ? new List<string>() 
                : defineSymbols.Split(';').ToList();
            
            return symbols.Contains(symbol);
        }

        public static void ToggleSymbol(string symbol, bool enable)
        {
            if (enable)
            {
                AddSymbolStatic(symbol);
            }
            else
            {
                RemoveSymbolStatic(symbol);
            }
        }
        
        public static void AddSymbolForTarget(string symbol, BuildTargetGroup targetGroup)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                return;
            }

            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbols(namedTarget);
            var symbols = string.IsNullOrEmpty(defineSymbols) 
                ? new List<string>() 
                : defineSymbols.Split(';').ToList();

            if (symbols.Contains(symbol))
            {
                return;
            }
            
            symbols.Add(symbol);
            var newDefines = string.Join(";", symbols);
            PlayerSettings.SetScriptingDefineSymbols(namedTarget, newDefines);
            AssetDatabase.SaveAssets();
        }
        
        public static void RemoveSymbolForTarget(string symbol, BuildTargetGroup targetGroup)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                return;
            }

            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbols(namedTarget);
            var symbols = string.IsNullOrEmpty(defineSymbols) 
                ? new List<string>() 
                : defineSymbols.Split(';').ToList();

            if (!symbols.Contains(symbol))
            {
                return;
            }
            
            symbols.Remove(symbol);
            var newDefines = string.Join(";", symbols);
            PlayerSettings.SetScriptingDefineSymbols(namedTarget, newDefines);
            AssetDatabase.SaveAssets();
        }
    }

    public class PredefinedSymbolsWindow : EditorWindow
    {
        private static readonly string[][] _predefinedSymbols = {
            new[] { "UNITY_PRO_LICENSE", "Professional Unity license" },
            new[] { "DEVELOPMENT_BUILD", "Development build flag" },
            new[] { "ACTIONFIT_DEBUG", "ActionFit debug mode" },
            new[] { "ACTIONFIT_LOGGING", "Enable extended logging" },
            new[] { "USE_ADDRESSABLES", "Use Addressable Assets system" },
            new[] { "DISABLE_ANALYTICS", "Disable analytics tracking" }
        };

        private static SymbolUtility _symbolUtility;
        private Vector2 _scrollPosition;

        public static void ShowWindow(SymbolUtility symbolUtility)
        {
            _symbolUtility = symbolUtility;
            
            var window = GetWindow<PredefinedSymbolsWindow>("Predefined Symbols");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("Predefined Symbols", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var symbolInfo in _predefinedSymbols)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(symbolInfo[0], EditorStyles.boldLabel);
                EditorGUILayout.LabelField(symbolInfo[1], EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
                
                if (GUILayout.Button("Add", GUILayout.Width(60)))
                {
                    if (_symbolUtility)
                    {
                        _symbolUtility.AddPredefinedSymbol(symbolInfo[0]);
                        Close();
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Close"))
            {
                Close();
            }
        }
    }
}