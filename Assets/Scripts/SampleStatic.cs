using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>静的リストサンプル</summary>
public class SampleStatic : MonoBehaviour {

	public static Text InfoText;
	private GameObject sampleList;



	private ReOrderableList reOrderableList;
	private Button doneButton;
	private GameObject orderControlPanel;

	private void Awake () {
		InfoText = transform.parent.GetChild (transform.parent.childCount - 1).GetComponentInChildren<Text> ();
		sampleList = transform.GetChild (0).gameObject;
		doneButton = sampleList.transform.GetChild (2).GetComponent<Button> ();
		reOrderableList = sampleList.GetComponentInChildren<ReOrderableList> ();
		orderControlPanel = sampleList.transform.GetChild (1).gameObject;
	}

	private void Start () {
		Restart ();
	}

	private void Update () {
		if (sampleList && reOrderableList.Interactable) {
			if (Input.GetKeyDown (KeyCode.Escape)) {
				OnPushBackButton ();
			}
			if (Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.KeypadEnter)) {
				OnPushDoneButton ();
			}
		}
	}

	public void Restart () {
		if (reOrderableList.Interactable) { return; }
		InfoText.text = "";
		reOrderableList.Interactable = true;
		doneButton.interactable = true;
	}

	/// <summary>戻るボタンが押された</summary>
	public void OnPushBackButton () {
		if (reOrderableList.Orderable) {
			reOrderableList.Orderable = false;
		} else {
			OnPushDoneButton ();
		}
	}

	/// <summary>完了ボタンが押された</summary>
	public void OnPushDoneButton () {
		reOrderableList.Interactable = false;
		doneButton.interactable = false;
		printResult (reOrderableList.Indexes, -1);
	}

	// モード切替コールバック
	public void OnChangeModeListener (bool orderable) {
		sampleList.transform.GetChild (1).gameObject.SetActive (orderable); // 並べ替え時専用コントロールパネル
		doneButton.interactable = !orderable; // 完了ボタン
		foreach (var obj in reOrderableList.GameObjects) {
			obj.transform.GetChild (3).gameObject.SetActive (orderable);
		}
	}
	
	// 選択コールバック
	public void OnSelectListener (int index) {
		reOrderableList.Interactable = false;
		doneButton.interactable = false;
		printResult (reOrderableList.Indexes, index);
	}

	// 並べ替えコールバック
	private int elementIndex;
	public void OnBeginOrderListener (int index) {
		elementIndex = index;
		StartCoroutine (printDelay (() => InfoText.text = $"開始: {index}"));
	}

	public void OnUpdateOrderListener (int index) {
		StartCoroutine (printDelay (() => InfoText.text = $"更新: {elementIndex} ⇒ {index}"));
	}

	public void OnEndOrderListener (int index) {
		StartCoroutine (printDelay (() => InfoText.text = $"終了: {string.Join (", ", reOrderableList.Indexes.ConvertAll (i => i.ToString ()))}"));
	}

	/// <summary>結果表示</summary>
	private void printResult (List<int> indexes, int index) {
		StartCoroutine (printDelay (() => $"クリックで再実行\n\n{((index < 0) ? "未選択" : $"選択: {index}")}\n配置: {string.Join (", ", indexes.ConvertAll (i => i.ToString ()))}"));
	}

	/// <summary>遅延表示</summary>
	private IEnumerator printDelay (System.Func<string> func) {
		yield return null;
		InfoText.text = func ();
	}

}
