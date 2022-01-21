using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XCharts;

public class RecordSceneScript : MonoBehaviour
{
  [SerializeField] private TextMeshProUGUI UIResultDetailText;
  [SerializeField] private TextMeshProUGUI UIAverageKPS;
  [SerializeField] private TextMeshProUGUI UIKPSStdDev;
  [SerializeField] private TextMeshProUGUI UIScoreText;
  [SerializeField] private TextMeshProUGUI UITimeText;
  [SerializeField] private TextMeshProUGUI UIAccuracyText;
  [SerializeField] private TextMeshProUGUI UIRank;
  [SerializeField] private TextMeshProUGUI UIWordsetName;
  [SerializeField] private GameObject ScoreInfoPanel;
  [SerializeField] private Material[] RankFontMaterials;
  [SerializeField] private Toggle DefaultToggle;
  [SerializeField] private Toggle DetailToggle;
  [SerializeField] private Button WordButtonPrefab;
  [SerializeField] private Button ReplayButton;
  [SerializeField] private Transform WordButtonList;
  // 詳細表示
  [SerializeField] private TextMeshProUGUI DetailResultWordNumText;
  [SerializeField] private TextMeshProUGUI DetailResultWordSentenceText;
  [SerializeField] private TextMeshProUGUI DetailResultWordReplayText;
  [SerializeField] private LineChart KPSLineChart;
  private readonly int[] RankScore = new int[20] {
    1000, 950, 900, 850, 800, 750, 700, 650, 600, 550,
    500, 450, 400, 350, 300, 250, 200, 150, 100, 0
  };
  private readonly int[] RankScoreJISKana = new int[20] {
    800, 760, 720, 680, 640, 600, 560, 520, 480, 440,
    400, 360, 320, 280, 240, 200, 160, 120, 80, 0
  };

  private readonly string[] RankName = new string[20] {
    "Legend", "GrandMaster 1", "GrandMaster 2", "GrandMaster 3", "Master 1",
    "Master 2", "Master 3", "S1", "S2", "S3",
    "A1", "A2", "A3", "B1", "B2",
    "B3", "C1", "C2", "C3", "C4"
  };

  private readonly int[] RankFontMaterialNum = new int[20] {
    0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 5, 6, 6, 6, 6
  };
  private static int DetailCurrentWordNum = 0;
  // 0 のとき通常表示、1のときキャプチャ用表示
  private static int ResultDisplayFlag = 0;
  private static StringBuilder ResultStringBuilder = new StringBuilder();
  private static StringBuilder ResultForCaptureStrBuilder = new StringBuilder();
  private static List<Button> WordButtonObjList = new List<Button>();

  /// <summary>
  /// 初期化など
  /// </summary>
  void Awake()
  {
    ResultForCaptureStrBuilder.Clear();
    ResultStringBuilder.Clear();
    ResultDisplayFlag = 0;
    if (ScoreInfoPanel != null) { ScoreInfoPanel.SetActive(false); }
    // 最初のワードをチャートに表示
    if (KPSLineChart != null) { SetWordKPSChart(0); }
    SetResult();
    SetWordResult();
    if (DefaultToggle != null && DetailToggle != null)
    {
      // OnValueChanged を強制動作させることで Panel を取得しなくて済む
      DefaultToggle.isOn = !DefaultToggle.isOn;
      DefaultToggle.isOn = true;
      DetailToggle.isOn = !DetailToggle.isOn;
      DetailToggle.isOn = false;
      SetDetailResult();
    }
  }

  /// <summary>
  /// ワードごとの KPS 折れ線表示
  /// </summary>
  /// <param name="wordNum">ワード番号(0-index)</param>
  private void SetWordKPSChart(int wordNum)
  {
    KPSLineChart.RemoveData();
    KPSLineChart.AddSerie(SerieType.Line);
    var perf = TypingSoft.Performance;
    var yVal = TypingSoft.Performance.GetKPSList(wordNum);
    var typedText = perf.GetTypedSentence(wordNum);
    KPSLineChart.xAxis0.splitNumber = typedText.Length;
    KPSLineChart.xAxis0.boundaryGap = false;

    for (int i = 0; i < typedText.Length; ++i)
    {
      KPSLineChart.AddXAxisData(typedText[i].ToString());
      if (i == 0) { KPSLineChart.AddData(0, 0); }
      else { KPSLineChart.AddData(0, yVal[i - 1]); }
    }
  }

