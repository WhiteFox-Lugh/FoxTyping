using System;
using System.IO;
using UnityEngine;
using UnityEditor;

public class AssetBundleBuild {

    [MenuItem("Expansion/Build AssetBundleData")]
    public static void Build()
    {
        string assetBundleDirectory = "./AssetBundleData";      // 出力先ディレクトリ

        // 出力先ディレクトリが無かったら作る
        if( !Directory.Exists(assetBundleDirectory) )
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        // AssetBundleのビルド(ターゲット(プラットフォーム)毎に3つ目の引数が違うので注意)
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.WebGL);

        // ビルド終了を表示
        EditorUtility.DisplayDialog("アセットバンドルビルド終了", "アセットバンドルビルドが終わりました", "OK");
    }
}