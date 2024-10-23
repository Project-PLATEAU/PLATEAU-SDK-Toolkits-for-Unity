
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using PlateauToolkit.Sandbox.RoadNetwork;

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

    [ExecuteAlways]
    public class PlateauSandboxTrafficMovement : PlateauSandboxMovementBase
    {
        [HideInInspector][SerializeField] RoadNetworkDataGetter m_RoadNetworkGetter;

        Coroutine m_MovementCoroutine;

        //[HideInInspector]
        [SerializeField] RoadNetworkTrafficController m_RoadParam;

        [SerializeField] public float m_SpeedKm = 40f;

        [SerializeField] RaodInfo m_RespawnPosition;


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

        RoadNetworkTrafficController CreateRoadParam(RaodInfo info)
        {
            var param = new RoadNetworkTrafficController(info);
            if(param.IsRoad)
            {
                var lineString = param.GetLineString();
                Debug.Log($"lineString count {lineString?.Points?.Count}");

                //初回配置
                var points = lineString.GetChildPointsVector(RoadNetworkGetter);
                var pos = points.FirstOrDefault();
                var nextPos = points.Count > 1 ? points[1] : pos;

                Vector3 vec = (nextPos - pos).normalized;
                gameObject.transform.position = pos;
                gameObject.transform.forward = vec;

                m_RespawnPosition = info;
            }
            else
            {
                Debug.LogError($"初回Road生成時にIntersectionが設定された");
            }
            return param;
        }

        public RoadNetworkTrafficController Respawn()
        {
            if(m_RespawnPosition == null)
                return null;

            Debug.Log($"Respawn {this.name}");
            return new RoadNetworkTrafficController(m_RespawnPosition);
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
                m_DistanceCalc = new DistanceCalculator(m_SpeedKm, m_RoadParam.GetLineString().GetTotalDistance(RoadNetworkGetter));
                m_DistanceCalc.Start();

                while (m_DistanceCalc.GetPercent() < 1)
                {
                    var points = m_RoadParam.GetLineString().GetChildPointsVector(RoadNetworkGetter);

                    if(m_RoadParam.IsLineStringReversed)
                        points.Reverse();

                    Vector3 pos = SplineTool.GetPointOnSplineDistanceBased(points, m_DistanceCalc.GetPercent());
                    //Vector3 pos = SplineTool.GetPointOnSpline(points, m_DistanceCalc.GetPercent());
                    //Vector3 pos = SplineTool.GetPointOnLine(points, m_DistanceCalc.GetPercent());
                    SetTransfrorm(pos);
                    yield return null;
                }

                //Debug.Break();
            }
            else if (m_RoadParam?.IsIntersection == true && m_RoadParam?.m_Intersection?.IsEmptyIntersection == false) // EmptyIntersectionは処理しない
            {
                RnDataTrack track = m_RoadParam.GetTrack();

                if (track == null)
                {
                    Stop();
                    yield break;
                }

                UnityEngine.Splines.Spline spline = track.Spline;

                if (m_RoadParam.IsReversed)
                {
                    spline.Knots = spline.Knots.Reverse();
                }

                m_DistanceCalc = new DistanceCalculator(m_SpeedKm, spline.GetLength());
                m_DistanceCalc.Start();

                //intersection
                while (m_DistanceCalc.GetPercent() < 1)
                {
                    var pos = SplineTool.GetPointOnSpline(spline, m_DistanceCalc.GetPercent());
                    SetTransfrorm(pos);
                    yield return null;
                }

                //Debug.Break();
            }
            else
            {
                Stop();
            }

            m_RoadParam = m_RoadParam.GetNextRoad();
            if (m_RoadParam == null)
            {
                //次が見つからない場合は、初回位置に戻る
                m_RoadParam = Respawn();
            }

            if (m_RoadParam == null)
            {
                Stop();
            }
            else
            {
                //次回移動開始
                StartMovement();
            }
        }

        //Debug Gizmo
        void OnDrawGizmos()
        {
            if (m_RoadParam == null || RoadNetworkGetter == null)
                return;
            if(m_RoadParam.IsRoad)
            {
                var points = m_RoadParam.GetLineString().GetChildPointsVector(RoadNetworkGetter);

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
                    //Vector3 pos = SplineTool.GetPointOnSpline(points, percent);
                    Vector3 pos = SplineTool.GetPointOnLine(points, percent);
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
                    var track = m_RoadParam.GetTrack();
                    Vector3 pos = SplineTool.GetPointOnSpline(track.Spline, percent);
                    if (lastpos == Vector3.zero)
                        lastpos = pos;

                    Gizmos.DrawLine(pos, lastpos);
                    lastpos = pos;
                }
            }

            //From/To Border
            if(m_RoadParam.m_FromBorder != null)
            {
                Gizmos.color = Color.blue;
                var vec = m_RoadParam.m_FromBorder.GetChildLineString(RoadNetworkGetter).GetChildPointsVector(RoadNetworkGetter);
                if(vec.Count > 0)
                {
                    Handles.Label(vec.First(), $"from :{m_RoadParam.m_FromBorder.LineString.ID}");
                    for (int k = 0; k < vec.Count - 1; k++)
                    {
                        Gizmos.DrawLine(vec[k], vec[k + 1]);
                    }
                }
            }

            if (m_RoadParam.m_ToBorder != null)
            {
                Gizmos.color = Color.cyan;
                var vec = m_RoadParam.m_ToBorder.GetChildLineString(RoadNetworkGetter).GetChildPointsVector(RoadNetworkGetter);
                if(vec.Count > 0)
                {
                    Handles.Label(vec.First(), $"to :{m_RoadParam.m_ToBorder.LineString.ID} rev {m_RoadParam.m_RoadInfo.m_IsReverse}/{m_RoadParam.GetLane()?.IsReverse::false}");
                    for (int k = 0; k < vec.Count - 1; k++)
                    {
                        Gizmos.DrawLine(vec[k], vec[k + 1]);
                    }
                }
            }

            //Debug
            if (m_RoadParam.expectedBorders?.Count > 0)
            {
                Gizmos.color = Color.green;
                Handles.color = Color.green;
                foreach (var border in m_RoadParam.expectedBorders)
                {
                    var vec = border.GetChildLineString(RoadNetworkGetter).GetChildPointsVector(RoadNetworkGetter);
                    Handles.Label(vec.First(), $"id:{border.LineString.ID}");
                    for (int k = 0; k < vec.Count - 1; k++)
                    {
                        Gizmos.DrawLine(vec[k], vec[k + 1]);
                    }
                }
            }

            if(m_RoadParam.actualBorders?.Count > 0)
            {
                Gizmos.color = Color.red;
                Handles.color = Color.red;
                foreach (var border in m_RoadParam.actualBorders)
                {
                    var vec = border.GetChildLineString(RoadNetworkGetter).GetChildPointsVector(RoadNetworkGetter);
                    Handles.Label(vec.Last(), $"id:{border.LineString.ID}");
                    for (int k = 0; k < vec.Count - 1; k++)
                    {
                        Gizmos.DrawLine(vec[k], vec[k + 1]);
                    }
                }
            }
        }

        public void SetTransfrorm(Vector3 pos)
        {
            Vector3 lastpos = gameObject.transform.position;
            Vector3 vec = (pos - lastpos).normalized;
            gameObject.transform.position = pos;
            if(vec != Vector3.zero)
            {
                gameObject.transform.forward = vec;
            }

            MovementInfo movementInfo = CreateMovementInfo(pos, vec);
            m_TrafficObject.OnMove(movementInfo);
        }

        MovementInfo CreateMovementInfo(Vector3 pos, Vector3 vec)
        {
            MovementInfo movementInfo = new MovementInfo();
            movementInfo.m_SecondAxisForward = vec;
            movementInfo.m_MoveDelta = m_SpeedKm;
            return movementInfo;
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
