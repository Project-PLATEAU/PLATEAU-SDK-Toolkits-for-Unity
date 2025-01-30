using PlateauToolkit.Editor;
using System;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    interface IPlateauSandboxAssetListView
    {
        void OnBegin();
        void OnUpdate(EditorWindow editorWindow);
        void OnGUI(PlateauSandboxContext context, float windowWidth, bool isShowAssetCreate);
        void OnEnd();
        GameObject GetAssetByPrefabConstantId(int prefabConstantId);

        void EnableMultipleSelect(bool enable);
        void SelectAll(bool select, PlateauSandboxContext context);

        SandboxAssetType SelectedAssetType { get; set; }
    }

    /// <summary>
    /// Assets List Base
    /// </summary>
    /// <remarks>
    /// Assets are operated by <see cref="PlateauSandboxAssetUtility" />
    /// </remarks>
    class PlateauSandboxAssetListViewBase<TAsset> : IPlateauSandboxAssetListView where TAsset : Component
    {
        SandboxAssetListState<TAsset> m_CurrentState = new SandboxAssetListState<TAsset>();
        CancellationTokenSource m_Cancellation;
        bool m_IsReadyApplied;
        bool m_IsMultipleSelect = false;
        GUIStyle m_FooterContentStyle;

        public SandboxAssetType SelectedAssetType
        {
            get => m_CurrentState.SelectedAssetType;
            set => m_CurrentState.SelectedAssetType = value;
        }

        public void EnableMultipleSelect(bool enable)
        {
            m_IsMultipleSelect = enable;
        }

        public void OnBegin()
        {
            SelectedAssetType = SandboxAssetType.All;
            PrepareAsync();
        }

        void PrepareAsync()
        {
            m_Cancellation = new CancellationTokenSource();
            _ = m_CurrentState.PrepareAsync(m_Cancellation.Token);
        }

        public void OnUpdate(EditorWindow editorWindow)
        {
            if (m_CurrentState.IsReady && !m_IsReadyApplied)
            {
                editorWindow.Repaint();
                m_IsReadyApplied = true;
            }
        }

        public void OnGUI(PlateauSandboxContext context, float windowWidth, bool isShowAssetCreate)
        {
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

            if (m_CurrentState.IsReady)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(15);

                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        GUILayout.Space(8);

                        using (var scope = new EditorGUILayout.ScrollViewScope(m_CurrentState.ScrollPosition))
                        {
                            Action[] buttonGuis = m_CurrentState.Assets
                                .Where(asset => asset != null)
                                .Where(asset => (asset.AssetType & m_CurrentState.SelectedAssetType) == asset.AssetType)
                                .Where(asset => asset.Asset != null)
                                .Select<SandboxAsset<TAsset>, Action>(propAsset =>
                                {
                                    if (m_IsMultipleSelect)
                                    {
                                        return () => PlateauSandboxGUI.AssetButton(
                                        propAsset,
                                        context.IsSelectedObjectMultiple(propAsset.Asset.gameObject),
                                        () =>
                                        {
                                            if (context.IsSelectedObject(propAsset.Asset.gameObject))
                                            {
                                                context.RemoveSelectedObjectMultiple(propAsset.Asset.gameObject);
                                            }
                                            else
                                            {
                                                context.SelectPlaceableObjectMultiple(propAsset.Asset.gameObject);
                                            }
                                        },
                                        isDragEnabled);
                                    }
                                    else
                                    {
                                        return () => PlateauSandboxGUI.AssetButton(
                                        propAsset,
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
                                    }
                                })
                                .ToArray();
                            PlateauToolkitEditorGUILayout.GridLayout(windowWidth, 100, 100, buttonGuis);

                            m_CurrentState.ScrollPosition = scope.scrollPosition;
                        }
                    }

                    GUILayout.Space(15);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("アセットを読み込んでいます...", MessageType.Info);
            }

            DrawFooter(isShowAssetCreate);
        }

        public void SelectAll(bool select, PlateauSandboxContext context)
        {
            var targets = m_CurrentState.Assets.Where(asset => (asset.AssetType & m_CurrentState.SelectedAssetType) == asset.AssetType).ToList();
            foreach (SandboxAsset<TAsset> asset in targets)
            {
                context.RemoveSelectedObjectMultiple(asset.Asset.gameObject);
                if (select)
                {
                    context.SelectPlaceableObjectMultiple(asset.Asset.gameObject);
                }
            }
        }

        void DrawFooter(bool isShowAssetCreate)
        {
            using (PlateauToolkitEditorGUILayout.FooterScope())
            {
                m_FooterContentStyle ??= new GUIStyle()
                {
                    normal = { textColor = Color.white },
                    alignment = TextAnchor.MiddleCenter,
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(5, 5, 15, 5),
                };
                using (new EditorGUILayout.VerticalScope(m_FooterContentStyle))
                {
                    if (isShowAssetCreate)
                    {
                        if (new PlateauToolkitImageButtonGUI(
                                220,
                                40,
                                PlateauToolkitGUIStyles.k_ButtonPrimaryColor).Button("アセットを作成"))
                        {
                            PlateauSandboxPrefabCreationWizard.DisplayWizard();
                        }
                    }

                    if (PlateauSandboxAssetUtility.GetSample(out Sample sample))
                    {
                        if (!sample.isImported)
                        {
                            GUILayout.Space(5);

                            if (new PlateauToolkitImageButtonGUI(
                                    220,
                                    40,
                                    PlateauToolkitGUIStyles.k_ButtonPrimaryColor).Button("ビルトインアセットをインポート"))
                            {
                                bool yes = EditorUtility.DisplayDialog(
                                    "PLATEAU Toolkit",
                                    "ビルトインアセットをインポートしますか？",
                                    "インポート", "キャンセル");

                                if (yes)
                                {
                                    bool isImport = sample.Import();
                                    if (isImport)
                                    {
                                        PrepareAsync();
                                    }
                                }
                            }

                        }
                    }
                    else
                    {
                        using (new EditorGUI.DisabledScope(true))
                        {
                            if (new PlateauToolkitImageButtonGUI(
                                    220,
                                    40,
                                    PlateauToolkitGUIStyles.k_ButtonDisableColor).Button("ビルドインアセットが読み込めません"))
                            {
                            }
                        }
                    }
                }
            }
        }

        public GameObject GetAssetByPrefabConstantId(int prefabConstantId)
        {
            if (m_CurrentState.Assets == null)
            {
                return null;
            }

            return m_CurrentState.Assets
                .Where(asset => asset != null && asset.Asset != null)
                .Where(asset => asset.Asset.gameObject.GetInstanceID() == prefabConstantId)
                .Select(asset => asset.Asset.gameObject)
                .FirstOrDefault();
        }

        public void OnEnd()
        {
            m_IsReadyApplied = false;

            if (m_Cancellation != null)
            {
                m_Cancellation.Cancel();
                m_Cancellation.Dispose();
                m_Cancellation = null;
            }
        }
    }
}