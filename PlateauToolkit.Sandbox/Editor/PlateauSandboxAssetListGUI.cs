using PlateauToolkit.Editor;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    class PlateauSandboxAssetListState<TAsset> where TAsset : Component
    {
        public Vector2 ScrollPosition { get; set; }
        public PlateauSandboxAsset<TAsset>[] Cache { get; set; }
        public int SelectedAssetType { get; set; }
    }

    /// <summary>
    /// Assets List GUI
    /// </summary>
    /// <remarks>
    /// Assets are operated by <see cref="PlateauSandboxAssetUtility" />
    /// </remarks>
    static class PlateauSandboxAssetListGUI
    {
        static GUIStyle s_FooterContentStyle;

        public static void OnGUI<TAsset>(
            float windowWidth, PlateauSandboxContext context,
            PlateauSandboxAssetListState<TAsset> state)
            where TAsset : Component
        {
            EditorGUILayout.LabelField("アセット", EditorStyles.boldLabel);

            state.SelectedAssetType = GUILayout.Toolbar(
                state.SelectedAssetType,
                new[] { "全て", "ユーザー", "ビルトイン" });

            PlateauSandboxAssetType assetType;
            switch (state.SelectedAssetType)
            {
                case 0:
                    assetType = PlateauSandboxAssetType.All;
                    break;
                case 1:
                    assetType = PlateauSandboxAssetType.UserDefined;
                    break;
                case 2:
                    assetType = PlateauSandboxAssetType.Builtin;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state.SelectedAssetType));
            }

            bool isDragEnabled = ToolManager.activeToolType != typeof(PlateauSandboxPlacementTool);

            if (isDragEnabled)
            {
                if (Event.current.type == EventType.DragUpdated &&
                    DragAndDrop.objectReferences != null &&
                    DragAndDrop.objectReferences.Length != 0)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                }
                else if (Event.current.type == EventType.DragPerform || Event.current.type == EventType.DragExited)
                {
                    DragAndDrop.activeControlID = 0;
                    DragAndDrop.AcceptDrag();
                }
                else
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.None;
                }
            }

            if (Event.current.type == EventType.Layout)
            {
                state.Cache = PlateauSandboxAssetUtility.FindAllAssets<TAsset>(assetType);
            }
            if (state.Cache == null)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Space(8);

                using (var scope = new EditorGUILayout.ScrollViewScope(state.ScrollPosition))
                {
                    Action[] buttonGuis = state.Cache
                        .Select<PlateauSandboxAsset<TAsset>, Action>(propAsset =>
                        {
                            return () => PlateauSandboxGUI.AssetButton(
                                propAsset.Asset.gameObject,
                                context.IsSelectedObject(propAsset.Asset.gameObject),
                                () =>
                                {
                                    if (context.IsSelectedObject(propAsset.Asset.gameObject))
                                    {
                                        context.SelectPlaceableObject(null);
                                    }
                                    else
                                    {
                                        context.SelectPlaceableObject(propAsset.Asset.gameObject);
                                    }
                                },
                                isDragEnabled);
                        })
                        .ToArray();
                    PlateauToolkitEditorGUILayout.GridLayout(windowWidth, 100, 100, buttonGuis);

                    state.ScrollPosition = scope.scrollPosition;
                }
            }

            using (PlateauToolkitEditorGUILayout.FooterScope())
            {
                s_FooterContentStyle ??= new GUIStyle(GUI.skin.box)
                {
                    normal = { textColor = Color.white },
                    alignment = TextAnchor.MiddleCenter,
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(5, 5, 5, 5),
                };
                using (new EditorGUILayout.HorizontalScope(s_FooterContentStyle))
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("アセットを作成"))
                    {
                        PlateauSandboxPrefabCreationWizard.DisplayWizard();
                    }

                    if (PlateauSandboxAssetUtility.GetSample(out Sample sample))
                    {
                        if (!sample.isImported)
                        {
                            if (GUILayout.Button("ビルトインアセットをインポート"))
                            {
                                bool yes = EditorUtility.DisplayDialog(
                                    "PLATEAU Toolkit",
                                    "ビルトインアセットをインポートしますか？",
                                    "インポート", "キャンセル");

                                if (yes)
                                {
                                    sample.Import();
                                }
                            }
                        }
                    }
                    else
                    {
                        using (new EditorGUI.DisabledScope(true))
                        {
                            GUILayout.Button("ビルドインアセットが読み込めません");
                        }
                    }
                }
            }
        }
    }
}