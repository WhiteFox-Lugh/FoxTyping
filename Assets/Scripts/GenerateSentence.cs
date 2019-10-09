using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
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
		{"^", new string[1] {"^"}},
		{"¥", new string[1] {"¥"}},
		{"[", new string[1] {"["}},
		{"]", new string[1] {"]"}},
		{"@", new string[1] {"@"}},
		{"/", new string[1] {"/"}},
		{"_", new string[1] {"_"}},
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

	// JP:原文, H:ひらがな, $:N2で置換, &:V2で置換
	private List<(string jp, string h)> qJP1 = new List<(string jp, string h)>() {
		("一家に一台$", "いっかにいちだい$"), ("注文の多い$", "ちゅうもんのおおい$"),
		("アルプスの少女$", "あるぷすのしょうじょ$"), ("ホップステップ$", "ほっぷすてっぷ$"),
		("次の元号は$", "つぎのげんごうは$"), ("会議は踊る、されど$", "かいぎはおどる、されど$"),
		("万物は&", "ばんぶつは&"), ("目には目を、歯には$", "めにはめを、はには$"),
		("やられたらやり返す、&", "やられたらやりかえす、&"),
		("我輩は$である", "わがはいは$である"), ("お値段異常$", "おねだんいじょう$"),
		("増税前に$", "ぞうぜいまえに$"), ("犬も歩けば$", "いぬもあるけば$"), ("$も歩けば棒に当たる", "$もあるけばぼうにあたる"),
		("優勝商品は$", "ゆうしょうしょうひんは$"), ("世にも奇妙な$", "よにもきみょうな$"), ("精神的に$のないものは馬鹿だ", "せいしんてきに$のないものはばかだ"),
		("不正をしないと勝てない$", "ふせいをしないとかてない$"), ("イージーモードが許されるのは$まで", "いーじーもーどがゆるされるのは$まで"),
		("今からお前の名前は$", "いまからおまえのなまえは$"), ("重箱の隅をつついてくる$", "じゅうばこのすみをつついてくる$"),
		("石の上にも$", "いしのうえにも$"), ("一浪した挙句$", "いちろうしたあげく$"), ("本日の議題は$", "ほんじつのぎだいは$"),
		("ギネス世界記録にもなった$", "ぎねすせかいきろくにもなった$"), ("羽ばたいたら$", "はばたいたら$"),
		("やる気のない$", "やるきのない$"), ("魔女の$", "まじょの$"), ("抽選で5名様に$", "ちゅうせんで5めいさまに$"),
		("一富士二鷹三$", "いちふじにたかさん$"), ("センスのない$", "せんすのない$"), ("大好物は$", "だいこうぶつは$"),
		("ツイッターでイキった挙句$", "ついったーでいきったあげく$"), ("この世の終わりみたいな$", "このよのおわりみたいな$"),
		("二段階認証を知らない$", "にだんかいにんしょうをしらない$"), ("3人寄れば$", "3にんよれば$"),
		("むかしむかし、あるところに$", "むかしむかし、あるところに$"), ("合格者の半分が$", "ごうかくしゃのはんぶんが$"),
		("きらきらひかる$", "きらきらひかる$"), ("学校では教えてくれない$", "がっこうではおしえてくれない$"),
		("ガチャで爆死したのち$", "がちゃでばくししたのち$"), ("月の光に照らされて輝く$", "つきのひかりにてらされてかがやく$"),
		("たかだか$", "たかだか$"), ("息をするように$", "いきをするように$"), ("今日から毎日$", "きょうからまいにち$"), ("お魚くわえた$", "おさかなくわえた$"),
		("志望動機は$", "しぼうどうきは$"), ("サウナで我慢したため&", "さうなでがまんしたため&"),
		("好きです、$", "すきです、$"), ("ショートコント、$", "しょーとこんと、$"), ("正解は$", "せいかいは$"),
		("ネズミ捕りに捕まるバカな$", "ねずみとりにつかまるばかな$"), ("給料日前に$", "きゅうりょうびまえに$"),
		("夢見心地な$", "ゆめみごこちな$"), ("いきなり$", "いきなり$"), ("人間なら$", "にんげんなら$"),
		("前門の$、後門の狼", "ぜんもんの$、こうもんのおおかみ"), ("前門の虎、後門の$", "ぜんもんのとら、こうもんの$"),
		("もういくつ寝ると$", "もういくつねると$"), ("お正月には$", "おしょうがつには$"),
		("定価の半額で売られた$", "ていかのはんがくでうられた$"), ("駆け込み乗車する$", "かけこみじょうしゃする$"),
		("バカって言う方が$", "ばかっていうほうが$"), ("目も当てられないほどお粗末な$", "めもあてられないほどおそまつな$"),
		("法定速度を頑なに遵守する$", "ほうていそくどをかたくなにじゅんしゅする$"), ("おーい、$", "おーい、$"),
		("選ばれたのは$でした", "えらばれたのは$でした"), ("匠の技が光る$", "たくみのわざがひかる$"),
		("唐揚げにレモンをかける$", "からあげにれもんをかける$"), ("賞味期限間近の$", "しょうみきげんまぢかの$"),
		("出でよ、$", "いでよ、$"), ("ナイス$", "ないす$"),
		("今回の目玉商品は$", "こんかいのめだましょうひんは$"), ("泣きっ面に$", "なきっつらに$"), ("明日の食事は$", "あしたのしょくじは$"),
		("そして輝くウルトラ$", "そしてかがやくうるとら$"), ("100万回生きた$", "100まんかいいきた$"),
		("ダイニング$", "だいにんぐ$"), ("一般ウケはしない$", "いっぱんうけはしない$"),
		("コミケ徹夜組を絶対許さない$", "こみけてつやぐみをぜったいゆるさない$"), ("犯人は$", "はんにんは$"),
		("転売屋を絶対に駆逐する$", "てんばいやをぜったいにくちくする$"), ("まんまとひっかかった$", "まんまとひっかかった$"),
		("閲覧履歴を必死に隠す$", "えつらんりれきをひっしにかくす$"), ("残念ながらそれは$", "ざんねんながらそれは$"), ("最後の$", "さいごの$"),
		("行列のできる$", "ぎょうれつのできる$"), ("亀の甲より$", "かめのこうより$"), ("シュレディンガーの$", "しゅれでぃんがーの$"),
		("うさんくさい$", "うさんくさい$"), ("三度の飯より$", "さんどのめしより$"), ("炎上商法にすがるダサい$", "えんじょうしょうほうにすがるださい$"),
		("虎の威を借る$", "とらのいをかる$"), ("虎穴に入らずんば$を得ず", "こけつにいらずんば$をえず"),
		("井の中の$", "いのなかの$"), ("華麗なる$", "かれいなる$"), ("交響曲第一番$", "こうきょうきょくだいいちばん$"),
		("ラプソディーイン$", "らぷそでぃーいん$"), ("あだ名は$", "あだなは$"), ("なんちゃって$", "なんちゃって$"),
		("朱に交われば$になる", "しゅにまじわれば$になる"), ("赤いきつねと緑の$", "あかいきつねとみどりの$"),
		("赤い$と緑のたぬき", "あかい$とみどりのたぬき"), ("増税しても&", "ぞうぜいしても&"),
		("キャッシュレス$還元", "きゃっしゅれす$かんげん"), ("俺たちの$", "おれたちの$")
	};

	private List<(string jp, string h)> qJP2N = new List<(string jp, string h)>() {
		("山月記", "さんげつき"), ("ごんぎつね", "ごんぎつね"), ("手袋を買いに", "てぶくろをかいに"),
		("マルゲリータ", "まるげりーた"), ("食パン", "しょくぱん"), ("讃岐うどん", "さぬきうどん"),
		("聖徳太子", "しょうとくたいし"), ("卑弥呼", "ひみこ"), ("ジンギスカン", "じんぎすかん"),
		("タンジェント", "たんじぇんと"), ("逆三角関数", "ぎゃくさんかくかんすう"), ("河野太郎", "こうのたろう"),
		("極限値", "きょくげんち"), ("リミット", "りみっと"), ("単位ベクトル", "たんいべくとる"), ("フィボナッチ数列", "ふぃぼなっちすうれつ"),
		("タクシー数", "たくしーすう"), ("フーリエ変換", "ふーりえへんかん"), ("オペレーティングシステム", "おぺれーてぃんぐしすてむ"),
		("人工知能", "じんこうちのう"), ("型安全", "かたあんぜん"), ("型エラー", "かたえらー"),
		("三温糖", "さんおんとう"), ("動的型付け", "どうてきかたづけ"), ("静的型付け", "せいてきかたづけ"),
		("データベース", "でーたべーす"), ("ハッシュ衝突", "はっしゅしょうとつ"), ("タイパー", "たいぱー"),
		("プロセッサ", "ぷろせっさ"), ("ミキサー", "みきさー"), ("ホールケーキ", "ほーるけーき"),
		("イリオモテヤマネコ", "いりおもてやまねこ"), ("ヤンバルクイナ", "やんばるくいな"),
		("キタキツネ", "きたきつね"), ("きりたんぽ", "きりたんぽ"), ("タイリクオオカミ", "たいりくおおかみ"),
		("チューリングマシン", "ちゅーりんぐましん"), ("トランペット", "とらんぺっと"), ("ホルン", "ほるん"),
		("クラリネット", "くらりねっと"), ("ユーフォニアム", "ゆーふぉにあむ"), ("ヴィブラフォン", "ゔぃぶらふぉん"),
		("ティンパニ", "てぃんぱに"), ("フォルテ", "ふぉるて"), ("ピアノ", "ぴあの"), ("バイオリン", "ばいおりん"),
		("サッカーボール", "さっかーぼーる"), ("バスケット", "ばすけっと"), ("ラグビー", "らぐびー"), ("バレーボール", "ばれーぼーる"),
		("フィギュアスケート", "ふぃぎゅあすけーと"), ("モーグル", "もーぐる"), ("スウェーデンリレー", "すうぇーでんりれー"),
		("砲丸投げ", "ほうがんなげ"), ("トライアスロン", "とらいあすろん"), ("マラソン", "まらそん"),
		("競技プログラミング", "きょうぎぷろぐらみんぐ"), ("音ゲー引退", "おとげーいんたい"), ("タングステン", "たんぐすてん"),
		("オストワルト法", "おすとわるとほう"), ("熱力学第一法則", "ねつりきがくだいいちほうそく"), ("ブラウン運動", "ぶらうんうんどう"),
		("ハローワールド", "はろーわーるど"), ("波動方程式", "はどうほうていしき"), ("単振動", "たんしんどう"),
		("電磁誘導", "でんじゆうどう"), ("ボイルの法則", "ぼいるのほうそく"), ("シャルルの法則", "しゃるるのほうそく"),
		("羅生門", "らしょうもん"), ("舞姫", "まいひめ"), ("メロス", "めろす"), ("セリヌンティウス", "せりぬんてぃうす"),
		("ナポレオン", "なぽれおん"), ("与謝野晶子", "よさのあきこ"), ("徳川家康", "とくがわいえやす"), ("徳川吉宗", "とくがわよしむね"),
		("明智光秀", "あけちみつひで"), ("廃藩置県", "はいはんちけん"), ("鹿苑寺金閣", "ろくおんじきんかく"),
		("慈照寺銀閣", "じしょうじぎんかく"), ("藤原道長", "ふじわらのみちなが"),
		("神武天皇", "じんむてんのう"), ("邪馬台国", "やまたいこく"), ("桐壺の更衣", "きりつぼのこうい"),
		("御堂関白記", "みどうかんぱくき"), ("満月の夜", "まんげつのよる"), ("しょぼん", "しょぼん"),
		("ペリー来航", "ぺりーらいこう"), ("五稜郭", "ごりょうかく"), ("大正デモクラシー", "たいしょうでもくらしー"),
		("アテルイ", "あてるい"), ("相対性理論", "そうたいせいりろん"), ("クォーク", "くぉーく"), ("炙りカルビ", "あぶりかるび"),
		("横山大観", "よこやまたいかん"), ("法隆寺", "ほうりゅうじ"), ("下村観山", "しもむらかんざん"),
		("アプリ開発", "あぷりかいはつ"), ("機械学習", "きかいがくしゅう"), ("アルゴリズム", "あるごりずむ"),
		("セマフォ", "せまふぉ"), ("ゴシック体", "ごしっくたい"), ("明朝体", "みんちょうたい"),
		("千利休", "せんのりきゅう"), ("リキュール", "りきゅーる"), ("シェイク", "しぇいく"), ("クソ上司", "くそじょうし"),
		("さくらんぼ計算", "さくらんぼけいさん"), ("タピオカパン", "たぴおかぱん"), ("タピオカ", "たぴおか"),
		("インスタ映え", "いんすたばえ"), ("獣道", "けものみち"), ("棒に当たる", "ぼうにあたる"),
		("組合せ爆発", "くみあわせばくはつ"), ("トキ", "とき"), ("サーバルキャット", "さーばるきゃっと"),
		("アライグマ", "あらいぐま"), ("フェネック", "ふぇねっく"), ("アルパカ", "あるぱか"), ("ライオン", "らいおん"),
		("チーター", "ちーたー"), ("カラカル", "からかる"), ("赤唐辛子", "あかとうがらし"), ("クソガキ", "くそがき"),
		("ダイクストラ法", "だいくすとらほう"), ("クソゲー", "くそげー"), ("三浪", "さんろう"), ("100パーセント", "100パーセント"),
		("あおり運転", "あおりうんてん"), ("校長先生", "こうちょうせんせい"), ("留年", "りゅうねん"), ("斎藤さん", "さいとうさん"),
		("鈴木さん", "すずきさん"), ("アセチレン", "あせちれん"), ("ゴミ", "ごみ"), ("ゴリラ", "ごりら"), ("パイソン", "ぱいそん"),
		("パンダ", "ぱんだ"), ("高収入", "こうしゅうにゅう"), ("水素の音", "すいそのおと"), ("バジリスクタイム", "ばじりすくたいむ"),
		("ぽんぽんぺいん", "ぽんぽんぺいん"), ("セキセイインコ", "せきせいいんこ"), ("三角フラスコ", "さんかくふらすこ"),
		("リービッヒ冷却器", "りーびっひれいきゃくき"), ("王水", "おうすい"), ("偏差値2の男", "へんさち2のおとこ"),
		("地球外生命体", "ちきゅうがいせいめいたい"), ("ハレとケの日", "はれとけのひ"), ("龍王", "りゅうおう"),
		("セリヌンティウス", "せりぬんてぃうす"), ("ティラノサウルス", "てぃらのさうるす"), ("プテラノドン", "ぷてらのどん"),
		("トリケラトプス", "とりけらとぷす"), ("速度制限", "そくどせいげん"), ("スマトラトラ", "すまとらとら"),
		("アムールトラ", "あむーるとら"), ("麒麟", "きりん"), ("青龍", "せいりゅう"), ("朱雀", "すざく"),
		("白虎", "びゃっこ"), ("玄武", "げんぶ"), ("アヌビス", "あぬびす"), ("わたあめ", "わたあめ"),
		("デスマッチ", "ですまっち"), ("マッチポイント", "まっちぽいんと"), ("スギ花粉", "すぎかふん"),
		("ヒイラギ", "ひいらぎ"), ("ホウセンカ", "ほうせんか"), ("バラの花", "ばらのはな"), ("ハスカップ", "はすかっぷ"),
		("抹茶パフェ", "まっちゃぱふぇ"), ("マヌルネコ", "まぬるねこ"), ("ドラゴン", "どらごん"), ("九尾の狐", "きゅうびのきつね"),
		("鎌鼬", "かまいたち"), ("森鴎外", "もりおうがい"), ("5000兆円", "5000ちょうえん"), ("確率変動", "かくりつへんどう"),
		("ジャックポット", "じゃっくぽっと"), ("フルハウス", "ふるはうす"), ("シェパード", "しぇぱーど"),
		("みじん切り", "みじんぎり"), ("インディゴブルー", "いんでぃごぶるー"), ("電光石火", "でんこうせっか"),
		("オーブ", "おーぶ"), ("オリーブオイル", "おりーぶおいる"), ("コンキリエ", "こんきりえ")
	};

	private List<(string jp, string h)> qJP2V = new List<(string jp, string h)>() {
		("逮捕されました", "たいほされました"), ("爆発しました", "ばくはつしました"), ("進捗どうですか", "しんちょくどうですか"),
		("お楽しみください", "おたのしみください"), ("動作を停止しました", "どうさをていししました"), ("力尽きました", "ちからつきました"),
		("遊ぶ金が欲しかった", "あそぶかねがほしかった"), ("ネコを吸う", "ねこをすう"), ("タバコはやめられない", "たばこはやめられない"),
		("働く", "はたらく"), ("詐欺にあう", "さぎにあう"), ("財布を忘れる", "さいふをわすれる")
	};

	private int gameModeEasy = ConfigScript.gameModeEasy;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
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
				int r2V = UnityEngine.Random.Range(0, qJP2V.Count);
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
					tmpJpStr = tmpJpStr.Replace("&", qJP2V[r2V].jp);
					tmpQhStr = tmpQhStr.Replace("&", qJP2V[r2V].h);
					if(minLength <= tmpQhStr.Length && tmpQhStr.Length <= maxLength){
						isOK = true;
						jpStr = tmpJpStr;
						qHStr = tmpQhStr;
						Debug.Log(jpStr);
						Debug.Log(qHStr);
					}
				}
			}
			catch {
				isOK = false;
			}
		}
		hiraganaSeparated = ParseHiraganaSentence(qHStr);
		typing = ConstructTypeSentence(hiraganaSeparated);
		return (jpStr, qHStr, hiraganaSeparated, typing);
	}
}
