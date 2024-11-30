using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.ElectricPost
{
    /// <summary>
    /// 連結部分のハンドラー
    /// </summary>
    public class PlateauSandboxElectricPostConnectPoints
    {
        private const string k_ElectricConnectPointRootName = "ReceiveConnectPoints";

        private List<(PlateauSandboxElectricPostWireType wireType, GameObject target)> m_ConnectPoints = new();

        public PlateauSandboxElectricPostConnectPoints(GameObject post)
        {
            var connectPointRoot = post.transform.Find(k_ElectricConnectPointRootName);
            InitializePoints(connectPointRoot);
        }

        private void InitializePoints(Transform connectPointRoot)
        {
            foreach (Transform child in connectPointRoot)
            {
                var wireType = PlateauSandboxElectricPostWireTypeExtensions.GetWireType(child.gameObject);
                if (wireType == PlateauSandboxElectricPostWireType.k_InValid)
                {
                    continue;
                }
                m_ConnectPoints.Add((wireType, child.gameObject));
            }
        }

        public Vector3 GetConnectPoint(PlateauSandboxElectricPostWireType wireType)
        {
            foreach (var connectPoint in m_ConnectPoints)
            {
                if (connectPoint.wireType == wireType)
                {
                    return connectPoint.target.transform.position;
                }
            }
            return Vector3.zero;
        }
    }
}