using UnityEngine;

namespace UnityEditor.Rendering.HighDefinition.ShaderGUI.Buildings
{
    /// <summary>
    /// GUI for HDRP Lit materials (and tesselation), does not include shader graph + function to setup material keywords for Lit
    /// </summary>
    class LitGUI : HDShaderGUI
    {
        // For lit GUI we don't display the heightmap nor layering options
        private const LitSurfaceInputsUIBlock.Features LitSurfaceFeatures = LitSurfaceInputsUIBlock.Features.All ^ LitSurfaceInputsUIBlock.Features.HeightMap ^ LitSurfaceInputsUIBlock.Features.LayerOptions;
        
        private readonly MaterialUIBlockList uiBlocks = new()
        {
            // new SurfaceOptionUIBlock(MaterialUIBlock.ExpandableBit.Base, features: SurfaceOptionUIBlock.Features.Lit),
            // new TessellationOptionsUIBlock(MaterialUIBlock.ExpandableBit.Tessellation),
            new LitSurfaceInputsUIBlock(MaterialUIBlock.ExpandableBit.Input, features: LitSurfaceFeatures),
            new LitSurfaceInputsUIBlockWithTextMap(),
            // new DetailInputsUIBlock(MaterialUIBlock.ExpandableBit.Detail),
            // We don't want distortion in Lit
            new TransparencyUIBlock(MaterialUIBlock.ExpandableBit.Transparency, features: TransparencyUIBlock.Features.All & ~TransparencyUIBlock.Features.Distortion),
            new EmissionUIBlock(MaterialUIBlock.ExpandableBit.Emissive),
            // new AdvancedOptionsUIBlock(MaterialUIBlock.ExpandableBit.Advance, AdvancedOptionsUIBlock.Features.StandardLit),
        };

        protected override void OnMaterialGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            uiBlocks.OnGUI(materialEditor, props);
        }

        // public override void ValidateMaterial(Material material) => LitAPI.ValidateMaterial(material);
    }

    public class LitSurfaceInputsUIBlockWithTextMap : MaterialUIBlock
    {
        private static readonly GUIContent s_header = EditorGUIUtility.TrTextContent("Surface TextMap Inputs");
        private static readonly GUIContent s_textMap = EditorGUIUtility.TrTextContent("Text Map", "Specifies the text Material and/or Color of the surface. If you’ve selected Transparent or Alpha Clipping under Surface Options, your Material uses the Texture’s alpha channel or color.");
        private static readonly GUIContent s_textOffsetXText = EditorGUIUtility.TrTextContent("Text OffsetX", "Controls the x position of TextMap.");
        private static readonly GUIContent s_textOffsetYText = EditorGUIUtility.TrTextContent("Text OffsetY", "Controls the y position of TextMap.");
        private static readonly GUIContent s_textureAspect = EditorGUIUtility.TrTextContent("Texture Aspect", "Aspect ratio of texture size to mesh size.");

        private MaterialProperty TextMapProp { get; set; }
        private MaterialProperty TextColorProp { get; set; }
        private MaterialProperty TextOffsetXProp { get; set; }
        private MaterialProperty TextOffsetYProp { get; set; }
        private MaterialProperty TextureAspectProp { get; set; }

        private static void DoTextMap(MaterialEditor materialEditor, MaterialProperty inTextMapProp, MaterialProperty inTextColorProp, MaterialProperty inTextOffsetXProp, MaterialProperty inTextOffsetYProp, MaterialProperty inTextureAspectProp)
        {
            if (inTextMapProp != null && inTextColorProp != null && inTextOffsetXProp != null && inTextOffsetYProp != null && inTextureAspectProp != null)
            {
                materialEditor.TexturePropertySingleLine(s_textMap, inTextMapProp, inTextColorProp);
                EditorGUI.indentLevel += 2;
                materialEditor.ShaderProperty(inTextOffsetXProp, s_textOffsetXText);
                materialEditor.ShaderProperty(inTextOffsetYProp, s_textOffsetYText);
                materialEditor.ShaderProperty(inTextureAspectProp, s_textureAspect);
                EditorGUI.indentLevel -= 2;
            }
        }

        public override void LoadMaterialProperties()
        {
            TextMapProp = FindProperty("_TextMap");
            TextColorProp = FindProperty("_TextColor");
            TextOffsetXProp = FindProperty("_TextOffsetX");
            TextOffsetYProp = FindProperty("_TextOffsetY");
            TextureAspectProp = FindProperty("_TextureAspect");
        }

        public override void OnGUI()
        {
            using var scope = new MaterialHeaderScope(s_header, (uint)ExpandableBit.User0, materialEditor, subHeader: isSubHeader);
            if (scope.expanded)
            {
                OnGUIOpen();
            }
        }

        protected override void OnGUIOpen()
        {
            DoTextMap(materialEditor, TextMapProp, TextColorProp, TextOffsetXProp, TextOffsetYProp, TextureAspectProp);
        }
    }
}
