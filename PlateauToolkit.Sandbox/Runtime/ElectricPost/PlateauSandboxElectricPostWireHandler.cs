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
            public List<PlateauSandboxElectricPostWire> PostWires => m_PostWires;

        private bool m_IsFrontShowing = false;
        public bool IsFrontShowing => m_IsFrontShowing;

        private bool m_IsBackShowing = false;
        public bool IsBackShowing => m_IsBackShowing;

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

        public void ShowToTarget(bool isOwnFront, PlateauSandboxElectricPost targetPost, bool isTargetFront)
        {
            foreach (var postWire in m_PostWires)
            {
                if (postWire.IsFrontWire != isOwnFront)
                {
                    continue;
                }

                var targetConnectPosition = targetPost.GetConnectPoint(postWire.WireType, isTargetFront);
                postWire.SetElectricNode(targetConnectPosition);
            }

            if (isOwnFront)
            {
                m_IsFrontShowing = true;
            }
            else
            {
                m_IsBackShowing = true;
            }
        }

        public void Hide(bool isFront)
        {
            Debug.Log($"================ Hide {m_WireRoot.name}");
            foreach (var postWire in m_PostWires)
            {
                if (postWire.IsFrontWire == isFront)
                {
                    postWire.Hide();
                }
            }
            if (isFront)
            {
                m_IsFrontShowing = false;
            }
            else
            {
                m_IsBackShowing = false;
            }
        }
    }
}