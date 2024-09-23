using PlateauToolkit.Editor;
using PlateauToolkit.Sandbox.Runtime;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    [CustomEditor(typeof(PlateauSandboxElectricPost))]
    public class PlateauSandboxElectricPostInspector : UnityEditor.Editor
    {
        private PlateauSandboxElectricPost m_Target;
        private PlateauSandboxElectricPostContext m_Context;

        private bool m_IsNode01Selected;
        private bool m_IsNode02Selected;

        private void OnEnable()
        {
            m_Context = PlateauSandboxElectricPostContext.GetCurrent();
            m_Target = target as PlateauSandboxElectricPost;

            m_Context.SetTarget(m_Target);
            m_Context.OnSelected.AddListener(() =>
            {
                ResetSelect();
            });
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();

            m_Target.electricNode01 = EditorGUILayout.ObjectField("Electric Node 01", m_Target.electricNode01, typeof(GameObject), true) as GameObject;

            GUILayout.Space(5);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (new PlateauToolkitImageButtonGUI(
                        100,
                        20,
                        m_IsNode01Selected ? PlateauToolkitGUIStyles.k_ButtonCancelColor : PlateauToolkitGUIStyles.k_ButtonNormalColor,
                        false)
                    .Button("選択する"))
                {
                    m_IsNode01Selected = !m_IsNode01Selected;
                    m_Target.SetSelectingElectricNode01(m_IsNode01Selected);
                    SetActiveTool();
                }
            }

            GUILayout.Space(10);

            m_Target.electricNode02 = EditorGUILayout.ObjectField("Electric Node 02", m_Target.electricNode02, typeof(GameObject), true) as GameObject;

            GUILayout.Space(5);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (new PlateauToolkitImageButtonGUI(
                        100,
                        20,
                        m_IsNode02Selected ? PlateauToolkitGUIStyles.k_ButtonCancelColor : PlateauToolkitGUIStyles.k_ButtonNormalColor,
                        false)
                    .Button("選択する"))
                {
                    m_IsNode02Selected = !m_IsNode02Selected;
                    m_Target.SetSelectingElectricNode02(m_IsNode02Selected);
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

            m_IsNode01Selected = false;
            m_IsNode02Selected = false;
            m_Target.SetSelectingElectricNode01(m_IsNode01Selected);
            m_Target.SetSelectingElectricNode02(m_IsNode02Selected);
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