  /// <summary>
  /// 簡易リザルトの表示処理
  /// </summary>
  private void SetResult()
  {
    // パフォーマンスの取得
    var perf = TypingSoft.Performance;
    var dataName = GenerateSentence.DataSetName;

    UIAccuracyText.text = perf.GetAccuracy().ToString("0.00") + " %";
    UITimeText.text = perf.GetElapsedTime().ToString("0.00") + " 秒";
    UIWordsetName.text = dataName;

    if (!ConfigScript.IsBeginnerMode)
    {
      var kpsPerf = perf.GetKpmAverageAndStdDev();
      int score = perf.GetNormalScore();
      UIAverageKPS.text = kpsPerf.kpsAvg.ToString("0.00") + " 打/秒";
      UIKPSStdDev.text = kpsPerf.kpsStdDev.ToString("0.00") + " 打/秒";
      UIScoreText.text = score.ToString();
      var isRated = WordsetData.ShortWordsetDict[ConfigScript.DataSetName].IsRated;
      // ランクを表示するワードセットならスコアからランクを割り出す
      if (isRated)
      {
        for (int i = 0; i < RankScore.Count(); ++i)
        {
          if ((ConfigScript.InputMode == (int)ConfigScript.InputType.roman && score >= RankScore[i]) ||
          (ConfigScript.InputMode == (int)ConfigScript.InputType.jisKana && score >= RankScoreJISKana[i]))
          {
            UIRank.text = RankName[i];
            UIRank.fontMaterial = RankFontMaterials[RankFontMaterialNum[i]];
            break;
          }
        }
      }
      else
      {
        UIRank.text = "-----";
        UIRank.fontMaterial = RankFontMaterials[RankFontMaterials.Count() - 1];
      }
    }
  }

  /// <summary>
  /// 簡易リザルトのワードごとの情報表示処理
  /// </summary>
  private void SetWordResult()
  {
    if (ResultStringBuilder.Length == 0)
    {
      var perf = TypingSoft.Performance;
      int len = perf.OriginSentenceList.Count();
      for (int i = 0; i < len; ++i)
      {
        if (perf.IsSentenceInfoValid(i))
        {
          ResultStringBuilder.Append(perf.ConvertDetailResult(i));
        }
      }
    }
    UIResultDetailText.text = ResultStringBuilder.ToString();
  }

  /// <summary>
  /// 簡易リザルト（圧縮表示）の情報表示処理
  /// </summary>
  private void SetWordResultCompressed()
  {
    if (ResultForCaptureStrBuilder.Length == 0)
    {
      var perf = TypingSoft.Performance;
      int len = perf.OriginSentenceList.Count();
      for (int i = 0; i < len; ++i)
      {
        if (perf.IsSentenceInfoValid(i))
        {
          ResultForCaptureStrBuilder.Append(perf.ConvertCompressedDetailResult(i));
        }
      }
    }
    UIResultDetailText.text = ResultForCaptureStrBuilder.ToString();
  }

  /// <summary>
  /// 詳細表示パネルの整備
  /// </summary>
  private void SetDetailResult()
  {
    var perf = TypingSoft.Performance;
    int len = perf.OriginSentenceList.Count();
    WordButtonObjList = new List<Button>();
    for (int i = 0; i < len; ++i)
    {
      if (perf.IsSentenceInfoValid(i))
      {
        // ボタンをリストに追加する
        var wordButton = Instantiate(WordButtonPrefab) as Button;
        wordButton.transform.SetParent(WordButtonList, false);
        var buttonTextComponent = wordButton.transform.Find("WordButtonText").GetComponent<TextMeshProUGUI>();
        var wordNumberStr = (i + 1).ToString();
        var originSentence = perf.OriginSentenceList[i];
        var wordLabel = originSentence.Substring(0, Math.Min(8, originSentence.Length));
        var buttonLabel = $"{wordNumberStr}\n{wordLabel}...";
        buttonTextComponent.text = buttonLabel;
        int n = i;
        wordButton.GetComponent<Button>().onClick.AddListener(() => OnClickWordButton(n));
        WordButtonObjList.Add(wordButton);
      }
    }
    // デフォルトで No.1 の結果を表示させる
    OnClickWordButton(0);
  }

