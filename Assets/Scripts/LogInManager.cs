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
    public InputField loginID;
    public InputField loginPW;
    public InputField registID;
    public InputField registPW;
    public InputField registMail;

    void Awake () {
        // ゲームオブジェクトを検索し取得する
        canvasLogin = GameObject.Find ("CanvasLogin");
        canvasRegister = GameObject.Find ("CanvasRegister");  
        canvasLogout = GameObject.Find ("CanvasLogout");
        if(UserAuth.currentPlayerName != null){
            canvasRegister.SetActive (false);
            canvasLogin.SetActive (false);
            canvasLogout.SetActive(true);
        }
        else {
            canvasRegister.SetActive (false);
            canvasLogin.SetActive (true);
            canvasLogout.SetActive(false);
        }
    }

    void OnGUI () {
        // currentPlayerを毎フレーム監視し、ログインが完了したら
        /*
        if( FindObjectOfType<UserAuth>().currentPlayer() != null ){
            LoadTitleScene();
        }
        */
    }

    public void OnClickLoginButton(){
        id = loginID.text;
        pw = loginPW.text;
        FindObjectOfType<UserAuth>().logIn( id, pw );
    }

    public void OnClickRegisterButton(){
        id = registID.text;
        pw = registPW.text;
        mail = registMail.text;
        FindObjectOfType<UserAuth>().signUp( id, mail, pw );
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
    }

    public void drawSignUpMenu(){
        // テキスト切り替え
        canvasLogin.SetActive (false);
        canvasRegister.SetActive (true);
    }
}