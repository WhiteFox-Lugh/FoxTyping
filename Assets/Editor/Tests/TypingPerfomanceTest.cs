using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

public class TypingPerfomanceTest
{
  const double EPS = 1.0e-8f;
  const int TEST_SENTENCE_NUM = 10;
  private List<string> testSentenceOriginList = new List<string>() {
    "あいうえお", "あいうえお", "あいうえお", "あいうえお", "あいうえお",
    "あいうえお", "あいうえお", "12345", "12345", "あいうえお"
  };
  private List<string> testSentenceTypeList = new List<string>() {
    "aiueo", "aitueo", "aiueao", "raiueo", "aaaaaaiueo",
    "aiueeeeeeo", "qqqqqaiueo", "12345", "abcde12345", "aiueo"
  };
  private List<List<int>> testJudgeList = new List<List<int>>() {
    new List<int>() {1, 1, 1, 1, 1},
    new List<int>() {1, 1, 0, 1, 1, 1},
    new List<int>() {1, 1, 1, 1, 0, 1},
    new List<int>() {0, 1, 1, 1, 1, 1},
    new List<int>() {1, 0, 0, 0, 0, 0, 1, 1, 1, 1},
    new List<int>() {1, 1, 1, 1, 0, 0, 0, 0, 0, 1},
    new List<int>() {0, 0, 0, 0, 0, 1, 1, 1, 1, 1},
    new List<int>() {1, 1, 1, 1, 1},
    new List<int>() {0, 0, 0, 0, 0, 1, 1, 1, 1, 1},
    new List<int>() {1, 1, 1, 1, 1}
  };
  private List<List<double>> testTypeTimeList = new List<List<double>>() {
    new List<double>() {1.0, 1.25, 1.5, 1.75, 2.0},
    new List<double>() {1.0, 1.25, 1.5, 1.6, 1.75, 2.0},
    new List<double>() {1.0, 1.25, 1.5, 1.75, 1.8, 2.0},
    new List<double>() {1.0, 1.1, 1.25, 1.5, 1.75, 2.0},
    new List<double>() {1.0, 1.1, 1.2, 1.3, 1.4, 1.5, 1.65, 1.75, 1.85, 2.0},
    new List<double>() {1.0, 1.1, 1.2, 1.3, 1.4, 1.5, 1.65, 1.75, 1.85, 2.0},
    new List<double>() {1.0, 1.1, 1.2, 1.3, 1.4, 1.5, 1.65, 1.75, 1.85, 2.0},
    new List<double>() {1.0, 1.5, 2.0, 2.5, 3.0},
    new List<double>() {1.0, 1.5, 2.0, 2.5, 3.0, 3.5, 4.0, 4.5, 5.0, 6.0},
    new List<double>() {1.0, 3.0, 5.0, 8.0, 11.0},
  };
  private static TypingPerformance TP;

  private void BeforeTest()
  {
    TP = new TypingPerformance();
    var type = TP.GetType();
    Assert.IsNotNull(TP);
    // テストデータを挿入
    for (int i = 0; i < TEST_SENTENCE_NUM; ++i)
    {
      TP.AddOriginSentence(testSentenceOriginList[i]);
      TP.AddTypedSentenceList(testSentenceTypeList[i]);
      TP.AddTypeJudgeList(testJudgeList[i]);
      TP.AddTypeTimeList(testTypeTimeList[i]);
    }

    // 正しく挿入されてるかチェック
    var prop1 = type.GetProperty("OriginSentenceList");
    var value1 = (List<string>)prop1.GetValue(TP);
    for (int i = 0; i < TEST_SENTENCE_NUM; ++i)
    {
      Assert.IsTrue(value1[i].Equals(testSentenceOriginList[i]));
    }
    var prop2 = type.GetProperty("TypedSentenceList");
    var value2 = (List<string>)prop2.GetValue(TP);
    for (int i = 0; i < TEST_SENTENCE_NUM; ++i)
    {
      Assert.IsTrue(value2[i].Equals(testSentenceTypeList[i]));
    }
    var prop3 = type.GetProperty("TypeJudgeList");
    var value3 = (List<List<int>>)prop3.GetValue(TP);
    for (int i = 0; i < TEST_SENTENCE_NUM; ++i)
    {
      for (int j = 0; j < testJudgeList[i].Count(); ++j)
      {
        Assert.IsTrue(value3[i][j] == testJudgeList[i][j]);
      }
    }
    var prop4 = type.GetProperty("TypeTimeList");
    var value4 = (List<List<double>>)prop4.GetValue(TP);
    for (int i = 0; i < TEST_SENTENCE_NUM; ++i)
    {
      for (int j = 0; j < testTypeTimeList[i].Count(); ++j)
      {
        Assert.IsTrue(value4[i][j] == testTypeTimeList[i][j]);
      }
    }
  }

