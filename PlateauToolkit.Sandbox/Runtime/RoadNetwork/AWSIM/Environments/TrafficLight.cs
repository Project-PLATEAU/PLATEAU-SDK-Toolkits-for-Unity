using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using PlateauToolkit.Sandbox.RoadNetwork;
using PlateauToolkit.Sandbox;
using static PlateauToolkit.Sandbox.RoadNetwork.TrafficLightData;

namespace AWSIM
{
    /// <summary>
    /// Traffic Light class.
    /// </summary>
    public class TrafficLight : MonoBehaviour
    {
        /// <summary>
        /// Define TrafficLight bulbs.
        /// </summary>
        [Serializable]
        class Bulb
        {
            public BulbType BulbType => bulbType;

            public BulbStatus BulbStatus => status;
            public BulbColor BulbColor => color;

            [SerializeField] public BulbType bulbType;
            [SerializeField, Tooltip("Specifies the index of the material to be used for the bulb.")] 
            public int materialIndex;

            // const parameters.

            // HDRP
            const string EmissiveColorHDRP = "_EmissiveColor";
            const string EmissiveExposureWeightHDRP = "_EmissiveExposureWeight";

            // URP
            const string EmissiveColorURP = "_EmissionColor";
            const string EmissiveIntensity = "_EmissiveIntensity";

            const string LightOnFlag = "_LightOn";
            const float flashIntervalSec = 0.5f;                // flash bulb lighting interval(sec).

            float timer = 0;                            // used for flashing status.     NOTE: Might as well make it static and refer to the same time. 
            Color defaultEmissiveColor;                 // default bulb material emissive color.
            float defaultEmissiveExposureWeightHDRP;        // default bulb mateiral emissive exposure weight
            Dictionary<BulbColor, EmissionConfig> bulbColorConfigPairs;
            Material material = null;                   // bulb mateiral(instance).
            bool initialized = false;
            BulbStatus status = BulbStatus.SOLID_OFF;
            BulbColor color;
            bool isLightOn = false;                     // used for flashing control.

            /// <summary>
            /// Called from TrafficLight class. Acquire and initialize bulb material.
            /// </summary>
            /// <param name="renderer">Renderer containing the bulb material.</param>
            /// <param name="bulbEmissionConfigs"></param>
            public void Initialize(Renderer renderer, EmissionConfig[] bulbEmissionConfigs)
            {
                // bulb color config.
                bulbColorConfigPairs = bulbEmissionConfigs.ToDictionary(x => x.BulbColor);

                if (renderer != null && renderer.materials.Length > materialIndex)
                {
                    // set material.
                    material = renderer.materials[materialIndex];

                    // cache default material parameters.
                    if (material.HasColor(EmissiveColorURP))
                    {
                        defaultEmissiveColor = Color.black;
                    }

                    if (material.HasColor(EmissiveColorHDRP))
                    {
                        defaultEmissiveColor = material.GetColor(EmissiveColorHDRP);
                    }

                    if (material.HasFloat(EmissiveExposureWeightHDRP))
                    {
                        defaultEmissiveExposureWeightHDRP = material.GetFloat(EmissiveExposureWeightHDRP);
                    }
                }

                initialized = true;
            }

            /// <summary>
            /// Called from TrafficLight class.
            /// Set the bulb lighting.
            /// </summary>
            /// <param name="status">bulb status</param>
            /// <param name="color">bulb color</param>
            public void SetBulbLighting(BulbStatus status, BulbColor color)
            {
                if (initialized == false)
                {
                    Debug.LogError("Not initialized Traffic bulb.");
                    return;
                }

                if (this.status == status && this.color == color)
                    return;

                this.status = status;
                this.color = color;

                switch (status)
                {
                    case BulbStatus.SOLID_ON:
                        timer = 0;
                        Set(true); break;
                    case BulbStatus.SOLID_OFF:
                        timer = 0;
                        Set(false); break;
                    case BulbStatus.FLASHING:
                        timer = 0;
                        Set(true); break;
                }
            }

            /// <summary>
            /// Called from TrafficLight class. 
            /// Update timer for bulb flashing.
            /// </summary>
            /// <param name="deltaTime"></param>
            public void Update(float deltaTime)
            {
                if (status != BulbStatus.FLASHING)
                    return;

                timer += deltaTime;

                if (timer > flashIntervalSec)
                {
                    Set(!isLightOn);
                }
            }

