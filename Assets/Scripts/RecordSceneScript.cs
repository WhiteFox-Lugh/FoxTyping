using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RecordSceneScript : MonoBehaviour
{
    private int currentRating = 0;
    private int nextRank = 0;
    private bool isShowHelpRating;
    private List<Color> ratingColor = new List<Color> {
		new Color(128f / 255f, 128f / 255f, 128f / 255f, 1f),
		new Color(0f / 255f, 0f / 255f, 0f / 255f, 1f),
		new Color(139f / 255f, 69f / 255f, 19f / 255f, 1f),
		new Color(30f / 255f, 30f / 255f, 255f / 255f, 1f),
		new Color(0f / 255f, 191f / 255f, 255f / 255f, 1f),
		new Color(60f / 255f, 179f / 255f, 113f / 255f, 1f),
		new Color(241f / 255f, 211f / 255f, 5f / 255f, 1f),
		new Color(255f / 255f, 140f / 255f, 0f / 255f, 1f),
		new Color(213f / 255f, 30f / 255f, 30f / 255f, 1f),
		new Color(188f / 255f, 87f / 255f, 242f / 255f, 1f)
	};
	private int[] ratingPartition = {
		1000, 1500, 2000, 2500, 3000, 3500, 4000, 4500, 5000,
		5500, 6500, 7500, 8500, 10000000
	};

    public TextMeshProUGUI UIRating;
    public Text UINextRank;
    public Text UIRatingDetail;
    public Text UILunaticHiscore;
    public Material ratingRainbow;
	public Material ratingGold;
	public Material ratingSilver;
	public Material ratingCopper;
    public GameObject canvasHelpRating;

    // Start is called before the first frame update
    void Awake(){
        InitData();
        GetUserData();
        SetRating();
    }

    // Update is called once per frame
    void Update(){
        
    }

    void InitData(){
        isShowHelpRating = false;
        canvasHelpRating.SetActive(isShowHelpRating);
        UIRatingDetail.text = "";
        currentRating = 0;
        nextRank = 0;
    }

    void GetUserData(){
        int i = 1;
		foreach (int topScore in UserData.scoreNormal){
			currentRating += topScore;
            UIRatingDetail.text += i.ToString() + ": " + topScore.ToString();
            UIRatingDetail.text += (i == 5) ? "\n" : "  ";
            ++i;
		}
        UILunaticHiscore.text = UserData.scoreLunatic.ToString();
	}

    public void OnClickHelpRatingButton (){
        isShowHelpRating = !isShowHelpRating;
        canvasHelpRating.SetActive(isShowHelpRating);
    }

    void SetRating(){
        int colorNum = 0;
        for (int i = 0; i < ratingPartition.Length; ++i){
			if(currentRating < ratingPartition[i]){
				colorNum = i;
				nextRank = ratingPartition[i] - currentRating;
				break;
			}
		}
		if(colorNum <= 9){
			UIRating.color = ratingColor[colorNum];
		}
		else if(colorNum == 10){
			UIRating.fontMaterial = ratingCopper;
		}
		else if(colorNum == 11){
			UIRating.fontMaterial = ratingSilver;
		}
		else if(colorNum == 12){
			UIRating.fontMaterial = ratingGold;
		}
		else if(colorNum <= 13){
			UIRating.fontMaterial = ratingRainbow;
		}
        UIRating.text = currentRating.ToString();
		UINextRank.text = ((nextRank <= 10000) ? nextRank.ToString() : " --- ");
    }

    void KeyCheck(KeyCode k){
		if(KeyCode.Escape == k){
			SceneManager.LoadScene("TitleScene");
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
