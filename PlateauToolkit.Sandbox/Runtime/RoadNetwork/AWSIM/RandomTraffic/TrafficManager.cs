using PlateauToolkit.Sandbox.RoadNetwork;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GeometryUtility = AWSIM.Lanelet.GeometryUtility;

namespace AWSIM.TrafficSimulation
{
    /// <summary>
    /// Component for managing traffic simulators. Traffic manager collets all traffic simulators and manages the spawning process.
    /// - Reproducibility by Seed value
    /// </summary>
    public class TrafficManager : MonoBehaviour
    {
        [SerializeField, Tooltip("Seed value for random generator.")]
        public int seed;
        [Header("NPC Vehicle Settings")]
        [SerializeField] private NPCVehicleConfig vehicleConfig = NPCVehicleConfig.Default();

        [SerializeField, Tooltip("Vehicle layer for raytracing the collision distances.")]
        private LayerMask vehicleLayerMask;

        [SerializeField, Tooltip("Ground layer for raytracing the collision distances.")]
        private LayerMask groundLayerMask;

        [SerializeField, Tooltip("A maximum number of vehicles that can simultaneously live in the scene. Lowering this value results in less dense traffic but improves the simulator's performance.")]
        public int maxVehicleCount = 40;

        [SerializeField, Tooltip("Vehicle Root GameObject")]
        private GameObject _vehicleRoot;
        public GameObject VehicleRoot
        {
            get
            {
                if(_vehicleRoot != null)
                    return _vehicleRoot;

                return gameObject;
            }
        }

        [Header("Debug")]
        [SerializeField] protected bool showGizmos = false;
        [SerializeField] protected bool showYieldingPhase = false;
        [SerializeField] protected bool showObstacleChecking = false;
        [SerializeField] protected bool showSpawnPoints = false;
        public RandomTrafficSimulatorConfiguration[] randomTrafficSims;
        public RouteTrafficSimulatorConfiguration[] routeTrafficSims;
        public NPCVehicleSimulator npcVehicleSimulator;
        private List<ITrafficSimulator> trafficSimulatorNodes;
        private Dictionary<NPCVehicleSpawnPoint, Dictionary<ITrafficSimulator, GameObject>> spawnLanes;

        /// <summary>
        /// �eSerializeField�̏����ݒ�
        /// </summary>
        /// <param name="_vehicleLayerMask_"></param>
        /// <param name="_groundLayerMask_"></param>
        /// <param name="_maxVehicleCount_"></param>
        /// <param name="_vehicleRoot_"></param>
        /// <param name="_debug_"></param>
        public void InitParams(LayerMask _vehicleLayerMask_, LayerMask _groundLayerMask_, int _maxVehicleCount_, GameObject _vehicleRoot_, bool _debug_)
        {
            vehicleLayerMask = (1 << _vehicleLayerMask_);
            groundLayerMask = (1 << _groundLayerMask_);
            maxVehicleCount = _maxVehicleCount_;
            _vehicleRoot = _vehicleRoot_;

            if (_debug_)
            {
                showGizmos = showYieldingPhase = showObstacleChecking = true;
                //showSpawnPoints = true;
            }
        }

        /// <summary>
        /// Adds traffic simulator to the manager
        /// </summary>
        public void AddTrafficSimulator(ITrafficSimulator simulator)
        {
            trafficSimulatorNodes.Add(simulator);
        }

        /// <summary>
        /// Clears all NPC vehicles running in simulation
        /// </summary>
        public void ClearAll()
        {
            trafficSimulatorNodes.Clear();
            npcVehicleSimulator?.ClearAll();
        }