  [Test]
  public void SentenceTypeTimeTest()
  {
    var expectedSentenceTypeTimeList = new List<double>() {
      1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 5.0, 10.0
    };
    BeforeTest();
    var type = TP.GetType();
    MethodInfo loader = type.GetMethod("GetSentenceTypeTime");
    for (int i = 0; i < TEST_SENTENCE_NUM; ++i)
    {
      var value = (double)loader.Invoke(TP, new object[] { i });
      double diff = value - expectedSentenceTypeTimeList[i];
      bool isApproximatelyEqual = Math.Abs(diff) < EPS;
      Assert.IsTrue(isApproximatelyEqual);
    }
  }

  [Test]
  public void SentenceKPMTest()
  {
    var expectedSentenceKPMList = new List<double>() {
      300.0, 300.0, 300.0, 300.0, 300.0, 300.0, 300.0, 150.0, 60.0, 30.0
    };
    BeforeTest();
    var type = TP.GetType();
    MethodInfo loader = type.GetMethod("GetSentenceKPM");
    for (int i = 0; i < TEST_SENTENCE_NUM; ++i)
    {
      var value = (double)loader.Invoke(TP, new object[] { i });
      double diff = value - expectedSentenceKPMList[i];
      bool isApproximatelyEqual = Math.Abs(diff) < EPS;
      Assert.IsTrue(isApproximatelyEqual);
    }
  }

  [Test]
  public void SentenceCorrectAndMistypeNumTest()
  {
    var expectedNum = new List<(int correct, int mistype)>() {
      (5, 0), (5, 1), (5, 1), (5, 1), (5, 5),
      (5, 5), (5, 5), (5, 0), (5, 5), (5, 0)
    };
    BeforeTest();
    var type = TP.GetType();
    MethodInfo loader = type.GetMethod("GetSentenceCorrectAndMistypeNum");
    for (int i = 0; i < TEST_SENTENCE_NUM; ++i)
    {
      var value = ((int correct, int mistype))loader.Invoke(TP, new object[] { i });
      Assert.AreEqual(value.correct, expectedNum[i].correct);
      Assert.AreEqual(value.mistype, expectedNum[i].mistype);
    }
  }

  [Test]
  public void ColoredTypedSentenceTest()
  {
    var expected = new List<string>() {
      "aiueo",
      "ai<color=red>t</color>ueo",
      "aiue<color=red>a</color>o",
      "<color=red>r</color>aiueo",
      "a<color=red>aaaaa</color>iueo",
      "aiue<color=red>eeeee</color>o",
      "<color=red>qqqqq</color>aiueo",
      "12345",
      "<color=red>abcde</color>12345",
      "aiueo"
    };
    BeforeTest();
    var type = TP.GetType();
    MethodInfo loader = type.GetMethod("GetColoredTypedSentence");
    for (int i = 0; i < TEST_SENTENCE_NUM; ++i)
    {
      var value = (string)loader.Invoke(TP, new object[] { i });
      Assert.AreEqual(value, expected[i]);
    }
  }

