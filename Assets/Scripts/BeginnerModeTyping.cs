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
  /// キーが入力されたとき等の処理
  /// </summary>
  void OnGUI()
  {
    Event e = Event.current;
    if (e.type == EventType.KeyDown)
    {
      // F1 が押されたときは、リトライ
      if (e.keyCode == KeyCode.F1)
      {
        TypingSoft.RetryPractice();
      }
      // F2 が押されたときは、パネル表示の切り替え
      else if (e.keyCode == KeyCode.F2)
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
      // Esc : 中断
      else if (e.keyCode == KeyCode.Escape)
      {
        TypingSoft.CancelPractice();
      }
    }
  }
}
