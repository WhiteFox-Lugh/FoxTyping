using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 差分表示用
public struct Diff
{
  public int op;
  public string before;
  public string after;

  public Diff(int op, string before, string after)
  {
    this.op = op;
    this.before = before;
    this.after = after;
  }
}

public class LongSentenceScript : MonoBehaviour
{
  private enum JudgeType
  {
    insert,
    delete,
    replace,
    correct
  };
  private const string ABPathLocal = "AssetBundleData/wordset_long";
  private const string ABPath = "https://whitefox-lugh.github.io/FoxTyping/AssetBundleData/wordset_long";
  // diff の表示色
  private const string COLOR_INSERT = "orange";
  private const string COLOR_DELETE = "red";
  private const string COLOR_REPLACE = "blue";
  // 正解、不正解の重み
  private const int CORRECT_SCORE = 1;
  private const int MISS_COST = 30;
  private double startTime;
  private bool isShowInfo;
  private bool isFinished;
  // AssetBundle
  private static AssetBundle abLongData;
  // UI
  [SerializeField] TextMeshProUGUI UIResultTextField;
  [SerializeField] TextMeshProUGUI UIResultElapsedTime;
  [SerializeField] RubyTextMeshProUGUI UITextField;
  [SerializeField] Text UIRestTime;
  [SerializeField] Text UICountDownText;
  [SerializeField] Text UIInputCounter;
  [SerializeField] TextMeshProUGUI UIScoreText;
  [SerializeField] TextMeshProUGUI UIDetailText;
  [SerializeField] TextMeshProUGUI TaskTextContent;
  [SerializeField] TextMeshProUGUI CurrentInputText;
  [SerializeField] TextMeshProUGUI PreviewText;
  [SerializeField] TMP_Dropdown DropdownSectionSelect;
  [SerializeField] GameObject TaskViewport;
  [SerializeField] GameObject InputArea;
  [SerializeField] TMP_InputField UIInputField;
  [SerializeField] GameObject SectionSelectPanel;
  [SerializeField] GameObject InputPanel;
  [SerializeField] GameObject ResultPanel;
  [SerializeField] GameObject TaskPanel;
  [SerializeField] GameObject InfoPanel;
  [SerializeField] GameObject ScorePanel;
  [SerializeField] GameObject OperationPanel;
  [SerializeField] GameObject ResultOperationPanel;
  [SerializeField] GameObject TaskVerticalBar;
  [SerializeField] GameObject InputVerticalBar;
  // 課題文章関係
  private static readonly string sectionRegex = @"\\[s|S]ection\{([\w|\p{P}]+)\}[\r|\r\n|\n]";
  private static readonly string rubyRegex = @"\\[r|R]uby\{(?<word>\w+)\}\{(?<ruby>\w+)\}";
  private static List<int> sectionStartPosList;
  // ルビ利用するかどうか
  private static bool isUseRuby;
  // 表示している文章
  private static string displayText;
  // ルビ付き
  private static string taskWithRuby;
  // オリジナル
  private static string taskText;
  // スコア表示
  private int correctCount = 0;
  private int deleteCount = 0;
  private int insertCount = 0;
  private int replaceCount = 0;
  // 制限時間
  private static int LimitSec
  {
    set;
    get;
  } = 300;

  /// <summary>
  /// Update() 前の処理
  /// </summary>
  void Awake()
  {
    InitUIPanel();
    StartCoroutine(LoadAssetBundle(CanStart));
  }

  /// <summary>
  /// パネルの表示を設定
  /// </summary>
  private void InitUIPanel()
  {
    InputPanel.SetActive(true);
    TaskPanel.SetActive(true);
    InfoPanel.SetActive(true);
    ResultPanel.SetActive(false);
    ScorePanel.SetActive(false);
    OperationPanel.SetActive(true);
    ResultOperationPanel.SetActive(false);
    SectionSelectPanel.SetActive(true);
    UIInputField.interactable = false;
  }

