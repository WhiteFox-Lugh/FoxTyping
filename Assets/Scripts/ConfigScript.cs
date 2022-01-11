

using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public sealed class ConfigScript
{
  private readonly static ConfigScript instance = new ConfigScript();
  private readonly static string configFileName = $"{Application.persistentDataPath}/foxtyping_config_data.json";
  // ワード数の最小値、最大値、デフォルトの設定
  private const int MIN_TASK_NUM = 5;
  private const int MAX_TASK_NUM = 100;
  private const int DEFAULT_TASK_NUM = 20;
  // 長文での制限時間の最小値、最大値、デフォルト値の設定
  private const int LONG_MIN_TIME_LIMIT = 1;
  private const int LONG_MAX_TIME_LIMIT = 600;
  private const int DEFAULT_LONG_TIME_LIMIT = 300;
  // 短文練習で次のワードを表示するまでの Delay
  private const int MIN_DELAY_TIME = 0;
  private const int MAX_DELAY_TIME = 3000;
  private const int DEFAULT_DELAY_TIME = 500;
  // カウントダウン時間
  private const int MIN_COUNTDOWN_SEC = 1;
  private const int MAX_COUNTDOWN_SEC = 5;
  private static int gameMode = (int)SingleMode.shortSentence;
  private static int taskNum = DEFAULT_TASK_NUM;
  private static int infoPanel = (int)MiddlePanel.typingPerf;
  private static int keyInputMode = (int)InputType.roman;
  private static int longSentencePracticeTimeLimit = DEFAULT_LONG_TIME_LIMIT;
  private static int wordChangeDelayTime = DEFAULT_DELAY_TIME;
  private static int countdownSec = 3;
  private static int inputArray = 0;

  /// <summary>
  /// コンストラクタ
  /// </summary>
  private ConfigScript()
  {

  }

  /// <summary>
  /// インスタンスの取得
  /// </summary>
  /// <returns>インスタンス</returns>
  public static ConfigScript GetInstance()
  {
    return instance;
  }

  /// <summary>
  /// シングルプレイでのモード
  /// </summary>
  public enum SingleMode
  {
    shortSentence,
    longSentence
  }

  /// <summary>
  /// シングルプレイの練習形態
  /// </summary>
  /// <value>SingleMode の値</value>
  [JsonProperty]
  public static int GameMode
  {
    set
    {
      var enumLen = Enum.GetNames(typeof(SingleMode)).Length;
      // 値が SingleMode に含まれるものであるかの Validation
      if (0 <= value && value < enumLen) { gameMode = value; }
      else { gameMode = (int)SingleMode.shortSentence; }
    }
    get
    {
      return gameMode;
    }
  }

  /// <summary>
  /// 短文練習でのワード数設定
  /// </summary>
  /// <value>ワード数</value>
  [JsonProperty]
  public static int Tasks
  {
    set
    {
      // 値の Validation
      if (MIN_TASK_NUM <= value && value <= MAX_TASK_NUM) { taskNum = value; }
      else { taskNum = DEFAULT_TASK_NUM; }
    }
    get
    {
      return taskNum;
    }
  }

  /// <summary>
  /// 画面中段の表示
  /// </summary>
  public enum MiddlePanel
  {
    typingPerf,
    assistKeyboard,
    both,
    none
  }

  /// <summary>
  /// 画面中段の表示
  /// </summary>
  /// <value>
  /// 0 : タイピングパフォーマンス情報
  /// 1 : アシストキーボード
  /// 2 : 両方表示
  /// 3 : なにも表示しない
  /// </value>
  [JsonProperty]
  public static int InfoPanelMode
  {
    set
    {
      // 値の Validation
      if (0 <= value && value <= 3) { infoPanel = value; }
      else { infoPanel = (int)MiddlePanel.typingPerf; }
    }
    get
    {
      return infoPanel;
    }
  }

  /// <summary>
  /// 短文練習のワードセットのファイル名
  /// </summary>
  /// <value>ワードセットファイル名(拡張子なし)</value>
  [JsonProperty]
  public static string DataSetName
  {
    set;
    get;
  } = "FoxTypingOfficial";

  /// <summary>
  /// 長文練習の文章ファイル名
  /// </summary>
  /// <value>ファイル名(拡張子なし)</value>
  [JsonProperty]
  public static string LongSentenceTaskName
  {
    set;
    get;
  } = "Constitution";

  /// <summary>
  /// 長文モードでの制限時間
  /// </summary>
  /// <value>制限時間(秒)</value>
  [JsonProperty]
  public static int LongSentenceTimeLimit
  {
    set
    {
      // 値の Validation
      if (LONG_MIN_TIME_LIMIT <= value && value <= LONG_MAX_TIME_LIMIT)
      {
        longSentencePracticeTimeLimit = value;
      }
      else
      {
        longSentencePracticeTimeLimit = DEFAULT_LONG_TIME_LIMIT;
      }
    }
    get
    {
      return longSentencePracticeTimeLimit;
    }
  }

  /// <summary>
  /// 長文モードでのルビ使用
  /// </summary>
  /// <value>ルビを使用するかどうか true / false</value>
  [JsonProperty]
  public static bool UseRuby { set; get; } = true;

  /// <summary>
  /// ビギナーモードでの練習であるかどうか
  /// </summary>
  /// <value>true ならビギナーモード / false なら通常の練習</value>
  public static bool IsBeginnerMode
  {
    set;
    get;
  } = false;

  /// <summary>
  /// 入力方式
  /// </summary>
  public enum InputType
  {
    roman,
    jisKana
  }

  /// <summary>
  /// 入力方式の名称
  /// </summary>
  /// <value></value>
  public static readonly string[] InputTypeString = new string[2] {
    "ローマ字(Qwerty)", "JISかな"
  };

  /// <summary>
  /// 入力モード
  /// </summary>
  /// <value>
  /// 0: Roman
  /// 1: Kana
  /// </value>
  [JsonProperty]
  public static int InputMode
  {
    set
    {
      var enumLen = Enum.GetNames(typeof(InputType)).Length;
      // 値が InputType に含まれるかどうかの Validation
      if (0 <= value && value < enumLen) { keyInputMode = value; }
      else { keyInputMode = (int)InputType.roman; }
    }
    get
    {
      return keyInputMode;
    }
  }

  /// <summary>
  /// キー配列
  /// </summary>
  public enum KeyArrayType
  {
    japanese,
    us
  }

  /// <summary>
  /// キー配列の指定
  /// </summary>
  /// <value>KeyArrayType の値</value>
  [JsonProperty]
  public static int InputArray
  {
    set
    {
      var enumLen = Enum.GetNames(typeof(KeyArrayType)).Length;
      // 値が KeyArrayType に含まれるかどうかの Vaildation
      if (0 <= value && value < enumLen) { inputArray = value; }
      else { inputArray = (int)KeyArrayType.japanese; }
    }
    get
    {
      return inputArray;
    }
  }

  /// <summary>
  /// 次のセンテンスに移行するまでのディレイ
  /// </summary>
  /// <value>ディレイ時間</value>
  [JsonProperty]
  public static int DelayTime
  {
    set
    {
      // 値の Validation
      if (MIN_DELAY_TIME <= value && value <= MAX_DELAY_TIME) { wordChangeDelayTime = value; }
      else if (value < MIN_DELAY_TIME) { wordChangeDelayTime = MIN_DELAY_TIME; }
      else if (value > MAX_DELAY_TIME) { wordChangeDelayTime = MAX_DELAY_TIME; }
    }
    get
    {
      return wordChangeDelayTime;
    }
  }

  /// <summary>
  /// タイプ文字列をミスタイプ前から表示するか
  /// </summary>
  /// <value></value>
  [JsonProperty]
  public static bool IsShowTypeSentence { set; get; } = true;

  /// 以下コンフィグ画面のみ
  /// <summary>
  /// カウントダウンの時間
  /// </summary>
  /// <value>カウントダウン時間</value>
  [JsonProperty]
  public static int CountDownSecond
  {
    set
    {
      // 値の Validation
      if (MIN_COUNTDOWN_SEC <= value && value <= MAX_COUNTDOWN_SEC) { countdownSec = value; }
      else if (value < MIN_COUNTDOWN_SEC) { countdownSec = MIN_COUNTDOWN_SEC; }
      else { countdownSec = MAX_COUNTDOWN_SEC; }
    }
    get
    {
      return countdownSec;
    }
  }

  /// <summary>
  /// 設定を JSON 形式で保存する
  /// </summary>
  public static void SaveConfig()
  {
    var json = JsonConvert.SerializeObject(instance);
    var writer = new StreamWriter(configFileName, false);
    writer.Write(json);
    writer.Flush();
    writer.Close();
  }

  /// <summary>
  /// JSON ファイルから設定を読み込む
  /// </summary>
  public static void LoadConfig()
  {
    UnityEngine.Debug.Log(configFileName);
    if (File.Exists(configFileName))
    {
      var reader = new StreamReader(configFileName);
      var dataStr = reader.ReadToEnd();
      reader.Close();
      var obj = JsonConvert.DeserializeObject<JsonConfigVars>(dataStr);
      var props = obj.GetType().GetProperties();
      foreach (var prop in props)
      {
        var propVal = typeof(ConfigScript).GetProperty(prop.Name);
        propVal.SetValue(instance, prop.GetValue(obj));
      }
    }
  }
}

/// <summary>
/// 設定保存用の JSON ファイルに記載するプロパティを記載したクラス
/// </summary>
public class JsonConfigVars
{
  public int GameMode { get; set; }
  public int Tasks { get; set; }
  public int InfoPanelMode { get; set; }
  public string DataSetName { get; set; }
  public string LongSentenceTaskName { get; set; }
  public int LongSentenceTimeLimit { get; set; }
  public bool UseRuby { get; set; }
  public int InputMode { get; set; }
  public int DelayTime { get; set; }
  public bool IsShowTypeSentence { get; set; }
  public int CountDownSecond { get; set; }
}
