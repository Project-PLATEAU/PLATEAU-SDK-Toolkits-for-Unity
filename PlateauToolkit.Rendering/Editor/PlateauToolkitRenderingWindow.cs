using PlateauToolkit.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_URP
using UnityEngine.Rendering.Universal;
#endif
#if UNITY_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace PlateauToolkit.Rendering.Editor
{
    public class PlateauToolkitRenderingWindow : EditorWindow
    {
        public PlateauToolkitRenderingWindow m_Window;

        readonly List<GameObject> m_SelectedObjects = new List<GameObject>();

        // Auto texturing
        AutoTexturing m_AutoTextureProcessor;

        // LOD grouping
        Grouping m_Grouping;
        CreateLodGroup m_CreateLodGroup;

        // Environment system
        GameObject m_EnvPrefab;
        GameObject m_EnvPrefabMobile;
        string m_SkyboxName = string.Empty;
        Material m_NewSkybox;
        GameObject m_EnvVolumeUrp;
        Cubemap m_EnvSpaceEmission;

        EnvironmentController m_SelectedEnvironment;
        EnvironmentControllerEditor m_EnvEditor;
        HideInHierarchyController m_HideInHierarchyEditor;

        int m_MaskPercentage = 20;
        int m_RandomAlphaSeed;

        // UI blocker when a process is running
        bool m_BlockUI = false;

        enum Tab
        {
            LODGrouping,
            Shader,
            Environment,
        }

        Tab m_CurrentTab;

        void OnEnable()
        {
            InitializePaths();
            m_CurrentTab = Tab.Environment;

            SceneView scene = SceneView.lastActiveSceneView;
            if (scene != null)
            {
                scene.sceneViewState.alwaysRefresh = true;
            }
        }

        void InitializePaths()
        {
#if UNITY_URP
            if (m_EnvPrefab == null)
            {
                m_EnvPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath(PlateauToolkitRenderingPaths.k_EnvPrefabUrp, typeof(GameObject)) as GameObject;
            }

            if(m_EnvPrefabMobile == null)
            {
                m_EnvPrefabMobile = UnityEditor.AssetDatabase.LoadAssetAtPath(PlateauToolkitRenderingPaths.k_EnvPrefabMobile, typeof(GameObject)) as GameObject;
            }

            m_SkyboxName = PlateauRenderingConstants.k_SkyboxNewShaderName;

            if (m_NewSkybox == null)
            {
                m_NewSkybox = (Material)AssetDatabase.LoadAssetAtPath(PlateauToolkitRenderingPaths.k_SkyboxUrp, typeof(Material));
            }

            if (m_EnvVolumeUrp == null)
            {
                m_EnvVolumeUrp = UnityEditor.AssetDatabase.LoadAssetAtPath(PlateauToolkitRenderingPaths.k_EnvVolumeUrp, typeof(GameObject)) as GameObject;
            }
#elif UNITY_HDRP
            if (m_EnvPrefab == null)
            {
                m_EnvPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath(PlateauToolkitRenderingPaths.k_EnvPrefabHdrp, typeof(GameObject)) as GameObject;
            }

            if (m_EnvSpaceEmission == null)
            {
                m_EnvSpaceEmission = UnityEditor.AssetDatabase.LoadAssetAtPath(PlateauToolkitRenderingPaths.k_EnvSpaceEmission, typeof(Cubemap)) as Cubemap;
            }
#endif

            if (m_Grouping == null)
            {
                m_Grouping = new Grouping();
                m_Grouping.OnProcessingFinished += UnblockUI;
            }

            if (m_CreateLodGroup == null)
            {
                m_CreateLodGroup = new CreateLodGroup();
                m_CreateLodGroup.OnProcessingFinished += UnblockUI;
            }

            if (m_AutoTextureProcessor == null)
            {
                m_AutoTextureProcessor = new AutoTexturing();
                m_AutoTextureProcessor.Initialize();
                m_AutoTextureProcessor.OnProcessingFinished += UnblockUI;
            }
        }

        void OnGUI()
        {
            int mediumButtonWidth = 53;
            int mediumButtonHeight = 53;
            #region Header
            m_Window ??= GetWindow<PlateauToolkitRenderingWindow>();
            PlateauToolkitEditorGUILayout.HeaderLogo(m_Window.position.width);

            #endregion

            if (m_BlockUI)
            {
                return;
            }

            InitializePaths();

            #region Rendering feature tabs

            var imageButtonGUILayout = new PlateauToolkitRenderingGUILayout.PlateauRenderingEditorImageButtonGUILayout(
              mediumButtonWidth,
            mediumButtonHeight);

            bool TabButton(string iconPath, Tab tab)
            {
                Color? buttonColor = tab == m_CurrentTab ? Color.cyan : null;
                if (imageButtonGUILayout.Button(iconPath, buttonColor))
                {
                    m_CurrentTab = tab;
                    return true;
                }

                return false;
            }

            PlateauToolkitRenderingGUILayout.GridLayout(
                m_Window.position.width,
                mediumButtonWidth,
                new Action[]
                {
                    () =>
                    {
                         if (TabButton(PlateauToolkitRenderingPaths.k_EnvIcon, Tab.Environment))
                        {
                        }
                    },
                    () =>
                    {
                        if (TabButton(PlateauToolkitRenderingPaths.k_ShaderIcon, Tab.Shader))
                        {

                        }
                    },
                    () =>
                    {
                         if (TabButton(PlateauToolkitRenderingPaths.k_LodIcon, Tab.LODGrouping))
                        {

                        }
                    }
                });

            switch (m_CurrentTab)
            {
                case Tab.LODGrouping:
                    PlateauToolkitRenderingGUILayout.Header("LODグループ生成");
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("カメラからの距離にしたがって表示される地物の詳細が変動します。遠くにあるものを簡易表示したり非表示することによってパフォーマンスが向上します。", MessageType.Info);
                    EditorGUILayout.Space();

                    if (GUILayout.Button("LODグループ生成"))
                    {
                        bool isOptionSelected = EditorUtility.DisplayDialog(
                               "LODグループ生成の確認",
                               "シーンのオブジェクトが変更されます。実行しますか？",
                               "はい",
                               "いいえ"
                           );

                        if (isOptionSelected)
                        {
                            m_BlockUI = true;
                            m_Grouping.TrySeparateMeshes();
                            m_Grouping.GroupObjects();
                            m_CreateLodGroup.CreateLodGroups();
                        }
                    }
                    break;
                case Tab.Shader:
                    PlateauToolkitRenderingGUILayout.Header("自動テクスチャ生成");
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("建物のテクスチャを自動的に生成します。実在する建物の見た目と異なる場合があります。", MessageType.Info);
                    EditorGUILayout.Space();

                    EditorGUI.BeginChangeCheck();
                    if (GUILayout.Button("テクスチャ生成"))
                    {
                        if (!SelectedObjectsExist())
                        {
                            EditorUtility.DisplayDialog(
                                "オブジェクト選択の確認",
                                "少なくともオブジェクトを一つ選択してください。",
                                "OK"
                                );
                        }
                        else
                        {
                            bool isOptionSelected = EditorUtility.DisplayDialog(
                                   "テクスチャ作成の確認",
                                   "選択された地物にテクスチャを生成します。必要に応じてHierarchy にある地物の構成が変更されることもあります。実行しますか？",
                                   "はい",
                                   "いいえ"
                               );

                            if (isOptionSelected)
                            {
                                m_BlockUI = true;
                                if (PlateauRenderingBuildingUtilities.GetMeshStructure(m_SelectedObjects[0]) != PlateauMeshStructure.CombinedArea && !m_SelectedObjects[0].name.Contains(PlateauRenderingConstants.k_Grouped))
                                {
                                    m_Grouping.GroupObjects();
                                }
                                m_AutoTextureProcessor.RunOptimizeProcess(m_SelectedObjects);
                            }
                        }
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("自動生成されたテクスチャの窓を表示または非表示します。LOD２のみに有効です。", MessageType.Info);
                    EditorGUILayout.Space();

                    if (GUILayout.Button("窓の表示の切り替え"))
                    {
                        if (!SelectedObjectsExist())
                        {
                            EditorUtility.DisplayDialog(
                               "オブジェクト選択の確認",
                               "少なくとも有効なオブジェクトを一つ選択してください。",
                               "OK"
                               );
                        }
                        else
                        {
                            foreach (GameObject building in m_SelectedObjects)
                            {
                                if (IsLod2(building))
                                {
                                    PlateauRenderingBuildingUtilities.SetWindowFlag(building, !PlateauRenderingBuildingUtilities.GetWindowFlag(building));
                                }
                            }
                        }
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("選択地物の窓用頂点カラーマスク（Gチャンネル）を調整します。各地物にはランダムな頂点アルファ値が割り当てられます。", MessageType.Info);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("地物上部からX％マスク (頂点カラーG)", GUILayout.Width(200));
                    m_MaskPercentage = EditorGUILayout.IntSlider(m_MaskPercentage, 0, 100);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("ランダムシード値 (頂点アルファ )", GUILayout.Width(200));
                    m_RandomAlphaSeed = EditorGUILayout.IntField(m_RandomAlphaSeed);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();

                    if (GUILayout.Button("頂点カラーの調整"))
                    {
                        m_SelectedObjects.Clear();
                        PlateauRenderingMeshUtilities.GetSelectedGameObjects(m_SelectedObjects);

                        if (m_SelectedObjects.Count == 0)
                        {
                            return;
                    }

                        int currentSeed = m_RandomAlphaSeed;
                        foreach (GameObject building in m_SelectedObjects)
                        {
                            MeshFilter meshFilter = building.GetComponent<MeshFilter>();
                            if (meshFilter != null)
                            {
                                Mesh mesh = meshFilter.sharedMesh;
                                Bounds boundingBox = mesh.bounds;
                                PlateauRenderingBuildingUtilities.SetBuildingVertexColorForWindow(mesh, boundingBox, building, m_MaskPercentage / 100f, currentSeed);
                                currentSeed++;
                            }
                        }
                    }
                    break;

                #region Environment
                case Tab.Environment:
                    PlateauToolkitRenderingGUILayout.Header("環境システムの設定");
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("シーンに昼と夜の明るさを調整したり雨のような天気の設定を行うことができます。Time of day を変更することによって１日における日光の方向調整ができます。", MessageType.Info);
                    EditorGUILayout.Space();

                    m_SelectedEnvironment = FindFirstObjectByType<EnvironmentController>();

                    // select the environment controller in the scene
                    if (m_SelectedEnvironment != null)
                    {
                        m_EnvEditor = (EnvironmentControllerEditor)UnityEditor.Editor.CreateEditor(m_SelectedEnvironment);
                        m_EnvEditor.OnInspectorGUI();
                        EditorGUILayout.Space();
                    }
                    else
                    {
                        // if there is no environment controller object in the scene
                        if (GUILayout.Button("環境要素"))
                        {
                            if (m_EnvPrefab != null && FindFirstObjectByType<EnvironmentController>() == null)
                            {
                                // If there is default directional light, disable it. Only run this once when loading the environment system.
                                Light[] directionalLights = FindObjectsByType<Light>(FindObjectsSortMode.None);

                                // Disable the default directional light
                                foreach (Light light in directionalLights)
                                {
                                    if (light.type == LightType.Directional)
                                    {
                                        light.gameObject.SetActive(false);
                                    }
                                }

                                if (FindFirstObjectByType<EnvironmentController>() == null)
                                {
#if UNITY_URP
                                    if(m_EnvPrefabMobile == null || m_EnvPrefab == null || m_EnvVolumeUrp == null)
                                    {
                                        Debug.LogError("Environment prefab is missing.");
                                        return;
                                    }

                                    // Show dialogue to confirm if user wants to use prefab for mobile
                                    bool isOptionSelected = EditorUtility.DisplayDialog(
                                        "Mobile Environment System",
                                        "モバイル用のパーティクルシステムを使用しますか？",
                                        "はい",
                                        "いいえ"
                                    );

                                    GameObject env = isOptionSelected? Instantiate(m_EnvPrefabMobile) : Instantiate(m_EnvPrefab);
                                    env.name = "Environment";

                                    // Replace skybox
                                    Material skybox = RenderSettings.skybox;
                                    if (skybox == null || skybox.shader.name != m_SkyboxName)
                                    {
                                        RenderSettings.skybox = m_NewSkybox;
                                        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
                                    }

                                    // Instantiate k_EnvVolumeUrp prefab
                                    GameObject envVolume = Instantiate(m_EnvVolumeUrp);
                                    envVolume.name = "Environment Volume";
#endif
#if UNITY_HDRP
                                    GameObject env = Instantiate(m_EnvPrefab);
                                    env.name = "Environment";

                                    GameObject skyAndFogVolumeObject = GameObject.Find("Sky and Fog Volume");
                                    if (skyAndFogVolumeObject != null)
                                    {
                                        Volume skyAndFogVolume = skyAndFogVolumeObject.GetComponent<Volume>();
                                        if (skyAndFogVolume != null)
                                        {
                                            VolumeProfile profile = skyAndFogVolume.sharedProfile;
                                            if (profile.TryGet<PhysicallyBasedSky>(out PhysicallyBasedSky physicallyBasedSky))
                                            {
                                                physicallyBasedSky.groundTint.overrideState = true;
                                                physicallyBasedSky.groundTint.value = new Color(0.453f, 0.540f, 0.594f, 1.0f);
                                                physicallyBasedSky.spaceEmissionTexture.overrideState = true;
                                                physicallyBasedSky.spaceEmissionTexture.value = m_EnvSpaceEmission;
                                                physicallyBasedSky.spaceEmissionMultiplier.overrideState = true;
                                                physicallyBasedSky.spaceEmissionMultiplier.value = 2.0f; // Adjust this value as needed
                                            }

                                            // sef default fog values
                                            if (profile.TryGet<Fog>(out Fog fog))
                                            {
                                                fog.colorMode.overrideState = true;
                                                fog.tint.overrideState = true;
                                                fog.tint.value = new Color(2.5f, 2.5f, 2.5f, 1.0f);
                                                fog.maximumHeight.overrideState = true;
                                                fog.maximumHeight.value = 700.0f;
                                                fog.enableVolumetricFog.overrideState = true;
                                            }
                                        }
                                    }
#endif
                                }
                            }
                        }
                    }
                    break;
                #endregion

            }
            #endregion
        }

        /// <summary>
        /// Check that the selected objects are Plateau objects. If a non texturable Plateau object is included, we remove that from the selected list and only
        /// texture the remaining Plateau objects.
        /// </summary>
        /// <returns></returns>
        bool SelectedObjectsExist()
        {
            m_SelectedObjects.Clear();
            PlateauRenderingMeshUtilities.GetSelectedGameObjects(m_SelectedObjects);

            for (int i = m_SelectedObjects.Count - 1; i >= 0; i--)
            {
                if (!PlateauRenderingBuildingUtilities.IsPlateauBuilding(m_SelectedObjects[i].transform))
                {
                    m_SelectedObjects.RemoveAt(i);
                }
            }

            return m_SelectedObjects.Count > 0;
        }

        public bool IsLod2(GameObject target)
        {
            if (target.name.Contains("LOD2") || target.transform.parent.name.Contains("LOD2"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal void UnblockUI()
        {
            m_BlockUI = false;
            Repaint();
        }
    }
}