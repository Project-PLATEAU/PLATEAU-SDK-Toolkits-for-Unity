using ProceduralToolkit;
using System;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Configs
{
    public abstract class CommercialFacilityConfig
    {
        [Serializable]
        public class Params
        {
            // [CustomLabel("Reload")]
            // public bool reload;
        }

        [Serializable]
        public class VertexColorPalette
        {
            public Color wallColor = ColorE.white;
            public Color depressionWallColor = ColorE.white;
            public Color windowPaneColor = ColorE.gray;
            public Color windowPaneGlassColor = ColorE.white;
            public Color roofColor = (ColorE.gray/4).WithA(1);
            public Color roofSideColor = (ColorE.gray/4).WithA(1);
        }

        [Serializable]
        public class VertexColorMaterialPalette
        {
            public Material vertexWall;
            public Material vertexWindowPane;
            public Material vertexRoof;

            public void LoadMaterial()
            {
                vertexWall = Resources.Load<Material>("CommercialFacility/Wall");
                vertexWindowPane = Resources.Load<Material>("CommercialFacility/WindowPane");
                vertexRoof = Resources.Load<Material>("CommercialFacility/Roof");
            }
        }

        [Serializable]
        public class MaterialPalette
        {
            public Material wall;
            public Material depressionWall;
            public Material windowPane;
            public Material windowGlass;
            public Material roof;
            public Material roofSide;

            public void LoadMaterial()
            {
                wall = Resources.Load<Material>("CommercialFacility/WallTextured");
                depressionWall = Resources.Load<Material>("CommercialFacility/DepressionWallTextured");
                windowPane = Resources.Load<Material>("CommercialFacility/WindowPaneTextured");
                windowGlass = Resources.Load<Material>("CommercialFacility/WindowGlassTextured");
                roof = Resources.Load<Material>("CommercialFacility/RoofTextured");
                roofSide = Resources.Load<Material>("CommercialFacility/RoofSideTextured");
            }
        }
    }
}
