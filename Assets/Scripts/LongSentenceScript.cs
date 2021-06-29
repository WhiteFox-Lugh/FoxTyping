using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 差分表示用
public class Diff {
    public string op;
    public string before;
    public string after;

    public Diff(string op, string before, string after) {
        this.op = op;
        this.before = before;
        this.after = after;
    }
}

public class LongSentenceScript : MonoBehaviour {
    // diff の タイプ
    const string OP_INSERT = "insert";
    const string OP_DELETE = "delete";
    const string OP_REPLACE = "replace";
    const string OP_EQUAL = "equal";
    // diff の表示色
    const string COLOR_INSERT = "orange";
    const string COLOR_DELETE = "red";
    const string COLOR_REPLACE = "blue";
    // 正解、不正解の重み
    const int CORRECT_SCORE = 1;
    const int MISS_COST = 10;
    const int MISS_COST_MP = 1;
    private double startTime;
    private bool isShowInfo;
    private bool isFinished;
    // UI
    [SerializeField] Text UIResultTextField;
    [SerializeField] Text UITextField;
    [SerializeField] Text UIRestTime;
    [SerializeField] Text UICountDownText;
    [SerializeField] Text UIInputCounter;
    [SerializeField] TextMeshProUGUI UIScoreText;
    [SerializeField] TextMeshProUGUI UIDetailText;
    [SerializeField] InputField UIInputField;
    [SerializeField] GameObject InputPanel;
    [SerializeField] GameObject ResultPanel;
    [SerializeField] GameObject TaskPanel;
    [SerializeField] GameObject InfoPanel;
    [SerializeField] GameObject ScorePanel;
    // 課題文章
    private string taskText;
    // スコア表示
    private int correctCount = 0;
    private int deleteCount = 0;
    private int insertCount = 0;
    private int replaceCount = 0;
    // 制限時間
    private static int LimitSec {
        set;
        get;
    } = 300;

    void Awake()
    {
        Debug.Log("Initialized!");
        Init();
        StartCoroutine(CountDown());
    }

    void Init(){
        startTime = 0.0;
        isShowInfo = false;
        isFinished = false;
        UIInputField.interactable = false;
        InputPanel.SetActive(true);
        TaskPanel.SetActive(true);
        InfoPanel.SetActive(true);
        ResultPanel.SetActive(false);
        ScorePanel.SetActive(false);
        taskText = LoadSentenceData("long_constitution");
    }

    void AfterCountDown(){
        startTime = Time.realtimeSinceStartup;
        isShowInfo = true;
        UITextField.text = taskText;
        UIInputField.interactable = true;
        UIInputField.ActivateInputField();
    }

    void Update()
    {
        // 必ず文末からしか編集できないようにする
        // インテルステノ方式
        if (!UIInputField.isFocused){
            UIInputField.Select();
        }
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
        UIInputCounter.text = "入力文字数: " + inputCount.ToString();
    }

    void Finish(){
        // 表示の切り替え
        ResultPanel.SetActive(true);
        ScorePanel.SetActive(true);
        InfoPanel.SetActive(false);
        InputPanel.SetActive(false);
        TaskPanel.SetActive(false);
        UIResultTextField.text = UIInputField.text;
        UIInputField.interactable = false;
        isFinished = true;
        // 得点計算と表示
        ShowScore();
    }

    void ShowScore(){
        const string EOS = "{END}";
        // 編集距離の計算
        string taskSentence = taskText;
        string userInputSentence = UIInputField.text;
        List<Diff> diff = GetDiff(taskText, userInputSentence);
        var coloredText = ConvertDiffToHtml(diff);
        SetScoreDetail(diff);
        ShowOriginalScore();
        UIResultTextField.text = coloredText;
    }

    void ShowOriginalScore(){
        int score = correctCount - (deleteCount + insertCount + replaceCount) * MISS_COST;
        var sbScore = new StringBuilder();
        var sbDetail = new StringBuilder();
        sbScore.Append("スコア(F)：").Append(score.ToString());
        sbDetail.Append("正解数：" + correctCount.ToString() + " x " + CORRECT_SCORE.ToString() +"点\n")
                .Append("<color=\"" + COLOR_DELETE + "\">削除：" + deleteCount.ToString())
                .Append(" x (-" + MISS_COST.ToString() + "点)</color> / ")
                .Append("<color=\"" + COLOR_INSERT + "\">余分：" + insertCount.ToString())
                .Append(" x (-" + MISS_COST.ToString() + "点)</color>\n")
                .Append("<color=\"" + COLOR_REPLACE + "\">置換：" + replaceCount.ToString())
                .Append(" x (-" + MISS_COST.ToString() + "点)</color>");
        UIScoreText.text = sbScore.ToString();
        UIDetailText.text = sbDetail.ToString();
    }

