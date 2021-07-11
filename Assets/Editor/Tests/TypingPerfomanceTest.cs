using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;
using Assert = UnityEngine.Assertions.Assert;

public class TypingPerfomanceTest
{
	const double EPS = 1.0e-8f;
	const int TEST_SENTENCE_NUM = 10;
	private List<string> testSentenceOriginList = new List<string>() {
		"あいうえお", "あいうえお", "あいうえお", "あいうえお", "あいうえお",
		"あいうえお", "あいうえお", "あいうえお", "あいうえお", "あいうえお"
	};
	private List<string> testSentenceTypeList = new List<string>() {
		"aiueo", "aitueo", "aiueao", "raiueo", "aaaaaaiueo",
		"aiueeeeeeo", "qqqqqaiueo", "aiueo", "aiueo", "aiueo"
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
		new List<int>() {1, 1, 1, 1, 1},
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
		new List<double>() {1.0, 2.0, 3.0, 4.0, 6.0},
		new List<double>() {1.0, 3.0, 5.0, 8.0, 11.0},
	};
	private static TypingPerformance TP;

	private void BeforeTest() {
		TP = new TypingPerformance();
		var type = TP.GetType();
		Assert.IsNotNull(TP);
		// テストデータを挿入
		for (int i = 0; i < TEST_SENTENCE_NUM; ++i){
			TP.AddOriginSentence(testSentenceOriginList[i]);
			TP.AddTypedSentenceList(testSentenceTypeList[i]);
			TP.AddTypeJudgeList(testJudgeList[i]);
			TP.AddTypeTimeList(testTypeTimeList[i]);
		}

		// 正しく挿入されてるかチェック
		var prop1 = type.GetProperty("OriginSentenceList");
		var value1 = (List<string>)prop1.GetValue(TP);
		for (int i = 0; i < TEST_SENTENCE_NUM; ++i){
			Assert.IsTrue(value1[i].Equals(testSentenceOriginList[i]));
		}
		var prop2 = type.GetProperty("TypedSentenceList");
		var value2 = (List<string>)prop2.GetValue(TP);
		for (int i = 0; i < TEST_SENTENCE_NUM; ++i){
			Assert.IsTrue(value2[i].Equals(testSentenceTypeList[i]));
		}
		var prop3 = type.GetProperty("TypeJudgeList");
		var value3 = (List<List<int>>)prop3.GetValue(TP);
		for (int i = 0; i < TEST_SENTENCE_NUM; ++i){
			for (int j = 0; j < testJudgeList[i].Count(); ++j){
				Assert.IsTrue(value3[i][j] == testJudgeList[i][j]);
			}
		}
		var prop4 = type.GetProperty("TypeTimeList");
		var value4 = (List<List<double>>)prop4.GetValue(TP);
		for (int i = 0; i < TEST_SENTENCE_NUM; ++i){
			for (int j = 0; j < testTypeTimeList[i].Count(); ++j){
				Assert.IsTrue(value4[i][j] == testTypeTimeList[i][j]);
			}
		}
	}

	[Test]
	public void SentenceTypeTimeTest() {
		var expectedSentenceTypeTimeList = new List<double>() {
			1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 5.0, 10.0
		};
		BeforeTest();
		var type = TP.GetType();
		MethodInfo loader = type.GetMethod("GetSentenceTypeTime");
		for (int i = 0; i < TEST_SENTENCE_NUM; ++i){
			var value = (double) loader.Invoke(TP, new object[] {i});
			double diff = value - expectedSentenceTypeTimeList[i];
			bool isApproximatelyEqual = Math.Abs(diff) < EPS;
			Assert.IsTrue(isApproximatelyEqual);
		}
	}

	[Test]
	public void SentenceKPMTest() {
		var expectedSentenceKPMList = new List<double>() {
			300.0, 300.0, 300.0, 300.0, 300.0, 300.0, 300.0, 150.0, 60.0, 30.0
		};
		BeforeTest();
		var type = TP.GetType();
		MethodInfo loader = type.GetMethod("GetSentenceKPM");
		for (int i = 0; i < TEST_SENTENCE_NUM; ++i){
			var value = (double) loader.Invoke(TP, new object[] {i});
			double diff = value - expectedSentenceKPMList[i];
			bool isApproximatelyEqual = Math.Abs(diff) < EPS;
			Assert.IsTrue(isApproximatelyEqual);
		}
	}

	[Test]
	public void SentenceCorrectAndMistypeNumTest() {
		var expectedNum = new List<(int correct, int mistype)>() {
			(5, 0), (5, 1), (5, 1), (5, 1), (5, 5),
			(5, 5), (5, 5), (5, 0), (5, 0), (5, 0)
		};
		BeforeTest();
		var type = TP.GetType();
		MethodInfo loader = type.GetMethod("GetSentenceCorrectAndMistypeNum");
		for (int i = 0; i < TEST_SENTENCE_NUM; ++i){
			var value = ((int correct, int mistype)) loader.Invoke(TP, new object[] {i});
			Assert.AreEqual(value.correct, expectedNum[i].correct);
			Assert.AreEqual(value.mistype, expectedNum[i].mistype);
		}
	}
}
