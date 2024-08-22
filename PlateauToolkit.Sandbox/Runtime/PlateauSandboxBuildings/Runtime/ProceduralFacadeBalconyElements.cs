using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using ProceduralToolkit;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime
{
    public abstract class ProceduralFacadeBalconyElement : ProceduralFacadeElement
    {
        public class BalconyColorData
        {
            public bool m_HasGlassWall;
            public Directions m_BalconyOuterFrameDirections;
            public Color m_WallColor;
            public Material m_VertexWallMat;
        }

        public class BalconyTexturedData
        {
            public bool m_HasGlassWall;
            public Directions m_BalconyOuterFrameDirections;
            public Vector2 m_UVScale;
            public Material m_WallMat;
        }

        protected static CompoundMeshDraft Balcony(
            Vector3 origin,
            float width,
            float height,
            BalconyColorData balconyColorData,
            ProceduralFacadeWindowElement.WindowColorData windowColorData
            )
        {
            Vector3 widthVector = Vector3.right*width;
            Vector3 heightVector = Vector3.up*height;
            Vector3 innerHeightOffset = Vector3.up*k_BalconyThickness;
            Vector3 windowHeightOffset = Vector3.up*k_WindowBottomOffset;
            Vector3 windowWidth = Vector3.right*(width - k_WindowWidthOffset*2);
            Vector3 windowHeight = Vector3.up*(height - k_WindowBottomOffset - k_WindowTopOffset);
            Vector3 windowDepth = Vector3.forward*k_WindowDepth;

            int rodCount = Mathf.FloorToInt(windowWidth.magnitude/k_WindowSegmentMinWidth);
            Vector3 doorWidth = Vector3.right*windowWidth.magnitude/(rodCount + 1);
            Vector3 doorHeight = windowHeightOffset + windowHeight;

            Vector3 outerFrameOrigin = origin + Vector3.right*k_WindowWidthOffset + innerHeightOffset;
            var outerFrame = new List<Vector3>
            {
                outerFrameOrigin,
                outerFrameOrigin + doorWidth,
                outerFrameOrigin + doorWidth + windowHeightOffset,
                outerFrameOrigin + windowWidth + windowHeightOffset,
                outerFrameOrigin + windowWidth + doorHeight,
                outerFrameOrigin + doorHeight,
            };

            var compoundDraft = new CompoundMeshDraft
            {
                BalconyAssembly(origin, width, height, balconyColorData, windowColorData),
                BalconyWallPanel(origin, widthVector, heightVector, windowDepth, outerFrame, balconyColorData)
            };

            Vector3 windowpaneMin1 = outerFrame[0] + windowDepth;
            Vector3 windowpaneMin2 = outerFrame[2] + windowDepth;
            compoundDraft.Add(ProceduralFacadeWindowElement.WindowPane(windowpaneMin1, windowpaneMin1 + doorWidth + doorHeight, windowColorData));
            compoundDraft.Add(ProceduralFacadeWindowElement.WindowPane(windowpaneMin2, windowpaneMin2 + windowWidth - doorWidth + windowHeight, windowColorData));

            return compoundDraft;
        }

        protected static CompoundMeshDraft BalconyTextured(
            Vector3 origin,
            float width,
            float height,
            BalconyTexturedData balconyTexturedData,
            ProceduralFacadeWindowElement.WindowTexturedData windowTexturedData
            )
        {
            Vector3 widthVector = Vector3.right*width;
            Vector3 heightVector = Vector3.up*height;
            Vector3 innerHeightOffset = Vector3.up*k_BalconyThickness;
            Vector3 windowHeightOffset = Vector3.up*k_WindowBottomOffset;
            Vector3 windowWidth = Vector3.right*(width - k_WindowWidthOffset*2);
            Vector3 windowHeight = Vector3.up*(height - k_WindowBottomOffset - k_WindowTopOffset);
            Vector3 windowDepth = Vector3.forward*k_WindowDepth;

            int rodCount = Mathf.FloorToInt(windowWidth.magnitude/k_WindowSegmentMinWidth);
            Vector3 doorWidth = Vector3.right*windowWidth.magnitude/(rodCount + 1);
            Vector3 doorHeight = windowHeightOffset + windowHeight;

            Vector3 outerFrameOrigin = origin + Vector3.right*k_WindowWidthOffset + innerHeightOffset;
            var outerFrame = new List<Vector3>
            {
                outerFrameOrigin,
                outerFrameOrigin + doorWidth,
                outerFrameOrigin + doorWidth + windowHeightOffset,
                outerFrameOrigin + windowWidth + windowHeightOffset,
                outerFrameOrigin + windowWidth + doorHeight,
                outerFrameOrigin + doorHeight,
            };

            var compoundDraft = new CompoundMeshDraft
            {
                BalconyAssemblyTextured(origin, width, height, balconyTexturedData, windowTexturedData),
                BalconyWallPanelTextured(origin, widthVector, heightVector, windowDepth, outerFrame, balconyTexturedData)
            };

            Vector3 windowpaneMin1 = outerFrame[0] + windowDepth;
            Vector3 windowpaneMin2 = outerFrame[2] + windowDepth;
            compoundDraft.Add(ProceduralFacadeWindowElement.WindowPaneTextured(windowpaneMin1, windowpaneMin1 + doorWidth + doorHeight, windowTexturedData));
            compoundDraft.Add(ProceduralFacadeWindowElement.WindowPaneTextured(windowpaneMin2, windowpaneMin2 + windowWidth - doorWidth + windowHeight, windowTexturedData));

            return compoundDraft;
        }

        private static CompoundMeshDraft BalconyAssembly(Vector3 origin, float width, float height, BalconyColorData balconyColorData, ProceduralFacadeWindowElement.WindowColorData windowColorData)
        {
            Vector3 widthVector = Vector3.right * width;
            Vector3 heightVector = Vector3.up*height;
            Vector3 innerHeightOffset = Vector3.up * k_BalconyThickness;
            Vector3 balconyDepth = Vector3.forward * k_BalconyDepth;

            var compoundMeshDraft = new CompoundMeshDraft();
            var balconyMeshDraft = new MeshDraft { name = k_WallDraftName };
            MeshDraft balconyOuter = BalconyOuter(origin, widthVector, heightVector, balconyDepth, balconyColorData, out Vector3 balconyCenter);
            balconyMeshDraft.Add(balconyOuter);
            MeshDraft balconyInner = BalconyInner(widthVector, heightVector, balconyDepth, balconyCenter, balconyColorData, out Vector3 innerWidthOffset, out Vector3 innerWidth, out Vector3 innerDepth);
            balconyMeshDraft.Add(balconyInner);
            Vector3 min = origin - balconyDepth;
            Vector3 max = origin - balconyDepth + widthVector + heightVector;
            Vector3 innerMin = origin - balconyDepth + innerWidthOffset + innerHeightOffset;
            Vector3 innerMax = origin - balconyDepth + widthVector - innerWidthOffset + heightVector - innerHeightOffset;
            MeshDraft balconyBorder = PerforatedQuad(min, max, innerMin, innerMax, Vector2.zero);
            balconyBorder.Paint(balconyColorData.m_WallColor, balconyColorData.m_VertexWallMat);
            balconyMeshDraft.Add(balconyBorder);
            compoundMeshDraft.Add(balconyMeshDraft);

            Vector3 balconyInnerSize = innerMax - innerMin;
            Vector3 balconyInnerWidth = balconyInnerSize.ToVector3XZ();
            if (balconyColorData.m_HasGlassWall)
            {
                Vector3 windowpaneMax = innerMin + balconyInnerWidth + Vector3.up * k_BalconyGlassHeight;
                compoundMeshDraft.Add(ProceduralFacadeWindowElement.WindowPane(innerMin, windowpaneMax, windowColorData));
            }
            else
            {
                MeshDraft wall = MeshDraft.PartialBox(balconyInnerWidth, Vector3.forward * k_BalconyGlassDepth, Vector3.up * k_BalconyGlassHeight, Directions.All & ~Directions.Down & ~Directions.Left & ~Directions.Right)
                    .Move(innerMin + balconyInnerWidth * 0.5f + Vector3.up * (k_BalconyGlassHeight * 0.5f) + Vector3.forward * (k_BalconyGlassDepth * 0.5f))
                    .Paint(balconyColorData.m_WallColor, balconyColorData.m_VertexWallMat);
                wall.name = k_WallDraftName;
                compoundMeshDraft.Add(wall);
            }

            return compoundMeshDraft;
        }

        private static CompoundMeshDraft BalconyAssemblyTextured(Vector3 origin, float width, float height, BalconyTexturedData balconyTexturedData, ProceduralFacadeWindowElement.WindowTexturedData windowTexturedData)
        {
            Vector3 widthVector = Vector3.right * width;
            Vector3 heightVector = Vector3.up*height;
            Vector3 innerHeightOffset = Vector3.up * k_BalconyThickness;
            Vector3 balconyDepth = Vector3.forward * k_BalconyDepth;

            var compoundMeshDraft = new CompoundMeshDraft();
            var balconyMeshDraft = new MeshDraft { name = k_WallTexturedDraftName };
            MeshDraft balconyOuter = BalconyOuterTextured(origin, widthVector, heightVector, balconyDepth, balconyTexturedData, out Vector3 balconyCenter);
            balconyMeshDraft.Add(balconyOuter);
            MeshDraft balconyInner = BalconyInnerTextured(widthVector, heightVector, balconyDepth, balconyCenter, balconyTexturedData, out Vector3 innerWidthOffset, out Vector3 innerWidth, out Vector3 innerDepth);
            balconyMeshDraft.Add(balconyInner);
            Vector3 min = origin - balconyDepth;
            Vector3 max = origin - balconyDepth + widthVector + heightVector;
            Vector3 innerMin = origin - balconyDepth + innerWidthOffset + innerHeightOffset;
            Vector3 innerMax = origin - balconyDepth + widthVector - innerWidthOffset + heightVector - innerHeightOffset;
            MeshDraft balconyBorder = PerforatedQuad(min, max, innerMin, innerMax, Vector2.zero, true);
            balconyBorder.Paint(balconyTexturedData.m_WallMat);
            balconyMeshDraft.Add(balconyBorder);
            compoundMeshDraft.Add(balconyMeshDraft);

            Vector3 balconyInnerSize = innerMax - innerMin;
            Vector3 balconyInnerWidth = balconyInnerSize.ToVector3XZ();
            if (balconyTexturedData.m_HasGlassWall)
            {
                Vector3 windowpaneMax = innerMin + balconyInnerWidth + Vector3.up * k_BalconyGlassHeight;
                compoundMeshDraft.Add(ProceduralFacadeWindowElement.WindowPaneTextured(innerMin, windowpaneMax, windowTexturedData));
            }
            else
            {
                MeshDraft wall = MeshDraft.PartialBox(balconyInnerWidth, Vector3.forward * k_BalconyGlassDepth, Vector3.up * k_BalconyGlassHeight, Directions.All & ~Directions.Down & ~Directions.Left & ~Directions.Right, true)
                    .Move(innerMin + balconyInnerWidth * 0.5f + Vector3.up * (k_BalconyGlassHeight * 0.5f) + Vector3.forward * (k_BalconyGlassDepth * 0.5f))
                    .Paint(balconyTexturedData.m_WallMat);
                wall.name = k_WallTexturedDraftName;
                compoundMeshDraft.Add(wall);
            }

            return compoundMeshDraft;
        }

        private static MeshDraft BalconyOuter(Vector3 origin, Vector3 widthVector, Vector3 heightVector, Vector3 balconyDepth, BalconyColorData balconyColorData, out Vector3 balconyCenter)
        {
            balconyCenter = origin + widthVector / 2 - balconyDepth / 2 + heightVector / 2;

            return MeshDraft.PartialBox(widthVector, balconyDepth, heightVector, balconyColorData.m_BalconyOuterFrameDirections)
                .Move(balconyCenter)
                .Paint(balconyColorData.m_WallColor, balconyColorData.m_VertexWallMat);
        }

        private static MeshDraft BalconyOuterTextured(Vector3 origin, Vector3 widthVector, Vector3 heightVector, Vector3 balconyDepth, BalconyTexturedData balconyTexturedData, out Vector3 balconyCenter)
        {
            balconyCenter = origin + widthVector / 2 - balconyDepth / 2 + heightVector / 2;

            return MeshDraft.PartialBox(widthVector, balconyDepth, heightVector, balconyTexturedData.m_BalconyOuterFrameDirections, true)
                .Move(balconyCenter)
                .Paint(balconyTexturedData.m_WallMat);
        }

        private static MeshDraft BalconyInner(Vector3 widthVector, Vector3 balconyHeight, Vector3 balconyDepth, Vector3 balconyCenter, BalconyColorData balconyColorData, out Vector3 innerWidthOffset, out Vector3 innerWidth, out Vector3 innerDepth)
        {
            innerWidthOffset = Vector3.right * k_BalconyThickness;
            innerWidth = widthVector - innerWidthOffset * 2;
            Vector3 innerHeightOffset = Vector3.up * k_BalconyThickness;
            Vector3 innerHeight = balconyHeight - innerHeightOffset * 2;
            innerDepth = balconyDepth;

            return MeshDraft.PartialBox(innerWidth, balconyDepth, innerHeight, Directions.Left | Directions.Right | Directions.Down | Directions.Up)
                .FlipFaces()
                .Move(balconyCenter)
                .Paint(balconyColorData.m_WallColor, balconyColorData.m_VertexWallMat);
        }

        private static MeshDraft BalconyInnerTextured(Vector3 widthVector, Vector3 balconyHeight, Vector3 balconyDepth, Vector3 balconyCenter, BalconyTexturedData balconyTexturedData, out Vector3 innerWidthOffset, out Vector3 innerWidth, out Vector3 innerDepth)
        {
            innerWidthOffset = Vector3.right * k_BalconyThickness;
            innerWidth = widthVector - innerWidthOffset * 2;
            Vector3 innerHeightOffset = Vector3.up * k_BalconyThickness;
            Vector3 innerHeight = balconyHeight - innerHeightOffset * 2;
            innerDepth = balconyDepth;

            return MeshDraft.PartialBox(innerWidth, balconyDepth, innerHeight, Directions.Left | Directions.Right | Directions.Down | Directions.Up, true)
                .FlipFaces()
                .Move(balconyCenter)
                .Paint(balconyTexturedData.m_WallMat);
        }

        // private static MeshDraft BalconyBorder(Vector3 origin, Vector3 widthVector, Vector3 balconyHeight, Vector3 balconyDepth, Vector3 innerWidthOffset, Vector3 innerWidth, Vector3 innerDepth, BalconyColorData balconyColorData)
        // {
        //     Vector3 borderOrigin = origin + balconyHeight;
        //     Vector3 borderInnerOrigin = borderOrigin + innerWidthOffset;
        //     Vector3 innerHeightOffset = Vector3.up * k_BalconyThickness;
        //
        //     return new MeshDraft().AddTriangleStrip(new List<Vector3>
        //         {
        //             // borderOrigin,
        //             // borderInnerOrigin,
        //             borderOrigin - balconyDepth,
        //             borderInnerOrigin - innerDepth,
        //             borderOrigin - balconyDepth - balconyHeight,
        //             borderOrigin - balconyDepth - balconyHeight + innerHeightOffset + innerWidthOffset,
        //             borderOrigin - balconyDepth - balconyHeight + widthVector,
        //             borderOrigin - balconyDepth - balconyHeight + innerHeightOffset + widthVector - innerWidthOffset,
        //             // borderOrigin + widthVector  - balconyDepth,
        //             // borderOrigin + widthVector - innerWidthOffset - balconyDepth,
        //             // borderOrigin - balconyHeight + innerHeightOffset - balconyDepth + widthVector,
        //             // borderInnerOrigin - innerDepth + innerWidth,
        //             // borderOrigin + widthVector,
        //             // borderInnerOrigin + innerWidth
        //         })
        //         .Paint(balconyColorData.m_WallColor, balconyColorData.m_VertexWallMat);
        // }

        private static MeshDraft BalconyWallPanel(Vector3 origin, Vector3 widthVector, Vector3 heightVector, Vector3 windowDepth, List<Vector3> outerFrame, BalconyColorData balconyColorData)
        {
            MeshDraft wall = new MeshDraft {name = k_WallDraftName}
                .AddTriangleStrip(new List<Vector3>
                {
                    outerFrame[0],
                    origin,
                    outerFrame[5],
                    origin + heightVector,
                    outerFrame[4],
                    origin + widthVector + heightVector,
                    outerFrame[3],
                    origin + widthVector,
                    outerFrame[2],
                    origin + widthVector,
                    outerFrame[1]
                });

            var innerFrame = outerFrame.Select(vertex => vertex + windowDepth).ToList();
            wall.AddFlatQuadBand(innerFrame, outerFrame, Vector2.zero, false);
            wall.Paint(balconyColorData.m_WallColor, balconyColorData.m_VertexWallMat);

            return wall;
        }

        private static CompoundMeshDraft BalconyWallPanelTextured(Vector3 origin, Vector3 widthVector, Vector3 heightVector, Vector3 windowDepth, List<Vector3> outerFrame, BalconyTexturedData balconyTexturedData)
        {
            var compoundDraft = new CompoundMeshDraft();
            MeshDraft wall = new MeshDraft {name = k_WallTexturedDraftName}
                .AddTriangleStrip(new List<Vector3>
                {
                    outerFrame[0],
                    origin,
                    outerFrame[5],
                    origin + heightVector,
                    outerFrame[4],
                    origin + widthVector + heightVector,
                    outerFrame[3],
                    origin + widthVector,
                    outerFrame[2],
                    origin + widthVector,
                    outerFrame[1]
                }, new List<Vector2>
                {
                    new(outerFrame[0].x * balconyTexturedData.m_UVScale.x, outerFrame[0].y * balconyTexturedData.m_UVScale.y),
                    new(origin.x * balconyTexturedData.m_UVScale.x, origin.y * balconyTexturedData.m_UVScale.y),
                    new(outerFrame[5].x * balconyTexturedData.m_UVScale.x, outerFrame[5].y * balconyTexturedData.m_UVScale.y),
                    new((origin.x + heightVector.x) * balconyTexturedData.m_UVScale.x, (origin.y + heightVector.y) * balconyTexturedData.m_UVScale.y),
                    new(outerFrame[4].x * balconyTexturedData.m_UVScale.x, outerFrame[4].y * balconyTexturedData.m_UVScale.y),
                    new((origin.x + widthVector.x + heightVector.x) * balconyTexturedData.m_UVScale.x, (origin.y + widthVector.y + heightVector.y) * balconyTexturedData.m_UVScale.y),
                    new(outerFrame[3].x * balconyTexturedData.m_UVScale.x, outerFrame[3].y * balconyTexturedData.m_UVScale.y),
                    new((origin.x + widthVector.x) * balconyTexturedData.m_UVScale.x, (origin.y + widthVector.y) * balconyTexturedData.m_UVScale.y),
                    new(outerFrame[2].x * balconyTexturedData.m_UVScale.x, outerFrame[2].y * balconyTexturedData.m_UVScale.y),
                    new((origin.x + widthVector.x) * balconyTexturedData.m_UVScale.x, (origin.y + widthVector.y) * balconyTexturedData.m_UVScale.y),
                    new(outerFrame[1].x * balconyTexturedData.m_UVScale.x, outerFrame[1].y * balconyTexturedData.m_UVScale.y),
                })
                .Paint(balconyTexturedData.m_WallMat);
            compoundDraft.Add(wall);

            var balconyWindowFrame = new MeshDraft { name = k_WallTexturedDraftName };
            var innerFrame = outerFrame.Select(vertex => vertex + windowDepth).ToList();
            balconyWindowFrame.AddFlatQuadBand(innerFrame, outerFrame, balconyTexturedData.m_UVScale, true);
            balconyWindowFrame.Paint(balconyTexturedData.m_WallMat);
            compoundDraft.Add(balconyWindowFrame);

            return compoundDraft;
        }

        protected static CompoundMeshDraft BalconyGlazed(
            Vector3 origin,
            float width,
            float height,
            Color windowpaneGlassColor,
            Color windowpaneFrameRodsColor,
            Color windowpaneOuterFrameColor,
            Material vertexWindowpaneMat)
        {
            Vector3 widthVector = Vector3.right*width;
            Vector3 heightVector = Vector3.up*height;

            Vector3 balconyHeight = Vector3.up*k_BalconyGlassHeight;
            Vector3 balconyDepth = Vector3.forward*k_BalconyDepth;

            var compoundDraft = new CompoundMeshDraft();

            // MeshDraft balcony = BalconyOuter(origin, widthVector, balconyHeight, balconyDepth, vertexColorPalette, materialPalette, false, out Vector3 balconyCenter)
            //     .Paint(vertexColorPalette.wallColor);
            // balcony.name = k_WallDraftName;
            // compoundDraft.Add(balcony);
            //
            // MeshDraft roof = BalconyGlazedRoof(origin, widthVector, heightVector, balconyDepth, vertexColorPalette.roofColor);
            // compoundDraft.Add(roof);
            //
            // var glassHeight = new Vector3(0, height - k_BalconyHeight, 0);
            // Vector3 glass0 = origin + balconyHeight;
            // Vector3 glass1 = glass0 - balconyDepth;
            // Vector3 glass2 = glass1 + widthVector;
            //
            // compoundDraft.Add(Windowpane(glass0, glass1 + glassHeight, vertexColorPalette, materialPalette));
            // compoundDraft.Add(Windowpane(glass1, glass2 + glassHeight, vertexColorPalette, materialPalette));
            // compoundDraft.Add(Windowpane(glass2, glass2 + balconyDepth + glassHeight, vertexColorPalette, materialPalette));

            return compoundDraft;
        }

        private static MeshDraft BalconyGlazedRoof(
            Vector3 origin,
            Vector3 widthVector,
            Vector3 heightVector,
            Vector3 balconyDepth,
            Color roofColor)
        {
            Vector3 roof0 = origin + heightVector;
            Vector3 roof1 = roof0 + widthVector;
            Vector3 roof2 = roof1 - balconyDepth;
            Vector3 roof3 = roof0 - balconyDepth;
            var roof = new MeshDraft {name = k_WallDraftName}
                .AddQuad(roof0, roof1, roof2, roof3, Vector3.up)
                .Paint(roofColor);
            return roof;
        }
    }
}