  /// <summary>
  /// リトライ処理
  /// </summary>
  public void Retry()
  {
    if (isShowInfo || isFinished)
    {
      InitUIPanel();
      SelectSection();
    }
  }

  /// <summary>
  /// スタートできるかどうかをチェック
  /// </summary>
  private void CanStart()
  {
    if (abLongData != null)
    {
      GetSectionInfo();
      SelectSection();
    }
    else
    {
      ReturnConfig();
    }
  }

  /// <summary>
  /// スタートボタンを押したときの挙動
  /// </summary>
  public void StartPractice()
  {
    SectionSelectPanel.SetActive(false);
    Init();
  }

  /// <summary>
  /// セクション選択に移動
  /// </summary>
  private void SelectSection()
  {
    SectionSelectPanel.SetActive(true);
  }

  /// <summary>
  /// セクション情報の取得
  /// </summary>
  private void GetSectionInfo()
  {
    sectionStartPosList = new List<int>();
    var sectionHeaderList = new List<string>();
    // \section{} の抽出
    var docData = abLongData.LoadAsset<TextAsset>(ConfigScript.LongSentenceTaskName).ToString();
    var idx = 1;
    foreach (Match match in Regex.Matches(docData, sectionRegex))
    {
      sectionStartPosList.Add(match.Index);
      var sectionSentence = Regex.Replace(match.Value, sectionRegex, "$1 ...");
      sectionHeaderList.Add($"第{idx}セクション: {sectionSentence}");
      idx++;
    }
    // プレイ前のセクション選択用に課題文を表示
    var sectionReplacedDoc = Regex.Replace(docData, sectionRegex, "");
    var previewDoc = Regex.Replace(sectionReplacedDoc, rubyRegex, "$1");
    PreviewText.text = previewDoc;
    // Dropdown にセット
    DropdownSectionSelect.ClearOptions();
    DropdownSectionSelect.AddOptions(sectionHeaderList);
  }

  /// <summary>
  /// AssetBundle の読み込み
  /// <param name="callback">callback 関数</param>
  /// </summary>
  private IEnumerator LoadAssetBundle(UnityAction callback)
  {
    var networkState = Application.internetReachability;
    // すでに AssetBundle が読み込まれているか、
    // そうでないときにネットワークに接続していないときはリクエストを送信しない
    // ネットワーク接続していないときは何かしらエラーを出すとよさそう
    if (abLongData != null)
    {
      callback();
      yield break;
    }

    // WebGL 時は WebRequest によって AssetBundle を取得
#if UNITY_WEBGL && !UNITY_EDITOR
		if (networkState == NetworkReachability.NotReachable){
			UnityEngine.Debug.Log("ネットワークに接続していません");
			callback();
			yield break;
		}
		UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(ABPath);
		yield return request.SendWebRequest();
		if (request.isNetworkError || request.isHttpError){
			UnityEngine.Debug.LogError(request.error);
		}
		else {
			abLongData = DownloadHandlerAssetBundle.GetContent(request);
			UnityEngine.Debug.Log("load successfully");
		}
#else
    abLongData = AssetBundle.LoadFromFile(ABPathLocal);
    if (abLongData == null)
    {
      UnityEngine.Debug.Log("Error: AssetBundle Load failed");
    }
#endif

    callback();
    yield break;
  }

  /// <summary>
  /// 各種初期化
  /// </summary>
  private void Init()
  {
    // クリップボードの中身を消去
    GUIUtility.systemCopyBuffer = "";
    // その他の初期化
    isUseRuby = ConfigScript.UseRuby;
    GenerateTaskText();
    startTime = 0.0;
    isShowInfo = false;
    isFinished = false;
    UIInputField.interactable = false;
    UITextField.text = "";
    UIInputField.text = "";
    UIRestTime.text = "";
    UIInputCounter.text = "";
    LimitSec = ConfigScript.LongSentenceTimeLimit;
    StartCoroutine(CountDown());
  }

