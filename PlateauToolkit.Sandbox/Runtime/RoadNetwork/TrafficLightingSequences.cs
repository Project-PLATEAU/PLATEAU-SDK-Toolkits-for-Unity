using AWSIM;
using System.Collections.Generic;
using static AWSIM.TrafficSimulation.TrafficIntersection;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    public class TrafficLightingSequences
    {
        public class TrafficLightingParam
        {
            public float greenRedInterval;
            public float yellowInterval;
            public float extraRedInterval;

            public TrafficLightingParam(float greenRedInterval, float yellowInterval, float extraRedInterval)
            {
                this.greenRedInterval = greenRedInterval;
                this.yellowInterval = yellowInterval;
                this.extraRedInterval = extraRedInterval;
            }

            public TrafficLightingParam()
            {
                greenRedInterval = PlateauSandboxTrafficManagerConstants.TRAFFIC_LIGHT_GREEN_INTERVAL_SECONDS;
                yellowInterval = PlateauSandboxTrafficManagerConstants.TRAFFIC_LIGHT_YELLOW_INTERVAL_SECONDS;
                extraRedInterval = PlateauSandboxTrafficManagerConstants.TRAFFIC_LIGHT_RED_INTERVAL_SECONDS;
            }
        }


        /// <summary>
        /// 青黄赤バルブ信号機用シーケンス
        /// </summary>
        /// <param name="numGroups"></param>
        /// <returns></returns>
        public static LightingSequence[] GetLightingSequences(int numGroups, TrafficLightingParam param)
        {
            if (numGroups >= 2)
                return LightingSequencesDynamicGroups(numGroups, param);

            return LightingSequencesSingleGroup(param);
        }

        public static LightingSequence[] LightingSequencesSingleGroup(TrafficLightingParam param)
        {
            const int Group1 = 0;

            var sequenceList = new List<LightingSequence>();

            TrafficLightData.BulbData bulbData;
            GroupLightingOrder order;
            var orderList = new List<GroupLightingOrder>();
            LightingSequence sequnece;

            // sequence 1 :
            // Vehicle group1 Green.
            bulbData = new TrafficLightData.BulbData(
                TrafficLightData.BulbType.GREEN_BULB,
                TrafficLightData.BulbColor.GREEN,
                TrafficLightData.BulbStatus.SOLID_ON);
            order = new GroupLightingOrder(Group1, bulbData);
            orderList.Add(order);

            // Create Sequence.
            sequnece = new LightingSequence(param.greenRedInterval, orderList.ToArray());
            sequenceList.Add(sequnece);

            return sequenceList.ToArray();
        }

        public static LightingSequence[] LightingSequencesDynamicGroups(int num, TrafficLightingParam param)
        {
            var sequenceList = new List<LightingSequence>();

            TrafficLightData.BulbData bulbData;
            GroupLightingOrder order;
            var orderList = new List<GroupLightingOrder>();
            LightingSequence sequnece;

            for (int i = 0; i < num; i++)
            {
                //  Green.
                orderList.Clear();
                bulbData = new TrafficLightData.BulbData(
                    TrafficLightData.BulbType.GREEN_BULB,
                    TrafficLightData.BulbColor.GREEN,
                    TrafficLightData.BulbStatus.SOLID_ON);
                order = new GroupLightingOrder(i, bulbData);
                orderList.Add(order);

                if (i == 0) //初回のみ
                {
                    // Red.
                    for (int j = 1; j < num; j++)
                    {
                        // Red.
                        bulbData = new TrafficLightData.BulbData(
                            TrafficLightData.BulbType.RED_BULB,
                            TrafficLightData.BulbColor.RED,
                            TrafficLightData.BulbStatus.SOLID_ON);
                        order = new GroupLightingOrder(j, bulbData);
                        orderList.Add(order);
                    }
                }

                sequnece = new LightingSequence(param.greenRedInterval, orderList.ToArray());
                sequenceList.Add(sequnece);

                // Yellow.
                orderList.Clear();
                bulbData = new TrafficLightData.BulbData(
                    TrafficLightData.BulbType.YELLOW_BULB,
                    TrafficLightData.BulbColor.YELLOW,
                    TrafficLightData.BulbStatus.SOLID_ON);
                order = new GroupLightingOrder(i, bulbData);
                orderList.Add(order);
                sequnece = new LightingSequence(param.yellowInterval, orderList.ToArray());
                sequenceList.Add(sequnece);

                //  Red.
                orderList.Clear();
                bulbData = new TrafficLightData.BulbData(
                    TrafficLightData.BulbType.RED_BULB,
                    TrafficLightData.BulbColor.RED,
                    TrafficLightData.BulbStatus.SOLID_ON);
                order = new GroupLightingOrder(i, bulbData);
                orderList.Add(order);
                sequnece = new LightingSequence(param.extraRedInterval, orderList.ToArray());
                sequenceList.Add(sequnece);
            }

            return sequenceList.ToArray();
        }
    }
}
