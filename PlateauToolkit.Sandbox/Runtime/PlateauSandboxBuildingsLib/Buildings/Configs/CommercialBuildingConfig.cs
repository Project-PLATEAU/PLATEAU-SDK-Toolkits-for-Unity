using ProceduralToolkit;
using System;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Configs
{
    public abstract class CommercialBuildingConfig
    {
        [Serializable]
        public class Params
        {
        }

        [Serializable]
        public class VertexColorPalette
        {
            public Color wallColor = ColorE.white;
            public Color depressionWallColor = ColorE.white;
            public Color windowFrameColor = ColorE.gray;
            public Color windowGlassColor = ColorE.white;
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
            public Material depressionWall;
            public Material windowFrame;
            public Material windowGlass;
            public Material roof;
            public Material roofSide;
        }
    }
}
