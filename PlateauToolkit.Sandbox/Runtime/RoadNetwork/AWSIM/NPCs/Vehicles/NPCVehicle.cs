using UnityEngine;
using PlateauToolkit.Sandbox;
using PlateauToolkit.Sandbox.RoadNetwork;
using System;
using AWSIM.TrafficSimulation;

namespace AWSIM
{
    /// <summary>
    /// NPC Vehicle class.
    /// Controlled by Position and Rotation.
    /// </summary>
    public class NPCVehicle : MonoBehaviour
    {
        public enum TurnSignalState
        {
            OFF,
            LEFT,
            RIGHT,
            HAZARD,
        }

        [SerializeField]
        GameObject visualObjectRoot;

        //Debug情報 NPCVehicleInternalState
        [Serializable]
        public class VehicleParameters
        {
            public NPCVehicleSpeedMode speedMode;
            public string timeRemains;
            public TrafficLane followingLane;
            public TrafficLane prevLane;

            public TrafficLightPassability trafficLightPassability;

            public int followingLanesCount;

            public void SetStatus(NPCVehicleInternalState state)
            {
                speedMode = state.SpeedMode;
                if (speedMode != NPCVehicleSpeedMode.NORMAL && speedMode != NPCVehicleSpeedMode.SLOW && state.SpeedModeStopStartTime != 0f)
                    timeRemains = $"{Time.time - state.SpeedModeStopStartTime}/{state.GetMaxIdleTime()}";
                else
                    timeRemains = "-";

                if(state.CurrentFollowingLane != followingLane)
                {
                    prevLane = followingLane;
                    followingLane = state.CurrentFollowingLane;
                }

                trafficLightPassability = state.TrafficLightPassability;

                followingLanesCount = state.FollowingLanes.Count;
            }
        }

        //debug情報
        [Header("Debug InternalStateInfo")]
        [SerializeField]
        public VehicleParameters status = new VehicleParameters();

        public void SetStatus(NPCVehicleInternalState state)
        {
            status.SetStatus(state);
        }

        /// <summary>
        /// Current visualObject's activeself
        /// </summary>
        public bool VisualObjectRootActiveSelf => visualObjectRoot.activeSelf;

        /// <summary>
        /// Vehicle bounding box.
        /// </summary>
        public Bounds Bounds => bounds;

        /// <summary>
        /// Vehicle ID.
        /// </summary>
        public uint VehicleID { get; set; }

        // dynamics settings const values.
        const float maxSteerAngle = 40f;                    // deg
        const float maxSteerSpeed = 60f;                    // deg/s
        const float maxVerticalSpeed = 40;                  // m/s
        const float maxSlope = 45;                          // deg

        Rigidbody rigidbody {
            get
            {
                if (gameObject.TryGetComponent<Rigidbody>(out var _rb))
                {
                    return _rb;
                }
                else
                {
                    var rigidbody = gameObject.AddComponent<Rigidbody>();
                    rigidbody.mass = 3000;
                    rigidbody.drag = 0;
                    rigidbody.angularDrag = 0;
                    rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                    rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    rigidbody.automaticCenterOfMass = true;
                    rigidbody.useGravity = true;
                    rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                    return rigidbody;
                }
            }
        }

        [Header("Bounding box Settngs")]
        [SerializeField] Bounds bounds;

        TurnSignalState turnSignalState = TurnSignalState.OFF;
        float turnSignalTimer = 0;
        bool currentTurnSignalOn = false;

        float wheelbase;        // m
        float acceleration;     // m/s^2
        Vector3 velocity;       // m/s
        float speed;            // m/s (forward only)
        float yawAngularSpeed;  // deg/s (yaw only)

        Vector3 lastVelocity;
        Vector3 lastPosition;
        float lastEulerAnguleY;
        float lastSpeed;

        IPlateauSandboxTrafficObject m_TrafficObject;

        public Transform RigidBodyTransform => rigidbody.transform;
        public Transform TrailerTransform => null;

        public static Bounds GetBounds(GameObject prefab)
        {
            Bounds bounds;
            var meshCollider = prefab.GetComponentInChildren<MeshCollider>();
            if (meshCollider != null)
            {
                bounds = meshCollider.bounds;
            }
            else
            {
                var renderer = prefab.GetComponentInChildren<Renderer>();
                bounds = renderer.bounds;
            }

            if (bounds.size == Vector3.zero)
            {
                bounds = new Bounds(Vector3.zero, Vector3.zero);
                MeshFilter[] mfs = prefab.GetComponentsInChildren<MeshFilter>();
                foreach (MeshFilter mf in mfs)
                {
                    Vector3 pos = mf.transform.localPosition;
                    Bounds child_bounds = mf.sharedMesh.bounds;
                    child_bounds.center += pos;
                    bounds.Encapsulate(child_bounds);
                }
            }

            bounds.center = bounds.center - prefab.transform.position;

            return bounds;
        }

        public void SetBounds(Bounds b)
        {
            bounds = b;
        }

        protected void Initialize(GameObject _visualObjectRoot_)
        {
            visualObjectRoot = _visualObjectRoot_;

            if (visualObjectRoot.TryGetComponent<IPlateauSandboxTrafficObject>(out IPlateauSandboxTrafficObject trafficObject))
            {
                m_TrafficObject = trafficObject;
                wheelbase = m_TrafficObject.GetWheelBase();
            }

            var collider = visualObjectRoot.GetComponentInChildren<Collider>();
            var physicMaterial = Resources.Load<PhysicMaterial>("VehiclePhisicMaterial");
            if (collider != null)
            {
                collider.excludeLayers = ~LayerMask.GetMask(PlateauSandboxTrafficManagerConstants.LAYER_MASK_GROUND);  // Everything but ground
                collider.material = physicMaterial;
            }
            else
            {
                var boxCollider =  visualObjectRoot.AddComponent<BoxCollider>();
                boxCollider.excludeLayers = ~LayerMask.GetMask(PlateauSandboxTrafficManagerConstants.LAYER_MASK_GROUND); // Everything but ground
                boxCollider.material = physicMaterial;
            }
            lastPosition = rigidbody.position;
        }

