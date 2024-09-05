using PlateauToolkit.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace PlateauToolkit.Sandbox.Editor
{
    /// <summary>
    /// Asset Tab View for Plateau Sandbox Window
    /// </summary>
    class PlateauSandboxAssetComponentTabsView
    {
        const float k_TabButtonSizeWidth = 44;
        const float k_TabButtonSizeHeight = 44;
        const int k_TabButtonPadding = 5;
        const int k_TabButtonImagePadding = 6;

        readonly Dictionary<SandboxAssetComponentType, Texture2D> m_AssetIcons = new Dictionary<SandboxAssetComponentType, Texture2D>();

        public UnityEvent<SandboxAssetComponentType> OnSelectedTypeChanged { get; } = new UnityEvent<SandboxAssetComponentType>();

        public void OnBegin()
        {
            // Load And Cache.
            foreach (SandboxAssetComponentType type in System.Enum.GetValues(typeof(SandboxAssetComponentType)))
            {
                m_AssetIcons[type] = type.LoadIcon();
            }
        }

        public void OnGUI(float windowWidth, SandboxAssetComponentType currentType)
        {
            var actions = new System.Action[m_AssetIcons.Count];
            int index = 0;
            foreach (KeyValuePair<SandboxAssetComponentType, Texture2D> iconPair in m_AssetIcons)
            {
                SandboxAssetComponentType type = iconPair.Key;
                Texture2D icon = iconPair.Value;
                actions[index] = () => DrawButton(type, icon, currentType == type);
                index++;
            }
            PlateauToolkitEditorGUILayout.GridLayout(windowWidth, k_TabButtonSizeWidth + k_TabButtonPadding, k_TabButtonSizeHeight + k_TabButtonPadding, actions);

            GUILayout.Space(10);
        }

        void DrawButton(SandboxAssetComponentType type, Texture2D icon, bool isSelected)
        {
            var buttonOffset = new RectOffset(k_TabButtonImagePadding, k_TabButtonImagePadding, k_TabButtonImagePadding, k_TabButtonImagePadding);
            if (new PlateauToolkitImageButtonGUI(
                    k_TabButtonSizeWidth,
                    k_TabButtonSizeHeight,
                    isSelected ? PlateauToolkitGUIStyles.k_ButtonPrimaryColor : PlateauToolkitGUIStyles.k_ButtonNormalColor
                ).Button(icon, buttonOffset))
            {
                if (isSelected)
                {
                    return;
                }
                OnSelectedTypeChanged.Invoke(type);
            }
        }
    }
}