using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Configs;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime
{
    [CreateAssetMenu(menuName = "ProceduralToolkit/Buildings/Procedural Roof Planner/Complex Building", order = 7)]
    public class ProceduralRoofComplexBuildingPlanner : RoofPlanner
    {
        public override IConstructible<CompoundMeshDraft> Plan(List<Vector2> foundationPolygon, BuildingGenerator.Config config)
        {
            ComplexBuildingConfig.ComplexBuildingType buildingType = config.m_ComplexBuildingPlannerParams.m_AddedBoundaryWall ? config.complexBuildingParams.higherFloorBuildingType : config.complexBuildingParams.lowerFloorBuildingType;

            return new ProceduralFlatRoof(foundationPolygon, config)
            {
                m_Thickness = buildingType == ComplexBuildingConfig.ComplexBuildingType.k_Hotel ? config.complexHotelParams.roofThickness : 0.05f,
                m_Overhang = 0
            };
        }
    }
}
