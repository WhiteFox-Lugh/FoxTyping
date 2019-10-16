using NCMB;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

public class UserData : MonoBehaviour{
    private static UserData instance = null;
    public static UserData Instance {
        get {
            return instance;
        }
        set {
            instance = value;
        }
    }

    public static int[] scoreNormal { get; set; } = new int[] {};

    public static int scoreLunatic { get; set; } = 0;

    public static ulong typedCount { get; set; } = 0;
    public static string userMame {
        get {
            return UserAuth.currentPlayerName;
        }
    }

    public static void userLogOut(){
        scoreLunatic = 0;
        scoreNormal = new int[] {};
        typedCount = 0;
    }

    // サーバーにハイスコアを保存 -------------------------
    public static void save(){
        // データストアの「UserData」クラスから、Nameをキーにして検索
        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject> ("UserData");
        query.WhereEqualTo ("Name", userMame);
        query.FindAsync ((List<NCMBObject> objList ,NCMBException e) => {
            //検索成功したら
            if (e == null) {
                objList[0]["scoreNormal"] = scoreNormal;
                objList[0]["scoreLunatic"] = scoreLunatic;
                objList[0]["typedCount"] = typedCount;
                objList[0].SaveAsync();
            }
        });
    }

    // サーバーからハイスコアを取得  -----------------
    public static void fetch(){
        // データストアの「UserData」クラスから、Nameをキーにして検索
        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject> ("UserData");
        query.WhereEqualTo ("Name", userMame);
        query.FindAsync ((List<NCMBObject> objList ,NCMBException e) => {
            //検索成功したら
            if (e == null) {
                // ハイスコアが未登録だったら
                if( objList.Count == 0 ){
                    NCMBObject obj = new NCMBObject("UserData");
                    scoreNormal = new int[10]{
                        0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0
                    };
                    obj["Name"]  = userMame;
                    obj["scoreNormal"] = scoreNormal;
                    obj["scoreLunatic"] = scoreLunatic;
                    obj["typedCount"] = typedCount;
                    obj.SaveAsync();
                }
                // ハイスコアが登録済みだったら
                else {
                    ArrayList arr = (ArrayList)(objList[0]["scoreNormal"]);
                    scoreNormal = new int[10];
                    for (int i = 0; i < arr.Count; ++i){
                        scoreNormal[i] = System.Convert.ToInt32(arr[i]);
                    }
                    scoreNormal = scoreNormal.OrderByDescending(x => x).ToArray();
                    
                    scoreLunatic = System.Convert.ToInt32(objList[0]["scoreLunatic"]);
                    typedCount = System.Convert.ToUInt64(objList[0]["typedCount"]);
                }
            }
        });
    }

    void Awake () {
        if (instance == null) {
            instance = this;
        }
    }
}