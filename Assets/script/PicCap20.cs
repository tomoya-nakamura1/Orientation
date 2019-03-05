using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class PicCap20 : MonoBehaviour
{
[SerializeField] string filename;
    GameObject parentCamera;
    GameObject mainCamera;

    GameObject sphereCenter;
    GameObject spherePoint;

    List<Vector3> icosa = new List<Vector3>();
    List<Vector3> dodec = new List<Vector3>(); //dodecahedron

    // Use this for initialization
    public void OnClick()
    {
        mainCamera = GameObject.Find("Main Camera");
        parentCamera = GameObject.Find("parentCamera");

        float magnitude = mainCamera.transform.position.magnitude;
        float g = (1.0f + Mathf.Sqrt(5)) / 2;

        dodec.Add(new Vector3(1.0f, 1.0f, 1.0f));//1,1,1
        dodec.Add(new Vector3(1.0f, 1.0f, -1.0f));
        dodec.Add(new Vector3(1.0f, -1.0f, 1.0f));
        dodec.Add(new Vector3(1.0f, -1.0f, -1.0f));
        dodec.Add(new Vector3(-1.0f, 1.0f, 1.0f));
        dodec.Add(new Vector3(-1.0f, 1.0f, -1.0f));
        dodec.Add(new Vector3(-1.0f, -1.0f, 1.0f));
        dodec.Add(new Vector3(-1.0f, -1.0f, -1.0f));
        dodec.Add(new Vector3(g, 0.0f, 1.0f / g));//最初g
        dodec.Add(new Vector3(g, 0.0f, -1.0f / g));
        dodec.Add(new Vector3(-g, 0.0f, 1.0f / g));
        dodec.Add(new Vector3(-g, 0.0f, -1.0f / g));
        dodec.Add(new Vector3(0.0f, 1.0f / g, g));//最初0
        dodec.Add(new Vector3(0.0f, -1.0f / g, g));
        dodec.Add(new Vector3(0.0f, 1.0f / g, -g));
        dodec.Add(new Vector3(0.0f, -1.0f / g, -g));
        dodec.Add(new Vector3(1.0f / g, g, 0f));//最後0
        dodec.Add(new Vector3(-1.0f / g, g, 0f));
        dodec.Add(new Vector3(1.0f / g, -g, 0f));
        dodec.Add(new Vector3(-1.0f / g, -g, 0f));

        //foreach (var position in dodec)
        //{
        //    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    Vector3 cameraPos = new Vector3();
        //    cameraPos.x = magnitude * position.x;
        //    cameraPos.y = magnitude * position.y;
        //    cameraPos.z = magnitude * position.z;

        //    sphere.transform.position = cameraPos;
        //}
        int count = 0;

        foreach (var position in dodec)
        {
            //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Vector3 cameraPos = new Vector3();

            cameraPos.x = magnitude / position.magnitude * position.x;
            cameraPos.y = magnitude / position.magnitude * position.y;
            cameraPos.z = magnitude / position.magnitude * position.z;

            //cameraPos.x = magnitude * position.x + meshCenter.x;
            //cameraPos.y = magnitude * position.y + meshCenter.y;
            //cameraPos.z = magnitude * position.z + meshCenter.z;

            //sphere.transform.position = cameraPos;
            mainCamera.transform.position = cameraPos;
            mainCamera.transform.LookAt(parentCamera.transform);

            count++;
            int i = count;
            //int j = count;
            Texture2D texture;
            texture = CaptchaScreen(i);

        }

        /*--------------------------------------------------------------------------------------*/


    }
    public Camera ArCam;
    GameObject mesh;

    public Texture2D CaptchaScreen(int num)
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

        //string fileName = "cap_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png";
        string fileName = num.ToString()  + ".png";
        //string fileName = num.ToString() + "_" + num2.ToString() + ".png";

        //File.WriteAllBytes(Application.persistentDataPath + "/" + fileName, bytes);
        File.WriteAllBytes(Application.dataPath + "/" + "compare" + "/"+  filename + "/" + fileName, bytes);

        return screenShot;

    }
}
