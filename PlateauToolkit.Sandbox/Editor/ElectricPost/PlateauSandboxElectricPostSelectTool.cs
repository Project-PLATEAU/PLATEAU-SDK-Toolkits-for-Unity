using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    [EditorTool("PLATEAU 電柱選択ツール", null, typeof(GameObjectToolContext))]
    public class PlateauSandboxElectricPostSelectTool : EditorTool
    {
        private PlateauSandboxElectricPostContext m_Context;

        private IPlateauSandboxPlaceableObject m_SelectedObject;

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

                ToolManager.RestorePreviousPersistentTool();
            }
        }

        private void MouseMove(EditorWindow window)
        {
            var mousePosition = Event.current.mousePosition;
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit))
            {
                if (m_Context.Target != null)
                {
                    m_Context.Target.SetElectricNodePosition(raycastHit.point);
                }

                PlateauSandboxObjectFinder
                    .TryGetSandboxObject(raycastHit.collider, out m_SelectedObject);
            }
        }

        private void MouseDown(EditorWindow window)
        {
            if (m_SelectedObject != null)
            {
                m_SelectedObject.gameObject.TryGetComponent(out PlateauSandboxElectricPost electricPost);
                if (electricPost != null)
                {
                    if (m_Context.Target != electricPost)
                    {
                        m_Context.Target.SetElectricNode(electricPost.gameObject);
                        m_Context.OnSelected.Invoke();
                    }
                }
            }
        }
    }
}