using PlateauToolkit.Editor;
using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using Object = System.Object;

namespace PlateauToolkit.Sandbox.Editor
{
    interface IPlateauSandboxWindowView
    {
        string Name { get; }

        void OnBegin(PlateauSandboxContext context)
        {
        }

        void OnEnd(PlateauSandboxContext context)
        {
        }

        void OnGUI(PlateauSandboxContext context, EditorWindow window);

        void OnHierarchyChange(PlateauSandboxContext context)
        {
        }
    }

    class PlateauSandboxWindow : EditorWindow
    {
        const int k_TabButtonSize = 54;

        PlateauSandboxWindow m_Window;
        IPlateauSandboxWindowView m_CurrentView;

        PlateauSandboxWindowTrackView m_TrackView = new();
        PlateauSandboxWindowAvatarView m_AvatarView = new();
        PlateauSandboxWindowVehicleView m_VehicleView = new();
        PlateauSandboxWindowPropsView m_PropsView = new();

        void OnEnable()
        {
            ToolManager.activeToolChanged += OnToolChanged;
        }

        void OnDisable()
        {
            ToolManager.activeToolChanged -= OnToolChanged;
        }

        void OnToolChanged()
        {
            if (m_Window == null)
            {
                return;
            }

            m_Window.Repaint();
        }

        void OnGUI()
        {
            if (m_Window == null)
            {
                m_Window = GetWindow<PlateauSandboxWindow>();
            }
            if (m_CurrentView == null)
            {
                m_CurrentView = m_TrackView;
                m_CurrentView.OnBegin(PlateauSandboxContext.GetCurrent());
            }

            PlateauToolkitEditorGUILayout.HeaderLogo(m_Window.position.width);

            PlateauToolkitEditorGUILayout.GridLayout(
                m_Window.position.width,
                k_TabButtonSize,
                k_TabButtonSize,
                new Action[]
                {
                    () => TabButton(PlateauSandboxPaths.TracksIcon, m_TrackView),
                    () => TabButton(PlateauSandboxPaths.HumanIcon, m_AvatarView),
                    () => TabButton(PlateauSandboxPaths.VehicleIcon, m_VehicleView),
                    () => TabButton(PlateauSandboxPaths.PropsIcon, m_PropsView),
                });

            PlateauToolkitEditorGUILayout.Header(m_CurrentView.Name);

            m_CurrentView.OnGUI(PlateauSandboxContext.GetCurrent(), m_Window);

            PlateauToolkitEditorGUILayout.Header("汎用機能");

            if (GUILayout.Button("カメラマネージャーを作成"))
            {
                GameObject cameraManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Packages/com.unity.plateautoolkit/PlateauToolkit.Sandbox/Runtime/Prefabs/PlateauSandboxCameraManager.prefab");
                GameObject cameraManager = Instantiate(cameraManagerPrefab);
                cameraManager.name = nameof(PlateauSandboxCameraManager);
                cameraManager.GetComponent<PlateauSandboxCameraManager>().EnableKeyboardCameraSwitch = true;
                cameraManager.transform.SetAsLastSibling();

                Undo.RegisterCreatedObjectUndo(cameraManager, $"Create {nameof(PlateauSandboxCameraManager)}");
            }

            EditorGUILayout.Space(8);
        }

        void TabButton(string iconPath, IPlateauSandboxWindowView tabView)
        {
            Color? buttonColor = tabView == m_CurrentView ? Color.cyan : null;
            var imageButtonGUILayout = new PlateauToolkitImageButtonGUI(k_TabButtonSize, k_TabButtonSize);
            if (imageButtonGUILayout.Button(iconPath, buttonColor))
            {
                m_CurrentView = tabView;
                m_CurrentView.OnBegin(PlateauSandboxContext.GetCurrent());
            }
        }

        void OnHierarchyChange()
        {
            if (m_CurrentView != null)
            {
                m_CurrentView.OnHierarchyChange(PlateauSandboxContext.GetCurrent());
            }
        }
    }
}