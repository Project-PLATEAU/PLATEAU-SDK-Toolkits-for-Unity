using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using ProceduralToolkit;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime.Parts
{
    [CustomEditor(typeof(WindowParts))]
    public class WindowPartsEditor : UnityEditor.Editor
    {
        BaseParts.WindowType m_PartsTypeIndex = 0;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            m_PartsTypeIndex = (BaseParts.WindowType)EditorGUILayout.EnumPopup("パーツタイプ", m_PartsTypeIndex);

            if (GUILayout.Button("Build Mesh"))
            {
                var parts = (WindowParts)target;
                parts.BuildMesh(m_PartsTypeIndex);
            }
        }
    }

    public class WindowParts : BaseParts
    {
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

        public static CompoundMeshDraft Window(
            Vector3 min,
            float width,
            float height,
            float widthOffset,
            float bottomOffset,
            float topOffset,
            Color wallColor,
            Color frameColor,
            Color glassColor)
        {
            Vector3 widthVector = Vector3.right*width;
            Vector3 heightVector = Vector3.up*height;
            Vector3 max = min + widthVector + heightVector;
            Vector3 frameMin = min + Vector3.right*widthOffset + Vector3.up*bottomOffset;
            Vector3 frameMax = max - Vector3.right*widthOffset - Vector3.up*topOffset;
            Vector3 frameWidth = Vector3.right*(width - widthOffset*2);
            Vector3 frameHeight = Vector3.up*(height - bottomOffset - topOffset);
            Vector3 frameDepth = Vector3.forward*WindowDepth;
            Vector3 frameSize = frameMax - frameMin;

            MeshDraft frame = MeshDraft.PartialBox(frameWidth, frameDepth, frameHeight, Directions.All & ~Directions.ZAxis)
                .FlipFaces()
                .Move(frameMin + frameSize/2 + frameDepth/2)
                .Paint(wallColor);
            frame.name = WallDraftName;

            MeshDraft wall = PerforatedQuad(min, max, frameMin, frameMax)
                .Paint(wallColor);
            wall.name = WallDraftName;

            CompoundMeshDraft windowpane = Windowpane(frameMin + frameDepth, frameMax + frameDepth, frameColor, glassColor);
            CompoundMeshDraft compoundDraft = new CompoundMeshDraft().Add(frame).Add(wall).Add(windowpane);

            Vector3 windowsillWidth = frameWidth + Vector3.right*WindowsillWidthOffset;
            Vector3 windowsillDepth = Vector3.forward*WindowsillDepth;
            Vector3 windowsillHeight = Vector3.up*WindowsillThickness;
            MeshDraft windowsill = MeshDraft.PartialBox(windowsillWidth, windowsillDepth, windowsillHeight, Directions.All & ~Directions.Forward)
                .Move(frameMin + frameWidth/2 + frameDepth - windowsillDepth/2)
                .Paint(frameColor);
            windowsill.name = WallDraftName;
            compoundDraft.Add(windowsill);

            return compoundDraft;
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

        public void BuildMesh(WindowType partsTypeIndex)
        {
            Vector3 min = Vector3.zero;
            Vector3 widthVector = Vector3.right*Width;
            Vector3 heightVector = Vector3.up*Height;
            Vector3 max = min + widthVector + heightVector;
            Vector3 frameMin = min + Vector3.right*WindowWidthOffset + Vector3.up*WindowBottomOffset;
            Vector3 frameMax = max - Vector3.right*WindowWidthOffset - Vector3.up*WindowTopOffset;
            Vector3 frameWidth = Vector3.right*(Width - WindowWidthOffset*2);
            Vector3 frameHeight = Vector3.up*(Height - WindowBottomOffset - WindowTopOffset);
            Vector3 frameDepth = Vector3.forward*WindowDepth;
            Vector3 frameSize = frameMax - frameMin;

            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = Resources.Load<Material>("Wall");

            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            switch (partsTypeIndex)
            {
                case WindowType.windowPane:
                {
                    CompoundMeshDraft windowpane = Windowpane(frameMin + frameDepth, frameMax + frameDepth, FrameColor, GlassColor);
                    meshFilter.mesh = windowpane.ToMeshDraft().ToMesh();
                    break;
                }
                case WindowType.windowpaneFrame:
                {
                    MeshDraft frame = WindowpaneFrame(frameMin + frameDepth, frameMax + frameDepth, FrameColor, out Vector3 frameDepthDummy, out Vector3 windowMin, out Vector3 windowWidth, out Vector3 windowHeight);
                    meshFilter.mesh = frame.ToMesh();
                    break;
                }
                case WindowType.windowpaneOuterFrame:
                {
                    Vector3 inMin = frameMin + frameDepth;
                    Vector3 inMax = frameMax + frameDepth;
                    Vector3 size = inMax - inMin;
                    Vector3 inWidthVector = size.ToVector3XZ();
                    Vector3 inHeightVector = size.ToVector3Y();
                    MeshDraft frame = WindowpaneFrameRods(inMin, inWidthVector, inHeightVector, out Vector3 outFrameWidth, out Vector3 outFrameHeight, out frameDepth, out Vector3 startPosition);
                    meshFilter.mesh = frame.ToMesh();

                    Vector3 windowMin = inMin + outFrameWidth + outFrameHeight;
                    Vector3 windowWidth = inWidthVector - outFrameWidth*2;
                    Vector3 windowHeight = inHeightVector - outFrameHeight*2;
                    Vector3 windowMax = windowMin + windowWidth + windowHeight;

                    MeshDraft frame2 = WindowpaneOuterFrame(inMin, inMax, inWidthVector, frameDepth, startPosition, windowMin, windowWidth, windowHeight, windowMax);
                        frame.Paint(FrameColor);
                    meshFilter.mesh = frame2.ToMesh();
                    break;
                }
                case WindowType.windowpaneFrameRods:
                {
                    Vector3 inMin = frameMin + frameDepth;
                    Vector3 inMax = frameMax + frameDepth;
                    Vector3 size = inMax - inMin;
                    Vector3 inWidthVector = size.ToVector3XZ();
                    Vector3 inHeightVector = size.ToVector3Y();
                    MeshDraft frame = WindowpaneFrameRods(inMin, inWidthVector, inHeightVector, out Vector3 outFrameWidth, out Vector3 outFrameHeight, out frameDepth, out Vector3 startPosition);
                    meshFilter.mesh = frame.ToMesh();
                    break;
                }
                case WindowType.windowpaneGlass:
                {
                    MeshDraft frame = WindowpaneFrame(frameMin + frameDepth, frameMax + frameDepth, FrameColor, out Vector3 frameDepthDummy, out Vector3 windowMin, out Vector3 windowWidth, out Vector3 windowHeight);
                    MeshDraft glass = WindowpaneGlass(frameDepthDummy, windowMin, windowWidth, windowHeight, GlassColor);
                    meshFilter.mesh = glass.ToMesh();
                    break;
                }
                case WindowType.sill:
                {
                    Vector3 windowsillWidth = frameWidth + Vector3.right*WindowsillWidthOffset;
                    Vector3 windowsillDepth = Vector3.forward*WindowsillDepth;
                    Vector3 windowsillHeight = Vector3.up*WindowsillThickness;
                    MeshDraft windowsill = MeshDraft.PartialBox(windowsillWidth, windowsillDepth, windowsillHeight, Directions.All & ~Directions.Forward)
                        .Move(frameMin + frameWidth/2 + frameDepth - windowsillDepth/2)
                        .Paint(FrameColor);

                    meshFilter.mesh = windowsill.ToMesh();
                    break;
                }
                case WindowType.frame:
                {
                    MeshDraft frame = MeshDraft.PartialBox(frameWidth, frameDepth, frameHeight, Directions.All & ~Directions.ZAxis)
                        .FlipFaces()
                        .Move(frameMin + frameSize/2 + frameDepth/2);
                    meshFilter.mesh = frame.ToMesh();
                    break;
                }
                case WindowType.wall:
                {
                    MeshDraft wall = PerforatedQuad(min, max, frameMin, frameMax)
                        .Paint(WallColor);
                    meshFilter.mesh = wall.ToMesh();
                    break;
                }
                case WindowType.windowAll:
                {
                    CompoundMeshDraft windowpane = Windowpane(frameMin + frameDepth, frameMax + frameDepth, FrameColor, GlassColor);

                    MeshDraft frame = MeshDraft.PartialBox(frameWidth, frameDepth, frameHeight, Directions.All & ~Directions.ZAxis)
                        .FlipFaces()
                        .Move(frameMin + frameSize/2 + frameDepth/2)
                        .Paint(WallColor);

                    MeshDraft wall = PerforatedQuad(min, max, frameMin, frameMax)
                        .Paint(WallColor);

                    Vector3 windowsillWidth = frameWidth + Vector3.right*WindowsillWidthOffset;
                    Vector3 windowsillDepth = Vector3.forward*WindowsillDepth;
                    Vector3 windowsillHeight = Vector3.up*WindowsillThickness;
                    MeshDraft windowsill = MeshDraft.PartialBox(windowsillWidth, windowsillDepth, windowsillHeight, Directions.All & ~Directions.Forward)
                        .Move(frameMin + frameWidth/2 + frameDepth - windowsillDepth/2)
                        .Paint(FrameColor);

                    windowpane.Add(frame).Add(wall).Add(windowsill);
                    meshFilter.mesh = windowpane.ToMeshDraft().ToMesh();
                    break;
                }
                case WindowType.entranceWindow:
                {
                    CompoundMeshDraft compoundDraft = Window(Vector3.zero, Width, Height/2, EntranceWindowWidthOffset, EntranceWindowHeightOffset, EntranceWindowHeightOffset, WallColor, FrameColor, GlassColor);
                    // compoundDraft.Add(Window(Vector3.zero + Vector3.up*Height/2, Width, Height/2, EntranceWindowWidthOffset, EntranceWindowHeightOffset, EntranceWindowHeightOffset, WallColor, FrameColor, GlassColor));
                    meshFilter.mesh = compoundDraft.ToMeshDraft().ToMesh();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(partsTypeIndex), partsTypeIndex, null);
            }
        }
    }
}
