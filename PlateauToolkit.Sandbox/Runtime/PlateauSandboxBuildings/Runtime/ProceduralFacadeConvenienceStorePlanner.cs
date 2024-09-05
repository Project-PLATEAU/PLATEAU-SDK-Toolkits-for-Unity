using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Configs;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces;
using ProceduralToolkit;
using ProceduralToolkit.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime
{
    [CreateAssetMenu(menuName = "ProceduralToolkit/Buildings/Procedural Facade Planner/Convenience Store", order = 3)]
    public class ProceduralFacadeConvenienceStorePlanner : FacadePlanner
    {
        private const float k_SocleHeight = 0.5f;
        private const float k_FloorHeight = 2.5f;
        private const float k_BufferWidth = 2;

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
                        config.faceDirection = BuildingGenerator.Config.FaceDirection.k_Back;
                        layouts.Add(PlanBackFacade(width, config, leftIsConvex, rightIsConvex));
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
            m_Constructors[PanelType.k_AtticWindow] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralFullWindow(config)
            };
            m_Constructors[PanelType.k_Entrance] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralFullWindow(config) {m_NumCenterRods = 1}
            };
            m_Constructors[PanelType.k_FullWindow] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralFullWindow(config)
            };
            m_Constructors[PanelType.k_Socle] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralSocle(config)
            };
            m_Constructors[PanelType.k_Wall] = new List<Func<ILayoutElement>>
            {
                () => new ProceduralFacadeCompoundElements.ProceduralWall(config)
            };
        }

        private ILayout PlanNormalFacade(float facadeWidth, BuildingGenerator.Config config, bool leftIsConvex, bool rightIsConvex)
        {
            List<PanelSize> panelSizes = DivideFacade(facadeWidth, leftIsConvex, rightIsConvex, out float remainder);

            m_CommonConstructors[PanelType.k_Wall] = m_Constructors[PanelType.k_Wall].GetRandom();

            Directions directions = Directions.Left;
            if (remainder > Geometry.Epsilon)
            {
                var horizontal = new HorizontalLayout { CreateBufferWallVertical(directions, remainder/2, config) };
                directions &= ~Directions.Left;
                horizontal.Add(CreateNormalFacadeVertical(directions, panelSizes, 0, panelSizes.Count, config));
                directions |= Directions.Right;
                horizontal.Add(CreateBufferWallVertical(directions, remainder/2, config));
                return horizontal;
            }

            directions |= Directions.Right;
            return CreateNormalFacadeVertical(directions, panelSizes, 0, panelSizes.Count, config);
        }

        private ILayout PlanEntranceFacade(float facadeWidth, BuildingGenerator.Config config, bool leftIsConvex, bool rightIsConvex)
        {
            List<PanelSize> panelSizes = DivideFacade(facadeWidth, leftIsConvex, rightIsConvex, out float remainder);

            m_CommonConstructors[PanelType.k_Wall] = m_Constructors[PanelType.k_Wall].GetRandom();

            Directions directions = Directions.None;
            var horizontal = new HorizontalLayout();
            bool hasRemainder = remainder > Geometry.Epsilon;
            if (hasRemainder)
            {
                directions |= Directions.Left;
                horizontal.Add(CreateBufferWallVertical(directions, remainder/2, config));
                directions &= ~Directions.Left;
            }

            const int entranceCount = 1;
            int entranceIndexInterval = (panelSizes.Count - entranceCount)/(entranceCount + 1);
            horizontal.Add(CreateNormalFacadeVertical(directions, panelSizes, 0, entranceIndexInterval, config, hasRemainder));
            horizontal.Add(CreateEntranceVertical(directions, m_SizeValues[panelSizes[entranceIndexInterval]], config, 0 < entranceIndexInterval, 0 < panelSizes.Count - (entranceIndexInterval + 1)));
            horizontal.Add(CreateNormalFacadeVertical(directions, panelSizes, entranceIndexInterval + 1, panelSizes.Count, config, hasRemainder));

            if (hasRemainder)
            {
                directions |= Directions.Right;
                horizontal.Add(CreateBufferWallVertical(directions, remainder/2, config));
            }
            return horizontal;
        }

        private ILayout PlanBackFacade(float facadeWidth, BuildingGenerator.Config config, bool leftIsConvex, bool rightIsConvex)
        {
            List<PanelSize> panelSizes = DivideFacade(facadeWidth, leftIsConvex, rightIsConvex, out float remainder);

            m_CommonConstructors[PanelType.k_Wall] = m_Constructors[PanelType.k_Wall].GetRandom();

            Directions directions = Directions.Left;
            if (remainder > Geometry.Epsilon)
            {
                var horizontal = new HorizontalLayout { CreateBufferWallVertical(directions, remainder/2, config) };
                directions &= ~Directions.Left;
                horizontal.Add(CreateNormalFacadeVertical(directions, panelSizes, 0, panelSizes.Count, config));
                directions |= Directions.Right;
                horizontal.Add(CreateBufferWallVertical(directions, remainder/2, config));
                return horizontal;
            }

            int from = 0;
            int to = panelSizes.Count;
            var vertical = new VerticalLayout
            {
                CreateHorizontal(panelSizes, from, to, k_SocleHeight, m_Constructors[PanelType.k_Socle]),
                CreateHorizontal(panelSizes, from, to, k_FloorHeight + ConvenienceStoreConfig.Params.k_BillboardHeight, m_Constructors[PanelType.k_Wall]),
            };

            return vertical;
        }

        private VerticalLayout CreateBufferWallVertical(Directions directions, float width, BuildingGenerator.Config config)
        {
            var vertical = new VerticalLayout();
            if (config.faceDirection.HasFlag(BuildingGenerator.Config.FaceDirection.k_Front))
            {
                vertical.Add(Construct(m_Constructors[PanelType.k_Socle], width, k_SocleHeight));
                vertical.Add(CreateVertical(width, k_FloorHeight, 1, m_CommonConstructors[PanelType.k_Wall]));
                vertical.Add(Construct(() => new ProceduralFacadeCompoundElements.ProceduralBillboard(config, directions), width, ConvenienceStoreConfig.Params.k_BillboardHeight));
            }
            else
            {
                vertical.Add(Construct(m_Constructors[PanelType.k_Socle], width, k_SocleHeight));
                vertical.Add(CreateVertical(width, k_FloorHeight + ConvenienceStoreConfig.Params.k_BillboardHeight, 1, m_CommonConstructors[PanelType.k_Wall]));
            }

            return vertical;
        }

        private VerticalLayout CreateNormalFacadeVertical(Directions directions, List<PanelSize> panelSizes, int from, int to, BuildingGenerator.Config config, bool hasRemainder = false)
        {
            var vertical = new VerticalLayout { CreateHorizontal(panelSizes, from, to, k_SocleHeight, m_Constructors[PanelType.k_Socle]) };
            if (config.conveniParams.isSideWall && config.faceDirection.HasFlag(BuildingGenerator.Config.FaceDirection.k_Left) ||
                config.conveniParams.isSideWall && config.faceDirection.HasFlag(BuildingGenerator.Config.FaceDirection.k_Right) ||
                config.faceDirection.HasFlag(BuildingGenerator.Config.FaceDirection.k_Back))
            {
                vertical.Add(CreateHorizontal(panelSizes, from, to, k_FloorHeight, m_Constructors[PanelType.k_Wall]));
            }
            else
            {
                vertical.Add(CreateHorizontal(panelSizes, from, to, k_FloorHeight, m_Constructors[PanelType.k_FullWindow]));
            }

            if (config.faceDirection == BuildingGenerator.Config.FaceDirection.k_Front)
            {
                var horizontal = new HorizontalLayout();
                for (int i = from; i < to; i++)
                {
                    float width = m_SizeValues[panelSizes[i]];
                    directions &= ~Directions.Left;

                    if (!hasRemainder && i == 0)
                    {
                        directions |= Directions.Left;
                    }

                    if (!hasRemainder && i == panelSizes.Count - 1)
                    {
                        directions |= Directions.Right;
                    }

                    Directions directionsInnerScope = directions;
                    var billboard = new List<Func<ILayoutElement>>
                    {
                        () => new ProceduralFacadeCompoundElements.ProceduralBillboard(config, directionsInnerScope)
                    };
                    horizontal.Add(Construct(billboard, width, ConvenienceStoreConfig.Params.k_BillboardHeight));
                }
                vertical.Add(horizontal);
            }
            else
            {
                vertical.Add(CreateHorizontal(panelSizes, from, to, ConvenienceStoreConfig.Params.k_BillboardHeight, m_Constructors[PanelType.k_Wall]));
            }

            return vertical;
        }

        private VerticalLayout CreateEntranceVertical(Directions directions, float width, BuildingGenerator.Config config, bool existLeftVertical, bool existRightVertical)
        {
            if (!existLeftVertical)
            {
                directions |= Directions.Left;
            }
            if (!existRightVertical)
            {
                directions |= Directions.Right;
            }

            var vertical = new VerticalLayout
            {
                Construct(m_Constructors[PanelType.k_Entrance], width, k_FloorHeight),
                Construct(m_Constructors[PanelType.k_AtticWindow], width, k_SocleHeight),
                Construct(() => new ProceduralFacadeCompoundElements.ProceduralBillboard(config, directions), width, ConvenienceStoreConfig.Params.k_BillboardHeight)
            };

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
            k_AtticWindow,
            k_Entrance,
            k_FullWindow,
            k_Socle,
            k_Wall,
        }
    }
}
