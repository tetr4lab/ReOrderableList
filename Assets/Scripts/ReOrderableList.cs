//	Copyright© tetr4lab.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Tetr4lab;

/// <summary>長押しで並べ替え可能なスクロールリスト</summary>
public class ReOrderableList : MonoBehaviour {

	[SerializeField, Tooltip ("ScrollView/Viewportの左下")] private Transform viewportMinMark = default;
	[SerializeField, Tooltip ("ScrollView/Viewportの右上")] private Transform viewportMaxMark = default;
	[SerializeField, Tooltip ("ScrollView/Contentの左下")] private Transform contentMinMark = default;
	[SerializeField, Tooltip ("ScrollView/Contentの右上")] private Transform contentMaxMark = default;
	[SerializeField, Tooltip ("長押しの秒数")] public float longPress = 0.4f;
	[SerializeField, Tooltip ("範囲外へドラッグした際のスクロール速度")] private Vector2 autoScrollSpeed = new Vector2 (20f, 20f);

	[Serializable] public class OnChangeModeCallback : UnityEvent<bool> { }
	[SerializeField, Tooltip ("モード切替コールバック")] private OnChangeModeCallback onChangeMode = default;
	[Serializable] public class OnReOrderCallback : UnityEvent<int> { }
	[SerializeField, Tooltip ("項目選択コールバック")] private OnReOrderCallback onSelect = default;
	[SerializeField, Tooltip ("並べ替え開始コールバック")] private OnReOrderCallback onBeginOrder = default;
	[SerializeField, Tooltip ("並べ替え更新コールバック")] private OnReOrderCallback onUpdateOrder = default;
	[SerializeField, Tooltip ("並べ替え終了コールバック")] private OnReOrderCallback onEndOrder = default;

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
			foreach (var element in GetComponentsInChildren<ListElement> ()) {
				element.Interactable = value;
			}
		}
	}
	private bool interactable = true;

	/// <summary>並べ替えモード切り替え (項目から呼ばれる)</summary>
	public bool Orderable {
		get {
			return orderable;
		}
		set {
			orderable = value;
			if (!value) { ListElement.ActiveElement = null; }
			if (onChangeMode != null) {
				onChangeMode.Invoke (value);
			}
		}
	}
	private bool orderable;

	/// <summary>並びの取得</summary>
	public List<int> Indexes {
		get { return new List<ElementIndex> (content.GetComponentsInChildren<ElementIndex> ()).ConvertAll (element => element.Index); }
	}

	/// <summary>項目リストの取得</summary>
	public List<GameObject> GameObjects {
		get { return new List<ElementIndex> (content.GetComponentsInChildren<ElementIndex> ()).ConvertAll (element => element.gameObject); }
	}

	/// <summary>項目の取得</summary>
	public GameObject this [int index] {
		get { return (index >= 0 && index < elementsCount) ? content.transform.GetChild (referencePointsCount + index).gameObject : null; }
	}

	/// <summary>初期化</summary>
	public void Init () {
		if (!scrollRect) {
			scrollRect = GetComponentInChildren<ScrollRect> ();
			contentLayoutGroup = content.GetComponent<VerticalLayoutGroup> ();
			referencePointsCount = content.transform.childCount - content.GetComponentsInChildren<ElementIndex> ().Length;
			Orderable = false;
		}
	}

	/// <summary>開始</summary>
	private void Awake () {
		Init ();
	}

	/// <summary>破棄</summary>
	private void OnDestroy () {
		if (onChangeMode != null) {
			onChangeMode.RemoveAllListeners ();
			onChangeMode = null;
		}
		if (onSelect != null) {
			onSelect.RemoveAllListeners ();
			onSelect = null;
		}
		if (onBeginOrder != null) {
			onBeginOrder.RemoveAllListeners ();
			onBeginOrder = null;
		}
		if (onUpdateOrder != null) {
			onUpdateOrder.RemoveAllListeners ();
			onUpdateOrder = null;
		}
		if (onEndOrder != null) {
			onEndOrder.RemoveAllListeners ();
			onEndOrder = null;
		}
	}

	/// <summary>モード切替コールバックの登録</summary>
	public void AddOnChangeModeListener (UnityAction<bool> onChangeModeAction) {
		if (onChangeModeAction != null) {
			if (onChangeMode == null) { onChangeMode = new OnChangeModeCallback (); }
			onChangeMode.AddListener (onChangeModeAction);
		}
	}

	/// <summary>モード切替コールバックの除去</summary>
	/// <param name="onChangeModeAction">nullなら全て</param>
	public void RemoveOnChangeModeListener (UnityAction<bool> onChangeModeAction) {
		if (onChangeModeAction != null) {
			if (onChangeMode != null) {
				onChangeMode.RemoveListener (onChangeModeAction);
			} else {
				onChangeMode.RemoveAllListeners ();
			}
		}
	}

	/// <summary>項目選択コールバックの登録</summary>
	public void AddOnSelectListener (UnityAction<int> onSelectAction) {
		if (onSelectAction != null) {
			if (onSelect == null) { onSelect = new OnReOrderCallback (); }
			onSelect.AddListener (onSelectAction);
		}
	}

	/// <summary>項目選択コールバックの除去</summary>
	/// <param name="onSelectAction">nullなら全て</param>
	public void RemoveOnSelectListener (UnityAction<int> onSelectAction) {
		if (onSelectAction != null) {
			if (onSelect != null) {
				onSelect.RemoveListener (onSelectAction);
			} else {
				onSelect.RemoveAllListeners ();
			}
		}
	}

	/// <summary>並べ替え開始コールバックの登録</summary>
	public void AddOnBeginOrderListener (UnityAction<int> onBeginOrderAction) {
		if (onBeginOrderAction != null) {
			if (onBeginOrder == null) { onBeginOrder = new OnReOrderCallback (); }
			onBeginOrder.AddListener (onBeginOrderAction);
		}
	}

	/// <summary>並べ替え開始コールバックの除去</summary>
	/// <param name="onBeginOrderAction">nullなら全て</param>
	public void RemoveOnBeginOrderListener (UnityAction<int> onBeginOrderAction) {
		if (onBeginOrderAction != null) {
			if (onBeginOrder != null) {
				onBeginOrder.RemoveListener (onBeginOrderAction);
			} else {
				onBeginOrder.RemoveAllListeners ();
			}
		}
	}

	/// <summary>並べ替え更新コールバックの登録</summary>
	public void AddOnUpdateOrderListener (UnityAction<int> onUpdateOrderAction) {
		if (onUpdateOrderAction != null) {
			if (onUpdateOrder == null) { onUpdateOrder = new OnReOrderCallback (); }
			onUpdateOrder.AddListener (onUpdateOrderAction);
		}
	}

	/// <summary>並べ替え更新コールバックの除去</summary>
	/// <param name="onUpdateOrderAction">nullなら全て</param>
	public void RemoveOnUpdateOrderListener (UnityAction<int> onUpdateOrderAction) {
		if (onUpdateOrderAction != null) {
			if (onUpdateOrder != null) {
				onUpdateOrder.RemoveListener (onUpdateOrderAction);
			} else {
				onUpdateOrder.RemoveAllListeners ();
			}
		}
	}

	/// <summary>並べ替え終了コールバックの登録</summary>
	public void AddOnEndOrderListener (UnityAction<int> onEndOrderAction) {
		if (onEndOrderAction != null) {
			if (onEndOrder == null) { onEndOrder = new OnReOrderCallback (); }
			onEndOrder.AddListener (onEndOrderAction);
		}
	}

	/// <summary>並べ替え終了コールバックの除去</summary>
	/// <param name="onEndOrderAction">nullなら全て</param>
	public void RemoveOnEndOrderListener (UnityAction<int> onEndOrderAction) {
		if (onEndOrderAction != null) {
			if (onEndOrder != null) {
				onEndOrder.RemoveListener (onEndOrderAction);
			} else {
				onEndOrder.RemoveAllListeners ();
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
			if (onUpdateOrder != null) { onUpdateOrder.Invoke (index); }
		}
		var delta = pos.OutArea (viewportRect);
		if ((delta.y < 0f && scrollRect.verticalNormalizedPosition >= 0f) || (delta.y > 0f && scrollRect.verticalNormalizedPosition <= 1f)) {
			if (delta.y > 0f && scrollRect.verticalNormalizedPosition < 0f) {
				scrollRect.verticalNormalizedPosition = 1e-5f; // 脱出速度的な何か (ドラッグ中に下端に達した後、上向きに自動スクロールしない場合は、この数値を大きくしてみてください。)
			}
			scrollRect.velocity = -delta * autoScrollSpeed;
		}
	}

	/// <summary>隙間の生成</summary>
	private GameObject createDummy (GameObject parent, GameObject element) {
		var obj = new GameObject ("DummyElement", new Type [] { typeof (RectTransform), typeof (ElementIndex), });
		obj.transform.SetParent (parent.transform);
		obj.transform.SetSiblingIndex (element.transform.GetSiblingIndex ());
		element.transform.SetParent (transform);
		var rect = obj.GetComponent<RectTransform> ();
		var sorceRect = element.GetComponent<RectTransform> ();
		rect.sizeDelta = sorceRect.sizeDelta;
		rect.pivot = sorceRect.pivot;
		rect.localRotation = sorceRect.localRotation;
		rect.localScale = sorceRect.localScale;
		var index = element.GetComponentInChildren<ElementIndex> ().Index;
		obj.GetComponent<ElementIndex> ().Index = index;
		if (onBeginOrder != null) { onBeginOrder.Invoke (index); }
		return obj;
	}

	/// <summary>隙間の抹消</summary>
	private void destroyDummy (GameObject element) {
		if (element != null) {
			element.transform.SetParent (content.transform);
			var index = dummy.transform.GetSiblingIndex ();
			element.transform.SetSiblingIndex (index);
			element = null;
			if (onEndOrder != null) { onEndOrder.Invoke (index); }
		}
		Destroy (dummy.gameObject);
	}

	/// <summary>項目が選択された</summary>
	public void OnSelect (int index = -1) {
		if (onSelect != null) {
			Interactable = false;
			onSelect.Invoke (index);
		}
	}

}