            /// <summary>
            ///  Called from TrafficLight class. 
            ///  Discard the material instance.
            /// </summary>
            public void Destroy()
            {
                if (material != null)
                    UnityEngine.Object.Destroy(material);
                initialized = false;
            }

            /// <summary>
            /// Change material parameters to control lighting.
            /// </summary>
            /// <param name="isLightOn">Light up bulb</param>
            void Set(bool isLightOn)
            {
                if (isLightOn)
                {
                    var config = bulbColorConfigPairs[color];
                    if (material != null)
                    {
                        if (material.HasColor(EmissiveColorURP))
                        {
                            material.SetColor(EmissiveColorURP, config.Color * config.Intensity);
                        }

                        if (material.HasColor(EmissiveColorHDRP))
                        {
                            material.SetColor(EmissiveColorHDRP, config.Color * config.Intensity);
                        }

                        if (material.HasFloat(EmissiveExposureWeightHDRP))
                        {
                            material.SetFloat(EmissiveExposureWeightHDRP, config.ExposureWeight);
                        }

                        if (material.HasProperty(LightOnFlag))
                        {
                            material.SetInt(LightOnFlag, 1);
                        }
                    }
                    this.isLightOn = true;
                    timer = 0;
                }
                else
                {
                    if (material != null)
                    {
                        if (material.HasColor(EmissiveColorURP))
                        {
                            material.SetColor(EmissiveColorURP, defaultEmissiveColor);
                        }

                        if (material.HasColor(EmissiveColorHDRP))
                        {
                            material.SetColor(EmissiveColorHDRP, defaultEmissiveColor);
                        }

                        if (material.HasFloat(EmissiveExposureWeightHDRP))
                        {
                            material.SetFloat(EmissiveExposureWeightHDRP, defaultEmissiveExposureWeightHDRP);
                        }

                        if (material.HasProperty(LightOnFlag))
                        {
                            material.SetInt(LightOnFlag, 0);
                        }
                    }
                    this.isLightOn = false;
                    timer = 0;
                }
            }

            /// <summary>
            /// Color取得
            /// </summary>
            /// <returns></returns>
            public Color GetColor()
            {
                return bulbColorConfigPairs[BulbColor].Color;
            }
        }

        /// <summary>
        /// PlateauSandboxInteractiveTrafficLightとRendererをセット
        /// </summary>
        /// <param name="interactiveAsset"></param>
        public void SetTrafficLightAsset(PlateauSandboxInteractiveTrafficLight interactiveAsset)
        {
            if (TrafficLightAsset == interactiveAsset)
            {
                return;
            }

            TrafficLightAsset = interactiveAsset;

            if (TrafficLightAsset != null)
            {
                //interactiveAsset.SetTrafficLight(this);
                CreateBulbsAndEmissionConfigsFromAssetData(interactiveAsset.TrafficLightAssetBulbData);

                SetRenderer(interactiveAsset.gameObject.GetComponentInChildren<Renderer>());
            }
        }

        /// <summary>
        /// Rendererのみセット
        /// </summary>
        /// <param name="_renderer_"></param>
        public void SetRenderer(Renderer _renderer_)
        {
            renderer = _renderer_;
        }

        [SerializeField, Tooltip("Set the PlateauSandboxInteractiveTrafficLight.")]
        PlateauSandboxInteractiveTrafficLight TrafficLightAsset;

        //[SerializeField, Tooltip("Set the Renderer containing the bulb material.")]
        [SerializeField][HideInInspector]
        new Renderer renderer;

        /// <summary>
        /// Define the Emission parameter to be applied to the material when the Bulb is turned on.
        /// </summary>
        [Header("Bulb Emission config")]
        [SerializeField, Tooltip("Define the Emission parameter for BulbColor.")][HideInInspector]
        TrafficLightData.EmissionConfig[] bulbEmissionConfigs = TrafficLightData.GetDefaultEmissionConfigs();

        [Header("Bulb material config"), Tooltip("Link the material of the bulb to the type.")][HideInInspector]
        [SerializeField] Bulb[] bulbs;