  /// <summary>
  /// カウントダウンの処理
  /// </summary>
  private IEnumerator CountDown()
  {
    UICountDownText.text = "3";
    yield return new WaitForSeconds(1.0f);
    UICountDownText.text = "2";
    yield return new WaitForSeconds(1.0f);
    UICountDownText.text = "1";
    yield return new WaitForSeconds(1.0f);
    UICountDownText.text = "";
    AfterCountDown();
  }

  /// <summary>
  /// カウントダウン後の処理
  /// </summary>
  private void AfterCountDown()
  {
    // 経過時間と入力文字数の表示
    isShowInfo = true;
    // 課題文表示
    UITextField.UnditedText = displayText;
    // 入力フィールドアクティブ化
    UIInputField.interactable = true;
    UIInputField.ActivateInputField();
    // 開始時刻取得
    startTime = Time.realtimeSinceStartup;
  }

  /// <summary>
  /// 毎フレーム処理
  /// </summary>
  void Update()
  {
    // // フォーカスされていなければ強制フォーカス
    // if (!){
    //   UIInputField.Select();
    // }
    if (isShowInfo && !isFinished)
    {
      // 入力中はタイマーを更新
      CheckTimer();
      CheckInputStr();
      // スクロール位置を調整
      if (UIInputField.isFocused)
      {
        AdjustScroll();
      }
    }
  }

  /// <summary>
  /// 課題文章に合わせてスクロール位置を調整
  /// </summary>
  private void AdjustScroll()
  {
    var scrollBarTask = TaskVerticalBar.GetComponent<Scrollbar>();
    var scrollBarInput = InputVerticalBar.GetComponent<Scrollbar>();
    // 現在のバーの位置を取得(0-1)
    var taskBarPos = scrollBarTask.value;
    var inputBarPos = scrollBarInput.value;
    // UnityEngine.Debug.Log($"task -> {taskBarPos}, input -> {inputBarPos}");
    // 表示ウィンドウの高さを取得
    var taskWindowHeight = TaskViewport.GetComponent<RectTransform>().sizeDelta.y;
    var inputWindowHeight = InputArea.GetComponent<RectTransform>().sizeDelta.y;
    // 課題文、入力文のコンテンツの高さと行数を取得
    var taskHeight = TaskTextContent.preferredHeight;
    var inputHeight = CurrentInputText.preferredHeight;
    var taskLineHeight = taskHeight / TaskTextContent.textInfo.lineCount;
    var inputLineHeight = inputHeight / CurrentInputText.textInfo.lineCount;
    // スクロールバーの同期
    // inputPos は入力した文章の上からどれだけスクロールしたか
    var inputPos = (Math.Max(inputHeight, inputWindowHeight) - inputWindowHeight) * inputBarPos;
    // UnityEngine.Debug.Log($"inputPos -> {inputPos}");
    // inputPos のスクロール量を Task のスクロールバーのほうで換算
    var nextTaskScrollBarValue = Math.Min(1, inputPos / (taskHeight - taskWindowHeight));
    scrollBarTask.value = 1 - nextTaskScrollBarValue;
    // scrollBarInput.value = nextInputBarPos;
    // UnityEngine.Debug.Log($"高さ: {taskHeight}, {inputHeight}、行の高さ: {taskLineHeight}, {inputLineHeight}");
  }

  /// <summary>
  /// 表示文章の生成
  /// </summary>
  private void GenerateTaskText()
  {
    int startIdx = sectionStartPosList[DropdownSectionSelect.value];
    string replacement = "<r=$2>$1</r>";
    string newlinePattern = @"[\n|\r\n|\r]";
    var docDataOrigin = abLongData.LoadAsset<TextAsset>(ConfigScript.LongSentenceTaskName).ToString().Substring(startIdx);
    var docData = Regex.Replace(docDataOrigin, sectionRegex, "");
    taskText = Regex.Replace(docData, rubyRegex, "$1");
    taskText = Regex.Replace(taskText, newlinePattern, "⏎\n");
    var convertedText = Regex.Replace(docData, rubyRegex, replacement);
    taskWithRuby = Regex.Replace(convertedText, newlinePattern, "⏎\n");
    displayText = (isUseRuby ? taskWithRuby : taskText);
  }

