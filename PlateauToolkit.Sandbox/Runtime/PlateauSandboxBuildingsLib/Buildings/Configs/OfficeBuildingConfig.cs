using ProceduralToolkit;
using System;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Configs
{
    public abstract class OfficeBuildingConfig
    {
        [Serializable]
        public class Params
        {
            [CustomLabel("エントランスを窓に変更")]
            public bool entranceWindow = true;
            public float smallWindowHeight = 1.25f;
        }

        [Serializable]
        public class VertexColorPalette
        {
            public Color wallColor = ColorE.white;
            public Color windowPaneColor = ColorE.gray;
            public Color windowPaneGlassAColor = ColorE.white;
            public Color windowPaneGlassBColor = ColorE.white;
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
                vertexWall = Resources.Load<Material>("OfficeBuilding/Wall");
                vertexWindowPane = Resources.Load<Material>("OfficeBuilding/WindowPane");
                vertexRoof = Resources.Load<Material>("OfficeBuilding/Roof");
            }
        }

        [Serializable]
        public class MaterialPalette
        {
            public Material wall;
            public Material windowPane;
            public Material windowGlassA;
            public Material windowGlassB;
            public Material roof;
            public Material roofSide;

            public void LoadMaterial()
            {
                wall = Resources.Load<Material>("OfficeBuilding/WallTextured");
                windowPane = Resources.Load<Material>("OfficeBuilding/WindowPaneTextured");
                windowGlassA = Resources.Load<Material>("OfficeBuilding/WindowGlassATextured");
                windowGlassB = Resources.Load<Material>("OfficeBuilding/WindowGlassBTextured");
                roof = Resources.Load<Material>("OfficeBuilding/RoofTextured");
                roofSide = Resources.Load<Material>("OfficeBuilding/RoofSideTextured");
            }
        }
    }
}
