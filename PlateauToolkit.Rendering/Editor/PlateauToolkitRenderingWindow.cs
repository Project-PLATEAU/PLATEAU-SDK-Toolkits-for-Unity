using PLATEAU.CityInfo;
using PLATEAU.GranularityConvert;
using PlateauToolkit.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using PLATEAU.PolygonMesh;
using PLATEAU.Util.Async;
using PlateauToolkit.Rendering.ImageProcessing;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;
#if UNITY_URP
using UnityEngine.Rendering.Universal;
#endif
#if UNITY_HDRP
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
#endif

namespace PlateauToolkit.Rendering.Editor
{
    public enum ImageEditParameters
    {
        Contrast,
        Brightness,
        HighPass,
        Sharpness,
        None
    }

    public enum ImageEditState
    {
        None,
        FilterEditing,
        Scaling
    }

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

        // Texture improvements
        TextureEnhance m_TextureEnhance;
        int m_Contrast;
        int m_Brightness;
        int m_Sharpness;
        int m_HighPass;

        int m_ContrastPrevious;
        int m_BrightnessPrevious;
        int m_SharpnessPrevious;
        int m_HighPassPrevious;

        IPlateauRenderingImageFilter m_ContrastFilter;
        IPlateauRenderingImageFilter m_BrightnessFilter;
        IPlateauRenderingImageFilter m_HpfFilter;
        IPlateauRenderingImageFilter m_SharpnessFilter;
        string m_ComputeShaderPath = PlateauToolkitRenderingPaths.k_ImageProcessingComputeShader;

        int m_InitialMaterialIndex;
        Material m_InitialMaterial;
        Material m_PreviewMaterial;
        Renderer m_TargetRenderer;
        Texture m_InitialTexture;
        Texture m_InitialTextureResized;
        ComputeShader m_ComputeShader;

        TextureDownscaleRatio m_TextureDownscaleRatio;
        TextureDownscaleRatio m_TextureDownscaleRatioPrevious;
        ImageEditState m_ImageEditState;

        GUIStyle m_HeaderTextStyle;

        Vector2 m_ScrollPosition;

        bool m_IsConvertingPlateauModel;

#if PLATEAU_SDK_222
        MeshGranularity m_MeshGranularity;
        bool m_TextureMerged;
#endif

        enum Tab
        {
            LodGrouping,
            Shader,
            Environment,
            TextureTuner
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

            m_ImageEditState = ImageEditState.None;

            m_HighPass = PlateauRenderingConstants.k_HighPassDefaultValue;
            m_Contrast = PlateauRenderingConstants.k_ContrastDefaultValue;
            m_Brightness = PlateauRenderingConstants.k_BrightnessDefaultValue;
            m_Sharpness = PlateauRenderingConstants.k_SharpnessDefaultValue;

            string computeShaderPath = PlateauToolkitRenderingPaths.k_ImageProcessingComputeShader;
            m_ComputeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(computeShaderPath);

            m_TextureDownscaleRatio = TextureDownscaleRatio.None;
            m_TextureDownscaleRatioPrevious = TextureDownscaleRatio.None;

            EditorSceneManager.sceneOpened += OnSceneOpened;

            m_ComputeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(m_ComputeShaderPath);
            m_ContrastFilter = new PlateauRenderingContrastFilter();
            m_BrightnessFilter = new PlateauRenderingBrightnessFilter();
            m_HpfFilter = new PlateauRenderingHighPassFilter();
            m_SharpnessFilter = new PlateauRenderingSharpenFilter();

            m_HeaderTextStyle = new GUIStyle();
            m_HeaderTextStyle.fontSize = 20;
            m_HeaderTextStyle.normal.textColor = Color.white;

            Selection.selectionChanged += CancelEditMode;
            Selection.selectionChanged += CancelScalingMode;
        }

        void OnDisable()
        {
            Selection.selectionChanged -= CancelEditMode;
            Selection.selectionChanged -= CancelScalingMode;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
        }