  /// <summary>
  /// 課題文をスクロールする
  /// <param name="numOfLine">スクロールする行数</param>
  /// </summary>
  public void ScrollTask(int numOfLine)
  {
    var scrollBar = TaskVerticalBar.GetComponent<Scrollbar>();
    // 現在のバーの位置を取得(0-1)
    var currentBarPos = scrollBar.value;
    // 表示ウィンドウの高さを取得
    var windowHeight = TaskViewport.GetComponent<RectTransform>().sizeDelta.y;
    // 課題文のコンテンツの高さと行数を取得
    var taskHeight = TaskTextContent.preferredHeight;
    var lineHeight = taskHeight / TaskTextContent.textInfo.lineCount;
    // 現在表示している下限のy座標
    var currentPosY = currentBarPos * (Math.Max(taskHeight, windowHeight) - windowHeight);
    // スクロール後の位置座標
    var setPosY = currentPosY - numOfLine * lineHeight;
    // スクロール後のバーの位置
    var setBarPos = setPosY / (Math.Max(taskHeight, windowHeight) - windowHeight);
    if (setBarPos > 1f)
    {
      setBarPos = 1f;
    }
    else if (setBarPos < 0f)
    {
      setBarPos = 0f;
    }
    scrollBar.value = setBarPos;
  }

  /// <summary>
  /// ルビを消す
  /// </summary>
  private void HideRuby()
  {
    displayText = taskText + "\n";
    isUseRuby = false;
    UITextField.UnditedText = displayText;
  }

  /// <summary>
  /// ルビを表示
  /// </summary>
  private void ShowRuby()
  {
    displayText = taskWithRuby + "\n";
    isUseRuby = true;
    UITextField.UnditedText = displayText;
  }

  /// <summary>
  /// タイマーのチェックと更新
  /// </summary>
  private void CheckTimer()
  {
    var elapsedTime = Time.realtimeSinceStartup - startTime;
    var elapsedTimeInt = Convert.ToInt32(Math.Floor(elapsedTime));
    if (elapsedTimeInt >= LimitSec)
    {
      Finish();
    }
    var restMin = (LimitSec - elapsedTimeInt) / 60;
    var restSec = (LimitSec - elapsedTimeInt) % 60;
    UIRestTime.text = $"残り時間: {restMin.ToString()}分 {restSec.ToString()}秒";
  }

  /// <summary>
  /// 入力文字数のカウントチェック
  /// </summary>
  private void CheckInputStr()
  {
    var inputText = UIInputField.text;
    int inputCount = inputText.Length;
    UIInputCounter.text = $"入力文字数: {inputCount.ToString()}";
  }

  /// <summary>
  /// 入力終了後の処理
  /// </summary>
  public void Finish()
  {
    var isPracticing = !isFinished && isShowInfo;
    if (!isPracticing) { return; }
    // 終了時刻の取得
    var endTime = Time.realtimeSinceStartup;
    var elapsedTime = endTime - startTime;
    var elapsedTimeInt = Convert.ToInt32(Math.Floor(elapsedTime));
    var elapsedMin = elapsedTimeInt / 60;
    var elapsedSec = elapsedTimeInt % 60;
    UIResultElapsedTime.text = $"経過時間: {elapsedMin.ToString()}分 {elapsedSec.ToString()}秒";
    // 表示の切り替え
    ResultPanel.SetActive(true);
    ScorePanel.SetActive(true);
    InfoPanel.SetActive(false);
    InputPanel.SetActive(false);
    TaskPanel.SetActive(false);
    OperationPanel.SetActive(false);
    ResultOperationPanel.SetActive(true);
    UIResultTextField.text = UIInputField.text;
    UIInputField.interactable = false;
    isFinished = true;
    // 得点計算と表示
    ShowScore();
  }

