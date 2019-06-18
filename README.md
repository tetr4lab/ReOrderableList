# Re-orderable scroll view / 長押しして並べ替えられるスクロールビュー (uGUI)
tags: Unity uGUI C#

# 前提
- unity 2018.4.2f1
- uGUIの`ScrollRect`(Scroll View)、`VerticalLayoutGroup`、`ContentSizeFitter`と併用するスクリプトコンポーネントです。

# できること
- 縦スクロールのリストビューにコンポーネントを追加することで、長押し＋ドラッグ&ドロップで項目の並べ替えができるようになります。  
![ScreenRecord_2019-06-13-15-52-49.gif](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/365845/93343581-bb80-90a7-5059-4c2d5e92d585.gif)
- モード切替、ドラッグ開始、並び順更新、ドラッグ終了、項目の選択のコールバックを設定できます。

# アセットの入手 (GitHub)
- スクリプトコンポーネントとして明示的に使用するのは、主に`ReOrderableList`と`ListElement`です。
    - `ListElement`は明示しない場合でも内部で使われます。
    - `ElementIndex`は主に内部で使用されるコンポです。
- `Tetr4labUtility`は、内部で使用されるユーティリティクラスです。
- `Sample～`と命名されているアセットは必須ではありません。

# 取り敢えず使ってみたい
- 動的リスト
    - リストをスクリプトで生成する場合は、`SampleScene (dynamic)`をご覧ください。
    - 対応するメインスクリプトは、`SampleDynamic`です。
- 静的リスト
    - シーン上であらかじめリストを完成させておく場合は、`SampleScene (static)`をご覧ください。
    - 対応するメインスクリプトは、`SampleStatic`です。

# 使い方の系統的な説明

### 導入
- "Scroll View"を作ったら、以下の手順でマーカーを置きます。
    - "ViewPort"の左下と右上にサイズ`(0, 0)`の空オブジェクトを置き、非アクティブにしてください。
    - "Content"にも同じように空オブジェクトを置き、非アクティブにしてください。
        - 非アクティブにしたくない場合は、`LayoutElement`を付けて、`ignoreLayout`をチェックしてください。
- "Scroll View"の"Content"に、`VerticalLayoutGroup`と`ContentSizeFitter`を付け、適切に設定してください。  
![VerticalLayoutGroup-Inspector.png](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/365845/7019ad91-2e4c-fac2-1c46-9917213aaf8f.png)
    - `VerticalLayoutGroup`の`Padding`を大きく取ると、ドラッグ時にずれが生じます。
- `ScrollRect`と同じか直系尊属にあたるオブジェクトに`ReOrderableList`を付け、適切に設定します。  
![ReOrderableList-Inspector.png](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/365845/b60d2994-5894-0f43-b22e-5273ca3f7064.png)
    - `SampleScene`では、"Scroll View"の親オブジェクトに付けています。

<details><summary>`ReOrderableList`の設定内容</summary><div>

|項目|説明|
|:---|:---|
|ViewportMinMark|ScrollView/Viewportの左下に置かれたマーカーを指定します。|
|ViewportMaxMark|ScrollView/Viewportの右上に置かれたマーカーを指定します。|
|ContentMinMark|ScrollView/Contentの左下に置かれたマーカーを指定します。|
|ContentMaxMark|ScrollView/Contentの右上に置かれたマーカーを指定します。|
|LongPress|長押しと判定する秒数を指定します。|
|AutoScrollSpeed|範囲外へドラッグした際のスクロール速度を指定します。単位は適当です。|
|OnChangeMode|モード切替コールバックを設定できます。|
|OnSelect|項目選択コールバックを設定できます。|
|OnBeginOrder|並べ替え開始コールバックを設定できます。|
|OnUpdateOrder|並べ替え更新コールバックを設定できます。|
|OnEndOrder|並べ替え終了コールバックを設定できます。|
- インスペクタでコールバック関数を割り当てる場合は、ダイナミックモードを使用する必要があります。(インスペクタ上で引数の指定ができてはダメです。)
</div></details>

