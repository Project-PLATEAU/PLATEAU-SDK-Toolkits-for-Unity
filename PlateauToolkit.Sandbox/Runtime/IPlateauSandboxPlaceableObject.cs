using System;
using UnityEngine;

namespace PlateauToolkit.Sandbox
{
    public interface IPlateauSandboxPlaceableObject
    {
#pragma warning disable IDE1006 // Naming Styles
        // ReSharper disable once InconsistentNaming
        GameObject gameObject { get; }
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Set position of the object.
        /// </summary>
        void SetPosition(in Vector3 position);

        /// <summary>
        /// Whether installed vertically when placed on the ground
        /// </summary>
        bool IsGroundPlacementVertical();

        /// <summary>
        /// Whether the object can be placed on other sandbox objects.
        /// </summary>
        /// <returns></returns>
        bool CanPlaceOnOtherSandboxObject();
    }
}