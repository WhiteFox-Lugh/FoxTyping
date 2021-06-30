using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;
using Assert = UnityEngine.Assertions.Assert;

public class LongSentenceScriptTest
{
    const string REP = "replace";
    const string DEL = "delete";
    const string INS = "insert";
    const string EQ = "equal";
    const string FILE_CONSTITUTION = "long_constitution";
    const string COLORFUL_MAKIGAMI = "あかまきがみあおまきがみきまきがみ";

    // Diff チェック関数
    private void DiffChecker(string taskStr, string inputStr, List<Diff> expectedDiffs){
        var lsClass = typeof(LongSentenceScript);
        Assert.IsNotNull(lsClass);
        MethodInfo diffParser = lsClass.GetMethod("GetDiff", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(diffParser);

        // diff 取得
        var diffList = (List<Diff>) diffParser.Invoke(this, new object[] {taskStr, inputStr});

        // debug 用
        for (int i = 0; i < diffList.Count; ++i){
            var actual = diffList[i];
            Debug.Log("[" + actual.op + ", " + actual.before + ", " + actual.after + "]");
        }
        Assert.AreEqual(diffList.Count, expectedDiffs.Count);

        // 個別に diff をチェック
        for (int i = 0; i < diffList.Count; ++i){
            var expected = expectedDiffs[i];
            var actual = diffList[i];
            Debug.Log("op : [" + expected.op + ", " + actual.op + "]");
            Debug.Log("before : [" + expected.before + ", " + actual.before + "]");
            Debug.Log("after : [" + expected.after + ", " + actual.after + "]");
            Assert.IsTrue((expected.op).Equals(actual.op));
            Assert.IsTrue((expected.before).Equals(actual.before));
            Assert.IsTrue((expected.after).Equals(actual.after));
        }
    }

    private string LoadTaskData(string fileName){
        var cls = typeof(LongSentenceScript);
        Assert.IsNotNull(cls);
        MethodInfo loader = cls.GetMethod("LoadSentenceData", BindingFlags.NonPublic | BindingFlags.Static);
        var ret = (string) loader.Invoke(this, new object[] {fileName});
        return ret;
    }

    [Test]
    [Category("CompleteMatch")]
    public void CompleteMatchTest01() {
        var taskStr = "きつね";
        var inputStr = "きつね";
        var expectedDiffs = new List<Diff> () {
            new Diff(EQ, "きつね", "")
        };
        DiffChecker(taskStr, inputStr, expectedDiffs);
    }

    [Test]
    [Category("CompleteMatch")]
    public void CompleteMatchTest02() {
        var taskStr = LoadTaskData(FILE_CONSTITUTION);
        var inputStr = taskStr;
        var expectedDiffs = new List<Diff> () {
            new Diff(EQ, taskStr, "")
        };
        DiffChecker(taskStr, inputStr, expectedDiffs);
    }

    [Test]
    [Category("PrefixMatch")]
    public void PrefixMatchTest01() {
        var taskStr = "きつねこんこん";
        var inputStr = "きつね";
        var expectedDiffs = new List<Diff> () {
            new Diff(EQ, "きつね", "")
        };
        DiffChecker(taskStr, inputStr, expectedDiffs);
    }

    [Test]
    [Category("PrefixMatch")]
    public void PrefixMatchTest02() {
        var taskStr = LoadTaskData(FILE_CONSTITUTION);
        var inputStr = "日本国民は、正当に選挙された国会における代表者を通じて行動し";
        var expectedDiffs = new List<Diff> () {
            new Diff(EQ, "日本国民は、正当に選挙された国会における代表者を通じて行動し", "")
        };
        DiffChecker(taskStr, inputStr, expectedDiffs);
    }

    [Test]
    [Category("SuffixMatch")]
    public void SuffixMatchTest01() {
        var taskStr = "きつねこんこん";
        var inputStr = "こんこん";
        var expectedDiffs = new List<Diff> () {
            new Diff(DEL, "きつね", ""),
            new Diff(EQ, "こんこん", "")
        };
        DiffChecker(taskStr, inputStr, expectedDiffs);
    }

    [Test]
    [Category("SuffixMatch")]
    public void SuffixMatchTest02() {
        var taskStr = "きつねこんこん";
        var inputStr = "んこん";
        var expectedDiffs = new List<Diff> () {
            new Diff(INS, "", "んこん")
        };
        DiffChecker(taskStr, inputStr, expectedDiffs);
    }

    [Test]
    [Category("SuffixMatch")]
    public void SuffixMatchTest03() {
        var taskStr = COLORFUL_MAKIGAMI;
        var inputStr = "まきがみ";
        var expectedDiffs = new List<Diff> () {
            new Diff(DEL, "あか", ""),
            new Diff(EQ, "まきがみ", ""),
        };
        DiffChecker(taskStr, inputStr, expectedDiffs);
    }

    [Test]
    [Category("MiddleMatch")]
    public void MiddleMatchTest01() {
        var taskStr = COLORFUL_MAKIGAMI;
        var inputStr = "あおまきがみ";
        var expectedDiffs = new List<Diff> () {
            new Diff(EQ, "あ", ""),
            new Diff(INS, "", "おまきがみ"),
        };
        DiffChecker(taskStr, inputStr, expectedDiffs);
    }

    [Test]
    [Category("MiddleMatch")]
    public void MiddleMatchTest02() {
        var taskStr = COLORFUL_MAKIGAMI;
        var inputStr = "あかあおまきがみ";
        var expectedDiffs = new List<Diff> () {
            new Diff(EQ, "あか", ""),
            new Diff(DEL, "まきがみ", ""),
            new Diff(EQ, "あおまきがみ", ""),
        };
        DiffChecker(taskStr, inputStr, expectedDiffs);
    }

    [Test]
    [Category("ExtremeCases")]
    public void NoneInputTest() {
        var taskStr = COLORFUL_MAKIGAMI;
        var inputStr = "";
        var expectedDiffs = new List<Diff> () {
            new Diff(EQ, "", "")
        };
        DiffChecker(taskStr, inputStr, expectedDiffs);
    }

    [Test]
    [Category("ExtremeCases")]
    public void NotMatchTest() {
        var taskStr = COLORFUL_MAKIGAMI;
        var inputStr = "いろはにほへと";
        var expectedDiffs = new List<Diff> () {
            new Diff(INS, "", "いろはにほへと")
        };
        DiffChecker(taskStr, inputStr, expectedDiffs);
    }

    [Test]
    [Category("ExtremeCases")]
    public void OvertypingTest01() {
        var taskStr = COLORFUL_MAKIGAMI;
        var inputStr = COLORFUL_MAKIGAMI + "あ";
        var expectedDiffs = new List<Diff> () {
            new Diff(EQ, COLORFUL_MAKIGAMI, ""),
            new Diff(INS, "", "あ")
        };
        DiffChecker(taskStr, inputStr, expectedDiffs);
    }

    [Test]
    [Category("ExtremeCases")]
    public void OvertypingTest02() {
        var taskStr = COLORFUL_MAKIGAMI;
        var inputStr = "あかまきがみあおまきがみあかまきがみ";
        var expectedDiffs = new List<Diff> () {
            new Diff(EQ, "あかまきがみあおまきがみ", ""),
            new Diff(REP, "き", "あ"),
            new Diff(INS, "", "か"),
            new Diff(EQ, "まきがみ", "")
        };
        DiffChecker(taskStr, inputStr, expectedDiffs);
    }

    [Test]
    [Category("ExtremeCases")]
    public void SuffixMultiMatchTest01() {
        var taskStr = COLORFUL_MAKIGAMI;
        var inputStr = "まきがみまきがみまきがみ";
        var expectedDiffs = new List<Diff> () {
            new Diff(REP, "あか", "まき"),
            new Diff(INS, "", "がみ"),
            new Diff(EQ, "まきがみ", ""),
            new Diff(DEL, "あお", ""),
            new Diff(EQ, "まきがみ", "")
        };
        DiffChecker(taskStr, inputStr, expectedDiffs);
    }

    [Test]
    [Category("ExtremeCases")]
    public void SuffixMultiMatchTest02() {
        var taskStr = COLORFUL_MAKIGAMI;
        var inputStr = "まきがみまきがみまきがみまきがみ";
        var expectedDiffs = new List<Diff> () {
            new Diff(REP, "あか", "まき"),
            new Diff(INS, "", "がみ"),
            new Diff(EQ, "まきがみ", ""),
            new Diff(DEL, "あお", ""),
            new Diff(EQ, "まきがみ", ""),
            new Diff(DEL, "き", ""),
            new Diff(EQ, "まきがみ", "")
        };
        DiffChecker(taskStr, inputStr, expectedDiffs);
    }

    [Test]
    [Category("ExtremeCases")]
    public void PrefixSuffixMiddleMatchTest01() {
        var taskStr = LoadTaskData(FILE_CONSTITUTION);
        var inputStr = "日本国民国民国民";
        var expectedDiffs = new List<Diff> () {
            new Diff(EQ, "日本国民", ""),
            new Diff(DEL, "は、正当に選挙された", ""),
            new Diff(EQ, "国", ""),
            new Diff(REP, "会", "民"),
            new Diff(INS, "", "国民"),
        };
        DiffChecker(taskStr, inputStr, expectedDiffs);
    }

    [Test]
    [Category("OrdinalCases")]
    public void OrdinalCaseTest01() {
        var taskStr = COLORFUL_MAKIGAMI;
        var inputStr = "あかまきがみあまきがみ";
        var expectedDiffs = new List<Diff> () {
            new Diff(EQ, "あかまきがみあ", ""),
            new Diff(DEL, "お", ""),
            new Diff(EQ, "まきがみ", "")
        };
        DiffChecker(taskStr, inputStr, expectedDiffs);
    }

    [Test]
    [Category("OrdinalCases")]
    public void OrdinalCaseTest02() {
        var taskStr = LoadTaskData(FILE_CONSTITUTION);
        var inputStr = "日本国民は、正答に占拠された国会における代表者を通じて行動しろよ、";
        var expectedDiffs = new List<Diff> () {
            new Diff(EQ, "日本国民は、正", ""),
            new Diff(REP, "当", "答"),
            new Diff(EQ, "に", ""),
            new Diff(REP, "選挙", "占拠"),
            new Diff(EQ, "された国会における代表者を通じて行動し", ""),
            new Diff(INS, "", "ろよ"),
            new Diff(EQ, "、", "")
        };
        DiffChecker(taskStr, inputStr, expectedDiffs);
    }

    [Test]
    [Category("OrdinalCases")]
    public void OrdinalCaseTest03() {
        var taskStr = LoadTaskData(FILE_CONSTITUTION);
        var inputStr = "国民は、正当に選挙されれた国会における代表者を通て行動しわれら";
        var expectedDiffs = new List<Diff> () {
            new Diff(DEL, "日本", ""),
            new Diff(EQ, "国民は、正当に選挙され", ""),
            new Diff(INS, "", "れ"),
            new Diff(EQ, "た国会における代表者を通", ""),
            new Diff(DEL, "じ", ""),
            new Diff(EQ, "て行動し", ""),
            new Diff(DEL, "、", ""),
            new Diff(EQ, "われら", "")
        };
        DiffChecker(taskStr, inputStr, expectedDiffs);
    }
}
