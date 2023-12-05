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
    }

}