  /// <summary>
  /// FuncAcc のテスト用(共通処理)
  /// </summary>
  private bool CheckFuncAccValue(double expected, (int a, int b) val)
  {
    var type = typeof(TypingPerformance);
    Assert.IsNotNull(type);
    MethodInfo method = type.GetMethod("FuncAcc", BindingFlags.NonPublic | BindingFlags.Static);
    var value = (double)method.Invoke(TP, new object[] { (val.a, val.b) });
    double diff = value - expected;
    UnityEngine.Debug.Log($"value -> {value}, expected -> {expected}");
    bool isApproximatelyEqual = Math.Abs(diff) < EPS;
    return isApproximatelyEqual;
  }

  [Test]
  public void FuncAccTest01()
  {
    Assert.IsTrue(CheckFuncAccValue(1.0, (100, 0)));
  }

  [Test]
  public void FuncAccTest02()
  {
    Assert.IsTrue(CheckFuncAccValue(0.65, (85, 15)));
  }

  [Test]
  public void FuncAccTest03()
  {
    Assert.IsTrue(CheckFuncAccValue(0.8723021333, (90, 10)));
  }

  [Test]
  public void FuncAccTest04()
  {
    Assert.IsTrue(CheckFuncAccValue(0.96680188877, (95, 5)));
  }

  [Test]
  public void FuncAccTest05()
  {
    Assert.IsTrue(CheckFuncAccValue(0.33319811122, (75, 25)));
  }

  [Test]
  public void StdDevTest()
  {
    // 仕様に則って計算した結果
    var expectedAvg = 3.9;
    var expectedStdDev = 1.74355957742;
    BeforeTest();
    var type = TP.GetType();
    MethodInfo loader = type.GetMethod("GetKpmAverageAndStdDev");
    var value = ((double kpsAvg, double kpsStdDev))loader.Invoke(TP, null);
    bool isAvgApproximatelyEqual = Math.Abs(value.kpsAvg - expectedAvg) < EPS;
    bool isStdDevApproximatelyEqual = Math.Abs(value.kpsStdDev - expectedStdDev) < EPS;
    Assert.IsTrue(isAvgApproximatelyEqual);
    Assert.IsTrue(isStdDevApproximatelyEqual);
  }

  [Test]
  public void GetElapsedTimeTest()
  {
    double expected = 24.0;
    BeforeTest();
    var type = TP.GetType();
    MethodInfo loader = type.GetMethod("GetElapsedTime");
    var value = (double)loader.Invoke(TP, null);
    double diff = value - expected;
    bool isApproximatelyEqual = Math.Abs(diff) < EPS;
    Assert.IsTrue(isApproximatelyEqual);
  }

  [Test]
  public void GetAccuracyTest()
  {
    double expected = 68.4931506849315;
    BeforeTest();
    var type = TP.GetType();
    MethodInfo loader = type.GetMethod("GetAccuracy");
    var value = (double)loader.Invoke(TP, null);
    double diff = value - expected;
    bool isApproximatelyEqual = Math.Abs(diff) < EPS;
    Assert.IsTrue(isApproximatelyEqual);
  }

  [Test]
  [Category("ScoreTest")]
  public void ScoreTest01()
  {
    // データ作成
    // 20ワード、300kpm固定
    int wordNum = 20;
    TP = new TypingPerformance();
    var type = TP.GetType();
    Assert.IsNotNull(TP);
    var dataOrigin = new List<string>();
    var dataTyped = new List<string>();
    var dataJudge = new List<List<int>>();
    var dataTime = new List<List<double>>();
    for (int i = 0; i < wordNum; ++i)
    {
      dataOrigin.Add("あいうえお");
      dataTyped.Add("aiueo");
      dataJudge.Add(new List<int>() { 1, 1, 1, 1, 1 });
      dataTime.Add(new List<double>() { 1.0, 1.25, 1.5, 1.75, 2.0 });
    }
    for (int i = 0; i < wordNum; ++i)
    {
      TP.AddOriginSentence(dataOrigin[i]);
      TP.AddTypedSentenceList(dataTyped[i]);
      TP.AddTypeJudgeList(dataJudge[i]);
      TP.AddTypeTimeList(dataTime[i]);
    }
    var expectedScore = Math.Floor(300.00);

    // 計算と判定
    MethodInfo loader = type.GetMethod("GetNormalScore");
    var value = (int)loader.Invoke(TP, null);
    Assert.AreEqual(value, expectedScore);
  }

