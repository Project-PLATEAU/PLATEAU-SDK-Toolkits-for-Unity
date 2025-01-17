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
        private PlateauSandboxElectricPostConnectCollider m_SelectedObject;

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
                    case EventType.MouseMove:
                        MouseMove(window);
                        break;
                    case EventType.MouseDown:
                        MouseDown(window);
                        break;
                    case EventType.MouseLeaveWindow:
                    case EventType.Repaint:
                    case EventType.MouseDrag:
                    case EventType.MouseUp:
                        break;
                }
            }
        }

        private void MouseMove(EditorWindow window)
        {
            var mousePosition = Event.current.mousePosition;
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit))
            {
                if (raycastHit.collider.gameObject.TryGetComponent<PlateauSandboxElectricPostConnectCollider>(out var connectCollider))
                {
                    connectCollider.OnMouseHover(m_Context.SelectingPost);
                    m_SelectedObject = connectCollider;
                    return;
                }
            }

            if (m_SelectedObject)
            {
                m_SelectedObject.OnMoveLeave(m_Context.SelectingPost);
                m_SelectedObject = null;
            }
        }

        private void MouseDown(EditorWindow window)
        {
            if (m_SelectedObject != null)
            {
                m_SelectedObject.OnSelect(m_Context.SelectingPost);
                m_Context.OnSelected.Invoke();
                m_SelectedObject = null;
            }
        }
    }
}