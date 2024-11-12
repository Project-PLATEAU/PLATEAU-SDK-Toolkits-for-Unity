﻿using PlateauToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    class PlateauSandboxWindowTrafficView : IPlateauSandboxWindowView
    {
        IPlateauSandboxAssetListView m_CurrentAssetList;
        bool m_IsShowAssetCreate = false;
        GameObject m_SelectedUserAsset = null;

        public string Name => "";

        public void OnBegin(PlateauSandboxContext context, EditorWindow editorWindow)
        {
            m_CurrentAssetList = new PlateauSandboxAssetListViewVehicle();
            m_CurrentAssetList.EnableMultipleSelect(true);
            m_CurrentAssetList.OnBegin();
        }

        public void OnGUI(PlateauSandboxContext context, EditorWindow window)
        {
            using (var scope = new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.Space(5);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(15);
                    GUILayout.Label("車両アセットの選択", PlateauToolkitGUIStyles.HeaderTextStyle, GUILayout.ExpandWidth(false));
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space(10);
                DrawSelectButtons(window.position.width, context);

                EditorGUILayout.Space(15);

                DrawTab(window.position.width);

                m_CurrentAssetList.OnGUI(context, window.position.width, m_IsShowAssetCreate);

                if (m_CurrentAssetList.SelectedAssetType == SandboxAssetType.UserDefined)
                {
                    DrawAddUserAsset();
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (new PlateauToolkitImageButtonGUI(
                        180,
                        40,
                        PlateauToolkitGUIStyles.k_ButtonPrimaryColor)
                    .Button("実行"))
                    {
                        //TODO

                        Debug.Log($"<color=red>実行</color>");

                        foreach(var obj in context.SelectedObjectsMultiple)
                        {
                            Debug.Log($"<color=yellow>Selected : {obj.name}</color>");
                        }

                        PlateauSandboxRoadNetwork roadNetwork = new PlateauSandboxRoadNetwork();
                        roadNetwork.PlaceVehicles(context.SelectedObjectsMultiple);
                    }
                }
            }
        }

        void DrawSelectButtons(float windowWidth, PlateauSandboxContext context)
        {
            using (var scope = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(15);

                if (new PlateauToolkitImageButtonGUI(
                    180,
                    30,
                    PlateauToolkitGUIStyles.k_ButtonNormalColor,
                    false)
                .Button(m_CurrentAssetList.SelectedAssetType.ToDisplayName() + "を全選択"))
                {
                    m_CurrentAssetList.SelectAll(true, context);
                }

                GUILayout.Space(15);

                if (new PlateauToolkitImageButtonGUI(
                    180,
                    30,
                    PlateauToolkitGUIStyles.k_ButtonNormalColor,
                    false)
                .Button(m_CurrentAssetList.SelectedAssetType.ToDisplayName() + "を全選択解除"))
                {
                    m_CurrentAssetList.SelectAll(false, context);
                }

                GUILayout.Space(15);
            }
        }

        void DrawAddUserAsset()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(15);

                if (new PlateauToolkitImageButtonGUI(
                220,
                40,
                PlateauToolkitGUIStyles.k_ButtonPrimaryColor).Button("アセットを作成"))
                {
                    PlateauSandboxPrefabCreationWizard.DisplayWizard();
                }

                //m_SelectedUserAsset = (GameObject)EditorGUILayout.ObjectField("ユーザーアセットを追加", m_SelectedUserAsset, typeof(GameObject), true, GUILayout.MinWidth(300));
                //GUILayout.Space(5);
                //if (new PlateauToolkitImageButtonGUI(
                //    60,
                //    20,
                //    PlateauToolkitGUIStyles.k_ButtonPrimaryColor,
                //    false)
                //.Button("追加"))
                //{
                //    //TODO
                //    Debug.Log($"<color=red>追加</color>");
                //}
                //GUILayout.Space(15);
            }
            GUILayout.Space(15);
        }

        void DrawTab(float windowWidth)
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
    }
}