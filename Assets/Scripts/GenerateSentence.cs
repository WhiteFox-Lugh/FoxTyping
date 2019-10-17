using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

public class GenerateSentence {

	private const int minLength = 10;
	private const int maxLength = 25;
	// ひらがな -> ローマ字マッピング
	private Dictionary<string, string[]> mp = new Dictionary<string, string[]> {
		{"あ", new string[1] {"a"}},
		{"い", new string[2] {"i", "yi"}},
		{"う", new string[3] {"u", "wu", "whu"}},
		{"え", new string[1] {"e"}},
		{"お", new string[1] {"o"}},
		{"か", new string[2] {"ka", "ca"}},
		{"き", new string[1] {"ki"}},
		{"く", new string[3] {"ku", "cu", "qu"}},
		{"け", new string[1] {"ke"}},
		{"こ", new string[2] {"ko", "co"}},
		{"さ", new string[1] {"sa"}},
		{"し", new string[3] {"si", "shi", "ci"}},
		{"す", new string[1] {"su"}},
		{"せ", new string[2] {"se", "ce"}},
		{"そ", new string[1] {"so"}},
		{"た", new string[1] {"ta"}},
		{"ち", new string[2] {"ti", "chi"}},
		{"つ", new string[2] {"tu", "tsu"}},
		{"て", new string[1] {"te"}},
		{"と", new string[1] {"to"}},
		{"な", new string[1] {"na"}},
		{"に", new string[1] {"ni"}},
		{"ぬ", new string[1] {"nu"}},
		{"ね", new string[1] {"ne"}},
		{"の", new string[1] {"no"}},
		{"は", new string[1] {"ha"}},
		{"ひ", new string[1] {"hi"}},
		{"ふ", new string[2] {"fu", "hu"}},
		{"へ", new string[1] {"he"}},
		{"ほ", new string[1] {"ho"}},
		{"ま", new string[1] {"ma"}},
		{"み", new string[1] {"mi"}},
		{"む", new string[1] {"mu"}},
		{"め", new string[1] {"me"}},
		{"も", new string[1] {"mo"}},
		{"や", new string[1] {"ya"}},
		{"ゆ", new string[1] {"yu"}},
		{"よ", new string[1] {"yo"}},
		{"ら", new string[1] {"ra"}},
		{"り", new string[1] {"ri"}},
		{"る", new string[1] {"ru"}},
		{"れ", new string[1] {"re"}},
		{"ろ", new string[1] {"ro"}},
		{"わ", new string[1] {"wa"}},
		{"を", new string[1] {"wo"}},
		{"ん", new string[3] {"n", "nn", "xn"}},
		{"が", new string[1] {"ga"}},
		{"ぎ", new string[1] {"gi"}},
		{"ぐ", new string[1] {"gu"}},
		{"げ", new string[1] {"ge"}},
		{"ご", new string[1] {"go"}},
		{"ざ", new string[1] {"za"}},
		{"じ", new string[2] {"ji", "zi"}},
		{"ず", new string[1] {"zu"}},
		{"ぜ", new string[1] {"ze"}},
		{"ぞ", new string[1] {"zo"}},
		{"だ", new string[1] {"da"}},
		{"ぢ", new string[1] {"di"}},
		{"づ", new string[1] {"du"}},
		{"で", new string[1] {"de"}},
		{"ど", new string[1] {"do"}},
		{"ば", new string[1] {"ba"}},
		{"び", new string[1] {"bi"}},
		{"ぶ", new string[1] {"bu"}},
		{"べ", new string[1] {"be"}},
		{"ぼ", new string[1] {"bo"}},
		{"ぱ", new string[1] {"pa"}},
		{"ぴ", new string[1] {"pi"}},
		{"ぷ", new string[1] {"pu"}},
		{"ぺ", new string[1] {"pe"}},
		{"ぽ", new string[1] {"po"}},
		{"ぁ", new string[2] {"la", "xa"}},
		{"ぃ", new string[4] {"li", "xi", "lyi", "xyi"}},
		{"ぅ", new string[2] {"lu", "xu"}},
		{"ぇ", new string[4] {"le", "xe", "lye", "xye"}},
		{"ぉ", new string[2] {"lo", "xo"}},
		{"ゃ", new string[2] {"lya", "xya"}},
		{"ゅ", new string[2] {"lyu", "xyu"}},
		{"ょ", new string[2] {"lyo", "xyo"}},
		{"っ", new string[4] {"ltu", "ltsu", "xtu", "xtsu"}},
		{"ー", new string[1] {"-"}},
		{"、", new string[1] {","}},
		{"。", new string[1] {"."}},
		{"0", new string[1] {"0"}},
		{"1", new string[1] {"1"}},
		{"2", new string[1] {"2"}},
		{"3", new string[1] {"3"}},
		{"4", new string[1] {"4"}},
		{"5", new string[1] {"5"}},
		{"6", new string[1] {"6"}},
		{"7", new string[1] {"7"}},
		{"8", new string[1] {"8"}},
		{"9", new string[1] {"9"}},
		{"-", new string[1] {"-"}},
		{",", new string[1] {","}},
		{".", new string[1] {"."}},
		{";", new string[1] {";"}},
		{":", new string[1] {":"}},
		{"[", new string[1] {"["}},
		{"]", new string[1] {"]"}},
		{"@", new string[1] {"@"}},
		{"/", new string[1] {"/"}},
		{"_", new string[1] {"_"}},
		{"!", new string[1] {"!"}},
		{"！", new string[1] {"!"}},
		{"?", new string[1] {"?"}},
		{"？", new string[1] {"?"}},
		{"きゃ", new string[1] {"kya"}},
		{"きぃ", new string[1] {"kyi"}},
		{"きゅ", new string[1] {"kyu"}},
		{"きぇ", new string[1] {"kye"}},
		{"きょ", new string[1] {"kyo"}},
		{"ぎゃ", new string[1] {"gya"}},
		{"ぎぃ", new string[1] {"gyi"}},
		{"ぎゅ", new string[1] {"gyu"}},
		{"ぎぇ", new string[1] {"gye"}},
		{"ぎょ", new string[1] {"gyo"}},
		{"くぁ", new string[2] {"qa", "kwa"}},
		{"くぃ", new string[3] {"qi", "qyi", "kwi"}},
		{"くぅ", new string[3] {"qu", "qwu", "kwu"}},
		{"くぇ", new string[3] {"qe", "qwe", "kwe"}},
		{"くぉ", new string[3] {"qo", "qwo", "kwo"}},
		{"ぐぁ", new string[1] {"gwa"}},
		{"ぐぃ", new string[1] {"gwi"}},
		{"ぐぅ", new string[1] {"gwu"}},
		{"ぐぇ", new string[1] {"gwe"}},
		{"ぐぉ", new string[1] {"gwo"}},
		{"しゃ", new string[2] {"sya", "sha"}},
		{"しぃ", new string[1] {"syi"}},
		{"しゅ", new string[2] {"syu", "shu"}},
		{"しぇ", new string[2] {"sye", "she"}},
		{"しょ", new string[2] {"syo", "sho"}},
		{"じゃ", new string[3] {"ja", "jya", "zya"}},
		{"じぃ", new string[2] {"jyi", "zyi"}},
		{"じゅ", new string[3] {"ju", "jyu", "zyu"}},
		{"じぇ", new string[3] {"je", "jye", "zye"}},
		{"じょ", new string[3] {"jo", "jyo", "zyo"}},
		{"すぁ", new string[1] {"swa"}},
		{"すぃ", new string[1] {"swi"}},
		{"すぅ", new string[1] {"swu"}},
		{"すぇ", new string[1] {"swe"}},
		{"すぉ", new string[1] {"swo"}},
		{"ちゃ", new string[3] {"tya", "cha", "cya"}},
		{"ちぃ", new string[2] {"tyi", "cyi"}},
		{"ちゅ", new string[3] {"tyu", "chu", "cyu"}},
		{"ちぇ", new string[3] {"tye", "che", "cye"}},
		{"ちょ", new string[3] {"tyo", "cho", "cyo"}},
		{"ぢゃ", new string[1] {"dya"}},
		{"ぢゅ", new string[1] {"dyu"}},
		{"ぢぇ", new string[1] {"dye"}},
		{"ぢょ", new string[1] {"dyo"}},
		{"てゃ", new string[1] {"tha"}},
		{"てぃ", new string[1] {"thi"}},
		{"てゅ", new string[1] {"thu"}},
		{"てぇ", new string[1] {"the"}},
		{"てょ", new string[1] {"tho"}},
		{"でゃ", new string[1] {"dha"}},
		{"でぃ", new string[1] {"dhi"}},
		{"でゅ", new string[1] {"dhu"}},
		{"でぇ", new string[1] {"dhe"}},
		{"でょ", new string[1] {"dho"}},
		{"とぁ", new string[1] {"twa"}},
		{"とぃ", new string[1] {"twi"}},
		{"とぅ", new string[1] {"twu"}},
		{"とぇ", new string[1] {"twe"}},
		{"とぉ", new string[1] {"two"}},
		{"どぁ", new string[1] {"dwa"}},
		{"どぃ", new string[1] {"dwi"}},
		{"どぅ", new string[1] {"dwu"}},
		{"どぇ", new string[1] {"dwe"}},
		{"どぉ", new string[1] {"dwo"}},
		{"にゃ", new string[1] {"nya"}},
		{"にぃ", new string[1] {"nyi"}},
		{"にゅ", new string[1] {"nyu"}},
		{"にぇ", new string[1] {"nye"}},
		{"にょ", new string[1] {"nyo"}},
		{"ぴゃ", new string[1] {"pya"}},
		{"ぴぃ", new string[1] {"pyi"}},
		{"ぴゅ", new string[1] {"pyu"}},
		{"ぴぇ", new string[1] {"pye"}},
		{"ぴょ", new string[1] {"pyo"}},
		{"ひゃ", new string[1] {"hya"}},
		{"ひぃ", new string[1] {"hyi"}},
		{"ひゅ", new string[1] {"hyu"}},
		{"ひぇ", new string[1] {"hye"}},
		{"ひょ", new string[1] {"hyo"}},
		{"びゃ", new string[1] {"bya"}},
		{"びぃ", new string[1] {"byi"}},
		{"びゅ", new string[1] {"byu"}},
		{"びぇ", new string[1] {"bye"}},
		{"びょ", new string[1] {"byo"}},
		{"ふゃ", new string[1] {"fya"}},
		{"ふぃ", new string[2] {"fi", "fyi"}},
		{"ふゅ", new string[1] {"fyu"}},
		{"ふぇ", new string[2] {"fe", "fye"}},
		{"ふょ", new string[1] {"fyo"}},
		{"ふぁ", new string[1] {"fa"}},
		{"ふぉ", new string[1] {"fo"}},
		{"みゃ", new string[1] {"mya"}},
		{"みぃ", new string[1] {"myi"}},
		{"みゅ", new string[1] {"myu"}},
		{"みぇ", new string[1] {"mye"}},
		{"みょ", new string[1] {"myo"}},
		{"りゃ", new string[1] {"rya"}},
		{"りぃ", new string[1] {"ryi"}},
		{"りゅ", new string[1] {"ryu"}},
		{"りぇ", new string[1] {"rye"}},
		{"りょ", new string[1] {"ryo"}},
		{"うぁ", new string[1] {"wha"}},
		{"うぃ", new string[2] {"wi", "whi"}},
		{"うぇ", new string[2] {"we", "whe"}},
		{"うぉ", new string[1] {"who"}},
		{"ゔぁ", new string[1] {"va"}},
		{"ゔぃ", new string[2] {"vi", "vyi"}},
		{"ゔ", new string[1] {"vu"}},
		{"ゔぇ", new string[2] {"ve", "vye"}},
		{"ゔぉ", new string[1] {"vo"}},
		{"ゔゃ", new string[1] {"vya"}},
		{"ゔゅ", new string[1] {"vyu"}},
		{"ゔょ", new string[1] {"vyo"}},
		{"ゐ", new string[1] {"wyi"}},
		{"ゑ", new string[1] {"wye"}},
	};

