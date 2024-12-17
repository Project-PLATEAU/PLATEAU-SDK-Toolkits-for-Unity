using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces;
using ProceduralToolkit;
using ProceduralToolkit.Buildings;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime
{
    [CreateAssetMenu(menuName = "ProceduralToolkit/Buildings/Procedural Facade Planner/Apartment", order = 0)]
    public class ProceduralFacadeSkyscraperCondominiumPlanner : FacadePlanner
    {
        private const float k_MaxBuildingHeight = 100f;
        private const float k_MinFloorHeight = 2.75f;
        private const float k_MaxFloorHeight = 3.25f;
        private const float k_BufferWidth = 2;
        private const float k_WindowBottomOffset = 1;
        private const float k_WindowTopOffset = 0.3f;
        private const float k_WindowFrameRodHeight = 0.05f;
        private const float k_NarrowPanelSize = 2.5f;
        private const float k_MinWallWidthOffset = 1.25f;
        private const float k_ShadowWallWidthOffset = 0.3f; // 影壁の幅(窓ガラスを突き抜ける分を抑制)
        private const float k_ShadowWallHeightOffset = 0f;
        private const float k_BalconyConcaveDepth = 0.6f;
        private const float k_BalconyConvexDepth = 1f;
        private const float k_BalconyWindowDepth = -0.15f;
        private const float k_EntranceWindowHeight = 2.5f;

        private readonly Dictionary<PanelType, List<Func<ILayoutElement>>> m_Constructors = new();
        private readonly Dictionary<PanelSize, float> m_SizeValues = new()
        {
            {PanelSize.k_Narrow, k_NarrowPanelSize}
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
                List<PanelSize> panelSizes = DivideFacade(width, leftIsConvex, rightIsConvex, out float remainderWidth);
                float wallWidthOffset = Mathf.Max(k_MinWallWidthOffset, remainderWidth);
                float wallAveWidthOffset = wallWidthOffset / panelSizes.Count;
                float floorWidthOffset = remainderWidth / panelSizes.Count;
                float shadowWallDepth = (config.skyscraperCondominiumParams.convexBalcony ? k_BalconyConvexDepth : k_BalconyConcaveDepth) - k_BalconyWindowDepth;
                float shadowWallWidthOffset = k_ShadowWallWidthOffset + (-floorWidthOffset + wallAveWidthOffset) * panelSizes.Count;
                float shadowWidth = width - k_ShadowWallWidthOffset + (floorWidthOffset - wallAveWidthOffset) * panelSizes.Count;

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
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Back;
                        shadowWallDepth += config.skyscraperCondominiumParams.hasBalconyBack ? 0 : -k_BalconyConcaveDepth;
                        break;
                    case 1:
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Right;
                        shadowWallDepth += config.skyscraperCondominiumParams.hasBalconyRight ? 0 : -k_BalconyConcaveDepth;
                        break;
                    case 2:
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Front;
                        shadowWallDepth += config.skyscraperCondominiumParams.hasBalconyFront ? 0 : -k_BalconyConcaveDepth;
                        break;
                    case 3:
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Left;
                        shadowWallDepth += config.skyscraperCondominiumParams.hasBalconyLeft ? 0 : -k_BalconyConcaveDepth;
                        break;
                    default:
                        return layouts;
                }

                var vertical = new VerticalLayout();
                vertical.AddElement(Construct(() => new ProceduralFacadeCompoundElements.ProceduralWall(config)
                {
                    m_IsShadowWall = true,
                    m_MoveShadowWallDepth = shadowWallDepth,
                    m_ShadowWallWidthOffset = shadowWallWidthOffset,
                    m_ShadowWallHeightOffset = 0
                }, shadowWidth, config.buildingHeight - k_ShadowWallHeightOffset));
                vertical.Add(config.faceDirection == BuildingGenerator.Config.FaceDirection.k_Front
                    ? PlanEntranceFacade(panelSizes, floorHeight, remainderWidth, config)
                    : PlanNormalFacade(panelSizes, floorHeight, remainderWidth, config));
                layouts.Add(vertical);
            }

            return layouts;
        }

        private void SetupConstructors(BuildingGenerator.Config config)
        {
            m_Constructors[PanelType.k_Wall] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralWall(config)
            };
            m_Constructors[PanelType.k_ShadowWall] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralWall(config)
                {
                    m_IsShadowWall = true,
                    m_MoveShadowWallDepth = 0.8f,
                    m_ShadowWallWidthOffset = k_ShadowWallHeightOffset,
                    m_ShadowWallHeightOffset = 0
                }
            };
            m_Constructors[PanelType.k_Window] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralWindow(config)
                {
                    m_WindowBottomOffset = 0,
                    m_WindowTopOffset = 0,
                    m_WindowFrameRodHeight = k_WindowFrameRodHeight,
                    m_NumCenterRods = 1,
                    m_WindowFrameRodType = ProceduralFacadeElement.WindowFrameRodType.k_Vertical,
                    m_HasWindowsill = false,
                    m_IsRectangleWindow = true
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

        private ILayout PlanNormalFacade(List<PanelSize> panelSizes, float floorHeight, float remainderWidth, BuildingGenerator.Config config)
        {
            int numFloorWithoutEntrance = (int)Mathf.Floor(config.buildingHeight / floorHeight) - 1;
            float entranceHeight = config.buildingHeight - numFloorWithoutEntrance * floorHeight;
            float floorWidthOffset = remainderWidth / panelSizes.Count;

            var vertical = new VerticalLayout { CreateHorizontal(panelSizes, 0, panelSizes.Count, entranceHeight, floorWidthOffset, m_Constructors[PanelType.k_Wall]) };
            float remainingHeight = config.buildingHeight - entranceHeight;
            if (0 < remainingHeight)
            {
                vertical.Add(CreateNormalFacadeVertical(panelSizes, floorHeight, remainderWidth, 0, panelSizes.Count, config));
            }

            return vertical;
        }

        private VerticalLayout CreateNormalFacadeVertical(List<PanelSize> panelSizes, float floorHeight, float remainderWidth, int from, int to, BuildingGenerator.Config config)
        {
            int numFloorWithoutEntrance = (int)Mathf.Floor(config.buildingHeight / floorHeight) - 1;
            float wallWidthOffset = Mathf.Max(k_MinWallWidthOffset, remainderWidth);
            float wallAveWidthOffset = wallWidthOffset / (to - from);
            float floorWidthOffset = remainderWidth / (to - from);

            var vertical = new VerticalLayout();
            bool hasBalcony = config.faceDirection == BuildingGenerator.Config.FaceDirection.k_Left && config.skyscraperCondominiumParams.hasBalconyLeft ||
                              config.faceDirection == BuildingGenerator.Config.FaceDirection.k_Right && config.skyscraperCondominiumParams.hasBalconyRight ||
                              config.faceDirection == BuildingGenerator.Config.FaceDirection.k_Front && config.skyscraperCondominiumParams.hasBalconyFront ||
                              config.faceDirection == BuildingGenerator.Config.FaceDirection.k_Back && config.skyscraperCondominiumParams.hasBalconyBack;

            {
                Directions directions = Directions.Down;
                if (numFloorWithoutEntrance == 1)
                {
                    directions |= Directions.Up;
                }

                if (hasBalcony)
                {
                    var horizontal = new HorizontalLayout
                    {
                        CreateHorizontal(panelSizes, 0, 1, floorHeight, -k_NarrowPanelSize + wallWidthOffset * 0.5f, m_Constructors[PanelType.k_Wall]),
                        CreateHorizontalBalcony(panelSizes, from, to, floorHeight, floorWidthOffset - wallAveWidthOffset, config, directions),
                        CreateHorizontal(panelSizes, 0, 1, floorHeight, -k_NarrowPanelSize + wallWidthOffset * 0.5f, m_Constructors[PanelType.k_Wall])
                    };
                    vertical.Add(horizontal);
                }
                else
                {
                    vertical.Add(CreateHorizontal(panelSizes, from, to, floorHeight, floorWidthOffset, m_Constructors[PanelType.k_Window]));
                }
            }

            for (int i = 1; i < numFloorWithoutEntrance; i++)
            {
                if (hasBalcony)
                {
                    Directions directions = Directions.None;
                    if (numFloorWithoutEntrance == i + 1)
                    {
                        directions = Directions.Up;
                    }
                    var horizontal = new HorizontalLayout
                    {
                        CreateHorizontal(panelSizes, 0, 1, floorHeight, -k_NarrowPanelSize + wallWidthOffset * 0.5f, m_Constructors[PanelType.k_Wall]),
                        CreateHorizontalBalcony(panelSizes, from, to, floorHeight, floorWidthOffset - wallAveWidthOffset, config, directions),
                        CreateHorizontal(panelSizes, 0, 1, floorHeight, -k_NarrowPanelSize + wallWidthOffset * 0.5f, m_Constructors[PanelType.k_Wall])
                    };
                    vertical.Add(horizontal);
                }
                else
                {
                    vertical.Add(CreateHorizontal(panelSizes, from, to, floorHeight, floorWidthOffset, m_Constructors[PanelType.k_Window]));
                }
            }

            return vertical;
        }

        private ILayout PlanEntranceFacade(List<PanelSize> panelSizes, float floorHeight, float remainderWidth, BuildingGenerator.Config config)
        {
            const int entranceCount = 1;
            int numFloorWithoutEntrance = (int)Mathf.Floor(config.buildingHeight / floorHeight) - 1;
            int entranceIndexInterval = (panelSizes.Count - entranceCount)/(entranceCount + 1);
            float entranceHeight = config.buildingHeight - numFloorWithoutEntrance * floorHeight;
            float wallWidthOffset = Mathf.Max(k_MinWallWidthOffset, remainderWidth);
            float wallAveWidthOffset = wallWidthOffset / panelSizes.Count;
            float floorWidthOffset = remainderWidth / panelSizes.Count;

            // 1階部分
            var vertical = new VerticalLayout();
            var horizontal = new HorizontalLayout
            {
                Construct(m_Constructors[PanelType.k_Wall], wallWidthOffset * 0.5f, entranceHeight),
                CreateHorizontal(panelSizes, 0, entranceIndexInterval, entranceHeight, floorWidthOffset - wallAveWidthOffset, m_Constructors[PanelType.k_Wall]),
                Construct(new List<Func<ILayoutElement>>
                {
                    () => new ProceduralFacadeCompoundElements.ProceduralWindow(config)
                    {
                        m_WindowBottomOffset = 0,
                        m_WindowWidthOffset = 0.3f,
                        m_WindowDepthOffset = 0,
                        m_WindowTopOffset = 0 < entranceHeight - k_EntranceWindowHeight ? entranceHeight - k_EntranceWindowHeight : Math.Abs(entranceHeight - k_EntranceWindowHeight),
                        m_WindowFrameRodHeight = k_WindowFrameRodHeight,
                        m_NumCenterRods = 1,
                        m_HasWindowsill = false
                    }
                }, m_SizeValues[panelSizes[entranceIndexInterval]] + floorWidthOffset - wallAveWidthOffset, entranceHeight),
                CreateHorizontal(panelSizes, entranceIndexInterval + 1, panelSizes.Count, entranceHeight, floorWidthOffset - wallAveWidthOffset, m_Constructors[PanelType.k_Wall]),
                Construct(m_Constructors[PanelType.k_Wall], wallWidthOffset * 0.5f, entranceHeight)
            };
            vertical.Add(horizontal);

            // 2階以降
            float remainingHeight = config.buildingHeight - entranceHeight;
            if (0 < remainingHeight)
            {
                vertical.Add(CreateNormalFacadeVertical(panelSizes, floorHeight, remainderWidth, 0, panelSizes.Count, config));
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
            foreach (KeyValuePair<PanelSize, int> pair in knapsack)
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

        private HorizontalLayout CreateHorizontalBalcony(List<PanelSize> panelSizes, int from, int to, float height, float floorWidthOffset, BuildingGenerator.Config config, Directions directions)
        {
            var horizontal = new HorizontalLayout();
            for (int i = from; i < to; i++)
            {
                float panelWidth = m_SizeValues[panelSizes[i]] + floorWidthOffset;
                if (from == to - 1 && panelSizes.Count == 1)
                {
                    directions |= Directions.Left | Directions.Right;
                }
                else if (i == 0)
                {
                    directions |= Directions.Left;
                }
                else if (i == to - 1 && to == panelSizes.Count)
                {
                    directions |= Directions.Right;
                }
                Directions directionsLocalScope = directions;

                var balcony = new List<Func<ILayoutElement>>
                {
                    () => new ProceduralFacadeCompoundElements.ProceduralBalcony(config, directionsLocalScope)
                };
                horizontal.Add(Construct(balcony, panelWidth, height));
            }
            return horizontal;
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
        }

        private enum PanelType : byte
        {
            k_FullWindow,
            k_Wall,
            k_ShadowWall,
            k_Window,
        }
    }
}
