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
        void CreateTexture();

        void AddConvertedObject(GameObject obj);
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

        List<GameObject> m_ConvertedObjectsInTile;

        public SceneTileChooserType SelectedType => m_SceneTileChooser?.SelectedType ?? SceneTileChooserType.SceneObject;

        public bool IsTileManagerSelected => m_SceneTileChooser?.SelectedType == SceneTileChooserType.DynamicTile && m_TileListElementData != null && m_TileListElementData.TileManager != null;

        internal PlateauToolkitRenderingTileStrategy(PlateauToolkitRenderingWindow editorWindow)
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
            if (m_Grouping == null)
            {
                m_Grouping = new Grouping();
            }

            if (m_AutoTextureProcessor == null)
            {
                m_AutoTextureProcessor = new AutoTexturing();
                m_AutoTextureProcessor.Initialize();
            }

            m_ConvertedObjectsInTile = new List<GameObject>();
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

        /// <summary>
        /// 解像度スケール/画素パラメータをコピーする前に、オブジェクトのリストをリセットします。
        /// </summary>
        public void ResetConvertedObjectsInTile()
        {
            m_ConvertedObjectsInTile.Clear();
        }

        /// <summary>
        /// 解像度スケール/画素パラメータをコピーした際に、タイル内の変換済みオブジェクトをリストに追加します。
        /// </summary>
        /// <param name="obj"></param>
        public void AddConvertedObject(GameObject obj)
        {
            if (obj.GetComponentInParent<PLATEAUTileManager>() != null)
            {
                if (!m_ConvertedObjectsInTile.Contains(obj))
                {
                    m_ConvertedObjectsInTile.Add(obj);
                }
            }
        }

        /// <summary>
        /// 解像度スケール/画素パラメータをコピー時に、タイル内に変換済みオブジェクトが存在するかどうかを返します。
        /// </summary>
        /// <returns></returns>
        public bool HasConvertedObjectsInTile => m_ConvertedObjectsInTile?.Count > 0;

        /// <summary>
        /// テクスチャ生成処理開始
        /// </summary>
        public void CreateTexture()
        {
            AutoTextureAsync().ContinueWithErrorCatch();
        }

        async Task AutoTextureAsync()
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

#if PLATEAU_SDK_222

                            IEnumerable<Transform> convertibleTiles = tileTransforms.Where(t => Enumerable.Range(0, t.childCount)
                              .Select(i => t.GetChild(i).name).All(n => n.StartsWith("LOD"))); // タイル直下の子が全てLODであるタイルのみ抽出 (改造構造が変わっていると変換できない）
                            if (convertibleTiles.Any())
                            {
                                // 主要地物に変換します。
                                PLATEAU.CityImport.Import.Convert.GranularityConvertResult result = await new CityGranularityConverter().ConvertAsync(
                                    new GranularityConvertOptionUnity(
                                        new GranularityConvertOption(ConvertGranularity.PerPrimaryFeatureObject, 1), new UniqueParentTransformList(convertibleTiles), true));
                                tileTransforms = TileConvertCommon.GetEditableTransforms(selectedBuildingTiles, editingTile); // 変換後の Tile を再取得
                            }
