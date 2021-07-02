using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleSelect : MonoBehaviour {

	/// <summary>
	/// 初期化
	/// </summary>
	void Start () {
	}

	/// <summary>
	/// 1f ごとの処理
	/// </summary>
	void Update () {
	}

	/// <summary>
	/// 押されたキーのチェックと対応する操作への移行
	/// </summary>
	void KeyCheck(KeyCode k){
		if(KeyCode.P == k){
			// シングルプレイに移行する
			LoadModeSelectScene();
		}
	}

	/// <summary>
	/// モードセレクトへシーン変更
	/// </summary>
	void LoadModeSelectScene(){
		SceneManager.LoadScene("ModeSelectScene");
	}

	/// <summary>
	/// play ボタンを押したときの挙動
	/// </summary>
	public void OnClickPlayButton(){
		LoadModeSelectScene();
	}

	/// <summary>
	/// イベント発生時の処理。主にキーボード入力処理
	/// </summary>
	void OnGUI() {
    Event e = Event.current;
    if (e.type == EventType.KeyDown && e.keyCode != KeyCode.None
				&& !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2)){
			KeyCheck(e.keyCode);
		}
  }
}
