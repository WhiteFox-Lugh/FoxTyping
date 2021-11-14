using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class TypingSoft : MonoBehaviour
{
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

  // load 関係
  private static bool isLoadSuccess = false;
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
  [SerializeField] private Text UICPUText;
  [SerializeField] private Text UITypeInfo;
  [SerializeField] private Text UINextWord;
  [SerializeField] private Text countdownText;
  [SerializeField] private GameObject DataPanel;
  [SerializeField] private GameObject AssistKeyboardPanel;
  [SerializeField] private GameObject CPUPanel;
  [SerializeField] private GameObject NowLoadingPanel;
  [SerializeField] private GameObject NextWordPanel;
  private static GenerateSentence gs;
  // Assist Keyboard JIS
  private static AssistKeyboardJIS AKJIS;

  // char からひらがなへの変換
  // JIS かな用
  private static readonly Dictionary<string, string> charToHiragana = new Dictionary<string, string> {
    {"1", "ぬ"}, {"!", "ぬ"}, {"2", "ふ"}, {"\"", "ふ"}, {"3", "あ"}, {"#", "ぁ"},
    {"4", "う"}, {"$", "ぅ"}, {"5", "え"}, {"%", "ぇ"}, {"6", "お"}, {"&", "ぉ"},
    {"7", "や"}, {"\'", "ゃ"}, {"8", "ゆ"}, {"(", "ゅ"}, {"9", "よ"}, {")", "ょ"},
    {"0", "わ"}, {"\t", "を"}, {"-", "ほ"}, {"=", "ほ"}, {"^", "へ"}, {"~", "へ"},
    {"Yen", "ー"}, {"|", "ー"}, {"q", "た"}, {"Q", "た"}, {"w", "て"}, {"W", "て"},
    {"e", "い"}, {"E", "ぃ"}, {"r", "す"}, {"R", "す"}, {"t", "か"}, {"T", "か"},
    {"y", "ん"}, {"Y", "ん"}, {"u", "な"}, {"U", "な"}, {"i", "に"}, {"I", "に"},
    {"o", "ら"}, {"O", "ら"}, {"p", "せ"}, {"P", "せ"}, {"@", "゛"}, {"`", "゛"},
    {"[", "゜"}, {"{", "「"}, {"a", "ち"}, {"A", "ち"}, {"s", "と"}, {"S", "と"},
    {"d", "し"}, {"D", "し"}, {"f", "は"}, {"F", "は"}, {"g", "き"}, {"G", "き"},
    {"h", "く"}, {"H", "く"}, {"j", "ま"}, {"J", "ま"}, {"k", "の"}, {"K", "の"},
    {"l", "り"}, {"L", "り"}, {";", "れ"}, {"+", "れ"}, {":", "け"}, {"*", "け"},
    {"]", "む"}, {"}", "」"}, {"z", "つ"}, {"Z", "っ"}, {"x", "さ"}, {"X", "さ"},
    {"c", "そ"}, {"C", "そ"}, {"v", "ひ"}, {"V", "ひ"}, {"b", "こ"}, {"B", "こ"},
    {"n", "み"}, {"N", "み"}, {"m", "も"}, {"M", "も"}, {",", "ね"}, {"<", "、"},
    {".", "る"}, {">", "。"}, {"/", "め"}, {"?", "・"}, {"\\", "ろ"}, {"_", "ろ"},
    {" ", " "}
  };
  // JIS かな用
  // 長音と「ろ」の識別用に OEM2 キーが押されたかのチェックを行う
  private Queue<bool> JISKanaOem2keyLog = new Queue<bool>();
  private const int OEM2KEY_LOG_FRAME = 30;

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
    Retry
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
    gs = GameObject.Find("generateSentenceScript").GetComponent<GenerateSentence>();
    AKJIS = GameObject.Find("AssistKeyboard").GetComponent<AssistKeyboardJIS>();
    // init より先に初期化すべき項目
    // ロード成功したかのフラグを false に
    isLoadSuccess = false;
    // 入力受付状態は一度 true に
    // リトライ機能の関係
    isInputValid = true;
    // ゲームコンディションを in progress にする
    CurrentGameCondition = (int)GameCondition.Progress;
    // ワードデータセットの読み込み
    StartCoroutine(LoadWordDataset(CanStart));
  }

  /// <summary>
  /// データセット読み込み
  /// </summary>
  private IEnumerator LoadWordDataset(UnityAction callback)
  {
    yield return StartCoroutine(gs.LoadAssetBundle(
      () => isLoadSuccess = gs.LoadSentenceData(ConfigScript.DataSetName))
      );
    callback();
  }

  /// <summary>
  /// スタートできるかの確認
  /// </summary>
  private void CanStart()
  {
    // 読み込み成功したらメインへ
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
    JISKanaOem2keyLog = new Queue<bool>();
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
    if (UINextWord != null)
    {
      UINextWord.text = "";
    }
    if (ConfigScript.IsBeginnerMode)
    {
      INTERVAL = 0f;
    }
    if (UICPUText != null)
    {
      UICPUText.text = "";
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
    countdownText.text = "3";
    yield return new WaitForSeconds(1f);
    countdownText.text = "2";
    yield return new WaitForSeconds(1f);
    countdownText.text = "1";
    yield return new WaitForSeconds(1f);
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
      // JIS かなモードの時は OEM2 キーが押されたかどうかを監視
      if (ConfigScript.InputMode == (int)ConfigScript.InputType.jisKana)
      {
        JISKanaOem2keyLog.Enqueue(Keyboard.current.oem2Key.wasPressedThisFrame);
        if (JISKanaOem2keyLog.Count > OEM2KEY_LOG_FRAME)
        {
          JISKanaOem2keyLog.Dequeue();
        }
      }
      // テキストカラーの設定
      TextColorChange();
      // パネル表示
      if (DataPanel != null && AssistKeyboardPanel != null)
      {
        ShowMiddlePanel(ConfigScript.InfoPanelMode);
      }
      if (CPUPanel != null && NextWordPanel != null)
      {
        ShowWordPanel(ConfigScript.WordPanelMode);
      }
      // アシストキーボード表示
      if (AssistKeyboardPanel != null)
      {
        if (CurrentTypingSentence == "" || !isInputValid)
        {
          AKJIS.SetAllKeyColorWhite();
          AKJIS.SetAllFingerColorWhite();
        }
        else if (isInputValid)
        {
          AKJIS.SetNextHighlight(CurrentTypingSentence[0].ToString());
        }
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
    var delayVal = (float)(ConfigScript.DelayTime / 1000.0);
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
      var t = gs.Generate();
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
    // 変数等の初期化
    isFirstInput = true;
    isIntervalEnded = false;
    isRecMistype = false;
    isSentenceMistyped = false;
    index = 0;
    sentenceLength = 0;
    // Space は打ったか打ってないかわかりにくいので表示上はアンダーバーに変更
    // SetUITypeText(nextTypingSentence);
    CurrentTypingSentence = nextTypingSentence;
    cpuTypeString = nextTypingSentence;
    // UI 上のテキスト変更
    UIOriginSentence.text = originSentence;
    UIYomigana.text = typeSentence;
    UIType.text = "";
    if (ConfigScript.IsBeginnerMode || ConfigScript.IsShowTypeSentence)
    {
      UIType.text = nextTypingSentence;
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
    // CPU Start
    if (UICPUText != null)
    {
      UICPUText.text = "";
      StartCoroutine("CPUType");
    }
    if (UINextWord != null)
    {
      if (currentTaskNumber + 1 < numOfTask)
      {
        UINextWord.text = originSentenceList[currentTaskNumber + 1];
      }
      else
      {
        UINextWord.text = "";
      }
    }
  }

  /// <summary>
  /// CPU のタイピング処理
  /// </summary>
  private IEnumerator CPUType()
  {
    var idx = 0;
    float waitTime = (float)(60.0 / ConfigScript.CPUKpm);
    // 1文字目を打つまでは待機
    while (isFirstInput && !isIntervalEnded)
    {
      yield return null;
    }
    // 残りの文字列処理
    while (isInputValid && idx < cpuTypeString.Length)
    {
      UICPUText.text += cpuTypeString[idx].ToString();
      idx++;
      yield return new WaitForSeconds(waitTime);
    }
    yield return null;
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
    // JIS かなの OEM2 キー判定用に 1フレームだけ遅延させる
    yield return null;
    // OEM2 キーが押されているかどうかの判定
    if (JISKanaOem2keyLog.Contains(true))
    {
      nextString = "\\";
      // 多重判定防止のためのクリア
      JISKanaOem2keyLog.Clear();
    }
    // リザルト集積
    if (ConfigScript.InputMode == (int)ConfigScript.InputType.roman)
    {
      typedLetter.Append(nextString);
    }
    else if (ConfigScript.InputMode == (int)ConfigScript.InputType.jisKana)
    {
      typedLetter.Append(charToHiragana[nextString]);
    }
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
      // ローマ字
      if (ConfigScript.InputMode == (int)ConfigScript.InputType.roman)
      {

        // 正解タイプ
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
      // JIS かな
      else if (ConfigScript.InputMode == (int)ConfigScript.InputType.jisKana)
      {
        string inputHiragana = charToHiragana[currentStr];
        if (inputHiragana.Equals(judgeString))
        {
          isMistype = false;
          indexAdd[index][i] = 1;
        }
        else
        {
          indexAdd[index][i] = 0;
        }
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
  private double GetSentenceKeyPerMinute(double sentenceTypeTime)
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
        double sectionKPM = GetSentenceKeyPerMinute(sentenceTypeTime);
        int intKPM = Convert.ToInt32(Math.Floor(keyPerMin));
        int intSectionKPM = Convert.ToInt32(Math.Floor(sectionKPM));
        UpdateUIKeyPerMinute(intKPM, intSectionKPM);
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
    correctString += (ConfigScript.InputMode == 0) ? typeChar : charToHiragana[typeChar];
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
      UIKPM.text = $"タイプ速度(ワード) : {intSentenceKPM.ToString()} kpm\n" + $"平均タイプ速度 : {intKPMAll.ToString()} kpm";
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
  /// ワード関連の UI 表示を更新
  /// <param name="activePanelVal">表示をアクティブにするパネルの番号</param>
  /// </summary>
  private void ShowWordPanel(int activePanelVal)
  {
    if (activePanelVal == 0)
    {
      CPUPanel.SetActive(false);
      NextWordPanel.SetActive(true);
    }
    else if (activePanelVal == 1)
    {
      CPUPanel.SetActive(true);
      NextWordPanel.SetActive(false);
    }
  }

  /// <summary>
  /// キーが入力されたとき等の処理
  /// </summary>
  void OnGUI()
  {
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

}