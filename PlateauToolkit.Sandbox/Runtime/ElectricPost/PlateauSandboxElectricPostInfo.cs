using System;
using System.Collections.Generic;
using System.Linq;

namespace PlateauToolkit.Sandbox.Runtime.ElectricPost
{
    public class PlateauSandboxElectricConnectInfo
    {
        public PlateauSandboxElectricPost m_Target;
        public bool m_IsTargetFront;
        public int m_OwnIndex;
        public int m_TargetIndex;
    }

    /// <summary>
    /// 電柱の情報
    /// </summary>
    public class PlateauSandboxElectricPostInfo
    {
        private List<PlateauSandboxElectricConnectInfo> m_FrontConnectedPosts = new();
        public List<PlateauSandboxElectricConnectInfo> FrontConnectedPosts => m_FrontConnectedPosts;

        private List<PlateauSandboxElectricConnectInfo> m_BackConnectedPosts = new();
        public List<PlateauSandboxElectricConnectInfo> BackConnectedPosts => m_BackConnectedPosts;

        public int AddConnectionSpace(bool isFront)
        {
            if (isFront)
            {
                int index = m_FrontConnectedPosts.Count;
                var info = new PlateauSandboxElectricConnectInfo()
                {
                    m_OwnIndex = index
                };
                m_FrontConnectedPosts.Add(info);
                return index;
            }
            else
            {
                int index = m_BackConnectedPosts.Count;
                var info = new PlateauSandboxElectricConnectInfo()
                {
                    m_OwnIndex = index
                };
                m_BackConnectedPosts.Add(info);
                return index;
            }
        }

        public void ResetConnection(bool isFront, int index)
        {
            if (isFront)
            {
                foreach (var plateauSandboxElectricConnectInfo in m_FrontConnectedPosts)
                {
                    if (plateauSandboxElectricConnectInfo.m_OwnIndex == index)
                    {
                        plateauSandboxElectricConnectInfo.m_IsTargetFront = false;
                        plateauSandboxElectricConnectInfo.m_Target = null;
                        plateauSandboxElectricConnectInfo.m_TargetIndex = -1;
                        return;
                    }
                }
            }
            else
            {
                foreach (var plateauSandboxElectricConnectInfo in m_BackConnectedPosts)
                {
                    if (plateauSandboxElectricConnectInfo.m_OwnIndex == index)
                    {
                        plateauSandboxElectricConnectInfo.m_IsTargetFront = false;
                        plateauSandboxElectricConnectInfo.m_Target = null;
                        plateauSandboxElectricConnectInfo.m_TargetIndex = -1;
                        return;
                    }
                }
            }
        }

        public void RemoveConnection(bool isFront, int index)
        {
            if (isFront)
            {
                m_FrontConnectedPosts.RemoveAll(x => x.m_OwnIndex == index);
            }
            else
            {
                m_BackConnectedPosts.RemoveAll(x => x.m_OwnIndex == index);
            }
        }

        public void SetFrontConnect(PlateauSandboxElectricPost other, bool isOtherFront, int index, int otherIndex)
        {
            foreach (var plateauSandboxElectricConnectInfo in m_FrontConnectedPosts)
            {
                if (plateauSandboxElectricConnectInfo.m_OwnIndex == index)
                {
                    plateauSandboxElectricConnectInfo.m_IsTargetFront = isOtherFront;
                    plateauSandboxElectricConnectInfo.m_Target = other;
                    plateauSandboxElectricConnectInfo.m_TargetIndex = otherIndex;
                    return;
                }
            }
        }

        public void SetBackConnect(PlateauSandboxElectricPost other, bool isOtherFront, int index, int otherIndex)
        {
            foreach (var plateauSandboxElectricConnectInfo in m_BackConnectedPosts)
            {
                if (plateauSandboxElectricConnectInfo.m_OwnIndex == index)
                {
                    plateauSandboxElectricConnectInfo.m_IsTargetFront = isOtherFront;
                    plateauSandboxElectricConnectInfo.m_Target = other;
                    plateauSandboxElectricConnectInfo.m_TargetIndex = otherIndex;
                    return;
                }
            }
        }

        public PlateauSandboxElectricConnectInfo GetConnectedPost(bool isFront, int index)
        {
            if (isFront)
            {
                foreach (var plateauSandboxElectricConnectInfo in m_FrontConnectedPosts)
                {
                    if (plateauSandboxElectricConnectInfo.m_OwnIndex == index)
                    {
                        return plateauSandboxElectricConnectInfo;
                    }
                }
            }
            else
            {
                foreach (var plateauSandboxElectricConnectInfo in m_BackConnectedPosts)
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