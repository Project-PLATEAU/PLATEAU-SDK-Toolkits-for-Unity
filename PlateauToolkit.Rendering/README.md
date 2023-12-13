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

<img width="372" alt="スクリーンショット 2023-07-12 19 20 22" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/eb74c98d-9bec-4873-8247-1a0706495274">

Rendering Toolkitのメインメニューが表示されます。

<img width="319" alt="スクリーンショット 2023-07-12 19 26 53" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/0aeabee9-6ef1-4a4c-8687-2dc730008a52">


## 1. 環境システムの設定
「環境システムの設定」では、シーンの時間帯や天候などを変更し、3D都市モデルを使った環境シミュレーションを行うことができます。

### 1-1. 時間の変更
「Time of Day」欄のスライダーを動かすと、表示時間帯を変更することができます。

<img width="500" alt="render_timeofday" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/750c2937-334b-48ec-8a0d-28eb262ebfbf">

![PLATEAU-Rrendering-NightDay (1)](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/91e9dff1-df17-49bc-a65d-7029c8f44ec5)


### 1-2. 天候の変更
「Snow」「Rain」「Cloud」バーを動かすことで天候を変更することが可能です。  
変更後にGameビューで表示を確認することができます。  
Sceneビューでも表示可能ですが、対象となるカメラの前方のみの表示になりますので、確認の際はご注意ください。

<img width="400" alt="render_rainsnow" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/47237bfe-c75d-4e13-88ef-9d09dda9796e">

![PLATEAU-Rrendering-SnowRain](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/da138ca0-777a-45c1-aed5-f5e40e876cae)


### 1-3. 太陽光・月光の色変更
「Sun Color」を押すことで太陽の色を、「Moon Color」を押すことで月の色をそれぞれ設定することができます。

<img width="400" alt="render_suncolor" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/bcf518b3-acfb-4d07-b2b3-1c3b6ea72a01">

![PLATEAU-Rrendering-SunMoonColor](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/b3e44a5a-d5ee-41bc-98e2-0aca272a216f)

### 1-4. 雲の濃度の設定

※version 0.2.1より追加となりました。

環境システムの設定にある「Cloud Intensity」のスライダーを調整することで、空に表示される雲の量を調整することができます。

<img width="400" alt="rendering_manual_1_cloudintensity" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/74c3dd36-6d6a-4a28-bab0-56bdd3176803">
<br>
<img width="400" alt="rendering_manual_2_cloud_0" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/9fe84acd-4021-4813-8f90-8805db7c9c7c">
<img width="400" alt="rendering_manual_3_cloud_90" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/12ee3b68-252a-441e-b988-802745691834">

Cloud Intensity = 0の場合とCloud Intensity = 0.9の場合の比較


### 1-5. Fog Distanceの設定
「Fog Distance」のスライダーを調整することで、霧の濃さを調整することができます。
<img width="800" alt="スクリーンショット 2023-07-12 19 27 49" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/7e92bc04-3207-4215-9a1f-16c23cf0f61b">


### 1-6. Material Fadeの設定
「material fade」スライダーを調整すると、自動生成されたテクスチャのmaterialを単色化することができます。

<img width="400" alt="render_1_materialfadeUI" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/8e40556d-986e-47c6-afce-5a5effeef064">


![PLATEAU-Rrendering-WeatherMaterialFade](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/b12e3580-b67c-4c37-a398-876f3b0a823b)


## 2. 自動テクスチャの生成
「自動テクスチャの生成」では、3D都市モデルの建築物モデルに対してランダムにテクスチャを貼り付けることができます。  
既にテクスチャを持つLOD2建築物モデルに対しては、窓のライトのみ付与します。

1. Unityの「Hierarchy」ビューより対象となる建物のGameObjectを選択してください。
<img width="497" alt="スクリーンショット 2023-07-12 19 28 29" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/66682257-2e1c-42c7-8d82-640e77ae460e">

2. 「自動テクスチャの生成」メニューの中の「テクスチャ生成」ボタンを押下してください。
3. テクスチャ作成の確認画面が表示されます。問題なければ「はい」ボタンを押下してください。
<img width="800" alt="スクリーンショット 2023-07-12 19 28 34" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/880403da-d215-4c61-8a67-7b5d1f6b815a">


4. テクスチャが自動的に生成され、モデルの見た目が変更されます。
<img width="800" alt="スクリーンショット 2023-07-12 19 28 41" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/2365981d-97d9-4e57-ab3b-c18180bee4dc">

この状態で「環境システムの設定」メニューから「「Time of Day」を夜にすると、窓のライトが表示されます。（なお、現在は主に高さのある建物に対してのみライトの表示が適用されます。)

<img width="800" alt="スクリーンショット 2023-07-12 19 28 47" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/7976bd88-3f94-4228-8450-0536bbe87319">


### 注意点
テクスチャの自動生成後は3D都市モデルのHierarchyビューの構成が変わります。あらかじめご注意ください。

#### 変更前
PLATEAU SDKでダウンロードした直後は専用親GameObject（下記の場合は13100_tokyo23-ku_2022_citygml_1_2_op）に3D都市オブジェクトが格納されている。

