using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SinglePlayTyping : MonoBehaviour
{
	// ゲームの状況
	private enum gameCondition {
		Progress,
		Finished,
		Canceled,
	};

	// Start is called before the first frame update
	void Start(){
	}

	// Update is called once per frame
	void Update()
	{
		if (TypingSoft.CurrentGameCondition == (int)gameCondition.Finished){
			StartCoroutine(FinishedEffect());
		}
		else if(TypingSoft.CurrentGameCondition == (int)gameCondition.Canceled){
			ReturnConfig();
		}
	}

	/// <summary>
	/// 終了前のエフェクト
	/// </summary>
	private IEnumerator FinishedEffect() {
    yield return new WaitForSeconds(1f);
		Finished();
  }

	/// <summary>
	/// Config 画面へ戻る
	/// </summary>
	void ReturnConfig() {
		SceneManager.LoadScene("SinglePlayConfigScene");
	}

	/// <summary>
	/// 結果画面へ遷移
	/// </summary>
	void Finished() {
		SceneManager.LoadScene("ResultScene");
	}
}
