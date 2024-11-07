using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AWSIM
{
    public class TimeScaleSettingsUI : MonoBehaviour
    {
        [SerializeField] Text timeScaleText;
        [SerializeField] Slider timeScaleSlider;

        void Start()
        {
            timeScaleSlider.value = Time.timeScale;
            timeScaleText.text = "x " + timeScaleSlider.value.ToString("F2");
        }

        public void SetTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;
            timeScaleText.text = "x " + timeScale.ToString("F2");

            // synchronisation of new timescale value with TimeScaleProvider
            //TimeScaleProvider.DoUpdate();
        }
    }
}