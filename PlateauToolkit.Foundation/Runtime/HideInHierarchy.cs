#if UNITY_EDITOR
using UnityEngine;

namespace PlateauToolkit.Foundation
{
    public class HideInHierarchy : MonoBehaviour
    {
        [SerializeField] private bool m_HideChildren = false;

        public bool m_ToggleHideChildren
        {
            get { return m_HideChildren; }
            set
            {
                if (m_HideChildren != value)
                {
                    m_HideChildren = value;
                    HideChildrenObjects();
                }
            }
        }

        void HideChildrenObjects()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.hideFlags = m_HideChildren ? HideFlags.HideInHierarchy : HideFlags.None;
            }
        }
    }
}
#endif