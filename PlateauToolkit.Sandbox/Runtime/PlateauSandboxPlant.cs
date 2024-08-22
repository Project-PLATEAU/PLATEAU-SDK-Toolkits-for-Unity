using UnityEngine;

namespace PlateauToolkit.Sandbox
{
    /// <summary>
    /// The definition of a plant
    /// </summary>
    [SelectionBase]
    public class PlateauSandboxPlant : MonoBehaviour, IPlateauSandboxPlaceableObject
    {
        public void SetPosition(in Vector3 position)
        {
            transform.position = position;
        }
    }
}