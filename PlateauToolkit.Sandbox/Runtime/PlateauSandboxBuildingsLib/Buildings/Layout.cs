using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings
{
    public abstract class Layout : ILayout
    {
        public Vector2 origin { get; set; }
        public float width { get; set; }
        public float widthScale { get; set; }
        public float height { get; set; }
        public float heightScale { get; set; }

        private readonly List<ILayoutElement> m_Elements = new();

        public IEnumerator<ILayoutElement> GetEnumerator()
        {
            return m_Elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Add(ILayoutElement element)
        {
            m_Elements.Add(element);
        }

        public virtual int Count()
        {
            return m_Elements.Count;
        }
    }
}
