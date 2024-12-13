using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Editor
{
    // Enumの要素に付けるAttribute
    // PropertyAttributeではなくSystem.Attributeを継承する
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Enum)]
    public class EnumElementAttribute : System.Attribute
    {
        public string DisplayName { get; }

        public EnumElementAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Enum)]
    public class EnumElementUsageAttribute : PropertyAttribute
    {
        public System.Type Type { get; }
        public string DisplayName { get; }

        public EnumElementUsageAttribute(string displayName, System.Type selfType)
        {
            DisplayName = displayName;
            Type = selfType;
        }
    }

    [CustomPropertyDrawer(typeof(EnumElementUsageAttribute))]
    public class EnumElementUsageAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as EnumElementUsageAttribute;
            var names = new List<string>();
            if (attr == null)
            {
                return;
            }

            foreach (FieldInfo fi in attr.Type.GetFields())
            {
                if (fi.IsSpecialName)
                {
                    continue;
                }

                var elementAttribute = fi.GetCustomAttributes(typeof(EnumElementAttribute), false).FirstOrDefault() as EnumElementAttribute;
                names.Add(elementAttribute == null ? fi.Name : elementAttribute.DisplayName);
            }

            IEnumerable<int> values = System.Enum.GetValues(attr.Type).Cast<int>();
            property.intValue = EditorGUI.IntPopup(position, attr.DisplayName, property.intValue, names.ToArray(), values.ToArray());
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }
    }
}