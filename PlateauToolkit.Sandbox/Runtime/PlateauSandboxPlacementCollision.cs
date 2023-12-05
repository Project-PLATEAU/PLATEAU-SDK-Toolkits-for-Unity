#if UNITY_EDITOR
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    /// <summary>
    /// Detects collision with other objects and provide flags for it.
    /// </summary>
    /// <remarks>
    /// This class is used for Editor scripts, then not included in Runtime scripts by #if.
    /// </remarks>
    [ExecuteAlways]
    class PlateauSandboxPlacementCollision : MonoBehaviour
    {
        public bool IsCollided { get; private set; }

        /// <summary>
        /// Reset <see cref="IsCollided" />
        /// </summary>
        /// <remarks>
        /// When the object colliding with this object is destroyed, <see cref="OnCollisionExit(Collision)" />
        /// won't be called and <see cref="IsCollided" /> will stay at <c>true</c>.
        /// </remarks>
        public void ResetCollided()
        {
            IsCollided = false;
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                ResetCollided();
            }
        }

        void ValidateCollision(Collision collision)
        {
            IsCollided = PlateauSandboxObjectFinder.TryGetSandboxObject(collision.collider, out _);
        }

        void OnCollisionStay(Collision collision)
        {
            ValidateCollision(collision);
        }
        void OnCollisionEnter(Collision collision)
        {
            ValidateCollision(collision);
        }
        void OnCollisionExit(Collision collision)
        {
            ValidateCollision(collision);
        }
    }
}
#endif