using AWSIM.TrafficSimulation;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Splines;
using static PlateauToolkit.Sandbox.RoadNetwork.RoadnetworkExtensions;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    public class RoadStatus
    {
        public int RoadId;

        public List<RoadInfo> m_Vehicles = new();

        public List<GameObject> m_VehiclePrefabs = new();

        public RoadStatus(int id)
        {
            RoadId = id;
        }

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
    public class RoadNetworkTrafficManager : MonoBehaviour
    {
        RoadNetworkDataGetter m_RoadNetworkGetter;
        Dictionary<int, RoadStatus> m_RoadSituation = new Dictionary<int,RoadStatus>();

        //List<PlateauSandboxTrafficMovement> m_Vehicles;
        //List<NPCVehicleInternalState> m_VehiclesStates;

        //TrafficJob m_job = new TrafficJob();
        //NPCVehicleCognitionStep m_NPCVehicleCognitionStep;


        [SerializeField]
        List<GameObject> m_VehiclePrefabs;

        [SerializeField]
        List<RoadNetworkTrafficController> m_Controllers;

        NPCVehicleSimulator m_Simulator;
        //RouteTrafficSimulator m_RouteSimulator;
        List<RouteTrafficSimulator> m_RouteSimulators = new List<RouteTrafficSimulator>();

        Coroutine m_MovementCoroutine;

        public TrafficManager TrManager
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

        void Awake()
        {
            InitializeVehicles();
        }

        void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            StartMovement();
        }

        public void SetPrefabs(List<GameObject> prefabs)
        {
            m_VehiclePrefabs = prefabs;
        }

        public void SetTrafficController(List<RoadNetworkTrafficController> controllers)
        {
            m_Controllers = controllers;
        }

        [ContextMenu("Start Movement")]
        public void StartMovement()
        {
            //var prefabs = m_Vehicles.Select(x => x.gameObject).ToArray();

            TrManager.Initialize();

            NPCVehicleConfig config = new NPCVehicleConfig();
            m_Simulator = new NPCVehicleSimulator(config, 0, 0, 100, gameObject);

            foreach (var controller in m_Controllers)
            {
                List<TrafficLane> route =  controller.CreateRoute();
                RouteTrafficSimulator routeSimulator = new RouteTrafficSimulator(gameObject, m_VehiclePrefabs.ToArray(), route.ToArray(), m_Simulator);
                m_RouteSimulators.Add(routeSimulator);

                TrManager.AddTrafficSimulator(routeSimulator);
            }


            //m_MovementCoroutine = StartCoroutine(MovementEnumerator());
        }

        [ContextMenu("Stop Movement")]
        public void Stop()
        {
            if (m_MovementCoroutine == null)
            {
                return;
            }

            //StopCoroutine(m_MovementCoroutine);
            //m_MovementCoroutine = null;
        }

        //移動処理コルーチン
        //IEnumerator MovementEnumerator()
        //{
        //    //YieldInstruction yieldFunc = new WaitForFixedUpdate();
        //    YieldInstruction yieldFunc = new WaitForEndOfFrame();

        //    foreach (var vehicle in m_Vehicles)
        //    {
        //        vehicle.PreMove();
        //    }

        //    while (Application.isPlaying)
        //    {
        //        //m_NPCVehicleCognitionStep.Execute(m_VehiclesStates, m_Vehicles.FirstOrDefault().transform);

        //        foreach (var vehicle in m_Vehicles)
        //        {
        //            if (vehicle.Move())
        //            {
        //                vehicle.PreMove();
        //            }
        //        }

        //        yield return yieldFunc;
        //    }
        //}

        public void InitializeVehicles()
        {
            //VehicleID 振り直し
            //m_Vehicles = new List<PlateauSandboxTrafficMovement>(GameObject.FindObjectsByType<PlateauSandboxTrafficMovement>(FindObjectsSortMode.None));
            //foreach (var vehicle in m_Vehicles.Select((value, index) => new { value, index }))
            //{
            //    vehicle.value.RoadInfo.m_VehicleID = vehicle.index; //Reassign ID
            //    vehicle.value.Initialize();
            //}
        }

        //暫定　ランダムにロードを抽出
        public (Vector3, RnDataRoad, RnDataLane) GetRandomRoad()
        {
            var roadNetworkRoads = RnGetter.GetRoadBases().OfType<RnDataRoad>().ToList();
            int randValue = UnityEngine.Random.Range(0, roadNetworkRoads.Count());

            RnDataRoad outRoad = roadNetworkRoads[randValue];
            RnDataLane outlane = outRoad.GetMainLanes(m_RoadNetworkGetter).First();

            if (IsRoadFilled(outRoad.GetId(RnGetter),outRoad.GetLaneIndexOfMainLanes(RnGetter,outlane), -1))
            {
                return GetRandomRoad();
            }

            RnDataLineString outLinestring = outlane.GetChildLineString(m_RoadNetworkGetter, LanePosition.Center);
            Vector3 position = outLinestring.GetChildPointsVector(m_RoadNetworkGetter).FirstOrDefault();
            return (position, outRoad, outlane);
        }

        /// <summary>
        /// IDが一番小さいRoad
        /// </summary>
        /// <returns></returns>
        public (Vector3, RnDataRoad, RnDataLane) GetStaticRoad()
        {
            var roadNetworkRoads = RnGetter.GetRoadBases().OfType<RnDataRoad>().ToList();
            if(roadNetworkRoads.TryFindMin(x => x.GetId(RnGetter), out var road)){
                RnDataRoad outRoad = road;
                RnDataLane outlane = outRoad.GetMainLanes(m_RoadNetworkGetter).First();
                RnDataLineString outLinestring = outlane.GetChildLineString(m_RoadNetworkGetter, LanePosition.Center);
                Vector3 position = outLinestring.GetChildPointsVector(m_RoadNetworkGetter).FirstOrDefault();
                return (position, outRoad, outlane);
            }
            return GetRandomRoad();
        }

        public struct RoadFilledStatus
        {
            public bool Filled;
            public int Count;
            public float lastPercent;

            public string DebugInfo;
        }

        public bool IsRoadFilled(int roadId, int laneIndex, int currentVehicleID)
        {
            RoadFilledStatus stat = new();
            stat.Filled = false;
            stat.Count = 0;
            stat.lastPercent = 0;

            if (m_RoadSituation.TryGetValue(roadId, out RoadStatus roadStat))
            {
                var roadBase = RnGetter.GetRoadBases().TryGet(roadId);
                var isRoad = (roadBase is RnDataRoad);

                List<RoadInfo> vehiclesOnTheLane = isRoad ?
                    roadStat.m_Vehicles.FindAll(x => x.m_LaneIndex == laneIndex && x.m_VehicleID != currentVehicleID).ToList() :
                    roadStat.m_Vehicles.FindAll(x => x.m_TrackIndex == laneIndex && x.m_VehicleID != currentVehicleID).ToList();

                if (vehiclesOnTheLane.TryFindMin(x => x.m_CurrentProgress, out RoadInfo firstCar))
                {
                    if(vehiclesOnTheLane.Count > 5)
                    {
                        //5台以上なら解放　暫定処置
                        return false;
                    }


                    if (vehiclesOnTheLane.Count > 2 && firstCar.m_CurrentProgress < 0.05f) // TODO : 距離判定
                    {
                        stat.Filled = true;
                        stat.Count = vehiclesOnTheLane.Count;
                        stat.lastPercent = firstCar.m_CurrentProgress;

                        stat.DebugInfo = string.Join(",", vehiclesOnTheLane.Select(x => x.m_VehicleID));
                        return true;
                    }
                }
            }
            return false;
        }

        public RoadFilledStatus GetRoadFillStatus(int roadId, int laneIndex, int currentVehicleID)
        {
            RoadFilledStatus stat = new();
            stat.Filled = false;
            stat.Count = 0;
            stat.lastPercent = 0;

            if (m_RoadSituation.TryGetValue(roadId, out RoadStatus roadStat))
            {
                var roadBase = RnGetter.GetRoadBases().TryGet(roadId);
                var isRoad = (roadBase is RnDataRoad);

                List<RoadInfo> vehiclesOnTheLane = isRoad ?
                    roadStat.m_Vehicles.FindAll(x => x.m_LaneIndex == laneIndex && x.m_VehicleID != currentVehicleID).ToList() :
                    roadStat.m_Vehicles.FindAll(x => x.m_TrackIndex == laneIndex && x.m_VehicleID != currentVehicleID).ToList();

                if (vehiclesOnTheLane.TryFindMin(x => x.m_CurrentProgress, out RoadInfo firstCar))
                {
                    if (firstCar.m_CurrentProgress < 0.05f) // TODO : 距離判定
                    {
                        stat.Filled = true;
                        stat.Count = vehiclesOnTheLane.Count;
                        stat.lastPercent = firstCar.m_CurrentProgress;

                        stat.DebugInfo = string.Join(",", vehiclesOnTheLane.Select(x => x.m_VehicleID));
                        return stat;
                    }
                }
            }
            return stat;
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
                stat = new RoadStatus(current.m_RoadId);
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
            public float m_DistanceBetweenLastCar;

            public float m_DistanceFromFirstPoint;

            public int m_NumVehiclesOncominglane; //対向車(Intersection only)
            public int m_NumVehiclesCrossing; //横断車(Intersection only)

            //public int m_NumConnectedRoads;

            public bool m_IsPriorityTrack; //直進可能

            public int m_RoadID;
            public int m_LaneIndex;

            public string m_DebugString;

            public bool m_IsValid;
        }

        public LaneStatus GetLaneInfo(RoadInfo roadInfo)
        {
            LaneStatus stat = new LaneStatus();
            stat.m_IsValid = false;

            var roadBase = RnGetter.GetRoadBases().TryGet(roadInfo.m_RoadId);
            var isRoad = (roadBase is RnDataRoad);

            if (m_RoadSituation.TryGetValue(roadInfo.m_RoadId, out RoadStatus roadStat))
            {
                stat.m_RoadID = roadInfo.m_RoadId;
                stat.m_LaneIndex = isRoad ? roadInfo.m_LaneIndex : roadInfo.m_TrackIndex;
                stat.m_NumVehiclesOnTheRoad = roadStat.m_Vehicles.Count;

                List<RoadInfo> vehiclesOnTheLane = isRoad ?
                    roadStat.m_Vehicles.FindAll(x => x.m_LaneIndex == roadInfo.m_LaneIndex && x.m_VehicleID != roadInfo.m_VehicleID).ToList() :
                    roadStat.m_Vehicles.FindAll(x => x.m_TrackIndex == roadInfo.m_TrackIndex && x.m_VehicleID != roadInfo.m_VehicleID).ToList();

                stat.m_NumVehiclesOnTheLane = vehiclesOnTheLane.Count;

                //List<RoadInfo> veheclesForward = roadInfo.m_IsReverse ?
                //    vehiclesOnTheLane.FindAll(x => x.m_CurrentProgress > roadInfo.m_CurrentProgress) :
                //    vehiclesOnTheLane.FindAll(x => x.m_CurrentProgress > roadInfo.m_CurrentProgress);
                List<RoadInfo> veheclesForward = vehiclesOnTheLane.FindAll(x => x.m_CurrentProgress > roadInfo.m_CurrentProgress);

                stat.m_NumVehiclesForward = veheclesForward.Count;

                stat.m_DebugString = string.Join(",", veheclesForward.Select(x => x.m_VehicleID));

                if (veheclesForward.TryFindMax(x => x.m_CurrentProgress, out RoadInfo lastCar))
                {
                    stat.m_LastCarProgress = lastCar.m_CurrentProgress;

                    stat.m_DistanceBetweenLastCar = Vector3.Distance(lastCar.m_CurrentPosition, roadInfo.m_CurrentPosition);

                    //bounds Collider
                    //var bounds = lastCar.GetComponentInChildren<MeshCollider>().bounds;
                    //var boundsAddition = lastCar.m_TrafficController.m_Distance / Mathf.Abs(Vector3.Distance(bounds.max, bounds.center));
                    ////Debug.Log($"boundsAddition {boundsAddition}");
                    //stat.m_LastCarProgress += boundsAddition;
                }

                //Road
                if (isRoad)
                {
                    var road = roadBase as RnDataRoad;
                    var way = road.GetChildWay(RnGetter, roadInfo.m_LaneIndex);
                    var linestring = road.GetChildLineString(RnGetter, roadInfo.m_LaneIndex);
                    var firstPoint = way?.IsReversed ?? false ? linestring?.GetChildPointsVector(RnGetter)?.LastOrDefault() : linestring.GetChildPointsVector(RnGetter)?.FirstOrDefault();
                    if(firstPoint != null)
                    {
                        stat.m_DistanceFromFirstPoint = Vector3.Distance(roadInfo.m_CurrentPosition, firstPoint.Value);
                    }
                }

                //intersection用
                if (!isRoad)
                {
                    var intersection = roadBase as RnDataIntersection;

                    var targetTrack = intersection.Tracks.TryGet(roadInfo.m_TrackIndex);
                    RnDataTrack straightTrack = intersection.GetTraksOfSameOriginByType(RnGetter, targetTrack, RnTurnType.Straight)?.FirstOrDefault();
                    if (straightTrack != null)
                    {
                        //対向車
                        var onComingTracks = intersection.GetOncomingTracks(RnGetter, straightTrack);
                        List<RoadInfo> veheclesOncomingLane = roadStat.m_Vehicles.FindAll(x => onComingTracks.Contains(intersection.Tracks.TryGet(x.m_TrackIndex)));
                        stat.m_NumVehiclesOncominglane = veheclesOncomingLane.Count;

                        //横断
                        var crossingTracks = intersection.GetCrossingTracks(RnGetter, straightTrack);
                        List<RoadInfo> veheclesCrossing = roadStat.m_Vehicles.FindAll(x => crossingTracks.Contains(intersection.Tracks.TryGet(x.m_TrackIndex)));
                        stat.m_NumVehiclesCrossing = veheclesCrossing.Count;

                        //優先トラック（直進可否）
                        var priorityTracks = new List<RnDataTrack>(crossingTracks);
                        priorityTracks.Add(straightTrack);
                        //一番IDの高いRoadと繋がるTrackが優先　暫定
                        if(priorityTracks.TryFindMax(x => intersection.GetEdgesFromBorder(RnGetter, x.GetFromBorder(RnGetter))?.FirstOrDefault()?.Road.ID ?? 0, out var priorityTrack)) 
                        {
                            stat.m_IsPriorityTrack = (priorityTrack == targetTrack);
                        }
                    }

                    stat.m_DistanceFromFirstPoint = Vector3.Distance(roadInfo.m_CurrentPosition, targetTrack.Spline.EvaluatePosition(0f));
                }

                stat.m_IsValid = true;
            }
            else
            {
               stat.m_NumVehiclesOnTheRoad = stat.m_NumVehiclesOnTheLane = 0;
            }
            return stat;
        }
    }
}
