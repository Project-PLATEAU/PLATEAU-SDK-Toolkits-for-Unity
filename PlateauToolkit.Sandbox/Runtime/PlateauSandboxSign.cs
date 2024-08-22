using UnityEngine;

namespace PlateauToolkit.Sandbox
{
    /// <summary>
    /// The definition of a Sign
    /// </summary>
    [SelectionBase]
    public class PlateauSandboxSign : MonoBehaviour, IPlateauSandboxPlaceableObject
    {
        public void SetPosition(in Vector3 position)
        {
            transform.position = position;
        }
    }
}