
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using ProjectCore.Module;

namespace ProjectCore.Editor
{
    [CustomEditor(typeof(ProjectModules))]
    internal class ProjectModulesEditor : UnityEditor.Editor
    {
        #region Fields

        private const string CreateAssetMenuPath = GlobalAccess.MenuItemCreateMenuRoot + "ProjectModules";
        private const string CreateMenuPath = GlobalAccess.ToolBarMenuRoot + "Project Modules";
        private const string MODULES_FIELD_NAME = "_projectModules";
        
        private SerializedProperty _modulesProperty;
        private List<ModuleContainer> _moduleContainers;
        
        private ProjectModules _projectModules;
        private GenericMenu _modulesGenericMenu;
        
        private static ModulesHandler _modulesHandler;

        #endregion

        #region Menu

        [MenuItem(CreateMenuPath, priority = 0)]
        public static void SelectProjectModules()
        {
            var selectedObject = GetProjectModules();
            if (selectedObject)
            {
                Selection.activeObject = selectedObject;
            }
            else
            {
                if (!Directory.Exists(GlobalAccess.MainAssetPath))
                {
                    Directory.CreateDirectory(GlobalAccess.MainAssetPath);
                    AssetDatabase.Refresh();
                }
                
                CreateAsset(GlobalAccess.MainAssetPath, true);
            }
        }
        
        [MenuItem(CreateAssetMenuPath)]
        public static void CreateAssetFromMenu()
        {
            var projectModules = GetProjectModules();
            if (projectModules)
            {
                Debug.Log("ProjectModules 파일이 이미 존재합니다!");
                EditorGUIUtility.PingObject(projectModules);
                return;
            }
            
            var selectionPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            selectionPath = Path.GetDirectoryName(selectionPath);

            if (string.IsNullOrEmpty(selectionPath) || !Directory.Exists(selectionPath))
            {
                selectionPath = GlobalAccess.MainAssetPath;
            }

            CreateAsset(selectionPath, true);
        }
        
        private static ProjectModules GetProjectModules()
        {
            var guids = AssetDatabase.FindAssets("t:ProjectModules");
            if (guids.Length <= 0)
            {
                return null;
            }
            
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<ProjectModules>(path);
        }

        #endregion

        #region Life Cycle

        protected void OnEnable()
        {
            _projectModules = (ProjectModules)target;
            _modulesHandler = new ModulesHandler();
            _modulesProperty = serializedObject.FindProperty(MODULES_FIELD_NAME);

            InitGenericMenu();
            InitCoreModules(_projectModules.Modules);

            LoadEditorsList();

            EditorApplication.playModeStateChanged += LogPlayModeState;
        }
        
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= LogPlayModeState;
        }
        
