﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TypingSoft : MonoBehaviour {
	private const double INTERVAL = 2.0F;
	private const int KPMDIFF = 20;
	private const int INITKPM = 300;
	private const int SAFETY = 100;
	private const int WARNING = 50;
	GenerateSentence gs = new GenerateSentence();
	// 入力された文字の queue
	private Queue<char> queue = new Queue<char>();
	// 時刻の queue
	private Queue<double> timeQueue = new Queue<double>();
	// ひらがなとタイプのマッピング
	// UI たち
	private Text UIJ;
	private Text UIR;
	private Text UII;
	private Text UIKPM;
	private Text UISTT;
	private Text UIcorrectA;
	private Text UIAccuracy;
	private Text UITypeInfo;
	// 問題表示関連
	private string nQJ;
	private string nQR;
	// これまで打った文字列
	private string correctString;
	// ミスタイプした文字の記録
	private bool isRecMistype;
	private string misTypeLetter;
	// 文章の読み
	private List<string> sentenceHiragana;
	// 文章タイピング読み
	private List<List<string>> sentenceTyping;
	// index 類
	private int index;
	private List<List<int>> indexAdd = new List<List<int>>();
	private List<List<int>> sentenceIndex = new List<List<int>>();
	private List<List<int>> sentenceValid = new List<List<int>>();
	// 時間計測関連
	private bool isFirstInput;
	private double lastUpdateTime;
	private double firstCharInputTime;
	private double lastJudgeTime;
	// 問題数関連
	private int tasksCompleted;
	// 色
	private Color colorGray = new Color(128f / 255f, 128f / 255f, 128f / 255f, 0.6f);
	private Color colorBrown = new Color(80f / 255f, 40f / 255f, 40f / 255f, 1f);
	private Color colorBlack = new Color(0f / 255f, 0f / 255f, 0f / 255f, 1f);
	private Color colorPurple = new Color(148f / 255f, 16f / 255f, 218f / 255f, 1f);
	// Lunatic mode 専用
	private bool isInputValid;
	private int sectionLength;

	// ゲームモード
	public static int GameMode {
		private set;
		get;
	}
	// 正解タイプ数
	public static int CorrectTypeNum {
		private set;
		get;
	}
	// kpm
	public static double Kpm {
		private set;
		get;
	}
	// ミスタイプ
	public static int MisTypeNum {
		private set;
		get;
	}
	// 正解率
	public static double Accuracy {
		private set;
		get;
	}
	// 合計時間
	public static double TotalTypingTime {
		private set;
		get;
	}
	// ミスタイプの記録
	public static Dictionary<string, int> MisTypeDictionary {
		private set;
		get;
	}
	// 文章数
	public static int Tasks {
		set;
		get;
	}
	// Lunatic 用
	public static int AcheivedKPM {
		private set;
		get;
	}

	// 初期化関連
	void Awake () {
		GetUI();
		InitGame();
	}

	void Update () {
		TextColorChange();
		if (queue.Count > 0 && timeQueue.Count > 0){
			if(queue.Count != timeQueue.Count){
				SceneManager.LoadScene("SinglePlayConfigScene");
			}
			Check();
		}
	}

	void TextColorChange(){
		double currentTime = Time.realtimeSinceStartup;
		if(isFirstInput && Math.Abs(lastUpdateTime - Time.realtimeSinceStartup) <= INTERVAL){
			UIJ.color = colorBlack;
		}
		else {
			UIJ.color = colorBrown;
		}
	}

	void GetUI(){
		UIJ = transform.Find("InputPanel/QuestionJ").GetComponent<Text>();
		UIR = transform.Find("InputPanel/QuestionR").GetComponent<Text>();
		UII = transform.Find("InputPanel/Input").GetComponent<Text>();
		UIKPM = transform.Find("DataPanel/KeyPerMinute").GetComponent<Text>();
		UISTT = transform.Find("DataPanel/SentenceTypeTime").GetComponent<Text>();
		UIcorrectA = transform.Find("DataPanel/CorrectAnswer").GetComponent<Text>();
		UITypeInfo = transform.Find("DataPanel/TypeInfo").GetComponent<Text>();
		UIAccuracy = transform.Find("DataPanel/CorrectRate").GetComponent<Text>();
	}

	void InitGame(){
		InitData();
		InitText();
		OutputQ();
	}

	void InitText(){
		// テキスト関連の初期化
		UITypeInfo.text = "Correct : - / Mistype: - ";
		UIAccuracy.text = "Accuracy : --.-- %";
		UIKPM.text = "Speed : --- kpm ";
		UIKPM.text += "\n[Sentence: --- kpm]";
		UISTT.text = "Time : --:--";
		UIcorrectA.text = "Tasks : - / - ";
	}

	void InitData(){
		// json
		gs.LoadSentenceData(ConfigScript.DataSetName);
		// データ関連の初期化
		CorrectTypeNum = 0;
		MisTypeNum = 0;
		TotalTypingTime = 0.0;
		Kpm = 0.0;
		Accuracy = 0.0;
		AcheivedKPM = 0;
		tasksCompleted = 0;
		isRecMistype = false;
		lastJudgeTime = -1.0;
		GameMode = ConfigScript.GameMode;
		Tasks = ConfigScript.Tasks;
		isInputValid = true;
		MisTypeDictionary = new Dictionary<string, int>();
		queue.Clear();
	}

	void ReturnConfig(){
		SceneManager.LoadScene("SinglePlayConfigScene");
	}

	void finished(){
		SceneManager.LoadScene("ResultScene");
	}

	//　新しい問題を表示するメソッド
	void OutputQ() {
		//　テキストUIを初期化する
		UIJ.text = "";
		UIR.text = "";
		UII.text = "";
		//　正解した文字列を初期化
		correctString = "";
		// 変数等の初期化
		isFirstInput = true;
		index = 0;
		sectionLength = 0;
		//　問題文生成
		ChangeSentence();
		UIcorrectA.text = "Tasks : " + tasksCompleted.ToString() + " / " + Tasks.ToString();
		// 時刻を取得
		lastUpdateTime = Time.realtimeSinceStartup;
	}

	void InitSentenceData (){
		var sLength = sentenceTyping.Count;
		sentenceIndex.Clear();
		sentenceValid.Clear();
		indexAdd.Clear();
		sentenceIndex = new List<List<int>>();
		sentenceValid = new List<List<int>>();
		indexAdd = new List<List<int>>();
		for (int i = 0; i < sLength; ++i){
			var typeNum = sentenceTyping[i].Count;
			sentenceIndex.Add(new List<int>());
			sentenceValid.Add(new List<int>());
			indexAdd.Add(new List<int>());
			for (int j = 0; j < typeNum; ++j){
				sentenceIndex[i].Add(0);
				sentenceValid[i].Add(1);
				indexAdd[i].Add(0);
			}
		}
	}

	void ChangeSentence (){
		var t = gs.Generate(GameMode);
		nQJ = t.originSentence;
		nQR = t.typeSentence;
		sentenceHiragana = t.hiSep;
		Debug.Log(nQJ);
		sentenceTyping = t.ty;
		// いろいろ初期化
		InitSentenceData();
		for (int i = 0; i < sentenceTyping.Count; ++i){
			UII.text += sentenceTyping[i][0];
		}
		// テキスト変更
		UIJ.text = nQJ;
		UIR.text = nQR;
	}

	void OnGUI() {
    Event e = Event.current;
		var isPushedShiftKey = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    if (isInputValid && e.type == EventType.KeyDown && e.keyCode != KeyCode.None
		&& !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2)){
			var inputChar = ConvertKeyCodeToChar(e.keyCode, isPushedShiftKey);
			if (isFirstInput){
				firstCharInputTime = Time.realtimeSinceStartup;
				isFirstInput = false;
			}
			queue.Enqueue(inputChar);
			timeQueue.Enqueue(Time.realtimeSinceStartup);
		}
  }

	// // ひらがな文の文字をKeyCodeに変換するための関数
	// KeyCode GetKeycode(char c){
	// 	// 数字
	// 	if('0' - '0' <= c - '0' && c - '0' <= '9' - '0'){
	// 		return (KeyCode)System.Enum.Parse(typeof(KeyCode), "Alpha" + c.ToString());
	// 	}
	// 	// アルファベット
	// 	else if('a' - 'a' <= c - 'a' && c - 'a' <= 'z' - 'a'){
	// 		return (KeyCode)System.Enum.Parse(typeof(KeyCode), c.ToString().ToUpper());
	// 	}
	// 	// 以下記号
	// 	switch(c){
	// 		case '.':
	// 			return (KeyCode)System.Enum.Parse(typeof(KeyCode), "Period");
	// 		case ',':
	// 			return (KeyCode)System.Enum.Parse(typeof(KeyCode), "Comma");
	// 		case '-':
	// 			return (KeyCode)System.Enum.Parse(typeof(KeyCode), "Minus");
	// 		case ';':
	// 			return (KeyCode)System.Enum.Parse(typeof(KeyCode), "Semicoron");
	// 		case ':':
	// 			return (KeyCode)System.Enum.Parse(typeof(KeyCode), "Coron");
	// 	}
	// 	return KeyCode.None;
	// }

	// タイピングチェック
	void Check(){
		while(queue.Count > 0){
			// queue に入ってる keycode を取得
			char inputChar = queue.Peek();
			queue.Dequeue();
			double keyDownTime = timeQueue.Peek();
			timeQueue.Dequeue();
			if(keyDownTime <= lastJudgeTime){
				continue;
			}
			// まだ可能性のあるセンテンス全てに対してミスタイプかチェックする
			bool isMistype = true;
			string str = "";
			// Esc なら Config 画面に戻る
			if (inputChar == ConvertKeyCodeToChar(KeyCode.Escape, false)){
				ReturnConfig();
				break;
			}
			// 全ての valid なセンテンスに対してチェックする
			for (int i = 0; i < sentenceTyping[index].Count; ++i){
				// invalid ならパス
				if(0 == sentenceValid[index][i]){
					continue;
				}
				int j = sentenceIndex[index][i];
				char nextInputChar = sentenceTyping[index][i][j];
				// 正解タイプ
				if(inputChar == nextInputChar){
					isMistype = false;
					indexAdd[index][i] = 1;
					str = sentenceTyping[index][i][j].ToString();
				}
				else {
					indexAdd[index][i] = 0;
				}
			}
			if(!isMistype){
				Correct(str);
			}
			else {
				Mistype();
			}
		}
	}

	//　タイピング正解時の処理
	void Correct(string str) {
		//　正解数を増やす
		CorrectTypeNum++;
		UITypeInfo.text = "Correct : " + CorrectTypeNum.ToString() + " / Mistype : " + MisTypeNum.ToString();
		sectionLength++;
		//　正解率の計算
		CorrectAnswerRate();
		// ミスタイプがあったら苦手キーに追加
		MisTypeAdd(str);
		isRecMistype = false;
		// 可能な入力パターンのチェック
		bool isIndexCountUp = CheckValidSentence(str);
		// ローマ字入力表示を更新
		UpdateSentence(str);
		if(isIndexCountUp){
			index++;
		}
		// 文章入力完了処理
		if(index >= sentenceTyping.Count){
			CompleteTask();
		}
	}

	bool CheckValidSentence(string str){
		bool ret = false;
		// 可能な入力パターンを残す
		for (int i = 0; i < sentenceTyping[index].Count; ++i){
			// str と一致しないものを無効化処理
			if(!str.Equals(sentenceTyping[index][i][sentenceIndex[index][i]].ToString())){
				sentenceValid[index][i] = 0;
			}
			// 次のキーへ
			sentenceIndex[index][i] += indexAdd[index][i];
			// 次の文字へ
			if(sentenceIndex[index][i] >= sentenceTyping[index][i].Length) {
				ret = true;
			}
		}
		return ret;
	}

	void CompleteTask(){
		tasksCompleted++;
		double currentTime = Time.realtimeSinceStartup;
		KeyPerMinute(currentTime);
		queue.Clear();
		// 終了
		if(tasksCompleted >= Tasks){
			finished();
		}
		else {
			// if(!(GameMode == gameModeLunatic && !isInputValid)){
			OutputQ();
			// }
		}
	}

	void UpdateSentence(string str){
		// 打った文字を消去するオプションの場合
		// if(GameMode == gameModeEasy){
		UII.text = "";
		for (int i = 0; i < sentenceTyping.Count; ++i){
			if(i < index){
				continue;
			}
			for (int j = 0; j < sentenceTyping[i].Count; ++j){
				if(index == i && sentenceValid[index][j] == 0){
					continue;
				}
				else if(index == i && sentenceValid[index][j] == 1){
					for (int k = 0; k < sentenceTyping[index][j].Length; ++k){
						if(k >= sentenceIndex[index][j]){
							UII.text += sentenceTyping[index][j][k].ToString();
						}
					}
					break;
				}
				else if(index != i && sentenceValid[i][j] == 1){
					UII.text += sentenceTyping[i][j];
					break;
				}
			}
		}
		// }
		// 正解した文字を表示するオプションの場合
		// else {
		// 	correctString += str;
		// 	UII.text = correctString;
		// }
	}

	//　タイピング失敗時の処理
	void Mistype() {
		//　ミスタイプ数を増やす
		MisTypeNum += Input.inputString.Length;
		UITypeInfo.text = "Correct: " + CorrectTypeNum.ToString() + " / Mistype: " + MisTypeNum.ToString();
		//　正解率の計算
		CorrectAnswerRate();
		//　Lunatic mode : ミスタイプした文字を赤く表示
		// if(GameMode == gameModeLunatic && Input.inputString != "") {
		// 	UII.text = correctString + "<color=#ff0000ff>" + Input.inputString + "</color>";
		// }
		// Easy / Normal mode : 打つべき文字を赤く表示
		if(!isRecMistype && Input.inputString != ""){
			string rest = "";
			string s = UII.text.ToString();
			Debug.Log(s);
			UII.text = "";
			for (int i = 1; i < s.Length; ++i){
				rest += s[i].ToString();
			}
			UII.text = "<color=#ff0000ff>" + s[0].ToString() + "</color>" + rest;
		}
		// 苦手キー記録用
		isRecMistype = true;
	}

	void MisTypeAdd(string str){
		if(isRecMistype){
			if(MisTypeDictionary.ContainsKey(str)){
				MisTypeDictionary[str]++;
			}
			else {
				MisTypeDictionary.Add(str, 1);
			}
		}
	}

	//　正解率の計算処理
	void CorrectAnswerRate() {
		//　正解率の計算
		Accuracy = 100f * CorrectTypeNum / (CorrectTypeNum + MisTypeNum);
		UIAccuracy.text = "Accuracy : " + Accuracy.ToString("0.00") + " %";
	}

	double GetSentenceTypeTime (double currentTime){
		double ret;
		if (Math.Abs(firstCharInputTime - lastUpdateTime) <= INTERVAL){
			Debug.Log(tasksCompleted.ToString() + " -> time:" + (currentTime - firstCharInputTime).ToString());
			ret = currentTime - firstCharInputTime;
		}
		else {
			Debug.Log(tasksCompleted.ToString() + " -> time(late):" + (currentTime - (lastUpdateTime + INTERVAL)).ToString());
			ret = currentTime - (lastUpdateTime + INTERVAL);
		}
		return ret;
	}

	// kpm
	void KeyPerMinute(double currentTime) {
		double sentenceTypeTime = GetSentenceTypeTime(currentTime);
		TotalTypingTime += sentenceTypeTime;
		Kpm = ((1.0 * CorrectTypeNum) / (1.0 * TotalTypingTime)) * 60.0;
		double sectionKPM = ((1.0 * sectionLength) / (1.0 * sentenceTypeTime)) * 60.0;
		int intKpm = Convert.ToInt32(Math.Floor(Kpm));
		int intSectionKPM = Convert.ToInt32(Math.Floor(sectionKPM));
		UIKPM.text = "Speed : " + intKpm.ToString() + " kpm\n[Sentence:" + intSectionKPM.ToString() + " kpm]";
		UISTT.text = "Time : " + sentenceTypeTime.ToString("0.00") + " sec\nTotal : "
		+ TotalTypingTime.ToString("0.00") + " sec";
	}

	// シフトキーが押されたときのキーコード変換
	char ConvertKeyCodeToChar(KeyCode kc, bool isShift){
		switch(kc){
			// かな入力用に便宜的にタブ文字を Shift+0 に割り当てている
			case KeyCode.Alpha0:
        return isShift ? '\t' : '0';
      case KeyCode.Alpha1:
        return isShift ? '!' : '1';
      case KeyCode.Alpha2:
        return isShift ? '\"' : '2';
      case KeyCode.Alpha3:
        return isShift ? '#' : '3';
      case KeyCode.Alpha4:
        return isShift ? '$' : '4';
      case KeyCode.Alpha5:
        return isShift ? '%' : '5';
      case KeyCode.Alpha6:
        return isShift ? '&' : '6';
      case KeyCode.Alpha7:
        return isShift ? '\'' : '7';
      case KeyCode.Alpha8:
        return isShift ? '(' : '8';
      case KeyCode.Alpha9:
        return isShift ? ')' : '9';
			case KeyCode.A:
        return isShift ? 'A' : 'a';
      case KeyCode.B:
        return isShift ? 'B' : 'b';
      case KeyCode.C:
        return isShift ? 'C' : 'c';
      case KeyCode.D:
        return isShift ? 'D' : 'd';
      case KeyCode.E:
        return isShift ? 'E' : 'e';
      case KeyCode.F:
        return isShift ? 'F' : 'f';
      case KeyCode.G:
        return isShift ? 'G' : 'g';
      case KeyCode.H:
        return isShift ? 'H' : 'h';
      case KeyCode.I:
        return isShift ? 'I' : 'i';
      case KeyCode.J:
        return isShift ? 'J' : 'j';
      case KeyCode.K:
        return isShift ? 'K' : 'k';
      case KeyCode.L:
        return isShift ? 'L' : 'l';
      case KeyCode.M:
        return isShift ? 'M' : 'm';
      case KeyCode.N:
        return isShift ? 'N' : 'n';
      case KeyCode.O:
        return isShift ? 'O' : 'o';
      case KeyCode.P:
        return isShift ? 'P' : 'p';
      case KeyCode.Q:
        return isShift ? 'Q' : 'q';
      case KeyCode.R:
        return isShift ? 'R' : 'r';
      case KeyCode.S:
        return isShift ? 'S' : 's';
      case KeyCode.T:
        return isShift ? 'T' : 't';
      case KeyCode.U:
        return isShift ? 'U' : 'u';
      case KeyCode.V:
        return isShift ? 'V' : 'v';
      case KeyCode.W:
        return isShift ? 'W' : 'w';
      case KeyCode.X:
        return isShift ? 'X' : 'x';
      case KeyCode.Y:
        return isShift ? 'Y' : 'y';
      case KeyCode.Z:
        return isShift ? 'Z' : 'z';
      case KeyCode.Minus:
        return isShift ? '=' : '-';
      case KeyCode.Caret:
        return isShift ? '~' : '^';
      case KeyCode.Backslash:
        return isShift ? '|' : '\\';
      case KeyCode.At:
        return isShift ? '`' : '@';
      case KeyCode.LeftBracket:
        return isShift ? '{' : '[';
			case KeyCode.RightBracket:
        return isShift ? '}' : ']';
      case KeyCode.Semicolon:
        return isShift ? '+' : ';';
      case KeyCode.Colon:
        return isShift ? '*' : ':';
      case KeyCode.Comma:
        return isShift ? '<' : ',';
      case KeyCode.Period:
        return isShift ? '>' : '.';
      case KeyCode.Slash:
        return isShift ? '?' : '/';
      case KeyCode.Underscore:
        return '_';
      case KeyCode.Space:
        return ' ';
			// Esc も便宜的にバックスペースに割り当てる
			case KeyCode.Escape:
				return '\b';
      default: // null
        return '\0';
		}
	}
}