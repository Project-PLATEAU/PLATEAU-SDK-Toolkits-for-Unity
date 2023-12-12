using UnityEngine;

namespace PlateauToolkit.Editor
{
    [CreateAssetMenu(fileName = "Building Plateau Material Assignment Table", menuName = "PLATEAU/New Plateau Material Assignment Table", order = 2)]
    public class PlateauRenderingMaterialAssignment : ScriptableObject
    {
        public Material[] m_Max10mMaterials;
        public Material[] m_Max40mMaterials;
        public Material[] m_Max80mMaterials;
        public Material[] m_Max150mMaterials;
        public Material[] m_MaxHeightMaterials;
    }
}