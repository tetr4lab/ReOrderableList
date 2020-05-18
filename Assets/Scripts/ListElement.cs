//	Copyright© tetr4lab.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>長押しで並べ替え可能なスクロールリストの項目</summary>
public class ListElement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {

	#region Static
	/// <summary>アクティブな項目 後優先で排他制御</summary>
	public static ListElement ActiveElement {
		get { return activeElement; }
		set {
			if (activeElement != null && activeElement != value) {
				activeElement.release ();
			}
			activeElement = value;
		}
	}
	private static ListElement activeElement;
	#endregion

	private ReOrderableList orderableList; // 親コンポ
	private ScrollRect scrollRect; // 親ScrollView
	private ElementIndex elementIndex; // インデックスを保持するコンポ

	private bool isPointerDown;
	private int pointerId = int.MinValue;
	private float pointerDownTime;
	private bool isDragging;

	private Button [] buttons;
	public bool Interactable {
		get { return interactable; }
		set {
			interactable = value;
			isPointerDown = isDragging = false;
			if (buttons != null) {
				foreach (var button in buttons) {
					button.interactable = value;
				}
			}
		}
	}
	private bool interactable = true;

	/// <summary>初期化</summary>
	public void Init () {
		if (!orderableList) {
			orderableList = GetComponentInParent<ReOrderableList> ();
			scrollRect = GetComponentInParent<ScrollRect> ();
			elementIndex = GetComponentInChildren<ElementIndex> ();
			buttons = GetComponentsInChildren<Button> ();
		}
	}

	/// <summary>開始</summary>
	private void Start () {
		Init ();
	}

	/// <summary>駆動</summary>
	private void Update () {
		if (!interactable) { return; }
		if (isPointerDown && !orderableList.Orderable) {
			if (pointerDownTime > orderableList.longPress) {
				orderableList.Orderable = true;
				isPointerDown = false;
			}
			pointerDownTime += Time.deltaTime;
		}
		if (isDragging && orderableList.Orderable) {
			orderableList.UpdateDraggingPosition (transform.position);
		}
	}

	/// <summary>アクティブな項目を強制解放</summary>
	private void release () {
		if (isPointerDown) { OnPointerUp (lastEventData); }
		if (isDragging) { OnEndDrag (lastEventData); }
		activeElement = null;
	}

	#region Pointer & Drag Events

	/// <summary>最後に使ったポインターイベントデータ</summary>
	private PointerEventData lastEventData;

	public void OnPointerDown (PointerEventData eventData) {
		if (!interactable) { return; }
		if (!isPointerDown && !isDragging && (eventData.pointerId == 0 || eventData.pointerId == -1)) {
			lastEventData = eventData;
			ActiveElement = this;
			pointerId = eventData.pointerId;
			isPointerDown = true;
			pointerDownTime = 0f;
			isDragging = false;
		}
	}

	public void OnPointerUp (PointerEventData eventData) {
		if (!interactable || ActiveElement != this) { return; }
		isPointerDown = false;
		if (!isDragging && !eventData.hovered.Contains (eventData.pointerPress)) {
			activeElement = null;
			lastEventData = eventData;
		}
	}

	public void OnPointerClick (PointerEventData eventData) {
		if (!interactable || ActiveElement != this) { return; }
		if (!orderableList.Orderable && !isDragging) {
			orderableList.OnSelect (elementIndex.Index);
			activeElement = null;
			lastEventData = eventData;
		}
	}

	public void OnBeginDrag (PointerEventData eventData) {
		if (!interactable || ActiveElement != this) { return; }
		if (!orderableList.Orderable) {
			if (scrollRect != null) {
				scrollRect.OnBeginDrag (eventData);
				lastEventData = eventData;
			}
			isPointerDown = false;
		} else {
			orderableList.Dummy = gameObject;
			lastEventData = eventData;
		}
		isDragging = true;
	}

	public void OnDrag (PointerEventData eventData) {
		if (!interactable || ActiveElement != this) { return; }
		if (!orderableList.Orderable) {
			if (scrollRect != null) {
				scrollRect.OnDrag (eventData);
				lastEventData = eventData;
			}
		} else {
#if true // スクロール方向への移動に限定
			var pos = transform.position;
			pos.y = eventData.position.y;
			transform.position = pos;
#else
			transform.position = eventData.position.y;
#endif
			lastEventData = eventData;
		}
	}

	public void OnEndDrag (PointerEventData eventData) {
		if (!interactable || ActiveElement != this) { return; }
		if (!orderableList.Orderable) {
			if (scrollRect != null) {
				scrollRect.OnEndDrag (eventData);
				lastEventData = eventData;
			}
		} else {
			orderableList.Dummy = null;
			lastEventData = eventData;
		}
		isDragging = false;
		activeElement = null;
	}

#endregion

}
