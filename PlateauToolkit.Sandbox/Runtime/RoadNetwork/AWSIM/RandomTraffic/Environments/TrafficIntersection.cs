using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AWSIM;
using PlateauToolkit.Sandbox.RoadNetwork;
using static AWSIM.TrafficSimulation.TrafficIntersection;
using PLATEAU.RoadNetwork.Data;

namespace AWSIM.TrafficSimulation
{
    /// <summary>
    /// Intersection class used by RandomTraffic.
    /// Traffic light sequences and information on vehicles in the intersection.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class TrafficIntersection : MonoBehaviour
    {
        /// <summary>
        /// Grouped TrafficLightData.
        /// </summary>
        [Serializable]
        public class TrafficLightGroup
        {
            public int Group => group;
            public TrafficLight[] TrafficLights => trafficLights;

            public TrafficLightGroup(int group, List<TrafficLight> trafficLights)
            {
                this.group = group;
                this.trafficLights = trafficLights.ToArray();
            }

            public void SetBulbData(TrafficLightData.BulbData[] bulbData)
            {
                foreach (var trafficLight in TrafficLights)
                {
                    trafficLight.SetBulbData(bulbData);
                }
            }

            public void TurnOffAllLights()
            {
                foreach (var trafficLight in TrafficLights)
                {
                    trafficLight.TurnOffAllBulbs();
                }
            }

            [SerializeField] int group;
            [SerializeField] TrafficLight[] trafficLights;
        }

        /// <summary>
        /// Lighting sequence of traffic lights bulbs within this intersection.
        /// This sequence is looped.
        /// </summary>
        [Serializable]
        public class LightingSequence
        {
            /// <summary>
            /// Time for this sequence (sec)
            /// </summary>
            public float IntervalSec => intervalSec;

            /// <summary>
            /// Lighting bulb orders applied to TrafficLightGroup.
            /// </summary>
            public GroupLightingOrder[] GroupLightingOrders => groupLightingOrders;

            public LightingSequence(float intervalSec, GroupLightingOrder[] groupLightingOrders)
            {
                this.intervalSec = intervalSec;
                this.groupLightingOrders = groupLightingOrders;
            }

            public void SetIntervalSec(float val)
            {
                intervalSec = val;
            }

            [SerializeField] float intervalSec;
            [SerializeField] GroupLightingOrder[] groupLightingOrders;
        }

        /// <summary>
        /// Lighting bulb order applied to TrafficLightGroup.
        /// </summary>
        [Serializable]
        public class GroupLightingOrder
        {
            public int Group => group;
            public TrafficLightData.BulbData[] BulbData => bulbData;

            public GroupLightingOrder(int group, TrafficLightData.BulbData[] bulbData)
            {
                this.group = group;
                this.bulbData = bulbData;
            }

            public GroupLightingOrder(int group, TrafficLightData.BulbData bulbData)
            {
                this.group = group;
                this.bulbData = new TrafficLightData.BulbData[] { bulbData };
            }

            [SerializeField] int group;
            [SerializeField] TrafficLightData.BulbData[] bulbData;
        }

        [SerializeField] LayerMask colliderMask;
        [SerializeField] List<TrafficLightGroup> trafficLightGroups;
        [SerializeField] LightingSequence[] lightingSequences;
        
        public List<TrafficLightGroup> TrafficLightGroups => trafficLightGroups;
        public LightingSequence[] LightingSequences => lightingSequences;

        public List<TrafficLightGroup> TrafficLightGroups => trafficLightGroups;
        public LightingSequence[] LightingSequences => lightingSequences;

        [Header("RoadNetwork")]
        [SerializeField]
        public RnDataTrafficLightController rnTrafficLightController;

        public void AddTrafficLightGroup(int group, List<TrafficLight> trafficLights)
        {
            if(trafficLightGroups == null)
                trafficLightGroups = new List<TrafficLightGroup>();

            var trafficLightGroup = new TrafficLightGroup(group, trafficLights);
            trafficLightGroups.Add(trafficLightGroup);

            //trafficLightGroup数に応じてsequence生成
            lightingSequences = TrafficLightingSequences.GetLightingSequences(trafficLightGroups.Count, new TrafficLightingSequences.TrafficLightingParam());
        }

        public void UpdateTrafficLightSequences(float green, float yellow, float red)
        {
            lightingSequences = TrafficLightingSequences.GetLightingSequences(trafficLightGroups.Count, new TrafficLightingSequences.TrafficLightingParam(green, yellow, red));
        }

        /// <summary>
        /// Is the vehicle exist in the intersection?
        /// </summary>
        public bool VehicleExists => triggerEnterCount > 0;

        Dictionary<int, TrafficLightGroup> trafficLightGroupPairs;
        int triggerEnterCount = 0;
        BoxCollider boxCollider;

        // Start is called before the first frame update
        void Start()
        {
            trafficLightGroupPairs = trafficLightGroups.ToDictionary(x => x.Group);
            StartCoroutine(StartLightingSequences());
        }

        void Reset()
        {
            boxCollider = GetComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(6, 2, 6);
            boxCollider.center = new Vector3(0, 1.1f, 0);

            // default sequence
            var defaultSequences = TrafficLightingSequences.LightingSequencesSingleGroup(new TrafficLightingSequences.TrafficLightingParam());
            lightingSequences = defaultSequences;

            // layer and collider mask
            var vehiclelayer = LayerMask.NameToLayer(PlateauSandboxTrafficManagerConstants.LAYER_MASK_VEHICLE);
            colliderMask.value = 1 << vehiclelayer;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (CompareLayer(colliderMask, other.gameObject.layer))
            {
                triggerEnterCount++;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (CompareLayer(colliderMask, other.gameObject.layer))
            {
                triggerEnterCount--;

                if (triggerEnterCount < 0)  // fail safe
                    triggerEnterCount = 0;
            }
        }

        IEnumerator StartLightingSequences()
        {
            int i = 0;
            while (true)
            {
                foreach (GroupLightingOrder groupCommand in lightingSequences[i].GroupLightingOrders)
                {
                    if (!trafficLightGroupPairs.ContainsKey(groupCommand.Group))
                    {
                        Debug.LogError($"trafficLightGroupPairs key not found {gameObject.name} Keys:{string.Join(",", trafficLightGroupPairs.Keys)} Grp:{groupCommand.Group}");
                    }

                    var trafficLightGroup = trafficLightGroupPairs[groupCommand.Group];
                    trafficLightGroup.TurnOffAllLights();
                    trafficLightGroup.SetBulbData(groupCommand.BulbData);
                }

                yield return new WaitForSeconds(lightingSequences[i].IntervalSec);

                i++;
                if (i == lightingSequences.Length)
                    i = 0;
            }
        }

        static bool CompareLayer(LayerMask layerMask, int layer)
        {
            return ((1 << layer) & layerMask) != 0;
        }
    }
}
