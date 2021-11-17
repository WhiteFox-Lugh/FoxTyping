using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public sealed class WordsetData
{
  private static AssetBundle _assetMetaData;
  public static AssetBundle AssetMetaData
  {
    get
    {
      return _assetMetaData;
    }
    set
    {
      _assetMetaData = value;
      var jsonStr = _assetMetaData.LoadAsset<TextAsset>("Dataset").ToString();
      var problemData = JsonUtility.FromJson<Metadata>(jsonStr);
      var sDict = new Dictionary<string, ShortWordsetData>();
      foreach (var item in problemData.ShortWordsetDataList)
      {
        var filename = item.WordsetFileName;
        sDict.Add(filename, item);
      }
      var lDict = new Dictionary<string, LongWordsetData>();
      foreach (var item in problemData.LongWordsetDataList)
      {
        var filename = item.DocumentFileName;
        lDict.Add(filename, item);
      }
      ShortWordsetDict = sDict;
      LongWordsetDict = lDict;
    }
  }
  public static AssetBundle AssetShortWordsetData { get; set; }
  public static AssetBundle AssetLongWordsetData { get; set; }
  public static Dictionary<string, ShortWordsetData> ShortWordsetDict { get; private set; } = new Dictionary<string, ShortWordsetData>();
  public static Dictionary<string, LongWordsetData> LongWordsetDict { get; private set; } = new Dictionary<string, LongWordsetData>();
}

[Serializable]
public class Metadata
{
  public ShortWordsetData[] ShortWordsetDataList;
  public LongWordsetData[] LongWordsetDataList;
}

[Serializable]
public class ShortWordsetData
{
  public string WordsetFileName;
  public string WordsetScreenName;
  public bool IsRated;
  public string Language;
}

[Serializable]
public class LongWordsetData
{
  public string DocumentFileName;
  public string WordsetScreenName;
  public string Language;
  public bool HasRuby;
}