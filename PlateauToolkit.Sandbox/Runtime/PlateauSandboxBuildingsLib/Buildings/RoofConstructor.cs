using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings
{
    public abstract class RoofConstructor : ScriptableObject, IRoofConstructor
    {
        public abstract void Construct(IConstructible<CompoundMeshDraft> constructible, Transform parentTransform);
        public abstract CompoundMeshDraft BuildMesh(IConstructible<CompoundMeshDraft> constructible, float facadesHeight);
    }
}
