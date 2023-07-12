![Toolkits_MainVisual_v2](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/79615787/e4c3ab6c-7c6c-4968-bce1-2c958659b50e)
# PLATEA-SDK-Toolkits-for-Unity

PLATEAU-SDK-Toolkits-for-Unityは、PLATEAUの提供する「3D都市モデル」のデータを利用したUnity上でのアプリケーション開発を支援するツールキット群です。  
**PLATEAU-SDK-Toolkits-for-Unityは[PLATEAU SDK for Unity](https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity)を前提とするアドオンです。**

## 4つのToolkit
PLATEAU-SDK-Toolkits-for-Unityは以下の4つのToolkitから構成されます。  
2023年度の開発はアジャイル方式で行われます。  
**年度内に3回程度、各Toolkitのベータ版をリリースし、ユーザの皆様からのフィードバックを開発に反映する予定です。**  
ベータ版についてのご意見やバグ報告等は、本リポジトリのIssues又はPull requestsをご利用ください。  

- [Rendering Toolkit](#RenderingToolkit)
- [Sandbox Toolkit](#SandboxToolkit)
- Map Toolkit
- AR Toolkit


### 更新履歴

|  2023/07/23  |  1st Release  |  Rendering Toolkit v0.1.0（ベータ版） <br> Sandbox Toolkit v0.1.0（ベータ版）  |
| :--- | :--- | :--- |


## RenderingToolkit

PLATEAUの3D都市モデルのグラフィックスを向上させる処理を行います。  
環境設定、テクスチャの自動作成、LOD設定等の機能をGUI上で提供します。  

<p align="center">
<img width="500" alt="スクリーンショット 2023-06-26 12 24 48" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/2352f906-157c-46d1-99e0-aa9413e49337">
</p>

### リリース済の機能
- 環境システムの設定
- テクスチャ自動生成
- LOD機能

使い方は[こちら](https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/blob/main/rendering_toolkit.md)

## SandboxToolkit

PLATEAUの3D都市モデルを用いたゲーム開発、映像製作、シミュレーション実行などを支援します。
乗り物、人、プロップなどの配置及び操作、トラックの設定機能などをGUI上で提供します。

<p align="center">
<img width="500" alt="スクリーンショット 2023-06-26 12 22 46" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/a33dad18-02e3-4baa-a1d4-918ceefb83eb">
</p>

### リリース済の機能
- Tracks(道路)の配置機能
- 人物エージェントの配置機能
- 乗り物エージェントの配置機能
- Props(施設器具等)の配置機能
  
使い方は[こちら](https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/blob/main/sandbox_toolkit.md)


## 検証済環境
### 推奨OS環境
- Windows11
- macOS Ventura 13.2

### Unity Version
- 2021.3.27f1(LTS)

### Rendering Pipeline
- URP
- HDRP

**Built-in Rendering Pipelineでは動作しません。**

## PLATEAU SDKバージョン
- [version 1.1.5](https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity/releases)

## 導入手順

### 1. Unityでのプロジェクト作成

Unity Version 2021.3.27f1(LTS)により新たにUnityプロジェクトを作成してください。  
その際のテンプレートとして「3D(URP)」もしくは「3D(HDRP)」を選択してください。

<p align="center">
<img width="500" alt="スクリーンショット 2023-07-11 13 24 11" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/135de29f-623a-46dd-a895-f335b5eff08b">
</p>


### 2. PLATEAU SDK for Unityのインストール

PLATEAU SDK-Toolkits for Unityを利用するにあたり、事前にPLATEAU SDKのインストールが必要となります。  
TarballかGithub URLからインストールをする必要があります。詳細はPLATEAU SDKのドキュメンテーションをご参照ください。  
[PLATEAU SDK for Unity利用マニュアル](https://project-plateau.github.io/PLATEAU-SDK-for-Unity/)

PLATEAU SDKを利用し、3D都市モデルをUnityシーン上へ配置してください。

### 3. PLATEAU-SDK-Toolkits-for-Unity のインストール

1. Unityエディタを開き、「Window」メニューから「Package Manager」を選択します。
2. 「Package Manager」ウィンドウが開いたら、右上にある「＋」ボタンをクリックします。
3. ドロップダウンメニューから「Add package from tarball...」を選択します。
4. ファイル選択ダイアログが開いたら、インストールしたいパッケージの.tar.gzファイルを探し、選択します。

[ダウンロードリンクはこちら](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/releases/tag/release)
<p align="center">
<img width="500" alt="スクリーンショット 2023-06-26 12 31 50" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/f40cd78c-2f9c-4302-a70f-f2f4329d09b6">
</p>

## ライセンス
- 本リポジトリはMITライセンスで提供されています。
- 本システムの開発はユニティ・ジャパン・テクノロジー社が行っています。
- ソースコードおよび関連ドキュメントの著作権は国土交通省に帰属します。

## 注意事項/利用規約
- 本ツールはベータバージョンです。バグ、動作不安定、予期せぬ挙動等が発生する可能性があり、動作保証はできかねますのでご了承ください。
- 処理をしたあとにToolkitsをアンインストールした場合、建物の表示が壊れるなど挙動がおかしくなる場合がございます。
- パフォーマンスの観点から、3D都市モデルをダウンロード・インポートする際は、3㎞2範囲内とすることを推奨しています。
- インポートエリアの広さや地物の種類（建物、道路、災害リスクなど）が増えると処理負荷が高くなる可能性があります。
- 本リポジトリの内容は予告なく変更・削除する可能性があります。
- 本リポジトリの利用により生じた損失及び損害等について、国土交通省はいかなる責任も負わないものとします。
