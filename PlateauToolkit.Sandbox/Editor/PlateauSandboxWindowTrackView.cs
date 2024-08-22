using PlateauToolkit.Editor;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Splines;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    class PlateauSandboxWindowTrackView : IPlateauSandboxWindowView
    {
        PlateauSandboxHierarchyView m_TreeView;
        TreeViewState m_TreeViewState;
        PlateauSandboxHierarchyContext m_HierarchyContext;

        public string Name => "トラック";

        public void OnBegin(PlateauSandboxContext context, EditorWindow editorWindow)
        {
            RefreshTracksHierarchy(context);
        }

        public void OnGUI(PlateauSandboxContext context, EditorWindow window)
        {
            PlateauToolkitEditorGUILayout.Header("ツール");
            using (new EditorGUILayout.VerticalScope())
            {
                if (ToolManager.activeToolType.Name != "KnotPlacementTool")
                {
                    if (new PlateauToolkitImageButtonGUI(
                            220,
                            40,
                            PlateauToolkitGUIStyles.k_ButtonPrimaryColor).Button("新しいトラックを作成"))
                    {
                        string gameObjectName = GameObjectUtility.GetUniqueNameForSibling(null, "Track");
                        GameObject trackGameObject = ObjectFactory.CreateGameObject(gameObjectName, typeof(PlateauSandboxTrack));

                        trackGameObject.transform.localPosition = Vector3.zero;
                        trackGameObject.transform.localRotation = Quaternion.identity;

                        Selection.activeObject = trackGameObject;
                        ActiveEditorTracker.sharedTracker.RebuildIfNecessary();
                        EditorApplication.delayCall += () =>
                        {
                            EditorSplineUtility.SetKnotPlacementTool();
                            RefreshTracksHierarchy(context);
                        };
                    }
                }
                else
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        if (new PlateauToolkitImageButtonGUI(
                                310,
                                40,
                                PlateauToolkitGUIStyles.k_ButtonCancelColor).Button("ESCキーまたはエンターキーでトラック作成を終了"))
                        {
                        }
                    }
                }
                EditorGUILayout.Space(10);

                using (new EditorGUI.DisabledGroupScope(context.Tracks.Count == 0))
                {
                    if (ToolManager.activeToolType == typeof(PlateauSandboxSpeedLimitTool))
                    {
                        if (new PlateauToolkitImageButtonGUI(
                                220,
                                40,
                                PlateauToolkitGUIStyles.k_ButtonCancelColor).Button("制限速度設定ツールを終了"))
                        {
                            ToolManager.RestorePreviousPersistentTool();
                        }
                    }
                    else
                    {
                        if (new PlateauToolkitImageButtonGUI(
                                220,
                                40,
                                PlateauToolkitGUIStyles.k_ButtonPrimaryColor).Button("制限速度設定ツールを起動"))
                        {
                            ToolManager.SetActiveTool<PlateauSandboxSpeedLimitTool>();
                        }
                    }
                }
            }
            EditorGUILayout.Space(15);

            m_TreeView.OnGUI(EditorGUILayout.GetControlRect(false, 200));
        }

        void IPlateauSandboxWindowView.OnHierarchyChange(PlateauSandboxContext context)
        {
            RefreshTracksHierarchy(context);
        }

        void RefreshTracksHierarchy(PlateauSandboxContext context)
        {
            if (m_TreeView == null)
            {
                SetUpTreeView();
                Debug.Assert(m_TreeView != null);
            }

            context.RefreshTracks();
            m_HierarchyContext.Hierarchy.Items = context.Tracks
                .Select(
                    track => new PlateauSandboxHierarchyItem
                    {
                        Id = track.GetTrackId(),
                        GameObjectName = track.gameObject.name,
                        KnotsCount = track.GetKnotsCount(),
                    })
                .ToArray();

            m_TreeView.Reload();
        }

        void SetUpTreeView()
        {
            m_TreeViewState ??= new TreeViewState();

            m_HierarchyContext ??= new PlateauSandboxHierarchyContext
            {
                Hierarchy = new PlateauSandboxHierarchy(
                    new PlateauSandboxHierarchyHeader(
                        new[]
                        {
                            new MultiColumnHeaderState.Column
                            {
                                headerContent = new GUIContent("GameObject"),
                                headerTextAlignment = TextAlignment.Center,
                                canSort = true,
                                width = 100,
                                minWidth = 10,
                                autoResize = true,
                                allowToggleVisibility = false,
                            },
                            new MultiColumnHeaderState.Column
                            {
                                headerContent = new GUIContent("Knots"),
                                headerTextAlignment = TextAlignment.Center,
                                canSort = true,
                                width = 100,
                                minWidth = 10,
                                autoResize = true,
                                allowToggleVisibility = true,
                            },
                        })),
            };

            var headerState = new MultiColumnHeaderState(m_HierarchyContext.Hierarchy.Header.Columns);
            var header = new MultiColumnHeader(headerState);

            m_TreeView = new PlateauSandboxHierarchyView(m_TreeViewState, header, m_HierarchyContext);
            m_TreeView.Reload();
        }
    }
}