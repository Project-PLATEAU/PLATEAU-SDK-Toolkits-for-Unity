using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    //Start後の経過時間から移動パーセント(0-1f)を計測
    public class DistanceCalculator
    {
        float m_SpeedKmPerHour = 0f;
        float m_SpeedMetersPerSecond = 0f;
        float m_TotalDistanceMeters = 0f;
        float m_StartTime = 0f;

        float m_AdditionalPercentage = 0f;

        float m_TargetSpeedKm = 0f; //ChangeSpeedTo の目標値

        public DistanceCalculator(float speed, float distance, float offset)
        {
            m_SpeedKmPerHour = speed;
            m_SpeedMetersPerSecond = (m_SpeedKmPerHour * 1000f) / 3600f;
            m_TotalDistanceMeters = distance;
            m_AdditionalPercentage = offset;
            Start();
        }

        public float GetCurrentSpeedKm()
        {
            return m_SpeedKmPerHour;
        }

        //バグってる
        //呼ばれる度に速度をstep単位で変更(加速、減速）
        //public void ChangeSpeedTo(float speed, float step = 0.1f)
        //{
        //    m_TargetSpeedKm = speed;
        //    var diff = m_SpeedKmPerHour - m_TargetSpeedKm;
        //    if (diff == 0f)
        //        return;

        //    var nextSpeed = diff > 0f ? m_SpeedKmPerHour - step : m_SpeedKmPerHour + step;

        //    Debug.Log($"nextSpeed {nextSpeed}");
        //    ChangeSpeed(nextSpeed);
        //}

        //走行中に速度変更
        public void ChangeSpeed(float speed)
        {
            m_AdditionalPercentage = GetPercent();

            float elapsedTimeSeconds = Time.time - m_StartTime;
            if (elapsedTimeSeconds * m_SpeedMetersPerSecond < m_TotalDistanceMeters)
            {
                float currentDistance = elapsedTimeSeconds * m_SpeedMetersPerSecond;
                m_TotalDistanceMeters -= currentDistance;
            }

            m_SpeedKmPerHour = speed;
            m_SpeedMetersPerSecond = (m_SpeedKmPerHour * 1000f) / 3600f;
            Start();
        }

        public void Start()
        {
            m_StartTime = Time.time;
        }

        public float GetPercent()
        {
            float elapsedTimeSeconds = Time.time - m_StartTime;
            float progressPercentage = 1f;

            if (elapsedTimeSeconds * m_SpeedMetersPerSecond < m_TotalDistanceMeters)
            {
                // 経過時間に基づく移動距離
                float currentDistance = elapsedTimeSeconds * m_SpeedMetersPerSecond;

                // 距離の進捗パーセンテージ
                progressPercentage = (currentDistance / m_TotalDistanceMeters);

                // 経過時間と進行状況を出力
                //Debug.Log($"経過時間: {elapsedTimeSeconds} 秒, 移動距離: {currentDistance} m, 進行状況: {progressPercentage}%");
            }
            return m_AdditionalPercentage + progressPercentage;
        }
    }
}