### 動的リストの場合
- リストをスクリプトで生成する場合です。
    - シーン上であらかじめリストを完成させておく場合は、「[静的リストの場合](#%E9%9D%99%E7%9A%84%E3%83%AA%E3%82%B9%E3%83%88%E3%81%AE%E5%A0%B4%E5%90%88)」を参照してください。
- `ReOrderableList`クラスのインスタンスに対して、以降で説明する操作が可能です。
- 生成したリスト項目は、`AddElement (～)`で配置してください。
    - `GameObject`1個を、あるいは、複数を`GameObject []`や`List<GameObject>`で、渡すことができます。
    - 項目を直接"Scroll View"に配置したり削除したりしてはいけません。
- 項目を一掃する場合は、`ClearElement ()`を使います。
- コールバックを指定するには以下のメソッドを使います。
    - モード切替コールバックの登録 `AddOnChangeModeListener ()`
    - モード切替コールバックの登録 `RemoveOnChangeModeListener ()`
    - 項目選択コールバックの登録 `AddOnSelectListener ()`
    - 項目選択コールバックの除去 `RemoveOnSelectListener ()`
    - 並べ替え開始コールバックの登録 `AddOnBeginOrderListener ()`
    - 並べ替え開始コールバックの除去 `RemoveOnBeginOrderListener ()`
    - 並べ替え更新コールバックの登録 `AddOnUpdateOrderListener ()`
    - 並べ替え更新コールバックの除去 `RemoveOnUpdateOrderListener ()`
    - 並べ替え終了コールバックの登録 `AddOnEndOrderListener ()`
    - 並べ替え終了コールバックの除去 `RemoveOnEndOrderListener ()`
- `bool Interactable`を使うと、リストの応答性を切り替えられます。
- `bool Orderable`で、現在ドラッグ可能モードかどうかを取得できます。
- `List<int> Indexes`で、現在の並び順を取得できます。
- `List<GameObject> GameObjects`で、現在の並び順の全項目オブジェクトを取得できます。
- `GameObject [int]`(インデクサ)で、現在の並びから指定の項目オブジェクトを取得できます。

### 静的リストの場合
- シーン上であらかじめリストを完成させておく場合です。
    - リストをスクリプトで生成する場合は、「[動的リストの場合](#%E5%8B%95%E7%9A%84%E3%83%AA%E3%82%B9%E3%83%88%E3%81%AE%E5%A0%B4%E5%90%88)」を参照してください。
- 項目のオブジェクトに、`ListElement`と`ElementIndex`を付け、`ElementIndex`にユニークな`Index`を指定してください。  
![ElementIndex-Inspector.png](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/365845/c18d3ccc-6fb6-4388-c6f4-635b5dcb09ed.png)
- インスペクタでコールバック関数を割り当てる場合は、ダイナミックモードを使用する必要があります。(インスペクタ上で引数の指定ができてはダメです。)

### コールバック
- モード切替コールバックのAPIは、`void Action (bool)`で、引数はドラッグ可能かどうかです。
- 残り全てのコールバックは、`void Action (int)`で、引数は、対象になっている項目の`Index`、または見かけ上の`SiblingIndex`です。

# やっていること
- 項目側でポインターイベントを取得して、使わない場合は親(`ScrollRect`)に投げています。
- ドラッグ中は、透明なダミーオブジェクトを生成して、項目と入れ替えています。
- `SiblingIndex`を使って、項目(とダミー)を並べ替えています。
- `CanvasScaler`が動的にレイアウトした結果を得るために、マーカーオブジェクトを埋め込んで、位置と距離を取得しています。
- ドラッグ中のポインタがスクロールの上下に外れたら、端からの距離に応じた速度でスクロールするようにしています。

# アレンジ
- 横並び(`HolizontalLayoutGroup`)を使いたい。
    - この要求に配慮して極力`Vector2`で計算していますが、一部(`ReOrderableList.UpdateDraggingPosition ()`など)が縦並びに依存しています。

# 更新情報
- 6/13 
    - マルチタッチや、タッチとマウスの併用に関わる不具合を修正しました。
- 6/14
    - モード切替のコールバックを用意して、UIデザインからの独立性を高めました。
        - 当初は、完了ボタンやドラッグハンドルなどをリスト側で制御していました。
    - 任意にリストの情報を取得する手段を拡充し、コールバックのAPIを簡素化しました。
- 6/15
    - 二本目以降の指には応じないようにしました。

---
### 参考情報
- 「Unity-UI-Extensions」にその名も「[Re-orderable List](https://bitbucket.org/UnityUIExtensions/unity-ui-extensions/wiki/Controls/ReorderableList)」というのがあるのですが、チラ見しただけで~~面倒になって、~~ 自分の目的に合わないと断じて、ちゃんと見てません。つまり、これは車輪の再発明の可能性があります。
