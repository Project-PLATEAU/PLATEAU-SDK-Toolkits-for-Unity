using System.Collections;
using UnityEngine;

namespace PlateauToolkit.Sandbox
{
    /// <summary>
    /// Define an movable object.
    /// </summary>
    interface IPlateauSandboxMovingObject : IPlateauSandboxPlaceableObject
    {
        Vector3 TransformUp { get; }
        float SecondAxisDistance => 0;

        float GetVelocityRatio(float lookAheadAngle)
        {
            return 1f;
        }

        /// <summary>
        /// An event when movement began.
        /// </summary>
        void OnMoveBegin()
        {
        }

        /// <summary>
        /// An event when movement is updated.
        /// </summary>
        /// <remarks>
        /// Then define behavior of the object in this interface.
        /// </remarks>
        void OnMove(in MovementInfo movementInfo, PlateauSandboxTrack track);

        /// <summary>
        /// An event when movement ends.
        /// </summary>
        void OnMoveEnd()
        {
        }
    }

    public struct MovementInfo
    {
        public float m_Velocity;
        public float m_MoveDelta;
        public float m_SplineContainerT;
        public Vector3 m_LookaheadForward;
        public Vector3 m_SecondAxisPoint;
        public Vector3 m_SecondAxisForward;
    }

    [ExecuteAlways]
    public class PlateauSandboxTrackMovement : MonoBehaviour
    {
        [SerializeField] PlateauSandboxTrack m_Track;

        /// <summary>Max velocity (m/s)</summary>
        [SerializeField] float m_MaxVelocity;

        /// <summary>Offset from the position of <see cref="m_Track" /></summary>
        [SerializeField] Vector3 m_TrackOffset;

        /// <summary>Offset from the current position where is the origin of collision detection</summary>
        [SerializeField] Vector3 m_CollisionDetectOriginOffset = new(0, 0, 3f);

        /// <summary>Size of a sphere of collision detection</summary>
        [SerializeField] float m_CollisionDetectRadius = 0.5f;

        /// <summary>Size of a sphere of collision detection</summary>
        [SerializeField] float m_CollisionDetectHeight = 0.5f;

        /// <summary>Distance of collision detection from its origin</summary>
        [SerializeField] float m_MinCollisionDetectDistance = 5f;

        [SerializeField] bool m_RunOnAwake = true;
        [SerializeField] bool m_IsPaused;
        [SerializeField] bool m_RandomStartPoint;
        [SerializeField] bool m_LoopOnOpenedTrack;

        [HideInInspector]
        [SerializeField] float m_SplineContainerT;

        [HideInInspector]
        [SerializeField] Object m_SerializedMovingObject;

        Coroutine m_RandomWalkCoroutine;

        IPlateauSandboxMovingObject m_MovingObject;

        float m_CurrentVelocity;
        float m_MoveDelta;
        Vector3? m_CollisionPoint;

        public bool IsCollisionPossible { get; private set; }

        public float CurrentVelocity => m_CurrentVelocity;

        public bool HasTrack => m_Track != null;

        public PlateauSandboxTrack Track
        {
            set => m_Track = value;
        }

        public bool IsMoving => m_RandomWalkCoroutine != null;

        public float SplineContainerT => m_SplineContainerT;

        public float MaxSplineContainerT => m_Track.MaxSplineContainerT;

        public bool IsPaused { get => m_IsPaused; set => m_IsPaused = value; }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_Track == null)
            {
                m_SplineContainerT = 0f;
                return;
            }
            m_SplineContainerT = Mathf.Clamp(m_SplineContainerT, 0, m_Track.MaxSplineContainerT);

            ApplyPosition();
        }
