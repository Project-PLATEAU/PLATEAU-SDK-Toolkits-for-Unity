using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    public class TrafficJob:IDisposable
    {
        private const int MaxBoxcastCount = 5;

        private JobHandle obstacleCheckJobHandle;
        private JobHandle raycastJobHandle;
        private JobHandle boxcastJobHandle;
        private JobHandle calculateObstacleDistanceJobHandle;

        // Obstacle Check
        private NativeArray<BoxcastCommand> boxcastCommands;
        private NativeArray<RaycastHit> obstacleHitInfoArray;
        private NativeArray<float> obstacleDistances;

        public TrafficJob(int maxVehicleCount)
        {
            boxcastCommands = new NativeArray<BoxcastCommand>(maxVehicleCount * MaxBoxcastCount, Allocator.Persistent);
            obstacleHitInfoArray =
                new NativeArray<RaycastHit>(maxVehicleCount * MaxBoxcastCount, Allocator.Persistent);
            obstacleDistances = new NativeArray<float>(maxVehicleCount, Allocator.Persistent);
        }

        public void Execute()
        {
            obstacleCheckJobHandle =
            new ObstacleCheckJob
            {
                Commands = boxcastCommands,
                //States = nativeStates,
                //VehicleLayerMask = vehicleLayerMask,
                //Waypoints = waypoints
            }.Schedule(boxcastCommands.Length, 16);

            boxcastJobHandle =
                BoxcastCommand.ScheduleBatch(
                    boxcastCommands,
                    obstacleHitInfoArray,
                    16,
                    obstacleCheckJobHandle);

            // Start background jobs
            JobHandle.ScheduleBatchedJobs();
        }

        /// <summary>
        /// Outputs <see cref="BoxcastCommand"/> for checking front obstacles.
        /// </summary>
        private struct ObstacleCheckJob : IJobParallelFor
        {
            public LayerMask VehicleLayerMask;

            //[ReadOnly] public NativeArray<NativeState> States;
            //[ReadOnly] public NativeArray<Vector3> Waypoints;

            [WriteOnly] public NativeArray<BoxcastCommand> Commands;

            public void Execute(int index)
            {
                //var stateIndex = index / MaxBoxcastCount;
                //var waypointIndex = index % MaxBoxcastCount;
                //var waypointOffset = stateIndex * MaxWaypointCount;

                //if (waypointIndex >= States[stateIndex].WaypointCount)
                //{
                //    Commands[index] = new BoxcastCommand();
                //    return;
                //}

                //var startPoint = waypointIndex == 0
                //    ? States[stateIndex].FrontCenterPosition
                //    : Waypoints[waypointOffset + waypointIndex - 1];

                //// Reduce the detection range so that large sized vehicles can pass each other.
                //var boxCastExtents = States[stateIndex].Extents * 0.5f;
                //boxCastExtents.y *= 1;
                //boxCastExtents.z = 0.1f;
                //var endPoint = Waypoints[waypointOffset + waypointIndex];

                //var distance = Vector3.Distance(startPoint, endPoint);
                //var direction = (endPoint - startPoint).normalized;
                //var rotation = Quaternion.LookRotation(direction);
                //Commands[index] = new BoxcastCommand(
                //    startPoint,
                //    boxCastExtents,
                //    rotation,
                //    direction,
                //    distance,
                //    VehicleLayerMask
                //);
            }
        }

        public void Dispose()
        {
            var dependsOn = JobHandle.CombineDependencies(
                raycastJobHandle,
                calculateObstacleDistanceJobHandle);

            boxcastCommands.Dispose(dependsOn);
            obstacleHitInfoArray.Dispose(dependsOn);
            obstacleDistances.Dispose(dependsOn);
        }
    }

}