    void ShowMpScore(){
        int missCount = deleteCount + insertCount + replaceCount;
        int spScore = (missCount <= 3) ? Convert.ToInt32(Math.Ceiling(correctCount * (0.20 - 0.05 * missCount))) : 0;
        int score = correctCount - missCount * MISS_COST_MP + spScore;
        var sbScore = new StringBuilder();
        var sbDetail = new StringBuilder();
        sbScore.Append("スコア(M)：").Append(score.ToString());
        sbDetail.Append("正解数：" + correctCount.ToString() + " x " + CORRECT_SCORE.ToString() +"点 / ")
                .Append("特別点：" + spScore.ToString() + "\n")
                .Append("<color=\"" + COLOR_DELETE + "\">削除：" + deleteCount.ToString())
                .Append(" x (-" + MISS_COST_MP.ToString() + "点)</color> / ")
                .Append("<color=\"" + COLOR_INSERT + "\">余分：" + insertCount.ToString())
                .Append(" x (-" + MISS_COST_MP.ToString() + "点)</color>\n")
                .Append("<color=\"" + COLOR_REPLACE + "\">置換：" + replaceCount.ToString())
                .Append(" x (-" + MISS_COST_MP.ToString() + "点)</color>");
        UIScoreText.text = sbScore.ToString();
        UIDetailText.text = sbDetail.ToString();
    }

    void SetScoreDetail(List<Diff> diffs){
        correctCount = 0;
        deleteCount = 0;
        insertCount = 0;
        replaceCount = 0;
        foreach (Diff diff in diffs){
            string op = diff.op;
            string beforeText = diff.before;
            string afterText = diff.after;
            if (op.Equals(OP_EQUAL)){
                correctCount += beforeText.Length;
            }
            else if (op.Equals(OP_DELETE)){
                deleteCount += beforeText.Length;
            }
            else if (op.Equals(OP_INSERT)){
                insertCount += afterText.Length;
            }
            else if (op.Equals(OP_REPLACE)){
                replaceCount += beforeText.Length;
            }
        }
    }

