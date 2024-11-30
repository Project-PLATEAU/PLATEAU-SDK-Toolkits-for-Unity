using PlateauToolkit.Editor;
using PlateauToolkit.Sandbox.Runtime;
using PlateauToolkit.Sandbox.Runtime.ElectricPost;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    [CustomEditor(typeof(PlateauSandboxElectricPost))]
    public class PlateauSandboxElectricPostInspector : UnityEditor.Editor
    {
        private PlateauSandboxElectricPostContext m_Context;

        private const string k_FrontTargetNodeName = "Front Connected Target Node";
        private const string k_BackTargetNodeName = "Back Connected Target Node";

        private void OnEnable()
        {
            m_Context = PlateauSandboxElectricPostContext.GetCurrent();
            m_Context.OnSelected.AddListener((targetPost, isFront) =>
            {
                ResetSelect();
            });
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();

            GUILayout.Space(5);

            var frontTarget = EditorGUILayout.ObjectField(
                k_FrontTargetNodeName,
                m_Context.FrontConnectedPost.target,
                typeof(PlateauSandboxElectricPost), true) as PlateauSandboxElectricPost;

            if (frontTarget != null && m_Context.FrontConnectedPost.target == null)
            {
                m_Context.OnSelected.Invoke(frontTarget, true);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (new PlateauToolkitImageButtonGUI(
                        100,
                        20,
                        m_Context.IsFrontNodeSelecting ? PlateauToolkitGUIStyles.k_ButtonCancelColor : PlateauToolkitGUIStyles.k_ButtonNormalColor,
                        false)
                    .Button("選択する"))
                {
                    m_Context.SetSelecting(true, !m_Context.IsFrontNodeSelecting);
                    SetActiveTool();
                }
            }

            GUILayout.Space(10);

            var backTarget = EditorGUILayout.ObjectField(
                k_BackTargetNodeName,
                  m_Context.BackConnectedPost.target,
                  typeof(PlateauSandboxElectricPost), true) as PlateauSandboxElectricPost;

            if (backTarget != null && m_Context.BackConnectedPost.target == null)
            {
                m_Context.OnSelected.Invoke(backTarget, false);
            }

            GUILayout.Space(5);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (new PlateauToolkitImageButtonGUI(
                        100,
                        20,
                        m_Context.IsBackNodeSelecting ? PlateauToolkitGUIStyles.k_ButtonCancelColor : PlateauToolkitGUIStyles.k_ButtonNormalColor,
                        false)
                    .Button("選択する"))
                {
                    m_Context.SetSelecting(false, !m_Context.IsBackNodeSelecting);
                    SetActiveTool();
                }
            }

            GUILayout.Space(10);

            if (Event.current.keyCode == KeyCode.Escape)
            {
                ResetSelect();
            }
        }

        private void ResetSelect()
        {
            GUIUtility.hotControl = 0;
            Event.current.Use();

            ToolManager.RestorePreviousPersistentTool();

            m_Context.SetSelecting(true, false);
            m_Context.SetSelecting(false, false);
        }

        private void SetActiveTool()
        {
            if (ToolManager.activeToolType != typeof(PlateauSandboxElectricPostSelectTool))
            {
                ToolManager.SetActiveTool<PlateauSandboxElectricPostSelectTool>();
            }
            else
            {
                ToolManager.RestorePreviousPersistentTool();
            }
        }
    }
}