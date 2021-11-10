

using System;
using UnityEngine;

[Serializable]
public sealed class ConfigScript
{
  private readonly static ConfigScript instance = null;
  private const int MIN_TASK_NUM = 5;
  private const int MAX_TASK_NUM = 100;
  private const int DEFAULT_TASK_NUM = 30;
  private const int LONG_MIN_TIME_LIMIT = 1;
  private const int LONG_MAX_TIME_LIMIT = 60 * 60;
  private const int DEFAULT_LONG_TIME_LIMIT = 5 * 60;
  private const int CPU_KPM_MIN = 1;
  private const int CPU_KPM_MAX = 10000;
  private const int CPU_KPM_DEFAULT = 300;
  private const float MIN_DELAY_TIME = 0f;
  private const float MAX_DELAY_TIME = 3f;
  private const float DEFAULT_DELAY_TIME = 0.5f;
  private readonly string filePath = Application.persistentDataPath;
  [SerializeField]
  private static int gameMode = (int)SingleMode.shortSentence;
  [SerializeField]
  private static int taskNum = DEFAULT_TASK_NUM;
  [SerializeField]
  private static int infoPanel = (int)MiddlePanel.typingPerf;
  [SerializeField]
  private static int wordPanel = (int)SmallPanel.nextWord;
  [SerializeField]
  private static int keyInputMode = (int)InputType.roman;
  [SerializeField]
  private static int longSentencePracticeTimeLimit = DEFAULT_LONG_TIME_LIMIT;
  [SerializeField]
  private static int cpuKpm = CPU_KPM_DEFAULT;
  [SerializeField]
  private static float wordChangeDelayTime = DEFAULT_DELAY_TIME;

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

  // ワードパネルのすぐ下に何を表示するか
  public enum SmallPanel
  {
    nextWord,
    assistSpeed
  }

  // シングルプレイでのモード
  // SingleMode を参照
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


  // ワードすぐ下の小さいパネルの表示
  // 0 : 次のワード
  // 1 : CPU 速度
  public static int WordPanelMode
  {
    set
    {
      var enumLen = Enum.GetNames(typeof(SmallPanel)).Length;
      if (0 <= value && value < enumLen)
      {
        wordPanel = value;
      }
      else
      {
        wordPanel = (int)SmallPanel.nextWord;
      }
    }
    get
    {
      return wordPanel;
    }
  }

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
  public static bool UseRuby
  {
    set;
    get;
  } = true;

  // CPU の kpm 設定
  public static int CPUKpm
  {
    set
    {
      if (CPU_KPM_MIN <= value && value <= CPU_KPM_MAX)
      {
        cpuKpm = value;
      }
      else if (value < CPU_KPM_MIN)
      {
        cpuKpm = CPU_KPM_MIN;
      }
      else if (value > CPU_KPM_MAX)
      {
        cpuKpm = CPU_KPM_MAX;
      }
    }
    get
    {
      return cpuKpm;
    }
  }

  // 初心者モードであるか
  public static bool IsBeginnerMode
  {
    set;
    get;
  } = false;

  // 入力モード
  // 0: Roman
  // 1: Kana
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

  // 次のセンテンスに移行するまでの休止時間
  public static float DelayTime
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

  public static ConfigScript GetInstance()
  {
    return instance;
  }

  /// <summary>
  /// 設定を JSON 形式で返す
  /// </summary>
  /// <returns>JSON</returns>
  public static string GetJsonString()
  {
    return JsonUtility.ToJson(instance);
  }
}
