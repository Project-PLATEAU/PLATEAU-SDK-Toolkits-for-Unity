using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces;
using ProceduralToolkit;
using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime
{
    public abstract class ProceduralFacadeElement : ILayoutElement, IConstructible<CompoundMeshDraft>
    {
        public Vector2 origin { get; set; }
        protected Vector2 UVScale { get; set; }
        public float width { get; set; }
        public float widthScale { get; set; } = 1.0f;
        public float height { get; set; }
        public float heightScale { get; set; } = 1.0f;
        protected bool UseTexture { get; set; }

        protected const string k_WallDraftName = "Wall";
        protected const string k_WallTexturedDraftName = "WallTextured";
        protected const string k_SocleTexturedDraftName = "SocleTextured";
        protected const string k_DepressionWallTexturedDraftName = "DepressionWallTextured";
        protected const string k_EntranceDoorDraftName = "EntranceDoor";
        protected const string k_EntranceDoorTexturedDraftName = "EntranceDoorTextured";
        protected const string k_EntranceDoorFrameTexturedDraftName = "EntranceDoorFrameTextured";
        protected const string k_EntranceDoorRoofTexturedDraftName = "EntranceDoorRoofTextured";
        protected const string k_GlassDraftName = "Glass";
        protected const string k_WindowPaneDraftName = "WindowPane";
        protected const string k_WindowPaneFrameTexturedDraftName = "WindowpaneFrameTextured";
        protected const string k_WindowPaneGlassTexturedDraftName = "WindowpaneGlassTextured";
        protected const string k_WindowSillTexturedDraftName = "WindowSillTextured";
        protected const string k_BillboardTexturedDraftName = "BillboardTextured";
        protected const string k_BillboardBottomTexturedDraftName = "BillboardBottomTextured";

        protected const float k_ConveniWindowFrameWidth = 0.15f;
        protected const float k_ConveniWindowFrameHeight = 0.15f;
        protected const float k_ConveniBillboardThickness = 0.25f;

        protected const float k_SocleWindowWidth = 0.7f;
        protected const float k_SocleWindowMinWidth = 0.9f;
        protected const float k_SocleWindowHeight = 0.4f;
        protected const float k_SocleWindowDepth = 0.1f;
        protected const float k_SocleWindowHeightOffset = 0.1f;

        protected const float k_EntranceDoorWidth = 1.8f;
        protected const float k_EntranceDoorHeight = 2;
        protected const float k_EntranceDoorThickness = 0.1f;

        protected const float k_EntranceDoorRoofDepth = 0.75f;
        protected const float k_EntranceDoorRoofHeight = 0.15f;

        protected const float k_EntranceWindowWidthOffset = 0.4f;
        protected const float k_EntranceWindowHeightOffset = 0.3f;

        protected const float k_WindowDepth = 0.1f;
        protected const float k_WindowWidthOffset = 0.5f;
        protected const float k_WindowBottomOffset = 0; // 値を変えると窓の高さが変わる
        protected const float k_WindowTopOffset = 0.3f;
        protected const float k_WindowFrameWidth = 0.05f;
        protected const float k_WindowFrameRodWidth = 0.05f;
        protected const float k_WindowFrameHeight = 0.05f;
        protected const float k_WindowFrameRodHeight = 0.05f;
        protected const float k_WindowFrameDepth = 0.05f;
        protected const float k_WindowFrameRodDepth = 0.05f;
        protected const float k_WindowSegmentMinWidth = 0.9f;
        protected const float k_WindowsillWidthOffset = 0.1f;
        protected const float k_WindowsillDepth = 0.15f;
        protected const float k_WindowsillThickness = 0.05f;

        protected const float k_BalconyConcaveDepth = 0.6f;
        protected const float k_BalconyConvexDepth = 1f;
        protected const float k_BalconyThickness = 0.1f;
        protected const float k_BalconyGlassDepth = 0.1f;
        protected const float k_BalconyGlassHeight = 1;

        protected const float k_AtticHoleWidth = 0.3f;
        protected const float k_AtticHoleMinWidth = 0.5f;
        protected const float k_AtticHoleHeight = 0.3f;
        protected const float k_AtticHoleDepth = 0.5f;

        public enum PositionType
        {
            k_NoLeft,
            k_NoRight,
            k_NoLeftRight,
            k_Middle,
        }

        public enum WindowFrameRodType
        {
            k_Vertical,
            k_Horizontal,
            k_Cross
        }

        public abstract CompoundMeshDraft Construct(Vector2 parentLayoutOrigin);

        protected static MeshDraft PerforatedQuad(
            Vector3 min,
            Vector3 max,
            Vector3 innerMin,
            Vector3 innerMax,
            Vector2 uvScale,
            bool generateUV = false)
        {
            Vector3 size = max - min;
            Vector3 widthVector = size.ToVector3XZ();
            Vector3 heightVector = size.ToVector3Y();
            Vector3 normal = Vector3.Cross(heightVector, widthVector).normalized;

            Vector3 innerSize = innerMax - innerMin;
            Vector3 innerHeight = innerSize.ToVector3Y();
            Vector3 innerWidth = innerSize.ToVector3XZ();

            Vector3 vertex0 = min;
            Vector3 vertex1 = min + heightVector;
            Vector3 vertex2 = max;
            Vector3 vertex3 = min + widthVector;
            Vector3 window0 = innerMin;
            Vector3 window1 = innerMin + innerHeight;
            Vector3 window2 = innerMax;
            Vector3 window3 = innerMin + innerWidth;

            var uv = new List<Vector2>(8);
            if (generateUV)
            {
                uv.Add(new Vector2(vertex0.x * uvScale.x, vertex0.y * uvScale.y));
                uv.Add(new Vector2(vertex1.x * uvScale.x, vertex1.y * uvScale.y));
                uv.Add(new Vector2(vertex2.x * uvScale.x, vertex2.y * uvScale.y));
                uv.Add(new Vector2(vertex3.x * uvScale.x, vertex3.y * uvScale.y));
                uv.Add(new Vector2(window0.x * uvScale.x, window0.y * uvScale.y));
                uv.Add(new Vector2(window1.x * uvScale.x, window1.y * uvScale.y));
                uv.Add(new Vector2(window2.x * uvScale.x, window2.y * uvScale.y));
                uv.Add(new Vector2(window3.x * uvScale.x, window3.y * uvScale.y));
            }

            return new MeshDraft
            {
                vertices = new List<Vector3>(8)
                {
                    vertex0,
                    vertex1,
                    vertex2,
                    vertex3,
                    window0,
                    window1,
                    window2,
                    window3,
                },
                normals = new List<Vector3>(8) {normal, normal, normal, normal, normal, normal, normal, normal},
                triangles = new List<int>(24) {0, 1, 4, 4, 1, 5, 1, 2, 5, 5, 2, 6, 2, 3, 6, 6, 3, 7, 3, 0, 7, 7, 0, 4},
                uv = uv
            };
        }
    }
}
