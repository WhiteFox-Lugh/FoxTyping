using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeSelectScript : MonoBehaviour
{

  /// <summary>
  /// 押されたキーのチェックと対応する操作への移行
  /// <param name="kc">keycode</param>
  /// </summary>
  private void KeyCheck(KeyCode kc)
  {
    if (KeyCode.S == kc)
    {
      LoadSinglePlayScene();
    }
    else if (KeyCode.B == kc)
    {
      LoadBeginnerModeScene();
    }
    else if (KeyCode.Escape == kc)
    {
      LoadTitleScene();
    }
  }

  /// <summary>
  /// シングルプレイへシーン変更
  /// </summary>
  public void LoadSinglePlayScene()
  {
    SceneManager.LoadScene("SinglePlayConfigScene");
  }

  /// <summary>
  /// ビギナーモードへシーン変更
  /// </summary>
  public void LoadBeginnerModeScene()
  {
    SceneManager.LoadScene("BeginnerModeScene");
  }

  /// <summary>
  /// その他へシーン変更
  /// </summary>
  public void LoadOtherScene()
  {
    SceneManager.LoadScene("OtherScene");
  }

  /// <summary>
  /// タイトルへ移動
  /// </summary>
  public void LoadTitleScene()
  {
    SceneManager.LoadScene("TitleScene");
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
