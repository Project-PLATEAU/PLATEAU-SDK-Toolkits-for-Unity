using PlateauToolkit.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Splines;
using UnityEngine;
using UnityEngine.Events;

namespace PlateauToolkit.Sandbox.Editor
{
    class PlateauSandboxWindowBulkPlaceView : IPlateauSandboxWindowView
    {
        PlateauSandboxBulkPlaceHierarchyView m_TreeView;
        TreeViewState m_TreeViewState;
        PlateauSandboxBulkPlaceHierarchyContext m_HierarchyContext;
        SandboxAssetListState<PlateauSandboxProp> m_AssetListState;
        CancellationTokenSource m_Cancellation;
        bool m_IsReadyApplied;
        static GUIStyle s_FooterContentStyle;

        string m_LoadedFileName;
        List<PlateauSandboxBulkPlaceData> m_LoadedData;
        bool m_IsClickedAssetPlace;
        Vector2 m_ScrollPosition;
        int m_SelectedCategoryId = -1;
        private PlateauSandboxFileParserValidationType m_IsValidLoadedFile;

        List<PlateauSandboxBulkPlaceHierarchyItem> m_HierarchyItems = new List<PlateauSandboxBulkPlaceHierarchyItem>();

        public string Name => "アセット一括配置";

        public void OnBegin(PlateauSandboxContext context, EditorWindow editorWindow)
        {
            m_AssetListState = new SandboxAssetListState<PlateauSandboxProp>();
            m_Cancellation = new CancellationTokenSource();

            _ = m_AssetListState.PrepareAsync(m_Cancellation.Token);

            context.OnSelectedObjectChanged.AddListener((selectedObject) =>
            {
                if (m_SelectedCategoryId < 0 || m_HierarchyItems.Count == 0)
                {
                    return;
                }
                m_HierarchyItems[m_SelectedCategoryId].PrefabName = selectedObject.name;
                m_HierarchyItems[m_SelectedCategoryId].PrefabConstantId = selectedObject.GetInstanceID();

                RefreshTracksHierarchy(context);
            });

            RefreshTracksHierarchy(context);
        }

        public void OnGUI(PlateauSandboxContext context, EditorWindow window)
        {
            EditorGUILayout.LabelField("ツール", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (!string.IsNullOrEmpty(m_LoadedFileName))
                {
                    using (PlateauToolkitEditorGUILayout.BackgroundColorScope(Color.green))
                    {
                        if (GUILayout.Button($"{m_LoadedFileName} を読み込み済"))
                        {
                            m_IsClickedAssetPlace = false;
                            m_LoadedFileName = string.Empty;
                            m_HierarchyItems.Clear();
                            RefreshTracksHierarchy(context);
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button("shapeファイル、csvファイルを読み込む"))
                    {
                        string filePath = EditorUtility.OpenFilePanel("Select File", "", "csv,shp");
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            m_LoadedFileName = System.IO.Path.GetFileName(filePath);
                            if (!string.IsNullOrEmpty(m_LoadedFileName))
                            {
                                string fileExtension = Path.GetExtension(filePath);
                                if (fileExtension == PlateauSandboxBulkPlaceData.k_CsvExtension)
                                {
                                    var parser = new PlateauSandboxFileCsvParser();
                                    m_IsValidLoadedFile = parser.IsValidate(filePath);
                                    if (m_IsValidLoadedFile == PlateauSandboxFileParserValidationType.k_Valid)
                                    {
                                        m_LoadedData = parser.Load(filePath);
                                        if (m_LoadedData.Count > 0)
                                        {
                                            RefreshTracksHierarchy(context);
                                        }
                                    }
                                    else
                                    {
                                        m_LoadedFileName = string.Empty;
                                    }
                                }
                                else if (fileExtension == PlateauSandboxBulkPlaceData.k_ShapeFileExtension)
                                {
                                    var parser = new PlateauSandboxFileShapeFileParser();
                                    m_IsValidLoadedFile = parser.IsValidate(filePath);
                                    if (m_IsValidLoadedFile == PlateauSandboxFileParserValidationType.k_Valid)
                                    {
                                        m_LoadedData = parser.Load(filePath);
                                        if (m_LoadedData.Count > 0)
                                        {
                                            RefreshTracksHierarchy(context);
                                        }
                                    }
                                    else
                                    {
                                        m_LoadedFileName = string.Empty;
                                    }
                                }
                            }
                        }
                    }
                }
                if (m_IsValidLoadedFile != PlateauSandboxFileParserValidationType.k_Valid)
                {
                    switch (m_IsValidLoadedFile)
                    {
                        case PlateauSandboxFileParserValidationType.k_NotExistsFile:
                            EditorGUILayout.HelpBox("csvもしくはshapeファイルが見つかりませんでした。", MessageType.Error);
                            break;
                        case PlateauSandboxFileParserValidationType.k_AccessControl:
                            EditorGUILayout.HelpBox("ファイルにアクセスできませんでした。", MessageType.Error);
                            break;
                        case PlateauSandboxFileParserValidationType.k_FileOpened:
                            EditorGUILayout.HelpBox("ファイルが開かれているためアクセスできませんでした。", MessageType.Error);
                            break;
                        case PlateauSandboxFileParserValidationType.k_NotExistsDbfFile:
                            EditorGUILayout.HelpBox("DBFファイルが見つかりませんでした。", MessageType.Error);
                            break;
                    }
                }
                if (GUILayout.Button("アセットを配置"))
                {
                    m_IsClickedAssetPlace = true;
                }
                if (string.IsNullOrEmpty(m_LoadedFileName) && m_IsClickedAssetPlace)
                {
                    EditorGUILayout.HelpBox("shapeファイル、csvファイルを読み込んでください", MessageType.Error);
                }
                else if (m_HierarchyItems.Any(item => item.PrefabConstantId < 0))
                {
                    EditorGUILayout.HelpBox("プレファブを設定してください", MessageType.Warning);
                }

                if (GUILayout.Button("CSVテンプレートの生成"))
                {
                    string filePath = EditorUtility.SaveFilePanel("Save File", "", "PlateauSandboxCSVTemplate", "csv");
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        var templateData = new List<PlateauSandboxBulkPlaceData>();

                        var data = new PlateauSandboxBulkPlaceData();
                        data.Set(0, 35.8994f, 139.5333f, 14.23f, new string[1]{"イチョウ"});
                        templateData.Add(data);

                        data = new PlateauSandboxBulkPlaceData();
                        data.Set(1, 35.9014f, 139.5721f, 16.3f, new string[1]{"ユリノキ"});
                        templateData.Add(data);

                        bool saveSuccess = new PlateauSandboxFileCsvParser().Save(filePath, templateData);
                        if (saveSuccess)
                        {
                            string directoryName = Path.GetDirectoryName(filePath);
                            if (!string.IsNullOrEmpty(directoryName))
                            {
                                System.Diagnostics.Process.Start(directoryName);
                            }
                            Debug.Log($"CSVテンプレートを保存しました: {filePath}");
                        }
                    }
                }
            }

