using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.VFX;
#if UNITY_HDRP
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
#endif

namespace PlateauToolkit.Rendering
{
    [ExecuteInEditMode]
    public class EnvironmentController : MonoBehaviour
    {
        // make it singleton
        static EnvironmentController s_Instance;
        public static EnvironmentController Instance
        {
            get
            {
                return s_Instance;
            }
        }

        public enum MaterialQuality
        {
            Low,
            Medium,
            High, m_SnowVFX
        }

        public MaterialQuality m_Quality = MaterialQuality.High;

        // Latitude, longitude
        public Vector2 m_Location = new Vector2(35.6895f, 139.69171f);
        public Vector3 m_Date = new Vector3(2023, 5, 10);
        public int m_TimeZone = 9;
        [Range(0, 1)]
        public float m_TimeOfDay;
        public float m_LerpSpeed = 1f;
        [Range(0, 1)]
        public float m_Cloud;
        public Color m_SunColor = Color.white;
        public Color m_MoonColor = new Color(0.0f, 0.2f, 0.3f);
        public float m_SunIntensity = 1;
        public float m_MoonIntensity = 1f;

        public float m_Rain = 0f;
        public float m_Snow = 0f;
        public float m_FogDistance = 200000;
        public Color m_FogColor = Color.white;
        public float m_MaterialFade = 0f;

        public Color m_BuildingColor = Color.white;

        public float m_OpacityDistMin = 0.0f;
        public float m_OpacityDistMax = 0.0f;
        public float m_GlobalOpacity = 1.0f;

        DateTime m_Time;
        float m_IsNight;
        VisualEffect m_RainVFX;
        VisualEffect m_SnowVFX;
        AudioSource m_RainAudio;
        AudioSource m_SnowAudio;
        ParticleSystem m_RainMobile;
        ParticleSystem m_SnowMobile;
        Light m_SunLight;
        Light m_MoonLight;
        public float m_MoonScale = 1f;

#if UNITY_URP
        public float m_MoonPhase = 0.0f;
        public Gradient m_CloudySaturationGradient = new Gradient();
#endif
        public Gradient m_CloudOpacityGradiant = new Gradient();

#if UNITY_HDRP
        HDAdditionalLightData m_SunHD;
        HDAdditionalLightData m_MoonHD;
        public GameObject m_EnvironmentVolume;
#endif
        DateTime m_DateTimeDelta;
        Quaternion m_SunTargetRot;
        Quaternion m_MoonTargetRot;
        const double k_Deg2Rad = Math.PI / 180;
        const double k_Rad2Deg = 180 / Math.PI;

        public float IsNight
        {
            get { return m_IsNight; }
        }

        public void Awake()
        {
            if (s_Instance != null)
            {
                s_Instance = this;
            }

            m_RainVFX = transform.Find("Rain")?.GetComponent<VisualEffect>();
            m_SnowVFX = transform.Find("Snow")?.GetComponent<VisualEffect>();
            m_RainAudio = transform.Find("Rain")?.GetComponent<AudioSource>();
            m_SnowAudio = transform.Find("Snow")?.GetComponent<AudioSource>();

            m_RainMobile = transform.Find("Rain")?.GetComponent<ParticleSystem>();
            m_SnowMobile = transform.Find("Snow")?.GetComponent<ParticleSystem>();

            m_SunLight = transform.Find("Sun")?.GetComponent<Light>();
            m_MoonLight = transform.Find("Moon")?.GetComponent<Light>();

#if UNITY_HDRP
            if (m_SunLight != null)
            {
                m_SunHD = m_SunLight.GetComponent<HDAdditionalLightData>();
            }
            if (m_MoonLight != null)
            {
                m_MoonHD = m_MoonLight.GetComponent<HDAdditionalLightData>();
            }

            // Set default value for HDRP
            if (m_SunIntensity == 1f)
            {
                m_SunIntensity = 130000f;
            }

            if (m_MoonIntensity == 1f)
            {
                m_MoonIntensity = 100f;
            }
#endif
        }

