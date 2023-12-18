### PLATEAU SDK-Toolkits for Unity

# Sandbox Toolkit 利用マニュアル

PLATEAUの3D都市モデルを用いたゲーム開発、映像製作、シミュレーション実行などを支援します。  
乗り物、アバター、プロップ(*1)などの配置及び操作、トラックの設定機能などをGUI上で提供します。  <br>

(*1)プロップ: 空間の装飾用のモデルのこと。PLATEAU Sandbox Toolkitでは樹木や柵、ベンチなど、一般的に街の中で見られるものをビルトインプロップとして用意しています。

- [利用手順](#利用手順)
  - [全般](#全般)
  - [1. オブジェクトの配置機能](#1-オブジェクトの配置機能)
  - [2. トラック機能](#2-トラック機能)
    - [2-1. トラックの作成](#2-1-トラックの作成)
    - [2-2. トラック移動コンポーネント](#2-2-トラック移動コンポーネント)
  - [3. カメラインタラクション機能](#3-カメラインタラクション機能)
    - [3-1. 操作方法](#3-1-操作方法)
    - [3-2. カメラのデフォルト位置の変更](#3-2-カメラのデフォルト位置の変更)
- [関連API](#関連api)

# 利用手順

PLATEAU SDK-Toolkits for Unityのインストール後、上部のメニューより「PLATEAU」>「PLATEAU Toolkit」>「Sandbox Toolkit」を選択します。

<img width="372" alt= "plateau_toolkit_menu" src="../Documentation~/Sandbox Images/plateau_toolkit_menu.png">

Sandbox Toolkitのメインメニューが表示されます。

<img width="497" alt="sandbox_toolkit_menu" src="../Documentation~/Sandbox Images/sandbox_toolkit_menu.png">

## 全般

Sandbox Toolkitの各機能を利用するためには、**シーンに配置された3D都市モデルのオブジェクトに `Collider` (コライダー) コンポーネントを設定する必要があります。** <br/>
コライダーはUnity上で衝突判定を行うためのものです。コライダーの詳細については[Unityマニュアル](https://docs.unity3d.com/ja/2022.3/Manual/CollidersOverview.html)をご参照ください。 <br/>

## 1. オブジェクトの配置機能

### 配置ツールの開始方法

アバター、乗り物、プロップの配置メニューから「配置ツール」を使用することで、複数のオブジェクトの同時配置やトラックに沿ったオブジェクトの配置などを行うことができます。<br>
<img width="500" alt="placement_tool" src="../Documentation~/Sandbox Images/placement_tool.png">

配置ツールを起動すると以下の画像のようにボタンの表示が変わり、配置するオブジェクトを Sandbox Toolkit ウィンドウのアセットリストからクリックして選択することができます。
配置するオブジェクトを選択してシーン上でマウスを操作すると、マウス上に画像のようにカーソルと配置するオブジェクトのプレビューが表示され、後述する配置オプションに応じた操作によって選択したオブジェクトを配置することができます。<br>
<img width="300" alt="placement_example" src="../Documentation~/Sandbox Images/placement_example.png">

### 配置オプション
配置ツールを起動すると、シーンビューの右下に次のような配置ツール設定ウィンドウが表示されます。<br>
<img width="300" alt="placement_tool_window" src="../Documentation~/Sandbox Images/placement_tool_window.png">

| メニュー    | 内容       |
|:----- |:-------- |
| 配置位置 | オブジェクトの配置位置を「表面に配置」「トラックに沿って配置」から選択できます。 |
| 配置方法 | オブジェクトの配置操作を「クリック」「ブラシ」から選択できます。 |
| オブジェクトの向き | 配置オブジェクトの法線方向を「配置面の法線」「ワールド座標」から選択できます。 |

#### 配置位置
##### 配置位置 - 表面に配置

Unity のコライダーに沿ってオブジェクトを配置するモードです。「表面に配置」オプションを選択している場合は、PLATEAUモデルや配置したい3Dモデルにコライダーが設定されていない場合はオブジェクトを配置するための場所を決定することができないため、オブジェクトを配置できません。PLATEAUモデルのコライダー設定やその他のコライダー設定に関しては各種ドキュメントを参照して設定してください。<br>
<img width="450" alt="surface_placement" src="../Documentation~/Sandbox Images/surface_placement.gif">

##### 配置位置 - トラックに沿って配置
[2. トラック機能](#2-トラック機能)で設定したシーン上のトラックに沿ってオブジェクトを配置します。
「トラックに沿って配置」と配置方法「クリック」を選択している状態で配置を行うと、自動的に配置されたオブジェクトにトラックに沿って移動するコンポーネント（ `PlateauSandboxTrackMovement` ）がアタッチされます。<br>
<img width="450" alt="track_placement" src="../Documentation~/Sandbox Images/track_placement.gif">

#### 配置方法
##### 配置方法 - クリック
<img width="450" alt="click_placement" src="../Documentation~/Sandbox Images/click_placement.gif">

クリックすることで一つのオブジェクトを配置できます。ドラッグすることで、配置する際の向きを設定できます。
他のオブジェクトとの衝突を判定し、オブジェクトが重なっている場合はカーソルが赤色になりオブジェクトが配置できません。配置する際のオブジェクトのデフォルトの向きは「配置位置」の設定により異なります。<br>
|配置位置の設定| デフォルトの向き      |
|:----- |:-------- |
| 表面に配置 | 初期値はToolkitによって設定されている向きになっていますが、一度向きを設定するとその向きが保持され次回のオブジェクトのデフォルトの向きになります。 |
| トラックに沿って配置 | 常にトラックの進行方向に設定されます。 |

##### 配置方法 - ブラシ
<img width="450" alt="brush_placement" src="../Documentation~/Sandbox Images/brush_placement.gif">

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


#### オブジェクトの向き
##### オブジェクトの向き - 配置面の法線

配置方法の設定により向きが変化します。
|配置位置の設定| デフォルトの向き      |
|:----- |:-------- |
| 表面に配置 | コライダーの法線方向に配置されます。 |
| トラックに沿って配置 | トラックの法線方向に配置され、トラックが配置されている面の法線は使用されません。 |

<img width="200" alt="normal_orientation_surface_placement" src="../Documentation~/Sandbox Images/normal_orientation_surface_placement.png">
<img width="400" alt="normal_orientation_track_placement" src="../Documentation~/Sandbox Images/normal_orientation_track_placement.gif">
<br>

##### オブジェクトの向き - ワールド座標
配置オブジェクトは、常にワールド座標の上方向が法線ベクトルの正になる向きで配置されます。<br>
<img width="200" alt="world_surface_placement" src="../Documentation~/Sandbox Images/world_surface_placement.png">
<img width="400" alt="world_track_placement" src="../Documentation~/Sandbox Images/world_track_placement.gif">

## 2. トラック機能

### 2-1. トラックの作成

3D都市モデルの中にオブジェクト移動用の経路(トラック)を作成することができ、作成したトラックには `PlateauSandboxTrack` コンポーネントがアタッチされます。

トラックは複数の点とそれぞれの点の接続状態によって定義されます。そのため、トラックを作成する際はトラックを形成するための点をシーン上に配置していくことで任意の形状のトラックを作成することができます。Sandbox Toolkit のトラックのシステムは Unity Splines をベースに実装されているため、詳細は[公式ドキュメント(英語のみ)](https://docs.unity3d.com/Packages/com.unity.splines@2.5/manual/index.html) を参照してください。

「ツール」メニューの中で「新しいトラックを作成」ボタンを押下します。<br>
   
<img width="670" alt="track_creation_ui" src="../Documentation~/Sandbox Images/track_creation_ui.png">

「新しいトラックを作成」をクリックすることでトラック編集ツールを開始することができます。<br>
トラック編集ツールを起動した状態でシーンビューにマウスカーソルを移動すると、マウスカーソル上にトラックを形成するためのポイントのプレビューが表示され、クリックすることでポイントを配置することができます。<br>

<img width="796" alt="track_creation_start" src="../Documentation~/Sandbox Images/track_creation_start.png">

シーンビュー内において、トラックを引きたい地表上でクリックをすると、トラック生成のポイントが作成されます。
そのまま続けて地表上をクリックしていくと、ポイントが生成され、ポイントに合わせてトラックが生成されます。

<img width="498" alt="track_creation_points" src="../Documentation~/Sandbox Images/track_creation_points.png">

最後に始点と同じポイントをクリックすると、ループ可能なトラックが作成できます。Escキーを押すことでトラックの作成を終了します。

<img width="498" alt="track_creation_loop" src="../Documentation~/Sandbox Images/track_creation_loop.png">

作成したトラックへのアバター、乗り物の配置方法は[1-5. 共通配置ツール](#1-5-共通配置ツール)で後述します。

#### トラックの速度制限

トラックはトラック移動コンポーネント ( `PlateauSandboxTrackMovement` ) の制限速度を設定することができます。トラック移動コンポーネントはアバターや乗り物にアタッチすることで、トラック上を移動させることができる機能です。

詳細は [2. トラック移動コンポーネント](#2-トラック移動コンポーネント) を参照してください。

速度制限の設定は、`PlateauSandboxTrack` のインスペクターから設定するか、速度制限設定ツールからシーン上で一括設定することができます。

- インスペクターから設定する場合は `Has Speed Limit` にチェックボックスをいれて、制限速度（m/s）を `Speed Limit` に設定してください。

<img width="482" alt="plateau_sandbox_track_speed_limit" src="../Documentation~/Sandbox Images/plateau_sandbox_track_speed_limit.png">

- 速度制限設定ツールは PLATEAU Sandbox Toolkit ウィンドウのトラックタブから起動することができます。

<img width="407" alt="plateau_sandbox_track_speed_limit_tool" src="../Documentation~/Sandbox Images/plateau_sandbox_track_speed_limit_tool.png">

- 制限速度ツールを使用するとシーンの各トラック上に設定ウィンドウが表示され、このウィンドウから制限速度を設定することができます。

<img width="1006" alt="plateau_sandbox_track_speed_limit_setting" src="../Documentation~/Sandbox Images/plateau_sandbox_track_speed_limit_setting.png">


### 2-2. トラック移動コンポーネント

アバター ( `PlateauSandboxAvatar` ) と乗り物 ( `PlateauSandboxVehicle` ) はトラック移動コンポーネント ( `PlateauTrackMovement` ) をアタッチすることで、トラック上を移動させることができます。

#### トラック上の正規化位置

トラック移動コンポーネントでは移動対象のオブジェクトのトラック上の位置を正規化された値で表現し、ここでは「正規化位置」と呼びます。値の最大値はトラックに含まれる分岐などによって異なります。単純な直線（一筆書きができる形状）の場合、正規化位置0から1までの値で表現され、0が始点、1が終点を示します。例えば単純な直線の場合、正規化位置が0.25のときオブジェクトは、始点からトラックの長さの1/4 (=0.25)の長さを進んだ位置にあります。

#### トラック移動コンポーネントの設定方法

| フィールド                            | 説明                                                                                               |
| -------------------------------- | ------------------------------------------------------------------------------------------------ |
| `Track`                          | オブジェクトを移動させるトラックを設定します。                                                                          |
| `MaxVelocity`                    | オブジェクトの最大移動速(m/s) を設定します。オブジェクトは速度0から開始し、最大速度まで加速します。                                            |
| `Track Offset`                   | オブジェクトのトラックからのオフセットを指定できます。                                                                      |
| `Collision Detect Origin Offset` | 衝突判定の始点の相対位置を指定します。この位置からトラック上で `Min Collision Detect Distance` 分だけ進んだ場所までに別のオブジェクトが存在するかを判定します。 |
| `Collision Detect Radius`        | 衝突判定の大きさを指定します。衝突判定は球体上の当たり判定が別のオブジェクトに当たるかどうかで判定され、このフィールドではその球体の半径を指定することができます。                |
| `Collision Detect Height`        | 衝突判定の終点のトラックからの高さを指定します。                                                                         |
| `Min Collision Detect Distance`  | 衝突判定を行う距離をトラック上の正規化位置 の距離で指定します。                                                            |
| `Run On Awake`                   | プレイモード実行時に移動を開始するかどうか                                                                            |
| `Is Paused`                      | 一時停止中かどうか                                                                                        |
| `Random Start Point`             | トラック上のランダムな場所から移動開始させるかどうか                                                                       |
| `Loop On Opened Track`           | ループしていないトラックで終点まで到達した場合に、自動で開始点から移動を再開するかどうか                                                     |
| `トラック上の位置`                       | トラック上の正規化位置                                                                                   |

#### トラック移動コンポーネントの利用方法

トラック移動コンポーネントはエディターモードでは動作しません。プレイモードを実行することで移動を開始させることができます。

#### トラック移動コンポーネントの機能

##### 衝突判定

トラック移動コンポーネントによって移動するオブジェクトが別のオブジェクトに衝突する可能性がある場合に速度を下げて衝突を回避する仕組みが実装されています。衝突判定がある場合、速度は最大で0まで減少します。

設定方法にあるように、衝突判定はトラック移動コンポーネントのフィールドを調整することができます。

衝突判定を行う距離は速度に応じて変化します。速度が遅い場合は `Min Collision Detect Distance` が衝突判定距離の最小値として使用されますが、速度が早い場合はその速度に応じて判定する距離を大きくします。これは速度によって制動距離が異なるためです。

##### 移動アニメーション

アバター ( `PlateauSandboxAvatar` ) と乗り物 ( `PlateauSandboxVehicle` )にはトラック移動中の演出が実装されています。

- アバター
    - トラックの進行方向に向かってアバターを回転させます。
- 乗り物
    - 前輪と後輪がトラックに沿うように乗り物を移動させます。
    - 前輪が移動する方向に向かうように回転させます。
    - タイヤが移動速度に応じて回転します。

## 3. カメラインタラクション機能

カメラマネージャー ( `PlateauSandboxCameraManager` ) を作成することで、PLATEAU Sandbox Toolkit によって配置されたオブジェクトの視点に切り替えることができるようになります。

カメラマネージャーは Sandbox Toolkit ウィンドウから「カメラマネージャーを作成」ボタンを押下して作成することができます。<br>
<img width="400" alt="camera_manager_creation" src="../Documentation~/Sandbox Images/camera_manager_creation.png">

ヒエラルキービュー内に "PlateauSandboxCameraManager" が作成されます。<br>
<img width="400" alt="camera_manager_creation_result" src="../Documentation~/Sandbox Images/camera_manager_creation_result.png">

プレイモードを実行し、配置したアバターや乗り物をクリックすると、デフォルトではそのオブジェクトの一人称視点に遷移します。<br>
カメラインタラクション機能には一人称視点、三人称視点、三人称中心視点が用意されており、対象のオブジェクトを様々な視点から見ることができます。<br>

<img width="300" alt="left" src="../Documentation~/Sandbox Images/left.png">
<img width="300" alt="front" src="../Documentation~/Sandbox Images/front.png">
<img width="300" alt="right" src="../Documentation~/Sandbox Images/right.png">

なお、特定のエージェントに対しカメラインタラクション機能を設定したくない場合は、 `PlateauSandboxAvatar` の `Is Camera View Available` のチェックボックスを外します。<br>

<img width="400" alt="sandbox_camerainteracation_cameraviewenable" src="../Documentation~/Sandbox Images/sandbox_camerainteracation_cameraviewenable.png">

尚、キーボード入力によるカメラ操作の有効/無効の設定は "PlateauSandboxCameraManager" の `Enable Keywboard Camera Switch` のチェックボックスによって切り替えることが可能です。<br>
<img width="475" alt="sandbox_cameramanager_enablekeyboard" src="../Documentation~/Sandbox Images/sandbox_cameramanager_enablekeyboard.png">

### 3-1. 操作方法

#### 共通

| 操作      | 内容                         | 備考        |
|:------- |:-------------------------- |:--------- |
| マウス移動   | カメラの向き変更                   |           |
| 「1」キー押下 | カメラを一人称視点に変更する           | キー操作有効時のみ |
| 「2」キー押下 | カメラを三人称視点に変更する           | キー操作有効時のみ |
| 「3」キー押下 | カメラを三人称中心視点に変更する | キー操作有効時のみ |
| 「0」キー押下 | カメラインタラクションモードを終了する        | キー操作有効時のみ |

#### 一人称視点

オブジェクトの視点の位置からカメラ表示するモードです。<br>
<img width="600" alt="sandbox_camerainteraction_fps" src="../Documentation~/Sandbox Images/sandbox_camerainteraction_fps.png">

| 操作    | 内容       | 備考  |
|:----- |:-------- |:--- |
| マウス移動 | カメラの向き変更 |     |

#### 三人称視点

オブジェクトの後方からオブジェクトの視点の先に向けたカメラを表示するモードです。<br>

<img width="600" alt="sandbox_camerainteraction_tps" src="../Documentation~/Sandbox Images/sandbox_camerainteraction_tps.png">

| 操作              | 内容           | 備考  |
|:--------------- |:------------ |:--- |
| マウス移動           | カメラの向き変更     |     |
| マウススクロール        | カメラ視点の前後移動   |     |
| マウス中央を押しながらドラッグ | カメラ視点の上下左右移動 |     |

#### 三人称中心視点

対象のオブジェクトを中心に軌道上を移動することができるモードです。<br>

<img width="600" alt="sandbox_camerainteraction_around" src="../Documentation~/Sandbox Images/sandbox_camerainteraction_around.png">

| 操作       | 内容                   | 備考  |
|:-------- |:-------------------- |:--- |
| マウス移動    | エージェントを中心としたカメラの回転移動 |     |
| マウススクロール | 対象のオブジェクトとの距離の調整           |     |

### 3-2. カメラのデフォルト位置の変更

アバターや乗り物のコンポーネント ( `PlateauSandboxAvatar` 、 `PlateauSandboxVehicle` ) の `CameraTargetSettings` を調整することでカメラのデフォルトの位置などカメラに関する設定を変更することができます。
位置は対象のオブジェクトの原点からの相対座標で表現され、Xは横方向, Yは高さ, Zで前後方向の設定をすることができます。<br>

<img width="684" alt="sandbox_camerainteraction_offset" src="../Documentation~/Sandbox Images/sandbox_camerainteraction_offset.png">

| 設定項目                                 | 内容                      |
|:------------------------------------ |:----------------------- |
| FirstPersonViewPosition              | 一人称視点モードでのデフォルト位置       |
| ThirdPersonViewDefaultCameraPosition | 三人称モードでのデフォルト位置         |
| ThirdPersonOrbitInitialRotation      | 見回しモードの注視点デフォルト位置       |
| ThirdPersonOrbitOffset               | 見回しモードの注視点の原点からのオフセット   |
| ThirdPersonOrbitDefaultDistance      | 見回しモードの注視点からカメラへの距離の初期値 |

# 関連API

Sandbox Toolkitの開発において、Unity の以下のAPIを使用しています。本ツールをカスタマイズしながら作りたい場合にはこちらを参考にしてください。

1. [Splines](https://docs.unity3d.com/Packages/com.unity.splines@2.1/manual/index.html)
2. [PackageManager](https://docs.unity3d.com/ja/2021.3/Manual/class-PackageManager.html)
3. [Preferences](https://docs.unity3d.com/2021.3/Documentation/Manual/Preferences.html)
4. [UI Toolkit](https://docs.unity3d.com/2021.3/Documentation/Manual/UIElements.html)
5. [Raycasters](https://docs.unity3d.com/2021.3/Documentation/Manual/Raycasters.html)
6. [Collision](https://docs.unity3d.com/2021.3/Documentation/Manual/collision-section.html)
7. [Scenes](https://docs.unity3d.com/2021.3/Documentation/Manual/CreatingScenes.html)
