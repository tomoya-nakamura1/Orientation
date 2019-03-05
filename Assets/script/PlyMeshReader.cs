using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

public class PlyMeshReader : MonoBehaviour
{

    public Material material;
    public string fileName;

    // Use this for initialization
    void Start()
    {

        if (ReadFile())
        {
            Debug.Log("OK");
        }
        else
        {
            Debug.Log("Error");
        }

    }

    //ファイルの読み込み
    bool ReadFile()
    {

        try
        {

            string path_name = Path.Combine(Application.dataPath, fileName);
            StreamReader reader = new System.IO.StreamReader(path_name, Encoding.UTF8);

            List<Vector3> sverts = new List<Vector3>();
            //加えた
            List<Color32> svertsColor = new List<Color32>();
            List<int> triangles = new List<int>();

            string line = "";
            int i, numVertex, numTriangle;

            /*-----------------------------------------------------------------------------------*/
            //sculptfabの方

            //データ数の読み込み
            for (i = 0; i < 4; ++i)
            {
                line = reader.ReadLine();
            }

            string[] numbers = line.Split(' ');

            numVertex = int.Parse(numbers[2]);

            for (i = 0; i < 7; ++i)
            {
                line = reader.ReadLine();
            }
            string[] numbers2 = line.Split(' ');
            numTriangle = int.Parse(numbers2[2]);

            for (i = 0; i < 2; ++i)
            {
                line = reader.ReadLine();
            }

            /*-----------------------------------------------------------------------------------*/
            /*-----------------------------------------------------------------------------------*/
            //meshlabで作ったほう

            ////データ数の読み込み
            //for (i = 0; i < 4; ++i)
            //{
            //    line = reader.ReadLine();
            //}

            //string[] numbers = line.Split(' ');

            //numVertex = int.Parse(numbers[2]);

            //for (i = 0; i < 8; ++i)
            //{
            //    line = reader.ReadLine();
            //}
            //string[] numbers2 = line.Split(' ');
            //numTriangle = int.Parse(numbers2[2]);

            //for (i = 0; i < 7; ++i)
            //{
            //    line = reader.ReadLine();
            //}
            /*-----------------------------------------------------------------------------------*/

            //座標
            //for (i = 0; i < numVertex; ++i)
            //Debug.Log(numVertex);

            //Debug.Log(numTriangle);

            for (i = 0; i < numVertex; i++)
            {
                line = reader.ReadLine();
                string[] vertex = line.Split(' ');

                float x = float.Parse(vertex[0]);
                float y = float.Parse(vertex[1]);
                float z = float.Parse(vertex[2]);
                //float x = float.Parse(vertex[0]);
                //float z = float.Parse(vertex[1]);
                //float y = -float.Parse(vertex[2]);
                //変えた　a合ってるかわかんない
                Color32 eachColor = new Color32();
                eachColor.a = 1;
                eachColor.r = byte.Parse(vertex[3]);
                eachColor.g = byte.Parse(vertex[4]);
                eachColor.b = byte.Parse(vertex[5]);


                sverts.Add(new Vector3(x, y, z));
                svertsColor.Add(eachColor);
            }

            //三角形
            //for (i = 0; i < numTriangle; ++i)
                for (i = 0; i < numTriangle; i++)

                {
                    line = reader.ReadLine();
                string[] triangle = line.Split(' ');

                triangles.Add(int.Parse(triangle[1]));
                triangles.Add(int.Parse(triangle[2]));
                triangles.Add(int.Parse(triangle[3]));
            }

            reader.Close();

            //メッシュの設定
            var mesh = new Mesh();
            mesh.vertices = sverts.ToArray();
            mesh.triangles = triangles.ToArray();
            //加えた
            mesh.colors32 = svertsColor.ToArray();


       
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var filter = GetComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            var renderer = GetComponent<MeshRenderer>();
            renderer.material = material;

        }
        catch (System.Exception e)
        {

            Debug.Log(e.Message);
            return false;

        }

        return true;
    }

    // Update is called once per frame
    void Update()
    {

    }
}