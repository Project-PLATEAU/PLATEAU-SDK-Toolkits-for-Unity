using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace PlateauToolkit.Sandbox
{
    public abstract class PlateauSandboxPlaceableHandler : MonoBehaviour, IPlateauSandboxPlaceableObject
    {
        public enum GroundPlacementDirection
        {
            Vertical,
            Horizontal,
        }

        // 地面に配置するときの方向
        public GroundPlacementDirection groundPlacementDirection = GroundPlacementDirection.Vertical;

        public virtual void SetPosition(in Vector3 position)
        {
            transform.position = position;
        }

        public virtual bool IsGroundPlacementVertical()
        {
            return groundPlacementDirection == GroundPlacementDirection.Vertical;
        }

        public virtual bool CanPlaceOnOtherSandboxObject()
        {
            return false;
        }
    }
}