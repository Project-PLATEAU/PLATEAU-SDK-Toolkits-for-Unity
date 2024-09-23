using PlateauToolkit.Sandbox.Editor;
using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace PlateauToolkit.Sandbox
{
    [ExecuteAlways]
    public class PlateauSandboxElectricPost : MonoBehaviour
    {
        [HideInInspector]
        public GameObject electricNode01;

        [HideInInspector]
        public GameObject electricNode02;

        private GameObject m_ElectricWire01;
        private GameObject m_ElectricWire02;

        private bool m_SelectingNode01;
        private bool m_SelectingNode02;

        private void OnEnable()
        {
            m_ElectricWire01 = transform.Find("StreetFurniture_Electric_Post_Wire_01_LOD0").gameObject;
            m_ElectricWire02 = transform.Find("StreetFurniture_Electric_Post_Wire_01_1_LOD0").gameObject;
        }

        private void Update()
        {
            if (m_ElectricWire01 != null)
            {
                if (m_SelectingNode01 || electricNode01 != null)
                {
                    m_ElectricWire01.SetActive(true);
                }
                else
                {
                    m_ElectricWire01.SetActive(false);
                }
            }

            if (m_ElectricWire02 != null)
            {
                if (m_SelectingNode02 || electricNode02 != null)
                {
                    m_ElectricWire02.SetActive(true);
                }
                else
                {
                    m_ElectricWire02.SetActive(false);
                }
            }
        }

        public void SetElectricNode(GameObject node)
        {
            if (m_SelectingNode01)
            {
                electricNode01 = node;
            }
            if (m_SelectingNode02)
            {
                electricNode02 = node;
            }
        }

        public void SetElectricNodePosition(Vector3 position)
        {
            if (m_SelectingNode01)
            {
                RotateWireToMouse(m_ElectricWire01, position);
            }
            if (m_SelectingNode02)
            {
                RotateWireToMouse(m_ElectricWire02, position);
            }
        }

        public void SetSelectingElectricNode01(bool isSelect)
        {
            m_SelectingNode01 = isSelect;
        }

        public void SetSelectingElectricNode02(bool isSelect)
        {
            m_SelectingNode02 = isSelect;
        }

        private void RotateWireToMouse(GameObject wire, Vector3 mousePosition)
        {
            var forward = m_SelectingNode01 ? transform.forward : transform.forward;
            var projectedForward = Vector3.ProjectOnPlane(forward, Vector3.up);
            var projectedOtherPoint = Vector3.ProjectOnPlane(mousePosition - transform.position, Vector3.up);

            float signedAngle = Vector3.SignedAngle(projectedForward, projectedOtherPoint, Vector3.up);


            // var rotation = Quaternion.LookRotation(mousePosition);
            var eulerAngle = wire.transform.eulerAngles;
            wire.transform.rotation = Quaternion.Euler(eulerAngle.x, signedAngle, eulerAngle.z);

            // Debug.Log($"======= {signedAngle} : {mousePosition}");
        }
    }
}