  /// <summary>
  /// 各ワードボタンが押された時の処理
  /// </summary>
  /// <param name="number">0-indexed のボタン番号</param>
  public void OnClickWordButton(int number)
  {
    DetailCurrentWordNum = number;
    var perf = TypingSoft.Performance;
    var typedText = perf.GetTypedSentence(number);
    DetailResultWordNumText.text = $"No.{number + 1}";
    DetailResultWordSentenceText.text = perf.OriginSentenceList[number];
    DetailResultWordReplayText.text = typedText;
    SetWordKPSChart(number);
  }

  /// <summary>
  /// リプレイボタンを押したときの挙動
  /// </summary>
  public void OnClickReplayButton()
  {
    StartCoroutine("DoWordReplay");
  }

  /// <summary>
  /// ワードごとのリプレイ
  /// </summary>
  /// <returns></returns>
  private IEnumerator DoWordReplay()
  {
    // ワード切り替え無効
    foreach (var btn in WordButtonObjList)
    {
      btn.interactable = false;
    }
    // リプレイボタンを終わるまで押せないようにする
    ReplayButton.interactable = false;
    DetailResultWordReplayText.text = "";
    // Wait を計算
    var perf = TypingSoft.Performance;
    var waitTimeList = new List<float>();
    var correctIdxList = new List<int>();
    var typedSentenceList = perf.TypedSentenceList[DetailCurrentWordNum];
    var judgeList = perf.TypeJudgeList[DetailCurrentWordNum];
    var timeList = perf.TypeTimeList[DetailCurrentWordNum];
    if (typedSentenceList.Count() == judgeList.Count())
    {
      double prevTime = -1.0;
      for (int i = 0; i < typedSentenceList.Count(); ++i)
      {
        if (judgeList[i] == 1)
        {
          if (waitTimeList.Count() == 0) { waitTimeList.Add(0.5f); }
          else { waitTimeList.Add((float)(timeList[i] - prevTime)); }
          correctIdxList.Add(i);
          prevTime = timeList[i];
        }
      }
      for (int i = 0; i < waitTimeList.Count(); ++i)
      {
        yield return new WaitForSeconds(waitTimeList[i]);
        DetailResultWordReplayText.text += typedSentenceList[correctIdxList[i]].ToString();
      }
    }
    yield return null;
    // ワード切り替え有効
    foreach (var btn in WordButtonObjList)
    {
      btn.interactable = true;
    }
    // リプレイボタン有効
    ReplayButton.interactable = true;
  }

