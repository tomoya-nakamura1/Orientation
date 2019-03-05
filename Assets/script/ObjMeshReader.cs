using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using System;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

public class ObjMeshReader : MonoBehaviour
{

    [SerializeField] private Material material;
    [SerializeField] private string fileName;
    [SerializeField] private float L;//解像度
    [SerializeField] private float angle1_deg;//メッシュモデルの回転角度[°]
    [SerializeField] private LayerMask layerMaskMesh;
    [SerializeField] private LayerMask layerMaskVoxel;


    VoxelGrid VoxSpace1 = new VoxelGrid();//ボクセル空間
                                     

    Solid InitSolid1 = new Solid();//初期メッシュ
                                 




    public GameObject voxelbase;

    GameObject parentVoxel;
    GameObject parentCamera;




    // Use this for initialization
    void Start()
    {
        /*------------------------------------------------------*/
        parentVoxel = new GameObject();

        ReadFile();
        //DefVoxelSpace();
        parentCamera = GameObject.Find("parentCamera");

        //VoxelVanish();

        //parentCamera.transform.Rotate(36, 0, 0);


        //VoxelVanish();
        float lengthXZ;
        lengthXZ = VoxSpace1.LengthXZ;

        for (int j = 0; j < 36; j++)
        {
            parentCamera.transform.Rotate(0, 10, 0);
            for (int i = 0; i < 36; i++)
            {
                parentCamera.transform.Rotate(10, 0, 0);

                //VoxelVanishOutMesh(lengthXZ);
            }
        }

        /*------------------------------------------------------*/

    }


