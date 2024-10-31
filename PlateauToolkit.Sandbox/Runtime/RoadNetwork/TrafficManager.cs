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
        public List<RoadInfo> m_Vehicles = new();

        public void Add(RoadInfo info)
        {
            m_Vehicles.Add(info);
        }

        public void Remove(int vehicleID)
        {
            m_Vehicles.RemoveAll(x => x.m_VehicleID == vehicleID);
        }
    }

    //交通状況管理 (各道路の自動車）
    public class TrafficManager : MonoBehaviour
    {
        RoadNetworkDataGetter m_RoadNetworkGetter;
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
            //VehicleID 振り直し
            var vehicles = new List<PlateauSandboxTrafficMovement>(GameObject.FindObjectsByType<PlateauSandboxTrafficMovement>(FindObjectsSortMode.None));
            foreach (var vehicle in vehicles.Select((value, index) => new { value, index }))
            {
                vehicle.value.RoadInfo.m_VehicleID = vehicle.index; //Reassign ID
            }
        }

        //暫定　ランダムにロードを抽出
        public (Vector3, RnDataRoad, RnDataLane) GetRandomRoad()
        {
            var roadNetworkRoads = RnGetter.GetRoadBases().OfType<RnDataRoad>().ToList();
            int randValue = UnityEngine.Random.Range(0, roadNetworkRoads.Count());

            RnDataRoad outRoad = roadNetworkRoads[randValue];
            RnDataLane outlane = outRoad.GetMainLanes(m_RoadNetworkGetter).First();
            RnDataLineString outLinestring = outlane.GetChildLineString(m_RoadNetworkGetter, LanePosition.Center);
            Vector3 position = outLinestring.GetChildPointsVector(m_RoadNetworkGetter).FirstOrDefault();
            return (position, outRoad, outlane);
        }

        public RnDataLane GetLaneByLottery(RnDataRoad road, List<RnDataLane> lanes)
        {
            return lanes.TryGet(UnityEngine.Random.Range(0, lanes.Count)); // Random抽選
        }

        public RnDataTrack GetTrackByLottery(RnDataIntersection intersection, List<RnDataTrack> tracks)
        {
            //var turnTypes = string.Join(",", tracks.Select(x => x.TurnType).ToList());
            //Debug.Log($"<color=yellow>turnTypes {turnTypes}</color>");

            return tracks.TryGet(UnityEngine.Random.Range(0, tracks.Count)); // Random抽選
        }

        public void RemoveRoadInfo(int fromRoadId, int vehicleID)
        {
            if (m_RoadSituation.TryGetValue(fromRoadId, out RoadStatus fromStat))
            {
                fromStat.Remove(vehicleID);
            }
        }

        public void SetRoadInfo(int fromRoadId, RoadInfo current)
        {
            RemoveRoadInfo(fromRoadId, current.m_VehicleID);
            if (m_RoadSituation.TryGetValue(current.m_RoadId, out RoadStatus stat))
            {
                if (stat.m_Vehicles.All(x => x.m_VehicleID != current.m_VehicleID))
                {
                    stat.Add(current);
                }
            }
            else
            {
                stat = new RoadStatus();
                stat.Add(current);

                m_RoadSituation.Add(current.m_RoadId, stat);
            }
        }

        public struct LaneStatus
        {
            public int m_NumVehiclesOnTheRoad;
            public int m_NumVehiclesOnTheLane;
            public int m_NumVehiclesForward;
            public float m_LastCarProgress;

            public int m_NumVehiclesOncominglane; //対向車(Intersection only)
            public int m_NumVehiclesCrossing; //横断車(Intersection only)

            //public int m_NumConnectedRoads;

            public int m_RoadID;
            public int m_LaneIndex;

            public string m_DebugString;

        }

        //isRoad : falseの場合はIntersection
        public LaneStatus GetLaneInfo(RoadInfo roadInfo, bool isRoad)
        {
            LaneStatus stat = new LaneStatus();

            if (m_RoadSituation.TryGetValue(roadInfo.m_RoadId, out RoadStatus roadStat))
            {
                stat.m_RoadID = roadInfo.m_RoadId;
                stat.m_LaneIndex = isRoad ? roadInfo.m_LaneIndex : roadInfo.m_TrackIndex;
                stat.m_NumVehiclesOnTheRoad = roadStat.m_Vehicles.Count;

                List<RoadInfo> vehiclesOnTheLane = isRoad ?
                    roadStat.m_Vehicles.FindAll(x => x.m_LaneIndex == roadInfo.m_LaneIndex && x.m_VehicleID != roadInfo.m_VehicleID).ToList() :
                    roadStat.m_Vehicles.FindAll(x => x.m_TrackIndex == roadInfo.m_TrackIndex && x.m_VehicleID != roadInfo.m_VehicleID).ToList();

                stat.m_NumVehiclesOnTheLane = vehiclesOnTheLane.Count;

                stat.m_DebugString = string.Join(",", roadStat.m_Vehicles.Select(x => x.m_VehicleID));

                List<RoadInfo> veheclesForward = vehiclesOnTheLane.FindAll(x => x.m_CurrentProgress > roadInfo.m_CurrentProgress);

                stat.m_NumVehiclesForward = veheclesForward.Count;

                if (veheclesForward.TryFindMax(x => x.m_CurrentProgress, out RoadInfo lastCar))
                {
                    stat.m_LastCarProgress = lastCar.m_CurrentProgress;

                    //bounds Collider
                    //var bounds = lastCar.GetComponentInChildren<MeshCollider>().bounds;
                    //var boundsAddition = lastCar.m_TrafficController.m_Distance / Mathf.Abs(Vector3.Distance(bounds.max, bounds.center));
                    ////Debug.Log($"boundsAddition {boundsAddition}");
                    //stat.m_LastCarProgress += boundsAddition;
                }

                //intersection用
                if (!isRoad)
                {
                    var intersection = RnGetter.GetRoadBases().TryGet(roadInfo.m_RoadId) as RnDataIntersection;

                    var targetTrack = intersection.Tracks.TryGet(roadInfo.m_TrackIndex);
                    RnDataTrack straightTrack = intersection.GetTraksOfSameOriginByType(RnGetter, targetTrack, RnTurnType.Straight)?.FirstOrDefault();
                    if(straightTrack != null)
                    {
                        //if (targetTrack.TurnType == RnTurnType.RightTurn) //とりあえず右折時のみ 
                        {
                            //対向車
                            var onComingTracks = intersection.GetOncomingTracks(RnGetter, straightTrack);
                            List<RoadInfo> veheclesOncomingLane = roadStat.m_Vehicles.FindAll(x => onComingTracks.Contains(intersection.Tracks.TryGet(x.m_TrackIndex)));
                            stat.m_NumVehiclesOncominglane = veheclesOncomingLane.Count;

                            //横断
                            var crossingTracks = intersection.GetCrossingTracks(RnGetter, straightTrack);
                            List<RoadInfo> veheclesCrossing = roadStat.m_Vehicles.FindAll(x => crossingTracks.Contains(intersection.Tracks.TryGet(x.m_TrackIndex)));
                            stat.m_NumVehiclesCrossing = veheclesCrossing.Count;
                        }
                    }
                }
            }
            else
            {
               stat.m_NumVehiclesOnTheRoad = stat.m_NumVehiclesOnTheLane = 0;
            }
            return stat;
        }
    }
}
