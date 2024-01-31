# PLATEAU Utilities利用マニュアル

PLATEAUの3D都市モデルの選択、高さや位置の調整などの編集の際に役立つ機能を提供します。



- [利用手順](#利用手順)
  * [1. メッシュレンダラーの選択](#1-メッシュレンダラーの選択)
  * [2. メッシュ頂点の平面化](#2-メッシュ頂点の平面化)
  * [3. 選択地物の整列](#3-選択地物の整列)
  * [4. プレハブへのライトマップの設定](#4-プレハブへのライトマップの設定)
    + [4-1. 利用例](#4-1-利用例)
  * [5. 関連API](#5-関連api)

# 利用手順

PLATEAU SDK-Toolkits for Unityのインストール後、上部のメニューより PLATEAU > PLATEAU Toolkit > Utilities を選択します。

<img width="400" alt="utilites_open_menu" src="../Documentation~/Utilities Images/utilites_open_menu.png">

PLATEAU Utilities ウィンドウのメインメニューが表示されます。

<img width="400" alt="utilities_main_ui" src="../Documentation~/Utilities Images/utilities_main_ui.png">

## 1. メッシュレンダラーの選択

メッシュレンダラーの選択機能では、地物を一括選択することができます。

3D都市モデルのビル群に対して、オートテクスチャリング機能を一括で適用する場合など、ビル群をまとめて選択したい場合に便利な機能です。

Unity標準のヒエラルキー検索からの一括選択と比較して、以下の違いがあります。

1. **無効オブジェクトの除外**:
    
    Unity標準のヒエラルキー検索では、無効化されているオブジェクトも含まれてしまうため、シーンで有効な3D都市モデルの地物だけを一括選択することが困難です。「メッシュレンダラーの選択」では、現在のシーンで有効なオブジェクトのみを対象にしており、これにより無効オブジェクトを自動的に除外して一括選択を行うことができます。
    
2. **選択オブジェクト階層内での一括選択**:
PLATEAU3D都市モデルでは、Unityの一般的なLODシステムとは異なる、独自のLOD概念が用いられています。このシステムでは、LODレベルが高くなるにつれて、モデルはより詳細になります。3D都市モデルのトップノードの下には、LOD1、LOD2、LOD3という階層が存在し、それぞれに同名の地物が配置されています。標準のUnityのヒエラルキー検索では、これらの同名地物が区別なく表示されてしまうため、特定のLODレベルの地物だけを選択することが困難です。「メッシュレンダラーの選択」機能では、親オブジェクトの階層からの選択を行うことで、任意のLODレベルの地物のみを選択することが可能です。例えば、LOD1のトップ階層のオブジェクトから実行することで、現在シーンに表示されているLOD1の地物だけを選択することができます。

### 使用方法

一括選択したい地物の親オブジェクトをヒエラルキーから選択します。

PLATEAU Utilities ウィンドウのパネル最上段の「地物種別の接頭語」の欄に選択を行いたい地物を表す接頭語を入力し、「メッシュレンダラーの選択」を押下してください。この例では建築物を選択するため、”bldg”を指定します。

<img width="800" alt="utilities_meshrenderer_select" src="../Documentation~/Utilities Images/utilities_meshrenderer_select.png">

親オブジェクト配下の該当するモデルが一括で選択状態になります。

<img width="800" alt="utilities_meshrenderer_selected" src="../Documentation~/Utilities Images/utilities_meshrenderer_selected.png">

地物の接頭語については以下のドキュメントの”3.2.3 _ 地物ごとのフォルダ分け”をご参照ください。

**[TOPIC 3｜3D都市モデルデータの基本[1/4]｜3D都市モデルの入手方法とデータ形式](https://www.mlit.go.jp/plateau/learning/tpc03-1/)**

## 2. メッシュ頂点の平面化

PLATEAU 3D都市モデルの地面メッシュ（DEM）上のメッシュ頂点を平面化する機能です。

地形モデルは1m、5m、10mなどのメッシュで作成されています。現実の都市空間で急な高低差がある場所のモデルには、起伏や凹みが生じていることがある為、地面上を移動する人や車を配置すると地面にめり込むことがあります。VRアプリケーションなどアイレベル（人の目線の高さ）でのユースケースでは地面を平面化することで地面上での配置物の操作が容易になります。

1. 平面化を行いたいメッシュをヒエラルキーから選択してください。複数のメッシュを平面化したい場合には「メッシュレンダラーの選択」機能を利用すると簡単に一括選択を行えます。
2. 平面化を行う高さを「高さ」の欄に入力し、「メッシュ頂点の平面化」を押下するとメッシュ頂点が平面化されます。

<img width="800" alt="utilities_flatten" src="../Documentation~/Utilities Images/utilities_flatten.png">

## 3. 選択地物の整列

地物の底面の高さを原点に整列させるための機能です。

3D都市モデルの地物は地面メッシュ（DEM）に沿ってそれぞれ高さを持っていますが、LoD1及びLoD2の道路（TRAN）のメッシュは高さを持ちません。そのため、道路と組み合わせた際に地物が浮いたりするなどの問題が発生することがあります。特に、VRアプリケーションなどアイレベル（人の目線の高さ）でのユースケースで調整が必要になる場合に有効な機能です。

1. 整列を行いたいメッシュをヒエラルキーから選択してください。複数のメッシュを整列したい場合には「メッシュレンダラーの選択」機能を利用すると簡単に一括選択を行えます。
2. 整列を行う高さを「高さ」の欄に入力し、「選択地物の整列」を押下すると地物が同一の高さに整列されます。

<img width="800" alt="utilities_flatten" src="../Documentation~/Utilities Images/utilities_flatten.png">

## 4. プレハブへのライトマップの設定

シーンのライトマップをプレハブに保存する機能です。

モバイルアプリではリアルタイムにライティングを行うことは一般的に高負荷な為、事前計算されたライトマップの仕様が推奨されます。また、ARアプリケーションではプレハブを呼び出す実装が一般的であるため、PLATEAUの3D都市モデルをプレハブ化する必要がありました。しかし、Unityの標準機能ではシーンのライトマップをプレハブに適用する機能がないため、こちらのカスタマイズ機能を用意しています。

事前にシーンにライトマップを作成し、適用したいプレハブをヒエラルキー上で選択した後に「プレハブへのライトマップの設定」を押下してください。

<img width="800" alt="utilites_prefab_lightmap" src="../Documentation~/Utilities Images/utilites_prefab_lightmap.png">

ライトマップについては[こちらのドキュメント](https://docs.unity3d.com/ja/2021.3/Manual/Lightmappers.html)をご参照ください。

また、ライトマップ作成の具体的な手順は以下の 4-1. 利用例 で解説します。

### 4-1. 利用例

この機能の利用にはシーンへのライトマップ設定など事前の手順が必要になるため、以下で例を通して手順を解説します。

#### 4-1-1. ライトマップとは

ライトマップは、3Dオブジェクトやシーンの照明情報を保存するテクスチャです。照明効果をリアルタイムで計算するのではなく、事前に計算された照明情報をテクスチャに「ベイク」し、3Dモデルに適用します。このプロセスにより、特にパフォーマンスが重要なモバイルデバイス等で、リッチな照明効果を3Dシーンに提供します。

<img width="800" alt="prefab_lightmapmain_00" src="../Documentation~/Utilities Images/prefab_lightmapmain_00.png">

**ライトマップの作成**

1. **事前計算**: 光の計算をシミュレートしてライトマップを生成します。
2. **マッピング**: 生成されたライトマップを3Dモデルにテクスチャとして適用します。

**ライトマップの利点**

- **効率性**: リアルタイムレンダリング時の計算負荷を大幅に軽減します。
- **品質**: グローバルイルミネーションやソフトシャドウなどの高品質な照明効果を実現できます。

**利用シナリオ**

- **パフォーマンスの改善**: 特にリソースが限られた環境（例: モバイルデバイス、VR）での使用に適しています。

**注意点**

- **静的シーン限定**: ライトマップを動的オブジェクトに適用することや、ライトマップを適用したプレハブの天候条件を後から変更することはできません。そのため、主に変化しない環境での使用に限られます。
たとえば、Sandbox Toolkitを用いてアバターや乗り物などのオブジェクトを追加してトラック上で動くように設定する場合、ライトマップをベイクするとオブジェクトへのライティングや影の表示が固定されるため、オブジェクトの動きに追従しません。また、Rendering Toolkitの環境システム機能を使用する場合、ライトマップのベイクを行うと地物への日照や影が固定されるため、時間帯や天候を変更するとライティングが二重に設定されてしまいます。

詳細はライトマップに関する[こちら](https://docs.unity3d.com/ja/2021.3/Manual/Lightmappers.html)のドキュメントをご参照ください。

#### 4-1-2**. 3D都市モデルのFBX化とライトマップのベイク**

Unityエディター上で3Dモデルにライトマップを適用するためには、UVマッピングの第2セット（UV2）が必要です。PLATEAUの3D都市モデルデータにはUV2が含まれていないので、これを作成します。UnityエディターにはFBXフォーマットのファイルからライトマップ用のUVを自動的に生成する機能が備わっています。このため、ライトマップを作成するための最初のステップとして、PLATEAUの3D都市モデルのデータをFBXフォーマットに変換する必要があります。

#### 4-1-3. 3D都市モデルのFBXエクスポート

PLATEAU SDKのエクスポート機能を使って3D都市モデルをFBXにエクスポートします。

<img width="500" alt="prefab_lightmap_export_city_model_00" src="../Documentation~/Utilities Images/prefab_lightmap_export_city_model_00.png">

#### 4-1-4. 3D都市モデルのエクスポート設定

1. 3D都市モデルのトップノードをヒエラルキーから選択し、PLATEAU SDKのエクスポート機能にある「エクスポート対象」項目にドラッグアンドドロップします。
2. 「出力形式」で「FBX」を選択します。
3. FBXファイルとテクスチャを出力するフォルダのパスを指定します。
4. 「エクスポート」を押下すると、指定されたフォルダにFBXファイルとテクスチャが出力されます。

<img width="800" alt="prefab_lightmap_export_city_model_01" src="../Documentation~/Utilities Images/prefab_lightmap_export_city_model_01.png">

#### 4-1-5. UnityエディターでFBXの設定を行う（ライトマップ用UVの作成）

Unityエディター上でFBX設定の項目のチェックを追加することでライトマップを適用する為のUV2がモデルに自動生成されます。ライトマップのテクスチャを適用する為に、UV2の設定は必須となります。

**手順**

Unityエディター上でエクスポートしたFBXとテクスチャを読み込み、プロジェクトウインドウで3D都市モデルのFBXファイルを選択します。

<img width="500" alt="prefab_lightmap_fbx_setting_00" src="../Documentation~/Utilities Images/prefab_lightmap_fbx_setting_00.png"><br>

<img width="500" alt="prefab_lightmap_fbx_setting_01" src="../Documentation~/Utilities Images/prefab_lightmap_fbx_setting_01.png">

インスペクターにFBX専用の設定項目が表示されます。「Model」タブから「Generate Lightmap UVs」にチェックボックスを「オン」にして、「Apply」ボタンを押下すると、ライトマップUVの生成が完了します。また、デフォルトの設定では3D都市モデルが100分の1にスケールされてモデルが読み込まれてしまうため、リアルスケールで3D都市モデルを読み込む場合は、「Convert Units」のチェックボックスは「オフ」にします。

<img width="500" alt="prefab_lightmap_fbx_setting_02" src="../Documentation~/Utilities Images/prefab_lightmap_fbx_setting_02.png">

シーンのトップノードの配下のノードを設定が完了したFBXに置き換えて準備完了です。

<img width="800" alt="prefab_lightmap_fbx_setting_03" src="../Documentation~/Utilities Images/prefab_lightmap_fbx_setting_03.png">

**ライトの設定 (RealtimeからBakedへ)**

ベイクするライトをヒエラルキーから選択して、インスペクターの General > Mode の項目を「Realtime」から「Baked」に設定します。

<img width="500" alt="prefab_lightmap_light_setting_00" src="../Documentation~/Utilities Images/prefab_lightmap_light_setting_00.png">

**3D都市モデルのStaticフラグの設定 (Static Flag→ Contribute GI→ ON)**

ヒエラルキーから3D都市モデルのトップノードを選択します。

<img width="400" alt="prefab_lightmap_static_falg_setting_00" src="../Documentation~/Utilities Images/prefab_lightmap_static_falg_setting_00.png">

インスペクター右上の「Static」の右のプルダウンボタン（下三角ボタン）から 「Contribute GI」を選択します。

<img width="400" alt="prefab_lightmap_static_falg_setting_01" src="../Documentation~/Utilities Images/prefab_lightmap_static_falg_setting_01.png">

「Yes, change children」を押下し、配下のノードも含めて一括でフラグを有効化します。

<img width="400" alt="prefab_lightmap_static_falg_setting_02" src="../Documentation~/Utilities Images/prefab_lightmap_static_falg_setting_02.png">

以上で、配下のノード全てに「Contribute GI」にチェックマークがつき、設定が適用されます。

※ ライトマップは静的オブジェクト（動かないオブジェクト）にのみ作成される仕様となっているため、このフラグを有効化して明示的に静的オブジェクトに指定する必要があります。

<img width="500" alt="prefab_lightmap_static_falg_setting_03" src="../Documentation~/Utilities Images/prefab_lightmap_static_falg_setting_03.png">

**ライトマップの設定とベイク**

1. **ライトマップの設定にアクセス**
    - Unityエディターのメインメニューから Window > Rendering > Lighting を選択し、Lightingウィンドウを開きます。
    
<img width="600" alt="prefab_lightmap_static_bake_setting_00" src="../Documentation~/Utilities Images/prefab_lightmap_static_bake_setting_00.png">
    
3. **ライトマップの設定を調整**
    - 「New Lighting Settings」を押下してライトマップの設定ファイルを作成します。
        
      <img width="500" alt="prefab_lightmap_static_bake_setting_01" src="../Documentation~/Utilities Images/prefab_lightmap_static_bake_setting_01.png"> <br>
        
      <img width="500" alt="prefab_lightmap_static_bake_setting_02" src="../Documentation~/Utilities Images/prefab_lightmap_static_bake_setting_02.png">
        
    - Lightingウィンドウではライトマップに関連する様々な設定を調整できます。
        
      <img width="500" alt="prefab_lightmap_static_bake_setting_03" src="../Documentation~/Utilities Images/prefab_lightmap_static_bake_setting_03.png">
        
    - **主に重要な項目（3D都市モデル用）**
    - Lightmapper: Progressive GPU (Preview)
        - GPUでライトマップベイクの計算を行う為、CPUより高速に処理が完了します。
    - Lightmap: Resolution: 1
        - この値が大きいほどライトマップが高精細になり、テクスチャの容量や計算が増加します。ライトマップはリアルスケールで計算され、PLATEAUの3D都市モデルの場合は非常に時間がかかるため1を推奨します。
    - Max Lightmap Size: 2048
        - ライトマップの出力されるテクスチャサイズの最大値です。この値が小さいとテクスチャの枚数が増えて読み込み負荷が高くなることもあるため、2048を推奨します。
    - その他各設定についてはUnityの公式マニュアルをご覧ください。
    https://docs.unity3d.com/ja/current/Manual/class-LightingSettings.html
    
4. **ライトマップのベイク**
    - ライトマップの設定を完了したら、「Generate Lighting」を押下します。これにより、シーンのライトマップのベイクが開始されます。
        
      <img width="500" alt="prefab_lightmap_light_setting_02" src="../Documentation~/Utilities Images/prefab_lightmap_light_setting_02.png">
        
    - ベイクの進行状況はウィンドウの下部にあるプログレスバーで確認できます。
5. **ベイク終了の確認**
    - ベイクが完了すると、プログレスバーが消え、シーンにライトマップが適用された状態が表示されます。
        
      <img width="800" alt="prefab_lightmap_light_setting_04" src="../Documentation~/Utilities Images/prefab_lightmap_light_setting_04.png">
        

#### 4-1-6. 3D都市モデルのプレハブ化とプレハブへのライトマップ適用

通常ライトマップは仕様上シーンに対して紐づくためプレハブに保存することができません。ユースケースによってはプレハブにライトマップを紐づけたいケースもあるため、 PLATEAU Toolkit にはプレハブにライトマップを適用するツールが用意されています。

#### 4-1-7. 3D都市モデルのプレハブ化

まずは3D都市モデルをプレハブ化します。3D都市モデルのオブジェクトを選択して、プロジェクトウインドウの任意のフォルダにドラッグアンドドロップすることでそのフォルダにプレハブを作成することができます。

<img width="800" alt="prefab_lightmap_prefab_create_00" src="../Documentation~/Utilities Images/prefab_lightmap_prefab_create_00.png">

プレハブ化されたシーン上のオブジェクトはヒエラルキーで青色に表示されます。

<img width="800" alt="prefab_lightmap_prefab_create_01" src="../Documentation~/Utilities Images/prefab_lightmap_prefab_create_01.png">

#### 4-1-8. PLATEAU ToolkitのUtilitiesツールでシーンにベイクしたライトマップを3D都市モデルのプレハブに適用

メニューから PLATEAU > PLATEAU Toolkit > Utilities を選択肢、PLATEAU Utilities ウィンドウを開きます。

<img width="500" alt="prefab_lightmap_utility_tool_0" src="../Documentation~/Utilities Images/prefab_lightmap_utility_tool_0.png">

プレハブ化した3D都市モデルオブジェクトを選択して、Utilities ウィンドウから「プレハブへのライトマップの設定」を押下します。

<img width="800" alt="prefab_lightmap_utility_tool_00" src="../Documentation~/Utilities Images/prefab_lightmap_utility_tool_00.png">

以下のダイアログが表示されるので「はい」を選択します。

<img width="500" alt="prefab_lightmap_utility_tool_01" src="../Documentation~/Utilities Images/prefab_lightmap_utility_tool_01.png">

プレハブにライトマップを保存するための「Prefab Lightmap Data」コンポーネントが3D都市モデルオブジェクトに追加されます。

<img width="600" alt="prefab_lightmap_utility_tool_02" src="../Documentation~/Utilities Images/prefab_lightmap_utility_tool_02.png">

もう一度プレハブ化した3D都市モデルオブジェクトをヒエラルキーから選択して、Utilities のパネルから「シーンのライトマップをプレハブに設定」を実行すると、シーンのライトマップ情報が「Prefab Lightmap Data」コンポーネントに格納されます。これでプレハブにライトマップが紐づきました。

<img width="600" alt="prefab_lightmap_utility_tool_03" src="../Documentation~/Utilities Images/prefab_lightmap_utility_tool_03.png">

ライトを置いていない空のシーンに、プレハブ化した3D都市モデルをシーンにドラッグアンドドロップして確認します。プレハブに紐づいたライトマップが表示され、ライトがないシーンでも事前計算したライティングや影が3D都市モデルに反映されていることが確認できます。

<img width="800" alt="prefab_lightmap_utility_tool_04" src="../Documentation~/Utilities Images/prefab_lightmap_utility_tool_04.png">

比較画像: ライトがない場合、通常以下のような表示になります。

<img width="800" alt="prefab_lightmap_utility_tool_05" src="../Documentation~/Utilities Images/prefab_lightmap_utility_tool_05.png">

## 5. 関連API

- [AssetDatabase](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/AssetDatabase.html)
- [Bounds](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Bounds.html)
- [GameObject](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/GameObject.html)
- [Light](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Light.html)
- [LightmapData](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/LightmapData.html)
- [Lightmapping](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Lightmapping.html)
- [LightmapSettings](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/LightmapSettings.html)
- [MeshRenderer](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/MeshRenderer.html)
- [PrefabUtility](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/PrefabUtility.html)
- [Scene](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/SceneManagement.Scene.html)
- [Texture2D](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Texture2D.html)
- [Transform](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Transform.html)

# ライセンス

- 本リポジトリはMITライセンスで提供されています。
- 本システムの開発はユニティ・テクノロジーズ・ジャパン株式会社が行っています。
- ソースコードおよび関連ドキュメントの著作権は国土交通省に帰属します。

# 注意事項/利用規約
- 本ツールをアンインストールした場合、本ツールの機能で作成されたアセットの動作に不備が発生する可能性があります。
- 本ツールをアップデートした際は、一度 Unity エディターを再起動することを推奨しています。
- パフォーマンスの観点から、3km²の範囲に収まる3D都市モデルをダウンロード・インポートすることを推奨しています。
- インポートする範囲の広さや地物の種類（建物、道路、災害リスクなど）が量に比例して処理負荷が高くなる可能性があります。
- 本リポジトリの内容は予告なく変更・削除される可能性があります。
- 本リポジトリの利用により生じた損失及び損害等について、国土交通省はいかなる責任も負わないものとします。
