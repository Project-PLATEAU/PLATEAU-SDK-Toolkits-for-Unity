using System.Collections.Generic;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces
{
    public interface ILayout : ILayoutElement, IEnumerable<ILayoutElement>
    {
        void Add(ILayoutElement element);
    }
}
