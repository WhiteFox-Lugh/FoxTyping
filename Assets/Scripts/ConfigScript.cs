using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ConfigScript : MonoBehaviour
{
  public static int Tasks
  {
    set;
    get;
  } = -1;

  // ゲームモード
  // 0 : 短文
  // 1 : 長文
  public static int GameMode
  {
    set;
    get;
  } = 0;

  // 画面中段の表示
  // 0 : タイピングパフォーマンス情報
  // 1 : アシストキーボード
  // 2 : 両方表示
  // 3 : なにも表示しない
  public static int InfoPanelMode
  {
    set;
    get;
  } = 0;

  // 短文打つモードでのデータセットのファイル名
  public static string DataSetName
  {
    set;
    get;
  } = "FoxTypingOfficial";

  // 長文打つモードでのデータセットのファイル名
  public static string LongSentenceTaskName
  {
    set;
    get;
  } = "Long_Constitution";

  // 長文モードでの制限時間
  public static int LongSentenceTimeLimit
  {
    set;
    get;
  } = 300;

  // 長文モードでルビを使用するか
  public static bool UseRuby
  {
    set;
    get;
  } = true;

  // CPU Speed Guide の利用をするか
  public static bool UseCPUGuide
  {
    set;
    get;
  } = true;

  // CPU の kpm 設定
  public static int CPUKpm
  {
    set;
    get;
  } = 300;

  // 初心者モードであるか
  public static bool IsBeginnerMode
  {
    set;
    get;
  } = false;

  // 入力モード
  // 0: Roman
  // 1: Kana
  public static int InputMode
  {
    set;
    get;
  } = 0;
}