    List<Diff> GetDiff(string strA, string strB){
        var retBackTrace = new List<Diff>() { };
        // 共通の prefix を探す
        int minLen = Math.Min(strA.Length, strB.Length);
        int commonPrefixIndex = -1;
        for (int i = 0; i < minLen; ++i){
            if (strA[i] == strB[i]){
                commonPrefixIndex = i;
            }
            else {
                break;
            }
        }
        string commonPrefix = (commonPrefixIndex == -1) ? "" : strA.Substring(0, commonPrefixIndex + 1);
        string restStrA = (commonPrefixIndex == -1) ? strA : strA.Substring(commonPrefixIndex + 1, strA.Length - commonPrefixIndex - 1);
        string restStrB = (commonPrefixIndex == -1) ? strB : strB.Substring(commonPrefixIndex + 1, strB.Length - commonPrefixIndex - 1);
        Debug.Log("Prefix: " + commonPrefix);
        Debug.Log("restA : " + restStrA);
        Debug.Log("restB : " + restStrB);
        // restB が空 -> そこまで全部正解
        if (restStrB.Equals("")){
            retBackTrace.Add(new Diff(OP_EQUAL, commonPrefix, commonPrefix));
            return retBackTrace;
        }
        // 入力した文章の最後n文字が課題文に一致するか？
        // ここでの suffix は厳密には suffix ではないが便宜上そう呼ぶことに
        int lb = 0, ub = restStrB.Length + 1;
        while (ub - lb > 1){
            int mid = lb + (ub - lb) / 2;
            string subStr = restStrB.Substring(restStrB.Length - mid, mid);
            int idxSubStr = restStrA.IndexOf(subStr);
            if (idxSubStr == -1){
                ub = mid;
            }
            else {
                lb = mid;
            }
        }
        int commonSuffixIndex = restStrB.Length - lb;
        string commonSuffix = restStrB.Substring(commonSuffixIndex, lb);
        int firstSubStrIdx = restStrA.IndexOf(commonSuffix);
        restStrA = (commonSuffixIndex == restStrB.Length) ? restStrA : restStrA.Substring(0, firstSubStrIdx);
        restStrB = (commonSuffixIndex == restStrB.Length) ? restStrB : restStrB.Substring(0, commonSuffixIndex);
        Debug.Log("Suffix: " + commonSuffix);
        Debug.Log("restA : " + restStrA);
        Debug.Log("restB : " + restStrB);
        // 共通 suffix と prefix をのぞいた残りの文字の diff を求める
        // BackTrace で前方一致させるため逆順にする
        string src = new string(restStrA.Reverse().ToArray());
        string dst = new string(restStrB.Reverse().ToArray());
        Debug.Log(src);
        Debug.Log(dst);
        var rows = src.Length + 1;
        var cols = dst.Length + 1;
        int[ , ] d = new int[rows, cols];
        for (int i = 0; i < rows; ++i){
            d[i, 0] = i;
        }
        for (int i = 0; i < cols; ++i){
            d[0, i] = i;
        }
        for (int i = 1; i < rows; ++i){
            for (int j = 1; j < cols; ++j){
                d[i, j] = Math.Min(d[i - 1, j - 1] + ((src[i - 1] == dst[j - 1]) ? 0 : 1),
                            Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1));
            }
        }
        var tmpBackTrace = BackTrace(src, dst, d);
        foreach (var t in tmpBackTrace){
            Debug.Log(t);
        }
        var prefixTrace = (commonPrefix.Equals("")) ? new List<Diff>() { }
                            : new List<Diff>() {(new Diff (OP_EQUAL, commonPrefix, commonPrefix))};
        var suffixTrace = (commonSuffix.Equals("")) ? new List<Diff>() { }
                            : new List<Diff>() {(new Diff (OP_EQUAL, commonSuffix, commonSuffix))};
        var trace = ConvertDiff(tmpBackTrace, restStrA, restStrB);
        retBackTrace.AddRange(prefixTrace);
        retBackTrace.AddRange(trace);
        retBackTrace.AddRange(suffixTrace);
        // 最後の2つのop処理
        // delete, equal だった場合
        var len = retBackTrace.Count;
        if (len >= 2 && (retBackTrace[len - 2].op).Equals(OP_DELETE) && (retBackTrace[len - 1].op).Equals(OP_EQUAL)){
            var diff2 = retBackTrace[len - 2];
            var diff1 = retBackTrace[len - 1];
            var delLen = diff2.before.Length;
            var eqLen = diff1.before.Length;
            Debug.Log("del : " + delLen.ToString() + ", eq : " + eqLen.ToString());
            // 脱字文字コスト + 正解数 よりも 余分文字コストのみの方がスコアが高くなる時置き換え
            if (MISS_COST * delLen > (MISS_COST + 1) * eqLen){
                retBackTrace.RemoveRange(len - 2, 2);
                retBackTrace.Add(new Diff(OP_INSERT, "", diff1.before));
            }
        }
        // replace, delete だった場合
        else if (len >= 2 && (retBackTrace[len - 2].op).Equals(OP_REPLACE) && (retBackTrace[len - 1].op).Equals(OP_DELETE)){
            var diff2 = retBackTrace[len - 2];
            retBackTrace.RemoveRange(len - 2, 2);
            retBackTrace.Add(new Diff(OP_INSERT, "", diff2.after));
        }
        return retBackTrace;
    }

    List<(string, (int, int))> BackTrace(string strA, string strB, int[ , ] matrix){
        const int INF = -1000;
        var ALen = strA.Length;
        var BLen = strB.Length;
        int row = ALen;
        int col = BLen;
        var trace = new List<(string, (int, int))>();
        while (row > 0 || col > 0){
            int cost = (row > 0 && col > 0 && (strA[row - 1] == strB[col - 1])) ? 0 : 1;
            int current = matrix[row, col];
            int costA = (row == 0) ? INF : matrix[row - 1, col];
            int costB = (row == 0 || col == 0) ? INF : matrix[row - 1, col - 1];
            int costC = (col == 0) ? INF : matrix[row, col - 1];
            // 置換 or 一致
            if (costB != INF && current == costB + cost){
                if (strA[row - 1] == strB[col - 1]){
                    trace.Add((OP_EQUAL, (row - 1, col - 1)));
                }
                else {
                    trace.Add((OP_REPLACE, (row - 1, col - 1)));
                }
                row--;
                col--;
            }
            // 挿入
            else if (costC != INF && current == costC + 1){
                trace.Add((OP_INSERT, (row, col - 1)));
                col--;
            }
            // 削除
            else if (costA != INF && current == costA + 1){
                trace.Add((OP_DELETE, (row - 1, col)));
                row--;
            }
        }
        // リバースした文字列のトレースをしたのでインデックスを変更
        var ret = new List<(string, (int, int))>();
        foreach (var p in trace){
            ret.Add((p.Item1, (ALen - p.Item2.Item1 - 1, BLen - p.Item2.Item2 - 1)));
        }
        return ret;
    }

    List<Diff> ConvertDiff(List<(string op, (int idxA, int idxB))> opList, string compStrA, string compStrB){
        var ret = new List<Diff>() { };
        int i = 0;
        if (compStrA == ""){
            ret.Add(new Diff(OP_INSERT, "", compStrB));
            return ret;
        }
        else if(compStrB == ""){
            ret.Add(new Diff(OP_DELETE, compStrA, ""));
            return ret;
        }
        while (i < opList.Count){
            var current = opList[i];
            var currentOp = current.op;
            var targetStrA = (current.op).Equals(OP_INSERT) ? "" : compStrA[current.Item2.idxA].ToString();
            var targetStrB = (current.op).Equals(OP_DELETE) ? "" : compStrB[current.Item2.idxB].ToString();
            int j = 0;
            while (i + j + 1 < opList.Count){
                var next = opList[i + j + 1];
                var nextOp = next.op;
                if (nextOp == currentOp){
                    j++;
                    targetStrA += nextOp.Equals(OP_INSERT) ? "" : compStrA[next.Item2.idxA].ToString();
                    targetStrB += nextOp.Equals(OP_DELETE) ? "" : compStrB[next.Item2.idxB].ToString();
                }
                else {
                    break;
                }
            }
            if (currentOp.Equals(OP_DELETE)){
                ret.Add(new Diff(currentOp, targetStrA, ""));
            }
            else if (currentOp.Equals(OP_INSERT)){
                ret.Add(new Diff(currentOp, "", targetStrB));
            }
            else if (currentOp.Equals(OP_REPLACE) || currentOp.Equals(OP_EQUAL)){
                ret.Add(new Diff(currentOp, targetStrA, targetStrB));
            }
            i += 1 + j;
        }
        return ret;
    }

    string ConvertDiffToHtml (List<Diff> diffs){
        var sb = new StringBuilder();
        foreach (Diff diff in diffs) {
            string beforeText = diff.before.Replace("&", "&amp;").Replace("<", "&lt;")
            .Replace(">", "&gt;").Replace("\n", "&para;<br>");
            string afterText = diff.after.Replace("&", "&amp;").Replace("<", "&lt;")
            .Replace(">", "&gt;").Replace("\n", "&para;<br>");
            if ((diff.op).Equals(OP_EQUAL)){
                sb.Append(beforeText);
            }
            else if((diff.op).Equals(OP_INSERT)){
                sb.Append("<color=\"" + COLOR_INSERT + "\">").Append(afterText).Append("</color>");
            }
            else if((diff.op).Equals(OP_DELETE)){
                sb.Append("<color=\"" + COLOR_DELETE + "\">").Append(beforeText).Append("</color>");
            }
            else if((diff.op).Equals(OP_REPLACE)){
                sb.Append("<color=\"" + COLOR_REPLACE + "\">[").Append(beforeText).Append(",").Append(afterText).Append("]</color>");
            }
        }
        var html = sb.ToString();
        var ret = html.Replace("&para;<br>", "[NL]\n");
        return ret;
    }

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
        else if (!isFinished && e.type == EventType.KeyDown && e.keyCode == KeyCode.V && isPushedCtrlKey){
            Debug.Log("Copy detected");
        }
        else if (isFinished && e.type == EventType.KeyDown){
            if (e.keyCode == KeyCode.M){
                ShowMpScore();
            }
            else if(e.keyCode == KeyCode.F){
                ShowOriginalScore();
            }
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