        void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            CancelEditMode();
            CancelScalingMode();
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
                            CancelEditMode();
                            CancelScalingMode();
                        }
                    },
                    () =>
                    {
                        if (TabButton(PlateauToolkitRenderingPaths.k_ShaderIcon, Tab.Shader))
                        {
                            CancelEditMode();
                            CancelScalingMode();
                        }
                    },
                    () =>
                    {
                        if (TabButton(PlateauToolkitRenderingPaths.k_LodIcon, Tab.LodGrouping))
                        {
                            CancelEditMode();
                            CancelScalingMode();
                        }
                    },
                    () =>
                    {
                        if (TabButton(PlateauToolkitRenderingPaths.k_ScalerIcon, Tab.TextureTuner))
                        {
                            CancelEditMode();
                            CancelScalingMode();
                        }
                    }
                });

            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            switch (m_CurrentTab)
            {
                case Tab.LodGrouping:
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

#if PLATEAU_SDK_222
                            var building = GameObject.FindObjectsOfType<GameObject>()
                                .Where(obj => obj.GetComponent<PLATEAUCityObjectGroup>() != null
                                        && (obj.name.Contains("BLD") || obj.name.Contains("bldg") || obj.name.Contains("group")))
                                .FirstOrDefault();
                            if (building != null)
                            {
                                m_MeshGranularity = building.GetComponent<PLATEAUCityObjectGroup>().Granularity;
                            }
                            if (m_MeshGranularity == MeshGranularity.PerCityModelArea)
                            {
                                PLATEAUInstancedCityModel rootCityModel = PlateauRenderingBuildingUtilities.RetrieveTopmostParentWithComponent<PLATEAUInstancedCityModel>(building.transform);
                                ConvertAsync(rootCityModel.gameObject, () =>
                                {
                                    m_Grouping.GroupObjects();
                                    m_CreateLodGroup.CreateLodGroups();
                                }).ContinueWithErrorCatch();
                            }
                            else
                            {
                                m_Grouping.TrySeparateMeshes();
                                m_Grouping.GroupObjects();
                                m_CreateLodGroup.CreateLodGroups();
                            }
#else

                            m_Grouping.TrySeparateMeshes();
                            m_Grouping.GroupObjects();
                            m_CreateLodGroup.CreateLodGroups();
#endif
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
                        m_SelectedObjects.Clear();
                        AutoTexture();
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("自動生成されたテクスチャの窓を表示または非表示します。LOD２のみに有効です。", MessageType.Info);
                    EditorGUILayout.Space();

                    if (GUILayout.Button("窓の表示の切り替え"))
                    {
                        m_SelectedObjects.Clear();
                        PlateauRenderingMeshUtilities.GetSelectedGameObjects(m_SelectedObjects);
                        if (!SelectedBuildings())
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
                                    if (!building.name.Contains("WindowToggled"))
                                    {
                                        MeshFilter targetMeshFilter = building.GetComponent<MeshFilter>();

                                        // Create a copy of the target's meshCopy for modification
                                        UnityEngine.Mesh meshCopy = UnityEngine.Object.Instantiate(targetMeshFilter.sharedMesh);
                                        meshCopy.name = targetMeshFilter.sharedMesh.name;
                                        var renderer = building.GetComponent<Renderer>();

                                        if (renderer == null)
                                        {
                                            throw new ArgumentNullException(nameof(m_TargetRenderer), "Parameter 'm_TargetRenderer' cannot be null.");
                                        }

                                        if (meshCopy == null)
                                        {
                                            throw new ArgumentNullException(nameof(meshCopy), "Parameter 'targetRenderer' cannot be null.");
                                        }
                                        (Material orig, int materialIndex) = PlateauRenderingMeshUtilities.GetSubmaterialAndIndexByLargestFaceArea(renderer, meshCopy);
                                        Material[] all = renderer.sharedMaterials;
                                        all[materialIndex] = new Material(orig);
                                        renderer.sharedMaterials = all;
                                        building.name += "_WindowToggled";
                                    }
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
                                UnityEngine.Mesh mesh = meshFilter.sharedMesh;
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

                #region Texture improvements

                case Tab.TextureTuner:
                    PlateauToolkitRenderingGUILayout.Header("テクスチャ調整");
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("オブジェクトを選択し「画素調整を開始する」を押下すると画素の編集モードに入ります。「画素調整を保存する」を押下すると調整処理が保存されます。", MessageType.Info);
                    EditorGUILayout.Space();

                    #region image processing
                    if (m_ImageEditState == ImageEditState.FilterEditing)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox("選択されている地物のテクスチャを編集中です。", MessageType.Info);
                        EditorGUILayout.Space();
                    }
                    else
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox("編集を開始するボタンを押すと編集できます。", MessageType.Info);
                        EditorGUILayout.Space();
                    }

                    EditorGUILayout.BeginHorizontal();

                    if (m_ImageEditState == ImageEditState.None || m_ImageEditState == ImageEditState.Scaling)
                    {
                        if (GUILayout.Button("編集開始"))
                        {
                            if (Selection.gameObjects.Length != 1)
                            {
                                EditorUtility.DisplayDialog(
                                   "オブジェクト選択の確認",
                                   "有効なオブジェクトを一つのみ選択してください。",
                                   "OK"
                                   );
                            }
                            else if (Selection.gameObjects.Length == 1 && PlateauRenderingBuildingUtilities.IsObjectAutoTextured(Selection.activeGameObject))
                            {
                                EditorUtility.DisplayDialog(
                                   "オブジェクト選択の確認",
                                   "自動テクスチャリングされたオブジェクトは対象外です。",
                                   "OK"
                                   );
                            }
                            else if (Selection.gameObjects.Length == 1 && PlateauRenderingBuildingUtilities.GetMeshLodLevel(Selection.activeGameObject) != PlateauMeshLodLevel.Lod2)
                            {
                                EditorUtility.DisplayDialog(
                                   "オブジェクト選択の確認",
                                   "LOD2 のみ対象です。",
                                   "OK"
                                   );
                            }
                            else
                            {
                                Selection.selectionChanged += CancelEditMode;
                                GameObject selected = Selection.activeGameObject;

                                if (m_ImageEditState == ImageEditState.None && CanImageEditSelection() && selected != null)
                                {
                                    m_ImageEditState = ImageEditState.FilterEditing;
                                    m_TargetRenderer = selected.GetComponent<MeshRenderer>();
                                    MeshFilter targetMeshFilter = selected.GetComponent<MeshFilter>();

                                    // Create a copy of the target's meshCopy for modification
                                    UnityEngine.Mesh meshCopy = UnityEngine.Object.Instantiate(targetMeshFilter.sharedMesh);
                                    meshCopy.name = targetMeshFilter.sharedMesh.name;

                                    if (m_TargetRenderer == null)
                                    {
                                        throw new ArgumentNullException(nameof(m_TargetRenderer), "Parameter 'm_TargetRenderer' cannot be null.");
                                    }

                                    if (meshCopy == null)
                                    {
                                        throw new ArgumentNullException(nameof(meshCopy), "Parameter 'targetRenderer' cannot be null.");
                                    }

                                    (Material initialMaterial, int materialIndex) = PlateauRenderingMeshUtilities.GetSubmaterialAndIndexByLargestFaceArea(m_TargetRenderer, meshCopy);
                                    m_InitialMaterial = initialMaterial;
                                    m_InitialMaterialIndex = materialIndex;
                                    m_InitialTexture = m_InitialMaterial.mainTexture;

                                    m_PreviewMaterial = new Material(m_InitialMaterial);

                                    m_InitialTextureResized = m_InitialTexture;
                                    if (m_InitialTexture.width > 1024 || m_InitialTexture.height > 1024)
                                    {
                                        m_InitialTextureResized = TextureScaler.Resize(m_InitialTexture as Texture2D, 1024, 1024);
                                    }

                                    Material[] sharedMaterials = m_TargetRenderer.sharedMaterials;
                                    sharedMaterials[m_InitialMaterialIndex] = m_PreviewMaterial;
                                    m_TargetRenderer.sharedMaterials = sharedMaterials;

                                    if (m_TextureEnhance == null)
                                    {
                                        m_TextureEnhance = new TextureEnhance(m_InitialMaterial);
                                    }
                                    else
                                    {
                                        m_TextureEnhance.SetTexture(m_InitialMaterial);
                                    }

                                    DestroyImmediate(meshCopy);
                                }
                                else if (m_ImageEditState == ImageEditState.Scaling)
                                {
                                    EditorUtility.DisplayDialog(
                                   "無効な操作",
                                   "解像度編集モードでは画像パラメータの調整はできません。",
                                   "OK"
                                   );
                                }
                            }
                        }
                    }
                    else if (m_ImageEditState == ImageEditState.FilterEditing)
                    {
                        if (GUILayout.Button("編集中止"))
                        {
                            CancelEditMode();
                        }
                    }

                    if (GUILayout.Button("編集を保存する"))
                    {
                        ExitImageEditMode();
                    }
                    EditorGUILayout.EndHorizontal();

                    ShowImageEditParameter(ImageEditParameters.HighPass.ToString(), ref m_HighPass, ImageEditParameters.HighPass);
                    ShowImageEditParameter(ImageEditParameters.Contrast.ToString(), ref m_Contrast, ImageEditParameters.Contrast);
                    ShowImageEditParameter(ImageEditParameters.Brightness.ToString(), ref m_Brightness, ImageEditParameters.Brightness);
                    ShowImageEditParameter(ImageEditParameters.Sharpness.ToString(), ref m_Sharpness, ImageEditParameters.Sharpness);

                    if (m_ImageEditState == ImageEditState.FilterEditing)
                    {
                        if (m_HighPass != m_HighPassPrevious)
                        {
                            m_HighPassPrevious = m_HighPass;
                            ApplyToPreview();
                        }
                        if (m_Contrast != m_ContrastPrevious)
                        {
                            m_ContrastPrevious = m_Contrast;
                            ApplyToPreview();
                        }
                        if (m_Brightness != m_BrightnessPrevious)
                        {
                            m_BrightnessPrevious = m_Brightness;
                            ApplyToPreview();
                        }
                        if (m_Sharpness != m_SharpnessPrevious)
                        {
                            m_SharpnessPrevious = m_Sharpness;
                            ApplyToPreview();
                        }
                    }
                    EditorGUILayout.Space(20);

                    if (m_TextureEnhance != null && m_TextureEnhance.ReadOnlyOperationsList.Count > 0)
                    {
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField("保存した画素パラメータのコピー", m_HeaderTextStyle);
                        EditorGUILayout.Space(3);
                        EditorGUILayout.LabelField("オブジェクトを選択し下記のボタンを押下すると、直前に保存した画素パラメータをそのまま適用することができます。");
                        EditorGUILayout.Space(5);
                        EditorGUILayout.LabelField("保存済のパラメーター");
                        EditorGUILayout.Space(2);
                        for (int i = 0; i < m_TextureEnhance.ReadOnlyOperationsList.Count; i++)
                        {
                            EditorGUILayout.LabelField(m_TextureEnhance.ReadOnlyOperationsList[i].ImageParam + " : " + m_TextureEnhance.ReadOnlyOperationsList[i].ImageParamValue);
                        }
                        EditorGUILayout.Space(5);
                        if (GUILayout.Button("選択したオブジェクトのテクスチャに保存済の画素パラメータをコピーする"))
                        {
                            ApplyImageOperations();
                        }
                        EditorGUILayout.EndVertical();
                    }

                    #endregion

                    #region scaling
                    DrawSeparatorLine();
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("オブジェクトを選択し「解像度変更を開始する」を押下すると解像度の編集モードに入ります。\r\n「解像度変更を保存する」を押下すると調整処理が保存されます。", MessageType.Info);
                    EditorGUILayout.Space();

                    if (m_ImageEditState == ImageEditState.Scaling)
                    {
                        EditorGUILayout.LabelField("解像度設定中");
                        if (m_TextureDownscaleRatio != m_TextureDownscaleRatioPrevious)
                        {
                            int newWidth = m_InitialTexture.width;
                            int newHeight = m_InitialTexture.height;

                            if (m_TextureDownscaleRatio != TextureDownscaleRatio.None)
                            {
                                switch (m_TextureDownscaleRatio)
                                {
                                    case TextureDownscaleRatio.Quarter:
                                        newWidth /= 4;
                                        newHeight /= 4;
                                        break;
                                    case TextureDownscaleRatio.Half:
                                        newWidth /= 2;
                                        newHeight /= 2;
                                        break;
                                    case TextureDownscaleRatio.OneEighth:
                                        newWidth /= 8;
                                        newHeight /= 8;
                                        break;
                                    case TextureDownscaleRatio.OneSixteenth:
                                        newWidth /= 16;
                                        newHeight /= 16;
                                        break;
                                }
                                Texture resizedTexture = TextureScaler.Resize(m_InitialTexture as Texture2D, newWidth, newHeight);

                                m_PreviewMaterial.mainTexture = resizedTexture;
                            }
                            else
                            {
                                m_PreviewMaterial.mainTexture = m_InitialTexture;
                            }
                            m_TextureDownscaleRatioPrevious = m_TextureDownscaleRatio;
                        }
                    }

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("解像度変更"))
                    {

                        if (Selection.gameObjects.Length != 1)
                        {
                            EditorUtility.DisplayDialog(
                               "オブジェクト選択の確認",
                               "有効なオブジェクトを一つのみ選択してください。",
                               "OK"
                               );
                        }
                        else if (Selection.gameObjects.Length == 1 && PlateauRenderingBuildingUtilities.IsObjectAutoTextured(Selection.activeGameObject))
                        {
                            EditorUtility.DisplayDialog(
                               "オブジェクト選択の確認",
                               "自動テクスチャリングされたオブジェクトは対象外です。",
                               "OK"
                               );
                        }
                        else if (Selection.gameObjects.Length == 1 && PlateauRenderingBuildingUtilities.GetMeshLodLevel(Selection.activeGameObject) != PlateauMeshLodLevel.Lod2)
                        {
                            EditorUtility.DisplayDialog(
                               "オブジェクト選択の確認",
                               "LOD2 のみ対象です。",
                               "OK"
                               );
                        }
                        else
                        {
                            switch (m_ImageEditState)
                            {
                                case ImageEditState.None:

                                    GameObject selected = Selection.activeGameObject;

                                    if (selected != null)
                                    {
                                        m_ImageEditState = ImageEditState.Scaling;

                                        m_TargetRenderer = selected.GetComponent<MeshRenderer>();
                                        MeshFilter targetMeshFilter = selected.GetComponent<MeshFilter>();

                                        // Create a copy of the target's meshCopy for modification
                                        UnityEngine.Mesh meshCopy = UnityEngine.Object.Instantiate(targetMeshFilter.sharedMesh);
                                        meshCopy.name = targetMeshFilter.sharedMesh.name;

                                        if (m_TargetRenderer == null)
                                        {
                                            throw new ArgumentNullException(nameof(m_TargetRenderer), "Parameter 'm_TargetRenderer' cannot be null.");
                                        }

                                        if (meshCopy == null)
                                        {
                                            throw new ArgumentNullException(nameof(meshCopy), "Parameter 'targetRenderer' cannot be null.");
                                        }

                                        (Material initialMaterial, int materialIndex) = PlateauRenderingMeshUtilities.GetSubmaterialAndIndexByLargestFaceArea(m_TargetRenderer, meshCopy);
                                        m_InitialMaterial = initialMaterial;
                                        m_InitialMaterialIndex = materialIndex;
                                        m_InitialTexture = m_InitialMaterial.mainTexture;

                                        m_PreviewMaterial = new Material(m_InitialMaterial);

                                        Material[] sharedMaterials = m_TargetRenderer.sharedMaterials;
                                        sharedMaterials[m_InitialMaterialIndex] = m_PreviewMaterial;
                                        m_TargetRenderer.sharedMaterials = sharedMaterials;

                                        DestroyImmediate(meshCopy);
                                    }

                                    break;
                                case ImageEditState.FilterEditing:
                                    break;
                                case ImageEditState.Scaling:
                                    break;
                            }
                        }
                    }

                    if (GUILayout.Button("解像度変更の保存"))
                    {
                        if (m_ImageEditState == ImageEditState.Scaling)
                        {
                            CancelScalingMode();
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("スケール", GUILayout.Width(120));
                    ShowScaleCheckbox(0, TextureDownscaleRatio.None);
                    ShowScaleCheckbox(4, TextureDownscaleRatio.Half);
                    ShowScaleCheckbox(3, TextureDownscaleRatio.Quarter);
                    ShowScaleCheckbox(2, TextureDownscaleRatio.OneEighth);
                    ShowScaleCheckbox(1, TextureDownscaleRatio.OneSixteenth);
                    EditorGUILayout.EndHorizontal();
                    #endregion

                    EditorGUILayout.Space(20);
                    if (m_ImageEditState == ImageEditState.None && m_TextureDownscaleRatio != TextureDownscaleRatio.None)
                    {
                        string ratio = "1/2";

                        switch (m_TextureDownscaleRatio)
                        {
                            case TextureDownscaleRatio.Quarter:
                                ratio = "1/4";
                                break;
                            case TextureDownscaleRatio.OneEighth:
                                ratio = "1/8";
                                break;
                            case TextureDownscaleRatio.OneSixteenth:
                                ratio = "1/16";
                                break;
                        }

                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField("保存した解像度スケールのコピー", m_HeaderTextStyle);
                        EditorGUILayout.Space(3);
                        EditorGUILayout.LabelField("オブジェクトを選択し下記のボタンを押下すると、直前に保存した解像度スケールをそのまま適用することができます。");
                        EditorGUILayout.Space(5);
                        EditorGUILayout.LabelField("保存済のパラメーター");
                        EditorGUILayout.Space(2);
                        EditorGUILayout.LabelField("スケール : " + ratio);
                        EditorGUILayout.Space(5);
                        if (GUILayout.Button("選択したオブジェクトのテクスチャに保存済の解像度スケールをコピーします"))
                        {
                            ApplyImageScaling();
                        }
                        EditorGUILayout.EndVertical();
                    }

                    break;
                    #endregion
            }
            EditorGUILayout.EndScrollView();
#endregion
        }

        void AutoTexture()
        {
            PlateauRenderingMeshUtilities.GetSelectedGameObjects(m_SelectedObjects);

            if (!SelectedObjectsExist())
            {
                if (!m_IsConvertingPlateauModel)
                {
                    string message = "有効な地物を選択する必要があります。";

                    EditorUtility.DisplayDialog(
                        "オブジェクト選択の確認",
                        message,
                        "OK"
                        );
                }
            }
            else
            {
                bool isOptionSelected = EditorUtility.DisplayDialog(
                       "テクスチャ作成の確認",
                       "テクスチャを生成します。必要に応じてHierarchy にある地物の構成が変更されることもあります。実行しますか？",
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
                else
                {
                    m_SelectedObjects.Clear();
                }
            }
        }

        void ApplyToPreview()
        {
            m_PreviewMaterial.mainTexture = m_InitialTextureResized;
            if (m_HighPass != 0)
            {
                m_PreviewMaterial.mainTexture = m_HpfFilter.ApplyFilter(m_PreviewMaterial.mainTexture, m_ComputeShader, m_HighPass);
            }
            if (m_Contrast != 0)
            {
                m_PreviewMaterial.mainTexture = m_ContrastFilter.ApplyFilter(m_PreviewMaterial.mainTexture, m_ComputeShader, m_Contrast);
            }
            if (m_Brightness != 0)
            {
                m_PreviewMaterial.mainTexture = m_BrightnessFilter.ApplyFilter(m_PreviewMaterial.mainTexture, m_ComputeShader, m_Brightness);
            }
            if (m_Sharpness != 0)
            {
                m_PreviewMaterial.mainTexture = m_SharpnessFilter.ApplyFilter(m_PreviewMaterial.mainTexture, m_ComputeShader, m_Sharpness);
            }
        }

        void ApplyImageScaling()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            foreach (GameObject gameObject in selectedObjects)
            {
                // Check if the GameObject is part of a prefab
                if (PrefabUtility.GetPrefabInstanceStatus(gameObject) != PrefabInstanceStatus.NotAPrefab &&
                    PrefabUtility.GetPrefabAssetType(gameObject) != PrefabAssetType.NotAPrefab)
                {
                    // This GameObject is part of a prefab, skip it
                    continue;
                }

                if (PlateauRenderingBuildingUtilities.IsObjectAutoTextured(gameObject))
                {
                    continue;
                }

                if (PlateauRenderingBuildingUtilities.GetMeshLodLevel(Selection.activeGameObject) != PlateauMeshLodLevel.Lod2)
                {
                    continue;
                }

                MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
                MeshFilter mf = gameObject.GetComponent<MeshFilter>();

                // Check for Mesh Filter and Mesh Renderer components
                if (mf != null && mr != null)
                {
                    Material[] materials = mr.sharedMaterials;

                    foreach (Material mat in materials)
                    {
                        if (mat.mainTexture == null)
                        {
                            continue;
                        }

                        // Record the material before changing the texture
                        Undo.RecordObject(mat, "Scale Texture");

                        int newWidth = mat.mainTexture.width;
                        int newHeight = mat.mainTexture.height;
                        switch (m_TextureDownscaleRatio)
                        {
                            case TextureDownscaleRatio.None:
                                break;
                            case TextureDownscaleRatio.Quarter:
                                newWidth /= 4;
                                newHeight /= 4;
                                mat.mainTexture = TextureScaler.Resize(mat.mainTexture as Texture2D, newWidth, newHeight);
                                break;
                            case TextureDownscaleRatio.Half:
                                newWidth /= 2;
                                newHeight /= 2;
                                mat.mainTexture = TextureScaler.Resize(mat.mainTexture as Texture2D, newWidth, newHeight);
                                break;
                            case TextureDownscaleRatio.OneEighth:
                                newWidth /= 8;
                                newHeight /= 8;
                                mat.mainTexture = TextureScaler.Resize(mat.mainTexture as Texture2D, newWidth, newHeight);
                                break;
                            case TextureDownscaleRatio.OneSixteenth:
                                newWidth /= 16;
                                newHeight /= 16;
                                mat.mainTexture = TextureScaler.Resize(mat.mainTexture as Texture2D, newWidth, newHeight);
                                break;

                        }
                    }
                }
            }
        }

        void ApplyImageOperations()
        {
            GameObject[] selectedObjects = Selection.gameObjects;

            foreach (GameObject gameObject in selectedObjects)
            {
                // Check if the GameObject is part of a prefab
                if (PrefabUtility.GetPrefabInstanceStatus(gameObject) != PrefabInstanceStatus.NotAPrefab &&
                    PrefabUtility.GetPrefabAssetType(gameObject) != PrefabAssetType.NotAPrefab)
                {
                    // This GameObject is part of a prefab, skip it
                    continue;
                }

                if (PlateauRenderingBuildingUtilities.IsObjectAutoTextured(gameObject))
                {
                    continue;
                }

                if (PlateauRenderingBuildingUtilities.GetMeshLodLevel(Selection.activeGameObject) != PlateauMeshLodLevel.Lod2)
                {
                    continue;
                }

                MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
                MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

                // Check for Mesh Filter and Mesh Renderer components
                if (meshFilter != null && meshRenderer != null)
                {
                    (Material original, int materialIndex) = PlateauRenderingMeshUtilities.GetSubmaterialAndIndexByLargestFaceArea(meshRenderer, meshFilter.sharedMesh);
                    Material newMat = new Material(original);

                    // apply the image processing on the texture
                    foreach (ImageEditOperation op in m_TextureEnhance.ReadOnlyOperationsList)
                    {
                        IPlateauRenderingImageFilter filter = null;
                        switch (op.ImageParam)
                        {
                            case ImageEditParameters.HighPass:
                                if (filter == null && op.ImageParamValue != 0)
                                {
                                    filter = new PlateauRenderingHighPassFilter();
                                }
                                break;
                            case ImageEditParameters.Contrast:
                                if (filter == null && op.ImageParamValue != 0)
                                {
                                    filter = new PlateauRenderingContrastFilter();
                                }
                                break;
                            case ImageEditParameters.Brightness:
                                if (filter == null && op.ImageParamValue != 0)
                                {
                                    filter = new PlateauRenderingBrightnessFilter();
                                }
                                break;
                            case ImageEditParameters.Sharpness:
                                if (filter == null && op.ImageParamValue != 0)
                                {
                                    filter = new PlateauRenderingSharpenFilter();
                                }
                                break;
                        }
                        if (filter != null && newMat.mainTexture != null)
                        {
                            newMat.mainTexture = filter.ApplyFilter(newMat.mainTexture, m_ComputeShader, op.ImageParamValue);
                        }
                    }

                    // Register the undo operation for the material changes
                    Undo.RegisterCompleteObjectUndo(meshRenderer, "Material Change");

                    Material[] sharedMaterials = meshRenderer.sharedMaterials;
                    sharedMaterials[materialIndex] = newMat;
                    meshRenderer.sharedMaterials = sharedMaterials;
                }
            }
        }

        bool SelectedBuildings()
        {
            for (int i = m_SelectedObjects.Count - 1; i >= 0; i--)
            {
                if (!PlateauRenderingBuildingUtilities.IsPlateauBuilding(m_SelectedObjects[i].transform))
                {
                    m_SelectedObjects.RemoveAt(i);
                }
            }

            return m_SelectedObjects.Count > 0;
        }

        /// <summary>
        /// Check that the selected objects are Plateau objects. If a non texturable Plateau object is included, we remove that from the selected list and only
        /// texture the remaining Plateau objects.
        /// </summary>
        /// <returns></returns>
        bool SelectedObjectsExist()
        {
            for (int i = m_SelectedObjects.Count - 1; i >= 0; i--)
            {
                if (!PlateauRenderingBuildingUtilities.IsPlateauBuilding(m_SelectedObjects[i].transform))
                {
                    m_SelectedObjects.RemoveAt(i);
                }
            }

            if (m_SelectedObjects.Count == 0)
            {
                return false;
            }

#if PLATEAU_SDK_222
            PLATEAUCityObjectGroup objGroup = m_SelectedObjects[0].GetComponent<PLATEAUCityObjectGroup>();
            if (objGroup != null)
            {

                m_MeshGranularity = objGroup.Granularity;
                m_TextureMerged = objGroup.InfoForToolkits.IsTextureCombined;

                string warningMessage = "";
                if (m_TextureMerged)
                {
                    warningMessage += "テクスチャ結合が有効なため、すべての地物に自動テクスチャリングを行います。";
                }
                if (m_MeshGranularity == MeshGranularity.PerCityModelArea || m_MeshGranularity == MeshGranularity.PerAtomicFeatureObject)
                {
                    warningMessage += "地域単位と最小地物単位の地物は地物単位に変換されます。";
                }

                if (!string.IsNullOrEmpty(warningMessage))
                {
                    bool selection = EditorUtility.DisplayDialog(
                                     "属性情報を付与",
                                     warningMessage,
                                     "OK", "Cancel"
                                     );
                    if (!selection)
                    {
                        m_SelectedObjects.Clear();
                        return false;
                    }
                }

                PLATEAUInstancedCityModel rootCityModel = PlateauRenderingBuildingUtilities.RetrieveTopmostParentWithComponent<PLATEAUInstancedCityModel>(m_SelectedObjects[0].transform);

                if (m_MeshGranularity == MeshGranularity.PerCityModelArea && rootCityModel != null)
                {
                    m_SelectedObjects.Clear();
                    ConvertAsync(rootCityModel.gameObject, ReselectAndAutoTextureBuildingsAfterConversion).ContinueWithErrorCatch();
                }
                else if (m_TextureMerged)
                {
                    SelectAllBuildings(m_SelectedObjects[0]);
                }
            }
#endif
            return m_SelectedObjects.Count > 0;
        }

        void ReselectAndAutoTextureBuildingsAfterConversion()
        {
            m_SelectedObjects.Clear();
            PLATEAUCityObjectGroup[] objGroup = FindObjectsOfType<PLATEAUCityObjectGroup>();
            foreach (var obj in objGroup)
            {
                if (obj.gameObject.name.Contains("BLD") || obj.gameObject.name.Contains("bldg"))
                {
                    m_SelectedObjects.Add(obj.gameObject);
                }
            }
            AutoTexture();
        }

        async Task ConvertAsync(GameObject target, Action callback)
        {
            m_IsConvertingPlateauModel = true;
            PLATEAU.CityImport.Import.Convert.GranularityConvertResult result = await new CityGranularityConverter().ConvertAsync(
                new GranularityConvertOptionUnity(
                    new GranularityConvertOption(
                        MeshGranularity.PerPrimaryFeatureObject,
                        1),
                    new GameObject[] { target },
                    true));
            m_IsConvertingPlateauModel = false;
            callback?.Invoke();
        }

        void SelectAllBuildings(GameObject selected)
        {
            PLATEAUInstancedCityModel rootCityModel = PlateauRenderingBuildingUtilities.RetrieveTopmostParentWithComponent<PLATEAUInstancedCityModel>(selected.transform);

            if (rootCityModel != null)
            {
                m_SelectedObjects.Clear();
                foreach (Transform childGml in rootCityModel.transform)
                {
                    foreach (Transform child in childGml.transform)
                    {
                        foreach (Transform buildingCandidate in child.transform)
                        {
                            if (buildingCandidate.GetComponent<PLATEAUCityObjectGroup>() != null)
                            {
                                m_SelectedObjects.Add(buildingCandidate.gameObject);
                            }
                        }
                    }
                }
            }
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

        bool CanImageEditSelection()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected != null && selected.GetComponent<MeshRenderer>() != null)
            {
                return true;
            }
            return false;
        }

        int ShowImageEditDialog(string imageParam)
        {
            int option = EditorUtility.DisplayDialogComplex(
                                       imageParam + "の保存",
                                       imageParam + "の設定を保存します。",
                                       "OK",
                                       "キャンセル",
                                       imageParam + "の編集を破棄します"
                                    );
            return option;
        }

        void DrawSeparatorLine()
        {
            // Layout options for the line
            var separatorStyle = new GUIStyle();
            separatorStyle.normal.background = EditorGUIUtility.whiteTexture;
            separatorStyle.margin = new RectOffset(0, 0, 4, 4);
            separatorStyle.fixedHeight = 1;

            // Drawing the line
            var rect = GUILayoutUtility.GetRect(GUIContent.none, separatorStyle);
            GUI.color = new Color(0.5f, 0.5f, 0.5f); // Set the color of the line
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
            GUI.color = Color.white; // Reset the GUI color to avoid affecting other GUI elements
        }

        void ShowScaleCheckbox(int index, TextureDownscaleRatio scalingMode)
        {
            string title = "1";
            switch (index)
            {
                case 0:
                    break;
                case 1:
                    title = "1/16";
                    break;
                case 2:
                    title = "1/8";
                    break;
                case 3:
                    title = "1/4";
                    break;
                case 4:
                    title = "1/2";
                    break;
            }
            bool isToggled = EditorGUILayout.ToggleLeft(title, m_TextureDownscaleRatio == scalingMode, GUILayout.Width(50));
            if (isToggled && m_TextureDownscaleRatio != scalingMode && m_ImageEditState == ImageEditState.Scaling)
            {
                m_TextureDownscaleRatio = scalingMode;
            }
        }

        void ShowImageEditParameter(string imageParamName, ref int value, ImageEditParameters imageEditParameterType)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(imageParamName, GUILayout.Width(100));
            int minVal = 0;
            int maxVal = 0;

            switch (imageEditParameterType)
            {
                case ImageEditParameters.Brightness:
                    minVal = PlateauRenderingConstants.k_BrightnessMinValue;
                    maxVal = PlateauRenderingConstants.k_BrightnessMaxValue;
                    break;
                case ImageEditParameters.Contrast:
                    minVal = PlateauRenderingConstants.k_ContrastMinValue;
                    maxVal = PlateauRenderingConstants.k_ContrastMaxValue;
                    break;
                case ImageEditParameters.HighPass:
                    minVal = PlateauRenderingConstants.k_HighPassMinValue;
                    maxVal = PlateauRenderingConstants.k_HighPassMaxValue;
                    break;
                case ImageEditParameters.Sharpness:
                    minVal = PlateauRenderingConstants.k_SharpnessMinValue;
                    maxVal = PlateauRenderingConstants.k_SharpnessMaxValue;
                    break;
            }

            value = EditorGUILayout.IntSlider(value, minVal, maxVal);
            EditorGUILayout.EndHorizontal();
        }

        void CancelEditMode()
        {
            if (m_ImageEditState == ImageEditState.FilterEditing)
            {
                if (m_TargetRenderer != null)
                {
                    Material[] sharedMaterials = m_TargetRenderer.sharedMaterials;
                    if (m_InitialMaterialIndex >= 0 && m_InitialMaterialIndex < sharedMaterials.Length)
                    {
                        sharedMaterials[m_InitialMaterialIndex] = m_InitialMaterial;
                        m_TargetRenderer.sharedMaterials = sharedMaterials;
                    }
                }

                if (m_PreviewMaterial != null)
                {
                    DestroyImmediate(m_PreviewMaterial);
                }

                m_ImageEditState = ImageEditState.None;
            }
        }

        void CancelScalingMode()
        {
            if (m_ImageEditState == ImageEditState.Scaling)
            {

                if (m_TargetRenderer != null)
                {
                    Material[] sharedMaterials = m_TargetRenderer.sharedMaterials;
                    if (m_InitialMaterialIndex >= 0 && m_InitialMaterialIndex < sharedMaterials.Length)
                    {
                        m_InitialMaterial.mainTexture = m_InitialTexture;
                        sharedMaterials[m_InitialMaterialIndex] = m_InitialMaterial;
                        m_TargetRenderer.sharedMaterials = sharedMaterials;
                    }
                }
                m_ImageEditState = ImageEditState.None;
                if (m_PreviewMaterial != null)
                {
                    DestroyImmediate(m_PreviewMaterial);
                }
            }
        }

        void ExitImageEditMode()
        {
            if (m_ImageEditState == ImageEditState.FilterEditing)
            {
                if (m_TargetRenderer != null)
                {
                    Material[] sharedMaterials = m_TargetRenderer.sharedMaterials;
                    if (m_InitialMaterialIndex >= 0 && m_InitialMaterialIndex < sharedMaterials.Length)
                    {
                        m_InitialMaterial.mainTexture = m_InitialTexture;
                        sharedMaterials[m_InitialMaterialIndex] = m_InitialMaterial;
                        m_TargetRenderer.sharedMaterials = sharedMaterials;
                    }
                }

                if (m_PreviewMaterial != null)
                {
                    DestroyImmediate(m_PreviewMaterial);
                }

                m_ImageEditState = ImageEditState.None;

                if (m_TextureEnhance != null)
                {
                    m_TextureEnhance.SaveImageParameter(ImageEditParameters.Contrast, m_Contrast);
                    m_TextureEnhance.SaveImageParameter(ImageEditParameters.Brightness, m_Brightness);
                    m_TextureEnhance.SaveImageParameter(ImageEditParameters.HighPass, m_HighPass);
                    m_TextureEnhance.SaveImageParameter(ImageEditParameters.Sharpness, m_Sharpness);
                }
            }
        }

        internal void UnblockUI()
        {
            m_BlockUI = false;
            Repaint();
        }
    }
}