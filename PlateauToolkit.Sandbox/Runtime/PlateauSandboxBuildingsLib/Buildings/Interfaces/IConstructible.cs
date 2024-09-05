using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces
{
    public interface IConstructible<out T>
    {
        T Construct(Vector2 parentLayoutOrigin);
    }
}
