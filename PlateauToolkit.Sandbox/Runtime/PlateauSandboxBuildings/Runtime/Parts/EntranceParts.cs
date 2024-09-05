using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using ProceduralToolkit;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime.Parts
{
    [CustomEditor(typeof(EntranceParts))]
    public class EntrancePartsEditor : UnityEditor.Editor
    {
        BaseParts.EntranceType m_PartsTypeIndex = 0;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            m_PartsTypeIndex = (BaseParts.EntranceType)EditorGUILayout.EnumPopup("パーツタイプ", m_PartsTypeIndex);

            if (GUILayout.Button("Build Mesh"))
            {
                var parts = (EntranceParts)target;
                parts.BuildMesh(m_PartsTypeIndex);
            }
        }
    }

    public class EntranceParts : BaseParts
    {
        public void BuildMesh(EntranceType partsTypeIndex)
        {
            Vector3 origin = Vector3.zero;
            Vector3 widthVector = Vector3.right*Width;
            Vector3 heightVector = Vector3.up*Height;

            Vector3 doorWidth = Vector3.right*EntranceDoorWidth;
            Vector3 doorHeight = Vector3.up*EntranceDoorHeight;
            Vector3 doorThickness = Vector3.back*EntranceDoorThickness;
            Vector3 doorOrigin = origin + widthVector/2 - doorWidth/2;

            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = Resources.Load<Material>("Wall");

            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            switch (partsTypeIndex)
            {
                case EntranceType.entranceBracket:
                {
                    MeshDraft entranceBracket = new MeshDraft().AddTriangleStrip(new List<Vector3>
                    {
                        doorOrigin,
                        origin,
                        doorOrigin + doorHeight,
                        origin + heightVector,
                        doorOrigin + doorHeight + doorWidth,
                        origin + heightVector + widthVector,
                        doorOrigin + doorWidth,
                        origin + widthVector
                    });
                    entranceBracket.Paint(WallColor);
                    meshFilter.mesh = entranceBracket.ToMesh();
                    break;
                }
                case EntranceType.doorFrame:
                {
                    MeshDraft doorFrame = MeshDraft.PartialBox(doorWidth, -doorThickness, doorHeight, Directions.All & ~Directions.ZAxis)
                        .Move(doorOrigin + doorWidth / 2 + doorHeight / 2 + doorThickness / 2)
                        .Paint(DoorColor);
                    meshFilter.mesh = doorFrame.ToMesh();
                    break;
                }
                case EntranceType.door:
                {
                    MeshDraft door = new MeshDraft().AddQuad(doorOrigin + doorThickness, doorWidth, doorHeight, calculateNormal: true)
                        .Paint(DoorColor);
                    meshFilter.mesh = door.ToMesh();
                    break;
                }
                case EntranceType.entranceAll:
                {
                    MeshDraft entranceBracket = new MeshDraft().AddTriangleStrip(new List<Vector3>
                    {
                        doorOrigin,
                        origin,
                        doorOrigin + doorHeight,
                        origin + heightVector,
                        doorOrigin + doorHeight + doorWidth,
                        origin + heightVector + widthVector,
                        doorOrigin + doorWidth,
                        origin + widthVector
                    });
                    entranceBracket.Paint(WallColor);

                    MeshDraft doorFrame = MeshDraft.PartialBox(doorWidth, -doorThickness, doorHeight, Directions.All & ~Directions.ZAxis)
                        .Move(doorOrigin + doorWidth / 2 + doorHeight / 2 + doorThickness / 2)
                        .Paint(DoorColor);
                    entranceBracket.Add(doorFrame);

                    MeshDraft door = new MeshDraft().AddQuad(doorOrigin + doorThickness, doorWidth, doorHeight, calculateNormal: true)
                        .Paint(DoorColor);
                    entranceBracket.Add(door);
                    meshFilter.mesh = entranceBracket.ToMesh();
                    break;
                }
                case EntranceType.roof:
                {
                    Vector3 roofWidthVector = Vector3.right*Width;
                    Vector3 depthVector = Vector3.forward*EntranceRoofDepth;
                    MeshDraft roof = MeshDraft.PartialBox(roofWidthVector, depthVector, Vector3.up*EntranceRoofHeight, Directions.All & ~Directions.Forward)
                        .Move(origin + roofWidthVector/2 + Vector3.up*(Height - EntranceRoofHeight/2) - depthVector/2)
                        .Paint(RoofColor);
                    meshFilter.mesh = roof.ToMesh();
                    break;
                }
                case EntranceType.entranceRoofedAll:
                {
                    MeshDraft entranceBracket = new MeshDraft().AddTriangleStrip(new List<Vector3>
                    {
                        doorOrigin,
                        origin,
                        doorOrigin + doorHeight,
                        origin + heightVector,
                        doorOrigin + doorHeight + doorWidth,
                        origin + heightVector + widthVector,
                        doorOrigin + doorWidth,
                        origin + widthVector
                    });
                    entranceBracket.Paint(WallColor);

                    MeshDraft doorFrame = MeshDraft.PartialBox(doorWidth, -doorThickness, doorHeight, Directions.All & ~Directions.ZAxis)
                        .Move(doorOrigin + doorWidth / 2 + doorHeight / 2 + doorThickness / 2)
                        .Paint(DoorColor);
                    entranceBracket.Add(doorFrame);

                    MeshDraft door = new MeshDraft().AddQuad(doorOrigin + doorThickness, doorWidth, doorHeight, calculateNormal: true)
                        .Paint(DoorColor);
                    entranceBracket.Add(door);

                    Vector3 roofWidthVector = Vector3.right*Width;
                    Vector3 depthVector = Vector3.forward*EntranceRoofDepth;
                    MeshDraft roof = MeshDraft.PartialBox(roofWidthVector, depthVector, Vector3.up*EntranceRoofHeight, Directions.All & ~Directions.Forward)
                        .Move(origin + roofWidthVector/2 + Vector3.up*(Height - EntranceRoofHeight/2) - depthVector/2)
                        .Paint(RoofColor);
                    entranceBracket.Add(roof);
                    meshFilter.mesh = entranceBracket.ToMesh();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(partsTypeIndex), partsTypeIndex, null);
            }
        }
    }
}
