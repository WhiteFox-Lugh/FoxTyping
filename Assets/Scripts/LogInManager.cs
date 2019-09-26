using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class LogInManager : MonoBehaviour {
    private GameObject canvasLogin;
    private GameObject canvasRegister;
    private string id;
    private string pw;
    private string mail;
    public InputField loginID;
    public InputField loginPW;
    public InputField registID;
    public InputField registPW;
    public InputField registMail;
    
    void Start () {
        FindObjectOfType<UserAuth>().logOut();
        // ゲームオブジェクトを検索し取得する
        canvasLogin = GameObject.Find ("CanvasLogin");
        canvasRegister = GameObject.Find ("CanvasRegister");  
        canvasRegister.SetActive (false);
        canvasLogin.SetActive (true);
    }

    void OnGUI () {
        // currentPlayerを毎フレーム監視し、ログインが完了したら
        if( FindObjectOfType<UserAuth>().currentPlayer() != null ){
            LoadTitleScene();
        }
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