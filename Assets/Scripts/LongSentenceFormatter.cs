using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LongSentenceFormatter : MonoBehaviour
{
  private static readonly string sectionRegex = @"\\[s|S]ection\{([\w|\p{P}| ]+)\}[\r|\r\n|\n]";
  private static readonly string rubyRegex = @"\\[r|R]uby\{(?<word>\w+)/(?<ruby>\w+)\}";
  private static readonly string replacement = "<r=$2>$1</r>";
  private static readonly string rubyTagRegex = @"\<r=(?<rb>\w+)\>(?<kanji>\w+)\<\/r\>";
  private static readonly string reverseReplacement = "\\ruby{$2/$1}";
  private static readonly string newLineSymbol = @"{NL}";
  private static readonly string newlinePattern = @"[\n|\r\n|\r]";
  private static string convertedText = "";
  private static string displayText = "";
  private static string taskText = "";
  [SerializeField] private TMP_InputField BeforeConvertInputField;
  [SerializeField] private TMP_InputField AfterConvertInputField;
  [SerializeField] private TMP_InputField DisplayInputField;
  [SerializeField] private TMP_InputField TaskTextField;
  [SerializeField] private TextMeshProUGUI beforeFormatText;
  [SerializeField] private GameObject ConvertPanel;
  [SerializeField] private GameObject ViewPanel;
  [SerializeField] private RubyTextMeshProUGUI PreviewText;

  /// <summary>
  /// 表示文章の生成
  /// </summary>
  private void GenerateTaskText()
  {
    var docDataOrigin = BeforeConvertInputField.text;
    var replacedDoc = Regex.Replace(docDataOrigin, sectionRegex, "");
    taskText = Regex.Replace(replacedDoc, rubyRegex, "$1");
    var taskDisplayText = Regex.Replace(taskText, newlinePattern, "\n");
    var convertedText = Regex.Replace(replacedDoc, rubyRegex, replacement);
    var taskWithRuby = Regex.Replace(convertedText, newlinePattern, "\n");
    DisplayInputField.text = taskText;
    displayText = taskWithRuby;
    Format();
  }

  private void Format()
  {
    var lineIdx = 0;
    var maxLastindex = 0;
    var inputInfo = beforeFormatText.GetTextInfo(displayText);
    var inputLineInfo = inputInfo.lineInfo;
    UnityEngine.Debug.Log(inputInfo);
    for (int i = 0; i < inputLineInfo.Length; ++i)
    {
      var lastIdx = inputLineInfo[i].lastCharacterIndex;
      if (maxLastindex < lastIdx && lastIdx < taskText.Length)
      {
        maxLastindex = inputLineInfo[i].lastCharacterIndex;
        lineIdx = i;
      }
    }
    var taskIdx = taskText.Length - 1;
    var displayIdx = displayText.Length - 1;
    var newlineIdx = maxLastindex;
    const string rubyTagEnd = "</r>";
    bool isRubyTag = false;
    while (taskIdx >= 0 && displayIdx >= 0 && lineIdx >= 0)
    {
      if (displayText[displayIdx] == '>')
      {
        isRubyTag = true;
      }
      else if (displayText[displayIdx] == '<')
      {
        isRubyTag = false;
      }
      if (!isRubyTag && taskText[taskIdx].ToString().Equals(displayText[displayIdx].ToString()))
      {
        if (taskIdx == newlineIdx)
        {
          if (taskText[taskIdx] == '\n')
          {
            displayText = displayText.Insert(displayIdx, "⏎");
          }
          else if (displayIdx + rubyTagEnd.Length < displayText.Length &&
          displayText.Substring(displayIdx + 1, rubyTagEnd.Length).Equals(rubyTagEnd))
          {
            displayText = displayText.Insert(displayIdx + 1 + rubyTagEnd.Length, "\n");
          }
          else
          {
            displayText = displayText.Insert(displayIdx + 1, "\n");
          }
          lineIdx--;
          if (lineIdx >= 0)
          {
            newlineIdx = inputLineInfo[lineIdx].lastCharacterIndex;
          }
        }
        taskIdx--;
      }
      displayIdx--;
    }
    // Rubyタグと矢印を戻す
    var rubyReversed = Regex.Replace(displayText, rubyTagRegex, reverseReplacement);
    convertedText = Regex.Replace(rubyReversed, "⏎", newLineSymbol);
    AfterConvertInputField.text = convertedText;
    MakePreview();
  }

  private void MakePreview()
  {
    int startIdx = 0;
    string replacement = "<r=$2>$1</r>";
    string newlinePattern = @"[\n|\r\n|\r]";
    string aLine;
    StringReader strReader = new StringReader(convertedText);
    StringBuilder sbuilder = new StringBuilder();
    while (true)
    {
      aLine = strReader.ReadLine();
      if (aLine != null)
      {
        UnityEngine.Debug.Log(aLine);
        sbuilder.Append(aLine);
      }
      else
      {
        break;
      }
    }
    var docDataOrigin = convertedText;
    // Section の情報は除去する
    var sectionReplacedDoc = Regex.Replace(docDataOrigin, sectionRegex, "");
    // // ディスプレイ用の作成
    // 改行記号を改行に置き換え
    var tmp = Regex.Replace(sectionReplacedDoc, newLineSymbol, "⏎");
    var taskWithRuby = Regex.Replace(tmp, rubyRegex, replacement);
    PreviewText.UnditedText = taskWithRuby;
    // 課題文作成
    // 改行、NewLine 記号は取り除く
    var newLineReplacedDoc = Regex.Replace(sectionReplacedDoc, newlinePattern, "");
    var newLineSymbolReplacedDoc = Regex.Replace(newLineReplacedDoc, newLineSymbol, "\n");
    // ルビの読みを取り除いて課題文完成
    var rubyReplacedDoc = Regex.Replace(newLineSymbolReplacedDoc, rubyRegex, "$1");
    var taskTextLength = rubyReplacedDoc.Length;
    UnityEngine.Debug.Log($"Text Length: {taskTextLength}");
    TaskTextField.text = rubyReplacedDoc;
  }

  public void OnClickConvertButton()
  {
    GenerateTaskText();
  }

  public void OnClickConvertPanelButton()
  {
    ViewPanel.SetActive(false);
    ConvertPanel.SetActive(true);
  }

  public void OnClickViewPanel()
  {
    ViewPanel.SetActive(true);
    ConvertPanel.SetActive(false);
  }
}
