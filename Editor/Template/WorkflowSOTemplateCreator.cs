
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.ProjectWindowCallback;

namespace ProjectCore.Editor
{
    internal class WorkflowSOTemplateCreator : EditorWindow
    {
        private const string MENU_PATH = GlobalAccess.MenuItemCreateMenuRoot + "WorkflowSO Script";
        private const string TEMPLATE_RESOURCE_PATH = "WorkflowTemplateScript";

        [MenuItem(MENU_PATH, priority = -98)]
        private static void CreateWorkflowSOScript()
        {
            var directoryPath = GetSelectedDirectoryPath();
            var templateAsset = Resources.Load<TextAsset>(TEMPLATE_RESOURCE_PATH);
            if (!templateAsset)
            {
                Debug.LogError("템플릿 파일을 찾을 수 없습니다. 'WorkflowTemplateScript.txt' 파일이 Editor/Resources 폴더 내에 존재하는지 확인해주세요.");
                return;
            }
            
            var templateContent = templateAsset.text;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                ScriptableObject.CreateInstance<DoCreateWorkflowSOScriptAsset>(),
                directoryPath + "/NewWorkflowSO.cs",
                null,
                templateContent
            );
        }
        
        private static string GetSelectedDirectoryPath()
        {
            var path = "Assets";
            foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (path.Length == 0)
                {
                    continue;
                }

                if (Directory.Exists(path))
                {
                    break;
                }

                if (File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                }
                
                break;
            }
            
            return path;
        }
        
        private class DoCreateWorkflowSOScriptAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var className = Path.GetFileNameWithoutExtension(pathName);
                className = Regex.Replace(className, @"[^a-zA-Z0-9_]", "_");
                if (char.IsDigit(className[0]))
                {
                    className = "_" + className;
                }

                var templateContent = resourceFile;
                var scriptContent = templateContent.Replace("[ScriptName]", className);
                
                File.WriteAllText(pathName, scriptContent);
                AssetDatabase.ImportAsset(pathName);
                
                var createdAsset = AssetDatabase.LoadAssetAtPath<Object>(pathName);
                if (!createdAsset)
                {
                    return;
                }
                
                Selection.activeObject = createdAsset;
                EditorGUIUtility.PingObject(createdAsset);
            }
        }
    }
}