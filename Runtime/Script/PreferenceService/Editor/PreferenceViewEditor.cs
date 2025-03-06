
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

namespace ProjectCore.Preference
{
    public class PreferenceDataEditor : EditorWindow
    {
        #region Fields

        private PreferenceSettingSO _preferenceSetting;
        private string[] _preferenceFileOptions;
        private int _selectedFileIndex;
        
        // Current file info
        private string _currentFileName;
        private bool _isCurrentFileEncrypted;
        private PreferenceService _currentService;

        // UI state
        private Vector2 _scrollPosition;
        private Vector2 _containerScrollPosition;
        private readonly Dictionary<int, bool> _containerFoldouts = new();
        private readonly Dictionary<int, Dictionary<string, bool>> _propertyFoldouts = new();
        
        // Json edit temp storage (per container)
        private readonly Dictionary<int, string> _editedJson = new();
        
        // Track modified containers
        private readonly HashSet<int> _modifiedContainers = new();

        // Filtering
        private string _searchText = "";
        private bool _showJson;
        private bool _hasUnsavedChanges = false;
        
        private GUIStyle _searchBoxStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _warningLabelStyle;
        
        private Texture2D _refreshIconTexture;
        private Texture2D _saveIconTexture;

        #endregion

        #region Unity Event Methods

        [MenuItem("ActionFit/Preference Data Viewer", priority = 13)]
        public static void ShowWindow()
        {
            var window = GetWindow<PreferenceDataEditor>("Preference Data Viewer");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }
        
        private void OnEnable()
        {
            FindPreferenceSetting();
        }
        
        private void FindPreferenceSetting()
        {
            var guids = AssetDatabase.FindAssets("t:PreferenceSettingSO");

            if (guids.Length <= 0)
            {
                return;
            }
            
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _preferenceSetting = AssetDatabase.LoadAssetAtPath<PreferenceSettingSO>(path);
            if (_preferenceSetting)
            {
                LoadPreferenceFileOptions();
            }
        }
        
        private void LoadPreferenceFileOptions()
        {
            if (!_preferenceSetting || _preferenceSetting.PreferenceFileInfos == null)
            {
                _preferenceFileOptions = Array.Empty<string>();
                return;
            }

            var fileInfos = _preferenceSetting.PreferenceFileInfos;
            _preferenceFileOptions = new string[fileInfos.Count];

            for (var i = 0; i < fileInfos.Count; i++)
            {
                _preferenceFileOptions[i] = fileInfos[i].PreferenceFileName + 
                                            (fileInfos[i].IsEncrypt ? " (Encrypted)" : "");
            }
        }
        
        private void InitStyles()
        {
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
            
            _warningLabelStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = Color.red },
                alignment = TextAnchor.MiddleRight
            };
            
