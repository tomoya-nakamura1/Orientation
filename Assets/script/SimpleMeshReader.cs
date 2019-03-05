using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using System;


//[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

public class SimpleMeshReader : MonoBehaviour
{

    //bool col = false;
    [SerializeField] private Material material;
    [SerializeField] private string fileName;
    [SerializeField] private string fileNameRGB;
    [SerializeField] private float L;//解像度
    [SerializeField] private float d;//距離場閾値
    [SerializeField] private float angle1_deg;//メッシュモデルの回転角度[°]
    //[SerializeField] private float deleteNum;
    public float deleteNum;
    [SerializeField] private float divisionNum;//surveyボクセルを何分割するのか,8か64


    private int starttime;
    private int now;
    private float duration;



    //[SerializeField] private string writeFileName;

    VoxelGrid VoxSpace1 = new VoxelGrid();//ボクセル空間
                                          //  VoxelGrid VoxSpaceRot = new VoxelGrid();//ボクセル空間

    Solid InitSolid1 = new Solid();//初期メッシュ
                                   //Solid InitSolid2 = new Solid();//y方向に4/3倍したメッシュ
                                   //  Solid InitSolidRot = new Solid();




    public GameObject voxelbase;

    GameObject parentVoxel;//= new GameObject();

    //private void OnCollisionStay(Collision collision)
    //{
    //    col = true;
    //}




    // Use this for initialization
    void Start()
    {
        starttime = DateTime.Now.Hour * 60 * 60 * 1000 + DateTime.Now.Minute * 60 * 1000 +
    DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
        //var a = new Vector3(1,2,3);
        //Debug.Log(a.z);

        //メッシュモデルの回転角度[rad]
        //  float angle1 = angle1_deg* Mathf.PI/180.0f;
        //parentVoxel = GameObject.CreatePrimitive(PrimitiveType.Capsule);



        /*------------------------------------------------------*/
        parentVoxel = new GameObject();

        ReadFile();

        //    if (ReadFile())
        //{
        //    Debug.Log("OK");
        //}
        //else
        //{
        //    Debug.Log("Error");
        //}
        //Debug.Log("解像度は次の値" + L);

        //PlyReadFile();
        Debug.Log("deleteNum" + deleteNum);
        Debug.Log("divisionNum" + divisionNum);


        //Debug.Log(L);
        //Debug.Log("表面からの距離が次の値以上のボクセルを除きます");
        //Debug.Log(d);

        //RotNumVoxel();
        //RotInRateVoxel();
        //Threshold();
        /*------------------------------------------------------*/


        now = DateTime.Now.Hour * 60 * 60 * 1000 + DateTime.Now.Minute * 60 * 1000 +
    DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;

        duration = now - starttime;
        duration = duration / 1000f;
        Debug.Log("duration" + duration);



        // Debug.Log(col);

        //Debug.Log(InitSolid1.BBmax);
        //Debug.Log(InitSolid1.BBmin);

        // VoxSpace1.SpaceDefine(mesh1, L);

        //for (int size_x = 0; size_x < VoxSpace1.boxSize[0]; size_x++)
        //{

        //    for (int size_y = 0; size_y < VoxSpace1.boxSize[1]; size_y++)
        //    {

        //        for (int size_z = 0; size_z < VoxSpace1.boxSize[2]; size_z++)
        //        {
        //            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //            cube.transform.position = VoxSpace1.voxelGrid[size_x][size_y][size_z].pos;
        //            //   cube.transform.localScale.x = 2.0f;
        //        }

        //    }

        //}
        // Collision
    }



