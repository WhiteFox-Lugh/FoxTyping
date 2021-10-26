using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BeginnerModeTyping : MonoBehaviour
{
  // ゲームの状況
  private enum gameCondition
  {
    Progress,
    Finished,
    Canceled,
  };

  // Start is called before the first frame update
  void Awake()
  {
    ConfigScript.InfoPanelMode = 2;
  }

  // Update is called once per frame
  void Update()
  {
    if (TypingSoft.CurrentGameCondition == (int)gameCondition.Canceled)
    {
      ReturnConfig();
    }
  }

  /// <summary>
  /// Config 画面へ戻る
  /// </summary>
  void ReturnConfig()
  {
    SceneManager.LoadScene("BeginnerModeScene");
  }

  /// <summary>
  /// キーが入力されたとき等の処理
  /// </summary>
  void OnGUI()
  {
    Event e = Event.current;
    if (e.type == EventType.KeyDown)
    {
      if (e.keyCode == KeyCode.F1)
      {
        ConfigScript.InfoPanelMode = 2 - ConfigScript.InfoPanelMode;
      }
    }
  }
}
