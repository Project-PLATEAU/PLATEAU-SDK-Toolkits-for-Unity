# PLATEAU SDK Toolkits for Unity
![mainvisual](https://github.com/SeiTakeuchi/SeiTakeuchi-PLATEAU-SDK-Toolkits-for-Unity-drafts/assets/127069970/5d1f4c70-1178-4330-90f7-65169b965669)



PLATEAU SDK-Toolkits for Unityは、PLATEAUの提供する「3D都市モデル」のデータを利用したUnity上でのアプリケーション開発を支援するツールキット群です。  
**PLATEAU SDK-Toolkits for Unityは[PLATEAU SDK for Unity](https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity)を前提とするアドオンです。**

[ダウンロードリンクはこちら](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/releases/tag/v0.2.1)


## Toolkitsの構成要素
PLATEAU SDK-Toolkits for Unityは以下の4つのコンポーネントから構成されます。
2023年度の開発はアジャイル方式で行われます。  
**年度内に4回程度、各Toolkitのベータ版をリリースし、ユーザの皆様からのフィードバックを開発に反映する予定です。**  
ベータ版についてのご意見やバグ報告等は、本リポジトリのIssues又はPull requestsをご利用ください。  

- [Rendering Toolkit（本リポジトリ)](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/blob/main/rendering_toolkit.md)
- [Sandbox Toolkit（本リポジトリ)](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/blob/main/sandbox_toolkit.md)
- [Map Toolkit](https://github.com/unity-shimizu/PLATEAU-SDK-Maps-Toolkit-for-Unity)
- [AR Extensions](https://github.com/Project-PLATEAU/PLATEAU-SDK-AR-extensions-for-Unity)

また、Release3に伴い、Toolkitを活用した下記の４種類のサンプルアプリプロジェクトを提供しています。<br>
下記リポジトリよりご利用ください。

- [AR City Miniature](https://github.com/unity-shimizu/PLATEAU-Toolkits-Sample-ARCityMiniature)
- [Urban Scape](https://github.com/unity-shimizu/PLATEAU-Toolkits-Sample-UrbanScape)
- [City Rescue Multi Play](https://github.com/unity-shimizu/PLATEAU-Toolkits-Sample-CityRescueMultiPlay)
- [AR Treasure Map](https://github.com/unity-shimizu/PLATEAU-Toolkits-Sample-ARTreasureMap)


### 更新履歴

|  2023/12/13  |  3rd Release <br> Sandbox Toolkitにおけるカメラインタラクション機能、その他調整　　<br>MapsToolkitを別パッケージ化  |    |
| :--- | :--- | :--- |
|  2023/10/28  |  2nd Release <br> MapsToolkitのリリース <br> Rendering Toolkitにおける雲量調整、ポストエフェクトの追加  |  Maps　Toolkit v0.2.1（ベータ版）  <br> Rendering Toolkit v0.2.1（ベータ版）   |
|  2023/09/15  |  Windows Buildに関する不具合の修正  |  Rendering Toolkit v0.1.1（ベータ版） <br> Sandbox Toolkit v0.1.1（ベータ版）  |
|  2023/07/23  |  1st Release  |  Rendering Toolkit v0.1.0（ベータ版） <br> Sandbox Toolkit v0.1.0（ベータ版）  |



## [Rendering Toolkit](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/blob/main/rendering_toolkit.md)

PLATEAUの3D都市モデルのグラフィックスを向上させる処理を行います。

環境設定、テクスチャの自動作成、LOD設定等の機能をGUI上で提供します。

https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/85cb8331-bb26-470c-ad69-33c56ffe9143


＜リリース済機能＞
### リリース済の機能
- 環境システムの設定
- テクスチャ自動生成
- LOD機能
- 雲の量を調整する機能
- ポストエフェクトを追加する機能

使い方は[こちら](https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/blob/main/rendering_toolkit.md)


## [Sandbox Toolkit]([https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity#renderingtoolkit](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/blob/main/sandbox_toolkit.md))

PLATEAUの3D都市モデルを用いたゲーム開発、映像製作、シミュレーション実行などを支援します。  
乗り物、人、プロップなどの配置及び操作、トラックの設定機能などをGUI上で提供します。  


https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/ba417e2b-5621-4f75-95d0-27ce78a1ba5d



### リリース済の機能
- Tracks(道路)の配置機能
- 人物エージェントの配置機能
- 乗り物エージェントの配置機能
- Props(施設器具等)の配置機能
  
使い方は[こちら](https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/blob/main/sandbox_toolkit.md)


## [PLATEAU SDK-Maps-Toolkit for Unity](https://github.com/unity-shimizu/PLATEAU-SDK-Maps-Toolkit-for-Unity)

PLATEAUの3D都市モデルを利用したGIS開発向けツールキットです。Cesium SDK for Unityと連携してPLATEAUの3Dモデルをグローバルな地形モデルに配置することが可能です。また、BIMモデル（IFCファイル）やGISデータ（シェープファイル及びGeoJSON）を読み込んでCesium for Unity上に配置することが可能です。<br>
[こちらのリポジトリ](https://github.com/unity-shimizu/PLATEAU-SDK-Maps-Toolkit-for-Unity)からご利用ください。


## [PLATEAU SDK-AR-Extensions for Unity](https://github.com/Project-PLATEAU/PLATEAU-SDK-AR-extensions-for-Unity)

PLATEAUの3D都市モデルを活用したARアプリ開発ツールです。AR空間に3Dモデルを配置し、位置のずれや地面の高さを調整できます。さらに、3Dオブジェクトを遮蔽するオクルージョン機能も備えています。Streamingにより配置された3D都市モデルを用いたARアプリ開発も可能です。
AR ExtensionsはToolkitsを前提としたエクステンションとして構築されています。<br>
[こちらのリポジトリ](https://github.com/Project-PLATEAU/PLATEAU-SDK-AR-extensions-for-Unity)からご利用ください。


# セットアップ環境




## 検証済環境
### 推奨OS環境
- Windows11
- macOS Ventura 13.2

### Unity Version
- Unity 2021.3.30
    - Unity 2021.3系であれば問題なく動作する見込みです。

### Rendering Pipeline
- URP
- HDRP

**Built-in Rendering Pipelineでは動作しません。**

## PLATEAU SDKバージョン
- [PLATEAU SDK for Unity v2.2.1-alpha](https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity/releases)

## 導入手順

### 1. Unityでのプロジェクト作成

Unity Version 2021.3.27f1(LTS)により新たにUnityプロジェクトを作成してください。  
その際のテンプレートとして「3D(URP)」もしくは「3D(HDRP)」を選択してください。

<p align="center">
<img width="493" alt="スクリーンショット 2023-07-12 19 18 33" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/b45abddf-fe60-4dbd-9127-910fcb916ed4">
</p>


### 2. PLATEAU SDK for Unityのインストール

PLATEAU SDK-Toolkits for Unityを利用するにあたり、事前にPLATEAU SDKのインストールが必要となります。  
TarballかGithub URLからインストールをする必要があります。詳細はPLATEAU SDKのドキュメンテーションをご参照ください。  
[PLATEAU SDK for Unity利用マニュアル](https://project-plateau.github.io/PLATEAU-SDK-for-Unity/)

PLATEAU SDKを利用し、3D都市モデルをUnityシーン上へ配置してください。

### 3. Cesium for Unity のインストール

PLATEAU SDK-ToolkitsではCesium for Unity が必要となるため、事前にインストールしていただく必要があります。下記のページでよりダウンロードしてください。Maps Toolkitではバージョンv1.6.3をサポートしています。

- [Cesium for Unity v1.6.3](https://github.com/CesiumGS/cesium-unity/releases/tag/v1.6.3)

ダウンロードした tgz ファイルは Maps Toolkit を使用する Unity プロジェクトのフォルダ内に配置することを推奨します。Unity プロジェクトのフォルダに配置することで、相対パスでパッケージを参照することができ、フォルダを移動したり別の環境での同じプロジェクトの利用が容易になります。Unityプロジェクト外を参照すると、絶対パスがmanifest.jsonに書き込まれることになり、少し不便になり、依存解決のエラーなどが将来的に発生してしまう可能性があります。

Windows > PackageManagerの「Add package from tarball…」を選択し、ダウンロードした Cesium for Unity の tgz ファイルを選択します。

<p align="center">
<img width="400" alt="top_manual_1_packagemanager" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/bf577dca-0d2f-4959-948a-0e8d8dfa899d">
</p>

### 4. PLATEAU SDK-Toolkits for Unity のインストール

1. Unityエディタを開き、「Window」メニューから「Package Manager」を選択します。
2. 「Package Manager」ウィンドウが開いたら、右上にある「＋」ボタンをクリックします。
3. ドロップダウンメニューから「Add package from tarball...」を選択します。
4. ファイル選択ダイアログが開いたら、インストールしたいパッケージの.tar.gzファイルを探し、選択します。
<p align="center">
<img width="493" alt="スクリーンショット 2023-07-12 19 18 42" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/b9076829-b7cb-45db-a5a1-5ce6dbc435b1">
</p>

新しいプロジェクトでPLATEAU SDK-Toolkits for Unityをインストールする際は、入力システムについての確認ダイアログが表示されます場合があります。その場合は「Yes」を選択してください。
Unityエディタが再起動します。

<p align="center">
<img width="500" alt="InputDialog" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/120234a9-1457-46f5-9a71-0c43febd44a2">
</p>

[ダウンロードリンクはこちら](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/releases/tag/v0.2.1)

## ライセンス
- 本リポジトリはMITライセンスで提供されています。
- 本システムの開発はユニティ・テクノロジーズ・ジャパン株式会社が行っています。
- ソースコードおよび関連ドキュメントの著作権は国土交通省に帰属します。

## 注意事項/利用規約
- 本ツールはベータバージョンです。バグ、動作不安定、予期せぬ挙動等が発生する可能性があり、動作保証はできかねますのでご了承ください。
- 処理をしたあとにToolkitsをアンインストールした場合、建物の表示が壊れるなど挙動がおかしくなる場合がございます。
- 本ツールをアップデートした際は、一度Unity エディタを再起動してご利用ください。
- パフォーマンスの観点から、3D都市モデルをダウンロード・インポートする際は、3㎞2範囲内とすることを推奨しています。
- インポートエリアの広さや地物の種類（建物、道路、災害リスクなど）が増えると処理負荷が高くなる可能性があります。
- 本リポジトリの内容は予告なく変更・削除する可能性があります。
- 本リポジトリの利用により生じた損失及び損害等について、国土交通省はいかなる責任も負わないものとします。
