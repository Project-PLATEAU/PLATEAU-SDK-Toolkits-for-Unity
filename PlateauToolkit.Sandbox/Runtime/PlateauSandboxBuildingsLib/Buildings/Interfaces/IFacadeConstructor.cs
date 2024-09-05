using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces
{
    public interface IFacadeConstructor
    {
        void Construct(List<Vector2> foundationPolygon, List<ILayout> layouts, Transform parentTransform);
        CompoundMeshDraft BuildMesh(List<Vector2> foundationPolygon, List<ILayout> layouts);
    }
}
