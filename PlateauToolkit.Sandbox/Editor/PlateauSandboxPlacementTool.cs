using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    [EditorTool("PLATEAU オブジェクト配置ツール", null, typeof(GameObjectToolContext))]
    sealed partial class PlateauSandboxPlacementTool : EditorTool
    {
        class PlacePoint
        {
            public Vector3 Position { get; }
            public Vector3 Normal { get; }
            public PlateauSandboxTrack Track { get; }
            public bool HitOnSandboxObject { get; }

            public PlacePoint(Vector3 position, Vector3 normal, PlateauSandboxTrack track, bool hitOnSandboxObject)
            {
                Position = position;
                Normal = normal;
                Track = track;
                HitOnSandboxObject = hitOnSandboxObject;
            }
        }

        interface IPlacement : IDisposable
        {
            public bool IsPlaceable { get; }

            void Repaint(EditorWindow window);

            void MouseMove(EditorWindow window);

            void MouseDown(EditorWindow window);

            void MouseUp(EditorWindow window);

            void MouseDrag(EditorWindow window);
        }

        [SerializeField] Texture2D m_ToolIcon;

        const float k_SelectDirectionThreshold = 10f;
        static readonly Vector3 k_DefaultDirectionVector = new(0, 0, 1);

        PlateauSandboxContext m_Context;
        IPlacement m_Placement;

        public override GUIContent toolbarIcon => EditorGUIUtility.TrIconContent(m_ToolIcon, "PLATEAU オブジェクト配置ツール");

        public override void OnActivated()
        {
            base.OnActivated();

            m_Context = PlateauSandboxContext.GetCurrent();

            ToolManager.SetActiveContext<GameObjectToolContext>();

            OnPlacementSettingsChanged();
            m_Context.PlacementSettings.OnModeChanged += OnPlacementSettingsChanged;
        }

        public override void OnWillBeDeactivated()
        {
            base.OnWillBeDeactivated();
            m_Context.PlacementSettings.OnModeChanged -= OnPlacementSettingsChanged;

            m_Placement?.Dispose();
            m_Placement = null;

            EditorWindow.GetWindow<SceneView>().RemoveNotification();
        }

        void OnPlacementSettingsChanged()
        {
            switch (m_Context.PlacementSettings.Mode)
            {
                case PlacementMode.Click:
                    m_Placement = new ClickPlacement(m_Context);
                    break;
                case PlacementMode.Brush:
                    m_Placement = new BrushPlacement(m_Context);
                    break;
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (ToolManager.activeContextType != typeof(GameObjectToolContext))
            {
                ToolManager.RestorePreviousPersistentTool();
            }

            base.OnToolGUI(window);

            if (m_Placement == null)
            {
                return;
            }

            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlId);

            if (SceneView.currentDrawingSceneView != null &&
                (GUIUtility.hotControl == 0 || GUIUtility.hotControl == controlId))
            {
                switch (Event.current.GetTypeForControl(controlId))
                {
                    case EventType.MouseLeaveWindow:
                        m_Placement.Dispose();
                        break;
                    case EventType.MouseMove:
                        m_Placement.MouseMove(window);
                        break;
                    case EventType.Repaint:
                        m_Placement.Repaint(window);
                        break;
                    case EventType.MouseDown:
                        m_Placement.MouseDown(window);
                        break;
                    case EventType.MouseDrag:
                        m_Placement.MouseDrag(window);
                        break;
                    case EventType.MouseUp:
                        m_Placement.MouseUp(window);
                        break;
                }
            }

            if (window.hasFocus && window == EditorWindow.mouseOverWindow)
            {
                string notPlaceableMessage = null;

                if (!m_Placement.IsPlaceable)
                {
                    switch (m_Context.PlacementSettings.Location)
                    {
                        case PlacementLocation.PlaceOnSurface:
                            notPlaceableMessage = "オブジェクトを配置できるコライダーが見つかりません。";
                            break;
                        case PlacementLocation.PlaceAlongTrack:
                            notPlaceableMessage = "オブジェクトを配置できるトラックが見つかりません。";
                            break;
                    }
                }

                if (notPlaceableMessage != null)
                {
                    window.ShowNotification(new(notPlaceableMessage), 0);
                }
                else
                {
                    window.RemoveNotification();
                }
            }

            if (Event.current.keyCode == KeyCode.Escape)
            {
                GUIUtility.hotControl = 0;
                Event.current.Use();

                ToolManager.RestorePreviousPersistentTool();
            }
        }

        static void DrawHandle(
            in Vector3 position, in Vector3 forward, in Vector3 up, float scale, in Color color)
        {
            Vector3 trsPosition = position;
            Quaternion trsRotation = Quaternion.FromToRotation(Vector3.up, up);
            Vector3 trsScale = Vector3.one * scale;

            using (new Handles.DrawingScope(color, Matrix4x4.TRS(trsPosition, trsRotation, trsScale)))
            {
                Handles.DrawWireDisc(Vector3.zero, Vector3.up, 0.3f);
                Handles.DrawLine(Vector3.zero, Vector3.up * 1f);

                DrawArrowHandle(forward);
            }
        }

        static void DrawArrowHandle(in Vector3 forward)
        {
            // The origin of the arrow
            Vector3 arrowOrigin = Vector3.zero;

            // The direction and length of the arrow
            Vector3 arrowEnd = forward;

            // Draw the main line of the arrow
            Handles.DrawLine(arrowOrigin, arrowEnd);

            // Calculate the length of the arrow
            float arrowLength = arrowEnd.magnitude;

            // The length of the arrowhead
            float arrowheadLength = arrowLength * 0.3f; // Adjust this value as needed

            // The direction of the arrow
            Vector3 arrowDirection = arrowEnd.normalized;

            // The points that make up the arrowhead
            Vector3 arrowheadPointA = arrowEnd - arrowDirection * arrowheadLength;

            // Arrowhead lines will have a 30 degree angle with the arrow body line
            Vector3 arrowheadPointB = Quaternion.Euler(0, 45, 0) * (arrowheadPointA - arrowEnd) + arrowEnd;
            Vector3 arrowheadPointC = Quaternion.Euler(0, -45, 0) * (arrowheadPointA - arrowEnd) + arrowEnd;

            // Draw the arrowhead
            Handles.DrawLine(arrowEnd, arrowheadPointB);
            Handles.DrawLine(arrowEnd, arrowheadPointC);
        }
    }
}