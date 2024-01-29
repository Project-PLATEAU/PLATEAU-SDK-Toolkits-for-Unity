using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

namespace PlateauToolkit.Utilities
{
    [ExecuteAlways]
    public class PrefabLightmapData : MonoBehaviour
    {
        [Serializable]
        struct RendererInfo
        {
            public Renderer m_Renderer;
            public int m_LightmapIndex;
            public Vector4 m_LightmapOffsetScale;
        }

        [Serializable]
        struct LightInfo
        {
            public Light m_Light;
            public int m_LightmapBakeType;
            public int m_MixedLightingMode;
        }

        [SerializeField] RendererInfo[] m_RendererInfo;
        [SerializeField] Texture2D[] m_Lightmaps;
        [SerializeField] Texture2D[] m_LightmapsDir;
        [SerializeField] Texture2D[] m_ShadowMasks;
        [SerializeField] LightInfo[] m_LightInfo;

        void Awake()
        {
            Init();
        }

        void Init()
        {
            if (m_RendererInfo == null || m_RendererInfo.Length == 0)
            {
                return;
            }

            LightmapData[] lightmaps = LightmapSettings.lightmaps;
            int[] offsetsIndexes = new int[m_Lightmaps.Length];
            int totalCount = lightmaps.Length;
            List<LightmapData> combinedLightmaps = new();

            for (int i = 0; i < m_Lightmaps.Length; i++)
            {
                bool exists = false;
                for (int j = 0; j < lightmaps.Length; j++)
                {

                    if (m_Lightmaps[i] == lightmaps[j].lightmapColor)
                    {
                        exists = true;
                        offsetsIndexes[i] = j;

                    }
                }
                if (!exists)
                {
                    offsetsIndexes[i] = totalCount;
                    LightmapData newLightmapData = new()
                    {
                        lightmapColor = m_Lightmaps[i],
                        lightmapDir = m_LightmapsDir.Length == m_Lightmaps.Length ? m_LightmapsDir[i] : default(Texture2D),
                        shadowMask = m_ShadowMasks.Length == m_Lightmaps.Length ? m_ShadowMasks[i] : default(Texture2D),
                    };

                    combinedLightmaps.Add(newLightmapData);

                    totalCount += 1;

                }
            }

            var combinedLightmaps2 = new LightmapData[totalCount];

            lightmaps.CopyTo(combinedLightmaps2, 0);
            combinedLightmaps.ToArray().CopyTo(combinedLightmaps2, lightmaps.Length);

            bool directional = true;

            foreach (Texture2D t in m_LightmapsDir)
            {
                if (t == null)
                {
                    directional = false;
                    break;
                }
            }

            LightmapSettings.lightmapsMode = (m_LightmapsDir.Length == m_Lightmaps.Length && directional) ? LightmapsMode.CombinedDirectional : LightmapsMode.NonDirectional;
            ApplyRendererInfo(m_RendererInfo, offsetsIndexes, m_LightInfo);
            LightmapSettings.lightmaps = combinedLightmaps2;
        }

        void OnEnable()
        {

            SceneManager.sceneLoaded += OnSceneLoaded;

        }

        // called second
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Init();
        }

