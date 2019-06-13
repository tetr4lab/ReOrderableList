using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>静的リストサンプル</summary>
public class SampleStatic : MonoBehaviour {

	private Text InfoText;
	private int elementIndex;
	private ReOrderableList reOrderableList;

	private void Start () {
		InfoText = transform.parent.GetChild (transform.parent.childCount - 1).GetComponentInChildren<Text> ();
		reOrderableList = GetComponentInChildren<ReOrderableList> ();
		Restart ();
	}

	public void Restart () {
		InfoText.text = "";
		reOrderableList.Interactable = true;
	}

	// 完了コールバック
	public void OnDoneListener (List<int> indexes, int index) {
		InfoText.text = $"クリックで再実行\n\n{((index < 0) ? "未選択" : $"選択: {index}")}\n配置: {string.Join (", ", indexes.ConvertAll (i => i.ToString ()))}";
	}

	// 並べ替えコールバック
	public void OnBeginReOrderListener (List<int> indexes, int index) {
		elementIndex = index;
		InfoText.text = $"開始: {index}";
	}

	public void OnUpdateReOrderListener (List<int> indexes, int index) {
		InfoText.text = $"更新: {elementIndex} ⇒ {index}";
	}

	public void OnEndReOrderListener (List<int> indexes, int index) {
		InfoText.text = $"終了: {string.Join (", ", indexes.ConvertAll (i => i.ToString ()))}";
	}

}
