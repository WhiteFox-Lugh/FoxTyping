

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
  private const int MIN_TASK_NUM = 5;
  private const int MAX_TASK_NUM = 100;
  private const int DEFAULT_TASK_NUM = 20;
  private const int LONG_MIN_TIME_LIMIT = 1;
  private const int LONG_MAX_TIME_LIMIT = 600;
  private const int DEFAULT_LONG_TIME_LIMIT = 300;
  private const int MIN_DELAY_TIME = 0;
  private const int MAX_DELAY_TIME = 3000;
  private const int DEFAULT_DELAY_TIME = 500;
  private const int MIN_COUNTDOWN_SEC = 1;
  private const int MAX_COUNTDOWN_SEC = 5;
  private static int gameMode = (int)SingleMode.shortSentence;
  private static int taskNum = DEFAULT_TASK_NUM;
  private static int infoPanel = (int)MiddlePanel.typingPerf;
  private static int keyInputMode = (int)InputType.roman;
  private static int longSentencePracticeTimeLimit = DEFAULT_LONG_TIME_LIMIT;
  private static int wordChangeDelayTime = DEFAULT_DELAY_TIME;
  private static bool showTypeSentence = true;
  private static int countdownSec = 3;
  private static int inputArray = 0;

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

  // 画面中段の表示
  public enum MiddlePanel
  {
    typingPerf,
    assistKeyboard,
    both,
    none
  }

  public enum InputType
  {
    roman,
    jisKana
  }

  public static readonly string[] InputTypeString = new string[2] {
    "ローマ字(Qwerty)", "JISかな"
  };

  public enum KeyArrayType
  {
    japanese,
    us
  }

  // シングルプレイでのモード
  // SingleMode を参照
  [JsonProperty]
  public static int GameMode
  {
    set
    {
      var enumLen = Enum.GetNames(typeof(SingleMode)).Length;
      if (0 <= value && value < enumLen)
      {
        gameMode = value;
      }
      else
      {
        gameMode = (int)SingleMode.shortSentence;
      }
    }
    get
    {
      return gameMode;
    }
  }


  // 短文で何個ワードを打つか
  [JsonProperty]
  public static int Tasks
  {
    set
    {
      if (MIN_TASK_NUM <= value && value <= MAX_TASK_NUM)
      {
        taskNum = value;
      }
      else
      {
        taskNum = DEFAULT_TASK_NUM;
      }
    }
    get
    {
      return taskNum;
    }
  }

  // 画面中段の表示
  // 0 : タイピングパフォーマンス情報
  // 1 : アシストキーボード
  // 2 : 両方表示
  // 3 : なにも表示しない
  [JsonProperty]
  public static int InfoPanelMode
  {
    set
    {
      if (0 <= value && value <= 3)
      {
        infoPanel = value;
      }
      else
      {
        infoPanel = (int)MiddlePanel.typingPerf;
      }
    }
    get
    {
      return infoPanel;
    }
  }

  // 短文打つモードでのデータセットのファイル名
  [JsonProperty]
  public static string DataSetName
  {
    set;
    get;
  } = "FoxTypingOfficial";

  // 長文打つモードでのデータセットのファイル名
  [JsonProperty]
  public static string LongSentenceTaskName
  {
    set;
    get;
  } = "Constitution";

  // 長文モードでの制限時間(s)
  [JsonProperty]
  public static int LongSentenceTimeLimit
  {
    set
    {
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

  // 長文モードでルビを使用するか
  [JsonProperty]
  public static bool UseRuby
  {
    set;
    get;
  } = true;

  // 初心者モードであるか
  public static bool IsBeginnerMode
  {
    set;
    get;
  } = false;

  // 入力モード
  // 0: Roman
  // 1: Kana
  [JsonProperty]
  public static int InputMode
  {
    set
    {
      var enumLen = Enum.GetNames(typeof(InputType)).Length;
      if (0 <= value && value < enumLen)
      {
        keyInputMode = value;
      }
      else
      {
        keyInputMode = (int)InputType.roman;
      }
    }
    get
    {
      return keyInputMode;
    }
  }

  /// <summary>
  /// 配列の指定
  /// </summary>
  /// <value></value>
  [JsonProperty]
  public static int InputArray
  {
    set
    {
      var enumLen = Enum.GetNames(typeof(KeyArrayType)).Length;
      if (0 <= value && value < enumLen)
      {
        inputArray = value;
      }
      else
      {
        inputArray = (int)KeyArrayType.japanese;
      }
    }
    get
    {
      return inputArray;
    }
  }

  [JsonProperty]
  // 次のセンテンスに移行するまでの休止時間
  public static int DelayTime
  {
    set
    {
      if (MIN_DELAY_TIME <= value && value <= MAX_DELAY_TIME)
      {
        wordChangeDelayTime = value;
      }
      else if (value < MIN_DELAY_TIME)
      {
        wordChangeDelayTime = MIN_DELAY_TIME;
      }
      else if (value > MAX_DELAY_TIME)
      {
        wordChangeDelayTime = MAX_DELAY_TIME;
      }
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
  public static bool IsShowTypeSentence
  {
    set
    {
      showTypeSentence = value;
    }
    get
    {
      return showTypeSentence;
    }
  }

  /// 以下コンフィグ画面のみ
  /// <summary>
  /// カウントダウンの時間
  /// </summary>
  /// <value></value>
  [JsonProperty]
  public static int CountDownSecond
  {
    set
    {
      if (MIN_COUNTDOWN_SEC <= value && value <= MAX_COUNTDOWN_SEC)
      {
        countdownSec = value;
      }
      else if (value < MIN_COUNTDOWN_SEC)
      {
        countdownSec = MIN_COUNTDOWN_SEC;
      }
      else
      {
        countdownSec = MAX_COUNTDOWN_SEC;
      }
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
  /// 設定を読み込む
  /// </summary>
  public static void LoadConfig()
  {
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
