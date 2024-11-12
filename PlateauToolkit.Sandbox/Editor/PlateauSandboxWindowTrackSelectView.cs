using PlateauToolkit.Editor;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Splines;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    class PlateauSandboxWindowTrackSelectView : IPlateauSandboxWindowView
    {
        IPlateauSandboxWindowView m_SelectedView;
        TreeViewState m_TreeViewState;
        PlateauSandboxHierarchyContext m_HierarchyContext;

        public string Name => "トラック";

        public void OnBegin(PlateauSandboxContext context, EditorWindow editorWindow)
        {
            m_SelectedView = new PlateauSandboxWindowTrafficView();
            m_SelectedView.OnBegin(context, editorWindow);
        }

        public void OnGUI(PlateauSandboxContext context, EditorWindow window)
        {
            EditorGUILayout.Space(15);

            using (new EditorGUILayout.HorizontalScope())
            {
                float width = ((window.position.width - 10) / 2) - 5;
                if (new PlateauToolkitImageButtonGUI(
                            width,
                            40,
                            m_SelectedView is PlateauSandboxWindowTrafficView ? PlateauToolkitGUIStyles.k_ButtonPrimaryColor : PlateauToolkitGUIStyles.k_ButtonNormalColor
                            ).TabButton("交通シミュレータ配置"))
                {
                    m_SelectedView = new PlateauSandboxWindowTrafficView();
                    m_SelectedView.OnBegin(context, window);
                }

                if (new PlateauToolkitImageButtonGUI(
                            width,
                            40,
                            m_SelectedView is PlateauSandboxWindowTrackView ? PlateauToolkitGUIStyles.k_ButtonPrimaryColor : PlateauToolkitGUIStyles.k_ButtonNormalColor
                            ).TabButton("手動トラック配置"))
                {
                    m_SelectedView = new PlateauSandboxWindowTrackView();
                    m_SelectedView.OnBegin(context, window);
                }
            }

            PlateauToolkitEditorGUILayout.BorderLine(PlateauToolkitGUIStyles.k_ButtonPrimaryColor, 3f);
            EditorGUILayout.Space(15);

            m_SelectedView.OnGUI(context, window);
        }

    }
}