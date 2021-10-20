using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleSelect : MonoBehaviour
{

  /// <summary>
  /// 初期化
  /// </summary>
  void Start()
  {
  }

  /// <summary>
  /// 1f ごとの処理
  /// </summary>
  void Update()
  {
  }

  /// <summary>
  /// 押されたキーのチェックと対応する操作への移行
  /// <param name="kc">keycode</param>
  /// </summary>
  void KeyCheck(KeyCode kc)
  {
    if (KeyCode.P == kc)
    {
      LoadModeSelectScene();
    }
  }

  /// <summary>
  /// モードセレクトへシーン変更
  /// </summary>
  public void LoadModeSelectScene()
  {
    SceneManager.LoadScene("ModeSelectScene");
  }

  /// <summary>
  /// イベント発生時の処理。主にキーボード入力処理
  /// </summary>
  void OnGUI()
  {
    Event e = Event.current;
    if (e.type == EventType.KeyDown && e.keyCode != KeyCode.None
        && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
    {
      KeyCheck(e.keyCode);
    }
  }
}
