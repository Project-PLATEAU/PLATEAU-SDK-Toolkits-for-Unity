### PLATEAU SDK-Toolkits for Unity

# Sandbox Toolkit 利用マニュアル

PLATEAUの3D都市モデルを用いたゲーム開発、映像製作、シミュレーション実行などを支援します。  
乗り物、アバター、建築物、広告、標識・標示などのSandboxアセットの配置及び操作、トラックの設定機能などをGUI上で提供します。

- [1. Sandbox Toolkitの利用に必要な設定](#1-sandbox-toolkitの利用に必要な設定)
- [2. Sandbox Toolkitの起動](#2-sandbox-toolkitの起動)
- [3. UI](#3-ui)
- [4. Sandboxアセット](#4-sandboxアセット)
  - [4-1. 種類](#4-1-種類)
  - [4-2 ビルトインアセットのインポート](#4-2-ビルトインアセットのインポート)
  - [4-3. 新規作成](#4-3-新規作成)
  - [4-4. フィルタリング表示](#4-4-フィルタリング表示)
- [5. アセット配置機能](#5-アセット配置機能)
  - [5-1. 配置ツールの開始方法](#5-1-配置ツールの開始方法)
  - [5-2. 配置オプション](#5-2-配置オプション)
- [6. アセット一括配置機能](#6-アセット一括配置機能)
  - [6-1. シェープファイルの読み込み](#6-1-シェープファイルの読み込み)
  - [6-2. CSVファイルの読み込み](#6-2-csvファイルの読み込み)
  - [6-3. 配置するアセットの選択](#6-3-配置するアセットの選択)
  - [6-4. アセットを一括配置](#6-4-アセットを一括配置)
- [7. トラック機能](#7-トラック機能)
  - [7-1. トラックの作成](#7-1-トラックの作成)
  - [7-2. トラック移動コンポーネント](#7-2-トラック移動コンポーネント)
  - [7-3. トラックに沿ってオブジェクトを生成する機能](#7-3-トラックに沿ってオブジェクトを生成する機能)
- [8. 配置したアセットの編集機能](#8-配置したアセットの編集機能)
  - [8-1. 広告変更機能](#8-1-広告変更機能)
  - [8-2. 建築物カスタマイズ機能](#8-2-建築物カスタマイズ機能)
- [9. カメラインタラクション機能](#9-カメラインタラクション機能)
  - [9-1. 操作方法](#9-1-操作方法)
  - [9-2. カメラのデフォルト位置の変更](#9-2-カメラのデフォルト位置の変更)
- [関連API](#関連api)

# 1. Sandbox Toolkitの利用に必要な設定

Sandbox Toolkitの各機能を利用するためには、**シーンに配置された3D都市モデルのオブジェクトに `Collider` (コライダー) コンポーネントを設定する必要があります。** <br/>
コライダーはUnity上で衝突判定を行うためのものです。コライダーの詳細については[Unityマニュアル](https://docs.unity3d.com/ja/2022.3/Manual/CollidersOverview.html)をご参照ください。 <br/>

# 2. Sandbox Toolkitの起動

PLATEAU SDK-Toolkits for Unityのインストール後、上部のメニューより「PLATEAU」>「PLATEAU Toolkit」>「Sandbox Toolkit」を選択します。

<img width="400" alt= "plateau_toolkit_menu" src="../Documentation~/Sandbox Images/plateau_toolkit_menu.png">

Sandbox Toolkitのメインメニューが表示されます。

<img width="500" alt="sandbox_toolkit_menu" src="../Documentation~/Sandbox Images/sandbox_toolkit_menu.png">

# 3. UI

メインメニュー上部の「トラック」「アセット配置」「一括配置」のボタンをクリックすると、それぞれ「トラック機能」「アセット配置機能」「アセット一括配置機能」のメニュー表示に切り替わります。

<img width="1200" alt="ui_tool_switcher" src="../Documentation~/Sandbox Images/ui_switch.png">

# 4. Sandboxアセット

Sandbox Toolkitでは、シーン上に配置するためのアセット群を提供しており、これらをSandboxアセットと呼んでいます。

## 4-1. 種類

Sandboxアセットは次の8種類に分類されます。
- 人
- 車両
- 建築物
- 街路樹・植生
- 広告物
- 路上設備
- 標識・標示
- その他

「アセット配置機能」と「アセット一括配置機能」では、種類を示すアイコンをクリックすると、それぞれの種類のSandboxアセットがリスト表示されます。

- 人アセットのリスト表示
<img width="500" alt="sandbox_asset_human" src="../Documentation~/Sandbox Images/sandbox_asset_human.png">

- 車両アセットのリスト表示
<img width="500" alt="sandbox_asset_vehicle" src="../Documentation~/Sandbox Images/sandbox_asset_vehicle.png">

> [!WARNING]
> 古いバージョンのUnity Editorバージョンを使用していると、配置オブジェクトのリスト表示に不具合が生じることがあります。<br>
> Unity 2022.3.25以上を使用してください。

> [!WARNING]
> 既知の課題として、アセットのサムネイル画像が正しく表示されない場合があります。
> <img width="500" alt="hdrp_asset_thumbnail" src="../Documentation~/Sandbox Images/hdrp_asset_thumbnail.png">

## 4-2 ビルトインアセットのインポート

`アセット配置`をクリックし、メニュー下部の`ビルトインアセットをインポート`ボタンをクリックします。

<img width="500" alt="builtin_asset_import" src="../Documentation~/Sandbox Images/builtin_asset_import.png">

「ビルトインアセットをインポートしますか？」と表示されたダイアログウィンドウが開きます。`インポート`を選択するとSandbox Toolkitが提供するビルトインアセットをプロジェクトにインポートできます。

<img width="500" alt="builtin_asset_import_dialogwindow" src="../Documentation~/Sandbox Images/builtin_asset_import_dialogwindow.png">

## 4-3. 新規作成

ビルトインアセットとは別に、ユーザー独自の3Dモデルを使用してSandboxアセットを作成することができます。

1. `アセット配置`をクリックし、メニュー下部の`アセットを作成`ボタンをクリックすると、`PLATEAU プレハブ作成` のウィンドウが表示されます。

<img width="500" alt="asset_create_attach" src="../Documentation~/Sandbox Images/asset_create_open.png">

2. `Base Object`フィールドにシーンにあるオブジェクトをドラッグアンドドロップまたはオブジェクト選択ウィンドウから選択することで、Sandboxアセットに追加したいオブジェクトを選択します。

<img width="700" alt="asset_create_attach" src="../Documentation~/Sandbox Images/asset_create_attach.png">

3. `Type`フィールドにて、作成するSandboxアセットの種類を選択します。

- Human：人アセット
- Vehicle：車両アセット
- Building：建築物アセット
- Plant：街路樹・植生アセット
- Advertisement：広告物アセット
- Street Furniture：路上設備アセット
- Sign：標識・標示アセット
- Miscellaneous：その他アセット

<img width="500" alt="asset_create_type" src="../Documentation~/Sandbox Images/asset_create_type.png">

4. `作成`ボタンをクリックすると、プロジェクトの`Assets`のフォルダが開かれます。`Assets`以下の任意の場所を指定してアセットを保存してください。

<img width="500" alt="asset_create_button" src="../Documentation~/Sandbox Images/asset_create_button.png">

## 4-4. フィルタリング表示

「アセット配置機能」と「アセット一括配置機能」では、ビルトインアセットとユーザーが作成したアセットをフィルタリング表示できます。
- `全て`：ビルトインアセットとユーザーが作成したアセットの両方を表示します。
- `ユーザー`：ユーザーが作成したアセットのみを表示します。
- `ビルトイン`：ビルトインアセットのみを表示します。

<img width="500" alt="asset_filter" src="../Documentation~/Sandbox Images/asset_filter.png">

# 5. アセット配置機能

## 5-1. 配置ツールの開始方法
アセット配置メニューから「配置ツール」を使用することで、複数のオブジェクトの同時配置やトラックに沿ったオブジェクトの配置などを行うことができます。<br>
<img width="500" alt="placement_tool" src="../Documentation~/Sandbox Images/placement_tool.png">

配置ツールを起動すると以上の画像のようにボタンの表示が変わり、配置するオブジェクトを Sandbox Toolkit ウィンドウのアセットリストからクリックして選択することができます。
配置するオブジェクトを選択してシーン上でマウスを操作すると、マウス上に画像のようにカーソルと配置するオブジェクトのプレビューが表示され、後述する配置オプションに応じた操作によって選択したオブジェクトを配置することができます。<br>
<img width="300" alt="placement_example" src="../Documentation~/Sandbox Images/placement_example.png">

> [!WARNING]
> カーソルがコライダーの設定されていない場所を指している場合や、指定した場所にオブジェクトを配置すると他のオブジェクトと重なってしまう場合、カーソルは赤色に変わり、クリックしてもオブジェクトを配置することができません。<br>
> <img width="300" alt="objects_overlapping" src="../Documentation~/Sandbox Images/objects_overlapping.png"><br>

> [!TIP]
> 建築物Sandboxアセットの表面には他のアセットを配置することができます。<br>
> <img width="300" alt="asset_cannot_be_placed_to_building" src="../Documentation~/Sandbox Images/asset_placed_to_building.png">

## 5-2. 配置オプション
配置ツールを起動すると、シーンビューの右下に次のような配置ツール設定ウィンドウが表示されます。<br>
<img width="300" alt="placement_tool_window" src="../Documentation~/Sandbox Images/placement_tool_window.png">

| メニュー    | 内容       |
|:----- |:-------- |
| 配置位置 | オブジェクトの配置位置を「表面に配置」「トラックに沿って配置」から選択できます。 |
| 配置方法 | オブジェクトの配置操作を「クリック」「ブラシ」から選択できます。 |
| オブジェクトの向き | 配置オブジェクトの法線方向を「配置面の法線」「ワールド座標」から選択できます。 |

#### 配置位置
##### 配置位置 - 表面に配置
Unity のコライダーに沿ってオブジェクトを配置するモードです。「表面に配置」オプションを選択した場合でも、PLATEAUモデルや配置したい3Dモデルにコライダーが設定されていない場合はオブジェクトを配置するための場所を決定することができないため、オブジェクトを配置できません。PLATEAUモデルのコライダー設定やその他のコライダー設定に関しては各種ドキュメントを参照して設定してください。<br>
<img width="450" alt="surface_placement" src="../Documentation~/Sandbox Images/surface_placement.gif">

##### 配置位置 - トラックに沿って配置
[7. トラック機能](#7-トラック機能)で設定したシーン上のトラックに沿ってオブジェクトを配置します。
「トラックに沿って配置」と配置方法「クリック」を選択している状態で配置を行うと、自動的に配置されたオブジェクトにトラックに沿って移動するコンポーネント（ `PlateauSandboxTrackMovement` ）がアタッチされます。<br>
<img width="450" alt="track_placement" src="../Documentation~/Sandbox Images/track_placement.gif">

#### 配置方法
##### 配置方法 - クリック
<img width="450" alt="click_placement" src="../Documentation~/Sandbox Images/click_placement.gif">

クリックすることで一つのオブジェクトを配置できます。
また、クリックにて配置する際にドラッグをすることで、配置する際の正面方向を変更できます。

##### 配置方法 - ブラシ
<img width="450" alt="brush_placement" src="../Documentation~/Sandbox Images/brush_placement.gif">

|設定項目| 概要      |
|:----- |:-------- |
| オブジェクトの回転 | 配置されるオブジェクトの向き(0～360度)を設定します。この角度は「オブジェクトの向き」を軸とした回転で定義されます。 |
| 配置数 | 一回のブラシ配置で配置されるオブジェクトの数を設定します。 |
| ブラシサイズ | ドラッグで連続配置を行う際の配置間隔を設定します。 |
| シード値固定 | 配置ごとにシード値を振りなおすかどうかを設定します。固定されていない場合は、配置ごとに新しいシード値が設定されるため、ブラシの形状が自動的に変化します。 |
| ブラシ乱数シード値 | ブラシの形状に現在使用されている乱数が設定されます。任意のシード値を設定することもできます。 |

複数のオブジェクトを同時に配置することができます。クリックでカーソルが表示されている位置に同時に複数配置、もしくはドラッグで連続で複数配置することができます。<br>
ブラシでオブジェクトを配置する際に設置面の判定を行っており、Toolkitで設定されている範囲内に設置面がない場合はオブジェクトは設置されません。例えば、ビルの屋上にブラシで配置する際、配置される位置が屋上に収まらないオブジェクトは配置されません。
ブラシの形状は「ブラシサイズ」と乱数（シード値はブラシ乱数シード値」で設定される）で決まります。マウスの位置を中心にして、「ブラシサイズ」を最大距離としてランダムな距離とランダムな角度に「配置数」分だけ配置位置が選択されたものがブラシの形状になります。<br>

#### オブジェクトの向き

配置する際のオブジェクトのデフォルトの正面方向は「配置位置」の設定により異なります。
|「配置位置」の設定|オブジェクトのデフォルトの正面方向|
|:----- |:-------- |
| 表面に配置 | 初期値はToolkitによって設定されている向きになっていますが、一度向きを設定するとその向きが保持され次回のオブジェクトのデフォルトの向きになります。 |
| トラックに沿って配置 | 常にトラックの進行方向に設定されます。 |

##### オブジェクトの向き - 配置面の法線

「配置位置」の設定に応じて、どのオブジェクトを対象に法線を取得するかが変化します。
|「配置位置」の設定|オブジェクトの法線方向|
|:----- |:-------- |
| 表面に配置 | オブジェクトはコライダーの法線方向に配置されます。 |
| トラックに沿って配置 | オブジェクトはトラックの法線方向に配置されます。トラックが配置されている面の法線は使用されません。 |

<img width="200" alt="normal_orientation_surface_placement" src="../Documentation~/Sandbox Images/normal_orientation_surface_placement.png">

↑「表面に配置」を選択したときの例 <br>
<img width="400" alt="normal_orientation_track_placement" src="../Documentation~/Sandbox Images/normal_orientation_track_placement.gif">

↑「トラックに沿って配置」を選択したときの例 <br>

> [!NOTE]
> 地面に配置するアセットと壁に配置するアセットでは配置時の法線の計算方法が異なります。<br>
> 各アセットには`PlateauSandbox〇〇`というコンポーネント（`〇〇`はアセットの種類ごとに異なります）がアタッチされており、`Ground Placement Direction`フィールドに設定されている値により地面に配置するか壁に配置するかを判別しています。
> 地面に配置するアセットには`Vertical`が、壁に配置するアセットには`Horizontal`が設定されています。
> <img width="600" alt="asset_place_to_wall" src="../Documentation~/Sandbox Images/asset_place_to_wall.png">

<br>

##### オブジェクトの向き - ワールド座標
配置オブジェクトは、常にワールド座標の上方向が法線ベクトルの正になる向きで配置されます。<br>
<img width="200" alt="world_surface_placement" src="../Documentation~/Sandbox Images/world_surface_placement.png">
<img width="400" alt="world_track_placement" src="../Documentation~/Sandbox Images/world_track_placement.gif">

> [!TIP]
> v2.0.0-alphaのToolkitでは、複数のアセットをぴったり揃えて配置する機能は提供されていません。こうした場合には、Unityの頂点スナップ機能を利用することで、以下の動画のように効率的に配置することができます。詳細については、[公式ドキュメント](https://docs.unity3d.com/ja/2022.3/Manual/PositioningGameObjects.html#:~:text=%E3%83%89%E3%83%A9%E3%83%83%E3%82%B0%E3%81%97%E3%81%BE%E3%81%99%E3%80%82-,%E9%A0%82%E7%82%B9%E3%82%B9%E3%83%8A%E3%83%83%E3%83%97,-%E9%A0%82%E7%82%B9%E3%82%B9%E3%83%8A%E3%83%83%E3%83%97%20%E3%82%92)をご参照ください。<br>
> <img width="400" alt="asset_vertex_snap" src="../Documentation~/Sandbox Images/asset_vertex_snap.gif">

# 6. アセット一括配置機能

`一括配置` のメニューを選択し、 CSVファイルやシェープファイルを読み込むことで、一括でアセットを配置できます。

<img width="600" alt="sandbox_bulk_place" src="../Documentation~/Sandbox Images/bulk_place_ui.png">

## 6-1. シェープファイルの読み込み

緯度、経度情報を持つシェープファイルを読み込むことができます。

シェープファイルは座標系が投影座標系（平面直角座標系）であることを確認してください。 座標系の変更は、QGISなどのGISツールを使用して変更を行ってください。

[参考URL]
  - [【QGIS 3】座標参照系（CRS）を変換する方法（地理座標系：緯度経度・投影座標系：XY）](https://www.inokumaaranuu.com/zahyousanshoukeiwxchange/)

また、シェープファイルを読み込むためには、シェープファイルと同じ階層に属性情報を持つDBFファイル(.dbf)が存在している必要があります。

<img width="300" alt="sandbox_bulk_place" src="../Documentation~/Sandbox Images/bulk_place_shape_file.png">

`SHP、CSVファイルを読み込む` ボタンを押下し、シェープファイル(.shp)を選択することで読み込むことができます。

読み込みが完了するとボタンの表示が以下のように変更されます。再度ボタンを押すと読み込みがキャンセルされます。

<img width="600" alt="sandbox_bulk_place" src="../Documentation~/Sandbox Images/bulk_place_shape_file_load.png">

#### シェープファイルのアセット種別の選択

シェープファイルに紐づけされている属性情報からアセット種別に指定する項目を選択します。

<img width="600" alt="sandbox_bulk_place" src="../Documentation~/Sandbox Images/bulk_place_shape_file_asset_select.png">

## 6-2. CSVファイルの読み込み

CSVファイルはテンプレートを使用して作成できます。テンプレートは `CSVテンプレートの生成` ボタンから任意の場所にCSVファイルを保存してください。

CSVファイルには `緯度` `経度` `高さ` `アセット種別` の情報が必要です。エクセル等の表計算ソフトを使用してCSVファイルを編集してデータを入力してください。

<img width="600" alt="sandbox_bulk_place" src="../Documentation~/Sandbox Images/bulk_place_csv_template.png">

#### CSVファイルに入力する値

`緯度` `経度` `高さ` に入力する値については、以下のURLを参考に入力してください。

[参考URL]
- [TOPIC 3｜3D都市モデルデータの基本[4/4]｜CityGMLの座標・高さとデータ変換](https://www.mlit.go.jp/plateau/learning/tpc03-4/)

CSVファイルに入力された高さ情報はそのままUnity上でのY座標として配置されます。

<img width="1000" alt="sandbox_bulk_place" src="../Documentation~/Sandbox Images/bulk_place_csv_height_sample.png">

都市モデルインポート時に原点の高さを0としてインポートする場合、3D都市モデルは「東京湾平均海面を基準とする標高」に配置されます。そのため、CSVには配置したい標高を記載すると、意図通りの場所に配置できます。
国土地理院の[標高を求めるプログラム](https://maps.gsi.go.jp/development/elevation.html) を使えば指定した緯度経度における標高値を求めることができます。

<img width="600" alt="sandbox_bulk_place" src="../Documentation~/Sandbox Images/bulk_place_csv_height_import.png">

<img width="1000" alt="sandbox_bulk_place" src="../Documentation~/Sandbox Images/bulk_place_csv_height.png">

#### CSVファイルの読み込み

編集して保存したCSVファイルを `SHP、CSVファイルを読み込む` ボタンから選択し読み込むことができます。

読み込みが完了するとボタンの表示が以下のように変更されます。再度ボタンを押すと読み込みがキャンセルされます。

<img width="600" alt="sandbox_bulk_place" src="../Documentation~/Sandbox Images/bulk_place_csv_loaded.png">

#### CSVファイルのアセットの配置高さの設定

`アセットの配置高さ` の設定で `ファイルで指定した高さに配置` を選択していると、CSVファイルに記載されている高さ情報を使用してアセットを配置します。

`地面に配置` を選択することで、高さ情報を無視して地面に配置することができます。ただし `地面に配置` の場合、指定した緯度、経度にコライダーが存在しない場合はアセットは配置されません。

<img width="600" alt="sandbox_bulk_place" src="../Documentation~/Sandbox Images/bulk_place_csv_asset_height.png">

#### CSVファイルの属性列の選択

属性列の選択ができます。 `緯度` `経度` `高さ` `アセット種別` に紐づけたい属性列を選択してください。

<img width="600" alt="sandbox_bulk_place" src="../Documentation~/Sandbox Images/bulk_place_csv_field.png">

## 6-3. 配置するアセットの選択

アセットの種別ごとに任意のプレハブを選択できます。

アセット種別の一覧から一つを選択すると、選択したアセット種別が選択された状態になります。選択状態を維持したままアセット一覧からアセットを選ぶと、選択されたアセット名が一覧のプレハブ名に表示されます。

再度同じアセットを選択することで、選択を解除することができます。

<img width="600" alt="sandbox_bulk_place" src="../Documentation~/Sandbox Images/bulk_place_asset_select.gif">

## 6-4. アセットを一括配置

事前に `PLATEAU SDK` から3D都市モデルを読み込んでください。3D都市モデルが読み込まれていない場合、アセットは配置されません。

`アセットを配置` ボタンを押下することでアセットを一括配置できます。

<img width="1200" alt="sandbox_bulk_place" src="../Documentation~/Sandbox Images/bulk_place_asset_place.gif">

読み込んでいるシェープファイル、CSVファイルの変更や、属性列の変更を行う場合は `戻る` ボタンより前の画面に戻り、変更を行ってください。

#### 配置の結果

アセットの一括配置が完了すると、ダイアログにて結果が表示されます。また、コンソールログを確認することで詳細を確認できます。

- 全てのアセットの配置に成功<br>
<img width="300" alt="sandbox_bulk_place" src="../Documentation~/Sandbox Images/bulk_place_result_all_success.png">

- 一部のアセットの配置に失敗<br>
<img width="300" alt="sandbox_bulk_place" src="../Documentation~/Sandbox Images/bulk_place_result_failed.png">

- 全てのアセットの配置に失敗<br>
<img width="300" alt="sandbox_bulk_place" src="../Documentation~/Sandbox Images/bulk_place_result_all_failed.png">

> [!TIP]
> 「地面に配置」を選択している場合、配置する緯度・経度にSandboxアセットや3D都市モデルが存在しても、モデルを無視して地面に配置されます。<br>
> <img width="400" alt="sandbox_bulk_place" src="../Documentation~/Sandbox Images/bulk_place_ground_place.png">

# 7. トラック機能

## 7-1. トラックの作成

3D都市モデルの中にオブジェクト移動用の経路(トラック)を作成することができ、作成したトラックには `PlateauSandboxTrack` コンポーネントがアタッチされます。

トラックは複数の点とそれぞれの点の接続状態によって定義されます。そのため、トラックを作成する際はトラックを形成するための点をシーン上に配置していくことで任意の形状のトラックを作成することができます。Sandbox Toolkit のトラックのシステムは Unity Splines をベースに実装されているため、詳細は[公式ドキュメント(英語のみ)](https://docs.unity3d.com/Packages/com.unity.splines@2.5/manual/index.html) を参照してください。

「ツール」メニューの中で「新しいトラックを作成」ボタンを押下します。<br>
   
<img width="700" alt="track_creation_ui" src="../Documentation~/Sandbox Images/track_creation_ui.png">

「新しいトラックを作成」をクリックすることでトラック編集ツールを開始することができます。<br>
トラック編集ツールを起動した状態でシーンビューにマウスカーソルを移動すると、マウスカーソル上にトラックを形成するためのポイントのプレビューが表示され、クリックすることでポイントを配置することができます。<br>

<img width="800" alt="track_creation_start" src="../Documentation~/Sandbox Images/track_creation_start.png">

シーンビュー内において、トラックを引きたい地表上でクリックをすると、トラック生成のポイントが作成されます。
そのまま続けて地表上をクリックしていくと、ポイントが生成され、ポイントに合わせてトラックが生成されます。

<img width="500" alt="track_creation_points" src="../Documentation~/Sandbox Images/track_creation_points.png">

最後に始点と同じポイントをクリックすると、ループ可能なトラックが作成できます。Escキーを押すことでトラックの作成を終了します。

<img width="500" alt="track_creation_loop" src="../Documentation~/Sandbox Images/track_creation_loop.png">

### トラックの速度制限

トラックはトラック移動コンポーネント ( `PlateauSandboxTrackMovement` ) の制限速度を設定することができます。
制限速度を設定することで、そのトラック上での人や乗り物の移動速度の上限を設定することができます。<br>

なお、トラック移動コンポーネントはアバターや乗り物にアタッチすることで、トラック上を移動させることができる機能です。<br>
詳細は [7-2. トラック移動コンポーネント](#7-2-トラック移動コンポーネント) を参照してください。


速度制限の設定は、`PlateauSandboxTrack` のインスペクターから設定するか、速度制限設定ツールからシーン上で一括設定することができます。

- インスペクターから設定する場合は `Has Speed Limit` にチェックボックスをいれて、制限速度（m/s）を `Speed Limit` に設定してください。

<img width="500" alt="plateau_sandbox_track_speed_limit" src="../Documentation~/Sandbox Images/plateau_sandbox_track_speed_limit.png">

- 速度制限設定ツールは PLATEAU Sandbox Toolkit ウィンドウのトラックタブから起動することができます。

<img width="400" alt="plateau_sandbox_track_speed_limit_tool" src="../Documentation~/Sandbox Images/plateau_sandbox_track_speed_limit_tool.png">

- 制限速度ツールを使用するとシーンの各トラック上に設定ウィンドウが表示され、このウィンドウから制限速度を設定することができます。

<img width="1000" alt="plateau_sandbox_track_speed_limit_setting" src="../Documentation~/Sandbox Images/plateau_sandbox_track_speed_limit_setting.png">


## 7-2. トラック移動コンポーネント

アバター ( `PlateauSandboxHuman` ) と乗り物 ( `PlateauSandboxVehicle` ) はトラック移動コンポーネント ( `PlateauTrackMovement` ) をアタッチすることで、トラック上を移動させることができます。

### トラック上の正規化位置

トラック移動コンポーネントでは移動対象のオブジェクトのトラック上の位置を正規化された値で表現し、ここでは「正規化位置」と呼びます。値の最大値はトラックに含まれる分岐などによって異なります。単純な直線（一筆書きができる形状）の場合、正規化位置0から1までの値で表現され、0が始点、1が終点を示します。例えば単純な直線の場合、正規化位置が0.25のときオブジェクトは、始点からトラックの長さの1/4 (=0.25)の長さを進んだ位置にあります。

### トラック移動コンポーネントの設定方法

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

### トラック移動コンポーネントの利用方法

トラック移動コンポーネントはエディターモードでは動作しません。プレイモードを実行することで移動を開始させることができます。

### トラック移動コンポーネントの機能

#### 衝突判定
<img width="800" alt="sandbox_collision_example" src="../Documentation~/Sandbox Images/sandbox_collision_example.gif">

トラック移動コンポーネントによって移動するオブジェクトが別のオブジェクトに衝突する可能性がある場合に速度を下げて衝突を回避する仕組みが実装されています。衝突判定がある場合、速度は最大で0まで減少します。

設定方法にあるように、衝突判定はトラック移動コンポーネントのフィールドを調整することができます。

衝突判定を行う距離は速度に応じて変化します。速度が遅い場合は `Min Collision Detect Distance` が衝突判定距離の最小値として使用されますが、速度が早い場合はその速度に応じて判定する距離を大きくします。これは速度によって制動距離が異なるためです。

#### 移動アニメーション
アバター ( `PlateauSandboxHuman` ) と乗り物 ( `PlateauSandboxVehicle` )にはトラック移動中の演出が実装されています。

- アバター
    - トラックの進行方向に向かってアバターを回転させます。
- 乗り物
    - 前輪と後輪がトラックに沿うように乗り物を移動させます。
    - 前輪が移動する方向に向かうように回転させます。
    - タイヤが移動速度に応じて回転します。

## 7-3. トラックに沿ってオブジェクトを生成する機能
`PlateauSandboxTrackInstantiate` コンポーネントを使用し、トラックに沿って任意のオブジェクトを自動生成することができます。

`PlateauSandboxTrackInstantiate` コンポーネントは Unity Splines パッケージの `SplineInstantiate` を拡張して実装されています。そのため、オブジェクトの生成ルールに関する詳細な設定は `SplineInstantiate` コンポーネントのパラメータを変更する必要がありますが、基本的な設定は `PlateauSandboxTrackInstantiate` のインスペクタに表示される機能を利用することで変更することができます。尚、 `SplineInstantiate` コンポーネントは `PlateauSandboxTrackInstantiate` をアタッチすると自動的にアタッチされます。

<img src="../Documentation~/Sandbox Images/sandbox_spline_instantiate_sample.png" />

### 生成アイテムリストの設定

#### 生成するオブジェクトの追加・削除
生成するオブジェクトは Sandbox Toolkit ウィンドウもしくはプロジェクトビューからプレハブを `PlateauSandboxTrackInstantiate` のインスペクターに表示される生成アイテムリストにドラッグアンドドロップすることで追加できます。
登録したオブジェクトを削除する場合は `生成アイテムリスト` から削除したいオブジェクトの "✕" を押下してください。

この設定は `SplineInstantiate` コンポーネントの `Items to Instnatiate` からも変更できます。

<img src="../Documentation~/Sandbox Images/sandbox_add_track_instantiate_item.gif" />

#### 生成するオブジェクトの出現割合の設定
登録されているアイテムがトラック上に出現する割合は登録されているオブジェクトのそれぞれの `生成割合` を変更することで調整できます。この値はオブジェクトが出現する割合の比率を表現しているため合計が100%になるように設定する必要はありません。例えばすべての値を "1" に設定するとすべてのオブジェクトが同じ割合で出現します。

この設定は `SplineInstantiate` コンポーネントの `Items to Instnatiate` からも変更できます。

<img src="../Documentation~/Sandbox Images/sandbox_track_instantiate_ratio.gif" />

### `SplineInstantiate` コンポーネントの設定

`SplineInstantiate` コンポーネントの設定では詳細なオブジェクトの生成ルールを変更することができ、オブジェクト生成位置のランダム性、オブジェクト毎の間隔、オブジェクトの最大数、オブジェクトの向きなど様々な生成ルールを定義することができます。

尚、生成ルールは登録されているすべてのオブジェクトに対して適用されます。そのため、オブジェクトごとに異なる生成ルールを定義したい場合は、複数の `PlateauSandboxTrackInstantiate` をスプラインにアタッチしてください。

| プロパティ | 説明 |
| --- | --- |
| Container | スプラインコンポーネントがアタッチされたゲームオブジェクトを選択し、その上にゲームオブジェクトやプレハブを生成します。 |
| Items To Instantiate | 生成したいゲームオブジェクトやプレハブのリストを作成します。リスト内の各要素について、ゲームオブジェクトまたはプレハブを選択し、そのアイテムがスプライン上で生成される確率を設定します。 |
| Up Axis | 生成されたアイテムが上方向として使用する軸を選択します。デフォルトの上方向はy軸です。 |
| Forward Axis | 生成されたアイテムが前方向として使用する軸を選択します。デフォルトの前方向はz軸です。 |
| Align To | 生成されたアイテムが向く空間を選択します。利用可能な空間は以下の通りです。<br/> `Spline Element` : 生成されたアイテムをアイテムが生成される場所に最も近いノットの回転から計算された補間された方向に合わせます。<br/> `Spline Object` : 生成されたアイテムをターゲットスプラインの向きに合わせます。<br/> `World Space` : 生成されたアイテムをワールドスペースの向きに合わせます。 |
| Instantiate Method | スプライン上でアイテムを生成する方法を選択します。利用可能な生成方法は以下の通りです。<br/> `Instance Count` : ターゲットスプライン上に特定数のアイテムを生成します。<br/> `Spline Distance` : スプラインに沿って測定される距離間隔でアイテムを生成します。<br/> `Linear Distance` : ワールドスペースで直線的に測定される距離間隔でアイテムを生成します。 |
| Count | 生成するアイテムの数または包括的なランダム範囲を設定します。このプロパティは `Instance Count` を選択した場合にのみ表示されます。 |
| Spacing (Spline) | スプラインに沿ってアイテムを生成するための距離間隔を設定します。距離はスプラインに沿って測定されます。正確な距離または包括的なランダム範囲の値を設定できます。このプロパティは `Spline Distance` を選択した場合にのみ表示されます。 |
| Spacing (Linear) | ワールドスペースで直線的に測定される距離間隔でアイテムを生成します。スプライン上にオーバーラップなく収まる限りのアイテムを生成するには、 `Auto` を選択します。このプロパティは `Linear Distance` を選択した場合にのみ表示されます。 |
| Position Offset | スプラインに対して相対的な位置でアイテムを生成するために有効にします。 |
| Rotation Offset | 元のゲームオブジェクトに対して相対的な回転でアイテムを生成するために有効にします。 |
| Scale Offset | 元のゲームオブジェクトに対して相対的なスケールでアイテムを生成するために有効にします。 |
| Override space | オフセットを選択した座標空間に適用するために有効にします。 `Override space` を有効にしない場合、オフセットは `Align to` で設定した座標空間に適用されます。このプロパティは `Position Offset` 、 `Rotation Offset` 、または `Scale Offset` を有効にした場合にのみ表示されます。 |
| X | オフセットのx軸の値を設定します。正確な値または包括的なランダム範囲の値を設定できます。オフセットは `Align to` で設定した座標空間に適用されます。このプロパティは `Position Offset` 、 `Rotation Offset` 、または `Scale Offset` を有効にした場合にのみ表示されます。 |
| Y | オフセットのy軸の値を設定します。正確な値または包括的なランダム範囲の値を設定できます。オフセットは `Align to` で設定した座標空間に適用されます。このプロパティは `Position Offset` 、 `Rotation Offset` 、または `Scale Offset` を有効にした場合にのみ表示されます。 |
| Z | オフセットのz軸の値を設定します。正確な値または包括的なランダム範囲の値を設定できます。オフセットは `Align to` で設定した座標空間に適用されます。このプロパティは `Position Offset` 、 `Rotation Offset` 、または `Scale Offset` を有効にした場合にのみ表示されます。 |
| Auto Refresh Generation | スプラインまたはそのパラメータを変更したときに、生成されたアイテムを自動的に再生成するために有効にします。 |
| Randomize | ランダム範囲に設定されたすべての値を再生成し、その後アイテムを再度生成します。 |
| Regenerate | スプライン上にアイテムを再度生成します。 |
| Clear | スプラインから生成されたすべてのアイテムを削除します。 |

### 生成ツール
オブジェクト生成時の乱数に関する設定や手動生成などのツールを提供しています。これらの機能は `SplineInstantiate` コンポーネントの "Randomize" や "Regenerate" と同様の機能ですが、 `PlateauSandboxTrackInstantiate` コンポーネントのよりアクセスしやすい位置にツール群を用意しています。また、シード値の手動設定は `SplineInstantiate` では提供されていません。

#### 自動リフレッシュ機能
初期設定ではこの機能は有効化されています。新しいオブジェクトが登録されたときやパラメータを変更したとき、自動的にその設定がオブジェクトの生成に反映（リフレッシュ）されます。自動リフレッシュではなく、任意のタイミングで生成を行いたい場合は `自動生成` を無効化し、生成したいタイミングで "生成" ボタンを押下してください。

#### リセット
"リセット" ボタンを押下すると、自動生成されたオブジェクトがすべて削除されます。自動生成が有効の場合でも、一度オブジェクトはすべて削除され、パラメータを変更するか "生成" ボタンを押下することで再度オブジェクトが生成されます。万が一、不具合などでパラメータを変更しても正しく生成ルールが変更されない場合や不正なオブジェクトが存在している場合はこの機能を使用して一度すべてのオブジェクトを削除してください。

#### 生成シード値 (乱数パターンの設定)
オブジェクト生成時の乱数生成に使用されるシード値を設定します。
オブジェクトの生成ルールに乱数を利用する場合、算出される乱数は `生成シード値` に設定された値によって一意に決定されます。従って、生成されるオブジェクトの位置をランダムに設定していても、 `生成シード値` が同じ限りは何度オブジェクトを生成しても同じパターンでトラックに沿ってオブジェクトが配置されます。

`生成シード値` は手動で任意に設定することもできますが、 "ランダム生成" ボタンを押下することでランダムな値を設定することができます。この機能を利用することで、様々な生成パターンを簡単に確認することができます。

<img src="../Documentation~/Sandbox Images/sandbox_track_instantiate_random.gif" />

# 8. 配置したアセットの編集機能

## 8-1. 広告変更機能

配置可能アセットの中には看板アセットのように広告物シミュレーションを行うための広告コンポーネント ( `PlateauSandboxAdvertisement` ) がアタッチされているものがあります。表示させたいテクスチャやビデオクリップを設定することで広告物の内容を変更できます。

<img width="600" alt="sandbox_ad_board" src="../Documentation~/Sandbox Images/sandbox_ad_assets.png">
<img width="600" alt="sandbox_ad_board" src="../Documentation~/Sandbox Images/sandbox_ad_board.png">

### テクスチャ切り替え

広告コンポーネントの `Advertisement Type` フィールドを `Image` に変更し、 `Advertisement Texture` フィールドに表示させたいテクスチャをドラッグアンドドロップまたはテクスチャ選択ウィンドウから選択することで広告物の内容を変更できます。

<img width="600" alt="sandbox_ad_board" src="../Documentation~/Sandbox Images/sandbox_ad_tex_replace.png">

テクスチャの推奨アスペクト比を `Aspect Ratio` フィールドに表示しているので、そのアスペクト比に合わせてテクスチャを設定してください。スケール変更時のアスペクト比率は最下部の `Info Box` にて表示しているのでご確認ください。

<img width="600" alt="sandbox_ad_board" src="../Documentation~/Sandbox Images/sandbox_ad_tex_replaced.png">

### ビデオクリップ切り替え

広告コンポーネントの `Advertisement Type` フィールドを `Video` に変更し、 `Advertisement Video` フィールドに表示させたいビデオクリップをドラッグアンドドロップまたはビデオクリップ選択ウィンドウから選択することで広告物の内容を変更できます。

<img width="600" alt="sandbox_ad_board" src="../Documentation~/Sandbox Images/sandbox_ad_video_replace.png">

<img width="600" alt="sandbox_ad_board" src="../Documentation~/Sandbox Images/sandbox_ad_video_replaced.gif">

### サイズ変更機能
広告コンポーネントの`広告の大きさ`フィールドには広告物アセットの実スケールでの大きさがメートル単位で表示されます。この値を変更すると、シーンに配置された広告物アセットの大きさを調整できます。

<img width="600" alt="advertisement_size_adjustment" src="../Documentation~/Sandbox Images/advertisement_size_adjustment.png">

## 8-2. 建築物カスタマイズ機能

配置可能アセットの中には建物アセットを配置してパラメータ変更により見た目をカスタマイズするための建築物コンポーネント ( `PlateauSandboxBuilding` ) がアタッチされているものがあります。
v2.0.0-alpha時点では以下の5種類の建築物アセットを提供しています。
- マンション
- 商業ビル
- コンビニエンスストア
- 一軒家
- オフィスビル

<img width="600" alt="sandbox_ad_board" src="../Documentation~/Sandbox Images/sandbox_buildings_assets.png">
<img width="600" alt="sandbox_ad_board" src="../Documentation~/Sandbox Images/sandbox_buildings_apartment.png">

### パラメータ変更方法

建物のパラメータを変更するには建物オブジェクトを選択し、インスペクター内の `PlateauSandboxBuilding` コンポーネントのパラメータを変更します。

<img width="600" alt="sandbox_ad_board" src="../Documentation~/Sandbox Images/sandbox_buildings_apartment_inspector.png">

#### 建造物設定

どの建物にも共通しているパラメータ「建物の高さ」「建物の横幅」「建物の奥行き」を変更できます。

> [!WARNING]
> 一部の建物タイプには変更ができない項目があります。

#### 建造物タイプの個別設定

建造物タイプに応じた個別のパラメータを変更できます。建物タイプによって表示されるパラメータは異なります。

##### マンション
<img width="400" alt="building_settings_apartment" src="../Documentation~/Sandbox Images/building_settings_apartment.png">

- バルコニーを外壁にせり出す(1m)：バルコニーを壁から1mせり出すことができます。

<img width="600" alt="building_settings_apartment" src="../Documentation~/Sandbox Images/sandbox_buildings_apartment_out_balcony.gif">

- 窓ガラスバルコニーに切り替え：バルコニーの見た目を壁から窓ガラスに切り替えることができます。

<img width="600" alt="building_settings_apartment" src="../Documentation~/Sandbox Images/sandbox_buildings_apartment_window_glass.gif">

- 左側にバルコニーを作成：建物の左側面にバルコニーを追加します。

<img width="600" alt="building_settings_apartment" src="../Documentation~/Sandbox Images/sandbox_buildings_apartment_left_balcony.gif">

##### 商業ビル
個別のパラメータはありません。

##### コンビニエンスストア
<img width="400" alt="building_settings_conviniencestore" src="../Documentation~/Sandbox Images/building_settings_conviniencestore.png">

- 側面を壁に設定：側面を窓ガラスから壁に切り替えることができます。
- 屋根の厚さ：屋根の厚さを指定できます。

##### 一軒家
<img width="400" alt="building_settings_house" src="../Documentation~/Sandbox Images/building_settings_house.png">

- 階数：階数を1から3までの間で指定できます。
- エントランスに庇を追加：一軒家の入り口に庇を追加できます。
- 屋根タイプ：屋根の形状を以下から選択できます。
  - Flat：平屋根
  - Hipped：寄棟屋根
- 屋根の厚さ：屋根の厚さを指定できます。

##### オフィスビル
<img width="400" alt="building_settings_officebuilding" src="../Documentation~/Sandbox Images/building_settings_officebuilding.png">

- 1階を窓に変更：建物の1階部分を壁から窓に変更できます。
- 壁パネルの高さ：上下の窓ガラスに挟まれた壁パネルの高さを指定できます。

#### 色設定

建物の色をテクスチャやカラー情報を利用して変更できます。テクスチャを利用する場合は、マテリアルで利用しているテクスチャを編集します。カラー情報を利用する場合は、「テクスチャ利用」チェックボックスをオフにすることでカラー編集項目が表示されます。

#### Prefab保存

建物アセットのPrefab保存では [インスタンスのオーバーライド](https://docs.unity3d.com/ja/2019.4/Manual/PrefabInstanceOverrides.html) に加えて、「建造物を新規プレハブとして保存」ボタンにより、カスタマイズしたパラメータを名前が連番の別のPrefabとして保存できます。Prefabと同時に利用しているメッシュも併せてサンプルフォルダ内のBuildingsフォルダ内に保存されます。

> [!WARNING]
> 現在のバージョンではヒエラルキーからのドラッグ＆ドロップによるPrefab保存、およびプレハブモードでのPrefabの編集はサポートされていません。

# 9. カメラインタラクション機能

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

なお、特定のエージェントに対しカメラインタラクション機能を設定したくない場合は、 `PlateauSandboxHuman` の `Is Camera View Available` のチェックボックスを外します。<br>

<img width="400" alt="sandbox_camerainteracation_cameraviewenable" src="../Documentation~/Sandbox Images/sandbox_camerainteracation_cameraviewenable.png">

尚、キーボード入力によるカメラ操作の有効/無効の設定は "PlateauSandboxCameraManager" の `Enable Keywboard Camera Switch` のチェックボックスによって切り替えることができます。<br>
<img width="500" alt="sandbox_cameramanager_enablekeyboard" src="../Documentation~/Sandbox Images/sandbox_cameramanager_enablekeyboard.png">

## 9-1. 操作方法

### 共通

| 操作      | 内容                         | 備考        |
|:------- |:-------------------------- |:--------- |
| マウス移動   | カメラの向き変更                   |           |
| 「1」キー押下 | カメラを一人称視点に変更する           | キー操作有効時のみ |
| 「2」キー押下 | カメラを三人称視点に変更する           | キー操作有効時のみ |
| 「3」キー押下 | カメラを三人称中心視点に変更する | キー操作有効時のみ |
| 「0」キー押下 | カメラインタラクションモードを終了する        | キー操作有効時のみ |

### 一人称視点

オブジェクトの視点の位置からカメラ表示するモードです。<br>
<img width="600" alt="sandbox_camerainteraction_fps" src="../Documentation~/Sandbox Images/sandbox_camerainteraction_fps.png">

| 操作    | 内容       | 備考  |
|:----- |:-------- |:--- |
| マウス移動 | カメラの向き変更 |     |

### 三人称視点

オブジェクトの後方からオブジェクトの視点の先に向けたカメラを表示するモードです。<br>

<img width="600" alt="sandbox_camerainteraction_tps" src="../Documentation~/Sandbox Images/sandbox_camerainteraction_tps.png">

| 操作              | 内容           | 備考  |
|:--------------- |:------------ |:--- |
| マウス移動           | カメラの向き変更     |     |
| マウススクロール        | カメラ視点の前後移動   |     |
| マウス中央を押しながらドラッグ | カメラ視点の上下左右移動 |     |

### 三人称中心視点

対象のオブジェクトを中心に軌道上を移動することができるモードです。<br>

<img width="600" alt="sandbox_camerainteraction_around" src="../Documentation~/Sandbox Images/sandbox_camerainteraction_around.png">

| 操作       | 内容                   | 備考  |
|:-------- |:-------------------- |:--- |
| マウス移動    | エージェントを中心としたカメラの回転移動 |     |
| マウススクロール | 対象のオブジェクトとの距離の調整           |     |

## 9-2. カメラのデフォルト位置の変更

アバターや乗り物のコンポーネント ( `PlateauSandboxHuman` 、 `PlateauSandboxVehicle` ) の `CameraTargetSettings` を調整することでカメラのデフォルトの位置などカメラに関する設定を変更することができます。
位置は対象のオブジェクトの原点からの相対座標で表現され、Xは横方向, Yは高さ, Zで前後方向の設定をすることができます。<br>

<img width="700" alt="sandbox_camerainteraction_offset" src="../Documentation~/Sandbox Images/sandbox_camerainteraction_offset.png">

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
