
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ProjectCore.Editor
{
    public static class ManifestEditor
    {
        /// <summary>
        /// 매니페스트 파일에서 중복된 스코프 정의를 제거합니다.
        /// </summary>
        [MenuItem("ActionFit/Utility/Manifest Remove Duplicate Scope Def", priority = 999)]
        public static void RemoveDuplicateScopeDefinitions()
        {
            var manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            
            if (!File.Exists(manifestPath))
            {
                Debug.LogError("manifest.json 파일을 찾을 수 없습니다.");
            }
            
            try
            {
                var json = File.ReadAllText(manifestPath);
                
                // 중복 스코프를 가진 레지스트리 제거
                // package.openupm.com 이름을 가진 레지스트리 전체를 제거
                var pattern = @",?\s*\{\s*""name"":\s*""package\.openupm\.com"",\s*""url"":\s*""https://package\.openupm\.com"",\s*""scopes"":\s*\[\s*""com\.google\.external-dependency-manager""\s*\]\s*\}";
                var updatedJson = Regex.Replace(json, pattern, "");
                
                if (json != updatedJson)
                {
                    File.WriteAllText(manifestPath, updatedJson);
                    AssetDatabase.Refresh();
                    Debug.Log("매니페스트 파일에서 중복된 스코프 정의가 제거되었습니다.");
                }
                
                Debug.Log("중복된 스코프 정의가 매니페스트 파일에서 제거되지 않았습니다.");
            }
            catch (Exception e)
            {
                Debug.LogError($"매니페스트 파일 수정 중 오류 발생: {e.Message}");
            }
        }
    }
}