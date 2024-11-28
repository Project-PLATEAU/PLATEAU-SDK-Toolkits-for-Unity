using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime
{
    [CreateAssetMenu(menuName = "ProceduralToolkit/Buildings/Procedural Roof Planner/House", order = 2)]
    public class ProceduralRoofResidencePlanner : RoofPlanner
    {
        public override IConstructible<CompoundMeshDraft> Plan(List<Vector2> foundationPolygon, BuildingGenerator.Config config)
        {
            switch (config.residenceParams.roofType)
            {
                case RoofType.flat:
                    return new ProceduralFlatRoof(foundationPolygon, config)
                    {
                        m_Thickness = config.residenceParams.roofThickness
                    };
                case RoofType.hipped:
                    return new ProceduralHippedRoof(foundationPolygon, config)
                    {
                        m_Thickness = config.residenceParams.roofThickness
                    };
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
