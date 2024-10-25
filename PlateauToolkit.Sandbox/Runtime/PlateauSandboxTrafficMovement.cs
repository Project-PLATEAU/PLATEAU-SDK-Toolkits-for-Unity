
using PLATEAU.RoadNetwork.Data;
using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using PlateauToolkit.Sandbox.RoadNetwork;
using static PlasticPipe.Server.MonitorStats;
using System.Collections.Generic;
using UnityEngine.Splines;

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
        [SerializeField]
        public RoadNetworkTrafficController m_TrafficController;

        [SerializeField]
        public float m_SpeedKm = 40f;

        [SerializeField]
        public float m_StartOffset = 0f;

        TrafficManager m_TrafficManager;
        Coroutine m_MovementCoroutine;

        IPlateauSandboxTrafficObject m_TrafficObject;

        DistanceCalculator m_DistanceCalc;

        RoadNetworkDataGetter RnGetter
        {
            get
            {
                return TrafficManager?.RnGetter;
            }
        }

        TrafficManager TrafficManager
        {
            get
            {
                if (m_TrafficManager == null)
                {
                    m_TrafficManager = GameObject.FindObjectOfType<TrafficManager>();
                    if (m_TrafficManager == null)
                    {
                        Debug.LogError($"TrafficManager is null");
                    }
                }
                return m_TrafficManager;
            }
        }

        public RoadInfo RoadInfo
        {
            set
            {
                m_TrafficController = CreateTrafficController(value);
            }
            get
            {
                return m_TrafficController?.m_RoadInfo;
            }
        }

        RoadNetworkTrafficController CreateTrafficController(RoadInfo info)
        {
            var param = new RoadNetworkTrafficController(info);
            if(param.IsRoad)
            {
                var lineString = param.GetLineString();
                Debug.Log($"lineString count {lineString?.Points?.Count}");

                //初回配置
                var points = lineString.GetChildPointsVector(RnGetter);
                var pos = points.FirstOrDefault();
                var nextPos = points.Count > 1 ? points[1] : pos;

                Vector3 vec = (nextPos - pos).normalized;
                gameObject.transform.position = pos;
                gameObject.transform.forward = vec;

                //m_RespawnPosition = info;
            }
            else
            {
                Debug.LogError($"初回Road生成時にIntersectionが設定された");
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

        void OnDestroy()
        {
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
            if (m_TrafficController?.IsRoad == true)
            {
                List<Vector3> points = m_TrafficController.GetLineString().GetChildPointsVector(RnGetter);
                if (m_TrafficController.IsLineStringReversed)
                {
                    points.Reverse();
                }

                Spline spline = SplineTool.CreateSplineFromPoints(points);

                m_TrafficController.SetDistance(spline.GetLength());
                //m_DistanceCalc = new DistanceCalculator(m_SpeedKm, m_TrafficController.GetLineString().GetTotalDistance(RnGetter));
                m_DistanceCalc = new DistanceCalculator(m_SpeedKm, spline.GetLength(), m_StartOffset);
                m_DistanceCalc.Start();

                while (m_DistanceCalc.GetPercent() < 1)
                {
                    float percent = m_DistanceCalc.GetPercent();

                    //Vector3 pos = SplineTool.GetPointOnSplineDistanceBased(points, percent);
                    //Vector3 pos = SplineTool.GetPointOnSpline(points, percent);
                    //Vector3 pos = SplineTool.GetPointOnLine(points, percent);
                    Vector3 pos = SplineTool.GetPointOnSpline(spline, percent);

                    SetTransfrorm(pos);

                    ProgressResult stat = m_TrafficController.SetProgress(percent);
                    SetSpeed(stat);

                    yield return null;
                }

                //Debug.Break();
            }
            else if (m_TrafficController?.IsIntersection == true && m_TrafficController?.m_Intersection?.IsEmptyIntersection == false) // EmptyIntersectionは処理しない
            {
                RnDataTrack track = m_TrafficController.GetTrack();

                if (track == null)
                {
                    Stop();
                    yield break;
                }

                Spline spline = track.Spline;

                if (m_TrafficController.IsReversed)
                {
                    spline.Knots = spline.Knots.Reverse();
                }

                m_TrafficController.SetDistance(spline.GetLength());
                m_DistanceCalc = new DistanceCalculator(m_SpeedKm, spline.GetLength(), m_StartOffset);
                m_DistanceCalc.Start();

                //intersection
                while (m_DistanceCalc.GetPercent() < 1)
                {
                    float percent = m_DistanceCalc.GetPercent();
                    Vector3 pos = SplineTool.GetPointOnSpline(spline, percent);
                    SetTransfrorm(pos);

                    ProgressResult stat = m_TrafficController.SetProgress(percent);
                    SetSpeed(stat);

                    yield return null;
                }

                //Debug.Break();
            }
            else
            {
                Stop();
            }

            m_TrafficController = m_TrafficController.GetNextRoad();

            if (m_TrafficController == null)
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
            if (m_TrafficController == null || RnGetter == null)
                return;
            if(m_TrafficController.IsRoad)
            {
                var points = m_TrafficController.GetLineString().GetChildPointsVector(RnGetter);

                Gizmos.color = Color.blue;
                for (int j = 0; j < points.Count - 1; j++)
                {
                    Gizmos.DrawLine(points[j], points[j + 1]);
                }

                Gizmos.color = Color.magenta;
                Vector3 lastpos = Vector3.zero;
                var spline = SplineTool.CreateSplineFromPoints(points);

                for (int i = 0; i < 100; i++)
                {
                    var percent = i * 0.01f;
                    //Vector3 pos = SplineTool.GetPointOnSpline(points, percent);
                    //Vector3 pos = SplineTool.GetPointOnLine(points, percent);
                    Vector3 pos = SplineTool.GetPointOnSpline(spline, percent);
                    if (lastpos == Vector3.zero)
                        lastpos = pos;

                    Gizmos.DrawLine(pos, lastpos);
                    lastpos = pos;
                }
            }
            else if(m_TrafficController.IsIntersection)
            {
                //intersection
                Gizmos.color = Color.yellow;
                Vector3 lastpos = Vector3.zero;
                for (int i = 0; i < 100; i++)
                {
                    var percent = i * 0.01f;
                    var track = m_TrafficController.GetTrack();
                    Vector3 pos = SplineTool.GetPointOnSpline(track.Spline, percent);
                    if (lastpos == Vector3.zero)
                        lastpos = pos;

                    Gizmos.DrawLine(pos, lastpos);
                    lastpos = pos;
                }
            }

            //From/To Border
            if(m_TrafficController.m_FromBorder != null)
            {
                Gizmos.color = Color.blue;
                var vec = m_TrafficController.m_FromBorder.GetChildLineString(RnGetter).GetChildPointsVector(RnGetter);
                if(vec.Count > 0)
                {
                    Handles.Label(vec.First(), $"from :{m_TrafficController.m_FromBorder.LineString.ID}");
                    for (int k = 0; k < vec.Count - 1; k++)
                    {
                        Gizmos.DrawLine(vec[k], vec[k + 1]);
                    }
                }
            }

            if (m_TrafficController.m_ToBorder != null)
            {
                Gizmos.color = Color.cyan;
                var vec = m_TrafficController.m_ToBorder.GetChildLineString(RnGetter).GetChildPointsVector(RnGetter);
                if(vec.Count > 0)
                {
                    Handles.Label(vec.First(), $"to :{m_TrafficController.m_ToBorder.LineString.ID} rev {m_TrafficController.m_RoadInfo.m_IsReverse}/{m_TrafficController.GetLane()?.IsReverse::false}");
                    for (int k = 0; k < vec.Count - 1; k++)
                    {
                        Gizmos.DrawLine(vec[k], vec[k + 1]);
                    }
                }
            }

            //Debug
            //if (m_RoadParam.expectedBorders?.Count > 0)
            //{
            //    Gizmos.color = Color.green;
            //    Handles.color = Color.green;
            //    foreach (var border in m_RoadParam.expectedBorders)
            //    {
            //        var vec = border.GetChildLineString(RnGetter).GetChildPointsVector(RnGetter);
            //        Handles.Label(vec.First(), $"id:{border.LineString.ID}");
            //        for (int k = 0; k < vec.Count - 1; k++)
            //        {
            //            Gizmos.DrawLine(vec[k], vec[k + 1]);
            //        }
            //    }
            //}

            //if(m_RoadParam.actualBorders?.Count > 0)
            //{
            //    Gizmos.color = Color.red;
            //    Handles.color = Color.red;
            //    foreach (var border in m_RoadParam.actualBorders)
            //    {
            //        var vec = border.GetChildLineString(RnGetter).GetChildPointsVector(RnGetter);
            //        Handles.Label(vec.Last(), $"id:{border.LineString.ID}");
            //        for (int k = 0; k < vec.Count - 1; k++)
            //        {
            //            Gizmos.DrawLine(vec[k], vec[k + 1]);
            //        }
            //    }
            //}
        }

        public void SetSpeed(ProgressResult result)
        {
            if (result.m_Speed != m_SpeedKm)
            {
                m_SpeedKm = result.m_Speed;
                m_DistanceCalc.ChangeSpeed(m_SpeedKm);
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
}
