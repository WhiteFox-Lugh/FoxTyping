using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ConfigScript : MonoBehaviour {
	private const int MIN_SENTENCE_NUM = 10;
	private const int MAX_SENTENCE_NUM = 100;
	private const int DEFAULT_SENTENCE_NUM = 30;
	[SerializeField] Dropdown UIGameMode;
	[SerializeField] TMP_InputField UISentenceNum;
	[SerializeField] TextMeshProUGUI UITextSentenceNum;
	[SerializeField] Text UITextSentenceNumDescription;

	public static int Tasks {
		private set;
		get;
	} = 30;

	public static int GameMode {
		private set;
		get;
	} = 0;

	public static string DataSetName {
		private set;
		get;
	} = "official";

	public static string LongSentenceDataSetName {
		private set;
		get;
	} = "long_constitution";

	void Awake () {
		UIGameMode.value = GameMode;
		UISentenceNum.text = Tasks.ToString();
		UISentenceNum.enabled = true;
	}
	// Update is called once per frame
	void Update () {
		if (!UISentenceNum.isFocused && !IsSentenceNumValid()){
			ComplementSentenceNum();
		}
	}

	void ComplementSentenceNum(){
		UISentenceNum.text = DEFAULT_SENTENCE_NUM.ToString();
	}

	bool IsSentenceNumValid(){
		int num;
		bool isNumOfSentenceParseSuccess = Int32.TryParse(UISentenceNum.text, out num);
		bool isNumOfSentenceValid = (isNumOfSentenceParseSuccess && MIN_SENTENCE_NUM <= num && num <= MAX_SENTENCE_NUM);
		if (isNumOfSentenceValid){
			Tasks = num;
		}
		return isNumOfSentenceValid;
	}

	void KeyCheck(KeyCode kc){
		if(KeyCode.Return == kc || KeyCode.KeypadEnter == kc){
			if (IsSentenceNumValid()){
				SceneManager.LoadScene("CountDownScene");
			}
		}
		else if(KeyCode.Escape == kc){
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
}
