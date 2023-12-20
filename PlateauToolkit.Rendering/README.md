### PLATEAU SDK-Toolkits for Unity
# Rendering Toolkit 利用マニュアル

PLATEAUの3D都市モデルのグラフィックスを向上させる処理を行います。  環境設定、テクスチャの自動作成、LOD設定等の機能をGUI上で提供します。  なお、URP環境においては下記のFog, Cloudといった天候に関する設定機能がありますがHDRP環境にはありませんのでご注意ください。

- [利用手順](#利用手順)
  * [1. 環境システムの設定](#1-環境システムの設定)
    + [1-1. 時間の変更](#1-1-時間の変更)
    + [1-2. 天候の変更](#1-2-天候の変更)
    + [1-3. 太陽光・月光の色変更](#1-3-太陽光月光の色変更)
    + [1-4. 雲の濃度の設定](#1-4-雲の濃度の設定)
    + [1-5. Fog Distanceの設定](#1-5-fog-distanceの設定)
    + [1-6. Material Fadeの設定](#1-6-material-fadeの設定)
    
  * [2. 自動テクスチャの生成](#2-自動テクスチャの生成)
  
  * [3. LODグループ生成](#3-LODグループ生成)
    
  * [4. シーンの保存](#4-シーンの保存)
    
  * [5. ポストエフェクト](#5-ポストエフェクト)
    + [5-1. 事前準備](#5-1-事前準備)
    + [5-2. 追加されるポストエフェクト](#5-2-追加されるポストエフェクト)
  * [6. 頂点カラーの設定](#6-頂点カラーの設定)

- [関連API](#関連api)

# 利用手順

PLATEAU SDK-Toolkits for Unityのインストール後、上部のメニューより「PLATEAU」>「PLATEAU Toolkit」>「Rendering Toolkit」を選択します。

<img width="372" alt="render_menu_selection" src="../Documentation~/Rendering Images/render_menu_selection.png">

Rendering Toolkitのメインメニューが表示されます。

<img width="319" alt="render_menu_ui" src="../Documentation~/Rendering Images/render_menu_ui.png">


## 1. 環境システムの設定
「環境システムの設定」では、シーンの時間帯や天候などを変更し、3D都市モデルを使った環境シミュレーションを行うことができます。

### 1-1. 時間の変更
「Time of Day」欄のスライダーを動かすと、表示時間帯を変更することができます。

<img width="400" alt="render_timeofday" src="../Documentation~/Rendering Images/render_timeofday.png">
<img width="800" alt="render_night_day" src="../Documentation~/Rendering Images/render_night_day.png">


### 1-2. 天候の変更
「Snow」「Rain」「Cloud」バーを動かすことで天候を変更することが可能です。  
変更後にGameビューで表示を確認することができます。  
Sceneビューでも表示可能ですが、対象となるカメラの前方のみの表示になりますので、確認の際はご注意ください。

<img width="400" alt="render_rainsnow" src="../Documentation~/Rendering Images/render_rainsnow.png">
<img width="800" alt="render_snow_rain" src="../Documentation~/Rendering Images/render_snow_rain.png">


### 1-3. 太陽光・月光の色変更
「Sun Color」を押すことで太陽の色を、「Moon Color」を押すことで月の色をそれぞれ設定することができます。

<img width="400" alt="render_suncolor" src="../Documentation~/Rendering Images/render_suncolor.png">
<img width="800" alt="render_sun_moon" src="../Documentation~/Rendering Images/render_sun_moon.png">

### 1-4. 雲の濃度の設定

※version 0.2.1より追加となりました。

環境システムの設定にある「Cloud Intensity」のスライダーを調整することで、空に表示される雲の量を調整することができます。<br>
<img width="400" alt="rendering_manual_1_cloudintensity" src="../Documentation~/Rendering Images/rendering_manual_1_cloudintensity.png"><br>
<img width="400" alt="rendering_manual_2_cloud_0" src="../Documentation~/Rendering Images/rendering_manual_2_cloud_0.png">
<img width="400" alt="rendering_manual_3_cloud_90" src="../Documentation~/Rendering Images/rendering_manual_3_cloud_90.png">

Cloud Intensity = 0の場合とCloud Intensity = 0.9の場合の比較


### 1-5. Fog Distanceの設定
「Fog Distance」のスライダーを調整することで、霧の濃さを調整することができます。
<img width="800" alt="render_fog_dist" src="../Documentation~/Rendering Images/render_fog_dist.png">


### 1-6. Material Fadeの設定
「material fade」スライダーを調整すると、自動生成されたテクスチャのmaterialを単色化することができます。

<img width="400" alt="render_1_materialfadeUI" src="../Documentation~/Rendering Images/render_1_materialfadeUI.png">
<img width="800" alt="render_weather_material_fade" src="../Documentation~/Rendering Images/render_weather_material_fade.png">



## 2. 自動テクスチャの生成
「自動テクスチャの生成」では、3D都市モデルの建築物モデルに対してランダムにテクスチャを貼り付けることができます。  
既にテクスチャを持つLOD2建築物モデルに対しては、窓のライトのみ付与します。

1. Unityの「Hierarchy」ビューより対象となる建物のGameObjectを選択してください。
<img width="497" alt="render_auto_texture_objselection" src="../Documentation~/Rendering Images/render_auto_texture_objselection.png">

2. 「自動テクスチャの生成」メニューの中の「テクスチャ生成」ボタンを押下してください。
3. テクスチャ作成の確認画面が表示されます。問題なければ「はい」ボタンを押下してください。
<img width="800" alt="render_auto_texture_generation" src="../Documentation~/Rendering Images/render_auto_texture_generation.png">


4. テクスチャが自動的に生成され、モデルの見た目が変更されます。
<img width="800" alt="render_auto_texture_application" src="../Documentation~/Rendering Images/render_auto_texture_application.png">

この状態で「環境システムの設定」メニューから「「Time of Day」を夜にすると、窓のライトが表示されます。（なお、現在は主に高さのある建物に対してのみライトの表示が適用されます。)

<img width="800" alt="render_auto_texture_windowlight" src="../Documentation~/Rendering Images/render_auto_texture_windowlight.png">


### 注意点
テクスチャの自動生成後は3D都市モデルのHierarchyビューの構成が変わります。あらかじめご注意ください。

#### 変更前
PLATEAU SDKでダウンロードした直後は専用親GameObject（下記の場合は13100_tokyo23-ku_2022_citygml_1_2_op）に3D都市オブジェクトが格納されている。

<img width="499" alt="render_auto_texture_objhierarchy_before" src="../Documentation~/Rendering Images/render_auto_texture_objhierarchy_before.png">


#### 変更後
「ParentForGroupedObjects」と呼ばれるGameObjectに全てのモデルデータが移動する。
<img width="499" alt="render_auto_texture_objhierarchy_after" src="../Documentation~/Rendering Images/render_auto_texture_objhierarchy_after.png">


### 窓の表示

「窓の表示切り替え」ボタンを押下すると、テクスチャに合わせた窓の表示もしくは非表示を切り替えることが可能です。  
この機能は、現時点ではLOD2建築物モデルのみが対象となります。


## 3. LODグループ生成
「LODグループ生成」ボタンを押すと、すべての3D都市モデルに対してUnityのLOD機能が設定されます。  
LODグループが生成されると、建物オブジェクトに対してのカメラの距離で表示されるグラフィクスが変化します。

> **Note**
> UnityのLODはPLATEAUのLODとは異なり、カメラとオブジェクトの距離によって、どれだけ細かく表示するかという設定を行う機能です。
> [Unity Documentation Level of Detail (LOD)](https://docs.unity3d.com/ja/2018.4/Manual/LevelOfDetail.html)


<img width="1000" alt="render_lod_ui" src="../Documentation~/Rendering Images/render_lod_ui.png">


### 注意点
PLATEAUで定義されている3D都市モデルのLOD概念とUnity上でのLOD概念が異なることにご注意ください。
- PLATEAU LOD・・・LOD0が最下位であり、上位になればよりリッチな詳細度を持ったモデルになります。
- UnityのLOD・・・最も建物とカメラが近いLOD0がハイグラフィックになり、カメラが遠ざかるとLODが上がり、簡素なに表現なります。

<img width="800" alt="render_lod_examples" src="../Documentation~/Rendering Images/render_lod_examples.png">



なお、カメラからの距離と対応するLODの設定は、各都市モデルの親オブジェクトのInspector Viewから調整できます。
<img width="1010" alt="render_lod_inspectorview" src="../Documentation~/Rendering Images/render_lod_inspectorview.png">


## 4. シーンの保存
テクスチャ生成などを行ったら右下にあるSceneビューの右下にある「シーンの保存」ボタンを押下し、保存してください。また、やり直したい場合は最後に保存した時点に戻ることも可能です。

<img width="193" alt="render_save_scene" src="../Documentation~/Rendering Images/render_save_scene.png">


## 5. ポストエフェクト

※version 0.2.1より追加となりました。

ポストエフェクトは、カメラ画像に追加的な視覚効果を施す機能です。画面全体にフィルターやブラー、光の効果などを後からかけることで、さまざまな雰囲気を演出できます。
Rendering Toolkitではデフォルトのポストエフェクトに加え3種類の追加的なポストエフェクトを利用可能です。

### 5-1. 事前準備

#### URPの場合

URPではUnityのRenderer Featureという機能を使ってポストエフェクトを実現します。

事前準備としてレンダラー設定が複数ある場合はプロジェクトのQuality設定を確認します。

Edit > Project Settings > Quality で設定画面に入り、QualityのPerformantを下記のように設定してください。

[(参考)Renderer Feature をレンダラーに追加する方法](https://docs.unity3d.com/ja/Packages/com.unity.render-pipelines.universal@14.0/manual/urp-renderer-feature-how-to-add.html)

<img width="600" alt="rendering_manual_4_qualitysettings" src="../Documentation~/Rendering Images/rendering_manual_4_qualitysettings.png">

Quality設定が完了したら、Renderer Featureを使ってエフェクト追加をします。

プロジェクトのAssets内で現在のレンダラー設定を選択します（下記のサンプルの中では「Assets/ Settings」フォルダの中にある「URP-Performant-Renderer」を選択）。

<img width="600" alt="rendering_manual_5_addfeaturedenreder" src="../Documentation~/Rendering Images/rendering_manual_5_addfeaturedenreder.png">

インスペクタービューの中にある「Add Renderer Feature」を押下すると、追加エフェクトを選択できます。

<img width="600" alt="rendering_manual_6_featurepulldown" src="../Documentation~/Rendering Images/rendering_manual_6_featurepulldown.png">

エフェクトの追加を行った後、インスペクターで通常のポストエフェクト同様、順番の切り替えや詳細な設定が可能になります。

<img width="600" alt="rendering_manual_7_nightvision" src="../Documentation~/Rendering Images/rendering_manual_7_nightvision.png">



#### HDRPの場合

HDRPの場合は`Custom Pass Volume` を利用する必要があります。`Custom Pass Volume` により、特定の物体や背景に独自のエフェクトや見た目の変更を適用することが可能になります

シーンに空のGameObjectを追加し、 `Custom Pass Volume` コンポーネントを追加してください。

[(参考)Custom Pass Volume ワークフロー](https://docs.unity3d.com/ja/Packages/com.unity.render-pipelines.high-definition@10.5/manual/Custom-Pass-Volume-Workflow.html)

**Custom Pass Volume ワークフロー**

<img width="400" alt="rendering_manual_8_custompassvolume" src="../Documentation~/Rendering Images/rendering_manual_8_custompassvolume.png">

`Custom Pass Volume` コンポーネントの Custom Passes リストの「+」ボタンを押下するとエフェクトの選択パネルが表示されます。

<img width="400" alt="rendering_manual_9_select" src="../Documentation~/Rendering Images/rendering_manual_9_select.png">


その後URPや通常のVolume同様エフェクトの順番や調整が可能になります。

※ `Injection Point`を「After Post Processing」に変更する必要があり、それ以外の場合は描画に不具合が出る可能性があるので注意してください。

<img width="600" alt="rendering_manual_10_hdrp" src="../Documentation~/Rendering Images/rendering_manual_10_hdrp.png">


### 5-2. 追加されるポストエフェクト

Rendering Toolkitのポストエフェクト機能では下記の3種類のエフェクト機能を提供しています。  
いずれの機能もRenderer Featureの中の「Settings」内のパラメーターを変更することで表現を調整することが可能です。

<img width="545" alt="rendering_manual_11_tiltshit" src="../Documentation~/Rendering Images/rendering_manual_11_tiltshit.png">


#### トイカメラ

カメラに対してぼかし処理をかけます。ぼかし処理は画面の中央から上下端に行くほどぼかしが強くなり、トイカメラのような描画になります。

<img width="600" alt="rendering_manual_12_toycamera" src="../Documentation~/Rendering Images/rendering_manual_12_toycamera.png">


「Settings」の中のメニューによってパラメータを変更することが可能です。

<img width="600" alt="rendering_manual_13_tiltshiftparam" src="../Documentation~/Rendering Images/rendering_manual_13_tiltshiftparam.png">

| 設定項目 | 内容 |
| --- | --- |
| Blur Size | ぼかしの強さ（広さ） |
| Blur Start Range | ぼかしが始まる基準点（0: 画面中央 1: 画面上下端） |
| Blur Iterations | ぼかしの適用回数 |
| Blur Samples | ピクセル毎のサンプリング回数 (Blur Iterationsと組み合わせて表現の滑らかさの調整ができます。） |

#### ハーフトーン

画面全体をドット絵のように表現します。画面のUVをグリッド状に細かく分割し、ハーフトーンのコマを無数に作成することで実現しています。

<img width="600" alt="rendering_manual_14_halftone" src="../Documentation~/Rendering Images/rendering_manual_14_halftone.png">

「Settings」の中のメニューによってパラメータを変更することが可能です。

<img width="404" alt="rendering_manual_15_halftoneparam" src="../Documentation~/Rendering Images/rendering_manual_15_halftoneparam.png">

| 設定項目 | 内容 |
| --- | --- |
| Halftone Size | コマの大きさ |
| Halftone Range | 明るさの適用される敏感度（数値が低いと明るく、高いと暗くなります。） |
| Use Color | シーンの色をエフェクトに適用するか（オフの場合白黒画像になります。） |

#### ナイトビジョン

カラーバッファーを取得し全体の明るさを表す白黒画像に変換にします。その後設定のスライダーから入力された値を元に全体の明るさを調整したり、設定した色（デフォルトではグリーン）などの色を混ぜることが可能です。

<img width="600" alt="rendering_manual_16_nightvision_2" src="../Documentation~/Rendering Images/rendering_manual_16_nightvision_2.png">

「Settings」の中のメニューによってパラメータを変更することが可能です。

<img width="404" alt="rendering_manual_17_nightvisionparam" src="../Documentation~/Rendering Images/rendering_manual_17_nightvisionparam.png">

## 6. 頂点カラーの設定
頂点カラーの設定メニューから、マスク範囲と頂点アルファのランダムシード値を設定できます。

<img width="600" alt="rendering_manual_18_vertexcolor_ui" src="../Documentation~/Rendering Images/rendering_manual_18_vertexcolor_ui.png">

頂点カラーマスク機能を使用することで、自動生成する窓のテクスチャをマスクする(隠す)範囲を設定できます。
地物の上端からの割合(%)で指定し、指定した範囲は窓が表示されなくなります。<br>
<img width="600" alt="rendering_manual_19_vertexcolor_range" src="../Documentation~/Rendering Images/rendering_manual_19_vertexcolor_range.png">

頂点アルファのランダムシード値設定を活用することで、地物ごとにランダムな値を割り振り、または調整を行うことができます。<br>
頂点アルファをシェーダーのパラメータ化し、建物ごとに色味を変える設定を行うと、街の外観を立体的に表現できます。
<br>
<img width="600" alt="rendering_manual_20_vertexcolor_randomseed" src="../Documentation~/Rendering Images/rendering_manual_20_vertexcolor_randomseed.png">

例として、夜の街灯りの表現をご紹介します。各ビルには色温度の範囲内でランダムカラーグラデーションが割り当てられます。<br>
地物ごとのランダムな値は、AutoTextruing実行時に各地物の頂点アルファにランダムな値が自動で付与されます。
<br>
<img width="600" alt="rendering_manual_20_vertexcolor_randomseed" src="../Documentation~/Rendering Images/render_vertexcolor1.png">

頂点アルファのランダムシード値設定を活用することで、地物に割り当てられた頂点アルファ値及びカラーグラデーションの微調整を行うことが可能です。<br>
例：黄味がかったグラデーションから青味がかったグラデーションに調整する場合:
<br>
<img width="600" alt="rendering_manual_20_vertexcolor_randomseed" src="../Documentation~/Rendering Images/render_vertexcolor2.png">
<br>
<img width="600" alt="rendering_manual_20_vertexcolor_randomseed" src="../Documentation~/Rendering Images/render_vertexcolor3.png">

AR City Miniatureサンプルで[活用例](https://github.com/PLATEAU-Toolkits-Internal/PLATEAU-Toolkits-Sample-ARCityMiniature?tab=readme-ov-file#4-2-%E3%83%A2%E3%83%90%E3%82%A4%E3%83%AB%E7%AB%AF%E6%9C%AB%E3%82%92%E5%AF%BE%E8%B1%A1%E3%81%AB%E3%81%97%E3%81%9F3d%E9%83%BD%E5%B8%82%E3%83%A2%E3%83%87%E3%83%AB%E3%81%AE%E4%BD%9C%E6%88%90)を解説しておりますので、ご参照ください。



## 関連API
Rendering Toolkit の開発において、Unity の以下のAPI を使用しています。本ツールをカスタマイズしながら作りたい場合にはこちらを参考にしてください。
1. [MeshRenderer](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/MeshRenderer.html)
2. [MeshFilter](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/MeshFilter.html)
3. [LOD](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/LOD.html)
4. [Skybox](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Skybox.html)
5. [HideFlags](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/HideFlags.html)
6. [Shaders](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Shader.html)
7. [VFX](https://docs.unity3d.com/Packages/com.unity.visualeffectgraph@7.0/api/UnityEngine.VFX.Utility.html)