    void RotNumVoxel()
    {

        int voxelNumber;
        Vector3 maxNumberVoxel = new Vector3();//角度，数，体積
        Vector3 minNumberVoxel = new Vector3();//角度，数，体積
        var writeVoxelNum = new List<int>();

        float unitAngle = 1.0f;

        for (int i = 0; i < 361; i++)
        {


            float angle_each = unitAngle * i;
            //メッシュデータを読み込んでから回転を加えて、メッシュの型で返す



            VoxelGrid VoxSpaceRot = new VoxelGrid();//ボクセル空間
            Solid InitSolidRot = new Solid();

            var meshRot = InitSolidRot.RotationReturnMesh(fileName, angle_each);

            // var meshRot = InitSolid1.RotationReturnMeshYZ(fileName, angle_each);

            //メッシュデータを読み込んでから回転を加えて、yを4/3倍してメッシュの型で返す


            //var filter = GetComponent<MeshFilter>();
            //filter.sharedMesh = mesh1;

            //var renderer = GetComponent<MeshRenderer>();
            //renderer.material = material;

            // Debug.Log(mesh1.bounds.extents);
            //Debug.Log(InitSolid1.BBmax);
            //Debug.Log(InitSolid1.BBmin);

            // var mesh2 = InitSolid2.ExRotationReturnMesh(fileName, angle1_deg);
            VoxSpaceRot.SpaceDefine(meshRot, L, InitSolidRot);//, voxelbase);
                                                              //VoxSpace2.SpaceDefine(mesh2, L);
                                                              // Debug.Log(InitSolid1.numVertex);
                                                              //VoxSpace1.En_sedges_sfaces(InitSolid1);

            InitSolidRot.FaceListConstructOFF();
            InitSolidRot.EdgeListConstructOFF();

            VoxSpaceRot.CrossVert(InitSolidRot);
            VoxSpaceRot.CrossEdge(InitSolidRot);
            VoxSpaceRot.CrossFace(InitSolidRot);

            //VoxSpace1.CheckSurface();

            // VoxSpace1.CrossToFill();
            VoxSpaceRot.InnerVoxelFill();
            VoxSpaceRot.SurfaceVoxel();


            //VoxSpace1.CreateVoxel(voxelbase,parentVoxel);
            //VoxSpace1.CreateVoxel_Fill(voxelbase, parentVoxel);


            //   VoxSpaceRot.VoxelNumberVolumeDebug(angle_each);
            /*------------------------------------------------------------------*/

            voxelNumber = VoxSpaceRot.VoxelNumberMaxMin(angle_each);
            writeVoxelNum.Add(voxelNumber);


            if (maxNumberVoxel.y == 0)
            {
                maxNumberVoxel.y = voxelNumber;
            }
            if (minNumberVoxel.y == 0)
            {
                minNumberVoxel.y = voxelNumber;
            }

            if (maxNumberVoxel.y < voxelNumber)
            {
                maxNumberVoxel.x = angle_each;
                maxNumberVoxel.y = voxelNumber;
            }
            if (minNumberVoxel.y > voxelNumber)
            {
                minNumberVoxel.x = angle_each;

                minNumberVoxel.y = voxelNumber;
            }




            //voxelVolume = LengthXZ * LengthXZ * LengthY;
            /*------------------------------------------------------------------*/


            VoxSpaceRot.IniVoxelGrid();
        }

        var dataname = EditorUtility.SaveFilePanel("Save", Application.dataPath, "", "txt");
        FileInfo fi = new FileInfo(dataname);
        using (StreamWriter sw = fi.AppendText())
        {
            int count = 0;
            foreach (var each in writeVoxelNum)
            {

                sw.WriteLine((count) + "  " + (each));
                count++;
            }


        }

        Debug.Log(minNumberVoxel);
        Debug.Log(maxNumberVoxel);

    }

