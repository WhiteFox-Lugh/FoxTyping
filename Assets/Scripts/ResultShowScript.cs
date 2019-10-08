using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultShowScript : MonoBehaviour {
	private const double Weight = 1.0 + 2284815.0 / 2406151.0;
	private const int gameModeNormal = ConfigScript.gameModeNormal;
	private const int gameModeEasy = ConfigScript.gameModeEasy;
	private const int gameModeLunatic = ConfigScript.gameModeLunatic;
	private const int ratingDiffMin = 1;
	// UI
	private Text UIScore;
	private Text UITypeNum;
	private Text UIMisType;
	private Text UIAccuracy;
	private Text UIMistypeKey;
	private Text UIKpm;

	private TextMeshProUGUI UIRating;
	private Text UIRatingDelta;
	private bool isLogin;
	private bool isNewRecord;
	private int typeNum;
	private int mistypeNum;
	private double kpm;
	private double accuracy;
	private int oldRating;
	private int newRating;
	private int ratingDiff;
	private int gameMode;
	private Dictionary<string, int> mistypeKey;

	private int score;
	
	// Use this for initialization
	void Start () {
		GetData();
		SetUI();
		CalculateScore();
		UpdateHiscore();
		ShowResult();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// Get data from TypingSoft.cs
	void GetData() {
		typeNum = TypingSoft.CorrectTypeNum;
		mistypeNum = TypingSoft.MisTypeNum;
		accuracy = TypingSoft.Accuracy;
		kpm = TypingSoft.Kpm;
		mistypeKey = TypingSoft.MisTypeDictionary;
		gameMode = TypingSoft.GameMode;
	}

	
	void UpdateHiscore(){
		isNewRecord = false;
		if(UserAuth.Instance == null || UserAuth.currentPlayerName == null){
			isLogin = false;
			return;
		}
		isLogin = true;
		// Rating の 計算 
		if(gameMode == gameModeNormal || gameMode == gameModeEasy){
			// Fetch
			oldRating = 0;
			newRating = 0;
			ratingDiff = 0;
			var oldarr = UserData.scoreNormal;
			int itr = -1;
			// 旧レートの計算
			foreach (int topScore in UserData.scoreNormal){
				oldRating += topScore;
			}
			// レート変動があるかチェック
			for (int i = 0; i < UserData.scoreNormal.Length; ++i){
				if(score - UserData.scoreNormal[i] >= ratingDiffMin){
					itr = i;
					break;
				}
			}
			// あれば書き換え
			if(itr != -1){
				for (int i = UserData.scoreNormal.Length - 1; i > itr ; --i){
					UserData.scoreNormal[i] = UserData.scoreNormal[i - 1];
				}
				UserData.scoreNormal[itr] = score;
				isNewRecord = true;
			}
			// 新レートの計算
			foreach (int topScore in UserData.scoreNormal){
				newRating += topScore;
			}
			ratingDiff = newRating - oldRating;
			var newarr = UserData.scoreNormal;
			Debug.Log(oldarr);
			Debug.Log(newarr);
		}
		// Lunatic の場合の計算
		else if(gameMode == gameModeLunatic){
			if(score > UserData.scoreLunatic){
				isNewRecord = true;
				UserData.scoreLunatic = (int)(score);
			}
		}
		if(isNewRecord){
			UserData.save();
		}
	}

	void SetUI(){
		UIScore = transform.Find("ResultText/Score").GetComponent<Text>();
		UITypeNum = transform.Find("ResultText/NumOfType").GetComponent<Text>();
		UIMisType = transform.Find("ResultText/NumOfMistype").GetComponent<Text>();
		UIAccuracy = transform.Find("ResultText/Accuracy").GetComponent<Text>();
		UIMistypeKey = transform.Find("ResultText/MistypeKey").GetComponent<Text>();
		UIKpm = transform.Find("ResultText/Kpm").GetComponent<Text>();
		UIRating = transform.Find("ResultText/Rating").GetComponent<TextMeshProUGUI>();
		UIRatingDelta = transform.Find("ResultText/RatingDelta").GetComponent<Text>();
		UIScore.text = "";
		UITypeNum.text = "";
		UIMisType.text = "";
		UIAccuracy.text = "";
		UIMistypeKey.text = "";
		UIKpm.text = "";
		UIRating.text = "---";
		UIRatingDelta.text = "+0";
	}

	void CalculateScore(){
		if (gameMode == gameModeEasy){
			score = GetEasyModeScore();
		}
		else if(gameMode == gameModeLunatic){
			score = TypingSoft.AcheivedKPM;
		}
		else {
			score = GetNormalModeScore();
		}
	}

	void ShowResult() {
		UIScore.text = score.ToString();
		UITypeNum.text = typeNum.ToString();
		UIMisType.text = mistypeNum.ToString();
		UIAccuracy.text = accuracy.ToString("0.00") + "%";
		ShowMistypeKey();
		if (gameMode == gameModeLunatic){
			UIKpm.text = "---";
		}
		else {
			UIKpm.text = kpm.ToString("0.00") + " key / min";
		}
		UIRating.text = (!isLogin) ? "---" : newRating.ToString();
		UIRatingDelta.text = (!isLogin) ? "--" : ("+" + ratingDiff.ToString());
	}

	double GetAccuracy (){
		return 1.0 * typeNum / (typeNum + mistypeNum);
	}
	int GetNormalModeScore(){
		return (int)(kpm * (1.05 + Math.Max(-1.0, Weight * Math.Log(GetAccuracy()))));
	}

	int GetEasyModeScore(){
		var x = GetNormalModeScore();
		const int c = 1200;
		return (int)(-1.0 * c * c / (x + c) + c);
	}

	void ShowMistypeKey(){
		var sortedMistypeKey = mistypeKey.OrderByDescending((str) => str.Value);
		int i = 0;
		foreach (var str in sortedMistypeKey){
			if(i >= 4){
				break;
			}
			if(i != 0){
				UIMistypeKey.text += ", ";
			}
			UIMistypeKey.text += str.Key + "(" + str.Value.ToString() + "回)";
			++i;
		}
	}

	void KeyCheck(KeyCode k){
		if(KeyCode.Space == k || KeyCode.Return == k || KeyCode.KeypadEnter == k){
			SceneManager.LoadScene("SinglePlayConfigScene");
		}
	}
	void OnGUI() {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.type != EventType.KeyUp && e.keyCode != KeyCode.None
		&& !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2)){
			KeyCheck(e.keyCode);
		}
    }
}
