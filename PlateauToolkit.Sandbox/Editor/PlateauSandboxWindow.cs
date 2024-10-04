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

        void OnBegin(PlateauSandboxContext context, EditorWindow editorWindow)
        {
        }

        void OnUpdate(EditorWindow editorWindow)
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
        const int k_TabButtonSizeWidth = 60;
        const int k_TabButtonSizeHeight = 48;
        const int k_TabButtonWidthPadding = 15;

        PlateauSandboxWindow m_Window;
        IPlateauSandboxWindowView m_CurrentView;

        //PlateauSandboxWindowTrackView m_TrackView = new();
        PlateauSandboxWindowTrackSelectView m_TrackView = new();

        PlateauSandboxWindowAssetPlaceView m_AssetPlaceView = new();
        PlateauSandboxWindowBulkPlaceView m_BulkPlaceView = new();

        void OnEnable()
        {
            ToolManager.activeToolChanged += OnToolChanged;
            EditorApplication.playModeStateChanged += CheckTools;
        }

        void OnDisable()
        {
            ToolManager.activeToolChanged -= OnToolChanged;
            EditorApplication.playModeStateChanged -= CheckTools;
        }

        void OnToolChanged()
        {
            if (m_Window == null)
            {
                return;
            }

            m_Window.Repaint();
        }

        void Update()
        {
            m_CurrentView?.OnUpdate(m_Window);
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
                m_CurrentView.OnBegin(PlateauSandboxContext.GetCurrent(), m_Window);
            }

            PlateauToolkitEditorGUILayout.HeaderLogo(m_Window.position.width);

            using (EditorGUILayout.HorizontalScope scope = PlateauToolkitEditorGUILayout.TabScope(m_Window.position.width))
            {
                PlateauSandboxGUI.DrawColorTexture(scope.rect, PlateauToolkitGUIStyles.k_TabBackgroundColor, 8);

                TabButton(PlateauSandboxPaths.TracksIcon, m_TrackView, scope.rect, PlateauSandboxTab.Tracks);
                TabButton(PlateauSandboxPaths.PlaceIcon, m_AssetPlaceView, scope.rect, PlateauSandboxTab.AssetPlace);
                TabButton(PlateauSandboxPaths.BulkPlaceIcon, m_BulkPlaceView, scope.rect, PlateauSandboxTab.BulkPlace);
            }

            PlateauToolkitEditorGUILayout.Title(m_Window.position.width, m_CurrentView.Name);

            m_CurrentView.OnGUI(PlateauSandboxContext.GetCurrent(), m_Window);

            PlateauToolkitEditorGUILayout.Header("汎用機能");

            if (new PlateauToolkitImageButtonGUI(
                    220,
                    40,
                    PlateauToolkitGUIStyles.k_ButtonNormalColor).Button("カメラマネージャーを作成"))
            {
                GameObject cameraManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Packages/com.synesthesias.plateau-unity-toolkit/PlateauToolkit.Sandbox/Runtime/Prefabs/PlateauSandboxCameraManager.prefab");
                GameObject cameraManager = Instantiate(cameraManagerPrefab);
                cameraManager.name = nameof(PlateauSandboxCameraManager);
                cameraManager.GetComponent<PlateauSandboxCameraManager>().EnableKeyboardCameraSwitch = true;
                cameraManager.transform.SetAsLastSibling();

                Undo.RegisterCreatedObjectUndo(cameraManager, $"Create {nameof(PlateauSandboxCameraManager)}");
            }

            EditorGUILayout.Space(8);
        }

        void TabButton(string iconPath, IPlateauSandboxWindowView tabView, Rect backGroundRect, PlateauSandboxTab tab)
        {
            float centerY = (backGroundRect.height - k_TabButtonSizeHeight) / 2;
            float centerX = (backGroundRect.width - k_TabButtonSizeWidth) / 2;
            var buttonRect = new Rect(backGroundRect.x + centerX, backGroundRect.y + centerY, k_TabButtonSizeWidth, k_TabButtonSizeHeight);

            switch (tab)
            {
                case PlateauSandboxTab.Tracks:
                    buttonRect.x -= k_TabButtonSizeWidth + k_TabButtonWidthPadding;
                    break;
                case PlateauSandboxTab.BulkPlace:
                    buttonRect.x += k_TabButtonSizeWidth + k_TabButtonWidthPadding;
                    break;
            }

            var imageButtonGUILayout = new PlateauToolkitImageButtonGUI(k_TabButtonSizeWidth, k_TabButtonSizeHeight, PlateauToolkitGUIStyles.k_TabActiveColor);
            if (imageButtonGUILayout.TabButton(iconPath, buttonRect, tabView == m_CurrentView))
            {
                if (tabView == m_CurrentView)
                {
                    return;
                }

                m_CurrentView?.OnEnd(PlateauSandboxContext.GetCurrent());
                m_CurrentView = tabView;
                m_CurrentView.OnBegin(PlateauSandboxContext.GetCurrent(), m_Window);
            }
        }

        void CheckTools(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode && ToolManager.activeToolType == typeof(PlateauSandboxPlacementTool))
            {
                ToolManager.RestorePreviousPersistentTool();
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