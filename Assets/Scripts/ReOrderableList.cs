//	Copyright© tetr4lab.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Tetr4lab;

/// <summary>長押しで並べ替え可能なスクロールリスト</summary>
public class ReOrderableList : MonoBehaviour {

	[SerializeField, Tooltip ("並べ替え中のみ表示するコンパネ")] private GameObject orderControlPanel = default;
	[SerializeField, Tooltip ("完了ボタン")] private Button doneButton = default;
	[SerializeField, Tooltip ("ドラッグ中の項目を所属させる画面")] private GameObject draggingScreen = default;
	[SerializeField, Tooltip ("ScrollView/Viewportの左下")] private Transform viewportMinMark = default;
	[SerializeField, Tooltip ("ScrollView/Viewportの右上")] private Transform viewportMaxMark = default;
	[SerializeField, Tooltip ("ScrollView/Contentの左下")] private Transform contentMinMark = default;
	[SerializeField, Tooltip ("ScrollView/Contentの右上")] private Transform contentMaxMark = default;
	[SerializeField, Tooltip ("長押しの秒数")] public float longPress = 0.4f;
	[SerializeField, Tooltip ("範囲外へドラッグした際のスクロール速度")] private Vector2 autoScrollSpeed = new Vector2 (20f, 20f);
	[SerializeField, Tooltip ("項目をボタンにする")] private bool elementSelectable = true;
	[SerializeField, Tooltip ("決定/取消キーの使用")] private bool enableKeyboardShortcut = true;
	[SerializeField, Tooltip ("完了時に破棄")] private bool destroyOnDone = true;

	[System.Serializable] public class OnReOrderCallback : UnityEvent<List<int>, int> { }
	[SerializeField, Tooltip ("完了コールバック")] private OnReOrderCallback onDone = default;
	[SerializeField, Tooltip ("並べ替え開始コールバック")] private OnReOrderCallback onBeginReOrder = default;
	[SerializeField, Tooltip ("並べ替え更新コールバック")] private OnReOrderCallback onUpdateReOrder = default;
	[SerializeField, Tooltip ("並べ替え終了コールバック")] private OnReOrderCallback onEndReOrder = default;

	/// <summary>ドラッグ中の隙間の制御 (項目から呼ばれる)</summary>
	public GameObject Dummy {
		get { return dummy; }
		set {
			if (dummy != null) {
				destroyDummy (element);
				dummy = null;
			}
			if (value != null) {
				dummy = createDummy (content, element = value);
			}
		}
	}
	private GameObject dummy; // 隙間
	private GameObject element; // ドラッグ中項目

	private int elementsCount { get { return content.transform.childCount - referencePointsCount; } } // 項目数
	private int referencePointsCount; // マーカー数
	private VerticalLayoutGroup contentLayoutGroup; // レイアウトグループ
	private Rect contentRect { get { return new Rect (contentMinMark.position, contentMaxMark.position - contentMinMark.position); } } // ScrollViewのContent矩形
	private Rect viewportRect { get { return new Rect (viewportMinMark.position, viewportMaxMark.position - viewportMinMark.position); } } // ScrollViewのViewport矩形

	private ScrollRect scrollRect; // ScrollView
	private GameObject content { get { return scrollRect.content.gameObject; } } // ScrollViewのContent

	public bool Interactable { // 応答可否
		get { return interactable; }
		set {
			interactable = value;
			if (scrollRect) { scrollRect.enabled = value; }
			if (doneButton) { doneButton.interactable = value && !orderable; }
			if (orderControlPanel && orderControlPanelButtons != null) {
				foreach (var button in orderControlPanelButtons) {
					button.interactable = value;
				}
			}
			foreach (var element in GetComponentsInChildren<ListElement> ()) {
				element.Interactable = value;
			}
		}
	}
	private bool interactable = true;
	private Button [] orderControlPanelButtons;

	/// <summary>並べ替えモード切り替え (項目から呼ばれる)</summary>
	public bool Orderable {
		get {
			return orderable;
		}
		set {
			orderable = value;
			if (orderControlPanel) { orderControlPanel.SetActive (value); }
			if (doneButton) { doneButton.interactable = !value; }
			var listElements = content.GetComponentsInChildren<ListElement> ();
			foreach (var element in listElements) {
				element.Orderable = value;
			}
		}
	}
	private bool orderable;

	/// <summary>並びの取得</summary>
	public List<int> Indexes {
		get { return new List<ElementIndex> (content.GetComponentsInChildren<ElementIndex> ()).ConvertAll (element => element.Index); }
	}

