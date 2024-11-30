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

        private const string k_ElectricWireRootName = "Wires";

        private readonly List<PlateauSandboxElectricPostWire> m_PostWires = new();

        public PlateauSandboxElectricPostWireHandler(GameObject post)
        {
            m_WireRoot = post.transform.Find(k_ElectricWireRootName).gameObject;
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

        public void ShowToTarget(PlateauSandboxElectricPost targetPost)
        {
            foreach (var postWire in m_PostWires)
            {
                var targetConnectPosition = targetPost.GetConnectPoint(postWire.WireType);
                postWire.SetElectricNode(targetConnectPosition);
            }
        }

        public void Hide()
        {
            foreach (var postWire in m_PostWires)
            {
                postWire.Hide();
            }
        }
    }
}