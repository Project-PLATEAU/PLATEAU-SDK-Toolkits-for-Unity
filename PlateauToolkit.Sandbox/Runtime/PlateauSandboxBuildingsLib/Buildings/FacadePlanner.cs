using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings
{
    public abstract class FacadePlanner : ScriptableObject, IFacadePlanner
    {
        public abstract List<ILayout> Plan(List<Vector2> foundationPolygon, BuildingGenerator.Config config);
    }
}
