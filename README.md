# PLATEAU SDK-Toolkits for Unity

<p align="center">
<img width="250" alt="スクリーンショット 2023-06-26 8 00 40" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/efd49976-5506-422f-b9a8-fe7132ef2377">
</p>

PLATEAU toolkit for Unityは、PLATEAUの3D都市モデルデータをUnityで扱うためのツールキットであり、主に以下の機能を提供しています。
このPLATEAU toolkit for Unityは[PLATEAU SDK for Unity](https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity)と併用して利用することで、
目的に応じたPLATREAUの3D都市モデルデータを利用したシステム開発を進めることができます。

＜リリース済の機能＞
- Rendering Toolkit
- Sandbox Toolkit

## Rendering Toolkit

PLATEAUの都市モデル・LODに適したグラフィックス向上処理を実現。テクスチャの自動作成・高解像度化や、時候に合わせた環境光などの調節機能を実現することで開発者がより手軽にシミュレーションを行えるような環境を提供します。

<p align="center">
<img width="500" alt="スクリーンショット 2023-06-26 12 24 48" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/2352f906-157c-46d1-99e0-aa9413e49337">
</p>

＜リリース済の機能＞
- テクスチャ自動生成
- LOD機能
- シミュレーション

使い方は[こちら](https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/blob/main/rendering_toolkit.md)

## Sandbox Toolkit

3D都市モデルを用いてゲームや映像などを直感的に開発できるように、カメラや乗り物、人などの導線の設計、インタラクションのためのコライダー設定などを簡易的に行えるような環境を実現します。

<p align="center">
<img width="500" alt="スクリーンショット 2023-06-26 12 22 46" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/a33dad18-02e3-4baa-a1d4-918ceefb83eb">
</p>

＜リリース済の機能＞
- Tracks(道路)の配置機能
- 人物エージェントの配置機能
- 乗り物エージェントの配置機能
- Props(施設器具等)の配置機能
  
使い方は[こちら](https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/blob/main/sandbox_toolkit.md)


# 検証済環境
## 推奨OS環境
- Windows
- MacOS

## Unity Version
- 2021.3.27f1(LTS)

## Rendering Pipeline
- URP
- HDRP

## PLATEAU SDKバージョン
- [version 1.1.5](https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity/releases)

# 利用手順

### PLATEAU SDK for Unity

PLATEAU SDK-Toolkits for Unityを利用するにあたり、事前にPLATEAU SDKのインストールと対象となる地域の3D都市モデルのダウンロードが必要となります。

TarballかGithub URL からのインストールのみなので手動でインストールする必要があります。詳細はPlateau SDKのドキュメンテーションをご参照ください。
[PLATEAU SDK for Unity](https://project-plateau.github.io/PLATEAU-SDK-for-Unity/)

PLATEAU SDKを利用し、対象となる地域の3D都市モデルのダウンロードとUnityへの展開を行なってください。

## PLATEAU SDK-Toolkits for Unity のインストール

1. Unityエディタを開き、「Window」メニューから「Package Manager」を選択します。
2. 「Package Manager」ウィンドウが開いたら、右上にある「＋」ボタンをクリックします。
3. ドロップダウンメニューから「Add package from tarball...」を選択します。
4. ファイル選択ダイアログが開いたら、インストールしたいパッケージの.tar.gzファイルを探し、選択します。

[ダウンロードリンクはこちら（公開後に更新します。）](https://project-plateau.github.io/PLATEAU-SDK-for-Unity/)

<p align="center">
<img width="500" alt="スクリーンショット 2023-06-26 12 31 50" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/f40cd78c-2f9c-4302-a70f-f2f4329d09b6">
</p>

# ライセンス
- 本リポジトリはMITライセンスで提供されています。
- ソースコードおよび関連ドキュメントの著作権は国土交通省に帰属します。

# 注意事項
- 処理をしたあとにアンインストールした場合、建物の表示が壊れるなど挙動がおかしくなる場合がございます。この場合は、3D都市モデルのダウンロードなどから行なっていただく必要性がございます。
- 地物の取り込みはＰＣで使う場合は3㎞範囲以内を推奨しています。
- エリアの広さや地物の種類（建物、道路、災害リスクなど）が増えると処理が負荷が高くなる可能性があります。
- 現在はベータバージョンとなっており、動作保証はできかねますのでご了承ください。
- 本リポジトリの内容は予告なく変更・削除する可能性があります。
- 本リポジトリの利用により生じた損失及び損害等について、国土交通省はいかなる責任も負わないものとします。
