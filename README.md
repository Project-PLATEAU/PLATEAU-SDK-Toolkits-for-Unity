# PLATEAU SDK-Toolkits for Unity

<img width="1080" alt="toolkits_key_visual" src="/Documentation~/Images/toolkits_key_visual.png">

PLATEAU SDK-Toolkits for Unityは、PLATEAUの提供する「3D都市モデル」のデータを利用したUnity上でのアプリケーション開発を支援するツールキット群です。  
**PLATEAU SDK-Toolkits for Unity は [PLATEAU SDK for Unity](https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity) を前提とするアドオンです。**

ダウンロードリンクは以下の通りです:
- [PLATEAU SDK-Toolkits （Rendering Toolkit / Sandbox Toolkit / PLATEAU Utilities）](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/releases/)
- [Maps Toolkit](https://github.com/Project-PLATEAU/PLATEAU-SDK-Maps-Toolkit-for-Unity/releases)
- [AR Extensions](https://github.com/Project-PLATEAU/PLATEAU-SDK-AR-Extensions-for-Unity/releases)

## PLATEAU SDK-Toolkitsの構成要素
PLATEAU SDK-Toolkits for Unityは以下の5つのコンポーネントから構成されます。

- [Rendering Toolkit](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/tree/main/PlateauToolkit.Rendering)
- [Sandbox Toolkit](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/tree/main/PlateauToolkit.Sandbox)
- [PLATEAU Utilities](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/tree/main/PlateauToolkit.Utilities)
- [Maps Toolkit](https://github.com/Project-PLATEAU/PLATEAU-SDK-Maps-Toolkit-for-Unity)（別リポジトリ）
- [AR Extensions](https://github.com/Project-PLATEAU/PLATEAU-SDK-AR-Extensions-for-Unity)（別リポジトリ）

PLATEAU SDK-Toolkitsを活用した下記の4種類のサンプルアプリプロジェクトを提供しています。<br>
開発の要点をまとめたチュートリアルも提供しています。  
詳しくは下記リポジトリをご覧ください。  

- [Urban Scape](https://github.com/Project-PLATEAU/PLATEAU-Toolkits-Sample-UrbanScape)
- [City Rescue Multiplay](https://github.com/Project-PLATEAU/PLATEAU-Toolkits-Sample-CityRescueMultiPlay)
- [AR Treasure Map](https://github.com/Project-PLATEAU/PLATEAU-Toolkits-Sample-ARTreasureMap)
- [AR City Miniature](https://github.com/Project-PLATEAU/PLATEAU-Toolkits-Sample-ARCityMiniature)

### 更新履歴
| 更新日時 | リリース | 更新内容 |
| :--- | :--- | :--- |
|  2024/09/06  |  **5th Release** <br>Sandbox Toolkitに以下の機能を追加 <br>1. アセットの一括配置機能<br>2. 建築物アセットや広告アセットの編集機能<br>3. Sandboxアセットの種類を拡充  |  Sandbox Toolkit v2.0.0（アルファ版）  |
|  2024/07/26  |  **ドキュメント更新**| 対応バージョンについて追記 |
|  2024/04/05  |  **ドキュメント更新**| サンプルアプリプロジェクトの開発チュートリアルを公開 |
|  2024/03/06  |  **ドキュメント更新**|対応VersionをUnity Editor 2021.3.35/SDK 2.3.2に統一|
|  2024/01/30  |  **4th Release** <br> Rendering Toolkitにテクスチャ調整機能を追加、SDKのテクスチャ結合機能に対応　<br> PLATEAU Utilitiesを追加 <br> AR Extensionsにテンプレートを追加  <br> サンプルシーン: AR City Miniatureのリリース|  Sandbox Toolkit v1.0.0 <br> Rendering Toolkit v1.0.0 <br> Maps Toolkit v1.0.0 <br> AR Extensions v1.0.0 <br> AR City Miniature v1.0.0 |
|  2023/12/25  |  **3rd Release** <br> Sandbox Toolkitにおけるカメラインタラクション機能、その他調整　　<br>MapsToolkitを別パッケージ化 <br>Rendering Toolkitに頂点カラー機能を追加  <br>AR Extensionsにマーカー位置合わせ機能を追加 <br> サンプルシーン: Urban Scape, City Rescue Multiplay, AR Treasure Mapのリリース|  Sandbox Toolkit v0.3.0（ベータ版） <br> Rendering Toolkit v0.3.0（ベータ版）<br> Maps Toolkit v0.3.0（ベータ版）<br> AR Extensions v0.3.0（ベータ版）<br> Urban Scape v1.0.0 <br> City Rescue Multiplay v1.0.0 <br> AR Treasure Map v1.0.0|
|  2023/10/28  |  **2nd Release** <br> MapsToolkitのリリース <br> AR Extensionsのリリース <br> Rendering Toolkitにおける雲量調整、ポストエフェクトの追加  |  Maps Toolkit v0.2.1（ベータ版）<br> AR Extensions v0.2.1（ベータ版） <br> Rendering Toolkit v0.2.1（ベータ版）   |
|  2023/09/15  |  Windowsにおけるビルドの不具合修正  |  Rendering Toolkit v0.1.1（ベータ版） <br> Sandbox Toolkit v0.1.1（ベータ版）  |
|  2023/07/23  |  **1st Release**  |  Rendering Toolkit v0.1.0（ベータ版） <br> Sandbox Toolkit v0.1.0（ベータ版）  |

## [PLATEAU SDK-Rendering-Toolkit](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/tree/main/PlateauToolkit.Rendering)

PLATEAUの3D都市モデルのグラフィックスを向上させる処理を行います。

環境設定、テクスチャの自動作成、LOD設定等の機能をGUI上で提供します。

https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/85cb8331-bb26-470c-ad69-33c56ffe9143

### リリース済の機能
- 環境システムの設定
- テクスチャ自動生成
- LOD機能
- 雲の量を調整する機能
- ポストエフェクトを追加する機能
- 頂点カラーの設定機能
- テクスチャ調整機能

使い方は[こちら](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/tree/main/PlateauToolkit.Rendering)

## [PLATEAU SDK-Sandbox-Toolkit](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/tree/main/PlateauToolkit.Sandbox)

PLATEAUの3D都市モデルを用いたゲーム開発、映像製作、シミュレーション実行などを支援します。  
乗り物、人、プロップなどの配置及び操作、トラックの設定機能などをGUI上で提供します。  

https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/ba417e2b-5621-4f75-95d0-27ce78a1ba5d

### リリース済の機能
- トラック（道路）の配置機能
- 人物エージェントの配置機能
- 乗り物エージェントの配置機能
- プロップ（施設器具等）の配置機能
- カメラインタラクション機能
  
使い方は[こちら](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/tree/main/PlateauToolkit.Sandbox)


## [PLATEAU SDK-Utilities](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/tree/main/PlateauToolkit.Utilities)

3D都市モデルの選択、高さや位置の調整などの編集の際に役立つ機能を提供します。 

### リリース済の機能
- メッシュレンダラーの選択機能
- メッシュ頂点の平面化機能
- 選択地物の整列機能
- プレハブへのライトマップ設定機能
  
使い方は[こちら](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/tree/main/PlateauToolkit.Utilities)

## [PLATEAU SDK-Maps-Toolkit for Unity](https://github.com/Project-PLATEAU/PLATEAU-SDK-Maps-Toolkit-for-Unity)
PLATEAUの3D都市モデルを利用したGIS開発向けツールキットです。Cesium SDK for Unityと連携してPLATEAUの3Dモデルをグローバルな地形モデルに配置することが可能です。また、BIMモデル（IFCファイル）やGISデータ（シェープファイル及びGeoJSON）を読み込んでCesium for Unity上に配置することが可能です。

https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/127069970/5ab8102d-046d-4423-9f7a-7a2029fca630

### リリース済の機能
- Cesium for Unityとの連携
- BIMモデルとの連携（IFCファイルの読み込み）
- GISデータとの連携

[こちら](https://github.com/Project-PLATEAU/PLATEAU-SDK-Maps-Toolkit-for-Unity)のリポジトリからご利用ください。

## [PLATEAU SDK-AR-Extensions for Unity](https://github.com/Project-PLATEAU/PLATEAU-SDK-AR-extensions-for-Unity)
PLATEAUの3D都市モデルを活用したARアプリ開発ツールです。AR空間に3Dモデルを配置し、位置のずれや地面の高さを調整できます。さらに、3Dオブジェクトを遮蔽するオクルージョン機能も備えています。ストリーミングにより配置された3D都市モデルを用いたARアプリ開発も可能です。
AR ExtensionsはPLATEAU SDK-Toolkitsを前提としたエクステンションとして構築されています。<br>


https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/127069970/de97d7a9-d6cf-45b6-877a-51bf6259237b

### リリース済の機能

- Geospatial APIを用いた3D都市モデルの位置合わせ機能
- ARマーカーを用いた3D都市モデルの位置合わせ機能
- ARオクルージョン機能

[こちら](https://github.com/Project-PLATEAU/PLATEAU-SDK-AR-Extensions-for-Unity)のリポジトリからご利用ください。<br>

# セットアップ環境

## 検証済環境
### 推奨OS環境
- Windows 11
- macOS Ventura 13.2

### Unity バージョン
- 動作確認環境：Unity 2021.3.35、Unity 2022.3.25
- 推奨：Unity 2021.3.35以上

### レンダリングパイプライン
- URP
- HDRP

**Built-in Rendering Pipelineでは動作しません。**

## PLATEAU SDKバージョン
- [PLATEAU SDK for Unity v2.3.2](https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity/releases/tag/v2.3.2)以上
    - v2.3.1以前のバージョンを使用する場合、SDK-Toolkitsの一部機能が利用できません。

## 導入手順

### 1. Unityでのプロジェクト作成

「3D(URP)」もしくは「3D(HDRP)」のテンプレートから Unity プロジェクトを作成してください。  

<p align="center">
<img width="493" alt="toolkits_setup_select_urp" src="/Documentation~/Images/toolkits_setup_select_urp.png">
</p>


### 2. PLATEAU SDK for Unityのインストール

PLATEAU SDK-Toolkits for Unityを利用するにあたり、事前にPLATEAU SDKのインストールが必要となります。  
TarballかGitHub URLからインストールをする必要があります。詳細はPLATEAU SDKのドキュメンテーションをご参照ください。  
[PLATEAU SDK for Unity利用マニュアル](https://project-plateau.github.io/PLATEAU-SDK-for-Unity/)

PLATEAU SDKを利用し、3D都市モデルをシーン上へ配置してください。

### 3. PLATEAU SDK-Toolkits for Unity のインストール

1. Unityエディタを開き、「Window」メニューから「Package Manager」を選択します。
2. Package Manager ウィンドウが開いたら、右上にある「＋」ボタンを押下します。
3. ドロップダウンメニューから「Add package from tarball...」を選択します。
4. ファイル選択ダイアログが開いたら、インストールしたいパッケージの .tar.gz ファイルを選択します。
<p align="center">
<img width="493" alt="toolkits_setup_install_tarball" src="/Documentation~/Images/toolkits_setup_install_tarball.png">
</p>

新しいプロジェクトでPLATEAU SDK-Toolkits for Unityをインストールする際は、入力システムについての確認ダイアログが表示されます場合があります。その場合は「Yes」を選択します（Unityエディターが再起動します）。

<p align="center">
<img width="500" alt="toolkits_setup_warning" src="/Documentation~/Images/toolkits_setup_warning.png">
</p>

ダウンロードは[こちら](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/releases/)

## ライセンス
- 本リポジトリはMITライセンスで提供されています。
- 本システムの開発は株式会社シナスタジアが行っています。
- ソースコードおよび関連ドキュメントの著作権は国土交通省に帰属します。

## 注意事項/利用規約
- 本ツールをアンインストールした場合、本ツールの機能で作成されたアセットの動作に不備が発生する可能性があります。
- 本ツールをアップデートした際は、一度 Unity エディターを再起動することを推奨しています。
- パフォーマンスの観点から、3km²の範囲に収まる3D都市モデルをダウンロード・インポートすることを推奨しています。
- インポートする範囲の広さや地物の種類（建物、道路、災害リスクなど）が量に比例して処理負荷が高くなる可能性があります。
- 本リポジトリの内容は予告なく変更・削除される可能性があります。
- 本リポジトリの利用により生じた損失及び損害等について、国土交通省はいかなる責任も負わないものとします。
