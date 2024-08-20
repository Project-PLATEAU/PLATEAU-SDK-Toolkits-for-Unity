using PlateauToolkit.Sandbox.Runtime;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace PlateauToolkit.Sandbox.Editor.PlateauToolkit.Sandbox.Editor
{
    [CustomEditor(typeof(PlateauSandboxAdvertisement))]
    public class PlateauSandboxAdvertisementInspector : UnityEditor.Editor
    {
        private readonly GUILayoutOption[] m_TextureAspectValueOptions =
        {
            GUILayout.MaxWidth(50.0f)
        };
        private readonly GUILayoutOption[] m_TextureAspectValueLabelOptions =
        {
            GUILayout.MaxWidth(14.0f)
        };
        private PlateauSandboxAdvertisement m_Target;
        private SerializedProperty m_AdvertisementMaterials;

        private void OnEnable()
        {
            m_AdvertisementMaterials = serializedObject.FindProperty("advertisementMaterials");

            m_Target = target as PlateauSandboxAdvertisement;
            if (m_Target != null)
            {
                // VideoPlayerコンポーネントが存在しない場合は追加
                m_Target.AddVideoPlayer();

                if (IsValidAdMaterials())
                {
                    m_Target.SetTexture();
                }
            }
        }

        private bool IsValidAdMaterials()
        {
            // マテリアルが設定されていない
            // 指定したマテリアル番号が存在しない
            // マテリアルが存在しない
            // 上記の内どれか１つでも当てはまれば有効ではない
            return m_Target.advertisementMaterials.Count > 0 &&
                   m_Target.advertisementMaterials.Count > m_Target.targetMaterialNumber &&
                   m_Target.advertisementMaterials[m_Target.targetMaterialNumber] != null;
        }

        private string GetMaterialDirPath()
        {
            (PlateauSandboxAdvertisement, string) foundAsset = PlateauSandboxAssetUtility.FindAsset<PlateauSandboxAdvertisement>();
            if (foundAsset.Item1 == null)
            {
                return "Assets/Samples/PLATEAU SDK-Toolkits for Unity/0.0.0/Sample Assets/Advertisement/Materials";
            }

            string materialDirPath = foundAsset.Item2.Split("Props")[0];
            materialDirPath += "Advertisement/Materials";
            if (!Directory.Exists(materialDirPath))
            {
                Directory.CreateDirectory(materialDirPath);
            }

            return materialDirPath;
        }

        private string GetDuplicatedMaterialPath(string inMatDirPath)
        {
            Material mat = m_Target.advertisementMaterials[0].materials[m_Target.targetMaterialNumber];
            string matFilePath = AssetDatabase.GetAssetPath(mat);
            return inMatDirPath + "/" + Path.GetFileName(matFilePath);
        }

        private string GetRenderTexturePath(string inMatDirPath)
        {
            Material mat = m_Target.advertisementMaterials[0].materials[m_Target.targetMaterialNumber];
            string matFilePath = AssetDatabase.GetAssetPath(mat);
            string matFileName = Path.GetFileNameWithoutExtension(matFilePath);
            return inMatDirPath + "/" + matFileName + ".renderTexture";
        }

        private bool DuplicateMaterial(string inDuplicatedMatPath)
        {
            // 対象テクスチャプロパティが存在するか？
            Material mat = m_Target.advertisementMaterials[0].materials[m_Target.targetMaterialNumber];
            if (!mat.HasProperty(m_Target.targetTextureProperty))
            {
                return false;
            }

            string matFilePath = AssetDatabase.GetAssetPath(mat);
            if (!File.Exists(inDuplicatedMatPath))
            {
                AssetDatabase.CopyAsset(matFilePath, inDuplicatedMatPath);
                AssetDatabase.SaveAssets();
            }

            return true;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            bool changedValue = false;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(m_AdvertisementMaterials, true);
            EditorGUI.EndDisabledGroup();

            if (!IsValidAdMaterials())
            {
                EditorGUILayout.HelpBox($"コンポーネントをリセットして下さい。", MessageType.Info);
                return;
            }

            GUILayout.Space(5);
            EditorGUI.BeginChangeCheck();

            // 広告タイプ切り替え
            var advertisementType = (PlateauSandboxAdvertisement.AdvertisementType)EditorGUILayout.EnumPopup(
                "Advertisement Type",
                m_Target.advertisementType);
            if (EditorGUI.EndChangeCheck())
            {
                m_Target.advertisementType = advertisementType;

                switch (advertisementType)
                {
                    case PlateauSandboxAdvertisement.AdvertisementType.Image:
                    {
                        // マテリアルにテクスチャを割り当てる
                        Material mat = m_Target.advertisementMaterials[0].materials[m_Target.targetMaterialNumber];
                        mat.SetTexture(m_Target.targetTextureProperty, m_Target.advertisementTexture);

                        if (EditorApplication.isPlaying)
                        {
                            m_Target.VideoPlayer.Stop();
                        }
                        break;
                    }
                    case PlateauSandboxAdvertisement.AdvertisementType.Video:
                    {
                        // VideoClip、RenderTextureを割当てる
                        string matDirPath = GetMaterialDirPath();
                        string duplicatedMatPath = GetDuplicatedMaterialPath(matDirPath);
                        if (File.Exists(duplicatedMatPath))
                        {
                            if (m_Target.advertisementVideoClip != null)
                            {
                                m_Target.VideoPlayer.clip = m_Target.advertisementVideoClip;
                            }

                            string renderTexturePath = GetRenderTexturePath(matDirPath);
                            RenderTexture renderTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>(renderTexturePath);
                            if (renderTexture != null)
                            {
                                Material mat = m_Target.advertisementMaterials[0].materials[m_Target.targetMaterialNumber];
                                renderTexture.Release();
                                renderTexture.width = (int)m_Target.VideoPlayer.width <= 0 ? 1 : (int)m_Target.VideoPlayer.width;
                                renderTexture.height = (int)m_Target.VideoPlayer.height <= 0 ? 1 : (int)m_Target.VideoPlayer.height;
                                renderTexture.Create();
                                mat.SetTexture(m_Target.targetTextureProperty, renderTexture);
                            }

                            if (EditorApplication.isPlaying)
                            {
                                m_Target.VideoPlayer.Play();
                            }
                        }
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                changedValue = true;
            }

            switch (m_Target.advertisementType)
            {
                case PlateauSandboxAdvertisement.AdvertisementType.Image:
                    if (DrawImageGUI())
                    {
                        changedValue = true;
                    }
                    break;
                case PlateauSandboxAdvertisement.AdvertisementType.Video:
                    if (DrawVideoGUI())
                    {
                        changedValue = true;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GUILayout.Space(5);

            DrawAspectGUI();

            if (changedValue)
            {
                EditorUtility.SetDirty(m_Target);
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// テクスチャ差し替え用GUI表示
        /// </summary>
        private bool DrawImageGUI()
        {
            EditorGUI.BeginChangeCheck();

            var adTexture = (Texture)EditorGUILayout.ObjectField(
                "Advertisement Texture",
                m_Target.advertisementTexture,
                typeof(Texture),
                false);
            if (EditorGUI.EndChangeCheck())
            {
                if (adTexture == null || adTexture == m_Target.advertisementTexture)
                {
                    return true;
                }

                m_Target.advertisementTexture = adTexture;

                // マテリアルを複製
                string matDirPath = GetMaterialDirPath();
                string duplicatedMatPath = GetDuplicatedMaterialPath(matDirPath);
                if (!DuplicateMaterial(duplicatedMatPath))
                {
                    return true;
                }

                // マテリアルにテクスチャを割り当てる
                Material duplicatedMat = AssetDatabase.LoadAssetAtPath<Material>(duplicatedMatPath);
                duplicatedMat.SetTexture(m_Target.targetTextureProperty, m_Target.advertisementTexture);

                // マテリアルを差し替える
                foreach (PlateauSandboxAdvertisement.AdvertisementMaterials advertisementMaterial in m_Target.advertisementMaterials)
                {
                    advertisementMaterial.materials[m_Target.targetMaterialNumber] = duplicatedMat;
                }
                m_Target.SetMaterials();

                return true;
            }

            return false;
        }

        /// <summary>
        /// ビデオ差し替え用GUI表示
        /// </summary>
        /// <returns></returns>
        private bool DrawVideoGUI()
        {
            EditorGUI.BeginChangeCheck();

            var adVideoClip = (VideoClip)EditorGUILayout.ObjectField(
                "Advertisement Video",
                m_Target.advertisementVideoClip,
                typeof(VideoClip),
                false);
            if (EditorGUI.EndChangeCheck())
            {
                if (adVideoClip == null || adVideoClip == m_Target.advertisementVideoClip)
                {
                    return false;
                }

                m_Target.advertisementVideoClip = adVideoClip;

                // マテリアルを複製
                string matDirPath = GetMaterialDirPath();
                string duplicatedMatPath = GetDuplicatedMaterialPath(matDirPath);
                if (!DuplicateMaterial(duplicatedMatPath))
                {
                    return true;
                }

                m_Target.VideoPlayer.clip = adVideoClip;

                // RenderTextureが存在しない場合は作成
                string duplicatedMatFileName = Path.GetFileNameWithoutExtension(duplicatedMatPath);
                string renderTexturePath = matDirPath + "/" + duplicatedMatFileName + ".renderTexture";
                RenderTexture renderTexture;
                if (!File.Exists(renderTexturePath))
                {
                    renderTexture = new RenderTexture((int)m_Target.VideoPlayer.width, (int)m_Target.VideoPlayer.height, 0);
                    renderTexture.Create();
                    AssetDatabase.CreateAsset(renderTexture, renderTexturePath);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    renderTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>(renderTexturePath);
                    renderTexture.Release();
                    renderTexture.width = (int)m_Target.VideoPlayer.width <= 0 ? 1 : (int)m_Target.VideoPlayer.width;
                    renderTexture.height = (int)m_Target.VideoPlayer.height <= 0 ? 1 : (int)m_Target.VideoPlayer.height;
                    renderTexture.Create();
                }

                m_Target.VideoPlayer.targetTexture = renderTexture;

                // マテリアルにRenderTextureを割り当てる
                Material duplicatedMat = AssetDatabase.LoadAssetAtPath<Material>(duplicatedMatPath);
                duplicatedMat.SetTexture(m_Target.targetTextureProperty, renderTexture);

                // マテリアルを差し替える
                foreach (PlateauSandboxAdvertisement.AdvertisementMaterials advertisementMaterial in m_Target.advertisementMaterials)
                {
                    advertisementMaterial.materials[m_Target.targetMaterialNumber] = duplicatedMat;
                }
                m_Target.SetMaterials();

                return true;
            }

            return false;
        }

        /// <summary>
        /// アスペクト比ガイド表示
        /// </summary>
        private void DrawAspectGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Aspect Ratio");
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("W", m_TextureAspectValueLabelOptions);
                EditorGUILayout.FloatField("", m_Target.textureAspectWidth, m_TextureAspectValueOptions);
                GUILayout.Space(2);
                EditorGUILayout.LabelField("H", m_TextureAspectValueLabelOptions);
                EditorGUILayout.FloatField("", m_Target.textureAspectHeight, m_TextureAspectValueOptions);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            float textureAspectWidthScale;
            float textureAspectHeightScale;
            Vector3 localScale = m_Target.transform.transform.localScale;
            switch (m_Target.frontAxis)
            {
                case PlateauSandboxAdvertisement.FrontAxis.X:
                    textureAspectWidthScale = localScale.z;
                    textureAspectHeightScale = localScale.y;
                    break;
                case PlateauSandboxAdvertisement.FrontAxis.Z:
                    textureAspectWidthScale = localScale.x;
                    textureAspectHeightScale = localScale.y;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            float d = GetFloatGcd(m_Target.textureAspectWidth * textureAspectWidthScale, m_Target.textureAspectHeight * textureAspectHeightScale);
            float scaledWidth = m_Target.textureAspectWidth * textureAspectWidthScale / d;
            float scaledHeight = m_Target.textureAspectHeight * textureAspectHeightScale / d;
            if (scaledWidth % Mathf.FloorToInt(scaledWidth) == 0 && scaledHeight % Mathf.FloorToInt(scaledHeight) == 0)
            {
                EditorGUILayout.HelpBox($"スケールを考慮したアスペクト比　W: {scaledWidth}  H: {scaledHeight}", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"スケールを考慮したアスペクト比　W: {scaledWidth:.0}  H: {scaledHeight:.0}", MessageType.Info);
            }
        }

        // <summary>
        // 最大公約数取得
        // </summary>
        private static float GetFloatGcd(float a, float b)
        {
            while (true)
            {
                if (a < b)
                {
                    (a, b) = (b, a);
                }

                // base case
                if (Mathf.Abs(b) < 0.001)
                {
                    return a;
                }

                float a1 = a;
                a = b;
                b = a1 - Mathf.Floor(a1 / b) * b;
            }
        }
    }
}
