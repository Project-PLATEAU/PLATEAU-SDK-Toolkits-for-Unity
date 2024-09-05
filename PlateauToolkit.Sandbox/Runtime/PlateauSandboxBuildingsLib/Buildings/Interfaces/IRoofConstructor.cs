using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces
{
    public interface IRoofConstructor
    {
        void Construct(IConstructible<CompoundMeshDraft> constructible, Transform parentTransform);
        CompoundMeshDraft BuildMesh(IConstructible<CompoundMeshDraft> constructible, float facadesHeight);
    }
}
