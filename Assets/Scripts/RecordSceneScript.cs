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

public class RecordSceneScript : MonoBehaviour {
	[SerializeField] Text UIResultDetailText;
	[SerializeField] TextMeshProUGUI UIScoreText;
	[SerializeField] TextMeshProUGUI UITimeText;
	[SerializeField] TextMeshProUGUI UIAccuracyText;

	/// <summary>
	/// 初期化など
	/// </summary>
	void Awake(){
		SetResult();
		SetResultDetail();
	}

	/// <summary>
	/// 1フレームごとの更新処理。現時点ではなし
	/// </summary>
	void Update(){
	}

	/// <summary>
	/// 簡易リザルトの表示処理
	/// </summary>
	void SetResult() {
		var perf = TypingSoft.Performance;
		UIScoreText.text = perf.GetScore().ToString();
		UITimeText.text = perf.GetElapsedTime().ToString("0.000") + " s";
		UIAccuracyText.text = perf.GetAccuracy().ToString("0.00") + " %";
	}

	/// <summary>
	/// 詳細リザルトの表示処理
	/// </summary>
	void SetResultDetail() {
		var sb = new StringBuilder();
		var perf = TypingSoft.Performance;
		int len = perf.OriginSentenceList.Count();
		for (int i = 0; i < len; ++i){
			sb.Append(perf.ConvertDetailResult(i));
		}
		UIResultDetailText.text = sb.ToString();
	}

	/// <summary>
	/// キー入力に対応する処理を実行
	/// </summary>
	void KeyCheck(KeyCode k){
		if(KeyCode.Escape == k){
			SceneManager.LoadScene("TitleScene");
		}
	}

	/// <summary>
	/// キー入力などのイベント処理
	/// </summary>
	void OnGUI() {
		Event e = Event.current;
		if (e.type == EventType.KeyDown && e.type != EventType.KeyUp && e.keyCode != KeyCode.None
				&& !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2)){
			KeyCheck(e.keyCode);
		}
	}

	/// <summary>
	/// ツイートボタンを押したときの処理（現時点で機能自体は非表示）
	/// </summary>
	public void OnClickTweetButton(){
		string tweetText = GetTweetText();
		string url = "";
		string hashTag = "FoxTyping";
		OpenTweetWindow(tweetText, hashTag, url);
	}

	/// <summary>
	/// ツイート文章を生成
	/// </summary>
	string GetTweetText(){
		string ret = "";
		// const string template = "FoxTyping で_User_難易度 _Difficulty_ でスコア _Score_ をゲット！";
		// ret = template;
		// ret = ret.Replace("_User_", ((isLogin) ? (" " + UserAuth.currentPlayerName + " が") : ("")));
		// ret = ret.Replace("_Score_", score.ToString());
		return ret;
	}

	[DllImport("__Internal")]
  private static extern void OpenTweetWindow(string text, string hashtags, string url);
}
