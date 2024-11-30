using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.ElectricPost
{
    /// <summary>
    /// 電柱のワイヤー
    /// </summary>
    public class PlateauSandboxElectricPostWire
    {
        private PlateauSandboxElectricPostWireType m_WireType = PlateauSandboxElectricPostWireType.k_InValid;
        public PlateauSandboxElectricPostWireType WireType => m_WireType;

        private GameObject m_ElectricWire;
        public Vector3 WirePosition => m_ElectricWire.transform.position;

        private Quaternion m_DefaultLocalRotate;
        private float m_WireScaleSize;

        public PlateauSandboxElectricPostWire(GameObject wire)
        {
            m_ElectricWire = wire;
            m_WireType = PlateauSandboxElectricPostWireTypeExtensions.GetWireType(wire);

            if (m_WireType == PlateauSandboxElectricPostWireType.k_InValid)
            {
                return;
            }

            m_DefaultLocalRotate = m_ElectricWire.transform.localRotation;

            if (wire.TryGetComponent<MeshRenderer>(out var meshRenderer))
            {
                m_WireScaleSize = meshRenderer.bounds.size.magnitude;
            }
        }

        public void Show(bool isShow)
        {
            m_ElectricWire.SetActive(isShow);
        }

        public void SetElectricNode(Vector3 position)
        {
            Show(true);
            RotateWire(position);
            ScaleWire(position);
        }

        private void RotateWire(Vector3 position)
        {
            // Wireの向きをポイントに向ける
            m_ElectricWire.transform.LookAt(position);

            // 回転を加算
            m_ElectricWire.transform.localRotation = m_ElectricWire.transform.localRotation * m_DefaultLocalRotate;
        }

        private void ScaleWire(Vector3 position)
        {
            // 2点間の距離を求める
            float distance = Vector3.Distance(m_ElectricWire.transform.position, position);
            if (distance <= 0 || m_WireScaleSize <= 0)
            {
                return;
            }

            // Wireの長さを変更
            m_ElectricWire.transform.localScale =
                new Vector3(m_ElectricWire.transform.localScale.x, distance / m_WireScaleSize, m_ElectricWire.transform.localScale.z);
        }

        public void Hide()
        {
            m_ElectricWire.transform.localScale = new Vector3(1, 0, 1);
            Show(false);
        }
    }
}