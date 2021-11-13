using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SinglePlayConfigOperate : MonoBehaviour
{
  private const int TASK_UNIT = 5;
  private const int LONG_MAX_TIME_LIMIT = 60 * 60;
  private const int LONG_MIN_TIME_LIMIT = 1;
  private static int longSentenceTimeLimitVal = 300;
  [SerializeField] private TMP_Dropdown UIGameMode;
  [SerializeField] private TMP_Dropdown UIDataSetName;
  [SerializeField] private TMP_Dropdown UILongDataSetName;
  [SerializeField] private TMP_Dropdown UISentenceNum;
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
    ConfigScript.LoadConfig();
    SetPreviousSettings();
  }

  // Update is called once per frame
  void Update()
  {
    ChangeConfigPanel();
  }

  /// <summary>
  /// 直前の練習内容を選択肢にセット
  /// </summary>
  private void SetPreviousSettings()
  {
    UIGameMode.value = ConfigScript.GameMode;
    // fix: データセットと dropdown の番号の対応付け
    // UIDataSetName.value = ConfigScript.DataSetName;
    // UILongDataSetName.value = ConfigScript.LongSentenceTaskName;
    UIUseYomigana.value = Convert.ToInt32(ConfigScript.UseRuby);
    UISentenceNum.value = ConfigScript.Tasks / TASK_UNIT - 1;
    UIInputType.value = ConfigScript.InputMode;
    longSentenceTimeLimitVal = ConfigScript.LongSentenceTimeLimit;
    InputCPUSpeed.text = ConfigScript.CPUKpm.ToString();
    SetLongSentenceTimeLimitUI();
  }

  /// <summary>
  /// 今回の練習内容を設定に反映させる
  /// </summary>
  private void SetCurrentSettings()
  {
    CheckKpmSettings();
    ConfigScript.GameMode = UIGameMode.value;
    // fix: dataset 名の保存
    // ConfigScript.DataSetName = shortDatasetFileName[UIDataSetName.value];
    // ConfigScript.LongSentenceTaskName = longDatasetFileName[prevDropdownLongDataset];
    ConfigScript.Tasks = (UISentenceNum.value + 1) * TASK_UNIT;
    ConfigScript.LongSentenceTimeLimit = longSentenceTimeLimitVal;
    ConfigScript.CPUKpm = Int32.Parse(InputCPUSpeed.text);
    ConfigScript.UseRuby = UIUseYomigana.value == 1;
    ConfigScript.IsBeginnerMode = false;
    ConfigScript.InfoPanelMode = 0;
    ConfigScript.InputMode = UIInputType.value;
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
    if (int.TryParse(InputCPUSpeed.text, out int kpm))
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
    ConfigScript.SaveConfig();
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
}
