# PLATEAU SDK-Rendering Toolkit for Unity

PLATEAUの都市モデル・LODに適したグラフィックス向上処理を実現。テクスチャの自動作成・高解像度化や、時候に合わせた環境光などの調節機能を実現することで開発者がより手軽にシミュレーションを行えるような環境を提供します。


# 利用手順

PLATEAU SDK-Toolkits for Unityのインストール後、上部のメニューより「PLATEAU」>「PLATEAU Toolkit」>「Rendering Toolkit」を選択します。

<img width="371" alt="スクリーンショット 2023-06-27 17 45 56" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/03b129e4-eed2-4096-8cf7-9679ae7652e0">

するとPLATEAU Toolkitのメインメニューが表示されます。

<img width="324" alt="スクリーンショット 2023-06-27 17 46 09" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/1bf3da51-c1ef-4056-9639-b7b8c6bd5002">



## 環境システムの設定
「環境システムの設定」では、シーンの時間帯や天候などを変更し、3D都市モデルを使った表示シミュレーションを行うことができます。

### 時間の変更
「Time of Day」欄のスライダーを動かすと、表示時間帯を変更することができます。
<img width="1000" alt="スクリーンショット 2023-06-27 17 56 09" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/c6bc6a65-9397-49f0-a195-2b7db44e003a">


### 天候の変更
「Snow」「Rain」「Cloud」バーを動かすことで天候を変更することが可能です。

※イメージ貼り付け予定


## 自動テクスチャの生成
「自動テクスチャの生成」では対象となる3D都市モデルの建物に対して、自動的にテクスチャを貼り付けることができ、見た目を綺麗にすることができます。

1. Unityの「Hierarchy」ビューより対象となる建物のGameObjectを選択してください。

<img width="500" alt="スクリーンショット 2023-06-27 18 20 09" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/2c7221aa-0756-488e-b485-d79b1ad7eb89">

2. 「自動テクスチャの生成」メニューの中の「テクスチャ生成」ボタンを押下してください。

3. テクスチャ作成の確認画面が表示されます。問題なければ「はい」ボタンを押下してください。

<img width="500" alt="スクリーンショット 2023-06-27 18 21 43" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/f6adc2ee-28ee-4ab8-a011-04fca260cc5f">

4. テクスチャが自動的に生成され、モデルの見た目が変更されます。
<img width="500" alt="スクリーンショット 2023-06-27 18 24 18" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/4f906794-cb24-4959-9639-ab908e0538e1">

この状態で「環境システムの設定」メニューから「「Time of Day」を夜にすると、窓のライトが表示されます。

<img width="500" alt="スクリーンショット 2023-06-27 18 32 13" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/6dbaa67e-0ca4-4eac-82a6-3a81412bfcda">



## シーンの保存
テクスチャ生成などを行ったら右下にあるSceneビューの右下にある「シーンの保存」ボタンを押下し、保存してください。また、やり直したい場合は最後に保存した時点に戻ることも可能です。

<img width="196" alt="スクリーンショット 2023-06-27 18 03 40" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/81bf706e-7902-41b9-afac-33cdbb301faf">







