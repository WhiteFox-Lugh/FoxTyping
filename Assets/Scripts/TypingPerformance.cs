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
	const double BETA = 0.75;
	const int PARAM_S = 10;
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
	/// </summary>
	public void AddOriginSentence(string str) {
		OriginSentenceList.Add(str);
	}

	/// <summary>
	/// 実際に打った文を追加する
	/// </summary>
	public void AddTypedSentenceList(string str) {
		TypedSentenceList.Add(str);
	}

	/// <summary>
	/// 正誤判定のリストを追加する
	/// </summary>
	public void AddTypeJudgeList(List<int> lst) {
		TypeJudgeList.Add(lst);
	}

	/// <summary>
	/// タイプした時刻のリストを追加する
	/// </summary>
	public void AddTypeTimeList(List<double> lst) {
		TypeTimeList.Add(lst);
	}

	/// <summary>
	/// n 番目の情報がすべて整合性が取れているかを判定する
	/// </summary>
	public bool isSentenceInfoValid(int i) {
		bool isCountValid = (TypedSentenceList[i].Length == TypeJudgeList[i].Count())
												&& (TypeJudgeList[i].Count() == TypeTimeList[i].Count());
		return isCountValid;
	}

	/// <summary>
	/// num 番目 (0-index) のセンテンスの入力時間を取得する
	/// </summary>
	public double GetSentenceTypeTime(int num){
		return TypeTimeList[num][TypeTimeList[num].Count() - 1] - TypeTimeList[num][0];
	}

	/// <summary>
	/// num 番目の Sentence KPM を取得する
	/// </summary>
	public double GetSentenceKPM(int num){
		return 60.0 * TypeJudgeList[num].Count(judge => judge == 1) / GetSentenceTypeTime(num);
	}

	/// <summary>
	/// num 番目の Sentence の正解タイプ数、ミスタイプ数を取得
	/// </summary>
	public (int correctTypeNum, int mistypeNum) GetSentenceCorrectAndMistypeNum(int num){
		return (TypeJudgeList[num].Count(judge => judge == 1), TypeJudgeList[num].Count(judge => judge == 0));
	}

	/// <summary>
	/// num 番目のセンテンスに対してミスタイプを色付けした文を返す
	/// </summary>
	public string GetColoredTypedSentence(int num) {
		var sb = new StringBuilder();
		Debug.Log(TypedSentenceList[num]);
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
	/// 正確さを倍率に換算する関数
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
	/// </summary>
	private double GetSentenceScore(int num){
		return GetSentenceKPM(num) * FuncAcc(GetSentenceCorrectAndMistypeNum(num));
	}


	/// <summary>
	/// センテンスごと音スコアを低い順にソートしたものを返す
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
	/// スコアを計算する
	/// </summary>
	public int GetNormalScore() {
		var sortedScoreList = GetSortedScoreList();
		var len = OriginSentenceList.Count();
		double numerator = 0.0;
		double denominator = 0.0;
		double weight = BETA;
		for (int i = 0; i < len; ++i){
			numerator += sortedScoreList[i] * weight;
			denominator += weight;
			weight *= BETA;
		}
		int ret = Convert.ToInt32(Math.Floor(numerator / denominator));
		return ret;
	}

	/// <summary>
	/// Fox スコア（上級者向け）を計算する
	/// </summary>
	public int GetFoxScore() {
		var sortedScoreList = GetSortedScoreList();
		var len = OriginSentenceList.Count();
		int ignoreNum = len / PARAM_S;
		double score = 0.0;
		for (int i = ignoreNum; i < len - ignoreNum; ++i){
			score += sortedScoreList[i];
		}
		int ret = Convert.ToInt32(Math.Floor(score / (len - 2 * ignoreNum)));
		return ret;
	}

	/// <summary>
	/// 全センテンス打ち終わるまでの経過時間を返す
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

	/// <summary>
	/// 全センテンスにおける kps を返す
	/// </summary>
	public double GetKPSAll() {
		int correctTypeCount = 0;
		double elapsedTime = GetElapsedTime();
		for (int i = 0; i < OriginSentenceList.Count(); ++i){
			var typeNum = GetSentenceCorrectAndMistypeNum(i);
			correctTypeCount += typeNum.correctTypeNum;
		}
		double kps = correctTypeCount / elapsedTime;
		return kps;
	}
}
