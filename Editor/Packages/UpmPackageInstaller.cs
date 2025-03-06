
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ProjectCore.Editor
{
    public static class UpmPackageInstaller
    {
        private static AddRequest _addRequest;
        private static Action<bool> _onComplete;
        
        // OpenUPM 레지스트리 상수
        private const string OpenUpmRegistryName = "package.openupm.com";
        private const string OpenUpmRegistryUrl = "https://package.openupm.com";
        
        /// <summary>
        /// Install a package from OpenUPM registry.
        /// </summary>
        public static void InstallPackage(string packageId, Action<bool> onComplete = null)
        {
            _onComplete = onComplete;

            if (string.IsNullOrEmpty(packageId))
            {
                Debug.LogError("패키지 ID가 비어 있습니다.");
                _onComplete?.Invoke(false);
                return;
            }
            
            if (IsPackageInstalled(packageId))
            {
                Debug.Log($"패키지가 이미 설치되어 있습니다: {packageId}");
                _onComplete?.Invoke(true);
                return;
            }
            
            // OpenUPM 레지스트리 확인 및 추가
            if (!EnsureOpenUPMRegistry(packageId))
            {
                Debug.LogError($"OpenUPM 레지스트리 구성 실패: {packageId}");
                _onComplete?.Invoke(false);
                return;
            }

            // 패키지 설치 시작 (짧은 지연 후)
            EditorApplication.delayCall += () =>
            {
                Debug.Log($"OpenUPM 패키지 설치 시작: {packageId}");
                _addRequest = Client.Add(packageId);
                EditorApplication.update += OnPackageInstallProgress;
            };
        }
        
        /// <summary>
        /// Check if package is already installed.
        /// </summary>
        private static bool IsPackageInstalled(string packageId)
        {
            return PackageValidator.IsInstallValidation(packageId);
        }
        
        /// <summary>
        /// Package installation progress.
        /// </summary>
        private static void OnPackageInstallProgress()
        {
            if (_addRequest is not { IsCompleted: true })
            {
                return;
            }
            
            EditorApplication.update -= OnPackageInstallProgress;
            
            switch (_addRequest.Status)
            {
                case StatusCode.Success:
                    Debug.Log($"패키지 설치 성공: {_addRequest.Result.displayName}");
                    _onComplete?.Invoke(true);
                    break;
                case >= StatusCode.Failure:
                    Debug.LogError($"OpenUPM 패키지 설치 실패: {_addRequest.Error.message}");
                    _onComplete?.Invoke(false);
                    break;
            }
            
            _addRequest = null;
        }
        
        /// <summary>
        /// Ensure OpenUPM registry is configured for the package.
        /// </summary>
        private static bool EnsureOpenUPMRegistry(string packageId)
        {
            var packages = new List<string> { packageId };
            return new OpenUPMRegistryManager().EnsureRegistry(packages);
        }
        
        /// <summary>
        /// OpenUPM Registry Manager for handling the manifest.
        /// </summary>
        private class OpenUPMRegistryManager
        {
            public bool EnsureRegistry(List<string> openUPMPackages)
            {
                var manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");

                if (!File.Exists(manifestPath))
                {
                    Debug.LogError("[패키지 설치] manifest.json 파일을 찾을 수 없습니다");
                    return false;
                }

                try
                {
                    var json = File.ReadAllText(manifestPath);
                    var needsUpdate = false;

                    // OpenUPM 패키지에서 고유한 스코프 목록 추출
                    var scopes = new HashSet<string>();
                    foreach (var package in openUPMPackages)
                    {
                        if (string.IsNullOrEmpty(package))
                        {
                            continue;
                        }
                        
                        var parts = package.Split('.');
                        scopes.Add(parts.Length >= 2 
                            ? $"{parts[0]}.{parts[1]}" 
                            : package);
                    }

                    if (scopes.Count == 0)
                    {
                        return true;
                    }

                    // OpenUPM 레지스트리가 없는 경우 새로 추가
                    if (!json.Contains(OpenUpmRegistryUrl))
                    {
                        var scopesList = scopes.ToList();
                        var scopesJson = "";
                        for (var i = 0; i < scopesList.Count; i++)
                        {
                            scopesJson += $"        \"{scopesList[i]}\"";
                            if (i < scopesList.Count - 1)
                                scopesJson += ",\n";
                        }

                        var registryEntry =
                            "\"scopedRegistries\": [\n" +
                            "    {\n" +
                            $"      \"name\": \"{OpenUpmRegistryName}\",\n" +
                            $"      \"url\": \"{OpenUpmRegistryUrl}\",\n" +
                            "      \"scopes\": [\n" +
                            scopesJson + "\n" +
                            "      ]\n" +
                            "    }\n" +
                            "  ],";

                        json = json.Contains("\"dependencies\":") 
                            ? json.Replace("\"dependencies\":", registryEntry + "\n  \"dependencies\":") 
                            : json.Insert(1, registryEntry + "\n");

                        needsUpdate = true;
                    }
                    else
                    {
                        // 이미 등록된 경우 스코프 추가 (package.openupm.com 이름 확인)
                        foreach (var scope in scopes)
                        {
                            if (json.Contains($"\"{scope}\""))
                            {
                                continue;
                            }
                            
                            // 기존 레지스트리 이름을 확인하여 적절한 레지스트리에 스코프 추가
                            var openupmPattern = $"\"name\"\\s*:\\s*\"(OpenUPM|{OpenUpmRegistryName})\"[\\s\\S]*?\"scopes\"\\s*:\\s*\\[[\\s\\S]*?(\\])";
                            var registryMatch = System.Text.RegularExpressions.Regex.Match(json, openupmPattern);
                            
                            if (registryMatch.Success)
                            {
                                var hasExistingScopes = registryMatch.Groups[0].Value.Contains("\":");
                                var separator = hasExistingScopes ? ",\n        " : "\n        ";
                                var replacement = registryMatch.Groups[0].Value.Replace(registryMatch.Groups[2].Value, separator + $"\"{scope}\"" + registryMatch.Groups[2].Value);
                                json = json.Replace(registryMatch.Groups[0].Value, replacement);
                                needsUpdate = true;
                            }
                        }
                        
                        // 레지스트리 이름이 "OpenUPM"인 경우 "package.openupm.com"으로 변경
                        if (json.Contains("\"name\": \"OpenUPM\""))
                        {
                            json = json.Replace("\"name\": \"OpenUPM\"", $"\"name\": \"{OpenUpmRegistryName}\"");
                            needsUpdate = true;
                        }
                    }

                    if (!needsUpdate)
                    {
                        return true;
                    }
                    
                    // 백업 파일 생성
                    var backupPath = manifestPath + ".bak";
                    File.Copy(manifestPath, backupPath, true);
                    
                    File.WriteAllText(manifestPath, json);
                    AssetDatabase.Refresh();
                    Debug.Log("[패키지 설치] OpenUPM 레지스트리 구성이 업데이트되었습니다");
                    
                    // 약간의 지연 추가
                    System.Threading.Thread.Sleep(300);
                    
                    return true;
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[패키지 설치] OpenUPM 레지스트리 설정 중 오류: {exception.Message}");
                    return false;
                }
            }
        }
    }
}