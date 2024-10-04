using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace PlateauToolkit.Sandbox.Editor
{
    public class PlateauSandboxGenerateRoadNetwork
    {
        RoadNetworkDataGetter m_roadNetworkGetter;

        internal PlateauSandboxInstantiation InstantiateSelectedObject(GameObject obj, Vector3 position, Quaternion rotation, HideFlags? hideFlags = null)
        {
            string gameObjectName = GameObjectUtility.GetUniqueNameForSibling(null, obj.name);

            var gameObject = (GameObject)PrefabUtility.InstantiatePrefab(obj);
            gameObject.name = gameObjectName;
            gameObject.transform.rotation = rotation;

            if (gameObject.TryGetComponent(out IPlateauSandboxPlaceableObject placeable))
            {
                placeable.SetPosition(position);
            }
            else
            {
                gameObject.transform.position = position;
            }

            if (hideFlags != null)
            {
                gameObject.hideFlags = hideFlags.Value;
            }

            return new PlateauSandboxInstantiation(gameObject, obj);
        }

        public void PlaceVehicles(List<GameObject> vehicles)
        {
            foreach (var vehicle in vehicles.Select((value, index) => new { value, index }))
            {
                Debug.Log($"place vehicle {vehicle.value.name}");

                (Vector3, Vector3) pos = GetPositionAndVector(vehicle.index);
                PlateauSandboxInstantiation obj = InstantiateSelectedObject(vehicle.value, pos.Item1, Quaternion.identity);

                //IPlateauSandboxTrafficObject継承、PlateauSandboxTrafficMovementがアタッチされていない
                if (obj.SceneObject.TryGetComponent<IPlateauSandboxTrafficObject>(out _) &&
                    !obj.SceneObject.TryGetComponent<PlateauSandboxTrafficMovement>(out _))
                {
                    PlateauSandboxTrafficMovement trafficMovement = obj.SceneObject.AddComponent<PlateauSandboxTrafficMovement>();
                }
            }
        }

        public (Vector3, Vector3) GetPositionAndVector(int index)
        {
            var roadNetworkRoads = m_roadNetworkGetter.GetRoadBases();

            Vector3 position = Vector3.zero;

            foreach (RnDataRoadBase roadbase in roadNetworkRoads)
            {
                if (roadbase is RnDataRoad)
                {
                    RnDataRoad road = (RnDataRoad)roadbase;

                    RnDataLane lane = road.GetMainLanes(m_roadNetworkGetter).First();
                    RnDataLineString linestring = lane.GetChildLineString(m_roadNetworkGetter, lane.IsReverse ? 1 : 2);
                    if (linestring.Points.Count > index)
                    {
                        var points = linestring.GetChildPoints(m_roadNetworkGetter);
                        position = points[index].Vertex;

                        //Debug.Log($"point {pointid.ID} road {GetRoadFromPointIndex(pointid.ID)?.TargetTran?.name}");
                        //var roads = GetRoadFromPointIndex(index);
                        var roads = points[index].GetRoad(m_roadNetworkGetter);
                        foreach (var foundRoad in roads)
                        {
                            Debug.Log($"point {index} road {foundRoad.TargetTran?.name}");
                        }


                        break;
                    }
                    else
                    {
                        index -= linestring.Points.Count;
                    }
                }
            }

            return (position, Vector3.forward);
        }

        public class IDGen : IRnIDGeneratable
        {

        }

        //public List<RnDataRoad> GetRoadFromLinestring(RnDataLineString linestring)
        //{
        //    List<RnDataRoad> outRoads = new();

        //    List<RnDataWay> ways = linestring.GetParentWays(m_roadNetworkGetter);
        //    foreach (RnDataWay way in ways)
        //    {
        //        List<RnDataLane> lanes = way.GetParentLanes(m_roadNetworkGetter);
        //        foreach (RnDataLane lane in lanes)
        //        {
        //            List<RnDataRoad> roads = lane.GetParentRoads(m_roadNetworkGetter);
        //            outRoads.AddRange(roads);
        //        }
        //    }
        //    return outRoads;
        //}

        //public List<RnDataRoad> GetRoadFromPointIndex(int index)
        //{
        //    List<RnDataRoad> outRoads = new();

        //    var point = m_roadNetworkGetter.GetPoints()[index];
        //    List<RnDataLineString> linestrings = point.GetParentLineString(m_roadNetworkGetter);
        //    foreach (RnDataLineString linestring in linestrings)
        //    {
        //        outRoads.AddRange(linestring.GetRoad(m_roadNetworkGetter));
        //        //outRoads.AddRange(GetRoadFromLinestring(linestring));
        //    }
        //    return outRoads;
        //}

        public void Initialize()
        {
            //if (roadNetworkManager != null)
            //{
            //    GameObject.DestroyImmediate(roadNetworkManager.gameObject);
            //}

            //var roadNetworkManagerGameObject = new GameObject("SimRoadNetwork");

            //roadNetworkManager = roadNetworkManagerGameObject.AddComponent<SimRoadNetworkManager>();

            var roadNetwork = GameObject.FindObjectOfType<PLATEAURnStructureModel>();

            m_roadNetworkGetter = roadNetwork.GetRoadNetworkDataGetter();

            //var roadNetworkWays = roadNetworkGetter.GetWays();

            //var roadNetworkLanes = roadNetworkGetter.GetLanes();

            //var roadNetworkPoints = roadNetworkGetter.GetPoints();

            //var roadNetworkBlocks = roadNetworkGetter.GetBlocks();

            //var roadNetworkRoads = roadNetworkGetter.GetRoadBases();

            var roadNetworkLineStrings = m_roadNetworkGetter.GetLineStrings();

            //var roadNetworkNodes = roadNetworkRoads.OfType<RnDataIntersection>().ToList();

            //var roadNetworkLinks = roadNetworkRoads.OfType<RnDataRoad>().ToList();

            //Debug.Log($"roadNetworkNodes {roadNetworkNodes.Count}");
            //Debug.Log($"roadNetworkLinks {roadNetworkLinks.Count}");
            //Debug.Log($"roadNetworkWays {roadNetworkWays.Count}");
            //Debug.Log($"roadNetworkLanes {roadNetworkLanes.Count}");
            //Debug.Log($"roadNetworkPoints {roadNetworkPoints.Count}");
            //Debug.Log($"roadNetworkBlocks {roadNetworkBlocks.Count}");
            Debug.Log($"roadNetworkLineStrings {roadNetworkLineStrings.Count}");


            //foreach (RnDataLineString line in roadNetworkLineStrings)
            //{
            //    //line.Points

            //}

            //foreach (RnDataWay way in roadNetworkWays)
            //{
            //    RnID<RnDataLineString> id = way.LineString;
            //    RnDataLineString linestring = roadNetworkLineStrings[id.ID];

            //    Debug.Log($"way line points {linestring.Points.Count}");
            //}

            //foreach (RnDataPoint point in roadNetworkPoints)
            //{

            //}

            //foreach (RnDataBlock block in roadNetworkBlocks)
            //{

            //}

            //foreach (RnDataLane lane in roadNetworkLanes)
            //{

            //}

            // ノードの生成

            //var simRoadNetworkNodes = new List<SimRoadNetworkNode>();

            //foreach (var node in roadNetworkRoads.Select((value, index) => new { value, index }))
            //{
            //    if (node.value is RnDataIntersection)
            //    {
            //        simRoadNetworkNodes.Add(new SimRoadNetworkNode("Node" + simRoadNetworkNodes.Count, node.index));

            //        RnDataIntersection intersection = (RnDataIntersection)node.value;

            //        Debug.Log($"intersection edges {intersection.Edges.Count}");
            //    }

            //    if (node.value is RnDataRoad)
            //    {
            //        RnDataRoad road = node.value as RnDataRoad;

            //        Debug.Log($"road lanes {road.MainLanes.Count}");
            //    }
            //}

            //roadNetworkManager.SimRoadNetworkNodes = simRoadNetworkNodes;

            // リンクの生成

            //var simRoadNetworkLinks = new List<SimRoadNetworkLink>();

            //Dictionary<int, SimRoadNetworkNode> vNodes = new Dictionary<int, SimRoadNetworkNode>();

            //foreach (var link in roadNetworkLinks.Select((value, index) => new { value, index }))
            //{
            //    // リンクが接続されていない場合はスキップ
            //    if (!link.value.Next.IsValid && !link.value.Prev.IsValid)
            //    {
            //        Debug.LogWarning("Link is not connected to any node.");

            //        continue;
            //    }

            //    var next = link.value.Next.IsValid ? roadNetworkRoads[link.value.Next.ID] as RnDataIntersection : null;
            //    var prev = link.value.Prev.IsValid ? roadNetworkRoads[link.value.Prev.ID] as RnDataIntersection : null;

            //    仮想ノードが割り当てられている場合はそのノードを取得
            //    SimRoadNetworkNode vNext = vNodes.ContainsKey(link.value.Next.ID) ? vNodes[link.value.Next.ID] : null;
            //    SimRoadNetworkNode vPrev = vNodes.ContainsKey(link.value.Prev.ID) ? vNodes[link.value.Prev.ID] : null;

            //    接続先がリンクかつ仮想ノードが割り当てられていない場合は仮想ノードを生成
            //    if (next == null && link.value.Next.IsValid && roadNetworkRoads[link.value.Next.ID] as RnDataRoad != null && vNext == null)
            //    {
            //        vNext = new SimRoadNetworkNode("Node" + simRoadNetworkNodes.Count, -1);

            //        simRoadNetworkNodes.Add(vNext);

            //        vNodes.Add(link.value.Next.ID, vNext);
            //    }
            //    if (prev == null && link.value.Prev.IsValid && roadNetworkRoads[link.value.Prev.ID] as RnDataRoad != null && vPrev == null)
            //    {
            //        vPrev = new SimRoadNetworkNode("Node" + simRoadNetworkNodes.Count, -1);

            //        simRoadNetworkNodes.Add(vPrev);

            //        vNodes.Add(link.value.Prev.ID, vPrev);
            //    }

            //    リンクの生成
            //   var simLinkUp = new SimRoadNetworkLink("Link" + link.index + "U", link.index);
            //    var simLinkDown = new SimRoadNetworkLink("Link" + link.index + "D", link.index);

            //    simLinkUp.IsLeft = true;
            //    simLinkDown.IsLeft = false;

            //    simLinkUp.UpNode = next != null ? simRoadNetworkNodes.Find(x => x.OriginNode == next) : vNext;
            //    simLinkUp.DownNode = prev != null ? simRoadNetworkNodes.Find(x => x.OriginNode == prev) : vPrev;

            //    simLinkDown.UpNode = prev != null ? simRoadNetworkNodes.Find(x => x.OriginNode == prev) : vPrev;
            //    simLinkDown.DownNode = next != null ? simRoadNetworkNodes.Find(x => x.OriginNode == next) : vNext;

            //    simLinkUp.Pair = simLinkDown;
            //    simLinkDown.Pair = simLinkUp;

            //    simRoadNetworkLinks.Add(simLinkUp);
            //    simRoadNetworkLinks.Add(simLinkDown);
            //}

            //roadNetworkManager.SimRoadNetworkLinks = simRoadNetworkLinks;

            // トラックの生成（マネージャにリンクが登録されている必要あり）
            //foreach (var node in roadNetworkManager.SimRoadNetworkNodes)
            //{
            //    node.GenerateTrack();
            //}
        }

    }
}
