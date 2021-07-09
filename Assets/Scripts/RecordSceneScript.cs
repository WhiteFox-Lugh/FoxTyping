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
	// Start is called before the first frame update
	void Awake(){
		SetResult();
		SetResultDetail();
	}

	// Update is called once per frame
	void Update(){
	}

	void SetResult() {
		var perf = TypingSoft.Performance;
		UIScoreText.text = perf.GetScore().ToString();
		UITimeText.text = perf.GetElapsedTime().ToString("0.000") + " s";
		UIAccuracyText.text = perf.GetAccuracy().ToString("0.00") + " %";
	}

	void SetResultDetail() {
		var sb = new StringBuilder();
		var perf = TypingSoft.Performance;
		int len = perf.OriginSentenceList.Count();
		for (int i = 0; i < len; ++i){
			sb.Append(perf.ConvertDetailResult(i));
		}
		UIResultDetailText.text = sb.ToString();
	}

	void KeyCheck(KeyCode k){
		if(KeyCode.Escape == k){
			SceneManager.LoadScene("TitleScene");
		}
	}

	void OnGUI() {
		Event e = Event.current;
		if (e.type == EventType.KeyDown && e.type != EventType.KeyUp && e.keyCode != KeyCode.None
				&& !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2)){
			KeyCheck(e.keyCode);
		}
	}

	public void OnClickTweetButton(){
		string tweetText = GetTweetText();
		string url = "";
		string hashTag = "FoxTyping";
		OpenTweetWindow(tweetText, hashTag, url);
	}

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
