using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SinglePlayConfigOperate : MonoBehaviour {
	private const int TASK_UNIT = 5;
	private const int LONG_MAX_TIME_LIMIT = 60 * 60;
	private const int LONG_MIN_TIME_LIMIT = 1;
	private static int prevDropdownGameMode = 0;
	private static int prevDropdownTaskNum = 5;
	private static int prevDropdownShortDataset = 0;
	private static int prevDropdownLongDataset = 0;
	private static int longSentenceTimeLimitVal = 0;
	[SerializeField] private TMP_Dropdown UIGameMode;
	[SerializeField] private TMP_Dropdown UIDataSetName;
	[SerializeField] private TMP_Dropdown UILongDataSetName;
	[SerializeField] private TMP_Dropdown UISentenceNum;
	[SerializeField] private GameObject ConfigPanel;
	[SerializeField] private GameObject LongSentenceConfigPanel;
	[SerializeField] private TMP_InputField LongSentenceTimeLimitMinute;
	[SerializeField] private TMP_InputField LongSentenceTimeLimitSecond;

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
		longSentenceTimeLimitVal = ConfigScript.LongSentenceTimeLimit;
		SetLongSentenceTimeLimitUI();
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
		ConfigScript.LongSentenceTimeLimit = longSentenceTimeLimitVal;
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

	/// <summary>
	/// val 秒を n分 m秒 に直す
	/// </summary>
	private (int minute, int second) GetTimeMSExpr(int val){
		return (val / 60, val % 60);
	}

	/// <summary>
	/// 制限時間表示をセット
	/// </summary>
	private void SetLongSentenceTimeLimitUI(){
		var timeLimit = GetTimeMSExpr(longSentenceTimeLimitVal);
		LongSentenceTimeLimitMinute.text = timeLimit.minute.ToString();
		LongSentenceTimeLimitSecond.text = timeLimit.second.ToString();
	}

	/// <summary>
	/// プラスボタンを押したときの挙動
	/// </sumamry>
	public void OnClickPlusButton(int num){
		longSentenceTimeLimitVal += (num == 0) ? 60 : 1;
		if (longSentenceTimeLimitVal > LONG_MAX_TIME_LIMIT){
			longSentenceTimeLimitVal = LONG_MAX_TIME_LIMIT;
		}
		SetLongSentenceTimeLimitUI();
	}

	/// <summary>
	/// マイナスボタンを押したときの挙動
	/// </sumamry>
	public void OnClickMinusButton(int num){
		longSentenceTimeLimitVal -= (num == 0) ? 60 : 1;
		if (longSentenceTimeLimitVal < LONG_MIN_TIME_LIMIT){
			longSentenceTimeLimitVal = LONG_MIN_TIME_LIMIT;
		}
		SetLongSentenceTimeLimitUI();
	}
}
