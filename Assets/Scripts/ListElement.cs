//	Copyright© tetr4lab.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>長押しで並べ替え可能なスクロールリストの項目</summary>
public class ListElement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {

	[SerializeField, Tooltip ("並べ替え中のみ表示するアイコン")] private GameObject dragHandle = default;

	private ReOrderableList orderableList; // 親コンポ
	private ScrollRect scrollRect; // 親ScrollView
	private ElementIndex elementIndex; // インデックスを保持するコンポ

	private bool isPointerDown;
	private int poinerId;
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

	/// <summary>並べ替えモード切り替え</summary>
	public bool Orderable {
		get { return orderableList.Orderable; }
		set { if (dragHandle) { dragHandle.SetActive (value); } }
	}

	/// <summary>初期化</summary>
	public void Init () {
		if (!orderableList) {
			orderableList = GetComponentInParent<ReOrderableList> ();
			scrollRect = GetComponentInParent<ScrollRect> ();
			elementIndex = GetComponentInChildren<ElementIndex> ();
			buttons = GetComponentsInChildren<Button> ();
			Orderable = false;
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
		if (Orderable && isDragging) {
			orderableList.UpdateDraggingPosition (transform.position);
		}
	}

	#region Pointer / Drag Events

	public void OnPointerDown (PointerEventData eventData) {
		if (!interactable) { return; }
		poinerId = eventData.pointerId;
		isPointerDown = true;
		pointerDownTime = 0f;
		isDragging = false;
	}

	public void OnPointerUp (PointerEventData eventData) {
		if (!interactable) { return; }
		if (poinerId == eventData.pointerId) {
			isPointerDown = false;
		}
	}

	public void OnPointerClick (PointerEventData eventData) {
		if (!interactable) { return; }
		if (eventData.pointerId == 0 || eventData.pointerId == -1) {
			if (!Orderable && !isDragging) {
				orderableList.OnSelect (elementIndex.Index);
			}
		}
	}

	public void OnBeginDrag (PointerEventData eventData) {
		if (!interactable) { return; }
		if (poinerId == eventData.pointerId) {
			if (!Orderable) {
				if (scrollRect != null) {
					scrollRect.OnBeginDrag (eventData);
				}
				isPointerDown = false;
			} else {
				orderableList.Dummy = gameObject;
			}
			isDragging = true;
		}
	}

	public void OnDrag (PointerEventData eventData) {
		if (!interactable) { return; }
		if (poinerId == eventData.pointerId) {
			if (!Orderable) {
				if (scrollRect != null) {
					scrollRect.OnDrag (eventData);
				}
			} else {
				transform.position = Input.mousePosition;
			}
		}
	}

	public void OnEndDrag (PointerEventData eventData) {
		if (!interactable) { return; }
		if (poinerId == eventData.pointerId) {
			if (!Orderable) {
				if (scrollRect != null) {
					scrollRect.OnEndDrag (eventData);
				}
			} else {
				orderableList.Dummy = null;
			}
			isDragging = false;
		}
	}

	#endregion

}
