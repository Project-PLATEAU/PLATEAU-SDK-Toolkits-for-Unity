### PLATEAU-SDK-Toolkits-for-Unity
# Rendering Toolkit 利用マニュアル

PLATEAUの3D都市モデルのグラフィックスを向上させる処理を行います。  
環境設定、テクスチャの自動作成、LOD設定等の機能をGUI上で提供します。  

# 利用手順

PLATEAU-SDK-Toolkits-for-Unityのインストール後、上部のメニューより「PLATEAU」>「PLATEAU Toolkit」>「Rendering Toolkit」を選択します。

<img width="371" alt="スクリーンショット 2023-06-27 17 45 56" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/03b129e4-eed2-4096-8cf7-9679ae7652e0">

Rendering Toolkitのメインメニューが表示されます。

<img width="324" alt="スクリーンショット 2023-06-27 17 46 09" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/1bf3da51-c1ef-4056-9639-b7b8c6bd5002">



## 1. 環境システムの設定
「環境システムの設定」では、シーンの時間帯や天候などを変更し、3D都市モデルを使った環境シミュレーションを行うことができます。

### 時間の変更
「Time of Day」欄のスライダーを動かすと、表示時間帯を変更することができます。
<img width="500" alt="スクリーンショット 2023-06-27 17 56 09" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/c6bc6a65-9397-49f0-a195-2b7db44e003a">


### 天候の変更
「Snow」「Rain」「Cloud」バーを動かすことで天候を変更することが可能です。  
変更後にGameビューで表示を確認することができます。  
Sceneビューでも表示可能ですが、対象となるカメラの前方のみの表示になりますので、確認の際はご注意ください。

<img width="500" alt="スクリーンショット 2023-06-27 18 48 34" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/47700266-6162-403b-bf58-db5eb6025990">

### 太陽光・月光の色変更
「Sun Color」を押すことで太陽の色を、「Moon Color」を押すことで月の色をそれぞれ設定することができます。

<img width="500" alt="スクリーンショット 2023-06-27 18 49 09" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/91bfa98b-95f0-4da9-86b3-8fffdd23c60e">



### Fog Distanceの設定
「Fog Distance」のスライダーを調整することで、霧の濃さを調整することができます。
<img width="500" alt="スクリーンショット 2023-07-11 6 37 32" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/43137921-88b2-4784-ae48-df6659d137ae">

### Weather Material Fadeの設定
「Weather material fade」スライダーを調整すると、自動生成されたテクスチャのmaterialを単色化することができます。

<img width="400" alt="スクリーンショット 2023-07-11 13 16 11" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/68e57f2a-3e35-4b81-b74b-b4194497ea46">

<img width="400" alt="スクリーンショット 2023-07-11 13 16 24" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/24ee2ef6-ab5b-417b-a26a-8e71c59c0c91">


### Hide child objectsの設定

「Hide child objects」のチェックボックスを入れるとHierarchy viewの中におけるParentForGroupObjectsの子オブジェクトの表示・非表示を切り替えることができます。

<img width="400" alt="スクリーンショット 2023-07-11 11 52 26" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/010bd0f0-abb1-474b-988c-dd1142f535b3">


<img width="400" alt="スクリーンショット 2023-07-11 11 52 38" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/d9df0edc-d57b-4b1d-b79e-19e3277aef37">


## 2. 自動テクスチャの生成
「自動テクスチャの生成」では、3D都市モデルの建築物モデルに対してランダムにテクスチャを貼り付けることができます。  
既にテクスチャを持つLOD2建築物モデルに対しては、窓のライトのみ付与します。

1. Unityの「Hierarchy」ビューより対象となる建物のGameObjectを選択してください。

<img width="500" alt="スクリーンショット 2023-06-27 18 20 09" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/2c7221aa-0756-488e-b485-d79b1ad7eb89">

2. 「自動テクスチャの生成」メニューの中の「テクスチャ生成」ボタンを押下してください。

3. テクスチャ作成の確認画面が表示されます。問題なければ「はい」ボタンを押下してください。

<img width="500" alt="スクリーンショット 2023-06-27 18 21 43" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/f6adc2ee-28ee-4ab8-a011-04fca260cc5f">