        public void SetMaterialQuality(MaterialQuality quality)
        {
#if UNITY_URP
            m_Quality = quality;
            switch (quality)
            {
                case MaterialQuality.Low:
                    Shader.EnableKeyword("MATERIAL_QUALITY_LOW");
                    Shader.DisableKeyword("MATERIAL_QUALITY_MEDIUM");
                    Shader.DisableKeyword("MATERIAL_QUALITY_HIGH");
                    break;
                case MaterialQuality.Medium:
                    Shader.DisableKeyword("MATERIAL_QUALITY_LOW");
                    Shader.EnableKeyword("MATERIAL_QUALITY_MEDIUM");
                    Shader.DisableKeyword("MATERIAL_QUALITY_HIGH");
                    break;
                case MaterialQuality.High:
                    Shader.DisableKeyword("MATERIAL_QUALITY_LOW");
                    Shader.DisableKeyword("MATERIAL_QUALITY_MEDIUM");
                    Shader.EnableKeyword("MATERIAL_QUALITY_HIGH");
                    break;
            }
#endif
        }

        void Update()
        {
#if UNITY_HDRP
            if (!m_SunHD || !m_MoonHD)
            {
                return;
            }
            UpdateCloudLayerVolume();
#endif

#if UNITY_URP
            if (!m_SunLight || !m_MoonLight)
            {
                return;
            }
#endif
            UpdateLights();
            UpdateDayNightShift();

            Shader.SetGlobalFloat("_HideMaterial", m_MaterialFade);
            Shader.SetGlobalColor("_BuildingColor", m_BuildingColor);
        }
#if UNITY_HDRP
        void UpdateCloudLayerVolume()
        {
            // Make sure the environmentVolumeGameObject is set
            if (m_EnvironmentVolume == null)
            {
                return;
            }

            // Get the Volume component from the environmentVolumeGameObject
            Volume volume = m_EnvironmentVolume.GetComponent<Volume>();
            if (volume == null || volume.profile == null)
            {
                return;
            }

            // Access the CloudLayer settings
            CloudLayer cloudLayer = null;
            if (!volume.profile.TryGet<CloudLayer>(out cloudLayer))
            {
                return;
            }

            cloudLayer.layerA.tint.value = new Color(1.0f - m_IsNight, 1.0f - m_IsNight, 1.0f - m_IsNight);
            Color opacityRGBA = m_CloudOpacityGradiant.Evaluate(Mathf.Abs(m_Cloud));
            cloudLayer.layerA.opacityR.value = opacityRGBA.r;
            cloudLayer.layerA.opacityG.value = opacityRGBA.g;
            cloudLayer.layerA.opacityB.value = opacityRGBA.b;
            cloudLayer.layerA.opacityA.value = opacityRGBA.a;

            // Access the CloudLayer settings
            Fog fog = null;
            if (!volume.profile.TryGet<Fog>(out fog))
            {
                return;
            }

            fog.meanFreePath.value = m_FogDistance;
            fog.tint.value = m_FogColor;
        }
#endif
        void UpdateLights()
        {
            m_Time = CombineDateAndTime();
            m_Time = m_Time.ToUniversalTime();

            Vector3 sunDir = CalculateSunDirection(m_Time, m_Location.x, m_Location.y);
            Vector3 moonDir = CalculateMoonDirection(m_Time, m_Location.x, m_Location.y);
            m_SunTargetRot = Quaternion.Euler(sunDir);
            m_MoonTargetRot = Quaternion.Euler(moonDir);

#if UNITY_HDRP
            m_SunHD.color = m_SunColor;
            m_MoonHD.color = m_MoonColor;
#endif
#if UNITY_URP
            m_SunLight.color = CalculateSkyColor(sunDir.x) * m_SunColor;
            Color grayscale = new Color(m_MoonColor.grayscale, m_MoonColor.grayscale, m_MoonColor.grayscale) * 0.75f;
            m_MoonLight.color = Color.Lerp(m_MoonColor, grayscale, m_Cloud);

            Shader.SetGlobalFloat("_CloudIntensity", m_CloudOpacityGradiant.Evaluate(Mathf.Abs(m_Cloud)).r);
            Shader.SetGlobalFloat("_CloudySaturation", m_CloudySaturationGradient.Evaluate(Mathf.Abs(m_Cloud)).r);
#endif

            Shader.SetGlobalFloat("_Cloud", Remap(m_Cloud, 0, 1, 0, 0.8f));
            Shader.SetGlobalFloat("_Rain", m_Rain);
            Shader.SetGlobalFloat("_Snow", m_Snow);
            Shader.SetGlobalFloat("_GlobalOpacityDistMin", m_OpacityDistMin);
            Shader.SetGlobalFloat("_GlobalOpacityDistMax", m_OpacityDistMax);
            Shader.SetGlobalFloat("_GlobalTransparency", 1.0f - m_GlobalOpacity);

            // check if m_RainVFX has particle rate
            if (m_RainVFX)
            {
                if (m_RainVFX.HasInt("Particle Rate"))
                {
                    m_RainVFX.SetInt("Particle Rate", (int)(m_Rain * 20000));
                }
                if (m_RainVFX.HasVector4("SunColor"))
                {
                    Color particleBrightness = Color.white * (1 - m_IsNight);
                    m_RainVFX.SetVector4("SunColor", particleBrightness);
                }
                if (Camera.main != null)
                {
                    m_RainVFX.transform.position = Camera.main.transform.position;
                }
            }

            if (m_RainMobile)
            {
                ParticleSystem.EmissionModule emission = m_RainMobile.emission;
                emission.rateOverTime = m_Rain * 500;
                if (Camera.main != null)
                {
                    m_RainMobile.transform.position = Camera.main.transform.position;
                }
            }

            if (m_RainAudio)
            {
                m_RainAudio.volume = m_Rain;
            }

            if (m_SnowVFX)
            {
                if (m_SnowVFX.HasInt("Particle Rate"))
                {
                    m_SnowVFX.SetInt("Particle Rate", (int)(m_Snow * 20000));

                    if (Camera.main != null)
                    {
                        m_SnowVFX.transform.position = Camera.main.transform.position;
                    }
                }
                if (m_SnowVFX.HasVector4("SunColor"))
                {
                    Color particleBrightness = Color.white * (1 - m_IsNight);
                    m_SnowVFX.SetVector4("SunColor", particleBrightness);
                }
                if (Camera.main != null)
                {
                    m_SnowVFX.transform.position = Camera.main.transform.position;
                }
            }

            if (m_SnowMobile)
            {
                ParticleSystem.EmissionModule emission = m_SnowMobile.emission;
                emission.rateOverTime = m_Snow * 500;

                if (Camera.main != null)
                {
                    m_SnowMobile.transform.position = Camera.main.transform.position;
                }
            }

            if (m_SnowAudio)
            {
                m_SnowAudio.volume = m_Snow;
            }

            m_DateTimeDelta = m_Time;

            m_SunLight.transform.rotation = Quaternion.Lerp(m_SunLight.transform.rotation, m_SunTargetRot, m_LerpSpeed * Time.deltaTime);
            m_MoonLight.transform.rotation = Quaternion.Lerp(m_MoonLight.transform.rotation, m_MoonTargetRot, m_LerpSpeed * Time.deltaTime);

            float threshold = 0.05f;

#if UNITY_HDRP
            float sunIntensity = m_SunHD.intensity;
            float moonIntensity = m_MoonHD.intensity;
#endif

            if (m_SunLight.transform.forward.y < threshold && m_MoonLight.transform.forward.y < threshold)
            {
#if UNITY_HDRP

                sunIntensity = m_SunHD.intensity;
                moonIntensity = m_MoonHD.intensity;

                float sunIntensitySmoothed = Mathf.SmoothStep(sunIntensity, m_SunIntensity, m_LerpSpeed * Time.deltaTime);
                m_SunHD.intensity = sunIntensitySmoothed;

                if (moonIntensity > 0.001f)
                {
                    float moonIntensitySmoothed = Mathf.SmoothStep(moonIntensity, 0f, m_LerpSpeed * Time.deltaTime);
                    m_MoonHD.intensity = moonIntensitySmoothed;
                }
                else
                {
                    m_MoonHD.intensity = 0f;
                }
#endif

#if UNITY_URP
                m_SunLight.intensity = Mathf.SmoothStep(m_SunLight.intensity, m_SunIntensity, m_LerpSpeed * Time.deltaTime);
                if (m_MoonLight.intensity > 0.001f)
                {
                    m_MoonLight.intensity = Mathf.SmoothStep(m_MoonLight.intensity, 0f, m_LerpSpeed * Time.deltaTime);
                }
                else
                {
                    m_MoonLight.intensity = 0f;
                }

                RenderSettings.ambientSkyColor = m_SunLight.color;
                float sunDirComp = -m_SunLight.transform.forward.y * 0.5f + 0.5f;
                RenderSettings.fogColor = m_FogColor * RenderSettings.ambientSkyColor * sunDirComp;
#endif
                m_SunLight.shadows = LightShadows.Soft;
                m_MoonLight.shadows = LightShadows.None;
                // Disable moonlight shadows
                Shader.SetGlobalFloat("_SkyMultiplier", m_SunLight.intensity);
            }
            else if (m_SunLight.transform.forward.y < threshold && m_MoonLight.transform.forward.y > threshold)
            {
#if UNITY_HDRP
                sunIntensity = m_SunHD.intensity;
                moonIntensity = m_MoonHD.intensity;

                float sunIntensitySmoothed = Mathf.SmoothStep(sunIntensity, m_SunIntensity, m_LerpSpeed * Time.deltaTime);
                m_SunHD.intensity = sunIntensitySmoothed;

                if (moonIntensity > 0.001f)
                {
                    float moonIntensitySmoothed = Mathf.SmoothStep(moonIntensity, 0f, m_LerpSpeed * Time.deltaTime);
                    m_MoonHD.intensity = moonIntensitySmoothed;
                }
                else
                {
                    m_MoonHD.intensity = 0f;
                }
#endif
#if UNITY_URP
                m_SunLight.intensity = Mathf.SmoothStep(m_SunLight.intensity, m_SunIntensity, m_LerpSpeed * Time.deltaTime);
                if (m_MoonLight.intensity > 0.001f)
                {
                    m_MoonLight.intensity = Mathf.SmoothStep(m_MoonLight.intensity, 0f, m_LerpSpeed * Time.deltaTime);
                }
                else
                {
                    m_MoonLight.intensity = 0f;
                }

                RenderSettings.ambientSkyColor = m_SunLight.color;
                float sunDirComp = -m_SunLight.transform.forward.y * 0.5f + 0.5f;
                RenderSettings.fogColor = m_FogColor * RenderSettings.ambientSkyColor * sunDirComp;
#endif
                m_SunLight.shadows = LightShadows.Soft;
                m_MoonLight.shadows = LightShadows.None;
                Shader.SetGlobalFloat("_SkyMultiplier", m_SunLight.intensity);
            }
            else
            {
#if UNITY_HDRP
                sunIntensity = m_SunHD.intensity;
                moonIntensity = m_MoonHD.intensity;

                float moonIntensitySmoothed = Mathf.SmoothStep(moonIntensity, m_MoonIntensity, m_LerpSpeed * Time.deltaTime);
                m_MoonHD.intensity = moonIntensitySmoothed;

                if (sunIntensity > 0.001f)
                {
                    float sunIntensitySmoothed = Mathf.SmoothStep(sunIntensity, 0f, m_LerpSpeed * Time.deltaTime);
                    m_SunHD.intensity = sunIntensitySmoothed;
                }
                else
                {
                    m_SunHD.intensity = 0f;
                }
#endif
#if UNITY_URP
                if (m_SunLight.intensity > 0.001f)
                {
                    m_SunLight.intensity = Mathf.SmoothStep(m_SunLight.intensity, 0f, m_LerpSpeed * Time.deltaTime);
                }
                else
                {
                    m_SunLight.intensity = 0f;
                }

                m_MoonLight.intensity = Mathf.SmoothStep(m_MoonLight.intensity, m_MoonIntensity, m_LerpSpeed * Time.deltaTime);
                RenderSettings.ambientSkyColor = m_MoonLight.color;
                float moonDirComp = -m_SunLight.transform.forward.y * 0.5f + 0.5f;
                RenderSettings.fogColor = m_FogColor * RenderSettings.ambientSkyColor * moonDirComp;

#endif
                m_SunLight.shadows = LightShadows.None;
                m_MoonLight.shadows = LightShadows.Soft;
                Shader.SetGlobalFloat("_SkyMultiplier", m_MoonLight.intensity);
            }

            // Use HDRP light activation
#if UNITY_HDRP
            m_SunHD.gameObject.SetActive(sunIntensity != 0);
            m_MoonHD.gameObject.SetActive(moonIntensity != 0);
            m_MoonLight.GetComponent<HDAdditionalLightData>().angularDiameter = m_MoonScale;
#endif
#if UNITY_URP
            m_SunLight.gameObject.SetActive(m_SunLight.intensity != 0);
            m_MoonLight.gameObject.SetActive(m_MoonLight.intensity != 0);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogEndDistance = m_FogDistance;

            if (RenderSettings.skybox != null)
            {
                Shader.SetGlobalFloat("_FogDistance", m_FogDistance);
                Shader.SetGlobalColor("_FogColor", m_FogColor);

                if (m_MoonLight)
                {
                    Vector3 moonLightDirection = m_MoonLight.transform.forward;
                    Vector3 moonLightRight = m_MoonLight.transform.right;
                    Vector3 moonLightUp = m_MoonLight.transform.up;

                    Shader.SetGlobalVector("_MoonLightDirection", moonLightDirection);
                    Shader.SetGlobalVector("_MoonLightRight", moonLightRight);
                    Shader.SetGlobalVector("_MoonLightUp", moonLightUp);
                    Shader.SetGlobalFloat("_MoonScale", m_MoonScale);
                    Shader.SetGlobalFloat("_MoonPhase", m_MoonPhase);
                }
            }
#endif
        }