	// JP:原文, H:ひらがな, $:N2で置換, &:V2で置換(未収録)
	private List<(string jp, string h)> qJP1;

	private List<(string jp, string h)> qJP2N;

	private int gameModeEasy = ConfigScript.gameModeEasy;

	public static string DataSetName {
		private set;
		get;
	}

	// ひらがな読みを区切る
	List<string> ParseHiraganaSentence(string str){
		var ret = new List<string>();
		int i = 0;
		string uni, bi;
		while (i < str.Length){
			uni = str[i].ToString();
			if(i + 1 < str.Length){
				bi = str[i].ToString() + str[i+1].ToString();
			}
			else {
				bi = "";
			}
			if(mp.ContainsKey(bi)){
				i += 2;
				ret.Add(bi);
			}
			else {
				i++;
				ret.Add(uni);
			}
		}
		return ret;
	}

	// ひらがな読みをパースしてタイピング文字列を生成
	public List<List<string>> ConstructTypeSentence(List<string> str){
		var ret = new List<List<string>>();
		string s, ns;
		for (int i = 0; i < str.Count; ++i){
			s = str[i];
			if(i + 1 < str.Count){
				ns = str[i+1];
			}
			else {
				ns = "";
			}
			var tmpList = new List<string>();
			// ん の処理
			if (s.Equals("ん")){
				bool isValidSingleN;
				var nList = mp[s];
				// 文末の「ん」-> nn, xn のみ
				if(str.Count - 1 == i){
					isValidSingleN = false;
				}
				// 後ろに母音, ナ行, ヤ行 -> nn, xn のみ
				else if(i + 1 < str.Count &&
				(ns.Equals("あ") || ns.Equals("い") || ns.Equals("う") || ns.Equals("え") ||
				ns.Equals("お") || ns.Equals("な") || ns.Equals("に") || ns.Equals("ぬ") ||
				ns.Equals("ね") || ns.Equals("の") || ns.Equals("や") || ns.Equals("ゆ") || ns.Equals("よ"))){
					isValidSingleN = false;
				}
				// それ以外は n も許容
				else {
					isValidSingleN = true;
				}
				foreach (var t in nList){
					if(!isValidSingleN && t.Equals("n")){
						continue;
					}
					tmpList.Add(t);
				}
			}
			// っ の処理
			else if(s.Equals("っ")){
				var ltuList = mp[s];
				var nextList = mp[ns];
				var hs = new HashSet<string>();
				// 次の文字の子音だけとってくる
				foreach (string t in nextList){
					string c = t[0].ToString();
					hs.Add(c);
				}
				var hsList = hs.ToList();
				List<string> ltuTypeList = hsList.Concat(ltuList).ToList();
				tmpList = ltuTypeList;
			}
			// ちゃ などのように tya, cha や ち + ゃ を許容するパターン
			else if(2 == s.Length && !string.Equals("ん", s[0])){
				// ちゃ などとそのまま打つパターンの生成
				tmpList = tmpList.Concat(mp[s]).ToList();
				// ち + ゃ などの分解して入力するパターンを生成
				var fstList = mp[s[0].ToString()];
				var sndList = mp[s[1].ToString()];
				var retList = new List<string>();
				foreach (string fstStr in fstList){
					foreach (string sndStr in sndList){
						string t = fstStr + sndStr;
						retList.Add(t);
					}
				}
				tmpList = tmpList.Concat(retList).ToList();
			}
			// それ以外
			else {
				tmpList = mp[s].ToList();
			}
			ret.Add(tmpList);
		}
		return ret;
	}

