using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PlateauToolkit.Rendering.Editor
{
    public static class PlateauRenderingMathUtilities
    {
        // constants used in approximate equality checks
        internal static readonly float k_EpsilonScaled = Mathf.Epsilon * 8;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(float a, float b)
        {
            float d = b - a;
            float absDiff = d >= 0f ? d : -d;
            return absDiff < k_EpsilonScaled;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproximatelyZero(float a)
        {
            return (a >= 0f ? a : -a) < k_EpsilonScaled;
        }
    }
}
