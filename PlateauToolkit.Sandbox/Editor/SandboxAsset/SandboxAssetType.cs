using System;

namespace PlateauToolkit.Sandbox.Editor
{
    [Flags]
    enum SandboxAssetType
    {
        /// <summary>Assets defined by users</summary>
        UserDefined = 1 << 0,

        /// <summary>Sample assets included in the package</summary>
        Builtin = 1 << 1,

        /// <summary>All assets</summary>
        All = UserDefined | Builtin,
    }

    static class SandboxAssetTypeExtensions
    {
        public static string ToDisplayName(this SandboxAssetType type)
        {
            switch (type)
            {
                case SandboxAssetType.UserDefined:
                    return "ユーザー";
                case SandboxAssetType.Builtin:
                    return "ビルトイン";
                case SandboxAssetType.All:
                    return "全て";
                default:
                    return null;
            }
        }
    }
}