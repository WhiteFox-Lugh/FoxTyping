using System.Collections;
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
  /// キーが入力されたとき等の処理
  /// </summary>
  void OnGUI()
  {
    Event e = Event.current;
    if (e.type == EventType.KeyDown)
    {
      // F1 : リトライ
      if (e.keyCode == KeyCode.F1)
      {
        TypingSoft.RetryPractice();
      }
      // F2 : ワードパネル表示切替
      else if (e.keyCode == KeyCode.F2)
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
      // F3 : 中段の大きいパネルの表示切り替え
      else if (e.keyCode == KeyCode.F3)
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
      // Esc : 中断
      else if (e.keyCode == KeyCode.Escape)
      {
        TypingSoft.CancelPractice();
      }
    }
  }
}