  [Test]
  [Category("ScoreTest")]
  public void ScoreTest02()
  {
    // データ作成
    // 25ワード、300kpm固定
    int wordNum = 25;
    TP = new TypingPerformance();
    var type = TP.GetType();
    Assert.IsNotNull(TP);
    var dataOrigin = new List<string>();
    var dataTyped = new List<string>();
    var dataJudge = new List<List<int>>();
    var dataTime = new List<List<double>>();
    for (int i = 0; i < wordNum; ++i)
    {
      dataOrigin.Add("あいうえお");
      dataTyped.Add("aiueo");
      dataJudge.Add(new List<int>() { 1, 1, 1, 1, 1 });
      dataTime.Add(new List<double>() { 1.0, 1.25, 1.5, 1.75, 2.0 });
    }
    for (int i = 0; i < wordNum; ++i)
    {
      TP.AddOriginSentence(dataOrigin[i]);
      TP.AddTypedSentenceList(dataTyped[i]);
      TP.AddTypeJudgeList(dataJudge[i]);
      TP.AddTypeTimeList(dataTime[i]);
    }
    var expectedScore = Math.Floor(300.00);

    // 計算と判定
    MethodInfo loader = type.GetMethod("GetNormalScore");
    var value = (int)loader.Invoke(TP, null);
    Assert.AreEqual(value, expectedScore);
  }

  [Test]
  [Category("ScoreTest")]
  public void ScoreTest03()
  {
    // データ作成
    // 100ワード、300kpm固定
    int wordNum = 100;
    TP = new TypingPerformance();
    var type = TP.GetType();
    Assert.IsNotNull(TP);
    var dataOrigin = new List<string>();
    var dataTyped = new List<string>();
    var dataJudge = new List<List<int>>();
    var dataTime = new List<List<double>>();
    for (int i = 0; i < wordNum; ++i)
    {
      dataOrigin.Add("あいうえお");
      dataTyped.Add("aiueo");
      dataJudge.Add(new List<int>() { 1, 1, 1, 1, 1 });
      dataTime.Add(new List<double>() { 1.0, 1.25, 1.5, 1.75, 2.0 });
    }
    for (int i = 0; i < wordNum; ++i)
    {
      TP.AddOriginSentence(dataOrigin[i]);
      TP.AddTypedSentenceList(dataTyped[i]);
      TP.AddTypeJudgeList(dataJudge[i]);
      TP.AddTypeTimeList(dataTime[i]);
    }
    var expectedScore = Math.Floor(300.00);

    // 計算と判定
    MethodInfo loader = type.GetMethod("GetNormalScore");
    var value = (int)loader.Invoke(TP, null);
    Assert.AreEqual(value, expectedScore);
  }

  [Test]
  [Category("ScoreTest")]
  public void ScoreTest04()
  {
    // データ作成
    // 5ワード、300kpm固定
    int wordNum = 5;
    TP = new TypingPerformance();
    var type = TP.GetType();
    Assert.IsNotNull(TP);
    var dataOrigin = new List<string>();
    var dataTyped = new List<string>();
    var dataJudge = new List<List<int>>();
    var dataTime = new List<List<double>>();
    for (int i = 0; i < wordNum; ++i)
    {
      dataOrigin.Add("あいうえお");
      dataTyped.Add("aiueo");
      dataJudge.Add(new List<int>() { 1, 1, 1, 1, 1 });
      dataTime.Add(new List<double>() { 1.0, 1.25, 1.5, 1.75, 2.0 });
    }
    for (int i = 0; i < wordNum; ++i)
    {
      TP.AddOriginSentence(dataOrigin[i]);
      TP.AddTypedSentenceList(dataTyped[i]);
      TP.AddTypeJudgeList(dataJudge[i]);
      TP.AddTypeTimeList(dataTime[i]);
    }
    var expectedScore = Math.Floor(108.75);

    // 計算と判定
    MethodInfo loader = type.GetMethod("GetNormalScore");
    var value = (int)loader.Invoke(TP, null);
    Assert.AreEqual(value, expectedScore);
  }

