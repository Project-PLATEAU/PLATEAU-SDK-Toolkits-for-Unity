using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using PlateauToolkit.Sandbox;
using PlateauToolkit.Sandbox.RoadNetwork;
using System.Collections;
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
        Dictionary<int,RoadStatus> m_RoadSituation = new Dictionary<int,RoadStatus>();

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

        void Start()
        {
            //m_RoadBases = RnGetter.GetRoadBases() as List<RnDataRoadBase>;
            var vehicles = new List<PlateauSandboxTrafficMovement>(GameObject.FindObjectsByType<PlateauSandboxTrafficMovement>(FindObjectsSortMode.None));

            m_Vehicles = new();
            foreach (var vehicle in vehicles)
            {
                m_Vehicles.Add(vehicle.m_RoadParam.m_RoadInfo.m_VehecleID, vehicle);
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

        public void SetRoadInfo(int fromId, int toId, int vehecleID)
        {
            if(m_RoadSituation.TryGetValue(fromId, out var fromStat))
            {
                fromStat.m_Vehecles.Remove(vehecleID);
            }

            if (m_RoadSituation.TryGetValue(toId, out var stat))
            {
                if (!stat.m_Vehecles.Contains(vehecleID))
                {
                    stat.m_Vehecles.Add(vehecleID);
                }
            }
            else
            {
                stat = new RoadStatus();
                stat.m_Vehecles.Add(vehecleID);
                m_RoadSituation.Add(toId, stat);
            }
        }

        public class LaneStatus
        {

        }

        public LaneStatus GetLaneInfo(int roadId, int laneIndex)
        {
            LaneStatus stat = new LaneStatus();

            if (m_RoadSituation.TryGetValue(roadId, out var roadStat))
            {
                Debug.Log($"<color=cyan>GetLaneInfo vehecles : {roadStat.m_Vehecles.Count}</color>");
            }
            //else
            //    Debug.Log($"<color=cyan>GetLaneInfo vehecles not found. traffic count {m_TrafficCircumstances.Count} </color>");


            return stat;
        }

        public LaneStatus GetTrackInfo(RnDataIntersection intersection, int trackIndex)
        {
            LaneStatus stat = new LaneStatus();




            Debug.Log($"<color=cyan>GetTrackInfo </color>");


            return stat;
        }
    }
}
