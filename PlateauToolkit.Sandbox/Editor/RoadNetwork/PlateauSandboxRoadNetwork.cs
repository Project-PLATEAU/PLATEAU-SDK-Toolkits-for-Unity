using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using UnityEngine;
using PlateauToolkit.Sandbox.RoadNetwork;
using AWSIM.TrafficSimulation;
using UnityEditor;
using PLATEAU.CityInfo;
using AWSIM;

namespace PlateauToolkit.Sandbox.Editor
{
    /// <summary>
    /// Editor側のTrafficSim生成処理
    /// PlateauSandboxWindowTrafficViewから呼ばれる
    /// </summary>
    public class PlateauSandboxRoadNetwork
    {
        RoadNetworkDataGetter m_RoadNetworkGetter;
        RoadNetworkTrafficManager m_RnTrafficManager;
        TrafficManager m_TrafficManager;

        public void ClearTrafficManager()
        {
            var manager = GameObject.Find(RoadNetworkConstants.TRAFFIC_MANAGER_NAME);
            if (manager != null)
            {
                GameObject.DestroyImmediate(manager);
            }
        }

        //AWSIM用
        public bool PlaceVehicles(List<GameObject> vehiclePrefabs)
        {
            if(vehiclePrefabs == null || vehiclePrefabs?.Count == 0)
            {
                EditorUtility.DisplayDialog("アセットの配置に失敗しました。", "アセットが選択されていません。", "OK");
                return false;
            }

            if (!Layers.LayerExists(RoadNetworkConstants.LAYER_MASK_VEHICLE))
                Layers.CreateLayer(RoadNetworkConstants.LAYER_MASK_VEHICLE);
            if (!Layers.LayerExists(RoadNetworkConstants.LAYER_MASK_GROUND))
                Layers.CreateLayer(RoadNetworkConstants.LAYER_MASK_GROUND);

            try
            {
                Initialize();
                m_RnTrafficManager.CreateSimulator(vehiclePrefabs);
                PostCreateSimulator();
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("アセットの配置に失敗しました。", ex.Message, "OK");
                return false;
            }

            EditorUtility.DisplayDialog("アセットの配置に成功しました。", $"交通シミュレータが配置されました。\n{vehiclePrefabs.Count}種類のアセットが追加されました。", "OK");
            return true;
        }

        public void PlaceTrafficLights()
        {
            var trafficLightParent = GameObject.Find(RoadNetworkConstants.TRAFFIC_LIGHT_ASSETS_ROOT_NAME);
            if (trafficLightParent == null)
            {
                trafficLightParent = new GameObject(RoadNetworkConstants.TRAFFIC_LIGHT_ASSETS_ROOT_NAME);
                trafficLightParent.transform.SetParent(m_TrafficManager.transform, false);
            }

            var trafficLightPrefab = PlateauSandboxAssetUtility.FindAssetByName<PlateauSandboxStreetFurniture>("StreetFurniture_TrafficLight_02")?.gameObject;
            //var trafficLightPrefab = Resources.Load("TrafficLightSample");

            var trafficLights = new List<TrafficLight>(GameObject.FindObjectsOfType<TrafficLight>());
            foreach (var trafficLight in trafficLights)
            {
                string gameObjectName = GameObjectUtility.GetUniqueNameForSibling(trafficLightParent.transform, trafficLightPrefab.name);
                var gameObject = (GameObject)PrefabUtility.InstantiatePrefab(trafficLightPrefab);
                gameObject.name = gameObjectName;
                gameObject.transform.position = trafficLight.transform.position;

                var (first, last) = trafficLight.GetFirstLastBorderPoints();
                var vector = (last - first).normalized;
                vector.y = 0;
                gameObject.transform.right = vector;
                gameObject.transform.SetParent(trafficLightParent.transform, false);
                trafficLight.SetRenderer(gameObject.GetComponentInChildren<Renderer>());
            }
        }

        //名前を含むCityObjectGroupをground Layerに
        void SetCityObjectAsGroundLayer(string nameContaines)
        {
            List<Transform> dems = new List<Transform>();
            PLATEAUInstancedCityModel citymodel = GameObject.FindAnyObjectByType<PLATEAUInstancedCityModel>();
            if (citymodel != null)
            {
                int len = citymodel.transform.childCount;
                for (int i = 0; i < len; i++)
                {
                    var child = citymodel.transform.GetChild(i);
                    if (child.name.Contains(nameContaines))
                    {
                        dems.Add(child);
                    }
                }
            }
            foreach (Transform trans in dems)
            {
                ChangeLayersIncludeChildren(trans, LayerMask.NameToLayer(RoadNetworkConstants.LAYER_MASK_GROUND));
            }
        }

        void ChangeLayersIncludeChildren(Transform trans, LayerMask layer)
        {
            trans.gameObject.layer = layer;
            int len = trans.childCount;
            for (int i = 0; i < len; i++)
            {
                ChangeLayersIncludeChildren(trans.GetChild(i), layer);
            }
        }

        void PostCreateSimulator()
        {
            if (RoadNetworkConstants.USE_RIGHT_OF_WAYS)
            {
                var lanes = GameObject.Find(RoadNetworkConstants.TRAFFIC_LANE_ROOT_NAME);
                for (int i = 0; i < lanes.transform.childCount; i++)
                {
                    TrafficLaneEditor.FindAndSetRightOfWays(lanes.transform.GetChild(i).GetComponent<TrafficLane>());
                }
            }

            //SetCityObjectAsGroundLayer("_tran_"); //Tranをground Layerに
            if (RoadNetworkConstants.SET_DEM_AS_GROUND_LAYER)
            {
                SetCityObjectAsGroundLayer("_dem_"); //Demをground Layerに
            }

            PlaceTrafficLights();
        }

        // 交通シミュレータ配置　実行時に呼ばれる
        public void Initialize()
        {
            ClearTrafficManager();

            PLATEAURnStructureModel roadNetwork = GameObject.FindObjectOfType<PLATEAURnStructureModel>();
            if (roadNetwork == null)
            {
                throw new System.Exception("道路ネットワークが見つかりませんでした。");
            }
            m_RoadNetworkGetter = roadNetwork.GetRoadNetworkDataGetter();

            //Attach Components
            GameObject managerGo = GameObject.Find(RoadNetworkConstants.TRAFFIC_MANAGER_NAME);
            if (managerGo == null)
            {
                managerGo = new GameObject(RoadNetworkConstants.TRAFFIC_MANAGER_NAME);
            }

            m_RnTrafficManager = managerGo.GetComponent<RoadNetworkTrafficManager>();
            if (m_RnTrafficManager == null)
            {
                m_RnTrafficManager = managerGo.AddComponent<RoadNetworkTrafficManager>();
            }

            m_TrafficManager = managerGo.GetComponent<TrafficManager>();
            if (m_TrafficManager == null)
            {
                m_TrafficManager = managerGo.AddComponent<TrafficManager>();
            }
        }
    }
}
