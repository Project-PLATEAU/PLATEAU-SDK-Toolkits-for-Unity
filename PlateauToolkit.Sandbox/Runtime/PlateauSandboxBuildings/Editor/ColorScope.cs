using System;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Editor
{
    public enum KColorScope
    {
        Color,
        ContentColor,
        Background
    }

    public class ColorScope : GUI.Scope
    {
        private readonly Color m_Color;
        private readonly KColorScope m_Scope;
        public ColorScope(KColorScope scope, Color color)
        {
            m_Scope = scope;
            switch (scope)
            {
                case KColorScope.Color:
                    m_Color = GUI.color;
                    GUI.color = color;
                    break;
                case KColorScope.ContentColor:
                    m_Color = GUI.contentColor;
                    GUI.contentColor = color;
                    break;
                case KColorScope.Background:
                    m_Color = GUI.backgroundColor;
                    GUI.backgroundColor = color;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
            }
        }

        protected override void CloseScope()
        {
            switch (m_Scope)
            {
                case KColorScope.Color:
                    GUI.color = m_Color;
                    break;
                case KColorScope.ContentColor:
                    GUI.contentColor = m_Color;
                    break;
                case KColorScope.Background:
                    GUI.backgroundColor = m_Color;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(m_Scope), m_Scope, null);
            }
        }
    }
}