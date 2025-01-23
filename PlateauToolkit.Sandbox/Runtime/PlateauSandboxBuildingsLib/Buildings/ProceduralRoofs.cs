using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Configs;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces;
using ProceduralToolkit;
using ProceduralToolkit.Skeleton;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings
{
    public abstract class ProceduralRoof : IConstructible<CompoundMeshDraft>
    {
        private readonly List<Vector2> m_FoundationPolygon;
        protected readonly BuildingGenerator.Config m_Config;
        protected readonly Color m_RoofColor;
        protected readonly Color m_RoofSideColor;
        protected readonly Color m_RoofSideFrontColor;
        protected readonly Material m_RoofMat;
        protected readonly Material m_RoofSideMat;
        protected readonly Material m_RoofSideFrontMat;
        protected readonly Material m_VertexRoofMat;
        public Vector2 m_UVScale;
        public float m_Thickness = 0.2f;
        public float m_Overhang = 0.2f;

        protected ProceduralRoof(List<Vector2> foundationPolygon, BuildingGenerator.Config config)
        {
            m_FoundationPolygon = foundationPolygon;
            m_Config = config;

            if (config.useTexture)
            {
                switch (config.buildingType)
                {
                    case BuildingType.k_Apartment:
                        m_RoofMat = m_Config.skyscraperCondominiumMaterialPalette.roof;
                        m_RoofSideMat = m_Config.skyscraperCondominiumMaterialPalette.roofSide;
                        m_UVScale = new Vector2(0.1f, 0.1f);
                        break;
                    case BuildingType.k_OfficeBuilding:
                        m_RoofMat = m_Config.officeBuildingMaterialPalette.roof;
                        m_RoofSideMat = m_Config.officeBuildingMaterialPalette.roofSide;
                        m_UVScale = new Vector2(0.1f, 0.1f);
                        break;
                    case BuildingType.k_House:
                        m_RoofMat = m_Config.residenceMaterialPalette.roof;
                        m_RoofSideMat = m_Config.residenceMaterialPalette.roofSide;
                        m_UVScale = new Vector2(0.1f, 0.1f);
                        break;
                    case BuildingType.k_ConvenienceStore:
                        m_RoofMat = m_Config.conveniMaterialPalette.roof;
                        m_RoofSideMat = m_Config.conveniMaterialPalette.roofSide;
                        m_UVScale = new Vector2(0.1f, 0.1f);
                        break;
                    case BuildingType.k_CommercialBuilding:
                        m_RoofMat = m_Config.commercialFacilityMaterialPalette.roof;
                        m_RoofSideMat = m_Config.commercialFacilityMaterialPalette.roofSide;
                        m_UVScale = new Vector2(0.1f, 0.1f);
                        break;
                    case BuildingType.k_Hotel:
                        m_RoofMat = m_Config.hotelMaterialPalette.roof;
                        m_RoofSideMat = m_Config.hotelMaterialPalette.roofSide;
                        m_RoofSideFrontMat = m_Config.hotelMaterialPalette.roofSideFront;
                        m_UVScale = new Vector2(0.1f, 0.1f);
                        break;
                    case BuildingType.k_Factory:
                        m_RoofMat = m_Config.factoryMaterialPalette.roof;
                        m_RoofSideMat = m_Config.factoryMaterialPalette.roofSide;
                        m_UVScale = new Vector2(0.1f, 0.1f);
                        break;
                    case BuildingType.k_ComplexBuilding:
                        ComplexBuildingConfig.ComplexBuildingType buildingType = config.m_ComplexBuildingPlannerParams.m_AddedBoundaryWall ? config.complexBuildingParams.higherFloorBuildingType : config.complexBuildingParams.lowerFloorBuildingType;
                        switch (buildingType)
                        {
                            case ComplexBuildingConfig.ComplexBuildingType.k_Apartment:
                                m_RoofMat = m_Config.complexBuildingMaterialPalette.apartmentRoof;
                                m_RoofSideMat = m_Config.complexBuildingMaterialPalette.apartmentRoofSide;
                                m_UVScale = new Vector2(0.1f, 0.1f);
                                break;
                            case ComplexBuildingConfig.ComplexBuildingType.k_OfficeBuilding:
                                m_RoofMat = m_Config.complexBuildingMaterialPalette.officeBuildingRoof;
                                m_RoofSideMat = m_Config.complexBuildingMaterialPalette.officeBuildingRoofSide;
                                m_UVScale = new Vector2(0.1f, 0.1f);
                                break;
                            case ComplexBuildingConfig.ComplexBuildingType.k_CommercialBuilding:
                                m_RoofMat = m_Config.complexBuildingMaterialPalette.commercialBuildingRoof;
                                m_RoofSideMat = m_Config.complexBuildingMaterialPalette.commercialBuildingRoofSide;
                                m_UVScale = new Vector2(0.1f, 0.1f);
                                break;
                            case ComplexBuildingConfig.ComplexBuildingType.k_Hotel:
                                m_RoofMat = m_Config.complexBuildingMaterialPalette.hotelRoof;
                                m_RoofSideMat = m_Config.complexBuildingMaterialPalette.hotelRoofSide;
                                m_RoofSideFrontMat = m_Config.complexBuildingMaterialPalette.hotelRoofSideFront;
                                m_UVScale = new Vector2(0.1f, 0.1f);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                switch (config.buildingType)
                {
                    case BuildingType.k_Apartment:
                        m_RoofColor = m_Config.skyscraperCondominiumVertexColorPalette.roofColor;
                        m_RoofSideColor = m_Config.skyscraperCondominiumVertexColorPalette.roofSideColor;
                        m_VertexRoofMat = m_Config.skyscraperCondominiumVertexColorMaterialPalette.vertexRoof;
                        break;
                    case BuildingType.k_OfficeBuilding:
                        m_RoofColor = m_Config.officeBuildingVertexColorPalette.roofColor;
                        m_RoofSideColor = m_Config.officeBuildingVertexColorPalette.roofSideColor;
                        m_VertexRoofMat = m_Config.officeBuildingVertexColorMaterialPalette.vertexRoof;
                        break;
                    case BuildingType.k_House:
                        m_RoofColor = m_Config.residenceVertexColorPalette.roofColor;
                        m_RoofSideColor = m_Config.residenceVertexColorPalette.roofSideColor;
                        m_VertexRoofMat = m_Config.residenceVertexColorMaterialPalette.vertexRoof;
                        break;
                    case BuildingType.k_ConvenienceStore:
                        m_RoofColor = m_Config.conveniVertexColorPalette.roofColor;
                        m_RoofSideColor = m_Config.conveniVertexColorPalette.roofSideColor;
                        m_VertexRoofMat = m_Config.conveniVertexColorMaterialPalette.vertexRoof;
                        break;
                    case BuildingType.k_CommercialBuilding:
                        m_RoofColor = m_Config.commercialFacilityVertexColorPalette.roofColor;
                        m_RoofSideColor = m_Config.commercialFacilityVertexColorPalette.roofSideColor;
                        m_VertexRoofMat = m_Config.commercialFacilityVertexColorMaterialPalette.vertexRoof;
                        break;
                    case BuildingType.k_Hotel:
                        m_RoofColor = m_Config.hotelVertexColorPalette.roofColor;
                        m_RoofSideColor = m_Config.hotelVertexColorPalette.roofSideColor;
                        m_RoofSideFrontColor = m_Config.hotelVertexColorPalette.roofSideFrontColor;
                        m_VertexRoofMat = m_Config.hotelVertexColorMaterialPalette.vertexRoof;
                        break;
                    case BuildingType.k_Factory:
                        m_RoofColor = m_Config.factoryVertexColorPalette.roofColor;
                        m_RoofSideColor = m_Config.factoryVertexColorPalette.roofSideColor;
                        m_VertexRoofMat = m_Config.factoryVertexColorMaterialPalette.vertexRoof;
                        break;
                    case BuildingType.k_ComplexBuilding:
                        ComplexBuildingConfig.ComplexBuildingType buildingType = config.m_ComplexBuildingPlannerParams.m_AddedBoundaryWall ? config.complexBuildingParams.higherFloorBuildingType : config.complexBuildingParams.lowerFloorBuildingType;
                        switch (buildingType)
                        {
                            case ComplexBuildingConfig.ComplexBuildingType.k_Apartment:
                                m_RoofColor = m_Config.complexBuildingVertexColorPalette.apartmentRoofColor;
                                m_RoofSideColor = m_Config.complexBuildingVertexColorPalette.apartmentRoofSideColor;
                                m_VertexRoofMat = m_Config.complexBuildingVertexColorMaterialPalette.vertexRoof;
                                break;
                            case ComplexBuildingConfig.ComplexBuildingType.k_OfficeBuilding:
                                m_RoofColor = m_Config.complexBuildingVertexColorPalette.officeBuildingRoofColor;
                                m_RoofSideColor = m_Config.complexBuildingVertexColorPalette.officeBuildingRoofSideColor;
                                m_VertexRoofMat = m_Config.complexBuildingVertexColorMaterialPalette.vertexRoof;
                                break;
                            case ComplexBuildingConfig.ComplexBuildingType.k_CommercialBuilding:
                                m_RoofColor = m_Config.complexBuildingVertexColorPalette.commercialBuildingRoofColor;
                                m_RoofSideColor = m_Config.complexBuildingVertexColorPalette.commercialBuildingRoofSideColor;
                                m_VertexRoofMat = m_Config.complexBuildingVertexColorMaterialPalette.vertexRoof;
                                break;
                            case ComplexBuildingConfig.ComplexBuildingType.k_Hotel:
                                m_RoofColor = m_Config.complexBuildingVertexColorPalette.hotelRoofColor;
                                m_RoofSideColor = m_Config.complexBuildingVertexColorPalette.hotelRoofSideColor;
                                m_RoofSideFrontColor = m_Config.complexBuildingVertexColorPalette.hotelRoofSideFrontColor;
                                m_VertexRoofMat = m_Config.complexBuildingVertexColorMaterialPalette.vertexRoof;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public abstract CompoundMeshDraft Construct(Vector2 parentLayoutOrigin);

        protected MeshDraft ConstructRoofBase(Vector2 uvScale, bool generateUV, out List<Vector2> roofPolygon2, out List<Vector3> roofPolygon3)
        {
            roofPolygon2 = Geometry.OffsetPolygon(m_FoundationPolygon, m_Overhang);
            roofPolygon3 = roofPolygon2.ConvertAll(v => v.ToVector3XZ());

            var roofDraft = new MeshDraft();
            if (m_Thickness > 0)
            {
                roofDraft.Add(ConstructBorder(roofPolygon2, roofPolygon3, m_Thickness, uvScale, generateUV));
            }
            if (m_Overhang > 0)
            {
                roofDraft.Add(ConstructOverhang(m_FoundationPolygon, roofPolygon3, uvScale, generateUV));
            }
            return roofDraft;
        }
        protected CompoundMeshDraft ConstructSeparatedFrontRoofBase(Vector2 uvScale, bool generateUV, out List<Vector2> roofPolygon2, out List<Vector3> roofPolygon3)
        {
            roofPolygon2 = Geometry.OffsetPolygon(m_FoundationPolygon, m_Overhang);
            roofPolygon3 = roofPolygon2.ConvertAll(v => v.ToVector3XZ());

            var roofDraft = new CompoundMeshDraft();
            if (m_Thickness > 0)
            {
                for (int i = 0; i < roofPolygon3.Count; i++)
                {
                    List<Vector3> upperRing = roofPolygon2.ConvertAll(v => v.ToVector3XZ() + Vector3.up * m_Thickness);
                    Vector3 lower0 = roofPolygon3[i];
                    Vector3 lower1 = roofPolygon3[(i + 1)% roofPolygon3.Count];
                    Vector3 upper0 = upperRing[i];
                    Vector3 upper1 = upperRing[(i + 1)% roofPolygon3.Count];
                    MeshDraft roofMeshDraft;
                    if (generateUV)
                    {
                        var uv00 = new Vector2(0, 0);
                        var uv10 = new Vector2(1, 0);
                        var uv01 = new Vector2(0, 1);
                        var uv11 = new Vector2(1, 1);
                        roofMeshDraft = new MeshDraft().AddQuad(lower1, upper1, upper0, lower0, true, uv00, uv01, uv11, uv10);
                    }
                    else
                    {
                        roofMeshDraft = new MeshDraft().AddQuad(lower1, upper1, upper0, lower0, calculateNormal:true);
                    }

                    if (i == 2 /* front */)
                    {
                        if (m_Config.useTexture)
                        {
                            roofMeshDraft.name = "RoofSideFrontDraft";
                            roofMeshDraft.Paint(m_RoofSideFrontMat);
                        }
                        else
                        {
                            roofMeshDraft.name = "HippedRoof";
                            roofMeshDraft.Paint(m_RoofSideFrontColor, m_VertexRoofMat);
                        }
                    }
                    else
                    {
                        if (m_Config.useTexture)
                        {
                            roofMeshDraft.name = "RoofSideDraft";
                            roofMeshDraft.Paint(m_RoofSideMat);
                        }
                        else
                        {
                            roofMeshDraft.name = "HippedRoof";
                            roofMeshDraft.Paint(m_RoofSideColor, m_VertexRoofMat);
                        }
                    }
                    roofDraft.Add(roofMeshDraft);
                }
            }
            if (m_Overhang > 0)
            {
                roofDraft.Add(ConstructOverhang(m_FoundationPolygon, roofPolygon3, uvScale, generateUV));
            }
            return roofDraft;
        }

        protected static MeshDraft ConstructBorder(List<Vector2> roofPolygon2, List<Vector3> roofPolygon3, float thickness, Vector2 uvScale, bool generateUV = false)
        {
            List<Vector3> upperRing = roofPolygon2.ConvertAll(v => v.ToVector3XZ() + Vector3.up * thickness);
            return new MeshDraft().AddFlatQuadBand(roofPolygon3, upperRing, uvScale, generateUV);
        }

        protected static MeshDraft ConstructOverhang(List<Vector2> foundationPolygon, List<Vector3> roofPolygon3, Vector2 uvScale, bool generateUV = false)
        {
            List<Vector3> lowerRing = foundationPolygon.ConvertAll(v => v.ToVector3XZ());
            return new MeshDraft().AddFlatQuadBand(lowerRing, roofPolygon3, uvScale, generateUV);
        }

        protected static MeshDraft ConstructContourDraft(List<Vector2> skeletonPolygon2, float roofPitch, bool generateUV = false)
        {
            Vector2 edgeA = skeletonPolygon2[0];
            Vector2 edgeB = skeletonPolygon2[1];
            Vector2 edgeDirection2 = (edgeB - edgeA).normalized;
            Vector3 roofNormal = CalculateRoofNormal(edgeDirection2, roofPitch);

            List<Vector3> skeletonPolygon3 = skeletonPolygon2.ConvertAll(v => v.ToVector3XZ());

            var tessellator = new Tessellator();
            tessellator.AddContour(skeletonPolygon3);
            tessellator.Tessellate(normal: Vector3.up);
            var contourDraft = tessellator.ToMeshDraft();

            for (int i = 0; i < contourDraft.VertexCount; i++)
            {
                Vector2 vertex = contourDraft.vertices[i].ToVector2XZ();
                float height = CalculateVertexHeight(vertex, edgeA, edgeDirection2, roofPitch);
                contourDraft.vertices[i] = new Vector3(vertex.x, height, vertex.y);
                contourDraft.normals.Add(roofNormal);

                if (generateUV)
                {
                    contourDraft.uv.Add(new Vector2(0.1f * vertex.x, 0.1f * vertex.y));
                }
            }
            return contourDraft;
        }

        protected static MeshDraft ConstructGableDraft(List<Vector2> skeletonPolygon2, float roofPitch)
        {
            Vector2 edgeA2 = skeletonPolygon2[0];
            Vector2 edgeB2 = skeletonPolygon2[1];
            Vector2 peak2 = skeletonPolygon2[2];
            Vector2 edgeDirection2 = (edgeB2 - edgeA2).normalized;

            float peakHeight = CalculateVertexHeight(peak2, edgeA2, edgeDirection2, roofPitch);
            Vector3 edgeA3 = edgeA2.ToVector3XZ();
            Vector3 edgeB3 = edgeB2.ToVector3XZ();
            var peak3 = new Vector3(peak2.x, peakHeight, peak2.y);
            Vector2 gableTop2 = Closest.PointSegment(peak2, edgeA2, edgeB2);
            var gableTop3 = new Vector3(gableTop2.x, peakHeight, gableTop2.y);

            return new MeshDraft().AddTriangle(edgeA3, edgeB3, gableTop3, true)
                .AddTriangle(edgeA3, gableTop3, peak3, true)
                .AddTriangle(edgeB3, peak3, gableTop3, true);
        }

        protected static float CalculateVertexHeight(Vector2 vertex, Vector2 edgeA, Vector2 edgeDirection, float roofPitch)
        {
            float distance = Distance.PointLine(vertex, edgeA, edgeDirection);
            return Mathf.Tan(roofPitch*Mathf.Deg2Rad)*distance;
        }

        protected static Vector3 CalculateRoofNormal(Vector2 edgeDirection2, float roofPitch)
        {
            return Quaternion.AngleAxis(roofPitch, edgeDirection2.ToVector3XZ())*Vector3.up;
        }
    }

    public class ProceduralFlatRoof : ProceduralRoof
    {
        public ProceduralFlatRoof(List<Vector2> foundationPolygon, BuildingGenerator.Config config)
            : base(foundationPolygon, config)
        {

        }

        public override CompoundMeshDraft Construct(Vector2 parentLayoutOrigin)
        {
            switch (m_Config.buildingType)
            {
                case BuildingType.k_Hotel:
                    return ConstructSeparatedFrontRoof();
                case BuildingType.k_ComplexBuilding:
                    ComplexBuildingConfig.ComplexBuildingType buildingType = m_Config.m_ComplexBuildingPlannerParams.m_AddedBoundaryWall ? m_Config.complexBuildingParams.higherFloorBuildingType : m_Config.complexBuildingParams.lowerFloorBuildingType;
                    if (buildingType == ComplexBuildingConfig.ComplexBuildingType.k_Hotel)
                    {
                        return ConstructSeparatedFrontRoof();
                    }
                    break;
            }

            var compoundMeshDraft = new CompoundMeshDraft();
            MeshDraft roofSideDraft = ConstructRoofBase(m_UVScale, m_Config.useTexture, out List<Vector2> roofPolygon2, out List<Vector3> roofPolygon3);

            // var tessellator = new Tessellator();
            // tessellator.AddContour(roofPolygon3);
            // tessellator.Tessellate(normal: Vector3.up);
            // MeshDraft roofTopDraft = tessellator.ToMeshDraft()
                // .Move(Vector3.up*m_Thickness);

            // for (int i = 0; i < roofTopDraft.VertexCount; i++)
            // {
                // roofTopDraft.normals.Add(Vector3.up);
            // }

            if (m_Config.useTexture)
            {
                roofSideDraft.Paint(m_RoofSideMat);
                roofSideDraft.name = "HippedRoofSideDraft";
                compoundMeshDraft.Add(roofSideDraft);

                MeshDraft roofTopDraft = new MeshDraft().AddQuad(roofPolygon3[0], roofPolygon3[1], roofPolygon3[2], roofPolygon3[3], true, new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0))
                    .Move(Vector3.up * m_Thickness);
                roofTopDraft.Paint(m_RoofMat);
                roofTopDraft.name = "HippedRoofTopDraft";
                compoundMeshDraft.Add(roofTopDraft);
            }
            else
            {
                roofSideDraft.Paint(m_RoofSideColor, m_VertexRoofMat);
                roofSideDraft.name = "HippedRoof";
                compoundMeshDraft.Add(roofSideDraft);

                MeshDraft roofTopDraft = new MeshDraft().AddQuad(roofPolygon3[0], roofPolygon3[1], roofPolygon3[2], roofPolygon3[3], true)
                    .Move(Vector3.up * m_Thickness);
                roofTopDraft.Paint(m_RoofColor, m_VertexRoofMat);
                roofTopDraft.name = "HippedRoof";
                compoundMeshDraft.Add(roofTopDraft);
            }

            return compoundMeshDraft;
        }

        private CompoundMeshDraft ConstructSeparatedFrontRoof()
        {
            var compoundMeshDraft = new CompoundMeshDraft();
            CompoundMeshDraft roofSideDraft = ConstructSeparatedFrontRoofBase(m_UVScale, m_Config.useTexture, out List<Vector2> roofPolygon2, out List<Vector3> roofPolygon3);
            compoundMeshDraft.Add(roofSideDraft);

            if (m_Config.useTexture)
            {
                MeshDraft roofTopDraft = new MeshDraft().AddQuad(roofPolygon3[0], roofPolygon3[1], roofPolygon3[2], roofPolygon3[3], true, new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0))
                    .Move(Vector3.up * m_Thickness);
                roofTopDraft.Paint(m_RoofMat);
                roofTopDraft.name = "HippedRoofTopDraft";
                compoundMeshDraft.Add(roofTopDraft);
            }
            else
            {
                MeshDraft roofTopDraft = new MeshDraft().AddQuad(roofPolygon3[0], roofPolygon3[1], roofPolygon3[2], roofPolygon3[3], true)
                    .Move(Vector3.up * m_Thickness);
                roofTopDraft.Paint(m_RoofColor, m_VertexRoofMat);
                roofTopDraft.name = "HippedRoof";
                compoundMeshDraft.Add(roofTopDraft);
            }

            return compoundMeshDraft;
        }
    }

    public class ProceduralHippedRoof : ProceduralRoof
    {
        private const float k_RoofPitch = 25;

        public ProceduralHippedRoof(List<Vector2> foundationPolygon, BuildingGenerator.Config config)
            : base(foundationPolygon, config)
        {
        }

        public override CompoundMeshDraft Construct(Vector2 parentLayoutOrigin)
        {
            MeshDraft roofSideDraft = ConstructRoofBase(m_UVScale, m_Config.useTexture, out List<Vector2> roofPolygon2, out List<Vector3> roofPolygon3);
            var skeletonGenerator = new StraightSkeletonGenerator();
            StraightSkeleton skeleton = skeletonGenerator.Generate(roofPolygon2);

            var roofTopDraft = new MeshDraft();
            foreach (List<Vector2> skeletonPolygon2 in skeleton.polygons)
            {
                roofTopDraft.Add(ConstructContourDraft(skeletonPolygon2, k_RoofPitch, m_Config.useTexture));
            }
            roofTopDraft.Move(Vector3.up*m_Thickness);

            if (m_Config.useTexture)
            {
                roofSideDraft.Paint(m_RoofSideMat);
                roofSideDraft.name = "HippedRoofSideDraft";
                roofTopDraft.Paint(m_RoofMat);
                roofTopDraft.name = "HippedRoofTopDraft";
            }
            else
            {
                roofSideDraft.Paint(m_RoofSideColor, m_VertexRoofMat);
                roofSideDraft.name = "HippedRoof";
                roofTopDraft.Paint(m_RoofColor, m_VertexRoofMat);
                roofTopDraft.name = "HippedRoof";
            }

            var compoundMeshDraft = new CompoundMeshDraft();
            compoundMeshDraft.Add(roofTopDraft).Add(roofSideDraft);

            return compoundMeshDraft;
        }
    }

    public class ProceduralGabledRoof : ProceduralRoof
    {
        private const float k_RoofPitch = 25;

        public ProceduralGabledRoof(List<Vector2> foundationPolygon, BuildingGenerator.Config config)
            : base(foundationPolygon, config)
        {
        }

        public override CompoundMeshDraft Construct(Vector2 parentLayoutOrigin)
        {
            MeshDraft roofDraft = ConstructRoofBase(m_UVScale, m_Config.useTexture, out List<Vector2> roofPolygon2, out List<Vector3> roofPolygon3);

            var skeletonGenerator = new StraightSkeletonGenerator();
            StraightSkeleton skeleton = skeletonGenerator.Generate(roofPolygon2);

            var roofTop = new MeshDraft();
            foreach (List<Vector2> skeletonPolygon2 in skeleton.polygons)
            {
                if (skeletonPolygon2.Count == 3)
                {
                    roofTop.Add(ConstructGableDraft(skeletonPolygon2, k_RoofPitch));
                }
                else
                {
                    roofTop.Add(ConstructContourDraft(skeletonPolygon2, k_RoofPitch));
                }
            }
            roofTop.Move(Vector3.up*m_Thickness);

            if (m_Config.useTexture)
            {
                roofDraft.Add(roofTop).Paint(m_RoofMat);
            }
            else
            {
                roofDraft.Add(roofTop).Paint(m_RoofColor, m_VertexRoofMat);
            }

            return new CompoundMeshDraft();
        }
    }
}
