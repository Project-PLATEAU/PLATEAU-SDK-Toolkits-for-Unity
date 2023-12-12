using PlateauToolkit.Editor;
using System;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Rendering.Editor
{
    public class PlateauToolkitRenderingGUILayout : MonoBehaviour
    {
        public struct GridInfo
        {
            public float margin;
            public int columns;
            public int rows;
        }

        public static GridInfo CalculateGridLayout(float width, float cellWidth, int cellCount)
        {
            var columns = Mathf.Min((int)(width / cellWidth), cellCount);
            var rows = Mathf.CeilToInt((float)cellCount / columns);
            var margin = width - cellWidth * columns - (cellCount - 1) * 3;

            return new GridInfo
            {
                margin = margin,
                columns = columns,
                rows = rows
            };
        }

        public static void BorderLine()
        {
            Rect borderRect = EditorGUILayout.GetControlRect(false, 1, PlateauToolkitGUIStyles.BorderStyle);
            EditorGUI.DrawRect(borderRect, PlateauToolkitGUIStyles.k_LineColor);
        }

        public static void Header(string label)
        {
            EditorGUILayout.Space(8);

            BorderLine();

            using (var scope = new EditorGUILayout.VerticalScope(PlateauToolkitGUIStyles.HeaderBoxStyle, GUILayout.Height(24)))
            {
                EditorGUI.DrawRect(scope.rect, PlateauToolkitGUIStyles.k_HeaderBackgroundColor);

                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.HorizontalScope(PlateauToolkitGUIStyles.HeaderContentStyle))
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(label, GUILayout.ExpandWidth(false));
                    GUILayout.FlexibleSpace();
                }
                GUILayout.FlexibleSpace();
            }

            BorderLine();

            EditorGUILayout.Space(8);
        }

        public static void GridLayout(float width, float cellWidth, Action[] cellGuis)
        {
            var gridInfo = CalculateGridLayout(width, cellWidth, cellGuis.Length);
            for (var row = 0; row < gridInfo.rows; row++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(gridInfo.margin / 2);

                for (var column = 0; column < gridInfo.columns; column++)
                {
                    var index = gridInfo.columns * row + column;
                    if (index >= cellGuis.Length)
                    {
                        break;
                    }

                    cellGuis[index].Invoke();
                }

                GUILayout.EndHorizontal();
            }
        }

        internal readonly struct PlateauRenderingEditorImageButtonGUILayout
        {
            readonly float width;
            readonly float height;

            public PlateauRenderingEditorImageButtonGUILayout(float width, float height)
            {
                this.width = width;
                this.height = height;
            }

            public bool Button(string iconTexturePath, Color? buttonColor = null)
            {
                var defaultColor = GUI.backgroundColor;
                GUI.backgroundColor = buttonColor.GetValueOrDefault(defaultColor);
                var button = GUILayout.Button(
                    (Texture2D)AssetDatabase.LoadAssetAtPath(iconTexturePath, typeof(Texture2D)),
                    GUILayout.Width(width),
                    GUILayout.Height(height));
                GUI.backgroundColor = defaultColor;

                return button;
            }
        }
    }

}