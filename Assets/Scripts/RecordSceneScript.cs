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

  /// <summary>
  /// 初期化など
  /// </summary>
  void Awake()
  {
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
    UIAverageKPS.text = kpsPerf.kpsAvg.ToString("0.00") + " 打/秒";
    UIKPSStdDev.text = kpsPerf.kpsStdDev.ToString("0.00") + " 打/秒";
    UIScoreText.text = perf.GetNormalScore().ToString();
    UITimeText.text = perf.GetElapsedTime().ToString("0.000") + " 秒";
    UIAccuracyText.text = perf.GetAccuracy().ToString("0.00") + " %";
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
    var tweetText = $"FoxTyping でスコア{scoreText}を出しました。\n精度: {accuracyText}% / 平均速度: {kpsText}打/秒";
    string url = "https://whitefox-lugh.github.io/FoxTyping/";
    string hashTag = "FoxTyping";
    OpenTweetWindow(tweetText, hashTag, url);
  }

  [DllImport("__Internal")]
  private static extern void OpenTweetWindow(string text, string hashtags, string url);
}
