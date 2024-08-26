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
        private const float k_MinBuildingHeight = 5f;
        private const float k_MaxBuildingHeight = 100f;
        private const float k_FloorHeight = 2.5f;
        private const float k_BufferWidth = 2;

        private readonly Dictionary<PanelType, List<Func<ILayoutElement>>> m_Constructors = new();
        private readonly Dictionary<PanelSize, float> m_SizeValues = new()
        {
            {PanelSize.k_Wide, 2},
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
                    m_WindowFrameRodWidth = 0.2f,
                    m_WindowFrameRodHeight = 0.3f,
                    m_NumCenterRods = 1
                }
            };
            m_Constructors[PanelType.k_Wall] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralWall(config)
            };
            m_Constructors[PanelType.k_SmallFullWindow] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralFullWindow(
                    config,
                    "SmallFullWindowTextured",
                    config.officeBuildingMaterialPalette.windowGlassB
                    )
                {
                    m_WindowFrameRodWidth = 0.2f,
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
            horizontal.Add(CreateEntranceVertical(m_SizeValues[panelSizes[entranceIndexInterval]], floorWidthOffset, config));
            horizontal.Add(CreateEntranceNormalFacadeVertical(panelSizes, floorWidthOffset, entranceIndexInterval + 1, panelSizes.Count, config));

            return horizontal;
        }

        private VerticalLayout CreateNormalFacadeVertical(List<PanelSize> panelSizes, float remainderWidth, int from, int to, BuildingGenerator.Config config)
        {
            float floorWidthOffset = remainderWidth / (to - from);
            var vertical = new VerticalLayout { CreateHorizontal(panelSizes, from, to, k_FloorHeight * 2, floorWidthOffset, m_Constructors[PanelType.k_FullWindow]) };

            float remainingHeight = config.buildingHeight - k_MinBuildingHeight;
            if (0 < remainingHeight)
            {
                int switchIndex = 0;
                while (0 < remainingHeight)
                {
                    switch (switchIndex++ % 2)
                    {
                        case 0:
                            remainingHeight -= config.officeBuildingParams.smallWindowHeight;
                            vertical.Add(remainingHeight < 0
                                ? CreateHorizontal(panelSizes, from, to, config.officeBuildingParams.smallWindowHeight + remainingHeight, floorWidthOffset, m_Constructors[PanelType.k_SmallFullWindow])
                                : CreateHorizontal(panelSizes, from, to, config.officeBuildingParams.smallWindowHeight, floorWidthOffset, m_Constructors[PanelType.k_SmallFullWindow]));
                            break;
                        case 1:
                            remainingHeight -= k_FloorHeight;
                            vertical.Add(remainingHeight < 0
                                ? CreateHorizontal(panelSizes, from, to, k_FloorHeight + remainingHeight, floorWidthOffset, m_Constructors[PanelType.k_FullWindow])
                                : CreateHorizontal(panelSizes, from, to, k_FloorHeight, floorWidthOffset, m_Constructors[PanelType.k_FullWindow]));
                            break;
                    }
                }
            }

            return vertical;
        }

        private VerticalLayout CreateEntranceNormalFacadeVertical(List<PanelSize> panelSizes, float floorWidthOffset, int from, int to, BuildingGenerator.Config config)
        {
            var vertical = new VerticalLayout();
            if (config.officeBuildingParams.entranceWindow)
            {
                vertical.Add(CreateHorizontal(panelSizes, from, to, k_FloorHeight * 2, floorWidthOffset, m_Constructors[PanelType.k_FullWindow]));
            }
            else
            {
                vertical.Add(CreateHorizontal(panelSizes, from, to, k_FloorHeight * 2, floorWidthOffset, m_Constructors[PanelType.k_Wall]));
            }

            float remainingHeight = config.buildingHeight - k_MinBuildingHeight;
            if (0 < remainingHeight)
            {
                int switchIndex = 0;
                while (0 < remainingHeight)
                {
                    switch (switchIndex++ % 2)
                    {
                        case 0:
                            remainingHeight -= config.officeBuildingParams.smallWindowHeight;
                            vertical.Add(remainingHeight < 0
                                ? CreateHorizontal(panelSizes, from, to, config.officeBuildingParams.smallWindowHeight + remainingHeight, floorWidthOffset, m_Constructors[PanelType.k_SmallFullWindow])
                                : CreateHorizontal(panelSizes, from, to, config.officeBuildingParams.smallWindowHeight, floorWidthOffset, m_Constructors[PanelType.k_SmallFullWindow]));
                            break;
                        case 1:
                            remainingHeight -= k_FloorHeight;
                            vertical.Add(remainingHeight < 0
                                ? CreateHorizontal(panelSizes, from, to, k_FloorHeight + remainingHeight, floorWidthOffset, m_Constructors[PanelType.k_FullWindow])
                                : CreateHorizontal(panelSizes, from, to, k_FloorHeight, floorWidthOffset, m_Constructors[PanelType.k_FullWindow]));
                            break;
                    }
                }
            }

            return vertical;
        }

        private VerticalLayout CreateEntranceVertical(float width, float floorWidthOffset, BuildingGenerator.Config config)
        {
            var vertical = new VerticalLayout();
            if (config.officeBuildingParams.entranceWindow)
            {
                vertical.Add(Construct(m_Constructors[PanelType.k_Entrance], width + floorWidthOffset, k_FloorHeight));
                vertical.Add(Construct(m_Constructors[PanelType.k_FullWindow], width + floorWidthOffset, k_FloorHeight));
            }
            else
            {
                vertical.Add(Construct(m_Constructors[PanelType.k_Entrance], width + floorWidthOffset, k_FloorHeight));
                vertical.Add(Construct(m_Constructors[PanelType.k_Wall], width + floorWidthOffset, k_FloorHeight));
            }

            float remainingHeight = config.buildingHeight - k_MinBuildingHeight;
            if (0 < remainingHeight)
            {
                int switchIndex = 0;
                while (0 < remainingHeight)
                {
                    switch (switchIndex++ % 2)
                    {
                        case 0:
                            remainingHeight -= config.officeBuildingParams.smallWindowHeight;
                            vertical.Add(remainingHeight < 0
                                ? Construct(m_Constructors[PanelType.k_SmallFullWindow], width + floorWidthOffset, config.officeBuildingParams.smallWindowHeight + remainingHeight)
                                : Construct(m_Constructors[PanelType.k_SmallFullWindow], width + floorWidthOffset, config.officeBuildingParams.smallWindowHeight));
                            break;
                        case 1:
                            remainingHeight -= k_FloorHeight;
                            vertical.Add(remainingHeight < 0
                                ? Construct(m_Constructors[PanelType.k_FullWindow], width + floorWidthOffset, k_FloorHeight + remainingHeight)
                                : Construct(m_Constructors[PanelType.k_FullWindow], width + floorWidthOffset, k_FloorHeight));
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
            k_SmallFullWindow,
            k_FullWindow,
        }
    }
}
