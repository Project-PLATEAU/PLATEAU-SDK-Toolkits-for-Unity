using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Rendering.Editor
{
    [CustomEditor(typeof(EnvironmentController))]
    public class EnvironmentControllerEditor : UnityEditor.Editor
    {
        EnvironmentController m_Env;

        void OnEnable()
        {
            if (target is EnvironmentController)
            {
                m_Env = (EnvironmentController)target;
            }
            else
            {
                m_Env = null;
            }
        }

        public override void OnInspectorGUI()
        {
            if (m_Env == null)
            {
                return;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Location");
            CheckUndo<float>(m_Env, ref m_Env.m_Location.x, EditorGUILayout.FloatField(m_Env.m_Location.x, GUILayout.Width(100)), "");
            CheckUndo<float>(m_Env, ref m_Env.m_Location.y, EditorGUILayout.FloatField(m_Env.m_Location.y, GUILayout.Width(100)), "");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Time of Day (" + m_Env.GetTimeString() + ")");
            CheckUndo<float>(m_Env, ref m_Env.m_TimeOfDay, EditorGUILayout.Slider(m_Env.m_TimeOfDay, 0f, 1f), "Time of Day");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Rain");
            CheckUndo<float>(m_Env, ref m_Env.m_Rain, EditorGUILayout.Slider(m_Env.m_Rain, 0f, 1f), "Rain");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Snow");
            CheckUndo<float>(m_Env, ref m_Env.m_Snow, EditorGUILayout.Slider(m_Env.m_Snow, 0f, 1f), "Snow");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Cloudy");
            CheckUndo<float>(m_Env, ref m_Env.m_Cloud, EditorGUILayout.Slider(m_Env.m_Cloud, 0f, 1f), "Cloudy");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sun Color");
            CheckUndo<Color>(m_Env, ref m_Env.m_SunColor, EditorGUILayout.ColorField(m_Env.m_SunColor), "Sun Color");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Moon Color");
            CheckUndo<Color>(m_Env, ref m_Env.m_MoonColor, EditorGUILayout.ColorField(m_Env.m_MoonColor), "Moon Color");
            EditorGUILayout.EndHorizontal();

#if UNITY_HDRP
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sun Intensity");
            CheckUndo<float>(m_Env, ref m_Env.m_SunIntensity, EditorGUILayout.Slider(m_Env.m_SunIntensity, 0f, 130000f), "Sun Intensity");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Moon Intensity");
            CheckUndo<float>(m_Env, ref m_Env.m_MoonIntensity, EditorGUILayout.Slider(m_Env.m_MoonIntensity, 0f, 100f), "Moon Intensity");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Moon Scale");
            CheckUndo<float>(m_Env, ref m_Env.m_MoonScale, EditorGUILayout.Slider(m_Env.m_MoonScale, 0f, 10f), "Moon Scale");
            EditorGUILayout.EndHorizontal();
#elif UNITY_URP
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sun Intensity");
            CheckUndo<float>(m_Env, ref m_Env.m_SunIntensity, EditorGUILayout.Slider(m_Env.m_SunIntensity, 0f, 10f), "Sun Intensity");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Moon Intensity");
            CheckUndo<float>(m_Env, ref m_Env.m_MoonIntensity, EditorGUILayout.Slider(m_Env.m_MoonIntensity, 0f, 10f), "Moon Intensity");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Moon Scale");
            CheckUndo<float>(m_Env, ref m_Env.m_MoonScale, EditorGUILayout.Slider(m_Env.m_MoonScale, 0f, 0.3f), "Moon Scale");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Moon Phase");
            CheckUndo<float>(m_Env, ref m_Env.m_MoonPhase, EditorGUILayout.Slider(m_Env.m_MoonPhase, 0f, 1f), "Moon Phase");
            EditorGUILayout.EndHorizontal();
#endif
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Fog Color");
            CheckUndo<Color>(m_Env, ref m_Env.m_FogColor, EditorGUILayout.ColorField(m_Env.m_FogColor), "Fog Color");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Fog Distance");
            CheckUndo<float>(m_Env, ref m_Env.m_FogDistance, EditorGUILayout.FloatField(m_Env.m_FogDistance), "Fog Distance");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Opacity");
            CheckUndo<float>(m_Env, ref m_Env.m_GlobalOpacity, EditorGUILayout.Slider(m_Env.m_GlobalOpacity, 0f, 1f), "Opacity");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Fade Distance");
            CheckUndo<float>(m_Env, ref m_Env.m_OpacityDistMax, EditorGUILayout.Slider(m_Env.m_OpacityDistMax, 0f, 200f), "Fade Distance");
            EditorGUILayout.EndHorizontal();

#if UNITY_URP
            EditorGUILayout.Space(20);

            // Material quality setting for urp
            string[] materialQualityNames = System.Enum.GetNames(typeof(EnvironmentController.MaterialQuality));
            GUIContent[] qualityOptions = new GUIContent[materialQualityNames.Length];
            for (int i = 0; i < materialQualityNames.Length; i++)
            {
                qualityOptions[i] = new GUIContent(materialQualityNames[i]);
            }

            int selectedIndex = (int)m_Env.m_Quality;
            selectedIndex = EditorGUILayout.Popup(new GUIContent("Material Quality"), selectedIndex, qualityOptions);

            if (selectedIndex != (int)m_Env.m_Quality)
            {
                Undo.RecordObject(m_Env, "Material Quality");
                m_Env.SetMaterialQuality((EnvironmentController.MaterialQuality)selectedIndex);
                EditorUtility.SetDirty(m_Env);
            }
#endif

            EditorGUILayout.Space(20);

            // Hide material slider
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Material Fade");
            CheckUndo<float>(m_Env, ref m_Env.m_MaterialFade, EditorGUILayout.Slider(m_Env.m_MaterialFade, 0f, 1f), "Hide Material");
            EditorGUILayout.EndHorizontal();
            if (m_Env.m_MaterialFade > 0f)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Building Color");
                CheckUndo<Color>(m_Env, ref m_Env.m_BuildingColor, EditorGUILayout.ColorField(m_Env.m_BuildingColor), "Building Color");
                EditorGUILayout.EndHorizontal();
            }
        }

        public bool CheckUndo<T>(UnityEngine.Object recordTarget, ref T origin, T value, string log) where T : System.IEquatable<T>
        {
            if (!origin.Equals(value))
            {
                Undo.RecordObject(recordTarget, log);
                origin = value;
                PrefabUtility.RecordPrefabInstancePropertyModifications(recordTarget);
                return true;
            }

            return false;
        }
    }
}