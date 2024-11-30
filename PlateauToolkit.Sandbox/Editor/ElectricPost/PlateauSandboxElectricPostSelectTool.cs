using PlateauToolkit.Sandbox.Runtime.ElectricPost;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    [EditorTool("PLATEAU 電柱選択ツール", null, typeof(GameObjectToolContext))]
    public class PlateauSandboxElectricPostSelectTool : EditorTool
    {
        private PlateauSandboxElectricPostContext m_Context;
        private PlateauSandboxElectricPost m_SelectedObject;

        public override void OnActivated()
        {
            m_Context = PlateauSandboxElectricPostContext.GetCurrent();
            base.OnActivated();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            base.OnToolGUI(window);

            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlId);

            if (SceneView.currentDrawingSceneView != null &&
                (GUIUtility.hotControl == 0 || GUIUtility.hotControl == controlId))
            {
                switch (Event.current.GetTypeForControl(controlId))
                {
                    case EventType.MouseLeaveWindow:
                        break;
                    case EventType.MouseMove:
                        MouseMove(window);
                        break;
                    case EventType.Repaint:
                        break;
                    case EventType.MouseDown:
                        MouseDown(window);
                        break;
                    case EventType.MouseDrag:
                        break;
                    case EventType.MouseUp:
                        break;
                }
            }

            if (Event.current.keyCode == KeyCode.Escape)
            {
                GUIUtility.hotControl = 0;
                Event.current.Use();

                if (m_Context.IsFrontNodeSelecting)
                {
                    m_Context.OnCancel.Invoke(true);
                    m_Context.SetSelecting(true, false);
                }

                if (m_Context.IsBackNodeSelecting)
                {
                    m_Context.OnCancel.Invoke(false);
                    m_Context.SetSelecting(false, false);
                }

                ToolManager.RestorePreviousPersistentTool();
            }
        }

        private void MouseMove(EditorWindow window)
        {
            var mousePosition = Event.current.mousePosition;
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit))
            {
                if (m_Context.IsFrontNodeSelecting)
                {
                    m_Context.OnMoseMove.Invoke(raycastHit.point, true);
                }

                if (m_Context.IsBackNodeSelecting)
                {
                    m_Context.OnMoseMove.Invoke(raycastHit.point, false);
                }

                if (PlateauSandboxObjectFinder
                    .TryGetSandboxObject(raycastHit.collider, out var selectedObject))
                {
                    if (selectedObject.gameObject.TryGetComponent(out PlateauSandboxElectricPost electricPost))
                    {
                        m_SelectedObject = electricPost;
                        m_SelectedObject.SetSelecting(true);
                        return;
                    }
                }
            }

            if (m_SelectedObject)
            {
                m_SelectedObject.SetSelecting(false);
                m_SelectedObject = null;
            }
        }

        private void MouseDown(EditorWindow window)
        {
            if (m_SelectedObject != null)
            {
                if (m_Context.IsFrontNodeSelecting)
                {
                    m_Context.OnSelected.Invoke(m_SelectedObject, true);
                }

                if (m_Context.IsBackNodeSelecting)
                {
                    m_Context.OnSelected.Invoke(m_SelectedObject, false);
                }
            }
        }
    }
}