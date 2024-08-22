using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime
{
    /// <summary>
    /// The definition of a building
    /// </summary>
    [SelectionBase]
    public class PlateauSandboxBuilding : MonoBehaviour, IPlateauSandboxPlaceableObject
    {
        public void SetPosition(in Vector3 position)
        {
            transform.position = position;
        }
    }
}