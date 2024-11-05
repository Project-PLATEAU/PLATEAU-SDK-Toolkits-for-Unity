
using PLATEAU.RoadNetwork.Data;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using PlateauToolkit.Sandbox.RoadNetwork;
using System.Collections.Generic;
using UnityEngine.Splines;
using PLATEAU.Util;
using AWSIM.TrafficSimulation;

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
        public float m_SpeedKm = 30f;

        [SerializeField]
        public float m_StartOffset = 0f;

        [SerializeField] public float m_CurrentSpeed;

        TrafficManager m_TrafficManager;
        Coroutine m_MovementCoroutine;

        IPlateauSandboxTrafficObject m_TrafficObject;

        public static readonly float k_FPS = 60f;

        DistanceCalculator m_DistanceCalc;

        public float CollisionDetectRadius { get => m_CollisionDetectRadius; }

        RoadNetworkDataGetter RnGetter { get => TrafficManager?.RnGetter; }

        public NPCVehicleInternalState InternalState = new();

        [SerializeField] new Rigidbody rigidbody;
        public Transform RigidBodyTransform => rigidbody.transform;

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
                //Debug.Log($"lineString count {lineString?.Points?.Count}");

                //初回配置
                var points = lineString.GetChildPointsVector(RnGetter);
                var pos = points.FirstOrDefault();
                var nextPos = points.Count > 1 ? points[1] : pos;

                Vector3 vec = (nextPos - pos).normalized;
                gameObject.transform.position = pos;
                gameObject.transform.forward = vec;

                //m_RespawnPosition = info;
                Initialize();
            }
            else
            {
                Debug.LogError($"初回Road生成時にIntersectionが設定された");
            }
            return param;
        }

        public void Initialize()
        {

            var bounds = GetComponentInChildren<MeshCollider>()?.bounds;
            if (bounds != null)
            {
                m_TrafficController?.SetBounds(bounds.Value);
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            //debug
            //Debug.Log($"change speed {m_CurrentSpeed}");
            //m_DistanceCalc.ChangeSpeed(m_CurrentSpeed);
        }
#endif
        void Awake()
        {
            TryGetComponent(out IPlateauSandboxTrafficObject trafficObject);
            m_TrafficObject = trafficObject;

            m_TrafficController?.Initialize();
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


        //TrafficManager側で動かす場合の処理
        public void PreMove()
        {
            if (m_TrafficController == null)
            {
                Stop();
            }

            if (m_TrafficController?.IsRoad == true)
            {
                List<Vector3> points = m_TrafficController.GetLineString().GetChildPointsVector(RnGetter);
                if (m_TrafficController.IsLineStringReversed)
                {
                    points.Reverse();
                }

                //InternalState = NPCVehicleInternalState.Create(this, m_TrafficController.GetLane());

            }

            else if (m_TrafficController?.IsIntersection == true && m_TrafficController?.m_Intersection?.IsEmptyIntersection == false) // EmptyIntersectionは処理しない
            {
                RnDataTrack track = m_TrafficController.GetTrack();
                if (track == null)
                {
                    Stop();
                    return;
                }

                //InternalState = NPCVehicleInternalState.Create(this, m_TrafficController.GetTrack());

            }
            m_StartOffset = 0f; //初回のみ利用
        }

        public bool Move()
        {
            //if (m_TrafficController == null || m_DistanceCalc == null || m_Spline == null)
            //{
            //    return false;
            //}

            //if (m_DistanceCalc.GetPercent() < 1)
            //{
            //    AnimateOnSpline(m_Spline);
            //    return true;
            //}
            //else
            //{
            //    m_TrafficController = m_TrafficController.GetNextRoad();
            //}

            return false;
        }



        [ContextMenu("Start Movement")]
        public void StartMovement()
        {
            m_IsPaused = false;

            //m_MovementCoroutine = StartCoroutine(MovementEnumerator());
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
            //YieldInstruction yieldFunc = new WaitForFixedUpdate();
            YieldInstruction yieldFunc = new WaitForEndOfFrame();

            while (m_TrafficManager?.IsRoadFilled(m_TrafficController.m_RoadInfo.m_RoadId, m_TrafficController.m_RoadInfo.m_LaneIndex, m_TrafficController.m_RoadInfo.m_VehicleID) == true) //レーンがいっぱい
            {
                if (m_TrafficController?.m_DebugInfo != null)
                    m_TrafficController.m_DebugInfo.SetFillStatus(m_TrafficManager?.GetRoadFillStatus(m_TrafficController.m_RoadInfo.m_RoadId, m_TrafficController.m_RoadInfo.m_LaneIndex, m_TrafficController.m_RoadInfo.m_VehicleID) ?? default);


                //待機
                yield return yieldFunc;
            }

            if (m_TrafficController?.m_DebugInfo != null)
                m_TrafficController.m_DebugInfo.SetFillStatus(m_TrafficManager?.GetRoadFillStatus(m_TrafficController.m_RoadInfo.m_RoadId, m_TrafficController.m_RoadInfo.m_LaneIndex, m_TrafficController.m_RoadInfo.m_VehicleID) ?? default);


            Spline currentSpline = null;
            if (m_TrafficController?.IsRoad == true)
            {
                List<Vector3> points = m_TrafficController.GetLineString().GetChildPointsVector(RnGetter);
                if (m_TrafficController.IsLineStringReversed)
                {
                    points.Reverse();
                }

                currentSpline = SplineTool.CreateSplineFromPoints(points);
            }
            else if (m_TrafficController?.IsIntersection == true && m_TrafficController?.m_Intersection?.IsEmptyIntersection == false) // EmptyIntersectionは処理しない
            {
                RnDataTrack track = m_TrafficController.GetTrack();

                if (track == null)
                {
                    Stop();
                    yield break;
                }

                currentSpline = track.Spline;

                if (m_TrafficController.IsReversed)
                {
                    currentSpline.Knots = currentSpline.Knots.Reverse();
                }
            }

            if (currentSpline != null)
            {
                m_TrafficController.SetDistance(currentSpline.GetLength());

                m_DistanceCalc = new DistanceCalculator(m_SpeedKm, currentSpline.GetLength(), k_FPS);

                while (m_DistanceCalc.GetPercent() < 1)
                {
                    AnimateOnSpline(currentSpline);
                    yield return yieldFunc;
                }

                //Debug.Break();
            }

            m_StartOffset = 0f; //初回のみ利用

            m_TrafficController = m_TrafficController.GetNextRoad();

            // EmptyIntersectionは更に次へ
            if (m_TrafficController?.m_Intersection?.IsEmptyIntersection == true)
            {
                m_TrafficController = m_TrafficController.GetNextRoad();
            }

            if (m_TrafficController == null)
            {
                Stop();

                DestroyImmediate(gameObject); //破棄
            }
            else
            {
                //次回移動開始
                StartMovement();
            }
        }

        void AnimateOnSpline(Spline spline)
        {
            float percent = m_DistanceCalc.GetPercent();

            ProgressResult stat = m_TrafficController.SetProgress(percent, transform.position);
            percent = m_DistanceCalc.GetNextPercent(stat.m_Speed, stat.m_LastCarProgress);
            Vector3 pos = SplineTool.GetPointOnSpline(spline, percent);
            SetTransfrorm(pos);
        }

        void SetTransfrorm(Vector3 pos)
        {
            if (pos.IsNaNOrInfinity())
                return;

            Vector3 lastpos = gameObject.transform.position;
            Vector3 vec = (pos - lastpos).normalized;
            gameObject.transform.position = pos;
            if (vec != Vector3.zero)
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

        //Debug Gizmo
        void OnDrawGizmos()
        {
            if (m_TrafficController == null || RnGetter == null)
            {
                return;
            }

            GizmoUtil.DebugRoadNetwork(m_TrafficController, RnGetter);
            GizmoUtil.DebugVehicle(this);
        }
    }
}
