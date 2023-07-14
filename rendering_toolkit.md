### PLATEAU-SDK-Toolkits-for-Unity
# Rendering Toolkit 利用マニュアル

PLATEAUの3D都市モデルのグラフィックスを向上させる処理を行います。  
環境設定、テクスチャの自動作成、LOD設定等の機能をGUI上で提供します。  

なお、URP環境においては下記のFog, Cloudといった天候に関する設定機能がありますがHDRP環境にはありませんのでご注意ください。

# 利用手順

PLATEAU-SDK-Toolkits-for-Unityのインストール後、上部のメニューより「PLATEAU」>「PLATEAU Toolkit」>「Rendering Toolkit」を選択します。

<img width="372" alt="スクリーンショット 2023-07-12 19 20 22" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/eb74c98d-9bec-4873-8247-1a0706495274">


Rendering Toolkitのメインメニューが表示されます。

<img width="319" alt="スクリーンショット 2023-07-12 19 26 53" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/0aeabee9-6ef1-4a4c-8687-2dc730008a52">


## 1. 環境システムの設定
「環境システムの設定」では、シーンの時間帯や天候などを変更し、3D都市モデルを使った環境シミュレーションを行うことができます。

### 時間の変更
「Time of Day」欄のスライダーを動かすと、表示時間帯を変更することができます。
![PLATEAU-Rrendering-NightDay (1)](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/91e9dff1-df17-49bc-a65d-7029c8f44ec5)


### 天候の変更
「Snow」「Rain」「Cloud」バーを動かすことで天候を変更することが可能です。  
変更後にGameビューで表示を確認することができます。  
Sceneビューでも表示可能ですが、対象となるカメラの前方のみの表示になりますので、確認の際はご注意ください。
![PLATEAU-Rrendering-SnowRain](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/da138ca0-777a-45c1-aed5-f5e40e876cae)


### 太陽光・月光の色変更
「Sun Color」を押すことで太陽の色を、「Moon Color」を押すことで月の色をそれぞれ設定することができます。
![PLATEAU-Rrendering-SunMoonColor](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/b3e44a5a-d5ee-41bc-98e2-0aca272a216f)


### Fog Distanceの設定
「Fog Distance」のスライダーを調整することで、霧の濃さを調整することができます。
<img width="800" alt="スクリーンショット 2023-07-12 19 27 49" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/7e92bc04-3207-4215-9a1f-16c23cf0f61b">


### Weather Material Fadeの設定
「Weather material fade」スライダーを調整すると、自動生成されたテクスチャのmaterialを単色化することができます。
![PLATEAU-Rrendering-WeatherMaterialFade](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/b12e3580-b67c-4c37-a398-876f3b0a823b)



### Hide child objectsの設定

「Hide child objects」のチェックボックスを入れるとHierarchy viewの中におけるParentForGroupObjectsの子オブジェクトの表示・非表示を切り替えることができます。

<img width="398" alt="スクリーンショット 2023-07-12 19 28 17" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/cbe38fca-2080-4981-bb33-52d5ef13645f">
<img width="370" alt="スクリーンショット 2023-07-12 19 28 22" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/fc65743c-2653-4c4a-b0ae-9c70ff2df218">


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

<img width="1000" alt="スクリーンショット 2023-07-12 19 29 16" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/ca0c27e4-4768-4373-b805-7dfa514cb58d">


### 注意点
PLATEAUで定義されている3D都市モデルのLOD概念とUnity上でのLOD概念が異なることにご注意ください。
- PLATEAU LOD・・・LOD0が最下位であり、上位になればよりリッチな詳細度を持ったモデルになります。
- UnityのLOD・・・最も建物とカメラが近いLOD0がハイグラフィックになり、カメラが遠ざかるとLODが上がり、簡素な表現なります。

<img width="800" alt="スクリーンショット 2023-07-12 19 29 30" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/ba2b123e-f61a-4768-af18-e96eb056851b">



なお、カメラからの距離と対応するLODの設定は、各都市モデルの親オブジェクトのInspector Viewから調整できます。
<img width="1010" alt="スクリーンショット 2023-07-12 19 29 37" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/78747342-5e0b-4849-9cb3-8b20423e03ce">


## シーンの保存
テクスチャ生成などを行ったら右下にあるSceneビューの右下にある「シーンの保存」ボタンを押下し、保存してください。また、やり直したい場合は最後に保存した時点に戻ることも可能です。

<img width="193" alt="スクリーンショット 2023-07-12 19 29 48" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/6809d886-6885-4d52-b264-c04ce3c927cd">


## 関連API
Rendering Toolkit の開発において、Unity の以下のAPI を使用しています。本ツールをカスタマイズしながら作りたい場合にはこちらを参考にしてください。
1. [MeshRenderer](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/MeshRenderer.html)
2. [MeshFilter](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/MeshFilter.html)
3. [LOD](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/LOD.html)
4. [Skybox](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Skybox.html)
5. [HideFlags](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/HideFlags.html)
6. [Shaders](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Shader.html)
7. [VFX](https://docs.unity3d.com/Packages/com.unity.visualeffectgraph@7.0/api/UnityEngine.VFX.Utility.html)

