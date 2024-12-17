using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Configs;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces;
using ProceduralToolkit;
using ProceduralToolkit.Buildings;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime
{
    [CreateAssetMenu(menuName = "ProceduralToolkit/Buildings/Procedural Facade Planner/Complex Building", order = 7)]
    public class ProceduralFacadeComplexBuildingPlanner : FacadePlanner
    {
        private const float k_MaxBuildingHeight = 100f;
        private const float k_MinFloorHeight = 2.75f;
        private const float k_MaxFloorHeight = 3.25f;
        private const float k_SmallWallHeight = 1.0f;
        private const float k_DepressionWallHeight = 1.0f;
        private const float k_SmallWindowHeight = 1.5f;
        private const float k_BufferWidth = 2;
        private const float k_ShadowWallOffset = 0.65f;
        private const float k_EntranceWindowHeight = 2.5f;
        private const float k_MinSkyscraperCondominiumWallWidthOffset = 1.25f;
        private const float k_SkyscraperCondominiumWindowFrameRodHeight = 0.05f;
        private const int   k_RoundDigit = 3;
        private const float k_NarrowPanelSize = 2.5f;
        private const float k_MinWallWidthOffset = 1.25f;

        private readonly Dictionary<PanelType, List<Func<ILayoutElement>>> m_Constructors = new();
        private readonly Dictionary<PanelSize, float> m_SizeValues = new()
        {
            {PanelSize.k_Narrow, 2.5f},
        };

        private int m_NumFloorWithoutEntrance;
        private int m_NumHigherFloor;
        private int m_NumLowerFloorWithoutEntrance;
        private float m_FloorHeight;
        private float m_HigherFloorHeight;
        private float m_LowerFloorHeight;

        private ComplexBuildingConfig.ComplexBuildingType GetComplexBuildingType (BuildingGenerator.Config config)
        {
            return config.m_ComplexBuildingPlannerParams.m_AddedBoundaryWall ? config.complexBuildingParams.higherFloorBuildingType : config.complexBuildingParams.lowerFloorBuildingType;
        }

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

                // 小数点が最も小さい（フロア数を求めた時に最も正確に割り切れるかを表す）フロア数から最大のフロア高を求める（壁のように大きさで形状が決まらない時に利用）
                float floorHeightRemaining = 1f;
                for (float tempFloorHeight = k_MinFloorHeight; tempFloorHeight < k_MaxFloorHeight;)
                {
                    float numFloor = config.buildingHeight / tempFloorHeight;
                    if (numFloor - Mathf.Floor(numFloor) < floorHeightRemaining)
                    {
                        m_FloorHeight = tempFloorHeight;
                        floorHeightRemaining = numFloor - Mathf.Floor(numFloor);
                    }
                    tempFloorHeight += 0.05f;
                }

                // 小数点が最も小さい（フロア数を求めた時に最も正確に割り切れるかを表す）フロア数から最大のフロア高を求める（窓のように大きさで形状が変更される時に利用）
                float higherFloorHeightRemaining = 1f;
                for (float tempFloorHeight = k_MinFloorHeight; tempFloorHeight < k_MaxFloorHeight;)
                {
                    float numFloor = (config.buildingHeight - config.complexBuildingParams.buildingBoundaryHeight - k_DepressionWallHeight) / tempFloorHeight;
                    if (numFloor - Mathf.Floor(numFloor) < higherFloorHeightRemaining)
                    {
                        m_HigherFloorHeight = tempFloorHeight;
                        higherFloorHeightRemaining = numFloor - Mathf.Floor(numFloor);
                    }
                    tempFloorHeight += 0.05f;
                }

                // 小数点が最も小さい（フロア数を求めた時に最も正確に割り切れるかを表す）フロア数から最大のフロア高を求める（窓のように大きさで形状が変更される時に利用）
                float lowerFloorHeightRemaining = 1f;
                for (float tempFloorHeight = k_MinFloorHeight; tempFloorHeight < k_MaxFloorHeight;)
                {
                    float buildingHeight = Math.Min(config.complexBuildingParams.buildingBoundaryHeight, config.buildingHeight);
                    float numFloor = buildingHeight / tempFloorHeight;
                    if (numFloor - Mathf.Floor(numFloor) < lowerFloorHeightRemaining)
                    {
                        m_LowerFloorHeight = tempFloorHeight;
                        lowerFloorHeightRemaining = numFloor - Mathf.Floor(numFloor);
                    }
                    tempFloorHeight += 0.05f;
                }

                m_NumFloorWithoutEntrance = (int)Mathf.Floor(config.buildingHeight / m_FloorHeight) - 1;
                m_NumHigherFloor = Math.Max(0, (int)Mathf.Floor((config.buildingHeight - config.complexBuildingParams.buildingBoundaryHeight - k_DepressionWallHeight) / m_HigherFloorHeight));
                m_NumLowerFloorWithoutEntrance = Math.Max(0, (int)Mathf.Floor(config.complexBuildingParams.buildingBoundaryHeight / m_LowerFloorHeight) - 1);

                config.m_ComplexBuildingPlannerParams.m_AddedBoundaryWall = false;
                switch (i)
                {
                    case 0:
                    {
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Back;
                        var vertical = new VerticalLayout();
                        vertical.AddElement(Construct(m_Constructors[PanelType.k_ShadowWall], width - k_ShadowWallOffset, config.buildingHeight - k_ShadowWallOffset));
                        vertical.Add(PlanNormalFacade(width, config, leftIsConvex, rightIsConvex));
                        layouts.Add(vertical);
                        break;
                    }
                    case 1:
                    {
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Right;
                        var vertical = new VerticalLayout();
                        vertical.AddElement(Construct(m_Constructors[PanelType.k_ShadowWall], width - k_ShadowWallOffset, config.buildingHeight - k_ShadowWallOffset));
                        vertical.Add(PlanNormalFacade(width, config, leftIsConvex, rightIsConvex));
                        layouts.Add(vertical);
                        break;
                    }
                    case 2:
                    {
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Front;
                        var vertical = new VerticalLayout();
                        vertical.AddElement(Construct(m_Constructors[PanelType.k_ShadowWall], width - k_ShadowWallOffset, config.buildingHeight - k_ShadowWallOffset));
                        vertical.Add(PlanEntranceFacade(width, config, leftIsConvex, rightIsConvex));
                        layouts.Add(vertical);
                        break;
                    }
                    case 3:
                    {
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Left;
                        var vertical = new VerticalLayout();
                        vertical.AddElement(Construct(m_Constructors[PanelType.k_ShadowWall], width - k_ShadowWallOffset, config.buildingHeight - k_ShadowWallOffset));
                        vertical.Add(PlanNormalFacade(width, config, leftIsConvex, rightIsConvex));
                        layouts.Add(vertical);
                        break;
                    }
                }
            }

            return layouts;
        }

        private void SetupConstructors(BuildingGenerator.Config config)
        {
            string shadowWallName;
            switch (config.complexBuildingParams.lowerFloorBuildingType)
            {
                case ComplexBuildingConfig.ComplexBuildingType.k_Apartment:
                    shadowWallName = "SkyscraperCondominiumWallTextured";
                    break;
                case ComplexBuildingConfig.ComplexBuildingType.k_OfficeBuilding:
                    shadowWallName = "OfficeBuildingWallTextured";
                    break;
                case ComplexBuildingConfig.ComplexBuildingType.k_CommercialBuilding:
                    shadowWallName = "CommercialBuildingWallTextured";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            m_Constructors[PanelType.k_ShadowWall] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralWall(config)
                {
                    m_WallName = shadowWallName,
                    m_IsShadowWall = true,
                    m_MoveShadowWallDepth = 0.7f,
                    m_ShadowWallWidthOffset = k_ShadowWallOffset,
                    m_ShadowWallHeightOffset = 0
                }
            };

            m_Constructors[PanelType.k_SkyscraperCondominiumWall] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralWall(config)
                {
                    m_WallName = "SkyscraperCondominiumWallTextured"
                }
            };

            m_Constructors[PanelType.k_SkyscraperCondominiumWindow] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralWindow(
                    config,
                    windowpaneFrameName: "SkyscraperCondominiumFrameTextured")
                {
                    m_WindowBottomOffset = 0,
                    m_WindowTopOffset = 0,
                    m_WindowFrameRodHeight = k_SkyscraperCondominiumWindowFrameRodHeight,
                    m_NumCenterRods = 1,
                    m_WindowFrameRodType = ProceduralFacadeElement.WindowFrameRodType.k_Vertical,
                    m_HasWindowsill = false,
                    m_IsRectangleWindow = true
                }
            };
            m_Constructors[PanelType.k_SkyscraperCondominiumFullWindow] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralFullWindow(
                    config,
                    windowpaneFrameName: "SkyscraperCondominiumFrameTextured")
                {
                    m_WindowFrameRodWidth = 0.2f
                }
            };

            m_Constructors[PanelType.k_CommercialFullWindow] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralFullWindow(
                    config,
                    windowpaneFrameName: "CommercialBuildingFrameTextured")
                {
                    m_WindowFrameRodWidth = 0.2f
                }
            };

            m_Constructors[PanelType.k_CommercialSmallFullWindow] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralFullWindow(
                    config,
                    windowpaneFrameName: "CommercialBuildingFrameTextured")
                {
                    m_NumCenterRods = 1,
                    m_WindowFrameRodType = ProceduralFacadeElement.WindowFrameRodType.k_Vertical,
                    m_WindowFrameRodWidth = 0.05f
                }
            };

            m_Constructors[PanelType.k_CommercialWallWithFrame] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralWallWithFrame(
                    config,
                    wallName: "CommercialBuildingWallTextured",
                    windowpaneFrameName: "CommercialBuildingFrameTextured")
                {
                    m_NumCenterRods = 0
                }
            };

            m_Constructors[PanelType.k_OfficeFullWindow] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralFullWindow(
                    config,
                    windowpaneFrameName: "OfficeBuildingFrameTextured")
                {
                    m_WindowFrameRodWidth = 0.2f
                }
            };

            m_Constructors[PanelType.k_OfficeSmallFullWindow] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralFullWindow(
                    config,
                    config.complexBuildingMaterialPalette.officeBuildingSpandrel,
                    "OfficeBuildingGlassSpandrelTextured",
                    "OfficeBuildingFrameTextured")
                {
                    m_WindowFrameRodWidth = 0.2f,
                }
            };
        }

        private ILayout PlanEntranceFacade(float facadeWidth, BuildingGenerator.Config config, bool leftIsConvex, bool rightIsConvex)
        {
            List<PanelSize> panelSizes = DivideFacade(facadeWidth, leftIsConvex, rightIsConvex, out float remainderWidth);
            var horizontal = new HorizontalLayout();
            const int entranceCount = 1;
            int entranceIndexInterval = (panelSizes.Count - entranceCount)/(entranceCount + 1);
            float floorWidthOffset = remainderWidth / panelSizes.Count;
            horizontal.Add(CreateEntranceVertical(panelSizes, remainderWidth, floorWidthOffset, entranceIndexInterval, 0 == entranceIndexInterval, entranceIndexInterval + 1 == panelSizes.Count, config));

            return horizontal;
        }

        private HorizontalLayout CreateEntranceVertical(List<PanelSize> panelSizes, float remainderWidth, float floorWidthOffset, int entranceIndexInterval, bool noLeftLayout, bool noRightLayout, BuildingGenerator.Config config)
        {
            // マンションタイプはHorizontalで生成しているので、下部の建造物も横幅を合わせるためにHorizontalで生成すること
            // つまり、from-toは必ず0, panelSizes.Countになる
            ComplexBuildingConfig.ComplexBuildingType complexBuildingType = GetComplexBuildingType(config);
            var horizontal = new HorizontalLayout();
            switch (complexBuildingType)
            {
                case ComplexBuildingConfig.ComplexBuildingType.k_Apartment:
                    config.m_ComplexBuildingPlannerParams.m_AddedBoundaryWall = false;
                    horizontal.Add(CreateSkyscraperCondominiumEntranceVertical(panelSizes, remainderWidth, floorWidthOffset, entranceIndexInterval, config));
                    break;
                case ComplexBuildingConfig.ComplexBuildingType.k_OfficeBuilding:
                    config.m_ComplexBuildingPlannerParams.m_AddedBoundaryWall = false;
                    horizontal.Add(CreateOfficeEntranceVertical(panelSizes, remainderWidth, floorWidthOffset, entranceIndexInterval, config));
                    break;
                case ComplexBuildingConfig.ComplexBuildingType.k_CommercialBuilding:
                    config.m_ComplexBuildingPlannerParams.m_AddedBoundaryWall = false;
                    horizontal.Add(CreateCommercialEntranceVertical(panelSizes, remainderWidth, floorWidthOffset, entranceIndexInterval, noLeftLayout, noRightLayout, config));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return horizontal;
        }

        private VerticalLayout CreateSkyscraperCondominiumEntranceVertical(List<PanelSize> panelSizes, float remainderWidth, float floorWidthOffset, int entranceIndexInterval, BuildingGenerator.Config config)
        {
            float entranceHeight = config.complexBuildingParams.buildingBoundaryHeight <= config.buildingHeight ? config.complexBuildingParams.buildingBoundaryHeight - m_NumLowerFloorWithoutEntrance * m_LowerFloorHeight : config.buildingHeight - m_NumFloorWithoutEntrance * m_FloorHeight;
            float wallWidthOffset = Mathf.Max(k_MinSkyscraperCondominiumWallWidthOffset, remainderWidth);
            float wallAveWidthOffset = wallWidthOffset / panelSizes.Count;
            var vertical = new VerticalLayout();
            var horizontal = new HorizontalLayout
            {
                Construct(m_Constructors[PanelType.k_SkyscraperCondominiumWall], wallWidthOffset * 0.5f, entranceHeight),
                CreateHorizontal(panelSizes, 0, entranceIndexInterval, entranceHeight, floorWidthOffset - wallAveWidthOffset, m_Constructors[PanelType.k_SkyscraperCondominiumWall]),
                Construct(new List<Func<ILayoutElement>>
                {
                    () => new ProceduralFacadeCompoundElements.ProceduralWindow(config)
                    {
                        m_WindowBottomOffset = 0,
                        m_WindowWidthOffset = 0.3f,
                        m_WindowDepthOffset = 0,
                        m_WindowTopOffset = 0 < entranceHeight - k_EntranceWindowHeight ? entranceHeight - k_EntranceWindowHeight : Math.Abs(entranceHeight - k_EntranceWindowHeight),
                        m_WindowFrameRodHeight = k_SkyscraperCondominiumWindowFrameRodHeight,
                        m_NumCenterRods = 1,
                        m_HasWindowsill = false
                    }
                }, m_SizeValues[panelSizes[entranceIndexInterval]] + floorWidthOffset - wallAveWidthOffset, entranceHeight),
                CreateHorizontal(panelSizes, entranceIndexInterval + 1, panelSizes.Count, entranceHeight, floorWidthOffset - wallAveWidthOffset, m_Constructors[PanelType.k_SkyscraperCondominiumWall]),
                Construct(m_Constructors[PanelType.k_SkyscraperCondominiumWall], wallWidthOffset * 0.5f, entranceHeight)
            };
            vertical.Add(horizontal);

            float remainingHeight = config.buildingHeight - entranceHeight;
            float currentHeight = entranceHeight;
            if (0 < remainingHeight)
            {
                vertical.Add(CreateNormalFacadeVerticalIter(panelSizes, remainingHeight, remainderWidth, currentHeight, floorWidthOffset, entranceHeight, 0, panelSizes.Count, config));
            }

            return vertical;
        }

        private VerticalLayout CreateOfficeEntranceVertical(List<PanelSize> panelSizes, float remainderWidth, float floorWidthOffset, int entranceIndexInterval, BuildingGenerator.Config config)
        {
            float entranceHeight = config.complexBuildingParams.buildingBoundaryHeight <= config.buildingHeight ? config.complexBuildingParams.buildingBoundaryHeight - m_NumLowerFloorWithoutEntrance * m_LowerFloorHeight : config.buildingHeight - m_NumFloorWithoutEntrance * m_FloorHeight;
            var entranceVerticalLayout = new VerticalLayout
            {
                Construct(new List<Func<ILayoutElement>>
                {
                    () => new ProceduralFacadeCompoundElements.ProceduralFullWindow(
                        config,
                        windowpaneFrameName: "OfficeBuildingFrameTextured")
                    {
                        m_WindowBottomOffset = 0,
                        m_WindowWidthOffset = 0,
                        m_WindowDepthOffset = 0,
                        m_WindowFrameRodHeight = 0.2f,
                        m_WindowFrameRodWidth = 0.2f,
                        m_NumCenterRods = 1,
                        m_HasWindowsill = false
                    }
                }, m_SizeValues[panelSizes[entranceIndexInterval]] + floorWidthOffset, k_EntranceWindowHeight),
                Construct(m_Constructors[PanelType.k_OfficeFullWindow], m_SizeValues[panelSizes[entranceIndexInterval]] + floorWidthOffset, entranceHeight - k_EntranceWindowHeight)
            };
            var horizontal = new HorizontalLayout
            {
                CreateHorizontal(panelSizes, 0, entranceIndexInterval, entranceHeight, floorWidthOffset, m_Constructors[PanelType.k_OfficeFullWindow]),
                entranceVerticalLayout,
                CreateHorizontal(panelSizes, entranceIndexInterval + 1, panelSizes.Count, entranceHeight, floorWidthOffset, m_Constructors[PanelType.k_OfficeFullWindow])
            };
            var vertical = new VerticalLayout{ horizontal };

            float remainingHeight = config.buildingHeight - entranceHeight;
            float currentHeight = entranceHeight;
            vertical.Add(CreateNormalFacadeVerticalIter(panelSizes, remainingHeight, remainderWidth, currentHeight, floorWidthOffset, entranceHeight, 0, panelSizes.Count, config));

            return vertical;
        }

        private VerticalLayout CreateCommercialEntranceVertical(List<PanelSize> panelSizes, float remainderWidth, float floorWidthOffset, int entranceIndexInterval, bool noLeftLayout, bool noRightLayout, BuildingGenerator.Config config)
        {
            float entranceHeight = config.complexBuildingParams.buildingBoundaryHeight <= config.buildingHeight ? config.complexBuildingParams.buildingBoundaryHeight - m_NumLowerFloorWithoutEntrance * m_LowerFloorHeight : config.buildingHeight - m_NumFloorWithoutEntrance * m_FloorHeight;
            entranceHeight = (float)Math.Round(entranceHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
            var entranceVerticalLayout = new VerticalLayout
            {
                Construct(new List<Func<ILayoutElement>>
                {
                    () => new ProceduralFacadeCompoundElements.ProceduralFullWindow(
                        config,
                        windowpaneFrameName: "CommercialBuildingFrameTextured")
                    {
                        m_WindowBottomOffset = 0,
                        m_WindowWidthOffset = 0,
                        m_WindowDepthOffset = 0,
                        m_WindowFrameRodHeight = 0.2f,
                        m_WindowFrameRodWidth = 0.2f,
                        m_NumCenterRods = 1,
                        m_HasWindowsill = false
                    }
                }, m_SizeValues[panelSizes[entranceIndexInterval]] + floorWidthOffset, k_EntranceWindowHeight),
                Construct(m_Constructors[PanelType.k_CommercialFullWindow], m_SizeValues[panelSizes[entranceIndexInterval]] + floorWidthOffset, entranceHeight - k_EntranceWindowHeight),
            };

            var horizontal = new HorizontalLayout
            {
                CreateHorizontal(panelSizes, 0, entranceIndexInterval, entranceHeight, floorWidthOffset, m_Constructors[PanelType.k_CommercialFullWindow]),
                entranceVerticalLayout,
                CreateHorizontal(panelSizes, entranceIndexInterval + 1, panelSizes.Count, entranceHeight, floorWidthOffset, m_Constructors[PanelType.k_CommercialFullWindow])
            };
            var vertical = new VerticalLayout{ horizontal };

            float remainingHeight = config.buildingHeight - entranceHeight;
            float currentHeight = entranceHeight;
            if (0 < remainingHeight - k_SmallWallHeight)
            {
                remainingHeight -= k_SmallWallHeight;
                currentHeight += k_SmallWallHeight;
                vertical.Add(CreateHorizontal(panelSizes, 0, panelSizes.Count, k_SmallWallHeight, floorWidthOffset, m_Constructors[PanelType.k_CommercialWallWithFrame]));
                if (0 < remainingHeight - k_DepressionWallHeight)
                {
                    remainingHeight -= k_DepressionWallHeight;
                    currentHeight += k_DepressionWallHeight;
                    vertical.Add(CreateDepressionWallNormalFacadeHorizontal(panelSizes, 0, panelSizes.Count, k_DepressionWallHeight, floorWidthOffset, true, config));
                }
            }

            vertical.Add(CreateNormalFacadeVerticalIter(panelSizes, remainingHeight, remainderWidth, currentHeight, floorWidthOffset, entranceHeight, 0, panelSizes.Count, config));

            return vertical;
        }

        private ILayout PlanNormalFacade(float facadeWidth, BuildingGenerator.Config config, bool leftIsConvex, bool rightIsConvex)
        {
            List<PanelSize> panelSizes = DivideFacade(facadeWidth, leftIsConvex, rightIsConvex, out float remainderWidth);
            return CreateNormalFacadeVertical(panelSizes, remainderWidth, 0, panelSizes.Count, config);
        }

        private VerticalLayout CreateNormalFacadeVertical(List<PanelSize> panelSizes, float remainderWidth, int from, int to, BuildingGenerator.Config config)
        {
            var vertical = new VerticalLayout();
            float entranceHeight = config.complexBuildingParams.buildingBoundaryHeight <= config.buildingHeight ? config.complexBuildingParams.buildingBoundaryHeight - m_NumLowerFloorWithoutEntrance * m_LowerFloorHeight : config.buildingHeight - m_NumFloorWithoutEntrance * m_FloorHeight;
            entranceHeight = (float)Math.Round(entranceHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
            float floorWidthOffset = remainderWidth / (to - from);
            floorWidthOffset = (float)Math.Round(floorWidthOffset, k_RoundDigit, MidpointRounding.AwayFromZero);

            ComplexBuildingConfig.ComplexBuildingType complexBuildingType = GetComplexBuildingType(config);
            switch (complexBuildingType)
            {
                case ComplexBuildingConfig.ComplexBuildingType.k_Apartment:
                {
                    float buildingHeight = Math.Min(config.complexBuildingParams.buildingBoundaryHeight, config.buildingHeight);
                    int numFloorWithoutEntrance = (int)Mathf.Floor(buildingHeight / m_LowerFloorHeight) - 1;
                    entranceHeight = buildingHeight - numFloorWithoutEntrance * m_LowerFloorHeight;
                    entranceHeight = (float)Math.Round(entranceHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
                    vertical.Add(CreateHorizontal(panelSizes, from, to, entranceHeight, floorWidthOffset, m_Constructors[PanelType.k_SkyscraperCondominiumWall]));
                    float remainingHeight = config.buildingHeight - entranceHeight;
                    float currentHeight = entranceHeight;
                    remainingHeight = (float)Math.Round(remainingHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
                    currentHeight = (float)Math.Round(currentHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
                    vertical.Add(CreateNormalFacadeVerticalIter(panelSizes, remainingHeight, remainderWidth, currentHeight, floorWidthOffset, entranceHeight, from, to, config));
                    break;
                }
                case ComplexBuildingConfig.ComplexBuildingType.k_OfficeBuilding:
                {
                    vertical.Add(CreateHorizontal(panelSizes, from, to, entranceHeight, floorWidthOffset, m_Constructors[PanelType.k_OfficeFullWindow]));
                    float remainingHeight = config.buildingHeight - entranceHeight;
                    float currentHeight = entranceHeight;
                    vertical.Add(CreateNormalFacadeVerticalIter(panelSizes, remainingHeight, remainderWidth, currentHeight, floorWidthOffset, entranceHeight, from, to, config));
                    break;
                }
                case ComplexBuildingConfig.ComplexBuildingType.k_CommercialBuilding:
                {
                    vertical.Add(CreateHorizontal(panelSizes, from, to, entranceHeight, floorWidthOffset, m_Constructors[PanelType.k_CommercialFullWindow]));
                    float remainingHeight = config.buildingHeight - entranceHeight;
                    float currentHeight = entranceHeight;
                    if (0 < remainingHeight - k_SmallWallHeight)
                    {
                        remainingHeight -= k_SmallWallHeight;
                        currentHeight += k_SmallWallHeight;
                        vertical.Add(CreateHorizontal(panelSizes, from, to, k_SmallWallHeight, floorWidthOffset, m_Constructors[PanelType.k_CommercialWallWithFrame]));
                        if (0 < remainingHeight - k_DepressionWallHeight)
                        {
                            remainingHeight -= k_DepressionWallHeight;
                            currentHeight += k_DepressionWallHeight;
                            vertical.Add(CreateDepressionWallNormalFacadeHorizontal(panelSizes, from, to, k_DepressionWallHeight, floorWidthOffset, true, config));
                        }
                    }
                    vertical.Add(CreateNormalFacadeVerticalIter(panelSizes, remainingHeight, remainderWidth, currentHeight, floorWidthOffset, entranceHeight, from, to, config));
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return vertical;
        }

        private VerticalLayout CreateNormalFacadeVerticalIter(List<PanelSize> panelSizes, float remainingHeight, float remainderWidth, float currentHeight, float floorWidthOffset, float entranceHeight, int from, int to, BuildingGenerator.Config config)
        {
            var vertical = new VerticalLayout();
            int officeFloorIndex = 0;
            int commercialFloorIndex = 0;
            bool addedBoundaryWall = false;

            while (0 < remainingHeight)
            {
                ComplexBuildingConfig.ComplexBuildingType complexBuildingType = GetComplexBuildingType(config);
                (int, bool, float, float) tupleData;
                switch (complexBuildingType)
                {
                    case ComplexBuildingConfig.ComplexBuildingType.k_Apartment:
                    {
                        float floorHeight = addedBoundaryWall ? m_HigherFloorHeight : m_LowerFloorHeight;
                        tupleData = CreateSkyscraperCondominiumPlanner(vertical, complexBuildingType, addedBoundaryWall, floorHeight, currentHeight, remainingHeight, remainderWidth, panelSizes, floorWidthOffset, entranceHeight, from, to, config);
                        break;
                    }
                    case ComplexBuildingConfig.ComplexBuildingType.k_OfficeBuilding:
                    {
                        float floorHeight = addedBoundaryWall ? m_HigherFloorHeight : m_FloorHeight;
                        tupleData = CreateBuildingPlanner(vertical, complexBuildingType, officeFloorIndex, addedBoundaryWall, canAddLowerFloor: true, floorHeight, currentHeight, remainingHeight, panelSizes, floorWidthOffset, entranceHeight, from, to, config);
                        officeFloorIndex = tupleData.Item1;
                        break;
                    }
                    case ComplexBuildingConfig.ComplexBuildingType.k_CommercialBuilding:
                    {
                        float floorHeight = addedBoundaryWall ? m_HigherFloorHeight : m_FloorHeight;
                        tupleData = CreateBuildingPlanner(vertical, complexBuildingType, commercialFloorIndex, addedBoundaryWall, canAddLowerFloor: true, floorHeight, currentHeight, remainingHeight, panelSizes, floorWidthOffset, entranceHeight, from, to, config);
                        commercialFloorIndex = tupleData.Item1;
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                addedBoundaryWall = tupleData.Item2;
                currentHeight = tupleData.Item3;
                remainingHeight = tupleData.Item4;
            }

            return vertical;
        }

        private (int, bool, float, float) CreateSkyscraperCondominiumPlanner(VerticalLayout vertical, ComplexBuildingConfig.ComplexBuildingType complexBuildingType, bool addedBoundaryWall, float floorHeight, float currentHeight, float remainingHeight, float remainderWidth, List<PanelSize> panelSizes, float floorWidthOffset, float entranceHeight, int from, int to, BuildingGenerator.Config config)
        {
            (int, bool, float, float) tupleData;

            // for文での生成処理を行っていないので生成範囲がなければそのまま返す
            if (to == from)
            {
                currentHeight = config.buildingHeight;
                remainingHeight = 0;
                return (0, false, currentHeight, remainingHeight);
            }

            // 窓の高さは残りの高さで割って余りが出ないようにする
            if (addedBoundaryWall)
            {
                float buildingHeight = config.buildingHeight - config.complexBuildingParams.buildingBoundaryHeight - k_DepressionWallHeight;
                int numFloor = (int)Mathf.Floor(buildingHeight / floorHeight);
                float wallWidthOffset = Mathf.Max(k_MinWallWidthOffset, remainderWidth);
                float wallAveWidthOffset = wallWidthOffset / panelSizes.Count;
                bool hasBalcony = config.faceDirection == BuildingGenerator.Config.FaceDirection.k_Left && config.complexSkyscraperCondominiumBuildingParams.hasBalconyLeft ||
                                  config.faceDirection == BuildingGenerator.Config.FaceDirection.k_Right && config.complexSkyscraperCondominiumBuildingParams.hasBalconyRight ||
                                  config.faceDirection == BuildingGenerator.Config.FaceDirection.k_Front && config.complexSkyscraperCondominiumBuildingParams.hasBalconyFront ||
                                  config.faceDirection == BuildingGenerator.Config.FaceDirection.k_Back && config.complexSkyscraperCondominiumBuildingParams.hasBalconyBack;

                {
                    Directions directions = Directions.Down;
                    if (1 <= numFloor)
                    {
                        directions |= Directions.Up;

                        if (hasBalcony)
                        {
                            var horizontal = new HorizontalLayout();
                            if (0 == from && to == panelSizes.Count)
                            {
                                horizontal.Add(CreateHorizontal(panelSizes, 0, 1, floorHeight, -k_NarrowPanelSize + wallWidthOffset * 0.5f, m_Constructors[PanelType.k_SkyscraperCondominiumWall]));
                                horizontal.Add(CreateHorizontalBalcony(panelSizes, from, to, floorHeight, floorWidthOffset - wallAveWidthOffset, config, directions));
                                horizontal.Add(CreateHorizontal(panelSizes, 0, 1, floorHeight, -k_NarrowPanelSize + wallWidthOffset * 0.5f, m_Constructors[PanelType.k_SkyscraperCondominiumWall]));
                            }
                            else if (0 == from)
                            {
                                horizontal.Add(CreateHorizontal(panelSizes, 0, 1, floorHeight, -k_NarrowPanelSize + wallWidthOffset * 0.5f, m_Constructors[PanelType.k_SkyscraperCondominiumWall]));
                                horizontal.Add(CreateHorizontalBalcony(panelSizes, from, to, floorHeight, floorWidthOffset - wallAveWidthOffset, config, directions));
                            }
                            else if (to == panelSizes.Count)
                            {
                                horizontal.Add(CreateHorizontal(panelSizes, 0, 1, floorHeight, -k_NarrowPanelSize + wallWidthOffset * 0.5f, m_Constructors[PanelType.k_SkyscraperCondominiumWall]));
                            }
                            else
                            {
                                vertical.Add(CreateHorizontal(panelSizes, from, to, floorHeight, floorWidthOffset, m_Constructors[PanelType.k_SkyscraperCondominiumWindow]));
                            }

                            vertical.Add(horizontal);
                        }
                        else
                        {
                            vertical.Add(CreateHorizontal(panelSizes, from, to, floorHeight, floorWidthOffset, m_Constructors[PanelType.k_SkyscraperCondominiumWindow]));
                        }

                        remainingHeight -= floorHeight;
                        remainingHeight = (float)Math.Round(remainingHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
                    }
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
                        var horizontal = new HorizontalLayout
                        {
                            CreateHorizontal(panelSizes, 0, 1, floorHeight, -k_NarrowPanelSize + wallWidthOffset * 0.5f, m_Constructors[PanelType.k_SkyscraperCondominiumWall]),
                            CreateHorizontalBalcony(panelSizes, from, to, floorHeight, floorWidthOffset - wallAveWidthOffset, config, directions),
                            CreateHorizontal(panelSizes, 0, 1, floorHeight, -k_NarrowPanelSize + wallWidthOffset * 0.5f, m_Constructors[PanelType.k_SkyscraperCondominiumWall])
                        };
                        vertical.Add(horizontal);
                    }
                    else
                    {
                        vertical.Add(CreateHorizontal(panelSizes, from, to, floorHeight, floorWidthOffset, m_Constructors[PanelType.k_SkyscraperCondominiumWindow]));
                    }

                    remainingHeight -= floorHeight;
                    remainingHeight = (float)Math.Round(remainingHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
                }

                // 残りの高さは壁で埋める
                if (0 < remainingHeight)
                {
                    vertical.Add(CreateHorizontal(panelSizes, from, to, remainingHeight, floorWidthOffset, m_Constructors[PanelType.k_SkyscraperCondominiumWall]));
                }

                currentHeight = config.buildingHeight;
                remainingHeight = 0;

                return (0, true, currentHeight, remainingHeight);
            }
            else
            {
                float buildingHeight = Math.Min(config.complexBuildingParams.buildingBoundaryHeight, config.buildingHeight);
                int numFloorWithoutEntrance = (int)Mathf.Floor(buildingHeight / floorHeight) - 1;
                float wallWidthOffset = Mathf.Max(k_MinWallWidthOffset, remainderWidth);
                float wallAveWidthOffset = wallWidthOffset / (to - from);
                bool hasBalcony = config.faceDirection == BuildingGenerator.Config.FaceDirection.k_Left &&
                                  config.complexSkyscraperCondominiumBuildingParams.hasBalconyLeft ||
                                  config.faceDirection == BuildingGenerator.Config.FaceDirection.k_Right &&
                                  config.complexSkyscraperCondominiumBuildingParams.hasBalconyRight ||
                                  config.faceDirection == BuildingGenerator.Config.FaceDirection.k_Front &&
                                  config.complexSkyscraperCondominiumBuildingParams.hasBalconyFront ||
                                  config.faceDirection == BuildingGenerator.Config.FaceDirection.k_Back &&
                                  config.complexSkyscraperCondominiumBuildingParams.hasBalconyBack;

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
                            CreateHorizontal(panelSizes, 0, 1, floorHeight, -k_NarrowPanelSize + wallWidthOffset * 0.5f, m_Constructors[PanelType.k_SkyscraperCondominiumWall]),
                            CreateHorizontalBalcony(panelSizes, from, to, floorHeight, floorWidthOffset - wallAveWidthOffset, config, directions),
                            CreateHorizontal(panelSizes, 0, 1, floorHeight, -k_NarrowPanelSize + wallWidthOffset * 0.5f, m_Constructors[PanelType.k_SkyscraperCondominiumWall])
                        };
                        vertical.Add(horizontal);
                    }
                    else
                    {
                        vertical.Add(CreateHorizontal(panelSizes, from, to, floorHeight, floorWidthOffset, m_Constructors[PanelType.k_SkyscraperCondominiumWindow]));
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
                            CreateHorizontal(panelSizes, 0, 1, floorHeight, -k_NarrowPanelSize + wallWidthOffset * 0.5f, m_Constructors[PanelType.k_SkyscraperCondominiumWall]),
                            CreateHorizontalBalcony(panelSizes, from, to, floorHeight, floorWidthOffset - wallAveWidthOffset, config, directions),
                            CreateHorizontal(panelSizes, 0, 1, floorHeight, -k_NarrowPanelSize + wallWidthOffset * 0.5f, m_Constructors[PanelType.k_SkyscraperCondominiumWall])
                        };
                        vertical.Add(horizontal);
                    }
                    else
                    {
                        vertical.Add(CreateHorizontal(panelSizes, from, to, floorHeight, floorWidthOffset, m_Constructors[PanelType.k_SkyscraperCondominiumWindow]));
                    }
                }

                currentHeight = config.complexBuildingParams.buildingBoundaryHeight;
                remainingHeight = config.buildingHeight - currentHeight;

                if (config.complexBuildingParams.buildingBoundaryHeight < config.buildingHeight)
                {
                    // 境界線を作成するだけに利用
                    tupleData = CreateBuildingPlanner(vertical, complexBuildingType, floorIndex: 0, addedBoundaryWall: false, canAddLowerFloor: false, floorHeight: 0, currentHeight, remainingHeight, panelSizes, floorWidthOffset, entranceHeight, from, to, config);
                    currentHeight = tupleData.Item3;
                    remainingHeight = tupleData.Item4;
                }

                return (0, true, currentHeight, remainingHeight);
            }
        }

        private (int, bool, float, float) CreateBuildingPlanner(VerticalLayout vertical, ComplexBuildingConfig.ComplexBuildingType complexBuildingType, int floorIndex, bool addedBoundaryWall, bool canAddLowerFloor, float floorHeight, float currentHeight, float remainingHeight, List<PanelSize> panelSizes, float floorWidthOffset, float entranceHeight, int from, int to, BuildingGenerator.Config config)
        {
            if (remainingHeight <= Geometry.Epsilon)
            {
                return (floorIndex, addedBoundaryWall, currentHeight, remainingHeight);
            }

            PanelType panelType;
            float panelHeight;
            switch (complexBuildingType)
            {
                case ComplexBuildingConfig.ComplexBuildingType.k_Apartment:
                    panelType = floorHeight <= remainingHeight ? PanelType.k_SkyscraperCondominiumWindow : PanelType.k_SkyscraperCondominiumWall;
                    panelHeight = floorHeight;
                    panelHeight = (float)Math.Round(panelHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
                    break;
                case ComplexBuildingConfig.ComplexBuildingType.k_OfficeBuilding:
                    panelType = floorIndex % 2 == 0 ? PanelType.k_OfficeSmallFullWindow : PanelType.k_OfficeFullWindow;
                    panelHeight = floorIndex % 2 == 0 ? config.complexOfficeBuildingParams.spandrelHeight : entranceHeight;
                    panelHeight = (float)Math.Round(panelHeight, 2, MidpointRounding.AwayFromZero); // 小数点第二位まで必要
                    break;
                case ComplexBuildingConfig.ComplexBuildingType.k_CommercialBuilding:
                    panelType = floorIndex % 4 == 3 ? PanelType.k_CommercialSmallFullWindow : PanelType.k_CommercialWallWithFrame;
                    panelHeight = floorIndex % 4 == 3 ? k_SmallWindowHeight : entranceHeight;
                    panelHeight = (float)Math.Round(panelHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(complexBuildingType), complexBuildingType, null);
            }

            if (panelHeight <= remainingHeight)
            {
                // 繋ぎ目未作成で境界線を超える場合
                currentHeight = (float)Math.Round(currentHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
                currentHeight = (float)Math.Round(currentHeight, k_RoundDigit, MidpointRounding.AwayFromZero);

                if (!addedBoundaryWall && config.complexBuildingParams.buildingBoundaryHeight <= currentHeight + panelHeight)
                {
                    addedBoundaryWall = true;

                    if (canAddLowerFloor)
                    {
                        // 境界線までの高さ分を埋める
                        float remainingLowerBuildingHeight = config.complexBuildingParams.buildingBoundaryHeight - currentHeight;
                        remainingLowerBuildingHeight = (float)Math.Round(remainingLowerBuildingHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
                        vertical.Add(CreateHorizontal(panelSizes, from, to, remainingLowerBuildingHeight, floorWidthOffset, m_Constructors[panelType]));
                        remainingHeight -= remainingLowerBuildingHeight;
                        remainingHeight = (float)Math.Round(remainingHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
                        currentHeight += remainingLowerBuildingHeight;
                        currentHeight = (float)Math.Round(currentHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
                    }

                    if (k_DepressionWallHeight <= remainingHeight)
                    {
                        // 境界線の上に配置されるDepressionWallの高さよりもremainingHeightが大きい
                        vertical.Add(CreateDepressionWallNormalFacadeHorizontal(panelSizes, from, to, k_DepressionWallHeight, floorWidthOffset, false, config));
                        remainingHeight -= k_DepressionWallHeight;
                        remainingHeight = (float)Math.Round(remainingHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
                        currentHeight += k_DepressionWallHeight;
                    }
                    else
                    {
                        // 残りの高さをDepressionWallで埋める
                        vertical.Add(CreateDepressionWallNormalFacadeHorizontal(panelSizes, from, to, remainingHeight, floorWidthOffset, false, config));
                        currentHeight += remainingHeight;
                        remainingHeight = 0;
                    }
                }
                else
                {
                    vertical.Add(CreateHorizontal(panelSizes, from, to, panelHeight, floorWidthOffset, m_Constructors[panelType]));
                    remainingHeight -= panelHeight;
                    remainingHeight = (float)Math.Round(remainingHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
                    currentHeight += panelHeight;
                }
            }
            else
            {
                if (!addedBoundaryWall && config.complexBuildingParams.buildingBoundaryHeight <= currentHeight + remainingHeight)
                {
                    addedBoundaryWall = true;

                    if (canAddLowerFloor)
                    {
                        // 境界線までの高さ分を埋める
                        float remainingLowerBuildingHeight = config.complexBuildingParams.buildingBoundaryHeight - currentHeight;
                        remainingLowerBuildingHeight = (float)Math.Round(remainingLowerBuildingHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
                        vertical.Add(CreateHorizontal(panelSizes, from, to, remainingLowerBuildingHeight, floorWidthOffset, m_Constructors[panelType]));
                        remainingHeight -= remainingLowerBuildingHeight;
                        remainingHeight = (float)Math.Round(remainingHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
                        currentHeight += remainingLowerBuildingHeight;
                        currentHeight = (float)Math.Round(currentHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
                    }

                    if (k_DepressionWallHeight <= remainingHeight)
                    {
                        // 境界線の上に配置されるDepressionWallの高さよりもremainingHeightが大きい
                        vertical.Add(CreateDepressionWallNormalFacadeHorizontal(panelSizes, from, to, k_DepressionWallHeight, floorWidthOffset, false, config));
                        remainingHeight -= k_DepressionWallHeight;
                        remainingHeight = (float)Math.Round(remainingHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
                        currentHeight += k_DepressionWallHeight;
                    }
                    else
                    {
                        // 残りの高さをDepressionWallで埋める
                        vertical.Add(CreateDepressionWallNormalFacadeHorizontal(panelSizes, from, to, remainingHeight, floorWidthOffset, false, config));
                        currentHeight += remainingHeight;
                        remainingHeight = 0;
                    }
                }
                else
                {
                    if (panelHeight <= remainingHeight)
                    {
                        vertical.Add(CreateHorizontal(panelSizes, from, to, panelHeight, floorWidthOffset, m_Constructors[panelType]));
                        remainingHeight -= panelHeight;
                        remainingHeight = (float)Math.Round(remainingHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
                        currentHeight += panelHeight;
                    }
                    else
                    {
                        // 残りの高さを埋める
                        vertical.Add(CreateHorizontal(panelSizes, from, to, remainingHeight, floorWidthOffset, m_Constructors[panelType]));
                        currentHeight += remainingHeight;
                        remainingHeight = 0;
                    }
                }
            }

            floorIndex++;
            config.m_ComplexBuildingPlannerParams.m_AddedBoundaryWall = addedBoundaryWall;
            currentHeight = (float)Math.Round(currentHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
            remainingHeight = (float)Math.Round(remainingHeight, k_RoundDigit, MidpointRounding.AwayFromZero);
            return (floorIndex, addedBoundaryWall, currentHeight, remainingHeight);
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

        private HorizontalLayout CreateDepressionWallNormalFacadeHorizontal(List<PanelSize> panelSizes, int from, int to, float height, float floorWidthOffset, bool isEntrance, BuildingGenerator.Config config)
        {
            var horizontal = new HorizontalLayout();
            for (int i = from; i < to; i++)
            {
                ProceduralFacadeElement.PositionType positionType;
                if (from == to - 1 && panelSizes.Count == 1)
                {
                    positionType = ProceduralFacadeElement.PositionType.k_NoLeftRight;
                }
                else if (i == 0)
                {
                    positionType = ProceduralFacadeElement.PositionType.k_NoLeft;
                }
                else if (i == to - 1 && to == panelSizes.Count)
                {
                    positionType = ProceduralFacadeElement.PositionType.k_NoRight;
                }
                else
                {
                    positionType = ProceduralFacadeElement.PositionType.k_Middle;
                }

                float panelWidth = m_SizeValues[panelSizes[i]] + floorWidthOffset;
                if (isEntrance)
                {
                    var depressionWall = new List<Func<ILayoutElement>>
                    {
                        () => new ProceduralFacadeCompoundElements.ProceduralDepressionWall(config, positionType)
                        {
                            m_DepressionWallDepth = 0.3f
                        }
                    };
                    horizontal.Add(Construct(depressionWall, panelWidth, height));
                }
                else
                {
                    var depressionWall = new List<Func<ILayoutElement>>
                    {
                        () => new ProceduralFacadeCompoundElements.ProceduralDepressionWall(config, positionType)
                        {
                            m_DepressionWallName = "BoundaryWallTextured",
                            m_DepressionWallDepth = 0f,
                            m_DepressionWallMat = config.complexBuildingMaterialPalette.boundaryWall,
                            m_DepressionWallColor = config.complexBuildingVertexColorPalette.boundaryWallColor,
                        }
                    };
                    horizontal.Add(Construct(depressionWall, panelWidth, height));
                }
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
            k_ShadowWall,

            k_SkyscraperCondominiumWall,
            k_SkyscraperCondominiumWindow,
            k_SkyscraperCondominiumFullWindow,

            k_CommercialFullWindow,
            k_CommercialWallWithFrame,
            k_CommercialSmallFullWindow,

            k_OfficeFullWindow,
            k_OfficeSmallFullWindow,
        }
    }
}
