using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BeginnerModeOperate : MonoBehaviour {
	private const int MODE_NUM = 0; // 短文練習モード固定
	private static int prevChapterNum = 1; // 前回の練習章を維持
	[SerializeField] private GameObject chapterSelect;

	// 練習データセット
	// 先頭が章番号、下2桁がナンバリング
	private static Dictionary<int, string> beginnerDatasetFileName = new Dictionary<int, string> {
		{101, "keyboardMiddle"},
		{102, "keyboardUpper"},
		{103, "keyboardLower"},
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
		{409, "hiragana_long_vowel"},
		{410, "hiragana_xn"}
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
	/// 直前の練習内容を画面に反映させる
	/// </summary>
	private void SetPreviousSettings(){
		for (int i = 1; i <= chapterSelect.transform.childCount; ++i){
			var toggle = transform.Find("ChapterSelect/Chapter" + i.ToString() + "Toggle").GetComponent<Toggle>();
			// OnValueChanged を強制動作させることで Panel を取得しなくて済む
			toggle.isOn = !toggle.isOn;
			toggle.isOn = prevChapterNum == i;
		}
	}

	/// <summary>
	/// Keycode と対応する操作
	/// <param name="kc">KeyCode</param>
	/// </summary>
	private void KeyCheck(KeyCode kc){
		if(KeyCode.Backspace == kc){
			SceneManager.LoadScene("ModeSelectScene");
		}
	}

	/// <summary>
	/// キーボードの入力などの受付処理
	/// </summary>
	void OnGUI() {
		Event e = Event.current;
		if (e.type == EventType.KeyDown && e.type != EventType.KeyUp && e.keyCode != KeyCode.None
		&& !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2)){
			KeyCheck(e.keyCode);
		}
  }

	/// <summary>
	/// 各練習ボタンを押したときの動作
	/// <param name="buttonNumber">押したボタンに割り当てられた番号</param>
	/// </summary>
	public void OnClickButton(int buttonNumber){
		ConfigScript.GameMode = 0; // 短文モード固定
		ConfigScript.DataSetName = beginnerDatasetFileName[buttonNumber];
		ConfigScript.Tasks = -1; // 無限回練習できるようにするため、-1
		ConfigScript.IsBeginnerMode = true;
		prevChapterNum = buttonNumber / 100;
		SceneManager.LoadScene("BeginnerTypingScene");
	}
}
