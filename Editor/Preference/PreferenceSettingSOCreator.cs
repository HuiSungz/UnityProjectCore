
using System.IO;
using ProjectCore.Preference;
using UnityEditor;
using UnityEngine;

namespace ProjectCore.Editor
{
    public static class PreferenceSettingSOCreator
    {
        private const string MENU_PATH = GlobalAccess.MenuItemCreateMenuRoot + "PreferenceSetting";
        
        [MenuItem(MENU_PATH, priority = 1)]
        public static void CreatePreferenceSetting()
        {
            var asset = ScriptableObject.CreateInstance<PreferenceSettingSO>();
            var path = GlobalAccess.MainAssetPath + "PreferenceSettings.asset";

            if (File.Exists(path))
            {
                Debug.LogWarning("SaveSettings.asset already exists! Delete it first if you want to create a new one.");
                return;
            }

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}