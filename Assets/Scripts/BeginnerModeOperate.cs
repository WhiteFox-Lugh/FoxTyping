using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BeginnerModeOperate : MonoBehaviour {
	private const int MODE_NUM = 0; // 短文練習モード固定
	private static int prevChapterNum = 1;
	[SerializeField] private GameObject chapterSelect;

	private static Dictionary<int, string> beginnerDatasetFileName = new Dictionary<int, string> {
		{101, "keyboardMiddle"},
		{102, "keyboardUpper"},
		{103, "keyboardLower"},
		{104, "chapter1All"},
		{201, "alphabetLower"},
		{202, "alphabetUpper"},
		{203, "numberAndSymbol"},
		{204, "chapter2All"},
		{301, "hiragana_a"},
		{302, "hiragana_k"},
		{303, "hiragana_s"},
		{304, "hiragana_t"},
		{305, "hiragana_n"},
		{306, "hiragana_h"},
		{307, "hiragana_m"},
		{308, "hiragana_y"},
		{309, "hiragana_r"},
		{310, "chapter3All"},
		{401, "hiragana_ky"},
		{402, "hiragana_sy"},
		{403, "hiragana_ty"},
		{404, "hiragana_ny"},
		{405, "hiragana_hy"},
		{406, "hiragana_my"},
		{407, "hiragana_ry"},
		{408, "hiragana_ltu"},
		{409, "hiragana_small"},
		{410, "chapter4All"},
		{501, "shortWords"},
		{502, "katakanaWords"},
		{503, "jukugo"},
		{504, "proverb"},
		{505, "conversation"}
	};

	// Start is called before the first frame update
	void Awake(){
		SetPreviousSettings();
	}

	// Update is called once per frame
	void Update(){
		// ChangeConfigPanel();
	}

	/// <summary>
	/// 直前の練習内容を選択肢にセット
	/// </summary>
	void SetPreviousSettings(){
		for (int i = 1; i <= chapterSelect.transform.childCount; ++i){
			var toggle = transform.Find("ChapterSelect/Chapter" + i.ToString() + "Toggle").GetComponent<Toggle>();
			toggle.isOn = !toggle.isOn; // OnValueChanged を強制動作させることで Panel を取得しなくて済む
			toggle.isOn = prevChapterNum == i;
		}
	}

	/// <summary>
	/// Keycode と対応する操作
	/// </summary>
	void KeyCheck(KeyCode kc){
		if(KeyCode.Backspace == kc){
			SceneManager.LoadScene("ModeSelectScene");
		}
	}

	/// <summary>
	/// キーボードの入力などの受付
	/// </summary>
	void OnGUI() {
		Event e = Event.current;
		if (e.type == EventType.KeyDown && e.type != EventType.KeyUp && e.keyCode != KeyCode.None
		&& !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2)){
			KeyCheck(e.keyCode);
		}
  }

	public void OnClickButton(int number){
		ConfigScript.GameMode = 0; // 短文モード固定
		ConfigScript.DataSetName = beginnerDatasetFileName[number];
		ConfigScript.Tasks = -1; // 無限回
		Debug.Log(number);
	}
}