    void RotInRateVoxel()
    {

        float voxelInNumber;
        Vector3 maxRateVoxel = new Vector3();//角度，平均surveyボクセル数,平均surveyボクセル率
        Vector3 minRateVoxel = new Vector3();//角度，平均surveyボクセル数,平均surveyボクセル率

        float unitAngle = 1.0f;
        var writeScore = new List<float>();


        for (int i = 0; i < 90; i++)
        {


            float angle_each = unitAngle * i;
            //メッシュデータを読み込んでから回転を加えて、メッシュの型で返す



            VoxelGrid VoxSpaceRot = new VoxelGrid();//ボクセル空間
            Solid InitSolidRot = new Solid();

            var meshRot = InitSolidRot.RotationReturnMesh(fileName, angle_each);

            // var meshRot = InitSolid1.RotationReturnMeshYZ(fileName, angle_each);

            //メッシュデータを読み込んでから回転を加えて、yを4/3倍してメッシュの型で返す


            //var filter = GetComponent<MeshFilter>();
            //filter.sharedMesh = mesh1;

            //var renderer = GetComponent<MeshRenderer>();
            //renderer.material = material;

            // Debug.Log(mesh1.bounds.extents);
            //Debug.Log(InitSolid1.BBmax);
            //Debug.Log(InitSolid1.BBmin);

            // var mesh2 = InitSolid2.ExRotationReturnMesh(fileName, angle1_deg);
            VoxSpaceRot.SpaceDefine(meshRot, L, InitSolidRot);//, voxelbase);
                                                              //VoxSpace2.SpaceDefine(mesh2, L);
                                                              // Debug.Log(InitSolid1.numVertex);
                                                              //VoxSpace1.En_sedges_sfaces(InitSolid1);

            InitSolidRot.FaceListConstructOFF();
            InitSolidRot.EdgeListConstructOFF();

            VoxSpaceRot.CrossVert(InitSolidRot);
            //VoxSpaceRot.CrossEdge(InitSolidRot);
            VoxSpaceRot.CrossFace(InitSolidRot);

            //VoxSpace1.CheckSurface();

            // VoxSpace1.CrossToFill();

            //VoxSpaceRot.InnerVoxelFill();
            //VoxSpaceRot.SurfaceVoxel();




            //VoxSpace1.CreateVoxel(voxelbase,parentVoxel);
            //VoxSpace1.CreateVoxel_Fill(voxelbase, parentVoxel);


            //   VoxSpaceRot.VoxelNumberVolumeDebug(angle_each);
            /*------------------------------------------------------------------*/

            //  voxelNumber = VoxSpaceRot.VoxelNumberMaxMin(angle_each);

            voxelInNumber = VoxSpaceRot.OffInternalRate(meshRot, L, InitSolidRot, deleteNum, divisionNum);
            writeScore.Add(voxelInNumber);

            if (maxRateVoxel.y == 0)
            {
                maxRateVoxel.y = voxelInNumber;
            }
            if (minRateVoxel.y == 0)
            {
                minRateVoxel.y = voxelInNumber;
            }

            if (maxRateVoxel.y < voxelInNumber)
            {
                maxRateVoxel.x = angle_each;
                maxRateVoxel.y = voxelInNumber;
            }
            if (minRateVoxel.y > voxelInNumber)
            {
                minRateVoxel.x = angle_each;

                minRateVoxel.y = voxelInNumber;
            }


            //voxelVolume = LengthXZ * LengthXZ * LengthY;
            /*------------------------------------------------------------------*/


            VoxSpaceRot.IniVoxelGrid();
        }
        minRateVoxel.z = minRateVoxel.y / 8f;
        maxRateVoxel.z = maxRateVoxel.y / 8f;

        var dataname = EditorUtility.SaveFilePanel("Save", Application.dataPath, "", "txt");
        FileInfo fi = new FileInfo(dataname);
        using (StreamWriter sw = fi.AppendText())
        {
            int count = 0;
            foreach (var each in writeScore)
            {

                sw.WriteLine((count) + "  " + (each));
                count++;
            }


        }

        Debug.Log("min"+minRateVoxel.x+" ,"+ minRateVoxel.y+" ,"+ minRateVoxel.z);
        Debug.Log("max"+ maxRateVoxel.x+" ,"+ maxRateVoxel.y+" ,"+ maxRateVoxel.z);
        //Debug.Log(maxRateVoxel);

    }



