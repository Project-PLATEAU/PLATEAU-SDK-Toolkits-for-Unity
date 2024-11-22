using ProceduralToolkit;
using System;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Configs
{
    public abstract class ApartmentConfig
    {
        [Serializable]
        public class Params
        {
            [CustomLabel("バルコニーを外壁にせり出す(1m)")]
            public bool convexBalcony;
            [CustomLabel("窓ガラスバルコニーに切り替え")]
            public bool hasBalconyGlass;
            [CustomLabel("左側にバルコニーを作成")]
            public bool hasBalconyLeft;
            [CustomLabel("右側にバルコニーを作成")]
            public bool hasBalconyRight;
            [CustomLabel("前側にバルコニーを作成")]
            public bool hasBalconyFront;
            [CustomLabel("後ろ側にバルコニーを作成")]
            public bool hasBalconyBack;
        }

        [Serializable]
        public class VertexColorPalette
        {
            public Color socleColor = ColorE.silver;
            public Color wallColor = ColorE.white;
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
            public Material windowFrame;
            public Material windowGlass;
            public Material roof;
            public Material roofSide;
        }
    }
}
