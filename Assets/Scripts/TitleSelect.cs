using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleSelect : MonoBehaviour {
	private Text userName;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		userName = GameObject.Find("ModeSelect/UserName").GetComponent<Text>();
		if(UserAuth.currentPlayerName == null){
			userName.text = "Typer ID: Guest";
		}
		else {
			userName.text = "Typer ID: " + UserAuth.currentPlayerName.ToString();
		}
	}

	void KeyCheck(KeyCode k){
		if(KeyCode.P == k){
			// シングルプレイに移行する
			LoadSPScene();
		}
		else if(KeyCode.U == k){
			// レコードに移行
			LoadRecordScene();
		}
		else if(KeyCode.L == k){
			// Log in / out
			LoadLoginScene();
		}
	}

	void LoadSPScene(){
		SceneManager.LoadScene("SinglePlayConfigScene");
	}

	void LoadRecordScene(){
		SceneManager.LoadScene("RecordScene");
	}

	void LoadLoginScene(){
		SceneManager.LoadScene("Login");
	}

	// キーが入力されるたびに発生する
	void OnGUI() {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.type != EventType.KeyUp && e.keyCode != KeyCode.None
		&& !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2)){
			KeyCheck(e.keyCode);
		}
    }

	public void OnClickSPModeButton(){
		LoadSPScene();
	}

	public void OnClickRecordButton(){
		LoadRecordScene();
	}

	public void OnClickRegistButton(){
		LoadLoginScene();
	}
}
