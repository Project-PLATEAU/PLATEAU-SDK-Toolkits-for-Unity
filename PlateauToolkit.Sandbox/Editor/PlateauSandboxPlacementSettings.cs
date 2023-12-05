using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    public enum PlacementLocation
    {
        PlaceOnSurface,
        PlaceAlongTrack,
    }

    public enum PlacementUpVector
    {
        Normal,
        World,
    }

    public enum PlacementMode
    {
        Click,
        Brush,
    }

    class PlacementSettings
    {
        public PlacementLocation Location { get; set; }
        public PlacementUpVector UpVector { get; set; }

        PlacementMode m_Mode;
        public PlacementMode Mode
        {
            get => m_Mode;
            set
            {
                m_Mode = value;
                OnModeChanged?.Invoke();
            }
        }

        public event Action OnModeChanged;

        public BrushPlacementSettings Brush { get; } = new();
    }

    class BrushPlacementSettings
    {
        int m_ShapeRandomSeed;

        public BrushPlacementSettings()
        {
            // Make the initial seed.
            RandomizeShapeSeed();
        }

        public int InstantiationCount { get; set; } = 10;
        public float Radius { get; set; } = 3f;
        public float ForwardYAxis { set; get; }
        public Vector3 Forward => Quaternion.Euler(0, ForwardYAxis, 0) * Vector3.right;

        public int ShapeRandomSeed
        {
            get => m_ShapeRandomSeed;
            set
            {
                if (m_ShapeRandomSeed == value)
                {
                    return;
                }
                m_ShapeRandomSeed = value;
                OnShapeRandomSeedChanged?.Invoke(m_ShapeRandomSeed);
            }
        }

        public bool IsShapeRandomSeedFixed { get; set; }
        public float Spacing { get; set; } = 5f;

        public event Action<int> OnShapeRandomSeedChanged;

        /// <summary>
        /// Get randomized offsets for brush placement based on <see cref="System.Random"/> with <see cref="ShapeRandomSeed"/>
        /// </summary>
        /// <returns>Enumeration for randomized offsets</returns>
        public IEnumerable<Vector3> GetRandomOffsets()
        {
            var random = new System.Random(ShapeRandomSeed);
            for (int i = 0; i < InstantiationCount; i++)
            {
                Vector3 offset = Quaternion.Euler(0f, (float)(random.NextDouble() * 360), 0f) * Vector3.right;
                offset *= Radius * (float)random.NextDouble();
                yield return offset;
            }
        }

        public void RandomizeShapeSeed()
        {
            ShapeRandomSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }
    }
}
