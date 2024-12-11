using PlateauToolkit.Editor;
using PlateauToolkit.Sandbox.Runtime;
using PlateauToolkit.Sandbox.Runtime.ElectricPost;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    [CustomEditor(typeof(PlateauSandboxElectricPost))]
    public class PlateauSandboxElectricPostInspector : UnityEditor.Editor
    {
        private PlateauSandboxElectricPostContext m_Context;
        private PlateauSandboxElectricPost m_Target;

        private PlateauSandboxElectricPostConnectionGUI m_FrontConnectionGUI;
        private PlateauSandboxElectricPostConnectionGUI m_BackConnectionGUI;

        private void OnEnable()
        {
            m_Target = target as PlateauSandboxElectricPost;
            m_Context = PlateauSandboxElectricPostContext.GetCurrent();
            m_Context.OnSelected.AddListener(ResetSelect);

            SetGUI();
        }

        private void SetGUI()
        {
            if (m_FrontConnectionGUI == null)
            {
                m_FrontConnectionGUI = new PlateauSandboxElectricPostConnectionGUI(m_Target, true);
                m_FrontConnectionGUI.OnClickSelect.AddListener(SelectingPost);
            }
            if (m_BackConnectionGUI == null)
            {
                m_BackConnectionGUI = new PlateauSandboxElectricPostConnectionGUI(m_Target, false);
                m_BackConnectionGUI.OnClickSelect.AddListener(SelectingPost);
            }
        }

        public override void OnInspectorGUI()
        {
            if (m_Context == null || m_Target == null)
            {
                return;
            }

            serializedObject.Update();
            base.OnInspectorGUI();

            m_FrontConnectionGUI.DrawLayout(m_Target.FrontConnectedPosts);
            m_BackConnectionGUI.DrawLayout(m_Target.BackConnectedPosts);

            // キーイベント
            if (Event.current.keyCode == KeyCode.Escape)
            {
                ResetSelect();
            }
        }

        private void SelectingPost(bool isSelecting)
        {
            if (isSelecting)
            {
                // 選択中状態に
                SetActiveTool();
            }
            else
            {
                ResetSelect();
            }
        }

        private void ResetSelect()
        {
            GUIUtility.hotControl = 0;
            Event.current.Use();

            ToolManager.RestorePreviousPersistentTool();
            m_FrontConnectionGUI.Reset();
            m_BackConnectionGUI.Reset();

            m_Context.OnCancel.Invoke();
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