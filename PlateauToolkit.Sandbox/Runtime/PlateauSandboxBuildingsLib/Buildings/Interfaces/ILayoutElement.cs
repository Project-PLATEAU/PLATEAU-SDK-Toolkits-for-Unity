using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces
{
    public interface ILayoutElement
    {
        Vector2 origin { get; set; }
        float width { get; set; }
        float widthScale { get; set; }
        float height { get; set; }
        float heightScale { get; set; }
    }
}
