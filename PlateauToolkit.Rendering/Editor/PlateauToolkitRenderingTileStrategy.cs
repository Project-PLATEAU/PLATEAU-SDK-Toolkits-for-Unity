using PLATEAU.CityInfo;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.DynamicTile;
using PLATEAU.Editor.Window.Common.Tile;
using PLATEAU.GranularityConvert;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Rendering.Editor
{
    public interface IPlateauToolkitRenderingStrategy
    {
        SceneTileChooserType SelectedType { get; }
        void Draw();
        void AutoTexture();
    }

    public class PlateauToolkitRenderingTileStrategy : IPlateauToolkitRenderingStrategy
    {
        PlateauToolkitRenderingWindow m_ParentWindow;

        SceneTileChooserImgui m_SceneTileChooser;
        TileListElement m_TileListElement;
        TileListElementData m_TileListElementData;

        TileRebuilder m_TileRebuilder;

        // Auto texturing
        AutoTexturing m_AutoTextureProcessor;
        // LOD grouping
        Grouping m_Grouping;

        public SceneTileChooserType SelectedType => m_SceneTileChooser?.SelectedType ?? SceneTileChooserType.SceneObject;

        public bool IsTileManagerSelected => m_SceneTileChooser?.SelectedType == SceneTileChooserType.DynamicTile && m_TileListElementData != null && m_TileListElementData.TileManager != null;

        internal PlateauToolkitRenderingTileStrategy(PlateauToolkitRenderingWindow editorWindow, AutoTexturing autoTextureProcessor, Grouping grouping)
        {
            if (m_TileListElementData == null)
            {
                m_TileListElementData = new TileListElementData(editorWindow);
                m_TileListElementData.EnableTileHierarchy = false; // 子要素選択不可
            }

            if (m_SceneTileChooser == null)
            {
                m_SceneTileChooser = new SceneTileChooserImgui();
            }

            if (m_TileListElement == null)
            {
                m_TileListElement = new TileListElement(m_TileListElementData);
            }

            m_ParentWindow = editorWindow;
            m_AutoTextureProcessor = autoTextureProcessor;
            m_Grouping = grouping;
        }

        public void Draw()
        {
            // タイル選択UIの表示
            m_SceneTileChooser.DrawAndInvoke(
                () => { }, // シーンタイル選択時の処理は不要
                () =>
                {
                    // 動的タイルを選択した場合の処理
                    if (m_TileListElementData.TileManager == null)
                    {
                        m_TileListElementData.TileManager = UnityEngine.Object.FindObjectOfType<PLATEAUTileManager>();
                    }
                    m_TileListElementData.TileManager =
                        (PLATEAUTileManager)EditorGUILayout.ObjectField(
                            "調整対象", m_TileListElementData.TileManager,
                            typeof(PLATEAUTileManager), true);
                    if (m_TileListElementData.TileManager != null)
                    {
                        m_TileListElement.DrawContent();
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("シーン上にタイルマネージャーが見つかりませんでした。", MessageType.Warning);
                    }
                });
        }

        public void AutoTexture()
        {
            AutoTextureAsync().ContinueWithErrorCatch();
        }

        public async Task AutoTextureAsync()
        {
            using (var cts = new CancellationTokenSource())
            {
                try
                {
                    CancellationToken ct = cts.Token;
                    m_TileRebuilder = new TileRebuilder();
                    PLATEAUTileManager tileManager = m_TileListElementData.TileManager;
                    ObservableCollection<TileSelectionItem> selectedBuildingTiles = TileConvertCommon.FilterByPackage(PLATEAU.Dataset.PredefinedCityModelPackage.Building, m_TileListElementData.ObservableSelectedTiles, tileManager); // 建物地物に絞り込み

                    List<PLATEAUDynamicTile> selectedTiles = TileConvertCommon.GetSelectedTiles(selectedBuildingTiles, tileManager);
                    Transform editingTile = await TileConvertCommon.GetEditableTransformParent(selectedBuildingTiles, tileManager, m_TileRebuilder, ct);
                    List<Transform> tileTransforms = TileConvertCommon.GetEditableTransforms(selectedBuildingTiles, editingTile);

                    if (tileTransforms.Count > 0)
                    {
                        bool isOptionSelected = EditorUtility.DisplayDialog(
                               "テクスチャ作成の確認",
                               "テクスチャを生成します。必要に応じてHierarchy にある地物の構成が変更されることもあります。実行しますか？",
                               "はい",
                               "いいえ"
                           );
                        if (isOptionSelected)
                        {
                            m_ParentWindow.BlockUI();
#if PLATEAU_SDK_222
                            // 主要地物に変換します。
                            PLATEAU.CityImport.Import.Convert.GranularityConvertResult result = await new CityGranularityConverter().ConvertAsync(new GranularityConvertOptionUnity(new GranularityConvertOption(ConvertGranularity.PerPrimaryFeatureObject, 1), new UniqueParentTransformList(tileTransforms), true));
                            tileTransforms = TileConvertCommon.GetEditableTransforms(selectedBuildingTiles, editingTile); // 変換後の Tile を再取得
#endif
                            List<GameObject> processsingGameObjects = GetChildGameObjects(tileTransforms); // 変換後の主要地物オブジェクトをすべて取得
                            if (PlateauRenderingBuildingUtilities.GetMeshStructure(processsingGameObjects[0]) != PlateauMeshStructure.CombinedArea && !processsingGameObjects[0].name.Contains(PlateauRenderingConstants.k_Grouped))
                            {
                                m_Grouping.GroupObjectsInTransform(editingTile);
                            }
                            var tcs = new TaskCompletionSource<bool>();
                            void OnProcessingFinishedHandler()
                            {
                                // 処理完了時のコールバック
                                m_ParentWindow.UnblockUI();
                                m_AutoTextureProcessor.OnProcessingFinished -= OnProcessingFinishedHandler;
                                tcs.TrySetResult(true);
                            }
                            m_AutoTextureProcessor.OnProcessingFinished += OnProcessingFinishedHandler;

                            m_AutoTextureProcessor.RunOptimizeProcess(processsingGameObjects); // 処理開始

                            await tcs.Task; // 完了待ち
                        }
                    }

                    //tileTransforms.Clear();
                    //tileTransforms = TileConvertCommon.GetEditableTransforms(selectedBuildingTiles, editingTile, true); // Tileとして取得し直す
                    await TileConvertCommon.SavePrefabAssets(tileTransforms, m_TileRebuilder, ct);
                    await m_TileRebuilder.RebuildByTiles(tileManager, selectedTiles);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    m_TileRebuilder?.CancelRebuild();
                }
            }
        }

        /// <summary>
        /// 子要素の PLATEAUCityObjectGroup をすべて取得する
        /// </summary>
        /// <param name="tileTransform"></param>
        /// <returns></returns>
        public List<GameObject> GetChildGameObjects(List<Transform> tileTransform)
        {
            var selectedGameObjects = new List<GameObject>();
            foreach (Transform trans in tileTransform)
            {
                PLATEAUCityObjectGroup[] children = trans.GetComponentsInChildren<PLATEAUCityObjectGroup>();
                foreach (PLATEAUCityObjectGroup child in children)
                {
                    selectedGameObjects.Add(child.gameObject);
                }
            }
            return selectedGameObjects;
        }
    }
}