        Dictionary<BulbType, Bulb> bulbPairs;
        int bulbCount;
        BulbData[] bulbDataArray;

        [SerializeField][HideInInspector]
        List<AWSIM.TrafficSimulation.StopLine> stoplines = new();

        [Header("RoadNetwork")]
        [SerializeField]
        public RnDataTrafficLight rnTrafficLight;

        //RoadNetworkDataGetter m_RoadNetworkGetter;
        public RoadNetworkDataGetter RnGetter
        {
            get
            {
                return RnManager?.RnGetter;
            }
        }

        PlateauSandboxTrafficManager m_PlateauTrafficManager;

        public PlateauSandboxTrafficManager RnManager
        {
            get
            {
                if (m_PlateauTrafficManager == null)
                {
                    m_PlateauTrafficManager = GameObject.FindObjectOfType<PlateauSandboxTrafficManager>();
                    if (m_PlateauTrafficManager == null)
                    {
                        Debug.LogError($"RoadNetworkTrafficManager is null");
                    }
                }
                return m_PlateauTrafficManager;
            }
        }

        void OnValidate()
        {
            RefreshAssets();
        }

        void Reset()
        {
            RefreshAssets();
        }

        void Awake()
        {
            bulbPairs = bulbs.ToDictionary(x => x.BulbType);
            bulbCount = bulbPairs.Count();
            bulbDataArray = new BulbData[bulbCount];

            // Initialize bulb materials.
            foreach (var e in bulbPairs.Values)
            {
                e.Initialize(renderer, bulbEmissionConfigs);
            }
        }

        void Update()
        {
            // Update timers for each Bulb.
            foreach (var e in bulbPairs.Values)
            {
                e.Update(Time.deltaTime);
            }
        }

        void OnDestroy()
        {
            // Destory bulb materials.
            foreach (var e in bulbs)
            {
                e.Destroy();
            }
        }

        public void TurnOffAllBulbs()
        {
            foreach (var e in bulbs)
            {
                e.SetBulbLighting(BulbStatus.SOLID_OFF, BulbColor.WHITE);
            }
        }

        /// <summary>
        /// Updates the status of each bulb of this traffic light.
        /// </summary>
        /// <param name="inputDatas">Input data to update each bulb.</param>
        public void SetBulbData(BulbData[] inputDatas)
        {
            for (int i = 0; i < inputDatas.Length; i++)
            {
                var inputData = inputDatas[i];
                SetBulbData(inputData);
            }
        }

        /// <summary>
        /// Updates the status of each bulb of this traffic light.
        /// </summary>
        /// <param name="inputData">Input data to update each bulb.</param>
        public void SetBulbData(BulbData inputData)
        {
            bulbPairs[inputData.Type].SetBulbLighting(inputData.Status, inputData.Color);
        }

        /// <summary>
        /// Get the current status of each bulb of the traffic light.
        /// </summary>
        /// <returns>bulb data array</returns>
        public BulbData[] GetBulbData()
        {
            int i = 0;

            foreach(var e in bulbPairs)
            {
                bulbDataArray[i] = new BulbData(e.Value.BulbType, e.Value.BulbColor, e.Value.BulbStatus);
                i++;
            }

            return bulbDataArray;
        }

        void RefreshAssets()
        {
            renderer = TrafficLightAsset?.GetComponentInChildren<Renderer>();

            if (TrafficLightAsset != null)
            {
                CreateBulbsAndEmissionConfigsFromAssetData(TrafficLightAsset.TrafficLightAssetBulbData);
            }
            else
            {
                CreateDefaultBulbs();
            }
        }

        void CreateBulbsAndEmissionConfigsFromAssetData(TrafficLightAssetBulbData[] data)
        {
            List<Bulb> bulbList = new List<Bulb>();
            List<EmissionConfig> emissionConfigList = new List<EmissionConfig>();
            foreach (TrafficLightAssetBulbData e in data)
            {
                var bulb = new Bulb()
                {
                    bulbType = e.bulbType,
                    materialIndex = e.materialIndex,
                };
                bulbList.Add(bulb);
                emissionConfigList.Add(e.EmissionConfig);
            }

            bulbs = bulbList.ToArray();
            bulbEmissionConfigs = emissionConfigList.ToArray();
        }

