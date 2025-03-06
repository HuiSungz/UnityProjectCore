using System;
using System.Collections.Generic;

namespace ProjectCore.Editor
{
    public class PlatformInstallerViewModel
    {
        public List<PlatformPackageInfo> Packages { get; private set; }
        public event Action OnPackagesChanged;
        
        public PlatformInstallerViewModel()
        {
            InitializePackages();
        }
        
        private void InitializePackages()
        {
            Packages = new List<PlatformPackageInfo>
            {
                // External Dependency Manager - 심볼 필요 없음
                new PlatformPackageInfo
                {
                    Name = "External Dependency Manager",
                    Description = "Unity용 외부 종속성 관리자",
                    Type = PackageType.OpenUPM,
                    PackageId = "com.google.external-dependency-manager",
                    AssemblyDefName = "Google.ExternalDependencyManager"
                },
                
                // AppLovin (웹 링크)
                new PlatformPackageInfo
                {
                    Name = "AppLovin MAX",
                    Description = "모바일 광고 미디에이션 플랫폼",
                    Type = PackageType.Web,
                    Url = "https://developers.applovin.com/en/max/unity/overview/integration/",
                    PackageId = "com.applovin.mediation.ads",
                    AssemblyDefName = "MaxSdk",
                    DefineSymbols = new[] { GlobalAccess.SYMBOL_INSTALLED_SDK_MAX }
                },
                
                // GameAnalytics
                new PlatformPackageInfo
                {
                    Name = "GameAnalytics",
                    Description = "게임 분석 SDK",
                    Type = PackageType.OpenUPM,
                    PackageId = "com.gameanalytics.sdk",
                    AssemblyDefName = "GameAnalytics",
                    DefineSymbols = new[] { GlobalAccess.SYMBOL_INSTALLED_SDK_GAN }
                },
                
                // Admob
                new PlatformPackageInfo
                {
                    Name = "AdMob",
                    Description = "모바일 광고 미디에이션 플랫폼",
                    Type = PackageType.OpenUPM,
                    PackageId = "com.google.ads.mobile",
                    AssemblyDefName = "GoogleMobileAds",
                    DefineSymbols = new[] { GlobalAccess.SYMBOL_INSTALLED_SDK_ADMOB }
                },
                
                // Singular
                new PlatformPackageInfo
                {
                    Name = "Singular",
                    Description = "마케팅 분석 SDK",
                    Type = PackageType.Git,
                    PackageId = "https://github.com/singular-labs/Singular-Unity-SDK.git",
                    AssemblyDefName = "Singular",
                    DefineSymbols = new[] { GlobalAccess.SYMBOL_INSTALLED_SDK_SINGULAR }
                },
                
                // Firebase Core - FB 심볼은 여기에만 추가
                new PlatformPackageInfo
                {
                    Name = "Firebase Core",
                    Description = "Firebase 핵심 SDK",
                    Type = PackageType.Git,
                    PackageId = "https://github.com/GameWorkstore/com.google.firebase.app.git",
                    AssemblyDefName = "Firebase.App",
                    DefineSymbols = new[] { GlobalAccess.SYMBOL_INSTALLED_SDK_FB }
                },
                
                // Firebase Analytics - 심볼 없음
                new PlatformPackageInfo
                {
                    Name = "Firebase Analytics",
                    Description = "Firebase 분석 SDK",
                    Type = PackageType.Git,
                    PackageId = "https://github.com/GameWorkstore/com.google.firebase.analytics.git",
                    AssemblyDefName = "Firebase.Analytics"
                },
                
                // Firebase Crashlytics - 심볼 없음
                new PlatformPackageInfo
                {
                    Name = "Firebase Crashlytics",
                    Description = "Firebase 충돌 분석 SDK",
                    Type = PackageType.Git,
                    PackageId = "https://github.com/GameWorkstore/com.google.firebase.crashlytics.git",
                    AssemblyDefName = "Firebase.Crashlytics"
                },
                
                // Firebase RemoteConfig - 심볼 없음
                new PlatformPackageInfo
                {
                    Name = "Firebase RemoteConfig",
                    Description = "Firebase 원격 구성 SDK",
                    Type = PackageType.Git,
                    PackageId = "https://github.com/GameWorkstore/com.google.firebase.remote-config.git",
                    AssemblyDefName = "Firebase.RemoteConfig"
                }
            };
            
            RefreshPackageStatus();
        }
        
        public void RefreshPackageStatus()
        {
            foreach (var package in Packages)
            {
                package.CheckInstallationStatus();
            }
            
            OnPackagesChanged?.Invoke();
        }
        
        public void InstallPackage(int index, Action<bool> onComplete = null)
        {
            if (index < 0 || index >= Packages.Count)
            {
                onComplete?.Invoke(false);
                return;
            }
            
            Packages[index].Install(success =>
            {
                // 설치가 완료되면 해당 패키지의 상태만 업데이트
                Packages[index].CheckInstallationStatus();
                OnPackagesChanged?.Invoke();
                onComplete?.Invoke(success);
            });
        }
    }
}