        public void Initialize()
        {
            Random.InitState(seed);

            spawnLanes = new Dictionary<NPCVehicleSpawnPoint, Dictionary<ITrafficSimulator, GameObject>>();

            npcVehicleSimulator = new NPCVehicleSimulator(vehicleConfig, vehicleLayerMask, groundLayerMask, maxVehicleCount);

            verifyIntegrationEnvironmentElements();
            trafficSimulatorNodes = new List<ITrafficSimulator>();
            if (npcVehicleSimulator == null)
            {
                Debug.LogError("Traffic manager requires NPC Vehicle Simulator script.");
                return;
            }
            if(randomTrafficSims != null)
            {
                foreach (var randomTrafficConf in randomTrafficSims)
                {
                    RandomTrafficSimulator randomTs = new RandomTrafficSimulator(
                        _vehicleRoot,
                        randomTrafficConf.npcPrefabs,
                        randomTrafficConf.spawnableLanes,
                        npcVehicleSimulator,
                        randomTrafficConf.maximumSpawns
                    );
                    randomTs.enabled = randomTrafficConf.enabled;
                    trafficSimulatorNodes.Add(randomTs);
                }
            }

            if (routeTrafficSims != null)
            {
                foreach (var routeTrafficSimConf in routeTrafficSims)
                {
                    RouteTrafficSimulator routeTs = new RouteTrafficSimulator(
                        _vehicleRoot,
                        routeTrafficSimConf.npcPrefabs,
                        routeTrafficSimConf.route,
                        npcVehicleSimulator,
                        routeTrafficSimConf.maximumSpawns
                    );
                    routeTs.enabled = routeTrafficSimConf.enabled;
                    trafficSimulatorNodes.Add(routeTs);
                }
            }
        }

        void Dispose()
        {
            npcVehicleSimulator?.Dispose();
            ClearAll();
            Despawn();
            spawnLanes.Clear();
        }

        void Start()
        {
            Initialize();
        }

        public void Restart(int newSeed = 0, int newMaxVehicleCount = 10)
        {
            Dispose();

            seed = newSeed;
            maxVehicleCount = newMaxVehicleCount;

            Initialize();
        }

        private void verifyIntegrationEnvironmentElements()
        {
            GameObject trafficLanesObject = GameObject.Find(RoadNetworkConstants.TRAFFIC_LANE_ROOT_NAME);
            if (trafficLanesObject == null)
            {
                Debug.LogError("VerifyIntegrationEnvironmentElements error: Object 'TrafficLanes' not found in the scene.");
            }

            Transform[] children = trafficLanesObject.GetComponentsInChildren<Transform>();
            HashSet<string> uniqueNames = new HashSet<string>();
            bool isAnyIntersectionLane = false;
            bool isAnyTrafficScript = false;
            foreach (Transform child in children)
            {
                var trafficScript = child.gameObject.GetComponent<TrafficLane>();
                if (trafficScript)
                {
                    isAnyTrafficScript = true;
                    if (trafficScript.intersectionLane)
                    {
                        isAnyIntersectionLane = true;
                    }
                    if (!uniqueNames.Add(child.name))
                    {
                        Debug.LogError("VerifyIntegrationEnvironmentElements error: Found repeated child name in the 'TrafficLanes' object: " + child.name);
                    }
                }
            }
            if (!isAnyIntersectionLane)
            {
                Debug.LogError("VerifyIntegrationEnvironmentElements error: Not found any TrafficLane with 'IntersectionLane' set to true.");
            }
            if (!isAnyTrafficScript)
            {
                Debug.LogError("VerifyIntegrationEnvironmentElements error: Not found any TrafficLane with 'TrafficScript'.");
            }
        }