        private void OnDestroy()
        {
            ClearModuleContainers();
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUICustomLayout.BeginMenuBoxGroup("Project Modules", _modulesGenericMenu);

            DrawModules(_modulesProperty);

            GUICustomLayout.EndBoxGroup();

            GUILayout.FlexibleSpace();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Initialize

        private void InitGenericMenu()
        {
            _modulesGenericMenu = new GenericMenu();

            // 현재 모듈 목록 로드
            var modules = _projectModules.Modules;

            // 등록된 모든 모듈 타입 가져오기
            var registeredTypes = GetRegisteredModuleTypes();
            foreach (var type in registeredTypes)
            {
                var moduleAttribute = GetModuleAttribute(type);
                if (moduleAttribute is not { IsCore: false })
                {
                    continue;
                }
                
                var isAlreadyActive = modules != null && modules.Any(modularSO => modularSO && modularSO.GetType() == type);
                if (isAlreadyActive)
                {
                    _modulesGenericMenu.AddDisabledItem(new GUIContent("Add Module/" + moduleAttribute.Path), false);
                }
                else
                {
                    _modulesGenericMenu.AddItem(new GUIContent("Add Module/" + moduleAttribute.Path), false, delegate
                    {
                        AddModule(type);
                        InitGenericMenu();
                    });
                }
            }
        }
        
        private void InitCoreModules(BaseProjectModuleSO[] modules)
        {
            var requiredModules = GetRequiredModules(modules);
            if (requiredModules.Length <= 0)
            {
                return;
            }
            
            foreach (var requiredModule in requiredModules)
            {
                AddModule(requiredModule.Type);
            }

            LoadEditorsList();

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }

        #endregion

        #region Modular

        private void AddModule(Type moduleType)
        {
            if(!moduleType.IsSubclassOf(typeof(BaseProjectModuleSO)))
            {
                Debug.LogError($"[Project Modules Editor]: 모듈 타입은 BaseProjectModuleSO의 하위 클래스여야 합니다: {moduleType.Name}");
                return;
            }

            Undo.RecordObject(target, "Add Module");

            _modulesProperty = serializedObject.FindProperty(MODULES_FIELD_NAME);
            serializedObject.Update();
            _modulesProperty.arraySize++;

            var modularSO = (BaseProjectModuleSO)CreateInstance(moduleType);
            modularSO.name = moduleType.Name;
            modularSO.hideFlags = HideFlags.HideInHierarchy; 
            
            AssetDatabase.AddObjectToAsset(modularSO, target);
            
            _modulesProperty.GetArrayElementAtIndex(_modulesProperty.arraySize - 1).objectReferenceValue = modularSO;
            serializedObject.ApplyModifiedProperties();
            LoadEditorsList();

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
        
        private void DrawModules(SerializedProperty arrayProperty)
        {
            var current = Event.current;
            if (arrayProperty.arraySize <= 0)
            {
                EditorGUILayout.HelpBox("You don't have module", MessageType.Info);
                return;
            }
            
            for (var iterIndex = 0; iterIndex < arrayProperty.arraySize; iterIndex++)
            {
                var moduleProperty = arrayProperty.GetArrayElementAtIndex(iterIndex);

                if (moduleProperty.objectReferenceValue)
                {
                    var modularSO = (BaseProjectModuleSO)moduleProperty.objectReferenceValue;
                    var moduleSerializedObject = new SerializedObject(moduleProperty.objectReferenceValue);

                    moduleSerializedObject.Update();

                    // 모듈 박스
                    var blockRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    // 헤더 배경
                    GUI.Box(new Rect(blockRect.x, blockRect.y, blockRect.width, 21), GUIContent.none);
                    GUILayout.Space(14);
                    // 모듈 이름 표시 (폴드아웃 없이)
                    EditorGUI.LabelField(new Rect(blockRect.x + 28, blockRect.y, blockRect.width - 55, 21), modularSO.Name);
                    // 토글(폴드아웃)을 우측으로 이동
                    moduleProperty.isExpanded = EditorGUI.Foldout(
                        new Rect(blockRect.x + 20, blockRect.y, 25, 21), 
                        moduleProperty.isExpanded, 
                        "", 
                        true
                    );

                    if (moduleProperty.isExpanded)
                    {
                        GUILayout.Space(12);
                        var moduleContainer = GetEditor(moduleProperty.objectReferenceValue.GetType());
                        if (moduleContainer == null)
                        {
                            continue;
                        }

                        moduleContainer.OnInspectorGUI();

                        GUILayout.Space(10);
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        moduleContainer.DrawButtons();

                        if(!moduleContainer.IsCore)
                        {
                            if (GUILayout.Button("Remove", GUILayout.Width(90)))
                            {
                                if (EditorUtility.DisplayDialog("Remove Module"
                                        , "Are you sure you want to delete this module?"
                                        , "YES"
                                        , "NO"))
                                {
                                    moduleContainer.OnRemoved();

                                    var removedObject = moduleProperty.objectReferenceValue;
                                    moduleProperty.isExpanded = false;
                                    arrayProperty.DeleteArrayElementAtIndex(iterIndex);

                                    LoadEditorsList();
                                    AssetDatabase.RemoveObjectFromAsset(removedObject);
                                    DestroyImmediate(removedObject, true);
                                    EditorUtility.SetDirty(target);
                                    AssetDatabase.SaveAssets(); 
                                    
                                    InitGenericMenu();

                                    return;
                                }
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();

                    // 메뉴 버튼
                    if(GUI.Button(new Rect(blockRect.x + blockRect.width - 20, blockRect.y + 2, 17, 17), "="))
                    {
                        var index = iterIndex;
                        var genericMenu = new GenericMenu();
                        if (iterIndex > 0)
                        {
                            genericMenu.AddItem(new GUIContent("Move Up"), false, delegate
                            {
                                var expandState = arrayProperty.GetArrayElementAtIndex(index - 1).isExpanded;
                                arrayProperty.MoveArrayElement(index, index - 1);
                                arrayProperty.GetArrayElementAtIndex(index - 1).isExpanded = moduleProperty.isExpanded;
                                arrayProperty.GetArrayElementAtIndex(index).isExpanded = expandState;
                                serializedObject.ApplyModifiedProperties();
                            });
                        }
                        else
                        {
                            genericMenu.AddDisabledItem(new GUIContent("Move Up"), false);
                        }

                        if (iterIndex + 1 < arrayProperty.arraySize)
                        {
                            genericMenu.AddItem(new GUIContent("Move Down"), false, delegate
                            {
                                var expandState = arrayProperty.GetArrayElementAtIndex(index + 1).isExpanded;
                                arrayProperty.MoveArrayElement(index, index + 1);
                                arrayProperty.GetArrayElementAtIndex(index + 1).isExpanded = moduleProperty.isExpanded;
                                arrayProperty.GetArrayElementAtIndex(index).isExpanded = expandState;
                                serializedObject.ApplyModifiedProperties();
                            });
                        }
                        else
                        {
                            genericMenu.AddDisabledItem(new GUIContent("Move Down"), false);
                        }

                        var moduleContainer = GetEditor(moduleProperty.objectReferenceValue.GetType());
                        moduleContainer?.PrepareMenuItems(ref genericMenu);
                        genericMenu.ShowAsContext();
                    }

                    moduleSerializedObject.ApplyModifiedProperties();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(EditorGUIUtility.IconContent("console.warnicon"), GUILayout.Width(16), GUILayout.Height(16));
                    EditorGUILayout.LabelField("Null Reference");
                    if (GUILayout.Button("Remove", EditorStyles.miniButton))
                    {
                        arrayProperty.DeleteArrayElementAtIndex(iterIndex);

                        InitGenericMenu();

                        GUIUtility.ExitGUI();
                        Event.current.Use();

                        return;
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();
                }
                
                GUILayout.Space(5); // 모듈 사이 약간의 간격
            }
        }

        #endregion

        #region Utility
        
        private void ClearModuleContainers()
        {
            if (_moduleContainers != null)
            {
                foreach (var t in _moduleContainers
                             .Where(t => t != null && t.Editor))
                {
                    DestroyImmediate(t.Editor);
                }

                _moduleContainers.Clear();
            }
            else
            {
                _moduleContainers = new List<ModuleContainer>();
            }
        }

        private void LoadEditorsList()
        {
            ClearModuleContainers();

            for (var i = 0; i < _modulesProperty.arraySize; i++)
            {
                var moduleProperty = _modulesProperty.GetArrayElementAtIndex(i);

                if (!moduleProperty.objectReferenceValue)
                {
                    continue;
                }
                
                var moduleSerializedObject = new SerializedObject(moduleProperty.objectReferenceValue);
                _moduleContainers.Add(new ModuleContainer(
                    moduleProperty.objectReferenceValue.GetType(), 
                    moduleSerializedObject, 
                    CreateEditor(moduleSerializedObject.targetObject), 
                    _modulesHandler.IsCoreModule(moduleProperty.objectReferenceValue.GetType())
                ));
            }
        }

        private static void CreateAsset(string folderPath, bool ping)
        {
            var projectModules = CreateInstance<ProjectModules>();
            projectModules.name = "Project Modules";
            
            var assetPath = Path.Combine(folderPath, projectModules.name + ".asset");
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            
            AssetDatabase.CreateAsset(projectModules, assetPath);
            AssetDatabase.SaveAssets();

            var serializedObject = new SerializedObject(projectModules);
            serializedObject.Update();

            var coreModulesProperty = serializedObject.FindProperty(MODULES_FIELD_NAME);

            var requiredModules = GetRequiredModules(null);
            var modules = new List<BaseProjectModuleSO>();
            foreach (var requiredModule in requiredModules)
            {
                // 코어 모듈 생성
                var modularSO = (BaseProjectModuleSO)CreateInstance(requiredModule.Type);
                modularSO.name = requiredModule.Type.Name;
                modularSO.hideFlags = HideFlags.HideInHierarchy;

                AssetDatabase.AddObjectToAsset(modularSO, projectModules);

                coreModulesProperty.arraySize++;
                coreModulesProperty.GetArrayElementAtIndex(coreModulesProperty.arraySize - 1).objectReferenceValue = modularSO;

                modules.Add(modularSO);
            }

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(projectModules);
            AssetDatabase.SaveAssets();

            if (ping)
            {
                EditorGUIUtility.PingObject(projectModules);
            }
        }
        
        private ModuleContainer GetEditor(Type type)
        {
            return _moduleContainers.FirstOrDefault(t => t.Type == type);
        }
        
        private void LogPlayModeState(PlayModeStateChange obj)
        {
            if (Equals(Selection.activeObject, target))
            {
                Selection.activeObject = null;
            }
        }
        
        private ProjectModuleAttribute GetModuleAttribute(Type type)
        {
            return (ProjectModuleAttribute)Attribute.GetCustomAttribute(type, typeof(ProjectModuleAttribute));
        }
        
        private IEnumerable<Type> GetRegisteredModuleTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes().Where(type => type.IsClass 
                                                              && !type.IsAbstract 
                                                              && type.IsSubclassOf(typeof(BaseProjectModuleSO)));
                foreach (var type in types)
                {
                    yield return type;
                }
            }
        }
        
        private static RequiredModule[] GetRequiredModules(BaseProjectModuleSO[] modules)
        {
            // 등록된 모든 모듈 타입 가져오기
            var registeredTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(BaseProjectModuleSO)));

            var requiredModules = (from type in registeredTypes let moduleAttribute 
                = (ProjectModuleAttribute)Attribute.GetCustomAttribute(type, typeof(ProjectModuleAttribute)) 
                where moduleAttribute is { IsCore: true } 
                let isExists = modules != null && modules.Any(x => x != null && x.GetType() == type) 
                where !isExists select new RequiredModule(moduleAttribute, type)).ToList();

            return requiredModules.OrderByDescending(x => x.Attribute.Order).ToArray();
        }

        #endregion

        #region Private - Class

        private class ModulesHandler
        {
            private readonly IEnumerable<ModuleData> _modulesData;

            public ModulesHandler()
            {
                _modulesData = GetModulesData();
            }

            private IEnumerable<ModuleData> GetModulesData()
            {
                var registeredTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(BaseProjectModuleSO)));

                foreach (var type in registeredTypes)
                {
                    var moduleAttribute = (ProjectModuleAttribute)Attribute.GetCustomAttribute(type, typeof(ProjectModuleAttribute));
                    if (moduleAttribute != null)
                    {
                        yield return new ModuleData()
                        {
                            ClassType = type,
                            Attribute = moduleAttribute
                        };
                    }
                }
            }

            public bool IsCoreModule(Type type)
            {
                return _modulesData.Any(data => type == data.ClassType && data.Attribute.IsCore);
            }

            public class ModuleData
            {
                public Type ClassType;
                public ProjectModuleAttribute Attribute;
            }
        }
        
