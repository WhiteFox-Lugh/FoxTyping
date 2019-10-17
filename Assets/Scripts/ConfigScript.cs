using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;

public class ConfigScript : MonoBehaviour {

	private const int taskUnit = 5;
	private const int defaultTasksOption = 3;
	public const int gameModeNormal = 0;
	public const int gameModeEasy = 1;
	public const int gameModeLunatic = 2;
	private Dropdown UIGameMode;
	private Dropdown UISentenceNum;
	private Text UITextSentenceNum;
	private Text UITextSentenceNumDescription;
	private Color colorBlack = new Color(0f / 255f, 0f / 255f, 0f / 255f, 1f);
	private Color colorGray = new Color(128f / 255f, 128f / 255f, 128f / 255f, 0.6f);

	public static int Tasks {
		private set;
		get;
	}

	public static int GameMode {
		private set;
		get;
	}

	public static string DataSetName {
		private set;
		get;
	} = "official";

	// Use this for initialization
	void Start () {
		UIGameMode = GameObject.Find("GameMode/Dropdown").GetComponent<Dropdown>();
		UISentenceNum = GameObject.Find("SentenceNum/Dropdown").GetComponent<Dropdown>();
		UITextSentenceNum = GameObject.Find("SentenceNum/TextSentenceNum").GetComponent<Text>();
		UITextSentenceNumDescription = GameObject.Find("SentenceNum/DescriptionSentenceNum").GetComponent<Text>();
		UIGameMode.value = GameMode;
		UISentenceNum.value = (Tasks > 0) ? (Tasks / taskUnit - 1) : defaultTasksOption;
	}
	
	// Update is called once per frame
	void Update () {
		DrawOption();
	}

	void KeyCheck(KeyCode k){
		if(KeyCode.Return == k || KeyCode.KeypadEnter == k){
			GameMode = UIGameMode.value;
			Tasks = (UISentenceNum.value + 1) * taskUnit;
			SceneManager.LoadScene("CountDownScene");
		}
		else if(KeyCode.Escape == k){
			SceneManager.LoadScene("TitleScene");
		}
		else if(KeyCode.N == k){
			UIGameMode.value = gameModeNormal;
		}
		else if(KeyCode.E == k){
			UIGameMode.value = gameModeEasy;
		}
		else if(KeyCode.J == k && UIGameMode.value != gameModeLunatic){
			UISentenceNum.value++;
		}
		else if(KeyCode.F == k && UIGameMode.value != gameModeLunatic){
			UISentenceNum.value--;
		}
	}

	void DrawOption(){
		if(UIGameMode.value == gameModeLunatic){
			UISentenceNum.enabled = false;
			UITextSentenceNumDescription.color = colorGray;
			UITextSentenceNum.color = colorGray;
		}
		else {
			UISentenceNum.enabled = true;
			UITextSentenceNumDescription.color = colorBlack;
			UITextSentenceNum.color = colorBlack;
		}
	}

	void OnGUI() {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.type != EventType.KeyUp && e.keyCode != KeyCode.None
		&& !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2)){
			KeyCheck(e.keyCode);
		}
    }
}
