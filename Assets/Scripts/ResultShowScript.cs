using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultShowScript : MonoBehaviour {
	private const double Weight = 1.0 + 2284815.0 / 2406151.0;
	private const int gameModeNormal = ConfigScript.gameModeNormal;
	private const int gameModeEasy = ConfigScript.gameModeEasy;
	private const int gameModeLunatic = ConfigScript.gameModeLunatic;
	// UI
	private Text UIScore;
	private Text UITypeNum;
	private Text UIMisType;
	private Text UIAccuracy;
	private Text UIMistypeKey;
	private Text UIKpm;
	private int typeNum;
	private int mistypeNum;
	private double kpm;
	private double accuracy;
	private int gameMode;
	private Dictionary<string, int> mistypeKey;
	
	// Use this for initialization
	void Start () {
		GetData();
		SetUI();
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

	void SetUI(){
		UIScore = transform.Find("ResultText/Score").GetComponent<Text>();
		UITypeNum = transform.Find("ResultText/NumOfType").GetComponent<Text>();
		UIMisType = transform.Find("ResultText/NumOfMistype").GetComponent<Text>();
		UIAccuracy = transform.Find("ResultText/Accuracy").GetComponent<Text>();
		UIMistypeKey = transform.Find("ResultText/MistypeKey").GetComponent<Text>();
		UIKpm = transform.Find("ResultText/Kpm").GetComponent<Text>();
		UIScore.text = "";
		UITypeNum.text = "";
		UIMisType.text = "";
		UIAccuracy.text = "";
		UIMistypeKey.text = "";
		UIKpm.text = "";
	}

	void ShowResult() {
		double score;
		if (gameMode == gameModeEasy){
			score = GetEasyModeScore();
		}
		else if(gameMode == gameModeLunatic){
			score = TypingSoft.AcheivedKPM;
		}
		else {
			score = GetNormalModeScore();
		}
		UIScore.text = (gameMode == gameModeLunatic) ? score.ToString("0") : score.ToString("0.00");
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
	}

	double GetAccuracy (){
		return 1.0 * typeNum / (typeNum + mistypeNum);
	}
	double GetNormalModeScore(){
		return kpm * (1.1 + Math.Max(-1.0, Weight * Math.Log(GetAccuracy())));
	}

	double GetEasyModeScore(){
		return 0.6 * GetNormalModeScore();
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
