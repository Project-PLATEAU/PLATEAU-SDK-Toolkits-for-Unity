using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces;
using ProceduralToolkit;
using ProceduralToolkit.Buildings;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime
{
    [CreateAssetMenu(menuName = "ProceduralToolkit/Buildings/Procedural Facade Planner/Office Building", order = 1)]
    public class ProceduralFacadeOfficeBuildingPlanner : FacadePlanner
    {
        private const float k_MaxBuildingHeight = 100f;
        private const float k_MinFloorHeight = 2.75f;
        private const float k_MaxFloorHeight = 3.25f;
        private const float k_BufferWidth = 2;
        private const float k_ShadowWallOffset = 0.1f;
        private const float k_EntranceWindowTopOffset = 0.6f;

        private readonly Dictionary<PanelType, List<Func<ILayoutElement>>> m_Constructors = new();
        private readonly Dictionary<PanelSize, float> m_SizeValues = new()
        {
            {PanelSize.k_Narrow, 2.5f},
        };

        public override List<ILayout> Plan(List<Vector2> foundationPolygon, BuildingGenerator.Config config)
        {
            if (k_MaxBuildingHeight < config.buildingHeight)
            {
                config.buildingHeight = k_MaxBuildingHeight;
            }

            SetupConstructors(config);

            // Supports only rectangular buildings
            var layouts = new List<ILayout>();
            for (int i = 0; i < foundationPolygon.Count; i++)
            {
                Vector2 a = foundationPolygon.GetLooped(i + 1);
                Vector2 aNext = foundationPolygon.GetLooped(i + 2);
                Vector2 b = foundationPolygon[i];
                Vector2 bPrevious = foundationPolygon.GetLooped(i - 1);
                float width = (b - a).magnitude;
                bool leftIsConvex = Geometry.GetAngle(b, a, aNext) <= 180;
                bool rightIsConvex = Geometry.GetAngle(bPrevious, b, a) <= 180;

                // 小数点が最も小さい（フロア数を求めた時に最も正確に割り切れるかを表す）フロア数から最大のフロア高を求める
                float floorHeight = 0;
                float floorHeightRemaining = 1f;
                for (float tempFloorHeight = k_MinFloorHeight; tempFloorHeight < k_MaxFloorHeight;)
                {
                    float numFloor = config.buildingHeight / tempFloorHeight;
                    if (numFloor - Mathf.Floor(numFloor) < floorHeightRemaining)
                    {
                        floorHeight = tempFloorHeight;
                        floorHeightRemaining = numFloor - Mathf.Floor(numFloor);
                    }
                    tempFloorHeight += 0.05f;
                }

                switch (i)
                {
                    case 0:
                    {
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Back;
                        var vertical = new VerticalLayout();
                        vertical.AddElement(Construct(m_Constructors[PanelType.k_ShadowWall], width - k_ShadowWallOffset, config.buildingHeight - k_ShadowWallOffset));
                        vertical.Add(PlanNormalFacade(width, floorHeight,　config, leftIsConvex, rightIsConvex));
                        layouts.Add(vertical);
                        break;
                    }
                    case 1:
                    {
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Right;
                        var vertical = new VerticalLayout();
                        vertical.AddElement(Construct(m_Constructors[PanelType.k_ShadowWall], width - k_ShadowWallOffset, config.buildingHeight - k_ShadowWallOffset));
                        vertical.Add(PlanNormalFacade(width, floorHeight, config, leftIsConvex, rightIsConvex));
                        layouts.Add(vertical);
                        break;
                    }
                    case 2:
                    {
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Front;
                        var vertical = new VerticalLayout();
                        vertical.AddElement(Construct(m_Constructors[PanelType.k_ShadowWall], width - k_ShadowWallOffset, config.buildingHeight - k_ShadowWallOffset));
                        vertical.Add(PlanEntranceFacade(width, floorHeight, config, leftIsConvex, rightIsConvex));
                        layouts.Add(vertical);
                        break;
                    }
                    case 3:
                    {
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Left;
                        var vertical = new VerticalLayout();
                        vertical.AddElement(Construct(m_Constructors[PanelType.k_ShadowWall], width - k_ShadowWallOffset, config.buildingHeight - k_ShadowWallOffset));
                        vertical.Add(PlanNormalFacade(width, floorHeight, config, leftIsConvex, rightIsConvex));
                        layouts.Add(vertical);
                        break;
                    }
                }
            }

            return layouts;
        }

        private void SetupConstructors(BuildingGenerator.Config config)
        {
            m_Constructors[PanelType.k_Entrance] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralFullWindow(config)
                {
                    m_WindowFrameRodWidth = 0.2f,
                    m_WindowFrameRodHeight = 0.3f,
                    m_NumCenterRods = 1
                }
            };
            m_Constructors[PanelType.k_Wall] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralWall(config)
            };
            m_Constructors[PanelType.k_ShadowWall] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralWall(config)
                {
                    m_IsShadowWall = true,
                    m_MoveShadowWallDepth = 0.1f,
                    m_ShadowWallWidthOffset = k_ShadowWallOffset,
                    m_ShadowWallHeightOffset = 0
                }
            };
            m_Constructors[PanelType.k_SmallFullWindow] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralFullWindow(
                    config,
                    config.officeBuildingMaterialPalette.spandrel,
                    "GlassSpandrelTextured",
                    "FrameSpandrelTextured")
                {
                    m_WindowFrameRodWidth = 0.2f
                }
            };
            m_Constructors[PanelType.k_FullWindow] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralFullWindow(config)
                {
                    m_WindowFrameRodWidth = 0.2f,
                }
            };
        }

        private ILayout PlanNormalFacade(float facadeWidth, float floorHeight, BuildingGenerator.Config config, bool leftIsConvex, bool rightIsConvex)
        {
            List<PanelSize> panelSizes = DivideFacade(facadeWidth, leftIsConvex, rightIsConvex, out float remainderWidth);
            return CreateNormalFacadeVertical(panelSizes, remainderWidth, floorHeight, 0, panelSizes.Count, config);
        }

        private ILayout PlanEntranceFacade(float facadeWidth, float floorHeight, BuildingGenerator.Config config, bool leftIsConvex, bool rightIsConvex)
        {
            List<PanelSize> panelSizes = DivideFacade(facadeWidth, leftIsConvex, rightIsConvex, out float remainderWidth);

            var horizontal = new HorizontalLayout();

            const int entranceCount = 1;
            int entranceIndexInterval = (panelSizes.Count - entranceCount)/(entranceCount + 1);
            const int lastEntranceIndex = -1;

            float floorWidthOffset = remainderWidth / panelSizes.Count;
            horizontal.Add(CreateEntranceNormalFacadeVertical(panelSizes, floorWidthOffset, floorHeight, lastEntranceIndex + 1, entranceIndexInterval, config));
            horizontal.Add(CreateEntranceVertical(m_SizeValues[panelSizes[entranceIndexInterval]], floorWidthOffset, floorHeight, config));
            horizontal.Add(CreateEntranceNormalFacadeVertical(panelSizes, floorWidthOffset, floorHeight, entranceIndexInterval + 1, panelSizes.Count, config));

            return horizontal;
        }

        private VerticalLayout CreateNormalFacadeVertical(List<PanelSize> panelSizes, float remainderWidth, float floorHeight, int from, int to, BuildingGenerator.Config config)
        {
            int numFloorWithoutEntrance = (int)Mathf.Floor(config.buildingHeight / floorHeight) - 1;
            float entranceHeight = config.buildingHeight - numFloorWithoutEntrance * floorHeight;
            float floorWidthOffset = remainderWidth / (to - from);
            var vertical = new VerticalLayout
            {
                config.officeBuildingParams.useWindow
                    ? CreateHorizontal(panelSizes, from, to, entranceHeight, floorWidthOffset, m_Constructors[PanelType.k_FullWindow])
                    : CreateHorizontal(panelSizes, from, to, entranceHeight, floorWidthOffset, m_Constructors[PanelType.k_Wall])
            };

            float remainingHeight = config.buildingHeight - entranceHeight;
            if (0 < remainingHeight)
            {
                int switchIndex = 0;
                while (0 < remainingHeight)
                {
                    switch (switchIndex++ % 2)
                    {
                        case 0:
                            remainingHeight -= config.officeBuildingParams.spandrelHeight;
                            vertical.Add(remainingHeight < 0
                                ? CreateHorizontal(panelSizes, from, to, config.officeBuildingParams.spandrelHeight + remainingHeight, floorWidthOffset, m_Constructors[PanelType.k_SmallFullWindow])
                                : CreateHorizontal(panelSizes, from, to, config.officeBuildingParams.spandrelHeight, floorWidthOffset, m_Constructors[PanelType.k_SmallFullWindow]));
                            break;
                        case 1:
                            remainingHeight -= floorHeight;
                            vertical.Add(remainingHeight < 0
                                ? CreateHorizontal(panelSizes, from, to, floorHeight + remainingHeight, floorWidthOffset, m_Constructors[PanelType.k_FullWindow])
                                : CreateHorizontal(panelSizes, from, to, floorHeight, floorWidthOffset, m_Constructors[PanelType.k_FullWindow]));
                            break;
                    }
                }
            }

            return vertical;
        }

        private VerticalLayout CreateEntranceNormalFacadeVertical(List<PanelSize> panelSizes, float floorWidthOffset, float floorHeight, int from, int to, BuildingGenerator.Config config)
        {
            int numFloorWithoutEntrance = (int)Mathf.Floor(config.buildingHeight / floorHeight) - 1;
            float entranceHeight = config.buildingHeight - numFloorWithoutEntrance * floorHeight;
            var vertical = new VerticalLayout
            {
                config.officeBuildingParams.useWindow
                    ? CreateHorizontal(panelSizes, from, to, entranceHeight, floorWidthOffset, m_Constructors[PanelType.k_FullWindow])
                    : CreateHorizontal(panelSizes, from, to, entranceHeight, floorWidthOffset, m_Constructors[PanelType.k_Wall])
            };

            float remainingHeight = config.buildingHeight - entranceHeight;
            if (0 < remainingHeight)
            {
                int switchIndex = 0;
                while (0 < remainingHeight)
                {
                    switch (switchIndex++ % 2)
                    {
                        case 0:
                            remainingHeight -= config.officeBuildingParams.spandrelHeight;
                            vertical.Add(remainingHeight < 0
                                ? CreateHorizontal(panelSizes, from, to, config.officeBuildingParams.spandrelHeight + remainingHeight, floorWidthOffset, m_Constructors[PanelType.k_SmallFullWindow])
                                : CreateHorizontal(panelSizes, from, to, config.officeBuildingParams.spandrelHeight, floorWidthOffset, m_Constructors[PanelType.k_SmallFullWindow]));
                            break;
                        case 1:
                            remainingHeight -= floorHeight;
                            vertical.Add(remainingHeight < 0
                                ? CreateHorizontal(panelSizes, from, to, floorHeight + remainingHeight, floorWidthOffset, m_Constructors[PanelType.k_FullWindow])
                                : CreateHorizontal(panelSizes, from, to, floorHeight, floorWidthOffset, m_Constructors[PanelType.k_FullWindow]));
                            break;
                    }
                }
            }

            return vertical;
        }

        private VerticalLayout CreateEntranceVertical(float width, float floorWidthOffset, float floorHeight, BuildingGenerator.Config config)
        {
            int numFloorWithoutEntrance = (int)Mathf.Floor(config.buildingHeight / floorHeight) - 1;
            float entranceHeight = config.buildingHeight - numFloorWithoutEntrance * floorHeight;
            var vertical = new VerticalLayout();
            if (config.officeBuildingParams.useWindow)
            {
                vertical.Add(Construct(m_Constructors[PanelType.k_Entrance], width + floorWidthOffset, entranceHeight - k_EntranceWindowTopOffset));
                vertical.Add(Construct(m_Constructors[PanelType.k_FullWindow], width + floorWidthOffset, k_EntranceWindowTopOffset));
            }
            else
            {
                vertical.Add(Construct(m_Constructors[PanelType.k_Entrance], width + floorWidthOffset, entranceHeight - k_EntranceWindowTopOffset));
                vertical.Add(Construct(m_Constructors[PanelType.k_Wall], width + floorWidthOffset, k_EntranceWindowTopOffset));
            }

            float remainingHeight = config.buildingHeight - entranceHeight;
            if (0 < remainingHeight)
            {
                int switchIndex = 0;
                while (0 < remainingHeight)
                {
                    switch (switchIndex++ % 2)
                    {
                        case 0:
                            remainingHeight -= config.officeBuildingParams.spandrelHeight;
                            vertical.Add(remainingHeight < 0
                                ? Construct(m_Constructors[PanelType.k_SmallFullWindow], width + floorWidthOffset, config.officeBuildingParams.spandrelHeight + remainingHeight)
                                : Construct(m_Constructors[PanelType.k_SmallFullWindow], width + floorWidthOffset, config.officeBuildingParams.spandrelHeight));
                            break;
                        case 1:
                            remainingHeight -= floorHeight;
                            vertical.Add(remainingHeight < 0
                                ? Construct(m_Constructors[PanelType.k_FullWindow], width + floorWidthOffset, floorHeight + remainingHeight)
                                : Construct(m_Constructors[PanelType.k_FullWindow], width + floorWidthOffset, floorHeight));
                            break;
                    }
                }
            }

            return vertical;
        }

        private List<PanelSize> DivideFacade(float facadeWidth, bool leftIsConvex, bool rightIsConvex, out float remainder)
        {
            float availableWidth = facadeWidth;
            if (!leftIsConvex)
            {
                availableWidth -= k_BufferWidth;
            }
            if (!rightIsConvex)
            {
                availableWidth -= k_BufferWidth;
            }

            Dictionary<PanelSize, int> knapsack = PTUtils.Knapsack(m_SizeValues, availableWidth);
            var sizes = new List<PanelSize>();
            remainder = facadeWidth;
            foreach (var pair in knapsack)
            {
                for (int i = 0; i < pair.Value; i++)
                {
                    sizes.Add(pair.Key);
                    remainder -= m_SizeValues[pair.Key];
                }
            }
            sizes.Shuffle();
            return sizes;
        }

        private HorizontalLayout CreateHorizontal(List<PanelSize> panelSizes, int from, int to, float height, float floorWidthOffset, List<Func<ILayoutElement>> constructors)
        {
            var horizontal = new HorizontalLayout();
            for (int i = from; i < to; i++)
            {
                float panelWidth = m_SizeValues[panelSizes[i]] + floorWidthOffset;
                horizontal.Add(Construct(constructors, panelWidth, height));
            }
            return horizontal;
        }

        private ILayoutElement Construct(PanelType panelType, float width, float height)
        {
            return Construct(m_Constructors[panelType], width, height);
        }

        private static ILayoutElement Construct(List<Func<ILayoutElement>> constructors, float width, float height)
        {
            return Construct(constructors.GetRandom(), width, height);
        }

        private static ILayoutElement Construct(Func<ILayoutElement> constructor, float width, float height)
        {
            ILayoutElement element = constructor();
            element.width = width * element.widthScale;
            element.height = height * element.heightScale;
            return element;
        }

        private enum PanelSize : byte
        {
            k_Narrow,
            k_Wide,
        }

        private enum PanelType : byte
        {
            k_Entrance,
            k_Wall,
            k_ShadowWall,
            k_SmallFullWindow,
            k_FullWindow,
        }
    }
}
