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
    private Text UITextField;
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
        // 必ず文末からしか編集できないようにする
        // インテルステノ方式
        UIInputField.MoveTextEnd(false);
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
        int inputLine = UIInputField.caretPosition;
        UIInputCounter.text = "行数: " + inputLine.ToString() + " / 入力文字数: " + inputCount.ToString();
    }

    void GetUI(){
        UIRestTime = transform.Find("Timer").GetComponent<Text>();
        UICountDownText = transform.Find("CountDownText").GetComponent<Text>();
        UIInputCounter = transform.Find("InputStrNum").GetComponent<Text>();
        UIInputField = transform.Find("InputTextField").GetComponent<InputField>();
        UITextField = transform.Find("TaskTextField/Viewport/Content").GetComponent<Text>();
    }

    void Finish(){
        Debug.Log("Finish");
    }

    void OnGUI() {
        Event e = Event.current;
        var isPushedCtrlKey = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape){
            ReturnConfig();
        }
        else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.V && isPushedCtrlKey){
            Debug.Log("Copy detected");
        }
    }

    void ReturnConfig(){
        // あとで長文用のコンフィグシーンに差し替える
        SceneManager.LoadScene("TitleScene");
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
