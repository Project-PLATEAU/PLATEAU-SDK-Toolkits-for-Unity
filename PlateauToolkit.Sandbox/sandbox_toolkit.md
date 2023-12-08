### PLATEAU SDK-Toolkits for Unity

# Sandbox Toolkit 利用マニュアル

PLATEAUの3D都市モデルを用いたゲーム開発、映像製作、シミュレーション実行などを支援します。  
乗り物、アバター、プロップなどの配置及び操作、トラックの設定機能などをGUI上で提供します。  <br>
※プロップ: 空間の装飾用のモデルのこと。このToolkitでは樹木や柵、ベンチなど、街の中でよく見るものをプロップとして用意しています。

- [利用手順](#利用手順)
  
  * [全般](#全般)
  * [1. オブジェクトの配置機能](#1-オブジェクトの配置機能)
    * [1-1. トラックの作成](#1-1-トラックの作成)
    * [1-2. アバターの作成](#1-2-アバターの作成)
    * [1-3. 乗り物の作成](#1-3-乗り物の作成)
    * [1-4. プロップの作成](#1-4-プロップの作成)
    * [1-5. 共通配置ツール](#1-5-共通配置ツール)
  * [2. カメラインタラクション機能](#2-カメラインタラクション機能)
    * [2-1. 操作方法](#2-1-操作方法)
    * [2-2. カメラのデフォルト位置の変更](#2-2-デフォルトのカメラ位置変更)

- [関連API](#関連api)

# 利用手順

PLATEAU SDK-Toolkits for Unityのインストール後、上部のメニューより「PLATEAU」>「PLATEAU Toolkit」>「Sandbox Toolkit」を選択します。

<img width="372" alt= "PLATEAU Toolkit Menu" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/58356a9f-7b52-49ce-9d5a-bbe55b56be87">

Sandbox Toolkitのメインメニューが表示されます。

<img width="497" alt="Sandbox Toolkit Menu" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%202.png">

## 全般

Sandbox Toolkitの各機能を利用するためには、**シーンに配置された3D都市モデルのオブジェクトに `Collider` (コライダー) コンポーネントを設定する必要があります。** <br/>
コライダーはUnity上で衝突判定を行うためのものです。コライダーの詳細については[Unityマニュアル](https://docs.unity3d.com/ja/2022.3/Manual/CollidersOverview.html)をご参照ください。 <br/>

## 1. オブジェクトの配置機能

### 1-1. トラックの作成

3D都市モデルの中にオブジェクト移動用の経路(トラック)を作成することができます。
後述する乗り物の配置と組み合わせることで、トラックに沿って車両を走らせることが可能です。<br>

トラックは複数の点とそれぞれの点の接続状態によって定義されます。そのため、トラックを作成する際はトラックを形成するための点をシーン上に配置していくことで任意の形状のトラックを作成することができます。Sandbox Toolkit のトラックのシステムは Unity Splines をベースに実装されているため、詳細は[公式ドキュメント(英語のみ)](https://docs.unity3d.com/Packages/com.unity.splines@2.5/manual/index.html) を参照してください。

「ツール」メニューの中で「新しいトラックを作成」ボタンを押下します。<br>
   
   <img width="670" alt="Track Creation" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/54d30e78-bbf1-497b-8d29-8f59656cfbd5">

「新しいトラックを作成」をクリックすることでトラック編集ツールを開始することができます。<br>
トラック編集ツールを起動した状態でシーンビューにマウスカーソルを移動すると、マウスカーソル上にトラックを形成するためのポイントのプレビューが表示され、クリックすることでポイントを配置することができます。<br>

<img width="796" alt="スクリーンショット 2023-07-12 19 39 27" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/49029b7d-f4dc-49dc-a1e4-1a24cff34563">

シーンビュー内において、トラックを引きたい地表上でクリックをすると、トラック生成のポイントが作成されます。
そのまま続けて地表上をクリックしていくと、ポイントが生成され、ポイントに合わせてトラックが生成されます。

<img width="498" alt="スクリーンショット 2023-07-12 19 39 34" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/3f483c28-1573-4d89-8b67-571b2262568f">

最後に始点と同じポイントをクリックすると、ループ可能なトラックが作成できます。Escキーを押すことでトラックの作成を終了します。

<img width="498" alt="スクリーンショット 2023-07-12 19 39 40" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/b66cdcb3-0317-4acc-9951-ccb04652a0dd">

※作成したトラックへのアバター、乗り物の配置方法は[1-5. 共通配置ツール](#1-5-共通配置ツール)で後述します。


### トラックの速度制限

トラックには速度制限を設定することができます。
オブジェクトを配置されると、動きを制御するための`PlateauSandboxMovement`コンポーネントがアタッチされます。
また、作成したトラックには `PlateauSandboxTrack` コンポーネントがアタッチされます。
速度制限を設定する際には、`PlateauSandboxTrack` のインスペクターから設定するか、速度制限設定ツールからシーン上で一括設定することができます。

- インスペクターから設定する場合は `Has Speed Limit` にチェックボックスをいれて、制限速度（m/s）を `Speed Limit` に設定してください。

<img width="482" alt="スクリーンショット 2023-07-12 19 39 54" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/f4c13c46-dce7-46d3-b52d-1e54cb36c407">

- 速度制限設定ツールは PLATEAU Sandbox Toolkit ウィンドウのトラックタブから起動することができます。

<img width="407" alt="スクリーンショット 2023-07-12 19 39 59" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/bdc7a5a0-c714-4917-8544-5f6f8253a4e1">

- 制限速度ツールを使用するとシーンの各トラック上に設定ウィンドウが表示され、このウィンドウから制限速度を設定することができます。

<img width="1006" alt="スクリーンショット 2023-07-12 19 40 14" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/6f17b30f-1fdb-43a5-8d14-3ad136773be0">

### 1-2. アバターの作成

専用のメニューから人型のモデルを選択し、3D都市モデルの中に配置できます。

1. 「アセット」メニューの中で対象となるモデルをクリックし選択します。

<img width="497" alt="スクリーンショット 2023-07-12 19 40 26" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/d68047f8-de01-4061-b961-f757b3ca6db0">

2. 配置ツールを起動ボタンを押下し、シーンビューで配置したい場所をクリックするとオブジェクトを配置することができます。

<img width="797" alt="スクリーンショット 2023-07-12 19 40 34" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/043449a7-334f-4b3a-90cc-35f41a437913">

### 1-3. 乗り物の作成

専用のメニューから乗り物型のモデルを選択し、3D都市モデルの中に配置できます。

1. 「アセット」メニューの中で対象となるモデルをクリックし選択します。

<img width="498" alt="スクリーンショット 2023-07-12 19 40 39" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/09360166-5822-4fab-9a19-7e0fe4f8c771">

2. 配置ツールを起動ボタンを押下し、シーンビューで配置したい場所をクリックするとオブジェクトを配置することができます。

<img width="796" alt="スクリーンショット 2023-07-12 19 40 45" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/1def056b-84a4-4a9b-824b-2d3951c4027d">

### 1-4. プロップの作成

樹木や柵、ベンチなどのオブジェクト（Props）を選択して配置することができます。
専用のメニューから施設器具型のモデルを選択し、3D都市モデルの中に配置します。

1. 「アセット」メニューの中で対象となるモデルをクリックし選択します。

<img width="677" alt="スクリーンショット 2023-07-12 19 40 52" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/fac5e124-6d66-4f43-95f6-4ce8fd73a7c2">

2. 配置ツールを起動ボタンを押下し、シーンビューで配置したい場所をクリックするとオブジェクトを配置することができます。

<img width="798" alt="スクリーンショット 2023-07-12 19 41 00" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/a01a4ecc-8b85-486a-9159-126c584ee758">

### 1-5. 共通配置ツール
アバター、乗り物、プロップの配置メニューから「配置ツール」を使用することで、複数のオブジェクトの同時配置、トラックに沿ったオブジェクトの配置などを行うことができます。<br>
<img width="500" alt="Placement Tool" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2016.png">

共通配置ツールを起動するとこのようにボタンの表示が変わり、配置するオブジェクトをアセットリストからクリックして選択してシーン上に簡単にオブジェクトを配置できます。
配置ツールを起動している状態でシーン上でマウスを操作すると、マウス上に次のようにカーソルと配置するオブジェクトのプレビューが表示され、クリックすることで選択したオブジェクトを配置することができます。<br>
<img width="300" alt="Placement Example" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2017.png">

#### 配置オプション / 配置方法
配置ツールを起動すると、シーンビューの右下に次のような配置ツール設定ウィンドウが表示されます。<br>
<img width="300" alt="Placement Tool Window" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2018.png">

| メニュー    | 内容       |
|:----- |:-------- |
| 配置位置 | オブジェクトの配置位置を「表面に配置」「トラックに沿って配置」から選択できます。 |
| 配置方法 | オブジェクトの配置操作を「クリック」「ブラシ」から選択できます。 |
| オブジェクトの向き | 配置オブジェクトの法線方向を「配置面の法線」「ワールド座標」から選択できます。 |

##### 配置位置
**【表面に配置】** <br>
Unity のコライダーに沿ってオブジェクトを配置するモードです。「表面に配置」オプションを選択している場合は、PLATEAUモデルや配置したい3Dモデルにコライダーが設定されていない場合はオブジェクトを配置するための場所を決定することができないため、オブジェクトを配置できません。PLATEAUモデルのコライダー設定やその他のコライダー設定に関しては各種ドキュメントを参照して設定してください。<br>
<img width="450" alt="Surface Placement" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2019.gif">


** 【トラックに沿って配置】** <br>
[1-1. トラックの作成](#1-1-トラックの作成)で設定したシーン上のトラックに沿ってオブジェクトを配置します。
「トラックに沿って配置」と配置方法「クリック」を選択している状態で配置を行うと、自動的に配置されたオブジェクトにトラックに沿って移動するコンポーネントがアタッチされます。<br>
<img width="450" alt="Track Placement" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2020.gif">


##### 配置方法
**【クリック】** <br>
<img width="450" alt="Click Placement" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2021.gif">

クリックすることで一つのオブジェクトを配置できます。ドラッグすることで、配置する際の向きを設定できます。
他のオブジェクトとの衝突を判定し、オブジェクトが重なっている場合はカーソルが赤色になりオブジェクトが配置できません。配置する際のオブジェクトのデフォルトの向きは「配置位置」の設定により異なります。<br>
|配置位置の設定| デフォルトの向き      |
|:----- |:-------- |
| 表面に配置 | 初期値はToolkitによって設定されている向きになっていますが、一度向きを設定するとその向きが保持され次回のオブジェクトのデフォルトの向きになります。 |
| トラックに沿って配置 | 常にトラックの進行方向に設定されます。 |

**【ブラシ】** <br>
<img width="450" alt="Brush Placement" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2022.gif">

|設定項目| 概要      |
|:----- |:-------- |
| オブジェクトの回転 | 配置されるオブジェクトの向き(0～360度)を設定します。この角度は「オブジェクトの向き」を軸とした回転で定義されます。 |
| 配置数 | 一回のブラシ配置で配置されるオブジェクトの数を設定します。 |
| ブラシサイズ | ドラッグで連続配置を行う際の配置間隔を設定します。 |
| シード値固定 | 配置ごとにシード値を振りなおすかどうかを設定します。固定されていない場合は、配置ごとに新しいシード値が設定されるため、ブラシの形状が自動的に変化します。 |
| ブラシ乱数シード値 | ブラシの形状に現在使用されている乱数が設定されます。任意のシード値を設定することも可能です。 |

複数のオブジェクトを同時に配置することができます。クリックでカーソルが表示されている位置に同時に複数配置、もしくはドラッグで連続で複数配置することができます。<br>
ブラシでオブジェクトを配置する際に設置面の判定を行っており、Toolkitで設定されている範囲内に設置面がない場合はオブジェクトは設置されません。例えば、ビルの屋上にブラシで配置する際、配置される位置が屋上に収まらないオブジェクトは配置されません。
ブラシの形状は「ブラシサイズ」と乱数（シード値はブラシ乱数シード値」で設定される）で決まります。マウスの位置を中心にして、「ブラシサイズ」を最大距離としてランダムな距離とランダムな角度に「配置数」分だけ配置位置が選択されたものがブラシの形状になります。<br>


##### オブジェクトの向き

**【配置面の法線】** <br>
配置方法の設定により向きが変化します。
|配置位置の設定| デフォルトの向き      |
|:----- |:-------- |
| 表面に配置 | コライダーの法線方向に配置されます。 |
| トラックに沿って配置 | トラックの法線方向に配置され、トラックが配置されている面の法線は使用されません。 |<br>
<img width="200" alt="Normal Orientation - Surface Placement" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2023.png">
<img width="400" alt="Normal Orientation - Track Placement" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2024.gif">

**【ワールド座標】** <br>
配置オブジェクトは、常にワールド座標の上方向が法線ベクトルの正になる向きで配置されます。<br>
<img width="200" alt="Normal Orientation - Surface Placement" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2025.png">
<img width="400" alt="Normal Orientation - Track Placement" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2026.gif">


# 2. カメラインタラクション機能

カメラマネージャーを作成することで、PLATEAU Sandbox Toolkit によって配置されたオブジェクトの視点に切り替えることができるようになります。

カメラマネージャーはPLATEAU Sandbox Toolkit ウィンドウから「カメラマネージャーを作成」ボタンを押下して作成することができます。<br>
<img width="400" alt="Camera Manager Creation" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2027.png">

ヒエラルキービュー内に "PlateauSandboxCameraManager" が作成されます。<br>
<img width="400" alt="Camera Manager Creation Result" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2028.png">

プレイモードにおいて配置したアバターや乗り物をクリックすると、そのオブジェクトの一人称視点に遷移します。<br>
カメラインタラクション機能には一人称視点、三人称視点、周囲を見回すモードが用意されており、マウス操作によって、さまざまな方向を見回すことが可能です。<br>

<img width="300" alt="left" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2029.png">
<img width="300" alt="front" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2030.png">
<img width="300" alt="right" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2031.png">

なお、特定のエージェントに対しカメラインタラクション機能を設定したくない場合は、 `PlateauSandboxAvatar` の `Is Camera View Available` のチェックボックスをオフにします。<br>

<img width="400" alt="sandbox_camerainteracation_cameraviewenable" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2032.png">

尚、キーボード入力によるカメラ操作の有効/無効の設定は "PlateauSandboxCameraManager" の `Enable Keywboard Camera Switch` のチェックボックスによって切り替えることが可能です。<br>
<img width="475" alt="sandbox_cameramanager_enablekeyboard" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2033.png">

## 2-1. 操作方法

### 共通

| 操作      | 内容                         | 備考        |
|:------- |:-------------------------- |:--------- |
| マウス移動   | カメラの向き変更                   |           |
| 「1」キー押下 | 視点を一人称視点モードに変更する           | キー操作有効時のみ |
| 「2」キー押下 | 視点を三人称視点モードに変更する           | キー操作有効時のみ |
| 「3」キー押下 | 注視点をエージェント本体にした見回しモードに変更する | キー操作有効時のみ |
| 「0」キー押下 | カメラインタラクションモードを終了する        | キー操作有効時のみ |

### 一人称視点モード

オブジェクトの視点の位置からカメラ表示するモードです。<br>
<img width="600" alt="sandbox_camerainteraction_fps" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2034.png">

| 操作    | 内容       | 備考  |
|:----- |:-------- |:--- |
| マウス移動 | カメラの向き変更 |     |

### 三人称視点モード

オブジェクトの後方からオブジェクトの視点の先に向けたカメラを表示するモードです。<br>

<img width="600" alt="sandbox_camerainteraction_tps" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2035.png">

| 操作              | 内容           | 備考  |
|:--------------- |:------------ |:--- |
| マウス移動           | カメラの向き変更     |     |
| マウススクロール        | カメラ視点の前後移動   |     |
| マウス中央を押しながらドラッグ | カメラ視点の上下左右移動 |     |

### 見回しモード

対象のオブジェクトを注視点として、見回すことのできるモードです。<br>

<img width="600" alt="sandbox_camerainteraction_around" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2036.png">

| 操作       | 内容                   | 備考  |
|:-------- |:-------------------- |:--- |
| マウス移動    | エージェントを中心としたカメラの回転移動 |     |
| マウススクロール | カメラ視点の前後移動           |     |

## 2-2. カメラのデフォルト位置の変更

アバターや乗り物のコンポーネント ( `PlateauSandboxAvatar` 、 `PlateauSandboxVehicle` ) の `CameraTargetSettings` を調整することでカメラのデフォルトの位置などカメラに関する設定を変更することができます。
位置は対象のオブジェクトの原点からの相対座標で表現され、Xは横方向, Yは高さ, Zで前後方向の設定をすることができます。<br>

<img width="684" alt="sandbox_camerainteraction_offset" src="https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity/blob/main/PlateauToolkit.Sandbox/SandboxReadmeImages/Sandbox%2037.png">

| 設定項目                                 | 内容                      |
|:------------------------------------ |:----------------------- |
| FirstPersonViewPosition              | 一人称視点モードでのデフォルト位置       |
| ThirdPersonViewDefaultCameraPosition | 三人称モードでのデフォルト位置         |
| ThirdPersonOrbitInitialRotation      | 見回しモードの注視点デフォルト位置       |
| ThirdPersonOrbitOffset               | 見回しモードの注視点の原点からのオフセット   |
| ThirdPersonOrbitDefaultDistance      | 見回しモードの注視点からカメラへの距離の初期値 |

## 関連API

Sandbox Toolkitの開発において、Unity の以下のAPIを使用しています。本ツールをカスタマイズしながら作りたい場合にはこちらを参考にしてください。

1. [Splines](https://docs.unity3d.com/Packages/com.unity.splines@2.1/manual/index.html)
2. [PackageManager](https://docs.unity3d.com/ja/2021.3/Manual/class-PackageManager.html)
3. [Preferences](https://docs.unity3d.com/2021.3/Documentation/Manual/Preferences.html)
4. [UI Toolkit](https://docs.unity3d.com/2021.3/Documentation/Manual/UIElements.html)
5. [Raycasters](https://docs.unity3d.com/2021.3/Documentation/Manual/Raycasters.html)
6. [Collision](https://docs.unity3d.com/2021.3/Documentation/Manual/collision-section.html)
7. [Scenes](https://docs.unity3d.com/2021.3/Documentation/Manual/CreatingScenes.html)
