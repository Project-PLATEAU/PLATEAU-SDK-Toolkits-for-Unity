### PLATEAU-SDK-Toolkits-for-Unity
# Sandbox Toolkit 利用マニュアル
PLATEAUの3D都市モデルを用いたゲーム開発、映像製作、シミュレーション実行などを支援します。  
乗り物、人、プロップなどの配置及び操作、トラックの設定機能などをGUI上で提供します。  

# 利用手順

PLATEAU-SDK-Toolkits-for-Unityのインストール後、上部のメニューより「PLATEAU」>「PLATEAU Toolkit」>「Rendering Toolkit」を選択します。

<img width="371" alt="スクリーンショット 2023-06-27 17 45 56" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/03b129e4-eed2-4096-8cf7-9679ae7652e0">

Sandbox Toolkitのメインメニューが表示されます。

<img width="500" alt="スクリーンショット 2023-06-27 19 36 19" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/503a029e-02ab-428c-bdc7-cb4c6e9b2416">

## 全般
Sandbox Toolkitの各機能を利用するためには、**シーンに配置された3D都市モデルのオブジェクトにCollider (コライダー) コンポーネントを設定する必要があります。**

## 1. トラックの作成
トラックの作成を行うことで、直感的に3D都市モデルの中にオブジェクト移動用の経路を作成することができます。

1. 「ツール」メニューの中で「新しいトラックを作成」ボタンを押下します。

<img width="674" alt="スクリーンショット 2023-06-27 19 48 03" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/8015dac6-e258-4616-a06c-a8e7c1f0c562">

2. 編集モードに入り、Sceneビューで専用の配置アイコンがマウスカーソルの位置に表示されます。
<img width="800" alt="スクリーンショット 2023-06-27 19 51 24" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/baf6e7a2-6abe-47b9-b482-806619bf1d2a">

3. その状態でSceneビュー内において、対象となる地表上でクリックをすると、トラック生成のポイントが作成されます。
4. そのまま続けて地表上をクリックしていくと、ポイントが生成され、ポイントに合わせてトラックが生成されます。

<img width="500" alt="スクリーンショット 2023-06-27 19 50 16" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/30425292-f11d-4311-990c-ee1c2249cb49">

5. 最後に視点と同じポイントをクリックすると、ループ可能なトラックが作成できます。

<img width="500" alt="スクリーンショット 2023-06-27 19 56 04" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/036036ca-02de-4b0b-adfe-b325e0f66986">

各モデル配置における「配置ツールを起動ボタン」を押下し、配置位置を「トラックに沿って配置」を選択しすると、作成したトラックに沿ってモデルを配置することができます。　

※配置したHumanとVehicleを動作させるためにはPlayModeを実行する必要があります。

<img width="598" alt="スクリーンショット 2023-06-27 22 11 06" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/040788ff-00a4-4330-91ac-6307f97dbddd">

### トラックの速度制限
トラックには速度制限を設定することができます。
速度制限は `PlateauSandboxTrack` のインスペクターから設定するか、速度制限設定ツールからシーン上で一括設定することができます。

- インスペクターから設定する場合は `Has Speed Limit` にチェックボックスをいれて、制限速度（m/s）を `Speed Limit` に設定してください。
  
![image](https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/122778529/3189450e-b09e-458a-87b5-c449d21c8ac6)

- 速度制限設定ツールはPLATEAU Sandbox Toolkitのトラックタブから起動できます。
 
![image](https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/122778529/b7793cdb-f883-4245-9b46-bd2b206a75ae)

- 制限速度ツールでは、シーンの各トラック上に設定ウィンドウが表示され、このウィンドウから制限速度を設定することができます。

![speed_limit_tool](https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/122778529/48567486-cde5-4a6a-9158-6785a1525dce)


## Humanの作成
専用のメニューから人型のモデルを選択し、3D都市モデルの中に配置できます。

1. 「アセット」メニューの中で対象となるモデルをクリックし選択します。

<img width="500" alt="スクリーンショット 2023-06-27 21 21 07" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/9142b44d-1783-4c32-acc3-d56e0e582ebb">

2. 配置ツールを起動ボタンを押下し、メニューからSceneビューへ対象モデルをドラッグ&ドロップします。

<img width="800" alt="スクリーンショット 2023-06-27 21 54 41" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/79fb89da-05f2-4da4-8ff5-1d3f82e1a262">

## Vehicleの作成

専用のメニューから乗り物型のモデルを選択し、3D都市モデルの中に配置できます。

1. 「アセット」メニューの中で対象となるモデルをクリックし選択します。

<img width="500" alt="スクリーンショット 2023-06-27 22 07 50" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/ae02e0c1-23c4-48c4-9a0c-16a78ad1ec34">

2. 配置ツールを起動ボタンを押下し、メニューからSceneビューへ対象モデルをドラッグ&ドロップします。

<img width="800" alt="スクリーンショット 2023-06-27 22 12 27" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/77335e58-a83f-46fa-8d5e-aad760a1eeab">


## Propsの作成

専用のメニューから施設器具型のモデルを選択し、3D都市モデルの中に配置できます。

1. 「アセット」メニューの中で対象となるモデルをクリックし選択します。

<img width="680" alt="スクリーンショット 2023-06-27 22 16 05" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/f5f0f467-e5e8-4d06-8469-cc241665b3db">


2. 配置ツールを起動ボタンを押下し、メニューからSceneビューへ対象モデルをドラッグ&ドロップします。

<img width="800" alt="スクリーンショット 2023-06-27 22 17 26" src="https://github.com/Project-PLATEAU/PLATEAU-Unity-Toolkit/assets/137732437/bb9b09a4-a5f1-4046-a9ea-9fdf458a8783">

## 関連API
SandboxToolkitの開発において、Unity の以下のAPI を使用しています。
1. [Splines](https://docs.unity3d.com/Packages/com.unity.splines@2.1/manual/index.html)
2. [PackageManager](https://docs.unity3d.com/ja/2021.3/Manual/class-PackageManager.html)
3. [Preferences](https://docs.unity3d.com/2021.3/Documentation/Manual/Preferences.html)
4. [UI Toolkit](https://docs.unity3d.com/2021.3/Documentation/Manual/UIElements.html)
5. [Raycasters](https://docs.unity3d.com/2021.3/Documentation/Manual/Raycasters.html)
6. [Collision](https://docs.unity3d.com/2021.3/Documentation/Manual/collision-section.html)
7. [Scenes](https://docs.unity3d.com/2021.3/Documentation/Manual/CreatingScenes.html)
