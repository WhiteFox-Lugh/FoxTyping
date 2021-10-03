using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;
using UnityEngine;

public class TypingPerformance {
	// ALPHA: 正確さから倍率への関数の定数
	const double ALPHA = 0.25;
	// BETA: ノーマルスコアの加重平均の重み
	const double BETA = 0.85;
	// CONFIDENCE_WORD_NUM: スコア計算でこの値以上のときスコアを保証
	const int CONFIDENCE_WORD_NUM = 30;
	// 原文のリスト
	public List<string> OriginSentenceList {
		private set;
		get;
	}
	// 実際に打った文のリスト
	public List<string> TypedSentenceList {
		private set;
		get;
	}
	// 正誤判定
	public List<List<int>> TypeJudgeList {
		private set;
		get;
	}
	// タイプした時刻
	public List<List<double>> TypeTimeList {
		private set;
		get;
	}

	/// <summary>
	/// コンストラクター
	/// </summary>
	public TypingPerformance() {
		OriginSentenceList = new List<string>();
		TypedSentenceList = new List<string>();
		TypeJudgeList = new List<List<int>>();
		TypeTimeList = new List<List<double>>();
	}

	/// <summary>
	/// 原文を追加する
	/// <param name="originSentence">原文</param>
	/// </summary>
	public void AddOriginSentence(string originSentence) {
		OriginSentenceList.Add(originSentence);
	}

	/// <summary>
	/// 実際に打った文を追加する
	/// <param name="typeSentence">実際に打つ文章</param>
	/// </summary>
	public void AddTypedSentenceList(string typeSentence) {
		TypedSentenceList.Add(typeSentence);
	}

	/// <summary>
	/// 正誤判定のリストを追加する
	/// <param name="judgeList">正誤判定を格納したリスト</param>
	/// </summary>
	public void AddTypeJudgeList(List<int> judgeList) {
		TypeJudgeList.Add(judgeList);
	}

	/// <summary>
	/// タイプした時刻のリストを追加する
	/// <paran name="typeTimeList">打鍵した時刻</param>
	/// </summary>
	public void AddTypeTimeList(List<double> typeTimeList) {
		TypeTimeList.Add(typeTimeList);
	}

	/// <summary>
	/// num 番目の情報がすべて整合性が取れているかを判定する
	/// <param name="num">確認したいセンテンス番号(0-index)</param>
	/// <returns>情報が整合性が取れていれば true、そうでなければ false</returns>
	/// </summary>
	public bool isSentenceInfoValid(int num) {
		bool isCountValid = (TypedSentenceList[num].Length == TypeJudgeList[num].Count())
												&& (TypeJudgeList[num].Count() == TypeTimeList[num].Count());
		return isCountValid;
	}

	/// <summary>
	/// num 番目 (0-index) のセンテンスの入力時間を取得する
	/// <param name="num">確認したいセンテンス番号(0-index)</param>
	/// <returns>num 番目のセンテンスの入力時間</returns>
	/// </summary>
	public double GetSentenceTypeTime(int num){
		return TypeTimeList[num][TypeTimeList[num].Count() - 1] - TypeTimeList[num][0];
	}

	/// <summary>
	/// num 番目の Sentence KPM を取得する
	/// <param name="num">確認したいセンテンス番号(0-index)</param>
	/// <returns>num 番目の文章での KPM</returns>
	/// </summary>
	public double GetSentenceKPM(int num){
		return 60.0 * TypeJudgeList[num].Count(judge => judge == 1) / GetSentenceTypeTime(num);
	}

	/// <summary>
	/// num 番目の Sentence の正解タイプ数、ミスタイプ数を取得
	/// <param name="num">確認したいセンテンス番号(0-index)</param>
	/// <returns>num 番目の正解タイプ数とミスタイプ数</returns>
	/// </summary>
	public (int correctTypeNum, int mistypeNum) GetSentenceCorrectAndMistypeNum(int num){
		return (TypeJudgeList[num].Count(judge => judge == 1), TypeJudgeList[num].Count(judge => judge == 0));
	}

