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

        private PlateauSandboxElectricPost m_Target;

        private bool m_IsFrontNodeSelecting;
        private bool m_IsBackNodeSelecting;

        private void OnEnable()
        {
            m_Target = target as PlateauSandboxElectricPost;

            m_Context = PlateauSandboxElectricPostContext.GetCurrent();
            m_Context.OnSelected.AddListener(ResetSelect);
        }

        public override void OnInspectorGUI()
        {
            if (m_Context == null || m_Target == null)
            {
                return;
            }

            serializedObject.Update();
            base.OnInspectorGUI();

            GUILayout.Space(5);

            var frontTarget = EditorGUILayout.ObjectField(
                 k_FrontTargetNodeName,
                 m_Target.FrontConnectedPost.target,
                 typeof(PlateauSandboxElectricPost), true) as PlateauSandboxElectricPost;

            if (frontTarget != null && m_Target.FrontConnectedPost.target == null)
            {
                m_Target.SetFrontConnectPointToFacing(frontTarget);
            }

            GUI.enabled = false;
            EditorGUILayout.Toggle("Is Connected Front", m_Target.FrontConnectedPost.isFront);
            GUI.enabled = true;

            GUILayout.Space(5);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (new PlateauToolkitImageButtonGUI(
                        100,
                        20,
                        m_IsFrontNodeSelecting ? PlateauToolkitGUIStyles.k_ButtonCancelColor : PlateauToolkitGUIStyles.k_ButtonNormalColor,
                        false)
                    .Button("選択する"))
                {
                    ResetSelect();
                    if (!m_IsFrontNodeSelecting)
                    {
                        if (m_Target.FrontConnectedPost.target != null)
                        {
                            // 選択中を外す
                            m_Target.FrontConnectedPost.target.RemoveConnectedPost(m_Target);
                            m_Target.RemoveConnectedPost(m_Target.FrontConnectedPost.target);
                        }

                        m_IsFrontNodeSelecting = !m_IsFrontNodeSelecting;
                        m_Context.SetSelectingPost(m_Target, true);
                        SetActiveTool();
                    }
                }
            }

            GUILayout.Space(10);

            var backTarget = EditorGUILayout.ObjectField(
                k_BackTargetNodeName,
                m_Target.BackConnectedPost.target,
                  typeof(PlateauSandboxElectricPost), true) as PlateauSandboxElectricPost;

            if (backTarget != null && m_Target.BackConnectedPost.target == null)
            {
                m_Target.SetBackConnectPointToFacing(backTarget);
            }

            GUI.enabled = false;
            EditorGUILayout.Toggle("Is Connected Front", m_Target.BackConnectedPost.isFront);
            GUI.enabled = true;


            GUILayout.Space(5);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (new PlateauToolkitImageButtonGUI(
                        100,
                        20,
                        m_IsBackNodeSelecting ? PlateauToolkitGUIStyles.k_ButtonCancelColor : PlateauToolkitGUIStyles.k_ButtonNormalColor,
                        false)
                    .Button("選択する"))
                {
                    ResetSelect();
                    if (!m_IsBackNodeSelecting)
                    {
                        if (m_Target.BackConnectedPost.target != null)
                        {
                            // 選択中を外す
                            m_Target.BackConnectedPost.target.RemoveConnectedPost(m_Target);
                            m_Target.RemoveConnectedPost(m_Target.BackConnectedPost.target);
                        }

                        SetActiveTool();
                        m_IsBackNodeSelecting = !m_IsBackNodeSelecting;
                        m_Context.SetSelectingPost(m_Target, false);
                    }
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

            m_Context.SetSelectingPost(null, false);

            m_IsBackNodeSelecting = false;
            m_IsFrontNodeSelecting = false;

            m_Context.OnCancel.Invoke();
        }

        private void SelectedPost(PlateauSandboxElectricPost targetPost)
        {
            ResetSelect();
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