
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectCore.PlatformService
{
    public static class PlatformServiceObjectSOCreator
    {
        #region Constants
        
        private const string BASE_MENU_PATH1 = "ActionFit/Platform Objects";
        private const string BASE_MENU_PATH2 = "Assets/ActionFit Create/Platform Objects";
        private const string BASE_ASSET_PATH = "Assets/ProjectSettings/Platform";
        
        #endregion

#if SDK_INSTALLED_FIREBASE
        #region Firebase
        
        [MenuItem(BASE_MENU_PATH1 + "/Firebase")]
        private static void CreateFirebaseSOFromMainMenu()
        {
            CreateServiceSO<PlatformFirebaseSO>("PlatformFirebaseSO", "Firebase");
        }
        
        [MenuItem(BASE_MENU_PATH2 + "/Firebase", priority = -90)]
        private static void CreateFirebaseSOFromContextMenu()
        {
            CreateServiceSO<PlatformFirebaseSO>("PlatformFirebaseSO", "Firebase");
        }
        
        #endregion
#endif
        
#if SDK_INSTALLED_GAMEANALYTICS
        #region GameAnalytics
        
        [MenuItem(BASE_MENU_PATH1 + "/GameAnalytics")]
        private static void CreateGameAnalyticsSOFromMainMenu()
        {
            CreateServiceSO<PlatformGameAnalyticsSO>("PlatformGameAnalyticsSO", "GameAnalytics");
        }
        
        [MenuItem(BASE_MENU_PATH2 + "/GameAnalytics", priority = -91)]
        private static void CreateGameAnalyticsSOFromContextMenu()
        {
            CreateServiceSO<PlatformGameAnalyticsSO>("PlatformGameAnalyticsSO", "GameAnalytics");
        }
        
        #endregion
#endif
        
#if SDK_INSTALLED_APPLOVINMAX
        #region AppLovin MAX
        
        [MenuItem(BASE_MENU_PATH1 + "/AppLovin MAX")]
        private static void CreateMaxSOFromMainMenu()
        {
            CreateServiceSO<PlatformMaxSO>("PlatformMaxSO", "AppLovin MAX");
        }
        
        [MenuItem(BASE_MENU_PATH2 + "/AppLovin MAX", priority = -92)]
        private static void CreateMaxSOFromContextMenu()
        {
            CreateServiceSO<PlatformMaxSO>("PlatformMaxSO", "AppLovin MAX");
        }
        
        #endregion
#endif
        
#if SDK_INSTALLED_SINGULAR
        #region Singular
        
        [MenuItem(BASE_MENU_PATH1 + "/Singular")]
        private static void CreateSingularSOFromMainMenu()
        {
            CreateServiceSO<PlatformSingularSO>("PlatformSingularSO", "Singular");
        }
        
        [MenuItem(BASE_MENU_PATH2 + "/Singular", priority = -93)]
        private static void CreateSingularSOFromContextMenu()
        {
            CreateServiceSO<PlatformSingularSO>("PlatformSingularSO", "Singular");
        }
        
        #endregion
#endif
        
        #region Helper Methods
        
        /// <summary>
        /// 지정된 타입의 플랫폼 서비스 SO를 생성합니다.
        /// </summary>
        /// <typeparam name="T">생성할 ScriptableObject 타입</typeparam>
        /// <param name="fileName">파일 이름</param>
        /// <param name="serviceName">서비스 이름</param>
        private static void CreateServiceSO<T>(string fileName, string serviceName) where T : ScriptableObject
        {
            string assetPath = $"{BASE_ASSET_PATH}/{fileName}.asset";
            
            // 기존 에셋이 있는지 확인
            var existingAsset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (existingAsset)
            {
                Debug.Log($"{serviceName} SO가 이미 존재합니다: {assetPath}");
                Selection.activeObject = existingAsset;
                return;
            }
            
            var directory = System.IO.Path.GetDirectoryName(assetPath);
            if (!System.IO.Directory.Exists(directory))
            {
                if (directory != null)
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
            }
            
            var newSO = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(newSO, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"{serviceName} SO가 생성되었습니다: {assetPath}");
            Selection.activeObject = newSO;
        }
        
        #endregion
    }
}
#endif