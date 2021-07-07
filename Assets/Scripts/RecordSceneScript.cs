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
	// Start is called before the first frame update
	void Awake(){
		SetResultDetail();
	}

	// Update is called once per frame
	void Update(){
	}

	// double GetAccuracy (){
	// 	return 1.0 * typeNum / (typeNum + mistypeNum);
	// }

	// double tasksNumPenalty(){
	// 	if(tasks >= 20){
	// 		return 1.0;
	// 	}
	// 	return (-15.0 / (tasks + 10.0) + 1.5);
	// }

	// void CalculateScore(){
	// 	score = GetScore();
	// }

	// int GetScore(){
	// 	return (int)(tasksNumPenalty() * kpm * (1.05 + Math.Max(-1.0, Weight * Math.Log(GetAccuracy()))));
	// }

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
