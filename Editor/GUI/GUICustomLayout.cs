
using UnityEditor;
using UnityEngine;

namespace ProjectCore.Editor
{
    internal static class GUICustomLayout
    {
        private const float HEADER_HEIGHT = 30;

        public static void BeginBoxGroup(string title)
        {
            CreateBoxBase(title);

            bool stylesInitialized = GUICustomStyles._initialized;
            GUIStyle contentStyle = stylesInitialized ? GUICustomStyles.BoxContent : GUIStyle.none;
            EditorGUILayout.BeginVertical(contentStyle);
        }

        public static void BeginMenuBoxGroup(string title, GenericMenu genericMenu)
        {
            Rect headerRect = CreateBoxBase(title);

            bool stylesInitialized = GUICustomStyles._initialized;
            var buttonStyle = EditorStyles.miniButton;
            var buttonContent = stylesInitialized ? GUICustomStyles.MenuContent : new GUIContent("+", "Menu");

            var buttonSize = new Vector2(20, 18);
            var yOffset = (headerRect.height - buttonSize.y) / 2;

            var buttonRect = new Rect(headerRect.x + headerRect.width - 8 - buttonSize.x, headerRect.y + yOffset, buttonSize.x, buttonSize.y);
            if (GUI.Button(buttonRect, buttonContent, buttonStyle))
            {
                genericMenu.ShowAsContext();
            }
            
            GUIStyle contentStyle = stylesInitialized ? GUICustomStyles.BoxContent : GUIStyle.none;
            EditorGUILayout.BeginVertical(contentStyle);
        }

        public static void EndBoxGroup()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private static Rect CreateBoxBase(string title)
        {
            if (!GUICustomStyles._initialized)
            {
                GUICustomStyles.InitGUIStyles();
            }
            
            var stylesInitialized = GUICustomStyles._initialized;
            var bgStyle = stylesInitialized ? GUICustomStyles.BoxGroupBackground : EditorStyles.helpBox;
            var rect = EditorGUILayout.BeginVertical(bgStyle);

            GUILayout.Space(HEADER_HEIGHT);

            var headerRect = new Rect(rect.x, rect.y, rect.width, HEADER_HEIGHT);

            GUI.Box(headerRect, GUIContent.none, stylesInitialized 
                ? GUICustomStyles.BoxHeaderWithBorder 
                : EditorStyles.toolbar);

            GUIStyle headerStyle = stylesInitialized ? GUICustomStyles.BoxHeader : EditorStyles.boldLabel;
            EditorGUI.LabelField(headerRect, "  " + title, headerStyle);

            return headerRect;
        }
    }
}