using UnityEngine;
using System.Collections;
using System.IO; //FileStream

public class PicStream : MonoBehaviour
{
    //private string imgFile = "unsplash.com-rula-sibai-pink-flowers-small.jpg";

    //// Use this for initialization
    //void Start()
    //{
    //    Texture2D tex = new Texture2D(0, 0);
    //    tex.LoadImage(LoadBin(Application.dataPath + "/" + imgFile));
    //    gameObject.GetComponent<Renderer>().material.mainTexture = tex;
    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

    //byte[] LoadBin(string path)
    //{
    //    FileStream fs = new FileStream(path, FileMode.Open);
    //    BinaryReader br = new BinaryReader(fs);
    //    byte[] buf = br.ReadBytes((int)br.BaseStream.Length);
    //    br.Close();
    //    return buf;
    //}
}