using System;
using System.Net.Mail;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text.RegularExpressions;
using NCMB;
using System.Collections.Generic;


public class UserAuth : MonoBehaviour {
    private UserAuth instance = null;
    public Text errMessage;
    private static string currentPlayerName {
        get;
        set;
    }

    void Start () {
        errMessage = GameObject.Find("CanvasRegister/TextRegister/ErrMessage").GetComponent<Text>();
    }

    // mobile backendに接続してログイン ------------------------
    public void logIn( string id, string pw ) {
        NCMBUser.LogInAsync (id, pw, (NCMBException e) => {
            // 接続成功したら
            if( e == null ){
                currentPlayerName = id;
            }
        });
    }

    // mobile backendに接続して新規会員登録 ------------------------
    public void signUp( string id, string mail, string pw ) {
        NCMBUser user = new NCMBUser();
        bool isValidPass = true;
        bool isValidMail = true;
        bool isValidID = true;
        errMessage.text = "";
        // ID をセット
        if(Regex.IsMatch(id, @"[0-9a-zA-Z]([0-9a-zA-Z_-]{1,29})")){
            isValidID = true;
            user.UserName = id;
        }
        else {
            isValidID = false;
        }
        // パスワードのチェック
        if(Regex.IsMatch(pw, @"[0-9a-zA-Z_-]{8,100}")){
            isValidPass = true;
            user.Password = pw;
        }
        else {
            isValidPass = false;
        }
        // Mail のチェック
        if(mail != null){
            if(Regex.IsMatch(mail, @"\A\P{Cc}+@\P{Cc}+\z",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase)){
                user.Email    = mail;
                isValidMail = true;
            }
            else {
                isValidMail = false;
            }
        }
        else {
            isValidMail = true;
            user.Email = "";
        }
        if(isValidMail && isValidPass && isValidID){
            Debug.Log("Good koyaya");
            /* user.SignUpAsync((NCMBException e) => { 
                if( e == null ){
                    currentPlayerName = id;
                }
            });
            */
        }
        else {
            if(!isValidID){
                Debug.Log("Bad ID");
                errMessage.text += "IDのフォーマットが正しくありません";
            }
            if(!isValidPass){
                Debug.Log("Bad Password");
                errMessage.text += "パスワードのフォーマットが正しくありません\n";
            }
            if(!isValidMail){
                Debug.Log("Bad mail address");
                errMessage.text += "メールアドレスが正しくありません\n";
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

    // 現在のプレイヤー名を返す --------------------
    public string currentPlayer() {
        return currentPlayerName;
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
}