  /// <summary>
  /// キー入力に対応する処理を実行
  /// <param name="kc">keycode</param>
  /// </summary>
  private void KeyCheck(KeyCode kc)
  {
    // 最優先: スコアとランクの説明パネルが開いている
    if (ScoreInfoPanel.activeSelf)
    {
      if (KeyCode.Escape == kc || KeyCode.H == kc) { OnClickScoreHelpCloseButton(); }
    }
    else if (!ConfigScript.IsBeginnerMode)
    {
      switch (kc)
      {
        case KeyCode.Escape: ReturnConfigScene(); break;
        case KeyCode.F2: Retry(); break;
        case KeyCode.H: OnClickScoreHelpButton(); break;
        case KeyCode.T: OnClickTweetButton(); break;
        case KeyCode.E:
          DefaultToggle.isOn = true;
          DetailToggle.isOn = false;
          break;
        case KeyCode.D:
          DetailToggle.isOn = true;
          DefaultToggle.isOn = false;
          break;
        case KeyCode.C:
          if (DefaultToggle.isOn) { OnClickChangeDisplayButton(); }
          break;
        case KeyCode.R:
          if (DetailToggle.isOn && ReplayButton.interactable) { OnClickReplayButton(); }
          break;
        case KeyCode.K:
        case KeyCode.UpArrow:
          if (0 <= DetailCurrentWordNum - 1 && DetailToggle.isOn && ReplayButton.interactable)
          {
            DetailCurrentWordNum--;
            OnClickWordButton(DetailCurrentWordNum);
          }
          break;
        case KeyCode.J:
        case KeyCode.DownArrow:
          if (DetailCurrentWordNum + 1 < ConfigScript.Tasks && DetailToggle.isOn && ReplayButton.interactable)
          {
            DetailCurrentWordNum++;
            OnClickWordButton(DetailCurrentWordNum);
          }
          break;
      }
    }
    else if (ConfigScript.IsBeginnerMode)
    {
      switch (kc)
      {
        case KeyCode.Escape: ReturnConfigScene(); break;
        case KeyCode.F2: Retry(); break;
      }
    }
  }

  /// <summary>
  /// リトライする
  /// </summary>
  public void Retry()
  {
    if (ConfigScript.IsBeginnerMode) { SceneManager.LoadScene("BeginnerTypingScene"); }
    else { SceneManager.LoadScene("TypingScene"); }
  }

  /// <summary>
  /// 設定画面に戻る
  /// </summary>
  private void ReturnConfigScene()
  {
    if (ConfigScript.IsBeginnerMode) { SceneManager.LoadScene("BeginnerModeScene"); }
    else { SceneManager.LoadScene("SinglePlayConfigScene"); }
  }

  /// <summary>
  /// キャプチャ/通常表示切替ボタンを押したときの挙動
  /// </summary>
  public void OnClickChangeDisplayButton()
  {
    if (ResultDisplayFlag == 0)
    {
      SetWordResultCompressed();
      ResultDisplayFlag = 1;
    }
    else if (ResultDisplayFlag == 1)
    {
      SetWordResult();
      ResultDisplayFlag = 0;
    }
  }

  /// <summary>
  /// 戻るボタンを押したときの挙動
  /// </summary>
  public void OnClickReturnButton()
  {
    ReturnConfigScene();
  }

  /// <summary>
  /// キー入力などのイベント処理
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
  /// ツイートボタンを押したときの処理（現時点で機能自体は非表示）
  /// </summary>
  public void OnClickTweetButton()
  {
    PostTweet();
  }

  /// <summary>
  /// ツイート文章を生成
  /// </summary>
  private void PostTweet()
  {
    var perf = TypingSoft.Performance;
    var kpsText = perf.GetKpmAverageAndStdDev().kpsAvg.ToString("0.00");
    var scoreText = perf.GetNormalScore().ToString();
    var accuracyText = perf.GetAccuracy().ToString("0.00");
    var strBuilder = new StringBuilder();
    var inputTypeName = ConfigScript.InputTypeString[ConfigScript.InputMode];
    strBuilder.Append($"{GenerateSentence.DataSetName} でスコア {scoreText} を出しました。\n");
    strBuilder.Append($"精度: {accuracyText}％ / 平均速度: {kpsText}打/秒 / ワード数: {ConfigScript.Tasks} / 入力方式: {inputTypeName}");
    string url = "https://whitefox-lugh.github.io/FoxTyping/";
    string hashTag = "FoxTyping";
    OpenTweetWindow(strBuilder.ToString(), hashTag, url);
  }

  /// <summary>
  /// スコアについての説明パネルを開く
  /// </summary>
  public void OnClickScoreHelpButton()
  {
    ScoreInfoPanel.SetActive(true);
  }

  /// <summary>
  /// スコアについての説明パネルを閉じる
  /// </summary>
  public void OnClickScoreHelpCloseButton()
  {
    ScoreInfoPanel.SetActive(false);
  }

  [DllImport("__Internal")]
  private static extern void OpenTweetWindow(string text, string hashtags, string url);
}
