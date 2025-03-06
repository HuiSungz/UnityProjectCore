
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectCore.Editor
{
    [Serializable]
    public class PlatformPackageInfo
    {
        public string Name;
        public string Description;
        public PackageType Type;
        public string PackageId;
        public string Url;
        public string AssemblyDefName;
        public bool IsInstalled;
        public string[] DefineSymbols;  // 패키지 설치 시 추가할 심볼 배열
        
        public void CheckInstallationStatus()
        {
            try
            {
                if (string.IsNullOrEmpty(PackageId))
                {
                    Debug.LogWarning($"[{Name}] 패키지 ID가 지정되지 않았습니다.");
                    IsInstalled = false;
                    return;
                }
                
                IsInstalled = PackageValidator.IsInstallValidation(PackageId, AssemblyDefName);

                if (!IsInstalled || DefineSymbols is not { Length: > 0 })
                {
                    return;
                }

                var allSymbolsExist = DefineSymbols.All(SymbolUtility.HasSymbol);
                if (allSymbolsExist)
                {
                    return;
                }
                
                foreach (var symbol in DefineSymbols)
                {
                    SymbolUtility.AddSymbolForTarget(symbol, BuildTargetGroup.Android);
                    SymbolUtility.AddSymbolForTarget(symbol, BuildTargetGroup.iOS);
                }
                
                Debug.Log($"[{Name}] 패키지에 필요한 심볼이 자동으로 추가되었습니다.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[{Name}] 패키지 상태 확인 중 오류 발생: {e.Message}");
                IsInstalled = false;
            }
        }
        
        public void Install(Action<bool> onComplete = null)
        {
            try
            {
                Action<bool> onInstallComplete = (success) => {
                    if (success && DefineSymbols is { Length: > 0 })
                    {
                        foreach (var symbol in DefineSymbols)
                        {
                            SymbolUtility.AddSymbolForTarget(symbol, BuildTargetGroup.Android);
                            SymbolUtility.AddSymbolForTarget(symbol, BuildTargetGroup.iOS);
                        }
                        Debug.Log($"[{Name}] 패키지 설치 완료 및 심볼이 추가되었습니다.");
                    }
                    onComplete?.Invoke(success);
                };
                
                switch (Type)
                {
                    case PackageType.Git:
                        if (string.IsNullOrEmpty(PackageId))
                        {
                            Debug.LogError($"[{Name}] Git 패키지 URL이 지정되지 않았습니다.");
                            onComplete?.Invoke(false);
                            return;
                        }
                        GitPackageInstaller.InstallPackage(PackageId, onInstallComplete);
                        break;
                    case PackageType.OpenUPM:
                        if (string.IsNullOrEmpty(PackageId))
                        {
                            Debug.LogError($"[{Name}] OpenUPM 패키지 ID가 지정되지 않았습니다.");
                            onComplete?.Invoke(false);
                            return;
                        }
                        UpmPackageInstaller.InstallPackage(PackageId, onInstallComplete);
                        break;
                    case PackageType.Web:
                        if (string.IsNullOrEmpty(Url))
                        {
                            Debug.LogError($"[{Name}] 웹 URL이 지정되지 않았습니다.");
                            onComplete?.Invoke(false);
                            return;
                        }
                        Application.OpenURL(Url);
                        onComplete?.Invoke(true);
                        break;
                    default:
                        Debug.LogError($"[{Name}] 알 수 없는 패키지 타입입니다.");
                        onComplete?.Invoke(false);
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[{Name}] 패키지 설치 중 오류 발생: {e.Message}");
                onComplete?.Invoke(false);
            }
        }
    }
}