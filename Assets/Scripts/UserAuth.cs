using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text.RegularExpressions;
using NCMB;
using System.Collections.Generic;


public class UserAuth : MonoBehaviour {
    private static UserAuth instance = null;
    public Text registErrMessage;
    public Text loginErrMessage;
    public Text logoutMessage;
    public static string currentPlayerName {
        get;
        private set;
    }

    void Start () {

    }

    void Update () {
        if(currentPlayerName != null){
            logoutMessage.text = "現在 " + currentPlayerName + " \n でログインしています";
        }
    }

    private string generateLoginErrMessage (string errCode){
        string ret = "";
        if(errCode.Equals(NCMBException.DUPPLICATION_ERROR)){
            ret = "既に使用されているIDです。他のIDをご利用ください。\n";
        }
        else if(errCode.Equals(NCMBException.INCORRECT_PASSWORD)){
            ret = "ID または パスワードが異なります。\n";
        }
        else if(errCode.Equals(NCMBException.REQUIRED)){
            ret = "未入力の項目があります。\n";
        }
        return ret;
    }

    private void returnTitle(){
        SceneManager.LoadScene("TitleScene");
    }

    // mobile backendに接続してログイン ------------------------
    public void logIn( string id, string pw ) {
        loginErrMessage.text = "";
        NCMBUser.LogInAsync (id, pw, (NCMBException e) => {
            // 接続成功したら
            if( e == null ){
                currentPlayerName = id;
                returnTitle();
            }
            else {
                loginErrMessage.text = generateLoginErrMessage(e.ErrorCode);
            }
        });
    }

    // mobile backendに接続して新規会員登録 ------------------------
    public void signUp( string id, string mail, string pw) {
        NCMBUser user = new NCMBUser();
        registErrMessage.text = "";
        bool isValidID = Regex.IsMatch(id, @"[0-9a-zA-Z]([0-9a-zA-Z_-]{1,29})");
        bool isValidPass = Regex.IsMatch(pw, @"[0-9a-zA-Z_-]{8,100}");
        bool isValidMail = mail.Equals("") ||
            Regex.IsMatch(mail, @"\A\P{Cc}+@\P{Cc}+\z",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        // Sign up
        if(isValidMail && isValidPass && isValidID){
            Debug.Log("Good koyaya");
            user.UserName = id;
            user.Password = pw;
            user.Email = mail;
            user.SignUpAsync((NCMBException e) => { 
                if( e == null ){
                    currentPlayerName = id;
                    returnTitle();
                }
                else {

                }
            });
        }
        else {
            if(!isValidID){
                Debug.Log("Bad ID");
                registErrMessage.text += "IDのフォーマットが正しくありません\n";
            }
            if(!isValidPass){
                Debug.Log("Bad Password");
                registErrMessage.text += "パスワードのフォーマットが正しくありません\n";
            }
            if(!isValidMail){
                Debug.Log("Bad mail address");
                registErrMessage.text += "メールアドレスが正しくありません\n";
            }
        }
    }

    // mobile backendに接続してログアウト ------------------------
    public void logOut() {
        NCMBUser.LogOutAsync ( (NCMBException e) => {
            if( e == null ){
                currentPlayerName = null;
            }
        });
    }

    void Awake () {
        if (instance == null) {
            instance = this;
            string name = gameObject.name;
            gameObject.name = name + "(Singleton)";
            GameObject duplicater = GameObject.Find (name);
            if (duplicater != null) {
                Destroy (gameObject);
            } 
            else {
                gameObject.name = name;
            }
        }
        else {
            Destroy (gameObject);
        }
    }
}
