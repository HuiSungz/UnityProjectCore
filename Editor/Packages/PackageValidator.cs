
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ProjectCore.Editor
{
    public static class PackageValidator
    {
        public static bool IsInstallValidation(string packageName, string assemblyDefName = null)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                Debug.LogWarning("패키지 이름이 비어 있습니다.");
                return false;
            }

            try
            {
                // 1. 패키지 매니페스트에서 확인
                var isFoundInManifest = CheckManifestFile(packageName);
                if (isFoundInManifest)
                    return true;

                // 2. Assembly Definition 이름으로 확인
                if (!string.IsNullOrEmpty(assemblyDefName))
                {
                    bool isFoundInAssemblies = CheckAssemblyByName(assemblyDefName);
                    if (isFoundInAssemblies)
                        return true;
                }

                // 3. 패키지 폴더 존재 여부 확인
                var isFoundPackageFolder = CheckPackageFolder(packageName);
                if (isFoundPackageFolder)
                {
                    return true;
                }

                // 4. 특정 타입이나 네임스페이스 확인
                var isFoundNamespace = CheckNamespace(packageName);
                
                return isFoundNamespace;
            }
            catch (Exception e)
            {
                Debug.LogError($"패키지 유효성 검사 중 오류 발생: {e.Message}");
                return false;
            }
        }

        #region Internal Methods

        /// <summary>
        /// Check if the package is found in the manifest file.
        /// </summary>
        private static bool CheckManifestFile(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return false;
            }
            
            var manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            if (!File.Exists(manifestPath))
            {
                Debug.LogWarning("Cannot find the manifest file.");
                return false;
            }
            
            var manifestContent = File.ReadAllText(manifestPath);
            
            // GitHub URL 형식으로 검색 ("github.com/사용자/패키지명.git")
            if (manifestContent.Contains(packageName))
            {
                return true;
            }
                
            // 패키지 이름만으로 검색 (슬래시로 분리하여 마지막 부분)
            string simpleName = packageName;
            
            if (packageName.Contains("/"))
            {
                var parts = packageName.Split('/');
                if (parts.Length > 0)
                {
                    simpleName = parts[parts.Length - 1];
                }
            }
            
            if (!string.IsNullOrEmpty(simpleName) && simpleName.EndsWith(".git"))
            {
                simpleName = simpleName.Replace(".git", "");
            }
            
            if (string.IsNullOrEmpty(simpleName))
            {
                return false;
            }
            
            return manifestContent.Contains($"\"{simpleName}\"");
        }
        
        /// <summary>
        /// Check if the assembly definition is found by the name.
        /// </summary>
        private static bool CheckAssemblyByName(string assemblyDefName)
        {
            if (string.IsNullOrEmpty(assemblyDefName))
            {
                return false;
            }
                
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return assemblies.Any(assembly => 
                assembly != null && 
                assembly.GetName() != null && 
                assembly.GetName().Name != null && 
                assembly.GetName().Name.Contains(assemblyDefName));
        }
        
        /// <summary>
        /// Check if the package folder is found in the Packages directory.
        /// </summary>
        private static bool CheckPackageFolder(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return false;
            }
                
            string simpleName = packageName;
            
            if (packageName.Contains("/"))
            {
                var parts = packageName.Split('/');
                if (parts.Length > 0)
                {
                    simpleName = parts[parts.Length - 1];
                }
            }
            
            if (!string.IsNullOrEmpty(simpleName) && simpleName.EndsWith(".git"))
            {
                simpleName = simpleName.Replace(".git", "");
            }
            
            if (string.IsNullOrEmpty(simpleName))
            {
                return false;
            }
            
            var packagesPath = Path.Combine(Application.dataPath, "..", "Packages");
            if (Directory.Exists(packagesPath))
            {
                // 패키지 폴더가 직접 있는 경우
                if (Directory.Exists(Path.Combine(packagesPath, simpleName)))
                {
                    return true;
                }
                
                // package-lock.json에서 확인
                var packageLockPath = Path.Combine(packagesPath, "package-lock.json");
                if (File.Exists(packageLockPath))
                {
                    var packageLockContent = File.ReadAllText(packageLockPath);
                    if (packageLockContent.Contains(simpleName) || 
                        packageLockContent.Contains(packageName))
                    {
                        return true;
                    }
                }
            }
            
            // Assets 폴더 내 패키지가 임포트된 경우 - Deprecated
            var potentialAssetsPath = Path.Combine(Application.dataPath, simpleName);
            return Directory.Exists(potentialAssetsPath);
        }
        
        /// <summary>
        /// Check if the namespace or specific type is found in the assemblies.
        /// </summary>
        private static bool CheckNamespace(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return false;
            }
                
            // 패키지 URL에서 패키지 이름만 추출하여 가능한 네임스페이스 유추
            string simpleName = packageName;
            
            if (packageName.Contains("/"))
            {
                var parts = packageName.Split('/');
                if (parts.Length > 0)
                {
                    simpleName = parts[^1];
                }
            }
            
            if (!string.IsNullOrEmpty(simpleName) && simpleName.EndsWith(".git"))
            {
                simpleName = simpleName.Replace(".git", "");
            }
            
            if (string.IsNullOrEmpty(simpleName))
            {
                return false;
            }
            
            var potentialNamespace = simpleName.Replace("-", ".");
            
            // 하이픈을 제거한 버전도 시도
            var altNamespace = simpleName.Replace("-", "");
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    if (assembly == null)
                    {
                        continue;
                    }
                    
                    // 가능한 네임스페이스 패턴으로 확인
                    var types = assembly.GetTypes();
                    if (types.Select(type => type.Namespace)
                             .Where(ns => !string.IsNullOrEmpty(ns))
                             .Any(ns => ns.Contains(potentialNamespace) || 
                                        ns.Contains(altNamespace) || 
                                        ns.Contains(simpleName.Replace("-", ""))))
                    {
                        return true;
                    }
                }
                catch
                {
                    // Ignored
                    continue;
                }
            }
            
            return false;
        }

        #endregion
    }
}