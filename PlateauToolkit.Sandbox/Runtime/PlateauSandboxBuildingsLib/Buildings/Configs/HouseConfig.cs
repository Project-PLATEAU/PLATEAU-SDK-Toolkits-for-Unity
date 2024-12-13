using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Editor;
using ProceduralToolkit;
using System;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Configs
{
    public abstract class HouseConfig
    {
        [Serializable]
        public class Params
        {
            public int numFloor = 2;
            [Label("エントランスに庇を追加")]
            public bool hasEntranceRoof;
            [Label("屋根タイプ")]
            public RoofType roofType = RoofType.flat;
            [Label("屋根の厚さ")]
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
            public Material entranceDoor;
            public Material entranceDoorFrame;
            public Material entranceDoorRoof;
            public Material windowFrame;
            public Material windowGlass;
            public Material roof;
            public Material roofSide;
        }
    }
}
