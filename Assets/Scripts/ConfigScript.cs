public sealed class ConfigScript
{
  private readonly static ConfigScript instance = new ConfigScript();

  public static ConfigScript GetInstance()
  {
    return instance;
  }
  private ConfigScript()
  {

  }

  public enum SingleMode
  {
    shortSentence,
    longSentence
  }

  // シングルプレイでのモード
  // SingleMode を参照
  public static int GameMode
  {
    set;
    get;
  } = (int)SingleMode.shortSentence;


  // 短文で何個ワードを打つか
  public static int Tasks
  {
    set;
    get;
  } = 10;

  // 画面中段の表示
  public enum MiddlePanel
  {
    typingPerf,
    assistKeyboard,
    both,
    none
  }

  // 画面中段の表示
  // 0 : タイピングパフォーマンス情報
  // 1 : アシストキーボード
  // 2 : 両方表示
  // 3 : なにも表示しない
  public static int InfoPanelMode
  {
    set;
    get;
  } = (int)MiddlePanel.typingPerf;

  // ワードパネルのすぐ下に何を表示するか
  public enum SmallPanel
  {
    nextWord,
    assistSpeed
  }

  // ワードすぐ下の小さいパネルの表示
  // 0 : 次のワード
  // 1 : CPU 速度
  public static int WordPanelMode
  {
    set;
    get;
  } = (int)SmallPanel.nextWord;

  // 短文打つモードでのデータセットのファイル名
  public static string DataSetName
  {
    set;
    get;
  } = "FoxTypingOfficial";

  // 長文打つモードでのデータセットのファイル名
  public static string LongSentenceTaskName
  {
    set;
    get;
  } = "Long_Constitution";

  // 長文モードでの制限時間(s)
  public static int LongSentenceTimeLimit
  {
    set;
    get;
  } = 300;

  // 長文モードでルビを使用するか
  public static bool UseRuby
  {
    set;
    get;
  } = true;

  // CPU の kpm 設定
  public static int CPUKpm
  {
    set;
    get;
  } = 300;

  // 初心者モードであるか
  public static bool IsBeginnerMode
  {
    set;
    get;
  } = false;

  public enum InputType
  {
    roman,
    jisKana
  }

  // 入力モード
  // 0: Roman
  // 1: Kana
  public static int InputMode
  {
    set;
    get;
  } = (int)InputType.roman;

  // 次のセンテンスに移行するまでの休止時間
  public static float DelayTime
  {
    set;
    get;
  } = 0.5f;
}
