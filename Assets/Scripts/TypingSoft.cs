using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class TypingSoft : MonoBehaviour
{

  private const int BEGINNER_MODE_COUNTDOWN = 3;
  private const float BEGINNER_NEXT_WORD_DELAY = 0.25f;
  private static double INTERVAL = 2.0F;
  // 問題表示関連
  private static List<string> originSentenceList = new List<string>();
  private static string originSentence;
  private static List<string> typeSentenceList = new List<string>();
  private static string typeSentence;
  // これまで打った文字列
  private static string correctString;
  // 入力受け付け
  private static bool isInputValid;
  // センテンスの長さ
  private static int sentenceLength;
  // ミスタイプ記録
  private static bool isRecMistype;
  private static bool isSentenceMistyped;
  // タイピングの正誤判定器
  private static List<List<List<string>>> sentenceJudgeDataList = new List<List<List<string>>>();
  private static List<List<string>> typingJudge;

  // index 類
  private static int index;
  private static List<List<int>> indexAdd = new List<List<int>>();
  private static List<List<int>> sentenceIndex = new List<List<int>>();
  private static List<List<int>> sentenceValid = new List<List<int>>();
  // 時間計測関連
  private static bool isFirstInput;
  private static bool isIntervalEnded;
  private static double lastSentenceUpdateTime;
  private static double firstCharInputTime;
  private static double lastJudgeTime;
  // タイピング情報表示関連
  private static int currentTaskNumber;
  private static int correctTypeNum;
  private static int misTypeNum;
  private static double keyPerMin;
  private static double accuracyValue;
  private static double totalTypingTime;
  private static int numOfTask;
  private static string cpuTypeString;
  // リザルト集計用
  private static StringBuilder typedLetter = new StringBuilder();
  private static List<int> typeJudgeList = new List<int>();
  private static List<double> typeTimeList = new List<double>();
  // 色
  private static Color colorBeforeMeasure = new Color(16f / 255f, 7f / 255f, 45f / 255f, 1f);
  private static Color colorMeasuring = new Color(102f / 255f, 50f / 255f, 80f / 255f, 1f);
  // UI たち
  [SerializeField] private Text UIOriginSentence;
  [SerializeField] private Text UIYomigana;
  [SerializeField] private Text UIType;
  [SerializeField] private Text UIKPM;
  [SerializeField] private Text UISTT;
  [SerializeField] private Text UITask;
  [SerializeField] private Text UIAccuracy;
  [SerializeField] private Text UITypeInfo;
  [SerializeField] private Text UINextWord;
  [SerializeField] private Text UINextYomi;
  [SerializeField] private Text countdownText;
  [SerializeField] private GameObject DataPanel;
  [SerializeField] private GameObject AssistKeyboardPanel;
  [SerializeField] private GameObject NowLoadingPanel;
  [SerializeField] private GameObject NextWordPanel;
  private static GenerateSentence gs;
  // Assist Keyboard JIS
  private static AssistKeyboardJIS AssistKeyboardObj;

  // JIS かなキーコードからひらがなへのマッピング
  private static readonly Dictionary<string, string> JISKanaMapping = new Dictionary<string, string>(){
    {"KeyA", "ち"}, {"KeyB", "こ"}, {"KeyC", "そ"}, {"KeyD", "し"}, {"KeyE", "い"},
    {"KeyF", "は"}, {"KeyG", "き"}, {"KeyH", "く"}, {"KeyI", "に"}, {"KeyJ", "ま"},
    {"KeyK", "の"}, {"KeyL", "り"}, {"KeyM", "も"}, {"KeyN", "み"}, {"KeyO", "ら"},
    {"KeyP", "せ"}, {"KeyQ", "た"}, {"KeyR", "す"}, {"KeyS", "と"}, {"KeyT", "か"},
    {"KeyU", "な"}, {"KeyV", "ひ"}, {"KeyW", "て"}, {"KeyX", "さ"}, {"KeyY", "ん"},
    {"KeyZ", "つ"}, {"Digit1", "ぬ"}, {"Digit2", "ふ"}, {"Digit3", "あ"}, {"Digit4", "う"},
    {"Digit5", "え"}, {"Digit6", "お"}, {"Digit7", "や"}, {"Digit8", "ゆ"}, {"Digit9", "よ"},
    {"Digit0", "わ"}, {"Minus", "ほ"}, {"Equal", "へ"}, {"IntlYen", "ー"}, {"BracketLeft", "゛"},
    {"BracketRight", "゜"}, {"Semicolon", "れ"}, {"Quote", "け"}, {"Backslash", "む"}, {"Comma", "ね"},
    {"Period", "る"}, {"Slash", "め"}, {"IntlRo", "ろ"}, {"Space", "　"},
    {"KeyA_SH", "ち"}, {"KeyB_SH", "こ"}, {"KeyC_SH", "そ"}, {"KeyD_SH", "し"}, {"KeyE_SH", "ぃ"},
    {"KeyF_SH", "は"}, {"KeyG_SH", "き"}, {"KeyH_SH", "く"}, {"KeyI_SH", "に"}, {"KeyJ_SH", "ま"},
    {"KeyK_SH", "の"}, {"KeyL_SH", "り"}, {"KeyM_SH", "も"}, {"KeyN_SH", "み"}, {"KeyO_SH", "ら"},
    {"KeyP_SH", "せ"}, {"KeyQ_SH", "た"}, {"KeyR_SH", "す"}, {"KeyS_SH", "と"}, {"KeyT_SH", "か"},
    {"KeyU_SH", "な"}, {"KeyV_SH", "ひ"}, {"KeyW_SH", "て"}, {"KeyX_SH", "さ"}, {"KeyY_SH", "ん"},
    {"KeyZ_SH", "っ"}, {"Digit1_SH", "ぬ"}, {"Digit2_SH", "ふ"}, {"Digit3_SH", "ぁ"}, {"Digit4_SH", "ぅ"},
    {"Digit5_SH", "ぇ"}, {"Digit6_SH", "ぉ"}, {"Digit7_SH", "ゃ"}, {"Digit8_SH", "ゅ"}, {"Digit9_SH", "ょ"},
    {"Digit0_SH", "を"}, {"Minus_SH", "ほ"}, {"Equal_SH", "へ"}, {"IntlYen_SH", "ー"}, {"BracketLeft_SH", "゛"},
    {"BracketRight_SH", "「"}, {"Semicolon_SH", "れ"}, {"Quote_SH", "け"}, {"Backslash_SH", "」"}, {"Comma_SH", "、"},
    {"Period_SH", "。"}, {"Slash_SH", "・"}, {"IntlRo_SH", "ろ"}, {"Space_SH", "　"}
  };

  // ローマ字のキー入力と対応文字
  // JIS かな、アルファベット、数字、記号対応
  private static readonly Dictionary<string, string> RomanJISArrayMapping = new Dictionary<string, string>(){
    {"KeyA", "a"}, {"KeyB", "b"}, {"KeyC", "c"}, {"KeyD", "d"}, {"KeyE", "e"},
    {"KeyF", "f"}, {"KeyG", "g"}, {"KeyH", "h"}, {"KeyI", "i"}, {"KeyJ", "j"},
    {"KeyK", "k"}, {"KeyL", "l"}, {"KeyM", "m"}, {"KeyN", "n"}, {"KeyO", "o"},
    {"KeyP", "p"}, {"KeyQ", "q"}, {"KeyR", "r"}, {"KeyS", "s"}, {"KeyT", "t"},
    {"KeyU", "u"}, {"KeyV", "v"}, {"KeyW", "w"}, {"KeyX", "x"}, {"KeyY", "y"},
    {"KeyZ", "z"}, {"Digit1", "1"}, {"Digit2", "2"}, {"Digit3", "3"}, {"Digit4", "4"},
    {"Digit5", "5"}, {"Digit6", "6"}, {"Digit7", "7"}, {"Digit8", "8"}, {"Digit9", "9"},
    {"Digit0", "0"}, {"Minus", "-"}, {"Equal", "^"}, {"IntlYen", "\\"}, {"BracketLeft", "@"},
    {"BracketRight", "["}, {"Semicolon", ";"}, {"Quote", ":"}, {"Backslash", "]"}, {"Comma", ","},
    {"Period", "."}, {"Slash", "/"}, {"IntlRo", "\\"}, {"Space", " "},
    {"KeyA_SH", "A"}, {"KeyB_SH", "B"}, {"KeyC_SH", "C"}, {"KeyD_SH", "D"}, {"KeyE_SH", "E"},
    {"KeyF_SH", "F"}, {"KeyG_SH", "G"}, {"KeyH_SH", "H"}, {"KeyI_SH", "I"}, {"KeyJ_SH", "J"},
    {"KeyK_SH", "K"}, {"KeyL_SH", "L"}, {"KeyM_SH", "M"}, {"KeyN_SH", "N"}, {"KeyO_SH", "O"},
    {"KeyP_SH", "P"}, {"KeyQ_SH", "Q"}, {"KeyR_SH", "R"}, {"KeyS_SH", "S"}, {"KeyT_SH", "T"},
    {"KeyU_SH", "U"}, {"KeyV_SH", "V"}, {"KeyW_SH", "W"}, {"KeyX_SH", "X"}, {"KeyY_SH", "Y"},
    {"KeyZ_SH", "Z"}, {"Digit1_SH", "!"}, {"Digit2_SH", "\""}, {"Digit3_SH", "#"}, {"Digit4_SH", "$"},
    {"Digit5_SH", "%"}, {"Digit6_SH", "&"}, {"Digit7_SH", "'"}, {"Digit8_SH", "("}, {"Digit9_SH", ")"},
    {"Digit0_SH", ""}, {"Minus_SH", "="}, {"Equal_SH", "~"}, {"IntlYen_SH", "|"}, {"BracketLeft_SH", "`"},
    {"BracketRight_SH", "{"}, {"Semicolon_SH", "+"}, {"Quote_SH", "*"}, {"Backslash_SH", "}"}, {"Comma_SH", "<"},
    {"Period_SH", ">"}, {"Slash_SH", "?"}, {"IntlRo_SH", "_"}, {"Space_SH", " "}
  };

  // US配列版
  private static readonly Dictionary<string, string> RomanUSArrayMapping = new Dictionary<string, string>(){
    {"KeyA", "a"}, {"KeyB", "b"}, {"KeyC", "c"}, {"KeyD", "d"}, {"KeyE", "e"},
    {"KeyF", "f"}, {"KeyG", "g"}, {"KeyH", "h"}, {"KeyI", "i"}, {"KeyJ", "j"},
    {"KeyK", "k"}, {"KeyL", "l"}, {"KeyM", "m"}, {"KeyN", "n"}, {"KeyO", "o"},
    {"KeyP", "p"}, {"KeyQ", "q"}, {"KeyR", "r"}, {"KeyS", "s"}, {"KeyT", "t"},
    {"KeyU", "u"}, {"KeyV", "v"}, {"KeyW", "w"}, {"KeyX", "x"}, {"KeyY", "y"},
    {"KeyZ", "z"}, {"Digit1", "1"}, {"Digit2", "2"}, {"Digit3", "3"}, {"Digit4", "4"},
    {"Digit5", "5"}, {"Digit6", "6"}, {"Digit7", "7"}, {"Digit8", "8"}, {"Digit9", "9"},
    {"Digit0", "0"}, {"Minus", "-"}, {"Equal", "="}, {"BracketLeft", "["},
    {"BracketRight", "]"}, {"Semicolon", ";"}, {"Quote", "'"}, {"Backslash", "\\"}, {"Comma", ","},
    {"Period", "."}, {"Slash", "/"}, {"IntlRo", "\\"}, {"Space", " "},
    {"KeyA_SH", "A"}, {"KeyB_SH", "B"}, {"KeyC_SH", "C"}, {"KeyD_SH", "D"}, {"KeyE_SH", "E"},
    {"KeyF_SH", "F"}, {"KeyG_SH", "G"}, {"KeyH_SH", "H"}, {"KeyI_SH", "I"}, {"KeyJ_SH", "J"},
    {"KeyK_SH", "K"}, {"KeyL_SH", "L"}, {"KeyM_SH", "M"}, {"KeyN_SH", "N"}, {"KeyO_SH", "O"},
    {"KeyP_SH", "P"}, {"KeyQ_SH", "Q"}, {"KeyR_SH", "R"}, {"KeyS_SH", "S"}, {"KeyT_SH", "T"},
    {"KeyU_SH", "U"}, {"KeyV_SH", "V"}, {"KeyW_SH", "W"}, {"KeyX_SH", "X"}, {"KeyY_SH", "Y"},
    {"KeyZ_SH", "Z"}, {"Digit1_SH", "!"}, {"Digit2_SH", "@"}, {"Digit3_SH", "#"}, {"Digit4_SH", "$"},
    {"Digit5_SH", "%"}, {"Digit6_SH", "^"}, {"Digit7_SH", "&"}, {"Digit8_SH", "*"}, {"Digit9_SH", "("},
    {"Digit0_SH", ")"}, {"Minus_SH", "_"}, {"Equal_SH", "+"}, {"BracketLeft_SH", "{"},
    {"BracketRight_SH", "}"}, {"Semicolon_SH", ":"}, {"Quote_SH", "\""}, {"Backslash_SH", "|"}, {"Comma_SH", "<"},
    {"Period_SH", ">"}, {"Slash_SH", "?"}, {"Space_SH", " "}
  };

  // エラーコードとエラータイプ
  private enum ErrorType
  {
    None,
    QueueLengthNotMatch,
    FailedLoadSentence
  };

  // ゲームの状況
  public enum GameCondition
  {
    Progress,
    Finished,
    Canceled,
    Retry,
    Countdown
  };

  public static int CurrentGameCondition
  {
    private set;
    get;
  } = 0;

  public static int ErrorCode
  {
    private set;
    get;
  } = 0;

  public static TypingPerformance Performance
  {
    private set;
    get;
  }

  public static string CurrentTypingSentence
  {
    private set;
    get;
  } = "";

  /// <summary>
  /// Update() 前に読み込み
  /// </summary>
  void Awake()
  {
    // Now Loading を表示
    NowLoadingPanel.SetActive(true);
    // コンポーネント読み込み
    AssistKeyboardObj = GameObject.Find("AssistKeyboard").GetComponent<AssistKeyboardJIS>();
    // init より先に初期化すべき項目
    // ロード成功したかのフラグを false に
    var isLoadSuccess = false;
    // 入力受付状態は一度 true に
    // リトライ機能の関係
    isInputValid = true;
    // ゲームコンディションを in progress にする
    CurrentGameCondition = (int)GameCondition.Progress;
    // ワードデータセットの読み込み
    isLoadSuccess = GenerateSentence.LoadSentenceData(ConfigScript.DataSetName);
    if (isLoadSuccess)
    {
      GameMain();
    }
    // 読み込み失敗時はエラーとしてフラグを立てる
    else
    {
      ErrorCode = (int)ErrorType.FailedLoadSentence;
      CurrentGameCondition = (int)GameCondition.Canceled;
    }
  }

  /// <summary>
  /// 初期化
  /// </summary>
  public void GameMain()
  {
    // 入力受付してないときは、リトライ多重発生防止のため return する
    if (!isInputValid) { return; }
    InitData();
    InitText();
    GenerateTask();
    NowLoadingPanel.SetActive(false);
    if (UINextWord != null && UINextYomi != null)
    {
      var firstWordInfo = GetNextWord(currentTaskNumber);
      UINextWord.text = firstWordInfo.sentence;
      UINextYomi.text = firstWordInfo.yomi;
    }
    StartCoroutine(CountDown());
  }

  /// <summary>
  /// 内部データの初期化
  /// </summary>
  private void InitData()
  {
    // ゲーム状態管理の初期化
    CurrentGameCondition = (int)GameCondition.Progress;
    ErrorCode = (int)ErrorType.None;
    // データ初期化
    Performance = new TypingPerformance();
    numOfTask = ConfigScript.Tasks;
    correctTypeNum = 0;
    misTypeNum = 0;
    totalTypingTime = 0.0;
    keyPerMin = 0.0;
    accuracyValue = 0.0;
    currentTaskNumber = 0;
    lastJudgeTime = -1.0;
    isRecMistype = false;
    isInputValid = false;
    isIntervalEnded = false;
    isSentenceMistyped = false;
    CurrentTypingSentence = "";
    cpuTypeString = "";
    UIOriginSentence.text = "";
    UIYomigana.text = "";
    UIType.text = "";
    InitKeyCodeQueue();
    if (ConfigScript.IsBeginnerMode)
    {
      INTERVAL = 0f;
    }
  }

  /// <summary>
  /// UI テキストの初期化
  /// </summary>
  private void InitText()
  {
    UpdateUITypeInfo();
    UpdateUICorrectTypeRate();
    UpdateUIKeyPerMinute(0, 0);
    UpdateUIElapsedTime(0.0);
    UpdateUITask();
  }

  /// <summary>
  /// カウントダウン演出
  /// </summary>
  private IEnumerator CountDown()
  {
    CurrentGameCondition = (int)GameCondition.Countdown;
    var count = ConfigScript.IsBeginnerMode ? BEGINNER_MODE_COUNTDOWN : ConfigScript.CountDownSecond;
    while (count > 0)
    {
      countdownText.text = count.ToString();
      yield return new WaitForSeconds(1f);
      count--;
    }
    countdownText.text = "";
    ChangeSentence();
  }

  /// <summary>
  /// 1f ごとの処理
  /// </summary>
  void Update()
  {
    // リトライの状態になっていればリトライを最優先
    if (CurrentGameCondition == (int)GameCondition.Retry)
    {
      GameMain();
    }
    else
    {
      // テキストカラーの設定
      TextColorChange();
      // パネル表示
      if (DataPanel != null && AssistKeyboardPanel != null)
      {
        ShowMiddlePanel(ConfigScript.InfoPanelMode);
      }
      // 正誤判定
      if (isInputValid)
      {
        GetKeyInput();
      }
      // アシストキーボード表示
      if (AssistKeyboardPanel != null)
      {
        if (CurrentTypingSentence == "" || !isInputValid)
        {
          AssistKeyboardObj.SetAllKeyColorWhite();
          AssistKeyboardObj.SetAllFingerColorWhite();
        }
        else if (isInputValid)
        {
          AssistKeyboardObj.SetNextHighlight(CurrentTypingSentence[0].ToString());
        }
      }
    }
  }

  /// <summary>
  /// JS のキーコードを取得して Unity 用に変換
  /// </summary>
  private void GetKeyInput()
  {
    var jsKey = GetKeyCodeFromJS();
    if (!String.IsNullOrEmpty(jsKey) && !jsKey.Equals("None"))
    {
      // 最初のキーのときはレイテンシも測定
      if (isFirstInput)
      {
        double currentTime = Time.realtimeSinceStartup;
        if (!ConfigScript.IsBeginnerMode)
        {
          firstCharInputTime = currentTime;
        }
        // 1文字目の時は反応時間もここで計測
        var latency = currentTime - lastSentenceUpdateTime;
        Performance.AddLatencyTime(latency);
        isFirstInput = false;
      }
      // 正誤判定
      var keyStr = "";
      // ローマ字
      if (ConfigScript.InputMode == (int)ConfigScript.InputType.roman)
      {
        if (ConfigScript.InputArray == (int)ConfigScript.KeyArrayType.japanese && RomanJISArrayMapping.ContainsKey(jsKey))
        {
          keyStr = RomanJISArrayMapping[jsKey];
        }
        else if (ConfigScript.InputArray == (int)ConfigScript.KeyArrayType.us && RomanUSArrayMapping.ContainsKey(jsKey))
        {
          keyStr = RomanUSArrayMapping[jsKey];
        }
      }
      // JIS かな
      else if (ConfigScript.InputMode == (int)ConfigScript.InputType.jisKana && JISKanaMapping.ContainsKey(jsKey))
      {
        keyStr = JISKanaMapping[jsKey];
      }
      // Function key などでなければ判定をする
      if (!String.IsNullOrEmpty(keyStr))
      {
        double currentTime = Time.realtimeSinceStartup;
        // 正誤チェック
        StartCoroutine(TypingCheck(keyStr, currentTime));
      }
    }
  }

  /// <summary>
  /// 課題文の文字色を変更
  /// 最初の1文字目を打つか時間経過で変わる
  /// </summary>
  private void TextColorChange()
  {
    double currentTime = Time.realtimeSinceStartup;
    if (isFirstInput && currentTime - lastSentenceUpdateTime <= INTERVAL)
    {
      UIOriginSentence.color = colorBeforeMeasure;
    }
    else
    {
      UIOriginSentence.color = colorMeasuring;
      isIntervalEnded = true;
    }
  }

  /// <summary>
  /// 次のセンテンスへ移行前に休止を挟む
  /// </summary>
  private IEnumerator DelayGenerateNewSentence()
  {
    var delayVal = 1f;
    if (ConfigScript.IsBeginnerMode)
    {
      delayVal = (float)BEGINNER_NEXT_WORD_DELAY;
    }
    else
    {
      delayVal = (float)(ConfigScript.DelayTime / 1000.0);
    }
    yield return new WaitForSeconds(delayVal);
    ChangeSentence();
  }

  /// <summary>
  /// 課題文章の生成
  /// </summary>
  private void GenerateTask()
  {
    var generatedNum = 0;
    // 初期化
    originSentenceList.Clear();
    typeSentenceList.Clear();
    sentenceJudgeDataList.Clear();
    while (generatedNum < numOfTask)
    {
      // 例文生成
      var t = GenerateSentence.Generate();
      if (!t.isGenerateSuccess)
      {
        continue;
      }
      // 生成したデータをリストに追加
      originSentenceList.Add(t.originSentence);
      typeSentenceList.Add(t.typeSentence);
      sentenceJudgeDataList.Add(t.typeJudge);
      generatedNum++;
    }
  }

  /// <summary>
  /// 課題文章の変更を行う
  /// </summary>
  private void ChangeSentence()
  {
    CurrentGameCondition = (int)GameCondition.Progress;
    // 文章などをセット
    originSentence = originSentenceList[currentTaskNumber];
    typeSentence = typeSentenceList[currentTaskNumber];
    typingJudge = sentenceJudgeDataList[currentTaskNumber];
    // 判定器などの初期化
    InitSentenceData();
    // ローマ字で次打つべき文字表示を判定からセット
    var nextTypingSentence = "";
    for (int i = 0; i < typingJudge.Count; ++i)
    {
      nextTypingSentence += typingJudge[i][0];
    }
    // 正解した文字列を初期化
    correctString = "";
    // リザルト集積用の変数を初期化
    typedLetter = new StringBuilder();
    typeJudgeList = new List<int>();
    typeTimeList = new List<double>();
    InitKeyCodeQueue();
    // 変数等の初期化
    isFirstInput = true;
    isIntervalEnded = false;
    isRecMistype = false;
    isSentenceMistyped = false;
    index = 0;
    sentenceLength = 0;
    CurrentTypingSentence = nextTypingSentence;
    cpuTypeString = nextTypingSentence;
    // UI 上のテキスト変更
    UIOriginSentence.text = originSentence;
    UIYomigana.text = typeSentence;
    if (ConfigScript.IsBeginnerMode || ConfigScript.IsShowTypeSentence)
    {
      SetUITypeText(nextTypingSentence);
    }
    else
    {
      SetUITypeText("");
    }

    UpdateUITask();
    // 入力受け付け状態にする
    isInputValid = true;
    // 時刻を取得
    lastSentenceUpdateTime = Time.realtimeSinceStartup;
    if (ConfigScript.IsBeginnerMode)
    {
      firstCharInputTime = lastSentenceUpdateTime;
    }
    if (UINextWord != null && UINextYomi != null)
    {
      var nextWordInfo = GetNextWord(currentTaskNumber + 1);
      UINextWord.text = nextWordInfo.sentence;
      UINextYomi.text = nextWordInfo.yomi;
    }
  }

  /// <summary>
  /// 次のワードを取得
  /// </summary>
  /// <param name="wordnumber">ワード番号(0-index)</param>
  /// <returns>(原文, タイプ文[読み方])</returns>
  private (string sentence, string yomi) GetNextWord(int wordnumber)
  {
    var nextWordBuilder = new StringBuilder();
    var nextYomiBuilder = new StringBuilder();
    // ワード数チェック
    if (!(wordnumber < numOfTask))
    {
      nextWordBuilder.Append("***** END *****");
      nextYomiBuilder.Append("");
    }
    else
    {
      nextWordBuilder.Append(originSentenceList[wordnumber]);
      nextYomiBuilder.Append(typeSentenceList[wordnumber]);
    }
    return (nextWordBuilder.ToString(), nextYomiBuilder.ToString());
  }

  /// <summary>
  /// タイピング文の半角スペースをアンダーバーに置換して表示
  /// 打ったか打ってないかわかりにくいため、アンダーバーを表示することで改善
  /// </summary>
  private void SetUITypeText(string sentence)
  {
    UIType.text = sentence.Replace(' ', '_');
  }

  /// <summary>
  /// タイピング正誤判定まわりの初期化
  /// </summary>
  private void InitSentenceData()
  {
    var sLength = typingJudge.Count;
    sentenceIndex.Clear();
    sentenceValid.Clear();
    indexAdd.Clear();
    sentenceIndex = new List<List<int>>();
    sentenceValid = new List<List<int>>();
    indexAdd = new List<List<int>>();
    for (int i = 0; i < sLength; ++i)
    {
      var typeNum = typingJudge[i].Count;
      sentenceIndex.Add(new List<int>());
      sentenceValid.Add(new List<int>());
      indexAdd.Add(new List<int>());
      for (int j = 0; j < typeNum; ++j)
      {
        sentenceIndex[i].Add(0);
        sentenceValid[i].Add(1);
        indexAdd[i].Add(0);
      }
    }
  }

  /// <summary>
  /// タイピングの正誤判定部分
  /// </summary>
  private IEnumerator TypingCheck(string nextString, double keyDownTime)
  {
    lastJudgeTime = keyDownTime;
    typedLetter.Append(nextString);
    typeTimeList.Add(keyDownTime);

    // まだ可能性のあるセンテンス全てに対してミスタイプかチェックする
    bool isMistype = JudgeTyping(nextString);
    if (!isMistype)
    {
      Correct(nextString);
    }
    else
    {
      Mistype();
    }
    yield return null;
  }

  /// <summary>
  /// 入力された文字と次打つべき文字の判定部分
  /// </summary>
  private bool JudgeTyping(string currentStr)
  {
    bool isMistype = true;
    // 全ての valid なセンテンスに対してチェックする
    for (int i = 0; i < typingJudge[index].Count; ++i)
    {
      // invalid ならパス
      if (0 == sentenceValid[index][i])
      {
        continue;
      }
      int j = sentenceIndex[index][i];
      string judgeString = typingJudge[index][i][j].ToString();
      if (currentStr.Equals(judgeString))
      {
        isMistype = false;
        indexAdd[index][i] = 1;
      }
      else
      {
        indexAdd[index][i] = 0;
      }
    }
    return isMistype;
  }

  /// <summary>
  /// タイピング正解時の処理
  /// <param name="typeChar">打った文字</param>
  /// </summary>
  private void Correct(string typeChar)
  {
    // 正解数を増やす
    correctTypeNum++;
    sentenceLength++;
    UpdateUITypeInfo();
    // 正解率の計算
    accuracyValue = GetCorrectTypeRate();
    UpdateUICorrectTypeRate();
    isRecMistype = false;
    // 可能な入力パターンのチェック
    bool isIndexCountUp = IsJudgeIndexCountUp(typeChar);
    // ローマ字入力表示を更新
    UpdateSentence(typeChar);
    if (isIndexCountUp)
    {
      index++;
    }
    // リザルト集積用
    typeJudgeList.Add(1);
    // 文章入力完了処理
    if (index >= typingJudge.Count)
    {
      CompleteTask();
    }
  }

  /// <summary>
  /// 有効パターンをチェックし、インデックスを増やすかどうか判定する
  /// <param names="typeChar">打った文字</param>
  /// <returns>インデックス増やすなら true、さもなくば false</returns>
  /// </summary>
  private bool IsJudgeIndexCountUp(string typeChar)
  {
    bool ret = false;
    // 可能な入力パターンを残す
    for (int i = 0; i < typingJudge[index].Count; ++i)
    {
      // typeChar と一致しないものを無効化処理
      if (!typeChar.Equals(typingJudge[index][i][sentenceIndex[index][i]].ToString()))
      {
        sentenceValid[index][i] = 0;
      }
      // 次のキーへ
      sentenceIndex[index][i] += indexAdd[index][i];
      // 次の文字へ
      if (sentenceIndex[index][i] >= typingJudge[index][i].Length)
      {
        ret = true;
      }
    }
    return ret;
  }

  /// <summary>
  /// 全ての正解タイプに対する KPM の取得
  /// <returns>これまで打った正解タイプにおける KPM</returns>
  /// </summary>
  private double GetKeyPerMinute()
  {
    return 60.0 * correctTypeNum / (1.0 * totalTypingTime);
  }

  /// <summary>
  /// 現在の文章に対する KPM の取得
  /// <param name="sentenceTypeTime">現在の文章を打つのにかかった時間</param>
  /// <returns>現在の文章の KPM</returns>
  /// </summary>
  private double GetSentenceKPM(double sentenceTypeTime)
  {
    return 60.0 * sentenceLength / (1.0 * sentenceTypeTime);
  }

  /// <summary>
  /// 1文打ち終わった後の処理
  /// </summary>
  private void CompleteTask()
  {
    currentTaskNumber++;
    // リザルト集積用に追加
    Performance.AddOriginSentence(originSentence);
    Performance.AddTypedSentenceList(typedLetter.ToString());
    Performance.AddTypeJudgeList(typeJudgeList);
    Performance.AddTypeTimeList(typeTimeList);
    // タイプした文字を緑色に
    UIType.text = $"<color=#20A01D>{UIType.text}</color>";
    // 現在時刻の取得
    if (UISTT != null)
    {
      double sentenceTypeTime = GetSentenceTypeTime(lastJudgeTime);
      totalTypingTime += sentenceTypeTime;
      UpdateUIElapsedTime(sentenceTypeTime);
      if (UIKPM != null)
      {
        keyPerMin = GetKeyPerMinute();
        double sectionKPM = GetSentenceKPM(sentenceTypeTime);
        int intKPM = Convert.ToInt32(Math.Floor(keyPerMin));
        int intsectionKPM = Convert.ToInt32(Math.Floor(sectionKPM));
        UpdateUIKeyPerMinute(intKPM, intsectionKPM);
      }
    }
    isInputValid = false;
    // 終了
    // numOfTask <= 0 の時は練習モード用で無限にできるようにするため
    if (currentTaskNumber >= numOfTask && numOfTask > 0)
    {
      CurrentGameCondition = (int)GameCondition.Finished;
    }
    else
    {
      StartCoroutine(DelayGenerateNewSentence());
    }
  }

  /// <summary>
  /// 画面上に表示する打つ文字の表示を更新する
  /// <param name="typeChar">打った文字</param>
  /// </summary>
  private void UpdateSentence(string typeChar)
  {
    // 打った文字を消去するオプションの場合
    var nextTypingSentence = "";
    for (int i = 0; i < typingJudge.Count; ++i)
    {
      if (i < index)
      {
        continue;
      }
      for (int j = 0; j < typingJudge[i].Count; ++j)
      {
        if (index == i && sentenceValid[index][j] == 0)
        {
          continue;
        }
        else if (index == i && sentenceValid[index][j] == 1)
        {
          for (int k = 0; k < typingJudge[index][j].Length; ++k)
          {
            if (k >= sentenceIndex[index][j])
            {
              nextTypingSentence += typingJudge[index][j][k].ToString();
            }
          }
          break;
        }
        else if (index != i && sentenceValid[i][j] == 1)
        {
          nextTypingSentence += typingJudge[i][j];
          break;
        }
      }
    }
    correctString += typeChar;
    // Space は打ったか打ってないかわかりにくいので表示上はアンダーバーに変更
    var UIStr = "";
    if (ConfigScript.IsBeginnerMode || ConfigScript.IsShowTypeSentence)
    {
      UIStr = nextTypingSentence;
    }
    else
    {
      UIStr = correctString + (isSentenceMistyped ? ("<color=#ff0000ff>" + nextTypingSentence + "</color>") : "");
    }
    SetUITypeText(UIStr);
    CurrentTypingSentence = nextTypingSentence;
  }

  /// <summary>
  /// ミスタイプ時の処理
  /// </summary>
  private void Mistype()
  {
    isSentenceMistyped = true;
    // ミスタイプ数を増やす
    misTypeNum++;
    UpdateUITypeInfo();
    // 正解率の計算
    accuracyValue = GetCorrectTypeRate();
    UpdateUICorrectTypeRate();
    // 打つべき文字を赤く表示
    if (!isRecMistype)
    {
      string UIStr = "";
      if (ConfigScript.IsBeginnerMode || ConfigScript.IsShowTypeSentence)
      {
        UIStr = "<color=#ff0000ff>" + CurrentTypingSentence.ToString() + "</color>";
      }
      else
      {
        UIStr = correctString + "<color=#ff0000ff>" + CurrentTypingSentence.ToString() + "</color>";
      }
      SetUITypeText(UIStr);
    }
    // color タグを多重で入れないようにする
    isRecMistype = true;
    // リザルト集積用
    typeJudgeList.Add(0);
  }

  /// <summary>
  /// これまでに打った文字の正解率を計算
  /// <returns>これまでの正解率</returns>
  /// </summary>
  private double GetCorrectTypeRate()
  {
    return 100f * correctTypeNum / (correctTypeNum + misTypeNum);
  }

  /// <summary>
  /// 1文打つのにかかった時間を取得
  /// <param name="currentTime">現在時刻</param>
  /// <returns>1文打つのにかかった時間</returns>
  /// </summary>
  private double GetSentenceTypeTime(double currentTime)
  {
    return (firstCharInputTime - lastSentenceUpdateTime <= INTERVAL) ? (currentTime - firstCharInputTime)
            : (currentTime - (lastSentenceUpdateTime + INTERVAL));
  }

  /// <summary>
  /// 正解率の UI 表示を更新
  /// </summary>
  private void UpdateUICorrectTypeRate()
  {
    if (UIAccuracy != null)
    {
      UIAccuracy.text = $"精度 : {accuracyValue.ToString("0.00")} %";
    }
  }

  /// <summary>
  /// 正解数、不正解数の UI 表示を更新
  /// </summary>
  private void UpdateUITypeInfo()
  {
    if (UITypeInfo != null)
    {
      UITypeInfo.text = $"正解タイプ数 : {correctTypeNum.ToString()}\nミスタイプ数 : {misTypeNum.ToString()}";
    }
  }

  /// <summary>
  /// KPM 関連の UI 表示を更新
  /// <param name="intKPMAll">全文における KPM</param>
  /// <param name="intSentenceKPM">現在の文章の KPM</param>
  /// </summary>
  private void UpdateUIKeyPerMinute(int intKPMAll, int intSentenceKPM)
  {
    if (UIKPM != null)
    {
      UIKPM.text = $"タイプ速度(ワード) : {intSentenceKPM.ToString()} KPM\n" + $"平均タイプ速度 : {intKPMAll.ToString()} KPM";
    }
  }

  /// <summary>
  /// 経過時間関連の UI 表示を更新
  /// <param name="sentenceTypeTime">現在の文章を打つのにかかった時間</param>
  /// </summary>
  private void UpdateUIElapsedTime(double sentenceTypeTime)
  {
    if (UISTT != null)
    {
      UISTT.text = $"ワードタイプ時間 : {sentenceTypeTime.ToString("0.00")} 秒\n"
      + $"合計タイプ時間 : {totalTypingTime.ToString("0.00")} 秒";
    }
  }

  /// <summary>
  /// 文章数関連の UI 表示を更新
  /// </summary>
  private void UpdateUITask()
  {
    if (UITask != null)
    {
      UITask.text = $"ワード数 : {(currentTaskNumber + 1).ToString()} / {numOfTask.ToString()}";
    }
  }

  /// <summary>
  /// 文章数関連の UI 表示を更新
  /// <param name="activePanelVal">表示をアクティブにするパネルの番号</param>
  /// </summary>
  private void ShowMiddlePanel(int activePanelVal)
  {
    if (activePanelVal == 0)
    {
      DataPanel.SetActive(true);
      AssistKeyboardPanel.SetActive(false);
    }
    else if (activePanelVal == 1)
    {
      DataPanel.SetActive(false);
      AssistKeyboardPanel.SetActive(true);
    }
    else if (activePanelVal == 2)
    {
      DataPanel.SetActive(true);
      AssistKeyboardPanel.SetActive(true);
    }
    else
    {
      DataPanel.SetActive(false);
      AssistKeyboardPanel.SetActive(false);
    }
  }

  /// <summary>
  /// 練習を中断するフラグを立てる
  /// </summary>
  public static void CancelPractice()
  {
    CurrentGameCondition = (int)GameCondition.Canceled;
  }

  /// <summary>
  /// 練習リトライのフラグを立てる
  /// </summary>
  public static void RetryPractice()
  {
    CurrentGameCondition = (int)GameCondition.Retry;
  }

  /// <summary>
  /// キーコードから string
  /// <param name="key">keycode</param>
  /// <param name="isShiftkeyPushed">シフトキーが押されたかどうか</param>
  /// </summary>
  private string ConvertKeyCodeToStr(KeyCode key, bool isShiftkeyPushed)
  {
    switch (key)
    {
      // かな入力用に便宜的にタブ文字を Shift+0 に割り当てている
      case KeyCode.Alpha0:
        return isShiftkeyPushed ? "\t" : "0";
      case KeyCode.Alpha1:
        return isShiftkeyPushed ? "!" : "1";
      case KeyCode.Alpha2:
        return isShiftkeyPushed ? "\"" : "2";
      case KeyCode.Alpha3:
        return isShiftkeyPushed ? "#" : "3";
      case KeyCode.Alpha4:
        return isShiftkeyPushed ? "$" : "4";
      case KeyCode.Alpha5:
        return isShiftkeyPushed ? "%" : "5";
      case KeyCode.Alpha6:
        return isShiftkeyPushed ? "&" : "6";
      case KeyCode.Alpha7:
        return isShiftkeyPushed ? "\'" : "7";
      case KeyCode.Alpha8:
        return isShiftkeyPushed ? "(" : "8";
      case KeyCode.Alpha9:
        return isShiftkeyPushed ? ")" : "9";
      case KeyCode.A:
        return isShiftkeyPushed ? "A" : "a";
      case KeyCode.B:
        return isShiftkeyPushed ? "B" : "b";
      case KeyCode.C:
        return isShiftkeyPushed ? "C" : "c";
      case KeyCode.D:
        return isShiftkeyPushed ? "D" : "d";
      case KeyCode.E:
        return isShiftkeyPushed ? "E" : "e";
      case KeyCode.F:
        return isShiftkeyPushed ? "F" : "f";
      case KeyCode.G:
        return isShiftkeyPushed ? "G" : "g";
      case KeyCode.H:
        return isShiftkeyPushed ? "H" : "h";
      case KeyCode.I:
        return isShiftkeyPushed ? "I" : "i";
      case KeyCode.J:
        return isShiftkeyPushed ? "J" : "j";
      case KeyCode.K:
        return isShiftkeyPushed ? "K" : "k";
      case KeyCode.L:
        return isShiftkeyPushed ? "L" : "l";
      case KeyCode.M:
        return isShiftkeyPushed ? "M" : "m";
      case KeyCode.N:
        return isShiftkeyPushed ? "N" : "n";
      case KeyCode.O:
        return isShiftkeyPushed ? "O" : "o";
      case KeyCode.P:
        return isShiftkeyPushed ? "P" : "p";
      case KeyCode.Q:
        return isShiftkeyPushed ? "Q" : "q";
      case KeyCode.R:
        return isShiftkeyPushed ? "R" : "r";
      case KeyCode.S:
        return isShiftkeyPushed ? "S" : "s";
      case KeyCode.T:
        return isShiftkeyPushed ? "T" : "t";
      case KeyCode.U:
        return isShiftkeyPushed ? "U" : "u";
      case KeyCode.V:
        return isShiftkeyPushed ? "V" : "v";
      case KeyCode.W:
        return isShiftkeyPushed ? "W" : "w";
      case KeyCode.X:
        return isShiftkeyPushed ? "X" : "x";
      case KeyCode.Y:
        return isShiftkeyPushed ? "Y" : "y";
      case KeyCode.Z:
        return isShiftkeyPushed ? "Z" : "z";
      case KeyCode.Minus:
        return isShiftkeyPushed ? "=" : "-";
      case KeyCode.Caret:
        return isShiftkeyPushed ? "~" : "^";
      case KeyCode.At:
        return isShiftkeyPushed ? "`" : "@";
      case KeyCode.LeftBracket:
        return isShiftkeyPushed ? "{" : "[";
      case KeyCode.RightBracket:
        return isShiftkeyPushed ? "}" : "]";
      case KeyCode.Semicolon:
        return isShiftkeyPushed ? "+" : ";";
      case KeyCode.Colon:
        return isShiftkeyPushed ? "*" : ":";
      case KeyCode.Comma:
        return isShiftkeyPushed ? "<" : ",";
      case KeyCode.Period:
        return isShiftkeyPushed ? ">" : ".";
      case KeyCode.Slash:
        return isShiftkeyPushed ? "?" : "/";
      case KeyCode.Underscore:
        return "_";
      case KeyCode.Space:
        return " ";
      case KeyCode.Backslash:
        return isShiftkeyPushed ? "|" : "Yen";
      default: // 改行文字を割り当てる
        return "";
    }
  }

#if UNITY_EDITOR
  /// <summary>
  /// キーが入力されたとき等の処理(Local ダミー処理)
  /// </summary>
  void OnGUI()
  {
    // JIS かなのときは Shift キー以外取得はしない
    if (ConfigScript.InputMode == (int)ConfigScript.InputType.jisKana) { return; }
    Event e = Event.current;
    var isPushedShiftKey = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    if (isInputValid && e.type == EventType.KeyDown && e.keyCode != KeyCode.None
    && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
    {
      var inputStr = ConvertKeyCodeToStr(e.keyCode, isPushedShiftKey);
      double currentTime = Time.realtimeSinceStartup;
      // タイピングで使用する文字以外は受け付けない
      // Esc など画面遷移などで使うキーと競合を避ける
      if (!inputStr.Equals(""))
      {
        if (isFirstInput)
        {
          if (!ConfigScript.IsBeginnerMode)
          {
            firstCharInputTime = currentTime;
          }
          // 1文字目の時は反応時間もここで計測
          var latency = currentTime - lastSentenceUpdateTime;
          Performance.AddLatencyTime(latency);
          isFirstInput = false;
        }
        // 正誤チェック
        StartCoroutine(TypingCheck(inputStr, currentTime));
      }
    }
  }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
  [DllImport("__Internal")]
  private static extern string GetKeyCodeFromJS();

  [DllImport("__Internal")]
  private static extern void InitKeyCodeQueue();
#else
  // editor 上ではダミーの関数を呼ぶ
  private static string GetKeyCodeFromJS()
  {
    return "";
  }

  private static void InitKeyCodeQueue()
  {
    return;
  }
#endif
}