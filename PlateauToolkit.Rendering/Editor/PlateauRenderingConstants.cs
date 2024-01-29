namespace PlateauToolkit.Rendering
{
    static class PlateauRenderingConstants
    {
        public static readonly string k_CombinedMeshChildren = "_combined_children";
        public static readonly string k_Grouped = "_grouped";
        public static readonly string k_PostfixLodGrouped = "_lod";
        public static readonly string k_PostfixAutoTextured = "_plateau_auto_textured";
        public static readonly string k_SkyboxNewShaderName = "Skybox/URPPhysicallyBasedSky";
        public static readonly float[] k_LodDistances = new float[] { 0.05f, 0.1f, 0.25f, 0.28f }; // up to LOD 3 for now and reversed from Unity's standard

        public static readonly int k_HighPassMinValue = 0;
        public static readonly int k_HighPassMaxValue = 100;
        public static readonly int k_HighPassDefaultValue = 0;

        public static readonly int k_ContrastMinValue = -100;
        public static readonly int k_ContrastMaxValue = 100;
        public static readonly int k_ContrastDefaultValue = 0;

        public static readonly int k_BrightnessMinValue = -100;
        public static readonly int k_BrightnessMaxValue = 100;
        public static readonly int k_BrightnessDefaultValue = 0;

        public static readonly int k_SharpnessMinValue = 0;
        public static readonly int k_SharpnessMaxValue = 100;
        public static readonly int k_SharpnessDefaultValue = 0;
    }

}