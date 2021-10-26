using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SinglePlayConfigOperate : MonoBehaviour
{
  private const int TASK_UNIT = 5;
  private const int LONG_MAX_TIME_LIMIT = 60 * 60;
  private const int LONG_MIN_TIME_LIMIT = 1;
  private const int SETTINGS_DEFAULT_VAL = 0;
  private const int SETTINGS_TASKNUM_DEFAULT_VAL = 5;
  private const int SETTINGS_LONG_TIME_DEFAULT_VAL = 300;
  private const int SETTINGS_CPU_KPM_DEFAULT = 300;
  private static int prevDropdownGameMode = 0;
  private static int prevDropdownTaskNum = 5;
  private static int prevDropdownShortDataset = 0;
  private static int prevDropdownLongDataset = 0;
  private static int prevDropdownUseYomigana = 0;
  private static int prevCPUKpm = 1;
  private static int prevIsUseCPUGuide = 0;
  private static int prevLongTimeLimit = 300;
  private static int prevInputType = 0;
  private static int longSentenceTimeLimitVal = 300;
  [SerializeField] private TMP_Dropdown UIGameMode;
  [SerializeField] private TMP_Dropdown UIDataSetName;
  [SerializeField] private TMP_Dropdown UILongDataSetName;
  [SerializeField] private TMP_Dropdown UISentenceNum;
  [SerializeField] private TMP_Dropdown UIUseCPUKpmGuide;
  [SerializeField] private TMP_Dropdown UIUseYomigana;
  [SerializeField] private TMP_Dropdown UIInputType;
  [SerializeField] private TMP_InputField InputCPUSpeed;
  [SerializeField] private GameObject ConfigPanel;
  [SerializeField] private GameObject LongSentenceConfigPanel;
  [SerializeField] private TMP_InputField LongSentenceTimeLimitMinute;
  [SerializeField] private TMP_InputField LongSentenceTimeLimitSecond;

  private static string[] shortDatasetFileName = new string[2] {
    "FoxTypingOfficial", "FoxTypingOfficialEnglish"
  };

  private static string[] longDatasetFileName = new string[2] {
    "Long_Constitution", "Long_Gongitsune"
  };

  enum GameModeNumber
  {
    ShortSentence,
    LongSentence
  }

  // Start is called before the first frame update
  void Awake()
  {
    LoadPlayerPrefSettings();
    SetPreviousSettings();
  }

  // Update is called once per frame
  void Update()
  {
    ChangeConfigPanel();
  }

  /// <summary>
  /// PlayerPrefs から設定を読み込む
  /// </summary>
  private void LoadPlayerPrefSettings()
  {
    prevDropdownGameMode = PlayerPrefs.GetInt("foxtyping_single_gamemode", SETTINGS_DEFAULT_VAL);
    prevDropdownTaskNum = PlayerPrefs.GetInt("foxtyping_single_tasknum", SETTINGS_TASKNUM_DEFAULT_VAL);
    prevDropdownShortDataset = PlayerPrefs.GetInt("foxtyping_single_short_data", SETTINGS_DEFAULT_VAL);
    prevDropdownLongDataset = PlayerPrefs.GetInt("foxtyping_single_long_data", SETTINGS_DEFAULT_VAL);
    prevDropdownUseYomigana = PlayerPrefs.GetInt("foxtyping_single_use_yomigana", SETTINGS_DEFAULT_VAL);
    prevLongTimeLimit = PlayerPrefs.GetInt("foxtyping_single_long_time", SETTINGS_LONG_TIME_DEFAULT_VAL);
    prevCPUKpm = PlayerPrefs.GetInt("foxtyping_single_cpukpm", SETTINGS_CPU_KPM_DEFAULT);
    prevIsUseCPUGuide = PlayerPrefs.GetInt("foxtyping_single_use_cpuguide", SETTINGS_DEFAULT_VAL);
  }

  /// <summary>
  /// PlayerPrefs に設定を保存
  /// </summary>
  private void SavePlayerPrefSettings()
  {
    PlayerPrefs.SetInt("foxtyping_single_gamemode", UIGameMode.value);
    PlayerPrefs.SetInt("foxtyping_single_tasknum", UISentenceNum.value);
    PlayerPrefs.SetInt("foxtyping_single_short_data", UIDataSetName.value);
    PlayerPrefs.SetInt("foxtyping_single_long_data", UILongDataSetName.value);
    PlayerPrefs.SetInt("foxtyping_single_use_yomigana", UIUseYomigana.value);
    PlayerPrefs.SetInt("foxtyping_single_long_time", longSentenceTimeLimitVal);
    PlayerPrefs.SetInt("foxtyping_single_cpukpm", Int32.Parse(InputCPUSpeed.text));
    PlayerPrefs.SetInt("foxtyping_single_use_cpuguide", UIUseCPUKpmGuide.value);
  }

  /// <summary>
  /// 直前の練習内容を選択肢にセット
  /// </summary>
  private void SetPreviousSettings()
  {
    UIGameMode.value = prevDropdownGameMode;
    UIDataSetName.value = prevDropdownShortDataset;
    UILongDataSetName.value = prevDropdownLongDataset;
    UIUseYomigana.value = prevDropdownUseYomigana;
    UISentenceNum.value = prevDropdownTaskNum;
    UIInputType.value = prevInputType;
    longSentenceTimeLimitVal = prevLongTimeLimit;
    UIUseCPUKpmGuide.value = (prevIsUseCPUGuide == 1 ? 1 : 0);
    InputCPUSpeed.interactable = prevIsUseCPUGuide == 1;
    InputCPUSpeed.text = prevCPUKpm.ToString();
    SetLongSentenceTimeLimitUI();
  }

  /// <summary>
  /// 今回の練習内容を設定に反映させる
  /// </summary>
  private void SetCurrentSettings()
  {
    CheckKpmSettings();
    prevDropdownGameMode = UIGameMode.value;
    prevDropdownShortDataset = UIDataSetName.value;
    prevDropdownLongDataset = UILongDataSetName.value;
    prevDropdownTaskNum = UISentenceNum.value;
    prevDropdownUseYomigana = UIUseYomigana.value;
    prevCPUKpm = Int32.Parse(InputCPUSpeed.text);
    prevInputType = UIInputType.value;
    ConfigScript.GameMode = prevDropdownGameMode;
    ConfigScript.DataSetName = shortDatasetFileName[prevDropdownShortDataset];
    ConfigScript.Tasks = (prevDropdownTaskNum + 1) * TASK_UNIT;
    ConfigScript.LongSentenceTaskName = longDatasetFileName[prevDropdownLongDataset];
    ConfigScript.LongSentenceTimeLimit = longSentenceTimeLimitVal;
    ConfigScript.UseCPUGuide = UIUseCPUKpmGuide.value == 1;
    ConfigScript.CPUKpm = prevCPUKpm;
    ConfigScript.UseRuby = UIUseYomigana.value == 1;
    ConfigScript.IsBeginnerMode = false;
    ConfigScript.InfoPanelMode = 0;
    ConfigScript.InputMode = prevInputType;
  }

  /// <summary>
  /// 選択されているゲームモードにより表示パネルを変更
  /// </summary>
  private void ChangeConfigPanel()
  {
    ConfigPanel.SetActive(UIGameMode.value == (int)GameModeNumber.ShortSentence);
    LongSentenceConfigPanel.SetActive(UIGameMode.value == (int)GameModeNumber.LongSentence);
  }

  /// <summary>
  /// kpm 設定が正しいかチェック
  /// </summary>
  public void CheckKpmSettings()
  {
    int kpm;
    if (int.TryParse(InputCPUSpeed.text, out kpm))
    {
      if (kpm <= 0)
      {
        InputCPUSpeed.text = "1";
      }
    }
    else
    {
      InputCPUSpeed.text = "300";
    }
  }

  /// <summary>
  /// Keycode と対応する操作
  /// <param name="kc">keycode</param>
  /// </summary>
  private void KeyCheck(KeyCode kc)
  {
    if (KeyCode.Space == kc)
    {
      BeforeStartPractice();
    }
    else if (KeyCode.Escape == kc)
    {
      ReturnModeSelectScene();
    }
  }

  /// <summary>
  /// 練習を開始する前の処理
  /// </summary>
  public void BeforeStartPractice()
  {
    var selectedMode = UIGameMode.value;
    SetCurrentSettings();
    SavePlayerPrefSettings();
    if (selectedMode == (int)GameModeNumber.ShortSentence)
    {
      SceneManager.LoadScene("TypingScene");
    }
    else if (selectedMode == (int)GameModeNumber.LongSentence)
    {
      SceneManager.LoadScene("LongSentenceTypingScene");
    }
  }

  /// <summary>
  /// モード選択画面へ戻る
  /// </summary>
  public void ReturnModeSelectScene()
  {
    SceneManager.LoadScene("ModeSelectScene");
  }

  /// <summary>
  /// キーボードの入力などの受付
  /// </summary>
	void OnGUI()
  {
    Event e = Event.current;
    if (e.type == EventType.KeyDown && e.type != EventType.KeyUp && e.keyCode != KeyCode.None
    && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
    {
      KeyCheck(e.keyCode);
    }
  }

  /// <summary>
  /// timeSecond 秒を n分 m秒 に直す
  /// <param name="timeSecond">時間(秒単位)</param>
  /// <returns>(分, 秒)に直したもの</returns>
  /// </summary>
  private (int minute, int second) GetTimeMSExpr(int timeSecond)
  {
    return (timeSecond / 60, timeSecond % 60);
  }

  /// <summary>
  /// 制限時間表示をセット
  /// </summary>
  private void SetLongSentenceTimeLimitUI()
  {
    var timeLimit = GetTimeMSExpr(longSentenceTimeLimitVal);
    LongSentenceTimeLimitMinute.text = timeLimit.minute.ToString();
    LongSentenceTimeLimitSecond.text = timeLimit.second.ToString();
  }

  /// <summary>
  /// プラスボタンを押したときの挙動
  /// <param name="num">分のボタンか秒のボタンか区別する引数</param>
  /// </sumamry>
  public void OnClickPlusButton(int num)
  {
    longSentenceTimeLimitVal += (num == 0) ? 60 : 1;
    if (longSentenceTimeLimitVal > LONG_MAX_TIME_LIMIT)
    {
      longSentenceTimeLimitVal = LONG_MAX_TIME_LIMIT;
    }
    SetLongSentenceTimeLimitUI();
  }

  /// <summary>
  /// マイナスボタンを押したときの挙動
  /// <param name="num">分のボタンか秒のボタンか区別する引数</param>
  /// </sumamry>
  public void OnClickMinusButton(int num)
  {
    longSentenceTimeLimitVal -= (num == 0) ? 60 : 1;
    if (longSentenceTimeLimitVal < LONG_MIN_TIME_LIMIT)
    {
      longSentenceTimeLimitVal = LONG_MIN_TIME_LIMIT;
    }
    SetLongSentenceTimeLimitUI();
  }

  /// <summary>
  /// CPU 速度ガイドの設定変更時の挙動
  /// </summary>
  public void OnUseCPUGuideValueChanged()
  {
    InputCPUSpeed.interactable = UIUseCPUKpmGuide.value == 1;
  }
}
