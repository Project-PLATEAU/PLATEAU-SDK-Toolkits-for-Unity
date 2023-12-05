using UnityEngine;
using UnityEngine.Splines;

namespace PlateauToolkit.Sandbox
{
    [ExecuteAlways]
    [RequireComponent(typeof(PlateauSandboxTrack))]
    public class PlateauSandboxTrackInstantiate : MonoBehaviour
    {
        SplineInstantiate m_SplineInstantiate;

#if UNITY_EDITOR
        internal SplineInstantiate SplineInstantiate => m_SplineInstantiate;
#endif

        void Awake()
        {
            if (!gameObject.TryGetComponent(out m_SplineInstantiate))
            {
                m_SplineInstantiate = gameObject.AddComponent<SplineInstantiate>();
                // m_SplineInstantiate.hideFlags = HideFlags.HideInInspector;
            }
        }

        void OnDestroy()
        {
            if (!gameObject.TryGetComponent(out m_SplineInstantiate))
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(m_SplineInstantiate);
            }
        }
    }
}