<img width="499" alt="スクリーンショット 2023-07-12 19 28 53" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/683c6f0a-c26d-4ee0-a31f-66e81b40045c">


#### 変更後
「ParentForGroupedObjects」と呼ばれるGameObjectに全てのモデルデータが移動する。
<img width="499" alt="スクリーンショット 2023-07-12 19 29 07" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/53852b0b-9439-49e3-9db7-4213c6021adf">


### 窓の表示

「窓の表示切り替え」ボタンを押下すると、テクスチャに合わせた窓の表示もしくは非表示を切り替えることが可能です。  
この機能は、現時点ではLOD2建築物モデルのみが対象となります。


## 3. LODグループ生成
「LODグループ生成」ボタンを押すと、すべての3D都市モデルに対してUnityのLOD機能が設定されます。  
LODグループが生成されると、建物オブジェクトに対してのカメラの距離で表示されるグラフィクスが変化します。

> **Note**
> UnityのLODはPLATEAUのLODとは異なり、カメラとオブジェクトの距離によって、どれだけ細かく表示するかという設定を行う機能です。
> [Unity Documentation Level of Detail (LOD)](https://docs.unity3d.com/ja/2018.4/Manual/LevelOfDetail.html)


<img width="1000" alt="スクリーンショット 2023-07-12 19 29 16" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/ca0c27e4-4768-4373-b805-7dfa514cb58d">


### 注意点
PLATEAUで定義されている3D都市モデルのLOD概念とUnity上でのLOD概念が異なることにご注意ください。
- PLATEAU LOD・・・LOD0が最下位であり、上位になればよりリッチな詳細度を持ったモデルになります。
- UnityのLOD・・・最も建物とカメラが近いLOD0がハイグラフィックになり、カメラが遠ざかるとLODが上がり、簡素なに表現なります。

<img width="800" alt="スクリーンショット 2023-07-12 19 29 30" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/ba2b123e-f61a-4768-af18-e96eb056851b">



なお、カメラからの距離と対応するLODの設定は、各都市モデルの親オブジェクトのInspector Viewから調整できます。
<img width="1010" alt="スクリーンショット 2023-07-12 19 29 37" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/78747342-5e0b-4849-9cb3-8b20423e03ce">


## 4. シーンの保存
テクスチャ生成などを行ったら右下にあるSceneビューの右下にある「シーンの保存」ボタンを押下し、保存してください。また、やり直したい場合は最後に保存した時点に戻ることも可能です。

<img width="193" alt="スクリーンショット 2023-07-12 19 29 48" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/6809d886-6885-4d52-b264-c04ce3c927cd">


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

<img width="600" alt="rendering_manual_4_qualitysettings" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/160feb1f-a9d1-416c-b828-69e701d32773">

Quality設定が完了したら、Renderer Featureを使ってエフェクト追加をします。

プロジェクトのAssets内で現在のレンダラー設定を選択します（下記のサンプルの中では「Assets/ Settings」フォルダの中にある「URP-Performant-Renderer」を選択）。

<img width="600" alt="rendering_manual_5_addfeaturedenreder" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/829fbd41-83c6-421b-8506-6237994f1c2d">

インスペクタービューの中にある「Add Renderer Feature」を押下すると、追加エフェクトを選択できます。

<img width="600" alt="rendering_manual_6_featurepulldown" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/70570152-895a-4612-a103-b3e1644500db">

エフェクトの追加を行った後、インスペクターで通常のポストエフェクト同様、順番の切り替えや詳細な設定が可能になります。

<img width="600" alt="rendering_manual_7_nightvision" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/44faa57a-5bbb-4a08-be65-8157c83fc6d0">



#### HDRPの場合

HDRPの場合は`Custom Pass Volume` を利用する必要があります。`Custom Pass Volume` により、特定の物体や背景に独自のエフェクトや見た目の変更を適用することが可能になります

シーンに空のGameObjectを追加し、 `Custom Pass Volume` コンポーネントを追加してください。

[(参考)Custom Pass Volume ワークフロー](https://docs.unity3d.com/ja/Packages/com.unity.render-pipelines.high-definition@10.5/manual/Custom-Pass-Volume-Workflow.html)

**Custom Pass Volume ワークフロー**

<img width="400" alt="rendering_manual_8_custompassvolume" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/f9ed4bb3-e580-4ad2-8917-358158c20f63">

`Custom Pass Volume` コンポーネントの Custom Passes リストの「+」ボタンを押下するとエフェクトの選択パネルが表示されます。

<img width="400" alt="rendering_manual_9_select" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/462082b7-571c-465c-b22b-5158902c948f">


その後URPや通常のVolume同様エフェクトの順番や調整が可能になります。

※ `Injection Point`を「After Post Processing」に変更する必要があり、それ以外の場合は描画に不具合が出る可能性があるので注意してください。

<img width="600" alt="rendering_manual_10_hdrp" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/e514fb75-37c6-4b0e-a227-44cc3e10092a">


### 5-2. 追加されるポストエフェクト

Rendering Toolkitのポストエフェクト機能では下記の3種類のエフェクト機能を提供しています。  
いずれの機能もRenderer Featureの中の「Settings」内のパラメーターを変更することで表現を調整することが可能です。

<img width="545" alt="rendering_manual_11_tiltshit" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/cd533b89-ebf8-4aba-b41a-ed4cb6aecf0f">


#### トイカメラ

カメラに対してぼかし処理をかけます。ぼかし処理は画面の中央から上下端に行くほどぼかしが強くなり、トイカメラのような描画になります。

<img width="600" alt="rendering_manual_12_toycamera" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/935ac935-0429-4ecc-9d9e-7d2c796b6c5c">


「Settings」の中のメニューによってパラメータを変更することが可能です。

<img width="600" alt="rendering_manual_13_tiltshiftparam" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/0f31cef0-1c15-4f59-8dd2-05e057b6a3fc">

| 設定項目 | 内容 |
| --- | --- |
| Blur Size | ぼかしの強さ（広さ） |
| Blur Start Range | ぼかしが始まる基準点（0: 画面中央 1: 画面上下端） |
| Blur Iterations | ぼかしの適用回数 |
| Blur Samples | ピクセル毎のサンプリング回数 (Blur Iterationsと組み合わせて表現の滑らかさの調整ができます。） |

#### ハーフトーン

画面全体をドット絵のように表現します。画面のUVをグリッド状に細かく分割し、ハーフトーンのコマを無数に作成することで実現しています。

<img width="600" alt="rendering_manual_14_halftone" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/ebe3edb3-b05d-49f5-8280-f9baf99bc40a">

「Settings」の中のメニューによってパラメータを変更することが可能です。

<img width="404" alt="rendering_manual_15_halftoneparam" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/0d1a67ed-6007-49a4-9508-b43c65b6c97f">

| 設定項目 | 内容 |
| --- | --- |
| Halftone Size | コマの大きさ |
| Halftone Range | 明るさの適用される敏感度（数値が低いと明るく、高いと暗くなります。） |
| Use Color | シーンの色をエフェクトに適用するか（オフの場合白黒画像になります。） |

#### ナイトビジョン

カラーバッファーを取得し全体の明るさを表す白黒画像に変換にします。その後設定のスライダーから入力された値を元に全体の明るさを調整したり、設定した色（デフォルトではグリーン）などの色を混ぜることが可能です。

<img width="600" alt="rendering_manual_16_nightvision_2" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/365adebc-4c70-479b-a5ac-795221597f11">

「Settings」の中のメニューによってパラメータを変更することが可能です。

<img width="404" alt="rendering_manual_17_nightvisionparam" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/2a3b8089-400e-4c7f-9fe2-d03808082ed6">

## 6. 頂点カラーの設定
頂点カラーの設定メニューから、マスク範囲と頂点アルファのランダムシード値を設定できます。
![image](https://github.com/PLATEAU-Toolkits-Internal/PLATEAU-SDK-Toolkits-for-Unity/assets/127069970/91682674-ea09-4257-9046-884cde40ffe8)

頂点カラーマスク機能を使用することで、自動生成する窓のテクスチャをマスクする(隠す)範囲を設定できます。
地物の上端からの割合(%)で指定し、指定した範囲は窓が表示されなくなります。
![image](https://github.com/PLATEAU-Toolkits-Internal/PLATEAU-SDK-Toolkits-for-Unity/assets/127069970/074c61d7-0104-4236-aeb0-84eafdfada34)

頂点アルファのランダムシード値設定を活用することで、地物ごとのアルファチャンネルをランダムに割り当てることができます。
![image](https://github.com/PLATEAU-Toolkits-Internal/PLATEAU-SDK-Toolkits-for-Unity/assets/127069970/4028d71c-7581-4d80-9eef-e9ec8b46f512)

AR City Miniatureサンプルで[建物ごとに色味を変えて3D都市モデルの審美性を高める例](https://github.com/PLATEAU-Toolkits-Internal/PLATEAU-Toolkits-Sample-ARCityMiniature?tab=readme-ov-file#4-2-%E3%83%A2%E3%83%90%E3%82%A4%E3%83%AB%E7%AB%AF%E6%9C%AB%E3%82%92%E5%AF%BE%E8%B1%A1%E3%81%AB%E3%81%97%E3%81%9F3d%E9%83%BD%E5%B8%82%E3%83%A2%E3%83%87%E3%83%AB%E3%81%AE%E4%BD%9C%E6%88%90)を解説しておりますので、ご参照ください。



## 関連API
Rendering Toolkit の開発において、Unity の以下のAPI を使用しています。本ツールをカスタマイズしながら作りたい場合にはこちらを参考にしてください。
1. [MeshRenderer](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/MeshRenderer.html)
2. [MeshFilter](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/MeshFilter.html)
3. [LOD](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/LOD.html)
4. [Skybox](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Skybox.html)
5. [HideFlags](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/HideFlags.html)
6. [Shaders](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Shader.html)
7. [VFX](https://docs.unity3d.com/Packages/com.unity.visualeffectgraph@7.0/api/UnityEngine.VFX.Utility.html)

