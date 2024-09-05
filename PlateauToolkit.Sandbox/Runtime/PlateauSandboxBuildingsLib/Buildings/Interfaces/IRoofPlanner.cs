using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces
{
    public interface IRoofPlanner
    {
        IConstructible<CompoundMeshDraft> Plan(List<Vector2> foundationPolygon, BuildingGenerator.Config config);
    }
}
