using System;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Editor
{
    public static class GuiUtility
    {
        public enum KColor
        {
            BtnColor,
            Separator,
        }

        private const int k_LightMode = 0;

        public static Color GetColor(KColor color)
        {
            return color switch
            {
                KColor.BtnColor => EditorPrefs.GetInt("UserSkin") == k_LightMode ? new Color(0, 166f/255f, 166f/255f) : Color.cyan,
                KColor.Separator => EditorPrefs.GetInt("UserSkin") == k_LightMode ? Color.black : Color.white,
                _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
            };
        }

        public static void Separator(Color color, int height=1, bool useIndentLevel=false)
        {
            EditorGUILayout.BeginHorizontal();

            if (useIndentLevel)
            {
                GUILayout.Space(EditorGUI.indentLevel * 15);
            }

            var horizontalLine = new GUIStyle
            {
                normal =
                {
                    background = EditorGUIUtility.whiteTexture
                },
                margin = new RectOffset(0, 0, 4, 4), fixedHeight = height
            };

            Color c = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, horizontalLine);
            GUI.color = c;

            EditorGUILayout.EndHorizontal();
        }
    }
}