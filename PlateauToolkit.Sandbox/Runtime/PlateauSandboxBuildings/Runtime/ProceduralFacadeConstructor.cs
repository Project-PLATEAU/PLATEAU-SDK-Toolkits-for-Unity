using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces;
using ProceduralToolkit;
using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime
{
    public class ProceduralFacadeConstructor : FacadeConstructor
    {
        [SerializeField]
        private RendererProperties rendererProperties;
        [SerializeField]
        private Material glassMaterial;
        [SerializeField]
        private Material roofMaterial;
        [SerializeField]
        private Material wallMaterial;
        [SerializeField]
        private Material sampleTexturedMaterial;

        public Material SampleTexturedMaterial { get; set; }

        public override void Construct(List<Vector2> foundationPolygon, List<ILayout> layouts, Transform parentTransform)
        {
            var facadesDraft = new CompoundMeshDraft();

            var rendererGo = new GameObject("Facades");
            rendererGo.transform.SetParent(parentTransform, false);

            for (int i = 0; i < layouts.Count; i++)
            {
                ILayout layout = layouts[i];

                Vector2 a = foundationPolygon.GetLooped(i + 1);
                Vector2 b = foundationPolygon[i];
                Vector3 normal = (b - a).Perp().ToVector3XZ();

                var facade = new CompoundMeshDraft();
                ConstructLayout(facade, Vector2.zero, layout);
                facade.Rotate(Quaternion.LookRotation(normal));
                facade.Move(a.ToVector3XZ());
                facadesDraft.Add(facade);
            }

            facadesDraft.MergeDraftsWithTheSameName();
            facadesDraft.SortDraftsByName();

            MeshFilter meshFilter = rendererGo.gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = facadesDraft.ToMeshWithSubMeshes();

            MeshRenderer meshRenderer = rendererGo.gameObject.AddComponent<MeshRenderer>();
            meshRenderer.ApplyProperties(rendererProperties);

            var materials = new List<Material>();
            foreach (MeshDraft draft in facadesDraft)
            {
                if (draft.name == "Glass")
                {
                    materials.Add(glassMaterial);
                }
                else if (draft.name == "Roof")
                {
                    materials.Add(roofMaterial);
                }
                else if (draft.name == "Wall")
                {
                    materials.Add(wallMaterial);
                }
            }
            meshRenderer.materials = materials.ToArray();
        }

        public override CompoundMeshDraft BuildMesh(List<Vector2> foundationPolygon, List<ILayout> layouts)
        {
            var facadesDraft = new CompoundMeshDraft();
            for (int i = 0; i < layouts.Count; i++)
            {
                ILayout layout = layouts[i];

                Vector2 a = foundationPolygon.GetLooped(i + 1);
                Vector2 b = foundationPolygon[i];
                Vector3 normal = (b - a).Perp().ToVector3XZ();

                var facade = new CompoundMeshDraft();
                ConstructLayout(facade, Vector2.zero, layout);
                facade.Rotate(Quaternion.LookRotation(normal));
                facade.Move(a.ToVector3XZ());
                facadesDraft.Add(facade);
            }
            facadesDraft.MergeDraftsWithTheSameName();
            facadesDraft.SortDraftsByName();

            return facadesDraft;
        }

        private static void ConstructLayout(CompoundMeshDraft draft, Vector2 parentLayoutOrigin, ILayout layout)
        {
            foreach (ILayoutElement element in layout)
            {
                ConstructElement(draft, parentLayoutOrigin + layout.origin, element);
            }
        }

        private static void ConstructElement(CompoundMeshDraft draft, Vector2 parentLayoutOrigin, ILayoutElement element)
        {
            var layout = element as ILayout;
            if (layout != null)
            {
                ConstructLayout(draft, parentLayoutOrigin, layout);
                return;
            }

            if (element is IConstructible<CompoundMeshDraft> constructible)
            {
                draft.Add(constructible.Construct(parentLayoutOrigin));
            }
        }
    }
}
