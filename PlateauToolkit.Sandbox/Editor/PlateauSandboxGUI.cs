using PlateauToolkit.Editor;
using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PlateauToolkit.Sandbox.Editor
{
    static class PlateauSandboxGUI
    {
        const int k_AssetButtonSelectedBorderSize = 3;
        const int k_AssetButtonSize = 75;
        const int k_AssetButtonBottomMargin = 3;

        static readonly Color k_AssetButtonNormal = new(0, 0, 0, 0);
        static readonly Color k_AssetButtonSelected = new(0.4f, 0.7f, 0.9f, 1f);

        static GUIStyle s_LoadingLabelStyle;

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

        /// <summary>
        /// Draw single colored texture to a rect.
        /// </summary>
        /// <remarks>
        /// If you don't need <see cref="borderRadius" />, use <see cref="EditorGUI.DrawRect" />.
        /// </remarks>
        public static void DrawColorTexture(Rect rect, Color color, float borderRadius)
        {
            GUI.DrawTexture(rect, WhiteTexture, ScaleMode.StretchToFill, true, 0, color, 0, borderRadius);
        }

        public static void PlacementToolButton(PlateauSandboxContext context)
        {
            if (ToolManager.activeToolType != typeof(PlateauSandboxPlacementTool))
            {
                if (GUILayout.Button("配置ツールを起動"))
                {
                    ToolManager.SetActiveTool<PlateauSandboxPlacementTool>();
                }
            }
            else
            {
                using (PlateauToolkitEditorGUILayout.BackgroundColorScope(Color.red))
                {
                    if (GUILayout.Button("配置ツールを終了"))
                    {
                        ToolManager.RestorePreviousPersistentTool();
                    }
                }

                if (context.IsSelectedObject(null))
                {
                    EditorGUILayout.HelpBox("リストから配置するオブジェクトを選択してください", MessageType.Error);
                }
                else
                {
                    EditorGUILayout.HelpBox("配置モードを実行中", MessageType.Info);
                }
            }
        }

        public static void AssetButton(GameObject asset, bool isSelected = false, Action onClick = null, bool isDragEnabled = false)
        {
            using (var scope = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                Rect buttonRect = EditorGUILayout.GetControlRect(GUILayout.Width(k_AssetButtonSize), GUILayout.Height(k_AssetButtonSize));
                EditorGUI.DrawRect(buttonRect, isSelected ? k_AssetButtonSelected : k_AssetButtonNormal);

                var textureRect = new Rect(
                    buttonRect.x + k_AssetButtonSelectedBorderSize,
                    buttonRect.y + k_AssetButtonSelectedBorderSize,
                    buttonRect.width - k_AssetButtonSelectedBorderSize * 2,
                    buttonRect.height - k_AssetButtonSelectedBorderSize * 2);
                Texture2D previewTexture = AssetPreview.GetAssetPreview(asset);
                if (previewTexture != null)
                {
                    GUI.DrawTexture(textureRect, previewTexture);
                }
                else
                {
                    // Show the loading label
                    s_LoadingLabelStyle ??= new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 10,
                        alignment = TextAnchor.MiddleCenter,
                    };
                    EditorGUI.DrawRect(textureRect, new Color(0.1f, 0.1f, 0.1f, 1f));
                    EditorGUI.LabelField(textureRect, "ロード中...", s_LoadingLabelStyle);
                }

                GUILayout.FlexibleSpace();

                if (Event.current.type == EventType.MouseUp &&
                    buttonRect.Contains(Event.current.mousePosition))
                {
                    onClick?.Invoke();
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseDrag &&
                    scope.rect.Contains(Event.current.mousePosition))
                {
                    if (isDragEnabled)
                    {
                        DragAndDrop.PrepareStartDrag();
                        DragAndDrop.paths = null;
                        DragAndDrop.objectReferences = new Object[] { asset };
                        // Uncomment the following code if you need to have additional data for dragging.
                        // DragAndDrop.SetGenericData("data", data);
                        DragAndDrop.StartDrag(asset.name);
                    }
                }
            }

            EditorGUILayout.Space(k_AssetButtonBottomMargin);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(asset, typeof(PlateauSandboxVehicle), false);
            EditorGUI.EndDisabledGroup();
        }
    }
}