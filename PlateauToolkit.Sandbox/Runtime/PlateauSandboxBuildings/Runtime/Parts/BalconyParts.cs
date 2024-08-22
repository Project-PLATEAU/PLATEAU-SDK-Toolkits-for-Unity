using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using ProceduralToolkit;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime.Parts
{
    [CustomEditor(typeof(BalconyParts))]
    public class BalconyPartsEditor : Editor
    {
        BaseParts.BalconyType m_PartsTypeIndex = 0;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            m_PartsTypeIndex = (BaseParts.BalconyType)EditorGUILayout.EnumPopup("パーツタイプ", m_PartsTypeIndex);

            if (GUILayout.Button("Build Mesh"))
            {
                var parts = (BalconyParts)target;
                parts.BuildMesh(m_PartsTypeIndex);
            }
        }
    }

    public class BalconyParts : BaseParts
    {
        public static CompoundMeshDraft BalconyGlazed(
            Vector3 origin,
            float width,
            float height,
            Color wallColor,
            Color frameColor,
            Color glassColor,
            Color roofColor)
        {
            Vector3 widthVector = Vector3.right*width;
            Vector3 heightVector = Vector3.up*height;

            Vector3 balconyHeight = Vector3.up*BalconyHeight;
            Vector3 balconyDepth = Vector3.forward*BalconyDepth;

            var compoundDraft = new CompoundMeshDraft();

            MeshDraft balcony = BalconyOuter(origin, widthVector, balconyHeight, balconyDepth, out Vector3 balconyCenter)
                .Paint(wallColor);
            balcony.name = WallDraftName;
            compoundDraft.Add(balcony);

            MeshDraft roof = BalconyGlazedRoof(origin, widthVector, heightVector, balconyDepth, roofColor);
            compoundDraft.Add(roof);

            Vector3 glassHeight = new Vector3(0, height - BalconyHeight, 0);
            Vector3 glass0 = origin + balconyHeight;
            Vector3 glass1 = glass0 - balconyDepth;
            Vector3 glass2 = glass1 + widthVector;

            compoundDraft.Add(Windowpane(glass0, glass1 + glassHeight, frameColor, glassColor));
            compoundDraft.Add(Windowpane(glass1, glass2 + glassHeight, frameColor, glassColor));
            compoundDraft.Add(Windowpane(glass2, glass2 + balconyDepth + glassHeight, frameColor, glassColor));

            return compoundDraft;
        }

        private static MeshDraft BalconyGlazedRoof(Vector3 origin, Vector3 widthVector, Vector3 heightVector, Vector3 balconyDepth, Color roofColor)
        {
            Vector3 roof0 = origin + heightVector;
            Vector3 roof1 = roof0 + widthVector;
            Vector3 roof2 = roof1 - balconyDepth;
            Vector3 roof3 = roof0 - balconyDepth;
            var roof = new MeshDraft {name = WallDraftName}
                .AddQuad(roof0, roof1, roof2, roof3, Vector3.up)
                .Paint(roofColor);
            return roof;
        }

        public static CompoundMeshDraft Balcony(
            Vector3 origin,
            float width,
            float height,
            Color wallColor,
            Color frameColor,
            Color glassColor)
        {
            Vector3 widthVector = Vector3.right*width;
            Vector3 heightVector = Vector3.up*height;

            var compoundDraft = new CompoundMeshDraft();
            compoundDraft.Add(Balcony(origin, width, wallColor));

            Vector3 innerHeightOffset = Vector3.up*BalconyThickness;

            Vector3 windowHeightOffset = Vector3.up*WindowBottomOffset;
            Vector3 windowWidth = Vector3.right*(width - WindowWidthOffset*2);
            Vector3 windowHeight = Vector3.up*(height - WindowBottomOffset - WindowTopOffset);
            Vector3 windowDepth = Vector3.forward*WindowDepth;

            int rodCount = Mathf.FloorToInt(windowWidth.magnitude/WindowSegmentMinWidth);
            Vector3 doorWidth = Vector3.right*windowWidth.magnitude/(rodCount + 1);
            Vector3 doorHeight = windowHeightOffset + windowHeight;

            Vector3 outerFrameOrigin = origin + Vector3.right*WindowWidthOffset + innerHeightOffset;
            var outerFrame = new List<Vector3>
            {
                outerFrameOrigin,
                outerFrameOrigin + doorWidth,
                outerFrameOrigin + doorWidth + windowHeightOffset,
                outerFrameOrigin + windowWidth + windowHeightOffset,
                outerFrameOrigin + windowWidth + doorHeight,
                outerFrameOrigin + doorHeight,
            };

            compoundDraft.Add(BalconyWallPanel(origin, widthVector, heightVector, windowDepth, outerFrame, wallColor));

            Vector3 windowpaneMin1 = outerFrame[0] + windowDepth;
            Vector3 windowpaneMin2 = outerFrame[2] + windowDepth;

            compoundDraft.Add(Windowpane(windowpaneMin1, windowpaneMin1 + doorWidth + doorHeight, frameColor, glassColor));
            compoundDraft.Add(Windowpane(windowpaneMin2, windowpaneMin2 + windowWidth - doorWidth + windowHeight, frameColor, glassColor));

            return compoundDraft;
        }

        private static MeshDraft Balcony(Vector3 origin, float width, Color wallColor)
        {
            Vector3 widthVector = Vector3.right*width;
            Vector3 balconyHeight = Vector3.up*BalconyHeight;
            Vector3 balconyDepth = Vector3.forward*BalconyDepth;

            var balconyOuter = BalconyOuter(origin, widthVector, balconyHeight, balconyDepth, out Vector3 balconyCenter);
            var balconyInner = BalconyInner(widthVector, balconyHeight, balconyDepth, balconyCenter,
                out Vector3 innerWidthOffset, out Vector3 innerWidth, out Vector3 innerDepth);

            var balconyBorder = BalconyBorder(origin, widthVector, balconyHeight, balconyDepth, innerWidthOffset, innerWidth, innerDepth);

            return new MeshDraft {name = WallDraftName}
                .Add(balconyOuter)
                .Add(balconyInner)
                .Add(balconyBorder)
                .Paint(wallColor);
        }

        private static MeshDraft BalconyOuter(Vector3 origin, Vector3 widthVector, Vector3 balconyHeight, Vector3 balconyDepth,
            out Vector3 balconyCenter)
        {
            balconyCenter = origin + widthVector/2 - balconyDepth/2 + balconyHeight/2;
            return MeshDraft.PartialBox(widthVector, balconyDepth, balconyHeight,
                    Directions.All & ~Directions.Up & ~Directions.Forward, false)
                .Move(balconyCenter);
        }

        private static MeshDraft BalconyInner(Vector3 widthVector, Vector3 balconyHeight, Vector3 balconyDepth, Vector3 balconyCenter,
            out Vector3 innerWidthOffset, out Vector3 innerWidth, out Vector3 innerDepth)
        {
            innerWidthOffset = Vector3.right*BalconyThickness;
            innerWidth = widthVector - innerWidthOffset*2;
            Vector3 innerHeightOffset = Vector3.up*BalconyThickness;
            Vector3 innerHeight = balconyHeight - innerHeightOffset;
            Vector3 innerDepthOffset = Vector3.forward*BalconyThickness;
            innerDepth = balconyDepth - innerDepthOffset;
            return MeshDraft.PartialBox(innerWidth, innerDepth, innerHeight,
                    Directions.All & ~Directions.Up & ~Directions.Forward)
                .FlipFaces()
                .Move(balconyCenter + innerDepthOffset/2 + innerHeightOffset/2);
        }

        private static MeshDraft BalconyBorder(Vector3 origin, Vector3 widthVector, Vector3 balconyHeight, Vector3 balconyDepth,
            Vector3 innerWidthOffset, Vector3 innerWidth, Vector3 innerDepth)
        {
            Vector3 borderOrigin = origin + balconyHeight;
            Vector3 borderInnerOrigin = borderOrigin + innerWidthOffset;
            return new MeshDraft().AddTriangleStrip(new List<Vector3>
            {
                borderOrigin,
                borderInnerOrigin,
                borderOrigin - balconyDepth,
                borderInnerOrigin - innerDepth,
                borderOrigin - balconyDepth + widthVector,
                borderInnerOrigin - innerDepth + innerWidth,
                borderOrigin + widthVector,
                borderInnerOrigin + innerWidth
            });
        }

        private static MeshDraft BalconyWallPanel(Vector3 origin, Vector3 widthVector, Vector3 heightVector, Vector3 windowDepth,
            List<Vector3> outerFrame, Color wallColor)
        {
            MeshDraft wall = new MeshDraft {name = WallDraftName}
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

            var innerFrame = new List<Vector3>();
            foreach (Vector3 vertex in outerFrame)
            {
                innerFrame.Add(vertex + windowDepth);
            }
            wall.AddFlatQuadBand(innerFrame, outerFrame, Vector2.zero, false);
            wall.Paint(wallColor);
            return wall;
        }

        private static CompoundMeshDraft Windowpane(Vector3 min, Vector3 max, Color frameColor, Color glassColor)
        {
            MeshDraft frame = WindowpaneFrame(min, max, frameColor, out Vector3 frameDepth, out Vector3 windowMin, out Vector3 windowWidth, out Vector3 windowHeight);
            MeshDraft glass = WindowpaneGlass(frameDepth, windowMin, windowWidth, windowHeight, glassColor);
            return new CompoundMeshDraft().Add(frame).Add(glass);
        }

        private static MeshDraft WindowpaneFrame(Vector3 min, Vector3 max, Color frameColor,
            out Vector3 frameDepth, out Vector3 windowMin, out Vector3 windowWidth, out Vector3 windowHeight)
        {
            Vector3 size = max - min;
            Vector3 widthVector = size.ToVector3XZ();
            Vector3 heightVector = size.ToVector3Y();
            MeshDraft frame = WindowpaneFrameRods(min, widthVector, heightVector, out Vector3 frameWidth, out Vector3 frameHeight, out frameDepth, out Vector3 startPosition);

            windowMin = min + frameWidth + frameHeight;
            windowWidth = widthVector - frameWidth*2;
            windowHeight = heightVector - frameHeight*2;
            Vector3 windowMax = windowMin + windowWidth + windowHeight;

            MeshDraft frame2 = WindowpaneOuterFrame(min, max, widthVector, frameDepth, startPosition, windowMin, windowWidth, windowHeight,
                windowMax);
            frame.Add(frame2);
            frame.Paint(frameColor);
            return frame;
        }

        private static MeshDraft WindowpaneFrameRods(Vector3 min, Vector3 widthVector, Vector3 heightVector,
            out Vector3 frameWidth, out Vector3 frameHeight, out Vector3 frameDepth, out Vector3 startPosition)
        {
            var frame = new MeshDraft {name = WallDraftName};

            Vector3 right = widthVector.normalized;
            Vector3 normal = Vector3.Cross(heightVector, right).normalized;

            float width = widthVector.magnitude;
            int rodCount = Mathf.FloorToInt(width/WindowSegmentMinWidth);
            float interval = width/(rodCount + 1);

            frameWidth = right*WindowFrameWidth/2;
            frameHeight = Vector3.up*WindowFrameWidth/2;
            frameDepth = -normal*WindowFrameWidth/2;
            startPosition = min + heightVector/2 + frameDepth/2;
            for (int i = 0; i < rodCount; i++)
            {
                MeshDraft rod = MeshDraft.PartialBox(frameWidth*2, frameDepth, heightVector - frameHeight*2,
                        Directions.Left | Directions.Back | Directions.Right, false)
                    .Move(startPosition + right*(i + 1)*interval);
                frame.Add(rod);
            }
            return frame;
        }

        private static MeshDraft WindowpaneOuterFrame(Vector3 min, Vector3 max, Vector3 widthVector, Vector3 frameDepth, Vector3 startPosition,
            Vector3 windowMin, Vector3 windowWidth, Vector3 windowHeight, Vector3 windowMax)
        {
            var outerFrame = new MeshDraft();
            outerFrame.Add(PerforatedQuad(min, max, windowMin, windowMax));
            MeshDraft box = MeshDraft.PartialBox(windowWidth, frameDepth, windowHeight, Directions.All & ~Directions.ZAxis, false)
                .FlipFaces()
                .Move(startPosition + widthVector/2);
            outerFrame.Add(box);
            return outerFrame;
        }

        private static MeshDraft WindowpaneGlass(Vector3 frameDepth, Vector3 windowMin, Vector3 windowWidth, Vector3 windowHeight, Color glassColor)
        {
            return new MeshDraft {name = GlassDraftName}
                .AddQuad(windowMin + frameDepth, windowWidth, windowHeight, true)
                .Paint(glassColor);
        }

        private static MeshDraft PerforatedQuad(
            Vector3 min,
            Vector3 max,
            Vector3 innerMin,
            Vector3 innerMax)
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
            };
        }

        public void BuildMesh(BalconyType partsTypeIndex)
        {
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = Resources.Load<Material>("Wall");

            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            switch (partsTypeIndex)
            {
                case BalconyType.balconyOuter:
                {
                    Vector3 widthVector = Vector3.right*Width;
                    Vector3 balconyHeight = Vector3.up*BalconyHeight;
                    Vector3 balconyDepth = Vector3.forward*BalconyDepth;

                    MeshDraft balconyOuter = BalconyOuter(Vector3.zero, widthVector, balconyHeight, balconyDepth, out Vector3 balconyCenter);
                    balconyOuter.name = WallDraftName;
                    balconyOuter.Paint(WallColor);
                    meshFilter.mesh = balconyOuter.ToMesh();
                    break;
                }
                case BalconyType.balconyInner:
                {
                    Vector3 widthVector = Vector3.right*Width;
                    Vector3 balconyHeight = Vector3.up*BalconyHeight;
                    Vector3 balconyDepth = Vector3.forward*BalconyDepth;

                    MeshDraft balconyOuter = BalconyOuter(Vector3.zero, widthVector, balconyHeight, balconyDepth, out Vector3 balconyCenter);
                    MeshDraft balconyInner = BalconyInner(widthVector, balconyHeight, balconyDepth, balconyCenter, out Vector3 innerWidthOffset, out Vector3 innerWidth, out Vector3 innerDepth);
                    balconyInner.name = WallDraftName;
                    balconyInner.Paint(WallColor);
                    meshFilter.mesh = balconyInner.ToMesh();
                    break;
                }
                case BalconyType.balconyBorder:
                {
                    Vector3 widthVector = Vector3.right*Width;
                    Vector3 balconyHeight = Vector3.up*BalconyHeight;
                    Vector3 balconyDepth = Vector3.forward*BalconyDepth;

                    MeshDraft balconyOuter = BalconyOuter(Vector3.zero, widthVector, balconyHeight, balconyDepth, out Vector3 balconyCenter);
                    MeshDraft balconyInner = BalconyInner(widthVector, balconyHeight, balconyDepth, balconyCenter, out Vector3 innerWidthOffset, out Vector3 innerWidth, out Vector3 innerDepth);
                    MeshDraft balconyBorder = BalconyBorder(Vector3.zero, widthVector, balconyHeight, balconyDepth, innerWidthOffset, innerWidth, innerDepth);
                    balconyBorder.name = WallDraftName;
                    balconyBorder.Paint(WallColor);
                    meshFilter.mesh = balconyBorder.ToMesh();
                    break;
                }
                case BalconyType.balconyWall:
                {
                    Vector3 widthVector = Vector3.right*Width;
                    Vector3 heightVector = Vector3.up*Height;

                    Vector3 innerHeightOffset = Vector3.up*BalconyThickness;

                    Vector3 windowHeightOffset = Vector3.up*WindowBottomOffset;
                    Vector3 windowWidth = Vector3.right*(Width - WindowWidthOffset*2);
                    Vector3 windowHeight = Vector3.up*(Height - WindowBottomOffset - WindowTopOffset);

                    int rodCount = Mathf.FloorToInt(windowWidth.magnitude/WindowSegmentMinWidth);
                    Vector3 doorWidth = Vector3.right*windowWidth.magnitude/(rodCount + 1);
                    Vector3 doorHeight = windowHeightOffset + windowHeight;

                    Vector3 outerFrameOrigin = Vector3.zero + Vector3.right*WindowWidthOffset + innerHeightOffset;
                    var outerFrame = new List<Vector3>
                    {
                        outerFrameOrigin,
                        outerFrameOrigin + doorWidth,
                        outerFrameOrigin + doorWidth + windowHeightOffset,
                        outerFrameOrigin + windowWidth + windowHeightOffset,
                        outerFrameOrigin + windowWidth + doorHeight,
                        outerFrameOrigin + doorHeight,
                    };

                    MeshDraft wall = new MeshDraft {name = WallDraftName}
                        .AddTriangleStrip(new List<Vector3>
                        {
                            outerFrame[0],
                            Vector3.zero,
                            outerFrame[5],
                            Vector3.zero + heightVector,
                            outerFrame[4],
                            Vector3.zero + widthVector + heightVector,
                            outerFrame[3],
                            Vector3.zero + widthVector,
                            outerFrame[2],
                            Vector3.zero + widthVector,
                            outerFrame[1]
                        });
                    meshFilter.mesh = wall.ToMesh();
                    break;
                }
                case BalconyType.balconyWallInnerFrame:
                {
                    Vector3 innerHeightOffset = Vector3.up*BalconyThickness;

                    Vector3 windowHeightOffset = Vector3.up*WindowBottomOffset;
                    Vector3 windowWidth = Vector3.right*(Width - WindowWidthOffset*2);
                    Vector3 windowHeight = Vector3.up*(Height - WindowBottomOffset - WindowTopOffset);
                    Vector3 windowDepth = Vector3.forward*WindowDepth;

                    int rodCount = Mathf.FloorToInt(windowWidth.magnitude/WindowSegmentMinWidth);
                    Vector3 doorWidth = Vector3.right*windowWidth.magnitude/(rodCount + 1);
                    Vector3 doorHeight = windowHeightOffset + windowHeight;

                    Vector3 outerFrameOrigin = Vector3.zero + Vector3.right*WindowWidthOffset + innerHeightOffset;
                    var outerFrame = new List<Vector3>
                    {
                        outerFrameOrigin,
                        outerFrameOrigin + doorWidth,
                        outerFrameOrigin + doorWidth + windowHeightOffset,
                        outerFrameOrigin + windowWidth + windowHeightOffset,
                        outerFrameOrigin + windowWidth + doorHeight,
                        outerFrameOrigin + doorHeight,
                    };

                    var innerFrame = new List<Vector3>();
                    foreach (Vector3 vertex in outerFrame)
                    {
                        innerFrame.Add(vertex + windowDepth);
                    }

                    var wall = new MeshDraft { name = WallDraftName };
                    wall.AddFlatQuadBand(innerFrame, outerFrame, Vector2.zero, false);
                    wall.Paint(WallColor);
                    meshFilter.mesh = wall.ToMesh();
                    break;
                }
                case BalconyType.balconyWindow:
                {
                    var compoundDraft = new CompoundMeshDraft();

                    Vector3 innerHeightOffset = Vector3.up*BalconyThickness;

                    Vector3 windowHeightOffset = Vector3.up*WindowBottomOffset;
                    Vector3 windowWidth = Vector3.right*(Width - WindowWidthOffset*2);
                    Vector3 windowHeight = Vector3.up*(Height - WindowBottomOffset - WindowTopOffset);
                    Vector3 windowDepth = Vector3.forward*WindowDepth;

                    int rodCount = Mathf.FloorToInt(windowWidth.magnitude/WindowSegmentMinWidth);
                    Vector3 doorWidth = Vector3.right*windowWidth.magnitude/(rodCount + 1);
                    Vector3 doorHeight = windowHeightOffset + windowHeight;

                    Vector3 outerFrameOrigin = Vector3.zero + Vector3.right*WindowWidthOffset + innerHeightOffset;
                    var outerFrame = new List<Vector3>
                    {
                        outerFrameOrigin,
                        outerFrameOrigin + doorWidth,
                        outerFrameOrigin + doorWidth + windowHeightOffset,
                        outerFrameOrigin + windowWidth + windowHeightOffset,
                        outerFrameOrigin + windowWidth + doorHeight,
                        outerFrameOrigin + doorHeight,
                    };

                    Vector3 windowpaneMin1 = outerFrame[0] + windowDepth;

                    compoundDraft.Add(Windowpane(windowpaneMin1, windowpaneMin1 + doorWidth + doorHeight, FrameColor, GlassColor));
                    meshFilter.mesh = compoundDraft.ToMeshDraft().ToMesh();
                    break;
                }
                case BalconyType.balconySmallWindow:
                {
                    var compoundDraft = new CompoundMeshDraft();

                    Vector3 innerHeightOffset = Vector3.up*BalconyThickness;

                    Vector3 windowHeightOffset = Vector3.up*WindowBottomOffset;
                    Vector3 windowWidth = Vector3.right*(Width - WindowWidthOffset*2);
                    Vector3 windowHeight = Vector3.up*(Height - WindowBottomOffset - WindowTopOffset);
                    Vector3 windowDepth = Vector3.forward*WindowDepth;

                    int rodCount = Mathf.FloorToInt(windowWidth.magnitude/WindowSegmentMinWidth);
                    Vector3 doorWidth = Vector3.right*windowWidth.magnitude/(rodCount + 1);
                    Vector3 doorHeight = windowHeightOffset + windowHeight;

                    Vector3 outerFrameOrigin = Vector3.zero + Vector3.right*WindowWidthOffset + innerHeightOffset;
                    var outerFrame = new List<Vector3>
                    {
                        outerFrameOrigin,
                        outerFrameOrigin + doorWidth,
                        outerFrameOrigin + doorWidth + windowHeightOffset,
                        outerFrameOrigin + windowWidth + windowHeightOffset,
                        outerFrameOrigin + windowWidth + doorHeight,
                        outerFrameOrigin + doorHeight,
                    };

                    Vector3 windowpaneMin2 = outerFrame[2] + windowDepth;

                    compoundDraft.Add(Windowpane(windowpaneMin2, windowpaneMin2 + windowWidth - doorWidth + windowHeight, FrameColor, GlassColor));
                    meshFilter.mesh = compoundDraft.ToMeshDraft().ToMesh();
                    break;
                }
                case BalconyType.balconyAll:
                {
                    CompoundMeshDraft frame = Balcony(Vector3.zero, Width, Height, WallColor, FrameColor, GlassColor);
                    meshFilter.mesh = frame.ToMeshDraft().ToMesh();
                    break;
                }
                case BalconyType.balconyGrazedOuter:
                {
                    Vector3 widthVector = Vector3.right*Width;
                    Vector3 balconyHeight = Vector3.up*BalconyHeight;
                    Vector3 balconyDepth = Vector3.forward*BalconyDepth;
                    MeshDraft balcony = BalconyOuter(Vector3.zero, widthVector, balconyHeight, balconyDepth, out Vector3 balconyCenter)
                        .Paint(WallColor);
                    balcony.name = WallDraftName;
                    meshFilter.mesh = balcony.ToMesh();
                    break;
                }
                case BalconyType.balconyGlazedRoof:
                {
                    Vector3 widthVector = Vector3.right*Width;
                    Vector3 heightVector = Vector3.up*Height;
                    Vector3 balconyDepth = Vector3.forward*BalconyDepth;
                    MeshDraft roof = BalconyGlazedRoof(Vector3.zero, widthVector, heightVector, balconyDepth, RoofColor);
                    meshFilter.mesh = roof.ToMesh();
                    break;
                }
                case BalconyType.balconyGlazedWindow1:
                {
                    Vector3 widthVector = Vector3.right*Width;
                    Vector3 balconyHeight = Vector3.up*BalconyHeight;
                    Vector3 balconyDepth = Vector3.forward*BalconyDepth;
                    var compoundDraft = new CompoundMeshDraft();
                    Vector3 glassHeight = new Vector3(0, Height - BalconyHeight, 0);
                    Vector3 glass0 = Vector3.zero + balconyHeight;
                    Vector3 glass1 = glass0 - balconyDepth;
                    Vector3 glass2 = glass1 + widthVector;

                    compoundDraft.Add(Windowpane(glass0, glass1 + glassHeight, FrameColor, GlassColor));
                    meshFilter.mesh = compoundDraft.ToMeshDraft().ToMesh();
                    break;
                }
                case BalconyType.balconyGlazedWindow2:
                {
                    Vector3 widthVector = Vector3.right*Width;
                    Vector3 balconyHeight = Vector3.up*BalconyHeight;
                    Vector3 balconyDepth = Vector3.forward*BalconyDepth;
                    var compoundDraft = new CompoundMeshDraft();
                    Vector3 glassHeight = new Vector3(0, Height - BalconyHeight, 0);
                    Vector3 glass0 = Vector3.zero + balconyHeight;
                    Vector3 glass1 = glass0 - balconyDepth;
                    Vector3 glass2 = glass1 + widthVector;

                    compoundDraft.Add(Windowpane(glass1, glass2 + glassHeight, FrameColor, GlassColor));
                    meshFilter.mesh = compoundDraft.ToMeshDraft().ToMesh();
                    break;
                }
                case BalconyType.balconyGlazedWindow3:
                {
                    Vector3 widthVector = Vector3.right*Width;
                    Vector3 balconyHeight = Vector3.up*BalconyHeight;
                    Vector3 balconyDepth = Vector3.forward*BalconyDepth;
                    var compoundDraft = new CompoundMeshDraft();
                    Vector3 glassHeight = new Vector3(0, Height - BalconyHeight, 0);
                    Vector3 glass0 = Vector3.zero + balconyHeight;
                    Vector3 glass1 = glass0 - balconyDepth;
                    Vector3 glass2 = glass1 + widthVector;

                    compoundDraft.Add(Windowpane(glass2, glass2 + balconyDepth + glassHeight, FrameColor, GlassColor));
                    meshFilter.mesh = compoundDraft.ToMeshDraft().ToMesh();
                    break;
                }
                case BalconyType.balconyGrazedAll:
                {
                    CompoundMeshDraft frame = BalconyGlazed(Vector3.zero, Width, Height, WallColor, FrameColor, GlassColor, RoofColor);
                    meshFilter.mesh = frame.ToMeshDraft().ToMesh();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(partsTypeIndex), partsTypeIndex, null);
            }
        }
    }
}