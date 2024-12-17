using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Common;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Configs;
using ProceduralToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime
{
    [SelectionBase]
    public class PlateauSandboxBuilding : PlateauSandboxPlaceableHandler
    {
        private const int k_DefaultBuildingWidth = 5;
        private const int k_DefaultBuildingDepth = 5;
        private readonly BuildingGenerator.Config m_Config = new();

        public BuildingType buildingType = BuildingType.k_Apartment;

        [HideInInspector]
        public float buildingHeight = 5;

        [HideInInspector]
        public float buildingWidth = 5;

        [HideInInspector]
        public float buildingDepth = 5;

        [HideInInspector]
        public bool useTexture = true;

        public ApartmentConfig.Params skyscraperCondominiumParams = new();
        public ApartmentConfig.VertexColorPalette skyscraperCondominiumVertexColorPalette = new();
        public ApartmentConfig.VertexColorMaterialPalette skyscraperCondominiumVertexColorMaterialPalette = new();
        public ApartmentConfig.MaterialPalette skyscraperCondominiumMaterialPalette = new();

        public OfficeBuildingConfig.Params officeBuildingParams = new();
        public OfficeBuildingConfig.VertexColorPalette officeBuildingVertexColorPalette = new();
        public OfficeBuildingConfig.VertexColorMaterialPalette officeBuildingVertexColorMaterialPalette = new();
        public OfficeBuildingConfig.MaterialPalette officeBuildingMaterialPalette = new();

        public HouseConfig.Params residenceParams = new();
        public HouseConfig.VertexColorPalette residenceVertexColorPalette = new();
        public HouseConfig.VertexColorMaterialPalette residenceVertexColorMaterialPalette = new();
        public HouseConfig.MaterialPalette residenceMaterialPalette = new();

        public ConvenienceStoreConfig.Params conveniParams = new();
        public ConvenienceStoreConfig.VertexColorPalette conveniVertexColorPalette = new();
        public ConvenienceStoreConfig.VertexColorMaterialPalette conveniVertexColorMaterialPalette = new();
        public ConvenienceStoreConfig.MaterialPalette conveniMaterialPalette = new();

        public CommercialBuildingConfig.Params commercialFacilityParams = new();
        public CommercialBuildingConfig.VertexColorPalette commercialFacilityVertexColorPalette = new();
        public CommercialBuildingConfig.VertexColorMaterialPalette commercialFacilityVertexColorMaterialPalette = new();
        public CommercialBuildingConfig.MaterialPalette commercialFacilityMaterialPalette = new();

        public HotelConfig.Params hotelParams = new();
        public HotelConfig.ShaderParams hotelShaderParams = new();
        public HotelConfig.VertexColorPalette hotelVertexColorPalette = new();
        public HotelConfig.VertexColorMaterialPalette hotelVertexColorMaterialPalette = new();
        public HotelConfig.MaterialPalette hotelMaterialPalette = new();

        public FactoryConfig.Params factoryParams = new();
        public FactoryConfig.VertexColorPalette factoryVertexColorPalette = new();
        public FactoryConfig.VertexColorMaterialPalette factoryVertexColorMaterialPalette = new();
        public FactoryConfig.MaterialPalette factoryMaterialPalette = new();

        public ComplexBuildingConfig.BuildingPlannerParams m_ComplexBuildingPlannerParams = new();
        public ComplexBuildingConfig.Params complexBuildingParams = new();
        public ComplexBuildingConfig.SkyscraperCondominiumParams complexSkyscraperCondominiumBuildingParams = new();
        public ComplexBuildingConfig.OfficeParams complexOfficeBuildingParams = new();
        public ComplexBuildingConfig.VertexColorPalette complexBuildingVertexColorPalette = new();
        public ComplexBuildingConfig.VertexColorMaterialPalette complexBuildingVertexColorMaterialPalette = new();
        public ComplexBuildingConfig.MaterialPalette complexBuildingMaterialPalette = new();

        public FacadePlanner facadePlanner;
        public FacadeConstructor facadeConstructor;
        public RoofPlanner roofPlanner;
        public RoofConstructor roofConstructor;

        public string GetBuildingName()
        {
            return buildingType switch
            {
                BuildingType.k_Apartment => "Apartment",
                BuildingType.k_OfficeBuilding => "OfficeBuilding",
                BuildingType.k_House => "House",
                BuildingType.k_ConvenienceStore => "ConvenienceStore",
                BuildingType.k_CommercialBuilding => "CommercialBuilding",
                BuildingType.k_Hotel => "Hotel",
                BuildingType.k_Factory => "Factory",
                BuildingType.k_ComplexBuilding => "ComplexBuilding",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void GenerateMesh(int inLodNum, float inBuildingWidth, float inBuildingDepth)
        {
            m_Config.buildingType = buildingType;
            m_Config.buildingHeight = buildingHeight;
            m_Config.useTexture = useTexture;

            m_Config.skyscraperCondominiumParams = skyscraperCondominiumParams;
            m_Config.skyscraperCondominiumVertexColorPalette = skyscraperCondominiumVertexColorPalette;
            m_Config.skyscraperCondominiumVertexColorMaterialPalette = skyscraperCondominiumVertexColorMaterialPalette;
            m_Config.skyscraperCondominiumMaterialPalette = skyscraperCondominiumMaterialPalette;

            m_Config.officeBuildingParams = officeBuildingParams;
            m_Config.officeBuildingVertexColorPalette = officeBuildingVertexColorPalette;
            m_Config.officeBuildingVertexColorMaterialPalette = officeBuildingVertexColorMaterialPalette;
            m_Config.officeBuildingMaterialPalette = officeBuildingMaterialPalette;

            m_Config.residenceParams = residenceParams;
            m_Config.residenceVertexColorPalette = residenceVertexColorPalette;
            m_Config.residenceVertexColorMaterialPalette = residenceVertexColorMaterialPalette;
            m_Config.residenceMaterialPalette = residenceMaterialPalette;

            m_Config.conveniParams = conveniParams;
            m_Config.conveniVertexColorPalette = conveniVertexColorPalette;
            m_Config.conveniVertexColorMaterialPalette = conveniVertexColorMaterialPalette;
            m_Config.conveniMaterialPalette = conveniMaterialPalette;

            m_Config.commercialFacilityParams = commercialFacilityParams;
            m_Config.commercialFacilityVertexColorPalette = commercialFacilityVertexColorPalette;
            m_Config.commercialFacilityVertexColorMaterialPalette = commercialFacilityVertexColorMaterialPalette;
            m_Config.commercialFacilityMaterialPalette = commercialFacilityMaterialPalette;

            m_Config.hotelParams = hotelParams;
            m_Config.hotelVertexColorPalette = hotelVertexColorPalette;
            m_Config.hotelVertexColorMaterialPalette = hotelVertexColorMaterialPalette;
            m_Config.hotelMaterialPalette = hotelMaterialPalette;

            m_Config.factoryParams = factoryParams;
            m_Config.factoryVertexColorPalette = factoryVertexColorPalette;
            m_Config.factoryVertexColorMaterialPalette = factoryVertexColorMaterialPalette;
            m_Config.factoryMaterialPalette = factoryMaterialPalette;

            m_Config.m_ComplexBuildingPlannerParams = m_ComplexBuildingPlannerParams;
            m_Config.complexBuildingParams = complexBuildingParams;
            m_Config.complexSkyscraperCondominiumBuildingParams = complexSkyscraperCondominiumBuildingParams;
            m_Config.complexOfficeBuildingParams = complexOfficeBuildingParams;
            m_Config.complexBuildingVertexColorPalette = complexBuildingVertexColorPalette;
            m_Config.complexBuildingVertexColorMaterialPalette = complexBuildingVertexColorMaterialPalette;
            m_Config.complexBuildingMaterialPalette = complexBuildingMaterialPalette;

            m_Config.lodNum = inLodNum;

            float buildingWidthDiff = (inBuildingWidth - k_DefaultBuildingWidth) * 1f;
            float buildingDepthDiff = (inBuildingDepth - k_DefaultBuildingDepth) * 1f;
            const float halfBoundingBoxMultiplier = 0.5f;
            var lsFoundationPolygonVertex = new List<Vector2>
            {
                new((k_DefaultBuildingWidth + buildingWidthDiff) * halfBoundingBoxMultiplier, (-k_DefaultBuildingDepth - buildingDepthDiff) * halfBoundingBoxMultiplier),
                new((-k_DefaultBuildingWidth - buildingWidthDiff) * halfBoundingBoxMultiplier, (-k_DefaultBuildingDepth - buildingDepthDiff) * halfBoundingBoxMultiplier),
                new((-k_DefaultBuildingWidth - buildingWidthDiff) * halfBoundingBoxMultiplier, (k_DefaultBuildingDepth + buildingDepthDiff) * halfBoundingBoxMultiplier),
                new((k_DefaultBuildingWidth + buildingWidthDiff) * halfBoundingBoxMultiplier, (k_DefaultBuildingDepth + buildingDepthDiff) * halfBoundingBoxMultiplier)
            };

            try
            {
                var generator = new BuildingGenerator();
                generator.SetFacadePlanner(facadePlanner);
                generator.SetFacadeConstructor(facadeConstructor);
                generator.SetRoofPlanner(roofPlanner);
                generator.SetRoofConstructor(roofConstructor);

                var lsLodObject = gameObject.GetComponentsInChildrenWithoutSelf<Transform>().ToList();
                foreach (Transform lodObject in lsLodObject)
                {
                    if (!lodObject.name.Contains($"{inLodNum}"))
                    {
                        continue;
                    }

                    float facadesHeight = 0;
                    Transform facadesObject = lodObject.Find("Facades");
                    if (facadesObject)
                    {
                        MeshFilter meshFilter = facadesObject.GetComponent<MeshFilter>();
                        MeshRenderer meshRenderer = facadesObject.GetComponent<MeshRenderer>();
                        (CompoundMeshDraft, float) generatedResult = generator.GenerateFacadesMesh(lsFoundationPolygonVertex, m_Config);
                        CompoundMeshDraft facadesDraft = generatedResult.Item1;
                        facadesHeight = generatedResult.Item2;
                        Mesh mesh = facadesDraft.ToMeshWithSubMeshes();
                        NormalSolver.RecalculateNormals(mesh, 0);
                        meshFilter.mesh = mesh;

                        var materials = new List<Material>();
                        foreach (MeshDraft draft in facadesDraft)
                        {
                            materials.AddRange(draft.materials);
                        }
                        meshRenderer.materials = materials.ToArray();

                        if (!facadesObject.TryGetComponent(out BoxCollider boxCollider))
                        {
                            boxCollider = facadesObject.gameObject.AddComponent<BoxCollider>();
                        }

                        boxCollider.center = meshFilter.sharedMesh.bounds.center;
                        boxCollider.size = meshFilter.sharedMesh.bounds.size;
                    }

                    Transform roofObject = lodObject.Find("Roof");
                    if (roofObject != null)
                    {
                        MeshFilter meshFilter = roofObject.GetComponent<MeshFilter>();
                        MeshRenderer meshRenderer = roofObject.GetComponent<MeshRenderer>();
                        CompoundMeshDraft roofMesh = generator.GenerateRoofMesh(lsFoundationPolygonVertex, m_Config, facadesHeight);
                        Mesh mesh = roofMesh.ToMeshWithSubMeshes();
                        NormalSolver.RecalculateNormals(mesh, 0);
                        meshFilter.mesh = mesh;

                        var materials = new List<Material>();
                        foreach (MeshDraft draft in roofMesh)
                        {
                            materials.AddRange(draft.materials);
                        }
                        meshRenderer.materials = materials.ToArray();

                        if (!roofObject.TryGetComponent(out BoxCollider boxCollider))
                        {
                            boxCollider = roofObject.gameObject.AddComponent<BoxCollider>();
                        }

                        boxCollider.center = meshFilter.sharedMesh.bounds.center;
                        boxCollider.size = meshFilter.sharedMesh.bounds.size;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public override bool CanPlaceOnOtherSandboxObject()
        {
            // 建物であれば他のオブジェクトを配置することができる
            return true;
        }
    }
}