  /// <summary>
  /// スコア表示の処理
  /// </summary>
  private void ShowScore()
  {
    // 編集距離の計算
    string taskSentence = taskText;
    string userInputSentence = UIInputField.text;
    List<Diff> diff = GetDiff(taskText, userInputSentence);
    var coloredText = ConvertDiffToHtml(diff);
    SetScoreDetail(diff);
    ShowOriginalScore();
    UIResultTextField.text = coloredText;
  }

  /// <summary>
  /// オリジナル計算のスコアの表示切替
  /// </summary>
  private void ShowOriginalScore()
  {
    int score = GetOriginalScore();
    var sbScore = new StringBuilder();
    var sbDetail = new StringBuilder();
    sbScore.Append($"スコア(F)： {score.ToString()}");
    sbDetail.Append($"正解数：{correctCount.ToString()} x {CORRECT_SCORE.ToString()}点\n")
            .Append($"<color=\"{COLOR_DELETE}\">削除：{deleteCount.ToString()}")
            .Append($" x (-{MISS_COST.ToString()}点)</color> / ")
            .Append($"<color=\"{COLOR_INSERT}\">余分：{insertCount.ToString()}")
            .Append($" x (-{MISS_COST.ToString()}点)</color>\n")
            .Append($"<color=\"{COLOR_REPLACE}\">置換：{replaceCount.ToString()}")
            .Append($" x (-{MISS_COST.ToString()}点)</color>");
    UIScoreText.text = sbScore.ToString();
    UIDetailText.text = sbDetail.ToString();
  }

  /// <summary>
  /// オリジナルスコアの値取得
  /// <returns>オリジナルスコア</returns>
  /// </summary>
  private int GetOriginalScore()
  {
    return correctCount - (deleteCount + insertCount + replaceCount) * MISS_COST;
  }

  /// <summary>
  /// 正解数、不正解数と不正解の内訳を Diff からカウント
  /// <param name="diffs">Diff のリスト</param>
  /// </summary>
  private void SetScoreDetail(List<Diff> diffs)
  {
    correctCount = 0;
    deleteCount = 0;
    insertCount = 0;
    replaceCount = 0;
    foreach (Diff diff in diffs)
    {
      int op = diff.op;
      string beforeText = diff.before;
      string afterText = diff.after;
      if (op == (int)JudgeType.correct)
      {
        correctCount += beforeText.Length;
      }
      else if (op == (int)JudgeType.delete)
      {
        deleteCount += beforeText.Length;
      }
      else if (op == (int)JudgeType.insert)
      {
        insertCount += afterText.Length;
      }
      else if (op == (int)JudgeType.replace)
      {
        replaceCount += beforeText.Length;
      }
    }
  }

