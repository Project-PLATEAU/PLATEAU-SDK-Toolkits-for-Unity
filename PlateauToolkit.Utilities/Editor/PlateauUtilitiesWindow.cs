using PlateauToolkit.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Utilities.Editor
{
    public class PlateauUtilitiesWindow : EditorWindow
    {
        public PlateauUtilitiesWindow m_Window;
 
        string m_SelectMeshRenderersPrefix;
        float m_FlattenMeshVerticesTargetHeight;
        float m_AlignMeshBottomsBaseHeight;

        List<MeshRenderer> m_MeshRenderers = new List<MeshRenderer>();

        Vector2 m_ScrollPosition;

        void OnEnable()
        {
            m_MeshRenderers.Clear();
        }

        void OnGUI()
        {
            #region Header
            m_Window ??= GetWindow<PlateauUtilitiesWindow>();
            PlateauToolkitEditorGUILayout.HeaderLogo(m_Window.position.width);
            #endregion

            PlateauUtilitiesLayout.Header("編集用ツール");
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("ヒエラルキー上の複数の地物を一括で選択します。複数の建物に対して一括で編集を行いたいときなどに使用できます。地物の種類を示す接頭語を指定することで、その接頭語を含むメッシュ群を一括で選択します。", MessageType.Info);
            EditorGUILayout.Space();

            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("地物種別の接頭語", GUILayout.Width(200));
            m_SelectMeshRenderersPrefix = EditorGUILayout.TextField(m_SelectMeshRenderersPrefix);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("メッシュレンダラーを選択"))
            {
                PlateauUtilitiesMeshOperations.SelectMeshRenderersByPrefix(prefix: m_SelectMeshRenderersPrefix);
            }

            EditorGUILayout.Space(30);
            EditorGUILayout.HelpBox("設定した高さにそろえてメッシュ頂点を平面化します。地面に使用することで、細かな凸凹を取り除き平らな面を作成することができます。", MessageType.Info);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("高さ", GUILayout.Width(200));
            m_FlattenMeshVerticesTargetHeight = EditorGUILayout.FloatField(m_FlattenMeshVerticesTargetHeight);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("メッシュ頂点を平面化"))
            {
                PlateauUtilitiesMeshOperations.FlattenSelectedMeshVertices(m_FlattenMeshVerticesTargetHeight);
            }
            EditorGUILayout.Space(30);
            EditorGUILayout.HelpBox("地物の底面の高さを整列させます。3D都市モデルの道路(TRAN)は高さを持たないため、この機能を用いて整列させると道路と建物の高さを合わせることができます", MessageType.Info);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("高さ", GUILayout.Width(200));
            m_AlignMeshBottomsBaseHeight = EditorGUILayout.FloatField(m_AlignMeshBottomsBaseHeight);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("選択地物を整列"))
            {
                PlateauUtilitiesMeshOperations.AlignMeshBottomsToBaseHeight(m_AlignMeshBottomsBaseHeight);
            }
            EditorGUILayout.Space(30);
            EditorGUILayout.HelpBox("プレハブにライトマップをベイクします。", MessageType.Info);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("シーンのライトマップをプレハブに設定"))
            {
                PrefabLightmapData.GenerateLightmapInfo();
            }

            EditorGUILayout.EndScrollView();
        }

        List<GameObject> GetMeshes(string filter = "")
        {
            string[] slicedArray = SliceString(filter);
            List<GameObject> meshes = new List<GameObject>();
            var selectedObjects = Selection.gameObjects;
            foreach (var obj in selectedObjects)
            {
                if (!PrefabUtility.IsPartOfAnyPrefab(obj) && obj.GetComponent<MeshRenderer>() != null)
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        foreach (string element in slicedArray)
                        {
                            if (obj.name.Contains(element))
                            {
                                meshes.Add(obj);
                            }
                        }
                    }
                    else
                    {
                        meshes.Add(obj);
                    }
                }
            }
            return meshes;
        }

        string[] SliceString(string input)
        {
            return input.Split(' '); // Splits the string by space character
        }
    }
}