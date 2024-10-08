using Codice.CM.Common;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static PlasticPipe.Server.MonitorStats;
using static PLATEAU.RoadNetwork.Structure.RnRoadEx.LaneIntersectionResult;
using static PlateauToolkit.Sandbox.RoadnetworkExtensions;
using static UnityEditor.PlayerSettings;

namespace PlateauToolkit.Sandbox
{
    interface IPlateauSandboxTrafficObject : IPlateauSandboxPlaceableObject
    {
        float SecondAxisDistance => 0;

        float GetVelocityRatio(float lookAheadAngle)
        {
            return 1f;
        }

        void OnMove(in MovementInfo movementInfo);
    }

    [Serializable]
    public class RaodInfo
    {
        [SerializeField] public RnDataRoadBase m_RoadBase;
        [SerializeField] public int m_LaneIndex;
        [SerializeField] public bool m_IsMainLane;
        [SerializeField] public LanePosition m_LanePosition;
        [SerializeField] public int m_TrackPosition;
    }

    [Serializable]
    public class RoadNetworkRoad
    {
        [SerializeField] public RaodInfo m_RoadInfo;
        [SerializeField] public RnDataRoad m_Road;
        [SerializeField] public RnDataIntersection m_Intersection;
        [SerializeField] public RnDataLineString m_LineString;

        public bool IsRoad => m_Road != null;
        public bool IsIntersection => m_Intersection != null;

        //初回
        public RoadNetworkRoad(RoadNetworkDataGetter getter, RaodInfo roadInfo)
        {
            m_RoadInfo = roadInfo;
            SetRoadBase();

            if (m_Road != null)
            {
                var road = roadInfo.m_RoadBase as RnDataRoad;
                if (roadInfo.m_IsMainLane)
                {
                    RnDataLane lane = road.GetMainLanes(getter)[roadInfo.m_LaneIndex];
                    m_LineString = lane.GetChildLineString(getter, roadInfo.m_LanePosition);
                    //lane.GetChildLineString(getter, lane.IsReverse ? LanePosition.Left : LanePosition.Right);

                }
            }
            else if(m_Intersection != null)
            {
                //intersection

                Debug.Log($"<color=yellow> TODO : Create intersection </color>");

            }
            Debug.Log($"<color=yellow>roadInfo {roadInfo.m_RoadBase} {roadInfo.m_IsMainLane} {roadInfo.m_LaneIndex} {roadInfo.m_LanePosition} </color>");
        }

        //次回作成
        public RoadNetworkRoad(RoadNetworkDataGetter getter, RoadNetworkRoad current, RnDataRoadBase next)
        {
            if (next == null)
                Debug.Log($"<color=cyan>next is null</color>");

            int trackPosition = -1;
            if (next is RnDataRoad)
            {
                var road = next as RnDataRoad;
                if (current.m_RoadInfo.m_IsMainLane)
                {
                    RnDataLane lane = road.GetMainLanes(getter)[current.m_RoadInfo.m_LaneIndex];
                    m_LineString = lane.GetChildLineString(getter, current.m_RoadInfo.m_LanePosition);
                    //lane.GetChildLineString(getter, lane.IsReverse ? LanePosition.Left : LanePosition.Right);
                }
            }
            else if (next is RnDataIntersection)
            {
                //intersection
                //var from = current.m_Road.GetMainLanes(getter)[current.m_RoadInfo.m_LaneIndex].GetChildWay(getter, current.m_RoadInfo.m_LanePosition);
                RnDataWay from = current.m_Road.GetChildWay(getter, current.m_RoadInfo.m_IsMainLane, current.m_RoadInfo.m_LaneIndex, current.m_RoadInfo.m_LanePosition);
                var intersection = next as RnDataIntersection;
                var tracks = intersection.GetFromTracks(getter, from);
                Debug.Log($"intersection from tracks {tracks.Count}");
                if(tracks.Count > 0)
                    trackPosition = intersection.Tracks.IndexOf(tracks.First());

                if(trackPosition == -1)
                {
                    var tracks2 = intersection.GetToTracks(getter, from);
                    if (tracks2.Count > 0)
                        trackPosition = intersection.Tracks.IndexOf(tracks2.First());
                }
            }

            m_RoadInfo = current.m_RoadInfo;
            m_RoadInfo.m_RoadBase = next;
            if (trackPosition > 0)
                m_RoadInfo.m_TrackPosition = trackPosition;
            bool success = SetRoadBase();

            if (!success)
            {
                var road = (RnDataRoad)next;
                var intersection = (RnDataIntersection)next;

                Debug.Log($"<color=cyan>SetRoadBase Failed {road} {intersection}</color>");
            }
        }