  /// <summary>
  /// strA (原文) から strB (入力文) への Diff を取得
  /// <param name="strA">原文</param>
  /// <param name="strB">入力された文章</param>
  /// <returns>Diff のリスト</returns>
  /// </summary>
  private static List<Diff> GetDiff(string strA, string strB)
  {
    var retBackTrace = new List<Diff>() { };

    // 1: 共通の prefix を探す
    int minLen = Math.Min(strA.Length, strB.Length);
    int commonPrefixIndex = -1;
    for (int i = 0; i < minLen; ++i)
    {
      if (strA[i] == strB[i])
      {
        commonPrefixIndex = i;
      }
      else
      {
        break;
      }
    }
    string commonPrefix = (commonPrefixIndex == -1) ? "" : strA.Substring(0, commonPrefixIndex + 1);
    string restStrA = (commonPrefixIndex == -1) ? strA : strA.Substring(commonPrefixIndex + 1, strA.Length - commonPrefixIndex - 1);
    string restStrB = (commonPrefixIndex == -1) ? strB : strB.Substring(commonPrefixIndex + 1, strB.Length - commonPrefixIndex - 1);
    // restB が空 -> そこまで全部正解
    if (restStrB.Equals(""))
    {
      retBackTrace.Add(new Diff((int)JudgeType.correct, commonPrefix, ""));
      return retBackTrace;
    }

    // 2: 入力した文章の最後n文字が課題文に一致するか？
    // ここでの suffix は厳密には suffix ではないが便宜上そう呼ぶことに

    // 入力された文字(strB)の後ろ最大何文字が課題文の一部に含まれるか二分探索
    int lb = 0, ub = restStrB.Length + 1;
    bool hasSuffix = false;
    while (ub - lb > 1)
    {
      int mid = lb + (ub - lb) / 2;
      string subStr = restStrB.Substring(restStrB.Length - mid, mid);
      int idxSubStr = restStrA.IndexOf(subStr);
      if (idxSubStr == -1)
      {
        ub = mid;
      }
      else
      {
        lb = mid;
        hasSuffix = true;
      }
    }
    int commonSuffixIndex = restStrB.Length - lb;
    string commonSuffix = restStrB.Substring(commonSuffixIndex, lb);

    // 点数をできるだけ大きくしたいので、rest の文字列長の差の絶対値が最小となるように切り取る
    var suffixIndexList = new List<int>();
    int trimSubstrIndex = restStrA.IndexOf(commonSuffix);
    if (hasSuffix)
    {
      int idx = 0;
      int nextIdx;
      do
      {
        nextIdx = restStrA.IndexOf(commonSuffix, idx);
        idx = nextIdx + 1;
        if (nextIdx != -1)
        {
          suffixIndexList.Add(nextIdx);
        }
      } while (nextIdx != -1);
      int diffAbsMin = Int32.MaxValue;
      var middleStrB = restStrB.Substring(0, commonSuffixIndex);
      foreach (int trimIdx in suffixIndexList)
      {
        var middleStrA = restStrA.Substring(0, trimIdx);
        int diff = Math.Abs(middleStrA.Length - middleStrB.Length);
        if (diff <= diffAbsMin)
        {
          trimSubstrIndex = trimIdx;
          diffAbsMin = diff;
        }
      }
    }

    // prefix と suffix をのぞいた残りの文字 (diff を取るべき文字列)
    restStrA = (commonSuffixIndex == restStrB.Length) ? restStrA : restStrA.Substring(0, trimSubstrIndex);
    restStrB = (commonSuffixIndex == restStrB.Length) ? restStrB : restStrB.Substring(0, commonSuffixIndex);

    // 3: 共通 suffix と prefix をのぞいた残りの文字の diff を求める
    // BackTrace で前方一致させるため、Reverse してから DP する

    // 編集距離を求める DP パート
    string src = new string(restStrA.Reverse().ToArray());
    string dst = new string(restStrB.Reverse().ToArray());
    var rows = src.Length + 1;
    var cols = dst.Length + 1;
    int[,] d = new int[rows, cols];
    for (int i = 0; i < rows; ++i)
    {
      d[i, 0] = i;
    }
    for (int i = 0; i < cols; ++i)
    {
      d[0, i] = i;
    }
    for (int i = 1; i < rows; ++i)
    {
      for (int j = 1; j < cols; ++j)
      {
        d[i, j] = Math.Min(d[i - 1, j - 1] + ((src[i - 1] == dst[j - 1]) ? 0 : 1),
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1));
      }
    }

    // バックトレース
    var tmpBackTrace = BackTrace(src, dst, d);

    // prefix, suffix と統合
    var prefixTrace = (commonPrefix.Equals("")) ? new List<Diff>() { }
                        : new List<Diff>() { (new Diff((int)JudgeType.correct, commonPrefix, "")) };
    var suffixTrace = (commonSuffix.Equals("")) ? new List<Diff>() { }
                        : new List<Diff>() { (new Diff((int)JudgeType.correct, commonSuffix, "")) };
    var trace = ConvertDiff(tmpBackTrace, restStrA, restStrB);
    retBackTrace.AddRange(prefixTrace);
    retBackTrace.AddRange(trace);
    retBackTrace.AddRange(suffixTrace);

