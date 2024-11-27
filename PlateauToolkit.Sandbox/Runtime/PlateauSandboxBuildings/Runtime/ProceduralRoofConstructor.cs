using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces;
using ProceduralToolkit;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime
{
    // [CreateAssetMenu(menuName = "ProceduralToolkit/Buildings/Procedural Roof Constructor", order = 1)]
    public class ProceduralRoofConstructor : RoofConstructor
    {
        [SerializeField]
        private RendererProperties rendererProperties;

        public override void Construct(IConstructible<CompoundMeshDraft> constructible, Transform parentTransform)
        {
            CompoundMeshDraft draft = constructible.Construct(Vector2.zero);

            MeshFilter meshFilter = parentTransform.gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = draft.ToMeshDraft().ToMesh();

            MeshRenderer meshRenderer = parentTransform.gameObject.AddComponent<MeshRenderer>();
            meshRenderer.ApplyProperties(rendererProperties);
        }

        public override CompoundMeshDraft BuildMesh(IConstructible<CompoundMeshDraft> constructible, float facadesHeight)
        {
            CompoundMeshDraft draft = constructible.Construct(Vector2.zero);
            draft.Move(new Vector3(0, facadesHeight, 0));
            draft.MergeDraftsWithTheSameName();
            draft.SortDraftsByName();

            return draft;
        }
    }
}