  [Test]
  [Category("ScoreTest")]
  public void ScoreTest05()
  {
    // データ作成
    // 10ワード、300kpm固定
    int wordNum = 10;
    TP = new TypingPerformance();
    var type = TP.GetType();
    Assert.IsNotNull(TP);
    var dataOrigin = new List<string>();
    var dataTyped = new List<string>();
    var dataJudge = new List<List<int>>();
    var dataTime = new List<List<double>>();
    for (int i = 0; i < wordNum; ++i)
    {
      dataOrigin.Add("あいうえお");
      dataTyped.Add("aiueo");
      dataJudge.Add(new List<int>() { 1, 1, 1, 1, 1 });
      dataTime.Add(new List<double>() { 1.0, 1.25, 1.5, 1.75, 2.0 });
    }
    for (int i = 0; i < wordNum; ++i)
    {
      TP.AddOriginSentence(dataOrigin[i]);
      TP.AddTypedSentenceList(dataTyped[i]);
      TP.AddTypeJudgeList(dataJudge[i]);
      TP.AddTypeTimeList(dataTime[i]);
    }
    var expectedScore = 195;

    // 計算と判定
    MethodInfo loader = type.GetMethod("GetNormalScore");
    var value = (int)loader.Invoke(TP, null);
    Assert.AreEqual(value, expectedScore);
  }

  [Test]
  [Category("ScoreTest")]
  public void ScoreTest06()
  {
    // データ作成
    // 15ワード、300kpm固定
    int wordNum = 15;
    TP = new TypingPerformance();
    var type = TP.GetType();
    Assert.IsNotNull(TP);
    var dataOrigin = new List<string>();
    var dataTyped = new List<string>();
    var dataJudge = new List<List<int>>();
    var dataTime = new List<List<double>>();
    for (int i = 0; i < wordNum; ++i)
    {
      dataOrigin.Add("あいうえお");
      dataTyped.Add("aiueo");
      dataJudge.Add(new List<int>() { 1, 1, 1, 1, 1 });
      dataTime.Add(new List<double>() { 1.0, 1.25, 1.5, 1.75, 2.0 });
    }
    for (int i = 0; i < wordNum; ++i)
    {
      TP.AddOriginSentence(dataOrigin[i]);
      TP.AddTypedSentenceList(dataTyped[i]);
      TP.AddTypeJudgeList(dataJudge[i]);
      TP.AddTypeTimeList(dataTime[i]);
    }
    var expectedScore = Math.Floor(258.75);

    // 計算と判定
    MethodInfo loader = type.GetMethod("GetNormalScore");
    var value = (int)loader.Invoke(TP, null);
    Assert.AreEqual(value, expectedScore);
  }

  [Test]
  [Category("ScoreTest")]
  public void ScoreTest07()
  {
    // データ作成
    // 20ワード、変速
    int wordNum = 20;
    TP = new TypingPerformance();
    var type = TP.GetType();
    Assert.IsNotNull(TP);
    var dataOrigin = new List<string>();
    var dataTyped = new List<string>();
    var dataJudge = new List<List<int>>();
    var dataTime = new List<List<double>>();
    for (int i = 0; i < wordNum; ++i)
    {
      dataOrigin.Add("あいうえお");
      dataTyped.Add("aiueo");
      dataJudge.Add(new List<int>() { 1, 1, 1, 1, 1 });
      dataTime.Add(new List<double>() { 1.0, 1.01, 1.02, 1.03, 1.75 + 0.05 * i });
    }
    for (int i = 0; i < wordNum; ++i)
    {
      TP.AddOriginSentence(dataOrigin[i]);
      TP.AddTypedSentenceList(dataTyped[i]);
      TP.AddTypeJudgeList(dataJudge[i]);
      TP.AddTypeTimeList(dataTime[i]);
    }
    var expectedScore = Math.Floor(282.178);

    // 計算と判定
    MethodInfo loader = type.GetMethod("GetNormalScore");
    var value = (int)loader.Invoke(TP, null);
    Assert.AreEqual(value, expectedScore);
  }

