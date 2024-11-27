
using PlateauToolkit.Sandbox;
using PlateauToolkit.Sandbox.RoadNetwork;
using System.Collections.Generic;
using UnityEngine;

namespace AWSIM.TrafficSimulation
{
    /// <summary>
    /// Spawn point information of NPC vehicles.
    /// </summary>
    public struct NPCVehicleSpawnPoint
    {
        public TrafficLane Lane { get; }
        public int WaypointIndex { get; }
        public Vector3 Position { get; }
        public Vector3 Forward { get; }

        public NPCVehicleSpawnPoint(TrafficLane lane, int waypointIndex)
        {
            Lane = lane;
            WaypointIndex = waypointIndex;
            Position = lane.Waypoints[waypointIndex];

            try
            {
                Forward = waypointIndex == lane.Waypoints.Length - 1
                ? Position - lane.Waypoints[waypointIndex - 1]
                : lane.Waypoints[waypointIndex + 1] - Position;
            }catch(System.Exception ex)
            {
                Debug.LogError($"Index out of range : {waypointIndex} / {lane.Waypoints.Length}");
                Forward = Vector3.zero;
            }

        }
    }

    /// <summary>
    /// This class spawns and despawns NPC vehicles.
    /// </summary>
    public class NPCVehicleSpawner
    {
        private GameObject[] prefabs;
        private NPCVehicleSpawnPoint[] spawnPoints;
        private GameObject NPCVehicleParentsObj;

        /// <summary>
        /// Initialize <see cref="NPCVehicleSpawner"/>.
        /// </summary>
        /// <param name="prefabs">NPC vehicle prefabs to be spawned.</param>
        /// <param name="spawnableLanes">Lanes where vehicles can spawn.</param>
        public NPCVehicleSpawner(GameObject parentsObj, GameObject[] prefabs, TrafficLane[] spawnableLanes)
        {
            this.NPCVehicleParentsObj = parentsObj;
            this.prefabs = prefabs;
            this.spawnPoints = new NPCVehicleSpawnPoint[spawnableLanes.Length];
            for (var i = 0; i < spawnableLanes.Length; i++)
            {
                this.spawnPoints[i] = new NPCVehicleSpawnPoint(spawnableLanes[i], 0);
            }
        }

        /// <summary>
        /// Get random NPC vehicle prefab.
        /// </summary>
        /// <returns>NPC vehicle prefab</returns>
        public GameObject GetRandomPrefab()
            => prefabs[Random.Range(0, prefabs.Length)];

        /// <summary>
        /// Get random spawn point.
        /// </summary>
        /// <returns>Spawn point</returns>
        public NPCVehicleSpawnPoint GetRandomSpawnPoint()
            => spawnPoints[Random.Range(0, spawnPoints.Length)];

        private int currentSpawnPointIndex = 0;
        public NPCVehicleSpawnPoint GetOrderedSpawnPoint()
        {
            var currentSpawnPoint = spawnPoints[currentSpawnPointIndex];
            currentSpawnPointIndex++;
            if (spawnPoints.Length <= currentSpawnPointIndex)
                currentSpawnPointIndex = 0;
            return currentSpawnPoint;
        }

        /// <summary>
        /// Get a <see cref="NPCVehicleSpawnPoint"/> from spawnable lanes.
        /// Return false if vehicles are present in all lanes.
        /// </summary>
        /// <param name="prefab">Prefab to be spawned. Bounds of the prefab is used for collision checking.</param>
        /// <param name="npcVehicleSpawnPoint">A point that vehicles can spawn.</param>
        /// <returns>Whether there was a spawnable point.</returns>
        public bool TryGetRandomSpawnablePoint(GameObject prefab, out NPCVehicleSpawnPoint npcVehicleSpawnPoint)
        {
            //var bounds = prefab.GetComponent<NPCVehicle>().Bounds;
            var bounds = NPCVehicle.GetBounds(prefab);
            var spawnablePoints = GetSpawnablePoints(bounds);
            if (spawnablePoints.Count == 0)
            {
                npcVehicleSpawnPoint = default;
                return false;
            }
            npcVehicleSpawnPoint = spawnablePoints[Random.Range(0, spawnablePoints.Count)];
            return true;
        }

