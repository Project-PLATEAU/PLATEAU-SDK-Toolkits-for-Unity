
using log4net.Util;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace PlateauToolkit.Sandbox
{
    public class SplineTool
    {
        // 4つの制御点を使ってCatmull-Romスプラインの点を計算する
        public static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            // Catmull-Rom スプラインの方程式
            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
            );
        }

        // 2次ベジェ曲線の点を取得するメソッド
        public static Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            // ベジェ曲線の式を使って計算
            float u = 1 - t;
            return u * u * p0 + 2 * u * t * p1 + t * t * p2;
        }

        // 指定したパーセンテージで曲線上の点を取得
        public static Vector3 GetPointOnSpline(List<Vector3> controlPoints, float t)
        {
            // t を 0～1 の範囲に制限
            t = Mathf.Clamp01(t);

            // controlPoints の個数が3点以下だとスプラインを生成できないので注意
            if (controlPoints.Count < 4)
            {
                return GetPointOnBezier(controlPoints, t);
            }

            // セグメント数
            int segmentCount = controlPoints.Count - 3;

            // パーセンテージからセグメントを計算
            float scaledT = t * segmentCount;
            int segmentIndex = Mathf.FloorToInt(scaledT);
            segmentIndex = Mathf.Clamp(segmentIndex, 0, segmentCount - 1);

            // 各セグメント内のローカル t 値を計算
            float localT = scaledT - segmentIndex;

            // スプラインの点を計算
            Vector3 p0 = controlPoints[segmentIndex];
            Vector3 p1 = controlPoints[segmentIndex + 1];
            Vector3 p2 = controlPoints[segmentIndex + 2];
            Vector3 p3 = controlPoints[segmentIndex + 3];

            return CatmullRom(p0, p1, p2, p3, localT);
        }

        // 曲線上の点をパーセント単位で取得するメソッド
        public static Vector3 GetPointOnBezier(List<Vector3> controlPoints, float t)
        {
            if (controlPoints.Count < 3)
            {
                if (controlPoints.Count < 2)
                {
                    Debug.LogError("At least 2 control points are required.");
                    return Vector3.zero;
                }

                return Vector3.Lerp(controlPoints[0], controlPoints[1], t);
            }

            // t を 0～1 の範囲に制限
            t = Mathf.Clamp01(t);

            // 制御点を指定
            Vector3 p0 = controlPoints[0];
            Vector3 p1 = controlPoints[1];
            Vector3 p2 = controlPoints[2];

            // 2次ベジェ曲線の点を計算
            return QuadraticBezier(p0, p1, p2, t);
        }

        // Unity Spline 上のPoint
        public static Vector3 GetPointOnSpline(Spline spline, float percent)
        {
            int splineIndex = (int)math.floor(percent);
            float t = math.frac(percent);
            if (splineIndex > 0 && t == 0f)
            {
                t = splineIndex - 1;
            }

            return spline.EvaluatePosition(t);
        }

    }
}
