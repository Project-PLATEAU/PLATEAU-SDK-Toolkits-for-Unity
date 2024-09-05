using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime
{
    // [CreateAssetMenu(menuName = "ProceduralToolkit/Buildings/Foundation Polygon", order = 0)]
    public class PolygonAsset : ScriptableObject
    {
        public List<Vector2> vertices = new List<Vector2>();
    }
}
