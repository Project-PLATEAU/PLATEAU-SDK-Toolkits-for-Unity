using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using UnityEngine;
using PlateauToolkit.Sandbox.RoadNetwork;
using AWSIM.TrafficSimulation;
using UnityEditor;
using PLATEAU.CityInfo;
using AWSIM;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;

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
            //var trafficLightPrefab = PlateauSandboxAssetUtility.FindAssetByName<PlateauSandboxStreetFurniture>("StreetFurniture_TrafficLight_02")?.gameObject;
            var trafficLightPrefab = PlateauSandboxAssetUtility.FindAssetByName<PlateauSandboxStreetFurniture>("StreetFurniture_TrafficLight_Interactive_01")?.gameObject;
            //var trafficLightPrefab = Resources.Load("TrafficLightSample");
            //var trafficLightPrefab = Resources.Load("StreetFurniture_TrafficLight_Interactive_01");

            if (trafficLightPrefab == null)
            {
                EditorUtility.DisplayDialog("信号機アセットの配置に失敗しました。", "信号機アセットが見つかりませんでした。", "OK");
                return;
            }

            m_RnTrafficManager.SetTrafficLightAsset(trafficLightPrefab);
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
        //void SetReproducedRoadsAsGroundLayer()
        //{
        //    PLATEAUReproducedRoad[] reproducedRoads = GameObject.FindObjectsOfType<PLATEAUReproducedRoad>();
        //    if (reproducedRoads != null)
        //    {
        //        for (int i = 0; i < reproducedRoads.Length; i++)
        //        {
        //            ChangeLayersIncludeChildren(reproducedRoads[i].transform, LayerMask.NameToLayer(RoadNetworkConstants.LAYER_MASK_GROUND));
        //        }
        //    }
        //}


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

            //SetReproducedRoadsAsGroundLayer();

            if (RoadNetworkConstants.SET_DEM_AS_GROUND_LAYER)
            {
                SetCityObjectAsGroundLayer("_dem_"); //Demをground Layerに
            }

            if (RoadNetworkConstants.ADD_TRAFFIC_LIGHTS)
            {
                PlaceTrafficLights();
            }
        }

        // 交通シミュレータ配置　実行ボタン押下時に呼ばれる
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
