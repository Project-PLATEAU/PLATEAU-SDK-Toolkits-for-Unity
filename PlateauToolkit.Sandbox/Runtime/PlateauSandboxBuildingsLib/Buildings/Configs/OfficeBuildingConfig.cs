using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Editor;
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
            [Label("１階を窓に変更")]
            public bool useWindow = true;
            public float spandrelHeight = 1.25f;
        }

        [Serializable]
        public class VertexColorPalette
        {
            public Color wallColor = ColorE.white;
            public Color windowFrameColor = ColorE.gray;
            public Color windowGlassColor = ColorE.white;
            public Color spandrelColor = ColorE.white;
            public Color roofColor = (ColorE.gray/4).WithA(1);
            public Color roofSideColor = (ColorE.gray/4).WithA(1);
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
            public Material wall;
            public Material windowFrame;
            public Material windowGlass;
            public Material spandrel;
            public Material roof;
            public Material roofSide;
        }
    }
}
