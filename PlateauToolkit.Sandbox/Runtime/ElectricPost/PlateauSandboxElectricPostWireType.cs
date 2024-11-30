using System;

namespace PlateauToolkit.Sandbox.Runtime.ElectricPost
{
    /// <summary>
    /// ワイヤーの種類
    /// </summary>
    public enum PlateauSandboxElectricPostWireType
    {
        k_InValid = -1,
        k_TopA, // 上部のA
        k_TopB, // 上部のB
        k_TopC, // 上部のC
        k_BottomA, // 下部のA
        k_BottomB, // 下部のB
    }

    public static class PlateauSandboxElectricPostWireTypeExtensions
    {
        public static string GetObjectName(this PlateauSandboxElectricPostWireType wireType)
        {
            switch (wireType)
            {
                case PlateauSandboxElectricPostWireType.k_TopA:
                    return "01_a";
                case PlateauSandboxElectricPostWireType.k_TopB:
                    return "01_b";
                case PlateauSandboxElectricPostWireType.k_TopC:
                    return "01_c";
                case PlateauSandboxElectricPostWireType.k_BottomA:
                    return "02_a";
                case PlateauSandboxElectricPostWireType.k_BottomB:
                    return "02_b";
                default:
                    return string.Empty;
            }
        }
    }
}