    //ファイルの読み込み//Offデータ
    void ReadFile()
   // bool ReadFile()
    {

        //try
        //{
            //メッシュデータを読み込んでから回転を加えて、メッシュの型で返す
            var mesh1 = InitSolid1.RotationReturnMesh(fileName, angle1_deg);

            // var mesh1 = InitSolid1.RotationReturnMeshYZ(fileName, angle1_deg);

            //メッシュデータを読み込んでから回転を加えて、yを4/3倍してメッシュの型で返す


            var filter = GetComponent<MeshFilter>();
            filter.sharedMesh = mesh1;

            var renderer = GetComponent<MeshRenderer>();
            renderer.material = material;

            // Debug.Log(mesh1.bounds.extents);
            //Debug.Log(InitSolid1.BBmax);
            //Debug.Log(InitSolid1.BBmin);

            // var mesh2 = InitSolid2.ExRotationReturnMesh(fileName, angle1_deg);
            VoxSpace1.SpaceDefine(mesh1, L, InitSolid1);//, voxelbase);
                                                        //VoxSpace1.SpaceDefine(mesh1, L);//, voxelbase);



            //VoxSpace2.SpaceDefine(mesh2, L);
            // Debug.Log(InitSolid1.numVertex);
            //VoxSpace1.En_sedges_sfaces(InitSolid1);

            InitSolid1.FaceListConstructOFF();
            InitSolid1.EdgeListConstructOFF();

            VoxSpace1.CrossVert(InitSolid1);

        //なぜかうまくいかない，馬
        //VoxSpace1.CrossEdge(InitSolid1);
        VoxSpace1.CrossFace(InitSolid1);
        //VoxSpace1.CrossFaceDistance(InitSolid1);


        //VoxSpace1.AllCrossFace(InitSolid1);

        //VoxSpace1.CheckSurface();

        // VoxSpace1.CrossToFill();


        VoxSpace1.InnerVoxelFill();
        //VoxSpace1.SurfaceVoxel();

        //VoxSpace1.CreateVoxel(voxelbase,parentVoxel);
        //VoxSpace1.CreateVoxel_Fill(voxelbase, parentVoxel);

        //VoxSpace1.WriteVoxelTxt();

        //foreach (var a in VoxSpace1.voxelGrid[20][20][20].crossedFacesList)
        //    {
        //        Debug.Log(a);
        //    }




        Debug.Log(VoxSpace1.OffInternalRate(mesh1, L, InitSolid1, deleteNum, divisionNum));
        //VoxSpace1.CreateVoxelSurf(voxelbase, parentVoxel);//使えなかった

        // VoxSpace1.CreateVoxelSurfNew(voxelbase, parentVoxel);
        // VoxSpace1.CreateVoxel(voxelbase, parentVoxel);//crossしているものだけ描画
        VoxSpace1.CreateVoxel_Fill(voxelbase, parentVoxel);
        //VoxSpace1.CreateSurveyVoxel_Fill(voxelbase, parentVoxel);

        VoxSpace1.VoxelNumberVolumeDebug(angle1_deg);
        //VoxSpace1.VoxelModelWrite();


        //}
        //catch (System.Exception e)
        //{

        //    Debug.Log(e.Message);
        //    return false;

        //}

        //return true;
    }


    //ファイルの読み込み//Plyデータ
    // bool PlyReadFile()
    void PlyReadFile()
    {

        //try
        //{
            //メッシュデータを読み込んでから回転を加えて、メッシュの型で返す
            var mesh1 = InitSolid1.PlyRotationReturnMesh(fileName, angle1_deg);

            // var mesh1 = InitSolid1.RotationReturnMeshYZ(fileName, angle1_deg);

            //メッシュデータを読み込んでから回転を加えて、yを4/3倍してメッシュの型で返す


            var filter = GetComponent<MeshFilter>();
            filter.sharedMesh = mesh1;

            var renderer = GetComponent<MeshRenderer>();
            renderer.material = material;

            // Debug.Log(mesh1.bounds.extents);
            //Debug.Log(InitSolid1.BBmax);
            //Debug.Log(InitSolid1.BBmin);

            // var mesh2 = InitSolid2.ExRotationReturnMesh(fileName, angle1_deg);
            VoxSpace1.PlySpaceDefine(mesh1, L, InitSolid1);//, voxelbase);
            //VoxSpace1.SpaceDefine(mesh1, L, InitSolid1);//, voxelbase);


            //VoxSpace2.SpaceDefine(mesh2, L);
            // Debug.Log(InitSolid1.numVertex);
            //VoxSpace1.En_sedges_sfaces(InitSolid1);

            InitSolid1.FaceListConstructOFF();
            InitSolid1.EdgeListConstructOFF();

            VoxSpace1.PlyCrossVert(InitSolid1);

            //VoxSpace1.PlyCrossEdge(InitSolid1);
            VoxSpace1.PlyCrossFace(InitSolid1);

            //VoxSpace1.CheckSurface();

            // VoxSpace1.CrossToFill();

            VoxSpace1.InnerVoxelFill();
            // VoxSpace1.SurfaceVoxel();

            // VoxSpace1.VoxelcoloringAve(InitSolid1);


            VoxSpace1.VoxelcoloringLin(InitSolid1);

        VoxSpace1.GetVoxelColor_lumi_mode(InitSolid1);
        //VoxSpace1.GetVoxelColor_hue_mode(InitSolid1);

        VoxSpace1.LegoRGB_to_Lab(fileNameRGB);
        VoxSpace1.RealColor_to_LegoColor_lab();


        //  Debug.Log(VoxSpace1.PlyInternalRate(mesh1, L, InitSolid1, deleteNum, divisionNum));

        VoxSpace1.PlyCreateVoxel(voxelbase, parentVoxel);



            //VoxSpace1.CreateVoxel_Fill(voxelbase, parentVoxel);
            // VoxSpace1.VoxelNumberVolumeDebug(angle1_deg);

            // VoxSpace1.WriteVoxelTxt();

        //}
        //catch (System.Exception e)
        //{

        //    Debug.Log(e.Message);
        //    return false;

        //}

        //return true;
    }

