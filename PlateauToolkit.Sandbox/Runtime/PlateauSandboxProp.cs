using UnityEngine;

namespace PlateauToolkit.Sandbox
{
    /// <summary>
    /// The definition of a prop
    /// </summary>
    [SelectionBase]
    public class PlateauSandboxProp : MonoBehaviour, IPlateauSandboxPlaceableObject
    {
        public void SetPosition(in Vector3 position)
        {
            transform.position = position;
        }
    }
}