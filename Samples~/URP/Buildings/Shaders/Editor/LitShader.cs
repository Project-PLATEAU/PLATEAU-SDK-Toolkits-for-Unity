using System;
using UnityEngine;

namespace UnityEditor.Rendering.Universal.ShaderGUI.Buildings
{
    public class LitShader : BaseShaderGUI
    {
        private static readonly string[] s_workflowModeNames = Enum.GetNames(typeof(LitGUI.WorkflowMode));
        protected override uint materialFilter => (uint)Expandable.SurfaceOptions | (uint)Expandable.SurfaceInputs | (uint)Expandable.Details;
        private static readonly GUIContent s_surfaceTextMapLabel = EditorGUIUtility.TrTextContent("Surface TextMap Inputs");
        private static readonly GUIContent s_textMap = EditorGUIUtility.TrTextContent("Text Map", "Specifies the text Material and/or Color of the surface. If you’ve selected Transparent or Alpha Clipping under Surface Options, your Material uses the Texture’s alpha channel or color.");
        private static readonly GUIContent s_textOffsetXText = EditorGUIUtility.TrTextContent("Text OffsetX", "Controls the x position of TextMap.");
        private static readonly GUIContent s_textOffsetYText = EditorGUIUtility.TrTextContent("Text OffsetY", "Controls the y position of TextMap.");
        private static readonly GUIContent s_textureAspect = EditorGUIUtility.TrTextContent("Texture Aspect", "Aspect ratio of texture size to mesh size.");        
        private LitGUI.LitProperties litProperties;

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
        
        // collect properties from the material properties
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            litProperties = new LitGUI.LitProperties(properties);
            TextMapProp = FindProperty("_TextMap", properties, false);
            TextColorProp = FindProperty("_TextColor", properties, false);
            TextOffsetXProp = FindProperty("_TextOffsetX", properties, false);
            TextOffsetYProp = FindProperty("_TextOffsetY", properties, false);
            TextureAspectProp = FindProperty("_TextureAspect", properties, false);
        }

        // material main surface options
        public override void DrawSurfaceOptions(Material material)
        {
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            if (litProperties.workflowMode != null)
                DoPopup(LitGUI.Styles.workflowModeText, litProperties.workflowMode, s_workflowModeNames);
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            LitGUI.Inputs(litProperties, materialEditor, material);
            DrawEmissionProperties(material, true);
            DrawTileOffset(materialEditor, baseMapProp);
        }

        public override void FillAdditionalFoldouts(MaterialHeaderScopeList materialScopesList)
        {
            materialScopesList.RegisterHeaderScope(s_surfaceTextMapLabel, (uint)Expandable.Details, DrawAdvancedOptions);
        }

        public override void DrawAdvancedOptions(Material material)
        {
            DoTextMap(materialEditor, TextMapProp, TextColorProp, TextOffsetXProp, TextOffsetYProp, TextureAspectProp);
        }
        
        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialBlendMode(material);
                return;
            }

            SurfaceType surfaceType = SurfaceType.Opaque;
            material.SetFloat("_Surface", (float)surfaceType);
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");

            if (oldShader.name.Equals("Standard (Specular setup)"))
            {
                material.SetFloat("_WorkflowMode", (float)LitGUI.WorkflowMode.Specular);
                Texture texture = material.GetTexture("_SpecGlossMap");
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }
            else
            {
                material.SetFloat("_WorkflowMode", (float)LitGUI.WorkflowMode.Metallic);
                Texture texture = material.GetTexture("_MetallicGlossMap");
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }
        }
    }
}
