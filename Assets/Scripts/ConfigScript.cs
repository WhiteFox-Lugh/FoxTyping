using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ConfigScript : MonoBehaviour {
	// 文章数のデフォルト値
	private const int TASK_NUM_UNIT = 5;
	private const int TASK_NUM_OPTION_INIT = 4;
		// 練習後、元の設定を再現するための変数
	private static int dataSetNameNum;
	private static int longDataSetNameNum;
	[SerializeField] TMP_Dropdown UIGameMode;
	[SerializeField] TMP_Dropdown UIDataSetName;
	[SerializeField] TMP_Dropdown UILongDataSetName;
	[SerializeField] TMP_Dropdown UISentenceNum;
	[SerializeField] GameObject ConfigPanel;
	[SerializeField] GameObject LongSentenceConfigPanel;

	enum GameModeNumber {
		ShortSentence,
		LongSentence
	}

	private static string[] datasetFileName = new string[2] {
		"FoxTypingOfficial", "FoxTypingOfficialEnglish"
	};

	private static string[] longDatasetFileName = new string[2] {
		"Long_Constitution", "Long_ConstitutionEnglish"
	};

	public static int Tasks {
		private set;
		get;
	} = -1;

	// ゲームモード
	// 0 : 短文
	// 1 : 長文
	public static int GameMode {
		private set;
		get;
	} = 0;

	// 画面中部に表示するもの
	// 0 : タイピングパフォーマンス情報
	// 1 : アシストキーボード
	public static int InfoPanelMode {
		set;
		get;
	} = 0;

	// 短文打つモードでのデータセットのファイル名
	public static string DataSetName {
		private set;
		get;
	} = "FoxTypingOfficial";

	// 長文打つモードでのデータセットのファイル名
	public static string LongSentenceTaskName {
		private set;
		get;
	} = "Long_Constitution";

	/// <summary>
	/// 初期化など
	/// </summary>
	void Awake () {
		UIGameMode.value = GameMode;
		UIDataSetName.value = dataSetNameNum;
		UILongDataSetName.value = longDataSetNameNum;
		UISentenceNum.enabled = true;
	}

	/// <summary>
	/// 1フレームごとの処理
	/// </summary>
	void Update () {
		SetGameMode();
		ChangeConfigPanel();
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
	/// オプションをプロパティにセット（短文練習）
	/// </summary>
	void SetShortModeConfig(){
		dataSetNameNum = UIDataSetName.value;
		DataSetName = datasetFileName[dataSetNameNum];
		Tasks = (UISentenceNum.value + 1) * TASK_NUM_UNIT;
	}

	/// <summary>
	/// 長文データセットをプロパティにセット
	/// </summary>
	void SetLongDataSet(){
		longDataSetNameNum = UILongDataSetName.value;
		LongSentenceTaskName = longDatasetFileName[longDataSetNameNum];
	}

	/// <summary>
	/// Keycode と対応する操作
	/// </summary>
	void KeyCheck(KeyCode kc){
		if(KeyCode.Return == kc || KeyCode.KeypadEnter == kc){
			SetGameMode();
			if (GameMode == (int)GameModeNumber.ShortSentence){
				SetShortModeConfig();
				SceneManager.LoadScene("TypingScene");
			}
			else if(GameMode == (int)GameModeNumber.LongSentence){
				SetLongDataSet();
				SceneManager.LoadScene("LongSentenceTypingScene");
			}
		}
		else if(KeyCode.Escape == kc){
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