        // called when the game is terminated
        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        static void ApplyRendererInfo(RendererInfo[] infos, int[] lightmapOffsetIndex, LightInfo[] lightsInfo)
        {
            foreach (RendererInfo info in infos)
            {
                info.m_Renderer.lightmapIndex = lightmapOffsetIndex[info.m_LightmapIndex];
                info.m_Renderer.lightmapScaleOffset = info.m_LightmapOffsetScale;

                // You have to release shaders.
                foreach (Material material in info.m_Renderer.sharedMaterials)
                {
                    if (material != null && Shader.Find(material.shader.name) != null)
                    {
                        material.shader = Shader.Find(material.shader.name);
                    }
                }
            }

            for (int i = 0; i < lightsInfo.Length; i++)
            {
                LightBakingOutput bakingOutput = new()
                {
                    isBaked = true,
                    lightmapBakeType = (LightmapBakeType)lightsInfo[i].m_LightmapBakeType,
                    mixedLightingMode = (MixedLightingMode)lightsInfo[i].m_MixedLightingMode,
                };

                lightsInfo[i].m_Light.bakingOutput = bakingOutput;

            }
        }

#if UNITY_EDITOR    
        public static void GenerateLightmapInfo()
        {
            GameObject[] selectedPrefabs = Selection.gameObjects;

            if (selectedPrefabs.Length == 0)
            {
                EditorUtility.DisplayDialog("操作ガイド", "シーンのライトマップをベイクするプレハブを選択する必要があります。", "閉じる");
                return;
            }

            // Create list of prefabs with missing components
            List<GameObject> prefabsWithoutComponent = new List<GameObject>();
            foreach (GameObject prefab in selectedPrefabs)
            {
                if (!prefab.GetComponent<PrefabLightmapData>())
                {
                    prefabsWithoutComponent.Add(prefab);
                }
            }

            // コンポーネントが不足しているプレハブがある場合、ダイアログを表示
            if (prefabsWithoutComponent.Count > 0)
            {
                string prefabNames = string.Join("\n", prefabsWithoutComponent.Select(p => p.name));
                bool userResponse = EditorUtility.DisplayDialog("操作ガイド",
                    $"選択されたプレハブには PrefabLightmapDataを追加する必要があります。追加しますか？\n{prefabNames}\n",
                    "はい", "いいえ");

                if (userResponse)
                {
                    foreach (GameObject prefab in prefabsWithoutComponent)
                    {
                        prefab.AddComponent<PrefabLightmapData>();
                    }
                }
                return;
            }

            if (LightmapSettings.lightmaps.Length == 0)
            {
                EditorUtility.DisplayDialog("操作ガイド", "シーンに紐づいたライトマップが存在しません。シーンでライトマップをベイクする必要があります。", "閉じる");
                return;
            }

            if (Lightmapping.giWorkflowMode != Lightmapping.GIWorkflowMode.OnDemand)
            {
                // ユーザーに操作ガイドを提供する
                EditorUtility.DisplayDialog(
                    "操作ガイド",
                    "ライトマップをベイク済みであり、ライトマップベイクの自動モードが無効になっていることが必要です。Unityのライトマッピング設定を確認して、設定を変更してください。",
                    "閉じる"
                );

                return;
            }

            Lightmapping.Bake();

            PrefabLightmapData[] prefabs = FindObjectsOfType<PrefabLightmapData>();

            foreach (PrefabLightmapData instance in prefabs)
            {
                GameObject gameObject = instance.gameObject;
                var rendererInfos = new List<RendererInfo>();
                var lightmaps = new List<Texture2D>();
                var lightmapsDir = new List<Texture2D>();
                var shadowMasks = new List<Texture2D>();
                var lightsInfos = new List<LightInfo>();

                GenerateLightmapInfo(gameObject, rendererInfos, lightmaps, lightmapsDir, shadowMasks, lightsInfos);

                instance.m_RendererInfo = rendererInfos.ToArray();
                instance.m_Lightmaps = lightmaps.ToArray();
                instance.m_LightmapsDir = lightmapsDir.ToArray();
                instance.m_LightInfo = lightsInfos.ToArray();
                instance.m_ShadowMasks = shadowMasks.ToArray();

                GameObject targetPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(instance.gameObject);
                if (targetPrefab != null)
                {
                    GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(instance.gameObject);

                    if (root != null)
                    {
                        GameObject rootPrefab = PrefabUtility.GetCorrespondingObjectFromSource(instance.gameObject);
                        string rootPath = AssetDatabase.GetAssetPath(rootPrefab);

                        PrefabUtility.UnpackPrefabInstanceAndReturnNewOutermostRoots(root, PrefabUnpackMode.OutermostRoot);
                        try
                        {
                            PrefabUtility.ApplyPrefabInstance(instance.gameObject, InteractionMode.AutomatedAction);
                        }
                        catch (Exception exception)
                        {
                            Debug.LogError(exception);
                        }
                        finally
                        {
                            PrefabUtility.SaveAsPrefabAssetAndConnect(root, rootPath, InteractionMode.AutomatedAction);
                        }
                    }
                    else
                    {
                        PrefabUtility.ApplyPrefabInstance(instance.gameObject, InteractionMode.AutomatedAction);
                    }
                }
            }
        }

        static void GenerateLightmapInfo(GameObject root, List<RendererInfo> rendererInfos, List<Texture2D> lightmaps, List<Texture2D> lightmapsDir, List<Texture2D> shadowMasks, List<LightInfo> lightsInfo)
        {
            MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                if (renderer.lightmapIndex != -1)
                {
                    RendererInfo info = new()
                    {
                        m_Renderer = renderer
                    };

                    if (renderer.lightmapScaleOffset != Vector4.zero)
                    {
                        if (renderer.lightmapIndex < 0 || renderer.lightmapIndex == 0xFFFE)
                        {
                            continue;
                        }

                        info.m_LightmapOffsetScale = renderer.lightmapScaleOffset;

                        Texture2D lightmap = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;
                        Texture2D lightmapDir = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapDir;
                        Texture2D shadowMask = LightmapSettings.lightmaps[renderer.lightmapIndex].shadowMask;

                        info.m_LightmapIndex = lightmaps.IndexOf(lightmap);
                        if (info.m_LightmapIndex == -1)
                        {
                            info.m_LightmapIndex = lightmaps.Count;
                            lightmaps.Add(lightmap);
                            lightmapsDir.Add(lightmapDir);
                            shadowMasks.Add(shadowMask);
                        }

                        rendererInfos.Add(info);
                    }
                }
            }

            Light[] lights = root.GetComponentsInChildren<Light>(true);

            foreach (Light l in lights)
            {
                LightInfo lightInfo = new()
                {
                    m_Light = l,
                    m_LightmapBakeType = (int)l.lightmapBakeType,
                    m_MixedLightingMode = (int)Lightmapping.lightingSettings.mixedBakeMode,
                };
                lightsInfo.Add(lightInfo);
            }
        }
#endif
    }
}