        private void FixedUpdate()
        {
            if (trafficSimulatorNodes == null)
                return;

            // Manage NPC spawning with the traffic simulators

            // Clear null elements in current list of traffic simulator
            trafficSimulatorNodes.RemoveAll(item => item == null);

            // Find out which lanes are used by multiple spawners
            // We can easly spawn vehicle on a lane which only one spawner hooked to that lane.
            // If there are multiple spawners for one lane, we need to manage spawning. Without
            // managing, one spawner might overcome all others because of the vehicle size check.
            foreach (var trafficSimulator in trafficSimulatorNodes)
            {
                if (!trafficSimulator.IsEnabled()) continue;

                trafficSimulator.GetRandomSpawnInfo(out var spawnPoint, out var prefab);
                if (spawnLanes.ContainsKey(spawnPoint))
                {
                    spawnLanes[spawnPoint].Add(trafficSimulator, prefab);
                }
                else
                {
                    var tsims = new Dictionary<ITrafficSimulator, GameObject>();
                    tsims.Add(trafficSimulator, prefab);
                    spawnLanes.Add(spawnPoint, tsims);
                }
            }

            // For lane with single vehicle spawner - just spawn it.
            // For lane with multiple vehicle spawners - make priorities and spawn one by one.
            foreach (var spawnLoc in spawnLanes)
            {
                NPCVehicle spawnedVehicle;

                if (spawnLoc.Value.Count == 1)
                {
                    var tsimAndPrefab = spawnLoc.Value.First();
                    var trafficSim = tsimAndPrefab.Key;
                    var prefab = tsimAndPrefab.Value;
                    if (!NPCVehicleSpawner.IsSpawnable(prefab, spawnLoc.Key))
                    {
                        //Debug.LogError($"NPCVehicleSpawner not Spawnable! {spawnLoc.Key.Lane.gameObject.name}");
                        continue;
                    }
                    var spawned = trafficSim.Spawn(prefab, spawnLoc.Key, out spawnedVehicle);
                }
                else
                {
                    var priorityTrafficSimList = spawnLoc.Value.OrderByDescending(x => x.Key.GetCurrentPriority());
                    var priorityTrafficSimGo = priorityTrafficSimList.First();
                    var prefab = priorityTrafficSimGo.Value;
                    if (!NPCVehicleSpawner.IsSpawnable(prefab, spawnLoc.Key))
                    {
                        //Debug.LogError($"NPCVehicleSpawner not Spawnable!  {spawnLoc.Key.Lane.gameObject.name}");
                        continue;
                    }
                    bool spawned = priorityTrafficSimGo.Key.Spawn(prefab, spawnLoc.Key, out spawnedVehicle);
                    if (spawned)
                    {
                        foreach (var rest in priorityTrafficSimList)
                        {
                            rest.Key.IncreasePriority(1);
                        }
                        priorityTrafficSimGo.Key.ResetPriority();
                    }
                }
            }

            spawnLanes.Clear();
            npcVehicleSimulator.StepOnce(Time.fixedDeltaTime);

            Despawn();
        }
        private void Despawn()
        {
            foreach (var state in npcVehicleSimulator.VehicleStates)
            {
                if (state.ShouldDespawn)
                {
                    Object.DestroyImmediate(state.Vehicle.gameObject);
                }
            }
            npcVehicleSimulator.RemoveInvalidVehicles();
        }

        private void OnDestroy()
        {
            npcVehicleSimulator?.Dispose();
        }

        private void DrawSpawnPoints()
        {
            Gizmos.color = Color.cyan;
            foreach (var randomTrafficConf in randomTrafficSims)
            {
                foreach (var lane in randomTrafficConf.spawnableLanes)
                {
                    Gizmos.DrawCube(lane.Waypoints[0], new Vector3(2.5f, 0.2f, 2.5f));
                }
            }

            Gizmos.color = Color.magenta;
            foreach (var routeTrafficSimConf in routeTrafficSims)
            {
                if (routeTrafficSimConf.route.Length > 0)
                {
                    Gizmos.DrawCube(routeTrafficSimConf.route[0].Waypoints[0], new Vector3(2.5f, 0.2f, 2.5f));
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!showGizmos)
                return;

            var defaultColor = Gizmos.color;
            npcVehicleSimulator?.ShowGizmos(showYieldingPhase, showObstacleChecking);
            if (showSpawnPoints)
                DrawSpawnPoints();

            Gizmos.color = defaultColor;
        }
    }
}