	public (string jp, string hi, List<string> hiSep, List<List<string>> ty) Generate(int g){
		bool isOK = false;
		string jpStr = "";
		string qHStr = "";
		var hiraganaSeparated = new List<string>();
		var typing = new List<List<string>>();
		while(!isOK){
			try {
				int r1 = UnityEngine.Random.Range(0, qJP1.Count);
				int r2N = UnityEngine.Random.Range(0, qJP2N.Count);
				if(gameModeEasy == g){
					isOK = true;
					jpStr = qJP2N[r2N].jp;
					qHStr = qJP2N[r2N].h;
				}
				else if(gameModeEasy != g){
					string tmpJpStr = qJP1[r1].jp;
					string tmpQhStr = qJP1[r1].h;
					tmpJpStr = tmpJpStr.Replace("$", qJP2N[r2N].jp);
					tmpQhStr = tmpQhStr.Replace("$", qJP2N[r2N].h);
					if(minLength <= tmpQhStr.Length && tmpQhStr.Length <= maxLength){
						jpStr = tmpJpStr;
						qHStr = tmpQhStr;
						Debug.Log(jpStr);
						Debug.Log(qHStr);
						hiraganaSeparated = ParseHiraganaSentence(qHStr);
						typing = ConstructTypeSentence(hiraganaSeparated);
						isOK = true;
					}
				}
			}
			catch {
				isOK = false;
				Debug.Log("例文再度生成");
			}
		}
		return (jpStr, qHStr, hiraganaSeparated, typing);
	}

	public void LoadSentenceData (string dataName){
		var fileName = Application.streamingAssetsPath + "/" + dataName + ".json";
		Debug.Log(fileName);
		string jsonStr = File.ReadAllText(fileName);
		var problemData = JsonUtility.FromJson<SentenceData>(jsonStr);
		DataSetName = problemData.sentenceDatasetScreenName;
		var listJp1 = new List<(string jp, string h)>();
		var listJp2 = new List<(string jp, string h)>();
		foreach (var word in problemData.words1){
			Debug.Log(word.Item1);
			var p = (word.Item1, word.Item2);
			listJp1.Add(p);
		}
		foreach (var word in problemData.words2){
			var p = (word.Item1, word.Item2);
			listJp2.Add(p);
		}
		qJP1 = listJp1;
		qJP2N = listJp2;
	}
}

[Serializable]
public class SentenceData
{
	public string sentenceDatasetName;
    public string sentenceDatasetScreenName;
    public string difficulty;
    public Word[] words1;
    public Word[] words2;
}

[Serializable]
public class Word {
	public string Item1;
	public string Item2;

}