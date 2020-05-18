using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>動的リストサンプル</summary>
public class SampleDynamic : MonoBehaviour {

	public static Text InfoText;
	private GameObject sampleList;
	private List<string> itemNames;
	private GameObject listPrefab;
	private GameObject elementPrefab;
	private ReOrderableList reOrderableList;
	private Button doneButton;
	private GameObject orderControlPanel;
	private Button backButton;

	private void Start () {
		InfoText = transform.parent.GetChild (transform.parent.childCount - 1).GetComponentInChildren<Text> ();
		listPrefab = Resources.Load ("SampleList") as GameObject;
		elementPrefab = Resources.Load ("SampleElement") as GameObject;
		Restart ();
	}

	private void Update () {
		if (sampleList && reOrderableList.Interactable) {
			if (Input.GetKeyDown (KeyCode.Escape)) {
				if (reOrderableList.Orderable) {
					reOrderableList.Orderable = false;
				} else {
					OnPushDoneButton ();
				}
			}
			if (Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.KeypadEnter)) {
				OnPushDoneButton ();
			}
		}
	}

	public void Restart () {
		if (sampleList) { return; }
		// デバッグ表示クリア
		InfoText.text = "";
		// 内部リスト項目生成
		itemNames = new List<string> { };
		for (var i = 0; i < 100; i++) {
			itemNames.Add (new RandomKey ().Key);
		}
		// リスト作成
		sampleList = Instantiate (listPrefab, transform);
		doneButton = sampleList.transform.GetChild (2).GetComponent<Button> ();
		doneButton.onClick.AddListener (OnPushDoneButton);
		reOrderableList = sampleList.GetComponentInChildren<ReOrderableList> ();
		orderControlPanel = sampleList.transform.GetChild (1).gameObject;
		backButton = orderControlPanel.GetComponentInChildren<Button> ();
		backButton.onClick.AddListener (() => { reOrderableList.Orderable = false; });
		// モード切替コールバック
		reOrderableList.AddOnChangeModeListener (
			(orderable) => {
				orderControlPanel.SetActive (orderable); // 並べ替え時専用コントロールパネル
				doneButton.interactable = !orderable; // 完了ボタン
				foreach (var obj in reOrderableList.GameObjects) {
					obj.transform.GetChild (3).gameObject.SetActive (orderable); // ドラッグハンドル
				}
			}
		);
		// 選択コールバック
		reOrderableList.AddOnSelectListener (
			(index) => {
				reOrderableList.Interactable = false;
				printResult (reOrderableList.Indexes, index);
				Destroy (sampleList);
			}
		);
		// 並べ替えコールバック
		int elementIndex = -1;
		reOrderableList.AddOnBeginOrderListener (
			(index) => {
				elementIndex = index;
				StartCoroutine (printDelay (() => $"開始: {index}"));
			}
		);
		reOrderableList.AddOnUpdateOrderListener (
			(index) => {
				StartCoroutine (printDelay (() => $"更新: {elementIndex} ⇒ {index}"));
			}
		);
		reOrderableList.AddOnEndOrderListener (
			(index) => {
				StartCoroutine (printDelay (() => $"終了: {string.Join (", ", reOrderableList.Indexes.ConvertAll (i => i.ToString ()))}"));
			}
		);
		// リスト項目作成
		for (var i = 0; i < itemNames.Count; i++) {
			var obj = Instantiate (elementPrefab, transform);
			var texts = obj.GetComponentsInChildren<Text> ();
			texts [0].text = i.ToString ();
			texts [1].text = itemNames [i];
			reOrderableList.AddElement (obj);
		}
	}

	/// <summary>完了ボタンが押された</summary>
	public void OnPushDoneButton () {
		reOrderableList.Interactable = false;
		printResult (reOrderableList.Indexes, -1);
		Destroy (sampleList);
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

public class RandomKey {

	private const int MinLength = 6;  // inclusive
	private const int MaxLength = 10; // exclusive
	private const string Letters = "1234567890-ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_";

	public string Key { get; private set; }
	public int Length { get { return Key.Length; } }

	public RandomKey () : this (MaxLength, MinLength) { }

	public RandomKey (int max, int min) {
		var len = Random.Range (min, max);
		var chars = new List<char> { };
		for (var i = 0; i < len; i++) {
			chars.Add (Letters [Random.Range (0, Letters.Length)]);
		}
		Key = new string (chars.ToArray ());
	}

}
