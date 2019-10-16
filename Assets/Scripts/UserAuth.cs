using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text.RegularExpressions;
using NCMB;
using System.Collections.Generic;


public class UserAuth : MonoBehaviour {
    private static UserAuth instance = null;

    public static string currentPlayerName {
        get;
        private set;
    }

    public static UserAuth Instance {
        get {
            return instance;
        }
    }

    public static string ErrMessage {
        get;
        private set;
    }

    void Awake () {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad (gameObject);
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

    private string generateErrMessage (string errCode){
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
        ErrMessage = "";
        NCMBUser.LogInAsync (id, pw, (NCMBException e) => {
            // 接続成功したら
            if( e == null ){
                currentPlayerName = id;
                UserData.fetch();
                returnTitle();
            }
            else {
                Debug.Log(e.ErrorCode);
                ErrMessage = generateErrMessage(e.ErrorCode);
            }
        });
    }

    // mobile backendに接続して新規会員登録 ------------------------
    public void signUp( string id, string mail, string pw) {
        NCMBUser user = new NCMBUser();
        ErrMessage = "";
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
            user.SignUpAsync((NCMBException e) => { 
                if( e == null ){
                    currentPlayerName = id;
                    UserData.fetch();
                    returnTitle();
                }
                else {
                    ErrMessage = generateErrMessage(e.ErrorCode);
                }
            });
        }
        else {
            if(!isValidID){
                Debug.Log("Bad ID");
                ErrMessage += "IDのフォーマットが正しくありません\n";
            }
            if(!isValidPass){
                Debug.Log("Bad Password");
                ErrMessage += "パスワードのフォーマットが正しくありません\n";
            }
            if(!isValidMail){
                Debug.Log("Bad mail address");
                ErrMessage += "メールアドレスが正しくありません\n";
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
}
