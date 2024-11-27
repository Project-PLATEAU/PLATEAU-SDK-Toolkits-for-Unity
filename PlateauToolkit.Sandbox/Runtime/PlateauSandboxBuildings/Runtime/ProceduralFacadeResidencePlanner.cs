using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces;
using ProceduralToolkit;
using ProceduralToolkit.Buildings;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime
{
    [CreateAssetMenu(menuName = "ProceduralToolkit/Buildings/Procedural Facade Planner/House", order = 2)]
    public class ProceduralFacadeResidencePlanner : FacadePlanner
    {
        private const float k_SocleHeight = 1;
        private const float k_FloorHeight = 2.5f;
        private const float k_BufferWidth = 2;
        private const string k_SocleTopTexturedDraftName = "SocleTopTextured";
        private const float k_ShadowWallOffset = 0.1f;

        private readonly Dictionary<PanelType, List<Func<ILayoutElement>>> m_Constructors = new();
        private readonly Dictionary<PanelType, Func<ILayoutElement>> m_CommonConstructors = new();
        private readonly Dictionary<PanelSize, float> m_SizeValues = new()
        {
            {PanelSize.k_Narrow, 2.5f},
            {PanelSize.k_Wide, 3},
        };

        public override List<ILayout> Plan(List<Vector2> foundationPolygon, BuildingGenerator.Config config)
        {
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
                    {
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Back;
                        var vertical = new VerticalLayout();
                        vertical.AddElement(Construct(m_Constructors[PanelType.k_ShadowWall], width - k_ShadowWallOffset, config.residenceParams.numFloor * k_FloorHeight - k_ShadowWallOffset));
                        vertical.Add(PlanNormalFacade(width, config, leftIsConvex, rightIsConvex));
                        layouts.Add(vertical);
                        break;
                    }
                    case 1:
                    {
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Right;
                        var vertical = new VerticalLayout();
                        vertical.AddElement(Construct(m_Constructors[PanelType.k_ShadowWall], width - k_ShadowWallOffset, config.residenceParams.numFloor * k_FloorHeight - k_ShadowWallOffset));
                        vertical.Add(PlanNormalFacade(width, config, leftIsConvex, rightIsConvex));
                        layouts.Add(vertical);
                        break;
                    }
                    case 2:
                    {
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Front;
                        var vertical = new VerticalLayout();
                        vertical.AddElement(Construct(m_Constructors[PanelType.k_ShadowWall], width - k_ShadowWallOffset, config.residenceParams.numFloor * k_FloorHeight - k_ShadowWallOffset));
                        vertical.Add(PlanEntranceFacade(width, config, leftIsConvex, rightIsConvex));
                        layouts.Add(vertical);
                        break;
                    }
                    case 3:
                    {
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Left;
                        var vertical = new VerticalLayout();
                        vertical.AddElement(Construct(m_Constructors[PanelType.k_ShadowWall], width - k_ShadowWallOffset, config.residenceParams.numFloor * k_FloorHeight - k_ShadowWallOffset));
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
            m_Constructors[PanelType.k_Attic] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralWall(config)
            };
            m_Constructors[PanelType.k_Entrance] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralEntrance(config),
            };
            m_Constructors[PanelType.k_Socle] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralSocle(
                    config,
                    socleColor: config.residenceVertexColorPalette.socleColor,
                    socleMat: config.residenceMaterialPalette.socle
                    )
                {
                    heightScale = k_SocleHeight * 0.15f,
                }
            };
            m_Constructors[PanelType.k_SocleTop] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralSocle(
                    config,
                    socleName: k_SocleTopTexturedDraftName,
                    socleColor: config.residenceVertexColorPalette.socleTopColor,
                    socleMat: config.residenceMaterialPalette.socleTop
                    )
                {
                    heightScale = k_SocleHeight * 0.05f,
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
            m_Constructors[PanelType.k_Window] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralWindow(config)
                {
                    m_WindowWidthOffset = 0.9f,
                    m_WindowBottomOffset = 0.8f,
                    m_NumCenterRods = 1,
                    m_WindowFrameRodType = ProceduralFacadeElement.WindowFrameRodType.k_Vertical,
                }
            };
            m_Constructors[PanelType.k_WallOrWindow] = new List<Func<ILayoutElement>>
            {
                m_Constructors[PanelType.k_Wall][0],
                m_Constructors[PanelType.k_Window][0]
            };
            m_Constructors[PanelType.k_SeparatedLongCrossWindow] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralWindow(config)
                {
                    m_WindowBottomOffset = 0,
                    m_NumCenterRods = 1,
                    m_WindowFrameRodType = ProceduralFacadeElement.WindowFrameRodType.k_Cross,
                    m_HasWindowsill = false
                }
            };
        }

        private ILayout PlanNormalFacade(float facadeWidth, BuildingGenerator.Config config, bool leftIsConvex, bool rightIsConvex)
        {
            List<PanelSize> panelSizes = DivideFacade(facadeWidth, leftIsConvex, rightIsConvex, out float remainder);

            m_CommonConstructors[PanelType.k_Wall] = m_Constructors[PanelType.k_Wall].GetRandom();

            VerticalLayout vertical = CreateNormalFacadeVertical(panelSizes, 0, panelSizes.Count, config);
            if (remainder > Geometry.Epsilon)
            {
                return new HorizontalLayout
                {
                    CreateBufferWallVertical(remainder/2, config),
                    vertical,
                    CreateBufferWallVertical(remainder/2, config)
                };
            }
            return vertical;
        }

        private ILayout PlanEntranceFacade(float facadeWidth, BuildingGenerator.Config config, bool leftIsConvex, bool rightIsConvex)
        {
            List<PanelSize> panelSizes = DivideFacade(facadeWidth, leftIsConvex, rightIsConvex, out float remainder);

            m_CommonConstructors[PanelType.k_Wall] = m_Constructors[PanelType.k_Wall].GetRandom();

            var horizontal = new HorizontalLayout();
            bool hasRemainder = remainder > Geometry.Epsilon;
            if (hasRemainder)
            {
                horizontal.Add(CreateBufferWallVertical(remainder/2, config));
            }

            const int entranceCount = 1;
            int entranceIndexInterval = (panelSizes.Count - entranceCount)/(entranceCount + 1);
            const int lastEntranceIndex = -1;
            horizontal.Add(CreateEntranceNormalFacadeVertical(panelSizes, lastEntranceIndex + 1, entranceIndexInterval, config));
            horizontal.Add(CreateEntranceVertical(m_SizeValues[panelSizes[entranceIndexInterval]], config));
            horizontal.Add(CreateEntranceNormalFacadeVertical(panelSizes, entranceIndexInterval + 1, panelSizes.Count, config));

            if (hasRemainder)
            {
                horizontal.Add(CreateBufferWallVertical(remainder/2, config));
            }
            return horizontal;
        }

        private VerticalLayout CreateBufferWallVertical(float width, BuildingGenerator.Config config)
        {
            var vertical = new VerticalLayout
            {
                Construct(m_Constructors[PanelType.k_Socle], width, k_SocleHeight),
                Construct(m_Constructors[PanelType.k_SocleTop], width, k_SocleHeight),
                CreateVertical(width, k_FloorHeight, 1, m_CommonConstructors[PanelType.k_Wall])
            };

            int floors = config.residenceParams.numFloor;
            for (int floorIndex = 1; floorIndex < floors; floorIndex++)
            {
                vertical.Add(Construct(m_Constructors[PanelType.k_SocleTop], width, k_SocleHeight));
                vertical.Add(Construct(m_Constructors[PanelType.k_Wall], width, k_FloorHeight));
            }

            return vertical;
        }

        private VerticalLayout CreateNormalFacadeVertical(List<PanelSize> panelSizes, int from, int to, BuildingGenerator.Config config)
        {
            var vertical = new VerticalLayout
            {
                CreateHorizontal(panelSizes, from, to, k_SocleHeight, m_Constructors[PanelType.k_Socle]),
                CreateHorizontal(panelSizes, from, to, k_SocleHeight, m_Constructors[PanelType.k_SocleTop])
            };

            int floors = config.residenceParams.numFloor;
            for (int floorIndex = 0; floorIndex < floors; floorIndex++)
            {
                if (floorIndex == 0)
                {
                    vertical.Add(CreateHorizontal(panelSizes, from, to, k_FloorHeight, m_Constructors[PanelType.k_WallOrWindow]));
                }
                else
                {
                    vertical.Add(CreateHorizontal(panelSizes, from, to, k_SocleHeight, m_Constructors[PanelType.k_SocleTop]));
                    vertical.Add(CreateHorizontal(panelSizes, from, to, k_FloorHeight, m_Constructors[PanelType.k_WallOrWindow]));
                }
            }

            return vertical;
        }

        private VerticalLayout CreateEntranceNormalFacadeVertical(List<PanelSize> panelSizes, int from, int to, BuildingGenerator.Config config)
        {
            var vertical = new VerticalLayout
            {
                CreateHorizontal(panelSizes, from, to, k_SocleHeight, m_Constructors[PanelType.k_Socle]),
                CreateHorizontal(panelSizes, from, to, k_SocleHeight, m_Constructors[PanelType.k_SocleTop])
            };

            int floors = config.residenceParams.numFloor;
            for (int floorIndex = 0; floorIndex < floors; floorIndex++)
            {
                if (floorIndex == 0)
                {
                    vertical.Add(CreateHorizontal(panelSizes, from, to, k_FloorHeight, m_Constructors[PanelType.k_SeparatedLongCrossWindow]));
                }
                else
                {
                    vertical.Add(CreateHorizontal(panelSizes, from, to, k_SocleHeight, m_Constructors[PanelType.k_SocleTop]));
                    vertical.Add(CreateHorizontal(panelSizes, from, to, k_FloorHeight, m_Constructors[PanelType.k_Window]));
                }
            }

            return vertical;
        }

        private VerticalLayout CreateEntranceVertical(float width, BuildingGenerator.Config config)
        {
            var vertical = new VerticalLayout
            {
                Construct(m_Constructors[PanelType.k_Socle], width, k_SocleHeight),
                Construct(m_Constructors[PanelType.k_SocleTop], width, k_SocleHeight),
                Construct(m_Constructors[PanelType.k_Entrance], width, k_FloorHeight),
            };

            int floors = config.residenceParams.numFloor;
            for (int floorIndex = 1; floorIndex < floors; floorIndex++)
            {
                vertical.Add(Construct(m_Constructors[PanelType.k_SocleTop], width, k_SocleHeight));
                vertical.Add(Construct(m_Constructors[PanelType.k_Wall], width, k_FloorHeight));
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

        private HorizontalLayout CreateHorizontal(List<PanelSize> panelSizes, int from, int to, float height, List<Func<ILayoutElement>> constructors)
        {
            var horizontal = new HorizontalLayout();
            for (int i = from; i < to; i++)
            {
                float width = m_SizeValues[panelSizes[i]];
                horizontal.Add(Construct(constructors, width, height));
            }
            return horizontal;
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
            k_Attic,
            k_Entrance,
            k_Socle,
            k_SocleTop,
            k_Wall,
            k_ShadowWall,
            k_Window,
            k_WallOrWindow,
            k_SeparatedLongCrossWindow,
        }
    }
}