        /// <summary>
        /// Spawn NPC vehicle prefab.
        /// Use <see cref="TryGetRandomSpawnablePoint"/> to find spawn point.
        /// </summary>
        /// <param name="prefab">NPC vehicle prefab</param>
        /// <param name="npcVehicleSpawnPoint">Spawn point</param>
        /// <returns>Spawned NPC vehicle.</returns>
        public NPCVehicle Spawn(GameObject prefab, uint vehicleID, NPCVehicleSpawnPoint npcVehicleSpawnPoint)
        {
            var obj = Object.Instantiate(prefab, npcVehicleSpawnPoint.Position, Quaternion.identity);
            var bounds = NPCVehicle.GetBounds(obj);
            obj.transform.forward = npcVehicleSpawnPoint.Forward;
            obj.transform.parent = NPCVehicleParentsObj.transform;

            //Layer ïœçX (ChildÇÃColliderÇÃLayerÇïœçXÇµÇ»Ç¢Ç∆BoxcastÇ™å¯Ç©Ç»Ç¢Åj
            obj.layer = LayerMask.NameToLayer(RoadNetworkConstants.LAYER_MASK_VEHICLE);
            foreach (UnityEngine.Transform child in obj.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer(RoadNetworkConstants.LAYER_MASK_VEHICLE);
            }

            if (obj.TryGetComponent<IPlateauSandboxTrafficObject>(out _) &&
            !obj.TryGetComponent<PlateauSandboxTrafficMovement>(out _))
            {
                PlateauSandboxTrafficMovement trafficMovement = obj.AddComponent<PlateauSandboxTrafficMovement>();
            }
            var vehicle = obj.GetComponent<PlateauSandboxTrafficMovement>() as NPCVehicle;

            vehicle.SetBounds(bounds);

            vehicle.VehicleID = vehicleID;
            return vehicle;
        }

        /// <summary>
        /// Destroy <paramref name="vehicle"/>
        /// </summary>
        public void Despawn(NPCVehicle vehicle)
        {
            Object.DestroyImmediate(vehicle.gameObject);
        }

        /// <summary>
        /// Check if the vehicle with <paramref name="localBounds"/> can be spawned at <paramref name="npcVehicleSpawnPoint"/>.
        /// </summary>
        public static bool IsSpawnable(Bounds localBounds, NPCVehicleSpawnPoint npcVehicleSpawnPoint)
        {
            var rotation = npcVehicleSpawnPoint.Forward == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(npcVehicleSpawnPoint.Forward);
            var center = rotation * localBounds.center + npcVehicleSpawnPoint.Position;
            var ignoreGroundLayerMask = ~LayerMask.GetMask(RoadNetworkConstants.LAYER_MASK_GROUND);
            return !Physics.CheckBox(
                center,
                localBounds.extents,
                rotation,
                ignoreGroundLayerMask,
                QueryTriggerInteraction.Ignore);
        }
        public static bool IsSpawnable(GameObject prefab, NPCVehicleSpawnPoint npcVehicleSpawnPoint)
        {
            var bounds = NPCVehicle.GetBounds(prefab);
            return IsSpawnable(bounds, npcVehicleSpawnPoint);
        }

        /// <summary>
        /// Get all spawnable point from spawnable lanes.
        /// </summary>
        public IList<NPCVehicleSpawnPoint> GetSpawnablePoints(Bounds localBounds)
        {
            var spawnablePoints = new List<NPCVehicleSpawnPoint>();
            foreach (var spawnPoint in spawnPoints)
            {
                if (IsSpawnable(localBounds, spawnPoint))
                    spawnablePoints.Add(spawnPoint);
            }

            return spawnablePoints;
        }
    }
}
