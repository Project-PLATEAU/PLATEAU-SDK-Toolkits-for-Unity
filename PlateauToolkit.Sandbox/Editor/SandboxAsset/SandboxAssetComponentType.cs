using System;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    [Flags]
    enum SandboxAssetComponentType
    {
        k_Avatar = 1 << 0,
        k_Vehicle = 1 << 1,
        k_Building = 1 << 2,
        k_Plant = 1 << 3,
        k_Advertisement = 1 << 4,
        k_StreetFurniture = 1 << 5,
        k_Sign = 1 << 6,
        k_Miscellaneous = 1 << 7,
    }

    static class SandboxAssetComponentTypeExtensions
    {
        public static Texture2D LoadIcon(this SandboxAssetComponentType type)
        {
            switch (type)
            {
                case SandboxAssetComponentType.k_Avatar:
                    return (Texture2D)AssetDatabase.LoadAssetAtPath(PlateauSandboxPaths.AvatarIcon, typeof(Texture2D));
                case SandboxAssetComponentType.k_Vehicle:
                    return (Texture2D)AssetDatabase.LoadAssetAtPath(PlateauSandboxPaths.VehicleIcon, typeof(Texture2D));
                case SandboxAssetComponentType.k_Building:
                    return (Texture2D)AssetDatabase.LoadAssetAtPath(PlateauSandboxPaths.BuildingIcon, typeof(Texture2D));
                case SandboxAssetComponentType.k_Plant:
                    return (Texture2D)AssetDatabase.LoadAssetAtPath(PlateauSandboxPaths.PlantIcon, typeof(Texture2D));
                case SandboxAssetComponentType.k_Advertisement:
                    return (Texture2D)AssetDatabase.LoadAssetAtPath(PlateauSandboxPaths.AdvertisementIcon, typeof(Texture2D));
                case SandboxAssetComponentType.k_StreetFurniture:
                    return (Texture2D)AssetDatabase.LoadAssetAtPath(PlateauSandboxPaths.StreetFurnitureIcon, typeof(Texture2D));
                case SandboxAssetComponentType.k_Sign:
                    return (Texture2D)AssetDatabase.LoadAssetAtPath(PlateauSandboxPaths.SignIcon, typeof(Texture2D));
                case SandboxAssetComponentType.k_Miscellaneous:
                    return (Texture2D)AssetDatabase.LoadAssetAtPath(PlateauSandboxPaths.MiscellaneousIcon, typeof(Texture2D));
                default:
                    return null;
            }
        }

        public static IPlateauSandboxAssetListView GetAssetList(this SandboxAssetComponentType type)
        {
            switch (type)
            {
                case SandboxAssetComponentType.k_Avatar:
                    return new PlateauSandboxAssetListViewAvatar();
                case SandboxAssetComponentType.k_Vehicle:
                    return new PlateauSandboxAssetListViewVehicle();
                case SandboxAssetComponentType.k_Building:
                    return new PlateauSandboxAssetListViewBuilding();
                case SandboxAssetComponentType.k_Plant:
                    return new PlateauSandboxAssetListViewPlant();
                case SandboxAssetComponentType.k_Advertisement:
                    return new PlateauSandboxAssetListViewAdvertisement();
                case SandboxAssetComponentType.k_StreetFurniture:
                    return new PlateauSandboxAssetListViewStreetFurniture();
                case SandboxAssetComponentType.k_Sign:
                    return new PlateauSandboxAssetListViewSign();
                case SandboxAssetComponentType.k_Miscellaneous:
                    return new PlateauSandboxAssetListViewMiscellaneous();
                default:
                    return null;
            }
        }
    }
}