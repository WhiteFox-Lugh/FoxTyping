using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class LogInManager : MonoBehaviour {
    private GameObject canvasLogin;
    private GameObject canvasRegister;
    private GameObject canvasLogout;
    private string id;
    private string pw;
    private string mail;
    private string nickname;
    private int screenMode;
    private bool showErr;
    public InputField loginID;
    public InputField loginPW;
    public InputField registID;
    public InputField registPW;
    public InputField registMail;
    public Text registErrMessage;
    public Text loginErrMessage;

    void Start () {
        // ゲームオブジェクトを検索し取得する
        canvasLogin = GameObject.Find ("CanvasLogin");
        canvasRegister = GameObject.Find ("CanvasRegister");  
        canvasLogout = GameObject.Find ("CanvasLogout");
        showErr = false;
        if(UserAuth.Instance != null && UserAuth.currentPlayerName != null){
            canvasRegister.SetActive (false);
            canvasLogin.SetActive (false);
            canvasLogout.SetActive(true);
            screenMode = 1;
        }
        else {
            canvasRegister.SetActive (false);
            canvasLogin.SetActive (true);
            canvasLogout.SetActive(false);
            screenMode = 2;
        }
    }

    void OnGUI () {
        if (screenMode == 2 && showErr){
            loginErrMessage.text = UserAuth.ErrMessage;
        }
        else if(screenMode == 3 && showErr){
            registErrMessage.text = UserAuth.ErrMessage;
        }
        else if(!showErr){
            loginErrMessage.text = "";
            registErrMessage.text = "";
        }
    }

    public void OnClickLoginButton(){
        id = loginID.text;
        pw = loginPW.text;
        FindObjectOfType<UserAuth>().logIn( id, pw );
        showErr = true;
    }

    public void OnClickRegisterButton(){
        id = registID.text;
        pw = registPW.text;
        mail = registMail.text;
        FindObjectOfType<UserAuth>().signUp( id, mail, pw );
        showErr = true;
    }

    public void OnClickLogoutButton () {
        FindObjectOfType<UserAuth>().logOut();
        LoadTitleScene();
    }

    public void OnClickGotoTitleButton () {
        LoadTitleScene();
    }

    public void LoadTitleScene(){
        SceneManager.LoadScene("TitleScene");
    }

    public void drawLogInMenu(){
        // テキスト切り替え
        canvasRegister.SetActive (false);
        canvasLogin.SetActive (true);
        screenMode = 2;
        showErr = false;
    }

    public void drawSignUpMenu(){
        // テキスト切り替え
        canvasLogin.SetActive (false);
        canvasRegister.SetActive (true);
        screenMode = 3;
        showErr = false;
    }
}