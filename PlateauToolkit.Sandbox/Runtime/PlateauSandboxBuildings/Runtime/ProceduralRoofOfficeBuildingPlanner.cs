using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime
{
    [CreateAssetMenu(menuName = "ProceduralToolkit/Buildings/Procedural Roof Planner/Office Building", order = 1)]
    public class ProceduralRoofOfficeBuildingPlanner : RoofPlanner
    {
        public override IConstructible<CompoundMeshDraft> Plan(List<Vector2> foundationPolygon, BuildingGenerator.Config config)
        {
            return new ProceduralFlatRoof(foundationPolygon, config)
            {
                m_Thickness = 0.05f,
                m_Overhang = 0
            };
        }
    }
}
