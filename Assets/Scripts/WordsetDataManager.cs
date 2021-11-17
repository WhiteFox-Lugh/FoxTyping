using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public sealed class WordsetDataManager : MonoBehaviour
{
  private const string ABMetaDataPathLocal = "AssetBundleData/wordset_metadata";
  private const string ABShortWordsetDataPathLocal = "AssetBundleData/wordset_short";
  private const string ABLongWordsetDataPathLocal = "AssetBundleData/wordset_long";
  private const string ABMetaDataPath = "https://whitefox-lugh.github.io/FoxTyping/AssetBundleData/wordset_metadata";
  private const string ABShortWordsetDataPath = "https://whitefox-lugh.github.io/FoxTyping/AssetBundleData/wordset_short";
  private const string ABLongWordsetDataPath = "https://whitefox-lugh.github.io/FoxTyping/AssetBundleData/wordset_long";

  private delegate void OnComplete(AssetBundle data);
  void Start()
  {
    StartCoroutine("LoadAssetBundleData");
  }

  public IEnumerator LoadAssetBundleData()
  {
    if (WordsetData.AssetMetaData == null)
    {
      var path = ABMetaDataPathLocal;
#if UNITY_WEBGL && !UNITY_EDITOR
      path = ABMetaDataPath;
#endif
      yield return StartCoroutine(
        FetchAssetBundleData(path, (AssetBundle data) => { WordsetData.AssetMetaData = data; })
        );
    }
    if (WordsetData.AssetShortWordsetData == null)
    {
      var path = ABShortWordsetDataPathLocal;
#if UNITY_WEBGL && !UNITY_EDITOR
      path = ABShortWordsetDataPath;
#endif
      yield return StartCoroutine(
      FetchAssetBundleData(path, (AssetBundle data) => { WordsetData.AssetShortWordsetData = data; })
      );
    }
    if (WordsetData.AssetLongWordsetData == null)
    {
      var path = ABLongWordsetDataPathLocal;
#if UNITY_WEBGL && !UNITY_EDITOR
      path = ABLongWordsetDataPath;
#endif
      yield return StartCoroutine(
      FetchAssetBundleData(path, (AssetBundle data) => { WordsetData.AssetLongWordsetData = data; })
      );
    }
  }

  /// <summary>
  /// AssetBundle の読み込み
  /// </summary>
  private IEnumerator FetchAssetBundleData(string path, OnComplete callback)
  {
    var networkState = Application.internetReachability;
    AssetBundle abData = null;
    // WebGL 時は WebRequest によって AssetBundle を取得
#if UNITY_WEBGL && !UNITY_EDITOR
		if (networkState == NetworkReachability.NotReachable){
			UnityEngine.Debug.Log("ネットワークに接続していません");
		}
    else {
      UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(path);
      yield return request.SendWebRequest();
      if (request.isNetworkError || request.isHttpError){
        UnityEngine.Debug.LogError(request.error);
      }
      else {
        abData = DownloadHandlerAssetBundle.GetContent(request);
        UnityEngine.Debug.Log("load successfully");
      }
    }
#else
    abData = AssetBundle.LoadFromFile(path);
#endif
    callback(abData);
    yield break;
  }
}