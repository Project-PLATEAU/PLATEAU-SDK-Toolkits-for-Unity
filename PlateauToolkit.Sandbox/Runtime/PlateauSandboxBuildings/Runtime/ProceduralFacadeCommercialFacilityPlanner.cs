using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces;
using ProceduralToolkit;
using ProceduralToolkit.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime
{
    [CreateAssetMenu(menuName = "ProceduralToolkit/Buildings/Procedural Facade Planner/Commercial Facility", order = 4)]
    public class ProceduralFacadeCommercialFacilityPlanner : FacadePlanner
    {
        private const float k_MinBuildingHeight = 5.5f;
        private const float k_MaxBuildingHeight = 100f;
        private const float k_FloorHeight = 2.5f;
        private const float k_LongWindowHeight = 3.5f;
        private const float k_SmallWallHeight = 1.0f;
        private const float k_SmallWindowHeight = 1.5f;
        private const float k_BufferWidth = 2;

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

                switch (i)
                {
                    case 0:
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Back;
                        layouts.Add(PlanNormalFacade(width, config, leftIsConvex, rightIsConvex));
                        break;
                    case 1:
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Right;
                        layouts.Add(PlanNormalFacade(width, config, leftIsConvex, rightIsConvex));
                        break;
                    case 2:
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Front;
                        layouts.Add(PlanEntranceFacade(width, config, leftIsConvex, rightIsConvex));
                        break;
                    case 3:
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Left;
                        layouts.Add(PlanNormalFacade(width, config, leftIsConvex, rightIsConvex));
                        break;
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
                    m_WindowFrameRodWidth = 0.1f,
                    m_WindowFrameRodHeight = 0.2f,
                    m_NumCenterRods = 1
                }
            };
            m_Constructors[PanelType.k_Wall] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralWall(config)
            };
            m_Constructors[PanelType.k_WallWithFrame] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralWallWithFrame(config)
                {
                    m_NumCenterRods = 0
                }
            };
            m_Constructors[PanelType.k_SmallFullWindow] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralFullWindow(config)
                {
                    m_NumCenterRods = 1,
                    m_WindowFrameRodType = ProceduralFacadeElement.WindowFrameRodType.k_Vertical,
                    m_WindowFrameRodWidth = 0.05f
                }
            };
            m_Constructors[PanelType.k_FullWindow] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralFullWindow(config)
                {
                    m_WindowFrameRodWidth = 0.1f
                }
            };
        }

        private ILayout PlanNormalFacade(float facadeWidth, BuildingGenerator.Config config, bool leftIsConvex, bool rightIsConvex)
        {
            List<PanelSize> panelSizes = DivideFacade(facadeWidth, leftIsConvex, rightIsConvex, out float remainderWidth);
            return CreateNormalFacadeVertical(panelSizes, remainderWidth, 0, panelSizes.Count, config);
        }

        private ILayout PlanEntranceFacade(float facadeWidth, BuildingGenerator.Config config, bool leftIsConvex, bool rightIsConvex)
        {
            List<PanelSize> panelSizes = DivideFacade(facadeWidth, leftIsConvex, rightIsConvex, out float remainderWidth);

            var horizontal = new HorizontalLayout();

            const int entranceCount = 1;
            int entranceIndexInterval = (panelSizes.Count - entranceCount)/(entranceCount + 1);
            const int lastEntranceIndex = -1;

            float floorWidthOffset = remainderWidth / panelSizes.Count;
            horizontal.Add(CreateEntranceNormalFacadeVertical(panelSizes, floorWidthOffset, lastEntranceIndex + 1, entranceIndexInterval, config));
            horizontal.Add(CreateEntranceVertical(m_SizeValues[panelSizes[entranceIndexInterval]] + floorWidthOffset, config, horizontal));
            horizontal.Add(CreateEntranceNormalFacadeVertical(panelSizes, floorWidthOffset, entranceIndexInterval + 1, panelSizes.Count, config));

            return horizontal;
        }

        private VerticalLayout CreateNormalFacadeVertical(List<PanelSize> panelSizes, float remainderWidth, int from, int to, BuildingGenerator.Config config)
        {
            float floorWidthOffset = remainderWidth / (to - from);
            var vertical = new VerticalLayout
            {
                CreateHorizontal(panelSizes, from, to, k_LongWindowHeight, floorWidthOffset, m_Constructors[PanelType.k_FullWindow]),
                CreateHorizontal(panelSizes, from, to, k_SmallWallHeight, floorWidthOffset, m_Constructors[PanelType.k_WallWithFrame]),
                CreateHorizontalDepressionWall(panelSizes, from, to, k_SmallWallHeight, floorWidthOffset, config)
            };

            int i = 0;
            float remainingHeight = config.buildingHeight - k_MinBuildingHeight;
            if (0 < remainingHeight)
            {
                while (0 < remainingHeight)
                {
                    switch (remainingHeight)
                    {
                        case >= k_FloorHeight:
                            if (i++ % 4 == 3)
                            {
                                vertical.Add(CreateHorizontal(panelSizes, from, to, k_SmallWindowHeight, floorWidthOffset, m_Constructors[PanelType.k_SmallFullWindow]));
                                remainingHeight -= k_SmallWindowHeight;
                            }
                            else
                            {
                                vertical.Add(CreateHorizontal(panelSizes, from, to, k_FloorHeight, floorWidthOffset, m_Constructors[PanelType.k_WallWithFrame]));
                                remainingHeight -= k_FloorHeight;
                            }
                            break;
                        default:
                            if (Geometry.Epsilon <= remainingHeight)
                            {
                                vertical.Add(CreateHorizontal(panelSizes, from, to, remainingHeight, floorWidthOffset, m_Constructors[PanelType.k_WallWithFrame]));
                            }
                            remainingHeight = -1;
                            break;
                    }
                }
            }

            return vertical;
        }

        private VerticalLayout CreateEntranceNormalFacadeVertical(List<PanelSize> panelSizes, float floorWidthOffset, int from, int to, BuildingGenerator.Config config)
        {
            var vertical = new VerticalLayout
            {
                CreateHorizontal(panelSizes, from, to, k_LongWindowHeight, floorWidthOffset, m_Constructors[PanelType.k_FullWindow]),
                CreateHorizontal(panelSizes, from, to, k_SmallWallHeight, floorWidthOffset, m_Constructors[PanelType.k_WallWithFrame]),
                CreateHorizontalDepressionWall(panelSizes, from, to, k_SmallWallHeight, floorWidthOffset, config),
            };

            int i = 0;
            float remainingHeight = config.buildingHeight - k_MinBuildingHeight;
            if (0 < remainingHeight)
            {
                while (0 < remainingHeight)
                {
                    switch (remainingHeight)
                    {
                        case >= k_FloorHeight:
                            if (i++ % 4 == 3)
                            {
                                vertical.Add(CreateHorizontal(panelSizes, from, to, k_SmallWindowHeight, floorWidthOffset, m_Constructors[PanelType.k_SmallFullWindow]));
                                remainingHeight -= k_SmallWindowHeight;
                            }
                            else
                            {
                                vertical.Add(CreateHorizontal(panelSizes, from, to, k_FloorHeight, floorWidthOffset, m_Constructors[PanelType.k_WallWithFrame]));
                                remainingHeight -= k_FloorHeight;
                            }
                            break;
                        default:
                            if (Geometry.Epsilon <= remainingHeight)
                            {
                                vertical.Add(CreateHorizontal(panelSizes, from, to, remainingHeight, floorWidthOffset, m_Constructors[PanelType.k_WallWithFrame]));
                            }
                            remainingHeight = -1;
                            break;
                    }
                }
            }

            return vertical;
        }

        private VerticalLayout CreateEntranceVertical(float width, BuildingGenerator.Config config, HorizontalLayout horizontalLayout)
        {
            ProceduralFacadeElement.PositionType positionType = ProceduralFacadeElement.PositionType.k_Middle;
            if (!horizontalLayout.Any())
            {
                positionType = ProceduralFacadeElement.PositionType.k_Left;
            }

            var vertical = new VerticalLayout
            {
                Construct(m_Constructors[PanelType.k_Entrance], width, k_FloorHeight),
                Construct(m_Constructors[PanelType.k_FullWindow], width, k_SmallWallHeight),
                Construct(m_Constructors[PanelType.k_WallWithFrame], width, k_SmallWallHeight),
                Construct(() => new ProceduralFacadeCompoundElements.ProceduralDepressionWall(config, positionType), width, k_SmallWallHeight)
            };

            int i = 0;
            float remainingHeight = config.buildingHeight - k_MinBuildingHeight;
            if (0 < remainingHeight)
            {
                while (0 < remainingHeight)
                {
                    switch (remainingHeight)
                    {
                        case >= k_FloorHeight:
                            if (i++ % 4 == 3)
                            {
                                vertical.Add(Construct(m_Constructors[PanelType.k_SmallFullWindow], width, k_SmallWindowHeight));
                                remainingHeight -= k_SmallWindowHeight;
                            }
                            else
                            {
                                vertical.Add(Construct(m_Constructors[PanelType.k_WallWithFrame], width, k_FloorHeight));
                                remainingHeight -= k_FloorHeight;
                            }
                            break;
                        default:
                            if (Geometry.Epsilon <= remainingHeight)
                            {
                                vertical.Add(Construct(m_Constructors[PanelType.k_WallWithFrame], width, remainingHeight));
                            }
                            remainingHeight = -1;
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

        private HorizontalLayout CreateHorizontalDepressionWall(List<PanelSize> panelSizes, int from, int to, float height, float floorWidthOffset, BuildingGenerator.Config config)
        {
            var horizontal = new HorizontalLayout();
            for (int i = from; i < to; i++)
            {
                float panelWidth = m_SizeValues[panelSizes[i]] + floorWidthOffset;
                ProceduralFacadeElement.PositionType positionType;
                if (i == 0)
                {
                    positionType = ProceduralFacadeElement.PositionType.k_Left;
                }
                else if (i == panelSizes.Count - 1)
                {
                    positionType = ProceduralFacadeElement.PositionType.k_Right;
                }
                else
                {
                    positionType = ProceduralFacadeElement.PositionType.k_Middle;
                }

                ProceduralFacadeElement.PositionType type = positionType;
                var balcony = new List<Func<ILayoutElement>>
                {
                    () => new ProceduralFacadeCompoundElements.ProceduralDepressionWall(config, type)
                };
                horizontal.Add(Construct(balcony, panelWidth, height));
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
            k_Narrow
        }

        private enum PanelType : byte
        {
            k_Entrance,
            k_Wall,
            k_WallWithFrame,
            k_SmallFullWindow,
            k_FullWindow
        }
    }
}
