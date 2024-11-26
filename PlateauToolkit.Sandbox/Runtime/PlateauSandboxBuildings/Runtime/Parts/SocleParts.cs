using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using ProceduralToolkit;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime.Parts
{
    [CustomEditor(typeof(SocleParts))]
    public class SoclePartsEditor : UnityEditor.Editor
    {
        BaseParts.SocleType m_PartsTypeIndex = 0;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            m_PartsTypeIndex = (BaseParts.SocleType)EditorGUILayout.EnumPopup("パーツタイプ", m_PartsTypeIndex);

            if (GUILayout.Button("Build Mesh"))
            {
                var parts = (SocleParts)target;
                parts.BuildMesh(m_PartsTypeIndex);
            }
        }
    }

    public class SocleParts : BaseParts
    {
        private static MeshDraft PerforatedQuad(
            Vector3 min,
            Vector3 max,
            Vector3 innerMin,
            Vector3 innerMax)
        {
            Vector3 size = max - min;
            Vector3 widthVector = size.ToVector3XZ();
            Vector3 heightVector = size.ToVector3Y();
            Vector3 normal = Vector3.Cross(heightVector, widthVector).normalized;

            Vector3 innerSize = innerMax - innerMin;
            Vector3 innerHeight = innerSize.ToVector3Y();
            Vector3 innerWidth = innerSize.ToVector3XZ();

            Vector3 vertex0 = min;
            Vector3 vertex1 = min + heightVector;
            Vector3 vertex2 = max;
            Vector3 vertex3 = min + widthVector;
            Vector3 window0 = innerMin;
            Vector3 window1 = innerMin + innerHeight;
            Vector3 window2 = innerMax;
            Vector3 window3 = innerMin + innerWidth;

            return new MeshDraft
            {
                vertices = new List<Vector3>(8)
                {
                    vertex0,
                    vertex1,
                    vertex2,
                    vertex3,
                    window0,
                    window1,
                    window2,
                    window3,
                },
                normals = new List<Vector3>(8) {normal, normal, normal, normal, normal, normal, normal, normal},
                triangles = new List<int>(24) {0, 1, 4, 4, 1, 5, 1, 2, 5, 5, 2, 6, 2, 3, 6, 6, 3, 7, 3, 0, 7, 7, 0, 4},
            };
        }

        public static MeshDraft Wall(Vector3 origin, float width, float height, Color wallColor)
        {
            return new MeshDraft {name = WallDraftName}
                .AddQuad(origin, Vector3.right*width, Vector3.up*height, calculateNormal:true)
                .Paint(wallColor);
        }

        private static CompoundMeshDraft SocleWindowed(Vector3 origin, float width, float height, Color wallColor, Color glassColor)
        {
            if (width < SocleWindowMinWidth)
            {
                return new CompoundMeshDraft().Add(Wall(origin, width, height, wallColor));
            }

            Vector3 widthVector = Vector3.right*width;
            Vector3 heightVector = Vector3.up*height;

            Vector3 windowWidth = Vector3.right*SocleWindowWidth;
            Vector3 windowHeigth = Vector3.up*SocleWindowHeight;
            Vector3 windowDepth = Vector3.forward*SocleWindowDepth;
            Vector3 windowOrigin = origin + widthVector/2 - windowWidth/2 + Vector3.up*SocleWindowHeightOffset;
            Vector3 windowMax = windowOrigin + windowWidth + windowHeigth;

            MeshDraft frame = MeshDraft.PartialBox(windowWidth, -windowDepth, windowHeigth, Directions.All & ~Directions.ZAxis)
                .Move(windowOrigin + windowWidth/2 + windowHeigth/2 + windowDepth/2);

            MeshDraft wall = PerforatedQuad(origin, origin + widthVector + heightVector, windowOrigin, windowMax)
                .Add(frame)
                .Paint(wallColor);
            wall.name = WallDraftName;

            MeshDraft glass = new MeshDraft()
                .AddQuad(windowOrigin + windowDepth/2, windowWidth, windowHeigth, true)
                .Paint(glassColor);
            glass.name = GlassDraftName;

            return new CompoundMeshDraft().Add(wall).Add(glass);
        }

        public void BuildMesh(SocleType partsTypeIndex)
        {
            Vector3 widthVector = Vector3.right*Width;
            Vector3 heightVector = Vector3.up*Height;

            Vector3 windowWidth = Vector3.right*SocleWindowWidth;
            Vector3 windowHeigth = Vector3.up*SocleWindowHeight;
            Vector3 windowDepth = Vector3.forward*SocleWindowDepth;
            Vector3 windowOrigin = widthVector/2 - windowWidth/2 + Vector3.up*SocleWindowHeightOffset;
            Vector3 windowMax = windowOrigin + windowWidth + windowHeigth;

            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = Resources.Load<Material>("Wall");

            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            switch (partsTypeIndex)
            {
                case SocleType.socle:
                {
                    MeshDraft wall = Wall(Vector3.zero, Width, Height, WallColor);
                    meshFilter.mesh = wall.ToMesh();
                    break;
                }
                case SocleType.socleWindowFrame:
                {
                    MeshDraft frame = MeshDraft.PartialBox(windowWidth, -windowDepth, windowHeigth, Directions.All & ~Directions.ZAxis)
                        .Move(windowOrigin + windowWidth/2 + windowHeigth/2 + windowDepth/2)
                        .Paint(WallColor);
                    meshFilter.mesh = frame.ToMesh();
                    break;
                }
                case SocleType.socleWindowWall:
                {
                    MeshDraft wall = PerforatedQuad(Vector3.zero, widthVector + heightVector, windowOrigin, windowMax)
                        .Paint(WallColor);
                    meshFilter.mesh = wall.ToMesh();
                    break;
                }
                case SocleType.socleWindowGlass:
                {
                    MeshDraft glass = new MeshDraft()
                        .AddQuad(windowOrigin + windowDepth/2, windowWidth, windowHeigth, true)
                        .Paint(GlassColor);
                    meshFilter.mesh = glass.ToMesh();
                    break;
                }
                case SocleType.socleWindowAll:
                {
                    CompoundMeshDraft windowpane = SocleWindowed(Vector3.zero, Width, Height, WallColor, GlassColor);
                    meshFilter.mesh = windowpane.ToMeshDraft().ToMesh();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(partsTypeIndex), partsTypeIndex, null);
            }
        }
    }
}
