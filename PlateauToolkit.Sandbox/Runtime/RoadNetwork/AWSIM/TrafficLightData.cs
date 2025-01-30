using System;
using UnityEngine;

namespace AWSIM
{
    public class TrafficLightData
    {
        /// <summary>
        /// Type of each bulb.
        /// </summary>
        public enum BulbType
        {
            ANY_CIRCLE_BULB = 0,
            RED_BULB = 1,
            YELLOW_BULB = 2,
            GREEN_BULB = 3,
            LEFT_ARROW_BULB = 4,
            RIGHT_ARROW_BULB = 5,
            UP_ARROW_BULB = 6,
            DOWN_ARROW_BULB = 7,
            DOWN_LEFT_ARROW_BULB = 8,
            DOWN_RIGHT_ARROW_BULB = 9,
            CROSS_BULB = 10,
        }

        /// <summary>
        /// Bulb lighting status.
        /// </summary>
        public enum BulbStatus
        {
            SOLID_OFF = 0,        // Lights off.
            SOLID_ON = 1,        // Lights on.
            FLASHING = 2,        // Lights on every flashSec.
        }

        /// <summary>
        /// Bulb lighting color.
        /// </summary>
        public enum BulbColor
        {
            RED = 0,
            YELLOW = 1,
            GREEN = 2,
            WHITE = 3,
        }

        /// <summary>
        /// Used in TrafficLight.SetBulbData(). Based on the data in this class, the lighting of each bulb is controlled.
        /// </summary>
        [Serializable]
        public struct BulbData
        {
            public BulbType Type => type;

            public BulbColor Color => color;

            public BulbStatus Status => status;

            [SerializeField] BulbType type;
            [SerializeField] BulbColor color;
            [SerializeField] BulbStatus status;

            public BulbData(BulbType type, BulbColor color, BulbStatus status)
            {
                this.type = type;
                this.color = color;
                this.status = status;
            }
        }

        /// <summary>
        /// Emission configuration to be applied to the material when the bulb is lit.
        /// </summary>
        [Serializable]
        public class EmissionConfig
        {
            public BulbColor BulbColor;
            public Color Color;
            public float Intensity;
            [Range(0, 1)] public float ExposureWeight;
        }

        [Serializable]
        public class TrafficLightAssetBulbData
        {
            [SerializeField] public BulbType bulbType;
            [SerializeField, Tooltip("Specifies the index of the material to be used for the bulb.")]
            public int materialIndex;
            public EmissionConfig EmissionConfig;
        }

        public static EmissionConfig[] GetDefaultEmissionConfigs()
        {
            return new EmissionConfig[]
            {
                new EmissionConfig()
                {
                    BulbColor = BulbColor.GREEN,
                    Color = Color.green,
                    Intensity = 14,
                    ExposureWeight = 0.8f,
                },
                new EmissionConfig()
                {
                    BulbColor = BulbColor.YELLOW,
                    Color = Color.yellow,
                    Intensity = 14,
                    ExposureWeight = 0.8f,
                },
                new EmissionConfig()
                {
                    BulbColor = BulbColor.RED,
                    Color = Color.red,
                    Intensity = 14,
                    ExposureWeight = 0.8f,
                },
            };
        }

        public static TrafficLightAssetBulbData[] GetDefaultTrafficLightAssetBulbData(bool isHDRP)
        {
            float intensity = isHDRP ? 14f : 5.0f;
            float exposureWeight = isHDRP ? 0.8f : 0f;
            return new TrafficLightAssetBulbData[]
            {
                new TrafficLightAssetBulbData()
                {
                    bulbType = BulbType.GREEN_BULB,
                    materialIndex = 1,
                    EmissionConfig = new EmissionConfig()
                    {
                        BulbColor = BulbColor.GREEN,
                        Color = Color.green,
                        Intensity = intensity,
                        ExposureWeight = exposureWeight,
                    },
                },
                new TrafficLightAssetBulbData()
                {
                    bulbType = BulbType.YELLOW_BULB,
                    materialIndex = 2,
                    EmissionConfig = new EmissionConfig()
                    {
                        BulbColor = BulbColor.YELLOW,
                        Color = Color.yellow,
                        Intensity = intensity,
                        ExposureWeight = exposureWeight,
                    },
                },
                new TrafficLightAssetBulbData()
                {
                    bulbType = BulbType.RED_BULB,
                    materialIndex = 3,
                    EmissionConfig = new EmissionConfig()
                    {
                        BulbColor = BulbColor.RED,
                        Color = Color.red,
                        Intensity = intensity,
                        ExposureWeight = exposureWeight,
                    },
                },
            };
        }
    }
}