#endif
                            foreach (Transform transform in tileTransforms)
                            {
                                List<GameObject> processsingGameObjects = GetChildGameObjects(transform);
                                if (PlateauRenderingBuildingUtilities.GetMeshStructure(processsingGameObjects.First()) != PlateauMeshStructure.CombinedArea && !processsingGameObjects.First().name.Contains(PlateauRenderingConstants.k_Grouped))
                                {
                                    m_Grouping.GroupObjectsInTransform(transform);
                                    processsingGameObjects.RemoveAll(go => go == null);
                                }

                                var tcs = new TaskCompletionSource<bool>();
                                void OnProcessingFinishedHandler()
                                {
                                    // 処理完了時のコールバック
                                    m_AutoTextureProcessor.OnProcessingFinished -= OnProcessingFinishedHandler;
                                    tcs.TrySetResult(true);
                                }

                                m_AutoTextureProcessor.OnProcessingFinished += OnProcessingFinishedHandler;
                                m_AutoTextureProcessor.RunOptimizeProcess(processsingGameObjects); // 処理開始
                                await tcs.Task; // 完了待ち
                            }
                        }

                        tileTransforms = TileConvertCommon.GetEditableTransforms(selectedBuildingTiles, editingTile, true); // Tileとして取得し直す
                        await TileConvertCommon.SavePrefabAssets(tileTransforms, m_TileRebuilder, ct);
                        await m_TileRebuilder.RebuildByTiles(tileManager, selectedTiles);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                        m_TileRebuilder?.CancelRebuild();
                    }
                    finally
                    {
                        m_ParentWindow.UnblockUI();
                    }
                }
            }
        }

        internal void DrawImageFilterSaveTileButton(TextureEnhance textureEnhance, ComputeShader computeShader)
        {

            if (!HasConvertedObjectsInTile)
            {
                return;
            }

            bool isOptionSelected = EditorUtility.DisplayDialog(
                "タイル保存の確認",
                "変更されたテクスチャを含むタイルを保存しなおします。実行しますか？",
                "はい",
                "いいえ"
            );

            //EditorGUILayout.Space(5);
            //if (GUILayout.Button("選択したオブジェクトのタイルを保存する"))
            if (isOptionSelected)
            {
                SaveSelectedTilesImageFilterAsync(textureEnhance, computeShader).ContinueWithErrorCatch();
            }

            ResetConvertedObjectsInTile();
        }

        async Task SaveSelectedTilesImageFilterAsync(TextureEnhance textureEnhance, ComputeShader computeShader)
        {
            m_ParentWindow.BlockUI();

            try
            {
                //List<GameObject> selectedObjects = new List<GameObject>(Selection.gameObjects);
                using (var cts = new CancellationTokenSource())
                {
                    CancellationToken ct = cts.Token;
                    m_TileRebuilder = new TileRebuilder();
                    await TileConvertCommon.EditAndSaveSelectedTilesAsync<(TextureEnhance, ComputeShader)>(m_ConvertedObjectsInTile, m_TileRebuilder, ApplyFilterHandler, (textureEnhance, computeShader), ct);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                m_TileRebuilder?.CancelRebuild();
            }
            finally
            {
                m_ParentWindow.UnblockUI();
            }
        }

        void ApplyFilterHandler(Transform target, (TextureEnhance, ComputeShader) param )
        {
            (TextureEnhance textureEnhance, ComputeShader computeShader) = param;
            List<Transform> children = target.GetAllChildrenWithComponent<MeshRenderer>();
            foreach (Transform child in children)
            {
                GameObject targetGameObject = child.gameObject;
                if (PlateauRenderingBuildingUtilities.IsObjectAutoTextured(targetGameObject))
                {
                    continue;
                }

                if (PlateauRenderingBuildingUtilities.GetMeshLodLevel(targetGameObject) != PlateauMeshLodLevel.Lod2)
                {
                    continue;
                }

                m_ParentWindow.ApplyImageOperationsForObject(targetGameObject, textureEnhance, computeShader);
            }
        }

        internal void DrawImageScalingSaveTileButton(TextureDownscaleRatio scaleRatio)
        {
            Debug.Log("DrawImageScalingSaveTileButton called");

            if (!HasConvertedObjectsInTile)
            {
                return;
            }

            bool isOptionSelected = EditorUtility.DisplayDialog(
                "タイル保存の確認",
                "変更されたテクスチャを含むタイルを保存しなおします。実行しますか？",
                "はい",
                "いいえ"
            );

            //EditorGUILayout.Space(5);
            //if (GUILayout.Button("選択したオブジェクトのタイルを保存する"))
            if (isOptionSelected)
            {
                SaveSelectedTilesImageScalingAsync(scaleRatio).ContinueWithErrorCatch();
            }

            ResetConvertedObjectsInTile();
        }

        async Task SaveSelectedTilesImageScalingAsync(TextureDownscaleRatio scaleRatio)
        {
            m_ParentWindow.BlockUI();

            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    CancellationToken ct = cts.Token;
                    m_TileRebuilder = new TileRebuilder();
                    await TileConvertCommon.EditAndSaveSelectedTilesAsync<TextureDownscaleRatio>(m_ConvertedObjectsInTile, m_TileRebuilder, ApplyImageScalingHandler, scaleRatio, ct);
                }
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
                m_TileRebuilder?.CancelRebuild();
            }
            finally
            {
                m_ParentWindow.UnblockUI();
            }
        }

        void ApplyImageScalingHandler(Transform target, TextureDownscaleRatio scaleRatio)
        {
            List<Transform> children = target.GetAllChildrenWithComponent<MeshRenderer>();
            foreach (Transform child in children)
            {
                GameObject targetGameObject = child.gameObject;
                if (PlateauRenderingBuildingUtilities.IsObjectAutoTextured(targetGameObject))
                {
                    continue;
                }

                if (PlateauRenderingBuildingUtilities.GetMeshLodLevel(targetGameObject) != PlateauMeshLodLevel.Lod2)
                {
                    continue;
                }

                m_ParentWindow.ApplyImageScalingForObject(targetGameObject, scaleRatio);
            }
        }

        /// <summary>
        /// 子要素の PLATEAUCityObjectGroup をすべて取得する
        /// </summary>
        /// <param name="tileTransform"></param>
        /// <returns></returns>
        public List<GameObject> GetChildGameObjects(Transform tileTransform)
        {
            var selectedGameObjects = new List<GameObject>();
            PLATEAUCityObjectGroup[] children = tileTransform.GetComponentsInChildren<PLATEAUCityObjectGroup>();
            foreach (PLATEAUCityObjectGroup child in children)
            {
                selectedGameObjects.Add(child.gameObject);
            }
            return selectedGameObjects;
        }
    }
}
