using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

public class OriginalMeshReader : MonoBehaviour
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

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            string line = "";
            int i, numVertex, numTriangle;

            //データ数の読み込み
            for (i = 0; i < 2; ++i)
            {
                line = reader.ReadLine();
            }

            string[] numbers = line.Split(' ');

            numVertex = int.Parse(numbers[0]);
            numTriangle = int.Parse(numbers[1]);

            //座標
            for (i = 0; i < numVertex; ++i)
            {
                line = reader.ReadLine();
                string[] vertex = line.Split(' ');

                float x = float.Parse(vertex[0]);
                float y = float.Parse(vertex[1]);
                float z = float.Parse(vertex[2]);

                vertices.Add(new Vector3(x, y, z));
            }

            //三角形
            for (i = 0; i < numTriangle; ++i)
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
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();

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