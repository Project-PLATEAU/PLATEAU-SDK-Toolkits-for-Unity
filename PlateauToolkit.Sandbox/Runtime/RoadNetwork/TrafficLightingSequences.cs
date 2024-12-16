using AWSIM;
using System.Collections.Generic;
using static AWSIM.TrafficSimulation.TrafficIntersection;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    public class TrafficLightingSequences
    {
        /// <summary>
        /// 青黄赤バルブ信号機用シーケンス
        /// </summary>
        /// <param name="numGroups"></param>
        /// <returns></returns>
        public static LightingSequence[] GetLightingSequences(int numGroups)
        {
            if (numGroups >= 2)
                return LightingSequencesDynamicGroups(numGroups);

            return LightingSequencesSingleGroup();
        }

        public static LightingSequence[] LightingSequencesSingleGroup()
        {
            const int Group1 = 0;

            var sequenceList = new List<LightingSequence>();

            TrafficLight.BulbData bulbData;
            GroupLightingOrder order;
            var orderList = new List<GroupLightingOrder>();
            LightingSequence sequnece;

            // sequence 1 :
            // Vehicle group1 Green.
            bulbData = new TrafficLight.BulbData(
                TrafficLight.BulbType.GREEN_BULB,
                TrafficLight.BulbColor.GREEN,
                TrafficLight.BulbStatus.SOLID_ON);
            order = new GroupLightingOrder(Group1, bulbData);
            orderList.Add(order);

            // Create Sequence.
            sequnece = new LightingSequence(RoadNetworkConstants.TRAFFIC_LIGHT_GREEN_INTERVAL_SECONDS, orderList.ToArray());
            sequenceList.Add(sequnece);

            return sequenceList.ToArray();
        }

        public static LightingSequence[] LightingSequencesDynamicGroups(int num)
        {
            var sequenceList = new List<LightingSequence>();

            TrafficLight.BulbData bulbData;
            GroupLightingOrder order;
            var orderList = new List<GroupLightingOrder>();
            LightingSequence sequnece;

            for (int i = 0; i < num; i++)
            {
                //  Green.
                orderList.Clear();
                bulbData = new TrafficLight.BulbData(
                    TrafficLight.BulbType.GREEN_BULB,
                    TrafficLight.BulbColor.GREEN,
                    TrafficLight.BulbStatus.SOLID_ON);
                order = new GroupLightingOrder(i, bulbData);
                orderList.Add(order);

                if (i == 0) //初回のみ
                {
                    // Red.
                    for (int j = 1; j < num; j++)
                    {
                        // Red.
                        bulbData = new TrafficLight.BulbData(
                            TrafficLight.BulbType.RED_BULB,
                            TrafficLight.BulbColor.RED,
                            TrafficLight.BulbStatus.SOLID_ON);
                        order = new GroupLightingOrder(j, bulbData);
                        orderList.Add(order);
                    }
                }

                sequnece = new LightingSequence(RoadNetworkConstants.TRAFFIC_LIGHT_GREEN_INTERVAL_SECONDS, orderList.ToArray());
                sequenceList.Add(sequnece);

                // Yellow.
                orderList.Clear();
                bulbData = new TrafficLight.BulbData(
                    TrafficLight.BulbType.YELLOW_BULB,
                    TrafficLight.BulbColor.YELLOW,
                    TrafficLight.BulbStatus.SOLID_ON);
                order = new GroupLightingOrder(i, bulbData);
                orderList.Add(order);
                sequnece = new LightingSequence(RoadNetworkConstants.TRAFFIC_LIGHT_YELLOW_INTERVAL_SECONDS, orderList.ToArray());
                sequenceList.Add(sequnece);

                //  Red.
                orderList.Clear();
                bulbData = new TrafficLight.BulbData(
                    TrafficLight.BulbType.RED_BULB,
                    TrafficLight.BulbColor.RED,
                    TrafficLight.BulbStatus.SOLID_ON);
                order = new GroupLightingOrder(i, bulbData);
                orderList.Add(order);
                sequnece = new LightingSequence(RoadNetworkConstants.TRAFFIC_LIGHT_RED_INTERVAL_SECONDS, orderList.ToArray());
                sequenceList.Add(sequnece);
            }

            return sequenceList.ToArray();
        }
    }
}
