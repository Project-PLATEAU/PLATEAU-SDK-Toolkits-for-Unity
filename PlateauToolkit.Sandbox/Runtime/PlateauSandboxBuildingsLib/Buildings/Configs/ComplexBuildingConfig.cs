using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Editor;
using ProceduralToolkit;
using System;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Configs
{
    public abstract class ComplexBuildingConfig
    {
        public enum ComplexBuildingType
        {
            [EnumElement("マンション")]
            k_Apartment,
            [EnumElement("オフィスビル")]
            k_OfficeBuilding,
            [EnumElement("商業ビル")]
            k_CommercialBuilding,
        }

        public class BuildingPlannerParams
        {
            public bool m_AddedBoundaryWall = false;
        }

        [Serializable]
        public class Params
        {
            [EnumElementUsage("上部の建造物タイプ", typeof(ComplexBuildingType))]
            public ComplexBuildingType higherFloorBuildingType = ComplexBuildingType.k_OfficeBuilding;
            [EnumElementUsage("下部の建造物タイプ", typeof(ComplexBuildingType))]
            public ComplexBuildingType lowerFloorBuildingType = ComplexBuildingType.k_CommercialBuilding;
            public float buildingBoundaryHeight = 15.0f;
        }

        [Serializable]
        public class SkyscraperCondominiumParams
        {
            [Label("バルコニーを外壁にせり出す(1m)")]
            public bool convexBalcony;
            [Label("窓ガラスバルコニーに切り替え")]
            public bool hasBalconyGlass;
            [Label("左側にバルコニーを作成")]
            public bool hasBalconyLeft;
            [Label("右側にバルコニーを作成")]
            public bool hasBalconyRight;
            [Label("前側にバルコニーを作成")]
            public bool hasBalconyFront;
            [Label("後ろ側にバルコニーを作成")]
            public bool hasBalconyBack;
        }

        [Serializable]
        public class OfficeParams
        {
            public float spandrelHeight = 1.25f;
        }

        [Serializable]
        public class VertexColorPalette
        {
            public Color boundaryWallColor = ColorE.gray;

            public Color apartmentWallColor = ColorE.white;
            public Color apartmentWindowFrameColor = ColorE.gray;
            public Color apartmentWindowGlassColor = ColorE.white;
            public Color apartmentRoofColor = (ColorE.gray/4).WithA(1);
            public Color apartmentRoofSideColor = (ColorE.gray/4).WithA(1);

            public Color officeBuildingWallColor = ColorE.white;
            public Color officeBuildingWindowFrameColor = ColorE.gray;
            public Color officeBuildingWindowGlassColor = ColorE.white;
            public Color officeBuildingSpandrelColor = ColorE.white;
            public Color officeBuildingRoofColor = (ColorE.gray/4).WithA(1);
            public Color officeBuildingRoofSideColor = (ColorE.gray/4).WithA(1);

            public Color commercialBuildingWallColor = ColorE.white;
            public Color commercialBuildingDepressionWallColor = ColorE.white;
            public Color commercialBuildingWindowFrameColor = ColorE.gray;
            public Color commercialBuildingWindowGlassColor = ColorE.white;
            public Color commercialBuildingRoofColor = (ColorE.gray/4).WithA(1);
            public Color commercialBuildingRoofSideColor = (ColorE.gray/4).WithA(1);
        }


        [Serializable]
        public class VertexColorMaterialPalette
        {
            public Material vertexWall;
            public Material vertexWindow;
            public Material vertexRoof;
        }

        [Serializable]
        public class MaterialPalette
        {
            public Material boundaryWall;

            public Material apartmentWall;
            public Material apartmentWindowFrame;
            public Material apartmentWindowGlass;
            public Material apartmentRoof;
            public Material apartmentRoofSide;

            public Material officeBuildingWall;
            public Material officeBuildingWindowFrame;
            public Material officeBuildingWindowGlass;
            public Material officeBuildingSpandrel;
            public Material officeBuildingRoof;
            public Material officeBuildingRoofSide;

            public Material commercialBuildingWall;
            public Material commercialBuildingDepressionWall;
            public Material commercialBuildingWindowFrame;
            public Material commercialBuildingWindowGlass;
            public Material commercialBuildingRoof;
            public Material commercialBuildingRoofSide;
        }
    }
}