	/// <summary>
	/// num 番目のセンテンスに対してミスタイプを色付けした文を返す
	/// <param name="num">確認したいセンテンス番号(0-index)</param>
	/// <returns>ミスタイプを赤くハイライトした string</returns>
	/// </summary>
	public string GetColoredTypedSentence(int num) {
		var sb = new StringBuilder();
		for (int i = 0; i < TypedSentenceList[num].Length; ++i){
			char c = TypedSentenceList[num][i];
			int judge = TypeJudgeList[num][i];
			int prevJudge = (i == 0) ? 1 : TypeJudgeList[num][i - 1];
			if (prevJudge == 1 && judge == 0) {
				sb.Append("<color=red>" + c.ToString());
			}
			else if(prevJudge == 0 && judge == 1){
				sb.Append("</color>" + c.ToString());
			}
			else {
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
	private string GetCorrectAndMistypeNumString(int num) {
		var sb = new StringBuilder();
		var typeInfo = GetSentenceCorrectAndMistypeNum(num);
		sb.Append("Correct: ").Append(typeInfo.correctTypeNum.ToString())
			.Append(" / Mistype: ").Append(typeInfo.mistypeNum.ToString());
		return sb.ToString();
	}

	/// <summary>
	/// num 番目のセンテンスに対して入力時間、kpm を文章化
	/// <param name="num">確認したいセンテンス番号(0-index)</param>
	/// <returns>num 番目のセンテンスの入力時間と kpm を string にしたもの</returns>
	/// </summary>
	private string GetTimeInfoString(int num) {
		var sb = new StringBuilder();
		var typeInfo = GetSentenceCorrectAndMistypeNum(num);
		sb.Append("Time: ").Append(GetSentenceTypeTime(num).ToString("0.00"))
			.Append(" / Sentence KPM: ").Append(GetSentenceKPM(num).ToString("0"));
		return sb.ToString();
	}

	/// <summary>
	/// num 番目のセンテンスに対して、リザルト表示用に整形した string を返す
	/// <param name="num">確認したいセンテンス番号(0-index)</param>
	/// <returns>num 番目のセンテンスのタイピング記録を、リザルト表示用に整形した string</returns>
	/// </summary>
	public string ConvertDetailResult(int num) {
		var sb = new StringBuilder();
		sb.Append(this.OriginSentenceList[num]).Append("\n");
		sb.Append(GetColoredTypedSentence(num)).Append("\n");
		sb.Append(GetCorrectAndMistypeNumString(num)).Append("\n");
		sb.Append(GetTimeInfoString(num)).Append("\n\n");
		return sb.ToString();
	}

	/// <summary>
	/// 正確さを考慮する関数
	/// <param name="typeInfo">正解数とミスタイプ数の情報</param>
	/// <returns>仕様書に則って計算された値</returns>
	/// </summary>
	private static double FuncAcc((int correct, int miss) typeInfo){
		double accuracy = 100.0 * typeInfo.correct / (typeInfo.correct + typeInfo.miss);
		double ret;
		if (accuracy >= 99.99){
			ret = 1.0;
		}
		else if (accuracy >= 95.0){
			ret = ALPHA * 1.0 / (1.0 + Math.Exp(-(accuracy - 95.0))) + (1.0 - ALPHA);
		}
		else {
			ret = (1.0 - ALPHA / 2.0) * Math.Exp((accuracy - 95.0) * ALPHA / (4.0 - 2.0 * ALPHA));
		}
		return ret;
	}

	/// <summary>
	/// 単文でのスコア換算
	/// <param name="num">確認したいセンテンス番号(0-index)</param>
	/// <returns>num 番目のセンテンス単体のスコア</returns>
	/// </summary>
	private double GetSentenceScore(int num){
		return GetSentenceKPM(num) * FuncAcc(GetSentenceCorrectAndMistypeNum(num));
	}


	/// <summary>
	/// センテンスごとにスコアを低い順にソートしたものを返す
	/// <returns>ソートされたセンテンススコアのリスト</returns>
	/// </summary>
	private List<double> GetSortedScoreList() {
		var sentenceScoreList = new List<double>();
		for (int i = 0; i < OriginSentenceList.Count(); ++i){
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
	public int GetNormalScore() {
		var sortedScoreList = GetSortedScoreList();
		var len = sortedScoreList.Count();
		double numerator = 0.0;
		double denominator = 0.0;
		double weight = BETA;
		// 指定ワード数以上かそれ未満で使う部分を変更
		if (len > CONFIDENCE_WORD_NUM){
			var startIndex = (len - CONFIDENCE_WORD_NUM) / 5;
			sortedScoreList = sortedScoreList.GetRange(startIndex, CONFIDENCE_WORD_NUM);
		}
		// 30ワード未満は0ptを補完
		else if (len < CONFIDENCE_WORD_NUM){
			while (sortedScoreList.Count() < CONFIDENCE_WORD_NUM){
				sortedScoreList.Add(0);
			}
		}
		for (int i = 0; i < CONFIDENCE_WORD_NUM; ++i){
			numerator += sortedScoreList[i] * weight;
			denominator += weight;
			weight *= BETA;
		}
		int ret = Convert.ToInt32(Math.Floor(numerator / denominator));
		return ret;
	}

	/// <summary>
	/// kpm の平均と標準偏差を求める
	/// <returns>(kpm 平均, kpm 標準偏差)</returns>
	/// </summary>
	public (double kpsAvg, double kpsStdDev) GetKpmAverageAndStdDev() {
		var len = OriginSentenceList.Count();
		var kpsList = new List<double>();
		for (int i = 0; i < len; ++i){
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
	public double GetElapsedTime() {
		double ret = 0.0;
		for (int i = 0; i < TypeTimeList.Count(); ++i){
			ret += GetSentenceTypeTime(i);
		}
		return ret;
	}

	/// <summary>
	/// 全センテンスにおける精度を返す
	/// <returns>全センテンス総合した打鍵の精度</returns>
	/// </summary>
	public double GetAccuracy() {
		double ret = 0.0;
		int correctTypeSum = 0;
		int mistypeSum = 0;
		for (int i = 0; i < TypeJudgeList.Count(); ++i){
			var num = GetSentenceCorrectAndMistypeNum(i);
			correctTypeSum += num.correctTypeNum;
			mistypeSum += num.mistypeNum;
		}
		ret = 100.0 * correctTypeSum / (correctTypeSum + mistypeSum);
		return ret;
	}
}
