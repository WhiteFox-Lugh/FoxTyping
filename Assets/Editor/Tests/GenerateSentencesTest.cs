using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

public class GenerateSentencesTest
{
  private static readonly HashSet<string> romanValidChar = new HashSet<string> {
    "あ", "い", "う", "え", "お", "か", "き", "く", "け", "こ", "が", "ぎ", "ぐ", "げ", "ご",
    "さ", "し", "す", "せ", "そ", "ざ", "じ", "ず", "ぜ", "ぞ", "た", "ち", "つ", "て", "と",
    "だ", "ぢ", "づ", "で", "ど", "な", "に", "ぬ", "ね", "の", "は", "ひ", "ふ", "へ", "ほ",
    "ば", "び", "ぶ", "べ", "ぼ", "ぱ", "ぴ", "ぷ", "ぺ", "ぽ", "ま", "み", "む", "め", "も",
    "や", "ゆ", "よ", "ら", "り", "る", "れ", "ろ", "わ", "を", "ん", "ぁ", "ぃ", "ぅ", "ぇ",
    "ぉ", "ゃ", "ゅ", "ょ", "っ", "ゔ", "、", "。", "！", "!", "？", "?", "-", "ー", " ",
    "　", "\'", "\"", "#", "$", "%", ",", ".", ";", ":", "(", ")",
    "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "a", "b", "c", "d", "e", "f", "g",
    "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x",
    "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O",
    "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"
  };

  private static readonly HashSet<string> jisKanaValidChar = new HashSet<string> {
    "あ", "い", "う", "え", "お", "か", "き", "く", "け", "こ", "が", "ぎ", "ぐ", "げ", "ご",
    "さ", "し", "す", "せ", "そ", "ざ", "じ", "ず", "ぜ", "ぞ", "た", "ち", "つ", "て", "と",
    "だ", "ぢ", "づ", "で", "ど", "な", "に", "ぬ", "ね", "の", "は", "ひ", "ふ", "へ", "ほ",
    "ば", "び", "ぶ", "べ", "ぼ", "ぱ", "ぴ", "ぷ", "ぺ", "ぽ", "ま", "み", "む", "め", "も",
    "や", "ゆ", "よ", "ら", "り", "る", "れ", "ろ", "わ", "を", "ん", "ぁ", "ぃ", "ぅ", "ぇ",
    "ぉ", "ゃ", "ゅ", "ょ", "っ", "ゔ", "、", "。", "「", "」", "ー", "・"
  };