    //ファイルの読み込み//Offデータ
    void DefVoxelSpace()
    {

       
            //メッシュデータを読み込んでから回転を加えて、メッシュの型で返す
            //var mesh1 = InitSolid1.RotationReturnObj(fileName, angle1_deg);
            var mesh1 = InitSolid1.RotationReturnMesh(fileName, angle1_deg);

        var filter = GetComponent<MeshFilter>();
        filter.sharedMesh = mesh1;

        var renderer = GetComponent<MeshRenderer>();
        renderer.material = material;

        VoxSpace1.SpaceDefine(mesh1, L, InitSolid1);
        VoxSpace1.CrossVert(InitSolid1);
        VoxSpace1.CrossEdge(InitSolid1);
        VoxSpace1.CrossFace(InitSolid1);

        VoxSpace1.CreateVoxel(voxelbase, parentVoxel);//crossしているものだけ描画
        //VoxSpace1.CreateVoxel_Fill(voxelbase, parentVoxel);

        VoxSpace1.VoxelNumberVolumeDebug(angle1_deg);

       
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
        VoxSpace1.CrossEdge(InitSolid1);
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




        //Debug.Log(VoxSpace1.OffInternalRate(mesh1, L, InitSolid1, deleteNum, divisionNum));
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

    /*  http://megumisoft.hatenablog.com/entry/2015/08/13/172136 */



    int[] Size = new int[3]; //xyz方向のそれぞれのボクセル数

    //public List<List<List<Voxel>>>  GetVoxel(int x, int y, int z)//範囲外のボクセルが来たらnullを返す
    //{

    //    if (/*!originVox.SequenceEqual(new int[] { x, y, z })&&*/(x < 0 || y < 0 || z < 0 || x >= Size[0] || y >= Size[1] || z >= Size[2]))
    //    {
    //        return null;
    //    }
    //    else
    //    {
    //        List<List<List<Voxel>>> voxelGrid = new List<List<List<Voxel>>>();
    //        return 
    //    }
    //}

    

    public void VoxelVanishOutMesh(float lengthXZ)
    {
        var width = 200;
        var height = 100;
        var positionMap = new Vector3[width, height];
        Ray screenRay;
        RaycastHit hit;
        float distance;
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                screenRay = Camera.main.ViewportPointToRay(new Vector3((float)i / width, (float)j / height, 0));
                if (Physics.Raycast(screenRay, out hit, 1000f,layerMaskMesh.value))
                {
                    distance = hit.distance;
                    //distance = hit.distance-lengthXZ*2;
                    if (Physics.Raycast(screenRay, out hit, distance, layerMaskVoxel.value))
                    {
                        //// 衝突したオブジェクトを消す
                        DestroyImmediate(hit.collider.gameObject);
                    }

                        
                }
                else
                {
                    if (Physics.Raycast(screenRay, out hit, 1000f, layerMaskVoxel.value))
                    {
                        //// 衝突したオブジェクトを消す
                        DestroyImmediate(hit.collider.gameObject);
                    }
                }
            }
        }
    }

    public void GetColor()
    {
        var width = 200;
        var height = 100;
        var positionMap = new Vector3[width, height];
        Ray screenRay;
        RaycastHit hit;
        float distance;
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                screenRay = Camera.main.ViewportPointToRay(new Vector3((float)i / width, (float)j / height, 0));
                if (Physics.Raycast(screenRay, out hit, 1000f, layerMaskMesh.value))
                {
                    distance = hit.distance;

                    if (Physics.Raycast(screenRay, out hit, distance, layerMaskVoxel.value))
                    {
                        //// 衝突したオブジェクトを消す
                        DestroyImmediate(hit.collider.gameObject);
                    }


                }
                else
                {
                    if (Physics.Raycast(screenRay, out hit, 1000f, layerMaskVoxel.value))
                    {
                        //// 衝突したオブジェクトを消す
                        DestroyImmediate(hit.collider.gameObject);
                    }
                }
            }
        }
    }
    public void RayMesh()
    {
        var width = 200;
        var height = 100;
        var positionMap = new Vector3[width, height];
        Ray screenRay;
        RaycastHit hit;
        float distance;
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                screenRay = Camera.main.ViewportPointToRay(new Vector3((float)i / width, (float)j / height, 0));
                if (Physics.Raycast(screenRay, out hit, 1000f, layerMaskMesh.value))
                {
                    distance = hit.distance;
                   
                    if (Physics.Raycast(screenRay, out hit, distance, layerMaskVoxel.value))
                    {
                        //// 衝突したオブジェクトを消す
                        DestroyImmediate(hit.collider.gameObject);
                    }


                }
                else
                {
                    if (Physics.Raycast(screenRay, out hit, 1000f, layerMaskVoxel.value))
                    {
                        //// 衝突したオブジェクトを消す
                        DestroyImmediate(hit.collider.gameObject);
                    }
                }
            }
        }
    }

    public int[] Raycast(Vector3 origin, Vector3 direction, float radius)
    {
        // From "A Fast Voxel Traversal Algorithm for Ray Tracing"
        // by John Amanatides and Andrew Woo, 1987
        // <http://www.cse.yorku.ca/~amana/research/grid.pdf>
        // <http://citeseer.ist.psu.edu/viewdoc/summary?doi=10.1.1.42.3443>
        // Extensions to the described algorithm:
        //   • Imposed a distance limit.
        //   • The face passed through to reach the current cube is provided to
        //     the callback.

        // The foundation of this algorithm is a parameterized representation of
        // the provided ray,
        //                    origin + t * direction,
        // except that t is not actually stored; rather, at any given point in the
        // traversal, we keep track of the *greater* t values which we would have
        // if we took a step sufficient to cross a cube boundary along that axis
        // (i.e. change the integer part of the coordinate) in the variables
        // tMaxX, tMaxY, and tMaxZ.

        // Real space ray to voxel space ray.
        //var oriLength = direction.magnitude;
        //origin = new Vector3((origin.x - Min.x) / xStep, (origin.y - Min.y) / yStep, (origin.z - Min.z) / zStep);
        //direction = new Vector3((direction.x) / xStep, (direction.y) / yStep, (direction.z) / zStep);
        //radius = direction.magnitude / oriLength;

        // var originVox = new int[] { Mathf.FloorToInt(origin[0]), Mathf.FloorToInt(origin[1]), Mathf.FloorToInt(origin[2])};

        var normal = Vector3.zero;
        // Cube containing origin point.
        var x = Mathf.FloorToInt(origin[0]);
        var y = Mathf.FloorToInt(origin[1]);
        var z = Mathf.FloorToInt(origin[2]);
        // Break out direction vector.
        var dx = direction[0];
        var dy = direction[1];
        var dz = direction[2];
        // Direction to increment x,y,z when stepping.
        var stepX = Signum(dx);
        var stepY = Signum(dy);
        var stepZ = Signum(dz);
        // See description above. The initial values depend on the fractional
        // part of the origin.
        var tMaxX = Intbound(origin[0], dx);
        var tMaxY = Intbound(origin[1], dy);
        var tMaxZ = Intbound(origin[2], dz);
        // The change in t when taking a step (always positive).
        var tDeltaX = stepX / dx;
        var tDeltaY = stepY / dy;
        var tDeltaZ = stepZ / dz;
        // Buffer for reporting faces to the callback.
        // var face = vec3.create();

        // Avoids an infinite loop.
        if (dx == 0 && dy == 0 && dz == 0)
            Debug.Log("Raycast in zero direction!");

        // Rescale from units of 1 cube-edge to units of 'direction' so we can
        // compare with 't'.
        radius /= Mathf.Sqrt(dx * dx + dy * dy + dz * dz);

        while (/* ray has not gone past bounds of world */
               (stepX > 0 ? x < Size[0] : x >= 0) &&
               (stepY > 0 ? y < Size[1] : y >= 0) &&
               (stepZ > 0 ? z < Size[2] : z >= 0))
        {

            // Invoke the callback, unless we are not *yet* within the bounds of the
            // world.
            if (/*!originVox.SequenceEqual(new int[] { x, y, z })&&*/!(x < 0 || y < 0 || z < 0 || x >= Size[0] || y >= Size[1] || z >= Size[2]))
            {
                //var voxel = GetVoxel(x, y, z);
                //if (voxel != null)
                //{
                //    return new int[] { x, y, z };
                //}

                return new int[] { x, y, z };

            }

            // tMaxX stores the t-value at which we cross a cube boundary along the
            // X axis, and similarly for Y and Z. Therefore, choosing the least tMax
            // chooses the closest cube boundary. Only the first case of the four
            // has been commented in detail.
            if (tMaxX < tMaxY)
            {
                if (tMaxX < tMaxZ)
                {
                    if (tMaxX > radius) break;
                    // Update which cube we are now in.
                    x += stepX;
                    // Adjust tMaxX to the next X-oriented boundary crossing.
                    tMaxX += tDeltaX;
                    // Record the normal vector of the cube face we entered.
                    normal = Vector3.left * stepX;
                    // face[0] = -stepX;
                    // face[1] = 0;
                    // face[2] = 0;
                }
                else
                {
                    if (tMaxZ > radius) break;
                    z += stepZ;
                    tMaxZ += tDeltaZ;
                    normal = Vector3.back * stepZ;

                    // face[0] = 0;
                    // face[1] = 0;
                    // face[2] = -stepZ;
                }
            }
            else
            {
                if (tMaxY < tMaxZ)
                {
                    if (tMaxY > radius) break;
                    y += stepY;
                    tMaxY += tDeltaY;
                    normal = Vector3.down * stepY;

                    // face[0] = 0;
                    // face[1] = -stepY;
                    // face[2] = 0;
                }
                else
                {
                    // Identical to the second case, repeated for simplicity in
                    // the conditionals.
                    if (tMaxZ > radius) break;
                    z += stepZ;
                    tMaxZ += tDeltaZ;
                    normal = Vector3.back * stepZ;

                    // face[0] = 0;
                    // face[1] = 0;
                    // face[2] = -stepZ;
                }
            }
        }
        return null;
    }
    static float Intbound(float s, float ds)
    {
        // Find the smallest positive t such that s+t*ds is an integer.
        if (ds < 0)
        {
            return Intbound(-s, -ds);
        }
        else
        {
            s = Mod(s, 1);
            // problem is now s+t*ds = 1
            return (1 - s) / ds;
        }
    }
    static int Signum(float x)
    {
        return x > 0 ? 1 : x < 0 ? -1 : 0;
    }
    static float Mod(float value, float modulus)
    {
        return (value % modulus + modulus) % modulus;
    }



}