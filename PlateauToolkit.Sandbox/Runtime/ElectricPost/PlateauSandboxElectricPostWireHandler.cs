using PlateauToolkit.Sandbox.Runtime.ElectricPost;
using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime
{
    /// <summary>
    /// 電柱のワイヤーのハンドラー
    /// </summary>
    public class PlateauSandboxElectricPostWireHandler
    {
        private GameObject m_WireRoot;

        private const string k_FrontElectricWireRootName = "Front_Wires";
        private const string k_BackElectricWireRootName = "Back_Wires";

        private readonly List<PlateauSandboxElectricPostWire> m_PostWires = new();

        public PlateauSandboxElectricPostWireHandler(GameObject post, bool isFront)
        {
            m_WireRoot = post.transform.Find(isFront ? k_FrontElectricWireRootName : k_BackElectricWireRootName).gameObject;
            InitializeWires();
        }

        private void InitializeWires()
        {
            foreach (Transform child in m_WireRoot.transform)
            {
                var wire = new PlateauSandboxElectricPostWire(child.gameObject);
                if (wire.WireType == PlateauSandboxElectricPostWireType.k_InValid)
                {
                    continue;
                }

                wire.Show(false);
                m_PostWires.Add(wire);
            }
        }

        public void ShowToPoint(Vector3 targetPoint)
        {
            // 中心からターゲットまでの位置
            var centerPoint = GetCenterWirePoint();
            var centerToTarget = centerPoint - targetPoint;
            foreach (var postWire in m_PostWires)
            {
                // ワイヤーの支点から、移動分伸ばした位置
                var expandedPoint = postWire.WirePosition - centerToTarget;
                postWire.SetElectricNode(expandedPoint);
            }
        }

        public void Cancel()
        {
            foreach (var postWire in m_PostWires)
            {
                postWire.Cancel();
            }
        }

        public Vector3 GetCenterWirePoint()
        {
            var centerWire = m_PostWires
                    .Find(wire => wire.WireType == PlateauSandboxElectricPostWireType.k_TopB);

            if (centerWire == null)
            {
                return Vector3.zero;
            }
            return centerWire.WirePosition;
        }
    }
}