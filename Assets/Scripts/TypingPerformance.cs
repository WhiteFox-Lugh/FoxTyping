using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class TypingPerformance
{
  // LB: どんなにミスっても kpm のこの割合はセンテンススコアとして保証する値
  const double PARAM_LB = 0.3;
  // UB: 1文字でもミスった時のセンテンススコアの保証割合上限値
  const double PARAM_UB = 1.0;
  // GRAD: 傾き
  const double PARAM_GRAD = 30;
  // INFL_PT: 変曲点
  const double PARAM_INFL_PT = 0.85;
  // ワード数の設定差分
  const int WORD_UNIT = 5;
  // CONFIDENCE_WORD_NUM: スコア計算でこの値以上のときスコアを保証
  const int CONFIDENCE_WORD_NUM = 20;
  // SCORE_WEIGHT: スコアの重み
  private readonly double[] SCORE_WEIGHT = { 1.5, 1.25, 1, 0.75, 0.5 };
  // 原文のリスト
  public List<string> OriginSentenceList { private set; get; }
  // 実際に打った文のリスト
  public List<string> TypedSentenceList { private set; get; }
  // 正誤判定
  public List<List<int>> TypeJudgeList { private set; get; }
  // タイプした時刻
  public List<List<double>> TypeTimeList { private set; get; }
  // Latency
  public List<double> LatencyList
  {
    private set;
    get;
  }

  /// <summary>
  /// コンストラクター
  /// </summary>
  public TypingPerformance()
  {
    OriginSentenceList = new List<string>();
    TypedSentenceList = new List<string>();
    TypeJudgeList = new List<List<int>>();
    TypeTimeList = new List<List<double>>();
    LatencyList = new List<double>();
  }

  /// <summary>
  /// 原文を追加する
  /// <param name="originSentence">原文</param>
  /// </summary>
  public void AddOriginSentence(string originSentence)
  {
    OriginSentenceList.Add(originSentence);
  }

  /// <summary>
  /// 実際に打った文を追加する
  /// <param name="typeSentence">実際に打つ文章</param>
  /// </summary>
  public void AddTypedSentenceList(string typeSentence)
  {
    TypedSentenceList.Add(typeSentence);
  }

  /// <summary>
  /// 正誤判定のリストを追加する
  /// <param name="judgeList">正誤判定を格納したリスト</param>
  /// </summary>
  public void AddTypeJudgeList(List<int> judgeList)
  {
    TypeJudgeList.Add(judgeList);
  }

  /// <summary>
  /// タイプした時刻のリストを追加する
  /// <param name="typeTimeList">打鍵した時刻</param>
  /// </summary>
  public void AddTypeTimeList(List<double> typeTimeList)
  {
    TypeTimeList.Add(typeTimeList);
  }

  /// <summary>
  /// Latency を追加
  /// <param name="latency">Latency 値</param>
  /// </summary>
  public void AddLatencyTime(double latency)
  {
    LatencyList.Add(latency);
  }

  /// <summary>
  /// num 番目の情報がすべて整合性が取れているかを判定する
  /// <param name="num">確認したいセンテンス番号(0-index)</param>
  /// <returns>情報が整合性が取れていれば true、そうでなければ false</returns>
  /// </summary>
  public bool IsSentenceInfoValid(int num)
  {
    bool isCountValid = (TypedSentenceList[num].Length == TypeJudgeList[num].Count())
                        && (TypeJudgeList[num].Count() == TypeTimeList[num].Count());
    return isCountValid;
  }

  /// <summary>
  /// num 番目 (0-index) のセンテンスの入力時間を取得する
  /// <param name="num">確認したいセンテンス番号(0-index)</param>
  /// <returns>num 番目のセンテンスの入力時間</returns>
  /// </summary>
  public double GetSentenceTypeTime(int num)
  {
    var sentenceTime = TypeTimeList[num][TypeTimeList[num].Count() - 1] - TypeTimeList[num][0];
    if (ConfigScript.IsBeginnerMode)
    {
      sentenceTime += LatencyList[num];
    }
    return sentenceTime;
  }

  /// <summary>
  /// num 番目の Sentence KPM を取得する
  /// <param name="num">確認したいセンテンス番号(0-index)</param>
  /// <returns>num 番目の文章での KPM</returns>
  /// </summary>
  public double GetSentenceKPM(int num)
  {
    return 60.0 * TypeJudgeList[num].Count(judge => judge == 1) / GetSentenceTypeTime(num);
  }

  /// <summary>
  /// num 番目の Sentence の正解タイプ数、ミスタイプ数を取得
  /// <param name="num">確認したいセンテンス番号(0-index)</param>
  /// <returns>num 番目の正解タイプ数とミスタイプ数</returns>
  /// </summary>
  public (int correctTypeNum, int mistypeNum) GetSentenceCorrectAndMistypeNum(int num)
  {
    return (TypeJudgeList[num].Count(judge => judge == 1), TypeJudgeList[num].Count(judge => judge == 0));
  }

  /// <summary>
  /// チャート表示用の KPS の値を取得する
  /// </summary>
  /// <param name="wordNum">0-index でのワード番号</param>
  /// <returns></returns>
  public List<double> GetKPSList(int wordNum)
  {
    var retList = new List<double>();
    var prevTime = -1.0;
    for (int i = 0; i < TypeTimeList[wordNum].Count; ++i)
    {
      if (TypeJudgeList[wordNum][i] == 1)
      {
        if (prevTime < 0)
        {
          prevTime = TypeTimeList[wordNum][i];
        }
        else
        {
          double kps = 1.0 / (TypeTimeList[wordNum][i] - prevTime);
          prevTime = TypeTimeList[wordNum][i];
          retList.Add(kps);
        }
      }
    }
    return retList;
  }

  /// <summary>
  /// num 番目のセンテンスに対してミスタイプを色付けした文を返す
  /// <param name="num">確認したいセンテンス番号(0-index)</param>
  /// <returns>ミスタイプを赤くハイライトした string</returns>
  /// </summary>
  public string GetColoredTypedSentence(int num)
  {
    var sb = new StringBuilder();
    for (int i = 0; i < TypedSentenceList[num].Length; ++i)
    {
      char c = TypedSentenceList[num][i];
      int judge = TypeJudgeList[num][i];
      int prevJudge = (i == 0) ? 1 : TypeJudgeList[num][i - 1];
      if (prevJudge == 1 && judge == 0)
      {
        sb.Append("<color=red>" + c.ToString());
      }
      else if (prevJudge == 0 && judge == 1)
      {
        sb.Append("</color>" + c.ToString());
      }
      else
      {
        sb.Append(c.ToString());
      }
    }
    return sb.ToString();
  }

  /// <summary>
  /// num 番目のセンテンスに対して正解数、ミスタイプ数を文章化
  /// <param name="num">確認したいセンテンス番号(0-index)</param>
  /// <returns>正解数とミスタイプ数を string にしたもの</returns>
  /// </summary>
  private string GetMistypeNumString(int num)
  {
    var sb = new StringBuilder();
    var typeInfo = GetSentenceCorrectAndMistypeNum(num);
    var missCount = typeInfo.mistypeNum.ToString();
    sb.Append($"<size=80%>ミス: {missCount}");
    return sb.ToString();
  }

  /// <summary>
  /// num 番目のセンテンスに対して入力時間、kpm を文章化
  /// <param name="num">確認したいセンテンス番号(0-index)</param>
  /// <returns>num 番目のセンテンスの入力時間と kpm を string にしたもの</returns>
  /// </summary>
  private string GetTimeInfoString(int num)
  {
    var sb = new StringBuilder();
    var typeInfo = GetSentenceCorrectAndMistypeNum(num);
    var typeTime = GetSentenceTypeTime(num).ToString("0.00");
    sb.Append($"<size=80%>タイプ時間: {typeTime}秒");
    if (!ConfigScript.IsBeginnerMode)
    {
      var kpmStr = GetSentenceKPM(num).ToString("0");
      sb.Append($"<size=80%> / KPM: {kpmStr}");
    }
    return sb.ToString();
  }

  /// <summary>
  /// num 番目のセンテンスに対して Latency を文章化
  /// <param name="num">確認したいセンテンス番号(0-index)</param>
  /// <returns>num 番目のセンテンスの Latency を文章化</returns>
  /// </summary>
  private string GetLatencyInfoString(int num)
  {
    var sb = new StringBuilder();
    var latencyInfo = LatencyList[num].ToString("0.000");
    sb.Append($"<size=80%>反応時間: {latencyInfo}秒");
    return sb.ToString();
  }

  /// <summary>
  /// 指定されたワードの正しいタイプ文字だけを抽出する
  /// </summary>
  /// <param name="wordNum">0-indexed のワード番号</param>
  public string GetTypedSentence(int wordNum)
  {
    var typeSentenceList = TypedSentenceList[wordNum];
    var typeJudgeList = TypeJudgeList[wordNum];
    var strBuilder = new StringBuilder();
    if (typeSentenceList.Count() == typeJudgeList.Count())
    {
      for (int i = 0; i < typeSentenceList.Count(); ++i)
      {
        if (typeJudgeList[i] == 1)
        {
          strBuilder.Append(typeSentenceList[i].ToString());
        }
      }
    }
    return strBuilder.ToString();
  }

  /// <summary>
  /// num 番目のセンテンスに対して、リザルト表示用に整形した string を返す
  /// <param name="num">確認したいセンテンス番号(0-index)</param>
  /// <returns>num 番目のセンテンスのタイピング記録を、リザルト表示用に整形した string</returns>
  /// </summary>
  public string ConvertDetailResult(int num)
  {
    var sb = new StringBuilder();
    sb.Append($"<size=100%>{this.OriginSentenceList[num]}\n");
    sb.Append($"<size=100%>{GetColoredTypedSentence(num)}\n");
    sb.Append("<size=50%>-------------------------------------------------------------------------\n");
    sb.Append($"{GetMistypeNumString(num)} / ");
    if (!ConfigScript.IsBeginnerMode)
    {
      sb.Append($"{GetLatencyInfoString(num)} / ");
    }
    sb.Append($"{GetTimeInfoString(num)}\n\n");
    return sb.ToString();
  }

  /// <summary>
  /// num 番目のセンテンスに対して、リザルト表示用に整形した string を返す
  /// <param name="num">確認したいセンテンス番号(0-index)</param>
  /// <returns>num 番目のセンテンスのタイピング記録を、リザルト表示用に整形した string</returns>
  /// </summary>
  public string ConvertCompressedDetailResult(int num)
  {
    var sb = new StringBuilder();
    if (GenerateSentence.Lang.Equals("Japanese"))
    {
      sb.Append($"<size=80%>{num + 1}:{OriginSentenceList[num]} / ");
    }
    else if (GenerateSentence.Lang.Equals("English"))
    {
      sb.Append($"<size=80%>{num + 1}:{GetTypedSentence(num)} / ");
    }
    sb.Append($"{GetMistypeNumString(num)} / ");
    sb.Append($"KPM:{GetSentenceKPM(num).ToString("0")}\n");
    return sb.ToString();
  }

  /// <summary>
  /// 正確さを考慮する関数
  /// <param name="typeInfo">正解数とミスタイプ数の情報</param>
  /// <returns>仕様書に則って計算された値</returns>
  /// </summary>
  private static double FuncAcc((int correct, int miss) typeInfo)
  {
    double accuracy = 1.0 * typeInfo.correct / (typeInfo.correct + typeInfo.miss);
    double ret;
    if (accuracy >= 0.999)
    {
      ret = 1.0;
    }
    else
    {
      ret = PARAM_LB + 1.0 * (PARAM_UB - PARAM_LB) / (1.0 + Math.Exp(-PARAM_GRAD * (accuracy - PARAM_INFL_PT)));
    }
    return ret;
  }

  /// <summary>
  /// 単文でのスコア換算
  /// <param name="num">確認したいセンテンス番号(0-index)</param>
  /// <returns>num 番目のセンテンス単体のスコア</returns>
  /// </summary>
  private double GetSentenceScore(int num)
  {
    double score = GetSentenceKPM(num) * FuncAcc(GetSentenceCorrectAndMistypeNum(num));
    return score;
  }


  /// <summary>
  /// センテンスごとにスコアを低い順にソートしたものを返す
  /// <returns>ソートされたセンテンススコアのリスト</returns>
  /// </summary>
  private List<double> GetSortedScoreList()
  {
    var sentenceScoreList = new List<double>();
    for (int i = 0; i < OriginSentenceList.Count(); ++i)
    {
      sentenceScoreList.Add(GetSentenceScore(i));
    }
    sentenceScoreList.Sort();
    sentenceScoreList.Reverse();
    return sentenceScoreList;
  }

  /// <summary>
  /// 練習した文章全体でのスコアを計算する
  /// <returns>スコア</returns>
  /// </summary>
  public int GetNormalScore()
  {
    var sortedScoreList = GetSortedScoreList();
    double numerator = 0.0;
    // 指定ワード数未満の場合は 0 点を補完
    if (sortedScoreList.Count() < CONFIDENCE_WORD_NUM)
    {
      while (sortedScoreList.Count() < CONFIDENCE_WORD_NUM)
      {
        sortedScoreList.Add(0);
      }
    }
    var len = sortedScoreList.Count();
    // memo: len は 5の倍数
    for (int i = 0; i < len; ++i)
    {
      numerator += SCORE_WEIGHT[i / (len / WORD_UNIT)] * sortedScoreList[i];
    }
    int ret = Convert.ToInt32(Math.Floor(numerator / len));
    return ret;
  }

  /// <summary>
  /// kpm の平均と標準偏差を求める
  /// <returns>(kpm 平均, kpm 標準偏差)</returns>
  /// </summary>
  public (double kpsAvg, double kpsStdDev) GetKpmAverageAndStdDev()
  {
    var len = OriginSentenceList.Count();
    var kpsList = new List<double>();
    for (int i = 0; i < len; ++i)
    {
      kpsList.Add(GetSentenceKPM(i) / 60.0);
    }
    var avg = kpsList.Average();
    var sumPower = kpsList.Select(x => x * x).Sum();
    var variance = sumPower / kpsList.Count - avg * avg;
    var stdDev = Math.Sqrt(variance);
    return (avg, stdDev);
  }

  /// <summary>
  /// 全センテンス打ち終わるまでの経過時間を返す
  /// <returns>全センテンス打ち終わるまでの経過時間</returns>
  /// </summary>
  public double GetElapsedTime()
  {
    double ret = 0.0;
    for (int i = 0; i < TypeTimeList.Count(); ++i)
    {
      ret += GetSentenceTypeTime(i);
    }
    return ret;
  }

  /// <summary>
  /// 全センテンスにおける精度を返す
  /// <returns>全センテンス総合した打鍵の精度</returns>
  /// </summary>
  public double GetAccuracy()
  {
    double ret = 0.0;
    int correctTypeSum = 0;
    int mistypeSum = 0;
    for (int i = 0; i < TypeJudgeList.Count(); ++i)
    {
      var num = GetSentenceCorrectAndMistypeNum(i);
      correctTypeSum += num.correctTypeNum;
      mistypeSum += num.mistypeNum;
    }
    ret = 100.0 * correctTypeSum / (correctTypeSum + mistypeSum);
    return ret;
  }
}
