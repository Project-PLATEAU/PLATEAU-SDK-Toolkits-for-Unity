using ProceduralToolkit;
using System;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Configs
{
    public abstract class ResidenceConfig
    {
        [Serializable]
        public class Params
        {
            [CustomLabel("2階建てに変更")]
            public bool twoStoryBuilding;
            [CustomLabel("エントランスに屋根を追加")]
            public bool hasEntranceRoof;
            [CustomLabel("屋根タイプ")]
            public RoofType roofType = RoofType.flat;
            [CustomLabel("屋根の厚さ")]
            public float roofThickness = 0.2f;
        }

        [Serializable]
        public class VertexColorPalette
        {
            public Color socleColor = ColorE.silver;
            public Color socleTopColor = ColorE.black;
            public Color wallColor = ColorE.white;
            public Color entranceDoorColor = ColorE.white;
            public Color entranceDoorFrameColor = ColorE.white;
            public Color entranceDoorRoofColor = (ColorE.gray/4).WithA(1);
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
                vertexWall = Resources.Load<Material>("Residence/Wall");
                vertexWindowPane = Resources.Load<Material>("Residence/WindowPane");
                vertexRoof = Resources.Load<Material>("Residence/Roof");
            }
        }

        [Serializable]
        public class MaterialPalette
        {
            public Material socle;
            public Material socleTop;
            public Material wall;
            public Material entranceDoor;
            public Material entranceDoorFrame;
            public Material entranceDoorRoof;
            public Material windowPane;
            public Material windowGlass;
            public Material roof;
            public Material roofSide;

            public void LoadMaterial()
            {
                socle = Resources.Load<Material>("Residence/SocleTextured");
                socleTop = Resources.Load<Material>("Residence/SocleTopTextured");
                wall = Resources.Load<Material>("Residence/WallTextured");
                entranceDoor = Resources.Load<Material>("Residence/EntranceDoorTextured");
                entranceDoorFrame = Resources.Load<Material>("Residence/EntranceDoorFrameTextured");
                entranceDoorRoof = Resources.Load<Material>("Residence/EntranceDoorRoofTextured");
                windowPane = Resources.Load<Material>("Residence/WindowPaneTextured");
                windowGlass = Resources.Load<Material>("Residence/WindowGlassTextured");
                roof = Resources.Load<Material>("Residence/RoofTextured");
                roofSide = Resources.Load<Material>("Residence/RoofSideTextured");
            }
        }
    }
}