4. テクスチャが自動的に生成され、モデルの見た目が変更されます。
<img width="500" alt="スクリーンショット 2023-06-27 18 24 18" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/4f906794-cb24-4959-9639-ab908e0538e1">

この状態で「環境システムの設定」メニューから「「Time of Day」を夜にすると、窓のライトが表示されます。（なお、現在は主に高さのある建物に対してのみライトの表示が適用されます。)

<img width="500" alt="スクリーンショット 2023-06-27 18 54 17" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/2135325b-f68a-48d2-8e37-3f7785c319cd">


### 注意点
テクスチャの自動生成後は3D都市モデルのHierarchyビューの構成が変わります。あらかじめご注意ください。

#### 変更前
PLATEAU SDKでダウンロードした直後は専用親GameObject（下記の場合は13100_tokyo23-ku_2022_citygml_1_2_op）に3D都市オブジェクトが格納されている。


<img width="500" alt="スクリーンショット 2023-06-30 8 11 09" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/58426be6-8b1d-46d7-b5b7-c4abad877b88">


#### 変更後
「ParentForGroupedObjects」と呼ばれるGameObjectに全てのモデルデータが移動する。

<img width="500" alt="スクリーンショット 2023-07-03 15 12 23" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/6b5981a9-1b1a-405b-b8de-689b4d4edea5">

### 窓の表示

「窓の表示切り替え」ボタンを押下すると、テクスチャに合わせた窓の表示もしくは非表示を切り替えることが可能です。  
この機能は、現時点ではLOD2建築物モデルのみが対象となります。


## 3. LODグループ生成
「LODグループ生成」ボタンを押すと、すべての3D都市モデルに対してUnityのLOD機能が設定されます。  
LODグループが生成されると、建物オブジェクトに対してのカメラの距離で表示されるグラフィクスが変化します。

<img width="500" alt="スクリーンショット 2023-07-11 21 18 01" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/26e1c951-e359-49af-9e9e-a62816dd8ddc">

### 注意点
PLATEAUで定義されている3D都市モデルのLOD概念とUnity上でのLOD概念が異なることにご注意ください。
- PLATEAU LOD・・・LOD0が最下位であり、上位になればよりリッチな詳細度を持ったモデルになります。
- UnityのLOD・・・最も建物とカメラが近いLOD0がハイグラフィックになり、カメラが遠ざかるとLODが上がり、簡素な表現なります。

<img width="250" alt="スクリーンショット 2023-07-11 21 14 52" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/40ab798a-f8af-4247-aaae-ac0dd7fe5da1">

<img width="250" alt="スクリーンショット 2023-07-11 21 14 58" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/a9ba5333-af24-48fc-8a16-db090da19e8a">

<img width="250" alt="スクリーンショット 2023-07-11 21 15 06" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/0b5aac90-d002-41f3-8692-a0f816b46319">



なお、カメラからの距離と対応するLODの設定は、各都市モデルの親オブジェクトのInspector Viewから調整できます。

<img width="1435" alt="スクリーンショット 2023-07-11 21 25 12" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/8f32fbb2-14f0-4d47-b8a4-7e614dbd89a3">


## シーンの保存
テクスチャ生成などを行ったら右下にあるSceneビューの右下にある「シーンの保存」ボタンを押下し、保存してください。また、やり直したい場合は最後に保存した時点に戻ることも可能です。

<img width="196" alt="スクリーンショット 2023-06-27 18 03 40" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/81bf706e-7902-41b9-afac-33cdbb301faf">

## 関連API
Rendering Toolkit の開発において、Unity の以下のAPI を使用しています。本ツールをカスタマイズしながら作りたい場合にはこちらを参考にしてください。
1. [MeshRenderer](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/MeshRenderer.html)
2. [MeshFilter](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/MeshFilter.html)
3. [LOD](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/LOD.html)
4. [Skybox](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Skybox.html)
5. [HideFlags](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/HideFlags.html)
6. [Shaders](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Shader.html)
7. [VFX](https://docs.unity3d.com/Packages/com.unity.visualeffectgraph@7.0/api/UnityEngine.VFX.Utility.html)