            EditorGUILayout.LabelField("アセット種別", EditorStyles.boldLabel);
            m_TreeView.OnGUI(EditorGUILayout.GetControlRect(false, 150));

            if (m_AssetListState.IsReady)
            {
                PlateauSandboxAssetListGUI.OnGUI(window.position.width, context, m_AssetListState);
            }
            else
            {
                EditorGUILayout.HelpBox("アセットを読み込んでいます...", MessageType.Info);
            }
        }

        public void OnUpdate(EditorWindow editorWindow)
        {
            if (m_AssetListState.IsReady && !m_IsReadyApplied)
            {
                editorWindow.Repaint();
                m_IsReadyApplied = true;
            }
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

            if (m_HierarchyItems.Count == 0)
            {
                if (!string.IsNullOrEmpty(m_LoadedFileName))
                {
                    m_LoadedData
                        .SelectMany(data => data.AssetTypes)
                        .GroupBy(assetType => assetType)
                        .Select(group => (group.Key, group.Count()))
                        .Select((group, index) => (group, index))
                        .ToList()
                        .ForEach(asset =>
                        {
                            string categoryName = asset.group.Key;
                            int count = asset.group.Item2;
                            m_HierarchyItems.Add(new PlateauSandboxBulkPlaceHierarchyItem()
                            {
                                Id = asset.index,
                                CategoryName = string.IsNullOrEmpty(categoryName) ? "指定なし" : categoryName,
                                Count = count,
                            });
                        });
                }

                foreach (var item in m_HierarchyItems)
                {
                    item.OnClicked.AddListener(() =>
                    {
                        m_SelectedCategoryId = item.Id;
                        if (item.PrefabConstantId > 0)
                        {
                            var selectedPrefab = m_AssetListState.Assets.FirstOrDefault(asset => asset.Asset.gameObject.GetInstanceID() == item.PrefabConstantId);
                            if (selectedPrefab != null)
                            {
                                context.SelectPlaceableObject(selectedPrefab.Asset.gameObject);
                            }
                        }
                    });
                }
            }

            m_HierarchyContext.Hierarchy.Items = m_HierarchyItems.ToArray();
            m_TreeView.Reload();
        }

        void SetUpTreeView()
        {
            m_TreeViewState ??= new TreeViewState();

            m_HierarchyContext ??= new PlateauSandboxBulkPlaceHierarchyContext
            {
                Hierarchy = new PlateauSandboxBulkPlaceHierarchy(
                    new PlateauSandboxBulkPlaceHierarchyHeader(
                        new[]
                        {
                            new MultiColumnHeaderState.Column
                            {
                                headerContent = new GUIContent("種別"),
                                headerTextAlignment = TextAlignment.Center,
                                canSort = true,
                                width = 100,
                                minWidth = 10,
                                autoResize = true,
                                allowToggleVisibility = false,
                            },
                            new MultiColumnHeaderState.Column
                            {
                                headerContent = new GUIContent("配置数"),
                                headerTextAlignment = TextAlignment.Center,
                                canSort = true,
                                width = 100,
                                minWidth = 10,
                                autoResize = true,
                                allowToggleVisibility = true,
                            },
                            new MultiColumnHeaderState.Column
                            {
                                headerContent = new GUIContent("プレファブ名"),
                                headerTextAlignment = TextAlignment.Center,
                                canSort = true,
                                width = 300,
                                minWidth = 10,
                                autoResize = true,
                                allowToggleVisibility = true,
                            },
                        })),
            };

            var headerState = new MultiColumnHeaderState(m_HierarchyContext.Hierarchy.Header.Columns);
            var header = new MultiColumnHeader(headerState);

            m_TreeView = new PlateauSandboxBulkPlaceHierarchyView(m_TreeViewState, header, m_HierarchyContext);
            m_TreeView.Reload();
        }

        public void OnEnd(PlateauSandboxContext context)
        {
            m_IsReadyApplied = false;

            m_Cancellation.Cancel();
            m_Cancellation.Dispose();
            m_Cancellation = null;

            m_AssetListState.Dispose();
            m_AssetListState = null;
        }
    }
}