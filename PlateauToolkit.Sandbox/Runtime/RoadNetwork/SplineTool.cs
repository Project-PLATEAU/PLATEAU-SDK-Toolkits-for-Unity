using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    public class SplineTool
    {
        //public static readonly int DISTANCE_CALCULATION_SAMPLES = 100; //距離計測のサンプリング数

        // 4つの制御点を使ってCatmull-Romスプラインの点を計算する
        //public static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        //{
        //    // Catmull-Rom スプラインの方程式
        //    return 0.5f * (
        //        (2f * p1) +
        //        (-p0 + p2) * t +
        //        (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
        //        (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        //    );
        //}

        // 2次ベジェ曲線の点を取得するメソッド
        //public static Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        //{
        //    // ベジェ曲線の式を使って計算
        //    float u = 1 - t;
        //    return u * u * p0 + 2 * u * t * p1 + t * t * p2;
        //}

        // 先頭と終了のアイテムをリストに追加（Control用Points)
        //public static List<Vector3> AddStartEndControlPoints(List<Vector3> controlPoints)
        //{
        //    List<Vector3> outControlPoints = new List<Vector3>();
        //    outControlPoints.Add(controlPoints.First());
        //    outControlPoints.AddRange(controlPoints);
        //    outControlPoints.Add(controlPoints.Last());
        //    return outControlPoints;
        //}

        // 指定したパーセンテージで曲線上の点を取得(距離ベース）
        // 車が横に移動してしまう問題あり
        //public static Vector3 GetPointOnSplineDistanceBased(List<Vector3> controlPoints, float t)
        //{
        //    t = Mathf.Clamp01(t);
        //    if (controlPoints.Count < 4)
        //    {
        //        return GetPointOnBezier(controlPoints, t);
        //    }

        //    controlPoints = AddStartEndControlPoints(controlPoints);

        //    float totalLength = CalculateSplineLength(controlPoints, DISTANCE_CALCULATION_SAMPLES);
        //    float targetDistance = totalLength * t;
        //    return GetPointOnSplineByDistance(controlPoints, targetDistance, DISTANCE_CALCULATION_SAMPLES);
        //}

        // スプラインの長さを計算するためにセグメントごとに細かくサンプリングする
        //public static float CalculateSplineLength(List<Vector3> points, int numSamples)
        //{
        //    float totalLength = 0;
        //    Vector3? previousPoint = null;

        //    for (int i = 0; i < points.Count - 3; i++)
        //    {
        //        for (int j = 0; j <= numSamples; j++)
        //        {
        //            float t = j / (float)numSamples;
        //            Vector3 currentPoint = CatmullRom(points[i], points[i + 1], points[i + 2], points[i + 3], t);

        //            if (previousPoint.HasValue)
        //            {
        //                totalLength += Vector3.Distance(previousPoint.Value, currentPoint);
        //            }

        //            previousPoint = currentPoint;
        //        }
        //    }

        //    return totalLength;
        //}

        // 距離に基づいたスプライン上の位置を取得
        //public static Vector3 GetPointOnSplineByDistance(List<Vector3> points, float distance, int numSamples)
        //{
        //    float accumulatedLength = 0;
        //    Vector3? previousPoint = null;

        //    for (int i = 0; i < points.Count - 3; i++)
        //    {
        //        for (int j = 0; j <= numSamples; j++)
        //        {
        //            float t = j / (float)numSamples;
        //            Vector3 currentPoint = CatmullRom(points[i], points[i + 1], points[i + 2], points[i + 3], t);

        //            if (previousPoint.HasValue)
        //            {
        //                float segmentLength = Vector3.Distance(previousPoint.Value, currentPoint);
        //                accumulatedLength += segmentLength;

        //                if (accumulatedLength >= distance)
        //                {
        //                    return currentPoint; // 指定した距離に達した点を返す
        //                }
        //            }

        //            previousPoint = currentPoint;
        //        }
        //    }

        //    return points[points.Count - 1]; // もし距離を超えた場合は、最後の点を返す
        //}

        // 指定したパーセンテージで曲線上の点を取得
        // ポイント単位のパーセントなので、アニメーションさせるとスピードがまちまちになる
        //public static Vector3 GetPointOnSpline(List<Vector3> controlPoints, float t)
        //{
        //    // t を 0～1 の範囲に制限
        //    t = Mathf.Clamp01(t);

        //    // controlPoints の個数が3点以下だとスプラインを生成できないので注意
        //    if (controlPoints.Count < 4)
        //    {
        //        return GetPointOnBezier(controlPoints, t);
        //    }

        //    controlPoints = AddStartEndControlPoints(controlPoints);

        //    // セグメント数
        //    int segmentCount = controlPoints.Count - 3;

        //    // パーセンテージからセグメントを計算
        //    float scaledT = t * segmentCount;
        //    int segmentIndex = Mathf.FloorToInt(scaledT);
        //    segmentIndex = Mathf.Clamp(segmentIndex, 0, segmentCount - 1);

        //    // 各セグメント内のローカル t 値を計算
        //    float localT = scaledT - segmentIndex;

        //    // スプラインの点を計算
        //    Vector3 p0 = controlPoints[segmentIndex];
        //    Vector3 p1 = controlPoints[segmentIndex + 1];
        //    Vector3 p2 = controlPoints[segmentIndex + 2];
        //    Vector3 p3 = controlPoints[segmentIndex + 3];

        //    return CatmullRom(p0, p1, p2, p3, localT);
        //}

        // 曲線上の点をパーセント単位で取得するメソッド
        //public static Vector3 GetPointOnBezier(List<Vector3> controlPoints, float t)
        //{
        //    if (controlPoints.Count < 3)
        //    {
        //        if (controlPoints.Count < 2)
        //        {
        //            if (controlPoints.Count <= 0)
        //                return Vector3.zero;

        //            return controlPoints.First();
        //        }

        //        return Vector3.Lerp(controlPoints[0], controlPoints[1], t);
        //    }

        //    // t を 0～1 の範囲に制限
        //    t = Mathf.Clamp01(t);

        //    // 制御点を指定
        //    Vector3 p0 = controlPoints[0];
        //    Vector3 p1 = controlPoints[1];
        //    Vector3 p2 = controlPoints[2];

        //    // 2次ベジェ曲線の点を計算
        //    return QuadraticBezier(p0, p1, p2, t);
        //}

        // Unity Spline 動的生成
        public static Spline CreateSplineFromPoints(List<Vector3> points)
        {
            Spline spline = new Spline();

            foreach (Vector3 point in points)
            {
                var knot = new BezierKnot(point);
                spline.Add(knot, TangentMode.AutoSmooth);
            }

            return spline;
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

        //　リニア（直線）
        public static Vector3 GetPointOnLine(List<Vector3> points, float percentage)
        {
            if (points == null || points.Count < 2)
            {
                return Vector3.zero;
            }


            if (percentage < 0f || percentage > 1f)
            {
                return points.Last();
            }


            // 総距離を計算
            float totalDistance = 0f;
            List<float> segmentDistances = new List<float>();

            for (int i = 0; i < points.Count - 1; i++)
            {
                float distance = Vector3.Distance(points[i], points[i + 1]);
                segmentDistances.Add(distance);
                totalDistance += distance;
            }

            // 目標の距離（前線分のパーセント）を計算
            float targetDistance = totalDistance * percentage;

            // 線をたどりながら、目標の距離に達するまで進む
            float accumulatedDistance = 0f;
            for (int i = 0; i < points.Count - 1; i++)
            {
                float segmentDistance = segmentDistances[i];
                if (accumulatedDistance + segmentDistance >= targetDistance)
                {
                    // この区間で目標の距離に達した場合、補間で位置を求める
                    float remainingDistance = targetDistance - accumulatedDistance;
                    float t = remainingDistance / segmentDistance; // 0から1までの補間係数
                    return Vector3.Lerp(points[i], points[i + 1], t);
                }
                accumulatedDistance += segmentDistance;
            }

            // 万が一、計算が範囲外の場合、最後の点を返す（通常は起こらない）
            return points.Last();
        }
    }
}
