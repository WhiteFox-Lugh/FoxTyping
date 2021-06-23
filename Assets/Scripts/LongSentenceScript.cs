using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LongSentenceScript : MonoBehaviour {
    private double startTime;
    private bool isShowInfo;
    // UI
    private Text UIRestTime;
    private Text UIInputCounter;
    private Text UICountDownText;
    private InputField UIInputField;
    private InputField UITextField;
    // 課題文章
    private string taskText;
    // 制限時間
    private static int LimitSec {
        set;
        get;
    } = 300;
    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("Initialized!");
        Init();
        StartCoroutine(CountDown());
    }

    void Init(){
        GetUI();
        startTime = 0.0;
        isShowInfo = false;
        UIInputField.interactable = false;
        taskText = LoadSentenceData("long_constitution");
    }

    void AfterCountDown(){
        startTime = Time.realtimeSinceStartup;
        isShowInfo = true;
        UITextField.text = taskText;
        UIInputField.interactable = true;
        UIInputField.ActivateInputField();
    }

    // Update is called once per frame
    void Update()
    {
        if (isShowInfo){
            CheckTimer();
            CheckInputStr();
        }
    }

    void CheckTimer(){
        var elapsedTime = Time.realtimeSinceStartup - startTime;
        var elapsedTimeInt = Convert.ToInt32(Math.Floor(elapsedTime));
        if (elapsedTimeInt >= LimitSec){
            Finish();
        }
        var restMin = (LimitSec - elapsedTimeInt) / 60;
        var restSec = (LimitSec - elapsedTimeInt) % 60;
        UIRestTime.text = "残り時間: " + restMin.ToString() + " 分 " + restSec.ToString() + " 秒";
    }

    void CheckInputStr(){
        var inputText = UIInputField.text;
        int inputCount = inputText.Length;
        UIInputCounter.text = "入力文字数: " + inputCount.ToString();
    }

    void GetUI(){
		UIRestTime = transform.Find("Timer").GetComponent<Text>();
        UICountDownText = transform.Find("CountDownText").GetComponent<Text>();
		UIInputCounter = transform.Find("InputStrNum").GetComponent<Text>();
        UIInputField = transform.Find("InputTextField").GetComponent<InputField>();
        UITextField = transform.Find("TaskTextField").GetComponent<InputField>();
	}

    void Finish(){
        Debug.Log("Finish");
    }

    string LoadSentenceData (string dataName){
        var str = "";
        try {
            var file = Resources.Load(dataName);
            str = file.ToString();
        }
        catch {
            return str;
        }
        return str;
    }

    IEnumerator CountDown(){
		UICountDownText.text = "3";
		yield return new WaitForSeconds(1.0f);
		UICountDownText.text = "2";
		yield return new WaitForSeconds(1.0f);
		UICountDownText.text = "1";
		yield return new WaitForSeconds(1.0f);
        UICountDownText.text = "";
        AfterCountDown();
	}
}
