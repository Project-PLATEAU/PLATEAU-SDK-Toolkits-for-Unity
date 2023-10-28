# Maps Toolkit 利用マニュアル



PLATEAUの3D都市モデルを用いた空間解析、可視化、建築情報との連携など、地図アプリ開発等を行うためのツールキットです。

- Maps Toolkit で提供される機能
    - Cesium for Unittyとの連携
        - PLATEAU SDKによりインポートした3D都市モデルにCesium for Unity上でのグローバル座標を与えます。
        - PLATEAUがストリーミング提供する地形モデル (PLATEAU Terrain) を用いることで、高精度の地形モデルをCesium for unityで利用可能になります。
    - BIMモデルとの連携（IFC ファイルの読み込み）
        - Revitなどで作られたIFCファイルをUnityシーン内に配置することができます。
        - IFCファイルに含まれる座標情報を用いてCesium for Unity上で自動的に配置することが可能です。
        - IFCファイルで定義される属性情報もUnity上で取り扱うことができます。
    - GISデータとの連携
        - シェープファイル及びGeoJSONをUnityシーン内に配置することができます。
        - シェープファイルのDBFファイルに含まれる座標情報を用いてCesium for Unity上で自動的に配置することが可能です。
        - シェープファイルのDBFファイル及びGeoJSONのプロパティで定義される属性情報もUnity上で取り扱うことができます。
     
# 目次

