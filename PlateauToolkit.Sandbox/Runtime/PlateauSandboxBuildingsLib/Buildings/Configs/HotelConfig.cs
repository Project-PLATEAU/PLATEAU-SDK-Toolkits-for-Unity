using ProceduralToolkit;
using System;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Configs
{
    public abstract class HotelConfig
    {
        [Serializable]
        public class Params
        {
            [CustomLabel("左側にバルコニーを作成")]
            public bool hasBalconyLeft;
            [CustomLabel("右側にバルコニーを作成")]
            public bool hasBalconyRight;
        }

        [Serializable]
        public class VertexColorPalette
        {
            public Color wallColor = ColorE.white;
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
                vertexWall = Resources.Load<Material>("SkyscraperCondominium/Wall");
                vertexWindowPane = Resources.Load<Material>("SkyscraperCondominium/WindowPane");
                vertexRoof = Resources.Load<Material>("SkyscraperCondominium/Roof");
            }
        }

        [Serializable]
        public class MaterialPalette
        {
            public Material wall;
            public Material windowPane;
            public Material windowGlass;
            public Material roof;
            public Material roofSide;

            public void LoadMaterial()
            {
                wall = Resources.Load<Material>("SkyscraperCondominium/WallTextured");
                windowPane = Resources.Load<Material>("SkyscraperCondominium/WindowPaneTextured");
                windowGlass = Resources.Load<Material>("SkyscraperCondominium/WindowGlassTextured");
                roof = Resources.Load<Material>("SkyscraperCondominium/RoofTextured");
                roofSide = Resources.Load<Material>("SkyscraperCondominium/RoofSideTextured");
            }
        }
    }
}