  [Test]
  [Category("ScoreTest")]
  public void ScoreTest08()
  {
    // データ作成
    // 100ワード、変速
    int wordNum = 100;
    TP = new TypingPerformance();
    var type = TP.GetType();
    Assert.IsNotNull(TP);
    var dataOrigin = new List<string>();
    var dataTyped = new List<string>();
    var dataJudge = new List<List<int>>();
    var dataTime = new List<List<double>>();
    for (int i = 0; i < wordNum; ++i)
    {
      dataOrigin.Add("あいうえお");
      dataTyped.Add("aiueo");
      dataJudge.Add(new List<int>() { 1, 1, 1, 1, 1 });
      dataTime.Add(new List<double>() { 1.0, 1.01, 1.02, 1.03, 1.5 + 0.001 * i });
    }
    for (int i = 0; i < wordNum; ++i)
    {
      TP.AddOriginSentence(dataOrigin[i]);
      TP.AddTypedSentenceList(dataTyped[i]);
      TP.AddTypeJudgeList(dataJudge[i]);
      TP.AddTypeTimeList(dataTime[i]);
    }
    var expectedScore = Math.Floor(557.449);

    // 計算と判定
    MethodInfo loader = type.GetMethod("GetNormalScore");
    var value = (int)loader.Invoke(TP, null);
    Assert.AreEqual(value, expectedScore);
  }

  [Test]
  [Category("ScoreTest")]
  public void ScoreTest09()
  {
    // データ作成
    // 10ワード、変速
    int wordNum = 10;
    TP = new TypingPerformance();
    var type = TP.GetType();
    Assert.IsNotNull(TP);
    var dataOrigin = new List<string>();
    var dataTyped = new List<string>();
    var dataJudge = new List<List<int>>();
    var dataTime = new List<List<double>>();
    for (int i = 0; i < wordNum; ++i)
    {
      dataOrigin.Add("あいうえお");
      dataTyped.Add("aiueo");
      dataJudge.Add(new List<int>() { 1, 1, 1, 1, 1 });
      dataTime.Add(new List<double>() { 1.0, 1.01, 1.02, 1.03, 1.5 + 0.001 * i });
    }
    for (int i = 0; i < wordNum; ++i)
    {
      TP.AddOriginSentence(dataOrigin[i]);
      TP.AddTypedSentenceList(dataTyped[i]);
      TP.AddTypeJudgeList(dataJudge[i]);
      TP.AddTypeTimeList(dataTime[i]);
    }
    var expectedScore = Math.Floor(386.828);

    // 計算と判定
    MethodInfo loader = type.GetMethod("GetNormalScore");
    var value = (int)loader.Invoke(TP, null);
    Assert.AreEqual(value, expectedScore);
  }

  [Test]
  [Category("ScoreTest")]
  public void ScoreTest10()
  {
    // データ作成
    // 20ワード、変速、90%精度
    int wordNum = 20;
    TP = new TypingPerformance();
    var type = TP.GetType();
    Assert.IsNotNull(TP);
    var dataOrigin = new List<string>();
    var dataTyped = new List<string>();
    var dataJudge = new List<List<int>>();
    var dataTime = new List<List<double>>();
    for (int i = 0; i < wordNum; ++i)
    {
      dataOrigin.Add("あいうえおえういあ");
      dataTyped.Add("aiueoeuiia");
      dataJudge.Add(new List<int>() { 1, 1, 1, 1, 1, 1, 1, 1, 0, 1 });
      dataTime.Add(new List<double>() { 1.0, 1.01, 1.02, 1.03, 1.04, 1.05, 1.06, 1.07, 1.08, 1.5 + 0.001 * i });
    }
    for (int i = 0; i < wordNum; ++i)
    {
      TP.AddOriginSentence(dataOrigin[i]);
      TP.AddTypedSentenceList(dataTyped[i]);
      TP.AddTypeJudgeList(dataJudge[i]);
      TP.AddTypeTimeList(dataTime[i]);
    }
    var expectedScore = Math.Floor(928.267);

    // 計算と判定
    MethodInfo loader = type.GetMethod("GetNormalScore");
    var value = (int)loader.Invoke(TP, null);
    Assert.AreEqual(value, expectedScore);
  }

