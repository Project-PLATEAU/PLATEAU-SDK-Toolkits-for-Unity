using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Common;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Editor
{
    [CustomEditor(typeof(Runtime.PlateauSandboxBuilding))]
    public class PlateauSandboxBuildingEditor : UnityEditor.Editor
    {
        private int m_AllowUndoCount;
        private List<Object> m_UndoObjectWithShaderParam;
        private MeshRenderer m_RoofMeshRenderer;
        private Runtime.PlateauSandboxBuilding m_Generator;
        private Color m_GeneratorBtnColor;
        private Color m_SeparatorColor;
        private SerializedProperty m_BuildingType;

        private SerializedProperty m_SkyscraperCondominiumParams;
        private SerializedProperty m_SkyscraperCondominiumVertexColorPalette;
        private SerializedProperty m_SkyscraperCondominiumVertexColorMaterialPalette;
        private SerializedProperty m_SkyscraperCondominiumMaterialPalette;

        private SerializedProperty m_OfficeBuildingParams;
        private SerializedProperty m_OfficeBuildingVertexColorPalette;
        private SerializedProperty m_OfficeBuildingVertexColorMaterialPalette;
        private SerializedProperty m_OfficeBuildingMaterialPalette;

        private SerializedProperty m_ResidenceParams;
        private SerializedProperty m_ResidenceVertexColorPalette;
        private SerializedProperty m_ResidenceVertexColorMaterialPalette;
        private SerializedProperty m_ResidenceMaterialPalette;

        private SerializedProperty m_ConveniParams;
        private SerializedProperty m_ConveniVertexColorPalette;
        private SerializedProperty m_ConveniVertexColorMaterialPalette;
        private SerializedProperty m_ConveniMaterialPalette;

        private SerializedProperty m_CommercialFacilityVertexColorPalette;
        private SerializedProperty m_CommercialFacilityVertexColorMaterialPalette;
        private SerializedProperty m_CommercialFacilityMaterialPalette;

        private SerializedProperty m_HotelParams;
        private SerializedProperty m_HotelShaderParams;
        private SerializedProperty m_HotelVertexColorPalette;
        private SerializedProperty m_HotelVertexColorMaterialPalette;
        private SerializedProperty m_HotelMaterialPalette;

        private SerializedProperty m_FactoryParams;
        private SerializedProperty m_FactoryVertexColorPalette;
        private SerializedProperty m_FactoryVertexColorMaterialPalette;
        private SerializedProperty m_FactoryMaterialPalette;

        private GUIStyle m_SaveMeshBtnTextColorStyle;

        private void OnEnable()
        {
            m_Generator = (Runtime.PlateauSandboxBuilding) target;
            m_UndoObjectWithShaderParam = new List<Object>();

            var lsLodObject = m_Generator.gameObject.GetComponentsInChildrenWithoutSelf<Transform>().ToList();
            foreach (Transform roofObject in lsLodObject.Select(lodObject => lodObject.Find("Roof")).Where(roofObject => roofObject != null))
            {
                m_RoofMeshRenderer = roofObject.GetComponent<MeshRenderer>();
                break;
            }

            m_GeneratorBtnColor = GuiUtility.GetColor(GuiUtility.KColor.BtnColor);
            m_SeparatorColor = GuiUtility.GetColor(GuiUtility.KColor.Separator);
            m_BuildingType = serializedObject.FindProperty("buildingType");

            m_SkyscraperCondominiumParams = serializedObject.FindProperty("skyscraperCondominiumParams");
            m_SkyscraperCondominiumVertexColorPalette = serializedObject.FindProperty("skyscraperCondominiumVertexColorPalette");
            m_SkyscraperCondominiumVertexColorMaterialPalette = serializedObject.FindProperty("skyscraperCondominiumVertexColorMaterialPalette");
            m_SkyscraperCondominiumMaterialPalette = serializedObject.FindProperty("skyscraperCondominiumMaterialPalette");

            m_OfficeBuildingParams = serializedObject.FindProperty("officeBuildingParams");
            m_OfficeBuildingVertexColorPalette = serializedObject.FindProperty("officeBuildingVertexColorPalette");
            m_OfficeBuildingVertexColorMaterialPalette = serializedObject.FindProperty("officeBuildingVertexColorMaterialPalette");
            m_OfficeBuildingMaterialPalette = serializedObject.FindProperty("officeBuildingMaterialPalette");

            m_ResidenceParams = serializedObject.FindProperty("residenceParams");
            m_ResidenceVertexColorPalette = serializedObject.FindProperty("residenceVertexColorPalette");
            m_ResidenceVertexColorMaterialPalette = serializedObject.FindProperty("residenceVertexColorMaterialPalette");
            m_ResidenceMaterialPalette = serializedObject.FindProperty("residenceMaterialPalette");

            m_ConveniParams = serializedObject.FindProperty("conveniParams");
            m_ConveniVertexColorPalette = serializedObject.FindProperty("conveniVertexColorPalette");
            m_ConveniVertexColorMaterialPalette = serializedObject.FindProperty("conveniVertexColorMaterialPalette");
            m_ConveniMaterialPalette = serializedObject.FindProperty("conveniMaterialPalette");

            m_CommercialFacilityVertexColorPalette = serializedObject.FindProperty("commercialFacilityVertexColorPalette");
            m_CommercialFacilityVertexColorMaterialPalette = serializedObject.FindProperty("commercialFacilityVertexColorMaterialPalette");
            m_CommercialFacilityMaterialPalette = serializedObject.FindProperty("commercialFacilityMaterialPalette");

            m_HotelParams = serializedObject.FindProperty("hotelParams");
            m_HotelShaderParams = serializedObject.FindProperty("hotelShaderParams");
            m_HotelVertexColorPalette = serializedObject.FindProperty("hotelVertexColorPalette");
            m_HotelVertexColorMaterialPalette = serializedObject.FindProperty("hotelVertexColorMaterialPalette");
            m_HotelMaterialPalette = serializedObject.FindProperty("hotelMaterialPalette");

            m_FactoryParams = serializedObject.FindProperty("factoryParams");
            m_FactoryVertexColorPalette = serializedObject.FindProperty("factoryVertexColorPalette");
            m_FactoryVertexColorMaterialPalette = serializedObject.FindProperty("factoryVertexColorMaterialPalette");
            m_FactoryMaterialPalette = serializedObject.FindProperty("factoryMaterialPalette");

            m_SaveMeshBtnTextColorStyle = null;

            Undo.undoRedoPerformed += OnUndo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndo;
        }

        private void OnUndo()
        {
            if (m_AllowUndoCount > 0)
            {
                m_AllowUndoCount--;
            }
            else
            {
                return;
            }

            // Supported LOD: LOD0
            foreach (int lodNum in new List<int> {0})
            {
                if (m_Generator != null)
                {
                    m_Generator.GenerateMesh(lodNum, m_Generator.buildingWidth, m_Generator.buildingDepth);
                }
            }
        }

        private bool DrawDynamicPropertyOnly(SerializedProperty inProperty)
        {
            int depth = inProperty.depth;
            SerializedProperty iterator = inProperty.Copy();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                // 次のプロパティが親の深さよりも浅い場合は終了
                if (iterator.depth < depth)
                {
                    return false;
                }
                depth = iterator.depth;

                EditorGUILayout.PropertyField(iterator, true);

                if (serializedObject.hasModifiedProperties)
                {
                    Undo.RecordObject(m_Generator, "Change Property");
                    serializedObject.ApplyModifiedProperties();
                    return true;
                }
            }

            return false;
        }

        private bool DrawDynamicPropertyOnly<T>(SerializedProperty inProperty, Dictionary<string, Tuple<string, T, T>> inMinMax = null, bool isUpdateShaderParams = false) where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
        {
            int depth = inProperty.depth;
            SerializedProperty iterator = inProperty.Copy();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                // 次のプロパティが親の深さよりも浅い場合は終了
                if (iterator.depth < depth)
                {
                    return false;
                }
                depth = iterator.depth;

                if (inMinMax != null && inMinMax.TryGetValue(iterator.name, out Tuple<string, T, T> minMaxTuple))
                {
                    switch (iterator.type)
                    {
                        case "int":
                            iterator.intValue = EditorGUILayout.IntSlider(minMaxTuple.Item1, iterator.intValue, (int)(dynamic)minMaxTuple.Item2, (int)(dynamic)minMaxTuple.Item3);
                            break;
                        case "float":
                            iterator.floatValue = EditorGUILayout.Slider(minMaxTuple.Item1, iterator.floatValue, (float)(dynamic)minMaxTuple.Item2, (float)(dynamic)minMaxTuple.Item3);
                            iterator.floatValue = Mathf.Floor(iterator.floatValue * 1000.0f) / 1000f;
                            break;
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }

                if (serializedObject.hasModifiedProperties)
                {
                    if (isUpdateShaderParams)
                    {
                        m_UndoObjectWithShaderParam.Add(m_Generator);
                        Undo.RecordObjects(m_UndoObjectWithShaderParam.ToArray(), "Change Building Params");
                    }
                    else
                    {
                        Undo.RecordObject(m_Generator, "Change property");
                        serializedObject.ApplyModifiedProperties();
                    }
                    return true;
                }
            }

            return false;
        }

        public override void OnInspectorGUI()
        {
            m_UndoObjectWithShaderParam.Clear();
            bool changedValue = false;
            serializedObject.Update();

            EditorGUILayout.LabelField("建造物設定", EditorStyles.boldLabel);
            if (BuildingDynamicGUI())
            {
                changedValue = true;
            }
            EditorGUILayout.Space(10);
            if (BuildingTypeDynamicGUI())
            {
                changedValue = true;
            }
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("色設定", EditorStyles.boldLabel);
            if (MaterialDynamicGUI())
            {
                changedValue = true;
            }
            if (m_Generator.useTexture)
            {
                switch (m_BuildingType.enumValueIndex)
                {
                    case (int)BuildingType.k_Apartment:
                        if (DrawDynamicPropertyOnly(m_SkyscraperCondominiumMaterialPalette))
                        {
                            changedValue = true;
                        }
                        break;
                    case (int)BuildingType.k_OfficeBuilding:
                        if (DrawDynamicPropertyOnly(m_OfficeBuildingMaterialPalette))
                        {
                            changedValue = true;
                        }
                        break;
                    case (int)BuildingType.k_House:
                        if (DrawDynamicPropertyOnly(m_ResidenceMaterialPalette))
                        {
                            changedValue = true;
                        }
                        break;
                    case (int)BuildingType.k_ConvenienceStore:
                        if (DrawDynamicPropertyOnly(m_ConveniMaterialPalette))
                        {
                            changedValue = true;
                        }
                        break;
                    case (int)BuildingType.k_CommercialBuilding:
                        if (DrawDynamicPropertyOnly(m_CommercialFacilityMaterialPalette))
                        {
                            changedValue = true;
                        }
                        break;
                    case (int)BuildingType.k_Hotel:
                        if (DrawDynamicPropertyOnly(m_HotelMaterialPalette))
                        {
                            changedValue = true;
                        }
                        EditorGUILayout.Space(10);
                        EditorGUILayout.LabelField("テクスチャ設定", EditorStyles.boldLabel);
                        if (DrawDynamicPropertyOnly(m_HotelShaderParams, new Dictionary<string, Tuple<string, float, float>>
                            {
                                {"textureOffsetX", new Tuple<string, float, float>("横のオフセット値", -1f, 1f)},
                                {"textureOffsetY", new Tuple<string, float, float>("縦のオフセット値", -1f, 1f)},
                            }))
                        {
                            SerializedProperty  roofSideFrontSerializedProperty = m_HotelMaterialPalette.FindPropertyRelative("roofSideFront");
                            if (roofSideFrontSerializedProperty != null)
                            {
                                Object roofSideFrontObjRef = roofSideFrontSerializedProperty.objectReferenceValue;
                                if (roofSideFrontObjRef != null)
                                {
                                    var material = (Material)roofSideFrontObjRef;
                                    if (material.shader.name.Contains("Movable Texture"))
                                    {
                                        Undo.RecordObject(material, "Change Shader Params");
                                        material.SetFloat("_TextOffsetX", m_HotelShaderParams.FindPropertyRelative("textureOffsetX").floatValue);
                                        material.SetFloat("_TextOffsetY", m_HotelShaderParams.FindPropertyRelative("textureOffsetY").floatValue);
                                    }
                                }
                            }
                        }
                        break;
                    case (int)BuildingType.k_Factory:
                        if (DrawDynamicPropertyOnly(m_FactoryMaterialPalette))
                        {
                            changedValue = true;
                        }
                        break;
                }
            }
            else
            {
                switch (m_BuildingType.enumValueIndex)
                {
                    case (int)BuildingType.k_Apartment:
                        if (DrawDynamicPropertyOnly(m_SkyscraperCondominiumVertexColorPalette) || DrawDynamicPropertyOnly(m_SkyscraperCondominiumVertexColorMaterialPalette))
                        {
                            changedValue = true;
                        }
                        break;
                    case (int)BuildingType.k_OfficeBuilding:
                        if (DrawDynamicPropertyOnly(m_OfficeBuildingVertexColorPalette) || DrawDynamicPropertyOnly(m_OfficeBuildingVertexColorMaterialPalette))
                        {
                            changedValue = true;
                        }
                        break;
                    case (int)BuildingType.k_House:
                        if (DrawDynamicPropertyOnly(m_ResidenceVertexColorPalette) || DrawDynamicPropertyOnly(m_ResidenceVertexColorMaterialPalette))
                        {
                            changedValue = true;
                        }
                        break;
                    case (int)BuildingType.k_ConvenienceStore:
                        if (DrawDynamicPropertyOnly(m_ConveniVertexColorPalette) || DrawDynamicPropertyOnly(m_ConveniVertexColorMaterialPalette))
                        {
                            changedValue = true;
                        }
                        break;
                    case (int)BuildingType.k_CommercialBuilding:
                        if (DrawDynamicPropertyOnly(m_CommercialFacilityVertexColorPalette) || DrawDynamicPropertyOnly(m_CommercialFacilityVertexColorMaterialPalette))
                        {
                            changedValue = true;
                        }
                        break;
                    case (int)BuildingType.k_Hotel:
                        if (DrawDynamicPropertyOnly(m_HotelVertexColorPalette) || DrawDynamicPropertyOnly(m_HotelVertexColorMaterialPalette))
                        {
                            changedValue = true;
                        }
                        break;
                    case (int)BuildingType.k_Factory:
                        if (DrawDynamicPropertyOnly(m_FactoryVertexColorPalette) || DrawDynamicPropertyOnly(m_FactoryVertexColorMaterialPalette))
                        {
                            changedValue = true;
                        }
                        break;
                }
            }
            if (changedValue)
            {
                m_AllowUndoCount++;
                EditorUtility.SetDirty(m_Generator);

                // Supported LOD: LOD0
                foreach (int lodNum in new List<int> {0})
                {
                    m_Generator.GenerateMesh(lodNum, m_Generator.buildingWidth, m_Generator.buildingDepth);
                }

                // Shaderパラメータと一緒にUndoするパラメータが登録されていれば、Shaderパラメータを更新
                if (0 < m_UndoObjectWithShaderParam.Count)
                {
                    UpdateShaderParam();
                }
            }
            EditorGUILayout.Space(10);

            using (new ColorScope(KColorScope.Background, m_GeneratorBtnColor))
            {
                if (m_SaveMeshBtnTextColorStyle == null)
                {
                    m_SaveMeshBtnTextColorStyle = new GUIStyle(GUI.skin.button)
                    {
                        normal = { textColor = Color.white },
                        hover = { textColor = Color.white },
                        active = { textColor = Color.white }
                    };
                }

                if (GUILayout.Button("建築物を新規プレハブとして保存", m_SaveMeshBtnTextColorStyle))
                {
                    SavePrefab();
                }
            }

            EditorGUILayout.Space(5);
            GuiUtility.Separator(m_SeparatorColor);
            EditorGUILayout.Space(5);

            m_Generator.facadePlanner = (FacadePlanner)EditorGUILayout.ObjectField("FacadePlanner", m_Generator.facadePlanner, typeof(ScriptableObject), allowSceneObjects: true);
            m_Generator.facadeConstructor = (FacadeConstructor)EditorGUILayout.ObjectField("FacadeConstructor", m_Generator.facadeConstructor, typeof(ScriptableObject), allowSceneObjects: true);
            m_Generator.roofPlanner = (RoofPlanner)EditorGUILayout.ObjectField("RoofPlanner", m_Generator.roofPlanner, typeof(ScriptableObject), allowSceneObjects: true);
            m_Generator.roofConstructor = (RoofConstructor)EditorGUILayout.ObjectField("RoofConstructor", m_Generator.roofConstructor, typeof(ScriptableObject), allowSceneObjects: true);
        }

        private void UpdateShaderParam()
        {
            SerializedProperty  roofSideFrontSerializedProperty = m_HotelMaterialPalette.FindPropertyRelative("roofSideFront");
            if (roofSideFrontSerializedProperty != null)
            {
                Object roofSideFrontObjRef = roofSideFrontSerializedProperty.objectReferenceValue;
                if (roofSideFrontObjRef != null)
                {
                    var roofSideFrontMat = (Material)roofSideFrontObjRef;
                    if (roofSideFrontMat.shader.name.Contains("Movable Texture"))
                    {
                        if (m_RoofMeshRenderer != null)
                        {
                            m_UndoObjectWithShaderParam.Add(roofSideFrontMat);
                            Undo.RecordObjects(m_UndoObjectWithShaderParam.ToArray(), "Change Building Params");

                            Bounds bounds = m_RoofMeshRenderer.bounds;
                            Vector3 size = bounds.size;
                            Texture textMap = roofSideFrontMat.GetTexture("_TextMap");
                            float meshAspect = size.x / size.y;
                            float textureAspect = (float)textMap.width / textMap.height;
                            float aspect = meshAspect / textureAspect;
                            float normalize = textureAspect / meshAspect;
                            roofSideFrontMat.SetFloat("_TextureCenterOffsetX", aspect * 0.5f - aspect * normalize * 0.5f);
                            roofSideFrontMat.SetFloat("_TextureAspect", aspect);
                            roofSideFrontMat.SetFloat("_TextureNormalize", normalize);
                        }
                    }
                }
            }
        }

        private bool BuildingDynamicGUI()
        {
            EditorGUI.BeginChangeCheck();

            float buildingHeight = m_Generator.buildingHeight;
            switch (m_BuildingType.enumValueIndex)
            {
                case (int)BuildingType.k_Apartment:
                case (int)BuildingType.k_OfficeBuilding:
                case (int)BuildingType.k_CommercialBuilding:
                case (int)BuildingType.k_Hotel:
                case (int)BuildingType.k_Factory:
                    buildingHeight = EditorGUILayout.Slider("高さ", m_Generator.buildingHeight, 5.0f, 100.0f);
                    break;
            }
            float buildingWidth = EditorGUILayout.Slider("横幅", m_Generator.buildingWidth, 3.0f, 50.0f);
            float buildingDepth = EditorGUILayout.Slider("奥行き", m_Generator.buildingDepth, 3.0f, 50.0f);

            if (EditorGUI.EndChangeCheck())
            {
                if (Math.Abs(buildingHeight - m_Generator.buildingHeight) > float.Epsilon)
                {
                    Undo.RecordObject(m_Generator, "Change height");
                    m_Generator.buildingHeight = Mathf.Floor(buildingHeight * 10.0f) / 10f;
                }
                else if (Math.Abs(buildingWidth - m_Generator.buildingWidth) > float.Epsilon)
                {
                    m_UndoObjectWithShaderParam.Add(m_Generator);
                    Undo.RecordObjects(m_UndoObjectWithShaderParam.ToArray(), "Change Building Params");
                    m_Generator.buildingWidth = Mathf.Floor(buildingWidth * 10.0f) / 10f;
                }
                else if (Math.Abs(buildingDepth - m_Generator.buildingDepth) > float.Epsilon)
                {
                    Undo.RecordObject(m_Generator, "Change depth");
                    m_Generator.buildingDepth = Mathf.Floor(buildingDepth * 10.0f) / 10f;
                }

                serializedObject.ApplyModifiedProperties();
                return true;
            }
            return false;
        }

        private bool BuildingTypeDynamicGUI()
        {
            switch (m_BuildingType.enumValueIndex)
            {
                case (int)BuildingType.k_Apartment:
                    EditorGUILayout.LabelField("マンション設定", EditorStyles.boldLabel);
                    return DrawDynamicPropertyOnly(m_SkyscraperCondominiumParams);
                case (int)BuildingType.k_OfficeBuilding:
                    EditorGUILayout.LabelField("オフィスビル設定", EditorStyles.boldLabel);
                    return DrawDynamicPropertyOnly(m_OfficeBuildingParams, new Dictionary<string, Tuple<string, float, float>>
                    {
                        {"spandrelHeight", new Tuple<string, float, float>("壁パネルの高さ", 0.25f, 2.5f)}
                    });
                case (int)BuildingType.k_House:
                    EditorGUILayout.LabelField("住宅設定", EditorStyles.boldLabel);
                    return DrawDynamicPropertyOnly(m_ResidenceParams, new Dictionary<string, Tuple<string, int, int>>
                    {
                        {"numFloor", new Tuple<string, int, int>("階数", 1, 3)}
                    });
                case (int)BuildingType.k_ConvenienceStore:
                    EditorGUILayout.LabelField("コンビニ設定", EditorStyles.boldLabel);
                    return DrawDynamicPropertyOnly(m_ConveniParams);
                case (int)BuildingType.k_CommercialBuilding:
                    // EditorGUILayout.LabelField("商業ビル設定", EditorStyles.boldLabel);
                    // return DrawDynamicPropertyOnly(m_CommercialFacilityParams);
                    return false;
                case (int)BuildingType.k_Hotel:
                    EditorGUILayout.LabelField("ホテル設定", EditorStyles.boldLabel);
                    return DrawDynamicPropertyOnly(m_HotelParams, new Dictionary<string, Tuple<string, float, float>>
                    {
                        {"roofThickness", new Tuple<string, float, float>("屋根の暑さ", 0f, 5f)}
                    }, isUpdateShaderParams:true);
                case (int)BuildingType.k_Factory:
                    EditorGUILayout.LabelField("工場設定", EditorStyles.boldLabel);
                    return DrawDynamicPropertyOnly(m_FactoryParams);
            }
            return false;
        }

        private bool MaterialDynamicGUI()
        {
            EditorGUI.BeginChangeCheck();
            bool useTexture = EditorGUILayout.Toggle("テクスチャ利用", m_Generator.useTexture);

            if (EditorGUI.EndChangeCheck())
            {
                if (useTexture != m_Generator.useTexture)
                {
                    Undo.RecordObject(m_Generator, "Change useTexture");
                    m_Generator.useTexture = useTexture;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// インスペクタ上のプレハブ保存ボタン押下時にコール
        /// </summary>
        private void SavePrefab()
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(m_Generator.gameObject))
            {
                EditorUtility.DisplayDialog("建築物を新規プレハブとして保存に失敗", "プレハブインスタンスに対してのみ実行可能です。", "はい");
                return;
            }

            Runtime.PlateauSandboxBuilding prefab = PrefabUtility.GetCorrespondingObjectFromSource(m_Generator);
            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            var lsFacadeMeshFilter = m_Generator.transform.GetComponentsInChildren<MeshFilter>().ToList();
            if (!BuildingMeshUtility.SaveMesh(prefabPath, m_Generator.GetBuildingName(), lsFacadeMeshFilter, true))
            {
                EditorUtility.DisplayDialog("建築物のメッシュ保存に失敗", "建築物の保存に失敗しました。建築物を再生成して下さい。", "はい");
                return;
            }

            string prefabFolderPath = Path.GetDirectoryName(prefabPath);
            if (prefabFolderPath == null)
            {
                EditorUtility.DisplayDialog("建築物のメッシュ保存に失敗", "保存パスが不正です。", "はい");
                return;
            }
            string newPrefabName = BuildingMeshUtility.GetMeshNamePrefix(prefabFolderPath, m_Generator.GetBuildingName(), true);
            m_Generator.gameObject.name = newPrefabName;
            string prefabFullPath = Path.Combine(prefabFolderPath, newPrefabName + ".prefab").Replace("\\", "/");
            PrefabUtility.UnpackPrefabInstance(m_Generator.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            OnPrefabInstanceUpdatedParameter.instance.canUpdatePrefabInstance = false;
            PrefabUtility.SaveAsPrefabAssetAndConnect(m_Generator.gameObject, prefabFullPath, InteractionMode.AutomatedAction);
            OnPrefabInstanceUpdatedParameter.instance.canUpdatePrefabInstance = true;
            EditorUtility.DisplayDialog("建築物を新規プレハブとして保存に成功", "建築物のプレハブが正常に保存されました。", "はい");
        }
    }
}
