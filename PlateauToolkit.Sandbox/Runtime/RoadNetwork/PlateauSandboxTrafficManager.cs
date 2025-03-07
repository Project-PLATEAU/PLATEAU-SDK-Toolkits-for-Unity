using AWSIM;
using AWSIM.TrafficSimulation;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    /// <summary>
    /// Traffic Simulator / RoadNetwork 総合管理
    /// </summary>
    public class PlateauSandboxTrafficManager : MonoBehaviour
    {

        [Header("TrafficLight")]
        [SerializeField, Tooltip("Traffic light prefab")]
        public GameObject m_TrafficLightPrefab;

        [SerializeField]
        [HideInInspector]
        GameObject m_CurrentTrafficLightPrefab;

        [SerializeField, Tooltip("TrafficLight Green interval seconds.")]
        public float m_GreenInterval = PlateauSandboxTrafficManagerConstants.TRAFFIC_LIGHT_GREEN_INTERVAL_SECONDS;

        [SerializeField, Tooltip("TrafficLight Yellow interval seconds.")]
        public float m_YellowInterval = PlateauSandboxTrafficManagerConstants.TRAFFIC_LIGHT_YELLOW_INTERVAL_SECONDS;

        // Red Interval = Green + Yellow + additional Red
        [SerializeField, Tooltip("TrafficLight additional Red interval seconds.")]
        public float m_ExtraRedInterval = PlateauSandboxTrafficManagerConstants.TRAFFIC_LIGHT_RED_INTERVAL_SECONDS;

        // Runtime時にRightOfWays（交差点での判定処理）を行わないフラグ (車の台数が100台以上の場合の負荷軽減用）
        [SerializeField, Tooltip("Use RightOfWays.")]
        public bool m_EnableRightOfWays = true;

        [Header("Debug")]
        [SerializeField, Tooltip("Show Traffic light Gizmos")]
        public bool m_ShowTrafficLightGizmos = PlateauSandboxTrafficManagerConstants.SHOW_DEBUG_GIZMOS;

        RoadNetworkDataGetter m_RoadNetworkGetter;

        public TrafficManager SimTrafficManager
        {
            get
            {
                if (TryGetComponent<TrafficManager>(out TrafficManager tManager))
                {
                    return tManager;
                }

                return gameObject.AddComponent<TrafficManager>();
            }
        }

        public RoadNetworkDataGetter RnGetter
        {
            get
            {
                if (m_RoadNetworkGetter == null)
                {
                    PLATEAURnStructureModel roadNetwork = GameObject.FindObjectOfType<PLATEAURnStructureModel>();
                    m_RoadNetworkGetter = roadNetwork?.GetRoadNetworkDataGetter();

                    if (m_RoadNetworkGetter == null)
                    {
                        Debug.LogError($"RoadNetworkDataGetter is null");
                    }
                }

                return m_RoadNetworkGetter;
            }
        }

        public bool ShowTrafficLightGizmos => m_ShowTrafficLightGizmos;

        public int GetNumMaxVehicles()
        {
            int numRoads = RnGetter.GetRoadBases().OfType<RnDataRoad>().Count();
            return (int)Mathf.Min(numRoads / 2, PlateauSandboxTrafficManagerConstants.NUM_MAX_VEHICLES); // 交差点以外の道路数の半分 or 最大車輛数
        }

        public void CreateSimulator(List<GameObject> prefabs)
        {
            GameObject vehicles = GameObject.Find(PlateauSandboxTrafficManagerConstants.VEHICLE_ROOT_NAME);
            if (vehicles == null)
            {
                vehicles = new GameObject(PlateauSandboxTrafficManagerConstants.VEHICLE_ROOT_NAME);
                vehicles.transform.SetParent(SimTrafficManager.transform, false);
            }

            SimTrafficManager.InitParams(LayerMask.NameToLayer(PlateauSandboxTrafficManagerConstants.LAYER_MASK_VEHICLE), LayerMask.NameToLayer(PlateauSandboxTrafficManagerConstants.LAYER_MASK_GROUND), GetNumMaxVehicles(), vehicles, PlateauSandboxTrafficManagerConstants.SHOW_DEBUG_GIZMOS);

            List<TrafficLane> allLanes = new RoadNetworkLaneConverter().Create(RnGetter, SimTrafficManager.transform); //全て変換 (TrafficLane)

            //初期Spawn可能なTrafficLanes
            List<TrafficLane> spawnableLanes = allLanes.FindAll(x => !x.intersectionLane); //交差点以外

            //ReSpawn可能なTrafficLanes
            List<TrafficLane> respawnableLanes = allLanes.FindAll(x => !x.intersectionLane && x.rnRoad.Next.IsValid && !x.rnRoad.Prev.IsValid); //交差点以外 : Prevが空でNextが存在 (RoadNetworkで判定）

            spawnableLanes.RemoveAll(x => respawnableLanes.Contains(x)); //重複を除去

            //初期Spawn
            RandomTrafficSimulatorConfiguration randomTrafficSimConfigInitial = new RandomTrafficSimulatorConfiguration();
            randomTrafficSimConfigInitial.maximumSpawns = GetNumMaxVehicles(); // 初回用　Respawn禁止
            randomTrafficSimConfigInitial.npcPrefabs = prefabs.ToArray();
            randomTrafficSimConfigInitial.spawnableLanes = spawnableLanes.ToArray();
            randomTrafficSimConfigInitial.enabled = true;

            RandomTrafficSimulatorConfiguration randomTrafficSimConfig = new RandomTrafficSimulatorConfiguration();
            randomTrafficSimConfig.maximumSpawns = 0; //0:Respawn可能
            randomTrafficSimConfig.npcPrefabs = prefabs.ToArray();
            randomTrafficSimConfig.spawnableLanes = respawnableLanes.ToArray();
            randomTrafficSimConfig.enabled = true;
            SimTrafficManager.randomTrafficSims = new RandomTrafficSimulatorConfiguration[] { randomTrafficSimConfigInitial, randomTrafficSimConfig };

            SimTrafficManager.Initialize();
        }

        public void SetTrafficLightAsset(GameObject trafficLightPrefab)
        {
#if UNITY_EDITOR
            m_TrafficLightPrefab = m_CurrentTrafficLightPrefab = trafficLightPrefab;

            var trafficLightParent = GameObject.Find(PlateauSandboxTrafficManagerConstants.TRAFFIC_LIGHT_ASSETS_ROOT_NAME);
            if (trafficLightParent == null)
            {
                trafficLightParent = new GameObject(PlateauSandboxTrafficManagerConstants.TRAFFIC_LIGHT_ASSETS_ROOT_NAME);
                trafficLightParent.transform.SetParent(transform, false);
            }

            var trafficLights = new List<TrafficLight>(GameObject.FindObjectsOfType<TrafficLight>());
            foreach (TrafficLight trafficLight in trafficLights)
            {
                string gameObjectName = GameObjectUtility.GetUniqueNameForSibling(trafficLightParent.transform, trafficLightPrefab.name);
                var gameObject = (GameObject)PrefabUtility.InstantiatePrefab(trafficLightPrefab);
                gameObject.name = gameObjectName;
                gameObject.transform.position = trafficLight.GetAssetPosition();
                gameObject.transform.right = trafficLight.GetRightVector();

                gameObject.transform.SetParent(trafficLightParent.transform, false);

                PlateauSandboxInteractiveTrafficLight component = gameObject.GetComponent<PlateauSandboxInteractiveTrafficLight>();
                if (component != null)
                {
                    trafficLight.SetTrafficLightAsset(component);
                }
                else
                {
                    trafficLight.SetRenderer(gameObject.GetComponentInChildren<Renderer>());
                }
            }
#endif
        }

#if UNITY_EDITOR
        public void UpdateTrafficLightSequences()
        {
            var trafficIntersections = GameObject.FindObjectsOfType<TrafficIntersection>();
            foreach (var intersection in trafficIntersections)
            {
                intersection.UpdateTrafficLightSequences(m_GreenInterval, m_YellowInterval, m_ExtraRedInterval);
            }

            if (m_TrafficLightPrefab != m_CurrentTrafficLightPrefab)
            {
                if (!m_TrafficLightPrefab.TryGetComponent<PlateauSandboxInteractiveTrafficLight>(out _))
                {
                    EditorUtility.DisplayDialog("エラー", "PlateauSandboxInteractiveTrafficLightコンポーネントがアタッチされたGameObjectを選択してください。", "OK");
                    return;
                }

                EditorApplication.delayCall += ClearCurrentAssetsAndSetTrafficLightAsset;
            }
        }

        void ClearCurrentAssetsAndSetTrafficLightAsset()
        {
            EditorApplication.delayCall -= ClearCurrentAssetsAndSetTrafficLightAsset;
            GameObject trafficLightParent = GameObject.Find(PlateauSandboxTrafficManagerConstants.TRAFFIC_LIGHT_ASSETS_ROOT_NAME);
            if (trafficLightParent != null)
            {
                DestroyImmediate(trafficLightParent);
            }

            if (m_TrafficLightPrefab != null)
            {
                SetTrafficLightAsset(m_TrafficLightPrefab);
            }
            else
            {
                m_CurrentTrafficLightPrefab = null;
            }
        }

        //void OnValidate()
        //{
        //    if (Application.isPlaying)
        //    {
        //        return;
        //    }

        //    if (m_TrafficLightPrefab != m_CurrentTrafficLightPrefab)
        //    {
        //        EditorApplication.delayCall += ClearCurrentAssetsAndSetTrafficLightAsset;
        //    }
        //}
#endif
    }
}
