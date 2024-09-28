using PlateauToolkit.Editor;
using System;
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
        PlateauSandboxWindowBulkPlaceButtonView m_ButtonView;
        TreeViewState m_TreeViewState;
        PlateauSandboxBulkPlaceHierarchyContext m_HierarchyContext;
        CancellationTokenSource m_Cancellation;
        List<PlateauSandboxBulkPlaceHierarchyItem> m_HierarchyItem = new List<PlateauSandboxBulkPlaceHierarchyItem>();
        PlateauSandboxBulkPlaceDataContext m_DataContext;
        PlateauSandboxAssetContainerView m_AssetContainerView;

        int m_SelectedCategoryId = -1;
        bool m_IsIgnoreHeight;
        Vector2 m_AssetScrollPosition = new Vector2();
        BulkPlaceViewPageIndex m_ViewPageIndex = BulkPlaceViewPageIndex.k_FieldSelect;
        PlateauSandboxPrefabPlacement m_PrefabPlacement;

        public string Name => "アセット一括配置";
        readonly string[] m_AssetHeightLabels = { "ファイルで指定した高さに配置", "地面に配置" };

        public void OnBegin(PlateauSandboxContext context, EditorWindow editorWindow)
        {
            m_DataContext = new PlateauSandboxBulkPlaceDataContext();
            m_ButtonView = new PlateauSandboxWindowBulkPlaceButtonView(m_DataContext);
            m_AssetContainerView = new PlateauSandboxAssetContainerView(isShowAssetCreate: false);

            m_AssetContainerView.OnBegin(context, editorWindow);

            // Event For Asset List Item Clicked.
            context.OnSelectedObjectChanged.AddListener((selectedObject) =>
            {
                PlateauSandboxBulkPlaceHierarchyItem selectedItem = m_HierarchyItem
                    .FirstOrDefault(item => item.ID == m_SelectedCategoryId);
                if (selectedItem == null)
                {
                    return;
                }

                if (selectedObject == null && m_SelectedCategoryId >= 0)
                {
                    // Unselect the object
                    selectedItem.PrefabConstantID = -1;
                    selectedItem.PrefabName = string.Empty;
                }
                else
                {
                    selectedItem.PrefabName = selectedObject.name;
                    selectedItem.PrefabConstantID = selectedObject.GetInstanceID();
                }

                RefreshTracksHierarchy(context);
            });

            RefreshTracksHierarchy(context);
        }

        void IPlateauSandboxWindowView.OnHierarchyChange(PlateauSandboxContext context)
        {
            RefreshTracksHierarchy(context);
        }

        public void OnGUI(PlateauSandboxContext context, EditorWindow window)
        {
            OnGUITool(context);
            OnGUIFieldType(context);
            OnGUIAssetType(context, window);
            OnGUIFooterButton(context);
        }

        void OnGUITool(PlateauSandboxContext context)
        {
            if (m_ViewPageIndex != BulkPlaceViewPageIndex.k_FieldSelect)
            {
                return;
            }

            PlateauToolkitEditorGUILayout.Header("ツール");
            DrawFileLoadTool(context);
            DrawCsvTemplateTool(context);
        }

        void DrawFileLoadTool(PlateauSandboxContext context)
        {
            if (m_DataContext.HasLoadedFile())
            {
                if (!m_ButtonView.DrawButton(
                        PlateauSandboxWindowBulkPlaceButtonView.ToolButtonType.k_FileLoaded))
                {
                    return;
                }

                // Clear the data
                m_DataContext.Clear();
                m_HierarchyItem.Clear();
                RefreshTracksHierarchy(context);
            }
            else
            {
                bool isClicked = m_ButtonView.DrawButton(
                    PlateauSandboxWindowBulkPlaceButtonView.ToolButtonType.k_FileNotLoaded);

                m_ButtonView.TryDrawHelperBox(PlateauSandboxWindowBulkPlaceButtonView.ToolButtonType.k_FileNotLoaded);

                if (!isClicked)
                {
                    return;
                }

                // Load the file
                string filePath = EditorUtility.OpenFilePanel("Select File", "", "csv,shp");
                bool isParseSuccess = m_DataContext.TryFileParse(filePath);
                if (isParseSuccess)
                {
                    m_ButtonView.SetInValidIndex(-1);
                    RefreshTracksHierarchy(context);
                }
                else
                {
                    m_ButtonView.SetInValidIndex((int)m_DataContext.FileValidationType);
                }
            }
        }

        void DrawCsvTemplateTool(PlateauSandboxContext context)
        {
            if (!m_ButtonView.DrawButton(
                    PlateauSandboxWindowBulkPlaceButtonView.ToolButtonType.k_CsvTemplate))
            {
                return;
            }

            // Generate CSV Template
            string filePath = EditorUtility.SaveFilePanel("Save File", "", "PlateauSandboxCSVTemplate", "csv");
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            var templateData = new List<PlateauSandboxBulkPlaceDataBase>();
            var data = new PlateauSandboxBulkPlaceCsvData(0, new List<string>()
            {
                "34.9873", "135.7596", "14.23", "イチョウ"
            }, new List<string>()
            {
                "緯度", "経度", "高さ", "アセット種別"
            });
            templateData.Add(data);

            data = new PlateauSandboxBulkPlaceCsvData(1, new List<string>()
            {
                "34.98742", "135.7596", "16.3", "ユリノキ"
            }, new List<string>()
            {
                "緯度", "経度", "高さ", "アセット種別"
            });
            templateData.Add(data);

            bool saveSuccess = new PlateauSandboxFileCsvParser().Save(filePath, templateData);
            if (!saveSuccess)
            {
                return;
            }

            string directoryName = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryName))
            {
                System.Diagnostics.Process.Start(directoryName);
            }
            Debug.Log($"CSVテンプレートを保存しました: {filePath}");
        }

        void OnGUIFieldType(PlateauSandboxContext context)
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

            PlateauToolkitEditorGUILayout.Header("パース対象設定");

            if (m_DataContext.GetFileType() == PlateauSandboxBulkPlaceFileType.k_Csv)
            {
                int currentIndex = m_IsIgnoreHeight ? 1 : 0;
                int selectedIndex = EditorGUILayout.Popup("アセットの配置高さ", currentIndex, m_AssetHeightLabels, GUILayout.Width(340));

                if (selectedIndex != currentIndex)
                {
                    m_IsIgnoreHeight = !m_IsIgnoreHeight;
                }
            }

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
                        m_HierarchyItem.Clear();
                        RefreshTracksHierarchy(context);
                    }
                    GUILayout.FlexibleSpace();
                }
            }

            if (m_DataContext.GetFileType() == PlateauSandboxBulkPlaceFileType.k_Csv)
            {
                DrawLabel(PlateauSandboxBulkPlaceCategory.k_Latitude.Label(),
                    m_DataContext.GetFieldIndex(PlateauSandboxBulkPlaceCategory.k_Latitude));
                DrawLabel(PlateauSandboxBulkPlaceCategory.k_Longitude.Label(),
                    m_DataContext.GetFieldIndex(PlateauSandboxBulkPlaceCategory.k_Longitude));
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
        }

        void OnGUIAssetType(PlateauSandboxContext context, EditorWindow window)
        {
            if (m_ViewPageIndex != BulkPlaceViewPageIndex.k_AssetSelect)
            {
                return;
            }

            GUILayout.Space(10);

            // Add Vertical Scroll,
            using (var scope = new EditorGUILayout.ScrollViewScope(m_AssetScrollPosition))
            {
                PlateauToolkitEditorGUILayout.Header("アセット選択", 15, 0);

                m_TreeView.OnGUI(EditorGUILayout.GetControlRect(false, 100, GUILayout.ExpandHeight(true)));

                GUILayout.Space(10);

                m_AssetContainerView.OnGUI(context, window);

                m_AssetScrollPosition = scope.scrollPosition;
            }
        }

        void OnGUIFooterButton(PlateauSandboxContext context)
        {
            if (!m_DataContext.HasLoadedFile())
            {
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(15);
                if (m_ViewPageIndex == BulkPlaceViewPageIndex.k_FieldSelect)
                {
                    GUILayout.FlexibleSpace();
                    if (m_ButtonView.DrawButton(PlateauSandboxWindowBulkPlaceButtonView.ToolButtonType.k_Next))
                    {
                        m_ViewPageIndex++;
                        m_ButtonView.SetInValidIndex(-1);
                    }
                }
                else
                {
                    if (m_ButtonView.DrawButton(PlateauSandboxWindowBulkPlaceButtonView.ToolButtonType.k_Back))
                    {
                        m_ViewPageIndex--;
                        m_ButtonView.SetInValidIndex(-1);
                    }

                    GUILayout.FlexibleSpace();
                    DrawAssetPlace(context);
                }
                GUILayout.Space(15);
            }

            if (m_ViewPageIndex == BulkPlaceViewPageIndex.k_AssetSelect)
            {
                GUILayout.Space(10);
                using (new EditorGUILayout.HorizontalScope())
                {
                    m_ButtonView.TryDrawHelperBox(PlateauSandboxWindowBulkPlaceButtonView.ToolButtonType.k_AssetNotPlace);
                }
            }
        }

        void DrawAssetPlace(PlateauSandboxContext context)
        {
            if (m_PrefabPlacement?.PlacingCount > 0)
            {
                if (!m_ButtonView.DrawButton(
                        PlateauSandboxWindowBulkPlaceButtonView.ToolButtonType.k_AssetPlacing))
                {
                    return;
                }

                // Stop the Asset placement
                m_Cancellation.Cancel();
                m_Cancellation.Dispose();
                m_Cancellation = null;
                m_PrefabPlacement.StopPlace();

                Debug.Log("アセットの一括配置を停止しました");
            }
            else
            {
                bool isClicked = m_ButtonView.DrawButton(
                                     PlateauSandboxWindowBulkPlaceButtonView.ToolButtonType.k_AssetNotPlace);
                if (!isClicked)
                {
                    return;
                }

                m_ButtonView.SetInValidIndex(-1);

                // Place the assets
                TryPlaceAssets();
            }
        }

        void RefreshTracksHierarchy(PlateauSandboxContext context)
        {
            if (m_TreeView == null)
            {
                DrawTreeView();
                Debug.Assert(m_TreeView != null);
            }

            if (m_HierarchyItem.Count == 0)
            {
                // clear the selection
                m_TreeView.SetSelection(new List<int>());

                if (m_DataContext.HasLoadedFile())
                {
                    m_DataContext.Data
                        .GroupBy(data => data.AssetType)
                        .Select(group => (group.Key, group.Count()))
                        .Select((group, index) => (group, index))
                        .ToList()
                        .ForEach(asset =>
                        {
                            string categoryName = asset.group.Key;
                            int count = asset.group.Item2;
                            m_HierarchyItem.Add(new PlateauSandboxBulkPlaceHierarchyItem()
                            {
                                ID = asset.index,
                                CategoryName = categoryName,
                                Count = count,
                            });
                        });
                }

                foreach (PlateauSandboxBulkPlaceHierarchyItem item in m_HierarchyItem)
                {
                    item.OnClicked.AddListener(() =>
                    {
                        m_SelectedCategoryId = item.ID;
                        if (item.PrefabConstantID > 0)
                        {
                            var selectedPrefab = m_AssetContainerView.GetSelectedAsset(item.PrefabConstantID);
                            if (selectedPrefab != null)
                            {
                                context.SelectPlaceableObject(selectedPrefab);
                            }
                        }
                    });
                }
            }

            m_HierarchyContext.Hierarchy.Items = m_HierarchyItem.ToArray();
            m_TreeView.Reload();
        }

        void DrawTreeView()
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

        void TryPlaceAssets()
        {
            m_PrefabPlacement = new PlateauSandboxPrefabPlacement();
            if (!m_PrefabPlacement.IsValidCityModel())
            {
                m_ButtonView.SetInValidIndex((int)PlateauSandboxWindowBulkPlaceButtonView.AssetPlaceValidationType.k_NoCityModel);
                return;
            }

            if (m_HierarchyItem.All(item => item.PrefabConstantID < 0))
            {
                m_ButtonView.SetInValidIndex((int)PlateauSandboxWindowBulkPlaceButtonView.AssetPlaceValidationType.k_NoAssetSelected);
                return;
            }

            foreach (PlateauSandboxBulkPlaceDataBase placeData in m_DataContext.Data)
            {
                PlateauSandboxBulkPlaceHierarchyItem hierarchyItem = m_HierarchyItem.FirstOrDefault(item => item.CategoryName == placeData.AssetType);
                if (hierarchyItem == null || hierarchyItem.PrefabConstantID < 0)
                {
                    continue;
                }

                var prefab = m_AssetContainerView.GetSelectedAsset(hierarchyItem.PrefabConstantID);
                if (prefab == null)
                {
                    continue;
                }

                try
                {
                    bool isIgnoreHeight = placeData.IsIgnoreHeight | m_IsIgnoreHeight;
                    var context = new PlateauSandboxPrefabPlacement.PlacementContext()
                    {
                        m_Latitude = double.Parse(placeData.Latitude),
                        m_Longitude = double.Parse(placeData.Longitude),
                        m_Height = isIgnoreHeight ? 0 : float.Parse(placeData.Height),
                        m_Prefab = prefab,
                        m_AssetType = placeData.AssetType,
                        m_ObjectId = placeData.ID.ToString(),
                        m_IsIgnoreHeight = isIgnoreHeight,
                        m_IsPlaced = false,
                    };
                    m_PrefabPlacement.AddContext(context);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("配置情報の取得に失敗しました: " + e.Message);
                }
            }

            if (m_PrefabPlacement.PlacingCount == 0)
            {
                Debug.LogWarning("配置できるアセットがありません");
                return;
            }

            Debug.Log($"アセットの一括配置を開始しました。{m_PrefabPlacement.PlacingCount}個の配置");

            if (m_Cancellation == null)
            {
                m_Cancellation = new CancellationTokenSource();
            }
            m_PrefabPlacement.PlaceAllAsync(m_Cancellation.Token);
        }

        public void OnEnd(PlateauSandboxContext context)
        {
            if (m_Cancellation != null)
            {
                m_Cancellation.Cancel();
                m_Cancellation.Dispose();
                m_Cancellation = null;
            }

            m_SelectedCategoryId = -1;
            m_IsIgnoreHeight = false;
            m_ViewPageIndex = BulkPlaceViewPageIndex.k_FieldSelect;
            m_PrefabPlacement = null;

            m_HierarchyItem.Clear();
            m_DataContext.Clear();
        }
    }

    /// <summary>
    /// Class that draws buttons.
    /// </summary>
    class PlateauSandboxWindowBulkPlaceButtonView
    {
        public PlateauSandboxWindowBulkPlaceButtonView(PlateauSandboxBulkPlaceDataContext context)
        {
            m_DataContext = context;
        }

        public enum ToolButtonType
        {
            k_FileLoaded,
            k_FileNotLoaded,
            k_AssetPlacing,
            k_AssetNotPlace,
            k_CsvTemplate,

            k_Next,
            k_Back,
        }

        public enum AssetPlaceValidationType
        {
            k_NoError,
            k_NoAssetSelected,
            k_NoCityModel,
        }

        GUIStyle PrimaryButtonStyle => new GUIStyle(GUI.skin.button)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(40, 40, 8, 8),
        };

        PlateauSandboxBulkPlaceDataContext m_DataContext;
        public int InValidIndex { get; private set; } = -1;

        public bool DrawButton(ToolButtonType type)
        {
            bool isClicked = false;
            switch (type)
            {
                case ToolButtonType.k_FileLoaded:
                    int textLength = m_DataContext.GetFileName().Length;
                    isClicked = new PlateauToolkitImageButtonGUI(
                            200 + (textLength * 6),
                            40,
                            PlateauToolkitGUIStyles.k_ButtonCancelColor).Button($"{m_DataContext.GetFileName()} を読み込み済");
                    break;
                case ToolButtonType.k_FileNotLoaded:
                    isClicked = new PlateauToolkitImageButtonGUI(
                        220,
                        40,
                        PlateauToolkitGUIStyles.k_ButtonPrimaryColor).Button($"SHP、CSVファイルを読み込む");
                    break;
                case ToolButtonType.k_AssetPlacing:
                    isClicked = new PlateauToolkitImageButtonGUI(
                        220,
                        40,
                        PlateauToolkitGUIStyles.k_ButtonCancelColor,
                        false).Button($"アセットを配置中です...");
                    break;
                case ToolButtonType.k_AssetNotPlace:
                    isClicked = new PlateauToolkitImageButtonGUI(
                        220,
                        40,
                        PlateauToolkitGUIStyles.k_ButtonPrimaryColor,
                        false).Button($"アセットを配置");
                    break;
                case ToolButtonType.k_CsvTemplate:
                    EditorGUILayout.Space(10);
                    isClicked = new PlateauToolkitImageButtonGUI(
                        220,
                        40,
                        PlateauToolkitGUIStyles.k_ButtonPrimaryColor).Button($"CSVテンプレートの生成");
                    break;
                case ToolButtonType.k_Next:
                    isClicked = new PlateauToolkitImageButtonGUI(
                        220,
                        40,
                        PlateauToolkitGUIStyles.k_ButtonPrimaryColor,
                        false).Button($"次へ");
                    break;
                case ToolButtonType.k_Back:
                    isClicked = new PlateauToolkitImageButtonGUI(
                        220,
                        40,
                        PlateauToolkitGUIStyles.k_ButtonPrimaryColor,
                        false).Button($"戻る");
                    break;
            }
            return isClicked;
        }

        public void TryDrawHelperBox(ToolButtonType buttonType)
        {
            if (InValidIndex < 0)
            {
                return;
            }

            switch (buttonType)
            {
                case ToolButtonType.k_FileNotLoaded:
                    switch (InValidIndex)
                    {
                        case (int)PlateauSandboxFileParserValidationType.k_NotExistsFile:
                            EditorGUILayout.HelpBox("CSVもしくはSHPファイルが見つかりませんでした。", MessageType.Error);
                            break;
                        case (int)PlateauSandboxFileParserValidationType.k_AccessControl:
                            EditorGUILayout.HelpBox("ファイルにアクセスできませんでした。", MessageType.Error);
                            break;
                        case (int)PlateauSandboxFileParserValidationType.k_FileOpened:
                            EditorGUILayout.HelpBox("ファイルが開かれているためアクセスできませんでした。", MessageType.Error);
                            break;
                        case (int)PlateauSandboxFileParserValidationType.k_NotExistsDbfFile:
                            EditorGUILayout.HelpBox("DBFファイルが見つかりませんでした。", MessageType.Error);
                            break;
                    }
                    break;
                case ToolButtonType.k_AssetNotPlace:
                    switch (InValidIndex)
                    {
                        case (int)AssetPlaceValidationType.k_NoAssetSelected:
                            EditorGUILayout.HelpBox("プレハブを設定してください", MessageType.Warning);
                            break;
                        case (int)AssetPlaceValidationType.k_NoCityModel:
                            EditorGUILayout.HelpBox("配置範囲内に3D都市モデルが存在しません", MessageType.Warning);
                            break;
                    }
                    break;
            }
        }

        public void SetInValidIndex(int index)
        {
            InValidIndex = index;
        }
    }
}