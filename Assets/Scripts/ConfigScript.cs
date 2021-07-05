using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ConfigScript : MonoBehaviour {
	// 文章数の最小値、最大値、デフォルト値
	private const int MIN_SENTENCE_NUM = 10;
	private const int MAX_SENTENCE_NUM = 100;
	private const int DEFAULT_SENTENCE_NUM = 30;
	[SerializeField] TMP_Dropdown UIGameMode;
	[SerializeField] TMP_InputField UISentenceNum;
	[SerializeField] GameObject ConfigPanel;
	[SerializeField] GameObject LongSentenceConfigPanel;

	enum GameModeNumber {
		ShortSentence,
		LongSentence
	}

	public static int Tasks {
		private set;
		get;
	} = 30;

	// ゲームモード
	// 0 : 短文を打つモード
	// 1 : 長文を打つモード
	public static int GameMode {
		private set;
		get;
	} = 0;

	// 短文打つモードでのデータセットのファイル名
	public static string DataSetName {
		private set;
		get;
	} = "official";

	// 長文打つモードでのデータセットのファイル名
	public static string LongSentenceTaskName {
		private set;
		get;
	} = "long_constitution";

	/// <summary>
	/// 初期化など
	/// </summary>
	void Awake () {
		UIGameMode.value = GameMode;
		UISentenceNum.text = Tasks.ToString();
		UISentenceNum.enabled = true;
	}

	/// <summary>
	/// 1フレームごとの処理
	/// </summary>
	void Update () {
		SetGameMode();
		ChangeConfigPanel();
		if (!UISentenceNum.isFocused && !IsSentenceNumValid()){
			ComplementSentenceNum();
		}
	}

	/// <summary>
	/// 文章数の値をデフォルト値で補完する
	/// </summary>
	void ComplementSentenceNum(){
		UISentenceNum.text = DEFAULT_SENTENCE_NUM.ToString();
	}

	/// <summary>
	/// 選択されているゲームモードにより表示パネルを変更
	/// </summary>
	void ChangeConfigPanel(){
		ConfigPanel.SetActive(GameMode == (int)GameModeNumber.ShortSentence);
    LongSentenceConfigPanel.SetActive(GameMode == (int)GameModeNumber.LongSentence);
	}

	/// <summary>
	/// ゲームモードの値をプロパティにセット
	/// </summary>
	void SetGameMode(){
		GameMode = UIGameMode.value;
	}

	/// <summary>
	/// 文章数の値が規定の値に入っているかチェックする
	/// </summary>
	bool IsSentenceNumValid(){
		int num;
		bool isNumOfSentenceParseSuccess = Int32.TryParse(UISentenceNum.text, out num);
		bool isNumOfSentenceValid = (isNumOfSentenceParseSuccess && MIN_SENTENCE_NUM <= num && num <= MAX_SENTENCE_NUM);
		if (isNumOfSentenceValid){
			Tasks = num;
		}
		return isNumOfSentenceValid;
	}

	/// <summary>
	/// Keycode と対応する操作
	/// </summary>
	void KeyCheck(KeyCode kc){
		if(KeyCode.Return == kc || KeyCode.KeypadEnter == kc){
			SetGameMode();
			if (GameMode == (int)GameModeNumber.ShortSentence && IsSentenceNumValid()){
				SceneManager.LoadScene("CountDownScene");
			}
			else if(GameMode == (int)GameModeNumber.LongSentence){
				SceneManager.LoadScene("LongSentenceTypingScene");
			}
		}
		else if(KeyCode.Escape == kc){
			SceneManager.LoadScene("TitleScene");
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
