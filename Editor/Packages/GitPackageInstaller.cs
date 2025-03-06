
using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ProjectCore.Editor
{
    public static class GitPackageInstaller
    {
        private static AddRequest _addRequest;
        private static Action<bool> _onComplete;
        
        /// <summary>
        /// Install a package from a git URL.
        /// </summary>
        public static void InstallPackage(string packageUrl, Action<bool> onComplete = null)
        {
            _onComplete = onComplete;
            
            // 이미 설치되어 있는지 확인
            if (PackageValidator.IsInstallValidation(packageUrl))
            {
                Debug.Log($"패키지가 이미 설치되어 있습니다: {packageUrl}");
                _onComplete?.Invoke(true);
                return;
            }
            
            // 패키지 설치 시작
            Debug.Log($"패키지 설치 시작: {packageUrl}");
            _addRequest = Client.Add(packageUrl);
            EditorApplication.update += OnPackageInstallProgress;
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
                    Debug.LogError($"패키지 설치 실패: {_addRequest.Error.message}");
                    _onComplete?.Invoke(false);
                    break;
            }
            
            _addRequest = null;
        }
        
        /// <summary>
        /// Wait for the asset import to complete.
        /// </summary>
        public static async Task WaitForAssetImport()
        {
            await Task.Delay(1000);
            AssetDatabase.Refresh();
            await Task.Delay(2000);
        }
    }
}