#endif

        void SetUpTarget()
        {
            TryGetComponent(out IPlateauSandboxMovingObject movingObject);

            if (movingObject == null)
            {
                Debug.LogError("移動制御が可能なオブジェクトが設定されていません");
                return;
            }

            m_MovingObject = movingObject;

            if ((Object)movingObject == m_SerializedMovingObject)
            {
                return;
            }

            switch (movingObject)
            {
                case PlateauSandboxVehicle vehicle:
                    m_MaxVelocity = 20;
                    m_SerializedMovingObject = vehicle;
                    break;
                case PlateauSandboxHuman avatar:
                    m_MaxVelocity = 1.5f;
                    m_SerializedMovingObject = avatar;
                    break;
            }
        }

        void Awake()
        {
            SetUpTarget();
        }

        void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (m_RunOnAwake)
            {
                if (m_RandomStartPoint)
                {
                    m_SplineContainerT = Random.Range(0, m_Track.MaxSplineContainerT);
                }

                StartRandomWalk();
            }
        }

        internal void ApplyPosition()
        {
            if (m_MovingObject == null)
            {
                SetUpTarget();
            }

            if (m_MovingObject == null)
            {
                return;
            }

            (Vector3 position, Vector3 forward) = m_Track.GetPositionAndForwardBySplineContainerT(m_SplineContainerT);

            // Apply the offset.
            position += transform.right * m_TrackOffset.x;
            position += transform.up * m_TrackOffset.y;
            position += transform.forward * m_TrackOffset.z;

            m_MovingObject.SetPosition(position);
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }

        public bool TrySetSplineContainerT(float t)
        {
            if (IsMoving)
            {
                return false;
            }

            m_SplineContainerT = t;
            ApplyPosition();
            return true;
        }

        readonly RaycastHit[] m_RaycastHitArray = new RaycastHit[64];

        void FixedUpdate()
        {
            if (Application.isPlaying)
            {
                if (m_CollisionPoint != null)
                {
                    (Vector3 detectionPoint, Vector3 detectionDirection, float detectionDistance) = GetCollisionDetectionPoints();
                    int hitCount;
                    if ((hitCount = Physics.SphereCastNonAlloc(detectionPoint, m_CollisionDetectRadius, detectionDirection, m_RaycastHitArray, detectionDistance)) > 0)
                    {
                        for (int i = 0; i < hitCount; i++)
                        {
                            RaycastHit hit = m_RaycastHitArray[i];

                            if (hit.collider.transform != transform &&
                                PlateauSandboxObjectFinder.TryGetSandboxObject(hit.collider, out IPlateauSandboxPlaceableObject sandboxObject))
                            {
                                if (sandboxObject.gameObject == m_MovingObject.gameObject)
                                {
                                    continue;
                                }

                                IsCollisionPossible = true;
                                return;
                            }
                        }
                    }
                }

                IsCollisionPossible = false;
            }
        }

        void OnDrawGizmos()
        {
            if (m_CollisionPoint != null)
            {
                (Vector3 detectionPoint, Vector3 detectionDirection, float detectionDistance) = GetCollisionDetectionPoints();

                Gizmos.color = IsCollisionPossible ? Color.red : Color.green;
                Gizmos.DrawRay(detectionPoint, detectionDirection * detectionDistance);
                Gizmos.DrawWireSphere(m_CollisionPoint.Value, m_CollisionDetectRadius);
            }
        }

        (Vector3, Vector3, float) GetCollisionDetectionPoints()
        {
            Debug.Assert(m_CollisionPoint != null);
            Vector3 detectionStartPoint = transform.position + transform.TransformDirection(m_CollisionDetectOriginOffset) + transform.up * 0.5f;
            Vector3 detectionDirection = m_CollisionPoint.Value - detectionStartPoint;
            float detectionDistance = detectionDirection.magnitude;

            detectionDirection.Normalize();

            return (detectionStartPoint, detectionDirection, detectionDistance);
        }

        [ContextMenu("Start Movement")]
        public void StartRandomWalk()
        {
            if (m_RandomWalkCoroutine != null)
            {
                Debug.LogWarning("既に移動を開始しています");
                return;
            }
            if (m_Track == null)
            {
                return;
            }
            if (m_MovingObject == null)
            {
                Debug.LogError("移動に対応したオブジェクトにアタッチされていません");
                return;
            }

            m_MovingObject.OnMoveBegin();

            m_IsPaused = false;
            m_RandomWalkCoroutine = StartCoroutine(RandomWalkEnumerator());
        }

        [ContextMenu("Stop Movement")]
        public void Stop()
        {
            if (m_RandomWalkCoroutine == null)
            {
                return;
            }

            StopCoroutine(m_RandomWalkCoroutine);
            m_RandomWalkCoroutine = null;
            m_MovingObject.OnMoveEnd();

            m_IsPaused = false;
        }

        /// <summary>
        /// <see cref="IEnumerator" /> to move along <see cref="PlateauSandboxTrack" />
        /// </summary>
        IEnumerator RandomWalkEnumerator()
        {
            // Prepare a random seed which decides which paths the enumerator chooses.
            int seed = Random.Range(int.MinValue, int.MaxValue);

            // Enumerator positions of movement along a track.
            TrackPathIterator randomWalkPathIterator = m_Track.GetRandomWalk(m_SplineContainerT, seed);

            while (randomWalkPathIterator.MovePoint(m_MoveDelta, out float t))
            {
                while (m_IsPaused)
                {
                    yield return null;
                }

                m_SplineContainerT = t;

                MovementInfo movementInfo;
                movementInfo.m_SplineContainerT = t;

                float secondAxisDistance = m_MovingObject.SecondAxisDistance;
                if (secondAxisDistance > 0)
                {
                    randomWalkPathIterator.Clone()
                        .MoveByLinearDistance(secondAxisDistance, out float secondAxisT);
                    (movementInfo.m_SecondAxisPoint, movementInfo.m_SecondAxisForward) =
                        m_Track.GetPositionAndForwardBySplineContainerT(secondAxisT);
                }
                else
                {
                    movementInfo.m_SecondAxisPoint = Vector3.zero;
                    movementInfo.m_SecondAxisForward = Vector3.zero;
                }

                // Get an interpolation value of the collision detection point.
                randomWalkPathIterator.Clone()
                    .MovePoint(Mathf.Max(m_CurrentVelocity, m_MinCollisionDetectDistance), out float collisionT);
                Vector3 collisionPoint = m_Track.GetPositionBySplineContainerT(collisionT);

                m_CollisionPoint = collisionPoint + m_MovingObject.TransformUp * m_CollisionDetectHeight;

                // Calculate lookahead forward and an angle between the forward and transform.forward.
                movementInfo.m_LookaheadForward = m_CollisionPoint.Value - transform.position;
                movementInfo.m_LookaheadForward.Normalize();

                // Collision Detection Slow Down
                float maxCurrentMaxVelocity;
                float timeScale;
                if (IsCollisionPossible)
                {
                    maxCurrentMaxVelocity = 0f;
                    timeScale = 2f;
                }
                else
                {
                    float lookaheadAngle = Vector3.Angle(movementInfo.m_LookaheadForward, transform.forward);
                    maxCurrentMaxVelocity = m_MaxVelocity * m_MovingObject.GetVelocityRatio(lookaheadAngle);
                    timeScale = 1f;
                }

                // Calculate current velocity of the moving object and move delta on a frame.
                // (IMPORTANT) These values will be used as parameters for the movement enumerator through the interface.
                m_CurrentVelocity = Mathf.Lerp(m_CurrentVelocity, maxCurrentMaxVelocity, Time.deltaTime * timeScale);
                m_MoveDelta = m_CurrentVelocity * Time.deltaTime;

                movementInfo.m_Velocity = m_CurrentVelocity;
                movementInfo.m_MoveDelta = m_MoveDelta;

                m_MovingObject.OnMove(movementInfo, m_Track);

                yield return null;
            }

            if (m_LoopOnOpenedTrack)
            {
                m_CurrentVelocity = 0;
                m_MoveDelta = 0;
                m_CollisionPoint = null;
                m_SplineContainerT = 0;
                ApplyPosition();
                m_RandomWalkCoroutine = StartCoroutine(RandomWalkEnumerator());
                yield break;
            }
            Stop();
        }
    }
}