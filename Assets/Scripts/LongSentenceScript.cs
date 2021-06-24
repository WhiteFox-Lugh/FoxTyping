using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DiffMatchPatch;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LongSentenceScript : MonoBehaviour {
    private double startTime;
    private bool isShowInfo;
    private bool isFinished;
    // UI
    private Text UIRestTime;
    private Text UIInputCounter;
    private Text UICountDownText;
    private InputField UIInputField;
    [SerializeField] Text UIResultTextField;
    [SerializeField] Text UITextField;
    [SerializeField] GameObject InputPanel;
    [SerializeField] GameObject ResultPanel;
    [SerializeField] GameObject TaskPanel;
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
        isFinished = false;
        UIInputField.interactable = false;
        ResultPanel.SetActive(false);
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
        if (isShowInfo && !isFinished){
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
        UIInputField = transform.Find("InputPanel/InputTextField").GetComponent<InputField>();
    }

    void Finish(){
        // 表示の切り替え
        ResultPanel.SetActive(true);
        InputPanel.SetActive(false);
        TaskPanel.SetActive(false);
        UIResultTextField.text = UIInputField.text;
        UIInputField.interactable = false;
        isFinished = true;
        // 得点計算と表示
        ShowScore();
    }

    void ShowScore(){
        // 編集距離の計算
        string taskSentence = taskText;
        string userInputSentence = UIInputField.text;
        diff_match_patch dmp = new diff_match_patch();
        List<Diff> diff = dmp.diff_main(taskSentence, userInputSentence);
        dmp.diff_cleanupSemantic(diff);
        for (int i = 0; i < diff.Count; i++) {
            Debug.Log(diff[i]);
        }
    }

    // (int dist, List<(int, int)> trace) GetEditDistance(string strA, string strB){
    //     string src = strA;
    //     string dst = strB;
    //     Debug.Log(src);
    //     Debug.Log(dst);
    //     var rows = src.Length + 1;
    //     var cols = dst.Length + 1;
    //     int[ , ] d = new int[rows, cols];
    //     for (int i = 0; i < rows; ++i){
    //         d[i, 0] = i;
    //     }
    //     for (int i = 0; i < cols; ++i){
    //         d[0, i] = i;
    //     }
    //     for (int i = 1; i < rows; ++i){
    //         for (int j = 1; j < cols; ++j){
    //             d[i, j] = Math.Min(d[i - 1, j - 1] + ((src[i - 1] == dst[j - 1] || src[i - 1] == '*') ? 0 : 1),
    //                         Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1));
    //         }
    //     }
    //     var retBackTrace = BackTrace(src, dst, d);
    //     var retDistance = -1;
    //     foreach (var (i, j) in retBackTrace){
    //         Debug.Log((i, j));
    //         if (i == 0){
    //             retDistance = d[i, j];
    //             break;
    //         }
    //     }
    //     return (retDistance, retBackTrace);
    // }

    // List<(int, int)> BackTrace(string strA, string strB, int[ , ] matrix){
    //     int row = strA.Length;
    //     int col = strB.Length;
    //     var trace = new List<(int, int)>() { (row, col) };
    //     while (row > 0 && col > 0){
    //         int cost = (strA[row - 1] == strB[col - 1]) ? 0 : 1;
    //         int current = matrix[row, col];
    //         int costA = matrix[row - 1, col];
    //         int costB = matrix[row - 1, col - 1];
    //         int costC = matrix[row, col - 1];
    //         // 置換 or 一致
    //         if (current == costB + cost){
    //             trace.Add((row - 1, col - 1));
    //             row--;
    //             col--;
    //         }
    //         // 挿入
    //         else if (current == costC + 1){
    //             trace.Add((row, col - 1));
    //             col--;
    //         }
    //         // 削除
    //         else if (current == costA + 1){
    //             trace.Add((row - 1, col));
    //             row--;
    //         }
    //     }
    //     return trace;
    // }

    void OnGUI() {
        Event e = Event.current;
        var isPushedCtrlKey = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape){
            if (!isFinished){
                Finish();
            }
            else {
                ReturnConfig();
            }
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
