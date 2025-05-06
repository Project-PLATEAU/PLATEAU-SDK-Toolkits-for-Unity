using PlateauToolkit.Editor;
using System;
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

        /// <summary>
        /// KnotPlacementToolの終了判定用
        /// </summary>
        private Type LastToolType { get; set; }

        /// <summary>
        /// 新規トラック作成で作成されたトラック
        /// </summary>
        private PlateauSandboxTrack SelectedTrack { get; set; }

        /// <summary>
        /// ToolManager.activeToolType切り替えコールバック
        /// </summary>
        void OnActiveToolChanged()
        {
            // KnotPlacementToolがinternalクラスでアクセスできないので名前ハードコーディング
            // KnotPlacementToolが終わった時に, 対象のTrackにknotsが無ければキャンセル扱いで破棄する
            if (LastToolType.Name == "KnotPlacementTool")
            {
                if (SelectedTrack && SelectedTrack.GetKnotsCount() == 0)
                {
                    // ↓でSelectedTrack初期化しているので一時変数に入れる
                    var tmp = SelectedTrack;
                    EditorApplication.delayCall += () =>
                    {
                        GameObject.DestroyImmediate(tmp.gameObject);
                    };
                }
                ToolManager.activeToolChanged -= OnActiveToolChanged;
                SelectedTrack = null;
            }

            LastToolType = ToolManager.activeToolType;
        }

        /// <summary>
        /// ToolManagerのactiveToolTypeの切り替え検知開始
        /// </summary>
        /// <param name="selectedTrack"></param>
        void StartToolChangeNotifier(PlateauSandboxTrack selectedTrack)
        {
            SelectedTrack = selectedTrack;
            LastToolType = ToolManager.activeToolType;
            ToolManager.activeToolChanged += OnActiveToolChanged;
        }


        public void OnGUI(PlateauSandboxContext context, EditorWindow window)
        {
            PlateauToolkitEditorGUILayout.Header("ツール");

            // ヒエラルキーで直接破壊したとき用.
            // Trackが外部から破壊されたときには表示を更新する
            if (context.IsAnyTrackDestroyed)
            {
                EditorApplication.delayCall += () =>
                {
                    RefreshTracksHierarchy(context);
                };
            }
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

                        StartToolChangeNotifier(trackGameObject.GetComponent<PlateauSandboxTrack>());
                        Selection.activeObject = trackGameObject;
                        ActiveEditorTracker.sharedTracker.RebuildIfNecessary();
                        EditorApplication.delayCall += () =>
                        {
                            EditorSplineUtility.SetKnotPlacementTool();
                            RefreshTracksHierarchy(context);
                        };

                        // EndLayoutGroup: BeginLayoutGroup must be called first.が出ないようにする対策
                        GUIUtility.ExitGUI();
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

                    var evt = Event.current;
                    if (evt.type == EventType.KeyDown && evt.keyCode == KeyCode.Escape)
                    {
                        EditorApplication.delayCall += () =>
                        {
                        };
                        GUIUtility.ExitGUI();
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