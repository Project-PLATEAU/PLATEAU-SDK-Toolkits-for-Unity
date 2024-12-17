using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Editor
{
    public class LabelAttribute : PropertyAttribute
    {
        public readonly GUIContent m_Label;
        public LabelAttribute(string label)
        {
            m_Label = new GUIContent(label);
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LabelAttribute))]
    public class CustomLabelAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (attribute is not LabelAttribute newLabel)
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
