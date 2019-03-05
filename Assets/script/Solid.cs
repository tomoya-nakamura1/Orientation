using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Linq;

public class Solid
{
    public List<Vertex> sverts = new List<Vertex>();        //頂点の配列
         
    public List<Face> sfaces = new List<Face>();//面の配列
    public List<Edge> sedges = new List<Edge>();//稜線の配列
    public List<Color32> svertsColor = new List<Color32>();//頂点の色の配列//頂点のメンバー変数にもcolorが存在する注意


    public List<int> triangles = new List<int>();
    public int  numVertex, numTriangle, numEdges;
    //public List<Vector3> sverts = new List<Vector3>(); List<Face*> sfaces;
    //面の配列
    //public List<Vector3> sverts = new List<Vector3>(); List<Edge*> sedges; 
    //稜線の配列

    //Bouding Box
    public Vector3 bBmax, bBmin; //箱の端点
    public float bBdiag;       //箱の対角線長さ
                                //public GameObject jiu(){
                                //    GameObject aaa = new GameObject(PrimitiveType.Cube);
                                //}
                                //public Vector3 centerXZ=new Vector3() ;//xz座標についてだけ中心座標，これだけずらすことによってメッシュの回転軸を中心にする
    public float centerX;
    public float centerZ;

    public Vector3 meshCenter;
                                //plyデータからメッシュデータを読み込んでから回転を加えて、メッシュの型で返す
                                //使ってる方
    public Mesh PlyRotationReturnMesh(string fileName, float angle)
    {

        string path_name = Path.Combine(Application.dataPath, fileName);
        StreamReader readerCenter = new System.IO.StreamReader(path_name, Encoding.UTF8);

        //List<Vector3> sverts = new List<Vector3>();//頂点
        //List<int> triangles = new List<int>();

        string line = "";
        int i;
        //    /*==========================================================================================================*/

        /*-----------------------------------------------------------------------------------*/
        //sculptfabの方
        //データ数の読み込み
        /*----------------------------------------------------------------------------------*/
        //データ数の読み込み
        List<Vector3> originalSvertsCen = new List<Vector3>();

        for (i = 0; i < 4; ++i)
        {
            line = readerCenter.ReadLine();
        }
        string[] numbersCen = line.Split(' ');

        numVertex = int.Parse(numbersCen[2]);

        for (i = 0; i < 7; ++i)
            //for (i = 0; i < 8; ++i)
            {
            line = readerCenter.ReadLine();
        }
        numbersCen = line.Split(' ');
        numTriangle = int.Parse(numbersCen[2]);

        for (i = 0; i < 2; ++i)
        {
            line = readerCenter.ReadLine();
        }


       
        //座標
        for (i = 0; i < numVertex; ++i)
        {

            var vertex_temp = new Vertex();
            line = readerCenter.ReadLine();
            string[] vertex = line.Split(' ');

            float x = float.Parse(vertex[0]);
            float y = float.Parse(vertex[1]);
            float z = float.Parse(vertex[2]);

            //yとz入れ替える
            //float x = float.Parse(vertex[0]);
            //float z = float.Parse(vertex[1]);
            //float y = -float.Parse(vertex[2]);


            //x = -x;
            // z = -z;
            // y = -y;

            Vector3 original_sverts = new Vector3(x, y, z);
            //Vector3 rotate_sverts = new Vector3();
            //rotate_sverts =
            //    Quaternion.AngleAxis(angle, Vector3.up) * original_sverts;

            originalSvertsCen.Add(original_sverts);

            // vertex_temp.vr = original_sverts;
            //sverts.Add(vertex_temp);
            //vertex_vec_list.Add(rotate_sverts);
        }
        float xMin = originalSvertsCen[0].x;
        float xMax = originalSvertsCen[0].x;
        float zMin = originalSvertsCen[0].y;
        float zMax = originalSvertsCen[0].y;



        foreach (var vert in originalSvertsCen)
        {

            if (xMax < vert.x)
            {
                xMax = vert.x;
            }
            if (xMin > vert.x)
            {
                xMin = vert.x;
            }

            if (zMax < vert.z)
            {
                zMax = vert.z;
            }
            if (zMin > vert.z)
            {
                zMin = vert.z;
            }

        }

        centerX = (xMax + xMin) / 2f;
        centerZ = (zMax + zMin) / 2f;
        bBdiag = Mathf.Sqrt((xMax - xMin) * (xMax - xMin) + (zMax - zMin) * (zMax - zMin));

        /*----------------------------------------------------------------------------------*/

        StreamReader reader = new System.IO.StreamReader(path_name, Encoding.UTF8);
        for (i = 0; i < 4; ++i)
        {
            line = reader.ReadLine();
        }

        string[] numbers = line.Split(' ');

        numVertex = int.Parse(numbers[2]);

        for (i = 0; i < 7; ++i)     //colored_penguin
            //for (i = 0; i < 8; ++i)
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
        //    /*==========================================================================================================*/

        //Rhinocerosの方
        //データ数の読み込み

        //for (i = 0; i < 4; ++i)
        //{
        //    line = reader.ReadLine();
        //}

        //string[] numbers = line.Split(' ');

        //numVertex = int.Parse(numbers[2]);

        //for (i = 0; i < 4; ++i)
        //{
        //    line = reader.ReadLine();
        //}
        //string[] numbers2 = line.Split(' ');
        //numTriangle = int.Parse(numbers2[2]);

        //for (i = 0; i < 2; ++i)
        //{
        //    line = reader.ReadLine();
        //}
        /*-----------------------------------------------------------------------------------*/
        /*-----------------------------------------------------------------------------------*/
        //万通に通じるやり方
        //データ数の読み込み
        //while (true)
        //{
        //    line = reader.ReadLine();

        //    string[] numbers = line.Split(' ');

        //    if (numbers[1] == "vertex")
        //    {
        //        numVertex = int.Parse(numbers[2]);
        //        break;
        //    }
        //}
        //while (true)
        //{
        //    line = reader.ReadLine();

        //    string[] numbers2 = line.Split(' ');

        //    if (numbers2[1] == "face")
        //    {
        //        numTriangle = int.Parse(numbers2[2]);
        //        break;
        //    }
        //}
        //while (true)
        //{
        //    line = reader.ReadLine();


        //    if (line == "end_header")
        //    {

        //        break;
        //    }
        //}


        /*-----------------------------------------------------------------------------------*/



        List<Vector3> vertex_vec_list = new List<Vector3>();
        //座標
        for (i = 0; i < numVertex; ++i)
        {

            var vertex_temp = new Vertex();
            line = reader.ReadLine();
            string[] vertex = line.Split(' ');

            float x = float.Parse(vertex[0]);
            float y = float.Parse(vertex[1]);
            float z = float.Parse(vertex[2]);


            //float x = float.Parse(vertex[0]);
            //float z = float.Parse(vertex[1]);
            //float y = -float.Parse(vertex[2]);

            //yとz入れ替える
            //float x = float.Parse(vertex[0]);
            //float z = float.Parse(vertex[1]);
            //float y = -float.Parse(vertex[2]);


            //x = -x;
            // z = -z;
            // y = -y;

            Vector3 original_sverts = new Vector3(x, y, z);
            Vector3 rotate_sverts = new Vector3();
            rotate_sverts =
                Quaternion.AngleAxis(angle, Vector3.up) * original_sverts;

          //  Quaternion.Angle();

            //Color32 eachColor = new Color32();
            //eachColor.a = 1;
            //eachColor.r = byte.Parse(vertex[3]);
            //eachColor.g = byte.Parse(vertex[4]);
            //eachColor.b = byte.Parse(vertex[5]);

            Color eachColor = new Color();
            eachColor.a = 1;
            eachColor.r = float.Parse(vertex[3]);
            eachColor.g = float.Parse(vertex[4]);
            eachColor.b = float.Parse(vertex[5]);


            vertex_temp.vr = rotate_sverts;
            sverts.Add(vertex_temp);
            vertex_vec_list.Add(rotate_sverts);

            svertsColor.Add(eachColor);
            sverts[i].color=eachColor;

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
        mesh.vertices = vertex_vec_list.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        //加えた
        mesh.colors32 = svertsColor.ToArray();

        ////メッシュデータを読み込んでBBmax, BBminに値を入れる
        //BBmax = mesh.bounds.center+mesh.bounds.extents;
        //BBmin = mesh.bounds.center - mesh.bounds.extents;



        return mesh;
    }
    //メッシュデータを読み込んでからメッシュの型で返す//使ってない
    public Mesh ReturnMesh(string fileName)
    {

        string path_name = Path.Combine(Application.dataPath, fileName);
        StreamReader reader = new System.IO.StreamReader(path_name, Encoding.UTF8);

        //List<Vector3> sverts = new List<Vector3>();//頂点
        //List<int> triangles = new List<int>();

        string line = "";
        int i;

        //データ数の読み込み
        for (i = 0; i < 2; ++i)
        {
            line = reader.ReadLine();
        }

        string[] numbers = line.Split(' ');

        numVertex = int.Parse(numbers[0]);
        numTriangle = int.Parse(numbers[1]);

       // var vertex_temp = new Vertex();
        List<Vector3> vertex_vec_list = new List<Vector3>();
        //座標
        for (i = 0; i < numVertex; ++i)
        {
            var vertex_temp = new Vertex();
            line = reader.ReadLine();
            string[] vertex = line.Split(' ');
         //   var vertex_vec = new Vertex();

            float x = float.Parse(vertex[0]);
            float y = float.Parse(vertex[1]);
            float z = float.Parse(vertex[2]);

            vertex_temp.vr = new Vector3(x,y,z);

            // sverts.Add(new Vector3(x, y, z));
            sverts.Add(vertex_temp);
            vertex_vec_list.Add(new Vector3(x, y, z));
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

        mesh.vertices = vertex_vec_list.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    //offデータからメッシュデータを読み込んでから回転を加えて、メッシュの型で返す
    //使ってる方//座標の中心を0にするプログラムを作成中
    public Mesh RotationReturnMesh(string fileName,float angle)
    {

        string path_name = Path.Combine(Application.dataPath, fileName);
        StreamReader reader = new System.IO.StreamReader(path_name, Encoding.UTF8);
        StreamReader readerCenter = new System.IO.StreamReader(path_name, Encoding.UTF8);//中心を判定してずらす


        //List<Vector3> sverts = new List<Vector3>();//頂点
        //List<int> triangles = new List<int>();

        string line = "";
        int i;
        List<Vector3> originalSvertsCen = new List<Vector3>();
        /*----------------------------------------------------------------------------------*/
        //データ数の読み込み
        for (i = 0; i < 2; ++i)
        {
            line = readerCenter.ReadLine();
        }
        string[] numbersCen = line.Split(' ');

        numVertex = int.Parse(numbersCen[0]);
        numTriangle = int.Parse(numbersCen[1]);
        numEdges = int.Parse(numbersCen[2]);
        /*----------------------------------------------------------------------------------*/

        /*--------------------------------------plyファイルの形状決めのみ--------------------------------------------*/
        ////sculptfabの方
        ////データ数の読み込み

        //for (i = 0; i < 4; ++i)
        //{
        //    line = readerCenter.ReadLine();
        //}

        //string[] numbersCen1 = line.Split(' ');

        //numVertex = int.Parse(numbersCen1[2]);

        //for (i = 0; i < 7; ++i)
        //{
        //    line = readerCenter.ReadLine();
        //}
        //string[] numbersCen2 = line.Split(' ');
        //numTriangle = int.Parse(numbersCen2[2]);

        //for (i = 0; i < 2; ++i)
        //{
        //    line = readerCenter.ReadLine();
        //}

        /*----------------------------------------------------------------------------------*/

        //座標
        for (i = 0; i < numVertex; ++i)
        {

            var vertex_temp = new Vertex();
            line = readerCenter.ReadLine();
            string[] vertex = line.Split(' ');

            //
            float x = float.Parse(vertex[0]);
            // float x = -float.Parse(vertex[0]);
            //float z = float.Parse(vertex[1]);
            //float y = float.Parse(vertex[2]);
            float y = float.Parse(vertex[1]);
            float z = float.Parse(vertex[2]);
            //x = -x;
            // z = -z;
            // y = -y;

            Vector3 original_sverts = new Vector3(x, y, z);
            //Vector3 rotate_sverts = new Vector3();
            //rotate_sverts =
            //    Quaternion.AngleAxis(angle, Vector3.up) * original_sverts;

            originalSvertsCen.Add(original_sverts);

            // vertex_temp.vr = original_sverts;
            //sverts.Add(vertex_temp);
            //vertex_vec_list.Add(rotate_sverts);
        }
        float xMin = originalSvertsCen[0].x;
        float xMax = originalSvertsCen[0].x;
        float zMin = originalSvertsCen[0].y;
        float zMax = originalSvertsCen[0].y;



        foreach (var vert in originalSvertsCen)
        {

            if (xMax < vert.x)
            {
                xMax = vert.x;
            }
            if (xMin > vert.x)
            {
                xMin = vert.x;
            }

            if (zMax < vert.z)
            {
                zMax = vert.z;
            }
            if (zMin > vert.z)
            {
                zMin = vert.z;
            }

        }

            centerX = (xMax + xMin) / 2f;
            centerZ = (zMax + zMin) / 2f;
        bBdiag = Mathf.Sqrt((xMax-xMin)*(xMax - xMin)+(zMax-zMin)*(zMax - zMin));

        //Debug.Log("centerX" + centerX);
        //Debug.Log("centerZ" + centerZ);


        /*----------------------------------------------------------------------------------*/

        //データ数の読み込み
        for (i = 0; i < 2; ++i)
        {
            line = reader.ReadLine();
        }

        string[] numbers = line.Split(' ');

        numVertex = int.Parse(numbers[0]);
        numTriangle = int.Parse(numbers[1]);
        numEdges = int.Parse(numbers[2]);

        /*-------------------------------------plyファイルの形状決めのみ---------------------------------------------*/

        ////sculptfabの方
        ////データ数の読み込み

        //for (i = 0; i < 4; ++i)
        //{
        //    line = reader.ReadLine();
        //}

        //string[] numbers1 = line.Split(' ');

        //numVertex = int.Parse(numbers1[2]);

        //for (i = 0; i < 7; ++i)
        //{
        //    line = reader.ReadLine();
        //}
        //string[] numbers2 = line.Split(' ');
        //numTriangle = int.Parse(numbers2[2]);

        //for (i = 0; i < 2; ++i)
        //{
        //    line = reader.ReadLine();
        //}
        /*----------------------------------------------------------------------------------*/




        List<Vector3> vertex_vec_list = new List<Vector3>();
        //座標
        for (i = 0; i < numVertex; ++i)
        {

            var vertex_temp = new Vertex();
            line = reader.ReadLine();
            string[] vertex = line.Split(' ');

            //
            float x = float.Parse(vertex[0]);
            // float x = -float.Parse(vertex[0]);
            //float z = float.Parse(vertex[1]);
            //float y = float.Parse(vertex[2]);
            float y = float.Parse(vertex[1]);
            float z = float.Parse(vertex[2]);
            //x = -x;
            // z = -z;
            // y = -y;


            //x = x - centerX;
            //z = z - centerZ;



            Vector3 original_sverts = new Vector3(x, y, z);
            Vector3 rotate_sverts = new Vector3();
            rotate_sverts = 
                Quaternion.AngleAxis(angle, Vector3.up) * original_sverts;
          


            vertex_temp.vr = rotate_sverts;
            sverts.Add(vertex_temp);
            vertex_vec_list.Add(rotate_sverts);
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
        mesh.vertices = vertex_vec_list.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();


        ////メッシュデータを読み込んでBBmax, BBminに値を入れる
        //BBmax = mesh.bounds.center+mesh.bounds.extents;
        //BBmin = mesh.bounds.center - mesh.bounds.extents;

        meshCenter = new Vector3();
        meshCenter = mesh.bounds.center;

        return mesh;
    }


    //メッシュデータを読み込んでからYとZを逆転させてから回転を加えて、メッシュの型で返す
    //使ってる方
    public Mesh RotationReturnMeshYZ(string fileName, float angle)
    {

        string path_name = Path.Combine(Application.dataPath, fileName);
        StreamReader reader = new System.IO.StreamReader(path_name, Encoding.UTF8);

        //List<Vector3> sverts = new List<Vector3>();//頂点
        //List<int> triangles = new List<int>();

        string line = "";
        int i;

        //データ数の読み込み
        for (i = 0; i < 2; ++i)
        {
            line = reader.ReadLine();
        }

        string[] numbers = line.Split(' ');

        numVertex = int.Parse(numbers[0]);
        numTriangle = int.Parse(numbers[1]);
        numEdges = int.Parse(numbers[2]);

        List<Vector3> vertex_vec_list = new List<Vector3>();
        //座標
        for (i = 0; i < numVertex; ++i)
        {

            var vertex_temp = new Vertex();
            line = reader.ReadLine();
            string[] vertex = line.Split(' ');

            float x = float.Parse(vertex[0]);
            //ここを変えた
            float z = float.Parse(vertex[1]);
            float y = float.Parse(vertex[2]);

            y = -y;

            Vector3 original_sverts = new Vector3(x, y, z);
            Vector3 rotate_sverts = new Vector3();
            rotate_sverts =
                Quaternion.AngleAxis(angle, Vector3.up) * original_sverts;
            vertex_temp.vr = rotate_sverts;
            sverts.Add(vertex_temp);
            vertex_vec_list.Add(rotate_sverts);
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
        mesh.vertices = vertex_vec_list.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        ////メッシュデータを読み込んでBBmax, BBminに値を入れる
        //BBmax = mesh.bounds.center+mesh.bounds.extents;
        //BBmin = mesh.bounds.center - mesh.bounds.extents;



        return mesh;
    }

    //メッシュデータを読み込んでから回転を加えて、yを4/3倍してメッシュの型で返す
    public Mesh ExRotationReturnMesh(string fileName, float angle)
    {

        string path_name = Path.Combine(Application.dataPath, fileName);
        StreamReader reader = new System.IO.StreamReader(path_name, Encoding.UTF8);

        //List<Vector3> sverts = new List<Vector3>();//頂点
        //List<int> triangles = new List<int>();

        string line = "";
        int i;

        //データ数の読み込み
        for (i = 0; i < 2; ++i)
        {
            line = reader.ReadLine();
        }

        string[] numbers = line.Split(' ');

        numVertex = int.Parse(numbers[0]);
        numTriangle = int.Parse(numbers[1]);

        List<Vector3> vertex_vec_list = new List<Vector3>();
        //座標
        for (i = 0; i < numVertex; ++i)
        {
        var vertex_temp = new Vertex();
            line = reader.ReadLine();
            string[] vertex = line.Split(' ');

            float x = float.Parse(vertex[0]);
            float y = float.Parse(vertex[1])*4/3;
            float z = float.Parse(vertex[2]);
            Vector3 original_sverts = new Vector3(x, y, z);
            Vector3 rotate_sverts = new Vector3();
            rotate_sverts =
                Quaternion.AngleAxis(angle, Vector3.up) * original_sverts;
            vertex_temp.vr = rotate_sverts;
            sverts.Add(vertex_temp);
            vertex_vec_list.Add(rotate_sverts);
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
        mesh.vertices = vertex_vec_list.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        ////メッシュデータを読み込んでBBmax, BBminに値を入れる
        //BBmax = mesh.bounds.center + mesh.bounds.extents;
        //BBmin = mesh.bounds.center - mesh.bounds.extents;



        return mesh;
    }
    

    public void EdgeListConstructOFF()//(string fileName, float angle)
    {
        Face tempF;
        Edge tempE;
        HalfEdge tempHE;

        

        //Edge list construction
        for (int i = 0; i < numTriangle; ++i)
        {       //面を巡っていく
            tempF = sfaces[i];
            tempHE = tempF.fedges;
            for (int j = 0; j < tempF.sidednum; ++j)
            {   //面に属するHalfEdgeをめぐる
                if (tempHE.hedge==null)
                {   //もし稜線がなければ
                    tempE = new Edge();
                    tempE.EdgeConstruct(tempHE, tempHE.HalfEdgeMate());
                    sedges.Add(tempE);    //edgeの本数が未知なのでpush_back
                }
                tempHE = tempHE.next;
            }
        }
    }

    public void FaceListConstructOFF()//(string fileName, float angle)
    {
        
        int vID;
        for (int i=0;i<numTriangle;++i)
        {
            var tempF = new Face();
           
            tempF.sidednum=3;  //何角形なのか？この数字が間違っているとまずいです。

            //n sided polygon
            for (int j = 0; j < tempF.sidednum; ++j)
            {   //面を構成する頂点を一つ一つ巡る
                vID=triangles[i*3+j];              //面を構成する頂点の番号を取得
                var tempHE = new HalfEdge();      //ポインタなのでnewしないと、前の値に影響を残してしまう
                tempHE.HalfEdgeConstruct(tempF, sverts[vID]);  //tempFに属して、vIDの頂点を根元に持つHalfEdgeを構築
            }

            //tempF->id = i;
            sfaces.Add(tempF);	//配列に格納

        }
    }

}

