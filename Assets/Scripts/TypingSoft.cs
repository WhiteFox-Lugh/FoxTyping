using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TypingSoft : MonoBehaviour {
	private const double INTERVAL = 3.0F;
	// 入力された文字の queue
	private static Queue<char> queue = new Queue<char>();
	// 時刻の queue
	private static Queue<double> timeQueue = new Queue<double>();
	// 問題表示関連
	private static string nQJ;
	private static string nQR;
	// これまで打った文字列
	private static string correctString;
	// 入力受け付け
	private static bool isInputValid;
	// センテンスの長さ
	private static int sectionLength;
	// ミスタイプ記録
	private static bool isRecMistype;
	// 文章の読み
	private static List<string> sentenceHiragana;
	// 文章タイピング読み
	private static List<List<string>> sentenceTyping;
	// index 類
	private static int index;
	private static List<List<int>> indexAdd = new List<List<int>>();
	private static List<List<int>> sentenceIndex = new List<List<int>>();
	private static List<List<int>> sentenceValid = new List<List<int>>();
	// 時間計測関連
	private static bool isFirstInput;
	private static double lastSentenceUpdateTime;
	private static double firstCharInputTime;
	private static double lastJudgeTime;
	// ゲームモード
	private static int gameMode;
	// タイピング情報表示関連
	private static int tasksCompleted;
	private static int correctTypeNum;
	private static int misTypeNum;
	private static double keyPerMin;
	private static double accuracyValue;
	private static double totalTypingTime;
	private static int numOfTask;
	// 色
	private static Color colorGray = new Color(128f / 255f, 128f / 255f, 128f / 255f, 0.6f);
	private static Color colorBrown = new Color(80f / 255f, 40f / 255f, 40f / 255f, 1f);
	private static Color colorBlack = new Color(0f / 255f, 0f / 255f, 0f / 255f, 1f);
	private static Color colorPurple = new Color(148f / 255f, 16f / 255f, 218f / 255f, 1f);
	// UI たち
	[SerializeField] Text UIJ;
	[SerializeField] Text UIR;
	[SerializeField] Text UII;
	[SerializeField] Text UIKPM;
	[SerializeField] Text UISTT;
	[SerializeField] Text UITask;
	[SerializeField] Text UIAccuracy;
	[SerializeField] Text UITypeInfo;
	GenerateSentence gs = new GenerateSentence();

	/// <summary>
	/// Update() 前に読み込み
	/// </summary>
	void Awake() {
		InitGame();
	}

	/// <summary>
	/// 初期化
	/// </summary>
	void InitGame() {
		InitData();
		InitText();
		GenerateNewSentence();
	}

	/// <summary>
	/// UI テキストの初期化
	/// </summary>
	void InitText() {
		UpdateUITypeInfo();
		UpdateUICorrectTypeRate();
		UpdateUIKeyPerMinute(0, 0);
		UpdateUIElapsedTime(0.0);
		UpdateUITask();
	}

	/// <summary>
	/// 内部データの初期化
	/// </summary>
	void InitData() {
		// json
		bool isLoadSuccess = gs.LoadSentenceData(ConfigScript.DataSetName);
		if (!isLoadSuccess){
			ReturnConfig();
		}
		// データ関連の初期化
		correctTypeNum = 0;
		misTypeNum = 0;
		totalTypingTime = 0.0;
		keyPerMin = 0.0;
		accuracyValue = 0.0;
		tasksCompleted = 0;
		isRecMistype = false;
		lastJudgeTime = -1.0;
		gameMode = ConfigScript.GameMode;
		numOfTask = ConfigScript.Tasks;
		isInputValid = true;
		queue.Clear();
		timeQueue.Clear();
	}

	/// <summary>
	/// Config 画面へ戻る
	/// </summary>
	void ReturnConfig() {
		SceneManager.LoadScene("SinglePlayConfigScene");
	}

	/// <summary>
	/// 結果画面へ遷移
	/// </summary>
	void Finished() {
		SceneManager.LoadScene("ResultScene");
	}

	/// <summary>
	/// 1f ごとの処理
	/// </summary>
	void Update() {
		TextColorChange();
		if (queue.Count > 0 && timeQueue.Count > 0){
			// キューの長さが一致しないなら Config へ戻す
			if(queue.Count != timeQueue.Count){
				ReturnConfig();
			}
			TypingCheck();
		}
	}

	/// <summary>
	/// 課題文の文字色を変更
	/// 最初の1文字目を打つか時間経過で変わる
	/// </summary>
	void TextColorChange() {
		double currentTime = Time.realtimeSinceStartup;
		if(isFirstInput && currentTime - lastSentenceUpdateTime <= INTERVAL){
			UIJ.color = colorBlack;
		}
		else {
			UIJ.color = colorBrown;
		}
	}

	/// <summary>
	/// 次のセンテンスへ移行前に休止を挟む
	/// </summary>
	private IEnumerator DelayGenerateNewSentence() {
    yield return new WaitForSeconds(1f);
		GenerateNewSentence();
  }

	/// <summary>
	/// 新しい課題文を生成する
	/// </summary>
	void GenerateNewSentence() {
		// テキストUIを初期化する
		UIJ.text = "";
		UIR.text = "";
		UII.text = "";
		// 正解した文字列を初期化
		correctString = "";
		// 変数等の初期化
		isFirstInput = true;
		index = 0;
		sectionLength = 0;
		// 問題文生成
		ChangeSentence();
		UpdateUITask();
		// 入力受け付け状態にする
		isInputValid = true;
		// 時刻を取得
		lastSentenceUpdateTime = Time.realtimeSinceStartup;
	}

	/// <summary>
	/// 課題文章の変更を行う
	/// </summary>
	void ChangeSentence() {
		var t = gs.Generate(gameMode);
		nQJ = t.originSentence;
		nQR = t.typeSentence;
		sentenceHiragana = t.hiSep;
		sentenceTyping = t.ty;
		// いろいろ初期化
		InitSentenceData();
		string tmpTypingSentence = "";
		for (int i = 0; i < sentenceTyping.Count; ++i){
			tmpTypingSentence += sentenceTyping[i][0];
		}
		// Space は打ったか打ってないかわかりにくいので表示上はアンダーバーに変更
		ReplaceWhitespaceToUnderbar(tmpTypingSentence);
		// テキスト変更
		UIJ.text = nQJ;
		UIR.text = nQR;
	}

	/// <summary>
	/// タイピング文の半角スペースをアンダーバーに置換
	/// 打ったか打ってないかわかりにくいため、アンダーバーを表示することで改善
	/// </summary>
	void ReplaceWhitespaceToUnderbar(string sentence) {
		UII.text = sentence.Replace(' ', '_');
	}

	/// <summary>
	/// タイピング正誤判定まわりの初期化
	/// </summary>
	void InitSentenceData() {
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

	/// <summary>
	/// キーが入力されたとき等の処理
	/// </summary>
	void OnGUI() {
		double currentTime = Time.realtimeSinceStartup;
    Event e = Event.current;
		var isPushedShiftKey = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    if (isInputValid && e.type == EventType.KeyDown && e.keyCode != KeyCode.None
		&& !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2)){
			var inputChar = ConvertKeyCodeToChar(e.keyCode, isPushedShiftKey);
			if (isFirstInput){
				firstCharInputTime = currentTime;
				isFirstInput = false;
			}
			queue.Enqueue(inputChar);
			timeQueue.Enqueue(currentTime);
		}
  }

	/// <summary>
	/// タイピングの正誤判定部分
	/// </summary>
	void TypingCheck() {
		while(queue.Count > 0){
			// queue に入ってる keycode を取得
			char inputChar = queue.Peek();
			queue.Dequeue();
			double keyDownTime = timeQueue.Peek();
			timeQueue.Dequeue();
			if(keyDownTime <= lastJudgeTime){
				continue;
			}
			lastJudgeTime = keyDownTime;

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
				Mistype(str);
			}
		}
	}

	/// <summary>
	/// タイピング正解時の処理
	/// </summary>
	void Correct(string str) {
		// 正解数を増やす
		correctTypeNum++;
		sectionLength++;
		UpdateUITypeInfo();
		// 正解率の計算
		accuracyValue = GetCorrectTypeRate();
		UpdateUICorrectTypeRate();
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

	/// <summary>
	/// 有効パターンのチェック
	/// </summary>
	bool CheckValidSentence(string str) {
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

	/// <summary>
	/// KPM の取得
	/// </summary>
	double GetKeyPerMinute() {
		return ((1.0 * correctTypeNum) / (1.0 * totalTypingTime)) * 60.0;
	}

	/// <summary>
	/// センテンスの KPM の取得
	/// </summary>
	double GetSentenceKeyPerMinute(double sentenceTypeTime) {
		return ((1.0 * sectionLength) / (1.0 * sentenceTypeTime)) * 60.0;
	}

	/// <summary>
	/// 1文打ち終わった後の処理
	/// </summary>
	void CompleteTask() {
		tasksCompleted++;
		// 現在時刻の取得
		double sentenceTypeTime = GetSentenceTypeTime(lastJudgeTime);
		totalTypingTime += sentenceTypeTime;
		keyPerMin = GetKeyPerMinute();
		double sectionKPM = GetSentenceKeyPerMinute(sentenceTypeTime);
		int intKPM = Convert.ToInt32(Math.Floor(keyPerMin));
		int intSectionKPM = Convert.ToInt32(Math.Floor(sectionKPM));
		UpdateUIKeyPerMinute(intKPM, intSectionKPM);
		UpdateUIElapsedTime(sentenceTypeTime);
		queue.Clear();
		timeQueue.Clear();
		isInputValid = false;
		// 終了
		if(tasksCompleted >= numOfTask){
			Finished();
		}
		else {
			StartCoroutine(DelayGenerateNewSentence());
		}
	}

	/// <summary>
	/// 画面上に表示する打つ文字の表示を更新する
	/// </summary>
	void UpdateSentence(string str) {
		// 打った文字を消去するオプションの場合
		string tmpTypingSentence = "";
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
							tmpTypingSentence += sentenceTyping[index][j][k].ToString();
						}
					}
					break;
				}
				else if(index != i && sentenceValid[i][j] == 1){
					tmpTypingSentence += sentenceTyping[i][j];
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
		// Space は打ったか打ってないかわかりにくいので表示上はアンダーバーに変更
		ReplaceWhitespaceToUnderbar(tmpTypingSentence);
	}

	/// <summary>
	/// ミスタイプ時の処理
	/// </summary>
	void Mistype(string str) {
		// ミスタイプ数を増やす
		misTypeNum++;
		UpdateUITypeInfo();
		// 正解率の計算
		accuracyValue = GetCorrectTypeRate();
		UpdateUICorrectTypeRate();
		// 打つべき文字を赤く表示
		if(!isRecMistype){
			string s = UII.text.ToString();
			string rest = s.Substring(1);
			UII.text = "<color=#ff0000ff>" + s[0].ToString() + "</color>" + rest;
		}
		// color タグを多重で入れないようにする
		isRecMistype = true;
	}

	/// <summary>
	/// 正解率の計算処理
	/// </summary>
	double GetCorrectTypeRate() {
		return 100f * correctTypeNum / (correctTypeNum + misTypeNum);
	}

	/// <summary>
	/// 1文打つのにかかった時間を取得
	/// </summary>
	double GetSentenceTypeTime(double currentTime) {
		return (firstCharInputTime - lastSentenceUpdateTime <= INTERVAL) ? (currentTime - firstCharInputTime)
						: (currentTime - (lastSentenceUpdateTime + INTERVAL));
	}

	/// <summary>
	/// 正解率の UI 表示を更新
	/// </summary>
	void UpdateUICorrectTypeRate() {
		UIAccuracy.text = "Accuracy : " + accuracyValue.ToString("0.00") + " %";
	}

	/// <summary>
	/// 正解数、不正解数の UI 表示を更新
	/// </summary>
	void UpdateUITypeInfo() {
		UITypeInfo.text = "Correct : " + correctTypeNum.ToString() + " / Mistype : " + misTypeNum.ToString();
	}

	/// <summary>
	/// KPM 関連の UI 表示を更新
	/// </summary>
	void UpdateUIKeyPerMinute(int intKPM, int intSectionKPM) {
		UIKPM.text = "Speed : " + intKPM.ToString() + " kpm\n[Sentence:" + intSectionKPM.ToString() + " kpm]";
	}

	/// <summary>
	/// 経過時間関連の UI 表示を更新
	/// </summary>
	void UpdateUIElapsedTime(double sentenceTypeTime) {
		UISTT.text = "Time : " + sentenceTypeTime.ToString("0.00") + " sec\nTotal : "
		+ totalTypingTime.ToString("0.00") + " sec";
	}

	/// <summary>
	/// 文章数関連の UI 表示を更新
	/// </summary>
	void UpdateUITask() {
		UITask.text = "Tasks : " + tasksCompleted.ToString() + " / " + numOfTask.ToString();
	}

	/// <summary>
	/// キーコードから char への変換
	/// </summary>
	char ConvertKeyCodeToChar(KeyCode kc, bool isShift) {
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