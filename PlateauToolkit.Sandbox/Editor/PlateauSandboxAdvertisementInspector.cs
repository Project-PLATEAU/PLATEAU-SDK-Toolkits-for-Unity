using PlateauToolkit.Sandbox.Runtime;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace PlateauToolkit.Sandbox.Editor.PlateauToolkit.Sandbox.Editor
{
    [CustomEditor(typeof(PlateauSandboxAdvertisement))]
    public class PlateauSandboxAdvertisementInspector : UnityEditor.Editor
    {
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
            // マテリアルが設定されている
            // 指定したマテリアル番号が存在する
            // マテリアルが存在する
            // マテリアルが指定プロパティを持っている
            // 上記が全て当てはまれば有効
            return m_Target.advertisementMaterials.Count > 0 &&
                   m_Target.advertisementMaterials[0].materials.Count > m_Target.targetMaterialNumber &&
                   m_Target.advertisementMaterials[0].materials[m_Target.targetMaterialNumber] != null &&
                   m_Target.advertisementMaterials[0].materials[m_Target.targetMaterialNumber].HasProperty(m_Target.targetTextureProperty);
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
                EditorGUILayout.HelpBox("コンポーネントをリセットして下さい。", MessageType.Info);
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
                        if (m_Target.advertisementTexture == null)
                        {
                            return;
                        }
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
                        // マテリアルにレンダーテクスチャを割り当てる
                        if (m_Target.advertisementVideoClip == null)
                        {
                            return;
                        }
                        Material mat = m_Target.advertisementMaterials[0].materials[m_Target.targetMaterialNumber];
                        m_Target.VideoPlayer.clip = m_Target.advertisementVideoClip;

                        // RenderTexture作成
                        var renderTexture = new RenderTexture(
                            (int)m_Target.VideoPlayer.width <= 0 ? 1 : (int)m_Target.VideoPlayer.width,
                            (int)m_Target.VideoPlayer.height <= 0 ? 1 : (int)m_Target.VideoPlayer.height,
                            0);
                        renderTexture.Create();
                        m_Target.VideoPlayer.targetTexture = renderTexture;
                        mat.SetTexture(m_Target.targetTextureProperty, renderTexture);

                        if (EditorApplication.isPlaying)
                        {
                            m_Target.VideoPlayer.Play();
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

            if (DrawAspectGUI())
            {
                changedValue = true;
            }

            GUILayout.Space(5);

            // ハンドル方向チェック
            EditorGUI.BeginChangeCheck();

            var placementDirection = (PlateauSandboxPlaceableHandler.GroundPlacementDirection)EditorGUILayout.EnumPopup(
                "Ground Placement Direction",
                m_Target.groundPlacementDirection);

            if (EditorGUI.EndChangeCheck())
            {
                m_Target.groundPlacementDirection = placementDirection;
                changedValue = true;
            }

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
                if (adTexture == m_Target.advertisementTexture)
                {
                    return false;
                }

                m_Target.advertisementTexture = adTexture;

                // マテリアル複製
                Material mat = m_Target.advertisementMaterials[0].materials[m_Target.targetMaterialNumber];
                var duplicatedMat = new Material(mat);
                duplicatedMat.SetTexture(m_Target.targetTextureProperty, m_Target.advertisementTexture);

                // マテリアルを差し替え
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
                // クリップ削除時はテクスチャを利用する
                Material mat = m_Target.advertisementMaterials[0].materials[m_Target.targetMaterialNumber];
                if (adVideoClip == null)
                {
                    m_Target.advertisementVideoClip = adVideoClip;
                    m_Target.VideoPlayer.clip = adVideoClip;

                    var duplicatedTexMat = new Material(mat);
                    duplicatedTexMat.SetTexture(m_Target.targetTextureProperty, m_Target.advertisementTexture);

                    // マテリアルを差し替える
                    foreach (PlateauSandboxAdvertisement.AdvertisementMaterials advertisementMaterial in m_Target.advertisementMaterials)
                    {
                        advertisementMaterial.materials[m_Target.targetMaterialNumber] = duplicatedTexMat;
                    }
                    m_Target.SetMaterials();

                    return true;
                }

                if (adVideoClip == m_Target.advertisementVideoClip)
                {
                    return false;
                }

                m_Target.advertisementVideoClip = adVideoClip;
                m_Target.VideoPlayer.clip = adVideoClip;

                // マテリアル複製
                var duplicatedMat = new Material(mat);

                // RenderTexture作成
                var renderTexture = new RenderTexture(
                    (int)m_Target.VideoPlayer.width <= 0 ? 1 : (int)m_Target.VideoPlayer.width,
                    (int)m_Target.VideoPlayer.height <= 0 ? 1 : (int)m_Target.VideoPlayer.height,
                    0);
                renderTexture.Create();
                m_Target.VideoPlayer.targetTexture = renderTexture;
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
        private bool DrawAspectGUI()
        {
            bool isEditedScale = false;

            EditorGUI.BeginChangeCheck();
            Vector3 adSize = EditorGUILayout.Vector3Field("広告の大きさ", m_Target.adSize);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(m_Target, "Change Ad Size");
                isEditedScale = true;
                m_Target.adSize = adSize;
                m_Target.transform.localScale = new Vector3(m_Target.adSize.x / m_Target.defaultAdSize.x, m_Target.adSize.y / m_Target.defaultAdSize.y, m_Target.adSize.z / m_Target.defaultAdSize.z);
            }

            return isEditedScale;
        }
    }
}
