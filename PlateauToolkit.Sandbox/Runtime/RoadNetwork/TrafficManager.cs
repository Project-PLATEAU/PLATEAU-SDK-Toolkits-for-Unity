using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PlateauToolkit.Sandbox.RoadNetwork.RoadnetworkExtensions;

namespace PlateauToolkit.Sandbox.RoadNetwork
{

    public class RoadStatus
    {
        public List<int> m_Vehecles = new List<int>();
    }

    //交通状況管理
    public class TrafficManager : MonoBehaviour
    {
        RoadNetworkDataGetter m_RoadNetworkGetter;

        Dictionary<int, PlateauSandboxTrafficMovement> m_Vehicles;
        Dictionary<int, RoadStatus> m_RoadSituation = new Dictionary<int,RoadStatus>();

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

        void Awake()
        {
            InitializeVehicles();
        }

        public void InitializeVehicles()
        {
            var vehicles = new List<PlateauSandboxTrafficMovement>(GameObject.FindObjectsByType<PlateauSandboxTrafficMovement>(FindObjectsSortMode.None));
            m_Vehicles = new();

            foreach (var vehicle in vehicles.Select((value, index) => new { value, index }))
            {
                vehicle.value.RoadInfo.m_VehicleID = vehicle.index; //Reassign ID
                m_Vehicles.Add(vehicle.value.RoadInfo.m_VehicleID, vehicle.value);
            }
        }

        //暫定　ランダムにロードを抽出
        public (Vector3, RnDataRoad, RnDataLane) GetRandomRoad()
        {
            var roadNetworkRoads = RnGetter.GetRoadBases().OfType<RnDataRoad>().ToList();
            int randValue = Random.Range(0, roadNetworkRoads.Count());

            RnDataRoad outRoad = roadNetworkRoads[randValue];
            RnDataLane outlane = outRoad.GetMainLanes(m_RoadNetworkGetter).First();
            RnDataLineString outLinestring = outlane.GetChildLineString(m_RoadNetworkGetter, LanePosition.Center);
            Vector3 position = outLinestring.GetChildPointsVector(m_RoadNetworkGetter).FirstOrDefault();
            return (position, outRoad, outlane);
        }

        public void SetRoadInfo(int fromRoadId, RoadInfo current)
        {
            if(m_RoadSituation.TryGetValue(fromRoadId, out var fromStat))
            {
                fromStat.m_Vehecles.Remove(current.m_VehicleID);
            }

            if (m_RoadSituation.TryGetValue(current.m_RoadId, out var stat))
            {
                if (!stat.m_Vehecles.Contains(current.m_VehicleID))
                {
                    stat.m_Vehecles.Add(current.m_VehicleID);
                }
            }
            else
            {
                stat = new RoadStatus();
                stat.m_Vehecles.Add(current.m_VehicleID);
                m_RoadSituation.Add(current.m_RoadId, stat);
            }
        }

        public struct LaneStatus
        {
            public int m_NumCars;
            public float m_LastCarProgress;
        }

        //isRoad : falseの場合はIntersection
        public LaneStatus GetLaneInfo(RoadInfo roadInfo, bool isRoad)
        {
            LaneStatus stat = new LaneStatus();

            if (m_Vehicles == null)
                Debug.LogError("m_Vehicles == null");

            if (m_RoadSituation.TryGetValue(roadInfo.m_RoadId, out RoadStatus roadStat))
            {
                List<PlateauSandboxTrafficMovement> veheclesOnTheLane = isRoad ?
                    roadStat.m_Vehecles.Select(x => m_Vehicles[x]).Where(x => x.RoadInfo.m_LaneIndex == roadInfo.m_LaneIndex && x.RoadInfo.m_VehicleID != roadInfo.m_VehicleID).ToList() :
                    roadStat.m_Vehecles.Select(x => m_Vehicles[x]).Where(x => x.RoadInfo.m_TrackIndex == roadInfo.m_TrackIndex && x.RoadInfo.m_VehicleID != roadInfo.m_VehicleID).ToList();

                stat.m_NumCars = veheclesOnTheLane.Count;

                PlateauSandboxTrafficMovement targetVehecle = m_Vehicles[roadInfo.m_VehicleID];
                List<PlateauSandboxTrafficMovement> veheclesForward = veheclesOnTheLane.FindAll(x => x.m_TrafficController.m_CurrentProgress < targetVehecle.m_TrafficController.m_CurrentProgress);

                if (veheclesForward.TryFindMax(x => x.m_TrafficController.m_CurrentProgress, out PlateauSandboxTrafficMovement lastCar))
                {
                    stat.m_LastCarProgress = lastCar.m_TrafficController.m_CurrentProgress;
                }
            }
            else
            {
                stat.m_NumCars = 0;
            }

            return stat;
        }
    }
}
