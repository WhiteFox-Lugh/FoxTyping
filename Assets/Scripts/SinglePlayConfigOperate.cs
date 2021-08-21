using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SinglePlayConfigOperate : MonoBehaviour {
	private const int TASK_UNIT = 5;
	private static int prevDropdownGameMode = 0;
	private static int prevDropdownTaskNum = 5;
	private static int prevDropdownShortDataset = 0;
	private static int prevDropdownLongDataset = 0;
	[SerializeField] private TMP_Dropdown UIGameMode;
	[SerializeField] private TMP_Dropdown UIDataSetName;
	[SerializeField] private TMP_Dropdown UILongDataSetName;
	[SerializeField] private TMP_Dropdown UISentenceNum;
	[SerializeField] private GameObject ConfigPanel;
	[SerializeField] private GameObject LongSentenceConfigPanel;

	private static string[] shortDatasetFileName = new string[2] {
		"FoxTypingOfficial", "FoxTypingOfficialEnglish"
	};

	private static string[] longDatasetFileName = new string[2] {
		"Long_Constitution", "Long_ConstitutionEnglish"
	};

	enum GameModeNumber {
		ShortSentence,
		LongSentence
	}

	// Start is called before the first frame update
	void Awake(){
		SetPreviousSettings();
	}

	// Update is called once per frame
	void Update(){
		ChangeConfigPanel();
	}

	/// <summary>
	/// 直前の練習内容を選択肢にセット
	/// </summary>
	void SetPreviousSettings(){
		UIGameMode.value = prevDropdownGameMode;
		UIDataSetName.value = prevDropdownShortDataset;
		UILongDataSetName.value = prevDropdownLongDataset;
		UISentenceNum.value = prevDropdownTaskNum;
	}

	/// <summary>
	/// 今回の練習内容を設定に反映させる
	/// </summary>
	void SetCurrentSettings(){
		prevDropdownGameMode = UIGameMode.value;
		prevDropdownShortDataset = UIDataSetName.value;
		prevDropdownLongDataset = UILongDataSetName.value;
		prevDropdownTaskNum = UISentenceNum.value;
		ConfigScript.GameMode = UIGameMode.value;
		ConfigScript.DataSetName = shortDatasetFileName[UIDataSetName.value];
		ConfigScript.Tasks = (UISentenceNum.value + 1) * TASK_UNIT;
		ConfigScript.LongSentenceTaskName = longDatasetFileName[UILongDataSetName.value];
	}

	/// <summary>
	/// 選択されているゲームモードにより表示パネルを変更
	/// </summary>
	void ChangeConfigPanel(){
		ConfigPanel.SetActive(UIGameMode.value == (int)GameModeNumber.ShortSentence);
    LongSentenceConfigPanel.SetActive(UIGameMode.value == (int)GameModeNumber.LongSentence);
	}

	/// <summary>
	/// Keycode と対応する操作
	/// </summary>
	void KeyCheck(KeyCode kc){
		if(KeyCode.Return == kc || KeyCode.KeypadEnter == kc){
			var selectedMode = UIGameMode.value;
			SetCurrentSettings();
			if (selectedMode == (int)GameModeNumber.ShortSentence){
				SceneManager.LoadScene("TypingScene");
			}
			else if(selectedMode == (int)GameModeNumber.LongSentence){
				SceneManager.LoadScene("LongSentenceTypingScene");
			}
		}
		else if(KeyCode.Backspace == kc){
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
