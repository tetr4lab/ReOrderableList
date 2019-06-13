using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>動的リストサンプル</summary>
public class SampleDynamic : MonoBehaviour {

	public static Text InfoText;
	private GameObject SampleList;
	private List<string> ItemNames;
	private GameObject ListPrefab;
	private GameObject ElementPrefab;

	private void Start () {
		ListPrefab = Resources.Load ("SampleList") as GameObject;
		ElementPrefab = Resources.Load ("SampleElement") as GameObject;
		InfoText = transform.parent.GetComponentInChildren<Text> ();
		Restart ();
	}

	public void Restart () {
		if (SampleList) { return; }
		// デバッグ表示クリア
		InfoText.text = "";
		// 内部リスト項目生成
		ItemNames = new List<string> { };
		for (var i = 0; i < 100; i++) {
			ItemNames.Add (new RandomKey ().Key);
		}
		// リスト作成
		SampleList = Instantiate (ListPrefab, transform);
		var reOrderableList = SampleList.GetComponentInChildren<ReOrderableList> ();
		// 完了コールバック
		reOrderableList.AddOnDoneListener ( 
			(indexes, index) => {
				InfoText.text = $"クリックで再実行\n\n{((index < 0) ? "未選択" : $"選択: {index}: {ItemNames [index]}")}\n配置: {string.Join (", ", indexes.ConvertAll (i => i.ToString ()))}";
			}
		);
		// 並べ替えコールバック
		int elementIndex = -1;
		reOrderableList.AddOnBeginReOrderListener (
			(indexes, index) => {
				elementIndex = index;
				InfoText.text = $"開始: {index}";
			}
		);
		reOrderableList.AddOnUpdateReOrderListener (
			(indexes, index) => {
				InfoText.text = $"更新: {elementIndex} ⇒ {index}";
			}
		);
		reOrderableList.AddOnEndReOrderListener (
			(indexes, index) => {
				InfoText.text = $"終了: {string.Join (", ", indexes.ConvertAll (i => i.ToString ()))}";
			}
		);
		// リスト項目作成
		for (var i = 0; i < ItemNames.Count; i++) {
			var obj = Instantiate (ElementPrefab, transform);
			var texts = obj.GetComponentsInChildren<Text> ();
			texts [0].text = i.ToString ();
			texts [1].text = ItemNames [i];
			reOrderableList.AddElement (obj);
		}
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
