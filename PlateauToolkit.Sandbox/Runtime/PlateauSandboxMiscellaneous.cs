using UnityEngine;

namespace PlateauToolkit.Sandbox
{
    /// <summary>
    /// The definition of a Miscellaneous
    /// </summary>
    [SelectionBase]
    public class PlateauSandboxMiscellaneous : MonoBehaviour, IPlateauSandboxPlaceableObject
    {
        public void SetPosition(in Vector3 position)
        {
            transform.position = position;
        }
    }
}