using ProceduralToolkit;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime.Parts
{
    [CustomEditor(typeof(WallParts))]
    public class WallPartsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Build Mesh"))
            {
                var parts = (WallParts)target;
                parts.BuildMesh();
            }
        }
    }

    public class WallParts : BaseParts
    {
        public void BuildMesh()
        {
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = Resources.Load<Material>("Wall");

            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            MeshDraft wall = new MeshDraft {name = WallDraftName}
                .AddQuad(Vector3.zero, Vector3.right*Width, Vector3.up*Height, calculateNormal:true)
                .Paint(WallColor);
            meshFilter.mesh = wall.ToMesh();
        }
    }
}
