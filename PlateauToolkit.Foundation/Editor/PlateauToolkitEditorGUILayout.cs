using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Editor
{
    /// <summary>
    /// GUI Layout tools for PLATEAU Toolkit
    /// </summary>
    public static class PlateauToolkitEditorGUILayout
    {
        /// <summary>
        /// Show GUIs with a grid layout.
        /// </summary>
        public static void GridLayout(float width, float cellWidth, float cellHeight, IEnumerable<Action> cellGuis)
        {
            Action[] cellGuiArray = cellGuis.ToArray();
            int cellsPerRow = Mathf.Max(1, Mathf.Min((int)(width / cellWidth), cellGuiArray.Length));
            int cellCount = cellGuiArray.Length % cellsPerRow == 0 ? cellGuiArray.Length : cellGuiArray.Length + (cellsPerRow - cellGuiArray.Length % cellsPerRow);

            bool isHorizontalGroupOpen = false;

            GUILayout.BeginVertical();
            {
                for (int i = 0; i < cellCount; i++)
                {
                    if (i % cellsPerRow == 0)
                    {
                        // If a horizontal group is open, end it before starting a new one.
                        if (isHorizontalGroupOpen)
                        {
                            GUILayout.FlexibleSpace(); // Center align
                            GUILayout.EndHorizontal();
                        }

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace(); // Center align
                        isHorizontalGroupOpen = true;
                    }

                    // Begin a GUI group for each cell, to isolate their layouts from each other.
                    GUILayout.BeginVertical(GUILayout.Width(cellWidth), GUILayout.Height(cellHeight));
                    if (i < cellGuiArray.Length)
                    {
                        cellGuiArray[i]();
                    }
                    else
                    {
                        GUILayout.Label("");
                    }
                    GUILayout.EndVertical();
                }

                // If a horizontal group is open at the end of the loop, end it.
                if (isHorizontalGroupOpen)
                {
                    // Add empty cells to fill the row if necessary
                    for (int j = cellGuiArray.Length; j % cellsPerRow != 0; j++)
                    {
                        GUILayout.BeginVertical(GUILayout.Width(cellWidth), GUILayout.Height(cellHeight));
                        GUILayout.EndVertical();
                    }

                    GUILayout.FlexibleSpace(); // Center align
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }

        public static IDisposable BackgroundColorScope(Color color)
        {
            Color defaultColor = GUI.backgroundColor;
            GUI.backgroundColor = color;

            return new CallbackDisposable(
                () =>
                {
                    GUI.backgroundColor = defaultColor;
                });
        }

        public static void HeaderLogo(float windowWidth)
        {
            EditorGUILayout.Space(25f);

            var logoTexture =(Texture2D)AssetDatabase.LoadAssetAtPath(
                    PlateauToolkitPaths.PlateauLogo, typeof(Texture2D));
            float width = Mathf.Min(windowWidth - 20, 170f);
            float height = (float)logoTexture.height / logoTexture.width * width;

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                Rect logoRect = EditorGUILayout.GetControlRect(GUILayout.Width(width), GUILayout.Height(height));
                GUI.DrawTexture(logoRect, logoTexture);
                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.Space(15f);
        }

        public static void BorderLine()
        {
            Rect borderRect = EditorGUILayout.GetControlRect(false, 1, PlateauToolkitGUIStyles.BorderStyle);
            EditorGUI.DrawRect(borderRect, PlateauToolkitGUIStyles.k_LineColor);
        }

        public static void BorderLine(Color color, float height = 1f)
        {
            Rect borderRect = EditorGUILayout.GetControlRect(false, height, PlateauToolkitGUIStyles.BorderStyle);
            EditorGUI.DrawRect(borderRect, color);
        }

        public static void Title(float windowWidth, string label)
        {
            EditorGUILayout.Space(15);

            float imageWidth = 40f;
            float imageHeight = 1f;
            float imageOffset = 3f;
            float textWidth = 120f;
            float textHeight = 17f;

            Texture2D titleImage = AssetDatabase.LoadAssetAtPath<Texture2D>(PlateauToolkitPaths.PlateauTitleBackground);
            Rect imageRect = EditorGUILayout.GetControlRect(GUILayout.Width(imageWidth), GUILayout.Height(imageHeight));
            imageRect.y += (textHeight / 2) + imageOffset;

            using (new EditorGUILayout.HorizontalScope(GUILayout.Height(textHeight)))
            {
                GUILayout.FlexibleSpace();

                imageRect.x = (windowWidth / 2) - (textWidth / 2) - imageWidth;
                GUI.DrawTexture(imageRect, titleImage);

                EditorGUILayout.LabelField(label,
                    PlateauToolkitGUIStyles.TitleTextStyle,
                    GUILayout.Height(textHeight),
                    GUILayout.Width(textWidth));

                imageRect.x = (windowWidth / 2) + (textWidth / 2);

                // Draw the image upside down
                GUIUtility.RotateAroundPivot (180f, imageRect.center);
                GUI.DrawTexture(imageRect, titleImage);
                GUI.matrix = Matrix4x4.identity;

                GUILayout.FlexibleSpace();
            }
        }

        public static void Header(string label, int topMargin = 15, int bottomMargin = 15)
        {
            EditorGUILayout.Space(topMargin);

            BorderLine();
            using (var scope = new EditorGUILayout.VerticalScope(PlateauToolkitGUIStyles.HeaderBoxStyle, GUILayout.Height(28)))
            {
                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.HorizontalScope(PlateauToolkitGUIStyles.HeaderContentStyle))
                {
                    EditorGUI.DrawRect(scope.rect, PlateauToolkitGUIStyles.k_HeaderBackgroundColor);
                    GUILayout.Label(label, PlateauToolkitGUIStyles.HeaderTextStyle,  GUILayout.ExpandWidth(false));
                    GUILayout.FlexibleSpace();
                }
                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.Space(bottomMargin);
        }

        public static EditorGUILayout.HorizontalScope TabScope(float width)
        {
            var scope = new EditorGUILayout.HorizontalScope(PlateauToolkitGUIStyles.TabBoxStyle,
                GUILayout.Height(64));
            GUILayoutUtility.GetRect(width, 64);
            return scope;
        }

        public static EditorGUILayout.VerticalScope FooterScope()
        {
            var scope = new EditorGUILayout.VerticalScope( GUILayout.Height(24));
            return scope;
        }
    }

    public static class PlateauToolkitGUIStyles
    {
        public static readonly Color k_LineColor = new Color(33 / 255f, 33 / 255f, 33 / 255f, 1);
        public static readonly Color k_HeaderBackgroundColor = new Color(62 / 255f, 62 / 255f, 62 / 255f, 1);
        public static readonly Color k_FooterBackgroundColor = new Color(51 / 255f, 51 / 255f, 51 / 255f, 1);
        public static readonly Color k_TabBackgroundColor = new Color(0, 0, 0, 0.5f);
        public static readonly Color k_TabActiveColor = new Color(88 / 255f, 88 / 255f, 88 / 255f, 1);
        public static readonly Color k_ButtonNormalColor = new Color(103 / 255f, 103 / 255f, 103 / 255f, 1);
        public static readonly Color k_ButtonPrimaryColor = new Color(0, 88 / 255f, 88 / 255f, 1);
        public static readonly Color k_ButtonDisableColor = new Color(0, 88 / 255f, 88 / 255f, 0.25f);
        public static readonly Color k_ButtonCancelColor = new Color(183 / 255f, 0, 0, 0.25f);

        public static GUIStyle BorderStyle { get; }
        public static GUIStyle HeaderBoxStyle { get; }
        public static GUIStyle HeaderContentStyle { get; }
        public static GUIStyle FooterBoxStyle { get; }
        public static GUIStyle TabBoxStyle { get; }
        public static GUIStyle ButtonStyle { get; }
        public static GUIStyle TitleTextStyle { get; }
        public static GUIStyle HeaderTextStyle { get; }

        static PlateauToolkitGUIStyles()
        {
            BorderStyle = new GUIStyle(GUIStyle.none);
            BorderStyle.normal.textColor = Color.white;
            BorderStyle.alignment = TextAnchor.MiddleCenter;
            BorderStyle.margin = new RectOffset(0, 0, 0, 0);
            BorderStyle.padding = new RectOffset(0, 0, 0, 0);

            FooterBoxStyle = new GUIStyle(GUI.skin.box);
            FooterBoxStyle.normal.textColor = Color.white;
            FooterBoxStyle.alignment = TextAnchor.MiddleCenter;
            FooterBoxStyle.margin = new RectOffset(0, 0, 0, 0);
            FooterBoxStyle.padding = new RectOffset(0, 0, 0, 0);

            HeaderBoxStyle = new GUIStyle(FooterBoxStyle);

            HeaderContentStyle = new GUIStyle(GUI.skin.box);
            HeaderContentStyle.normal.textColor = Color.white;
            HeaderContentStyle.alignment = TextAnchor.MiddleLeft;
            HeaderContentStyle.margin = new RectOffset(0, 0, 0, 0);
            HeaderContentStyle.padding = new RectOffset(15, 15, 10, 10);

            TabBoxStyle = new GUIStyle( GUIStyle.none);
            TabBoxStyle.normal.textColor = Color.white;
            TabBoxStyle.alignment = TextAnchor.MiddleCenter;
            TabBoxStyle.margin = new RectOffset(15, 15, 0, 0);
            TabBoxStyle.padding = new RectOffset(20, 20, 0, 0);

            ButtonStyle = new GUIStyle(GUIStyle.none)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white}
            };

            TitleTextStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            HeaderTextStyle = new GUIStyle()
            {
                fontSize = 12,
                normal = { textColor = Color.white }
            };
        }
    }

    public readonly struct PlateauToolkitImageButtonGUI
    {
        static Texture2D s_WhiteTexture;
        static Texture2D WhiteTexture
        {
            get
            {
                if (s_WhiteTexture == null)
                {
                    Color[] pixels = { Color.white };
                    var texture = new Texture2D(1, 1);
                    texture.SetPixels(pixels);
                    texture.Apply();

                    s_WhiteTexture = texture;
                }
                return s_WhiteTexture;
            }
        }

        readonly float m_Width;
        readonly float m_Height;
        readonly Color m_Color;
        readonly bool m_IsPositionCenter;

        public PlateauToolkitImageButtonGUI(float width, float height, Color color, bool isPositionCenter = true)
        {
            m_Width = width;
            m_Height = height;
            m_Color = color;
            m_IsPositionCenter = isPositionCenter;
        }

        public bool TabButton(
            string iconTexturePath,
            Rect rect,
            bool isActive)
        {
            if (isActive)
            {
                GUI.DrawTexture(rect, WhiteTexture, ScaleMode.StretchToFill, true, 0, m_Color, 0, 4);
            }

            var buttonStyle = new GUIStyle(GUIStyle.none);
            bool button = GUI.Button(
                rect,
                (Texture2D)AssetDatabase.LoadAssetAtPath(iconTexturePath, typeof(Texture2D)),
                buttonStyle);

            return button;
        }

        public bool TabButton(string label)
        {
            float scopeWidth = m_IsPositionCenter ? 0 : m_Width;
            using (var scope = new EditorGUILayout.HorizontalScope(GUILayout.Height(m_Height), GUILayout.Width(scopeWidth)))
            {
                GUILayout.FlexibleSpace();

                float centerY = (scope.rect.height - m_Height) / 2;
                float centerX = (scope.rect.width - m_Width) / 2;
                var buttonRect = new Rect(scope.rect.x + centerX, scope.rect.y + centerY, m_Width, m_Height);

                GUI.DrawTexture(buttonRect, WhiteTexture, ScaleMode.StretchToFill, true, 0, m_Color, new Vector4(), new Vector4(6f, 6f, 0f, 0f));

                bool button = GUILayout.Button(
                    label,
                    PlateauToolkitGUIStyles.ButtonStyle,
                    GUILayout.Height(m_Height),
                    GUILayout.Width(m_Width));

                GUILayout.FlexibleSpace();
                return button;
            }
        }

        public bool Button(string label)
        {
            float scopeWidth = m_IsPositionCenter ? 0 : m_Width;
            using (var scope = new EditorGUILayout.HorizontalScope(GUILayout.Height(m_Height), GUILayout.Width(scopeWidth)))
            {
                GUILayout.FlexibleSpace();

                float centerY = (scope.rect.height - m_Height) / 2;
                float centerX = (scope.rect.width - m_Width) / 2;
                var buttonRect = new Rect(scope.rect.x + centerX, scope.rect.y + centerY, m_Width, m_Height);

                GUI.DrawTexture(buttonRect, WhiteTexture, ScaleMode.StretchToFill, true, 0, m_Color, 0, 5);

                bool button = GUILayout.Button(
                    label,
                    PlateauToolkitGUIStyles.ButtonStyle,
                    GUILayout.Height(m_Height),
                    GUILayout.Width(m_Width));

                GUILayout.FlexibleSpace();
                return button;
            }
        }

        public bool Button(Texture2D texture2D, RectOffset padding)
        {
            using (var scope = new EditorGUILayout.HorizontalScope(GUILayout.Height(m_Height)))
            {
                GUILayout.FlexibleSpace();

                float centerY = (scope.rect.height - m_Height) / 2;
                float centerX = (scope.rect.width - m_Width) / 2;
                var buttonRect = new Rect(scope.rect.x + centerX, scope.rect.y + centerY, m_Width, m_Height);

                GUI.DrawTexture(buttonRect, WhiteTexture, ScaleMode.StretchToFill, true, 0, m_Color, 0, 5);

                var style = PlateauToolkitGUIStyles.ButtonStyle;
                style.padding = padding;

                bool button = GUILayout.Button(
                    texture2D,
                    style,
                    GUILayout.Height(m_Height),
                    GUILayout.Width(m_Width));

                GUILayout.FlexibleSpace();
                return button;
            }
        }
    }
}