using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SinglePlayConfigOperate : MonoBehaviour
{
  private static int longSentenceTimeLimitVal = 300;
  private static int sentenceNum = 20;
  private static Dictionary<int, ShortWordsetData> valToShortWordset = new Dictionary<int, ShortWordsetData>();
  private static Dictionary<int, LongWordsetData> valToLongWordset = new Dictionary<int, LongWordsetData>();
  [SerializeField] private List<Toggle> PracticeToggleList;
  // [SerializeField] private TMP_Dropdown UIGameMode;
  [SerializeField] private TMP_Dropdown UIDataSetName;
  [SerializeField] private TMP_Dropdown UILongDataSetName;
  [SerializeField] private TMP_InputField UISentenceNum;
  [SerializeField] private TMP_Dropdown UIUseYomigana;
  [SerializeField] private TMP_Dropdown UIInputType;
  [SerializeField] private TMP_Dropdown UICountDownSec;
  [SerializeField] private TMP_Dropdown UIInputStrings;
  [SerializeField] private TMP_InputField UINextWordIntervalTime;
  [SerializeField] private GameObject ConfigPanel;
  [SerializeField] private GameObject LongSentenceConfigPanel;
  [SerializeField] private GameObject DetailSettingsPanel;
  [SerializeField] private GameObject DetailSettingsBgPanel;
  [SerializeField] private TMP_InputField LongSentenceTimeLimitMinute;
  [SerializeField] private TMP_InputField LongSentenceTimeLimitSecond;
  [SerializeField] private TMP_Dropdown UIInputKeyArrayType;

  enum GameModeNumber
  {
    ShortSentence,
    LongSentence
  }

  /// <summary>
  /// 初期化処理
  /// </summary>
  void Awake()
  {
    ConfigScript.LoadConfig();
    // 詳細設定パネルは非表示
    DetailSettingsPanel.SetActive(false);
    DetailSettingsBgPanel.SetActive(false);
    LoadWordsetMetadata();
    SetPreviousSettings();
  }

  /// <summary>
  /// フレームごとの処理
  /// </summary>
  void Update()
  {
    ChangeConfigPanel();
  }

  /// <summary>
  /// ワードセット情報を取得
  /// </summary>
  private void LoadWordsetMetadata()
  {
    // ドロップダウンの初期化
    UIDataSetName.ClearOptions();
    UILongDataSetName.ClearOptions();
    var wordsetListShort = new List<string>();
    var wordsetListLong = new List<string>();
    valToShortWordset = new Dictionary<int, ShortWordsetData>();
    valToLongWordset = new Dictionary<int, LongWordsetData>();
    var idx = 0;
    foreach (var wordData in WordsetData.ShortWordsetDict.Values)
    {
      wordsetListShort.Add(wordData.WordsetScreenName);
      valToShortWordset.Add(idx, wordData);
      idx++;
    }
    idx = 0;
    foreach (var wordData in WordsetData.LongWordsetDict.Values)
    {
      wordsetListLong.Add(wordData.WordsetScreenName);
      valToLongWordset.Add(idx, wordData);
      idx++;
    }
    UIDataSetName.AddOptions(wordsetListShort);
    UILongDataSetName.AddOptions(wordsetListLong);
  }

  /// <summary>
  /// ワードセットと入力方式のチェック
  /// </summary>
  /// <param name="val">ドロップダウンに対応する値</param>
  public void CheckInputType(int val)
  {
    var wordsetLang = valToShortWordset[val].Language;
    // 英語のワードを選んだら、JISかなは選択不可
    if (wordsetLang.Equals("English"))
    {
      UIInputType.interactable = false;
      UIInputType.value = (int)ConfigScript.InputType.roman;
    }
    else
    {
      UIInputType.interactable = true;
    }
  }

  /// <summary>
  /// 入力方式とキー配列の整合性のチェック
  /// </summary>
  /// <param name="val">ドロップダウンに対応する値</param>
  public void CheckArrayType(int val)
  {
    // JIS かなを選択したら、JIS 配列のみ選択可能
    if (val == (int)ConfigScript.InputType.jisKana)
    {
      UIInputKeyArrayType.interactable = false;
      UIInputKeyArrayType.value = (int)ConfigScript.KeyArrayType.japanese;
    }
    else if (val == (int)ConfigScript.InputType.roman)
    {
      UIInputKeyArrayType.interactable = true;
    }
  }

  /// <summary>
  /// 直前の練習内容を選択肢にセット
  /// </summary>
  private void SetPreviousSettings()
  {
    // UIGameMode.value = ConfigScript.GameMode;
    // ConfigScript.GameMode は 0 or 1 で管理している
    for (int idx = 0; idx < PracticeToggleList.Count; ++idx)
    {
      var toggle = PracticeToggleList[idx];
      toggle.isOn = !toggle.isOn;
      toggle.isOn = ConfigScript.GameMode == idx;
      if (toggle.isOn) { toggle.Select(); }
    }
    UIDataSetName.value = valToShortWordset.FirstOrDefault(x => x.Value.WordsetFileName.Equals(ConfigScript.DataSetName)).Key;
    UILongDataSetName.value = valToLongWordset.FirstOrDefault(x => x.Value.DocumentFileName.Equals(ConfigScript.LongSentenceTaskName)).Key;
    UIUseYomigana.value = Convert.ToInt32(ConfigScript.UseRuby);
    UISentenceNum.text = ConfigScript.Tasks.ToString();
    sentenceNum = ConfigScript.Tasks;
    UIInputType.value = ConfigScript.InputMode;
    longSentenceTimeLimitVal = ConfigScript.LongSentenceTimeLimit;
    UICountDownSec.value = ConfigScript.CountDownSecond - 1;
    UINextWordIntervalTime.text = ConfigScript.DelayTime.ToString();
    UIInputStrings.value = ConfigScript.IsShowTypeSentence ? 1 : 0;
    UIInputKeyArrayType.value = ConfigScript.InputArray;
    SetLongSentenceTimeLimitUI();
  }

  /// <summary>
  /// 今回の練習内容を設定に反映させる
  /// </summary>
  private void SetCurrentSettings()
  {
    // ConfigScript.GameMode = UIGameMode.value;
    // ConfigScript.GameMode は 0 or 1 で管理している
    for (int idx = 0; idx < PracticeToggleList.Count; ++idx)
    {
      if (PracticeToggleList[idx].isOn)
      {
        ConfigScript.GameMode = idx;
        break;
      }
    }
    ConfigScript.DataSetName = valToShortWordset[UIDataSetName.value].WordsetFileName;
    ConfigScript.LongSentenceTaskName = valToLongWordset[UILongDataSetName.value].DocumentFileName;
    ConfigScript.Tasks = sentenceNum;
    ConfigScript.LongSentenceTimeLimit = longSentenceTimeLimitVal;
    if (Int32.TryParse(UINextWordIntervalTime.text, out int intervalTime))
    {
      ConfigScript.DelayTime = intervalTime;
    }
    else
    {
      ConfigScript.DelayTime = ConfigScript.DEFAULT_DELAY_TIME;
    }
    ConfigScript.CountDownSecond = UICountDownSec.value + 1;
    ConfigScript.UseRuby = UIUseYomigana.value == 1;
    ConfigScript.IsBeginnerMode = false;
    ConfigScript.InfoPanelMode = 0;
    ConfigScript.InputMode = UIInputType.value;
    ConfigScript.IsShowTypeSentence = UIInputStrings.value == 1;
    ConfigScript.InputArray = UIInputKeyArrayType.value;
  }

  /// <summary>
  /// 選択されているゲームモードにより表示パネルを変更
  /// </summary>
  private void ChangeConfigPanel()
  {
    // ConfigPanel.SetActive(UIGameMode.value == (int)GameModeNumber.ShortSentence);
    // LongSentenceConfigPanel.SetActive(UIGameMode.value == (int)GameModeNumber.LongSentence);
  }

  /// <summary>
  /// Keycode と対応する操作
  /// <param name="kc">keycode</param>
  /// </summary>
  private void KeyCheck(KeyCode kc)
  {
    // 最優先: 詳細設定パネルが開いているかどうか
    if (DetailSettingsPanel.activeSelf)
    {
      switch (kc)
      {
        case KeyCode.D: UIInputStrings.Select(); break;
        case KeyCode.C: UICountDownSec.Select(); break;
        case KeyCode.S: UINextWordIntervalTime.Select(); break;
        case KeyCode.Escape: DetailSettingsClose(); break;
      }
    }
    else if (KeyCode.Space == kc) { BeforeStartPractice(); }
    else if (KeyCode.Escape == kc) { ReturnModeSelectScene(); }
    // toggle 切り替え
    else if (KeyCode.S == kc)
    {
      PracticeToggleList[0].Select();
      PracticeToggleList[0].isOn = true;
    }
    else if (KeyCode.L == kc)
    {
      PracticeToggleList[1].Select();
      PracticeToggleList[1].isOn = true;
    }
    // 以下ショートカットキー
    // S, L, Space, Esc 以外かつ KeyCode で正しく使えるもの
    else
    {
      if (ConfigPanel.activeSelf)
      {
        switch (kc)
        {
          case KeyCode.W: UIDataSetName.Select(); break;
          case KeyCode.I: UIInputType.Select(); break;
          case KeyCode.A: UIInputKeyArrayType.Select(); break;
          case KeyCode.H: OnClickTaskNumMinusButton(); break;
          case KeyCode.J: OnClickTaskNumPlusButton(); break;
          case KeyCode.D: OnClickDetailSettingsButton(); break;
        }
      }
      else if (LongSentenceConfigPanel.activeSelf)
      {
        switch (kc)
        {
          case KeyCode.D: UILongDataSetName.Select(); break;
          case KeyCode.F: OnClickLimitTimeMinusButton(0); break;
          case KeyCode.G: OnClickLimitTimePlusButton(0); break;
          case KeyCode.H: OnClickLimitTimeMinusButton(1); break;
          case KeyCode.J: OnClickLimitTimePlusButton(1); break;
          case KeyCode.R: UIUseYomigana.Select(); break;
        }
      }
    }
  }

  /// <summary>
  /// 練習を開始する前の処理
  /// </summary>
  public void BeforeStartPractice()
  {
    SetCurrentSettings();
    ConfigScript.SaveConfig();
    var selectedMode = ConfigScript.GameMode;
    if (selectedMode == (int)GameModeNumber.ShortSentence) { SceneManager.LoadScene("TypingScene"); }
    else if (selectedMode == (int)GameModeNumber.LongSentence) { SceneManager.LoadScene("LongSentenceTypingScene"); }
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
  /// 文章数のプラスボタンを押したときの挙動
  /// </sumamry>
  public void OnClickTaskNumPlusButton()
  {
    sentenceNum += ConfigScript.TASK_UNIT;
    if (sentenceNum > ConfigScript.MAX_TASK_NUM)
    {
      sentenceNum = ConfigScript.MAX_TASK_NUM;
    }
    UISentenceNum.text = sentenceNum.ToString();
  }

  /// <summary>
  /// 文章数のマイナスボタンを押したときの挙動
  /// </sumamry>
  public void OnClickTaskNumMinusButton()
  {
    sentenceNum -= ConfigScript.TASK_UNIT;
    if (sentenceNum < ConfigScript.MIN_TASK_NUM)
    {
      sentenceNum = ConfigScript.MIN_TASK_NUM;
    }
    UISentenceNum.text = sentenceNum.ToString();
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
  public void OnClickLimitTimePlusButton(int num)
  {
    longSentenceTimeLimitVal += (num == 0) ? 60 : 1;
    if (longSentenceTimeLimitVal > ConfigScript.LONG_MAX_TIME_LIMIT)
    {
      longSentenceTimeLimitVal = ConfigScript.LONG_MAX_TIME_LIMIT;
    }
    SetLongSentenceTimeLimitUI();
  }

  /// <summary>
  /// マイナスボタンを押したときの挙動
  /// <param name="num">分のボタンか秒のボタンか区別する引数</param>
  /// </sumamry>
  public void OnClickLimitTimeMinusButton(int num)
  {
    longSentenceTimeLimitVal -= (num == 0) ? 60 : 1;
    if (longSentenceTimeLimitVal < ConfigScript.LONG_MIN_TIME_LIMIT)
    {
      longSentenceTimeLimitVal = ConfigScript.LONG_MIN_TIME_LIMIT;
    }
    SetLongSentenceTimeLimitUI();
  }

  /// <summary>
  /// 詳細設定を開く（パネルを表示）
  /// </summary>
  public void OnClickDetailSettingsButton()
  {
    DetailSettingsPanel.SetActive(true);
    DetailSettingsBgPanel.SetActive(true);
  }

  /// <summary>
  /// 詳細設定を閉じる
  /// </summary>
  public void DetailSettingsClose()
  {
    DetailSettingsPanel.SetActive(false);
    DetailSettingsBgPanel.SetActive(false);
  }
}
