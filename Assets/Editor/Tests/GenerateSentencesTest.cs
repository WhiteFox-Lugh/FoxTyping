using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;
using Assert = UnityEngine.Assertions.Assert;

public class GenerateSentencesTest
{
    // 汎用チェッカー
    private void GenerateSentenceChecker(string str, int[] expectedNum){
        int LEN = expectedNum.Length;
        var gsClass = typeof(GenerateSentence);
        Assert.IsNotNull(gsClass);
        MethodInfo parser = gsClass.GetMethod("ParseHiraganaSentence", BindingFlags.NonPublic | BindingFlags.Static);
        MethodInfo builder = gsClass.GetMethod("ConstructTypeSentence", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(parser);
        Assert.IsNotNull(builder);

        // 区切りのチェック
        var separate = (List<string>) parser.Invoke(this, new object[] {str});
        Assert.AreEqual(separate.Count, LEN);

        // 生成した判定のチェック
        var matrix = (List<List<string>>) builder.Invoke(this, new object[] {separate});
        Assert.AreEqual(LEN, matrix.Count);
        for (int i = 0; i < matrix.Count; ++i){
            Assert.AreEqual(expectedNum[i], matrix[i].Count);
        }
    }

    // テスト集
    // 特に「っ」「ん」を含むものはテストに追加すると強固
    [Test]
    public void GenerateSentenceTest01(){
        var testStr = "あいうえおかきくけこさしすせそたちつてとなにぬねの";
        var expectedNum = new int[25] {
            1, 2, 3, 1, 1,
            2, 1, 3, 1, 2,
            1, 3, 1, 2, 1,
            1, 2, 2, 1, 1,
            1, 1, 1, 1, 1
        };
        GenerateSentenceChecker(testStr, expectedNum);
    }

    [Test]
    public void GenerateSentenceTest02(){
        var testStr = "はひふへほまみむめもやゆよらりるれろわゐゑをん";
        var expectedNum = new int[23] {
            1, 1, 2, 1, 1,
            1, 1, 1, 1, 1,
            1, 1, 1,
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 2
        };
        GenerateSentenceChecker(testStr, expectedNum);
    }

    [Test]
    public void GenerateSentenceTest03(){
        var testStr = "がぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽ";
        var expectedNum = new int[25] {
            1, 1, 1, 1, 1,
            1, 2, 1, 1, 1,
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1
        };
        GenerateSentenceChecker(testStr, expectedNum);
    }

    [Test]
    public void GenerateSentenceTest04(){
        var testStr = "びゃっこしろきつねこゃーん";
        var expectedNum = new int[11] {
            3, 10, 3, 1, 1,
            2, 1, 2, 2, 1,
            2,
        };
        GenerateSentenceChecker(testStr, expectedNum);
    }

    [Test]
    public void GenerateSentenceTest05(){
        var testStr = "りっしゅんがせっきんする";
        var expectedNum = new int[7] {
            1, 40, 3, 2, 5,
            3, 1,
        };
        GenerateSentenceChecker(testStr, expectedNum);
    }

    [Test]
    public void GenerateSentenceTest06(){
        var testStr = "わくちんせっしゅで5Gにせつぞくできん";
        var expectedNum = new int[16] {
            1, 3, 2, 6, 40,
            1, 1, 1, 1, 2,
            2, 1, 3, 1, 1,
            2
        };
        GenerateSentenceChecker(testStr, expectedNum);
    }

    [Test]
    public void GenerateSentenceTest07(){
        var testStr = "ちょっとだけかふぇによってこーひーをのむ";
        var expectedNum = new int[16] {
            7, 5, 1, 1, 2,
            10, 1, 1, 5, 2,
            1, 1, 1, 1, 1,
            1
        };
        GenerateSentenceChecker(testStr, expectedNum);
    }

    [Test]
    public void GenerateSentenceTest08(){
        var testStr = "でぃすぺんさー";
        var expectedNum = new int[5] {
            5, 1, 1, 3, 1
        };
        GenerateSentenceChecker(testStr, expectedNum);
    }

    [Test]
    public void GenerateSentenceTest09(){
        var testStr = "はんにゃしんぎょう";
        var expectedNum = new int[5] {
            1, 6, 3, 9, 3
        };
        GenerateSentenceChecker(testStr, expectedNum);
    }

    [Test]
    public void GenerateSentenceTest10(){
        var testStr = "んごろんごろしぜんほごく";
        var expectedNum = new int[9] {
            3, 1, 3, 1, 3,
            1, 3, 1, 3
        };
        GenerateSentenceChecker(testStr, expectedNum);
    }

    [Test]
    public void GenerateSentenceTest11(){
        var testStr = "ついったーにばかったーがちょうりょうばっこする";
        var expectedNum = new int[18] {
            2, 2, 5, 1, 1,
            1, 2, 5, 1, 1,
            7, 3, 3, 3, 1,
            10, 1, 1
        };
        GenerateSentenceChecker(testStr, expectedNum);
    }

    [Test]
    public void GenerateSentenceTest12(){
        var testStr = "ぶるーたす、おまえもか!?！？";
        var expectedNum = new int[15] {
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1,
            2, 1, 1, 1, 1
        };
        GenerateSentenceChecker(testStr, expectedNum);
    }

    [Test]
    public void GenerateSentenceTest13(){
        var testStr = "すぷりっつぁー";
        var expectedNum = new int[5] {
            1, 1, 1, 25, 1
        };
        GenerateSentenceChecker(testStr, expectedNum);
    }

    [Test]
    public void GenerateSentenceTest14(){
        var testStr = "ぐっないいっぬ";
        var expectedNum = new int[5] {
            1, 4, 2, 2, 4
        };
        GenerateSentenceChecker(testStr, expectedNum);
    }

    [Test]
    public void GenerateSentenceTest15(){
        var testStr = "3.1415926535897";
        var expectedNum = new int[15] {
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1
        };
        GenerateSentenceChecker(testStr, expectedNum);
    }

    [Test]
    public void GenerateSentenceTest16(){
        var testStr = "The quick brown fox jumps over the lazy dog";
        var expectedNum = new int[43] {
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1
        };
        GenerateSentenceChecker(testStr, expectedNum);
    }

    [Test]
    public void GenerateSentenceTest17(){
        var testStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var expectedNum = new int[26] {
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1
        };
        GenerateSentenceChecker(testStr, expectedNum);
    }

    [Test]
    public void GenerateSentenceTest18(){
        var testStr = " -,.;:[]@/!?\"#$%&\'()=~|`{}+*<>_";
        var expectedNum = new int[31] {
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1
        };
        GenerateSentenceChecker(testStr, expectedNum);
    }
}
