using System.Diagnostics;
using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RecordSceneScript : MonoBehaviour
{
  [SerializeField] Text UIResultDetailText;
  [SerializeField] TextMeshProUGUI UIAverageKPS;
  [SerializeField] TextMeshProUGUI UIKPSStdDev;
  [SerializeField] TextMeshProUGUI UIScoreText;
  [SerializeField] TextMeshProUGUI UITimeText;
  [SerializeField] TextMeshProUGUI UIAccuracyText;
  [SerializeField] TextMeshProUGUI UIRank;
  [SerializeField] GameObject ScoreInfoPanel;
  [SerializeField] Material[] RankFontMaterials;
  private int[] RankScore = new int[20] {
    1000, 950, 900, 850, 800, 750, 700, 650, 600, 550,
    500, 450, 400, 350, 300, 250, 200, 150, 100, 0
  };

  private string[] RankName = new string[20] {
    "Legend", "GrandMaster 1", "GrandMaster 2", "GrandMaster 3", "Master 1",
    "Master 2", "Master 3", "S1", "S2", "S3",
    "A1", "A2", "A3", "B1", "B2",
    "B3", "C1", "C2", "C3", "C4"
  };

  private int[] RankFontMaterialNum = new int[20] {
    0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 5, 6, 6, 6, 6
  };

  /// <summary>
  /// 初期化など
  /// </summary>
  void Awake()
  {
    ScoreInfoPanel.SetActive(false);
    SetResult();
    SetResultDetail();
  }

  /// <summary>
  /// 1フレームごとの更新処理。現時点ではなし
  /// </summary>
  void Update()
  {
  }

  /// <summary>
  /// 簡易リザルトの表示処理
  /// </summary>
  private void SetResult()
  {
    var perf = TypingSoft.Performance;
    var kpsPerf = perf.GetKpmAverageAndStdDev();
    int score = perf.GetNormalScore();
    UIAverageKPS.text = kpsPerf.kpsAvg.ToString("0.000") + " 打/秒";
    UIKPSStdDev.text = kpsPerf.kpsStdDev.ToString("0.000") + " 打/秒";
    UIScoreText.text = score.ToString();
    UITimeText.text = perf.GetElapsedTime().ToString("0.000") + " 秒";
    UIAccuracyText.text = perf.GetAccuracy().ToString("0.000") + " %";
    for (int i = 0; i < RankScore.Count(); ++i)
    {
      if (score >= RankScore[i])
      {
        UIRank.text = RankName[i];
        UIRank.fontMaterial = RankFontMaterials[RankFontMaterialNum[i]];
        break;
      }
    }
  }

  /// <summary>
  /// 詳細リザルトの表示処理
  /// </summary>
  private void SetResultDetail()
  {
    var sb = new StringBuilder();
    var perf = TypingSoft.Performance;
    int len = perf.OriginSentenceList.Count();
    for (int i = 0; i < len; ++i)
    {
      if (perf.isSentenceInfoValid(i))
      {
        sb.Append(perf.ConvertDetailResult(i));
      }
    }
    UIResultDetailText.text = sb.ToString();
  }

  /// <summary>
  /// キー入力に対応する処理を実行
  /// <param name="kc">keycode</param>
  /// </summary>
  private void KeyCheck(KeyCode kc)
  {
    if (KeyCode.Backspace == kc)
    {
      ReturnConfigScene();
    }
    else if (KeyCode.F2 == kc)
    {
      Retry();
    }
  }

  /// <summary>
  /// リトライする
  /// </summary>
  public void Retry()
  {
    SceneManager.LoadScene("TypingScene");
  }

  /// <summary>
  /// 設定画面に戻る
  /// </summary>
  public void ReturnConfigScene()
  {
    SceneManager.LoadScene("SinglePlayConfigScene");
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
    var tweetText = $"FoxTyping でスコア {scoreText} を出しました。\n精度: {accuracyText}％ / 平均速度: {kpsText}打/秒";
    string url = "https://whitefox-lugh.github.io/FoxTyping/";
    string hashTag = "FoxTyping";
    OpenTweetWindow(tweetText, hashTag, url);
  }

  /// <summary>
  /// スコア横の ? ボタンを押したときにヘルプを開く
  /// </summary>
  public void OnClickScoreHelpButton()
  {
    ScoreInfoPanel.SetActive(true);
  }

  /// <summary>
  /// スコア横の ? ボタンを押したときにヘルプを開く
  /// </summary>
  public void OnClickScoreHelpCloseButton()
  {
    ScoreInfoPanel.SetActive(false);
  }

  [DllImport("__Internal")]
  private static extern void OpenTweetWindow(string text, string hashtags, string url);
}
