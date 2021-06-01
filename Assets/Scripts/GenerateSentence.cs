using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
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
		{"きゃ", new string[3] {"kya", "kilya", "kixya"}},
		{"きぃ", new string[5] {"kyi", "kili", "kilyi", "kixi", "kixyi"}},
		{"きゅ", new string[3] {"kyu", "kilyu", "kixyu"}},
		{"きぇ", new string[5] {"kye", "kile", "kilye", "kixe", "kixye"}},
		{"きょ", new string[3] {"kyo", "kilyo", "kixyo"}},
		{"ぎゃ", new string[3] {"gya", "gilya", "gixya"}},
		{"ぎぃ", new string[5] {"gyi", "gili", "gilyi", "gixi", "gixyi"}},
		{"ぎゅ", new string[3] {"gyu", "gilyu", "gixyu"}},
		{"ぎぇ", new string[5] {"gye", "gile", "gilye", "gixe", "gixye"}},
		{"ぎょ", new string[3] {"gyo", "gilyo", "gixyo"}},
		{"くぁ", new string[8] {"qa", "kwa", "kula", "kuxa", "cula", "cuxa", "qula", "quxa"}},
		{"くぃ", new string[14] {"qi", "qyi", "kuli", "kuxi", "kulyi", "kuxyi", "culi", "culyi", "cuxi", "cuxyi", "quli", "quxi", "qulyi", "quxyi"}},
		{"くぅ", new string[7] {"qwu", "kulu", "kuxu", "culu", "cuxu", "qulu", "quxu"}},
		{"くぇ", new string[14] {"qe", "qwe", "kule", "kuxe", "kulye", "kuxye", "cule", "cuxe", "culye", "cuxye", "qule", "quxe", "qulye", "quxye"}},
		{"くぉ", new string[8] {"qo", "qwo", "kulo", "kuxo", "culo", "cuxo", "qulo", "quxo"}},
		{"ぐぁ", new string[3] {"gwa", "gula", "guxa"}},
		{"ぐぃ", new string[5] {"gwi", "guli", "gulyi", "guxi", "guxyi"}},
		{"ぐぅ", new string[3] {"gwu", "gulu", "guxu"}},
		{"ぐぇ", new string[5] {"gwe", "gule", "guxe", "gulye", "guxye"}},
		{"ぐぉ", new string[3] {"gwo", "gulo", "guxo"}},
		{"しゃ", new string[8] {"sya", "sha", "silya", "sixya", "shilya", "shixya", "cilya", "cixya"}},
		{"しぃ", new string[13] {"syi", "sili", "sixi", "silyi", "sixyi", "shili", "shixi", "shilyi", "shixyi", "cili", "cixi", "cilyi", "cixyi"}},
		{"しゅ", new string[8] {"syu", "shu", "silyu", "sixyu", "shilyu", "shixyu", "cilyu", "cixyu"}},
		{"しぇ", new string[14] {"sye", "she", "sile", "silye", "sixe", "sixye", "shile", "shilye", "shixe", "shixye", "cile", "cilye", "cixe", "cixye"}},
		{"しょ", new string[8] {"syo", "sho", "silyo", "sixyo", "shilyo", "shixyo", "cilyo", "cixyo"}},
		{"じゃ", new string[7] {"ja", "jya", "zya", "jilya", "jixya", "zilya", "zixya"}},
		{"じぃ", new string[10] {"jyi", "zyi", "jili", "jixi", "jilyi", "jixyi", "zili", "zixi", "zilyi", "zixyi"}},
		{"じゅ", new string[7] {"ju", "jyu", "zyu", "jilyu", "jixyu", "zilyu", "zixyu"}},
		{"じぇ", new string[11] {"je", "jye", "zye", "jile", "jixe", "jilye", "jixye", "zile", "zixe", "zilye", "zixye"}},
		{"じょ", new string[7] {"jo", "jyo", "zyo", "jilyo", "jixyo", "zilyo", "zixyo"}},
		{"すぁ", new string[3] {"swa", "sula", "suxa"}},
		{"すぃ", new string[5] {"swi", "suli", "sulyi", "suxi", "suxyi"}},
		{"すぅ", new string[3] {"swu", "sulu", "suxu"}},
		{"すぇ", new string[5] {"swe", "sule", "suxe", "sulye", "suxye"}},
		{"すぉ", new string[3] {"swo", "sulo", "suxo"}},
		{"ちゃ", new string[7] {"tya", "cha", "cya", "tilya", "tixya", "chilya", "chixya"}},
		{"ちぃ", new string[10] {"tyi", "cyi", "tili", "tixi", "tilyi", "tixyi", "chili", "chixi", "chilyi", "chixyi"}},
		{"ちゅ", new string[7] {"tyu", "chu", "cyu", "tilyu", "tixyu", "chilyu", "chixyu"}},
		{"ちぇ", new string[11] {"tye", "che", "cye", "tile", "tixe", "tilye", "tixye", "chile", "chixe", "chilye", "chixye"}},
		{"ちょ", new string[7] {"tyo", "cho", "cyo", "tilyo", "tixyo", "chilyo", "chixyo"}},
		{"ぢゃ", new string[3] {"dya", "dilya", "dixya"}},
		{"ぢゅ", new string[3] {"dyu", "dilyu", "dixyu"}},
		{"ぢぇ", new string[5] {"dye", "dile", "dixe", "dilye", "dixye"}},
		{"ぢょ", new string[3] {"dyo", "dilyo", "dixyo"}},
		{"つぁ", new string[5] {"tsa", "tula", "tuxa", "tsula", "tsuxa"}},
		{"つぃ", new string[9] {"tsi", "tuli", "tuxi", "tulyi", "tuxyi", "tsuli", "tsuxi", "tsulyi", "tsuxyi"}},
		{"つぇ", new string[9] {"tse", "tule", "tuxe", "tulye", "tuxye", "tsule", "tsuxe", "tsulye", "tsuxye"}},
		{"つぉ", new string[5] {"tso", "tulo", "tuxo", "tsulo", "tsuxo"}},
		{"てゃ", new string[3] {"tha", "telya", "texya"}},
		{"てぃ", new string[5] {"thi", "teli", "texi", "telyi", "texyi"}},
		{"てゅ", new string[3] {"thu", "telyu", "texyu"}},
		{"てぇ", new string[5] {"the", "tele", "texe", "telye", "texye"}},
		{"てょ", new string[3] {"tho", "telyo", "texyo"}},
		{"でゃ", new string[3] {"dha", "delya", "dexya"}},
		{"でぃ", new string[5] {"dhi", "deli", "dexi", "delyi", "dexyi"}},
		{"でゅ", new string[3] {"dhu", "delyu", "dexyu"}},
		{"でぇ", new string[5] {"dhe", "dele", "dexe", "delye", "dexye"}},
		{"でょ", new string[3] {"dho", "delyo", "dexyo"}},
		{"とぁ", new string[3] {"twa", "tola", "toxa"}},
		{"とぃ", new string[5] {"twi", "toli", "toxi", "tolyi", "toxyi"}},
		{"とぅ", new string[3] {"twu", "tolu", "toxu"}},
		{"とぇ", new string[5] {"twe", "tole", "toxe", "tolye", "toxye"}},
		{"とぉ", new string[3] {"two", "tolo", "toxo"}},
		{"どぁ", new string[3] {"dwa", "dola", "doxa"}},
		{"どぃ", new string[5] {"dwi", "doli", "doxi", "dolyi", "doxyi"}},
		{"どぅ", new string[3] {"dwu", "dolu", "doxu"}},
		{"どぇ", new string[5] {"dwe", "dole", "doxe", "dolye", "doxye"}},
		{"どぉ", new string[3] {"dwo", "dolo", "doxo"}},
		{"にゃ", new string[3] {"nya", "nilya", "nixya"}},
		{"にぃ", new string[5] {"nyi", "nili", "nixi", "nilyi", "nixyi"}},
		{"にゅ", new string[3] {"nyu", "nilyu", "nixyu"}},
		{"にぇ", new string[5] {"nye", "nile", "nixe", "nilye", "nixye"}},
		{"にょ", new string[3] {"nyo", "nilyo", "nixyo"}},
		{"ぴゃ", new string[3] {"pya", "pilya", "pixya"}},
		{"ぴぃ", new string[5] {"pyi", "pili", "pixi", "pilyi", "pixyi"}},
		{"ぴゅ", new string[3] {"pyu", "pilyu", "pixyu"}},
		{"ぴぇ", new string[5] {"pye", "pile", "pixe", "pilye", "pixye"}},
		{"ぴょ", new string[3] {"pyo", "pilyo", "pixyo"}},
		{"ひゃ", new string[3] {"hya", "hilya", "hixya"}},
		{"ひぃ", new string[5] {"hyi", "hili", "hixi", "hilyi", "hixyi"}},
		{"ひゅ", new string[3] {"hyu", "hilyu", "hixyu"}},
		{"ひぇ", new string[5] {"hye", "hile", "hixe", "hilye", "hixye"}},
		{"ひょ", new string[3] {"hyo", "hilyo", "hixyo"}},
		{"びゃ", new string[3] {"bya", "bilya", "bixya"}},
		{"びぃ", new string[5] {"byi", "bili", "bixi", "bilyi", "bixyi"}},
		{"びゅ", new string[3] {"byu", "bilyu", "bixyu"}},
		{"びぇ", new string[5] {"bye", "bile", "bixe", "bilye", "bixye"}},
		{"びょ", new string[3] {"byo", "bilyo", "bixyo"}},
		{"ふゃ", new string[5] {"fya", "fulya", "fuxya", "hulya", "huxya"}},
		{"ふぃ", new string[10] {"fi", "fyi", "fuli", "fuxi", "fulyi", "fuxyi", "huli", "huxi", "hulyi", "huxyi"}},
		{"ふゅ", new string[5] {"fyu", "fulyu", "fuxyu", "hulyu", "huxyu"}},
		{"ふぇ", new string[10] {"fe", "fye", "fule", "fuxe", "fulye", "fuxye", "hule", "huxe", "hulye", "huxye"}},
		{"ふょ", new string[5] {"fyo", "fulyo", "fuxyo", "hulyo", "huxyo"}},
		{"ふぁ", new string[5] {"fa", "fula", "fuxa", "hula", "huxa"}},
		{"ふぉ", new string[5] {"fo", "fulo", "fuxo", "hulo", "huxo"}},
		{"みゃ", new string[3] {"mya", "milya", "mixya"}},
		{"みぃ", new string[5] {"myi", "mili", "mixi", "milyi", "mixyi"}},
		{"みゅ", new string[3] {"myu", "milyu", "mixyu"}},
		{"みぇ", new string[5] {"mye", "mile", "mixe", "milye", "mixye"}},
		{"みょ", new string[3] {"myo", "milyo", "mixyo"}},
		{"りゃ", new string[3] {"rya", "rilya", "rixya"}},
		{"りぃ", new string[5] {"ryi", "rili", "rixi", "rilyi", "rixyi"}},
		{"りゅ", new string[3] {"ryu", "rilyu", "rixyu"}},
		{"りぇ", new string[5] {"rye", "rile", "rixe", "rilye", "rixye"}},
		{"りょ", new string[3] {"ryo", "rilyo", "rixyo"}},
		{"うぁ", new string[7] {"wha", "ula", "uxa", "wula", "wuxa", "whula", "whuxa"}},
		{"うぃ", new string[11] {"wi", "whi", "uli", "uxi", "ulyi", "uxyi", "wuli", "wuxi", "wulyi", "wuxyi", "whuli"}},
		{"うぇ", new string[14] {"we", "whe", "ule", "uxe", "ulye", "uxye", "wule", "wuxe", "wulye", "wuxye", "whule", "whuxe", "whulye", "whuxye"}},
		{"うぉ", new string[7] {"who", "ulo", "uxo", "wulo", "wuxo", "whulo", "whuxo"}},
		{"ゔぁ", new string[3] {"va", "vula", "vuxa"}},
		{"ゔぃ", new string[6] {"vi", "vyi", "vuli", "vuxi", "vulyi", "vuxyi"}},
		{"ゔ", new string[1] {"vu"}},
		{"ゔぇ", new string[6] {"ve", "vye", "vule", "vuxe", "vulye", "vuxye"}},
		{"ゔぉ", new string[3] {"vo", "vulo", "vuxo"}},
		{"ゔゃ", new string[3] {"vya", "vulya", "vuxya"}},
		{"ゔゅ", new string[3] {"vyu", "vulyu", "vuxyu"}},
		{"ゔょ", new string[3] {"vyo", "vulyo", "vuxyo"}},
		{"ゐ", new string[1] {"wyi"}},
		{"ゑ", new string[1] {"wye"}},
		{"んあ", new string[2] {"nna", "xna"}},
		{"んい", new string[4] {"nni", "xni", "nnyi", "xnyi"}},
		{"んう", new string[6] {"nnu", "xnu", "nnwu", "xnwu", "nnwhu", "xnwhu"}},
		{"んえ", new string[2] {"nne", "xne"}},
		{"んお", new string[2] {"nno", "xno"}},
		{"んな", new string[2] {"nnna", "xnna"}},
		{"んに", new string[2] {"nnni", "xnni"}},
		{"んぬ", new string[2] {"nnnu", "xnnu"}},
		{"んね", new string[2] {"nnne", "xnne"}},
		{"んの", new string[2] {"nnno", "xnno"}},
		{"んや", new string[2] {"nnya", "xnya"}},
		{"んゆ", new string[2] {"nnyu", "xnyu"}},
		{"んよ", new string[2] {"nnyo", "xnyo"}},
		{"んにゃ", new string[6] {"nnnya", "xnnya", "nnnixya", "xnnixya", "nnnilya", "xnnilya"}},
		{"んにゅ", new string[6] {"nnnyu", "xnnyu", "nnnixyu", "xnnixyu", "nnnilyu", "xnnilyu"}},
		{"んにょ", new string[6] {"nnnyo", "xnnyo", "nnnixyo", "xnnixyo", "nnnilyo", "xnnilyo"}},
		{"っか", new string[10] {"kka", "cca", "ltuka", "xtuka", "ltsuka", "xtsuka", "ltuca", "xtuca", "ltsuca", "xtsuca"}},
		{"っき", new string[5] {"kki", "ltuki", "xtuki", "ltsuki", "xtsuki"}},
		{"っく", new string[10] {"kku", "ccu", "ltuku", "xtuku", "ltsuku", "xtsuku", "ltucu", "xtucu", "ltsucu", "xtsucu"}},
		{"っけ", new string[5] {"kke", "ltuke", "xtuke", "ltsuke", "xtsuke"}},
		{"っこ", new string[10] {"kko", "cco", "ltuko", "xtuko", "ltsuko", "xtsuko", "ltuco", "xtuco", "ltsuco", "xtsuco"}},
		{"っさ", new string[5] {"ssa", "ltusa", "xtusa", "ltsusa", "xtsusa"}},
		{"っし", new string[15] {"ssi", "cci", "sshi", "ltusi", "xtsusi", "ltsusi", "xtsusi", "ltuci", "xtuci", "ltsuci", "xtsuci", "ltushi", "xtushi", "ltsushi", "xtsushi"}},
		{"っす", new string[5] {"ssu", "ltusu", "xtusu", "ltsusu", "xtsusu"}},
		{"っせ", new string[10] {"sse", "cce", "ltuse", "xtuse", "ltsuse", "xtsuse", "ltuce", "xtuce", "ltsuce", "xtsuce"}},
		{"っそ", new string[5] {"sso", "ltuso", "xtuso", "ltsuso", "xtsuso"}},
		{"った", new string[5] {"tta", "ltuta", "xtuta", "ltsuta", "xtsuta"}},
		{"っち", new string[10] {"tti", "cchi", "ltuti", "xtuti", "ltsuti", "xtsuti", "ltuchi", "xtuchi", "ltsuchi", "xtsuchi"}},
		{"っつ", new string[10] {"ttu", "ttsu", "ltutu", "xtutu", "ltsutu", "xtsutu", "ltutsu", "xtutsu", "ltsutsu", "xtsutsu"}},
		{"って", new string[5] {"tte", "ltute", "xtute", "ltsute", "xtsute"}},
		{"っと", new string[5] {"tto", "ltuto", "xtuto", "ltsuto", "xtsuto"}},
		{"っな", new string[4] {"ltuna", "xtuna", "ltsuna", "xtsuna"}},
		{"っに", new string[4] {"ltuni", "xtuni", "ltsuni", "xtsuni"}},
		{"っぬ", new string[4] {"ltunu", "xtunu", "ltsunu", "xtsunu"}},
		{"っね", new string[4] {"ltune", "xtune", "ltsune", "xtsune"}},
		{"っの", new string[4] {"ltuno", "xtuno", "ltsuno", "xtsuno"}},
		{"っは", new string[5] {"hha", "ltuha", "xtuha", "ltsuha", "xtsuha"}},
		{"っひ", new string[5] {"hhi", "ltuhi", "xtuhi", "ltsuhi", "xtsuhi"}},
		{"っふ", new string[10] {"ffu", "hhu", "ltufu", "xtufu", "ltsufu", "xtsufu", "ltuhu", "xtuhu", "ltsuhu", "xtsuhu"}},
		{"っへ", new string[5] {"hhe", "ltuhe", "xtuhe", "ltsuhe", "xtsuhe"}},
		{"っほ", new string[5] {"hho", "ltuho", "xtuho", "ltsuho", "xtsuho"}},
		{"っま", new string[5] {"mma", "ltuma", "xtuma", "ltsuma", "xtsuma"}},
		{"っみ", new string[5] {"mmi", "ltumi", "xtumi", "ltsumi", "xtsumi"}},
		{"っむ", new string[5] {"mmu", "ltumu", "xtumu", "ltsumu", "xtsumu"}},
		{"っめ", new string[5] {"mme", "ltume", "xtume", "ltsume", "xtsume"}},
		{"っも", new string[5] {"mmo", "ltumo", "xtumo", "ltsumo", "xtsumo"}},
		{"っや", new string[5] {"yya", "ltuya", "xtuya", "ltsuya", "xtsuya"}},
		{"っゆ", new string[5] {"yyu", "ltuyu", "xtuyu", "ltsuyu", "xtsuyu"}},
		{"っよ", new string[5] {"yyo", "ltuyo", "xtuyo", "ltsuyo", "xtsuyo"}},
		{"っら", new string[5] {"rra", "ltura", "xtura", "ltsura", "xtsura"}},
		{"っり", new string[5] {"rri", "lturi", "xturi", "ltsuri", "xtsuri"}},
		{"っる", new string[5] {"rru", "lturu", "xturu", "ltsuru", "xtsuru"}},
		{"っれ", new string[5] {"rre", "lture", "xture", "ltsure", "xtsure"}},
		{"っろ", new string[5] {"rro", "lturo", "xturo", "ltsuro", "xtsuro"}},
		{"っわ", new string[5] {"wwa", "ltuwa", "xtuwa", "ltsuwa", "xtsuwa"}},
		{"っを", new string[5] {"wwo", "ltuwo", "xtuwo", "ltsuwo", "xtsuwo"}},
		{"っが", new string[5] {"gga", "ltuga", "xtuga", "ltsuga", "xtsuga"}},
		{"っぎ", new string[5] {"ggi", "ltugi", "xtugi", "ltsugi", "xtsugi"}},
		{"っぐ", new string[5] {"ggu", "ltugu", "xtugu", "ltsugu", "xtsugu"}},
		{"っげ", new string[5] {"gge", "ltuge", "xtuge", "ltsuge", "xtsuge"}},
		{"っご", new string[5] {"ggo", "ltugo", "xtugo", "ltsugo", "xtsugo"}},
		{"っざ", new string[5] {"zza", "ltuza", "xtuza", "ltsuza", "xtsuza"}},
		{"っじ", new string[10] {"jji", "zzi", "ltuji", "xtuji", "ltsuji", "xtsuji", "ltuzi", "xtuzi", "ltsuzi", "xtsuzi"}},
		{"っず", new string[5] {"zzu", "ltuzu", "xtuzu", "ltsuzu", "xtsuzu"}},
		{"っぜ", new string[5] {"zze", "ltuze", "xtuze", "ltsuze", "xtsuze"}},
		{"っぞ", new string[5] {"zzo", "ltuzo", "xtuzo", "ltsuzo", "xtsuzo"}},
		{"っだ", new string[5] {"dda", "ltuda", "xtuda", "ltsuda", "xtsuda"}},
		{"っぢ", new string[5] {"ddi", "ltudi", "xtudi", "ltsudi", "xtsudi"}},
		{"っづ", new string[5] {"ddu", "ltudu", "xtudu", "ltsudu", "xtsudu"}},
		{"っで", new string[5] {"dde", "ltude", "xtude", "ltsude", "xtsude"}},
		{"っど", new string[5] {"ddo", "ltudo", "xtudo", "ltsudo", "xtsudo"}},
		{"っば", new string[5] {"bba", "ltuba", "xtuba", "ltsuba", "xtsuba"}},
		{"っび", new string[5] {"bbi", "ltubi", "xtubi", "ltsubi", "xtsubi"}},
		{"っぶ", new string[5] {"bbu", "ltubu", "xtubu", "ltsubu", "xtsubu"}},
		{"っべ", new string[5] {"bbe", "ltube", "xtube", "ltsube", "xtsube"}},
		{"っぼ", new string[5] {"bbo", "ltubo", "xtubo", "ltsubo", "xtsubo"}},
		{"っぱ", new string[5] {"ppa", "ltupa", "xtupa", "ltsupa", "xtsupa"}},
		{"っぴ", new string[5] {"ppi", "ltupi", "xtupi", "ltsupi", "xtsupi"}},
		{"っぷ", new string[5] {"ppu", "ltupu", "xtupu", "ltsupu", "xtsupu"}},
		{"っぺ", new string[5] {"ppe", "ltupe", "xtupe", "ltsupe", "xtsupe"}},
		{"っぽ", new string[5] {"ppo", "ltupo", "xtupo", "ltsupo", "xtsupo"}},
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
		{"？", new string[1] {"?"}}
	};

	// JP:原文, H:ひらがな, $:N2で置換, &:V2で置換(未収録)
	private List<(string jp, string h)> qJP1 = new List<(string jp, string h)>();

	private List<(string jp, string h)> qJP2N = new List<(string jp, string h)>();

	private int gameModeEasy = ConfigScript.gameModeEasy;

	public static string DataSetName {
		private set;
		get;
	}

	// ひらがな読みを1~3文字に区切る
	List<string> ParseHiraganaSentence(string str){
		var ret = new List<string>();
		int i = 0;
		string uni = "";
		string bi = "";
		string tri = "";
		while (i < str.Length){
			uni = str[i].ToString();
			bi = (i + 1 < str.Length) ? str.Substring(i, 2) : "";
			tri = (i + 2 < str.Length) ? str.Substring(i, 3) : "";
			if(mp.ContainsKey(tri)){
				i += 3;
				ret.Add(tri);
			}
			else if(mp.ContainsKey(bi)){
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
		string s;
		for (int i = 0; i < str.Count; ++i){
			s = str[i];
			var validTypeList = new List<string>();
			// 文末「ん」の処理
			if (s.Equals("ん") && str.Count - 1 == i){
				var nList = new List<string>(mp[s]);
				nList.Remove("n");
				validTypeList = nList;
			}
			else {
				validTypeList = mp[s].ToList();
			}
			ret.Add(validTypeList);
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
		var file = Resources.Load(dataName);
		var jsonStr = file.ToString();
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