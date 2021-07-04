using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;

public class ConfigScript : MonoBehaviour {

	private const int taskUnit = 5;
	private const int defaultTasksOption = 3;
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
		UISentenceNum.enabled = true;
		UITextSentenceNumDescription.color = colorBlack;
		UITextSentenceNum.color = colorBlack;
	}
	// Update is called once per frame
	void Update () {
	}

	void KeyCheck(KeyCode kc){
		if(KeyCode.Return == kc || KeyCode.KeypadEnter == kc){
			GameMode = UIGameMode.value;
			Tasks = (UISentenceNum.value + 1) * taskUnit;
			SceneManager.LoadScene("CountDownScene");
		}
		else if(KeyCode.Escape == kc){
			SceneManager.LoadScene("TitleScene");
		}
		else if(KeyCode.J == kc){
			UISentenceNum.value++;
		}
		else if(KeyCode.F == kc){
			UISentenceNum.value--;
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
