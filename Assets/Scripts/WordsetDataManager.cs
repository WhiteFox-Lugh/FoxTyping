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
  [SerializeField] Button TitlePlayButton;
  [SerializeField] Text NowLoadingText;
  private const string NOW_LOADING = "Now Loading...";
  private const string SUCCESS_TEXT = "Load Complete!";
  private const string FAILED_TEXT = "Failed to load word data";
  private static bool isTryWordsetLoading = false;

  private delegate void OnComplete(AssetBundle data);
  void Awake()
  {
    isTryWordsetLoading = false;
    if (TitlePlayButton != null)
    {
      TitlePlayButton.interactable = false;
    }
    if (NowLoadingText != null)
    {
      NowLoadingText.text = NOW_LOADING;
    }
    StartCoroutine("LoadAssetBundleData");
  }

  public IEnumerator LoadAssetBundleData()
  {
    bool isLoadSuccessMetadata = false;
    bool isLoadSuccessShortData = false;
    bool isLoadSuccessLongData = false;
    if (WordsetData.AssetMetaData == null)
    {
      var path = ABMetaDataPathLocal;
#if UNITY_WEBGL && !UNITY_EDITOR
      path = ABMetaDataPath;
#endif
      yield return StartCoroutine(
        FetchAssetBundleData(path, (AssetBundle data) =>
        {
          if (data != null)
          {
            WordsetData.AssetMetaData = data;
            isLoadSuccessMetadata = true;
          }
        })
        );
    }
    if (WordsetData.AssetShortWordsetData == null)
    {
      var path = ABShortWordsetDataPathLocal;
#if UNITY_WEBGL && !UNITY_EDITOR
      path = ABShortWordsetDataPath;
#endif
      yield return StartCoroutine(
      FetchAssetBundleData(path, (AssetBundle data) =>
      {
        if (data != null)
        {
          WordsetData.AssetShortWordsetData = data;
          isLoadSuccessShortData = true;
        }
      })
      );
    }
    if (WordsetData.AssetLongWordsetData == null)
    {
      var path = ABLongWordsetDataPathLocal;
#if UNITY_WEBGL && !UNITY_EDITOR
      path = ABLongWordsetDataPath;
#endif
      yield return StartCoroutine(
      FetchAssetBundleData(path, (AssetBundle data) =>
      {
        if (data != null)
        {
          WordsetData.AssetLongWordsetData = data;
          isLoadSuccessLongData = true;
        }
      })
      );
    }
    isTryWordsetLoading = true;
    if (TitlePlayButton != null)
    {
      TitlePlayButton.interactable = true;
    }
    if (isLoadSuccessMetadata && isLoadSuccessShortData && isLoadSuccessLongData)
    {
      NowLoadingText.text = SUCCESS_TEXT;
    }
    else
    {
      NowLoadingText.text = FAILED_TEXT;
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