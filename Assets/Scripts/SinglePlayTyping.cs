using System.Collections;
using System.Security.Permissions;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SinglePlayTyping : MonoBehaviour
{
  // ゲームの状況
  private enum GameCondition
  {
    Progress,
    Finished,
    Canceled,
  };

  // Start is called before the first frame update
  void Awake()
  {
  }

  // Update is called once per frame
  void Update()
  {
    if (TypingSoft.CurrentGameCondition == (int)GameCondition.Finished)
    {
      StartCoroutine(FinishedEffect());
    }
    else if (TypingSoft.CurrentGameCondition == (int)GameCondition.Canceled)
    {
      ReturnConfig();
    }
  }

  /// <summary>
  /// 終了前のエフェクト
  /// </summary>
  private IEnumerator FinishedEffect()
  {
    yield return new WaitForSeconds(1f);
    Finished();
  }

  /// <summary>
  /// Config 画面へ戻る
  /// </summary>
  void ReturnConfig()
  {
    SceneManager.LoadScene("SinglePlayConfigScene");
  }

  /// <summary>
  /// 結果画面へ遷移
  /// </summary>
  void Finished()
  {
    SceneManager.LoadScene("ResultScene");
  }

  /// <summary>
  /// 小さいパネルの切り替え
  /// </summary>
  public void ChangeSmallPanel()
  {
    if (ConfigScript.WordPanelMode == (int)ConfigScript.SmallPanel.nextWord)
    {
      ConfigScript.WordPanelMode = (int)ConfigScript.SmallPanel.assistSpeed;
    }
    else if (ConfigScript.WordPanelMode == (int)ConfigScript.SmallPanel.assistSpeed)
    {
      ConfigScript.WordPanelMode = (int)ConfigScript.SmallPanel.nextWord;
    }
  }

  /// <summary>
  /// 中段の大きいパネルの切り替え
  /// </summary>
  public void ChangeInfoPanel()
  {
    if (ConfigScript.InfoPanelMode == (int)ConfigScript.MiddlePanel.typingPerf)
    {
      ConfigScript.InfoPanelMode = (int)ConfigScript.MiddlePanel.assistKeyboard;
    }
    else if (ConfigScript.InfoPanelMode == (int)ConfigScript.MiddlePanel.assistKeyboard)
    {
      ConfigScript.InfoPanelMode = (int)ConfigScript.MiddlePanel.typingPerf;
    }
  }

  /// <summary>
  /// キーが入力されたとき等の処理
  /// </summary>
  void OnGUI()
  {
    Event e = Event.current;
    // Esc: リトライか中断か
    if (e.type == EventType.KeyDown)
    {
      if (e.keyCode == KeyCode.Escape)
      {
        // カウントダウン中なら中断
        if (TypingSoft.CurrentGameCondition == (int)TypingSoft.GameCondition.Countdown)
        {
          TypingSoft.CancelPractice();
        }
        // 練習中ならリトライ
        else
        {
          TypingSoft.RetryPractice();
        }
      }
      // F2: 次ワード / CPU 切り替え
      else if (e.keyCode == KeyCode.F2)
      {
        ChangeSmallPanel();
      }
      // F8: 中段パネル切り替え
      else if (e.keyCode == KeyCode.F8)
      {
        ChangeInfoPanel();
      }
    }
  }
}
