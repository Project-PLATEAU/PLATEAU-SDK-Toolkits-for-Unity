using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Rendering.Editor
{

    class CreateLodGroup
    {
        internal event Action OnProcessingFinished;

        internal void CreateLodGroups()
        {

            foreach (GameObject buildingGroup in GameObject.FindObjectsOfType<GameObject>())
            {
                if (!buildingGroup.name.Contains("bldg") && !buildingGroup.name.Contains("BLD"))
                {
                    continue;
                }
                if (!buildingGroup.name.Contains("groupParent"))
                {
                    continue;
                }
                if (buildingGroup.GetComponent<LODGroup>() != null)
                {
                    continue;
                }
                if (buildingGroup.name.Contains(PlateauRenderingConstants.k_PostfixLodGrouped))
                {
                    continue;
                }
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("LOD grouping");
                Undo.RegisterCompleteObjectUndo(buildingGroup, "LOD grouping");

                LODGroup lodGroup = buildingGroup.AddComponent<LODGroup>();
                lodGroup.fadeMode = LODFadeMode.CrossFade;

                List<Renderer> lodRenderers = new List<Renderer>();
                List<LOD> lods = new List<LOD>();

                // Because the loop is reversed, the highest LOD as defined by Plateau will be stored as Unity's lowest LOD
                // This is assuming Plateau model will be stacked LOD0, Lod1, Lod2... etc
                for (int i = buildingGroup.transform.childCount - 1; i >= 0; i--)
                {
                    lodRenderers.Clear();
                    Transform lodBldgGroup = buildingGroup.transform.GetChild(i);
                    Renderer[] renderersInChildren = lodBldgGroup.GetComponentsInChildren<Renderer>();
                    lodBldgGroup.gameObject.SetActive(true);
                    lodRenderers.AddRange(renderersInChildren);
                    lods.Add(new LOD(PlateauRenderingConstants.k_LodDistances[i], lodRenderers.ToArray()));
                }

                lodGroup.SetLODs(lods.ToArray());
                lodGroup.RecalculateBounds();
                buildingGroup.name += PlateauRenderingConstants.k_PostfixLodGrouped;

                // When creating LOD groups from combined mesh,
                // we break down the meshes and these operations include reparenting and destroying objects.
                // We need to clear the Undo stack for these operations.
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                Undo.ClearUndo(buildingGroup);
            }
            OnProcessingFinished();
        }
    }
}