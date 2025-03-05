
using UnityEditor;
using UnityEngine;

namespace ProjectCore.Editor
{
    [InitializeOnLoad]
    internal static class GUICustomStyles
    {
        #region Fields & Constructor

        public static GUIStyle Box;
        public static GUIStyle BoxHeader;
        public static GUIStyle BoxGroupBackground;
        public static GUIStyle BoxHeaderWithBorder;
        public static GUIStyle BoxContent;
        public static GUIStyle Padding00;

        public static GUIContent MenuContent;

        public static bool _initialized = false;
        private static bool _stylesCreated = false;

        static GUICustomStyles()
        {
            CreateBaseInstances();
            
            EditorApplication.update += CheckAndInitStyles;
        }

        #endregion
        
        private static void CreateBaseInstances()
        {
            if (_stylesCreated)
            {
                return;
            }
            
            Box = new GUIStyle();
            BoxHeader = new GUIStyle();
            BoxGroupBackground = new GUIStyle();
            BoxHeaderWithBorder = new GUIStyle();
            BoxContent = new GUIStyle();
            Padding00 = new GUIStyle();
            
            MenuContent = new GUIContent("+", "Menu");
            
            _stylesCreated = true;
        }
        
        /// <summary>
        /// 에디터가 준비되었는지 확인하고 스타일 초기화 수행
        /// </summary>
        private static void CheckAndInitStyles()
        {
            if (_initialized)
            {
                EditorApplication.update -= CheckAndInitStyles;
                return;
            }

            if (!EditorWindow.focusedWindow)
            {
                return;
            }
            
            EditorApplication.delayCall += () => {
                if (EditorWindow.focusedWindow != null)
                {
                    EditorWindow.focusedWindow.Repaint();
                }
            };

            EditorApplication.update -= CheckAndInitStyles;
        }

        /// <summary>
        /// OnGUI 내에서 호출되어야 하는 스타일 초기화 메서드
        /// </summary>
        public static void InitGUIStyles()
        {
            if (_initialized)
            {
                return;
            }
            
            try
            {
                if (EditorStyles.boldLabel == null || GUI.skin == null) return;
                
                Box = new GUIStyle(GUI.skin.box)
                {
                    margin = new RectOffset(4, 4, 4, 4),
                    padding = new RectOffset(6, 6, 6, 6)
                };

                BoxHeader = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(6, 6, 4, 4)
                };
                
                BoxContent = new GUIStyle
                {
                    padding = new RectOffset(6, 6, 4, 6)
                };
                
                Padding00 = new GUIStyle
                {
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0)
                };

                BoxGroupBackground = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(4, 4, 0, 4),
                    margin = new RectOffset(0, 0, 4, 4),
                    border = new RectOffset(1, 1, 1, 1)
                };
                
                var borderedTexture1 = CreateBorderedTexture(256, 256, 1,
                    Color.black, new Color(0.65f, 0.65f, 0.65f, 0.2f));
                    
                var borderedTexture2 = CreateBorderedTexture(64, 64, 1,
                    Color.black, new Color(0.3f, 0.3f, 0.3f, 0.1f));
                
                BoxHeaderWithBorder = new GUIStyle(BoxHeader) { normal = { background = borderedTexture1 } };
                BoxGroupBackground.normal.background = borderedTexture2;
                
                _initialized = true;
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"EditorCustomStyles 초기화 실패: {exception.Message}");
            }
        }

        private static Texture2D CreateBorderedTexture(int textureWidth, int textureHeight, int borderSize, Color borderColor, Color fillColor)
        {
            var texture = new Texture2D(textureWidth, textureHeight)
            {
                filterMode = FilterMode.Point
            };

            var colors = new Color[textureWidth * textureHeight];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = fillColor;
            }
            
            texture.SetPixels(colors);

            for (var y = 0; y < textureHeight; y++)
            {
                for (var x = 0; x < textureWidth; x++)
                {
                    if (y < borderSize)
                    {
                        texture.SetPixel(x, y, borderColor);
                    }
                    else if (y >= textureHeight - borderSize)
                    {
                        texture.SetPixel(x, y, borderColor);
                    }
                    else if (x < borderSize)
                    {
                        texture.SetPixel(x, y, borderColor);
                    }
                    else if (x >= textureWidth - borderSize)
                    {
                        texture.SetPixel(x, y, borderColor);
                    }
                }
            }
            
            texture.Apply();
            return texture;
        }
    }
}