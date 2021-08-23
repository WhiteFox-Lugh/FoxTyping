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
	private static int prevSectionNum = 1;
	[SerializeField] private TMP_Dropdown UIGameMode;
	[SerializeField] private TMP_Dropdown UIDataSetName;
	[SerializeField] private TMP_Dropdown UILongDataSetName;
	[SerializeField] private TMP_Dropdown UISentenceNum;
	[SerializeField] private GameObject ConfigPanel;
	[SerializeField] private GameObject LongSentenceConfigPanel;

	private static string[] practiceDataset = new string[2] {
		"FoxTypingOfficial", "FoxTypingOfficialEnglish"
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
}
