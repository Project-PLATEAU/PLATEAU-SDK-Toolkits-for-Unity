using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using ProceduralToolkit;
using System;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime
{
    public abstract class ProceduralFacadeWindowElement : ProceduralFacadeElement
    {
        public class WindowColorData
        {
            public Color m_WallColor;
            public Color m_WindowPaneColor;
            public Color m_WindowPaneGlassColor;
            public Material m_VertexWallMat;
            public Material m_VertexWindowPaneMat;
        }

        public class WindowTexturedData
        {
            public string m_WindowpaneGlassName = k_WindowPaneTexturedDraftName;
            public Vector2 m_UVScale;
            public Material m_WallMat;
            public Material m_WindowGlassMat;
            public Material m_WindowPaneMat;
        }

        protected static CompoundMeshDraft Window(
            Vector3 min,
            float width,
            float height,
            float windowWidthOffset,
            float windowBottomOffset,
            float windowTopOffset,
            float windowDepthOffset,
            float windowFrameRodWidth,
            float windowFrameRodHeight,
            float windowFrameRodDepth,
            int numCenterRod,
            WindowFrameRodType windowFrameRodType,
            WindowColorData windowColorData,
            bool hasWindowsill,
            bool isRectangleWindow
            )
        {
            Vector3 widthVector = Vector3.right * width;
            Vector3 heightVector = Vector3.up * height;
            Vector3 max = min + widthVector + heightVector;
            Vector3 frameMin = min + Vector3.right * windowWidthOffset + Vector3.up * windowBottomOffset;
            Vector3 frameMax = max - Vector3.right * windowWidthOffset - Vector3.up * windowTopOffset;
            Vector3 frameWidth = Vector3.right * (width - windowWidthOffset * 2);
            Vector3 frameHeight = Vector3.up * (height - windowBottomOffset - windowTopOffset);
            Vector3 frameDepth = Vector3.forward * windowDepthOffset;
            Vector3 frameSize = frameMax - frameMin;

            if (isRectangleWindow)
            {
                float widthOffset;
                float heightOffset;
                if (width < height)
                {
                    widthOffset = width * 0.2f;
                    heightOffset = (height - width) * 0.5f + widthOffset;
                }
                else
                {
                    heightOffset = height * 0.2f;
                    widthOffset = (width - height) * 0.5f + heightOffset;
                }

                frameMin = min + Vector3.right * widthOffset + Vector3.up * heightOffset;
                frameMax = max - Vector3.right * widthOffset - Vector3.up * heightOffset;
                frameWidth = Vector3.right * (width - widthOffset * 2);
                frameHeight = Vector3.up * (height - heightOffset * 2);
                frameSize = frameMax - frameMin;
            }

            MeshDraft frame = MeshDraft.PartialBox(frameWidth, frameDepth, frameHeight, Directions.All & ~Directions.ZAxis)
                .FlipFaces()
                .Move(frameMin + frameSize / 2 + frameDepth / 2)
                .Paint(windowColorData.m_WindowPaneColor, windowColorData.m_VertexWindowPaneMat);
            frame.name = k_WindowPaneDraftName;

            MeshDraft wall = PerforatedQuad(min, max, frameMin, frameMax, Vector2.zero)
                .Paint(windowColorData.m_WallColor, windowColorData.m_VertexWallMat);
            wall.name = k_WallDraftName;

            CompoundMeshDraft windowpane = WindowPane(frameMin + frameDepth, frameMax + frameDepth, windowColorData, windowFrameRodWidth, windowFrameRodHeight, windowFrameRodDepth, numCenterRod, windowFrameRodType);
            CompoundMeshDraft compoundDraft = new CompoundMeshDraft().Add(frame).Add(wall).Add(windowpane);

            if (hasWindowsill)
            {
                Vector3 windowsillWidth = frameWidth + Vector3.right * k_WindowsillWidthOffset;
                Vector3 windowsillDepth = Vector3.forward * k_WindowsillDepth;
                Vector3 windowsillHeight = Vector3.up * k_WindowsillThickness;
                MeshDraft windowsill = MeshDraft.PartialBox(windowsillWidth, windowsillDepth, windowsillHeight, Directions.All & ~Directions.Forward)
                    .Move(frameMin + frameWidth / 2 + frameDepth - windowsillDepth / 2)
                    .Paint(windowColorData.m_WindowPaneColor, windowColorData.m_VertexWindowPaneMat);
                windowsill.name = k_WindowPaneDraftName;
                compoundDraft.Add(windowsill);
            }

            return compoundDraft;
        }

        protected static CompoundMeshDraft WindowTextured(
            Vector3 min,
            float width,
            float height,
            float windowWidthOffset,
            float windowBottomOffset,
            float windowTopOffset,
            float windowDepthOffset,
            float windowFrameRodWidth,
            float windowFrameRodHeight,
            float windowFrameRodDepth,
            int numCenterRod,
            WindowFrameRodType windowFrameRodType,
            WindowTexturedData windowTexturedData,
            bool hasWindowsill,
            bool isRectangleWindow
            )
        {
            Vector3 widthVector = Vector3.right * width;
            Vector3 heightVector = Vector3.up * height;
            Vector3 max = min + widthVector + heightVector;
            Vector3 frameMin = min + Vector3.right * windowWidthOffset + Vector3.up * windowBottomOffset;
            Vector3 frameMax = max - Vector3.right * windowWidthOffset - Vector3.up * windowTopOffset;
            Vector3 frameWidth = Vector3.right * (width - windowWidthOffset * 2);
            Vector3 frameHeight = Vector3.up * (height - windowBottomOffset - windowTopOffset);
            Vector3 frameDepth = Vector3.forward * windowDepthOffset;
            Vector3 frameSize = frameMax - frameMin;

            if (isRectangleWindow)
            {
                float widthOffset;
                float heightOffset;
                if (width < height)
                {
                    widthOffset = width * 0.2f;
                    heightOffset = (height - width) * 0.5f + widthOffset;
                }
                else
                {
                    heightOffset = height * 0.2f;
                    widthOffset = (width - height) * 0.5f + heightOffset;
                }

                frameMin = min + Vector3.right * widthOffset + Vector3.up * heightOffset;
                frameMax = max - Vector3.right * widthOffset - Vector3.up * heightOffset;
                frameWidth = Vector3.right * (width - widthOffset * 2);
                frameHeight = Vector3.up * (height - heightOffset * 2);
                frameSize = frameMax - frameMin;
            }

            MeshDraft frame = MeshDraft.PartialBox(frameWidth, frameDepth, frameHeight, Directions.All & ~Directions.ZAxis, true)
                .FlipFaces()
                .Move(frameMin + frameSize/2 + frameDepth/2)
                .Paint(windowTexturedData.m_WindowPaneMat);
            frame.name = k_WindowPaneTexturedDraftName;

            MeshDraft wall = PerforatedQuad(min, max, frameMin, frameMax, windowTexturedData.m_UVScale, true)
                .Paint(windowTexturedData.m_WallMat);
            wall.name = k_WallTexturedDraftName;

            CompoundMeshDraft windowpane = WindowPaneTextured(frameMin + frameDepth, frameMax + frameDepth, windowTexturedData, windowFrameRodWidth, windowFrameRodHeight, windowFrameRodDepth, numCenterRod, windowFrameRodType);
            CompoundMeshDraft compoundDraft = new CompoundMeshDraft().Add(frame).Add(wall).Add(windowpane);

            if (hasWindowsill)
            {
                Vector3 windowsillWidth = frameWidth + Vector3.right*k_WindowsillWidthOffset;
                Vector3 windowsillDepth = Vector3.forward*k_WindowsillDepth;
                Vector3 windowsillHeight = Vector3.up*k_WindowsillThickness;
                MeshDraft windowsill = MeshDraft.PartialBox(windowsillWidth, windowsillDepth, windowsillHeight, Directions.All & ~Directions.Forward, true)
                    .Move(frameMin + frameWidth/2 + frameDepth - windowsillDepth/2)
                    .Paint(windowTexturedData.m_WindowPaneMat);
                windowsill.name = k_WindowPaneTexturedDraftName;
                compoundDraft.Add(windowsill);
            }
            return compoundDraft;
        }

        public static CompoundMeshDraft WindowPane(Vector3 min, Vector3 max, WindowColorData windowColorData, float windowFrameRodWidth = k_WindowFrameRodWidth, float windowFrameRodHeight = k_WindowFrameRodHeight, float windowFrameRodDepth = k_WindowFrameRodDepth, int numCenterRod = -1, WindowFrameRodType windowFrameRodType = WindowFrameRodType.k_Vertical)
        {
            MeshDraft windowpaneFrame = WindowpaneFrame(min, max, windowFrameRodWidth, windowFrameRodHeight, windowFrameRodDepth, numCenterRod, windowFrameRodType, windowColorData, out Vector3 frameDepth, out Vector3 windowMin, out Vector3 windowWidth, out Vector3 windowHeight);
            windowpaneFrame.name = k_WindowPaneDraftName;

            MeshDraft glass = WindowpaneGlass(frameDepth, windowMin, windowWidth, windowHeight)
                .Paint(windowColorData.m_WindowPaneGlassColor, windowColorData.m_VertexWindowPaneMat);
            glass.name = k_WindowPaneDraftName;

            return new CompoundMeshDraft().Add(windowpaneFrame).Add(glass);
        }

        public static CompoundMeshDraft WindowPaneTextured(Vector3 min, Vector3 max, WindowTexturedData windowTexturedData, float windowFrameRodWidth = k_WindowFrameRodWidth, float windowFrameRodHeight = k_WindowFrameRodHeight, float windowFrameRodDepth = k_WindowFrameRodDepth, int numCenterRod = -1, WindowFrameRodType windowFrameRodType = WindowFrameRodType.k_Vertical)
        {
            MeshDraft windowpaneFrame = WindowpaneFrameTextured(min, max, windowFrameRodWidth, windowFrameRodHeight, windowFrameRodDepth, numCenterRod, windowFrameRodType, out Vector3 frameDepth, out Vector3 windowMin, out Vector3 windowWidth, out Vector3 windowHeight);
            windowpaneFrame.Paint(windowTexturedData.m_WindowPaneMat);
            windowpaneFrame.name = k_WindowPaneTexturedDraftName;

            MeshDraft glass = WindowpaneGlassTextured(frameDepth, windowMin, windowWidth, windowHeight, windowTexturedData.m_UVScale);
            glass.Paint(windowTexturedData.m_WindowGlassMat);
            glass.name = windowTexturedData.m_WindowpaneGlassName;

            return new CompoundMeshDraft().Add(windowpaneFrame).Add(glass);
        }

        public static MeshDraft WindowpaneFrame(Vector3 min, Vector3 max, float windowFrameRodWidth, float windowFrameRodHeight, float windowFrameRodDepth, int numCenterRod, WindowFrameRodType windowFrameRodType, WindowColorData windowColorData,
            out Vector3 frameDepth, out Vector3 windowMin, out Vector3 windowWidth, out Vector3 windowHeight)
        {
            Vector3 size = max - min;
            Vector3 widthVector = size.ToVector3XZ();
            Vector3 heightVector = size.ToVector3Y();
            MeshDraft frameRods = WindowpaneFrameRods(min, widthVector, heightVector, windowFrameRodWidth, windowFrameRodHeight, windowFrameRodDepth, numCenterRod, windowFrameRodType, generateUV:false, out Vector3 frameWidth, out Vector3 frameHeight, out frameDepth, out Vector3 startPosition);
            frameRods.Paint(windowColorData.m_WindowPaneColor, windowColorData.m_VertexWindowPaneMat);

            windowMin = min + frameWidth + frameHeight;
            windowWidth = widthVector - frameWidth*2;
            windowHeight = heightVector - frameHeight*2;
            Vector3 windowMax = windowMin + windowWidth + windowHeight;
            MeshDraft windowpaneOuterFrame = WindowpaneOuterFrame(min, max, widthVector, frameDepth, startPosition, windowMin, windowWidth, windowHeight, windowMax, generateUV:false);
            windowpaneOuterFrame.Paint(windowColorData.m_WindowPaneColor, windowColorData.m_VertexWindowPaneMat);
            frameRods.Add(windowpaneOuterFrame);

            return frameRods;
        }

        public static MeshDraft WindowpaneFrameTextured(Vector3 min, Vector3 max, float windowFrameWidth, float windowFrameHeight, float windowFrameDepth, int numCenterRod, WindowFrameRodType windowFrameRodType,
            out Vector3 frameDepth, out Vector3 windowMin, out Vector3 windowWidth, out Vector3 windowHeight)
        {
            Vector3 size = max - min;
            Vector3 widthVector = size.ToVector3XZ();
            Vector3 heightVector = size.ToVector3Y();
            MeshDraft frameRods = WindowpaneFrameRods(min, widthVector, heightVector, windowFrameWidth, windowFrameHeight, windowFrameDepth, numCenterRod, windowFrameRodType, generateUV:true, out Vector3 frameWidth, out Vector3 frameHeight, out frameDepth, out Vector3 startPosition);

            windowMin = min + frameWidth + frameHeight;
            windowWidth = widthVector - frameWidth*2;
            windowHeight = heightVector - frameHeight*2;
            Vector3 windowMax = windowMin + windowWidth + windowHeight;
            MeshDraft windowpaneOuterFrame = WindowpaneOuterFrame(min, max, widthVector, frameDepth, startPosition, windowMin, windowWidth, windowHeight, windowMax, generateUV:true);
            frameRods.Add(windowpaneOuterFrame);

            return frameRods;
        }

        private static MeshDraft WindowpaneFrameRods(Vector3 min, Vector3 widthVector, Vector3 heightVector, float windowFrameWidth, float windowFrameHeight, float windowFrameDepth, int numCenterRod, WindowFrameRodType windowFrameRodType, bool generateUV,
            out Vector3 frameWidth, out Vector3 frameHeight, out Vector3 frameDepth, out Vector3 startPosition)
        {
            var frame = new MeshDraft();

            Vector3 right = widthVector.normalized;
            Vector3 normal = Vector3.Cross(heightVector, right).normalized;

            float width = widthVector.magnitude;
            int rodCount = Mathf.FloorToInt(width/k_WindowSegmentMinWidth);
            if (0 <= numCenterRod)
            {
                rodCount = Mathf.Min(numCenterRod, rodCount);
            }
            float interval = width/(rodCount + 1);

            frameWidth = right*windowFrameWidth/2;
            frameHeight = Vector3.up*windowFrameHeight/2;
            frameDepth = -normal*windowFrameDepth/2;
            startPosition = min + heightVector/2 + frameDepth/2;

            for (int i = 0; i < rodCount; i++)
            {
                switch (windowFrameRodType)
                {
                    case WindowFrameRodType.k_Vertical:
                    {
                        MeshDraft rod = MeshDraft.PartialBox(frameWidth*2, frameDepth, heightVector - frameHeight*2, Directions.Left | Directions.Back | Directions.Right, generateUV)
                            .Move(startPosition + right * ((i + 1) * interval));
                        frame.Add(rod);
                        break;
                    }
                    case WindowFrameRodType.k_Horizontal:
                    {
                        MeshDraft rod = MeshDraft.PartialBox(widthVector - frameWidth*2, frameDepth, frameHeight*2, Directions.Up | Directions.Back | Directions.Down, generateUV)
                            .Move(startPosition + right * ((i + 1) * interval));
                        frame.Add(rod);
                        break;
                    }
                    case WindowFrameRodType.k_Cross:
                    {
                        MeshDraft verticalRod = MeshDraft.PartialBox(frameWidth*2, frameDepth, heightVector - frameHeight*2, Directions.Left | Directions.Back | Directions.Right, generateUV)
                            .Move(startPosition + right * ((i + 1) * interval));
                        frame.Add(verticalRod);

                        MeshDraft horizontalLeftRod = MeshDraft.PartialBox(widthVector/2 - frameWidth*2, frameDepth, frameHeight*2, Directions.Up | Directions.Back | Directions.Down, generateUV)
                            .Move(startPosition + right * ((i + 1) * interval) - widthVector/4);
                        frame.Add(horizontalLeftRod);

                        MeshDraft horizontalRightRod = MeshDraft.PartialBox(widthVector/2 - frameWidth*2, frameDepth, frameHeight*2, Directions.Up | Directions.Back | Directions.Down, generateUV)
                            .Move(startPosition + right * ((i + 1) * interval) + widthVector/4);
                        frame.Add(horizontalRightRod);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(windowFrameRodType), windowFrameRodType, null);
                }

            }
            return frame;
        }

        private static MeshDraft WindowpaneOuterFrame(Vector3 min, Vector3 max, Vector3 widthVector, Vector3 frameDepth, Vector3 startPosition, Vector3 windowMin, Vector3 windowWidth, Vector3 windowHeight, Vector3 windowMax, bool generateUV)
        {
            var outerFrame = new MeshDraft();
            outerFrame.Add(PerforatedQuad(min, max, windowMin, windowMax, Vector2.zero, generateUV:generateUV));
            MeshDraft box = MeshDraft.PartialBox(windowWidth, frameDepth, windowHeight, Directions.All & ~Directions.ZAxis, generateUV)
                .FlipFaces()
                .Move(startPosition + widthVector/2);
            outerFrame.Add(box);
            return outerFrame;
        }

        private static MeshDraft WindowpaneGlass(Vector3 frameDepth, Vector3 windowMin, Vector3 windowWidth, Vector3 windowHeight)
        {
            return new MeshDraft().AddQuad(windowMin + frameDepth, windowWidth, windowHeight, true);
        }

        private static MeshDraft WindowpaneGlassTextured(Vector3 frameDepth, Vector3 windowMin, Vector3 windowWidth, Vector3 windowHeight, Vector2 uvScale)
        {
            return new MeshDraft().AddQuad(windowMin + frameDepth, windowWidth, windowHeight, uvScale, true, true);
        }
    }
}