  [Test]
  [Category("ScoreTest")]
  public void ScoreTest11()
  {
    // データ作成
    // 100ワード、変速、95%精度
    int wordNum = 100;
    TP = new TypingPerformance();
    var type = TP.GetType();
    Assert.IsNotNull(TP);
    var dataOrigin = new List<string>();
    var dataTyped = new List<string>();
    var dataJudge = new List<List<int>>();
    var dataTime = new List<List<double>>();
    for (int i = 0; i < wordNum; ++i)
    {
      dataOrigin.Add("あいうえおあいうえおあいうえおあいうえ");
      dataTyped.Add("aiueoaiueoaiueoaiuoe");
      dataJudge.Add(new List<int>() {
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1, 0, 1 });
      dataTime.Add(new List<double>() {
        1.0, 1.01, 1.02, 1.03, 1.04, 1.05, 1.06, 1.07, 1.08, 1.09,
        1.1, 1.11, 1.12, 1.13, 1.14, 1.15, 1.16, 1.17, 1.18, 2.0 + 0.001 * i });
    }
    for (int i = 0; i < wordNum; ++i)
    {
      TP.AddOriginSentence(dataOrigin[i]);
      TP.AddTypedSentenceList(dataTyped[i]);
      TP.AddTypeJudgeList(dataJudge[i]);
      TP.AddTypeTimeList(dataTime[i]);
    }
    var expectedScore = Math.Floor(1060.99);

    // 計算と判定
    MethodInfo loader = type.GetMethod("GetNormalScore");
    var value = (int)loader.Invoke(TP, null);
    Assert.AreEqual(value, expectedScore);
  }
  [Test]
  [Category("ScoreTest")]
  public void ScoreTest12()
  {
    // データ作成
    // 15ワード、変速、95%精度
    int wordNum = 15;
    TP = new TypingPerformance();
    var type = TP.GetType();
    Assert.IsNotNull(TP);
    var dataOrigin = new List<string>();
    var dataTyped = new List<string>();
    var dataJudge = new List<List<int>>();
    var dataTime = new List<List<double>>();
    for (int i = 0; i < wordNum; ++i)
    {
      dataOrigin.Add("あいうえおあいうえおあいうえおあいうえ");
      dataTyped.Add("aiueoaiueoaiueoaiuoe");
      dataJudge.Add(new List<int>() {
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1, 0, 1 });
      dataTime.Add(new List<double>() {
        1.0, 1.01, 1.02, 1.03, 1.04, 1.05, 1.06, 1.07, 1.08, 1.09,
        1.1, 1.11, 1.12, 1.13, 1.14, 1.15, 1.16, 1.17, 1.18, 2.0 + 0.001 * i });
    }
    for (int i = 0; i < wordNum; ++i)
    {
      TP.AddOriginSentence(dataOrigin[i]);
      TP.AddTypedSentenceList(dataTyped[i]);
      TP.AddTypeJudgeList(dataJudge[i]);
      TP.AddTypeTimeList(dataTime[i]);
    }
    var expectedScore = Math.Floor(944.941);

    // 計算と判定
    MethodInfo loader = type.GetMethod("GetNormalScore");
    var value = (int)loader.Invoke(TP, null);
    Assert.AreEqual(value, expectedScore);
  }
}
