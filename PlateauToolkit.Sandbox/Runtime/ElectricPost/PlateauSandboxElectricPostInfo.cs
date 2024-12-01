namespace PlateauToolkit.Sandbox.Runtime.ElectricPost
{
    /// <summary>
    /// 電柱の情報
    /// </summary>
    public class PlateauSandboxElectricPostInfo
    {
        private PlateauSandboxElectricPost m_OwnPost;

        private (PlateauSandboxElectricPost target, bool isFront) m_FrontConnectedPost = (null, false);
        public (PlateauSandboxElectricPost target, bool isFront) FrontConnectedPost => m_FrontConnectedPost;

        private (PlateauSandboxElectricPost target, bool isFront) m_BackConnectedPost = (null, false);
        public (PlateauSandboxElectricPost target, bool isFront) BackConnectedPost => m_BackConnectedPost;

        private PlateauSandboxElectricPostContext m_Context;

        public PlateauSandboxElectricPostInfo(PlateauSandboxElectricPost ownPost)
        {
            m_Context = PlateauSandboxElectricPostContext.GetCurrent();
            m_OwnPost = ownPost;
        }

        public bool CanConnect(bool isFront, PlateauSandboxElectricPost target)
        {
            if (isFront)
            {
                return m_FrontConnectedPost.target != target;
            }
            else
            {
                return m_BackConnectedPost.target != target;
            }
        }

        public void SetFrontConnect(PlateauSandboxElectricPost other, bool isOtherFront)
        {
            if (other == m_FrontConnectedPost.target)
            {
                return;
            }
            m_FrontConnectedPost = (other, isOtherFront);
        }

        public void SetBackConnect(PlateauSandboxElectricPost other, bool isOtherFront)
        {
            if (other == m_BackConnectedPost.target)
            {
                return;
            }
            m_BackConnectedPost = (other, isOtherFront);
        }

        public bool CanShowFrontWire()
        {
            if (FrontConnectedPost.target == null)
            {
                return false;
            }

            // 相手が表示されてなければ表示
            if (FrontConnectedPost.isFront && !FrontConnectedPost.target.IsShowingFrontWire)
            {
                return true;
            }
            else if (!FrontConnectedPost.isFront && !FrontConnectedPost.target.IsShowingBackWire)
            {
                return true;
            }
            return false;
        }

        public bool CanShowBackWire()
        {
            if (BackConnectedPost.target == null)
            {
                return false;
            }

            // 相手が表示されてなければ表示
            if (BackConnectedPost.isFront && !BackConnectedPost.target.IsShowingFrontWire)
            {
                return true;
            }
            else if (!BackConnectedPost.isFront && !BackConnectedPost.target.IsShowingBackWire)
            {
                return true;
            }
            return false;
        }
    }
}