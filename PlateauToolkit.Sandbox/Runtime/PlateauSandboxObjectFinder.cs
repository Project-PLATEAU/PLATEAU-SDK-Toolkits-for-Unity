using UnityEngine;

namespace PlateauToolkit.Sandbox
{
    static class PlateauSandboxObjectFinder
    {
        public static bool TryGetSandboxObject(Collider collider, out IPlateauSandboxPlaceableObject sandboxPlaceableObject)
        {
            // Check the reference cache at runtime.
            if (Application.isPlaying)
            {
                if (collider.gameObject.TryGetComponent(out PlateauSandboxColliderReference reference))
                {
                    if (reference.SandboxObject == null)
                    {
                        throw new System.NullReferenceException("The reference is broken");
                    }
                    sandboxPlaceableObject = reference.SandboxObject;
                    return true;
                }
            }

            Transform parent = collider.gameObject.transform;
            while (parent != null)
            {
                if (TryGetAndCacheReference(collider, parent.gameObject, out sandboxPlaceableObject))
                {
                    return true;
                }

                parent = parent.parent;
            }

            sandboxPlaceableObject = null;
            return false;
        }

        static bool TryGetAndCacheReference(Collider collider, GameObject gameObject, out IPlateauSandboxPlaceableObject sandboxObject)
        {
            if (gameObject.TryGetComponent(out sandboxObject))
            {
                if (Application.isPlaying)
                {
                    collider.gameObject.AddComponent<PlateauSandboxColliderReference>().SandboxObject = sandboxObject;
                }

                return true;
            }

            sandboxObject = null;
            return false;
        }

        /// <summary>
        /// Reference to a Sandbox object for colliders.
        /// </summary>
        class PlateauSandboxColliderReference : MonoBehaviour
        {
            public IPlateauSandboxPlaceableObject SandboxObject { get; set; }
        }
    }
}