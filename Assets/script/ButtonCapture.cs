using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.IO;


public class ButtonCapture : MonoBehaviour {

	
    public Camera ArCam;
    GameObject mesh;

    public void OnClick()
    {
        CaptchaScreen(1,1);
    }

    public Texture2D CaptchaScreen(int num, int num2)
    {
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        RenderTexture rt = new RenderTexture(screenShot.width, screenShot.height, 24);
        RenderTexture prev = ArCam.targetTexture;
        ArCam.targetTexture = rt;
        ArCam.Render();
        ArCam.targetTexture = prev;
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, screenShot.width, screenShot.height), 0, 0);
        screenShot.Apply();

        byte[] bytes = screenShot.EncodeToPNG();
        //UnityEngine.Object.Destroy(screenShot);

        string fileName = "cap_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png";
        //string fileName = num.ToString() + "_" + num2.ToString() + ".png";

        //File.WriteAllBytes(Application.persistentDataPath + "/" + fileName, bytes);
        File.WriteAllBytes(Application.dataPath + "/" + "Captcha" + "/" + fileName, bytes);

        return screenShot;

    }
}
