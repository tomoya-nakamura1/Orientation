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

public class TexMeshReader : MonoBehaviour
{

    //bool col = false;
    [SerializeField] private Material material;
    [SerializeField] private string fileName;
    [SerializeField] private string fileNameRGB;
    [SerializeField] private float L;//解像度
    [SerializeField] private float d;//距離場閾値
    [SerializeField] private float angle1_deg;//メッシュモデルの回転角度[°]
    [SerializeField] private float deleteNum;
    [SerializeField] private float divisionNum;//surveyボクセルを何分割するのか,8か64
    [SerializeField] private LayerMask layerMaskMesh;
    [SerializeField] private LayerMask layerMaskVoxel;
    [SerializeField] private int wantToUSE;


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
    public GameObject cubeCollider;
    GameObject parentCamera;
    GameObject mainCamera;

    List<Vector3> dodec = new List<Vector3>(); //dodecahedron



    GameObject parentVoxel;//= new GameObject();

    //private void OnCollisionStay(Collision collision)
    //{
    //    col = true;
    //}




    // Use this for initialization
    //IEnumerator Start()
    void Awake()
    {
        //yield return null;

        parentCamera = GameObject.Find("parentCamera");
        mainCamera = GameObject.Find("Main Camera");


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

        /*------------------------------------------------------*/


        now = DateTime.Now.Hour * 60 * 60 * 1000 + DateTime.Now.Minute * 60 * 1000 +
    DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;

        duration = now - starttime;
        duration = duration / 1000f;
        Debug.Log("duration" + duration);

        //for (int j = 0; j < 36; j++)
        //{
        //    parentCamera.transform.Rotate(0, 10, 0);
        //    for (int i = 0; i < 36; i++)
        //    {
        //        parentCamera.transform.Rotate(10, 0, 0);
        //        RayColor();
        //        //VoxelVanishOutMesh(lengthXZ);
        //    }
        //}


        TexTotalColor(InitSolid1);

        //VoxSpace1.TexVoxelColor_lumi_mode(InitSolid1);
        VoxSpace1.TexVoxelColor_hue_mode(InitSolid1);

        //VoxSpace1.TexLegoRGB_to_Lab(fileNameRGB, wantToUSE);


        //VoxSpace1.TexColor_to_LegoColor_lab();

        VoxSpace1.TexCreateVoxel(voxelbase, parentVoxel);


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

    public void TexTotalColor(Solid solid)
    {
        //Texture2D texture;
        //texture = CaptchaScreen(1, 1);
        ////texture = Resources.Load("1_1") as Texture2D;
        //GetColor(texture, VoxSpace1);
        /*--------------------------------------------------------------------------------------*/


        //for (int i = 0; i < 6; i++)
        //{
        //    parentCamera.transform.Rotate(0, 60, 0);
        //    for (int j = 0; j < 6; j++)
        //    {
        //        parentCamera.transform.Rotate(60, 0, 0);
        //        //RayColor();
        //        Texture2D texture;
        //        texture = CaptchaScreen(i, j);

        //        GetColor(texture, VoxSpace1);
        //        //GetColor(screenShot,VoxSpace1);
        //        //VoxSpace1.TexColorToVoxel();
        //        //VoxSpace1.TexCreateVoxel(voxelbase, parentVoxel);

        //        //VoxelVanishOutMesh(lengthXZ);
        //    }
        //}

        /*--------------------------------------------------------------------------------------*/
 Vector3 meshCenter = new Vector3();
        meshCenter = solid.meshCenter;
        Vector3 firstPos = new Vector3();
        firstPos = mainCamera.transform.position;
        //float magnitude = (mainCamera.transform.position - meshCenter).magnitude;
        float magnitude = (mainCamera.transform.position ).magnitude;
        //float magnitude = 15.6f;
        float g = (1.0f + Mathf.Sqrt(5)) / 2;

        mainCamera.transform.LookAt(parentCamera.transform);


        dodec.Add(new Vector3(1.0f, 1.0f, 1.0f));//1,1,1
        dodec.Add(new Vector3(1.0f, -1.0f, 1.0f));
        dodec.Add(new Vector3(1.0f, -1.0f, -1.0f));
        dodec.Add(new Vector3(-1.0f, -1.0f, 1.0f));
        dodec.Add(new Vector3(-1.0f, -1.0f, -1.0f));
        dodec.Add(new Vector3(g, 0.0f, 1.0f / g));//最初g
        dodec.Add(new Vector3(g, 0.0f, -1.0f / g));
        dodec.Add(new Vector3(-g, 0.0f, -1.0f / g));
        dodec.Add(new Vector3(0.0f, 1.0f / g, -g));
        dodec.Add(new Vector3(0.0f, -1.0f / g, -g));
        dodec.Add(new Vector3(1.0f / g, g, 0f));//最後0
        dodec.Add(new Vector3(-1.0f / g, -g, 0f));
        dodec.Add(new Vector3(1.0f / g, -g, 0f));
        dodec.Add(new Vector3(-1.0f / g, g, 0f));
        dodec.Add(new Vector3(1.0f, 1.0f, -1.0f));
        dodec.Add(new Vector3(-g, 0.0f, 1.0f / g));
        dodec.Add(new Vector3(0.0f, -1.0f / g, g));
        dodec.Add(new Vector3(0.0f, 1.0f / g, g));//最初0
        dodec.Add(new Vector3(-1.0f, 1.0f, 1.0f));
        dodec.Add(new Vector3(-1.0f, 1.0f, -1.0f));

        int count=0;

        foreach (var position in dodec)
        {
            //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Vector3 cameraPos = new Vector3();

            cameraPos.x = magnitude/position.magnitude * position.x + meshCenter.x;
            cameraPos.y = magnitude/position.magnitude * position.y + meshCenter.y;
            cameraPos.z = magnitude/position.magnitude * position.z + meshCenter.z;

            //cameraPos.x = magnitude * position.x+meshCenter.x;
            //cameraPos.y = magnitude * position.y+meshCenter.y;
            //cameraPos.z = magnitude * position.z+meshCenter.z;

            //sphere.transform.position = cameraPos;
            mainCamera.transform.position = cameraPos;
            mainCamera.transform.LookAt(parentCamera.transform);

            count++;
            int i = count;
            int j = count;
            Texture2D texture;
            texture = CaptchaScreen(i, j);

            GetColor(texture, VoxSpace1);
        }
        mainCamera.transform.position = firstPos;
        mainCamera.transform.LookAt(parentCamera.transform);

        /*--------------------------------------------------------------------------------------*/
    }


    //private void Update()
    //{

    //    //メインカメラ上のマウスカーソルのある位置からRayを飛ばす
    //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //    RaycastHit hit;



    //    //Rayの長さ
    //    float maxDistance = 1000;

    //    if (Physics.Raycast(ray, out hit, maxDistance))
    //    {
    //        hit.collider.GetComponent<MeshRenderer>().material.color = Color.red;

    //        //Rayが当たるオブジェクトがあった場合はそのオブジェクト名をログに表示
    //        Debug.Log(hit.collider.gameObject.name);
    //    }

    //    //Rayを画面に表示
    //    Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green, 5, false);

    //}

    public Camera ArCam;
    GameObject mesh;


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

        //string fileName = "cap_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png";
        string fileName = num.ToString() + "_" + num2.ToString() + ".png";

        //File.WriteAllBytes(Application.persistentDataPath + "/" + fileName, bytes);
        File.WriteAllBytes(Application.dataPath + "/" + "OutImage" + "/" + fileName, bytes);

        return screenShot;

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


    public void RayColor()
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


                //if (Physics.Raycast(screenRay, out hit, 1000f))
                if (Physics.Raycast(screenRay, out hit, 1000f, layerMaskVoxel.value))
                {

                    hit.collider.GetComponent<MeshRenderer>().material.color = Color.red;

                    //distance = hit.distance;

                    //if (Physics.Raycast(screenRay, out hit, distance, layerMaskVoxel.value))
                    //{
                    //    hit.collider.GetComponent<MeshRenderer>().material.color = Color.red;
                    //    ////// 衝突したオブジェクトを消す
                    //    //DestroyImmediate(hit.collider.gameObject);
                    //}


                }
                //else
                //{
                //    if (Physics.Raycast(screenRay, out hit, 1000f, layerMaskVoxel.value))
                //    {
                //        //// 衝突したオブジェクトを消す
                //        DestroyImmediate(hit.collider.gameObject);
                //    }
                //}
            }
        }
    }

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
                if (Physics.Raycast(screenRay, out hit, 1000f, layerMaskMesh.value))
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

    public void GetColor(Texture2D texture,VoxelGrid VoxSpace)
    {
        //var width = 200f;
        //var height = 100f;
       float width = texture.width;
       float height = texture.height;
        //var positionMap = new Vector3[width, height];
        Ray screenRay;
        RaycastHit hit;
        RaycastHit hitV;

        Color[] colors = texture.GetPixels(
                (int)Mathf.Floor(0),
                (int)Mathf.Floor(0),
                (int)Mathf.Floor(texture.width),
                (int)Mathf.Floor(texture.height)
            );

        //Debug.Log(colors);
        //Debug.Log(colors[100000]);
        //Debug.Log(colors[180000]);
        //Debug.Log(colors[150000]);
        //Debug.Log(colors[100000]);
        //Debug.Log(colors[50000]);
        //Debug.Log(colors[60000]);

        //for (int k=0;k<colors.Length;k++)
        //{
        //    if (colors[k].r <0.7)
        //    {
        //        Debug.Log(k);
        //        break;

        //    }
        //}



        float distance;
        //Voxel tempV = VoxSpace.voxelGrid[0][0][0];
        VoxelGrid tempV = VoxSpace;
        float diagonal = Mathf.Sqrt(2f)*tempV.LengthXZ;
        diagonal = Mathf.Sqrt(diagonal*diagonal+tempV.LengthY* tempV.LengthY);

        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                screenRay = Camera.main.ViewportPointToRay(new Vector3((float)i / width, (float)j / height, 0));
                if (Physics.Raycast(screenRay, out hit, 1000f, layerMaskMesh.value))
                {
                    //distance = hit.distance;
                    //int x = Mathf.FloorToInt((i / width )* texture.width);
                    //int y = Mathf.FloorToInt((j / height )* texture.height);

                    float x = ((i / width) * texture.width);
                    float y = ((j / height) * texture.height);
                    //Debug.Log(x + " , "+y);
                    //Debug.Log(texture.width);
                    //Debug.Log(texture.height);
                    //int x = Mathf.FloorToInt(i / width * 280);
                    //int y = Mathf.FloorToInt(j / height * 240);

                    //Color colorTemp;
                    Color colorTemp = new Color();

                    //System.String assetPath = UnityEditor.AssetDatabase.GetAssetPath(texture); // インポート元のパスを取得
                    //UnityEditor.TextureImporter importer = UnityEditor.AssetImporter.GetAtPath(assetPath) as UnityEditor.TextureImporter; // パスからインポート設定にアクセス
                    //if (importer.isReadable == false) importer.isReadable = true; // 設定の確認と書き換え


                    colorTemp = texture.GetPixel((int)x,(int) y);


                    //colorTemp = colors[26375];
                    int number;
                    //y = -y +texture.height;
                    number = Mathf.FloorToInt(x * y);
                    //colorTemp = colors[number];
                    //colorTemp = colors[number];
                    //Debug.Log(colorTemp);
                    //colorTemp =new Color(200,200,0)  ;
                    //colorTemp = texture.GetPixelBilinear(1-i/width,j/height);

                    float distanceM = hit.distance;
                    float rayLength = distanceM + diagonal;

                    if (Physics.Raycast(screenRay, out hitV, rayLength, layerMaskVoxel.value) )
                    {
                        if (Mathf.Abs(distanceM - hitV.distance) < diagonal)
                        {
                            string name = hitV.collider.gameObject.name;
                            //Debug.Log(name);
                            string[] nameEach = name.Split(' ');
                            int xx = int.Parse(nameEach[0]);
                            int yy = int.Parse(nameEach[1]);
                            int zz = int.Parse(nameEach[2]);

                            //VoxSpace.voxelGrid[xx][yy][zz].color.r += colorTemp.r;
                            //VoxSpace.voxelGrid[xx][yy][zz].color.g += colorTemp.g;
                            //VoxSpace.voxelGrid[xx][yy][zz].color.b += colorTemp.b;

                            VoxSpace.voxelGrid[xx][yy][zz].colorTex.Add(colorTemp);


                            VoxSpace.voxelGrid[xx][yy][zz].rayCount++;

                            //// 衝突したオブジェクトを消す
                            //DestroyImmediate(hit.collider.gameObject);
                        }
                    }


                }

            }
        }
        for(int x=0; x < VoxSpace.boxSize[0]; x++)
        {
            for (int y=0; y < VoxSpace.boxSize[1]; y++)
            {
                for (int z=0; z < VoxSpace.boxSize[2]; z++)
                {
                    float r=0;
                    float g=0;
                    float b=0;
                    foreach (var each in VoxSpace.voxelGrid[x][y][z].colorTex)
                    {
                        r += each.r;
                        g += each.g;
                        b += each.b;
                    }
                    var num = VoxSpace.voxelGrid[x][y][z].rayCount;
                    VoxSpace.voxelGrid[x][y][z].color.r = r / num;
                    VoxSpace.voxelGrid[x][y][z].color.g = g / num;
                    VoxSpace.voxelGrid[x][y][z].color.b = b / num;


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

        Debug.Log(minRateVoxel);
        Debug.Log(maxRateVoxel);

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
        //VoxSpace1.CreateVoxelSurf(voxelbase, parentVoxel);
        VoxSpace1.CreateVoxelSurf(cubeCollider, parentVoxel);

        //VoxSpace1.CreateVoxelSurfNew(voxelbase, parentVoxel);
        // VoxSpace1.CreateVoxel(voxelbase, parentVoxel);//crossしているものだけ描画
        //VoxSpace1.CreateVoxel_Fill(voxelbase, parentVoxel);
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

        VoxSpace1.PlyCrossEdge(InitSolid1);
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




    // Update is called once per frame

    //ボクセル空間を定義
    //VoxSpace1.SpaceDefine(m, angle);

}