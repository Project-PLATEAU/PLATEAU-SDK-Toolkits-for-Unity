using PlateauToolkit.Editor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    enum BulkPlaceViewPageIndex
    {
        k_FieldSelect = 0,
        k_AssetSelect = 1,
    }

    class PlateauSandboxWindowBulkPlaceView : IPlateauSandboxWindowView
    {
        PlateauSandboxBulkPlaceHierarchyView m_TreeView;
        TreeViewState m_TreeViewState;
        PlateauSandboxBulkPlaceHierarchyContext m_HierarchyContext;
        SandboxAssetListState<PlateauSandboxProp> m_AssetListState;
        CancellationTokenSource m_Cancellation;
        PlateauSandboxFileParserValidationType m_IsValidLoadedFile;
        List<PlateauSandboxBulkPlaceHierarchyItem> m_HierarchyItems = new List<PlateauSandboxBulkPlaceHierarchyItem>();
        PlateauSandboxBulkPlaceDataContext m_DataContext;

        bool m_IsReadyApplied;
        bool m_IsClickedAssetPlace;
        int m_SelectedCategoryId = -1;
        bool m_IsIgnoreHeight;
        BulkPlaceViewPageIndex m_ViewPageIndex = BulkPlaceViewPageIndex.k_FieldSelect;
        bool m_IsValidPlaceAsset = true;

        public string Name => "アセット一括配置";

        public void OnBegin(PlateauSandboxContext context, EditorWindow editorWindow)
        {
            m_AssetListState = new SandboxAssetListState<PlateauSandboxProp>();
            m_Cancellation = new CancellationTokenSource();
            m_DataContext = new PlateauSandboxBulkPlaceDataContext();

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
            OnGUITool(context, window);
            OnGUIFieldType(context, window);
            OnGUIAssetType(context, window);
        }

        void OnGUITool(PlateauSandboxContext context, EditorWindow window)
        {
            EditorGUILayout.LabelField("ツール", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (m_DataContext.HasLoadedFile())
                {
                    using (PlateauToolkitEditorGUILayout.BackgroundColorScope(Color.green))
                    {
                        if (GUILayout.Button($"{m_DataContext.GetFileName()} を読み込み済"))
                        {
                            m_IsClickedAssetPlace = false;
                            m_DataContext.Clear();
                            m_HierarchyItems.Clear();
                            RefreshTracksHierarchy(context);
                            m_ViewPageIndex = BulkPlaceViewPageIndex.k_FieldSelect;
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
                            m_DataContext.SetFilePath(filePath);
                            if (m_DataContext.HasLoadedFile())
                            {
                                if (m_DataContext.GetFileType() == PlateauSandboxBulkPlaceFileType.k_Csv)
                                {
                                    var parser = new PlateauSandboxFileCsvParser();
                                    m_IsValidLoadedFile = parser.IsValidate(filePath);
                                    if (m_IsValidLoadedFile == PlateauSandboxFileParserValidationType.k_Valid)
                                    {
                                        var parsedData = parser.Load(filePath);
                                        if (parsedData.Count > 0)
                                        {
                                            m_DataContext.SetAllData(parsedData);
                                            RefreshTracksHierarchy(context);
                                        }
                                    }
                                    else
                                    {
                                        m_DataContext.Clear();
                                    }
                                }
                                else if (m_DataContext.GetFileType() == PlateauSandboxBulkPlaceFileType.k_ShapeFile)
                                {
                                    var parser = new PlateauSandboxFileShapeFileParser();
                                    m_IsValidLoadedFile = parser.IsValidate(filePath);
                                    if (m_IsValidLoadedFile == PlateauSandboxFileParserValidationType.k_Valid)
                                    {
                                        var parsedData = parser.Load(filePath);
                                        if (parsedData.Count > 0)
                                        {
                                            m_DataContext.SetAllData(parsedData);
                                            RefreshTracksHierarchy(context);
                                        }
                                    }
                                    else
                                    {
                                        m_DataContext.Clear();
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
                    if (m_DataContext.HasLoadedFile() && m_HierarchyItems.Any(item => item.PrefabConstantId >= 0))
                    {
                        PlaceAssets();
                    }
                    m_IsClickedAssetPlace = true;
                }

                if (m_IsClickedAssetPlace)
                {
                    if (!m_DataContext.HasLoadedFile())
                    {
                        EditorGUILayout.HelpBox("shapeファイル、csvファイルを読み込んでください", MessageType.Error);
                    }

                    if (m_HierarchyItems.All(item => item.PrefabConstantId == -1))
                    {
                        EditorGUILayout.HelpBox("プレファブを設定してください", MessageType.Warning);
                    }

                    if (!m_IsValidPlaceAsset)
                    {
                        EditorGUILayout.HelpBox("該当地区のデータをロードしてください", MessageType.Warning);
                    }
                }

                if (GUILayout.Button("CSVテンプレートの生成"))
                {
                    string filePath = EditorUtility.SaveFilePanel("Save File", "", "PlateauSandboxCSVTemplate", "csv");
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        var templateData = new List<PlateauSandboxBulkPlaceDataBase>();

                        var data = new PlateauSandboxBulkPlaceCsvData(0, new []
                        {
                            "35.8994", "139.5333", "14.23", "イチョウ",
                        }, new []{ "経度", "緯度", "高さ", "アセット種別" });
                        templateData.Add(data);

                        data = new PlateauSandboxBulkPlaceCsvData(1, new []
                        {
                            "35.9014", "139.5721", "16.3", "ユリノキ",
                        }, new []{ "経度", "緯度", "高さ", "アセット種別" });
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
        }

        void OnGUIFieldType(PlateauSandboxContext context, EditorWindow window)
        {
            if (m_ViewPageIndex != BulkPlaceViewPageIndex.k_FieldSelect)
            {
                return;
            }

            if (!m_DataContext.HasLoadedFile())
            {
                m_IsIgnoreHeight = false;
                return;
            }

            EditorGUILayout.LabelField("パース対象設定", EditorStyles.boldLabel);

            float originalValue = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 180; // Set the label width
            m_IsIgnoreHeight = m_IsIgnoreHeight = EditorGUILayout.Toggle("ファイルの高さ情報を無視する",
                                   m_IsIgnoreHeight,
                                   GUILayout.Width(1000));
            EditorGUIUtility.labelWidth = originalValue;

            EditorGUILayout.LabelField("利用する属性列の選択", EditorStyles.label);

            EditorGUILayout.BeginVertical();

            void DrawLabel(string titleLabel, int labelIndex)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(30);
                    int selectIndex = EditorGUILayout.Popup(titleLabel, labelIndex, m_DataContext.GetFieldLabels(), GUILayout.Width(350));
                    if (selectIndex != labelIndex)
                    {
                        m_DataContext.ReplaceField(labelIndex, selectIndex);
                        m_HierarchyItems.Clear();
                        RefreshTracksHierarchy(context);
                    }
                    GUILayout.FlexibleSpace();
                }
            }

            if (m_DataContext.GetFileType() == PlateauSandboxBulkPlaceFileType.k_Csv)
            {
                DrawLabel(PlateauSandboxBulkPlaceCategory.k_Longitude.Label(),
                    m_DataContext.GetFieldIndex(PlateauSandboxBulkPlaceCategory.k_Longitude));
                DrawLabel(PlateauSandboxBulkPlaceCategory.k_Latitude.Label(),
                    m_DataContext.GetFieldIndex(PlateauSandboxBulkPlaceCategory.k_Latitude));
                DrawLabel(PlateauSandboxBulkPlaceCategory.k_Height.Label(),
                    m_DataContext.GetFieldIndex(PlateauSandboxBulkPlaceCategory.k_Height));
                DrawLabel(PlateauSandboxBulkPlaceCategory.k_AssetType.Label(),
                    m_DataContext.GetFieldIndex(PlateauSandboxBulkPlaceCategory.k_AssetType));
            }
            else
            {
                DrawLabel(PlateauSandboxBulkPlaceCategory.k_AssetType.Label(),
                    m_DataContext.GetFieldIndex(PlateauSandboxBulkPlaceCategory.k_AssetType));
            }

            EditorGUILayout.EndVertical();

            SetFooterButton(true);
        }

        void OnGUIAssetType(PlateauSandboxContext context, EditorWindow window)
        {
            if (m_ViewPageIndex != BulkPlaceViewPageIndex.k_AssetSelect)
            {
                return;
            }

            m_TreeView.OnGUI(EditorGUILayout.GetControlRect(false, 150));

            if (m_AssetListState.IsReady)
            {
                PlateauSandboxAssetListGUI.OnGUI(window.position.width, context, m_AssetListState);
            }
            else
            {
                EditorGUILayout.HelpBox("アセットを読み込んでいます...", MessageType.Info);
            }

            SetFooterButton(false);
        }

        void SetFooterButton(bool isNext)
        {
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (isNext)
            {
                // rightAlignment
                GUILayout.FlexibleSpace();
            }

            if (GUILayout.Button(isNext ? "次へ" : "戻る", GUILayout.Width(150)))
            {
                if (isNext)
                {
                    m_ViewPageIndex++;
                }
                else
                {
                    m_ViewPageIndex--;
                }
            }
            if (!isNext)
            {
                // leftAlignment
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(20);
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
                if (m_DataContext.HasLoadedFile())
                {
                    m_DataContext.Datas
                        .GroupBy(data => data.AssetType)
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
                                width = 250,
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
                                headerContent = new GUIContent("プレハブ名"),
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

        void PlaceAssets()
        {
            var placement = new PlateauSandboxPrefabPlacement();
            m_IsValidPlaceAsset = placement.IsValid();
            if (!m_IsValidPlaceAsset)
            {
                return;
            }

            foreach (var placeData in m_DataContext.Datas)
            {
                var hierarchyItem = m_HierarchyItems.FirstOrDefault(item => item.CategoryName == placeData.AssetType);
                if (hierarchyItem == null || hierarchyItem.PrefabConstantId < 0)
                {
                    continue;
                }

                var prefab = m_AssetListState.Assets
                    .FirstOrDefault(asset => asset.Asset.gameObject.GetInstanceID() == hierarchyItem.PrefabConstantId)?.Asset.gameObject;
                if (prefab == null)
                {
                    continue;
                }

                placement.Place(new PlateauSandboxPrefabPlacement.PlacementContext()
                {
                    m_Latitude = double.Parse(placeData.Latitude),
                    m_Longitude = double.Parse(placeData.Longitude),
                    m_Height = m_IsIgnoreHeight ? 0 : double.Parse(placeData.Height),
                    m_Prefab = prefab,
                    m_AssetType = placeData.AssetType
                });
            }


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