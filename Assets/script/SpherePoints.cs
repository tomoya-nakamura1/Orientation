using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpherePoints : MonoBehaviour
{
    GameObject parentCamera;
    GameObject mainCamera;

    GameObject sphereCenter;
    GameObject spherePoint;

    List<Vector3> icosa = new List<Vector3>();
    List<Vector3> dodec = new List<Vector3>(); //dodecahedron

    // Use this for initialization
    void Start()
    {
        //parentCamera = GameObject.Find("parentCamera");
        mainCamera = GameObject.Find("Main Camera");
        //for (int i = 0; i < 12; i++)
        //{
        //    parentCamera.transform.Rotate(0, 30, 0);
        //    for (int j = 0; j < 12; j++)
        //    {
        //        parentCamera.transform.Rotate(30, 0, 0);
        //        Vector3 position;
        //        position = mainCamera.transform.position;

        //        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //        sphere.transform.position = position;
        //    }
        //}

        //sphereCenter = GameObject.Find("SphereCenter");
        //spherePoint = GameObject.Find("SpherePoint");

        //float nx = 4;
        //float ny = 5;
        //for (int x=0;x<nx;x++)
        //{
        //   float lon = 360 * ((x + 0.5f) / nx);
        //    for(int y = 0; y < ny; y++)
        //    {
        //        float midpt = (y + 0.5f) / ny;
        //        float lat = 180 * Mathf.Asin(2 * ((y + 0.5f) / ny - 0.5f));

        //        //Vector3 position = new Vector3(1,0,0);
        //        //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //        //sphere.transform.position = position;
        //        //Vector3 rot = new Vector3(lon,lat,0);
        //        //sphere.transform.Rotate ( rot);

        //        Vector3 rot = new Vector3(0, lat, 0);
        //        sphereCenter.transform.Rotate(rot);
        //        Vector3 position = spherePoint.transform.position;
        //        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //        sphere.transform.position = position;
        //    }

        //}
        /*--------------------------------------------------------------------------------------*/
        //float magnitude= mainCamera.transform.position.magnitude;
        //float g = (1.0f + Mathf.Sqrt(5)) / 2;

        //icosa.Add(new Vector3(1.0f, g, 0.0f));//A1
        //icosa.Add(new Vector3(-1.0f, g, 0.0f));
        //icosa.Add(new Vector3(1.0f, -g, 0.0f));
        //icosa.Add(new Vector3(-1.0f, -g, 0.0f));
        //icosa.Add(new Vector3(g, 0.0f, 1.0f));//B1
        //icosa.Add(new Vector3(g, 0.0f, -1.0f));
        //icosa.Add(new Vector3(-g, 0.0f, 1.0f));
        //icosa.Add(new Vector3(-g, 0.0f, -1.0f));
        //icosa.Add(new Vector3(0.0f, 1.0f, g));//C1
        //icosa.Add(new Vector3(0.0f, -1.0f, g));
        //icosa.Add(new Vector3(0.0f, 1.0f, -g));
        //icosa.Add(new Vector3(0.0f, -1.0f, -g));

        //foreach(var position in icosa)
        //{
        //    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    Vector3 cameraPos = new Vector3();
        //    cameraPos.x = magnitude * position.x;
        //    cameraPos.y = magnitude * position.y;
        //    cameraPos.z = magnitude * position.z;

        //    sphere.transform.position = cameraPos;
        //}
        /*--------------------------------------------------------------------------------------*/
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

        foreach (var position in dodec)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Vector3 cameraPos = new Vector3();
            cameraPos.x = magnitude * position.x;
            cameraPos.y = magnitude * position.y;
            cameraPos.z = magnitude * position.z;

            sphere.transform.position = cameraPos;
        }

        //foreach (var position in dodec)
        //{
        //    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    sphere.GetComponent<Renderer>().material.color = new Color(1,1,0);
        //    Vector3 cameraPos = new Vector3();
        //    cameraPos.x = magnitude * position.x;
        //    cameraPos.y = magnitude * position.y;
        //    cameraPos.z = magnitude * position.z;
        //    cameraPos =
        //        Quaternion.AngleAxis(45, new Vector3(1,0,0)) * cameraPos;
        //    //cameraPos = cameraPos * Quaternion(0,0,0.0);

        //    sphere.transform.position = cameraPos;
        //}


        /*--------------------------------------------------------------------------------------*/


    }
}
