using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BeginnerModeOperate : MonoBehaviour
{
  // ビギナーモードでの練習ワード数
  private const int WORD_TASK_NUM = 30;
  // 前回の練習章の保持
  private static int prevChapterNum = 1;
  // ショートカットキーと番号のマッピング
  private static readonly Dictionary<KeyCode, int> shortCutKeyCode = new Dictionary<KeyCode, int> {
    {KeyCode.Q, 1},
    {KeyCode.W, 2},
    {KeyCode.E, 3},
    {KeyCode.R, 4},
    {KeyCode.T, 5},
    {KeyCode.Y, 6},
    {KeyCode.U, 7},
    {KeyCode.I, 8},
    {KeyCode.O, 9},
    {KeyCode.P, 10}
  };
  // ショートカットキーと章番号のマッピング
  private static readonly Dictionary<KeyCode, int> shortCutKeyCodeChapter = new Dictionary<KeyCode, int> {
    {KeyCode.Alpha1, 1},
    {KeyCode.Alpha2, 2},
    {KeyCode.Alpha3, 3},
    {KeyCode.Alpha4, 4}
  };
  private static int practiceInputMode = 0;
  private static List<List<Button>> PracticeButtonList;
  private static readonly string DefaultInstructionText = "[ ] に書かれているキーを押すと練習内容の説明が\nここに表示されるよ。\nその状態で Enter キーを押すか，ボタンをクリックすると\n練習がスタートするよ。";

  // 練習データセット
  // 先頭が章番号、下2桁がナンバリング
  private static readonly Dictionary<int, string> beginnerDatasetFileName = new Dictionary<int, string> {
    {101, "keyboardMiddle"}, {102, "keyboardUpper"}, {103, "keyboardLower"},
    {201, "alphabetLower"}, {202, "alphabetUpper"}, {203, "numberAndSymbol"}, {204, "chapter2All"},
    {301, "hiragana_a"}, {302, "hiragana_k"}, {303, "hiragana_s"},
    {304, "hiragana_t"}, {305, "hiragana_n"}, {306, "hiragana_h"},
    {307, "hiragana_m"}, {308, "hiragana_y"}, {309, "hiragana_r"}, {310, "chapter3All"},
    {401, "hiragana_ky"}, {402, "hiragana_sy"}, {403, "hiragana_ty"},
    {404, "hiragana_ny"}, {405, "hiragana_hy"}, {406, "hiragana_my"},
    {407, "hiragana_ry"}, {408, "hiragana_ltu"}, {409, "hiragana_long_vowel"}, {410, "hiragana_xn"}
  };

  // 練習内容の説明(QwertyJP)
  private static readonly Dictionary<string, string> practiceDescriptionDict = new Dictionary<string, string> {
    {"B0101", "最初はキーボードの真ん中の段を練習しよう！\nFキーに左手の人差し指、Jキーに右手の人差し指を置いて、\nそのまま横並びに指を置いて、親指はスペースキーに\n置くとホームポジションになるよ。\nどの指で押したらいいかわからなくなったら、練習中に表示されているキーボードと指を見てね。"},
    {"B0102", "次はキーボードの上の段を練習しよう！\nホームポジションのまま、少しだけ指を上に動かしてキーを押そう。"},
    {"B0103", "次はキーボードの下の段を練習しよう！\nホームポジションのまま、少しだけ指を下に動かしてキーを押そう。"},
    {"B0201", "アルファベットの小文字を練習するよ。\n最初はキーボードの表示を見ながら場所を覚えて、\n慣れてきたら表示を消して手元を見ないで打ってみよう！\n焦らず一つ一つ丁寧にね。"},
    {"B0202", "アルファベットの大文字を練習するよ。\nShiftキーを押しながらアルファベットのキーを押すと、\n大文字のアルファベットが入力できるよ。"},
    {"B0203", "数字と記号の練習をするよ。\n数字はキーボードの一番上の段にあるよ。\n数字や記号のキーは、Shiftキーを押していないときは\nキーの下に印字された文字、Shiftキーを押しながら入力すると、キーの上に印字されている文字が入力できるよ。"},
    {"B0204", "Chapter2 を全部復習できるよ。\nこれがすらすらできるようになったら、\nもうアルファベットや数字、記号の位置はだいたいマスター！"},
    {"B0301", "あ行の練習だよ。\nあいうえおはそれぞれ a、i、u、e、o のキーで打てるよ。\nまずはこの5文字をしっかり覚えよう。"},
    {"B0302", "か・が行の練習だよ。\nか行は k + a、i、u、e、oの組み合わせなんだ。\n「か」は ka、「き」は ki、「く」は ku、\n「け」は ke、「こ」は ko だよ。\nが行は g + a、i、u、e、oの組み合わせなんだ。\n「が」は ga、「ぎ」は gi、「ぐ」は gu、\n「げ」は ge、「ご」は go だよ。"},
    {"B0303", "さ・ざ行の練習だよ。\nさ行は s + a、i、u、e、oの組み合わせなんだ。\n「さ」は sa、「し」は si、「す」は su、\n「せ」は se、「そ」は so だよ。\nざ行は z + a、i、u、e、oの組み合わせなんだ。\n「ざ」は za、「じ」は zi、「ず」は zu、\n「ぜ」は ze、「ぞ」は zo だよ。\n「じ」は ji でもオッケー。"},
    {"B0304", "た・だ行の練習だよ。\nた行は t + a、i、u、e、oの組み合わせなんだ。\n「た」は ta、「ち」は ti、「つ」は tu、\n「て」は te、「と」は to だよ。\nだ行は d + a、i、u、e、oの組み合わせなんだ。\n「だ」は da、「ぢ」は di、「づ」は du、\n「で」は de、「ど」は do だよ。"},
    {"B0305", "な行の練習だよ。\nな行は n + a、i、u、e、oの組み合わせなんだ。\n「な」は na、「に」は ni、「ぬ」は nu、\n「ね」は ne、「の」は no で打てるよ。"},
    {"B0306", "は・ば・ぱ行の練習だよ。\nは行は h + あいうえお、ば行は b + あいうえお、\nぱ行は p + a、i、u、e、oの組み合わせなんだ。\n「はひふへほ」はそれぞれ ha、hi、hu(fu)、he、ho、\n「ばびぶべぼ」はそれぞれ ba、bi、bu、be、bo、\n「ぱぴぷぺぽ」はそれぞれ pa、pi、pu、pe、po で打てるよ。"},
    {"B0307", "ま行の練習だよ。\nま行は m + a、i、u、e、oの組み合わせなんだ。\n「まみむめも」はそれぞれ ma、mi、mu、me、mo で打てるよ。"},
    {"B0308", "や・わ行の練習だよ。\nや行は y + a、u、oの組み合わせなんだ。\n「やゆよ」はそれぞれ ya、yu、yo、\n「わ」は wa、「を」は wo、「ん」は nn だよ。"},
    {"B0309", "ら行の練習だよ。\nら行は r + a、i、u、e、oの組み合わせなんだ。\n「らりるれろ」はそれぞれ ra、ri、ru、re、ro で打てるよ。"},
    {"B0310", "五十音の総復習ができるよ。\nたくさん練習して、しっかりマスターしよう！"},
    {"B0401", "「きゃ」「きぃ」「きゅ」「きぇ」「きょ」はそれぞれ\nkya、kyi、kyu、kye、kyo、\n「ぎゃ」「ぎぃ」「ぎゅ」「ぎぇ」「ぎょ」はそれぞれ\ngya、gyi、gyu、gye、gyo で打てるんだ。"},
    {"B0402", "「しゃ」は sya か sha、「しぃ」は syi、\n「しゅ」は syu か shu、「しぇ」は sye か she、\n「しょ」は syo か sho で打てるよ。「じゃ」は ja か zya、\n「じぃ」は jyi か zyi、「じゅ」は ju か zyu、\n「じぇ」は je か zye、「じょ」は jo か zyo がおすすめ。"},
    {"B0403", "「ちゃ」は tya か cha、「ちぃ」は tyi、\n「ちゅ」は tyu か chu、「ちぇ」は tye か che、\n「ちょ」は tyo か cho で打てるよ。\n「ぢゃ」「ぢぃ」「ぢゅ」「ぢぇ」「ぢょ」は\nあまり見かけないけど、\nそれぞれ dya、dyi、dyu、dye、dyo で打てるんだ。"},
    {"B0404", "「にゃ」は nya、「にぃ」は nyi、「にゅ」は nyu、\n「にぇ」は nye、「にょ」は nyo で打てるよ。"},
    {"B0405", "「ひゃ」「ひぃ」「ひゅ」「ひぇ」「ひょ」はそれぞれ\nhya、hyi、hyu、hye、hyo、\n「びゃ」「びぃ」「びゅ」「びぇ」「びょ」はそれぞれ\nbya、byi、byu、bye、byo、\n「ぴゃ」「ぴぃ」「ぴゅ」「ぴぇ」「ぴょ」はそれぞれ\npya、pyi、pyu、pye、pyo で打てるよ。"},
    {"B0406", "「みゃ」「みぃ」「みゅ」「みぇ」「みょ」はそれぞれ\nmya、myi、myu、mye、myo で打てるよ。"},
    {"B0407", "「りゃ」「りぃ」「りゅ」「りぇ」「りょ」はそれぞれ\nrya、ryi、ryu、rye、ryo で打てるよ。"},
    {"B0408", "「っ」は子音を二つ重ねるんだ。\n例えば「った」は tta、「っき」は kki というようにね。\n慣れが必要だから指を動かして練習するのがおすすめ！"},
    {"B0409", "長音は伸ばし棒の「ー」のことだよ。\nキーボードの一番上の段、数字の0の右隣にあるよ。"},
    {"B0410", "最後は「ん」の練習だよ。「ん」はちょっと特殊。\n「ん」の後ろに「あいうえおなにぬねのやゆよ」が続くときと文末は nn、それ以外は n、nn どちらでもいいよ。\nあとは裏技で、実は「ん」は xn でも打てるんだ。\n覚えておくと便利！\n"},
    {"Chapter1Toggle", "Chapter.1 はホームポジションを学ぼう！\nFキーに左手の人差し指、Jキーに右手の人差し指を置いて、\nそのまま横並びに指を置いて、親指はスペースキーに\n置くとホームポジションになるよ。\nFキーとJキーにはボコッと目印がついているから、\nそこに人差し指を置くって覚えればOK！"},
    {"Chapter2Toggle", "Chapter.2 はアルファベットと数字、基本的な記号の練習をするよ。"},
    {"Chapter3Toggle", "Chapter.3 は五十音の練習だよ。\n覚えることがたくさんあるから、少しずつやっていこう！"},
    {"Chapter4Toggle", "Chapter.4 は拗音・促音・長音などの練習だよ。\n拗音は「きゃ」「しゅ」「ちょ」とかみたいに小さい「ゃ」「ゅ」「ょ」が入るもの、「促音」は「っ」が入るもの、「長音」は伸ばし棒の「ー」が入るもののことを言うよ。あとはローマ字入力では「ん」がちょっと特殊だからそれも練習しよう！"}
  };

  // 練習内容の説明(JISかな)
  private static readonly Dictionary<string, string> practiceJISKanaDescriptionDict = new Dictionary<string, string> {
    {"B0301", "あ行の練習だよ。\nまずはこの5文字をしっかり覚えよう。\nキーボードの左上にかたまってるから、左手をいっぱい使うよ。"},
    {"B0302", "か・が行の練習だよ。\nか行は「け」だけキーボードの右のほうにあるけど、それ以外は真ん中によっているよ。\nJISかな入力で「゛（濁点）」をうつときは「か」＋「゛」という風に分けて打つんだ。「゛」のキーはキーボードの右側の「＠」が書いてあるところだよ。"},
    {"B0303", "さ・ざ行の練習だよ。\nさ行はキーボードの左側。「せ」だけ右にあるよ。"},
    {"B0304", "た・だ行の練習だよ。\nた行はキーボードの左側にかたまっているね。"},
    {"B0305", "な行の練習だよ。\nな行はキーボードの右手寄り。「ぬ」だけ左端にあるよ。"},
    {"B0306", "は・ば・ぱ行の練習だよ。\nは行は結構バラバラな配置だね。\n半濁点の「゜」は「゛」の右隣にあるよ。"},
    {"B0307", "ま行の練習だよ。\nま行はキーボードの右側に寄っているね。"},
    {"B0308", "や・わ行の練習だよ。\nや行はキーボードの上側にかたまっていて覚えやすいよ。\n「わ」は数字の0が書いてあるところ。\n「を」は「わ」のキーを Shift キーを押しながら打つと打てる文字だよ。"},
    {"B0309", "ら行の練習だよ。\nら行はキーボードの右側にかたまっているね。"},
    {"B0310", "五十音の総復習ができるよ。\n配置がバラバラで覚えるの大変だけど、\n繰り返し練習してマスターしよう！"},
    {"B0401", "小さい「ぁぃぅぇぉゃゅょ」は Shift キーを押しながらそのキーを押すと文字が小さくなるよ！\n「きゃ」などは「き」と「ゃ」、「ぎゃ」などは「き」「゛」「ゃ」と分けて打つよ。"},
    {"B0402", "「しゃ」「しぃ」「しゅ」「しぇ」「しょ」も「し」＋「ゃ」「ぃ」「ゅ」「ぇ」「ょ」と分けて打つよ。「じゃ」「じぃ」「じゅ」「じぇ」「じょ」も「し」＋「゛」＋「ゃ」「ぃ」「ゅ」「ぇ」「ょ」って感じ！"},
    {"B0403", "「ちゃ」「ぢゃ」などもこれまでと同じ。\n「ち」＋小さい文字、「ち」＋「゛」＋小さい文字で打とう！"},
    {"B0404", "「にゃ」などもこれまでと同じ。\n「に」＋小さい文字で打とう！"},
    {"B0405", "「ひゃ」「びゃ」「ぴゃ」なども同じようにできるよ。\n「ひ」＋小さい文字、「ひ」＋「゛」＋小さい文字、「ひ」＋「゜」＋小さい文字で打とう！"},
    {"B0406", "「みゃ」なども同じように、「み」＋小さい文字で打てるよ。"},
    {"B0407", "「りゃ」なども同じように、「り」＋小さい文字で打てるよ。"},
    {"B0408", "「っ」は Shift キーを押しながら「つ」のキーを押そう。\n「つ」のキーが左下にあるから、Shift キーは右手の小指で押すのが良いかな。"},
    {"B0409", "長音は伸ばし棒の「ー」のことだよ。\nキーボードの右上のほうにあるよ。"},
    {"B0410", "最後は「ん」の練習はローマ字(JIS配列)用の練習コンテンツだよ。"},
    {"Chapter1Toggle", "Chapter.1 はローマ字(JIS配列)で練習できるコンテンツだよ。"},
    {"Chapter2Toggle", "Chapter.2 はローマ字(JIS配列)で練習できるコンテンツだよ。"},
    {"Chapter3Toggle", "Chapter.3 は五十音の練習だよ。\nJISかな特有の配置にまずは慣れよう！"},
    {"Chapter4Toggle", "Chapter.4 は拗音・促音・長音などの練習だよ。\n拗音は「きゃ」「しゅ」「ちょ」とかみたいに小さい「ゃ」「ゅ」「ょ」が入るもの、「促音」は「っ」が入るもの、「長音」は伸ばし棒の「ー」が入るもののことを言うよ。あとはローマ字入力では「ん」がちょっと特殊だからそれも練習しよう！"}
  };

  // QwertyJP の各練習で使うキーをハイライト
  private static readonly Dictionary<string, string[]> practiceKeys = new Dictionary<string, string[]> {
    {"B0101", new string[]{"Key_A", "Key_S", "Key_D", "Key_F", "Key_G", "Key_H", "Key_J", "Key_K", "Key_L"}},
    {"B0102", new string[]{"Key_Q", "Key_W", "Key_E", "Key_R", "Key_T", "Key_Y", "Key_U", "Key_I", "Key_O", "Key_P"}},
    {"B0103", new string[]{"Key_Z", "Key_X", "Key_C", "Key_V", "Key_B", "Key_N", "Key_M", "Key_Comma", "Key_Period", "Key_Slash"}},
    {"B0201", new string[]{}},
    {"B0202", new string[]{"Key_LShift", "Key_RShift"}},
    {"B0203", new string[]{"Key_1", "Key_2", "Key_3", "Key_4", "Key_5", "Key_6", "Key_7", "Key_8", "Key_9", "Key_0", "Key_Hyphen", "Key_Comma", "Key_Period", "Key_Slash", "Key_LShift", "Key_RShift"}},
    {"B0204", new string[]{}},
    {"B0301", new string[]{"Key_A", "Key_I", "Key_U", "Key_E", "Key_O"}},
    {"B0302", new string[]{"Key_K", "Key_G", "Key_A", "Key_I", "Key_U", "Key_E", "Key_O"}},
    {"B0303", new string[]{"Key_S", "Key_Z", "Key_A", "Key_I", "Key_U", "Key_E", "Key_O"}},
    {"B0304", new string[]{"Key_T", "Key_D", "Key_A", "Key_I", "Key_U", "Key_E", "Key_O"}},
    {"B0305", new string[]{"Key_N", "Key_A", "Key_I", "Key_U", "Key_E", "Key_O"}},
    {"B0306", new string[]{"Key_H", "Key_B", "Key_P", "Key_A", "Key_I", "Key_U", "Key_E", "Key_O"}},
    {"B0307", new string[]{"Key_M", "Key_A", "Key_I", "Key_U", "Key_E", "Key_O"}},
    {"B0308", new string[]{"Key_Y", "Key_W", "Key_A", "Key_I", "Key_U", "Key_E", "Key_O"}},
    {"B0309", new string[]{"Key_R", "Key_A", "Key_I", "Key_U", "Key_E", "Key_O"}},
    {"B0310", new string[]{}},
    {"B0401", new string[]{"Key_K", "Key_G", "Key_Y", "Key_A", "Key_I", "Key_U", "Key_E", "Key_O"}},
    {"B0402", new string[]{"Key_S", "Key_J", "Key_Z", "Key_Y", "Key_H", "Key_A", "Key_I", "Key_U", "Key_E", "Key_O"}},
    {"B0403", new string[]{"Key_C", "Key_T", "Key_Y", "Key_H", "Key_D", "Key_A", "Key_I", "Key_U", "Key_E", "Key_O"}},
    {"B0404", new string[]{"Key_N", "Key_Y", "Key_A", "Key_I", "Key_U", "Key_E", "Key_O"}},
    {"B0405", new string[]{"Key_H", "Key_B", "Key_P", "Key_Y", "Key_A", "Key_I", "Key_U", "Key_E", "Key_O"}},
    {"B0406", new string[]{"Key_M", "Key_Y", "Key_A", "Key_I", "Key_U", "Key_E", "Key_O"}},
    {"B0407", new string[]{"Key_R", "Key_Y", "Key_A", "Key_I", "Key_U", "Key_E", "Key_O"}},
    {"B0408", new string[]{}},
    {"B0409", new string[]{"Key_Hyphen"}},
    {"B0410", new string[]{"Key_N", "Key_X"}},
    {"Chapter1Toggle", new string[]{"Key_A", "Key_S", "Key_D", "Key_F", "Key_Space", "Key_J", "Key_K", "Key_L", "Key_Semicolon"}},
    {"Chapter2Toggle", new string[]{}},
    {"Chapter3Toggle", new string[]{}},
    {"Chapter4Toggle", new string[]{}}
  };

  // JIS かな練習で使うキーをハイライト
  private static readonly Dictionary<string, string[]> practiceKeysJISKana = new Dictionary<string, string[]> {
    {"B0101", new string[]{}},
    {"B0102", new string[]{}},
    {"B0103", new string[]{}},
    {"B0201", new string[]{}},
    {"B0202", new string[]{}},
    {"B0203", new string[]{}},
    {"B0204", new string[]{}},
    {"B0301", new string[]{"Key_3", "Key_E", "Key_4", "Key_5", "Key_6"}},
    {"B0302", new string[]{"Key_T", "Key_G", "Key_H", "Key_Colon", "Key_B", "Key_At"}},
    {"B0303", new string[]{"Key_X", "Key_D", "Key_R", "Key_P", "Key_C", "Key_At"}},
    {"B0304", new string[]{"Key_Q", "Key_A", "Key_Z", "Key_W", "Key_S", "Key_At"}},
    {"B0305", new string[]{"Key_U", "Key_I", "Key_1", "Key_Comma", "Key_K", "Key_At"}},
    {"B0306", new string[]{"Key_F", "Key_V", "Key_2", "Key_Caret", "Key_Hyphen", "Key_At", "Key_LBracket"}},
    {"B0307", new string[]{"Key_J", "Key_N", "Key_RBracket", "Key_Slash", "Key_M"}},
    {"B0308", new string[]{"Key_7", "Key_8", "Key_9", "Key_0", "Key_LShift", "Key_RShift", "Key_Y"}},
    {"B0309", new string[]{"Key_O", "Key_L", "Key_Period", "Key_Semicolon", "Key_BackSlash"}},
    {"B0310", new string[]{}},
    {"B0401", new string[]{"Key_G", "Key_7", "Key_8", "Key_9", "Key_At", "Key_LShift", "Key_RShift"}},
    {"B0402", new string[]{"Key_D", "Key_7", "Key_8", "Key_9", "Key_At", "Key_LShift", "Key_RShift" }},
    {"B0403", new string[]{"Key_A", "Key_7", "Key_8", "Key_9", "Key_At", "Key_LShift", "Key_RShift" }},
    {"B0404", new string[]{"Key_I", "Key_7", "Key_8", "Key_9", "Key_At", "Key_LShift", "Key_RShift" }},
    {"B0405", new string[]{"Key_V", "Key_7", "Key_8", "Key_9", "Key_At", "Key_LShift", "Key_RShift" }},
    {"B0406", new string[]{"Key_N", "Key_7", "Key_8", "Key_9", "Key_At", "Key_LShift", "Key_RShift" }},
    {"B0407", new string[]{"Key_L", "Key_7", "Key_8", "Key_9", "Key_At", "Key_LShift", "Key_RShift" }},
    {"B0408", new string[]{"Key_Z", "Key_LShift", "Key_RShift"}},
    {"B0409", new string[]{"Key_Yen"}},
    {"B0410", new string[]{}},
    {"Chapter1Toggle", new string[]{"Key_A", "Key_S", "Key_D", "Key_F", "Key_Space", "Key_J", "Key_K", "Key_L", "Key_Semicolon"}},
    {"Chapter2Toggle", new string[]{}},
    {"Chapter3Toggle", new string[]{}},
    {"Chapter4Toggle", new string[]{}}
  };
  [SerializeField] private GameObject chapterSelect;
  [SerializeField] private List<Toggle> ChapterToggleList;
  [SerializeField] private TextMeshProUGUI InstructionText;
  [SerializeField] private EventSystem evs;
  [SerializeField] private AssistKeyboardJIS DisplayKeyboard;
  [SerializeField] private TextMeshProUGUI InputModeText;
  [SerializeField] private List<Button> JISKanaDisableButtonList;

  /// <summary>
  /// 初期化処理
  /// </summary>
  void Awake()
  {
    InstructionText.text = DefaultInstructionText;
    SetPreviousSettings();
  }

  /// <summary>
  /// キーボード表示を切り替える
  /// </summary>
  /// <param name="keyList"></param>
  private void ChangeKeyboardDisplay(IReadOnlyCollection<string> keyList)
  {
    DisplayKeyboard.SetAllKeyColorWhite();
    DisplayKeyboard.SetAllFingerColorWhite();
    DisplayKeyboard.SetHighlights(keyList);
  }

  /// <summary>
  /// 直前の練習内容を画面に反映させる
  /// </summary>
  private void SetPreviousSettings()
  {
    for (int i = 1; i <= chapterSelect.transform.childCount; ++i)
    {
      var toggle = transform.Find("ChapterSelect/Chapter" + i.ToString() + "Toggle").GetComponent<Toggle>();
      // OnValueChanged を強制動作させることで Panel を取得しなくて済む
      toggle.isOn = !toggle.isOn;
      toggle.isOn = prevChapterNum == i;
    }
    // 入力モードの設定
    practiceInputMode = ConfigScript.InputMode;
  }

  /// <summary>
  /// キーに対応するチャプターのボタンを押したことにする
  /// </summary>
  /// <param name="kc">押したキーの KeyCode</param>
  private void SelectChapterToggle(KeyCode kc)
  {
    var chapterNum = shortCutKeyCodeChapter[kc] - 1;
    DisplayKeyboard.SetAllKeyColorWhite();
    // 対応するチャプターの Toggle を ON にしつつ
    // キーボードや説明も更新する
    for (int idx = 0; idx < ChapterToggleList.Count; ++idx)
    {
      if (idx == chapterNum)
      {
        var toggleName = ChapterToggleList[idx].name;
        ChapterToggleList[idx].Select();
        ChapterToggleList[idx].isOn = true;

        // QwertyJP での説明
        if (practiceInputMode == (int)ConfigScript.InputType.roman && practiceKeys.ContainsKey(toggleName))
        {
          InstructionText.text = practiceDescriptionDict[toggleName];
          ChangeKeyboardDisplay(practiceKeys[toggleName]);
        }
        // JIS かなでの説明
        else if (practiceInputMode == (int)ConfigScript.InputType.jisKana && practiceKeysJISKana.ContainsKey(toggleName))
        {
          InstructionText.text = practiceJISKanaDescriptionDict[toggleName];
          ChangeKeyboardDisplay(practiceKeysJISKana[toggleName]);
        }
      }
      else
      {
        ChapterToggleList[idx].isOn = false;
      }
    }
  }

  /// <summary>
  /// キーに対応する練習ボタンを選択状態にして、説明文等を更新
  /// </summary>
  /// <param name="kc"></param>
  private void SelectPracticeButton(KeyCode kc)
  {
    // xxyy : xx がチャプター、yy がボタン番号
    int buttonNum = 0;
    for (int idx = 0; idx < ChapterToggleList.Count; ++idx)
    {
      if (ChapterToggleList[idx].isOn)
      {
        buttonNum += 100 * (idx + 1);
        break;
      }
    }
    buttonNum += shortCutKeyCode[kc];

    // ボタン番号とデータセットの対応付けをする
    if (beginnerDatasetFileName.ContainsKey(buttonNum))
    {
      // ボタンを選択状態に
      EventSystem.current.SetSelectedGameObject(null);
      var buttonName = $"B0{buttonNum.ToString()}";
      var selectButton = GameObject.Find(buttonName).GetComponent<Button>();
      selectButton.Select();

      // QwertyJP での説明
      if (practiceInputMode == (int)ConfigScript.InputType.roman && practiceDescriptionDict.ContainsKey(buttonName))
      {
        InstructionText.text = practiceDescriptionDict[buttonName];
        ChangeKeyboardDisplay(practiceKeys[buttonName]);
      }
      // JIS かなでの説明
      else if (practiceInputMode == (int)ConfigScript.InputType.jisKana && practiceJISKanaDescriptionDict.ContainsKey(buttonName))
      {
        InstructionText.text = practiceJISKanaDescriptionDict[buttonName];
        ChangeKeyboardDisplay(practiceKeysJISKana[buttonName]);
      }
    }
  }

  /// <summary>
  /// 入力方式を切り替える
  /// </summary>
  private void ChangeInputMode()
  {
    // QwertyJP から JIS かなへの切り替え
    if (practiceInputMode == (int)ConfigScript.InputType.roman)
    {
      practiceInputMode = (int)ConfigScript.InputType.jisKana;
      InputModeText.text = "JIS かな";
      foreach (var button in JISKanaDisableButtonList)
      {
        button.interactable = false;
      }
    }
    // JIS かなから QwertyJP への切り替え
    else if (practiceInputMode == (int)ConfigScript.InputType.jisKana)
    {
      practiceInputMode = (int)ConfigScript.InputType.roman;
      InputModeText.text = "ローマ字(JIS配列)";
      foreach (var button in JISKanaDisableButtonList)
      {
        button.interactable = true;
      }
    }
    DisplayKeyboard.GetAllKeys(practiceInputMode, ConfigScript.InputArray);
  }

  /// <summary>
  /// Keycode と対応する操作
  /// <param name="kc">KeyCode</param>
  /// </summary>
  private void KeyCheck(KeyCode kc)
  {
    if (KeyCode.Escape == kc) { ReturnModeSelectScene(); }
    else if (shortCutKeyCode.ContainsKey(kc)) { SelectPracticeButton(kc); }
    else if (shortCutKeyCodeChapter.ContainsKey(kc)) { SelectChapterToggle(kc); }
    else if (KeyCode.J == kc) { ChangeInputMode(); }
  }

  /// <summary>
  /// モードセレクト画面へ戻る
  /// </summary>
  public void ReturnModeSelectScene()
  {
    SceneManager.LoadScene("ModeSelectScene");
  }

  /// <summary>
  /// キーボードの入力などの受付処理
  /// </summary>
  void OnGUI()
  {
    Event e = Event.current;
    if (e.type == EventType.KeyDown && e.type != EventType.KeyUp && e.keyCode != KeyCode.None
    && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
    {
      KeyCheck(e.keyCode);
    }
  }

  /// <summary>
  /// 各練習ボタンを押したときの動作
  /// <param name="buttonNumber">押したボタンに割り当てられた番号</param>
  /// </summary>
  public void OnClickButton(int buttonNumber)
  {
    // 設定の反映
    ConfigScript.GameMode = (int)ConfigScript.SingleMode.shortSentence;
    ConfigScript.DataSetName = beginnerDatasetFileName[buttonNumber];
    ConfigScript.Tasks = WORD_TASK_NUM;
    ConfigScript.IsBeginnerMode = true;
    ConfigScript.InputMode = practiceInputMode;
    ConfigScript.InputArray = (int)ConfigScript.KeyArrayType.japanese;
    prevChapterNum = buttonNumber / 100;
    // 練習スタート
    SceneManager.LoadScene("BeginnerTypingScene");
  }
}
