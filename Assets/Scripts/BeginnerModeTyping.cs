using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BeginnerModeTyping : MonoBehaviour
{

  // Start is called before the first frame update
  void Awake()
  {
    ConfigScript.InfoPanelMode = (int)ConfigScript.MiddlePanel.both;
  }

  // Update is called once per frame
  void Update()
  {
    if (TypingSoft.CurrentGameCondition == (int)TypingSoft.GameCondition.Finished)
    {
      StartCoroutine(FinishedEffect());
    }
    else if (TypingSoft.CurrentGameCondition == (int)TypingSoft.GameCondition.Canceled)
    {
      ReturnConfig();
    }
  }

  /// <summary>
  /// 終了前のエフェクト
  /// </summary>
  private IEnumerator FinishedEffect()
  {
    yield return new WaitForSeconds(0.5f);
    Finished();
  }

  /// <summary>
  /// Config 画面へ戻る
  /// </summary>
  void ReturnConfig()
  {
    SceneManager.LoadScene("BeginnerModeScene");
  }

  /// <summary>
  /// 結果画面へ遷移
  /// </summary>
  void Finished()
  {
    SceneManager.LoadScene("BeginnerModeResultScene");
  }

  /// <summary>
  /// 中段の大きいパネルの切り替え
  /// </summary>
  public void ChangeInfoPanel()
  {
    if (ConfigScript.InfoPanelMode == (int)ConfigScript.MiddlePanel.both)
    {
      ConfigScript.InfoPanelMode = (int)ConfigScript.MiddlePanel.typingPerf;
    }
    else if (ConfigScript.InfoPanelMode == (int)ConfigScript.MiddlePanel.typingPerf)
    {
      ConfigScript.InfoPanelMode = (int)ConfigScript.MiddlePanel.both;
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
      // F8: 中段パネル切り替え
      else if (e.keyCode == KeyCode.F8)
      {
        ChangeInfoPanel();
      }
    }
  }
}