	/// <summary>初期化</summary>
	public void Init () {
		if (!scrollRect) {
			scrollRect = GetComponentInChildren<ScrollRect> ();
			contentLayoutGroup = content.GetComponent<VerticalLayoutGroup> ();
			referencePointsCount = content.transform.childCount - content.GetComponentsInChildren<ElementIndex> ().Length;
			if (orderControlPanel) { orderControlPanelButtons = orderControlPanel.GetComponentsInChildren<Button> (); }
			Orderable = false;
		}
	}

	/// <summary>開始</summary>
	private void Awake () {
		Init ();
	}

	/// <summary>駆動</summary>
	private void Update () {
		if (enableKeyboardShortcut) {
			if (Input.GetKeyDown (KeyCode.Escape)) { onPressEscapeKey (); }
			if (Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.KeypadEnter)) { onPressEnterKey (); }
		}
	}

	/// <summary>破棄</summary>
	private void OnDestroy () {
		if (onDone != null) {
			onDone.RemoveAllListeners ();
			onDone = null;
		}
		if (onBeginReOrder != null) {
			onBeginReOrder.RemoveAllListeners ();
			onBeginReOrder = null;
		}
		if (onUpdateReOrder != null) {
			onUpdateReOrder.RemoveAllListeners ();
			onUpdateReOrder = null;
		}
		if (onEndReOrder != null) {
			onEndReOrder.RemoveAllListeners ();
			onEndReOrder = null;
		}
	}

	/// <summary>完了コールバックの登録</summary>
	public void AddOnDoneListener (UnityAction<List<int>, int> onDoneAction) {
		if (onDoneAction != null) {
			if (onDone == null) { onDone = new OnReOrderCallback (); }
			onDone.AddListener (onDoneAction);
		}
	}

	/// <summary>完了コールバックの除去</summary>
	/// <param name="onDoneAction">nullなら全て</param>
	public void RemoveOnDoneListener (UnityAction<List<int>, int> onDoneAction) {
		if (onDoneAction != null) {
			if (onDone != null) {
				onDone.RemoveListener (onDoneAction);
			} else {
				onDone.RemoveAllListeners ();
			}
		}
	}

	/// <summary>並べ替え開始コールバックの登録</summary>
	public void AddOnBeginReOrderListener (UnityAction<List<int>, int> onBeginReOrderAction) {
		if (onBeginReOrderAction != null) {
			if (onBeginReOrder == null) { onBeginReOrder = new OnReOrderCallback (); }
			onBeginReOrder.AddListener (onBeginReOrderAction);
		}
	}

	/// <summary>並べ替え開始コールバックの除去</summary>
	/// <param name="onBeginReOrderAction">nullなら全て</param>
	public void RemoveOnBeginReOrderListener (UnityAction<List<int>, int> onBeginReOrderAction) {
		if (onBeginReOrderAction != null) {
			if (onBeginReOrder != null) {
				onBeginReOrder.RemoveListener (onBeginReOrderAction);
			} else {
				onBeginReOrder.RemoveAllListeners ();
			}
		}
	}

	/// <summary>並べ替え更新コールバックの登録</summary>
	public void AddOnUpdateReOrderListener (UnityAction<List<int>, int> onUpdateReOrderAction) {
		if (onUpdateReOrderAction != null) {
			if (onUpdateReOrder == null) { onUpdateReOrder = new OnReOrderCallback (); }
			onUpdateReOrder.AddListener (onUpdateReOrderAction);
		}
	}

	/// <summary>並べ替え更新コールバックの除去</summary>
	/// <param name="onUpdateReOrderAction">nullなら全て</param>
	public void RemoveOnUpdateReOrderListener (UnityAction<List<int>, int> onUpdateReOrderAction) {
		if (onUpdateReOrderAction != null) {
			if (onUpdateReOrder != null) {
				onUpdateReOrder.RemoveListener (onUpdateReOrderAction);
			} else {
				onUpdateReOrder.RemoveAllListeners ();
			}
		}
	}

	/// <summary>並べ替え終了コールバックの登録</summary>
	public void AddOnEndReOrderListener (UnityAction<List<int>, int> onEndReOrderAction) {
		if (onEndReOrderAction != null) {
			if (onEndReOrder == null) { onEndReOrder = new OnReOrderCallback (); }
			onEndReOrder.AddListener (onEndReOrderAction);
		}
	}

	/// <summary>並べ替え終了コールバックの除去</summary>
	/// <param name="onEndReOrderAction">nullなら全て</param>
	public void RemoveOnEndReOrderListener (UnityAction<List<int>, int> onEndReOrderAction) {
		if (onEndReOrderAction != null) {
			if (onEndReOrder != null) {
				onEndReOrder.RemoveListener (onEndReOrderAction);
			} else {
				onEndReOrder.RemoveAllListeners ();
			}
		}
	}

	/// <summary>リストに項目を追加する</summary>
	public void AddElement (GameObject element) {
		if (!element) { return; }
		element.transform.SetParent (content.transform);
		element.transform.SetAsLastSibling ();
		var id = element.GetComponentInChildren<ElementIndex> ();
		if (id == null) { id = element.AddComponent<ElementIndex> (); }
		id.Index = content.GetComponentsInChildren<ElementIndex> ().Length - 1; // 呼ばれる状況が不明なので実測
		var le = element.GetComponentInChildren<ListElement> ();
		if (le == null) { le = element.AddComponent<ListElement> (); }
		le.Init ();
	}

	/// <summary>リストに複数項目を追加する</summary>
	public void AddElement (IEnumerable<GameObject> elements) {
		if (!element) { return; }
		foreach (var element in elements) {
			AddElement (element);
		}
	}

	/// <summary>リストから項目を一掃する</summary>
	public void ClearElement () {
		foreach (var element in content.GetComponentsInChildren<ElementIndex> ()) {
			Destroy (element.gameObject);
		}
	}

	/// <summary>隙間位置の更新 (項目から呼ばれる)</summary>
	public void UpdateDraggingPosition (Vector2 pos) {
		var normarizedContentPosition = Vector2.Max (Vector2.Min ((pos - contentRect.position) / contentRect.size, Vector2.one), Vector2.zero);
		var index = Mathf.Clamp ((elementsCount - 1) - Mathf.FloorToInt (normarizedContentPosition.y / (1f / elementsCount)), 0, elementsCount - 1);
		if (dummy.transform.GetSiblingIndex () != referencePointsCount + index) {
			dummy.transform.SetSiblingIndex (referencePointsCount + index);
			if (onUpdateReOrder != null) { onUpdateReOrder.Invoke (Indexes, index); }
		}
		var delta = pos.OutArea (viewportRect);
		if ((delta.y < 0f && scrollRect.verticalNormalizedPosition >= 0f) || (delta.y > 0f && scrollRect.verticalNormalizedPosition <= 1f)) {
			if (delta.y > 0f && scrollRect.verticalNormalizedPosition < 0f) {
				scrollRect.verticalNormalizedPosition = 1.2e-6f;
			}
			scrollRect.velocity = -delta * autoScrollSpeed;
		}
	}

	/// <summary>隙間の生成</summary>
	private GameObject createDummy (GameObject parent, GameObject element) {
		var obj = new GameObject ("DummyElement", new Type [] { typeof (RectTransform), typeof (ElementIndex), });
		obj.transform.SetParent (parent.transform);
		obj.transform.SetSiblingIndex (element.transform.GetSiblingIndex ());
		element.transform.SetParent ((draggingScreen ?? this.gameObject).transform);
		var rect = obj.GetComponent<RectTransform> ();
		var sorceRect = element.GetComponent<RectTransform> ();
		rect.sizeDelta = sorceRect.sizeDelta;
		rect.pivot = sorceRect.pivot;
		rect.localRotation = sorceRect.localRotation;
		rect.localScale = sorceRect.localScale;
		var index = element.GetComponentInChildren<ElementIndex> ().Index;
		obj.GetComponent<ElementIndex> ().Index = index;
		if (onBeginReOrder != null) { onBeginReOrder.Invoke (Indexes, index); }
		return obj;
	}

	/// <summary>隙間の抹消</summary>
	private void destroyDummy (GameObject element) {
		if (element != null) {
			var indexes = Indexes;
			element.transform.SetParent (content.transform);
			var index = dummy.transform.GetSiblingIndex ();
			element.transform.SetSiblingIndex (index);
			element = null;
			if (onEndReOrder != null) { onEndReOrder.Invoke (indexes, index); }
		}
		Destroy (dummy.gameObject);
	}

	/// <summary>完了処理</summary>
	private void done (int index) {
		Interactable = false;
		if (onDone != null) {
			onDone.Invoke (Indexes, index);
		}
		if (destroyOnDone) { Destroy (this.gameObject); }
	}

	/// <summary>戻るキーが押された</summary>
	private void onPressEscapeKey () {
		if (Orderable && orderControlPanel) { OnPushBack (); } else { OnPushDone (); }
	}

	/// <summary>決定キーが押された</summary>
	private void onPressEnterKey () {
		if (!Orderable && doneButton) { OnPushDone (); }
	}

	/// <summary>項目が選択された</summary>
	public void OnSelect (int index) {
		if (elementSelectable) {
			done (index);
		}
	}

	#region Button Events

	/// <summary>戻るボタンが押された</summary>
	public void OnPushBack () {
		Orderable = false;
	}

	/// <summary>完了ボタンが押された</summary>
	public void OnPushDone () {
		done (-1);
	}

	#endregion

}