        void CreateDefaultBulbs()
        {
            bulbs = new Bulb[]
            {
                new Bulb()
                {
                    bulbType = BulbType.GREEN_BULB,
                    materialIndex = 1,
                },
                new Bulb()
                {
                    bulbType = BulbType.YELLOW_BULB,
                    materialIndex = 2,
                },
                new Bulb()
                {
                    bulbType = BulbType.RED_BULB,
                    materialIndex = 3,
                },
            };
        }

        public Color GetBulbColor()
        {
            Color color = Color.black;
            foreach (var e in bulbs)
            {
                if (e.BulbStatus != BulbStatus.SOLID_OFF)
                {
                    color = e.GetColor();
                }
            }
            return color;
        }

        public void AddStopLine(AWSIM.TrafficSimulation.StopLine stopline)
        {
            stoplines.Add(stopline);
        }

        //NeighborのLinestring point取得
        public List<Vector3> GetAllBorderPoints()
        {
            var points = new List<Vector3>();
            var borders = rnTrafficLight.Neighbor;
            foreach (var b in borders)
            {
                var vectors = RnGetter.GetWays().TryGet(b)?.GetChildLineString(RnGetter)?.GetChildPointsVector(RnGetter);
                points.AddRange(vectors);
            }
            return points;
        }

        // NeighborのLinestringを一本の線とした場合の始点、終点
        public (Vector3, Vector3) GetFirstLastBorderPoints()
        {
            List<Vector3> points = GetAllBorderPoints();
            (Vector3 first, Vector3 last) = GeometryTool.GetLongestLine(points);
            return (first, last);
        }


        //全StopLineを一本の線とした場合の始点、終点
        public (Vector3, Vector3) GetFirstLastStopLinePoints()
        {
            var stoplinePoints = new List<Vector3>();
            foreach (var item in stoplines)
            {
                stoplinePoints.Add(item.Points[0]);
                stoplinePoints.Add(item.Points[1]);
            }
            return GeometryTool.GetLongestLine(stoplinePoints);
        }

        // 停止線を考慮して開始、終了位置を取得( 最初のポイントが信号位置 ）
        public (Vector3, Vector3) GetFirstLastBorderPointsNormalized()
        {
            (Vector3 first, Vector3 last) = GetFirstLastBorderPoints();
            var intersectionCenter = transform.parent.position;

            bool isLeft = GeometryTool.IsFacingLeft(first, last, intersectionCenter);
            return isLeft ? (first, last) : (last, first);
        }

        //　信号機アセット配置時のRight Vector取得
        public Vector3 GetRightVector()
        {
            (Vector3 first, Vector3 last) = GetFirstLastBorderPointsNormalized();
            Vector3 vector = (first - last).normalized;
            vector.y = 0;
            return vector;
        }

        //　信号機アセットの配置位置
        public Vector3 GetAssetPosition()
        {
            var (firstPoint, lastPoint) = GetFirstLastBorderPointsNormalized();
            if (firstPoint != Vector3.zero)
                return firstPoint;

            var edges = rnTrafficLight.GetEdges(RnGetter);
            return edges.LastOrDefault().GetChildLineString(RnGetter).GetChildPointsVector(RnGetter).LastOrDefault();
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            if (!RnManager.ShowTrafficLightGizmos)
                return;

            var (firstPoint, lastPoint) = GetFirstLastBorderPointsNormalized();
            Gizmos.color = GetBulbColor();
            Gizmos.DrawSphere(GetAssetPosition(), 0.5f);

            var (firstPointSp, lastPointSp) = GetFirstLastStopLinePoints();
            if (firstPointSp != Vector3.zero && lastPointSp != Vector3.zero)
            {
                Gizmos.DrawLine(firstPointSp, lastPointSp);
            }
        }

        void OnDrawGizmosSelected()
        {
            var (firstPoint, lastPoint) = GetFirstLastBorderPointsNormalized();
            Gizmos.color = Color.white;
            if (firstPoint != Vector3.zero && lastPoint != Vector3.zero)
            {
                Gizmos.DrawLine(firstPoint, lastPoint);
                Gizmos.DrawSphere(GetAssetPosition(), 0.2f);
            }
        }
    }
}