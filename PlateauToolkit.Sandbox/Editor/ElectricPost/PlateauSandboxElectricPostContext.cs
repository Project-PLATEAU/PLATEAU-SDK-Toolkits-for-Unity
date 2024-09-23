using UnityEngine;
using UnityEngine.Events;

namespace PlateauToolkit.Sandbox.Editor
{
    public class PlateauSandboxElectricPostContext
    {
        static PlateauSandboxElectricPostContext s_Current = new();

        private PlateauSandboxElectricPost m_Target;
        public PlateauSandboxElectricPost Target => m_Target;

        public UnityEvent OnSelected { get; } = new();

        public static PlateauSandboxElectricPostContext GetCurrent()
        {
            return s_Current;
        }

        public void SetTarget(PlateauSandboxElectricPost target)
        {
            m_Target = target;
        }
    }
}