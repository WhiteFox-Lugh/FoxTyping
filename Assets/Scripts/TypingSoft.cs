using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class TypingSoft : MonoBehaviour {
	private const double INTERVAL = 2.0F;
	// 入力された文字の queue
	private static Queue<char> inputKeyQueue = new Queue<char>();
	// 時刻の queue
	private static Queue<double> timeQueue = new Queue<double>();
	// 問題表示関連
	private static string originSentence;
	private static string typeSentence;
	// これまで打った文字列
	private static string correctString;
	// 入力受け付け
	private static bool isInputValid;
	// センテンスの長さ
	private static int sentenceLength;
	// ミスタイプ記録
	private static bool isRecMistype;
	private static bool isSentenceMistyped;
	// タイピングの正誤判定器
	private static List<List<string>> typingJudge;
	// load 関係
	private static bool isLoadSuccess = false;
	// index 類
	private static int index;
	private static List<List<int>> indexAdd = new List<List<int>>();
	private static List<List<int>> sentenceIndex = new List<List<int>>();
	private static List<List<int>> sentenceValid = new List<List<int>>();
	// 時間計測関連
	private static bool isFirstInput;
	private static bool isIntervalEnded;
	private static double lastSentenceUpdateTime;
	private static double firstCharInputTime;
	private static double lastJudgeTime;
	// タイピング情報表示関連
	private static int tasksCompleted;
	private static int correctTypeNum;
	private static int misTypeNum;
	private static double keyPerMin;
	private static double accuracyValue;
	private static double totalTypingTime;
	private static int numOfTask;
	private static string cpuTypeString;
	// リザルト集計用
	private static StringBuilder typedLetter = new StringBuilder();
	private static List<int> typeJudgeList = new List<int>();
	private static List<double> typeTimeList = new List<double>();
	// 色
	private static Color colorBrown = new Color(80f / 255f, 40f / 255f, 40f / 255f, 1f);
	private static Color colorBlack = new Color(0f / 255f, 0f / 255f, 0f / 255f, 1f);
	private static Color colorCpuPanelDisable = new Color(128f / 255f, 128f / 255f, 128f / 255f, 100f / 255f);
	private static Color colorCpuPanelAble = new Color(1, 1, 1, 100f/ 255f);
	// UI たち
	[SerializeField] private Text UIOriginSentence;
	[SerializeField] private Text UIYomigana;
	[SerializeField] private Text UIType;
	[SerializeField] private Text UIKPM;
	[SerializeField] private Text UISTT;
	[SerializeField] private Text UITask;
	[SerializeField] private Text UIAccuracy;
	[SerializeField] private Text UICPUText;
	[SerializeField] private Text UITypeInfo;
	[SerializeField] private Text countdownText;
	[SerializeField] private GameObject DataPanel;
	[SerializeField] private GameObject AssistKeyboardPanel;
	[SerializeField] private GameObject CPUPanel;
	[SerializeField] private GameObject NowLoadingPanel;
	private static GenerateSentence gs = new GenerateSentence();
	// Assist Keyboard JIS
	private static AssistKeyboardJIS AKJIS = new AssistKeyboardJIS();

	// エラーコードとエラータイプ
	private enum errorType {
		None,
		QueueLengthNotMatch,
		FailedLoadSentence
	};

	// ゲームの状況
	private enum gameCondition {
		Progress,
		Finished,
		Canceled,
	};

	public static int CurrentGameCondition {
		private set;
		get;
	} = 0;

	public static int ErrorCode {
		private set;
		get;
	} = 0;

	public static TypingPerformance Performance {
		private set;
		get;
	}

	public static string CurrentTypingSentence {
		private set;
		get;
	} = "";

	/// <summary>
	/// Update() 前に読み込み
	/// </summary>
	void Awake() {
		NowLoadingPanel.SetActive(true);
		isLoadSuccess = false;
		CurrentGameCondition = (int)gameCondition.Progress;
		StartCoroutine(LoadWordDataset(CanStart));
	}

	/// <summary>
	/// スタートできるかの確認
	/// </summary>
	private void CanStart(){
		if (isLoadSuccess){
			InitGame();
		}
		else {
			ErrorCode = (int)errorType.FailedLoadSentence;
			CurrentGameCondition = (int)gameCondition.Canceled;
		}
	}

	/// <summary>
	/// 初期化
	/// </summary>
	private void InitGame() {
		InitData();
		InitText();
		NowLoadingPanel.SetActive(false);
		StartCoroutine(CountDown());
	}

	/// <summary>
	/// UI テキストの初期化
	/// </summary>
	private void InitText() {
		UpdateUITypeInfo();
		UpdateUICorrectTypeRate();
		UpdateUIKeyPerMinute(0, 0);
		UpdateUIElapsedTime(0.0);
		UpdateUITask();
	}

	/// <summary>
	/// データセット読み込み
	/// </summary>
	private IEnumerator LoadWordDataset(UnityAction callback){
		yield return StartCoroutine(gs.LoadAssetBundle(
			() => isLoadSuccess = gs.LoadSentenceData(ConfigScript.DataSetName))
			);
		callback();
	}

	/// <summary>
	/// 内部データの初期化
	/// </summary>
	private void InitData() {
		// データ関連の初期化
		ErrorCode = (int)errorType.None;
		CurrentGameCondition = (int)gameCondition.Progress;
		correctTypeNum = 0;
		misTypeNum = 0;
		totalTypingTime = 0.0;
		keyPerMin = 0.0;
		accuracyValue = 0.0;
		tasksCompleted = 0;
		isRecMistype = false;
		lastJudgeTime = -1.0;
		numOfTask = ConfigScript.Tasks;
		isInputValid = false;
		isIntervalEnded = false;
		isSentenceMistyped = false;
		AKJIS = new AssistKeyboardJIS();
		Performance = new TypingPerformance();
		CurrentTypingSentence = "";
		cpuTypeString = "";
		UIOriginSentence.text = "";
		UIYomigana.text = "";
		UIType.text = "";
		if (UICPUText != null){
			UICPUText.text = "";
		}
		if (CPUPanel != null){
			CPUPanel.GetComponent<Image>().color = (ConfigScript.UseCPUGuide ? colorCpuPanelAble : colorCpuPanelDisable);
		}
		inputKeyQueue.Clear();
		timeQueue.Clear();
	}

	/// <summary>
	/// 1f ごとの処理
	/// </summary>
	void Update() {
		TextColorChange();
		if (DataPanel != null && AssistKeyboardPanel != null){
			ShowMiddlePanel(ConfigScript.InfoPanelMode);
		}
		if (inputKeyQueue.Count > 0 && timeQueue.Count > 0){
			// キューの長さが一致しないなら Config へ戻す
			if(inputKeyQueue.Count != timeQueue.Count){
				ErrorCode = (int)errorType.QueueLengthNotMatch;
			}
			TypingCheck();
		}
		if (AssistKeyboardPanel != null){
			if (CurrentTypingSentence == "" || !isInputValid){
				AKJIS.SetAllKeyColorWhite();
				AKJIS.SetAllFingerColorWhite();
			}
			else if (isInputValid){
				AKJIS.SetNextHighlight(CurrentTypingSentence[0]);
			}
		}
	}

	/// <summary>
	/// カウントダウン演出
	/// </summary>
	private IEnumerator CountDown() {
    countdownText.text = "3";
		yield return new WaitForSeconds(1f);
		countdownText.text = "2";
		yield return new WaitForSeconds(1f);
		countdownText.text = "1";
		yield return new WaitForSeconds(1f);
		countdownText.text = "";
		GenerateNewSentence();
  }

	/// <summary>
	/// 課題文の文字色を変更
	/// 最初の1文字目を打つか時間経過で変わる
	/// </summary>
	private void TextColorChange() {
		double currentTime = Time.realtimeSinceStartup;
		if(isFirstInput && currentTime - lastSentenceUpdateTime <= INTERVAL){
			UIOriginSentence.color = colorBlack;
		}
		else {
			UIOriginSentence.color = colorBrown;
			isIntervalEnded = true;
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
	private void GenerateNewSentence() {
		// テキストUIを初期化する
		UIOriginSentence.text = "";
		UIYomigana.text = "";
		UIType.text = "";
		if (UICPUText != null){
			UICPUText.text = "";
		}
		// 正解した文字列を初期化
		correctString = "";
		// リザルト集積用の変数を初期化
		typedLetter = new StringBuilder();
		typeJudgeList = new List<int>();
		typeTimeList = new List<double>();
		// 変数等の初期化
		isFirstInput = true;
		isIntervalEnded = false;
		isRecMistype = false;
		isSentenceMistyped = false;
		index = 0;
		sentenceLength = 0;
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
	private void ChangeSentence() {
		// 例文生成
		var t = gs.Generate();
		if (!t.isGenerateSuccess){
			CurrentGameCondition = (int)gameCondition.Canceled;
		}
		originSentence = t.originSentence;
		typeSentence = t.typeSentence;
		typingJudge = t.typeJudge;
		// 判定器などの初期化
		InitSentenceData();
		var nextTypingSentence = "";
		for (int i = 0; i < typingJudge.Count; ++i){
			nextTypingSentence += typingJudge[i][0];
		}
		// Space は打ったか打ってないかわかりにくいので表示上はアンダーバーに変更
		// SetUITypeText(nextTypingSentence);
		CurrentTypingSentence = nextTypingSentence;
		cpuTypeString = nextTypingSentence;
		// UI 上のテキスト変更
		UIOriginSentence.text = originSentence;
		UIYomigana.text = typeSentence;
		if (ConfigScript.IsBeginnerMode){
			UIType.text = nextTypingSentence;
		}
		// CPU Start
		if (ConfigScript.UseCPUGuide && UICPUText != null){
			StartCoroutine("CPUType");
		}
	}

	/// <summary>
	/// CPU のタイピング処理
	/// </summary>
	private IEnumerator CPUType(){
		var idx = 0;
		float waitTime = (float)(60.0 / ConfigScript.CPUKpm);
		// 1文字目を打つまでは待機
		while (isFirstInput && !isIntervalEnded){
			yield return null;
		}
		// 残りの文字列処理
		while (isInputValid && idx < cpuTypeString.Length) {
			UICPUText.text += cpuTypeString[idx].ToString();
			idx++;
			yield return new WaitForSeconds(waitTime);
		}
		yield return null;
	}

	/// <summary>
	/// タイピング文の半角スペースをアンダーバーに置換して表示
	/// 打ったか打ってないかわかりにくいため、アンダーバーを表示することで改善
	/// </summary>
	private void SetUITypeText(string sentence) {
		UIType.text = sentence.Replace(' ', '_');
	}

	/// <summary>
	/// タイピング正誤判定まわりの初期化
	/// </summary>
	private void InitSentenceData() {
		var sLength = typingJudge.Count;
		sentenceIndex.Clear();
		sentenceValid.Clear();
		indexAdd.Clear();
		sentenceIndex = new List<List<int>>();
		sentenceValid = new List<List<int>>();
		indexAdd = new List<List<int>>();
		for (int i = 0; i < sLength; ++i){
			var typeNum = typingJudge[i].Count;
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
	/// タイピングの正誤判定部分
	/// </summary>
	private void TypingCheck() {
		while(inputKeyQueue.Count > 0){
			// inputKeyQueue に入ってる keycode を取得
			char inputChar = inputKeyQueue.Peek();
			inputKeyQueue.Dequeue();
			double keyDownTime = timeQueue.Peek();
			timeQueue.Dequeue();
			if(keyDownTime <= lastJudgeTime){
				continue;
			}
			lastJudgeTime = keyDownTime;

			// リザルト集積用
			typedLetter.Append(inputChar.ToString());
			typeTimeList.Add(keyDownTime);

			// まだ可能性のあるセンテンス全てに対してミスタイプかチェックする
			bool isMistype = true;
			// 全ての valid なセンテンスに対してチェックする
			for (int i = 0; i < typingJudge[index].Count; ++i){
				// invalid ならパス
				if(0 == sentenceValid[index][i]){
					continue;
				}
				int j = sentenceIndex[index][i];
				char nextInputChar = typingJudge[index][i][j];
				// 正解タイプ
				if(inputChar == nextInputChar){
					isMistype = false;
					indexAdd[index][i] = 1;
				}
				else {
					indexAdd[index][i] = 0;
				}
			}
			if(!isMistype){
				Correct(inputChar.ToString());
			}
			else {
				Mistype();
			}
		}
	}

	/// <summary>
	/// タイピング正解時の処理
	/// <param name="typeChar">打った文字</param>
	/// </summary>
	private void Correct(string typeChar) {
		// 正解数を増やす
		correctTypeNum++;
		sentenceLength++;
		UpdateUITypeInfo();
		// 正解率の計算
		accuracyValue = GetCorrectTypeRate();
		UpdateUICorrectTypeRate();
		isRecMistype = false;
		// 可能な入力パターンのチェック
		bool isIndexCountUp = IsJudgeIndexCountUp(typeChar);
		// ローマ字入力表示を更新
		UpdateSentence(typeChar);
		if(isIndexCountUp){
			index++;
		}
		// リザルト集積用
		typeJudgeList.Add(1);
		// 文章入力完了処理
		if(index >= typingJudge.Count){
			CompleteTask();
		}
	}

	/// <summary>
	/// 有効パターンをチェックし、インデックスを増やすかどうか判定する
	/// <param names="typeChar">打った文字</param>
	/// <returns>インデックス増やすなら true、さもなくば false</returns>
	/// </summary>
	private bool IsJudgeIndexCountUp(string typeChar) {
		bool ret = false;
		// 可能な入力パターンを残す
		for (int i = 0; i < typingJudge[index].Count; ++i){
			// typeChar と一致しないものを無効化処理
			if(!typeChar.Equals(typingJudge[index][i][sentenceIndex[index][i]].ToString())){
				sentenceValid[index][i] = 0;
			}
			// 次のキーへ
			sentenceIndex[index][i] += indexAdd[index][i];
			// 次の文字へ
			if(sentenceIndex[index][i] >= typingJudge[index][i].Length) {
				ret = true;
			}
		}
		return ret;
	}

	/// <summary>
	/// 全ての正解タイプに対する KPM の取得
	/// <returns>これまで打った正解タイプにおける KPM</returns>
	/// </summary>
	private double GetKeyPerMinute() {
		return ((1.0 * correctTypeNum) / (1.0 * totalTypingTime)) * 60.0;
	}

	/// <summary>
	/// 現在の文章に対する KPM の取得
	/// <param name="sentenceTypeTime">現在の文章を打つのにかかった時間</param>
	/// <returns>現在の文章の KPM</returns>
	/// </summary>
	private double GetSentenceKeyPerMinute(double sentenceTypeTime) {
		return ((1.0 * sentenceLength) / (1.0 * sentenceTypeTime)) * 60.0;
	}

	/// <summary>
	/// 1文打ち終わった後の処理
	/// </summary>
	private void CompleteTask() {
		tasksCompleted++;
		// リザルト集積用に追加
		Performance.AddOriginSentence(originSentence);
		Performance.AddTypedSentenceList(typedLetter.ToString());
		Performance.AddTypeJudgeList(typeJudgeList);
		Performance.AddTypeTimeList(typeTimeList);
		// 現在時刻の取得
		if (UISTT != null && UIKPM != null){
			double sentenceTypeTime = GetSentenceTypeTime(lastJudgeTime);
			totalTypingTime += sentenceTypeTime;
			keyPerMin = GetKeyPerMinute();
			double sectionKPM = GetSentenceKeyPerMinute(sentenceTypeTime);
			int intKPM = Convert.ToInt32(Math.Floor(keyPerMin));
			int intSectionKPM = Convert.ToInt32(Math.Floor(sectionKPM));
			UpdateUIKeyPerMinute(intKPM, intSectionKPM);
			UpdateUIElapsedTime(sentenceTypeTime);
		}
		inputKeyQueue.Clear();
		timeQueue.Clear();
		isInputValid = false;
		// 終了
		// numOfTask <= 0 の時は練習モード用で無限にできるようにするため
		if(tasksCompleted >= numOfTask && numOfTask > 0){
			CurrentGameCondition = (int)gameCondition.Finished;
		}
		else {
			StartCoroutine(DelayGenerateNewSentence());
		}
	}

	/// <summary>
	/// 画面上に表示する打つ文字の表示を更新する
	/// <param name="typeChar">打った文字</param>
	/// </summary>
	private void UpdateSentence(string typeChar) {
		// 打った文字を消去するオプションの場合
		var nextTypingSentence = "";
		for (int i = 0; i < typingJudge.Count; ++i){
			if(i < index){
				continue;
			}
			for (int j = 0; j < typingJudge[i].Count; ++j){
				if(index == i && sentenceValid[index][j] == 0){
					continue;
				}
				else if(index == i && sentenceValid[index][j] == 1){
					for (int k = 0; k < typingJudge[index][j].Length; ++k){
						if(k >= sentenceIndex[index][j]){
							nextTypingSentence += typingJudge[index][j][k].ToString();
						}
					}
					break;
				}
				else if(index != i && sentenceValid[i][j] == 1){
					nextTypingSentence += typingJudge[i][j];
					break;
				}
			}
		}
		correctString += typeChar;
		// Space は打ったか打ってないかわかりにくいので表示上はアンダーバーに変更
		var UIStr = "";
		if (ConfigScript.IsBeginnerMode){
			UIStr = nextTypingSentence;
		}
		else {
			UIStr = correctString + (isSentenceMistyped ? ("<color=#ff0000ff>" + nextTypingSentence + "</color>") : "");
		}
		SetUITypeText(UIStr);
		CurrentTypingSentence = nextTypingSentence;
	}

	/// <summary>
	/// ミスタイプ時の処理
	/// </summary>
	private void Mistype() {
		isSentenceMistyped = true;
		// ミスタイプ数を増やす
		misTypeNum++;
		UpdateUITypeInfo();
		// 正解率の計算
		accuracyValue = GetCorrectTypeRate();
		UpdateUICorrectTypeRate();
		// 打つべき文字を赤く表示
		if (!isRecMistype){
			string UIStr = "";
			if (ConfigScript.IsBeginnerMode){
				UIStr = "<color=#ff0000ff>" + CurrentTypingSentence.ToString() + "</color>";
			}
			else {
				UIStr = correctString + "<color=#ff0000ff>" + CurrentTypingSentence.ToString() + "</color>";
			}
			SetUITypeText(UIStr);
		}
		// color タグを多重で入れないようにする
		isRecMistype = true;
		// リザルト集積用
		typeJudgeList.Add(0);
	}

	/// <summary>
	/// これまでに打った文字の正解率を計算
	/// <returns>これまでの正解率</returns>
	/// </summary>
	private double GetCorrectTypeRate() {
		return 100f * correctTypeNum / (correctTypeNum + misTypeNum);
	}

	/// <summary>
	/// 1文打つのにかかった時間を取得
	/// <param name="currentTime">現在時刻</param>
	/// <returns>1文打つのにかかった時間</returns>
	/// </summary>
	private double GetSentenceTypeTime(double currentTime) {
		return (firstCharInputTime - lastSentenceUpdateTime <= INTERVAL) ? (currentTime - firstCharInputTime)
						: (currentTime - (lastSentenceUpdateTime + INTERVAL));
	}

	/// <summary>
	/// 正解率の UI 表示を更新
	/// </summary>
	private void UpdateUICorrectTypeRate() {
		if (UIAccuracy != null){
			UIAccuracy.text = "Accuracy : " + accuracyValue.ToString("0.00") + " %";
		}
	}

	/// <summary>
	/// 正解数、不正解数の UI 表示を更新
	/// </summary>
	private void UpdateUITypeInfo() {
		if (UITypeInfo != null){
			UITypeInfo.text = "Correct : " + correctTypeNum.ToString() + "\nMistype : " + misTypeNum.ToString();
		}
	}

	/// <summary>
	/// KPM 関連の UI 表示を更新
	/// <param name="intKPMAll">全文における KPM</param>
	/// <param name="intSentenceKPM">現在の文章の KPM</param>
	/// </summary>
	private void UpdateUIKeyPerMinute(int intKPMAll, int intSentenceKPM) {
		if (UIKPM != null){
			UIKPM.text = "Speed : " + intKPMAll.ToString() + " kpm\n[Sentence:" + intSentenceKPM.ToString() + " kpm]";
		}
	}

	/// <summary>
	/// 経過時間関連の UI 表示を更新
	/// <param name="sentenceTypeTime">現在の文章を打つのにかかった時間</param>
	/// </summary>
	private void UpdateUIElapsedTime(double sentenceTypeTime) {
		if (UISTT != null){
			UISTT.text = "Time : " + sentenceTypeTime.ToString("0.00") + " sec\nTotal : "
			+ totalTypingTime.ToString("0.00") + " sec";
		}
	}

	/// <summary>
	/// 文章数関連の UI 表示を更新
	/// </summary>
	private void UpdateUITask() {
		if (UITask != null){
			UITask.text = "Tasks : " + tasksCompleted.ToString() + " / " + numOfTask.ToString();
		}
	}

	/// <summary>
	/// 文章数関連の UI 表示を更新
	/// <param name="activePanelVal">表示をアクティブにするパネルの番号</param>
	/// </summary>
	private void ShowMiddlePanel(int activePanelVal) {
		if (activePanelVal == 0){
			DataPanel.SetActive(true);
			AssistKeyboardPanel.SetActive(false);
		}
		else if(activePanelVal == 1){
			DataPanel.SetActive(false);
			AssistKeyboardPanel.SetActive(true);
		}
		else if(activePanelVal == 2){
			DataPanel.SetActive(true);
			AssistKeyboardPanel.SetActive(true);
		}
		else {
			DataPanel.SetActive(false);
			AssistKeyboardPanel.SetActive(false);
		}
	}

	/// <summary>
	/// キーが入力されたとき等の処理
	/// </summary>
	void OnGUI() {
		double currentTime = Time.realtimeSinceStartup;
		Event e = Event.current;
		var isPushedShiftKey = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Backspace){
			CurrentGameCondition = (int)gameCondition.Canceled;
		}
    else if (isInputValid && e.type == EventType.KeyDown && e.keyCode != KeyCode.None
		&& !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2)){
			// F2 キーならリトライ
			if (e.keyCode == KeyCode.F2){
				InitGame();
			}
			var inputChar = ConvertKeyCodeToChar(e.keyCode, isPushedShiftKey);
			if (isFirstInput && inputChar != '\\'){
				firstCharInputTime = currentTime;
				isFirstInput = false;
			}
			// タイピングで使用する文字以外は受け付けない
			// Esc など画面遷移などで使うキーと競合を避ける
			if (inputChar != '\\'){
				inputKeyQueue.Enqueue(inputChar);
				timeQueue.Enqueue(currentTime);
			}
		}
	}

	/// <summary>
	/// キーコードから char への変換
	/// <param name="key">keycode</param>
	/// <param name="isShiftkeyPushed">シフトキーが押されたかどうか</param>
	/// </summary>
	private char ConvertKeyCodeToChar(KeyCode key, bool isShiftkeyPushed) {
		switch(key){
			// かな入力用に便宜的にタブ文字を Shift+0 に割り当てている
			case KeyCode.Alpha0:
        return isShiftkeyPushed ? '\t' : '0';
      case KeyCode.Alpha1:
        return isShiftkeyPushed ? '!' : '1';
      case KeyCode.Alpha2:
        return isShiftkeyPushed ? '\"' : '2';
      case KeyCode.Alpha3:
        return isShiftkeyPushed ? '#' : '3';
      case KeyCode.Alpha4:
        return isShiftkeyPushed ? '$' : '4';
      case KeyCode.Alpha5:
        return isShiftkeyPushed ? '%' : '5';
      case KeyCode.Alpha6:
        return isShiftkeyPushed ? '&' : '6';
      case KeyCode.Alpha7:
        return isShiftkeyPushed ? '\'' : '7';
      case KeyCode.Alpha8:
        return isShiftkeyPushed ? '(' : '8';
      case KeyCode.Alpha9:
        return isShiftkeyPushed ? ')' : '9';
			case KeyCode.A:
        return isShiftkeyPushed ? 'A' : 'a';
      case KeyCode.B:
        return isShiftkeyPushed ? 'B' : 'b';
      case KeyCode.C:
        return isShiftkeyPushed ? 'C' : 'c';
      case KeyCode.D:
        return isShiftkeyPushed ? 'D' : 'd';
      case KeyCode.E:
        return isShiftkeyPushed ? 'E' : 'e';
      case KeyCode.F:
        return isShiftkeyPushed ? 'F' : 'f';
      case KeyCode.G:
        return isShiftkeyPushed ? 'G' : 'g';
      case KeyCode.H:
        return isShiftkeyPushed ? 'H' : 'h';
      case KeyCode.I:
        return isShiftkeyPushed ? 'I' : 'i';
      case KeyCode.J:
        return isShiftkeyPushed ? 'J' : 'j';
      case KeyCode.K:
        return isShiftkeyPushed ? 'K' : 'k';
      case KeyCode.L:
        return isShiftkeyPushed ? 'L' : 'l';
      case KeyCode.M:
        return isShiftkeyPushed ? 'M' : 'm';
      case KeyCode.N:
        return isShiftkeyPushed ? 'N' : 'n';
      case KeyCode.O:
        return isShiftkeyPushed ? 'O' : 'o';
      case KeyCode.P:
        return isShiftkeyPushed ? 'P' : 'p';
      case KeyCode.Q:
        return isShiftkeyPushed ? 'Q' : 'q';
      case KeyCode.R:
        return isShiftkeyPushed ? 'R' : 'r';
      case KeyCode.S:
        return isShiftkeyPushed ? 'S' : 's';
      case KeyCode.T:
        return isShiftkeyPushed ? 'T' : 't';
      case KeyCode.U:
        return isShiftkeyPushed ? 'U' : 'u';
      case KeyCode.V:
        return isShiftkeyPushed ? 'V' : 'v';
      case KeyCode.W:
        return isShiftkeyPushed ? 'W' : 'w';
      case KeyCode.X:
        return isShiftkeyPushed ? 'X' : 'x';
      case KeyCode.Y:
        return isShiftkeyPushed ? 'Y' : 'y';
      case KeyCode.Z:
        return isShiftkeyPushed ? 'Z' : 'z';
      case KeyCode.Minus:
        return isShiftkeyPushed ? '=' : '-';
      case KeyCode.Caret:
        return isShiftkeyPushed ? '~' : '^';
      case KeyCode.At:
        return isShiftkeyPushed ? '`' : '@';
      case KeyCode.LeftBracket:
        return isShiftkeyPushed ? '{' : '[';
			case KeyCode.RightBracket:
        return isShiftkeyPushed ? '}' : ']';
      case KeyCode.Semicolon:
        return isShiftkeyPushed ? '+' : ';';
      case KeyCode.Colon:
        return isShiftkeyPushed ? '*' : ':';
      case KeyCode.Comma:
        return isShiftkeyPushed ? '<' : ',';
      case KeyCode.Period:
        return isShiftkeyPushed ? '>' : '.';
      case KeyCode.Slash:
        return isShiftkeyPushed ? '?' : '/';
      case KeyCode.Underscore:
        return '_';
      case KeyCode.Space:
        return ' ';
      default: // backslash
        return '\\';
		}
	}
}