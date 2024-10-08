using System.Collections;
using UnityEngine;

namespace PlateauToolkit.Sandbox
{
    public class PlateauSandboxMovementBase : MonoBehaviour
    {
        /// <summary>Max velocity (m/s)</summary>
        [SerializeField] protected float m_MaxVelocity;

        /// <summary>Offset from the position of <see cref="m_Track" /></summary>
        [SerializeField] protected Vector3 m_TrackOffset;

        /// <summary>Offset from the current position where is the origin of collision detection</summary>
        [SerializeField] protected Vector3 m_CollisionDetectOriginOffset = new(0, 0, 3f);

        /// <summary>Size of a sphere of collision detection</summary>
        [SerializeField] protected float m_CollisionDetectRadius = 0.5f;

        /// <summary>Size of a sphere of collision detection</summary>
        [SerializeField] protected float m_CollisionDetectHeight = 0.5f;

        /// <summary>Distance of collision detection from its origin</summary>
        [SerializeField] protected float m_MinCollisionDetectDistance = 5f;

        [SerializeField] protected bool m_RunOnAwake = true;
        [SerializeField] protected bool m_IsPaused;
        [SerializeField] protected bool m_RandomStartPoint;
        [SerializeField] protected bool m_LoopOnOpenedTrack;

        protected float m_CurrentVelocity;
        protected float m_MoveDelta;
        protected Vector3? m_CollisionPoint;

        public bool IsCollisionPossible { get; protected set; }

        public float CurrentVelocity => m_CurrentVelocity;

        public bool IsPaused { get => m_IsPaused; set => m_IsPaused = value; }

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
                                //if (sandboxObject.gameObject == m_TrafficObject.gameObject)
                                if (sandboxObject.gameObject == gameObject)
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

        (Vector3, Vector3, float) GetCollisionDetectionPoints()
        {
            Debug.Assert(m_CollisionPoint != null);
            Vector3 detectionStartPoint = transform.position + transform.TransformDirection(m_CollisionDetectOriginOffset) + transform.up * 0.5f;
            Vector3 detectionDirection = m_CollisionPoint.Value - detectionStartPoint;
            float detectionDistance = detectionDirection.magnitude;

            detectionDirection.Normalize();

            return (detectionStartPoint, detectionDirection, detectionDistance);
        }
    }
}