    public void Threshold()
    {
            starttime = DateTime.Now.Hour * 60 * 60 * 1000 + DateTime.Now.Minute * 60 * 1000 +
    DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
        List<float> volumeList = new List<float>();
        for(int i = 0; i < 8; i++)
        {
            deleteNum = i;

     


                float angle_each = 0;
                //メッシュデータを読み込んでから回転を加えて、メッシュの型で返す



                VoxelGrid VoxSpaceRot = new VoxelGrid();//ボクセル空間
                Solid InitSolidRot = new Solid();

                var meshRot = InitSolidRot.RotationReturnMesh(fileName, angle_each);

                // var meshRot = InitSolid1.RotationReturnMeshYZ(fileName, angle_each);

                //メッシュデータを読み込んでから回転を加えて、yを4/3倍してメッシュの型で返す


                //var filter = GetComponent<MeshFilter>();
                //filter.sharedMesh = mesh1;

                //var renderer = GetComponent<MeshRenderer>();
                //renderer.material = material;

                // Debug.Log(mesh1.bounds.extents);
                //Debug.Log(InitSolid1.BBmax);
                //Debug.Log(InitSolid1.BBmin);

                // var mesh2 = InitSolid2.ExRotationReturnMesh(fileName, angle1_deg);
                VoxSpaceRot.SpaceDefine(meshRot, L, InitSolidRot);//, voxelbase);
                                                                  //VoxSpace2.SpaceDefine(mesh2, L);
                                                                  // Debug.Log(InitSolid1.numVertex);
                                                                  //VoxSpace1.En_sedges_sfaces(InitSolid1);

                InitSolidRot.FaceListConstructOFF();
                InitSolidRot.EdgeListConstructOFF();

                VoxSpaceRot.CrossVert(InitSolidRot);
                //VoxSpaceRot.CrossEdge(InitSolidRot);
                VoxSpaceRot.CrossFace(InitSolidRot);

            //VoxSpace1.CheckSurface();

            // VoxSpace1.CrossToFill();

            VoxSpaceRot.InnerVoxelFill();
            //VoxSpaceRot.SurfaceVoxel();




            //VoxSpace1.CreateVoxel(voxelbase,parentVoxel);
            //VoxSpace1.CreateVoxel_Fill(voxelbase, parentVoxel);


            //   VoxSpaceRot.VoxelNumberVolumeDebug(angle_each);
            /*------------------------------------------------------------------*/

            //  voxelNumber = VoxSpaceRot.VoxelNumberMaxMin(angle_each);

            VoxSpaceRot.OffInternalRate(meshRot, L, InitSolidRot, deleteNum, divisionNum);
            float eachV = VoxSpaceRot.VoxelVolumeReturn(angle_each);

            volumeList.Add(eachV);


            Debug.Log("deleteNum" + deleteNum);


            //voxelVolume = LengthXZ * LengthXZ * LengthY;
            /*------------------------------------------------------------------*/


        }
          

            var dataname = EditorUtility.SaveFilePanel("Save", Application.dataPath, "", "txt");
            FileInfo fi = new FileInfo(dataname);
            using (StreamWriter sw = fi.AppendText())
            {
                int count = 0;
                foreach (var each in volumeList)
                {

                    sw.WriteLine((count) + "  " + (each));
                    count++;
                }


            }
           


        
            now = DateTime.Now.Hour * 60 * 60 * 1000 + DateTime.Now.Minute * 60 * 1000 +
        DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;

            duration = now - starttime;
            duration = duration / 1000f;
            Debug.Log("duration" + duration);

    }



}