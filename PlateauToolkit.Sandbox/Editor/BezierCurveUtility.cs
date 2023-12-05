using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace PlateauToolkit.Sandbox.Editor
{
    static class BezierCurveUtility
    {
        const int k_SegmentsPointCount = 30;
        static readonly Vector3[] k_ClosedPointArray = new Vector3[k_SegmentsPointCount];

        /// <summary>
        /// Get the nearest point from the mouse position in the Scene view.
        /// </summary>
        /// <remarks>
        /// This method is copied from the Unity splines package.
        /// </remarks>
        // ReSharper disable once OutParameterValueIsAlwaysDiscarded.Local
        public static void GetHandleNearestPointOnCurve(BezierCurve curve, out Vector3 position, out float t, out float distance)
        {
            Vector3 closestA = Vector3.zero;
            Vector3 closestB = Vector3.zero;
            float closestDist = float.MaxValue;
            int closestSegmentFirstPoint = -1;

            GetCurveSegments(curve, k_ClosedPointArray);
            for (int j = 0; j < k_ClosedPointArray.Length - 1; ++j)
            {
                Vector3 a = k_ClosedPointArray[j];
                Vector3 b = k_ClosedPointArray[j + 1];
                float dist = HandleUtility.DistanceToLine(a, b);

                if (dist < closestDist)
                {
                    closestA = a;
                    closestB = b;
                    closestDist = dist;
                    closestSegmentFirstPoint = j;
                }
            }

            // Calculate position
            Vector2 screenPosA = HandleUtility.WorldToGUIPoint(closestA);
            Vector2 screenPosB = HandleUtility.WorldToGUIPoint(closestB);
            Vector2 relativePoint = Event.current.mousePosition - screenPosA;
            Vector2 lineDirection = screenPosB - screenPosA;
            float length = lineDirection.magnitude;
            float dot = Vector3.Dot(lineDirection, relativePoint);
            if (length > .000001f)
            {
                dot /= length * length;
            }

            dot = Mathf.Clamp01(dot);
            position = Vector3.Lerp(closestA, closestB, dot);

            // Calculate percent on curve's segment
            float percentPerSegment = 1.0f / (k_SegmentsPointCount - 1);
            float percentA = closestSegmentFirstPoint * percentPerSegment;
            float lengthA2B = (closestB - closestA).magnitude;
            float lengthAToClosest = (position - closestA).magnitude;
            t = percentA + percentPerSegment * (lengthAToClosest / lengthA2B);
            distance = closestDist;
        }

        static void GetCurveSegments(BezierCurve curve, Vector3[] results)
        {
            float segmentPercentage = 1f / (results.Length - 1);
            for (int i = 0; i < k_SegmentsPointCount; ++i)
            {
                results[i] = CurveUtility.EvaluatePosition(curve, i * segmentPercentage);
            }
        }
    }
}