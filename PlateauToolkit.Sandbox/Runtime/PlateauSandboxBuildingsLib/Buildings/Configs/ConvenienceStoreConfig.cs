using ProceduralToolkit;
using System;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Configs
{
    public abstract class ConvenienceStoreConfig
    {
        [Serializable]
        public class Params
        {
            public const float k_BillboardHeight = 0.5f;

            [CustomLabel("側面を壁に設定")]
            public bool isSideWall = true;
            [CustomLabel("屋根の厚さ")]
            public float roofThickness = 0.2f;
        }

        [Serializable]
        public class VertexColorPalette
        {
            public Color socleColor = ColorE.silver;
            public Color wallColor = ColorE.white;
            public Color billboardColor = ColorE.white;
            public Color billboardBottomColor = ColorE.white;
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
                vertexWall = Resources.Load<Material>("ConvenienceStore/Wall");
                vertexWindowPane = Resources.Load<Material>("ConvenienceStore/WindowPane");
                vertexRoof = Resources.Load<Material>("Residence/Roof");
            }
        }

        [Serializable]
        public class MaterialPalette
        {
            public Material socle;
            public Material wall;
            public Material windowPane;
            public Material windowGlass;
            public Material billboard;
            public Material billboardBottom;
            public Material roof;
            public Material roofSide;

            public void LoadMaterial()
            {
                socle = Resources.Load<Material>("ConvenienceStore/SocleTextured");
                wall = Resources.Load<Material>("ConvenienceStore/WallTextured");
                windowPane = Resources.Load<Material>("ConvenienceStore/WindowPaneTextured");
                windowGlass = Resources.Load<Material>("ConvenienceStore/WindowGlassTextured");
                billboard = Resources.Load<Material>("ConvenienceStore/BillboardTextured");
                billboardBottom = Resources.Load<Material>("ConvenienceStore/BillboardBottomTextured");
                roof = Resources.Load<Material>("ConvenienceStore/RoofTextured");
                roofSide = Resources.Load<Material>("Residence/RoofSideTextured");
            }
        }
    }
}