            _refreshIconTexture = EditorGUIUtility.FindTexture("d_Refresh");
            _saveIconTexture = EditorGUIUtility.FindTexture("SaveActive");
        }
        
        private void OnGUI()
        {
            InitStyles();

            EditorGUILayout.Space(15);
            GUILayout.Label("<color=#4E7FFF>Preference Data Viewer</color>", _titleStyle);
            
            EditorGUILayout.BeginVertical();
            
            SettingFileFinder();
            DrawFileSelector();
            EditorGUILayout.Space(7);
            DrawSearchField();
            GUILayout.Space(7);
            DrawActionButtons();
            GUILayout.Space(15);

            if (_currentService != null)
            {
                DrawServiceData();
            }
            else if (_selectedFileIndex >= 0 && _selectedFileIndex < _preferenceFileOptions.Length)
            {
                EditorGUILayout.HelpBox($"'{_preferenceFileOptions[_selectedFileIndex]}' file not loaded.", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }

        private void SettingFileFinder()
        {
            // 설정 파일 로드 UI
            if (_preferenceSetting)
            {
                return;
            }
            
            EditorGUILayout.HelpBox("PreferenceSettingSO not found. Please load the settings file.", MessageType.Warning);

            if (!GUILayout.Button("Find Preference Setting File"))
            {
                return;
            }
                
            var path = EditorUtility.OpenFilePanel("Select PreferenceSettingSO", "Assets", "asset");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            path = "Assets" + path[Application.dataPath.Length..];
            _preferenceSetting = AssetDatabase.LoadAssetAtPath<PreferenceSettingSO>(path);

            if (_preferenceSetting)
            {
                LoadPreferenceFileOptions();
            }
        }

        #endregion

        #region Draw File Selector
        
        private void DrawFileSelector()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            // File selection dropdown
            EditorGUI.BeginChangeCheck();
            
            // Warning if there are unsaved changes
            if (_hasUnsavedChanges)
            {
                EditorGUI.BeginDisabledGroup(true);
                _selectedFileIndex = EditorGUILayout.Popup("Choose Data File:", _selectedFileIndex, _preferenceFileOptions);
                EditorGUI.EndDisabledGroup();
                
                if (GUILayout.Button("Cancel Changes", GUILayout.Width(100)))
                {
                    bool confirmed = EditorUtility.DisplayDialog("Confirm", "You have unsaved changes. Discard changes and select another file?", "Yes", "No");
                    if (confirmed)
                    {
                        ResetChanges();
                    }
                }
            }
            else
            {
                _selectedFileIndex = EditorGUILayout.Popup("Choose Data File:", _selectedFileIndex, _preferenceFileOptions);
                if (EditorGUI.EndChangeCheck() && _preferenceFileOptions.Length > 0)
                {
                    // Reset Json edit temp data when selecting a new file
                    ResetChanges();
                    LoadSelectedFile();
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void ResetChanges()
        {
            _editedJson.Clear();
            _modifiedContainers.Clear();
            _hasUnsavedChanges = false;
        }
        
        private void LoadSelectedFile()
        {
            if (!_preferenceSetting || _selectedFileIndex < 0 || _selectedFileIndex >= _preferenceSetting.PreferenceFileInfos.Count)
            {
                return;
            }

            var fileInfo = _preferenceSetting.PreferenceFileInfos[_selectedFileIndex];
            _currentFileName = fileInfo.PreferenceFileName;
            _isCurrentFileEncrypted = fileInfo.IsEncrypt;

            // Get persistentDataPath
            var persistentDataPath = Application.persistentDataPath;
            var filePath = Path.Combine(persistentDataPath, _currentFileName);

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"File does not exist: {filePath}");
                _currentService = null;
                return;
            }

            try
            {
                if (_isCurrentFileEncrypted)
                {
                    var encryptedData = File.ReadAllBytes(filePath);
                    var decryptedData = PreferenceEncrypt.DecryptBinary(encryptedData);

                    if (decryptedData == null || decryptedData.Length == 0)
                    {
                        Debug.LogError($"Failed to decrypt file: {filePath}");
                        _currentService = null;
                        return;
                    }

                    using var memoryStream = new MemoryStream(decryptedData);
                    var binaryFormatter = new BinaryFormatter();
                    _currentService = (PreferenceService)binaryFormatter.Deserialize(memoryStream);
                }
                else
                {
                    using var fileStream = File.Open(filePath, FileMode.Open);
                    var binaryFormatter = new BinaryFormatter();
                    _currentService = (PreferenceService)binaryFormatter.Deserialize(fileStream);
                }
                
                _currentService.Initialize();
                Debug.Log($"File loaded successfully: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading file: {e.Message}");
                _currentService = null;
            }
        }
        
        #endregion

        #region Search & Buttons

        private void DrawSearchField()
        {
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label("Search:", GUILayout.Width(40));
            _searchText = EditorGUILayout.TextField(_searchText, _searchBoxStyle);
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            
            // Refresh button
            if (GUILayout.Button(new GUIContent(_refreshIconTexture, "Reload file (Refresh)"),
                    GUILayout.Width(30), GUILayout.Height(18)))
            {
                if (_hasUnsavedChanges)
                {
                    bool confirmed = EditorUtility.DisplayDialog("Confirm", "You have unsaved changes. Discard changes and refresh?", "Yes", "No");
                    if (confirmed)
                    {
                        ResetChanges();
                        LoadSelectedFile();
                    }
                }
                else
                {
                    LoadSelectedFile();
                }
            }
            
            // Save button (enabled only when there are changes)
            EditorGUI.BeginDisabledGroup(!_hasUnsavedChanges);
            if (GUILayout.Button(new GUIContent(_saveIconTexture, "Save changes to file"),
                    GUILayout.Width(30), GUILayout.Height(18)))
            {
                SaveChangesToFile();
            }
            EditorGUI.EndDisabledGroup();
            
            // JSON view mode toggle
            _showJson = EditorGUILayout.Toggle("View Json Mode", _showJson);
            
            // Warning for unsaved changes
            if (_hasUnsavedChanges)
            {
                GUILayout.Label("* Unsaved Changes", _warningLabelStyle);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void SaveChangesToFile()
        {
            if (_currentService == null || !_hasUnsavedChanges)
            {
                return;
            }

            bool confirmed = EditorUtility.DisplayDialog("Confirm Save", 
                "Are you sure you want to save these changes to the preference file?\n\nThis action cannot be undone.", 
                "Save", "Cancel");
                
            if (!confirmed)
            {
                return;
            }

            try
            {
                // Flush the service to prepare for serialization
                _currentService.Flush();
                
                // Get the file path
                string persistentDataPath = Application.persistentDataPath;
                string filePath = Path.Combine(persistentDataPath, _currentFileName);
                
                // Serialize and save
                if (_isCurrentFileEncrypted)
                {
                    // For encrypted files, serialize to memory stream first, then encrypt
                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(ms, _currentService);
                        
                        byte[] serializedData = ms.ToArray();
                        byte[] encryptedData = PreferenceEncrypt.EncryptBinary(serializedData);
                        
                        if (encryptedData == null)
                        {
                            Debug.LogError("Failed to encrypt the data.");
                            EditorUtility.DisplayDialog("Error", "Failed to encrypt the data.", "OK");
                            return;
                        }
                        
                        File.WriteAllBytes(filePath, encryptedData);
                    }
                }
                else
                {
                    // For non-encrypted files, serialize directly to file
                    using (FileStream fs = File.Open(filePath, FileMode.Create))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(fs, _currentService);
                    }
                }
                
                Debug.Log($"Successfully saved preference file: {filePath}");
                EditorUtility.DisplayDialog("Success", "Changes saved successfully!", "OK");
                
                // Reset change tracking
                ResetChanges();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving file: {e.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to save file: {e.Message}", "OK");
            }
        }

        #endregion

        #region Draw Data

        private void DrawServiceData()
        {
            if (_currentService == null) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("FileName:", EditorStyles.boldLabel, GUILayout.Width(110));
            EditorGUILayout.LabelField(_currentFileName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("IsEncrypted:", EditorStyles.boldLabel, GUILayout.Width(110));
            EditorGUILayout.LabelField(_isCurrentFileEncrypted ? "YES" : "NO");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            // Get containers field
            var containersField = typeof(PreferenceService).GetField("_prefContainers",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (containersField == null)
            {
                EditorGUILayout.HelpBox("Cannot retrieve PreferenceContainer information.", MessageType.Error);
                EditorGUILayout.EndVertical();
                return;
            }

            if (containersField.GetValue(_currentService) is not 
                    PreferenceContainer[] containers || containers.Length == 0)
            {
                EditorGUILayout.HelpBox("No data available.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.LabelField($"Total, {containers.Length} Container(s)", EditorStyles.boldLabel);

            _containerScrollPosition = EditorGUILayout.BeginScrollView(_containerScrollPosition);

            for (int i = 0; i < containers.Length; i++)
            {
                var container = containers[i];

                // Filter by search text
                if (!string.IsNullOrEmpty(_searchText))
                {
                    var jsonField = typeof(PreferenceContainer).GetField("_json",
                            BindingFlags.Instance | BindingFlags.NonPublic)
                        ?.GetValue(container) as string;

                    if (jsonField == null || !jsonField.Contains(_searchText))
                    {
                        continue;
                    }
                }
                
                var hashObj = typeof(PreferenceContainer).GetField("_hash", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(container);
                if (hashObj == null) continue;
                var hash = (int)hashObj;
                var jsonFieldOriginal = typeof(PreferenceContainer).GetField("_json",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                var originalJson = jsonFieldOriginal?.GetValue(container) as string ?? "";

                _containerFoldouts.TryAdd(hash, false);
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                _containerFoldouts[hash] =
                    EditorGUILayout.Foldout(_containerFoldouts[hash], $"Container #{i + 1} (HASH: {hash})");
                
                // Show indicator if this container has been modified
                if (_modifiedContainers.Contains(hash))
                {
                    GUILayout.Label("*", _warningLabelStyle, GUILayout.Width(15));
                }
                
                EditorGUILayout.EndHorizontal();

                if (_containerFoldouts[hash])
                {
                    EditorGUI.indentLevel++;

                    if (_showJson)
                    {
                        EditorGUILayout.LabelField("JSON Data:", EditorStyles.boldLabel);
                        
                        // Initialize temp Json value (formatted JSON)
                        if (!_editedJson.ContainsKey(hash))
                        {
                            _editedJson[hash] = FormatJson(originalJson);
                        }
                        
                        // Display editable TextArea
                        var editedJson = EditorGUILayout.TextArea(
                            _editedJson[hash],
                            GUILayout.ExpandWidth(true),
                            GUILayout.Height(150)
                        );
                        if (editedJson != _editedJson[hash])
                        {
                            _editedJson[hash] = editedJson;
                            
                            // Mark container as modified
                            if (editedJson != originalJson && !_modifiedContainers.Contains(hash))
                            {
                                _modifiedContainers.Add(hash);
                                _hasUnsavedChanges = true;
                            }
                        }
                        
                        if (editedJson != originalJson)
                        {
                            try
                            {
                                // Validate JSON is valid
                                Newtonsoft.Json.JsonConvert.DeserializeObject(editedJson);
                                
                                // Apply edited Json value directly to container (using reflection)
                                if (jsonFieldOriginal != null)
                                {
                                    jsonFieldOriginal.SetValue(container, editedJson);
                                    
                                    // Mark as changed, needs to be saved
                                    if (!_modifiedContainers.Contains(hash))
                                    {
                                        _modifiedContainers.Add(hash);
                                        _hasUnsavedChanges = true;
                                    }
                                }
                            }
                            catch (Newtonsoft.Json.JsonException jsonEx)
                            {
                                EditorUtility.DisplayDialog("Invalid JSON", $"The JSON is not valid:\n{jsonEx.Message}", "OK");
                                // Revert to original JSON
                                _editedJson[hash] = originalJson;
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            DrawJsonData(hash, originalJson);
                        }
                        catch (Exception e)
                        {
                            EditorGUILayout.HelpBox($"JSON Parse Error: {e.Message}", MessageType.Error);
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawJsonData(int hash, string json)
        {
            if (string.IsNullOrEmpty(json)) return;

            Dictionary<string, object> data =
                Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            if (data == null) return;

            // Initialize property foldouts
            if (!_propertyFoldouts.ContainsKey(hash))
            {
                _propertyFoldouts[hash] = new Dictionary<string, bool>();
            }

            foreach (var kvp in data)
            {
                if (kvp.Key == "AssociatedPreferenceKey")
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Associated FileName Key:", EditorStyles.boldLabel, GUILayout.Width(180));
                    EditorGUILayout.LabelField(kvp.Value?.ToString() ?? "null");
                    EditorGUILayout.EndHorizontal();
                    continue;
                }

                // Initialize property foldout
                if (!_propertyFoldouts[hash].ContainsKey(kvp.Key))
                {
                    _propertyFoldouts[hash][kvp.Key] = false;
                }

                // Complex object case
                if (kvp.Value != null && (kvp.Value is Newtonsoft.Json.Linq.JObject ||
                                          kvp.Value is Newtonsoft.Json.Linq.JArray))
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    _propertyFoldouts[hash][kvp.Key] =
                        EditorGUILayout.Foldout(_propertyFoldouts[hash][kvp.Key], kvp.Key);

                    if (_propertyFoldouts[hash][kvp.Key])
                    {
                        EditorGUI.indentLevel++;

                        if (kvp.Value is Newtonsoft.Json.Linq.JObject jObj)
                        {
                            foreach (var prop in jObj.Properties())
                            {
                                DrawProperty(prop.Name, prop.Value);
                            }
                        }
                        else if (kvp.Value is Newtonsoft.Json.Linq.JArray jArr)
                        {
                            for (int i = 0; i < jArr.Count; i++)
                            {
                                EditorGUILayout.LabelField($"[{i}]:", EditorStyles.boldLabel);
                                EditorGUI.indentLevel++;

                                if (jArr[i] is Newtonsoft.Json.Linq.JObject itemObj)
                                {
                                    foreach (var prop in itemObj.Properties())
                                    {
                                        DrawProperty(prop.Name, prop.Value);
                                    }
                                }
                                else
                                {
                                    EditorGUILayout.LabelField(jArr[i]?.ToString() ?? "null");
                                }

                                EditorGUI.indentLevel--;
                            }
                        }

                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.EndVertical();
                }
                else
                {
                    // Simple value case
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(kvp.Key + ":", GUILayout.Width(150));
                    EditorGUILayout.LabelField(kvp.Value?.ToString() ?? "null");
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void DrawProperty(string name, object value)
        {
            if (value is Newtonsoft.Json.Linq.JObject jObj)
            {
                EditorGUILayout.LabelField(name + ":", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                foreach (var prop in jObj.Properties())
                {
                    DrawProperty(prop.Name, prop.Value);
                }

                EditorGUI.indentLevel--;
            }
            else if (value is Newtonsoft.Json.Linq.JArray jArr)
            {
                EditorGUILayout.LabelField(name + ":", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                for (int i = 0; i < jArr.Count; i++)
                {
                    EditorGUILayout.LabelField($"[{i}]:");
                    EditorGUI.indentLevel++;

                    if (jArr[i] is Newtonsoft.Json.Linq.JObject itemObj)
                    {
                        foreach (var prop in itemObj.Properties())
                        {
                            DrawProperty(prop.Name, prop.Value);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField(jArr[i]?.ToString() ?? "null");
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(name + ":", GUILayout.Width(150));
                EditorGUILayout.LabelField(value?.ToString() ?? "null");
                EditorGUILayout.EndHorizontal();
            }
        }

        private string FormatJson(string json)
        {
            try
            {
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            catch
            {
                return json;
            }
        }

        #endregion
    }
}
#endif