        void SetProperty(object obj, string propertyName, object value)
        {
            PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (propertyInfo != null)
            {
                MethodInfo overrideMethod = propertyInfo.PropertyType.GetMethod("Override", BindingFlags.Instance | BindingFlags.Public);
                if (overrideMethod != null)
                {
                    overrideMethod.Invoke(propertyInfo.GetValue(obj), new object[] { value });
                }
            }
        }

        float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        void UpdateDayNightShift()
        {
            float y = m_SunLight.transform.forward.y;
            y = Mathf.Clamp(y, -0.2f, 0.2f);
            m_IsNight = Remap(y, -0.2f, 0.2f, 0f, 1f);
            Shader.SetGlobalFloat("_IsNight", m_IsNight);
        }

        Color CalculateSkyColor(float sunAltitude)
        {
            const float temperatureAtHorizon = 1000; // K
            const float temperatureAtZenith = 13000; // K

            float v = Remap(sunAltitude, -10, 90, 0, 1);
            float temperature = Mathf.Lerp(temperatureAtHorizon, temperatureAtZenith, v);
            temperature = Mathf.Clamp(temperature, 1000, 6500);
            temperature /= 100;

            float red, green, blue;
            if (temperature <= 66)
            {
                red = 255;
            }
            else
            {
                red = temperature - 60;
                red = 329.698727446f * Mathf.Pow(red, -0.1332047592f);
                red = Mathf.Clamp(red, 0, 255);
            }
            if (temperature <= 66)
            {
                green = temperature;
                green = 99.4708025861f * Mathf.Log(green) - 161.1195681661f;
                green = Mathf.Clamp(green, 0, 255);
            }
            else
            {
                green = temperature - 60;
                green = 288.1221695283f * Mathf.Pow(green, -0.0755148492f);
                green = Mathf.Clamp(green, 0, 255);
            }
            if (temperature >= 66)
            {
                blue = 255;
            }
            else
            {
                if (temperature <= 19)
                {
                    blue = 0;
                }
                else
                {
                    blue = temperature - 10;
                    blue = 138.5177312231f * Mathf.Log(blue) - 305.0447927307f;
                    blue = Mathf.Clamp(blue, 0, 255);
                }
            }

            var col = new Color(red / 255f, green / 255f, blue / 255f);
            col = Color.Lerp(col, Color.white * 0.75f, m_Cloud);
            return col;
        }

