using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;

public class TypingPerformance {
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
	/// num 番目 (0-index) のセンテンスの入力時間を取得する
	/// </summary>
	private double GetSentenceTypeTime(int num){
		return TypeTimeList[num][TypeTimeList[num].Count() - 1] - TypeTimeList[num][0];
	}

	/// <summary>
	/// num 番目の Sentence KPM を取得する
	/// </summary>
	private double GetSentenceKPM(int num){
		return 60.0 * TypeJudgeList[num].Count(judge => judge == 1) / GetSentenceTypeTime(num);
	}

	/// <summary>
	/// num 番目の Sentence の正解タイプ数、ミスタイプ数を取得
	/// </summary>
	private (int correctTypeNum, int mistypeNum) GetSentenceCorrectAndMistypeNum(int num){
		return (TypeJudgeList[num].Count(judge => judge == 1), TypeJudgeList[num].Count(judge => judge == 0));
	}

	/// <summary>
	/// num 番目のセンテンスに対してミスタイプを色付けした文を返す
	/// </summary>
	private string GetColoredTypedSentence(int num) {
		var sb = new StringBuilder();
		for (int i = 0; i < TypedSentenceList[num].Length; ++i){
			char c = TypedSentenceList[num][i];
			int judge = TypeJudgeList[num][i];
			sb.Append((judge == 1) ? c.ToString() : ("<color=red>" + c.ToString() + "</color>"));
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
		sb.Append("Time: ").Append(GetSentenceTypeTime(num).ToString("0.000"))
			.Append(" / Sentence KPM: ").Append(GetSentenceKPM(num).ToString("0.000"));
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
}
