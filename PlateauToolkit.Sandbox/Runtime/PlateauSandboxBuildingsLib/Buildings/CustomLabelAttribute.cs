using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings
{
    public class CustomLabelAttribute : PropertyAttribute
    {
        public readonly GUIContent m_Label;
        public CustomLabelAttribute(string label)
        {
            m_Label = new GUIContent(label);
        }
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(CustomLabelAttribute))]
    public class CustomLabelAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (attribute is not CustomLabelAttribute newLabel)
            {
                return;
            }

            label = newLabel.m_Label;
            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }
    }
    #endif
}