        private bool SetRoadBase()
        {
            if(m_RoadInfo.m_RoadBase is RnDataRoad)
            {
                m_Road = m_RoadInfo.m_RoadBase as RnDataRoad;

                Debug.Log($"<color=yellow>SetRoadBase : Road</color>");
            }
            else if (m_RoadInfo.m_RoadBase is RnDataIntersection)
            {
                m_Intersection = m_RoadInfo.m_RoadBase as RnDataIntersection;

                Debug.Log($"<color=yellow>SetRoadBase : Intersection</color>");
            }
            else
            {
                Debug.Log($"<color=yellow>SetRoadBase Failed </color>");
                return false;
            }
            return true;
        }
    }

    [ExecuteAlways]
    public class PlateauSandboxTrafficMovement : PlateauSandboxMovementBase
    {
        [HideInInspector][SerializeField] RoadNetworkDataGetter m_RoadNetworkGetter;

        Coroutine m_MovementCoroutine;

        //[HideInInspector]
        [SerializeField] RoadNetworkRoad m_RoadParam;

        [SerializeField] public float m_SpeedKm = 20f;

        IPlateauSandboxTrafficObject m_TrafficObject;

        DistanceCalculator m_DistanceCalc;

        RoadNetworkDataGetter RoadNetworkGetter
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

        public RaodInfo RoadInfo
        {
            set
            {
                m_RoadParam = CreateRoadParam(value);
            }
        }

        RoadNetworkRoad CreateRoadParam(RaodInfo info)
        {
            var param = new RoadNetworkRoad(RoadNetworkGetter, info);
            if(param.IsRoad)
            {
                var lineString = param.m_LineString;

                Debug.Log($"lineString count {lineString.Points.Count}");

                //初回配置
                var points = lineString.GetChildPointsVector(m_RoadNetworkGetter);
                var pos = points.FirstOrDefault();
                var nextPos = points.Count > 0 ? points[1] : pos;

                Vector3 vec = (nextPos - pos).normalized;
                gameObject.transform.position = pos;
                gameObject.transform.forward = vec;
            }
            return param;
        }


#if UNITY_EDITOR
        void OnValidate()
        {

        }
#endif
        void Awake()
        {
            TryGetComponent(out IPlateauSandboxTrafficObject trafficObject);
            m_TrafficObject = trafficObject;
        }

        void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (m_RunOnAwake)
            {
                StartMovement();
            }
        }

        [ContextMenu("Start Movement")]
        public void StartMovement()
        {
            //if (m_MovementCoroutine != null)
            //{
            //    Debug.LogWarning("既に移動を開始しています");
            //    return;
            //}
            //if (m_MovingObject == null)
            //{
            //    Debug.LogError("移動に対応したオブジェクトにアタッチされていません");
            //    return;
            //}

            //m_MovingObject.OnMoveBegin();

            m_IsPaused = false;

            m_MovementCoroutine = StartCoroutine(MovementEnumerator());
        }

        [ContextMenu("Stop Movement")]
        public void Stop()
        {
            if (m_MovementCoroutine == null)
            {
                return;
            }

            StopCoroutine(m_MovementCoroutine);
            m_MovementCoroutine = null;
            //m_MovingObject.OnMoveEnd();

            m_IsPaused = false;
        }


