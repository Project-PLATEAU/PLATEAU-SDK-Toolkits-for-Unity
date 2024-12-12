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
    public class RoadNetworkTrafficManager : MonoBehaviour
    {

        [SerializeField, Tooltip("Traffic light prefab")]
        GameObject m_TrafficLightPrefab;

        [SerializeField][HideInInspector]
        GameObject m_CurrentTrafficLightPrefab;

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

        public int GetNumMaxVehicles()
        {
            int numRoads = RnGetter.GetRoadBases().OfType<RnDataRoad>().Count();
            return (int)Mathf.Min(numRoads / 2, RoadNetworkConstants.NUM_MAX_VEHICLES); // 交差点以外の道路数の半分 or 最大車輛数
        }

        public void CreateSimulator(List<GameObject> prefabs)
        {
            GameObject vehicles = GameObject.Find(RoadNetworkConstants.VEHICLE_ROOT_NAME);
            if (vehicles == null)
            {
                vehicles = new GameObject(RoadNetworkConstants.VEHICLE_ROOT_NAME);
                vehicles.transform.SetParent(SimTrafficManager.transform, false);
            }

            SimTrafficManager.InitParams(LayerMask.NameToLayer(RoadNetworkConstants.LAYER_MASK_VEHICLE), LayerMask.NameToLayer(RoadNetworkConstants.LAYER_MASK_GROUND), GetNumMaxVehicles(), vehicles, RoadNetworkConstants.SHOW_DEBUG_GIZMOS);

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
            m_TrafficLightPrefab = m_CurrentTrafficLightPrefab = trafficLightPrefab;

            var trafficLightParent = GameObject.Find(RoadNetworkConstants.TRAFFIC_LIGHT_ASSETS_ROOT_NAME);
            if (trafficLightParent == null)
            {
                trafficLightParent = new GameObject(RoadNetworkConstants.TRAFFIC_LIGHT_ASSETS_ROOT_NAME);
                trafficLightParent.transform.SetParent(transform, false);
            }

            var trafficLights = new List<TrafficLight>(GameObject.FindObjectsOfType<TrafficLight>());
            foreach (TrafficLight trafficLight in trafficLights)
            {
                string gameObjectName = GameObjectUtility.GetUniqueNameForSibling(trafficLightParent.transform, trafficLightPrefab.name);
                var gameObject = (GameObject)PrefabUtility.InstantiatePrefab(trafficLightPrefab);
                gameObject.name = gameObjectName;
                gameObject.transform.position = trafficLight.GetAssetPosition();
                //gameObject.transform.position = trafficLight.transform.position;
                gameObject.transform.right = trafficLight.GetRightVector();

                gameObject.transform.SetParent(trafficLightParent.transform, false);
                trafficLight.SetRenderer(gameObject.GetComponentInChildren<Renderer>());
                //gameObject.GetComponentsInChildren<Renderer>();
            }
        }

#if UNITY_EDITOR
        void ClearCurrentAssetsAndSetTrafficLightAsset()
        {
            EditorApplication.delayCall -= ClearCurrentAssetsAndSetTrafficLightAsset;
            GameObject trafficLightParent = GameObject.Find(RoadNetworkConstants.TRAFFIC_LIGHT_ASSETS_ROOT_NAME);
            if (trafficLightParent != null)
            {
                DestroyImmediate(trafficLightParent);
            }

            if (m_TrafficLightPrefab != null)
            {
                SetTrafficLightAsset(m_TrafficLightPrefab);
            }
        }

        void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (m_TrafficLightPrefab != m_CurrentTrafficLightPrefab)
            {
                EditorApplication.delayCall += ClearCurrentAssetsAndSetTrafficLightAsset;
            }
        }
#endif
    }
}
