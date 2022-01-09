using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabController : MonoBehaviour
{
  void Start()
  {
    PlayFabAuthService.Instance.Authenticate(Authtypes.Silent);
  }
  void OnEnable()
  {
    PlayFabAuthService.OnLoginSuccess += PlayFabLogin_OnLoginSuccess;
  }
  private void PlayFabLogin_OnLoginSuccess(LoginResult result)
  {
    Debug.Log("Login Success!");
  }
  private void OnDisable()
  {
    PlayFabAuthService.OnLoginSuccess -= PlayFabLogin_OnLoginSuccess;
  }
}