    // 4. 最後の2つの Diff 例外処理 (得点をできるだけ大きくするため)

    // delete, equal だった場合
    var len = retBackTrace.Count;
    if (len >= 2 && (retBackTrace[len - 2].op == (int)JudgeType.delete) && (retBackTrace[len - 1].op == (int)JudgeType.correct))
    {
      var diff2 = retBackTrace[len - 2];
      var diff1 = retBackTrace[len - 1];
      var delLen = diff2.before.Length;
      var eqLen = diff1.before.Length;
      // 脱字文字コスト + 正解数 よりも 余分文字コストのみの方がスコアが高くなる時置き換え
      if (MISS_COST * delLen > (MISS_COST + 1) * eqLen)
      {
        retBackTrace.RemoveRange(len - 2, 2);
        retBackTrace.Add(new Diff((int)JudgeType.insert, "", diff1.before));
      }
    }
    // replace, delete だった場合は置き換えて削除より余分文字として減点したほうが必ず得点が高い
    else if (len >= 2 && (retBackTrace[len - 2].op == (int)JudgeType.replace) && (retBackTrace[len - 1].op == (int)JudgeType.delete))
    {
      var diff2 = retBackTrace[len - 2];
      retBackTrace.RemoveRange(len - 2, 2);
      retBackTrace.Add(new Diff((int)JudgeType.insert, "", diff2.after));
    }
    return retBackTrace;
  }

  /// <summary>
  /// 編集グラフをバックトレース
  /// <param name="matrix">編集行列</param>
  /// <returns>バックトレース結果</returns>
  /// </summary>
  private static List<(int, (int, int))> BackTrace(string strA, string strB, int[,] matrix)
  {
    const int INF = -1000;
    var ALen = strA.Length;
    var BLen = strB.Length;
    int row = ALen;
    int col = BLen;
    var trace = new List<(int, (int, int))>();
    while (row > 0 || col > 0)
    {
      int cost = (row > 0 && col > 0 && (strA[row - 1] == strB[col - 1])) ? 0 : 1;
      int current = matrix[row, col];
      int costA = (row == 0) ? INF : matrix[row - 1, col];
      int costB = (row == 0 || col == 0) ? INF : matrix[row - 1, col - 1];
      int costC = (col == 0) ? INF : matrix[row, col - 1];
      // 置換 or 一致
      if (costB != INF && current == costB + cost)
      {
        if (strA[row - 1] == strB[col - 1])
        {
          trace.Add(((int)JudgeType.correct, (row - 1, col - 1)));
        }
        else
        {
          trace.Add(((int)JudgeType.replace, (row - 1, col - 1)));
        }
        row--;
        col--;
      }
      // 挿入
      else if (costC != INF && current == costC + 1)
      {
        trace.Add(((int)JudgeType.insert, (row, col - 1)));
        col--;
      }
      // 削除
      else if (costA != INF && current == costA + 1)
      {
        trace.Add(((int)JudgeType.delete, (row - 1, col)));
        row--;
      }
    }
    // リバースした文字列のトレースをしたのでインデックスを変更
    var ret = new List<(int, (int, int))>();
    foreach (var p in trace)
    {
      ret.Add((p.Item1, (ALen - p.Item2.Item1 - 1, BLen - p.Item2.Item2 - 1)));
    }
    return ret;
  }

  /// <summary>
  /// バックトレースした行列の座標から Diff へ変換
  /// <param name="opList">文字列操作のリスト</param>
  /// <param name="compStrA">元の文</param>
  /// <param name="compStrB">編集後の文</param>
  /// <returns>Diff のリスト</returns>
  /// </summary>
  private static List<Diff> ConvertDiff(List<(int op, (int idxA, int idxB))> opList, string compStrA, string compStrB)
  {
    var ret = new List<Diff>() { };
    int i = 0;
    if (compStrA == "")
    {
      ret.Add(new Diff((int)JudgeType.insert, "", compStrB));
      return ret;
    }
    else if (compStrB == "")
    {
      ret.Add(new Diff((int)JudgeType.delete, compStrA, ""));
      return ret;
    }
    while (i < opList.Count)
    {
      var current = opList[i];
      var currentOp = current.op;
      var targetStrA = (current.op == (int)JudgeType.insert) ? "" : compStrA[current.Item2.idxA].ToString();
      var targetStrB = (current.op == (int)JudgeType.delete) ? "" : compStrB[current.Item2.idxB].ToString();
      int j = 0;
      while (i + j + 1 < opList.Count)
      {
        var next = opList[i + j + 1];
        var nextOp = next.op;
        if (nextOp == currentOp)
        {
          j++;
          targetStrA += (nextOp == (int)JudgeType.insert) ? "" : compStrA[next.Item2.idxA].ToString();
          targetStrB += (nextOp == (int)JudgeType.delete) ? "" : compStrB[next.Item2.idxB].ToString();
        }
        else
        {
          break;
        }
      }
      if (currentOp == (int)JudgeType.delete)
      {
        ret.Add(new Diff(currentOp, targetStrA, ""));
      }
      else if (currentOp == (int)JudgeType.insert)
      {
        ret.Add(new Diff(currentOp, "", targetStrB));
      }
      else if (currentOp == (int)JudgeType.replace)
      {
        ret.Add(new Diff(currentOp, targetStrA, targetStrB));
      }
      else if (currentOp == (int)JudgeType.correct)
      {
        ret.Add(new Diff(currentOp, targetStrA, ""));
      }
      i += 1 + j;
    }
    return ret;
  }

  /// <summary>
  /// Diff から Html を生成
  /// 文字に色を付けて強調表示を行う
  /// <param name="diffs">diff のリスト</param>
  /// <returns>html 化された入力文章</returns>
  /// </summary>
  private static string ConvertDiffToHtml(List<Diff> diffs)
  {
    var sb = new StringBuilder();
    foreach (Diff diff in diffs)
    {
      string beforeText = diff.before.Replace("&", "&amp;").Replace("<", "&lt;")
      .Replace(">", "&gt;").Replace("\n", "&para;<br>");
      string afterText = diff.after.Replace("&", "&amp;").Replace("<", "&lt;")
      .Replace(">", "&gt;").Replace("\n", "&para;<br>");
      if (diff.op == (int)JudgeType.correct)
      {
        sb.Append(beforeText);
      }
      else if (diff.op == (int)JudgeType.insert)
      {
        sb.Append("<color=\"" + COLOR_INSERT + "\">").Append(afterText).Append("</color>");
      }
      else if (diff.op == (int)JudgeType.delete)
      {
        sb.Append("<color=\"" + COLOR_DELETE + "\">").Append(beforeText).Append("</color>");
      }
      else if (diff.op == (int)JudgeType.replace)
      {
        sb.Append("<color=\"" + COLOR_REPLACE + "\">[").Append(beforeText).Append(",").Append(afterText).Append("]</color>");
      }
    }
    var html = sb.ToString();
    var ret = html.Replace("&para;<br>", "⏎\n");
    return ret;
  }

  /// <summary>
  /// ルビ切り替えボタンを押したときの挙動
  /// </summary>
  public void OnClickRubyButton()
  {
    var isPracticing = !isFinished && isShowInfo;
    if (!isPracticing)
    {
      return;
    }
    if (isUseRuby)
    {
      HideRuby();
    }
    else
    {
      ShowRuby();
    }
  }

  /// <summary>
  /// Config 画面へ戻る
  /// </summary>
  public void ReturnConfig()
  {
    SceneManager.LoadScene("SinglePlayConfigScene");
  }
}
