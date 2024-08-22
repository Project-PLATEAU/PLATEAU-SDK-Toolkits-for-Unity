using UnityEngine;

namespace PlateauToolkit.Sandbox
{
    /// <summary>
    /// The definition of a StreetFurniture
    /// </summary>
    [SelectionBase]
    public class PlateauSandboxStreetFurniture : MonoBehaviour, IPlateauSandboxPlaceableObject
    {
        public void SetPosition(in Vector3 position)
        {
            transform.position = position;
        }
    }
}