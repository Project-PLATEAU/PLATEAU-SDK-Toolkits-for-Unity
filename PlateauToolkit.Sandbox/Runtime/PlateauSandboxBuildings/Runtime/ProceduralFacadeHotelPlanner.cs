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
    [CreateAssetMenu(menuName = "ProceduralToolkit/Buildings/Procedural Facade Planner/Hotel", order = 0)]
    public class ProceduralFacadeHotelPlanner : FacadePlanner
    {
        private const float k_MaxBuildingHeight = 100f;
        private const float k_FloorHeight = 3.5f;
        private const float k_BufferWidth = 2;
        private const float k_WindowBottomOffset = 1;
        private const float k_WindowTopOffset = 0.2f;
        private const float k_WindowFrameRodHeight = 0.05f;

        private readonly Dictionary<PanelType, List<Func<ILayoutElement>>> m_Constructors = new();
        private readonly Dictionary<PanelSize, float> m_SizeValues = new()
        {
            {PanelSize.k_Narrow, 2.5f}
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
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Front;
                        layouts.Add(PlanEntranceFacade(width, config, leftIsConvex, rightIsConvex));
                        break;
                    case 1:
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Left;
                        layouts.Add(PlanNormalFacade(width, config, leftIsConvex, rightIsConvex));
                        break;
                    case 2:
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Back;
                        layouts.Add(PlanNormalFacade(width, config, leftIsConvex, rightIsConvex));
                        break;
                    case 3:
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Right;
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
                () => new ProceduralFacadeCompoundElements.ProceduralWindow(config)
                {
                    m_WindowBottomOffset = 0,
                    m_WindowWidthOffset = 0.3f,
                    m_WindowDepthOffset = 0,
                    m_WindowTopOffset = 1.2f,
                    m_WindowFrameRodHeight = k_WindowFrameRodHeight,
                    m_NumCenterRods = 1,
                    m_HasWindowsill = false
                }
            };
            m_Constructors[PanelType.k_Wall] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralWall(config)
            };
            m_Constructors[PanelType.k_Window] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralWindow(config)
                {
                    m_WindowBottomOffset = k_WindowBottomOffset,
                    m_WindowTopOffset = k_WindowTopOffset,
                    m_WindowFrameRodHeight = k_WindowFrameRodHeight,
                    m_NumCenterRods = 0,
                    m_HasWindowsill = false,
                    m_RectangleWindow = true
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
            List<PanelSize> panelSizes = DivideFacade(facadeWidth, leftIsConvex, rightIsConvex, out float remainder);

            var horizontal = new HorizontalLayout();

            float remainingHeight = config.buildingHeight - k_FloorHeight;
            int numFloor = (int)(remainingHeight / 2f);
            float floorHeight = remainingHeight / numFloor;

            bool hasRemainder = remainder > Geometry.Epsilon;
            if (hasRemainder)
            {
                horizontal.Add(CreateBufferWallVertical(remainder/2, floorHeight, numFloor, config));
            }

            const int entranceCount = 1;
            int entranceIndexInterval = (panelSizes.Count - entranceCount)/(entranceCount + 1);
            const int lastEntranceIndex = -1;
            horizontal.Add(CreateEntranceNormalFacadeVertical(panelSizes, lastEntranceIndex + 1, entranceIndexInterval, config));
            horizontal.Add(CreateEntranceVertical(m_SizeValues[panelSizes[entranceIndexInterval]], config));
            horizontal.Add(CreateEntranceNormalFacadeVertical(panelSizes, entranceIndexInterval + 1, panelSizes.Count, config));

            if (hasRemainder)
            {
                horizontal.Add(CreateBufferWallVertical(remainder/2, floorHeight, numFloor, config));
            }

            return horizontal;
        }

        private VerticalLayout CreateBufferWallVertical(float width, float height, int numFloor, BuildingGenerator.Config config)
        {
            var vertical = new VerticalLayout
            {
                Construct(m_Constructors[PanelType.k_Wall], width, k_FloorHeight),
                CreateVertical(width, height, numFloor, () => new ProceduralFacadeCompoundElements.ProceduralWall(config))
            };

            return vertical;
        }

        private VerticalLayout CreateNormalFacadeVertical(List<PanelSize> panelSizes, float remainderWidth, int from, int to, BuildingGenerator.Config config)
        {
            float remainingHeight = config.buildingHeight - k_FloorHeight;
            int numFloor = (int)(remainingHeight / 2f);
            float floorHeight = remainingHeight / numFloor;
            float floorWidthOffset = remainderWidth / (to - from);

            var vertical = new VerticalLayout { CreateHorizontal(panelSizes, from, to, k_FloorHeight, floorWidthOffset, m_Constructors[PanelType.k_Wall]) };
            bool hasBalcony = config.faceDirection == BuildingGenerator.Config.FaceDirection.k_Left && config.hotelParams.hasBalconyLeft ||
                              config.faceDirection == BuildingGenerator.Config.FaceDirection.k_Right && config.hotelParams.hasBalconyRight;

            {
                Directions directions = Directions.Down;
                if (numFloor == 1)
                {
                    directions |= Directions.Up;
                }

                vertical.Add(hasBalcony
                    ? CreateHorizontalBalcony(panelSizes, from, to, floorHeight, floorWidthOffset, config, directions)
                    : CreateHorizontal(panelSizes, from, to, floorHeight, floorWidthOffset, m_Constructors[PanelType.k_Window]));
            }

            for (int i = 1; i < numFloor; i++)
            {
                if (hasBalcony)
                {
                    Directions directions = Directions.None;
                    if (numFloor == i + 1)
                    {
                        directions = Directions.Up;
                    }
                    vertical.Add(CreateHorizontalBalcony(panelSizes, from, to, floorHeight, floorWidthOffset, config, directions));
                }
                else
                {
                    vertical.Add(CreateHorizontal(panelSizes, from, to, floorHeight, floorWidthOffset, m_Constructors[PanelType.k_Window]));
                }
            }

            return vertical;
        }

        private VerticalLayout CreateEntranceNormalFacadeVertical(List<PanelSize> panelSizes, int from, int to, BuildingGenerator.Config config)
        {
            var vertical = new VerticalLayout { CreateHorizontal(panelSizes, from, to, k_FloorHeight, 0, m_Constructors[PanelType.k_Wall]) };
            float remainingHeight = config.buildingHeight - k_FloorHeight;
            int numFloor = (int)(remainingHeight / 2f);
            float floorHeight = remainingHeight / numFloor;
            vertical.Add(CreateHorizontal(panelSizes, from, to, floorHeight, 0, m_Constructors[PanelType.k_Window]));

            for (int i = 1; i < numFloor; i++)
            {
                vertical.Add(CreateHorizontal(panelSizes, from, to, floorHeight, 0, m_Constructors[PanelType.k_Window]));
            }

            return vertical;
        }

        private VerticalLayout CreateEntranceVertical(float width, BuildingGenerator.Config config)
        {
            var vertical = new VerticalLayout { Construct(m_Constructors[PanelType.k_Entrance], width, k_FloorHeight) };
            float remainingHeight = config.buildingHeight - k_FloorHeight;
            int numFloor = (int)(remainingHeight / 2f);
            float floorHeight = remainingHeight / numFloor;
            vertical.Add(Construct(m_Constructors[PanelType.k_Window], width, floorHeight));

            for (int i = 1; i < numFloor; i++)
            {
                vertical.Add(Construct(m_Constructors[PanelType.k_Window], width, floorHeight));
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

        private VerticalLayout CreateVertical(float width, float height, int floors, Func<ILayoutElement> constructor)
        {
            var verticalLayout = new VerticalLayout();
            for (int i = 0; i < floors; i++)
            {
                verticalLayout.Add(Construct(constructor, width, height));
            }
            return verticalLayout;
        }

        private HorizontalLayout CreateHorizontal(List<PanelSize> panelSizes, int from, int to, float height, float widthOffset, List<Func<ILayoutElement>> constructors)
        {
            var horizontal = new HorizontalLayout();
            for (int i = from; i < to; i++)
            {
                float panelWidth = m_SizeValues[panelSizes[i]] + widthOffset;
                horizontal.Add(Construct(constructors, panelWidth, height));
            }
            return horizontal;
        }

        private HorizontalLayout CreateHorizontalBalcony(List<PanelSize> panelSizes, int from, int to, float height, float widthOffset, BuildingGenerator.Config config, Directions directions)
        {
            var horizontal = new HorizontalLayout();
            for (int i = from; i < to; i++)
            {
                float panelWidth = m_SizeValues[panelSizes[i]] + widthOffset;
                if (i == 0)
                {
                    directions |= Directions.Left;
                }
                else if (i == panelSizes.Count - 1)
                {
                    directions |= Directions.Right;
                }
                else
                {
                    directions &= ~Directions.Left;
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
            k_Window,
        }
    }
}
