namespace PlateauToolkit.Sandbox.Runtime.ElectricPost
{
    /// <summary>
    /// 電柱の情報
    /// </summary>
    public class PlateauSandboxElectricPostInfo
    {
        private PlateauSandboxElectricPost m_OwnPost;

        private PlateauSandboxElectricPost m_FrontConnectedPost;
        public PlateauSandboxElectricPost FrontConnectedPost => m_FrontConnectedPost;

        private PlateauSandboxElectricPost m_BackConnectedPost;
        public PlateauSandboxElectricPost BackConnectedPost => m_BackConnectedPost;

        public PlateauSandboxElectricPostInfo(PlateauSandboxElectricPost ownPost)
        {
            m_OwnPost = ownPost;
        }

        public void SetConnect(PlateauSandboxElectricPost target, bool isFront)
        {
            if (isFront)
            {
                m_FrontConnectedPost = target;
            }
            else
            {
                m_BackConnectedPost = target;
            }

            if (target == null)
            {
                return;
            }

            // 選択中の電柱側を設定
            bool isTargetFront = target.IsTargetFacingForward(m_OwnPost.transform.position);
            target.SetConnectToPost(m_OwnPost, isTargetFront);
        }
    }
}