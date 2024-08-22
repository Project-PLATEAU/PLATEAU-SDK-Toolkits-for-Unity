using PlateauToolkit.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    class PlateauSandboxAssetContainerView
    {
        readonly Dictionary<SandboxAssetComponentType, IPlateauSandboxAssetListView> m_AssetLists =
            new Dictionary<SandboxAssetComponentType, IPlateauSandboxAssetListView>();

        SandboxAssetComponentType m_SelectedComponentType = SandboxAssetComponentType.k_Avatar;
        readonly PlateauSandboxAssetComponentTabsView m_ComponentTabView = new PlateauSandboxAssetComponentTabsView();
        IPlateauSandboxAssetListView m_CurrentAssetList;
        bool m_IsShowAssetCreate;

        public PlateauSandboxAssetContainerView(bool isShowAssetCreate = true)
        {
            m_IsShowAssetCreate = isShowAssetCreate;
        }

        public void OnBegin(PlateauSandboxContext context, EditorWindow editorWindow)
        {
            // Load And Cache.
            foreach (SandboxAssetComponentType type in System.Enum.GetValues(typeof(SandboxAssetComponentType)))
            {
                m_AssetLists[type] = type.GetAssetList();
            }

            void SetComponentType(SandboxAssetComponentType type)
            {
                m_CurrentAssetList?.OnEnd();

                m_SelectedComponentType = type;
                m_CurrentAssetList = m_AssetLists[m_SelectedComponentType];
                m_CurrentAssetList.OnBegin();
            }

            // Set Tab
            m_ComponentTabView.OnBegin();
            m_ComponentTabView.OnSelectedTypeChanged.AddListener(SetComponentType);

            // Set Default
            SetComponentType(SandboxAssetComponentType.k_Avatar);
        }

        public void OnGUI(PlateauSandboxContext context, EditorWindow window)
        {
            PlateauToolkitEditorGUILayout.Header("アセット");

            // Draw Asset Component Tab.
            m_ComponentTabView.OnGUI(window.position.width, m_SelectedComponentType);

            // Draw Asset Type Tab.
            DrawAssetTypeTab(window.position.width);

            // Draw Asset List.
            m_CurrentAssetList.OnGUI(context, window.position.width, m_IsShowAssetCreate);
        }

        void DrawAssetTypeTab(float windowWidth)
        {
            using (var scope = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(15);
                float width = ((windowWidth - 30) / 3) - 5;
                void DrawButton(SandboxAssetType type)
                {
                    bool isSelected = m_CurrentAssetList.SelectedAssetType == type;
                    if (new PlateauToolkitImageButtonGUI(
                            width,
                            40,
                            isSelected ? PlateauToolkitGUIStyles.k_ButtonPrimaryColor : PlateauToolkitGUIStyles.k_ButtonNormalColor)
                        .Button(type.ToDisplayName()))
                    {
                        m_CurrentAssetList.SelectedAssetType = type;
                    }
                }

                DrawButton(SandboxAssetType.All);
                DrawButton(SandboxAssetType.UserDefined);
                DrawButton(SandboxAssetType.Builtin);

                GUILayout.Space(15);
            }
            GUILayout.Space(5);
        }

        public virtual void OnUpdate(EditorWindow editorWindow)
        {
            m_CurrentAssetList?.OnUpdate(editorWindow);
        }

        public virtual void OnEnd(PlateauSandboxContext context)
        {
            m_CurrentAssetList?.OnEnd();
            m_ComponentTabView.OnSelectedTypeChanged.RemoveAllListeners();
        }

        public GameObject GetSelectedAsset(int prefabConstantId)
        {
            foreach (IPlateauSandboxAssetListView plateauSandboxAssetListView in m_AssetLists.Values)
            {
                var asset = plateauSandboxAssetListView.GetAssetByPrefabConstantId(prefabConstantId);
                if (asset != null)
                {
                    return asset;
                }
            }
            return null;
        }
    }
}