        // Start is called before the first frame update
        void Awake()
        {
        }

        // Update is called once per frame
        void Update()
        {
            // Update Wheel visuals.
            var steerAngle = CalcSteerAngle(speed, yawAngularSpeed, wheelbase);

            m_TrafficObject.UpdateVisual(speed, steerAngle);

            // --- inner methods ---
            static float CalcSteerAngle(float speed, float yawAngularSpeed, float wheelBase)
            {
                yawAngularSpeed *= Mathf.Deg2Rad;

                if (Mathf.Abs(yawAngularSpeed) < 0.01f || Mathf.Abs(speed) < 0.01f)
                {
                    return 0f;
                }
                var gyrationRadius = speed / Mathf.Tan(yawAngularSpeed);
                var yaw = Mathf.Asin(Mathf.Clamp(wheelBase / gyrationRadius, -1f, 1f)) * Mathf.Rad2Deg;
                yaw = Mathf.Clamp(yaw, -maxSteerAngle, maxSteerAngle);

                return yaw;
            }
        }

        void FixedUpdate()
        {
            // Calculate physical states for visual update.
            // velocity & speed.
            velocity = (rigidbody.position - lastPosition) / Time.deltaTime;

            speed = Vector3.Dot(velocity, transform.forward);

            // accleration.
            acceleration = (speed - lastSpeed) / Time.deltaTime;

            // yaw angular speed.
            yawAngularSpeed = (rigidbody.rotation.eulerAngles.y - lastEulerAnguleY) / Time.deltaTime;

            // TODO: set WheelCollider steer angle?

            // Cache current frame values.
            lastPosition = rigidbody.position;
            lastVelocity = velocity;
            lastEulerAnguleY = rigidbody.rotation.eulerAngles.y;
            lastSpeed = speed;
        }

        void OnDestroy()
        {
        }

        /// <summary>
        /// Move the vehicle so that its x,z coordinates are the same as <paramref name="position"/>.<br/>
        /// Vertical movement is determined by physical operations that take effects of suspension and gravity into account.<br/>
        /// This method should be called from FixedUpdate because <see cref="Rigidbody"/> is updated internally.
        /// </summary>
        /// <param name="position">New position of the vehicle.</param>
        public void SetPosition(Vector3 position)
        {
            rigidbody.MovePosition(new Vector3(position.x, rigidbody.position.y, position.z));
            var velocityY = Mathf.Min(rigidbody.velocity.y, maxVerticalSpeed);
            rigidbody.velocity = new Vector3(0, velocityY, 0);
        }

        /// <summary>
        /// Rotate the vehicle so that its yaw is equal to that of <paramref name="rotation"/>.<br/>
        /// Pitch and roll are determined by physical operations that take effects of suspension and gravity into account.<br/>
        /// This method should be called from FixedUpdate because <see cref="Rigidbody"/> is updated internally.
        /// </summary>
        /// <param name="rotation">New rotation of the vehicle.</param>
        public void SetRotation(Quaternion rotation)
        {
            var inputAngles = rotation.eulerAngles;
            var rigidbodyAngles = rigidbody.rotation.eulerAngles;
            var pitch = ClampDegree360(rigidbodyAngles.x, maxSlope);
            var roll = ClampDegree360(rigidbodyAngles.z, maxSlope);
            rigidbody.MoveRotation(Quaternion.Euler(pitch, inputAngles.y, roll));
            var angularVelocity = rigidbody.angularVelocity;
            rigidbody.angularVelocity = new Vector3(angularVelocity.x, 0f, angularVelocity.z);

            static float ClampDegree360(float value, float maxAbsValue)
            {
                if (value < 360f - maxAbsValue && value > 180f)
                {
                    return 360f - maxAbsValue;
                }

                if (value > maxAbsValue && value <= 180f)
                {
                    return maxAbsValue;
                }

                return value;
            }
        }

        /// Set <see cref="TurnSignalState"/> and turn on/off blinkers according to the value of <see cref="TurnSignalState"/>.
        /// </summary>
        /// <param name="state">New value of <see cref="TurnSignalState"/>.</param>
        /// <exception cref="InvalidEnumArgumentException">Exception when <paramref name="state"/> is invalid value.</exception>
        public void SetTurnSignalState(TurnSignalState turnSignalState)
        {
            if (this.turnSignalState != turnSignalState)
                this.turnSignalState = turnSignalState;
        }

        /// <summary>
        /// Visual objects on/off
        /// </summary>
        /// <param name="isActive">visual on/off</param>
        public void SetActiveVisualObjects(bool isActive)
        {
            if (visualObjectRoot.activeSelf == isActive)
                return;

            visualObjectRoot.SetActive(isActive);
        }

        // Draw bounding box 
        private void OnDrawGizmos()
        {
            // Cache Gizmos default values.
            var cacheColor = Gizmos.color;
            var cacheMatrix = Gizmos.matrix;

            // Apply color and matrix.
            Gizmos.color = Color.white;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

            // Draw wire cube.
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            // Return to default value.
            Gizmos.color = cacheColor;
            Gizmos.matrix = cacheMatrix;
        }
    }
}
