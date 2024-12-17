using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Editor;
using ProceduralToolkit;
using System;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Configs
{
    public abstract class FactoryConfig
    {
        [Serializable]
        public class Params
        {
            [Label("シャッターに庇を追加")]
            public bool hasEntranceRoof;
        }

        [Serializable]
        public class VertexColorPalette
        {
            public Color socleColor = ColorE.white;
            public Color socleTopColor = ColorE.black;
            public Color wallColor = ColorE.white;
            public Color entranceShutter;
            public Color entranceShutterFrame;
            public Color entranceShutterRoof;
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
            public Material socleTop;
            public Material wall;
            public Material entranceShutter;
            public Material entranceShutterFrame;
            public Material entranceShutterRoof;
            public Material windowFrame;
            public Material windowGlass;
            public Material roof;
            public Material roofSide;
        }
    }
}
