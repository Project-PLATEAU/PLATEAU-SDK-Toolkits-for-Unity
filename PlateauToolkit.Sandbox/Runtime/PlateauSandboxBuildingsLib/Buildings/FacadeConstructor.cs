using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings
{
    public abstract class FacadeConstructor : ScriptableObject, IFacadeConstructor
    {
        public abstract void Construct(List<Vector2> foundationPolygon, List<ILayout> layouts, Transform parentTransform);
        public abstract CompoundMeshDraft BuildMesh(List<Vector2> foundationPolygon, List<ILayout> layouts);
    }
}
