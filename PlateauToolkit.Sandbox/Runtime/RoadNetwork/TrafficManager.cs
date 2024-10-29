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
        public List<int> m_VehiclesOnRoad = new List<int>();
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

        public RnDataLane GetLaneByLottery(RnDataRoad road, List<RnDataLane> lanes)
        {
            return lanes.TryGet(UnityEngine.Random.Range(0, lanes.Count)); // Random抽選
        }

        public RnDataTrack GetTrackByLottery(RnDataIntersection intersection, List<RnDataTrack> tracks)
        {
            return tracks.TryGet(UnityEngine.Random.Range(0, tracks.Count)); // Random抽選
        }

        public void SetRoadInfo(int fromRoadId, RoadInfo current)
        {
            if(m_RoadSituation.TryGetValue(fromRoadId, out var fromStat))
            {
                fromStat.m_VehiclesOnRoad.Remove(current.m_VehicleID);
            }

            if (m_RoadSituation.TryGetValue(current.m_RoadId, out var stat))
            {
                if (!stat.m_VehiclesOnRoad.Contains(current.m_VehicleID))
                {
                    stat.m_VehiclesOnRoad.Add(current.m_VehicleID);
                }
            }
            else
            {
                stat = new RoadStatus();
                stat.m_VehiclesOnRoad.Add(current.m_VehicleID);
                m_RoadSituation.Add(current.m_RoadId, stat);
            }
        }

        public struct LaneStatus
        {
            public int m_NumVehicles;
            public int m_NumVehiclesForward;
            public float m_LastCarProgress;

            public int m_NumVehiclesOncominglane; //対向車(Intersection only)
            public int m_NumVehiclesCrossing; //横断車(Intersection only)

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
                    roadStat.m_VehiclesOnRoad.Select(x => m_Vehicles[x]).Where(x => x.RoadInfo.m_LaneIndex == roadInfo.m_LaneIndex && x.RoadInfo.m_VehicleID != roadInfo.m_VehicleID).ToList() :
                    roadStat.m_VehiclesOnRoad.Select(x => m_Vehicles[x]).Where(x => x.RoadInfo.m_TrackIndex == roadInfo.m_TrackIndex && x.RoadInfo.m_VehicleID != roadInfo.m_VehicleID).ToList();

                stat.m_NumVehicles = veheclesOnTheLane.Count;

                PlateauSandboxTrafficMovement targetVehecle = m_Vehicles[roadInfo.m_VehicleID];
                List<PlateauSandboxTrafficMovement> veheclesForward = veheclesOnTheLane.FindAll(x => x.m_TrafficController.m_CurrentProgress > targetVehecle.m_TrafficController.m_CurrentProgress);

                stat.m_NumVehiclesForward = veheclesForward.Count;

                if (veheclesForward.TryFindMax(x => x.m_TrafficController.m_CurrentProgress, out PlateauSandboxTrafficMovement lastCar))
                {
                    stat.m_LastCarProgress = lastCar.m_TrafficController.m_CurrentProgress;

                    //bounds Colliderがない
                    //var bounds = lastCar.GetComponent<Collider>().bounds;
                    //var boundsAddition = lastCar.m_TrafficController.m_Distance / Mathf.Abs(Vector3.Distance(bounds.max, bounds.center));
                    //Debug.Log($"boundsAddition {boundsAddition}");
                    //stat.m_LastCarProgress += boundsAddition;
                }

                //intersection用
                if (!isRoad)
                {
                    if (m_Vehicles.TryGetValue(roadInfo.m_VehicleID, out PlateauSandboxTrafficMovement currentVehicle))
                    {
                        var intersection = RnGetter.GetRoadBases().TryGet(roadInfo.m_RoadId) as RnDataIntersection;

                        var targetTrack = intersection.Tracks.TryGet(roadInfo.m_TrackIndex);
                        var veheclesOnTheRoad = roadStat.m_VehiclesOnRoad.Select(x => m_Vehicles[x]).ToList();
                        if (targetTrack.TurnType != RnTurnType.Straight)
                        {
                            //if (targetTrack.TurnType == RnTurnType.RightTurn) //とりあえず右折時のみ 
                            {
                                //対向車 (fromとtoが逆）
                                var fromBorder = currentVehicle.m_TrafficController.m_FromBorder;
                                List<PlateauSandboxTrafficMovement> veheclesOncomingLane = veheclesOnTheRoad.FindAll(x => x.m_TrafficController.m_ToBorder.IsSameLine(fromBorder));
                                stat.m_NumVehiclesOncominglane = veheclesOncomingLane.Count;

                                //横断( from / to が一致しない )
                                //List<PlateauSandboxTrafficMovement> veheclesCrossing = veheclesOnTheRoad.FindAll(x => !x.m_TrafficController.m_ToBorder.IsSameLine(fromBorder) && !x.m_TrafficController.m_FromBorder.IsSameLine(fromBorder));
                                //stat.m_NumVehiclesCrossing = veheclesCrossing.Count;

                                var crossingTracks = intersection.GetCrossingTracks(RnGetter, targetTrack);
                                List<PlateauSandboxTrafficMovement> veheclesCrossing = veheclesOnTheRoad.FindAll(x => crossingTracks.Contains(x.m_TrafficController.GetTrack()));
                                stat.m_NumVehiclesCrossing = veheclesCrossing.Count;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"m_Vehicles not found {roadInfo.m_VehicleID}");
                    }
                }
            }
            else
            {
                stat.m_NumVehicles = 0;
            }

            return stat;
        }
    }
}
