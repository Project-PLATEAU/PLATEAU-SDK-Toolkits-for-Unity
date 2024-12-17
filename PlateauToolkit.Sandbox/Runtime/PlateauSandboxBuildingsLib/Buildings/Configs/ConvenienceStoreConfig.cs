using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Editor;
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

            [Label("側面を壁に設定")]
            public bool isSideWall = true;
            [Label("屋根の厚さ")]
            public float roofThickness = 0.2f;
        }

        [Serializable]
        public class VertexColorPalette
        {
            public Color socleColor = ColorE.silver;
            public Color wallColor = ColorE.white;
            public Color billboardColor = ColorE.white;
            public Color billboardBottomColor = ColorE.white;
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
            public Material socle;
            public Material wall;
            public Material windowFrame;
            public Material windowGlass;
            public Material billboard;
            public Material billboardBottom;
            public Material roof;
            public Material roofSide;
        }
    }
}
