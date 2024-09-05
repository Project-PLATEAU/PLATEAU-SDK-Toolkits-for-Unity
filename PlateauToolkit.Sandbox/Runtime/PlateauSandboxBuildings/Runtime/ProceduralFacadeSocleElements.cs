using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using ProceduralToolkit;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime
{
    public abstract class ProceduralFacadeSocleElement : ProceduralFacadeElement
    {
        protected static CompoundMeshDraft SocleWindowed(Vector3 origin, float width, float height, Color wallColor, Color glassColor)
        {
            // if (width < k_SocleWindowMinWidth)
            // {
            //     return new CompoundMeshDraft().Add(Wall(origin, width, height, wallColor));
            // }

            Vector3 widthVector = Vector3.right*width;
            Vector3 heightVector = Vector3.up*height;

            Vector3 windowWidth = Vector3.right*k_SocleWindowWidth;
            Vector3 windowHeigth = Vector3.up*k_SocleWindowHeight;
            Vector3 windowDepth = Vector3.forward*k_SocleWindowDepth;
            Vector3 windowOrigin = origin + widthVector/2 - windowWidth/2 + Vector3.up*k_SocleWindowHeightOffset;
            Vector3 windowMax = windowOrigin + windowWidth + windowHeigth;

            var frame = MeshDraft.PartialBox(windowWidth, -windowDepth, windowHeigth, Directions.All & ~Directions.ZAxis, false)
                .Move(windowOrigin + windowWidth/2 + windowHeigth/2 + windowDepth/2);

            var wall = PerforatedQuad(origin, origin + widthVector + heightVector, windowOrigin, windowMax, Vector2.zero, generateUV:false)
                .Add(frame)
                .Paint(wallColor);
            wall.name = k_WallDraftName;

            var glass = new MeshDraft()
                .AddQuad(windowOrigin + windowDepth/2, windowWidth, windowHeigth, true)
                .Paint(glassColor);
            glass.name = k_GlassDraftName;

            return new CompoundMeshDraft().Add(wall).Add(glass);
        }
    }
}