- [事前準備](#事前準備)
  * [PLATEAU SDK Toolkits for Unity のインストール](#plateau-sdk-toolkits-for-unity-のインストール)
  * [PLATEAU SDK for Unity を使って都市モデルをインポート](#plateau-sdk-for-unity-を使って都市モデルをインポート)
 
- [利用手順](#利用手順)
  * [1. PLATEAUモデル位置合わせ](#1-plateauモデル位置合わせ)
    + [1-1. シーンを用意する](#1-1-シーンを用意する)
    + [1-2. 地形モデルを作成する](#1-2-地形モデルを作成する)
    + [1-3. 地形モデルにPLATEAU Terrainを利用する](#1-3-地形モデルにplateau-terrainを利用する)
    + [1-4. 地形モデルにラスターをオーバーレイする](#1-4-地形モデルにラスターをオーバーレイする)
    + [1-5. Cesium for Unity上への3D都市モデルの配置](#1-5-cesium-for-unity上への3d都市モデルの配置)
    + [1-6. 3D都市モデルのストリーミング設定](#1-6-3d都市モデルのストリーミング設定)
      
  * [2. IFCモデルの読み込み](#2-ifcモデルの読み込み)
    + [2-1. IFCファイルをインポートする](#2-1-ifcファイルをインポートする)
    + [2-2. 属性情報を付与](#2-2-属性情報を付与)
    + [2-3. 指定した位置に配置](#2-3-指定した位置に配置)
    + [2-4. IFC属性情報から自動配置](#2-4-ifc属性情報から自動配置)
    + [2-5. IFC読み込みの環境設定](#2-5-ifc読み込みの環境設定)
      
  * [3. GISデータ読み込み](#3-gisデータ読み込み)
    


# 事前準備

## PLATEAU SDK Toolkits for Unity のインストール

Maps Toolkit は PLATEAU SDK Toolkits for Unity の一つのツールキットとして提供されます。

[こちら](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity#3-plateau-sdk-toolkits-for-unity-%E3%81%AE%E3%82%A4%E3%83%B3%E3%82%B9%E3%83%88%E3%83%BC%E3%83%AB)を参照して PLATEAU SDK Toolkits for Unity をインストールしてください。

## PLATEAU SDK for Unity を使って都市モデルをインポート

PLATEAU SDK for Unityをインストールしていていない場合は、[マニュアルの手順](https://project-plateau.github.io/PLATEAU-SDK-for-Unity/manual/Installation.html)に従ってインストールします。

同じく、PLATEAU SDK for Unity[「都市モデルのインポート」の手順](https://project-plateau.github.io/PLATEAU-SDK-for-Unity/manual/ImportCityModels.html)に従って都市モデルをUnityエディターへインポートします。

# **利用手順**

上部のメニューより PLATEAU > PLATEAU Toolkit > Maps Toolkit を選択し、Maps Toolkit ウィンドウを開いて、それぞれの機能を利用することができます。

<img width="600" alt="map_manual_1_menu" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/88a05650-4f42-48ca-bd09-74ca3effff78">

<img width="600" alt="map_manual_2_toolkittop" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/db61db90-c7db-4df0-beaa-2bc7de17d629">


## 1. PLATEAUモデル位置合わせ

### 1-1. シーンを用意する

3D都市モデルを利用するシーンを用意し、開いてください。

### 1-2. 地形モデルを作成する

Unityエディターのメニューから Cesium > Cesium を選択し、Cesium ウィンドウを開きます。

Cesiumウィンドウの「Quick  Basic Assets」メニューの下にある 「Blank 3D Tiles Tileset」をクリックし、シーン上に3D地形モデルオブジェクトを作成します。


<img width="600" alt="map_manual_3_cesium" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/c6550211-6a87-4521-8a85-56024fb0519f">


シーンに「CesiumGeoreference」という名前のゲームオブジェクトが作成されていることを確認してください。また、ヒエラルキーで「Cesium3DTileset」というオブジェクトがCesiumGeoreference の子オブジェクトとして作成されており、このオブジェクトの `Cesium 3D Tileset` というコンポーネントに地形モデルの設定を行います。


<img width="600" alt="map_manual_4_3dtileset" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/5b5282d2-9e4e-4690-968f-5a6fa316c413">


### 1-3. 地形モデルにPLATEAU Terrainを利用する

> **Note**
> 地形モデルにPLATEAUの地形モデルを使用しない場合(Cesium World Terrainを利用する場合など)は3D都市モデルと地形モデルの地面の形状が合わず、3D都市モデルに含まれる建物が地面に埋まってしまったり、地面から浮いてしまう場合があります。
> このため、PLATEAUで提供している地形モデル ([PLATEAU Terrain](https://github.com/Project-PLATEAU/plateau-streaming-tutorial/blob/main/terrain/plateau-terrain-streaming.md)) を利用することを推奨します。

ヒエラルキーの「Cesium3DTileset」オブジェクトを選択し、インスペクターから `Cesium 3D Tileset` コンポーネントの `ion Asset ID` と `ion Access Token` を変更します。  
ここでは、[PLATEAU配信サービス（試験運用）](https://github.com/Project-PLATEAU/plateau-streaming-tutorial)が提供する地形モデル（PLATEAU Terrain）を利用することができます。
チュートリアルの「**[2.1. アクセストークン及びアセットID](https://github.com/Project-PLATEAU/plateau-streaming-tutorial/blob/main/terrain/plateau-terrain-streaming.md#21-%E3%82%A2%E3%82%AF%E3%82%BB%E3%82%B9%E3%83%88%E3%83%BC%E3%82%AF%E3%83%B3%E5%8F%8A%E3%81%B3%E3%82%A2%E3%82%BB%E3%83%83%E3%83%88id)**」に記載されている値を入力します。

<img width="600" alt="map_manual_6_cesiumtileset" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/85f435f7-2828-4779-bea6-f7fbf27583e3">

<img width="600" alt="map_manual_7_terrain_notexture" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/2f637b61-3ac5-446f-aeba-104c4da13b7d">


正しく設定されていれば、シーンにPLATEAUの地形モデルが描画されます（表示されない場合は `Cesium 3D Tileset` コンポーネントの上部にある「Refresh Tileset」ボタンを押してください）。この時点ではテクスチャを設定していないため、単色のメッシュのみが表示されています。

<img width="600" alt="map_manual_8_showtilesHierarchy" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/56350d69-d688-416e-8113-fa8c2c02a956">

なお、ヒエラルキーではメッシュのゲームオブジェクトは非表示なっていますが、 `Cesium 3D Tileset` の `Show Tiles in Hierarchy` を有効にすることで表示させることができます。メッシュオブジェクトは `Cesium 3D Tileset` コンポーネントがアタッチされているゲームオブジェクトの子オブジェクトとして生成されています。


### 1-4. 地形モデルにラスターをオーバーレイする

Cesium 3D Tilesetによって配置された地形モデルにテクスチャを付与するためにはCesium for UnityのRaster Overlay機能を利用します。  
ここではテクスチャ画像にCesiumから提供される航空画像テクスチャを使用しますが、この場合 Cesium Ion アカウントへ接続し、アクセストークンを取得する必要があります。  
テクスチャが不要の場合はこの手順をスキップしてください。

> **Note**
> [PLATEAU配信サービス（試験運用）](https://github.com/Project-PLATEAU/plateau-streaming-tutorial)ではPLATEAUが提供する航空写真データである[PLATEAU Ortho](https://github.com/Project-PLATEAU/plateau-streaming-tutorial/blob/main/ortho/plateau-ortho-streaming.md)を提供していますが、**現時点でCesium for Unityに対応していません**。今後Cesium for Unity内でPLATEAU Orthoを利用可能とする実装を進める予定です。

#### Cesium Ion アカウントへの接続

PLATEAUの地形モデルの利用のみでCesiumのその他のアセットやBing Mapsなどの外部アセットデータを利用しない場合は接続する必要はありません。

ログインするためにはCesiumウィンドウから「Connect to Cesium ion」を押下し、表示されるURLをコピーしてブラウザで開きます。

<img width="400" alt="map_manual_9_cesiumtop" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/0993d253-dde2-42eb-b6b5-42fadb431c6c">

ログイン画面が表示されるので、アカウント情報を入力してログインします。アカウントがない場合はCesium ionの[サインアップ](https://ion.cesium.com/signup/)をしてください。

#### Cesium サインイン
ユーザー名、パスワードを入力してサインインします。

<img width="400" alt="map_manual_10_cesimion_login" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/95110b8e-eacb-4c0a-a613-2d1e00865d63">

ログインに成功すると、次のような画面が表示されるので「Allow」を押下します。

<img width="400" alt="map_manual_11_cesiumion_permission" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/690c68c1-f5fb-475d-90d6-6351b45c92fd">

正しくログインが完了すると、Cesiumウィンドウに Cesium ion Assets を用いた機能など、Cesiumへのログインが必要な機能が利用可能になります。

<img width="400" alt="map_manual_12_logined" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/bcb9edc7-cafd-46cd-8275-d0d0df5c284d">

#### ラスターオーバーレイ設定

「Cesium3DTileset」オブジェクトのインスペクタから「Add Componet」を押下し `Cesium Ion Raster Overlay` コンポーネントを追加します。

<img width="400" alt="map_manual_13_cesimraster" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/6378cd89-ca68-4ca8-8f9a-b8e84d37259a">


追加した `Cesium Ion Raster Overlay` コンポーネントの `ion Asset ID` を 2 に変更します。

<img width="400" alt="map_manual_14_ionassetid" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/75e1d805-e534-45da-acc3-54f8fbd43547">


この状態で、決定しようとすると、下記のようなダイアログが表示され、access tokenが求められます。「Select or create a new project default token」を選択してください。

<img width="400" alt="map_manual_15_troubleshooting" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/08b1a1f9-553a-4b2d-84fb-5146c12428ac">

「Select Cesium ion Token」ダイアログが開くので、「Use an existing token」にチェックを入れ、プルダウンの「Default Token」を選択します。入力したら「Use as Project Default Token」ボタンを押下します。

<img width="400" alt="map_manual_16_cesium_selecttoken" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/a6b4b367-c88a-4012-a99b-817f0b181eb6">


Cesium3DTilesetの地形モデルにテクスチャが表示されるようになります。

<img width="600" alt="map_manual_17_terrain_textured" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/d85b62a0-7561-40b1-8ded-8016e57c4a1d">


### 1-5. Cesium for Unity上への3D都市モデルの配置

#### Cesium Globe Anchor を3D都市モデルに設定する

PLATEAU SDKを用いて配置された3D都市モデルにCesium for Unity上でのグローバル座標を付与するため、3D都市モデルオブジェクト に `Cesium Globe Anchor` コンポーネントをアタッチします。このコンポーネントがアタッチされたオブジェクトを `Cesium Georeference` の子オブジェクトとすることで、 `Cesium Georeference` の座標に基づきシーン上に配置することが可能です。

ヒエラルキー上でインポートしてある3D都市モデルを `CesiumGeoreference` の子オブジェクトとして配置します（左図が配置変更前、右図が配置変更後になります）。

<img width="300" alt="map_manual_18_hierarchy_before" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/4527b649-2083-4f63-94ec-f13fca0f77d3">
<img width="300" alt="map_manual_19_hierarchy_after" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/66a28f2f-d27c-4e0a-9732-520e3eb04926">

3D都市モデルオブジェクトのインスペクタ上で「Add Component」を押下し、 `Cesium Globe Anchor` を選択してアタッチします。これで3D都市モデルを位置合わせするための準備は完了です。

<img width="600" alt="map_manual_20_addcomponent" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/67eacc17-cad9-46a1-b8e3-dbf991b8b080">
<img width="600" alt="map_manual_21_globeanchor" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/f6da7258-4da8-4862-bfeb-a634b24f6b31">

#### 位置合わせを実行する

Maps Toolkit ウィンドウの `PLATEAUモデル` フィールドにシーン上の3D都市モデルオブジェクトを設定します（ヒエラルキーからドラッグアンドドロップして設定できます）。

<img width="1000" alt="map_manual_22_setplateaumodel" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/0dc9fc2d-77f9-4250-a9f5-b6a9657c0629">


「PLATEAUモデルの位置を合わせる」を押すと選択した3D都市モデルオブジェクトがCesiumの地形モデル上で正しい位置に配置されます。

(例) 上記の設定で東京タワー周辺のPLATEAU建築物モデルの位置合わせの実行結果
<img width="1000" alt="map_manual_23_modelfitting" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/15b2a0b0-89b6-4449-9596-5a84a6ded6ea">

> **Note**
**位置合わせ**を実行すると`PLATEAU Instanced City Model`オブジェクトの緯度経度高さが`Cesium Georeference` ゲームオブジェクトのコンポーネントの `Origin (Longitude Latitude Height)`の値へ自動的に入力されます。

<img width="400" alt="map_manual_24_citymodel_lonlat" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/c8224783-3da1-4c7a-99fb-a758879b4b48">

<img width="400" alt="map_manual_25_cesiumgeo_lonlat" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/50b856a6-5d19-47e1-a6a2-cdd9e3442d89">


> **Warning**
> `Cesium Georeference`の*緯度・経度・高度は、大きい値を取り扱う際に生じる小数の計算誤差問題をさけるために、利用する3D都市モデルの近くの緯度経度に設定する必要があります（一般的にはこの座標中心が利用する3D都市モデルから10㎞以上離れている場合に計算誤差が発生します）。*

### 1-6. 3D都市モデルのストリーミング設定

PLATEAU SDK によって特定の範囲の3D都市モデルをインポートする他に、Cesiumを利用してその周辺の3D都市モデルを表示することで、全体的な景観などを確認することができます。

#### ストリーミング用の `Cesium 3D Tileset` オブジェクトの作成

Cesium ウィンドウから再度「Blank 3D Tiles Tileset」を押下し、新しい `Cesium 3D Tileset` オブジェクトを作成します。このとき、既にシーン内に `Cesium Georeference` オブジェクトが存在する場合はそのオブジェクトの子オブジェクトとして作成されます。

<img width="400" alt="map_manual_26_blank3dtileasset" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/5bd144aa-290f-487e-a8ef-9986184c6d06">


#### ストリーミングURLを設定

`Cesium 3D Tileset` の `Tileset Source` を「From Url」に変更します。

次に、 [PLATEAU配信サービス（試験運用）](https://github.com/Project-PLATEAU/plateau-streaming-tutorial)から配信されている3D都市モデル（3DTiles)の利用の設定を行います。  
3D都市モデル（3DTiles）は都市単位でURLが設定されているため、以下のページからストリーミングしたい地域を選び、 `URL` に入力します。

plateau-3D Tiles-streaming

- https://github.com/Project-PLATEAU/plateau-streaming-tutorial/blob/main/3d-tiles/plateau-3dtiles-streaming.md

<img width="400" alt="map_manual_27_plateau-3D Tiles-streaming" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/9d12418f-a1a6-4e73-80f4-0092d56ea3e3">


なお、複数の地域をまたがる場所の開発を行う場合は、これまでの手順を参考に `Cesium 3D Tileset` オブジェクトを作成し、それぞれURLを設定してください。

ストリーミング3D都市モデルを追加することで、下図のように青いアウトラインのあるインポートされた3D都市モデルの周囲の建物を表示することができます。

<img width="600" alt="map_manual_28_blueoutline" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/6d57edf4-8e7f-43fc-8b8a-01c0183ba038">


**ストリーミングされる3D都市モデルの範囲設定**

ここまでの手順で周囲の3D都市モデルを表示することができますが、インポートした部分の3D都市モデルも重複して表示されています。

<img width="600" alt="[map_manual_29_imported" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/05475ce7-066e-4f5c-a9d6-ba8b40d0af42">

Cesium では特定の領域の建物の表示を制限する仕組みが用意されているため、ここではその仕組を用いてインポートされた3D都市モデルがストリーミングされる3D都市モデルと重複しないような設定を行います。

Cesiumには `CesiumTileExcluder` というクラスが用意されており、このクラスを継承したクラスに3D Tileを除外するルールを記述することで表示範囲を制限することができます。

ここでは以下の GitHub ページで紹介さている方法を用いた3D Tilesの除外方法を説明します。

https://github.com/CesiumGS/cesium-unity/pull/248


次のようなスクリプトを作成し、プロジェクト内に保存します。
```csharp
using CesiumForUnity;
using UnityEngine;

[ExecuteInEditMode] // エディター上でも確認できるように追加
[RequireComponent(typeof(BoxCollider))]
public class CesiumBoxExcluder : CesiumTileExcluder
{
    BoxCollider m_BoxCollider;
    Bounds m_Bounds;

    protected override void OnEnable()
    {
        m_BoxCollider = gameObject.GetComponent<BoxCollider>();
        m_Bounds = new Bounds(m_BoxCollider.center, m_BoxCollider.size);

        base.OnEnable();
    }

    protected void Update()
    {
        m_Bounds.center = m_BoxCollider.center;
        m_Bounds.size = m_BoxCollider.size;
    }

    public bool CompletelyContains(Bounds bounds)
    {
        return Vector3.Min(this.m_Bounds.max, bounds.max) == bounds.max &&
               Vector3.Max(this.m_Bounds.min, bounds.min) == bounds.min;
    }

    public override bool ShouldExclude(Cesium3DTile tile)
    {
        if (!enabled)
        {
            return false;
        }

        return m_Bounds.Intersects(tile.bounds);
    }
}
```

範囲を制限したい `Cesium 3DTileset` オブジェクトに作成した `CesiumBoxExcluder` をアタッチします。このコンポーネントをアタッチしたときに自動的にアタッチされる `Box Collider` というコンポーネントの利用することで範囲を設定します。 `Box Collider` は箱型の衝突を検知するためのコンポーネントですが、ここではシーン上で簡単に範囲を確認できる機能として利用しています。

 `CesiumBoxExcluder` の `Invert` をオンにすると `Box Collider` の外側の3D Tilesのみが表示され、オフにすると内側のみが表示されます。

<img width="600" alt="map_manual_30_box" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/b378183c-d973-44fe-8a18-5bfb0f926938">


## 2. IFCモデルの読み込み

IFC読み込みツールでは読み込んだIFCモデルを選択し、以下のような操作を行うことが可能です。

<img width="600" alt="map_manual_31_ifctop" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/3192600d-09d5-4a30-b9d2-32003b67990d">


| 項目 | 説明 |
| --- | --- |
| IFCモデル | 操作するIFCモデルを設定します。 |
| IFC属性情報 | IFCファイルを読み込んだ際に保存されたXMLファイルを設定します。 |
| 属性情報を付与 | IFCファイルの属性情報をUnity上のIFCモデルに関連付けします。これにより、そのIFCの位置などの属性情報をUnity上で利用することができます。 |
| 指定した位置に配置  | 指定された項目を入力し、取り込んだIFCモデルを指定した位置に配置します。 |
| IFC属性情報から自動配置  | 設定した属性情報をもとに自動的にIFCモデルを配置します。属性情報に緯度経度情報が含まれない場合は使用できません。この機能を使用する際は「IFC属性情報」にXMLファイルを指定する必要があります（属性情報を付与していてもXMLファイルの指定が必要です）。 |

以降では、上記の各操作について手順を説明します。

> **Note**
> PLATEAUでは3D都市モデルと互換性のあるBIMモデルとしてIFC2x3を指定し、必要なデータ仕様を定義しています。
> 特に座標情報を付与したIFCファイルを扱う場合は、PLATEAUが定義するデータ仕様に則る必要があります。
> 詳細は以下のドキュメントを参照してください。  
> - 「[3D都市モデル整備のためのBIM活用マニュアル 第2.0版](https://www.mlit.go.jp/plateau/file/libraries/doc/plateau_doc_0003_ver03.pdf)」  
> -  [「3D都市モデルとの連携のためのBIMモデルIDM・MVD 第2.0版」](https://www.mlit.go.jp/plateau/file/libraries/doc/plateau_doc_00031_ver02.pdf)  
 
### 2-1. IFCファイルをインポートする

「ローカルディスクからIFCファイルを読み込み」を押下し、ファイル選択ウィンドウが表示されるので読み込むIFCファイルを選択します。

[リリースページ](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/releases/tag/v0.2.1)からIFCのサンプルファイルをダウンロードできます。以下の手順では[sample.ifc](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/releases/download/v0.2.1/sample.ifc)を用いて説明します。

 PLATEAU都市モデルとBIMモデル（ここではIFCファイル）を活用するにあたって詳細は「[3D都市モデル整備のためのBIM活用マニュアル](https://www.mlit.go.jp/plateau/file/libraries/doc/plateau_doc_0003_ver03.pdf)」をご参照ください。
 
<img width="600" alt="map_manual_32_ifcload" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/e83c1758-533a-4f8f-ad82-9dbd5c41b8f2">

コンソール画面が開き、読み込みと変換処理が開始されます。（時間がかかる場合があります。）

<img width="400" alt="map_manual_33_terminal" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/644738b1-960e-4e11-9ea8-5b5ac0294616">


完了するとプロジェクト内のAssets/MeshesフォルダにGLBファイルとXMLファイルが保存されます。

<img width="400" alt="map_manual_34_asettsmeshes" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/44e58269-2a0d-4379-851b-aa50f37b892c">


インポートしたGLBファイルは、シーンにドラッグドロップして配置することができます。

<img width="400" alt="map_manual_35_dragdrop" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/4e41cf85-710a-4beb-86a1-c29e47d2f185">



### 2-2. 属性情報を付与

「ローカルディスクからIFCファイルを読み込み」によってIFCを読み込んだ結果、Assets/MeshesフォルダにGLBファイル（3Dモデル）とXMLファイル（属性情報）が作成されます。

「属性情報を付与」機能ではUnityエディタ内でGLBファイル（3Dモデル）とXMLファイル（属性情報）の関連付けを行いいます。

<img width="600" alt="map_manual_36_attribution" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/c27951db-d7a2-4612-a30b-11e9358070ab">


IFCモデルの項目には、ヒエラルキーからGLBのゲームオブジェクトをドラック＆ドロップして設定します。

<img width="600" alt="map_manual_37_drop" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/d5c014a5-446a-49b8-9d09-b7e86f383d7f">


IFC属性情報の項目には、Assets/MeshesフォルダからXMLファイルをドラック＆ドロップして設定します。

<img width="400" alt="map_manual_38_" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/4bbdd87e-651e-40a3-8ed0-c25d1ebee7cd">


最後に「属性情報を付与」ボタンをクリックするとIFCファイルの属性情報をUnity上のIFCモデルに関連付けされ、位置情報などの属性情報をUnity上で利用することができます。

### 2-3. 指定した位置に配置

ヒエラルキーにて、GLBオブジェクト（IFCを読み込んだ結果）を`Cesium Georeference`の子オブジェクトに設定します。

<img width="600" alt="map_manual_39_glbundergeo" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/08b46041-c04e-4a7b-bd8d-7d55c25f1c41">

さらにGLBオブジェクトのインスペクタ上で「Add Component」を押下し、 `Cesium Globe Anchor` を選択してアタッチします。これで自動配置するための準備は完了です。

![Untitled 16](https://github.com/unity-shimizu/PLATEAU-SDK-Toolkits-for-Unity-Release2Draft/assets/113009946/d9c9b561-35a1-4a44-9c33-c7dca5afa961)


読み込んだIFCモデルを指定された緯度、経度、標高に配置します。また回転角度、縮尺を設定することができます。

<img width="600" alt="map_manual_40_ifc_posture" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/97fac068-7f74-4047-866a-3ded6c248e66">


### 2-4. IFC属性情報から自動配置

IFC属性情報に位置情報が保存されている場合、その情報を元にモデルを配置します。

あらかじめ「属性情報を付与」機能を使用してモデルと属性情報を関連付けさせておく必要があります。

ヒエラルキーにて、GLBオブジェクト（IFCを読み込んだ結果）を`Cesium Georeference`の子オブジェクトに設定します。

<img width="600" alt="map_manual_39_glbundergeo" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/08b46041-c04e-4a7b-bd8d-7d55c25f1c41">

さらにGLBオブジェクトのインスペクタ上で「Add Component」を押下し、 `Cesium Globe Anchor` を選択してアタッチします。これで自動配置するための準備は完了です。

<img width="400" alt="mapsnew" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/52cd0f6f-eb88-44aa-be51-3e554133de5f">


「IFC属性情報から自動配置」ボタンを押すとモデルが配置されます。

<img width="600" alt="map_manual_41_ifcautoplace" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/76566af7-b88e-4739-8d78-896f5931c6a8">


### 2-5. IFC読み込みの環境設定
| 項目 | 説明 |
| --- | --- |
| IFC ローダーパス | IFCローダー (IfcConvert) はWindowsでは基本的に変更する必要はないですが、 macOS では選択する必要があります（後述）。 |
| IFC アウトプットパス | 生成されるファイルの出力先フォルダは、デフォルトではUnityプロジェクト内の Assets/Meshes に設定されていますが、 IFC アウトプットパス を設定することで別のフォルダを指定することができます。 |

<img width="600" alt="map_manual_42_ifcloaderpath" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/01f8a681-afa4-445a-8077-a6df8db5d54a">


#### macOSについて

macOSではmacOSのセキュリティ機能により、Maps Toolkit を利用する上で追加の設定が必要です。

##### Maps Toolkit が利用する IfcConvert の利用を許可する

IfcConvertとはMaps ToolkitでIFCファイルを読み込む際に利用するファイルです。このファイルの実行をmacOS上で許可しない場合、Maps ToolkitのIFC読み込みが利用できません。

macOSでは、セキュリティ面の観点からダウンロードしたバイナリは実行が許可されていないため、手動で許可する必要があります。「ターミナル」アプリを開き、で上記のバイナリのフォルダまで移動し以下のコマンドを実行します。

PLATEAU SDK Toolkits for Unity をインストールしたあとmacOSでは以下のフォルダにIfcConvertの実行ファイルがインストールされています。

`{インストールしたUnityプロジェクトのフォルダパス}/Library/PackageCache/com.unity.plateautoolkit@{ハッシュ}/PlateauToolkit.Maps/Editor/IfcConvert/`

<img width="400" alt="map_manual_43_macexplorer" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/42bdf792-afe1-4765-b226-b63e6eeca007">


> **Warning**
>  上記の画像の`com.unity.plateautoolkit@f713d0cb7891` の`f713d0cb7891`はインストールごとに生成されるIDなので環境によって異なります。

上記のフォルダの中に4つの実行ファイルがあります。

1. IfcConvert-macos-64 (Intel系CPU)
2. IfcConvert-macos-64-M1 (Apple Silicon系CPU)
3. IfcConvert-x32.exe
4. IfcConvert-x64.exe

exe形式の実行ファイルはWindows向けなので、以降の手順では(1) か (2) を自分のmacOS環境（CPU）に合わせて選んでください。

`IfcConvert-macos-64-M1 {IFCファイルのパス} {出力するGLBファイルのパス}`

(例) `IfcConvert-macos-64-M1 Test.ifc Test.glb`

“Permission Denied” が表示されていれば環境設定のセキュリティ画面を開きます。

<img width="600" alt="map_manual_44_macospermission" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/37ec1f3d-0430-4a7b-80a4-f7519690e74b">


上記の図のように、IfcConvert はブロックされましたというメッセージを見つけて「許可」を押下してください。その後、再度「ターミナルアプリ」から上記コマンドを実行してGLBファイルが生成されることを確認し、正しくIfcConvertの実行されることを確認してください。

##### Unityエディタで IfcConvert の実行ファイルを設定する

デフォルトでは Windows の実行ファイルのパスが設定されていますが、macOSでは Maps Toolkit ウィンドウから環境設定を開き、上記の実行ファイルのパスを `IFC ローダーパス` に設定してください。

<img width="600" alt="map_manual_45_ifcloaderpath" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/f652beb1-2b6e-474a-9a23-b9eb9863e45b">


設定が完了したら Windowsと同様の手順でMaps ToolkitによるIFCの読み込みができます。

## 3. GISデータ読み込み

Cesium for Unity上にGISデータ（シェープファイルもしくはGeoJSON）を配置します。

[リリースページ](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/releases/tag/v0.2.1)からシェープファイルやGeoJSONファイルのサンプルをダウンロードできます。以下の手順では[SHP_Sample.zip](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/releases/download/v0.2.1/SHP_Sample.zip)を用いて説明します。  
Maps ToolkitでGISデータを扱うためには、緯度経度（WGS84を推奨）が付されたデータが必要です。

GISデータは緯度経度を用いるデータであり、GISデータの読み込みを行う際は緯度経度を用いたオブジェクトの配置を行うために `Cesium Georeference` の設定が必要です。位置合わせの手順を参考に `Cesium Georeference` オブジェクトをシーン内に作成してください。

| 項目 | 説明 |
| --- | --- |
| GIS type | 読み込むファイルに合わせてシェープファイル（SHP）かGeoJSONを選択します。 |
| フォルダパス | シェープファイルもしくはGeoJSONファイルが入っているフォルダを指定します。フォルダに複数のシェープファイルやGeoJSONファイルが入っている場合はすべて描画されます。ファイルサイズやファイルの数が大きすぎると動作が遅くなる可能性があります。参考として、1つのファイルサイズは1MB以内で、フォルダ内に含まれるファイル数は3つまでです。 |
| 配置する高さ | オブジェクトを配置する CesiumGeoreference 上での高度を指定します。 |
| SHPのレンダー方法 | GISデータはメッシュあるいは線として描画することができます。GeoJSONの場合は自動で決定されます（ファイル内のプロパティから自動判断されます）。 |
| GISの線幅 | 描画する先の太さを指定します。SHPのレンダー方法がLineのときのみ表示されます。|

<img width="600" alt="map_manual_46_shppng" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/a3bb42e0-9586-46d3-8e1f-ddabe8638db9">

<img width="600" alt="map_manual_47_shpmesh" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/223001e4-43da-453e-806e-486aa607a03e">


ファイルを指定した後に「GISデータの読み込み」を押すと、でGISオブジェクトが描画されます。

<img width="600" alt="map_manual_48_shprever" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/0f56d730-f980-4ae6-a230-9feacdd69f1f">


> **Note**
> GISデータはファイルサイズに比例して読み込み時間が長くなります。シェープファイルの合計ファイルサイズは40MB程度以下を推奨します。<br>
> シェープファイルからインポートしたモデルの高さはインポートした際に設定されない場合もあるため、手動で適切な高さに設定する必要があります。
