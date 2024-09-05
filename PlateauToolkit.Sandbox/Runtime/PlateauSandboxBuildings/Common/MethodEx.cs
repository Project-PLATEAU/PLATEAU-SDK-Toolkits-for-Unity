using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Common
{
    public static class ComponentEx
    {
        public static T[] GetComponentsInChildrenWithoutSelf<T>(this GameObject self) where T : Component
        {
            return self.GetComponentsInChildren<T>().Where(c => self != c.gameObject).ToArray();
        }
    }

    public static class ListEx
    {
        public static bool IsEmpty<T>(this List<T> self)
        {
            return self.Count == 0;
        }
    }
}