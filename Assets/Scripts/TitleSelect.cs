using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleSelect : MonoBehaviour {
	private Button UISP;
	private Button UIREC;

	// Use this for initialization
	void Start () {
		UISP = transform.Find("ButtonPlay").GetComponent<Button>();
		UIREC = transform.Find("ButtonRecord").GetComponent<Button>();
	}
	
	// Update is called once per frame
	void Update () {
		
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
		// SceneManager.LoadScene("RecordScene");
	}

	void LoadLoginScene(){
		// SceneManager.LoadScene("LoginScene");
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
}