        //移動処理コルーチン
        IEnumerator MovementEnumerator()
        {
            if (m_RoadParam?.IsRoad == true)
            {
                m_DistanceCalc = new DistanceCalculator(m_SpeedKm, m_RoadParam.m_LineString.GetTotalDistance(RoadNetworkGetter));
                m_DistanceCalc.Start();

                while (m_DistanceCalc.GetPercent() < 1)
                {
                    var points = m_RoadParam.m_LineString.GetChildPointsVector(RoadNetworkGetter);
                    //Vector3 pos = SplineTool.GetPointOnSpline(points, percent);
                    Vector3 pos = SplineTool.GetPointOnSpline(points, m_DistanceCalc.GetPercent());

                    Vector3 lastpos = gameObject.transform.position;
                    Vector3 vec = (pos - lastpos).normalized;

                    gameObject.transform.position = pos;
                    gameObject.transform.forward = vec;

                    MovementInfo movementInfo = CreateMovementInfo(pos, vec);
                    m_TrafficObject.OnMove(movementInfo);

                    //percent += speed;

                    yield return null;
                }
            }
            else if (m_RoadParam?.IsIntersection == true)
            {

                var track = m_RoadParam.m_Intersection.Tracks[m_RoadParam.m_RoadInfo.m_TrackPosition];
                UnityEngine.Splines.Spline spline = track.Spline;

                m_DistanceCalc = new DistanceCalculator(m_SpeedKm, spline.GetLength());
                m_DistanceCalc.Start();

                //intersection
                while (m_DistanceCalc.GetPercent() < 1)
                {
                    //var track = m_RoadParam.m_Intersection.Tracks[m_RoadParam.m_RoadInfo.m_TrackPosition];
                    //UnityEngine.Splines.Spline spline = track.Spline;

                    //var pos = SplineTool.GetPointOnSpline(spline, percent);
                    var pos = SplineTool.GetPointOnSpline(spline, m_DistanceCalc.GetPercent());

                    Vector3 lastpos = gameObject.transform.position;
                    Vector3 vec = (pos - lastpos).normalized;

                    gameObject.transform.position = pos;
                    gameObject.transform.forward = vec;

                    MovementInfo movementInfo = CreateMovementInfo(pos, vec);
                    m_TrafficObject.OnMove(movementInfo);

                    //percent += speed;

                    yield return null;
                }
            }
            else
            {
                Stop();
            }


            m_RoadParam = GetNextRoad();
            if (m_RoadParam == null)
            {
                Stop();
            }
            else
            {
                //percent = 0;
                StartMovement();
            }
        }

        //次のRoadBaseを取得
        RoadNetworkRoad GetNextRoad()
        {
            if (m_RoadParam?.IsRoad == true)
            {
                RnDataRoad road = m_RoadParam.m_Road;
                RnDataWay way = road.GetChildWay(RoadNetworkGetter, m_RoadParam.m_RoadInfo.m_IsMainLane, m_RoadParam.m_RoadInfo.m_LaneIndex, m_RoadParam.m_RoadInfo.m_LanePosition);
                RnDataRoadBase nextRoad = (!way.IsReversed) ? road.GetNextRoad(RoadNetworkGetter) : road.GetPrevRoad(RoadNetworkGetter);

                //絶対取得
                if (nextRoad == null)
                    nextRoad = (way.IsReversed) ? road.GetNextRoad(RoadNetworkGetter) : road.GetPrevRoad(RoadNetworkGetter);

                RoadNetworkRoad nextParam = new(RoadNetworkGetter, m_RoadParam, nextRoad);

                Debug.Log($"<color=green>next road found </color>");
                return nextParam;
            }
            else if (m_RoadParam?.IsIntersection == true)
            {
                var intersection = m_RoadParam.m_Intersection;
                var track = intersection.Tracks[m_RoadParam.m_RoadInfo.m_TrackPosition];
                RnDataWay toWay = track.GetToBorder(RoadNetworkGetter);
                RnDataWay fromWay = track.GetFromBorder(RoadNetworkGetter);

                RnDataWay way = toWay.IsReversed ? fromWay : toWay;

                //絶対取得
                if (way == null)
                    way = !toWay.IsReversed ? fromWay : toWay;

                RnDataNeighbor edge = intersection.GetEdge(RoadNetworkGetter, way);
                RnDataRoadBase road = edge.GetRoad(RoadNetworkGetter);

                RoadNetworkRoad nextParam = new(RoadNetworkGetter, m_RoadParam, road);

                Debug.Log($"<color=green>next road found </color>");
                return nextParam;

            }
            else
            {
                if( m_RoadParam.m_RoadInfo.m_RoadBase is RnDataRoad )
                    Debug.Log($"<color=red>next road is road. failed to get!</color>");
                if (m_RoadParam.m_RoadInfo.m_RoadBase is RnDataIntersection)
                    Debug.Log($"<color=red>next road is intersection. failed to get!</color>");


                //var road = m_RoadParam.m_RoadInfo.m_RoadBase;
                //Debug.Log($"<color=red>road type { road.GetType().ToString() } RnDataRoad {road is RnDataRoad} RnDataIntersection {road is RnDataIntersection}  </color>");
            }

            Debug.Log($"<color=red>next road not found </color>");
            return null;
        }