        public string GetTimeString()
        {
            // Return 23:59 format from m_time which is DateTime
            DateTime localTime = m_Time.ToLocalTime();
            return localTime.ToString("HH:mm");

        }
        DateTime CombineDateAndTime()
        {
            int year = (int)m_Date.x;
            int month = (int)m_Date.y;
            int day = (int)m_Date.z;
            double totalHours = m_TimeOfDay * 24;
            int hour = (int)totalHours;
            int minute = (int)((totalHours - hour) * 60);
            int second = (int)((((totalHours - hour) * 60) - minute) * 60);

            if (m_TimeOfDay >= 0.9999)
            {
                hour = hour % 24;
            }

            DateTime combinedDateTime;

            try
            {
                combinedDateTime = new DateTime(year, month, day, hour, minute, second, m_TimeZone);
            }
            catch
            {

                combinedDateTime = DateTime.Now;
            }

            return combinedDateTime;
        }

        public Vector3 CalculateSunDirection(DateTime dateTime, float latitude, float longitude)
        {
            double julianDate = dateTime.ToOADate() + 2415018.5;

            double julianCenturies = julianDate / 36525.0;
            double siderealTimeHours = 6.6974 + 2400.0513 * julianCenturies;
            double siderealTimeUT = siderealTimeHours +
                (366.2422 / 365.2422) * dateTime.TimeOfDay.TotalHours;
            double siderealTime = siderealTimeUT * 15 + longitude;
            julianDate += dateTime.TimeOfDay.TotalHours / 24.0;
            julianCenturies = julianDate / 36525.0;
            double meanLongitude = NormalizeAngle(k_Deg2Rad * (280.466 + 36000.77 * julianCenturies));
            double meanAnomaly = NormalizeAngle(k_Deg2Rad * (357.529 + 35999.05 * julianCenturies));
            double equationOfCenter = k_Deg2Rad * ((1.915 - 0.005 * julianCenturies) *
                Math.Sin(meanAnomaly) + 0.02 * Math.Sin(2 * meanAnomaly));
            double elipticalLongitude = NormalizeAngle(meanLongitude + equationOfCenter);

            double obliquity = (23.439 - 0.013 * julianCenturies) * Math.PI / 180.0;
            double rightAscension = Math.Atan2(
                Math.Cos(obliquity) * Math.Sin(elipticalLongitude),
                Math.Cos(elipticalLongitude));
            double declination = Math.Asin(
                Math.Sin(rightAscension) * Math.Sin(obliquity));
            double hourAngle = rightAscension - NormalizeAngle(siderealTime * Math.PI / 180.0);

            if (hourAngle > Math.PI)
            {
                hourAngle -= 2 * Math.PI;
            }

            double altitude = Math.Asin(Math.Sin(latitude * Math.PI / 180.0) *
                Math.Sin(declination) + Math.Cos(latitude * Math.PI / 180.0) *
                Math.Cos(declination) * Math.Cos(hourAngle));
            double aziNom = -Math.Sin(hourAngle);
            double aziDenom =
                Math.Tan(declination) * Math.Cos(latitude * Math.PI / 180.0) -
                Math.Sin(latitude * Math.PI / 180.0) * Math.Cos(hourAngle);

            double azimuth = Math.Atan(aziNom / aziDenom);

            if (aziDenom < 0)
            {
                azimuth += Math.PI;
            }
            else if (aziNom < 0)
            {
                azimuth += 2 * Math.PI;
            }

            altitude = altitude * k_Rad2Deg;
            azimuth = azimuth * k_Rad2Deg;

            return new Vector3((float)altitude, (float)azimuth, 0);
        }

