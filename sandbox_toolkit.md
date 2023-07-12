### PLATEAU-SDK-Toolkits-for-Unity
# Sandbox Toolkit 利用マニュアル
PLATEAUの3D都市モデルを用いたゲーム開発、映像製作、シミュレーション実行などを支援します。  
乗り物、人、プロップなどの配置及び操作、トラックの設定機能などをGUI上で提供します。  

# 利用手順

PLATEAU-SDK-Toolkits-for-Unityのインストール後、上部のメニューより「PLATEAU」>「PLATEAU Toolkit」>「Rendering Toolkit」を選択します。

<img width="372" alt="スクリーンショット 2023-07-12 19 20 22" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/58356a9f-7b52-49ce-9d5a-bbe55b56be87">

Sandbox Toolkitのメインメニューが表示されます。

<img width="497" alt="スクリーンショット 2023-07-12 19 39 14" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/89ed4c23-ebe9-4dec-b6c8-8bd2b78d39f9">


## 全般
Sandbox Toolkitの各機能を利用するためには、**シーンに配置された3D都市モデルのオブジェクトにCollider (コライダー) コンポーネントを設定する必要があります。**

## 1. トラックの作成
トラックの作成を行うことで、直感的に3D都市モデルの中にオブジェクト移動用の経路を作成することができます。

1. 「ツール」メニューの中で「新しいトラックを作成」ボタンを押下します。
2. 
<img width="670" alt="スクリーンショット 2023-07-12 19 39 21" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/54d30e78-bbf1-497b-8d29-8f59656cfbd5">


3. 編集モードに入り、Sceneビューで専用の配置アイコンがマウスカーソルの位置に表示されます。

<img width="796" alt="スクリーンショット 2023-07-12 19 39 27" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/49029b7d-f4dc-49dc-a1e4-1a24cff34563">


4. その状態でSceneビュー内において、対象となる地表上でクリックをすると、トラック生成のポイントが作成されます。
5. そのまま続けて地表上をクリックしていくと、ポイントが生成され、ポイントに合わせてトラックが生成されます。

<img width="498" alt="スクリーンショット 2023-07-12 19 39 34" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/3f483c28-1573-4d89-8b67-571b2262568f">


5. 最後に視点と同じポイントをクリックすると、ループ可能なトラックが作成できます。

<img width="498" alt="スクリーンショット 2023-07-12 19 39 40" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/b66cdcb3-0317-4acc-9951-ccb04652a0dd">


各モデル配置における「配置ツールを起動ボタン」を押下し、配置位置を「トラックに沿って配置」を選択しすると、作成したトラックに沿ってモデルを配置することができます。　

※配置したHumanとVehicleを動作させるためにはPlayModeを実行する必要があります。

<img width="594" alt="スクリーンショット 2023-07-12 19 39 47" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/d4d27a36-8b65-42bd-85a7-14440c0c25cd">


### トラックの速度制限
トラックには速度制限を設定することができます。
速度制限は `PlateauSandboxTrack` のインスペクターから設定するか、速度制限設定ツールからシーン上で一括設定することができます。

- インスペクターから設定する場合は `Has Speed Limit` にチェックボックスをいれて、制限速度（m/s）を `Speed Limit` に設定してください。
  
<img width="482" alt="スクリーンショット 2023-07-12 19 39 54" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/f4c13c46-dce7-46d3-b52d-1e54cb36c407">


- 速度制限設定ツールはPLATEAU Sandbox Toolkitのトラックタブから起動できます。
 
<img width="407" alt="スクリーンショット 2023-07-12 19 39 59" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/bdc7a5a0-c714-4917-8544-5f6f8253a4e1">

- 制限速度ツールでは、シーンの各トラック上に設定ウィンドウが表示され、このウィンドウから制限速度を設定することができます。

<img width="1006" alt="スクリーンショット 2023-07-12 19 40 14" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/6f17b30f-1fdb-43a5-8d14-3ad136773be0">


## Humanの作成
専用のメニューから人型のモデルを選択し、3D都市モデルの中に配置できます。

1. 「アセット」メニューの中で対象となるモデルをクリックし選択します。

<img width="497" alt="スクリーンショット 2023-07-12 19 40 26" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/d68047f8-de01-4061-b961-f757b3ca6db0">


2. 配置ツールを起動ボタンを押下し、メニューからSceneビューへ対象モデルをドラッグ&ドロップします。

<img width="797" alt="スクリーンショット 2023-07-12 19 40 34" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/043449a7-334f-4b3a-90cc-35f41a437913">


## Vehicleの作成

専用のメニューから乗り物型のモデルを選択し、3D都市モデルの中に配置できます。

1. 「アセット」メニューの中で対象となるモデルをクリックし選択します。

<img width="498" alt="スクリーンショット 2023-07-12 19 40 39" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/09360166-5822-4fab-9a19-7e0fe4f8c771">


2. 配置ツールを起動ボタンを押下し、メニューからSceneビューへ対象モデルをドラッグ&ドロップします。

<img width="796" alt="スクリーンショット 2023-07-12 19 40 45" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/1def056b-84a4-4a9b-824b-2d3951c4027d">


## Propsの作成

専用のメニューから施設器具型のモデルを選択し、3D都市モデルの中に配置できます。

1. 「アセット」メニューの中で対象となるモデルをクリックし選択します。

<img width="677" alt="スクリーンショット 2023-07-12 19 40 52" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/fac5e124-6d66-4f43-95f6-4ce8fd73a7c2">


2. 配置ツールを起動ボタンを押下し、メニューからSceneビューへ対象モデルをドラッグ&ドロップします。

<img width="798" alt="スクリーンショット 2023-07-12 19 41 00" src="https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity/assets/137732437/a01a4ecc-8b85-486a-9159-126c584ee758">


## 関連API
Sandbox Toolkitの開発において、Unity の以下のAPIを使用しています。本ツールをカスタマイズしながら作りたい場合にはこちらを参考にしてください。
1. [Splines](https://docs.unity3d.com/Packages/com.unity.splines@2.1/manual/index.html)
2. [PackageManager](https://docs.unity3d.com/ja/2021.3/Manual/class-PackageManager.html)
3. [Preferences](https://docs.unity3d.com/2021.3/Documentation/Manual/Preferences.html)
4. [UI Toolkit](https://docs.unity3d.com/2021.3/Documentation/Manual/UIElements.html)
5. [Raycasters](https://docs.unity3d.com/2021.3/Documentation/Manual/Raycasters.html)
6. [Collision](https://docs.unity3d.com/2021.3/Documentation/Manual/collision-section.html)
7. [Scenes](https://docs.unity3d.com/2021.3/Documentation/Manual/CreatingScenes.html)
