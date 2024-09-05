using ProceduralToolkit;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime.Parts
{
    public class BaseParts : MonoBehaviour
    {
        public float Width = 1;
        public float Height = 1;

        public const float SocleWindowWidth = 0.7f;
        public const float SocleWindowMinWidth = 0.9f;
        public const float SocleWindowHeight = 0.4f;
        public const float SocleWindowDepth = 0.1f;
        public const float SocleWindowHeightOffset = 0.1f;

        public const float EntranceDoorWidth = 1.8f;
        public const float EntranceDoorHeight = 2;
        public const float EntranceDoorThickness = 0.05f;

        public const float EntranceRoofDepth = 1;
        public const float EntranceRoofHeight = 0.15f;

        public const float EntranceWindowWidthOffset = 0.4f;
        public const float EntranceWindowHeightOffset = 0.3f;

        public const float WindowDepth = 0.1f;
        public const float WindowWidthOffset = 0.5f;
        public const float WindowBottomOffset = 1;
        public const float WindowTopOffset = 0.3f;
        public const float WindowFrameWidth = 0.05f;
        public const float WindowSegmentMinWidth = 0.9f;
        public const float WindowsillWidthOffset = 0.1f;
        public const float WindowsillDepth = 0.15f;
        public const float WindowsillThickness = 0.05f;

        public const float BalconyHeight = 1;
        public const float BalconyDepth = 0.8f;
        public const float BalconyThickness = 0.1f;

        public const float AtticHoleWidth = 0.3f;
        public const float AtticHoleMinWidth = 0.5f;
        public const float AtticHoleHeight = 0.3f;
        public const float AtticHoleDepth = 0.5f;

        public Color SocleColor = ColorE.silver;
        public Color SocleWindowColor = (ColorE.silver/2).WithA(1);
        public Color DoorColor = (ColorE.silver/2).WithA(1);
        public Color WallColor = ColorE.white;
        public Color FrameColor = ColorE.silver;
        public Color GlassColor = ColorE.white;
        public Color RoofColor = (ColorE.gray/4).WithA(1);

        public const string WallDraftName = "Wall";
        public const string GlassDraftName = "Glass";

        public enum WindowType
        {
            windowPane,
            windowpaneGlass,
            windowpaneFrame,
            windowpaneOuterFrame,
            windowpaneFrameRods,
            sill,
            frame,
            wall,
            windowAll,
            entranceWindow,
        }

        public enum EntranceType
        {
            entranceBracket,
            doorFrame,
            door,
            entranceAll,
            roof,
            entranceRoofedAll,
        }

        public enum BalconyType
        {
            balconyOuter,
            balconyInner,
            balconyBorder,
            balconyWall,
            balconyWallInnerFrame,
            balconyWindow,
            balconySmallWindow,
            balconyAll,
            balconyGrazedOuter,
            balconyGlazedRoof,
            balconyGlazedWindow1,
            balconyGlazedWindow2,
            balconyGlazedWindow3,
            balconyGrazedAll
        }

        public enum SocleType
        {
            socle,
            socleWindowFrame,
            socleWindowWall,
            socleWindowGlass,
            socleWindowAll,
        }
    }
}
