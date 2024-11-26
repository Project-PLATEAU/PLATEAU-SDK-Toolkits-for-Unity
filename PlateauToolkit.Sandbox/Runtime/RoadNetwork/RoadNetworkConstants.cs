namespace PlateauToolkit.Sandbox.RoadNetwork
{
    public class RoadNetworkConstants
    {
        /// <summary>
        /// 最大車輛数
        /// </summary>
        public static readonly int NUM_MAX_VEHICLES = 100;

        /// <summary>
        /// 各レーンの最大速度
        /// </summary>
        public static readonly float SPEED_LIMIT = 15f;

        /// <summary>
        /// 停止してから消すまでの時間(秒）
        /// </summary>
        public static readonly float MAX_IDLE_TIME = 20f;

        /// <summary>
        /// Goundからの距離判定を行うか (Groundから一定距離離れた車輛は削除)
        /// </summary>
        public static readonly bool CHECK_DISTANCE_FROM_GROUND = true;

        /// <summary>
        /// Goundからの距離判定を行う場合の距離 (5f以下だとBusがdefaultで消えてしまう）
        /// </summary>
        public static readonly float MAX_DISTANCE_FROM_GROUND = 50f; //Goundからの距離判定 (5f以下だとBusがdefaultで消えてしまう）

        /// <summary>
        /// GameObject/Layer 名
        /// </summary>
        public static readonly string LAYER_MASK_VEHICLE = "Vehicle";
        public static readonly string LAYER_MASK_GROUND = "Ground";
        public static readonly string TRAFFIC_MANAGER_NAME = "TrafficManager";
        public static readonly string VEHICLE_ROOT_NAME = "Vehicles";
        public static readonly string TRAFFIC_LANE_ROOT_NAME = "TrafficLanes";
        public static readonly string STOPLINE_ROOT_NAME = "StopLines";

        /// <summary>
        /// 実行後にRightOfWaysを自動生成
        /// </summary>
        public static readonly bool USE_RIGHT_OF_WAYS = false;

        /// <summary>
        /// DemをGround Layerとして設定
        /// </summary>
        public static readonly bool SET_DEM_AS_GROUND_LAYER = false;

        /// <summary>
        /// isTurnings 判定用 (Original 45f)  NPCVehicleCognitionStep.CurveCheckJob
        /// 60fにすると道がない平地を斜め移動してしまう
        /// </summary>
        public static readonly float IS_TURNING_ANGLE = 30f;

        //RoadNetworkLaneConverter

        /// <summary>
        /// Spline使用時の分割数
        /// </summary>
        public static readonly int SPLINE_POINTS = 6;

        /// <summary>
        /// LineString IsReverseを無視　(IsReverseの場合はPrev/Nextを逆に）
        /// </summary>
        public static readonly bool IGNORE_REVERSED_LANE = false;

        /// <summary>
        /// Trackのspline形状が破綻している場合に停止するのを回避 (First/Last Knotsのみ利用）
        /// </summary>
        public static readonly bool USE_SIMPLE_SPLINE_POINTS = false;

        /// <summary>
        /// Lane Linestringを平滑化
        /// </summary>
        public static readonly bool USE_SIMPLE_LINESTRINGS = false;

        /// <summary>
        /// StopLine を生成
        /// 信号がない場合は無意味
        /// </summary>
        public static readonly bool ADD_STOPLINES = false;

    }
}
