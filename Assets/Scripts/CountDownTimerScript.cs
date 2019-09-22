using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class CountDownTimerScript : MonoBehaviour {

	private Text countdownText;
	// Use this for initialization
	void Start () {
		countdownText = transform.Find("countdownText").GetComponent<Text>();
		countdownText.text = "";
		StartCoroutine(CountDown());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	IEnumerator CountDown(){
		countdownText.text = "3";
		yield return new WaitForSeconds(1.0f);
		countdownText.text = "2";
		yield return new WaitForSeconds(1.0f);
		countdownText.text = "1";
		yield return new WaitForSeconds(1.0f);
		SceneManager.LoadScene("TypingScene");
	}
}