        private class ModuleContainer
        {
            public readonly Type Type;
            public readonly UnityEditor.Editor Editor;
            public readonly bool IsCore;

            public ModuleContainer(Type type, SerializedObject serializedObject, UnityEditor.Editor editor, bool isCore)
            {
                Type = type;
                Editor = editor;
                IsCore = isCore;
            }

            public void OnInspectorGUI()
            {
                if (!Editor)
                {
                    return;
                }
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                
                EditorGUILayout.BeginVertical();
                Editor.OnInspectorGUI();
                EditorGUILayout.EndVertical();
                
                GUILayout.Space(10);
                EditorGUILayout.EndHorizontal();
            }

            public void DrawButtons()
            {
                // 모듈별 커스텀 버튼이 필요한 경우 여기에 구현
            }

            public void PrepareMenuItems(ref GenericMenu genericMenu)
            {
                // 모듈별 컨텍스트 메뉴 항목이 필요한 경우 여기에 구현
            }

            public void OnRemoved()
            {
                // 모듈이 제거될 때 필요한 작업이 있는 경우 여기에 구현
            }
        }

        private class RequiredModule
        {
            public ProjectModuleAttribute Attribute { get; private set; }
            public Type Type { get; private set; }

            public RequiredModule(ProjectModuleAttribute attribute, Type type)
            {
                Attribute = attribute;
                Type = type;
            }
        }

        #endregion
    }
}