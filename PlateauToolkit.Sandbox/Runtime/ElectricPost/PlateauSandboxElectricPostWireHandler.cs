using PlateauToolkit.Sandbox.Runtime.ElectricPost;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime
{
    /// <summary>
    /// 電柱のワイヤーのハンドラー
    /// </summary>
    public class PlateauSandboxElectricPostWireHandler
    {
        private GameObject m_Root;
        private GameObject m_WireRoot;
        private GameObject m_OriginalWireRoot;

        private const string k_ElectricOriginalWireRootName = "OriginalWires";
        private const string k_ElectricWireRootName = "Wires";

        private readonly List<PlateauSandboxElectricPostWire> m_OriginalFrontPostWires = new();
        private readonly List<PlateauSandboxElectricPostWire> m_OriginalBackPostWires = new();

        private readonly List<PlateauSandboxElectricPostWire> m_FrontPostWires = new();
        private readonly List<PlateauSandboxElectricPostWire> m_BackPostWires = new();

        public PlateauSandboxElectricPostWireHandler(GameObject post)
        {
            m_Root = post;
            m_WireRoot = post.transform.Find(k_ElectricWireRootName).gameObject;
            m_OriginalWireRoot = post.transform.Find(k_ElectricOriginalWireRootName).gameObject;
            InitializeWires();
        }

        private void InitializeWires()
        {
            ResetWire();

            foreach (Transform child in m_OriginalWireRoot.transform)
            {
                var wire = new PlateauSandboxElectricPostWire(child.gameObject);
                if (wire.WireType == PlateauSandboxElectricPostWireType.k_InValid)
                {
                    continue;
                }

                wire.Show(false);
                if (wire.IsFrontWire)
                {
                    m_OriginalFrontPostWires.Add(wire);
                }
                else
                {
                    m_OriginalBackPostWires.Add(wire);
                }
            }
        }

        private void ResetWire()
        {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(m_WireRoot.gameObject);
#else
            GameObject.Destroy(m_WireRoot.gameObject);
#endif
            m_WireRoot = new GameObject(k_ElectricWireRootName);
            m_WireRoot.transform.SetParent(m_Root.transform);
            m_WireRoot.transform.localPosition = Vector3.zero;
        }

        public void CreateWires(bool isFront, int index)
        {
            if (isFront)
            {
                foreach (var postWire in m_OriginalFrontPostWires)
                {
                    // 複製して使用する
                    var wire = GameObject.Instantiate(postWire.ElectricWire, m_WireRoot.transform);
                    var createWire = new PlateauSandboxElectricPostWire(wire, index);
                    createWire.Show(false);
                    m_FrontPostWires.Add(createWire);
                }
            }
            else
            {
                foreach (var postWire in m_OriginalBackPostWires)
                {
                    // 複製して使用する
                    var wire = GameObject.Instantiate(postWire.ElectricWire, m_WireRoot.transform);
                    var createWire = new PlateauSandboxElectricPostWire(wire, index);
                    createWire.Show(false);
                    m_BackPostWires.Add(createWire);
                }
            }
        }

        public string RemoveWires(bool isFront, int index)
        {
            string wireID = string.Empty;
            if (isFront)
            {
                foreach (var postWire in m_FrontPostWires.ToList())
                {
                    if (postWire.Index == index)
                    {
                        postWire.Remove();
                        m_FrontPostWires.Remove(postWire);
                        wireID = postWire.WireID;
                    }
                }
            }
            else
            {
                foreach (var postWire in m_BackPostWires.ToList())
                {
                    if (postWire.Index == index)
                    {
                        postWire.Remove();
                        m_BackPostWires.Remove(postWire);
                        wireID = postWire.WireID;
                    }
                }
            }
            return wireID;
        }

        public (int index, bool isFront) RemoveWires(string wireID)
        {
            int index = -1;
            bool isFront = false;
            foreach (var postWire in m_FrontPostWires.ToList())
            {
                if (postWire.WireID == wireID)
                {
                    postWire.Remove();
                    m_FrontPostWires.Remove(postWire);
                    index = postWire.Index;
                    isFront = true;
                }
            }
            foreach (var postWire in m_BackPostWires.ToList())
            {
                if (postWire.WireID == wireID)
                {
                    postWire.Remove();
                    m_BackPostWires.Remove(postWire);
                    index = postWire.Index;
                    isFront = false;
                }
            }
            return (index, isFront);
        }

        public void TryShowWires(bool isFront, PlateauSandboxElectricConnectInfo target)
        {
            foreach (var postWire in isFront ? m_FrontPostWires : m_BackPostWires)
            {
                if (target != null && (target.m_WireID != postWire.WireID))
                {
                    continue;
                }

                if (postWire.WireID == string.Empty || target == null || target.m_Target == null)
                {
                    postWire.Show(false);
                    continue;
                }

                if (target.m_Target.IsShowingWire(postWire.WireID))
                {
                    // 既に相手側で表示されているワイヤーは表示しない
                    postWire.Show(false);
                    continue;
                }

                if (!postWire.IsTarget(target))
                {
                    continue;
                }

                var targetConnectPosition = target.m_Target.GetConnectPoint(postWire.WireType, target.m_IsTargetFront);
                postWire.SetElectricNode(targetConnectPosition);
            }
        }

        public void TryShowWiresNoTarget(bool isFront, PlateauSandboxElectricConnectInfo target)
        {
            foreach (var postWire in isFront ? m_FrontPostWires : m_BackPostWires)
            {
                if (target.m_Target == null)
                {
                    postWire.Show(false);
                    continue;
                }
                var targetConnectPosition = target.m_Target.GetConnectPoint(postWire.WireType, target.m_IsTargetFront);
                postWire.SetElectricNode(targetConnectPosition);
            }
        }

        public void HideWires(bool isFront, int index)
        {
            foreach (var postWire in isFront ? m_FrontPostWires : m_BackPostWires)
            {
                postWire.TryHide(index);
            }
        }

        public void SetWireID(bool isFront, int index, string wireID)
        {
            foreach (var postWire in isFront ? m_FrontPostWires : m_BackPostWires)
            {
                if (postWire.Index == index)
                {
                    postWire.SetWireID(wireID);
                }
            }
        }

        public void SetTarget(bool isFront, int index, PlateauSandboxElectricConnectInfo info)
        {
            foreach (var postWire in isFront ? m_FrontPostWires : m_BackPostWires)
            {
                if (postWire.Index == index)
                {
                    postWire.SetTarget(info);
                }
            }
        }

        public string RemoveWireID(bool isFront, int index)
        {
            string wireID = string.Empty;
            foreach (var postWire in isFront ? m_FrontPostWires : m_BackPostWires)
            {
                if (postWire.Index == index)
                {
                    wireID = postWire.RemoveWireID();
                }
            }
            return wireID;
        }

        public (bool isFront, int index) RemoveWireID(string wireID)
        {
            bool isFront = false;
            int index = -1;
            foreach (var postWire in m_FrontPostWires)
            {
                if (postWire.WireID == wireID)
                {
                    index = postWire.Index;
                    isFront = true;
                    postWire.RemoveWireID();
                }
            }
            foreach (var postWire in m_BackPostWires)
            {
                if (postWire.WireID == wireID)
                {
                    index = postWire.Index;
                    isFront = false;
                    postWire.RemoveWireID();
                }
            }
            return (isFront, index);
        }

        public bool IsShowingWire(string wireID)
        {
            foreach (var postWire in m_FrontPostWires)
            {
                if (postWire.WireID == wireID)
                {
                    return postWire.IsShow;
                }
            }
            foreach (var postWire in m_BackPostWires)
            {
                if (postWire.WireID == wireID)
                {
                    return postWire.IsShow;
                }
            }
            return false;
        }
    }
}