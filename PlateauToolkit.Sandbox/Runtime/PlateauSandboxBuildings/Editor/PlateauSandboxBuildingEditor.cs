using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Editor
{
    [CustomEditor(typeof(Runtime.PlateauSandboxBuilding))]
    public class PlateauSandboxBuildingEditor : UnityEditor.Editor
    {
        private int m_AllowUndoCount;
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

        private SerializedProperty m_CommercialFacilityParams;
        private SerializedProperty m_CommercialFacilityVertexColorPalette;
        private SerializedProperty m_CommercialFacilityVertexColorMaterialPalette;
        private SerializedProperty m_CommercialFacilityMaterialPalette;

        private SerializedProperty m_HotelParams;
        private SerializedProperty m_HotelVertexColorPalette;
        private SerializedProperty m_HotelVertexColorMaterialPalette;
        private SerializedProperty m_HotelMaterialPalette;

        private SerializedProperty m_BuildingName;
        private SerializedProperty m_FacadePlanner;
        private SerializedProperty m_RoofPlanner;
        private GUIStyle m_SaveMeshBtnTextColorStyle;

        private void OnEnable()
        {
            m_Generator = (Runtime.PlateauSandboxBuilding) target;
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

            m_CommercialFacilityParams = serializedObject.FindProperty("commercialFacilityParams");
            m_CommercialFacilityVertexColorPalette = serializedObject.FindProperty("commercialFacilityVertexColorPalette");
            m_CommercialFacilityVertexColorMaterialPalette = serializedObject.FindProperty("commercialFacilityVertexColorMaterialPalette");
            m_CommercialFacilityMaterialPalette = serializedObject.FindProperty("commercialFacilityMaterialPalette");

            m_HotelParams = serializedObject.FindProperty("hotelParams");
            m_HotelVertexColorPalette = serializedObject.FindProperty("hotelVertexColorPalette");
            m_HotelVertexColorMaterialPalette = serializedObject.FindProperty("hotelVertexColorMaterialPalette");
            m_HotelMaterialPalette = serializedObject.FindProperty("hotelMaterialPalette");

            m_FacadePlanner = serializedObject.FindProperty("facadePlanner");
            m_RoofPlanner = serializedObject.FindProperty("roofPlanner");
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

        private bool DrawDynamicPropertyOnly(SerializedProperty inProperty, Dictionary<string, Tuple<string, float, float>> inMinMax = null)
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

                if (inMinMax != null && inMinMax.TryGetValue(iterator.name, out Tuple<string, float, float> minMaxTuple))
                {
                    if (iterator.type == "float")
                    {
                        iterator.floatValue = EditorGUILayout.Slider(minMaxTuple.Item1, iterator.floatValue, minMaxTuple.Item2, minMaxTuple.Item3);
                        iterator.floatValue = Mathf.Floor(iterator.floatValue * 100.0f) / 100f;
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }

                if (serializedObject.hasModifiedProperties)
                {
                    Undo.RecordObject(m_Generator, "Change property");
                    serializedObject.ApplyModifiedProperties();
                    return true;
                }
            }

            return false;
        }

        public override void OnInspectorGUI()
        {
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
                    case (int)BuildingType.k_SkyscraperCondominium:
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
                    case (int)BuildingType.k_Residence:
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
                    case (int)BuildingType.k_CommercialFacility:
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
                        break;
                }
            }
            else
            {
                switch (m_BuildingType.enumValueIndex)
                {
                    case (int)BuildingType.k_SkyscraperCondominium:
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
                    case (int)BuildingType.k_Residence:
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
                    case (int)BuildingType.k_CommercialFacility:
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
            m_Generator.buildingName = EditorGUILayout.TextField("Building Name", m_Generator.buildingName);
        }

        private bool BuildingDynamicGUI()
        {
            EditorGUI.BeginChangeCheck();
            int buildingTypeIndex = EditorGUILayout.Popup("建造物タイプ", m_BuildingType.enumValueIndex, new[]
            {
                "マンション", "オフィスビル", "住宅", "コンビニ", "商業ビル"//, "ホテル"
            });
            float buildingHeight = m_Generator.buildingHeight;
            switch (buildingTypeIndex)
            {
                case (int)BuildingType.k_SkyscraperCondominium:
                case (int)BuildingType.k_Hotel:
                    buildingHeight = EditorGUILayout.Slider("高さ", m_Generator.buildingHeight, 8.0f, 100.0f);
                    break;
                case (int)BuildingType.k_OfficeBuilding:
                case (int)BuildingType.k_CommercialFacility:
                    buildingHeight = EditorGUILayout.Slider("高さ", m_Generator.buildingHeight, 5.0f, 100.0f);
                    break;
            }
            float buildingWidth = EditorGUILayout.Slider("横幅", m_Generator.buildingWidth, 5.0f, 20.0f);
            float buildingDepth = EditorGUILayout.Slider("奥行き", m_Generator.buildingDepth, 5.0f, 20.0f);

            if (EditorGUI.EndChangeCheck())
            {
                if (buildingTypeIndex != m_BuildingType.enumValueIndex)
                {
                    Undo.RecordObject(m_Generator, "Change buildingType");
                    m_Generator.buildingType = (BuildingType)buildingTypeIndex;
                    m_BuildingType.enumValueIndex = buildingTypeIndex;

                    switch (buildingTypeIndex)
                    {
                        case (int)BuildingType.k_SkyscraperCondominium:
                            m_Generator.facadePlanner = Resources.Load<FacadePlanner>("ProceduralFacadeSkyscraperCondominiumPlanner");
                            m_FacadePlanner.objectReferenceValue = m_Generator.facadePlanner;
                            m_Generator.roofPlanner = Resources.Load<RoofPlanner>("ProceduralRoofSkyscraperCondominiumPlanner");
                            m_RoofPlanner.objectReferenceValue = m_Generator.roofPlanner;
                            break;
                        case (int)BuildingType.k_OfficeBuilding:
                            m_Generator.facadePlanner = Resources.Load<FacadePlanner>("ProceduralFacadeOfficeBuildingPlanner");
                            m_FacadePlanner.objectReferenceValue = m_Generator.facadePlanner;
                            m_Generator.roofPlanner = Resources.Load<RoofPlanner>("ProceduralRoofOfficeBuildingPlanner");
                            m_RoofPlanner.objectReferenceValue = m_Generator.roofPlanner;
                            break;
                        case (int)BuildingType.k_Residence:
                            m_Generator.facadePlanner = Resources.Load<FacadePlanner>("ProceduralFacadeResidencePlanner");
                            m_FacadePlanner.objectReferenceValue = m_Generator.facadePlanner;
                            m_Generator.roofPlanner = Resources.Load<RoofPlanner>("ProceduralRoofResidencePlanner");
                            m_RoofPlanner.objectReferenceValue = m_Generator.roofPlanner;
                            break;
                        case (int)BuildingType.k_ConvenienceStore:
                            m_Generator.facadePlanner = Resources.Load<FacadePlanner>("ProceduralFacadeConvenienceStorePlanner");
                            m_FacadePlanner.objectReferenceValue = m_Generator.facadePlanner;
                            m_Generator.roofPlanner = Resources.Load<RoofPlanner>("ProceduralRoofConvenienceStorePlanner");
                            m_RoofPlanner.objectReferenceValue = m_Generator.roofPlanner;
                            break;
                        case (int)BuildingType.k_CommercialFacility:
                            m_Generator.facadePlanner = Resources.Load<FacadePlanner>("ProceduralFacadeCommercialFacilityPlanner");
                            m_FacadePlanner.objectReferenceValue = m_Generator.facadePlanner;
                            m_Generator.roofPlanner = Resources.Load<RoofPlanner>("ProceduralRoofCommercialFacilityPlanner");
                            m_RoofPlanner.objectReferenceValue = m_Generator.roofPlanner;
                            break;
                        case (int)BuildingType.k_Hotel:
                            m_Generator.facadePlanner = Resources.Load<FacadePlanner>("ProceduralFacadeHotelPlanner");
                            m_FacadePlanner.objectReferenceValue = m_Generator.facadePlanner;
                            m_Generator.roofPlanner = Resources.Load<RoofPlanner>("ProceduralRoofHotelPlanner");
                            m_RoofPlanner.objectReferenceValue = m_Generator.roofPlanner;
                            break;
                    }
                }
                else if (Math.Abs(buildingHeight - m_Generator.buildingHeight) > float.Epsilon)
                {
                    Undo.RecordObject(m_Generator, "Change height");
                    m_Generator.buildingHeight = Mathf.Floor(buildingHeight * 10.0f) / 10f;
                }
                else if (Math.Abs(buildingWidth - m_Generator.buildingWidth) > float.Epsilon)
                {
                    Undo.RecordObject(m_Generator, "Change width");
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
                case (int)BuildingType.k_SkyscraperCondominium:
                    EditorGUILayout.LabelField("マンション設定", EditorStyles.boldLabel);
                    return DrawDynamicPropertyOnly(m_SkyscraperCondominiumParams);
                case (int)BuildingType.k_OfficeBuilding:
                    EditorGUILayout.LabelField("オフィスビル設定", EditorStyles.boldLabel);
                    return DrawDynamicPropertyOnly(m_OfficeBuildingParams, new Dictionary<string, Tuple<string, float, float>>
                    {
                        {"smallWindowHeight", new Tuple<string, float, float>("小さい窓の高さ", 0.25f, 2.5f)}
                    });
                case (int)BuildingType.k_Residence:
                    EditorGUILayout.LabelField("住宅設定", EditorStyles.boldLabel);
                    return DrawDynamicPropertyOnly(m_ResidenceParams);
                case (int)BuildingType.k_ConvenienceStore:
                    EditorGUILayout.LabelField("コンビニ設定", EditorStyles.boldLabel);
                    return DrawDynamicPropertyOnly(m_ConveniParams);
                case (int)BuildingType.k_CommercialFacility:
                    // EditorGUILayout.LabelField("商業ビル設定", EditorStyles.boldLabel);
                    // return DrawDynamicPropertyOnly(m_CommercialFacilityParams);
                    return false;
                case (int)BuildingType.k_Hotel:
                    EditorGUILayout.LabelField("ホテル設定", EditorStyles.boldLabel);
                    return DrawDynamicPropertyOnly(m_HotelParams);
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

        private void SavePrefab()
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(m_Generator.gameObject))
            {
                EditorUtility.DisplayDialog("建築物を新規プレハブとして保存", "プレハブインスタンスに対してのみ実行可能です。", "はい");
                return;
            }

            string meshAssetsFolderPath = BuildingMeshUtility.GetMeshAssetsFolderPath();
            if (!Directory.Exists(meshAssetsFolderPath))
            {
                Directory.CreateDirectory(meshAssetsFolderPath);
            }

            string prefabAssetsFolderPath = BuildingMeshUtility.GetPrefabAssetsFolderPath();
            if (!Directory.Exists(prefabAssetsFolderPath))
            {
                Directory.CreateDirectory(prefabAssetsFolderPath);
            }

            Match matchRes = Regex.Match(m_Generator.buildingName, "[0-9]+$");
            string newPrefabName;
            if (matchRes.Success)
            {
                int count = 0;
                m_Generator.buildingName = m_Generator.buildingName.Remove(matchRes.Index, matchRes.Length);

                do
                {
                    count++;
                    newPrefabName = m_Generator.buildingName + $"{int.Parse(matchRes.Value) + count:D2}";
                }
                while (File.Exists(Path.Combine(prefabAssetsFolderPath, newPrefabName + ".prefab").Replace("\\", "/")));

                m_Generator.buildingName += $"{int.Parse(matchRes.Value) + count:D2}";
            }
            else
            {
                m_Generator.buildingName += "_01";
                newPrefabName = m_Generator.buildingName;
            }

            var lsFacadeMeshFilter = m_Generator.transform.GetComponentsInChildren<MeshFilter>().ToList();
            if (!BuildingMeshUtility.SaveMesh(lsFacadeMeshFilter, newPrefabName))
            {
                EditorUtility.DisplayDialog("建築物のメッシュを保存", "建築物の保存に失敗しました。建築物を再生成して下さい。", "はい");
                return;
            }

            m_Generator.gameObject.name = m_Generator.buildingName;
            string prefabPath = Path.Combine(prefabAssetsFolderPath, newPrefabName + ".prefab").Replace("\\", "/");
            PrefabUtility.UnpackPrefabInstance(m_Generator.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            OnPrefabInstanceUpdatedParameter.instance.canUpdate = false;
            PrefabUtility.SaveAsPrefabAssetAndConnect(m_Generator.gameObject, prefabPath, InteractionMode.AutomatedAction);
            OnPrefabInstanceUpdatedParameter.instance.canUpdate = true;
            EditorUtility.DisplayDialog("建築物を新規プレハブとして保存", "建築物のプレハブが正常に保存されました。", "はい");
        }
    }
}
