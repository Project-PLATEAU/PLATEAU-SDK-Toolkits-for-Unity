using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Editor;
using ProceduralToolkit;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Configs
{
    public abstract class HotelConfig
    {
        [Serializable]
        public class Params
        {
            [Label("左側に窓を付ける")]
            public bool hasWindowLeft;
            [Label("右側に窓を付ける")]
            public bool hasWindowRight;
            [Label("屋根の暑さ")]
            public float roofThickness;
        }

        [Serializable]
        public class ShaderParams
        {
            [Label("横のオフセット")]
            public float textureOffsetX;
            [Label("縦のオフセット")]
            public float textureOffsetY;
        }

        [Serializable]
        public class VertexColorPalette
        {
            public Color wallColor = ColorE.white;
            [FormerlySerializedAs("windowWallColor")]
            public Color windowTopAndBottomWallColor = ColorE.white;
            public Color windowPaneColor = ColorE.gray;
            public Color windowPaneGlassColor = ColorE.white;
            public Color roofColor = (ColorE.gray/4).WithA(1);
            public Color roofSideColor = (ColorE.gray/4).WithA(1);
            public Color roofSideFrontColor = (ColorE.gray/4).WithA(1);
        }

        [Serializable]
        public class VertexColorMaterialPalette
        {
            public Material vertexWall;
            public Material vertexWindowPane;
            public Material vertexRoof;

            public void LoadMaterial()
            {
                vertexWall = Resources.Load<Material>("Hotel/Wall");
                vertexWindowPane = Resources.Load<Material>("Hotel/WindowPane");
                vertexRoof = Resources.Load<Material>("Hotel/Roof");
            }
        }

        [Serializable]
        public class MaterialPalette
        {
            public Material wall;
            [FormerlySerializedAs("windowTopAndBottomWallTextured")]
            [FormerlySerializedAs("windowWallTextured")]
            public Material windowTopAndBottomWall;
            public Material windowPane;
            public Material windowGlass;
            public Material roof;
            public Material roofSide;
            public Material roofSideFront;

            public void LoadMaterial()
            {
                wall = Resources.Load<Material>("Hotel/WallTextured");
                windowTopAndBottomWall = Resources.Load<Material>("Hotel/WindowTopAndBottomWallTextured");
                windowPane = Resources.Load<Material>("Hotel/WindowPaneTextured");
                windowGlass = Resources.Load<Material>("Hotel/WindowGlassTextured");
                roof = Resources.Load<Material>("Hotel/RoofTextured");
                roofSide = Resources.Load<Material>("Hotel/RoofSideTextured");
                roofSideFront = Resources.Load<Material>("Hotel/FixedTexture");
            }
        }
    }
}
