using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using ProceduralToolkit;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime
{
    public abstract class ProceduralFacadeBillboardElement : ProceduralFacadeElement
    {
        public class BillboardColorData
        {
            public Directions m_FrameDirections;
            public Color m_BillboardColor;
            public Material m_VertexColorBillboardMat;
        }

        public class BillboardTexturedData
        {
            public Directions m_FrameDirections;
            public Material m_BillboardMat;
            public Material m_BillboardBottomMat;
        }

        protected static CompoundMeshDraft Billboard(
            Vector3 origin,
            float width,
            float height,
            BillboardColorData billboardColorData
            )
        {
            Vector3 widthVector = Vector3.right*width;
            Vector3 heightVector = Vector3.up*height;
            Vector3 depthVector = Vector3.back*k_ConveniBillboardThickness;

            MeshDraft billboardFrame = MeshDraft.PartialBox(widthVector, -depthVector, heightVector, billboardColorData.m_FrameDirections, generateUV: false)
                .Move(origin + widthVector / 2 + heightVector / 2 + depthVector / 2)
                .Paint(billboardColorData.m_BillboardColor, billboardColorData.m_VertexColorBillboardMat);
            billboardFrame.name = k_WallDraftName;

            MeshDraft billboard = new MeshDraft().AddQuad(origin + depthVector, widthVector, heightVector, calculateNormal: true)
                .Paint(billboardColorData.m_BillboardColor, billboardColorData.m_VertexColorBillboardMat);
            billboard.name = k_WallDraftName;

            return new CompoundMeshDraft().Add(billboardFrame).Add(billboard);
        }

        protected static CompoundMeshDraft BillboardTextured(
            Vector3 origin,
            float width,
            float height,
            BillboardTexturedData billboardTexturedData
            )
        {
            Vector3 widthVector = Vector3.right*width;
            Vector3 heightVector = Vector3.up*height;
            Vector3 depthVector = Vector3.back*k_ConveniBillboardThickness;

            MeshDraft billboardFrame = MeshDraft.PartialBox(widthVector, -depthVector, heightVector, billboardTexturedData.m_FrameDirections, generateUV: true)
                .Move(origin + widthVector / 2 + heightVector / 2 + depthVector / 2)
                .Paint(billboardTexturedData.m_BillboardMat);
            billboardFrame.name = k_BillboardTexturedDraftName;

            MeshDraft billboardDownFrame = MeshDraft.PartialBox(widthVector, -depthVector, heightVector, Directions.Down, generateUV: true)
                .Move(origin + widthVector / 2 + heightVector / 2 + depthVector / 2)
                .Paint(billboardTexturedData.m_BillboardBottomMat);
            billboardDownFrame.name = k_BillboardBottomTexturedDraftName;

            Vector2 uv0 = new(0, 0);
            Vector2 uv1 = new(0, 1);
            Vector2 uv2 = new(1, 1);
            Vector2 uv3 = new(1, 0);
            MeshDraft billboardMeshDraft = new MeshDraft().AddQuad(origin + depthVector, widthVector, heightVector, true, uv0, uv1, uv2, uv3)
                .Paint(billboardTexturedData.m_BillboardMat);
            billboardMeshDraft.name = k_BillboardTexturedDraftName;

            return new CompoundMeshDraft().Add(billboardFrame).Add(billboardDownFrame).Add(billboardMeshDraft);
        }
    }
}
