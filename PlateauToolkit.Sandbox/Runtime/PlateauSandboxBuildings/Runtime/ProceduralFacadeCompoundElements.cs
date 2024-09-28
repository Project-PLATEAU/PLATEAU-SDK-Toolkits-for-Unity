using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings;
using ProceduralToolkit;
using System;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime
{
    public abstract class ProceduralFacadeCompoundElements
    {
        public class ProceduralWall : ProceduralFacadeWallElement
        {
            protected readonly WallColorData m_WallColorData;
            protected readonly WallTexturedData m_WallTexturedData;
            public bool m_IsShadowWall = false;
            public float m_MoveShadowWallDepth = 0;
            public float m_ShadowWallWidthOffset = 0;
            public float m_ShadowWallHeightOffset = 0;

            public ProceduralWall(BuildingGenerator.Config config)
            {
                UseTexture = config.useTexture;

                switch (config.buildingType)
                {
                    case BuildingType.k_Apartment:
                        if (UseTexture)
                        {
                            m_WallTexturedData = new WallTexturedData
                            {
                                m_UVScale = config.textureScale,
                                m_WallMat = config.skyscraperCondominiumMaterialPalette.wall
                            };
                        }
                        else
                        {
                            m_WallColorData = new WallColorData
                            {
                                m_WallColor = config.skyscraperCondominiumVertexColorPalette.wallColor,
                                m_VertexColorWallMat = config.skyscraperCondominiumVertexColorMaterialPalette.vertexWall
                            };
                        }
                        break;
                    case BuildingType.k_OfficeBuilding:
                        if (UseTexture)
                        {
                            m_WallTexturedData = new WallTexturedData
                            {
                                m_UVScale = config.textureScale,
                                m_WallMat = config.officeBuildingMaterialPalette.wall
                            };
                        }
                        else
                        {
                            m_WallColorData = new WallColorData
                            {
                                m_WallColor = config.officeBuildingVertexColorPalette.wallColor,
                                m_VertexColorWallMat = config.officeBuildingVertexColorMaterialPalette.vertexWall
                            };
                        }
                        break;
                    case BuildingType.k_House:
                        if (UseTexture)
                        {
                            m_WallTexturedData = new WallTexturedData
                            {
                                m_UVScale = config.textureScale,
                                m_WallMat = config.residenceMaterialPalette.wall
                            };
                        }
                        else
                        {
                            m_WallColorData = new WallColorData
                            {
                                m_WallColor = config.residenceVertexColorPalette.wallColor,
                                m_VertexColorWallMat = config.residenceVertexColorMaterialPalette.vertexWall
                            };
                        }
                        break;
                    case BuildingType.k_ConvenienceStore:
                        if (UseTexture)
                        {
                            m_WallTexturedData = new WallTexturedData
                            {
                                m_UVScale = config.textureScale,
                                m_WallMat = config.conveniMaterialPalette.wall
                            };
                        }
                        else
                        {
                            m_WallColorData = new WallColorData
                            {
                                m_WallColor = config.conveniVertexColorPalette.wallColor,
                                m_VertexColorWallMat = config.conveniVertexColorMaterialPalette.vertexWall
                            };
                        }
                        break;
                    case BuildingType.k_CommercialBuilding:
                        if (UseTexture)
                        {
                            m_WallTexturedData = new WallTexturedData
                            {
                                m_UVScale = config.textureScale,
                                m_WallMat = config.commercialFacilityMaterialPalette.wall
                            };
                        }
                        else
                        {
                            m_WallColorData = new WallColorData
                            {
                                m_WallColor = config.commercialFacilityVertexColorPalette.wallColor,
                                m_VertexColorWallMat = config.commercialFacilityVertexColorMaterialPalette.vertexWall
                            };
                        }
                        break;
                    case BuildingType.k_Hotel:
                        if (UseTexture)
                        {
                            m_WallTexturedData = new WallTexturedData
                            {
                                m_UVScale = config.textureScale,
                                m_WallMat = config.hotelMaterialPalette.wall
                            };
                        }
                        else
                        {
                            m_WallColorData = new WallColorData
                            {
                                m_WallColor = config.hotelVertexColorPalette.wallColor,
                                m_VertexColorWallMat = config.hotelVertexColorMaterialPalette.vertexWall
                            };
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public override CompoundMeshDraft Construct(Vector2 parentLayoutOrigin)
            {
                if (UseTexture)
                {
                    m_WallTexturedData.m_IsShadowWall = m_IsShadowWall;
                    m_WallTexturedData.m_MoveShadowWallDepth = m_MoveShadowWallDepth;
                    m_WallTexturedData.m_ShadowWallWidthOffset = m_ShadowWallWidthOffset;
                    m_WallTexturedData.m_ShadowWallHeightOffset = m_ShadowWallHeightOffset;
                }
                else
                {
                    m_WallColorData.m_IsShadowWall = m_IsShadowWall;
                    m_WallColorData.m_MoveShadowWallDepth = m_MoveShadowWallDepth;
                    m_WallColorData.m_ShadowWallWidthOffset = m_ShadowWallWidthOffset;
                    m_WallColorData.m_ShadowWallHeightOffset = m_ShadowWallHeightOffset;
                }

                return new CompoundMeshDraft().Add(UseTexture
                    ? WallTextured(parentLayoutOrigin + origin, width, height, m_WallTexturedData)
                    : Wall(parentLayoutOrigin + origin, width, height, m_WallColorData));
            }
        }

        public class ProceduralWallWithFrame : ProceduralFacadeWallElement
        {
            ProceduralFacadeWindowElement.WindowColorData m_WindowColorData;
            ProceduralFacadeWindowElement.WindowTexturedData m_WindowTexturedData;
            protected readonly WallColorData m_WallColorData;
            protected readonly WallTexturedData m_WallTexturedData;
            public int m_NumCenterRods;

            public ProceduralWallWithFrame(BuildingGenerator.Config config)
            {
                UseTexture = config.useTexture;
                m_NumCenterRods = -1;

                switch (config.buildingType)
                {
                    case BuildingType.k_CommercialBuilding:
                        if (UseTexture)
                        {
                            m_WindowTexturedData = new ProceduralFacadeWindowElement.WindowTexturedData
                            {
                                m_WindowpaneGlassName = k_WindowGlassTexturedDraftName,
                                m_UVScale = config.textureScale,
                                m_WallMat = config.commercialFacilityMaterialPalette.wall,
                                m_WindowPaneMat = config.commercialFacilityMaterialPalette.windowFrame,
                                m_WindowGlassMat = config.commercialFacilityMaterialPalette.windowGlass,
                            };

                            m_WallTexturedData = new WallTexturedData
                            {
                                m_UVScale = config.textureScale,
                                m_WallMat = config.commercialFacilityMaterialPalette.wall
                            };
                        }
                        else
                        {
                            m_WindowColorData = new ProceduralFacadeWindowElement.WindowColorData
                            {
                                m_WallColor = config.commercialFacilityVertexColorPalette.wallColor,
                                m_VertexWallMat = config.commercialFacilityVertexColorMaterialPalette.vertexWall,
                                m_WindowPaneColor = config.commercialFacilityVertexColorPalette.windowFrameColor,
                                m_WindowPaneGlassColor = config.commercialFacilityVertexColorPalette.windowGlassColor,
                                m_VertexWindowPaneMat = config.commercialFacilityVertexColorMaterialPalette.vertexWindow,
                            };

                            m_WallColorData = new WallColorData
                            {
                                m_WallColor = config.commercialFacilityVertexColorPalette.wallColor,
                                m_VertexColorWallMat = config.commercialFacilityVertexColorMaterialPalette.vertexWall
                            };
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public override CompoundMeshDraft Construct(Vector2 parentLayoutOrigin)
            {
                var compoundMeshDraft = new CompoundMeshDraft();

                Vector3 widthVector = Vector3.right*width;
                Vector3 heightVector = Vector3.up*height;
                Vector3 min = parentLayoutOrigin + origin;
                Vector3 max = min + widthVector + heightVector;

                if (UseTexture)
                {
                    MeshDraft windowpaneFrameTextured = ProceduralFacadeWindowElement.WindowpaneFrameTextured(min, max, k_WindowFrameRodWidth, k_WindowFrameRodHeight, k_WindowFrameRodDepth, m_NumCenterRods, WindowFrameRodType.k_Vertical, out Vector3 frameDepth, out Vector3 windowMin, out Vector3 windowWidth, out Vector3 windowHeight);
                    windowpaneFrameTextured.Paint(m_WindowTexturedData.m_WindowPaneMat);
                    windowpaneFrameTextured.name = k_WindowPaneTexturedDraftName;
                    compoundMeshDraft.Add(windowpaneFrameTextured);

                    MeshDraft wallTexturedData = WallTextured(windowMin, windowWidth, windowHeight, m_WallTexturedData);
                    wallTexturedData.name = k_WallTexturedDraftName;
                    compoundMeshDraft.Add(wallTexturedData);
                }
                else
                {
                    MeshDraft windowpaneFrame = ProceduralFacadeWindowElement.WindowpaneFrame(min, max, k_WindowFrameRodWidth, k_WindowFrameRodHeight, k_WindowFrameRodDepth, m_NumCenterRods, WindowFrameRodType.k_Vertical, m_WindowColorData, out Vector3 frameDepth, out Vector3 windowMin, out Vector3 windowWidth, out Vector3 windowHeight);
                    windowpaneFrame.Paint(m_WindowColorData.m_WindowPaneColor, m_WindowColorData.m_VertexWindowPaneMat);
                    windowpaneFrame.name = k_WindowPaneDraftName;
                    compoundMeshDraft.Add(windowpaneFrame);

                    MeshDraft wall = Wall(windowMin, windowWidth, windowHeight, m_WallColorData);
                    wall.name = k_WallDraftName;
                    compoundMeshDraft.Add(wall);
                }

                return compoundMeshDraft;
            }
        }

        public class ProceduralWindow : ProceduralFacadeWindowElement
        {
            public WindowColorData m_WindowColorData;
            public WindowTexturedData m_WindowTexturedData;
            public float m_WindowWidthOffset;
            public float m_WindowBottomOffset;
            public float m_WindowTopOffset;
            public float m_WindowDepthOffset;
            public float m_WindowFrameRodWidth;
            public float m_WindowFrameRodHeight;
            public float m_WindowFrameRodDepth;
            public int m_NumCenterRods;
            public WindowFrameRodType m_WindowFrameRodType;
            public bool m_HasWindowsill;
            public bool m_RectangleWindow;

            public ProceduralWindow(BuildingGenerator.Config config)
            {
                UseTexture = config.useTexture;
                m_WindowWidthOffset = k_WindowWidthOffset;
                m_WindowBottomOffset = k_WindowBottomOffset;
                m_WindowTopOffset = k_WindowTopOffset;
                m_WindowDepthOffset = k_WindowDepth;
                m_WindowFrameRodWidth = k_WindowFrameRodWidth;
                m_WindowFrameRodHeight = k_WindowFrameRodHeight;
                m_WindowFrameRodDepth = k_WindowFrameRodDepth;
                m_NumCenterRods = -1;
                m_WindowFrameRodType = WindowFrameRodType.k_Vertical;
                m_HasWindowsill = true;
                m_RectangleWindow = false;

                switch (config.buildingType)
                {
                    case BuildingType.k_Apartment:
                        if (UseTexture)
                        {
                            m_WindowTexturedData = new WindowTexturedData
                            {
                                m_WindowpaneGlassName = k_WindowGlassTexturedDraftName,
                                m_UVScale = config.textureScale,
                                m_WallMat = config.skyscraperCondominiumMaterialPalette.wall,
                                m_WindowPaneMat = config.skyscraperCondominiumMaterialPalette.windowFrame,
                                m_WindowGlassMat = config.skyscraperCondominiumMaterialPalette.windowGlass,
                            };
                        }
                        else
                        {
                            m_WindowColorData = new WindowColorData
                            {
                                m_WallColor = config.skyscraperCondominiumVertexColorPalette.wallColor,
                                m_VertexWallMat = config.skyscraperCondominiumVertexColorMaterialPalette.vertexWall,
                                m_WindowPaneColor = config.skyscraperCondominiumVertexColorPalette.windowFrameColor,
                                m_WindowPaneGlassColor = config.skyscraperCondominiumVertexColorPalette.windowGlassColor,
                                m_VertexWindowPaneMat = config.skyscraperCondominiumVertexColorMaterialPalette.vertexWindow,
                            };
                        }
                        break;
                    case BuildingType.k_OfficeBuilding:
                        if (UseTexture)
                        {
                            m_WindowTexturedData = new WindowTexturedData
                            {
                                m_WindowpaneGlassName = k_WindowGlassTexturedDraftName,
                                m_UVScale = config.textureScale,
                                m_WallMat = null,
                                m_WindowPaneMat = config.officeBuildingMaterialPalette.windowFrame,
                                m_WindowGlassMat = config.officeBuildingMaterialPalette.windowGlass,
                            };
                        }
                        else
                        {
                            m_WindowColorData = new WindowColorData
                            {
                                m_WallColor = Color.white,
                                m_VertexWallMat = null,
                                m_WindowPaneColor = config.officeBuildingVertexColorPalette.windowFrameColor,
                                m_WindowPaneGlassColor = config.officeBuildingVertexColorPalette.windowGlassColor,
                                m_VertexWindowPaneMat = config.officeBuildingVertexColorMaterialPalette.vertexWindow,
                            };
                        }
                        break;
                    case BuildingType.k_House:
                        if (UseTexture)
                        {
                            m_WindowTexturedData = new WindowTexturedData
                            {
                                m_WindowpaneGlassName = k_WindowGlassTexturedDraftName,
                                m_UVScale = config.textureScale,
                                m_WallMat = config.residenceMaterialPalette.wall,
                                m_WindowPaneMat = config.residenceMaterialPalette.windowFrame,
                                m_WindowGlassMat = config.residenceMaterialPalette.windowGlass,
                            };
                        }
                        else
                        {
                            m_WindowColorData = new WindowColorData
                            {
                                m_WallColor = config.residenceVertexColorPalette.wallColor,
                                m_VertexWallMat = config.residenceVertexColorMaterialPalette.vertexWall,
                                m_WindowPaneColor = config.residenceVertexColorPalette.windowFrameColor,
                                m_WindowPaneGlassColor = config.residenceVertexColorPalette.windowGlassColor,
                                m_VertexWindowPaneMat = config.residenceVertexColorMaterialPalette.vertexWindow,
                            };
                        }
                        break;
                    case BuildingType.k_ConvenienceStore:
                        if (UseTexture)
                        {
                            m_WindowTexturedData = new WindowTexturedData
                            {
                                m_WindowpaneGlassName = k_WindowGlassTexturedDraftName,
                                m_UVScale = config.textureScale,
                                m_WallMat = config.conveniMaterialPalette.wall,
                                m_WindowPaneMat = config.conveniMaterialPalette.windowFrame,
                                m_WindowGlassMat = config.conveniMaterialPalette.windowGlass,
                            };
                        }
                        else
                        {
                            m_WindowColorData = new WindowColorData
                            {
                                m_WallColor = config.conveniVertexColorPalette.wallColor,
                                m_VertexWallMat = config.conveniVertexColorMaterialPalette.vertexWall,
                                m_WindowPaneColor = config.conveniVertexColorPalette.windowFrameColor,
                                m_WindowPaneGlassColor = config.conveniVertexColorPalette.windowGlassColor,
                                m_VertexWindowPaneMat = config.conveniVertexColorMaterialPalette.vertexWindow,
                            };
                        }
                        break;
                    case BuildingType.k_CommercialBuilding:
                        if (UseTexture)
                        {
                            m_WindowTexturedData = new WindowTexturedData
                            {
                                m_WindowpaneGlassName = k_WindowGlassTexturedDraftName,
                                m_UVScale = config.textureScale,
                                m_WallMat = config.commercialFacilityMaterialPalette.wall,
                                m_WindowPaneMat = config.commercialFacilityMaterialPalette.windowFrame,
                                m_WindowGlassMat = config.commercialFacilityMaterialPalette.windowGlass,
                            };
                        }
                        else
                        {
                            m_WindowColorData = new WindowColorData
                            {
                                m_WallColor = config.commercialFacilityVertexColorPalette.wallColor,
                                m_VertexWallMat = config.commercialFacilityVertexColorMaterialPalette.vertexWall,
                                m_WindowPaneColor = config.commercialFacilityVertexColorPalette.windowFrameColor,
                                m_WindowPaneGlassColor = config.commercialFacilityVertexColorPalette.windowGlassColor,
                                m_VertexWindowPaneMat = config.commercialFacilityVertexColorMaterialPalette.vertexWindow,
                            };
                        }
                        break;
                    case BuildingType.k_Hotel:
                        if (UseTexture)
                        {
                            m_WindowTexturedData = new WindowTexturedData
                            {
                                m_WindowpaneGlassName = k_WindowGlassTexturedDraftName,
                                m_UVScale = config.textureScale,
                                m_WallMat = config.hotelMaterialPalette.wall,
                                m_WindowPaneMat = config.hotelMaterialPalette.windowPane,
                                m_WindowGlassMat = config.hotelMaterialPalette.windowGlass,
                            };
                        }
                        else
                        {
                            m_WindowColorData = new WindowColorData
                            {
                                m_WallColor = config.hotelVertexColorPalette.wallColor,
                                m_VertexWallMat = config.hotelVertexColorMaterialPalette.vertexWall,
                                m_WindowPaneColor = config.hotelVertexColorPalette.windowPaneColor,
                                m_WindowPaneGlassColor = config.hotelVertexColorPalette.windowPaneGlassColor,
                                m_VertexWindowPaneMat = config.hotelVertexColorMaterialPalette.vertexWindowPane,
                            };
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public override CompoundMeshDraft Construct(Vector2 parentLayoutOrigin)
            {
                return UseTexture
                    ? WindowTextured(parentLayoutOrigin + origin, width, height * heightScale, m_WindowWidthOffset, m_WindowBottomOffset, m_WindowTopOffset, m_WindowDepthOffset, m_WindowFrameRodWidth, m_WindowFrameRodHeight, m_WindowFrameRodDepth, m_NumCenterRods, m_WindowFrameRodType, m_WindowTexturedData, m_HasWindowsill, m_RectangleWindow)
                    : Window(parentLayoutOrigin + origin, width, height * heightScale, m_WindowWidthOffset, m_WindowBottomOffset, m_WindowTopOffset, m_WindowDepthOffset, m_WindowFrameRodWidth, m_WindowFrameRodHeight, m_WindowFrameRodDepth, m_NumCenterRods, m_WindowFrameRodType, m_WindowColorData, m_HasWindowsill, m_RectangleWindow);
            }
        }

        public class ProceduralFullWindow : ProceduralWindow
        {
            public new float m_WindowFrameRodWidth;
            public new float m_WindowFrameRodHeight;
            public new int m_NumCenterRods;

            public ProceduralFullWindow(BuildingGenerator.Config config) : base(config)
            {
                UseTexture = config.useTexture;
                UVScale = config.textureScale;
                m_WindowFrameRodWidth = k_WindowFrameRodWidth;
                m_WindowFrameRodHeight = k_WindowFrameRodHeight;
                m_NumCenterRods = 0;
            }

            public ProceduralFullWindow(BuildingGenerator.Config config, string windowpaneGlassName, Material windowGlassMat) : base(config)
            {
                UseTexture = config.useTexture;
                UVScale = config.textureScale;
                m_WindowFrameRodWidth = k_WindowFrameRodWidth;
                m_WindowFrameRodHeight = k_WindowFrameRodHeight;
                m_NumCenterRods = 0;

                if (UseTexture)
                {
                    m_WindowTexturedData.m_WindowpaneGlassName = windowpaneGlassName;
                    m_WindowTexturedData.m_WindowGlassMat = windowGlassMat;
                }
                else
                {
                    switch (config.buildingType)
                    {
                        case BuildingType.k_OfficeBuilding:
                            m_WindowColorData.m_WindowPaneGlassColor = config.officeBuildingVertexColorPalette.spandrelColor;
                            break;
                        case BuildingType.k_CommercialBuilding:
                            m_WindowColorData.m_WindowPaneGlassColor = config.commercialFacilityVertexColorPalette.windowGlassColor;
                            break;
                        case BuildingType.k_Hotel:
                            m_WindowColorData.m_WindowPaneGlassColor = config.hotelVertexColorPalette.windowPaneGlassColor;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            public override CompoundMeshDraft Construct(Vector2 parentLayoutOrigin)
            {
                Vector3 widthVector = Vector3.right * width;
                Vector3 heightVector = Vector3.up * (height * heightScale);
                Vector3 min = parentLayoutOrigin + origin;
                Vector3 frameMax = min + widthVector + heightVector;
                return UseTexture
                    ? new CompoundMeshDraft { WindowPaneTextured(min, frameMax, m_WindowTexturedData, m_WindowFrameRodWidth, m_WindowFrameRodHeight * heightScale, numCenterRod:m_NumCenterRods, windowFrameRodType:m_WindowFrameRodType) }
                    : new CompoundMeshDraft { WindowPane(min, frameMax, m_WindowColorData, m_WindowFrameRodWidth, m_WindowFrameRodHeight * heightScale, numCenterRod:m_NumCenterRods, windowFrameRodType:m_WindowFrameRodType) };
            }
        }

        public class ProceduralBalcony : ProceduralFacadeBalconyElement
        {
            private readonly Material m_VertexBalconyMaterial;
            private readonly BalconyColorData m_BalconyColorData;
            private readonly BalconyTexturedData m_BalconyTexturedData;
            private readonly ProceduralFacadeWindowElement.WindowTexturedData m_WindowTexturedData;
            private readonly ProceduralFacadeWindowElement.WindowColorData m_WindowColorData;

            public ProceduralBalcony(BuildingGenerator.Config config, Directions balconyOuterFrameDirections)
            {
                UseTexture = config.useTexture;
                UVScale = config.textureScale;

                switch (config.buildingType)
                {
                    case BuildingType.k_Apartment:
                        if (UseTexture)
                        {
                            m_BalconyTexturedData = new BalconyTexturedData
                            {
                                m_ConvexBalcony = config.skyscraperCondominiumParams.convexBalcony,
                                m_HasGlassWall = config.skyscraperCondominiumParams.hasBalconyGlass,
                                m_UVScale = config.textureScale,
                                m_BalconyOuterFrameDirections = balconyOuterFrameDirections,
                                m_WallMat = config.skyscraperCondominiumMaterialPalette.wall,
                            };

                            m_WindowTexturedData = new ProceduralFacadeWindowElement.WindowTexturedData
                            {
                                m_WindowpaneGlassName = k_WindowGlassTexturedDraftName,
                                m_UVScale = config.textureScale,
                                m_WallMat = config.skyscraperCondominiumMaterialPalette.wall,
                                m_WindowPaneMat = config.skyscraperCondominiumMaterialPalette.windowFrame,
                                m_WindowGlassMat = config.skyscraperCondominiumMaterialPalette.windowGlass,
                            };
                        }
                        else
                        {
                            m_BalconyColorData = new BalconyColorData
                            {
                                m_ConvexBalcony = config.skyscraperCondominiumParams.convexBalcony,
                                m_HasGlassWall = config.skyscraperCondominiumParams.hasBalconyGlass,
                                m_BalconyOuterFrameDirections = balconyOuterFrameDirections,
                                m_WallColor = config.skyscraperCondominiumVertexColorPalette.wallColor,
                                m_VertexWallMat = config.skyscraperCondominiumVertexColorMaterialPalette.vertexWall,
                            };

                            m_WindowColorData = new ProceduralFacadeWindowElement.WindowColorData
                            {
                                m_WallColor = config.skyscraperCondominiumVertexColorPalette.wallColor,
                                m_VertexWallMat = config.skyscraperCondominiumVertexColorMaterialPalette.vertexWall,
                                m_WindowPaneColor = config.skyscraperCondominiumVertexColorPalette.windowFrameColor,
                                m_WindowPaneGlassColor = config.skyscraperCondominiumVertexColorPalette.windowGlassColor,
                                m_VertexWindowPaneMat = config.skyscraperCondominiumVertexColorMaterialPalette.vertexWindow,
                            };
                        }
                        break;
                    case BuildingType.k_Hotel:
                        if (UseTexture)
                        {
                            m_BalconyTexturedData = new BalconyTexturedData
                            {
                                m_ConvexBalcony = false,
                                m_HasGlassWall = false,
                                m_UVScale = config.textureScale,
                                m_BalconyOuterFrameDirections = balconyOuterFrameDirections,
                                m_WallMat = config.hotelMaterialPalette.wall,
                            };

                            m_WindowTexturedData = new ProceduralFacadeWindowElement.WindowTexturedData
                            {
                                m_WindowpaneGlassName = k_WindowGlassTexturedDraftName,
                                m_UVScale = config.textureScale,
                                m_WallMat = config.hotelMaterialPalette.wall,
                                m_WindowPaneMat = config.hotelMaterialPalette.windowPane,
                                m_WindowGlassMat = config.hotelMaterialPalette.windowGlass,
                            };
                        }
                        else
                        {
                            m_BalconyColorData = new BalconyColorData
                            {
                                m_ConvexBalcony = false,
                                m_HasGlassWall = false,
                                m_BalconyOuterFrameDirections = balconyOuterFrameDirections,
                                m_WallColor = config.hotelVertexColorPalette.wallColor,
                                m_VertexWallMat = config.hotelVertexColorMaterialPalette.vertexWall,
                            };

                            m_WindowColorData = new ProceduralFacadeWindowElement.WindowColorData
                            {
                                m_WallColor = config.hotelVertexColorPalette.wallColor,
                                m_VertexWallMat = config.hotelVertexColorMaterialPalette.vertexWall,
                                m_WindowPaneColor = config.hotelVertexColorPalette.windowPaneColor,
                                m_WindowPaneGlassColor = config.hotelVertexColorPalette.windowPaneGlassColor,
                                m_VertexWindowPaneMat = config.hotelVertexColorMaterialPalette.vertexWindowPane,
                            };
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public override CompoundMeshDraft Construct(Vector2 parentLayoutOrigin)
            {
                return UseTexture
                    ? BalconyTextured(parentLayoutOrigin + origin, width, height, m_BalconyTexturedData, m_WindowTexturedData)
                    : Balcony(parentLayoutOrigin + origin, width, height, m_BalconyColorData, m_WindowColorData);
            }
        }

        public class ProceduralBalconyGlazed : ProceduralFacadeBalconyElement
        {
            public ProceduralBalconyGlazed(BuildingGenerator.Config config)
            {
                UseTexture = config.useTexture;
            }

            public override CompoundMeshDraft Construct(Vector2 parentLayoutOrigin)
            {
                return new CompoundMeshDraft();
                // return UseTexture
                    // ? BalconyTextured(parentLayoutOrigin + origin, width, height, UVScale, m_VertexColorPaletteForUseTexture, m_MaterialPalette)
                    // : BalconyGlazed(parentLayoutOrigin + origin, width, height, m_VertexColorPalette, m_MaterialPalette);
            }
        }

        public class ProceduralEntrance : ProceduralFacadeEntranceElement
        {
            protected readonly EntranceColorData m_EntranceColorData;
            protected readonly EntranceTexturedData m_EntranceTexturedData;
            private readonly BuildingType m_BuildingType;

            public ProceduralEntrance(BuildingGenerator.Config config)
            {
                UseTexture = config.useTexture;
                m_BuildingType = config.buildingType;

                switch (config.buildingType)
                {
                    case BuildingType.k_House:
                        if (UseTexture)
                        {
                            m_EntranceTexturedData = new EntranceTexturedData
                            {

                                m_HasRoof = config.residenceParams.hasEntranceRoof,
                                m_UVScale = config.textureScale,
                                m_WallMat = config.residenceMaterialPalette.wall,
                                m_EntranceDoorMat = config.residenceMaterialPalette.entranceDoor,
                                m_EntranceDoorFrameMat = config.residenceMaterialPalette.entranceDoorFrame,
                                m_EntranceDoorRoofMat = config.residenceMaterialPalette.entranceDoorRoof
                            };
                        }
                        else
                        {
                            m_EntranceColorData = new EntranceColorData
                            {
                                m_HasRoof = config.residenceParams.hasEntranceRoof,
                                m_WallColor = config.residenceVertexColorPalette.wallColor,
                                m_EntranceDoorColor = config.residenceVertexColorPalette.entranceDoorColor,
                                m_EntranceDoorFrameColor = config.residenceVertexColorPalette.entranceDoorFrameColor,
                                m_EntranceRoofColor = config.residenceVertexColorPalette.entranceDoorRoofColor,
                                m_VertexWallMaterial = config.residenceVertexColorMaterialPalette.vertexWall,
                            };
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public override CompoundMeshDraft Construct(Vector2 parentLayoutOrigin)
            {
                if (BuildingType.k_House == m_BuildingType)
                {
                    return UseTexture
                        ? ResidenceEntranceTextured(parentLayoutOrigin + origin, width, height, m_EntranceTexturedData)
                        : new CompoundMeshDraft().Add(ResidenceEntrance(parentLayoutOrigin + origin, width, height, m_EntranceColorData));
                }

                return UseTexture
                    ? EntranceTextured(parentLayoutOrigin + origin, width, height, m_EntranceTexturedData)
                    : new CompoundMeshDraft().Add(Entrance(parentLayoutOrigin + origin, width, height, m_EntranceColorData));
            }
        }

        public class ProceduralEntranceWindow : ProceduralFacadeEntranceElement
        {
            private Color wallColor;
            private Color frameColor;
            private Color glassColor;

            public ProceduralEntranceWindow(Color wallColor, Color frameColor, Color glassColor)
            {
                this.wallColor = wallColor;
                this.frameColor = frameColor;
                this.glassColor = glassColor;
            }

            public override CompoundMeshDraft Construct(Vector2 parentLayoutOrigin)
            {
                return EntranceWindow(parentLayoutOrigin + origin, width, height, wallColor, frameColor, glassColor);
            }
        }

        public class ProceduralSocle : ProceduralWall
        {
            public ProceduralSocle(
                BuildingGenerator.Config config,
                string socleName = k_SocleTexturedDraftName,
                Color socleColor = default,
                Material socleMat = null
                ) : base(config)
            {
                switch (config.buildingType)
                {
                    case BuildingType.k_Apartment:
                        if (UseTexture)
                        {
                            m_WallTexturedData.m_WallName = socleName;
                            m_WallTexturedData.m_WallMat = config.skyscraperCondominiumMaterialPalette.wall;
                        }
                        else
                        {
                            m_WallColorData.m_WallColor = config.skyscraperCondominiumVertexColorPalette.socleColor;
                        }
                        break;
                    case BuildingType.k_House:
                        if (UseTexture)
                        {
                            m_WallTexturedData.m_WallName = socleName;
                            m_WallTexturedData.m_WallMat = socleMat ? socleMat : config.residenceMaterialPalette.socle;
                        }
                        else
                        {
                            m_WallColorData.m_WallColor = socleColor;
                        }
                        break;
                    case BuildingType.k_ConvenienceStore:
                        if (UseTexture)
                        {
                            m_WallTexturedData.m_WallName = socleName;
                            m_WallTexturedData.m_WallMat = config.conveniMaterialPalette.socle;
                        }
                        else
                        {
                            m_WallColorData.m_WallColor = config.conveniVertexColorPalette.socleColor;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public class ProceduralDepressionWall : ProceduralWall
        {
            protected readonly DepressionWallColorData m_DepressionWallColorData;
            protected readonly DepressionWallTexturedData m_DepressionWallTexturedData;

            public ProceduralDepressionWall(BuildingGenerator.Config config, PositionType positionType) : base(config)
            {
                switch (config.buildingType)
                {
                    case BuildingType.k_CommercialBuilding:
                        if (UseTexture)
                        {
                            m_DepressionWallTexturedData = new DepressionWallTexturedData
                            {
                                m_WallName = k_DepressionWallTexturedDraftName,
                                m_UVScale = config.textureScale,
                                m_WallMat = config.commercialFacilityMaterialPalette.depressionWall,
                                m_PositionType = positionType
                            };
                        }
                        else
                        {
                            m_DepressionWallColorData = new DepressionWallColorData
                            {
                                m_WallColor = config.commercialFacilityVertexColorPalette.depressionWallColor,
                                m_VertexColorWallMat = config.commercialFacilityVertexColorMaterialPalette.vertexWall,
                                m_PositionType = positionType
                            };
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public override CompoundMeshDraft Construct(Vector2 parentLayoutOrigin)
            {
                Vector3 widthVector = Vector3.right * width;
                Vector3 heightVector = Vector3.up * height;

                return new CompoundMeshDraft().Add(UseTexture
                    ? DepressionWallTextured(parentLayoutOrigin + origin, widthVector, heightVector, m_DepressionWallTexturedData)
                    : DepressionWall(parentLayoutOrigin + origin, widthVector, heightVector, m_DepressionWallColorData));
            }
        }

        public class ProceduralBillboard : ProceduralFacadeBillboardElement
        {
            private readonly Color m_BillboardColor;
            private readonly BillboardColorData m_BillboardColorData;
            private readonly BillboardTexturedData m_BillboardTexturedData;

            public ProceduralBillboard(BuildingGenerator.Config config, Directions frameDirections)
            {
                UseTexture = config.useTexture;
                UVScale = config.textureScale;

                switch (config.buildingType)
                {
                    case BuildingType.k_ConvenienceStore:
                    {
                        if (UseTexture)
                        {
                            m_BillboardTexturedData = new BillboardTexturedData
                            {
                                m_FrameDirections = frameDirections,
                                m_BillboardMat = config.conveniMaterialPalette.billboard,
                                m_BillboardBottomMat = config.conveniMaterialPalette.billboardBottom
                            };
                        }
                        else
                        {
                            m_BillboardColorData = new BillboardColorData
                            {
                                m_FrameDirections = frameDirections,
                                m_BillboardColor = config.conveniVertexColorPalette.billboardColor,
                                m_BillboardBottomColor = config.conveniVertexColorPalette.billboardBottomColor,
                                m_VertexColorBillboardMat = config.conveniVertexColorMaterialPalette.vertexWall
                            };
                        }
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public override CompoundMeshDraft Construct(Vector2 parentLayoutOrigin)
            {
                return UseTexture
                    ? BillboardTextured(parentLayoutOrigin + origin, width, height * heightScale, m_BillboardTexturedData)
                    : Billboard(parentLayoutOrigin + origin, width, height * heightScale, m_BillboardColorData);
            }
        }
    }
}