namespace PlateauToolkit.Sandbox.RoadNetwork
{
    public class PlateauSandboxTrafficManagerConstants
    {
        /// <summary>
        /// 最大車輛数
        /// </summary>
        public static readonly int NUM_MAX_VEHICLES = 100;

        /// <summary>
        /// 各レーンの最大速度
        /// </summary>
        public static readonly float SPEED_LIMIT = 15f;
        public static readonly float SPEED_LIMIT_INTERSECTION = 12f;

        /// <summary>
        /// 停止してから消すまでの時間(秒）
        /// </summary>
        public static readonly float MAX_IDLE_TIME = 30f;

        //信号の切替秒数
        public static readonly float TRAFFIC_LIGHT_GREEN_INTERVAL_SECONDS = 10f;
        public static readonly float TRAFFIC_LIGHT_YELLOW_INTERVAL_SECONDS = 3f;
        public static readonly float TRAFFIC_LIGHT_RED_INTERVAL_SECONDS = 1f;

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
        public static readonly string TRAFFIC_INTERSECTION_ROOT_NAME = "TrafficIntersections";
        public static readonly string STOPLINE_ROOT_NAME = "StopLines";
        public static readonly string TRAFFIC_LIGHT_ASSETS_ROOT_NAME = "TrafficLights";

        /// <summary>
        /// 信号機アセット(PlateauSandboxStreetFurniture)
        /// </summary>
        public static readonly string TRAFFIC_LIGHT_INTERACTIVE_ASSET_NAME = "Interactive_TrafficLight_01";

        /// <summary>
        /// 実行後にRightOfWaysを自動生成
        /// </summary>
        public static readonly bool USE_RIGHT_OF_WAYS = true;

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
        /// TrafficIntersection, TrafficLight, StopLine を生成
        /// </summary>
        public static readonly bool ADD_TRAFFIC_LIGHTS = true;

        /// <summary>
        /// 信号機アセット変更可否
        /// </summary>
        public static readonly bool TRAFFIC_LIGHT_ASSET_REPLACEABLE = true;

        /// <summary>
        /// DefaultでGizmoを表示
        /// </summary>
        public static readonly bool SHOW_DEBUG_GIZMOS = true;

        /// <summary>
        /// インスペクタにRoadNetwork参照、VehicleInternalState等のDebug情報を表示
        /// </summary>
        public static readonly bool SHOW_DEBUG_ROADNETWORK_INFO = false;
    }
}
