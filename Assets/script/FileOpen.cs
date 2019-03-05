using UnityEngine;
using System.Collections;
using System.IO; //System.IO.FileInfo, System.IO.StreamReader, System.IO.StreamWriter
using System; //Exception
using System.Text; //Encoding

// ファイル読み込み
public class FileOpen : MonoBehaviour
{

    private string guitxt = "";

    // Update is called once per frame
    void Update()
    {
        // スペースキーを押したらファイル読み込みする
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ReadFile();
        }
    }
    // 読み込んだ情報をGUIとして表示
    void OnGUI()
    {
        GUI.TextArea(new Rect(5, 5, Screen.width, 50), guitxt);
    }

    // 読み込み関数
    void ReadFile()
    {
        // FileReadTest.txtファイルを読み込む
        FileInfo fi = new FileInfo(Application.dataPath + "/" + "FileReadTest.txt");
        try
        {
            // 一行毎読み込み
            using (StreamReader sr = new StreamReader(fi.OpenRead(), Encoding.UTF8))
            {
                guitxt = sr.ReadToEnd();
            }
        }
        catch //(Exception e)
        {
            // 改行コード
            guitxt += SetDefaultText();
        }
    }

    // 改行コード処理
    string SetDefaultText()
    {
        return "C#あ\n";
    }
}