        public Vector3 CalculateMoonDirection(DateTime dateTime, float latitude, float longitude)
        {
            dateTime = dateTime.AddHours(5); // Add 5 hour offset

            double j1899 = 2415018.5;
            double j2000 = 2451545;
            double julianDate = (dateTime.ToOADate() + j1899) - j2000;

            double lw = k_Deg2Rad * -longitude;
            double phi = k_Deg2Rad * latitude;
            double eclipLongitude = (218.316 + 13.176396 * julianDate) * k_Deg2Rad;
            double lunarMeanAnomaly = (134.963 + 13.064993 * julianDate) * k_Deg2Rad;
            double lunarMeanDistance = (93.272 + 13.229350 * julianDate) * k_Deg2Rad;

            double lng = eclipLongitude + k_Deg2Rad * 6.289 * Math.Sin(lunarMeanAnomaly);
            double lat = k_Deg2Rad * 5.128 * Math.Sin(lunarMeanDistance);
            double distance = 385000 - 20905 * Math.Cos(lunarMeanAnomaly);

            double obliquity = k_Deg2Rad * 23.4397;
            double rightAscension = Math.Atan2(Math.Sin(lng) * Math.Cos(obliquity) - Math.Tan(lat) * Math.Sin(obliquity), Math.Cos(lng));

            // Add a virtual tilt to the moon's declination
            double virtualTilt = k_Deg2Rad * 25; // Adjust this value to change the moon's rising and setting time
            double dec = Math.Asin(Math.Sin(lng) * Math.Cos(obliquity + virtualTilt) + Math.Cos(lat) * Math.Sin(obliquity + virtualTilt) * Math.Sin(longitude));

            double h = (k_Deg2Rad * (280.16 + 360.9856235 * julianDate) - lw) - rightAscension;
            double altitude = Math.Asin(Math.Sin(phi) * Math.Sin(dec) + Math.Cos(phi) * Math.Cos(dec) * Math.Cos(h));
            double azimuth = Math.Atan2(Math.Sin(h), Math.Cos(h) * Math.Sin(phi) - Math.Tan(dec) * Math.Cos(phi));

            altitude = altitude * k_Rad2Deg;
            azimuth = azimuth * k_Rad2Deg;

            return new Vector3((float)altitude, (float)azimuth, 0);
        }

        double NormalizeAngle(double inputAngle)
        {
            double twoPi = 2 * Mathf.PI;

            if (inputAngle < 0)
            {
                return twoPi - (Math.Abs(inputAngle) % twoPi);
            }
            else if (inputAngle > twoPi)
            {
                return inputAngle % twoPi;
            }
            else
            {
                return inputAngle;
            }
        }
    }
}