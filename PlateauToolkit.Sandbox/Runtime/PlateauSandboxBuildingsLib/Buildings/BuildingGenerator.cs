using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Configs;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings
{
    public class BuildingGenerator
    {
        private IFacadePlanner m_FacadePlanner;
        private IFacadeConstructor m_FacadeConstructor;
        private IRoofPlanner m_RoofPlanner;
        private IRoofConstructor m_RoofConstructor;

        public void SetFacadePlanner(IFacadePlanner facadePlanner)
        {
            m_FacadePlanner = facadePlanner;
        }

        public void SetFacadeConstructor(IFacadeConstructor facadeConstructor)
        {
            m_FacadeConstructor = facadeConstructor;
        }

        public void SetRoofPlanner(IRoofPlanner roofPlanner)
        {
            m_RoofPlanner = roofPlanner;
        }

        public void SetRoofConstructor(IRoofConstructor roofConstructor)
        {
            m_RoofConstructor = roofConstructor;
        }

        /// <summary>
        /// Generates a new building from the input polygon and config.
        /// Foundation polygon vertices should be in clockwise order.
        /// Returns Transform of the generated building.
        /// </summary>
        /// <param name="foundationPolygon">Vertices of the foundation polygon in clockwise order.</param>
        /// <param name="config">Generator config.</param>
        /// <param name="parent">Parent transform of the generated building. Will be created if null.</param>
        public Transform Generate(List<Vector2> foundationPolygon, Config config, Transform parent = null)
        {
            Assert.IsTrue(config.entranceInterval > 0);

            List<ILayout> facadeLayouts = m_FacadePlanner.Plan(foundationPolygon, config);
            float height = facadeLayouts[0].height;

            if (parent == null)
            {
                parent = new GameObject("Building").transform;
            }
            m_FacadeConstructor.Construct(foundationPolygon, facadeLayouts, parent);

            if (m_RoofPlanner != null && m_RoofConstructor != null)
            {
                IConstructible<CompoundMeshDraft> roofConstructible = m_RoofPlanner.Plan(foundationPolygon, config);

                Transform roof = new GameObject("Roof").transform;
                roof.SetParent(parent, false);
                roof.SetLocalPositionAndRotation(new Vector3(0, height, 0), Quaternion.identity);
                m_RoofConstructor.Construct(roofConstructible, roof);
            }
            return parent;
        }

        public (CompoundMeshDraft, float) GenerateFacadesMesh(List<Vector2> foundationPolygon, Config config)
        {
            Assert.IsTrue(config.entranceInterval > 0);

            List<ILayout> facadeLayouts = m_FacadePlanner.Plan(foundationPolygon, config);
            CompoundMeshDraft facadesDraft = m_FacadeConstructor.BuildMesh(foundationPolygon, facadeLayouts);

            return (facadesDraft, facadeLayouts[0].height);
        }

        public CompoundMeshDraft GenerateRoofMesh(List<Vector2> foundationPolygon, Config config, float facadesHeight)
        {
            Assert.IsTrue(config.entranceInterval > 0);

            IConstructible<CompoundMeshDraft> roofConstructible = m_RoofPlanner.Plan(foundationPolygon, config);
            return m_RoofConstructor.BuildMesh(roofConstructible, facadesHeight);
        }

        [Serializable]
        public class Config
        {
            [Flags]
            public enum FaceDirection
            {
                k_Front   = 1 << 0,
                k_Left    = 1 << 1,
                k_Back    = 1 << 2,
                k_Right   = 1 << 3,
            }

            public FaceDirection faceDirection = FaceDirection.k_Front;
            public BuildingType buildingType = BuildingType.k_Apartment;
            public float entranceInterval = 12;
            public float buildingHeight = 5;
            public bool hasAttic = true;
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
            public HotelConfig.VertexColorPalette hotelVertexColorPalette = new();
            public HotelConfig.VertexColorMaterialPalette hotelVertexColorMaterialPalette = new();
            public HotelConfig.MaterialPalette hotelMaterialPalette = new();

            public FactoryConfig.Params factoryParams = new();
            public FactoryConfig.VertexColorPalette factoryVertexColorPalette = new();
            public FactoryConfig.VertexColorMaterialPalette factoryVertexColorMaterialPalette = new();
            public FactoryConfig.MaterialPalette factoryMaterialPalette = new();

            public Vector2 textureScale = new(0.1f, 0.1f);
            public int lodNum;
        }
    }

    public enum RoofType
    {
        flat,
        hipped,
        // gabled,
    }

    public enum BuildingType
    {
        k_Apartment,
        k_OfficeBuilding,
        k_House,
        k_ConvenienceStore,
        k_CommercialBuilding,
        k_Hotel,
        k_Factory
    }
}