  [Description("ローマ字日本語&英語のテスト")]
  [TestCase("きゃっしゅ", new int[2] { 3, 40 })]
  [TestCase("きゅうきょ", new int[3] { 3, 3, 3 })]
  [TestCase("ぎゃんぐ", new int[2] { 3, 3 })]
  [TestCase("ぎゅうにゅう", new int[4] { 3, 3, 3, 3 })]
  [TestCase("ぎょうむれんらく", new int[6] { 3, 3, 1, 1, 3, 3 })]
  [TestCase("くぁとろふぉるまっじ", new int[7] { 8, 1, 1, 5, 1, 1, 10 })]
  [TestCase("くぃーん", new int[3] { 14, 1, 2 })]
  [TestCase("しゃしょう", new int[3] { 8, 8, 3 })]
  [TestCase("しゅれでぃんがー", new int[5] { 8, 1, 5, 3, 1 })]
  [TestCase("ぱなしぇ", new int[3] { 1, 1, 14 })]
  [TestCase("じゃんぐる", new int[3] { 7, 3, 1 })]
  [TestCase("じゅんじゅわー", new int[4] { 7, 21, 1, 1 })]
  [TestCase("じぇらーと", new int[4] { 11, 1, 1, 1 })]
  [TestCase("じょばんのだんじょん", new int[6] { 7, 1, 2, 1, 21, 2 })]
  [TestCase("ちゃんく", new int[2] { 7, 9 })]
  [TestCase("ちゅうちょ", new int[3] { 7, 3, 7 })]
  [TestCase("ちぇんじ", new int[2] { 11, 6 })]
  [TestCase("つぁーり", new int[3] { 5, 1, 1 })]
  [TestCase("てぃんぱに", new int[3] { 5, 3, 1 })]
  [TestCase("でぃーぷ", new int[3] { 5, 1, 1 })]
  [TestCase("にゃんこ", new int[2] { 3, 6 })]
  [TestCase("にゅーふぁんどらんど", new int[6] { 3, 1, 5, 3, 1, 3 })]
  [TestCase("にょうぼう", new int[4] { 3, 3, 1, 3 })]
  [TestCase("ぴゅーま", new int[3] { 3, 1, 1 })]
  [TestCase("かばぴょ", new int[3] { 2, 1, 3 })]
  [TestCase("ひゃくせんれんま", new int[5] { 3, 3, 2, 3, 3 })]
  [TestCase("ひゅーりすてぃっく", new int[6] { 3, 1, 1, 1, 5, 10 })]
  [TestCase("ひょうてんか", new int[4] { 3, 3, 1, 6 })]
  [TestCase("びゃっこ", new int[2] { 3, 10 })]
  [TestCase("ごびゅう", new int[3] { 1, 3, 3 })]
  [TestCase("びょうき", new int[3] { 3, 3, 1 })]
  [TestCase("ふぃなーれ", new int[4] { 10, 1, 1, 1 })]
  [TestCase("ふゅーじょん", new int[4] { 5, 1, 7, 2 })]
  [TestCase("ふぇんしんぐ", new int[3] { 10, 9, 3 })]
  [TestCase("ふぁっくす", new int[3] { 5, 10, 1 })]
  [TestCase("ふぉっくす", new int[3] { 5, 10, 1 })]
  [TestCase("みゃんまー", new int[3] { 3, 3, 1 })]
  [TestCase("みゅんへん", new int[3] { 3, 3, 2 })]
  [TestCase("みょうしんじ", new int[4] { 3, 3, 3, 6 })]
  [TestCase("りゃんぴん", new int[3] { 3, 3, 2 })]
  [TestCase("てんりゅうじのはっぽうにらみのりゅう", new int[14] { 1, 9, 3, 2, 1, 1, 5, 3, 1, 1, 1, 1, 3, 3 })]
  [TestCase("りょこう", new int[3] { 3, 2, 3 })]
  [TestCase("うぁっと", new int[2] { 7, 5 })]
  [TestCase("うぃんたー", new int[3] { 11, 3, 1 })]
  [TestCase("のるうぇー", new int[4] { 1, 1, 14, 1 })]
  [TestCase("うぉんばっと", new int[3] { 7, 3, 5 })]
  [TestCase("ゔぁんてーじ", new int[4] { 3, 3, 1, 2 })]
  [TestCase("ゔぃぶらふぉん", new int[5] { 6, 1, 1, 5, 2 })]
  [TestCase("ゔっ", new int[2] { 1, 4 })]
  [TestCase("ゔぇねちあ", new int[4] { 6, 1, 2, 1 })]
  [TestCase("ゔぉるてーる", new int[5] { 3, 1, 1, 1, 1 })]
  [TestCase("うゐすきー", new int[5] { 3, 1, 1, 1, 1 })]
  [TestCase("ゐひもせず", new int[5] { 1, 1, 1, 2, 1 })]
  [TestCase("あんあんというざっし", new int[7] { 1, 2, 3, 2, 3, 1, 15 })]
  [TestCase("かんいけんさ", new int[4] { 2, 4, 1, 3 })]
  [TestCase("うんうんうにうむ", new int[6] { 3, 6, 6, 1, 3, 1 })]
  [TestCase("はんえん", new int[3] { 1, 2, 2 })]
  [TestCase("けんお", new int[2] { 1, 2 })]
  [TestCase("はんかんをかう", new int[5] { 1, 6, 3, 2, 3 })]
  [TestCase("げんきんひゃくまんえん", new int[7] { 1, 3, 9, 3, 1, 2, 2 })]
  [TestCase("しんくうぱっく", new int[5] { 3, 9, 3, 1, 10 })]
  [TestCase("はんけんもの", new int[4] { 1, 3, 3, 1 })]
  [TestCase("れんこん", new int[3] { 1, 6, 2 })]
  [TestCase("さんさい", new int[3] { 1, 3, 2 })]
  [TestCase("ざんしんな", new int[3] { 1, 9, 2 })]
  [TestCase("たんす", new int[2] { 1, 3 })]
  [TestCase("かんせい", new int[3] { 2, 6, 2 })]
  [TestCase("さんそますく", new int[5] { 1, 3, 1, 1, 3 })]
  [TestCase("かんたんのため", new int[5] { 2, 3, 2, 1, 1 })]
  [TestCase("けんちんじる", new int[4] { 1, 6, 6, 1 })]
  [TestCase("めんつゆ", new int[3] { 1, 6, 1 })]
  [TestCase("はんてん", new int[3] { 1, 3, 2 })]
  [TestCase("はんと", new int[2] { 1, 3 })]
  [TestCase("けんにんじ", new int[3] { 1, 2, 6 })]
  [TestCase("かんぬ", new int[2] { 2, 2 })]
  [TestCase("いんねんのたいけつ", new int[7] { 2, 2, 2, 1, 2, 1, 2 })]
  [TestCase("はんはん", new int[3] { 1, 3, 2 })]
  [TestCase("ごせんふ", new int[3] { 1, 2, 6 })]
  [TestCase("めんぜんほんいつ", new int[5] { 1, 3, 3, 4, 2 })]
  [TestCase("じんみん", new int[3] { 2, 3, 2 })]
  [TestCase("かんむてんのう", new int[5] { 2, 3, 1, 2, 3 })]
  [TestCase("せんめんじょ", new int[3] { 2, 3, 21 })]
  [TestCase("だいかんやま", new int[5] { 1, 2, 2, 2, 1 })]
  [TestCase("いんゆ", new int[2] { 2, 2 })]
  [TestCase("かんよ", new int[2] { 2, 2 })]
  [TestCase("るんるん", new int[3] { 1, 3, 2 })]
  [TestCase("だんろ", new int[2] { 1, 3 })]
  [TestCase("しんわ", new int[2] { 3, 3 })]
  [TestCase("にほんぎんこう", new int[5] { 1, 1, 3, 6, 3 })]
  [TestCase("けんげんをにぎる", new int[6] { 1, 3, 3, 1, 1, 1 })]
  [TestCase("あたらしいげんごうは、れいわであります。", new int[19] { 1, 1, 1, 3, 2, 1, 3, 3, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1 })]
  [TestCase("さんざんなめにあう", new int[7] { 1, 3, 2, 1, 1, 1, 3 })]
  [TestCase("しんずい", new int[3] { 3, 3, 2 })]
  [TestCase("いずくんぞ", new int[4] { 2, 1, 3, 3 })]
  [TestCase("はんだごて", new int[4] { 1, 3, 1, 1 })]
  [TestCase("ぢんぢんする", new int[4] { 1, 3, 3, 1 })]
  [TestCase("がんづぎ", new int[3] { 1, 3, 1 })]
  [TestCase("はんで", new int[2] { 1, 3 })]
  [TestCase("あんどん", new int[3] { 1, 3, 2 })]
  [TestCase("おんびん", new int[3] { 1, 3, 2 })]
  [TestCase("はんぶん", new int[3] { 1, 3, 2 })]
  [TestCase("ひゃくまんべん", new int[5] { 3, 3, 1, 3, 2 })]
  [TestCase("はんぼうき", new int[4] { 1, 3, 3, 1 })]
  [TestCase("ちんぱんじー", new int[4] { 2, 3, 6, 1 })]
  [TestCase("いもけんぴ", new int[4] { 2, 1, 1, 3 })]
  [TestCase("はんぷ", new int[2] { 1, 3 })]
  [TestCase("こんぺ", new int[2] { 2, 3 })]
  [TestCase("さんぽ", new int[2] { 1, 3 })]
  [TestCase("きゃんきゃん", new int[3] { 3, 9, 2 })]
  [TestCase("ふぁんきぃー", new int[3] { 5, 15, 1 })]
  [TestCase("けんきゅう", new int[3] { 1, 9, 3 })]
  [TestCase("せんきょうんどう", new int[5] { 2, 9, 3, 3, 3 })]
  [TestCase("ぜんこくあんぎゃ", new int[5] { 1, 6, 3, 1, 9 })]
  [TestCase("しんぎゅらりてぃー", new int[6] { 3, 9, 1, 1, 5, 1 })]
  [TestCase("かんしゃ", new int[2] { 2, 24 })]
  [TestCase("せんしゅせんせい", new int[5] { 2, 24, 2, 6, 2 })]
  [TestCase("ふぃなんしぇ", new int[3] { 10, 1, 42 })]
  [TestCase("しんしょう", new int[3] { 3, 24, 3 })]
  [TestCase("ちんじゃおろーす", new int[6] { 2, 21, 1, 1, 1, 1 })]
  [TestCase("もんじゅ", new int[2] { 1, 21 })]
  [TestCase("えんじぇる", new int[3] { 1, 33, 1 })]
  [TestCase("せんじょうち", new int[4] { 2, 21, 3, 2 })]
  [TestCase("せんちゃく", new int[3] { 2, 21, 3 })]
  [TestCase("はんちゅう", new int[3] { 1, 21, 3 })]
  [TestCase("こんちぇると", new int[4] { 2, 33, 1, 1 })]
  [TestCase("ぽんちょ", new int[2] { 1, 21 })]
  [TestCase("ぱんつぁねっら", new int[4] { 1, 15, 1, 5 })]
  [TestCase("えんてぃてぃー", new int[4] { 1, 15, 5, 1 })]
  [TestCase("いんでぃご", new int[3] { 2, 15, 1 })]
  [TestCase("ひんどぅーきょう", new int[5] { 1, 9, 1, 3, 3 })]
  [TestCase("はんにゃしんぎょう", new int[5] { 1, 6, 3, 9, 3 })]
  [TestCase("れんにゅう", new int[3] { 1, 6, 3 })]
  [TestCase("しんにょう", new int[3] { 3, 6, 3 })]
  [TestCase("ぼんぴゃく", new int[3] { 1, 9, 3 })]
  [TestCase("こんぴゅーた", new int[4] { 2, 9, 1, 1 })]
  [TestCase("かんぴょう", new int[3] { 2, 9, 3 })]
  [TestCase("とうひょうすうさんひょう", new int[9] { 1, 3, 3, 3, 1, 3, 1, 9, 3 })]
  [TestCase("かんふぁれんす", new int[4] { 2, 15, 1, 3 })]
  [TestCase("いんふぉめーしょん", new int[6] { 2, 15, 1, 1, 8, 2 })]
  [TestCase("さんみゃく", new int[3] { 1, 9, 3 })]
  [TestCase("しんみょう", new int[3] { 3, 9, 3 })]
  [TestCase("せんりゃく", new int[3] { 2, 9, 3 })]
  [TestCase("せんりゅう", new int[3] { 2, 9, 3 })]
  [TestCase("まんりょう", new int[3] { 1, 9, 3 })]
  [TestCase("さいん、こさいん、たんじぇんと", new int[10] { 1, 2, 3, 2, 1, 2, 3, 1, 33, 3 })]
  [TestCase("でんこうせっか", new int[5] { 1, 6, 3, 2, 10 })]
  [TestCase("せっきじだい", new int[5] { 2, 5, 2, 1, 2 })]
  [TestCase("ほっけふらい", new int[5] { 1, 5, 2, 1, 2 })]
  [TestCase("とびっこ", new int[3] { 1, 1, 10 })]
  [TestCase("とっさ", new int[2] { 1, 5 })]
  [TestCase("ばっすい", new int[3] { 1, 5, 2 })]
  [TestCase("まろんぐらっせ", new int[5] { 1, 1, 3, 1, 10 })]
  [TestCase("ちっそ", new int[2] { 2, 5 })]
  [TestCase("くららがたった", new int[6] { 3, 1, 1, 1, 1, 5 })]
  [TestCase("はいたっち", new int[4] { 1, 2, 1, 10 })]
  [TestCase("げっつー", new int[3] { 1, 10, 1 })]
  [TestCase("みがって", new int[3] { 1, 1, 5 })]
  [TestCase("はっととりっく", new int[5] { 1, 5, 1, 1, 10 })]
  [TestCase("いっぬ", new int[2] { 2, 4 })]
  [TestCase("ばっは", new int[2] { 1, 5 })]
  [TestCase("りーびっひれいきゃくき", new int[9] { 1, 1, 1, 5, 1, 2, 3, 3, 1 })]
  [TestCase("すたっふ", new int[3] { 1, 1, 10 })]
  [TestCase("えっへん", new int[3] { 1, 5, 2 })]
  [TestCase("ごっほ", new int[2] { 1, 5 })]
  [TestCase("れっぐ", new int[2] { 1, 5 })]
  [TestCase("どっご", new int[2] { 1, 5 })]
  [TestCase("えっじ", new int[2] { 1, 10 })]
  [TestCase("おっず", new int[2] { 1, 5 })]
  [TestCase("ぶっだ", new int[2] { 1, 5 })]
  [TestCase("はりうっど", new int[4] { 1, 1, 3, 5 })]
  [TestCase("せっぱつまる", new int[5] { 2, 5, 2, 1, 1 })]
  [TestCase("はっぴー", new int[3] { 1, 5, 1 })]
  [TestCase("にいかっぷ", new int[4] { 1, 2, 2, 5 })]
  [TestCase("ふらっぺ", new int[3] { 2, 1, 5 })]
  [TestCase("かっぽ", new int[2] { 2, 5 })]
  [TestCase("しっきゃく", new int[3] { 3, 15, 3 })]
  [TestCase("くっきぃー", new int[3] { 3, 25, 1 })]
  [TestCase("さっきゅう", new int[3] { 1, 15, 3 })]
  [TestCase("らっきょう", new int[3] { 1, 15, 3 })]
  [TestCase("せっしゃ", new int[2] { 2, 40 })]
  [TestCase("よっしぃ", new int[2] { 1, 65 })]
  [TestCase("せっしゅ", new int[2] { 2, 40 })]
  [TestCase("くれっしぇんど", new int[4] { 3, 1, 70, 3 })]
  [TestCase("がっしょう", new int[3] { 1, 40, 3 })]
  [TestCase("まっちゃ", new int[2] { 1, 35 })]
  [TestCase("はっちゅうみす", new int[5] { 1, 35, 3, 1, 1 })]
  [TestCase("あっちぇれらんど", new int[5] { 1, 55, 1, 1, 3 })]
  [TestCase("はっちょうみそ", new int[5] { 1, 35, 3, 1, 1 })]
  [TestCase("すぷりっつぁー", new int[5] { 1, 1, 1, 25, 1 })]
  [TestCase("まりとっつぉ", new int[4] { 1, 1, 1, 25 })]
  [TestCase("すぱげってぃ", new int[4] { 1, 1, 1, 25 })]
  [TestCase("うっでぃー", new int[3] { 3, 25, 1 })]
  [TestCase("はっぴゃく", new int[3] { 1, 15, 3 })]
  [TestCase("すろっぴぃ", new int[3] { 1, 1, 25 })]
  [TestCase("はっぴょう", new int[3] { 1, 15, 3 })]
  [TestCase("みっふぃー", new int[3] { 1, 50, 1 })]
  [TestCase("びゅっふぇ", new int[2] { 3, 50 })]
  [TestCase("えとぴりか", new int[5] { 1, 1, 1, 1, 2 })]
  [TestCase("おのづから", new int[5] { 1, 1, 1, 2, 1 })]
  [TestCase("ぺぺろんちーの", new int[6] { 1, 1, 1, 6, 1, 1 })]
  [TestCase("やよいじだい", new int[6] { 1, 1, 2, 2, 1, 2 })]
  [TestCase("てぬぐい", new int[4] { 1, 1, 1, 2 })]
  [TestCase("べるぬーい", new int[5] { 1, 1, 1, 1, 2 })]
  [TestCase("ぞろぞろ", new int[4] { 1, 1, 1, 1 })]
  [TestCase("すっっっごい", new int[5] { 1, 4, 4, 5, 2 })]
  [TestCase("the quick brown fox jumps over the lazy dog.", new int[44] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 })]
  [TestCase("THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG.", new int[44] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 })]
  [TestCase("3.1415926535897", new int[15] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 })]
  [TestCase(" -,.;:[]@/!?\"#$%&\'()=~|`{}+*<>_", new int[31] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 })]
  public void GenerateSentenceTestRoman(string testStr, int[] expectedPattern)
  {
    int LEN = expectedPattern.Length;
    var gsClass = typeof(GenerateSentence);
    Assert.IsNotNull(gsClass);
    MethodInfo builder = gsClass.GetMethod("ConstructTypeSentence", BindingFlags.NonPublic | BindingFlags.Static);
    Assert.IsNotNull(builder);

    // 生成した判定のチェック
    var matrix = (List<List<string>>)builder.Invoke(this, new object[] { testStr });
    Assert.AreEqual(LEN, matrix.Count);
    for (int i = 0; i < matrix.Count; ++i)
    {
      Assert.AreEqual(expectedPattern[i], matrix[i].Count);
    }
  }

  [Description("JISかな用のテスト")]
  [TestCase("あいうえおかきくけこさしすせそたちつてと", "あいうえおかきくけこさしすせそたちつてと")]
  [TestCase("なにぬねのはひふへほまみむめもやゆよらりるれろわをん", "なにぬねのはひふへほまみむめもやゆよらりるれろわをん")]
  [TestCase("がぎぐげござじずぜぞだぢづでどばびぶべぼ", "か゛き゛く゛け゛こ゛さ゛し゛す゛せ゛そ゛た゛ち゛つ゛て゛と゛は゛ひ゛ふ゛へ゛ほ゛")]
  [TestCase("ぱぴぷぺぽぁぃぅぇぉゃゅょっー、。・「」", "は゜ひ゜ふ゜へ゜ほ゜ぁぃぅぇぉゃゅょっー、。・「」")]
  public void GenerateSentenceTestJisKana(string testStr, string expectedStr)
  {
    var gsClass = typeof(GenerateSentence);
    Assert.IsNotNull(gsClass);
    MethodInfo builder = gsClass.GetMethod("ConstructJISKanaTypeSentence", BindingFlags.NonPublic | BindingFlags.Static);
    Assert.IsNotNull(builder);

    // 生成した判定のチェック
    var matrix = (List<List<string>>)builder.Invoke(this, new object[] { testStr });
    var strBuilder = new StringBuilder();
    for (int i = 0; i < matrix.Count; ++i)
    {
      for (int j = 0; j < matrix[i].Count; ++j)
      {
        strBuilder.Append(matrix[i][j]);
      }
    }
    Assert.AreEqual(expectedStr, strBuilder.ToString());
  }

  [Description("ワードセットで入力不能な文字列がないかのチェック（ローマ字）")]
  [TestCase("FoxTypingOfficial")]
  [TestCase("FoxTypingOfficialEnglish")]
  [TestCase("Idiom_Jp")]
  [TestCase("Cocktail")]
  public void WordsetCheckerRoman(string fileName)
  {
    var sectionRegex = @"\{__[\w|_]+__\}";
    var jsonStr = AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/Wordset/Short/{fileName}.json");
    var problemData = JsonUtility.FromJson<SentenceData>(jsonStr.text);
    var wordSetDict = new Dictionary<string, List<(string originSentence, string typeSentence)>>();
    foreach (var word in problemData.words)
    {
      var wordSection = word.wordSection;
      var wordInfo = (word.sentence, word.typeString);
      if (wordSetDict.ContainsKey(wordSection))
      {
        wordSetDict[wordSection].Add(wordInfo);
      }
      else
      {
        wordSetDict[wordSection] = new List<(string, string)>() { wordInfo };
      }
    }
    foreach (var key in wordSetDict.Keys)
    {
      var dictCache = wordSetDict[key];
      foreach (var wordInfo in dictCache)
      {
        var tSentence = Regex.Replace(wordInfo.typeSentence, sectionRegex, "");
        foreach (var ch in tSentence)
        {
          Assert.IsTrue(romanValidChar.Contains(ch.ToString()));
        }
      }
    }
  }

  [Description("ワードセットで入力不能な文字列がないかのチェック（JISかな）")]
  [TestCase("FoxTypingOfficial")]
  [TestCase("Idiom_Jp")]
  [TestCase("Cocktail")]
  [TestCase("Informatics")]
  public void WordsetCheckerJISKana(string fileName)
  {
    var sectionRegex = @"\{__[\w|_]+__\}";
    var jsonStr = AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/Wordset/Short/{fileName}.json");
    var problemData = JsonUtility.FromJson<SentenceData>(jsonStr.text);
    var wordSetDict = new Dictionary<string, List<(string originSentence, string typeSentence)>>();
    foreach (var word in problemData.words)
    {
      var wordSection = word.wordSection;
      var wordInfo = (word.sentence, word.typeString);
      if (wordSetDict.ContainsKey(wordSection))
      {
        wordSetDict[wordSection].Add(wordInfo);
      }
      else
      {
        wordSetDict[wordSection] = new List<(string, string)>() { wordInfo };
      }
    }
    foreach (var key in wordSetDict.Keys)
    {
      var dictCache = wordSetDict[key];
      foreach (var wordInfo in dictCache)
      {
        var tSentence = Regex.Replace(wordInfo.typeSentence, sectionRegex, "");
        foreach (var ch in tSentence)
        {
          Assert.IsTrue(jisKanaValidChar.Contains(ch.ToString()));
        }
      }
    }
  }
}
