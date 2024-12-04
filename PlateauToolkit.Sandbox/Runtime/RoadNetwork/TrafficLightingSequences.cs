using AWSIM;
using System.Collections.Generic;
using static AWSIM.TrafficSimulation.TrafficIntersection;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    public class TrafficLightingSequences
    {
        //信号の切替秒数 (GREEN_INTERVAL_SECONDS + RED_INTERVAL_SECONDS は RoadNetworkConstants.MAX_IDLE_TIMEより短く設定)
        public static readonly float GREEN_INTERVAL_SECONDS = 10f;
        public static readonly float YELLOW_INTERVAL_SECONDS = 3f;
        public static readonly float RED_INTERVAL_SECONDS = 1f;

        public static LightingSequence[] GetLightingSequences(int numGroups)
        {
            //if (numGroups > 2)
            //{
            //    return LightingSequences3Groups();
            //}
            //else if (numGroups == 2)
            //{
            //    return LightingSequences2Groups();
            //}

            if (numGroups >= 2)
                return LightingSequencesDynamicGroups(numGroups);

            return LightingSequences1Group();
        }

        public static LightingSequence[] LightingSequences1Group()
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
            sequnece = new LightingSequence(GREEN_INTERVAL_SECONDS, orderList.ToArray());
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

                sequnece = new LightingSequence(GREEN_INTERVAL_SECONDS, orderList.ToArray());
                sequenceList.Add(sequnece);

                // Yellow.
                orderList.Clear();
                bulbData = new TrafficLight.BulbData(
                    TrafficLight.BulbType.YELLOW_BULB,
                    TrafficLight.BulbColor.YELLOW,
                    TrafficLight.BulbStatus.SOLID_ON);
                order = new GroupLightingOrder(i, bulbData);
                orderList.Add(order);
                sequnece = new LightingSequence(YELLOW_INTERVAL_SECONDS, orderList.ToArray());
                sequenceList.Add(sequnece);

                //  Red.
                orderList.Clear();
                bulbData = new TrafficLight.BulbData(
                    TrafficLight.BulbType.RED_BULB,
                    TrafficLight.BulbColor.RED,
                    TrafficLight.BulbStatus.SOLID_ON);
                order = new GroupLightingOrder(i, bulbData);
                orderList.Add(order);
                sequnece = new LightingSequence(RED_INTERVAL_SECONDS, orderList.ToArray());
                sequenceList.Add(sequnece);
            }

            return sequenceList.ToArray();
        }

        //public static LightingSequence[] LightingSequences2Groups()
        //{
        //    const int Group1 = 0;
        //    const int Group2 = 1;

        //    var sequenceList = new List<LightingSequence>();

        //    TrafficLight.BulbData bulbData;
        //    GroupLightingOrder order;
        //    var orderList = new List<GroupLightingOrder>();
        //    LightingSequence sequnece;

        //    // sequence 1 :
        //    // Vehicle group1 Green.
        //    bulbData = new TrafficLight.BulbData(
        //        TrafficLight.BulbType.GREEN_BULB,
        //        TrafficLight.BulbColor.GREEN,
        //        TrafficLight.BulbStatus.SOLID_ON);
        //    order = new GroupLightingOrder(Group1, bulbData);
        //    orderList.Add(order);

        //    // Vehicle group2 Red.
        //    bulbData = new TrafficLight.BulbData(
        //        TrafficLight.BulbType.RED_BULB,
        //        TrafficLight.BulbColor.RED,
        //        TrafficLight.BulbStatus.SOLID_ON);
        //    order = new GroupLightingOrder(Group2, bulbData);
        //    orderList.Add(order);

        //    // Create Sequence.
        //    sequnece = new LightingSequence(GREEN_INTERVAL_SECONDS, orderList.ToArray());
        //    sequenceList.Add(sequnece);

        //    // sequence 2
        //    // - Vehicle group1 Yellow.
        //    orderList.Clear();
        //    bulbData = new TrafficLight.BulbData(
        //        TrafficLight.BulbType.YELLOW_BULB,
        //        TrafficLight.BulbColor.YELLOW,
        //        TrafficLight.BulbStatus.SOLID_ON);
        //    order = new GroupLightingOrder(Group1, bulbData);
        //    orderList.Add(order);
        //    sequnece = new LightingSequence(YELLOW_INTERVAL_SECONDS, orderList.ToArray());
        //    sequenceList.Add(sequnece);

        //    // sequence 3
        //    // - Vehicle group1 Red.
        //    orderList.Clear();
        //    bulbData = new TrafficLight.BulbData(
        //        TrafficLight.BulbType.RED_BULB,
        //        TrafficLight.BulbColor.RED,
        //        TrafficLight.BulbStatus.SOLID_ON);
        //    order = new GroupLightingOrder(Group1, bulbData);
        //    orderList.Add(order);
        //    sequnece = new LightingSequence(RED_INTERVAL_SECONDS, orderList.ToArray());
        //    sequenceList.Add(sequnece);

        //    // sequence 4
        //    // - Vehicle group2 Green.
        //    orderList.Clear();
        //    bulbData = new TrafficLight.BulbData(
        //        TrafficLight.BulbType.GREEN_BULB,
        //        TrafficLight.BulbColor.GREEN,
        //        TrafficLight.BulbStatus.SOLID_ON);
        //    order = new GroupLightingOrder(Group2, bulbData);
        //    orderList.Add(order);
        //    sequnece = new LightingSequence(GREEN_INTERVAL_SECONDS, orderList.ToArray());
        //    sequenceList.Add(sequnece);

        //    // sequence 5
        //    // - Vehicle group2 Yellow.
        //    orderList.Clear();
        //    bulbData = new TrafficLight.BulbData(
        //        TrafficLight.BulbType.YELLOW_BULB,
        //        TrafficLight.BulbColor.YELLOW,
        //        TrafficLight.BulbStatus.SOLID_ON);
        //    order = new GroupLightingOrder(Group2, bulbData);
        //    orderList.Add(order);
        //    sequnece = new LightingSequence(YELLOW_INTERVAL_SECONDS, orderList.ToArray());
        //    sequenceList.Add(sequnece);

        //    // sequence 6
        //    // - Vehicle group2 Red.
        //    orderList.Clear();
        //    bulbData = new TrafficLight.BulbData(
        //        TrafficLight.BulbType.RED_BULB,
        //        TrafficLight.BulbColor.RED,
        //        TrafficLight.BulbStatus.SOLID_ON);
        //    order = new GroupLightingOrder(Group2, bulbData);
        //    orderList.Add(order);
        //    sequnece = new LightingSequence(RED_INTERVAL_SECONDS, orderList.ToArray());
        //    sequenceList.Add(sequnece);

        //    return sequenceList.ToArray();
        //}

        //public static LightingSequence[] LightingSequences3Groups()
        //{
        //    const int Group1 = 0;
        //    const int Group2 = 1;
        //    const int Group3 = 2;

        //    var sequenceList = new List<LightingSequence>();

        //    TrafficLight.BulbData bulbData;
        //    GroupLightingOrder order;
        //    var orderList = new List<GroupLightingOrder>();
        //    LightingSequence sequnece;

        //    // sequence 1 :
        //    // Vehicle group1 Green.
        //    bulbData = new TrafficLight.BulbData(
        //        TrafficLight.BulbType.GREEN_BULB,
        //        TrafficLight.BulbColor.GREEN,
        //        TrafficLight.BulbStatus.SOLID_ON);
        //    order = new GroupLightingOrder(Group1, bulbData);
        //    orderList.Add(order);

        //    // Vehicle group2 Red.
        //    bulbData = new TrafficLight.BulbData(
        //        TrafficLight.BulbType.RED_BULB,
        //        TrafficLight.BulbColor.RED,
        //        TrafficLight.BulbStatus.SOLID_ON);
        //    order = new GroupLightingOrder(Group2, bulbData);
        //    orderList.Add(order);

        //    // Vehicle group3 Red.
        //    bulbData = new TrafficLight.BulbData(
        //        TrafficLight.BulbType.RED_BULB,
        //        TrafficLight.BulbColor.RED,
        //        TrafficLight.BulbStatus.SOLID_ON);
        //    order = new GroupLightingOrder(Group3, bulbData);
        //    orderList.Add(order);

        //    // Create Sequence.
        //    sequnece = new LightingSequence(GREEN_INTERVAL_SECONDS, orderList.ToArray());
        //    sequenceList.Add(sequnece);

        //    // sequence 2
        //    // - Vehicle group1 Yellow.
        //    orderList.Clear();
        //    bulbData = new TrafficLight.BulbData(
        //        TrafficLight.BulbType.YELLOW_BULB,
        //        TrafficLight.BulbColor.YELLOW,
        //        TrafficLight.BulbStatus.SOLID_ON);
        //    order = new GroupLightingOrder(Group1, bulbData);
        //    orderList.Add(order);
        //    sequnece = new LightingSequence(YELLOW_INTERVAL_SECONDS, orderList.ToArray());
        //    sequenceList.Add(sequnece);

        //    // sequence 3
        //    // - Vehicle group1 Red.
        //    orderList.Clear();
        //    bulbData = new TrafficLight.BulbData(
        //        TrafficLight.BulbType.RED_BULB,
        //        TrafficLight.BulbColor.RED,
        //        TrafficLight.BulbStatus.SOLID_ON);
        //    order = new GroupLightingOrder(Group1, bulbData);
        //    orderList.Add(order);
        //    sequnece = new LightingSequence(RED_INTERVAL_SECONDS, orderList.ToArray());
        //    sequenceList.Add(sequnece);

        //    // sequence 4
        //    // - Vehicle group2 Green.
        //    orderList.Clear();
        //    bulbData = new TrafficLight.BulbData(
        //        TrafficLight.BulbType.GREEN_BULB,
        //        TrafficLight.BulbColor.GREEN,
        //        TrafficLight.BulbStatus.SOLID_ON);
        //    order = new GroupLightingOrder(Group2, bulbData);
        //    orderList.Add(order);
        //    sequnece = new LightingSequence(GREEN_INTERVAL_SECONDS, orderList.ToArray());
        //    sequenceList.Add(sequnece);

        //    // sequence 5
        //    // - Vehicle group2 Yellow.
        //    orderList.Clear();
        //    bulbData = new TrafficLight.BulbData(
        //        TrafficLight.BulbType.YELLOW_BULB,
        //        TrafficLight.BulbColor.YELLOW,
        //        TrafficLight.BulbStatus.SOLID_ON);
        //    order = new GroupLightingOrder(Group2, bulbData);
        //    orderList.Add(order);
        //    sequnece = new LightingSequence(YELLOW_INTERVAL_SECONDS, orderList.ToArray());
        //    sequenceList.Add(sequnece);

        //    // sequence 6
        //    // - Vehicle group2 Red.
        //    orderList.Clear();
        //    bulbData = new TrafficLight.BulbData(
        //        TrafficLight.BulbType.RED_BULB,
        //        TrafficLight.BulbColor.RED,
        //        TrafficLight.BulbStatus.SOLID_ON);
        //    order = new GroupLightingOrder(Group2, bulbData);
        //    orderList.Add(order);
        //    sequnece = new LightingSequence(RED_INTERVAL_SECONDS, orderList.ToArray());
        //    sequenceList.Add(sequnece);

        //    // sequence 7
        //    // - Vehicle group3 Green.
        //    orderList.Clear();
        //    bulbData = new TrafficLight.BulbData(
        //        TrafficLight.BulbType.GREEN_BULB,
        //        TrafficLight.BulbColor.GREEN,
        //        TrafficLight.BulbStatus.SOLID_ON);
        //    order = new GroupLightingOrder(Group3, bulbData);
        //    orderList.Add(order);
        //    sequnece = new LightingSequence(GREEN_INTERVAL_SECONDS, orderList.ToArray());
        //    sequenceList.Add(sequnece);

        //    // sequence 8
        //    // - Vehicle group3 Yellow.
        //    orderList.Clear();
        //    bulbData = new TrafficLight.BulbData(
        //        TrafficLight.BulbType.YELLOW_BULB,
        //        TrafficLight.BulbColor.YELLOW,
        //        TrafficLight.BulbStatus.SOLID_ON);
        //    order = new GroupLightingOrder(Group3, bulbData);
        //    orderList.Add(order);
        //    sequnece = new LightingSequence(YELLOW_INTERVAL_SECONDS, orderList.ToArray());
        //    sequenceList.Add(sequnece);

        //    // sequence 9
        //    // - Vehicle group3 Red.
        //    orderList.Clear();
        //    bulbData = new TrafficLight.BulbData(
        //        TrafficLight.BulbType.RED_BULB,
        //        TrafficLight.BulbColor.RED,
        //        TrafficLight.BulbStatus.SOLID_ON);
        //    order = new GroupLightingOrder(Group3, bulbData);
        //    orderList.Add(order);
        //    sequnece = new LightingSequence(RED_INTERVAL_SECONDS, orderList.ToArray());
        //    sequenceList.Add(sequnece);

        //    return sequenceList.ToArray();
        //}

    }
}
