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
                if (PlateauSandboxObjectFinder
                    .TryGetSandboxObject(raycastHit.collider, out var selectedObject))
                {
                    if (selectedObject.gameObject.TryGetComponent(out PlateauSandboxElectricPost electricPost))
                    {
                        if (m_Context.IsFrontNodeSelecting)
                        {
                            m_Context.SetConnect(true, electricPost);
                        }

                        if (m_Context.IsBackNodeSelecting)
                        {
                            m_Context.SetConnect(false, electricPost);
                        }

                        electricPost.SetHighLight(true);
                        m_SelectedObject = electricPost;
                        return;
                    }
                }
            }

            if (m_SelectedObject)
            {
                if (m_Context.IsFrontNodeSelecting)
                {
                    m_Context.SetConnect(true, null);
                }

                if (m_Context.IsBackNodeSelecting)
                {
                    m_Context.SetConnect(false, null);
                }
                m_SelectedObject.SetHighLight(false);
                m_SelectedObject = null;
            }
        }

        private void MouseDown(EditorWindow window)
        {
            if (m_SelectedObject != null)
            {
                m_Context.OnSelected.Invoke();
                m_SelectedObject.SetHighLight(false);
                m_SelectedObject = null;
            }
        }
    }
}