        void OnDrawGizmos()
        {
            if (m_RoadParam == null || RoadNetworkGetter == null)
                return;
            if(m_RoadParam.IsRoad && m_RoadParam.m_LineString != null)
            {
                var points = m_RoadParam.m_LineString.GetChildPointsVector(RoadNetworkGetter);

                Gizmos.color = Color.blue;
                for (int j = 0; j < points.Count - 1; j++)
                {
                    Gizmos.DrawLine(points[j], points[j + 1]);
                }

                Gizmos.color = Color.magenta;
                Vector3 lastpos = Vector3.zero;
                for (int i = 0; i < 100; i++)
                {
                    var percent = i * 0.01f;
                    Vector3 pos = SplineTool.GetPointOnSpline(points, percent);
                    if (lastpos == Vector3.zero)
                        lastpos = pos;

                    Gizmos.DrawLine(pos, lastpos);
                    lastpos = pos;
                }
            }
            else if(m_RoadParam.IsIntersection)
            {
                //intersection
                Gizmos.color = Color.yellow;
                Vector3 lastpos = Vector3.zero;
                for (int i = 0; i < 100; i++)
                {
                    var percent = i * 0.01f;
                    var track = m_RoadParam.m_Intersection.Tracks[m_RoadParam.m_RoadInfo.m_TrackPosition];
                    Vector3 pos = SplineTool.GetPointOnSpline(track.Spline, percent);

                    //var points = track.GetToLineString(RoadNetworkGetter).GetChildPointsVector(RoadNetworkGetter);
                    //Vector3 pos = SplineTool.GetPointOnSpline(points, percent);

                    if (lastpos == Vector3.zero)
                        lastpos = pos;

                    Gizmos.DrawLine(pos, lastpos);
                    lastpos = pos;
                }
            }
        }

        MovementInfo CreateMovementInfo(Vector3 pos, Vector3 vec)
        {
            MovementInfo movementInfo = new MovementInfo();
            movementInfo.m_SecondAxisForward = vec;
            //movementInfo.m_MoveDelta = speed * 100f;
            movementInfo.m_MoveDelta = m_SpeedKm;

            return movementInfo;
        }
    }

    //Start後の経過時間から移動パーセント(0-1f)を計測
    public class DistanceCalculator
    {
        float m_SpeedKmPerHour = 0f;
        float m_SpeedMetersPerSecond = 0f;
        float m_TotalDistanceMeters = 0f;
        float m_StartTime = 0f;

        public DistanceCalculator(float speed, float distance)
        {
            m_SpeedKmPerHour = speed;
            m_SpeedMetersPerSecond = (m_SpeedKmPerHour * 1000f) / 3600f;
            m_TotalDistanceMeters = distance;
            Start();
        }

        public void Start()
        {
            m_StartTime = Time.time;
        }

        public float GetPercent()
        {
            float elapsedTimeSeconds = Time.time - m_StartTime;
            float progressPercentage = 1f;

            if (elapsedTimeSeconds * m_SpeedMetersPerSecond < m_TotalDistanceMeters)
            {
                // 経過時間に基づく移動距離
                float currentDistance = elapsedTimeSeconds * m_SpeedMetersPerSecond;

                // 距離の進捗パーセンテージ
                progressPercentage = (currentDistance / m_TotalDistanceMeters);

                // 経過時間と進行状況を出力
                //Debug.Log($"経過時間: {elapsedTimeSeconds} 秒, 移動距離: {currentDistance} m, 進行状況: {progressPercentage}%");
            }
            return progressPercentage;
        }
    }

    /// <summary>
    /// Iterates a position to move along a track.
    /// </summary>
    public struct LineStringIterator
    {
        bool MoveNextPath()
        {
            return true;
        }
        public bool MovePoint(float delta, out float t)
        {
            t = 0f;
            return true;
        }
    }
}
