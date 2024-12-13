using System;
using System.Collections.Generic;
using System.Linq;

namespace PlateauToolkit.Sandbox.Runtime.ElectricPost
{
    [Serializable]
    public class PlateauSandboxElectricConnectInfo
    {
        public PlateauSandboxElectricPost m_Target;
        public bool m_IsTargetFront;
        public int m_OwnIndex;
        public int m_TargetIndex;
        public string m_WireID;
    }

    /// <summary>
    /// 電柱の情報
    /// </summary>
    public class PlateauSandboxElectricPostInfo
    {
        private PlateauSandboxElectricPost m_Owner;

        public PlateauSandboxElectricPostInfo(PlateauSandboxElectricPost owner)
        {
            m_Owner = owner;
        }

        public int AddConnectionSpace(bool isFront)
        {
            if (isFront)
            {
                int index = m_Owner.FrontConnectedPosts.Count;
                var info = new PlateauSandboxElectricConnectInfo()
                {
                    m_OwnIndex = index
                };
                m_Owner.FrontConnectedPosts.Add(info);
                return index;
            }
            else
            {
                int index = m_Owner.BackConnectedPosts.Count;
                var info = new PlateauSandboxElectricConnectInfo()
                {
                    m_OwnIndex = index
                };
                m_Owner.BackConnectedPosts.Add(info);
                return index;
            }
        }

        public void ResetConnection(bool isFront, int index)
        {
            if (isFront)
            {
                foreach (var plateauSandboxElectricConnectInfo in m_Owner.FrontConnectedPosts)
                {
                    if (plateauSandboxElectricConnectInfo.m_OwnIndex == index)
                    {
                        plateauSandboxElectricConnectInfo.m_IsTargetFront = false;
                        plateauSandboxElectricConnectInfo.m_Target = null;
                        plateauSandboxElectricConnectInfo.m_TargetIndex = -1;
                        plateauSandboxElectricConnectInfo.m_WireID = string.Empty;
                        return;
                    }
                }
            }
            else
            {
                foreach (var plateauSandboxElectricConnectInfo in m_Owner.BackConnectedPosts)
                {
                    if (plateauSandboxElectricConnectInfo.m_OwnIndex == index)
                    {
                        plateauSandboxElectricConnectInfo.m_IsTargetFront = false;
                        plateauSandboxElectricConnectInfo.m_Target = null;
                        plateauSandboxElectricConnectInfo.m_TargetIndex = -1;
                        plateauSandboxElectricConnectInfo.m_WireID = string.Empty;
                        return;
                    }
                }
            }
        }

        public void RemoveConnection(bool isFront, int index)
        {
            if (isFront)
            {
                m_Owner.FrontConnectedPosts.RemoveAll(x => x.m_OwnIndex == index);
            }
            else
            {
                m_Owner.BackConnectedPosts.RemoveAll(x => x.m_OwnIndex == index);
            }
        }

        public void SetFrontConnect(PlateauSandboxElectricPost other, bool isOtherFront, int index, int otherIndex, string wireID)
        {
            foreach (var plateauSandboxElectricConnectInfo in m_Owner.FrontConnectedPosts)
            {
                if (plateauSandboxElectricConnectInfo.m_OwnIndex == index)
                {
                    plateauSandboxElectricConnectInfo.m_IsTargetFront = isOtherFront;
                    plateauSandboxElectricConnectInfo.m_Target = other;
                    plateauSandboxElectricConnectInfo.m_TargetIndex = otherIndex;
                    plateauSandboxElectricConnectInfo.m_WireID = wireID;
                    return;
                }
            }
        }

        public void SetBackConnect(PlateauSandboxElectricPost other, bool isOtherFront, int index, int otherIndex, string wireID)
        {
            foreach (var plateauSandboxElectricConnectInfo in m_Owner.BackConnectedPosts)
            {
                if (plateauSandboxElectricConnectInfo.m_OwnIndex == index)
                {
                    plateauSandboxElectricConnectInfo.m_IsTargetFront = isOtherFront;
                    plateauSandboxElectricConnectInfo.m_Target = other;
                    plateauSandboxElectricConnectInfo.m_TargetIndex = otherIndex;
                    plateauSandboxElectricConnectInfo.m_WireID = wireID;
                    return;
                }
            }
        }

        public PlateauSandboxElectricConnectInfo GetConnectedPost(bool isFront, int index)
        {
            if (isFront)
            {
                foreach (var plateauSandboxElectricConnectInfo in m_Owner.FrontConnectedPosts)
                {
                    if (plateauSandboxElectricConnectInfo.m_OwnIndex == index)
                    {
                        return plateauSandboxElectricConnectInfo;
                    }
                }
            }
            else
            {
                foreach (var plateauSandboxElectricConnectInfo in m_Owner.BackConnectedPosts)
                {
                    if (plateauSandboxElectricConnectInfo.m_OwnIndex == index)
                    {
                        return plateauSandboxElectricConnectInfo;
                    }
                }
            }
            return new PlateauSandboxElectricConnectInfo();
        }

        public string GetNextWireID()
        {
            return Guid.NewGuid().ToString();
        }
    }
}