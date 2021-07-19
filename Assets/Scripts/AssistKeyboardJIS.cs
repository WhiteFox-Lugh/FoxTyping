using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Shapes2D;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AssistKeyboardJIS : MonoBehaviour {
	[SerializeField] GameObject AKParent;
	// key_name -> GameObject のマップ
	private static Dictionary<string, GameObject> AKKeys = new Dictionary<string, GameObject>();
	// string -> key_name[]
	private static Dictionary<char, string[]> keyMapping = new Dictionary<char, string[]>() {
		{'0', new string[1] {"Key_0"}},
		{'1', new string[1] {"Key_1"}},
		{'2', new string[1] {"Key_2"}},
		{'3', new string[1] {"Key_3"}},
		{'4', new string[1] {"Key_4"}},
		{'5', new string[1] {"Key_5"}},
		{'6', new string[1] {"Key_6"}},
		{'7', new string[1] {"Key_7"}},
		{'8', new string[1] {"Key_8"}},
		{'9', new string[1] {"Key_9"}},
		{'a', new string[1] {"Key_A"}},
		{'b', new string[1] {"Key_B"}},
		{'c', new string[1] {"Key_C"}},
		{'d', new string[1] {"Key_D"}},
		{'e', new string[1] {"Key_E"}},
		{'f', new string[1] {"Key_F"}},
		{'g', new string[1] {"Key_G"}},
		{'h', new string[1] {"Key_H"}},
		{'i', new string[1] {"Key_I"}},
		{'j', new string[1] {"Key_J"}},
		{'k', new string[1] {"Key_K"}},
		{'l', new string[1] {"Key_L"}},
		{'m', new string[1] {"Key_M"}},
		{'n', new string[1] {"Key_N"}},
		{'o', new string[1] {"Key_O"}},
		{'p', new string[1] {"Key_P"}},
		{'q', new string[1] {"Key_Q"}},
		{'r', new string[1] {"Key_R"}},
		{'s', new string[1] {"Key_S"}},
		{'t', new string[1] {"Key_T"}},
		{'u', new string[1] {"Key_U"}},
		{'v', new string[1] {"Key_V"}},
		{'w', new string[1] {"Key_W"}},
		{'x', new string[1] {"Key_X"}},
		{'y', new string[1] {"Key_Y"}},
		{'z', new string[1] {"Key_Z"}},
		{'A', new string[2] {"Key_A", "Key_RShift"}},
		{'B', new string[2] {"Key_B", "Key_RShift"}},
		{'C', new string[2] {"Key_C", "Key_RShift"}},
		{'D', new string[2] {"Key_D", "Key_RShift"}},
		{'E', new string[2] {"Key_E", "Key_RShift"}},
		{'F', new string[2] {"Key_F", "Key_RShift"}},
		{'G', new string[2] {"Key_G", "Key_RShift"}},
		{'H', new string[2] {"Key_H", "Key_LShift"}},
		{'I', new string[2] {"Key_I", "Key_LShift"}},
		{'J', new string[2] {"Key_J", "Key_LShift"}},
		{'K', new string[2] {"Key_K", "Key_LShift"}},
		{'L', new string[2] {"Key_L", "Key_LShift"}},
		{'M', new string[2] {"Key_M", "Key_LShift"}},
		{'N', new string[2] {"Key_N", "Key_LShift"}},
		{'O', new string[2] {"Key_O", "Key_LShift"}},
		{'P', new string[2] {"Key_P", "Key_LShift"}},
		{'Q', new string[2] {"Key_Q", "Key_RShift"}},
		{'R', new string[2] {"Key_R", "Key_RShift"}},
		{'S', new string[2] {"Key_S", "Key_RShift"}},
		{'T', new string[2] {"Key_T", "Key_RShift"}},
		{'U', new string[2] {"Key_U", "Key_LShift"}},
		{'V', new string[2] {"Key_V", "Key_RShift"}},
		{'W', new string[2] {"Key_W", "Key_RShift"}},
		{'X', new string[2] {"Key_X", "Key_RShift"}},
		{'Y', new string[2] {"Key_Y", "Key_LShift"}},
		{'Z', new string[2] {"Key_Z", "Key_RShift"}},
		{' ', new string[1] {"Key_Space"}},
		{'-', new string[1] {"Key_Hyphen"}},
		{',', new string[1] {"Key_Comma"}},
		{'.', new string[1] {"Key_Period"}},
		{';', new string[1] {"Key_Semicolon"}},
		{':', new string[1] {"Key_Colon"}},
		{'[', new string[1] {"Key_LBracket"}},
		{']', new string[1] {"Key_RBracket"}},
		{'@', new string[1] {"Key_At"}},
		{'/', new string[1] {"Key_Slash"}},
		{'!', new string[2] {"Key_1", "Key_RShift"}},
		{'?', new string[2] {"Key_Slash", "Key_LShift"}}
	};
	// キーの色
	private static Color colorGray = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1);
	private static Color colorWhite = new Color(1, 1, 1, 1);
	private static Color colorBlue = new Color(0, 123f / 255f, 1, 1);
	private static Color colorLightBlue = new Color(186f / 255f, 206f / 255f, 1, 1);

	/// <summary>
	/// 初期化処理
	/// </summary>
	void Awake() {
		GetAllKeys();
		SetAllKeyColorWhite();
  }

	/// <summary>
	/// キーのオブジェクトを取得する
	/// </summary>
	private void GetAllKeys() {
		AKKeys = new Dictionary<string, GameObject>();
		for (int i = 0; i < AKParent.transform.childCount; ++i){
			var keyboardRows = AKParent.transform.GetChild(i);
			for (int j = 0; j < keyboardRows.transform.childCount; ++j){
				var obj = keyboardRows.transform.GetChild(j).gameObject;
				AKKeys.Add(obj.name, obj);
			}
		}
	}

	/// <summary>
	/// 指定したキーの色を白に設定する
	/// </summary>
	private void SetKeyColorWhite(string keyName){
		var shape = AKKeys[keyName].GetComponent<Shape>();
		shape.settings.outlineColor = colorGray;
		shape.settings.fillColor = colorWhite;
	}

	/// <summary>
	/// 指定したキーの色を青色に変更する
	/// </summary>
	private void SetKeyColorBlue(string keyName){
		var shape = AKKeys[keyName].GetComponent<Shape>();
		shape.settings.outlineColor = colorBlue;
		shape.settings.fillColor = colorLightBlue;
	}

	/// <summary>
	/// 全てのキーの色を白にする
	/// </summary>
	public void SetAllKeyColorWhite(){
		foreach (var kvp in keyMapping){
			var keyList = new List<string>(kvp.Value);
			foreach (var keyName in keyList){
				SetKeyColorWhite(keyName);
			}
		}
	}

	/// <summary>
	/// 次に打つべき文字をハイライトする
	/// </summary>
	public void SetNextPushKeyColorBlue(char ch){
		SetAllKeyColorWhite();
		var keyList = new List<string>(keyMapping[ch]);
		foreach (var keyName in keyList){
			SetKeyColorBlue(keyName);
		}
	}
}
