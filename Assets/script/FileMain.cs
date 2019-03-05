using UnityEngine;
using System.Collections;
using System.IO; //System.IO.FileInfo, System.IO.StreamReader, System.IO.StreamWriter
using System; //Exception
using System.Text; //Encoding

public class FileMain : MonoBehaviour
{
    private string guitxt = "";
    private string outputFileName = "tryExternalText.txt";
    // Use this for initialization
    void Start()
    {
        string txt = Application.dataPath;
        string txt2 = Application.persistentDataPath;
        guitxt = "dataPath:" + txt + "\npersistentDataPath:" + txt2 + "\n";
        ReadFile();


        Debug.Log(guitxt);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        GUI.TextArea(new Rect(5, 5, Screen.width, 50), guitxt);
    }

    void WriteFile(string txt)
    {
        FileInfo fi = new FileInfo(Application.dataPath + "/" + outputFileName);
        using (StreamWriter sw = fi.AppendText())
        {
            sw.WriteLine(guitxt);
        }
    }

    void ReadFile()
    {
        FileInfo fi = new FileInfo(Application.dataPath + "/" + outputFileName);
        try
        {
            using (StreamReader sr = new StreamReader(fi.OpenRead(), Encoding.UTF8))
            {
                guitxt = sr.ReadToEnd();
            }
        }
        catch (Exception e)
        {
            guitxt += SetDefaultText();
            WriteFile(guitxt);
        }
    }

    string SetDefaultText()
    {
        return "C#あ\n";
    }
}