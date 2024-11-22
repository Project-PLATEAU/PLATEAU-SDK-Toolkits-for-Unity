

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    public class RoadNetworkConstants
    {

        public static readonly int NUM_MAX_VEHICLES = 100;

        public static readonly float SPEED_LIMIT = 15f;

        public static readonly float MAX_IDLE_TIME = 20f; //停止してから消すまでの時間(秒）

        public static readonly bool CHECK_DISTANCE_FROM_GROUND = true; //Goundからの距離判定を行うか
        public static readonly float MAX_DISTANCE_FROM_GROUND = 50f; //Goundからの距離判定 (5f以下だとBusがdefaultで消えてしまう）

        public static readonly string LAYER_MASK_VEHICLE = "Vehicle";
        public static readonly string LAYER_MASK_GROUND = "Ground";

        public static readonly string TRAFFIC_MANAGER_NAME = "TrafficManager";
        public static readonly string VEHICLE_ROOT_NAME = "Vehicles";
        public static readonly string TRAFFIC_LANE_ROOT_NAME = "TrafficLanes";
        public static readonly string STOPLINE_ROOT_NAME = "StopLines";

        public static readonly bool USE_RIGHT_OF_WAYS = false;
        public static readonly bool SET_DEM_AS_GROUND_LAYER = false;

        public static readonly float IS_TURNING_ANGLE = 30f; //isTurnings 判定用 (Original 45f) 60fとかにすると道がない平地を斜め移動してしまう


        //RoadNetworkLaneConverter

        public static readonly bool IgnoreReversedLane = false; //LineString IsReverseを無視　(IsReverseの場合はPrev/Nextを逆に）
        public static readonly bool UseSimpleSplinePoints = false; //Trackのspline形状が破綻している場合に停止するのを回避 (First/Last Knotsのみ利用）
        public static readonly bool UseSimpleLineStrings = false; //Lane Linestringがガタガタなのを平滑化
        public static readonly bool AddStopLines = false; //信号がない場合は無意味
        public static readonly int SplinePoints = 6; //Spline使用時の分割数

    }
}
