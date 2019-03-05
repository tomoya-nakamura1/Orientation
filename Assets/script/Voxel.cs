using UnityEngine;
using UnityEditor;
using System.IO;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;


/*========================単位Voxelブロックのクラス================================*/
//[System.Serializable]
public class Voxel
{
    static int __MAX_DISTANCE = 1000000;             //距離場の初期値
                                                     // public bool fill = false;                       //内部Voxel
    public float distance = __MAX_DISTANCE;           //距離場の格納
    public bool known = false;                      //距離場の計算時
    public bool suport = false;                     //サポート用のVoxelかどうか
    public bool pole = false;
    //public byte color = 0;                          //colorが0ならなし
    //public Color32 color = new Color32();
    public Color color = new Color();               //一番最後に出力するときに255で割る必要あり
    public List<Color> colorTex = new List<Color>();
    public List<float> hueTex = new List<float>();

    public LABColor colorLab = new LABColor();              //Labに変換された色を格納


    public float inVoxelNum;   //いくつsurveyボクセルが内部に存在しているのか

    public int num = 0;//ボクセルの番号

    public bool surface = false;                      //表面かどうか
    public Vector3 pos = Vector3.zero;
    public bool edge = false;//ボクセル空間の端については特殊な判定をする必要のある場合が多々あり、予め判定しておくと便利
    public bool Fill = true;//中身が詰まっているか//初期値がtrue

    public bool cross = false;
    public int colorID;
    public int rayCount;
    public int[] local_pos = new int[2];
    //public float disTri; //メッシュの三角形の面からボクセルの中心までの距離

    public List<int> crossedVertsList = new List<int>();
    public List<int> crossedEdgesList = new List<int>();
    public List<int> crossedFacesList = new List<int>();
    public bool featureVoxel = false;
    public float hue;//色相 - 色の種類（例えば赤、青、黄色）。0 - 360の範囲（アプリケーションによっては0 - 100 % に正規化されることもある）。
    public float saturation;//彩度 - 色の鮮やかさ。0 - 100 % の範囲。刺激純度と colorimetric purity の色彩的な量と比較して「純度」などともいう。
    //色の彩度の低下につれて、灰色さが顕著になり、くすんだ色が現れる。また彩度の逆として「desaturation」を定義すると有益である。
    public float value;//明度 - 色の明るさ。0 - 100 % の範囲。
    public List<float> luminance = new List<float>();//輝度値


    //各ボクセルを初期化する
    public void IniVoxel()
    {
        surface = false;                      //表面かどうか
        pos = Vector3.zero;
        edge = false;//ボクセル空間の端については特殊な判定をする必要のある場合が多々あり、予め判定しておくと便利
        Fill = true;//中身が詰まっているか//初期値がtrue
    }

    public Color DistanceColor(float max)
    {
        float variable = distance / max;

        var d_color = new Color();
        d_color.a = 0.5f;
        if (variable < 0.25)
        {
            d_color.g = variable / 0.25f;
            d_color.b = 1.0f;
            d_color.r = 0;
        }
        else if (variable < 0.5f)
        {
            d_color.g = 1.0f;
            d_color.b = 1.0f - (variable - 0.25f) / 0.25f;
            d_color.r = 0;
        }
        else if (variable < 0.75f)
        {
            d_color.b = 0;
            d_color.r = (variable - 0.5f) / 0.25f;
            d_color.g = 1.0f;
        }
        else
        {
            d_color.b = 0;
            d_color.r = 1.0f;
            d_color.g = 1.0f - (variable - 0.75f) / 0.25f;
        }

        return d_color;
    }


}

/*========================Voxelブロック集合のクラス================================*/
//[System.Serializable]
public partial class VoxelGrid : MonoBehaviour
{
    public List<List<List<Voxel>>> voxelGrid = new List<List<List<Voxel>>>(); //Voxelのリスト

    public List<List<List<Voxel>>> surveyVoxel = new List<List<List<Voxel>>>(); //Voxelのリスト

    //public int[] boxSize = new int[3];
    private List<Voxel> boundary = new List<Voxel>();
    public float average = 0.0f;
    public Voxel max_distance;



    public int[] boxSize = new int[3];              //ボクセル空間のx,y,z方向のボクセル数
    public int[] surveyBoxSize = new int[3];              //ボクセル空間のx,y,z方向のsurveyボクセル数
    public float Length;          //ボクセル1辺の長さ
    public float LengthY;          //ボクセル1辺の長さY
    public float LengthXZ;          //ボクセル1辺の長さXZ

    public float surveyLengY;          //surveyボクセル1辺の長さY
    public float surveyLengXZ;          //surveyボクセル1辺の長さXZ

    public int fillVoxelNumber;    //Fillボクセル数
    public float crossVoxelNum;//surfaceボクセル数


    Vector3 SpaceSize;    //ボクセル空間の大きさ

    Color backColor = new Color();
    public List<Color> legoColor = new List<Color>();
    List<Color> legoColor_Lab = new List<Color>();



    //public int maxNumberVoxel=0;
    //public int minNumberVoxel = 0;

    //public Vector3 maxNumberVoxel = new Vector3();//角度，数，体積
    //public Vector3 minNumberVoxel = new Vector3();//角度，数，体積

    //コンストラクタ:Gridのサイズを決定-基本使わない
    // public VoxelGrid(int[] grid_size)
    // {
    //     for (int size_x = 0; size_x < grid_size[0]; size_x++)
    //     {
    //         List<List<Voxel>> yz = new List<List<Voxel>>();
    //         for (int size_y = 0; size_y < grid_size[1]; size_y++)
    //         {
    //             List<Voxel> z = new List<Voxel>(grid_size[2]);
    //             yz.Add(z);
    //         }
    //         voxelGrid.Add(yz);
    //     }
    // }
    // //コンストラクタ
    //public VoxelGrid() {  }

    //インデクサーの定義:位置からVoxel情報の参照が可能
    public Voxel this[int x, int y, int z]
    {
        get
        {
            if (x >= boxSize[0] || y >= boxSize[1] || z >= boxSize[2] || x < 0 || y < 0 || z < 0) return null;
            else return voxelGrid[x][y][z];

        }
        set
        {
            voxelGrid[x][y][z] = value;
        }
    }
    //Vector3からVoxel取得
    public Voxel FromPosition(Vector3 position)
    {
        return this[(int)position[0], (int)position[1], (int)position[2]];
    }

    ////モデルを表示する
    //public void Createmodel(Transform obj, Material mat, DisplayCondition condition, Color[] block_color)
    //{
    //    foreach (Transform inobj in obj)
    //    {
    //        GameObject.Destroy(inobj.gameObject);
    //    }
    //    GameObject voxel;
    //    foreach (Voxel xyz in ToVoxelList())
    //    {
    //        //Objectの生成
    //        if (condition.CheckVoxel(xyz))
    //        {
    //            voxel = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //            voxel.transform.SetParent(obj);                                    //親をVoxelsにする
    //            voxel.transform.position = xyz.pos - Offset();                     //位置を変更
    //            Renderer rend = voxel.GetComponent<Renderer>();

    //            rend.material = mat;
    //            if (xyz.pole) rend.material.color = Color.red;
    //            else if (xyz.suport) rend.material.color = Color.blue;
    //            ////else if (xyz.lego != null) rend.material.color = xyz.lego.random;
    //            //else if (xyz.lego != null) rend.material.color = block_color[xyz.lego.main_color];
    //            ////else rend.material.color = block_color[xyz.color];
    //            ////rend.material.color = xyz.DistanceColor(7f);
    //        }
    //    }
    //}

    //モデル位置を中心にするベクトル
    public Vector3 Offset()
    {
        Vector3 offset = new Vector3(0.5f * boxSize[0], 0, 0.5f * boxSize[2]);
        return offset;
    }

    //近傍のVoxelを取得
    public Voxel Neighbor(Voxel xyz, int direction, int dis = 1)
    {
        switch (direction)
        {
            case 0:
                if (((int)xyz.pos[0] + dis) < boxSize[0]) return this[(int)xyz.pos[0] + dis, (int)xyz.pos[1], (int)xyz.pos[2]];     //x+方向
                else return null;
            case 1:
                if (((int)xyz.pos[0] - dis) >= 0) return this[(int)xyz.pos[0] - dis, (int)xyz.pos[1], (int)xyz.pos[2]];             //x-方向
                else return null;
            case 2:
                if (((int)xyz.pos[1] + dis) < boxSize[1]) return this[(int)xyz.pos[0], (int)xyz.pos[1] + dis, (int)xyz.pos[2]];       //y+方向
                else return null;
            case 3:
                if (((int)xyz.pos[1] - dis) >= 0) return this[(int)xyz.pos[0], (int)xyz.pos[1] - dis, (int)xyz.pos[2]];             //y-方向
                else return null;
            case 4:
                if (((int)xyz.pos[2] + dis) < boxSize[2]) return this[(int)xyz.pos[0], (int)xyz.pos[1], (int)xyz.pos[2] + dis];       //z+方向
                else return null;
            case 5:
                if (((int)xyz.pos[2] - dis) >= 0) return this[(int)xyz.pos[0], (int)xyz.pos[1], (int)xyz.pos[2] - dis];             //z-方向
                else return null;
            default:
                return null;
        }
    }

    ////FMMを用いた距離場の計算
    //public void CalculateDistance()
    //{
    //    var trial = new List<Voxel>();

    //    //境界に位置するVoxelの配列からtrialへ
    //    foreach (Voxel v in boundary)
    //    {
    //        for (int i = 0; i < 6; i++)
    //        {
    //            var vv = this.Neighbor(v, i);
    //            if (!(vv == null) && vv.fill && !vv.known)
    //            {
    //                AssignDistance(vv);
    //                for (int ii = 0; ii <= trial.Count; ii++)
    //                {
    //                    if (ii == trial.Count)
    //                    {
    //                        trial.Add(vv);
    //                        break;
    //                    }
    //                    else if (vv.distance < trial[ii].distance)
    //                    {
    //                        trial.Insert(ii, vv);
    //                        break;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    max_distance = boundary[0];
    //    //trial内がなくなるまで
    //    while (trial.Count > 0)
    //    {
    //        //距離の一番小さいVoxelを取り出し
    //        Voxel first = trial[0];
    //        trial.RemoveAt(0);

    //        for (int i = 0; i < 6; i++)
    //        {
    //            var vn = this.Neighbor(first, i);
    //            if (!(vn == null) && vn.fill && !vn.known)
    //            {
    //                AssignDistance(vn);
    //                //距離が最大なものを格納
    //                if (max_distance.distance < vn.distance) { max_distance = vn; }
    //                for (int ii = 0; ii <= trial.Count; ii++)
    //                {
    //                    if (ii == trial.Count)
    //                    {
    //                        trial.Add(vn);
    //                        break;
    //                    }
    //                    else if (vn.distance < trial[ii].distance)
    //                    {
    //                        trial.Insert(ii, vn);
    //                        break;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    AverageDistance();
    //}

    //VoxelGrid中のVoxelの距離を計算
    void AssignDistance(Voxel trialV)
    {
        //trial中の距離場の計算
        float a = 0.0f;         //2次の項
        float b = 0.0f;         //1次の項
        float c = 0.0f;         //0次の項
        for (int i = 0; i < 6; i++)
        {
            var neighbor = this.Neighbor(trialV, i);
            if (!(neighbor == null) && neighbor.known)
            {
                a += 1.0f;
                b -= neighbor.distance * 2.0f;
                c += neighbor.distance * neighbor.distance;
            }
        }

        c -= 1.0f;           //距離/時間
        float D = b * b - 4 * a * c;       //判別式
        if (D > 0.0)        //実数解をもつ時
        {
            trialV.distance = (float)((-b + Mathf.Sqrt((float)D)) / 2.0 / a);
        }
        else if (D == 0.0)  //重解をもつ時
        {
            trialV.distance = (float)(-b / 2.0 / a);
        }
        else            //複素数解をもつ時
        {
            trialV.distance = (float)(-b / 2.0 / a);

        }
        trialV.known = true;
        //Debug.Log("距離は" + trialV.distance);

    }
    //void AverageDistance()
    //{
    //    var fill_count = 0;
    //    foreach (List<List<Voxel>> z in voxelGrid)
    //    {
    //        foreach (List<Voxel> yz in z)
    //        {
    //            foreach (Voxel xyz in yz)
    //            {
    //                if (xyz.fill)
    //                {
    //                    average += xyz.distance;
    //                    fill_count++;
    //                }
    //            }
    //        }
    //    }
    //    average /= fill_count;
    //}

    //xyz座標のボクセルをただのListのボクセルとして取得
    public List<Voxel> ToVoxelList()
    {
        var voxel_list = new List<Voxel>();
        foreach (List<List<Voxel>> z in voxelGrid)
        {
            foreach (List<Voxel> yz in z)
            {
                foreach (Voxel xyz in yz)
                {
                    voxel_list.Add(xyz);
                }
            }
        }
        return voxel_list;
    }

    ////空隙率(Void Ratio)を計算する
    //public float VoidRatio(ref int all, ref int not_hollow, ref int in_support)
    //{
    //    all = 0;            //全体の内部ボクセル数
    //    not_hollow = 0;     //中空でないボクセル数
    //    in_support = 0;     //内部サポートの数
    //    foreach (Voxel xyz in ToVoxelList())
    //    {
    //        if (xyz.fill) all++;
    //        //  if (xyz.lego != null && !xyz.lego.hollow) not_hollow++;
    //        if (xyz.fill && xyz.suport) in_support++;
    //    }
    //    return (all - not_hollow - in_support) / (float)all;
    //}



    ////voxel_dataからvoxelGridへ
    //void DataToGrid(List<List<float>> data)
    //{
    //    /*==================================　Size計算　========================================================================*/
    //    var max_size = data.Where((value, index) => index != 0).Select(a => new { x = a[0], y = a[2], z = a[1] }).Aggregate((post, next) => new { x = post.x > next.x ? post.x : next.x, y = post.y > next.y ? post.y : next.y, z = post.z > next.z ? post.z : next.z });
    //    var min_size = data.Where((value, index) => index != 0).Select(a => new { x = a[0], y = a[2], z = a[1] }).Aggregate((post, next) => new { x = post.x < next.x ? post.x : next.x, y = post.y < next.y ? post.y : next.y, z = post.z < next.z ? post.z : next.z });

    //    boxSize[0] = (int)max_size.x - (int)min_size.x + 1;
    //    boxSize[1] = (int)max_size.y - (int)min_size.y + 1;
    //    boxSize[2] = (int)max_size.z - (int)min_size.z + 1;

    //    var data2 = data.Where((value, index) => index != 0).Select(a => new { x = a[0] - min_size.x, y = a[2] - min_size.y, z = a[1] - min_size.z, c = a[3] }).ToList();
    //    /*===========================================VoxelGridへ============================================*/
    //    for (int size_x = 0; size_x < boxSize[0]; size_x++)
    //    {
    //        List<List<Voxel>> yz = new List<List<Voxel>>();
    //        for (int size_y = 0; size_y < boxSize[1]; size_y++)
    //        {
    //            List<Voxel> z = new List<Voxel>();
    //            for (int size_z = 0; size_z < boxSize[2]; size_z++) { z.Add(new Voxel()); z[size_z].pos = new Vector3((float)size_x, (float)size_y, (float)size_z); }
    //            yz.Add(z);
    //        }
    //        voxelGrid.Add(yz);
    //    }
    //    //内部Voxelの判定
    //    foreach (var block in data2)
    //    {
    //        Voxel bb = this[(int)block.x, (int)block.y, (int)block.z];
    //        bb.fill = true;
    //        bb.color = (byte)block.c;
    //    }
    //    CheckSurface();
    //}

    //全Voxel中、表面の抽出
    //public void CheckSurface()
    //{
    //    boundary.Clear();
    //    foreach (List<List<Voxel>> z in voxelGrid)
    //    {
    //        foreach (List<Voxel> yz in z)
    //        {
    //            foreach (Voxel xyz in yz)
    //            {
    //                if (xyz.fill)
    //                {
    //                    for (int i = 0; i < 6; i++)
    //                    {
    //                        var neighbor = this.Neighbor(xyz, i);
    //                        if (neighbor == null || !neighbor.fill)
    //                        {
    //                            xyz.surface = true;
    //                            xyz.distance = 0;
    //                            xyz.known = true;
    //                            boundary.Add(xyz);
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}



    //メッシュ型の引数をもらって、バウンディングボックスと
    //ユーザーが入力した解像度とボクセル空間の高さからボクセル辺の長さを決定(4:4:3)
    //使ってる方
    public void SpaceDefine(Mesh mesh, float L, Solid model)//,GameObject voxelbase)
    {
        Vector3 min;
        //Vector3 max, max_new;
        //float VoxSpaceSizeMax;

        //    /*==========================================================================================================*/

        //SpaceSize = mesh.bounds.extents * 2;

        //min = (mesh.bounds.center - mesh.bounds.extents);
        ////エッジのボクセルにも判定を行いたいので一つずつ大きくする，合計2つ
        //boxSize[0] = (int)(SpaceSize.x / LengthXZ) + 4;
        //boxSize[1] = (int)L + 3;
        //boxSize[2] = (int)(SpaceSize.z / LengthXZ) + 4;
        //    /*==========================================================================================================*/

        SpaceSize = mesh.bounds.size;
        //SpaceSize.x = model.bBdiag;
        //SpaceSize.z = model.bBdiag;

        //min.y = (mesh.bounds.center - mesh.bounds.extents).y;
        min = (mesh.bounds.center - mesh.bounds.extents);

        ////ここを考えればいいと思う
        //min.x = -model.bBdiag / 2f ;
        //min.z = -model.bBdiag / 2f ;

        //Debug.Log(mesh.bounds.center);

        //ユーザーが入力した解像度とボクセル空間の高さからボクセル一辺の長さを決定
        LengthY = SpaceSize.y / L;
        //LengthXZ = LengthY * 5 / 6;//レゴ
        LengthXZ = LengthY * 4 / 3;
        //各方向のボクセル数を決定
        //boxSize[0] = (int)(SpaceSize.x / LengthXZ) +2;
        //boxSize[1] = (int)L+1;
        //boxSize[2] = (int)(SpaceSize.z / LengthXZ) + 2;

        //エッジのボクセルにも判定を行いたいので一つずつ大きくする，合計2つ
        //boxSize[0] = (int)(SpaceSize.x / LengthXZ) + 4;
        //boxSize[1] = (int)L + 3;
        //boxSize[2] = (int)(SpaceSize.z / LengthXZ) + 4;
        boxSize[0] = (int)(SpaceSize.x / LengthXZ) + 8;
        boxSize[1] = (int)L + 7;
        boxSize[2] = (int)(SpaceSize.z / LengthXZ) + 8;

        //ここを考えればいいと思う
        //min.x = -model.bBdiag / 2f - 4*LengthXZ;
        //min.y -= -3 * LengthY;
        //min.z = -model.bBdiag / 2f - 4*LengthXZ;

        min.x -= 3 * LengthXZ;
        min.y -= 3 * LengthY;
        min.z -= 3 * LengthXZ;

        //min.x = -LengthXZ * (boxSize[0] + 1) / 2f;
        //min.z = -LengthXZ * (boxSize[2] + 1) / 2f;




        //if (boxSize[0] % 2 == 0)
        //{
        //    min.x = LengthXZ*(boxSize[0]+1)/2f;
        //    min.z = LengthXZ *( boxSize[2] +1)/ 2f;


        //}
        //else
        //{
        //    min.x = LengthXZ * (boxSize[0] + 1) / 2f;
        //    min.z = LengthXZ * (boxSize[2] + 1) / 2f;
        //}



        for (int size_x = 0; size_x < boxSize[0]; size_x++)
        {
            List<List<Voxel>> yz = new List<List<Voxel>>();
            for (int size_y = 0; size_y < boxSize[1]; size_y++)
            {
                List<Voxel> z = new List<Voxel>();
                for (int size_z = 0; size_z < boxSize[2]; size_z++)
                {
                    z.Add(new Voxel());
                    //z[size_z].pos = new Vector3(min.x+ LengthXZ*((float)size_x),
                    //    min.y+ LengthY * ((float)size_y), 
                    //    min.z+ LengthXZ * ((float)size_z)); }
                    //エッジのボクセルにも判定を行いたいので一つずつ大きくする，合計2つ大きくしている

                    z[size_z].pos = new Vector3(min.x + LengthXZ * ((float)size_x - 1),
                       min.y + LengthY * ((float)size_y - 1),
                       min.z + LengthXZ * ((float)size_z - 1));
                }
                yz.Add(z);
            }
            voxelGrid.Add(yz);
        }




    }

    //上手くいかなかったから2つに分けた，一つでも役割上は大丈夫
    public void PlySpaceDefine(Mesh mesh, float L, Solid model)//,GameObject voxelbase)
    {
        Vector3 min;
        //Vector3 max, max_new;
        //float VoxSpaceSizeMax;

        //    /*==========================================================================================================*/

        SpaceSize = mesh.bounds.extents * 2;

        min = (mesh.bounds.center - mesh.bounds.extents);
        LengthY = SpaceSize.y / L;
        //LengthXZ = LengthY * 4 / 3;
        //オチビサン用
        LengthXZ = LengthY * 5 / 8;
        //エッジのボクセルにも判定を行いたいので一つずつ大きくする，合計2つ
        boxSize[0] = (int)(SpaceSize.x / LengthXZ) + 8;
        boxSize[1] = (int)L + 7;
        boxSize[2] = (int)(SpaceSize.z / LengthXZ) + 8;
        //    /*==========================================================================================================*/

        //SpaceSize = mesh.bounds.size;
        //SpaceSize.x = model.bBdiag;
        //SpaceSize.z = model.bBdiag;

        //min.y = (mesh.bounds.center - mesh.bounds.extents).y;
        //ここを考えればいいと思う
        //min.x = -model.bBdiag / 2f;
        //min.z = -model.bBdiag / 2f;


        //Debug.Log(mesh.bounds.center);

        //ユーザーが入力した解像度とボクセル空間の高さからボクセル一辺の長さを決定
        LengthY = SpaceSize.y / L;
        LengthXZ = LengthY * 4 / 3;
        //各方向のボクセル数を決定
        //boxSize[0] = (int)(SpaceSize.x / LengthXZ) +2;
        //boxSize[1] = (int)L+1;
        //boxSize[2] = (int)(SpaceSize.z / LengthXZ) + 2;

        //エッジのボクセルにも判定を行いたいので一つずつ大きくする，合計2つ
        //boxSize[0] = (int)(SpaceSize.x / LengthXZ) + 4;
        //boxSize[1] = (int)L + 3;
        //boxSize[2] = (int)(SpaceSize.z / LengthXZ) + 4;

        //min.x = -LengthXZ * (boxSize[0] + 1) / 2f;
        //min.z = -LengthXZ * (boxSize[2] + 1) / 2f;



        min.x -= 3*LengthXZ;
        min.y -= 3*LengthY;
        min.z -= 3*LengthXZ;



        //if (boxSize[0] % 2 == 0)
        //{
        //    min.x = LengthXZ*(boxSize[0]+1)/2f;
        //    min.z = LengthXZ *( boxSize[2] +1)/ 2f;


        //}
        //else
        //{
        //    min.x = LengthXZ * (boxSize[0] + 1) / 2f;
        //    min.z = LengthXZ * (boxSize[2] + 1) / 2f;
        //}



        for (int size_x = 0; size_x < boxSize[0]; size_x++)
        {
            List<List<Voxel>> yz = new List<List<Voxel>>();
            for (int size_y = 0; size_y < boxSize[1]; size_y++)
            {
                List<Voxel> z = new List<Voxel>();
                for (int size_z = 0; size_z < boxSize[2]; size_z++)
                {
                    z.Add(new Voxel());
                    //z[size_z].pos = new Vector3(min.x+ LengthXZ*((float)size_x),
                    //    min.y+ LengthY * ((float)size_y), 
                    //    min.z+ LengthXZ * ((float)size_z)); }
                    //エッジのボクセルにも判定を行いたいので一つずつ大きくする，合計2つ大きくしている

                    z[size_z].pos = new Vector3(min.x + LengthXZ * ((float)size_x - 1),
                       min.y + LengthY * ((float)size_y - 1),
                       min.z + LengthXZ * ((float)size_z - 1));
                }
                yz.Add(z);
            }
            voxelGrid.Add(yz);
        }




    }

    //メッシュ型の引数をもらって、バウンディングボックスと
    //ユーザーが入力した解像度とボクセル空間の高さからボクセル一辺の長さを決定
    //立方体のためメッシュを縦に4/3倍している必要、また戻す必要あり
    public void CubeSpaceDefine(Mesh mesh, float L)
    {
        Vector3 min;
        //Vector3 max, max_new;
        //float VoxSpaceSizeMax;
        SpaceSize = mesh.bounds.extents * 2;

        min = (mesh.bounds.center - mesh.bounds.extents);

        //ユーザーが入力した解像度とボクセル空間の高さからボクセル一辺の長さを決定
        Length = SpaceSize.y / L;
        //各方向のボクセル数を決定
        boxSize[0] = (int)(SpaceSize.x / Length) + 2;
        boxSize[1] = (int)L + 1;
        boxSize[2] = (int)(SpaceSize.z / Length) + 2;




        for (int size_x = 0; size_x < boxSize[0]; size_x++)
        {
            List<List<Voxel>> yz = new List<List<Voxel>>();
            for (int size_y = 0; size_y < boxSize[1]; size_y++)
            {
                List<Voxel> z = new List<Voxel>();
                for (int size_z = 0; size_z < boxSize[2]; size_z++)
                {
                    z.Add(new Voxel());
                    z[size_z].pos = new Vector3(min.x + Length * ((float)size_x),
                        min.y + Length * ((float)size_y),
                        min.z + Length * ((float)size_z));
                }
                yz.Add(z);
            }
            voxelGrid.Add(yz);
        }

        var size = new Vector3(Length, Length, Length);

        // Vector3 size = Transform.transform.localScale;
        for (int size_x = 0; size_x < boxSize[0]; size_x++)
        {

            for (int size_y = 0; size_y < boxSize[1]; size_y++)
            {

                for (int size_z = 0; size_z < boxSize[2]; size_z++)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = voxelGrid[size_x][size_y][size_z].pos;
                    cube.transform.localScale = size;

                    GameObject.Destroy(cube, 5f);
                    //GameObject.DestroyImmediate(cube);

                    cube = null;
                    //   cube.transform.localScale.x = 2.0f;
                }

            }

        }

    }
    //Solidクラスのsedgesとsfacesに値を格納する関数
    //今のところはsfacesには格納していない
    //使ってない
    public void En_sedges_sfaces(Solid m)
    {
        var edge_temp = new Edge();
        //Debug.Log(m.sverts[1].vr);
        edge_temp.he1.hvert.vr = m.sverts[1].vr;

        //for (int i = 0; i < m.numVertex - 1; ++i)
        //{
        //    var edge_temp = new Edge();
        //    edge_temp.he1.hedge = edge_temp;
        //    edge_temp.he2.hedge = edge_temp;
        //    ////Debug.Log(m.sverts[i].vr);
        //    edge_temp.he1.hvert.vr = m.sverts[i].vr;
        //    edge_temp.he2.hvert.vr = m.sverts[i + 1].vr;
        //    edge_temp.num = i;
        //    m.sedges.Add(edge_temp);
        //}
    }

    //メッシュ型の引数をもらって、バウンディングボックスと
    //ユーザーが入力した解像度とボクセル空間の高さからボクセル辺の長さを決定(4:4:3)
    //使ってる方
    public void SurveySpaceDefine(Mesh mesh, float L, float divisionNum)//,GameObject voxelbase)
    {
        Vector3 min;
        float division = Mathf.Log(divisionNum, 2);
        division = division / 1.5f;
        //division = division * 2f;
        //Vector3 max, max_new;
        //float VoxSpaceSizeMax;

        //SpaceSize = mesh.bounds.extents * 2;

        min = voxelGrid[0][0][0].pos;

        //Debug.Log(mesh.bounds.center);

        //ユーザーが入力した解像度とボクセル空間の高さからボクセル一辺の長さを決定
        //LengthY = SpaceSize.y / L;
        //LengthXZ = LengthY * 4 / 3;
        ////各方向のボクセル数を決定
        //boxSize[0] = (int)(SpaceSize.x / LengthXZ) + 2;
        //boxSize[1] = (int)L + 1;
        //boxSize[2] = (int)(SpaceSize.z / LengthXZ) + 2;

        surveyLengY = LengthY / division;
        surveyLengXZ = LengthXZ / division;
        //各方向のボクセル数を決定
        surveyBoxSize[0] = boxSize[0] * (int)division;
        surveyBoxSize[1] = boxSize[1] * (int)division;
        surveyBoxSize[2] = boxSize[2] * (int)division;

        //min.x -= surveyLengXZ / division;
        //min.y -= surveyLengY / division;
        //min.z -= surveyLengXZ / division;


        min.x = min.x - LengthXZ / 2f + surveyLengXZ / 2f;
        min.y = min.y - LengthY / 2f + surveyLengY / 2f;
        min.z = min.z - LengthXZ / 2f + surveyLengXZ / 2f;



        for (int size_x = 0; size_x < surveyBoxSize[0]; size_x++)
        {
            List<List<Voxel>> yz = new List<List<Voxel>>();
            for (int size_y = 0; size_y < surveyBoxSize[1]; size_y++)
            {
                List<Voxel> z = new List<Voxel>();
                for (int size_z = 0; size_z < surveyBoxSize[2]; size_z++)
                {
                    z.Add(new Voxel());
                    z[size_z].pos = new Vector3(min.x + surveyLengXZ * ((float)size_x),
                        min.y + surveyLengY * ((float)size_y),
                        min.z + surveyLengXZ * ((float)size_z));
                }
                yz.Add(z);
            }
            surveyVoxel.Add(yz);
        }


    }

    //メッシュ頂点を含むボクセルの判定
    public void CrossVert(Solid m)
    {
        //計算中のHalfEdge
        // HalfEdge temp;

        //各三角形のバウンディングボックス
        Vector3 small, large;

        //バウンディングボックスの余白
        Vector3 unitVox = new Vector3(LengthXZ / 2.0f, LengthY / 2.0f, LengthXZ / 2.0f);

        // Vector3 P0;

        // bool cross = false;//このボクセルが交差しているなら

        //float t = 0.0f;//三角形の辺の直線の式のパラメーター
        //Vector3 P;//ボクセルの面と三角形の辺の直線との交点

        //int c = 0;
        //foreach(var v in m.sverts)
        //{
        //    c++;
        //    Debug.Log(c);

        //}

        //for(int v=0;v<m.numVertex;++v)
        foreach (var v in m.sverts)
        {
            small = v.vr;
            large = v.vr;
            // Debug.Log(m.sverts[v].vr);
            //small = m.sverts[v].vr;
            //large = m.sverts[v].vr;


            small -= unitVox;
            large += unitVox;
            //各頂点のバウンディングボックスと各ボクセルとの交差判定
            for (int i = 0; i < boxSize[0]; ++i)
            {
                //各頂点のバウンディングボックスのx方向による判定
                if ((voxelGrid[i][0][0].pos.x < small.x) || (large.x < voxelGrid[i][0][0].pos.x)) continue;

                for (int j = 0; j < boxSize[1]; ++j)
                {
                    //各頂点のバウンディングボックスのy方向による判定
                    if ((voxelGrid[0][j][0].pos.y < small.y) || (large.y < voxelGrid[0][j][0].pos.y)) continue;

                    for (int k = 0; k < boxSize[2]; ++k)
                    {
                        //各頂点のバウンディングボックスのz方向による判定
                        if ((voxelGrid[0][0][k].pos.z < small.z) || (large.z < voxelGrid[0][0][k].pos.z)) continue;

                        voxelGrid[i][j][k].cross = true;
                        voxelGrid[i][j][k].surface = true;
                        //cross = true;
                        //break;
                    }
                }
            }
        }


    }


    //メッシュ辺と交差するボクセルの判定
    public void CrossEdge(Solid m)
    {
        //各辺のバウンディングボックス
        Vector3 small, large;

        //バウンディングボックスの余白
        Vector3 unitVox = new Vector3(LengthXZ / 2.0f, LengthY / 2.0f, LengthXZ / 2.0f);

        Vector3 P0, P1;

        //このボクセルが交差しているなら
        bool cross = false;

        float t;//辺の直線の式のパラメーター
        Vector3 P;//ボクセルの面と辺の直線との交点

        //foreach (var e in m.sedges)
        //{
        //    Debug.Log(e.he1.hvert.vr);
        //}

        //各辺について調べる
        int count = 0;
        foreach (var e in m.sedges)
        {
            e.colorNum = count;
            ++count;
            //    for (auto e : m.sedges)
            //{
            //各辺のバウンディングボックスを調べる
            P0 = e.he1.hvert.vr;
            //Debug.Log(e.he1.hvert.vr);
            P1 = e.he2.hvert.vr;

            if (P0.x < P1.x)
            {
                small.x = P0.x;
                large.x = P1.x;
            }
            else
            {
                small.x = P1.x;
                large.x = P0.x;
            }

            if (P0.y < P1.y)
            {
                small.y = P0.y;
                large.y = P1.y;
            }
            else
            {
                small.y = P1.y;
                large.y = P0.y;
            }

            if (P0.z < P1.z)
            {
                small.z = P0.z;
                large.z = P1.z;
            }
            else
            {
                small.z = P1.z;
                large.z = P0.z;
            }

            small -= unitVox;
            large += unitVox;


            //各辺のバウンディングボックスと各ボクセルとの交差判定
            for (int i = 0; i < boxSize[0]; ++i)
            {
                //各辺のバウンディングボックスのx方向による判定
                if ((voxelGrid[i][0][0].pos.x < small.x) || (large.x < voxelGrid[i][0][0].pos.x)) continue;

                for (int j = 0; j < boxSize[1]; ++j)
                {
                    //各辺のバウンディングボックスのy方向による判定
                    if ((voxelGrid[0][j][0].pos.y < small.y) || (large.y < voxelGrid[0][j][0].pos.y)) continue;

                    for (int k = 0; k < boxSize[2]; ++k)
                    {
                        //各辺のバウンディングボックスのz方向による判定
                        if ((voxelGrid[0][0][k].pos.z < small.z) || (large.z < voxelGrid[0][0][k].pos.z)) continue;

                        if (voxelGrid[i][j][k].cross) continue;

                        //辺がボクセルのいずれかの面と交差
                        cross = false;
                        // cross = true;//勝手に変えた


                        P0 = e.he1.hvert.vr;
                        P1 = e.he2.hvert.vr;

                        //辺とボクセルのyz平面2面との交差
                        for (int i0 = 0; i0 < 2; ++i0)
                        {
                            //ボクセルのyz平面のx座標
                            P.x = voxelGrid[i][j][k].pos.x + (float)(2 * i0 - 1) * (LengthXZ / 2.0f);

                            //三角形の辺の直線のパラメーター
                            t = (P.x - P0.x) / (P1.x - P0.x);

                            //tは線分P0-P1内(0.0<=t<=1.0)
                            if ((t < 0.0) || (1.0 < t)) continue;

                            //辺の直線とボクセルのyz平面の交点
                            P = (P1 - P0) * t + P0;

                            //交点のy座標の確認
                            //if ((voxelGrid[i][j][k].pos.y - (Length / 2.0) <= P.y) && (P.y <= voxelGrid[i][j][k].pos.y + (Length / 2.0)))
                            if ((voxelGrid[i][j][k].pos.y - (LengthY / 2.0) - P.y <= 1.0e-8) && (-1.0e-8 <= voxelGrid[i][j][k].pos.y + (LengthY / 2.0) - P.y))
                            {
                                //交点のz座標の確認
                                //if ((voxelGrid[i][j][k].pos.z - (Length / 2.0) <= P.z) && (P.z <= voxelGrid[i][j][k].pos.z + (Length / 2.0)))
                                if ((voxelGrid[i][j][k].pos.z - (LengthXZ / 2.0) - P.z <= 1.0e-8) && (-1.0e-8 <= voxelGrid[i][j][k].pos.z + (LengthXZ / 2.0) - P.z))
                                {
                                    voxelGrid[i][j][k].cross = true;
                                    voxelGrid[i][j][k].surface = true;
                                    cross = true;
                                    break;
                                }
                            }
                        }
                        if (cross) continue;    //辺とボクセルのxz平面2面との交差
                        for (int j0 = 0; j0 < 2; ++j0)
                        {
                            //ボクセルのxz平面のy座標
                            P.y = voxelGrid[i][j][k].pos.y + (float)(2 * j0 - 1) * (LengthY / 2.0f);

                            //辺の直線のパラメーター
                            t = (P.y - P0.y) / (P1.y - P0.y);

                            //tは線分P0-P1内(0.0<=t<=1.0)
                            if ((t < 0.0) || (1.0 < t)) continue;

                            //辺の直線とボクセルのxz平面の交点
                            P = (P1 - P0) * t + P0;

                            //交点のx座標の確認
                            //if ((voxelGrid[i][j][k].pos.x - (Length / 2.0) <= P.x) && (P.x <= voxelGrid[i][j][k].pos.x + (Length / 2.0)))
                            if ((voxelGrid[i][j][k].pos.x - (LengthXZ / 2.0) - P.x <= 1.0e-8) && (-1.0e-8 <= voxelGrid[i][j][k].pos.x + (LengthXZ / 2.0) - P.x))
                            {
                                //交点のz座標の確認
                                //if ((voxelGrid[i][j][k].pos.z - (Length / 2.0) <= P.z) && (P.z <= voxelGrid[i][j][k].pos.z + (Length / 2.0)))
                                if ((voxelGrid[i][j][k].pos.z - (LengthXZ / 2.0) - P.z <= 1.0e-8) && (-1.0e-8 <= voxelGrid[i][j][k].pos.z + (LengthXZ / 2.0) - P.z))
                                {
                                    voxelGrid[i][j][k].cross = true;
                                    voxelGrid[i][j][k].surface = true;
                                    cross = true;
                                    break;
                                }
                            }
                        }
                        if (cross) continue;

                        //辺とボクセルのxy平面2面との交差
                        for (int k0 = 0; k0 < 2; ++k0)
                        {
                            //ボクセルのxy平面のz座標
                            P.z = voxelGrid[i][j][k].pos.z + (float)(2 * k0 - 1) * (LengthXZ / 2.0f);

                            //辺の直線のパラメーター
                            t = (P.z - P0.z) / (P1.z - P0.z);

                            //tは線分P0-P1内(0.0<=t<=1.0)
                            if ((t < 0.0) || (1.0 < t)) continue;

                            //辺の直線とボクセルのxy平面の交点
                            P = (P1 - P0) * t + P0;

                            //交点のx座標の確認
                            //if ((voxelGrid[i][j][k].pos.x - (Length / 2.0) <= P.x) && (P.x <= voxelGrid[i][j][k].pos.x + (Length / 2.0)))
                            if ((voxelGrid[i][j][k].pos.x - (LengthXZ / 2.0) - P.x <= 1.0e-8) && (-1.0e-8 <= voxelGrid[i][j][k].pos.x + (LengthXZ / 2.0) - P.x))
                            {
                                //交点のy座標の確認
                                //if ((voxelGrid[i][j][k].pos.y - (Length / 2.0) <= P.y) && (P.y <= voxelGrid[i][j][k].pos.y + (Length / 2.0)))
                                if ((voxelGrid[i][j][k].pos.y - (LengthY / 2.0) - P.y <= 1.0e-8) && (-1.0e-8 <= voxelGrid[i][j][k].pos.y + (LengthY / 2.0) - P.y))
                                {
                                    voxelGrid[i][j][k].cross = true;
                                    voxelGrid[i][j][k].surface = true;
                                    cross = true;
                                    break;
                                }
                            }
                        }
                        if (cross) continue;
                    }
                }
            }
        }

    }
    //メッシュ面を含むボクセルの判定
    public void CrossFace(Solid m)
    {
        //計算中のHalfEdge
        HalfEdge temp;

        //各三角形のバウンディングボックス
        Vector3 small, large;

        //バウンディングボックスの余白
        Vector3 unitVox = new Vector3(LengthXZ / 2.0f, LengthY / 2.0f, LengthXZ / 2.0f);

        Vector3 P0, P1, P2;
        float seki01, seki12, seki20;

        Vector3 P;
        // float area01, area12, area20;
        float area, area012;

        bool cross = false;//このボクセルが交差しているなら

        float t3, t4;//三角形の辺の直線の式のパラメーター
        Vector3 P3, P4;//ボクセルの面と三角形の辺の直線との交点

        Vector3 P34;//三角形の面とボクセルのいずれかの面の交線上の点
        float t34;//三角形の面とボクセルのいずれかの面の交線の式のパラメーター
        float halfDiagonal;

        float cosAlpha;
        Vector3 g0, g1, g2, v1, v2, v3;
        float g0g1Mag;
        float f1, f2, f3;

        float xzDia;
        xzDia = Mathf.Sqrt(LengthXZ * LengthXZ * 2f);
        halfDiagonal = Mathf.Sqrt(xzDia * xzDia + LengthY * LengthY) / 2f;



        int count = 0;
        //各三角形について調べる
        foreach (var f in m.sfaces)
        {
            f.colorNum = count;
            ++count;

            temp = f.fedges;
            small = temp.hvert.vr;
            large = temp.hvert.vr;

            //各三角形のバウンディングボックスを調べる
            for (int i = 0; i < f.sidednum - 1; ++i)
            {
                temp = temp.next;
                if (small.x > temp.hvert.vr.x) small.x = temp.hvert.vr.x;
                if (large.x < temp.hvert.vr.x) large.x = temp.hvert.vr.x;

                if (small.y > temp.hvert.vr.y) small.y = temp.hvert.vr.y;
                if (large.y < temp.hvert.vr.y) large.y = temp.hvert.vr.y;

                if (small.z > temp.hvert.vr.z) small.z = temp.hvert.vr.z;
                if (large.z < temp.hvert.vr.z) large.z = temp.hvert.vr.z;
            }

            small -= unitVox;
            large += unitVox;

            //各三角形のバウンディングボックスと各ボクセルとの交差判定
            for (int i = 0; i < boxSize[0]; ++i)
            {
                //各三角形のバウンディングボックスのx方向による判定
                if ((voxelGrid[i][0][0].pos.x < small.x) || (large.x < voxelGrid[i][0][0].pos.x)) continue;

                for (int j = 0; j < boxSize[1]; ++j)
                {
                    //各三角形のバウンディングボックスのy方向による判定
                    if ((voxelGrid[0][j][0].pos.y < small.y) || (large.y < voxelGrid[0][j][0].pos.y)) continue;

                    for (int k = 0; k < boxSize[2]; ++k)
                    {
                        //各三角形のバウンディングボックスのz方向による判定
                        if ((voxelGrid[0][0][k].pos.z < small.z) || (large.z < voxelGrid[0][0][k].pos.z)) continue;

                        if (voxelGrid[i][j][k].cross) continue;

                        //三角形の面がボクセルのいずれかの面と交差
                        cross = false;

                        P0 = f.fedges.hvert.vr;
                        P1 = f.fedges.next.hvert.vr;
                        P2 = f.fedges.next.next.hvert.vr;
                        g0 = voxelGrid[i][j][k].pos;

                        //Journal of Chemical and Pharmaceutical Research, 2013, 5(9):420-427 Reading and voxelization of 3D modelsより，この距離以下のボクセルは省く

                        //f.CalNormal();
                        //cosAlpha = Vector3.Dot(g0 - P0, f.normal) / (g0 - P0).magnitude / f.normal.magnitude;
                        //g0g1Mag = (P0 - g0).magnitude * cosAlpha;
                        //g1 = g0 - g0g1Mag * Vector3.Normalize(f.normal);

                        //v1 = Vector3.Normalize(P0 - P1) + Vector3.Normalize(P0 - P2);
                        //v2 = Vector3.Normalize(P1 - P2) + Vector3.Normalize(P1 - P0);
                        //v3 = Vector3.Normalize(P0 - P1) + Vector3.Normalize(P0 - P2);

                        //f1 = Vector3.Dot(Vector3.Cross(v1, (g1 - P0)), f.normal);
                        //f2 = Vector3.Dot(Vector3.Cross(v2, (g1 - P1)), f.normal);
                        //f3 = Vector3.Dot(Vector3.Cross(v3, (g1 - P2)), f.normal);

                        //float distance=0;//三角形メッシュの面からボクセルの中心の距離

                        //if (f1 > 0 && f2 < 0)//線上にあった場合どうすればいいのか
                        //{
                        //    float decideOut;//これが外側の時にマイナス
                        //    decideOut = Vector3.Dot(Vector3.Cross((P0 - g1), (P1 - g1)), f.normal);
                        //    if (decideOut > 0)
                        //    {
                        //        distance = g0g1Mag;
                        //        // voxelGrid[i][j][k].disTri=
                        //    }
                        //    else
                        //    {
                        //        Vector3 r = new Vector3();
                        //        r = Vector3.Cross(Vector3.Cross(P1 - g1, P0 - g1), P1 - P0);
                        //        float cosGanma = Vector3.Dot((P0 - g1).normalized, r.normalized);
                        //        float g1g2Mag = (P0 - g1).magnitude * cosGanma;
                        //        Vector3 g1g2 = new Vector3();
                        //        g1g2 = g1g2Mag * r.normalized;
                        //        g2 = g1 + g1g2;
                        //        float t;
                        //        //辺に近いのか頂点に近いのかの判定
                        //        if ((P1.x - P0.x) != 0)//これでいいのか
                        //        {
                        //            t = (g2.x - P0.x) / (P1.x - P0.x);

                        //        }
                        //        else
                        //        {
                        //            t = (g2.y - P0.y) / (P1.y - P0.y);

                        //        }
                        //        if (0 <= t && t <= 1)
                        //        {
                        //            distance = Mathf.Sqrt((g2 - g1).sqrMagnitude + (g1 - g0).sqrMagnitude);

                        //        }
                        //        else if (t < 0)
                        //        {
                        //            distance = (P0 - g0).magnitude;
                        //        }
                        //        else
                        //        {
                        //            distance = (P1 - g0).magnitude;
                        //        }
                        //    }

                        //}
                        //if (f2 > 0 && f3 < 0)//線上にあった場合どうすればいいのか
                        //{
                        //    float decideOut;//これが外側の時にマイナス
                        //    decideOut = Vector3.Dot(Vector3.Cross((P1 - g1), (P2 - g1)), f.normal);
                        //    if (decideOut > 0)
                        //    {
                        //        distance = g0g1Mag;
                        //        // voxelGrid[i][j][k].disTri=
                        //    }
                        //    else
                        //    {
                        //        Vector3 r = new Vector3();
                        //        r = Vector3.Cross(Vector3.Cross(P2 - g1, P1 - g1), P2 - P1);
                        //        float cosGanma = Vector3.Dot((P1 - g1).normalized, r.normalized);
                        //        float g1g2Mag = (P1 - g1).magnitude * cosGanma;
                        //        Vector3 g1g2 = new Vector3();
                        //        g1g2 = g1g2Mag * r.normalized;
                        //        g2 = g1 + g1g2;
                        //        float t;
                        //        //辺に近いのか頂点に近いのかの判定
                        //        if ((P2.x - P1.x) != 0)//これでいいのか
                        //        {
                        //            t = (g2.x - P1.x) / (P2.x - P1.x);

                        //        }
                        //        else
                        //        {
                        //            t = (g2.y - P1.y) / (P2.y - P1.y);

                        //        }
                        //        if (0 <= t && t <= 1)
                        //        {
                        //            distance = Mathf.Sqrt((g2 - g1).sqrMagnitude + (g1 - g0).sqrMagnitude);

                        //        }
                        //        else if (t < 0)
                        //        {
                        //            distance = (P1 - g0).magnitude;
                        //        }
                        //        else
                        //        {
                        //            distance = (P2 - g0).magnitude;
                        //        }
                        //    }

                        //}
                        //if (f3 > 0 && f1 < 0)//線上にあった場合どうすればいいのか
                        //{
                        //    float decideOut;//これが外側の時にマイナス
                        //    decideOut = Vector3.Dot(Vector3.Cross((P2 - g1), (P0 - g1)), f.normal);
                        //    if (decideOut > 0)
                        //    {
                        //        distance = g0g1Mag;
                        //        // voxelGrid[i][j][k].disTri=
                        //    }
                        //    else
                        //    {
                        //        Vector3 r = new Vector3();
                        //        r = Vector3.Cross(Vector3.Cross(P0 - g1, P2 - g1), P0 - P2);
                        //        float cosGanma = Vector3.Dot((P2 - g1).normalized, r.normalized);
                        //        float g1g2Mag = (P2 - g1).magnitude * cosGanma;
                        //        Vector3 g1g2 = new Vector3();
                        //        g1g2 = g1g2Mag * r.normalized;
                        //        g2 = g1 + g1g2;
                        //        float t;
                        //        //辺に近いのか頂点に近いのかの判定
                        //        if ((P0.x - P2.x) != 0)//これでいいのか
                        //        {
                        //            t = (g2.x - P2.x) / (P0.x - P2.x);

                        //        }
                        //        else
                        //        {
                        //            t = (g2.y - P2.y) / (P0.y - P2.y);

                        //        }
                        //        if (0 <= t && t <= 1)
                        //        {
                        //            distance = Mathf.Sqrt((g2 - g1).sqrMagnitude + (g1 - g0).sqrMagnitude);

                        //        }
                        //        else if (t < 0)
                        //        {
                        //            distance = (P2 - g0).magnitude;
                        //        }
                        //        else
                        //        {
                        //            distance = (P0 - g0).magnitude;
                        //        }
                        //    }

                        //}

                        //if (distance>halfDiagonal) continue;



                        //三角形の面とボクセルのyz平面2面との交差
                        for (int i0 = 0; i0 < 2; ++i0)
                        {
                            P3.x = voxelGrid[i][j][k].pos.x + (2 * i0 - 1) * (LengthXZ / 2.0f);

                            //三角形の面とボクセルのyz面が一致する場合
                            //if ((P0.x == P3.x) && (P1.x == P3.x) && (P2.x == P3.x))
                            if ((Math.Abs(P0.x - P3.x) < 1.0e-8) && (Math.Abs(P1.x - P3.x) < 1.0e-8) && (Math.Abs(P2.x - P3.x) < 1.0e-8))
                            {
                                area = (Vector3.Cross((P0 - P1), (P0 - P2))).magnitude / 2.0f;
                                // float a=Vector3.(P0)
                                area012 = 0.0f;
                                P.x = P3.x;
                                for (int j1 = 0; j1 < 2; ++j1)
                                {
                                    P.y = voxelGrid[i][j][k].pos.y + (float)(2 * j1 - 1) * (LengthY / 2.0f);
                                    for (int k1 = 0; k1 < 2; ++k1)
                                    {
                                        P.z = voxelGrid[i][j][k].pos.z + (float)(2 * k1 - 1) * (LengthXZ / 2.0f);
                                        area012 += Vector3.Cross((P0 - P), (P1 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P1 - P), (P2 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P2 - P), (P0 - P)).magnitude / 2.0f;
                                        //if (area == area012)
                                        if (Math.Abs(area - area012) < 1.0e-8)
                                        {
                                            voxelGrid[i][j][k].cross = true;
                                            voxelGrid[i][j][k].surface = true;
                                            cross = true;
                                            break;
                                        }
                                    }
                                    if (cross) break;
                                }
                                if (cross) break;
                            }

                            //三角形の面とボクセルのyz面が一致しない場合
                            else
                            {
                                seki01 = (P0.x - P3.x) * (P1.x - P3.x);
                                seki12 = (P1.x - P3.x) * (P2.x - P3.x);
                                seki20 = (P2.x - P3.x) * (P0.x - P3.x);
                                P4.x = P3.x;

                                //P2.xのみ異符号
                                if ((seki01 > 0.0) && (seki12 < 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.x - P0.x) / (P2.x - P0.x);
                                    t4 = (P4.x - P1.x) / (P2.x - P1.x);
                                    P3 = (P2 - P0) * t3 + P0;
                                    P4 = (P2 - P1) * t4 + P1;
                                }
                                //P0.xのみ異符号
                                else if ((seki01 < 0.0) && (seki12 > 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.x - P1.x) / (P0.x - P1.x);
                                    t4 = (P4.x - P2.x) / (P0.x - P2.x);
                                    P3 = (P0 - P1) * t3 + P1;
                                    P4 = (P0 - P2) * t4 + P2;
                                }
                                //P1.xのみ異符号
                                else if ((seki01 < 0.0) && (seki12 < 0.0) && (seki20 > 0.0))
                                {
                                    t3 = (P3.x - P0.x) / (P1.x - P0.x);
                                    t4 = (P4.x - P2.x) / (P1.x - P2.x);
                                    P3 = (P1 - P0) * t3 + P0;
                                    P4 = (P1 - P2) * t4 + P2;
                                }
                                else
                                {
                                    continue;
                                }
                                //t3は線分内(0.0<=t3<=1.0)
                                //if ((t3 < 0.0) || (1.0 < t3))
                                //{
                                //cout << "!" << endl;
                                //	continue;
                                //}
                                //t4は線分内(0.0<=t4<=1.0)
                                //if ((t4 < 0.0) || (1.0 < t4))
                                //{
                                //cout << "!!" << endl;
                                //	continue;
                                //}
                                //三角形とボクセルのyz平面の交線とボクセルの辺y方向が交差するかどうか調査
                                for (int j0 = 0; j0 < 2; ++j0)
                                {
                                    P34.y = voxelGrid[i][j][k].pos.y + (2 * j0 - 1) * (LengthY / 2.0f);
                                    t34 = (P34.y - P3.y) / (P4.y - P3.y);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34))
                                    {
                                        //cout << "!" << endl;
                                        continue;
                                    }
                                    P34.z = (P4.z - P3.z) * t34 + P3.z;
                                    if ((voxelGrid[i][j][k].pos.z - (LengthXZ / 2.0) <= P34.z) && (P34.z <= voxelGrid[i][j][k].pos.z + (LengthXZ / 2.0)))
                                    {
                                        voxelGrid[i][j][k].cross = true;
                                        voxelGrid[i][j][k].surface = true;
                                        cross = true;
                                        break;
                                    }
                                }
                                if (cross) break;
                                //三角形とボクセルのyz平面の交線とボクセルの辺z方向が交差するかどうか調査
                                for (int k0 = 0; k0 < 2; ++k0)
                                {
                                    P34.z = voxelGrid[i][j][k].pos.z + (2 * k0 - 1) * (LengthXZ / 2.0f);
                                    t34 = (P34.z - P3.z) / (P4.z - P3.z);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    P34.y = (P4.y - P3.y) * t34 + P3.y;
                                    if ((voxelGrid[i][j][k].pos.y - (LengthY / 2.0) <= P34.y) && (P34.y <= voxelGrid[i][j][k].pos.y + (LengthY / 2.0)))
                                    {
                                        voxelGrid[i][j][k].cross = true;
                                        voxelGrid[i][j][k].surface = true;
                                        cross = true;
                                        break;
                                    }
                                }
                                if (cross) break;
                            }
                        }
                        if (cross) continue;

                        //三角形の面とボクセルのxz平面2面との交差
                        for (int j0 = 0; j0 < 2; ++j0)
                        {
                            P3.y = voxelGrid[i][j][k].pos.y + (2 * j0 - 1) * (LengthY / 2.0f);

                            //三角形の面とボクセルのxz面が一致する場合
                            //if ((P0.y == P3.y) && (P1.y == P3.y) && (P2.y == P3.y))
                            if ((Math.Abs(P0.y - P3.y) < 1.0e-8) && (Math.Abs(P1.y - P3.y) < 1.0e-8) && (Math.Abs(P2.y - P3.y) < 1.0e-8))
                            {
                                area = Vector3.Cross((P0 - P1), (P0 - P2)).magnitude / 2.0f;
                                area012 = 0.0f;
                                P.y = P3.y;
                                for (int i1 = 0; i1 < 2; ++i1)
                                {
                                    P.x = voxelGrid[i][j][k].pos.x + (float)(2 * i1 - 1) * (LengthXZ / 2.0f);
                                    for (int k1 = 0; k1 < 2; ++k1)
                                    {
                                        P.z = voxelGrid[i][j][k].pos.z + (float)(2 * k1 - 1) * (LengthXZ / 2.0f);
                                        area012 += Vector3.Cross((P0 - P), (P1 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P1 - P), (P2 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P2 - P), (P0 - P)).magnitude / 2.0f;
                                        //if (area == area012)
                                        if (Math.Abs(area - area012) < 1.0e-8)
                                        {
                                            voxelGrid[i][j][k].cross = true;
                                            voxelGrid[i][j][k].surface = true;
                                            cross = true;
                                            break;
                                        }
                                    }
                                    if (cross) break;
                                }
                                if (cross) break;
                            }

                            //三角形の面とボクセルのxz面が一致しない場合
                            else
                            {
                                seki01 = (P0.y - P3.y) * (P1.y - P3.y);
                                seki12 = (P1.y - P3.y) * (P2.y - P3.y);
                                seki20 = (P2.y - P3.y) * (P0.y - P3.y);
                                P4.y = P3.y;

                                //P2.yのみ異符号
                                if ((seki01 > 0.0) && (seki12 < 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.y - P0.y) / (P2.y - P0.y);
                                    t4 = (P4.y - P1.y) / (P2.y - P1.y);
                                    P3 = (P2 - P0) * t3 + P0;
                                    P4 = (P2 - P1) * t4 + P1;
                                }
                                //P0.yのみ異符号
                                else if ((seki01 < 0.0) && (seki12 > 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.y - P1.y) / (P0.y - P1.y);
                                    t4 = (P4.y - P2.y) / (P0.y - P2.y);
                                    P3 = (P0 - P1) * t3 + P1;
                                    P4 = (P0 - P2) * t4 + P2;
                                }
                                //P1.yのみ異符号
                                else if ((seki01 < 0.0) && (seki12 < 0.0) && (seki20 > 0.0))
                                {
                                    t3 = (P3.y - P0.y) / (P1.y - P0.y);
                                    t4 = (P4.y - P2.y) / (P1.y - P2.y);
                                    P3 = (P1 - P0) * t3 + P0;
                                    P4 = (P1 - P2) * t4 + P2;
                                }
                                else
                                {
                                    continue;
                                }
                                //t3は線分内(0.0<=t3<=1.0)
                                if ((t3 < 0.0) || (1.0 < t3)) continue;
                                //t4は線分内(0.0<=t4<=1.0)
                                if ((t4 < 0.0) || (1.0 < t4)) continue;
                                //三角形とボクセルのxz平面の交線とボクセルの辺x方向が交差するかどうか調査
                                for (int i0 = 0; i0 < 2; ++i0)
                                {
                                    P34.x = voxelGrid[i][j][k].pos.x + (2 * i0 - 1) * (LengthXZ / 2.0f);
                                    t34 = (P34.x - P3.x) / (P4.x - P3.x);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    P34 = (P4 - P3) * t34 + P3;
                                    if ((voxelGrid[i][j][k].pos.z - (LengthXZ / 2.0) <= P34.z) && (P34.z <= voxelGrid[i][j][k].pos.z + (LengthXZ / 2.0)))
                                    {
                                        voxelGrid[i][j][k].cross = true;
                                        voxelGrid[i][j][k].surface = true;
                                        cross = true;
                                        break;
                                    }
                                }
                                if (cross) break;
                                //三角形とボクセルのxz平面の交線とボクセルの辺z方向が交差するかどうか調査
                                for (int k0 = 0; k0 < 2; ++k0)
                                {
                                    P34.z = voxelGrid[i][j][k].pos.z + (2 * k0 - 1) * (LengthXZ / 2.0f);
                                    t34 = (P34.z - P3.z) / (P4.z - P3.z);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    P34 = (P4 - P3) * t34 + P3;
                                    if ((voxelGrid[i][j][k].pos.x - (LengthXZ / 2.0) <= P34.x) && (P34.x <= voxelGrid[i][j][k].pos.x + (LengthXZ / 2.0)))
                                    {
                                        voxelGrid[i][j][k].cross = true;
                                        voxelGrid[i][j][k].surface = true;
                                        cross = true;
                                        break;
                                    }
                                }
                                if (cross) break;
                            }
                        }
                        if (cross) continue;

                        //三角形の面とボクセルのxy平面2面との交差
                        for (int k0 = 0; k0 < 2; ++k0)
                        {
                            P3.z = voxelGrid[i][j][k].pos.z + (2 * k0 - 1) * (LengthXZ / 2.0f);

                            //三角形の面とボクセルのxy面が一致する場合
                            //if ((P0.z == P3.z) && (P1.z == P3.z) && (P2.z == P3.z))
                            if ((Math.Abs(P0.z - P3.z) < 1.0e-8) && (Math.Abs(P1.z - P3.z) < 1.0e-8) && (Math.Abs(P2.z - P3.z) < 1.0e-8))
                            {
                                area = Vector3.Cross((P0 - P1), (P0 - P2)).magnitude / 2.0f;
                                area012 = 0.0f;
                                P.z = P3.z;
                                for (int i1 = 0; i1 < 2; ++i1)
                                {
                                    P.x = voxelGrid[i][j][k].pos.x + (float)(2 * i1 - 1) * (LengthXZ / 2.0f);
                                    for (int j1 = 0; j1 < 2; ++j1)
                                    {
                                        P.y = voxelGrid[i][j][k].pos.y + (float)(2 * j1 - 1) * (LengthY / 2.0f);
                                        area012 += Vector3.Cross((P0 - P), (P1 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P1 - P), (P2 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P2 - P), (P0 - P)).magnitude / 2.0f;
                                        //if (area == area012)
                                        if (Math.Abs(area - area012) < 1.0e-8)
                                        {
                                            voxelGrid[i][j][k].cross = true;
                                            voxelGrid[i][j][k].surface = true;
                                            cross = true;
                                            break;
                                        }
                                    }
                                    if (cross) break;
                                }
                                if (cross) break;
                            }

                            //三角形の面とボクセルのxy面が一致しない場合
                            else
                            {
                                seki01 = (P0.z - P3.z) * (P1.z - P3.z);
                                seki12 = (P1.z - P3.z) * (P2.z - P3.z);
                                seki20 = (P2.z - P3.z) * (P0.z - P3.z);
                                P4.z = P3.z;

                                //P2.zのみ異符号
                                if ((seki01 > 0.0) && (seki12 < 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.z - P0.z) / (P2.z - P0.z);
                                    t4 = (P4.z - P1.z) / (P2.z - P1.z);
                                    P3 = (P2 - P0) * t3 + P0;
                                    P4 = (P2 - P1) * t4 + P1;
                                }
                                //P0.zのみ異符号
                                else if ((seki01 < 0.0) && (seki12 > 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.z - P1.z) / (P0.z - P1.z);
                                    t4 = (P4.z - P2.z) / (P0.z - P2.z);
                                    P3 = (P0 - P1) * t3 + P1;
                                    P4 = (P0 - P2) * t4 + P2;
                                }
                                //P1.zのみ異符号
                                else if ((seki01 < 0.0) && (seki12 < 0.0) && (seki20 > 0.0))
                                {
                                    t3 = (P3.z - P0.z) / (P1.z - P0.z);
                                    t4 = (P4.z - P2.z) / (P1.z - P2.z);
                                    P3 = (P1 - P0) * t3 + P0;
                                    P4 = (P1 - P2) * t4 + P2;
                                }
                                else
                                {
                                    continue;
                                }
                                //t3は線分内(0.0<=t3<=1.0)
                                if ((t3 < 0.0) || (1.0 < t3)) continue;
                                //t4は線分内(0.0<=t4<=1.0)
                                if ((t4 < 0.0) || (1.0 < t4)) continue;
                                //三角形とボクセルのxy平面の交線とボクセルの辺x方向が交差するかどうか調査
                                for (int i0 = 0; i0 < 2; ++i0)
                                {
                                    P34.x = voxelGrid[i][j][k].pos.x + (2 * i0 - 1) * (LengthXZ / 2.0f);
                                    t34 = (P34.x - P3.x) / (P4.x - P3.x);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    P34 = (P4 - P3) * t34 + P3;
                                    if ((voxelGrid[i][j][k].pos.y - (LengthY / 2.0) <= P34.y) && (P34.y <= voxelGrid[i][j][k].pos.y + (LengthY / 2.0)))
                                    {
                                        voxelGrid[i][j][k].cross = true;
                                        voxelGrid[i][j][k].surface = true;
                                        cross = true;
                                        break;
                                    }
                                }
                                if (cross) break;
                                //三角形とボクセルのxy平面の交線とボクセルの辺y方向が交差するかどうか調査
                                for (int j0 = 0; j0 < 2; ++j0)
                                {
                                    P34.y = voxelGrid[i][j][k].pos.y + (2 * j0 - 1) * (LengthY / 2.0f);
                                    t34 = (P34.y - P3.y) / (P4.y - P3.y);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    //P34.x = (P4.x - P3.x)*t34 + P3.x;
                                    P34 = (P4 - P3) * t34 + P3;
                                    if ((voxelGrid[i][j][k].pos.x - (LengthXZ / 2.0) <= P34.x) && (P34.x <= voxelGrid[i][j][k].pos.x + (LengthXZ / 2.0)))
                                    {
                                        voxelGrid[i][j][k].cross = true;
                                        voxelGrid[i][j][k].surface = true;
                                        cross = true;
                                        break;
                                    }
                                }
                                if (cross) break;
                            }
                        }
                        if (cross) continue;

                    }
                }
            }
        }
    }

    //メッシュ面を含むボクセルの判定
    public void CrossFaceDistance(Solid m)
    {
        //計算中のHalfEdge
        HalfEdge temp;

        //各三角形のバウンディングボックス
        Vector3 small, large;

        //バウンディングボックスの余白
        Vector3 unitVox = new Vector3(LengthXZ / 2.0f, LengthY / 2.0f, LengthXZ / 2.0f);

        Vector3 P0, P1, P2;
        float seki01, seki12, seki20;

        Vector3 P;
        // float area01, area12, area20;
        float area, area012;

        bool cross = false;//このボクセルが交差しているなら

        float t3, t4;//三角形の辺の直線の式のパラメーター
        Vector3 P3, P4;//ボクセルの面と三角形の辺の直線との交点

        Vector3 P34;//三角形の面とボクセルのいずれかの面の交線上の点
        float t34;//三角形の面とボクセルのいずれかの面の交線の式のパラメーター
        float halfDiagonal;

        float cosAlpha;
        //Vector3 g0, g1, g2, v1, v2, v3;
        float g0g1Mag;
        float f1, f2, f3;

        float xzDiaSquare;
        xzDiaSquare = LengthXZ * LengthXZ;
        //xzDia = Mathf.Sqrt(LengthXZ * LengthXZ * 2f);
        halfDiagonal = Mathf.Sqrt(xzDiaSquare + LengthY * LengthY) / 2f;



        int count = 0;
        //各三角形について調べる
        foreach (var f in m.sfaces)
        {
            f.colorNum = count;
            ++count;

            temp = f.fedges;
            small = temp.hvert.vr;
            large = temp.hvert.vr;

            //各三角形のバウンディングボックスを調べる
            for (int i = 0; i < f.sidednum - 1; ++i)
            {
                temp = temp.next;
                if (small.x > temp.hvert.vr.x) small.x = temp.hvert.vr.x;
                if (large.x < temp.hvert.vr.x) large.x = temp.hvert.vr.x;

                if (small.y > temp.hvert.vr.y) small.y = temp.hvert.vr.y;
                if (large.y < temp.hvert.vr.y) large.y = temp.hvert.vr.y;

                if (small.z > temp.hvert.vr.z) small.z = temp.hvert.vr.z;
                if (large.z < temp.hvert.vr.z) large.z = temp.hvert.vr.z;
            }

            small -= unitVox;
            large += unitVox;

            //各三角形のバウンディングボックスと各ボクセルとの交差判定
            for (int i = 0; i < boxSize[0]; ++i)
            {
                //各三角形のバウンディングボックスのx方向による判定
                if ((voxelGrid[i][0][0].pos.x < small.x) || (large.x < voxelGrid[i][0][0].pos.x)) continue;

                for (int j = 0; j < boxSize[1]; ++j)
                {
                    //各三角形のバウンディングボックスのy方向による判定
                    if ((voxelGrid[0][j][0].pos.y < small.y) || (large.y < voxelGrid[0][j][0].pos.y)) continue;

                    for (int k = 0; k < boxSize[2]; ++k)
                    {
                        //各三角形のバウンディングボックスのz方向による判定
                        if ((voxelGrid[0][0][k].pos.z < small.z) || (large.z < voxelGrid[0][0][k].pos.z)) continue;

                        if (voxelGrid[i][j][k].cross) continue;

                        //三角形の面がボクセルのいずれかの面と交差
                        cross = false;
                        P0 = f.fedges.hvert.vr;
                        P1 = f.fedges.next.hvert.vr;
                        P2 = f.fedges.next.next.hvert.vr;

                        //Journal of Chemical and Pharmaceutical Research, 2013, 5(9):420-427 Reading and voxelization of 3D modelsより，この距離以下のボクセルは省く
                        Vector3 g0, g1, g2, v1, v2, v3;
                        g0 = new Vector3();
                        g1 = new Vector3();
                        g2 = new Vector3();
                        v1 = new Vector3();
                        v2 = new Vector3();
                        v3 = new Vector3();



                        g0 = voxelGrid[i][j][k].pos;

                        f.CalNormal();
                        cosAlpha = Vector3.Dot((g0 - P0).normalized, f.normal.normalized);
                        //cosAlpha = Vector3.Dot(g0 - P0, f.normal) / (g0 - P0).magnitude / f.normal.magnitude;
                        g0g1Mag = (P0 - g0).magnitude * cosAlpha;//ここ違うのではないか
                        g1 = g0 - g0g1Mag * Vector3.Normalize(f.normal);//プラスマイナス逆にしても意味ないぞ？
                        //g1 = g0 - g0g1Mag * Vector3.Normalize(f.normal);

                        v1 = Vector3.Normalize(P0 - P1) + Vector3.Normalize(P0 - P2);
                        v2 = Vector3.Normalize(P1 - P2) + Vector3.Normalize(P1 - P0);
                        v3 = Vector3.Normalize(P2 - P0) + Vector3.Normalize(P2 - P1);
                        //これが正なら反時計回り
                        f1 = Vector3.Dot(Vector3.Cross(v1, (g1 - P0)), f.normal);
                        f2 = Vector3.Dot(Vector3.Cross(v2, (g1 - P1)), f.normal);
                        f3 = Vector3.Dot(Vector3.Cross(v3, (g1 - P2)), f.normal);

                        float distance = 0;//三角形メッシュの面からボクセルの中心の距離

                        if (f1 > 0 && f2 < 0)//線上にあった場合どうすればいいのか
                        {
                            float decideOut;//これが外側の時にマイナス
                            decideOut = Vector3.Dot(Vector3.Cross((P0 - g1), (P1 - g1)), f.normal);//マイナスにしても変わらない
                            if (decideOut > 0)
                            {
                                distance = g0g1Mag;
                                // voxelGrid[i][j][k].disTri=
                            }
                            else
                            {
                                Vector3 r = new Vector3();
                                r = Vector3.Cross(Vector3.Cross(P1 - g1, P0 - g1), P1 - P0);
                                float cosGanma = Vector3.Dot((P0 - g1).normalized, r.normalized);
                                float g1g2Mag = (P0 - g1).magnitude * cosGanma;
                                Vector3 g1g2 = new Vector3();
                                g1g2 = g1g2Mag * r.normalized;
                                g2 = g1 + g1g2;
                                float t;
                                //辺に近いのか頂点に近いのかの判定

                                if ((P1.x - P0.x) != 0)//これでいいのか
                                {
                                    t = (g2.x - P0.x) / (P1.x - P0.x);

                                }
                                else if ((P1.y - P0.y) != 0)
                                {
                                    t = (g2.y - P0.y) / (P1.y - P0.y);

                                }
                                else
                                {
                                    t = (g2.z - P0.z) / (P1.z - P0.z);

                                }
                                if (0 <= t && t <= 1)
                                {
                                    distance = Mathf.Sqrt(Mathf.Pow((g2 - g1).magnitude, 2f) + Mathf.Pow((g1 - g0).magnitude, 2f));
                                    //distance = Mathf.Sqrt((g2 - g1).sqrMagnitude + (g1 - g0).sqrMagnitude);

                                }
                                else if (t < 0)
                                {
                                    //distance = (P1 - g0).magnitude;
                                    distance = (P0 - g0).magnitude;
                                }
                                else
                                {
                                    //distance = (P0 - g0).magnitude;
                                    distance = (P1 - g0).magnitude;
                                }
                            }
                            if (Mathf.Abs(distance) <= halfDiagonal)
                            {
                                voxelGrid[i][j][k].cross = true;
                                voxelGrid[i][j][k].surface = true;
                                // cross = true;
                                // break;

                            }

                        }
                        if (f2 > 0 && f3 < 0)//線上にあった場合どうすればいいのか
                        {
                            float decideOut;//これが外側の時にマイナス
                            decideOut = Vector3.Dot(Vector3.Cross((P1 - g1), (P2 - g1)), f.normal);
                            if (decideOut > 0)
                            {
                                distance = g0g1Mag;
                                // voxelGrid[i][j][k].disTri=
                            }
                            else
                            {
                                Vector3 r = new Vector3();
                                r = Vector3.Cross(Vector3.Cross(P2 - g1, P1 - g1), P2 - P1);
                                float cosGanma = Vector3.Dot((P1 - g1).normalized, r.normalized);
                                float g1g2Mag = (P1 - g1).magnitude * cosGanma;
                                Vector3 g1g2 = new Vector3();
                                g1g2 = g1g2Mag * r.normalized;
                                g2 = g1 + g1g2;
                                float t = 0;
                                //辺に近いのか頂点に近いのかの判定
                                if ((P2.x - P1.x) != 0)//これでいいのか
                                {
                                    t = (g2.x - P1.x) / (P2.x - P1.x);

                                }
                                else if ((P2.y - P1.y) != 0)
                                {
                                    t = (g2.y - P1.y) / (P2.y - P1.y);

                                }
                                //else
                                else if ((P2.z - P1.z) != 0)
                                {
                                    t = (g2.z - P1.z) / (P2.z - P1.z);

                                }
                                if (0 <= t && t <= 1)
                                {
                                    distance = Mathf.Sqrt((g2 - g1).sqrMagnitude + (g1 - g0).sqrMagnitude);

                                }
                                else if (t < 0)
                                {
                                    //distance = (P2 - g0).magnitude;
                                    distance = (P1 - g0).magnitude;
                                }
                                else
                                {
                                    //distance = (P1 - g0).magnitude;
                                    distance = (P2 - g0).magnitude;
                                }
                            }
                            //if (distance <= halfDiagonal)
                            if (Mathf.Abs(distance) <= halfDiagonal)

                            {
                                voxelGrid[i][j][k].cross = true;
                                voxelGrid[i][j][k].surface = true;
                                // cross = true;
                                // break;

                            }
                        }
                        // else
                        if (f3 > 0 && f1 < 0)//線上にあった場合どうすればいいのか
                        {
                            float decideOut;//これが外側の時にマイナス
                            decideOut = Vector3.Dot(Vector3.Cross((P2 - g1), (P0 - g1)), f.normal);
                            if (decideOut > 0)
                            {
                                distance = g0g1Mag;
                                // voxelGrid[i][j][k].disTri=
                            }
                            else
                            {
                                Vector3 r = new Vector3();
                                r = Vector3.Cross(Vector3.Cross(P0 - g1, P2 - g1), P0 - P2);
                                float cosGanma = Vector3.Dot((P2 - g1).normalized, r.normalized);
                                float g1g2Mag = (P2 - g1).magnitude * cosGanma;
                                Vector3 g1g2 = new Vector3();
                                g1g2 = g1g2Mag * r.normalized;
                                g2 = g1 + g1g2;
                                float t;
                                //辺に近いのか頂点に近いのかの判定
                                if ((P0.x - P2.x) != 0)//これでいいのか
                                {
                                    t = (g2.x - P2.x) / (P0.x - P2.x);

                                }
                                else if ((P0.y - P2.y) != 0)
                                {
                                    t = (g2.y - P2.y) / (P0.y - P2.y);

                                }
                                else
                                {
                                    t = (g2.z - P2.z) / (P0.z - P2.z);

                                }
                                if (0 <= t && t <= 1)
                                {
                                    distance = Mathf.Sqrt((g2 - g1).sqrMagnitude + (g1 - g0).sqrMagnitude);

                                }
                                else if (t < 0)
                                {
                                    //distance = (P0 - g0).magnitude;
                                    distance = (P2 - g0).magnitude;
                                }
                                else
                                {
                                    //distance = (P2 - g0).magnitude;
                                    distance = (P0 - g0).magnitude;
                                }
                            }
                            //if (distance <= halfDiagonal)
                            if (Mathf.Abs(distance) <= halfDiagonal)

                            {
                                voxelGrid[i][j][k].cross = true;
                                voxelGrid[i][j][k].surface = true;
                                //  cross = true;
                                // break;

                            }

                        }

                        // if (distance > halfDiagonal)
                        // {
                        // voxelGrid[i][j][k].cross = true;
                        // voxelGrid[i][j][k].surface = true;
                        // cross = true;
                        //// break;

                        // }






                    }
                }
            }
            // if (count >= 2) { break; }
        }
    }
    //メッシュ頂点を含むボクセルの判定,色の判定も含む
    public void PlyCrossVert(Solid m)
    {
        //計算中のHalfEdge
        // HalfEdge temp;

        //各三角形のバウンディングボックス
        Vector3 small, large;

        //バウンディングボックスの余白
        Vector3 unitVox = new Vector3(LengthXZ / 2.0f, LengthY / 2.0f, LengthXZ / 2.0f);

        // Vector3 P0;

        // bool cross = false;//このボクセルが交差しているなら

        //float t = 0.0f;//三角形の辺の直線の式のパラメーター
        //Vector3 P;//ボクセルの面と三角形の辺の直線との交点

        //int c = 0;
        //foreach(var v in m.sverts)
        //{
        //    c++;
        //    Debug.Log(c);

        //}

        //for(int v=0;v<m.numVertex;++v)
        int count = 0;
        foreach (var v in m.sverts)
        {
            small = v.vr;
            large = v.vr;

            v.colorNum = count;
            ++count;
            // Debug.Log(m.sverts[v].vr);



            //    small = m.sverts[v].vr;
            //large = m.sverts[v].vr;


            small -= unitVox;
            large += unitVox;
            //各頂点のバウンディングボックスと各ボクセルとの交差判定
            for (int i = 0; i < boxSize[0]; ++i)
            {
                //各頂点のバウンディングボックスのx方向による判定
                if ((voxelGrid[i][0][0].pos.x < small.x) || (large.x < voxelGrid[i][0][0].pos.x)) continue;

                for (int j = 0; j < boxSize[1]; ++j)
                {
                    //各頂点のバウンディングボックスのy方向による判定
                    if ((voxelGrid[0][j][0].pos.y < small.y) || (large.y < voxelGrid[0][j][0].pos.y)) continue;

                    for (int k = 0; k < boxSize[2]; ++k)
                    {
                        //各頂点のバウンディングボックスのz方向による判定
                        if ((voxelGrid[0][0][k].pos.z < small.z) || (large.z < voxelGrid[0][0][k].pos.z)) continue;

                        voxelGrid[i][j][k].cross = true;
                        voxelGrid[i][j][k].surface = true;

                        voxelGrid[i][j][k].crossedVertsList.Add(v.colorNum);

                        //一番最初の各頂点に含まれている
                        // voxelGrid[i][j][k].voxelColor = m.svertsColor[v];

                        //cross = true;
                        //break;
                    }
                }
            }
        }


    }



    //メッシュ辺を含むボクセルの判定,色の判定も含む

    public void PlyCrossEdge(Solid m)
    {
        //各辺のバウンディングボックス
        Vector3 small, large;

        //バウンディングボックスの余白
        Vector3 unitVox = new Vector3(LengthXZ / 2.0f, LengthY / 2.0f, LengthXZ / 2.0f);

        Vector3 P0, P1;

        //このボクセルが交差しているなら
        bool cross = false;

        float t;//辺の直線の式のパラメーター
        Vector3 P;//ボクセルの面と辺の直線との交点

        //foreach (var e in m.sedges)
        //{
        //    Debug.Log(e.he1.hvert.vr);
        //}

        //各辺について調べる
        int count = 0;
        foreach (var e in m.sedges)
        {
            e.colorNum = count;
            ++count;
            //    for (auto e : m.sedges)
            //{
            //各辺のバウンディングボックスを調べる
            P0 = e.he1.hvert.vr;
            //Debug.Log(e.he1.hvert.vr);
            P1 = e.he2.hvert.vr;

            if (P0.x < P1.x)
            {
                small.x = P0.x;
                large.x = P1.x;
            }
            else
            {
                small.x = P1.x;
                large.x = P0.x;
            }

            if (P0.y < P1.y)
            {
                small.y = P0.y;
                large.y = P1.y;
            }
            else
            {
                small.y = P1.y;
                large.y = P0.y;
            }

            if (P0.z < P1.z)
            {
                small.z = P0.z;
                large.z = P1.z;
            }
            else
            {
                small.z = P1.z;
                large.z = P0.z;
            }

            small -= unitVox;
            large += unitVox;


            //各辺のバウンディングボックスと各ボクセルとの交差判定
            for (int i = 0; i < boxSize[0]; ++i)
            {
                //各辺のバウンディングボックスのx方向による判定
                if ((voxelGrid[i][0][0].pos.x < small.x) || (large.x < voxelGrid[i][0][0].pos.x)) continue;

                for (int j = 0; j < boxSize[1]; ++j)
                {
                    //各辺のバウンディングボックスのy方向による判定
                    if ((voxelGrid[0][j][0].pos.y < small.y) || (large.y < voxelGrid[0][j][0].pos.y)) continue;

                    for (int k = 0; k < boxSize[2]; ++k)
                    {
                        //各辺のバウンディングボックスのz方向による判定
                        if ((voxelGrid[0][0][k].pos.z < small.z) || (large.z < voxelGrid[0][0][k].pos.z)) continue;

                        if (voxelGrid[i][j][k].cross) continue;

                        //辺がボクセルのいずれかの面と交差
                        cross = false;
                        // cross = true;//勝手に変えた


                        P0 = e.he1.hvert.vr;
                        P1 = e.he2.hvert.vr;

                        //辺とボクセルのyz平面2面との交差
                        for (int i0 = 0; i0 < 2; ++i0)
                        {
                            //ボクセルのyz平面のx座標
                            P.x = voxelGrid[i][j][k].pos.x + (float)(2 * i0 - 1) * (LengthXZ / 2.0f);

                            //三角形の辺の直線のパラメーター
                            t = (P.x - P0.x) / (P1.x - P0.x);

                            //tは線分P0-P1内(0.0<=t<=1.0)
                            if ((t < 0.0) || (1.0 < t)) continue;

                            //辺の直線とボクセルのyz平面の交点
                            P = (P1 - P0) * t + P0;

                            //交点のy座標の確認
                            //if ((voxelGrid[i][j][k].pos.y - (Length / 2.0) <= P.y) && (P.y <= voxelGrid[i][j][k].pos.y + (Length / 2.0)))
                            if ((voxelGrid[i][j][k].pos.y - (LengthY / 2.0) - P.y <= 1.0e-8) && (-1.0e-8 <= voxelGrid[i][j][k].pos.y + (LengthY / 2.0) - P.y))
                            {
                                //交点のz座標の確認
                                //if ((voxelGrid[i][j][k].pos.z - (Length / 2.0) <= P.z) && (P.z <= voxelGrid[i][j][k].pos.z + (Length / 2.0)))
                                if ((voxelGrid[i][j][k].pos.z - (LengthXZ / 2.0) - P.z <= 1.0e-8) && (-1.0e-8 <= voxelGrid[i][j][k].pos.z + (LengthXZ / 2.0) - P.z))
                                {
                                    voxelGrid[i][j][k].cross = true;
                                    voxelGrid[i][j][k].surface = true;
                                    voxelGrid[i][j][k].crossedEdgesList.Add(e.colorNum);
                                    //voxelGrid[i][j][k].color = Color.Lerp(e.he1.hvert.color, e.he2.hvert.color,t);
                                    cross = true;
                                    // break;
                                }
                            }
                        }
                        if (cross) continue;    //辺とボクセルのxz平面2面との交差
                        for (int j0 = 0; j0 < 2; ++j0)
                        {
                            //ボクセルのxz平面のy座標
                            P.y = voxelGrid[i][j][k].pos.y + (float)(2 * j0 - 1) * (LengthY / 2.0f);

                            //辺の直線のパラメーター
                            t = (P.y - P0.y) / (P1.y - P0.y);

                            //tは線分P0-P1内(0.0<=t<=1.0)
                            if ((t < 0.0) || (1.0 < t)) continue;

                            //辺の直線とボクセルのxz平面の交点
                            P = (P1 - P0) * t + P0;

                            //交点のx座標の確認
                            //if ((voxelGrid[i][j][k].pos.x - (Length / 2.0) <= P.x) && (P.x <= voxelGrid[i][j][k].pos.x + (Length / 2.0)))
                            if ((voxelGrid[i][j][k].pos.x - (LengthXZ / 2.0) - P.x <= 1.0e-8) && (-1.0e-8 <= voxelGrid[i][j][k].pos.x + (LengthXZ / 2.0) - P.x))
                            {
                                //交点のz座標の確認
                                //if ((voxelGrid[i][j][k].pos.z - (Length / 2.0) <= P.z) && (P.z <= voxelGrid[i][j][k].pos.z + (Length / 2.0)))
                                if ((voxelGrid[i][j][k].pos.z - (LengthXZ / 2.0) - P.z <= 1.0e-8) && (-1.0e-8 <= voxelGrid[i][j][k].pos.z + (LengthXZ / 2.0) - P.z))
                                {
                                    voxelGrid[i][j][k].cross = true;
                                    voxelGrid[i][j][k].surface = true;
                                    voxelGrid[i][j][k].crossedEdgesList.Add(e.colorNum);
                                    // voxelGrid[i][j][k].color = Color.Lerp(e.he1.hvert.color, e.he2.hvert.color, t);
                                    cross = true;
                                    // break;
                                }
                            }
                        }
                        if (cross) continue;

                        //辺とボクセルのxy平面2面との交差
                        for (int k0 = 0; k0 < 2; ++k0)
                        {
                            //ボクセルのxy平面のz座標
                            P.z = voxelGrid[i][j][k].pos.z + (float)(2 * k0 - 1) * (LengthXZ / 2.0f);

                            //辺の直線のパラメーター
                            t = (P.z - P0.z) / (P1.z - P0.z);

                            //tは線分P0-P1内(0.0<=t<=1.0)
                            if ((t < 0.0) || (1.0 < t)) continue;

                            //辺の直線とボクセルのxy平面の交点
                            P = (P1 - P0) * t + P0;

                            //交点のx座標の確認
                            //if ((voxelGrid[i][j][k].pos.x - (Length / 2.0) <= P.x) && (P.x <= voxelGrid[i][j][k].pos.x + (Length / 2.0)))
                            if ((voxelGrid[i][j][k].pos.x - (LengthXZ / 2.0) - P.x <= 1.0e-8) && (-1.0e-8 <= voxelGrid[i][j][k].pos.x + (LengthXZ / 2.0) - P.x))
                            {
                                //交点のy座標の確認
                                //if ((voxelGrid[i][j][k].pos.y - (Length / 2.0) <= P.y) && (P.y <= voxelGrid[i][j][k].pos.y + (Length / 2.0)))
                                if ((voxelGrid[i][j][k].pos.y - (LengthY / 2.0) - P.y <= 1.0e-8) && (-1.0e-8 <= voxelGrid[i][j][k].pos.y + (LengthY / 2.0) - P.y))
                                {
                                    voxelGrid[i][j][k].cross = true;
                                    voxelGrid[i][j][k].surface = true;
                                    voxelGrid[i][j][k].crossedEdgesList.Add(e.colorNum);
                                    // voxelGrid[i][j][k].color = Color.Lerp(e.he1.hvert.color, e.he2.hvert.color, t);
                                    cross = true;
                                    // break;
                                }
                            }
                        }
                        if (cross) continue;
                    }
                }
            }
        }

    }
    //メッシュ面を含むボクセルの判定,色の判定も含む

    public void PlyCrossFace(Solid m)
    {
        //計算中のHalfEdge
        HalfEdge temp;

        //各三角形のバウンディングボックス
        Vector3 small, large;

        //バウンディングボックスの余白
        Vector3 unitVox = new Vector3(LengthXZ / 2.0f, LengthY / 2.0f, LengthXZ / 2.0f);

        Vector3 P0, P1, P2;
        float seki01, seki12, seki20;

        Vector3 P;
        // float area01, area12, area20;
        float area, area012;

        bool cross = false;//このボクセルが交差しているなら

        float t3, t4;//三角形の辺の直線の式のパラメーター
        Vector3 P3, P4;//ボクセルの面と三角形の辺の直線との交点

        Vector3 P34;//三角形の面とボクセルのいずれかの面の交線上の点
        float t34;//三角形の面とボクセルのいずれかの面の交線の式のパラメーター

        int count = 0;
        //各三角形について調べる
        foreach (var f in m.sfaces)
        {
            f.colorNum = count;
            ++count;

            temp = f.fedges;
            small = temp.hvert.vr;
            large = temp.hvert.vr;

            //各三角形のバウンディングボックスを調べる
            for (int i = 0; i < f.sidednum - 1; ++i)
            {
                temp = temp.next;
                if (small.x > temp.hvert.vr.x) small.x = temp.hvert.vr.x;
                if (large.x < temp.hvert.vr.x) large.x = temp.hvert.vr.x;

                if (small.y > temp.hvert.vr.y) small.y = temp.hvert.vr.y;
                if (large.y < temp.hvert.vr.y) large.y = temp.hvert.vr.y;

                if (small.z > temp.hvert.vr.z) small.z = temp.hvert.vr.z;
                if (large.z < temp.hvert.vr.z) large.z = temp.hvert.vr.z;
            }

            small -= unitVox;
            large += unitVox;

            //各三角形のバウンディングボックスと各ボクセルとの交差判定
            for (int i = 0; i < boxSize[0]; ++i)
            {
                //各三角形のバウンディングボックスのx方向による判定
                if ((voxelGrid[i][0][0].pos.x < small.x) || (large.x < voxelGrid[i][0][0].pos.x)) continue;

                for (int j = 0; j < boxSize[1]; ++j)
                {
                    //各三角形のバウンディングボックスのy方向による判定
                    if ((voxelGrid[0][j][0].pos.y < small.y) || (large.y < voxelGrid[0][j][0].pos.y)) continue;

                    for (int k = 0; k < boxSize[2]; ++k)
                    {
                        //各三角形のバウンディングボックスのz方向による判定
                        if ((voxelGrid[0][0][k].pos.z < small.z) || (large.z < voxelGrid[0][0][k].pos.z)) continue;

                        if (voxelGrid[i][j][k].cross) continue;

                        //三角形の面がボクセルのいずれかの面と交差
                        cross = false;

                        P0 = f.fedges.hvert.vr;
                        P1 = f.fedges.next.hvert.vr;
                        P2 = f.fedges.next.next.hvert.vr;

                        //三角形の面とボクセルのyz平面2面との交差
                        for (int i0 = 0; i0 < 2; ++i0)
                        {
                            P3.x = voxelGrid[i][j][k].pos.x + (2 * i0 - 1) * (LengthXZ / 2.0f);

                            //三角形の面とボクセルのyz面が一致する場合
                            //if ((P0.x == P3.x) && (P1.x == P3.x) && (P2.x == P3.x))
                            if ((Math.Abs(P0.x - P3.x) < 1.0e-8) && (Math.Abs(P1.x - P3.x) < 1.0e-8) && (Math.Abs(P2.x - P3.x) < 1.0e-8))
                            {
                                area = (Vector3.Cross((P0 - P1), (P0 - P2))).magnitude / 2.0f;
                                // float a=Vector3.(P0)
                                area012 = 0.0f;
                                P.x = P3.x;
                                for (int j1 = 0; j1 < 2; ++j1)
                                {
                                    P.y = voxelGrid[i][j][k].pos.y + (float)(2 * j1 - 1) * (LengthY / 2.0f);
                                    for (int k1 = 0; k1 < 2; ++k1)
                                    {
                                        P.z = voxelGrid[i][j][k].pos.z + (float)(2 * k1 - 1) * (LengthXZ / 2.0f);
                                        area012 += Vector3.Cross((P0 - P), (P1 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P1 - P), (P2 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P2 - P), (P0 - P)).magnitude / 2.0f;
                                        //if (area == area012)
                                        if (Math.Abs(area - area012) < 1.0e-8)
                                        {
                                            voxelGrid[i][j][k].cross = true;
                                            voxelGrid[i][j][k].surface = true;
                                            voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);
                                            //voxelGrid[i][j][k].color.r = (f.fedges.hvert.color.r + f.fedges.next.hvert.color.r + f.fedges.next.next.hvert.color.r) / 3.0f;
                                            //voxelGrid[i][j][k].color.g = (f.fedges.hvert.color.g + f.fedges.next.hvert.color.g + f.fedges.next.next.hvert.color.g) / 3.0f;
                                            //voxelGrid[i][j][k].color.b = (f.fedges.hvert.color.b + f.fedges.next.hvert.color.b + f.fedges.next.next.hvert.color.b) / 3.0f;

                                            cross = true;
                                            break;
                                        }
                                    }
                                    if (cross) break;
                                }
                                if (cross) break;
                            }

                            //三角形の面とボクセルのyz面が一致しない場合
                            else
                            {
                                seki01 = (P0.x - P3.x) * (P1.x - P3.x);
                                seki12 = (P1.x - P3.x) * (P2.x - P3.x);
                                seki20 = (P2.x - P3.x) * (P0.x - P3.x);
                                P4.x = P3.x;

                                //P2.xのみ異符号
                                if ((seki01 > 0.0) && (seki12 < 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.x - P0.x) / (P2.x - P0.x);
                                    t4 = (P4.x - P1.x) / (P2.x - P1.x);
                                    P3 = (P2 - P0) * t3 + P0;
                                    P4 = (P2 - P1) * t4 + P1;
                                }
                                //P0.xのみ異符号
                                else if ((seki01 < 0.0) && (seki12 > 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.x - P1.x) / (P0.x - P1.x);
                                    t4 = (P4.x - P2.x) / (P0.x - P2.x);
                                    P3 = (P0 - P1) * t3 + P1;
                                    P4 = (P0 - P2) * t4 + P2;
                                }
                                //P1.xのみ異符号
                                else if ((seki01 < 0.0) && (seki12 < 0.0) && (seki20 > 0.0))
                                {
                                    t3 = (P3.x - P0.x) / (P1.x - P0.x);
                                    t4 = (P4.x - P2.x) / (P1.x - P2.x);
                                    P3 = (P1 - P0) * t3 + P0;
                                    P4 = (P1 - P2) * t4 + P2;
                                }
                                else
                                {
                                    continue;
                                }
                                //t3は線分内(0.0<=t3<=1.0)
                                //if ((t3 < 0.0) || (1.0 < t3))
                                //{
                                //cout << "!" << endl;
                                //	continue;
                                //}
                                //t4は線分内(0.0<=t4<=1.0)
                                //if ((t4 < 0.0) || (1.0 < t4))
                                //{
                                //cout << "!!" << endl;
                                //	continue;
                                //}
                                //三角形とボクセルのyz平面の交線とボクセルの辺y方向が交差するかどうか調査
                                for (int j0 = 0; j0 < 2; ++j0)
                                {
                                    P34.y = voxelGrid[i][j][k].pos.y + (2 * j0 - 1) * (LengthY / 2.0f);
                                    t34 = (P34.y - P3.y) / (P4.y - P3.y);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34))
                                    {
                                        //cout << "!" << endl;
                                        continue;
                                    }
                                    P34.z = (P4.z - P3.z) * t34 + P3.z;
                                    if ((voxelGrid[i][j][k].pos.z - (LengthXZ / 2.0) <= P34.z) && (P34.z <= voxelGrid[i][j][k].pos.z + (LengthXZ / 2.0)))
                                    {
                                        voxelGrid[i][j][k].cross = true;
                                        voxelGrid[i][j][k].surface = true;
                                        voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);
                                        //voxelGrid[i][j][k].color.r = (f.fedges.hvert.color.r + f.fedges.next.hvert.color.r + f.fedges.next.next.hvert.color.r) / 3.0f;
                                        //voxelGrid[i][j][k].color.g = (f.fedges.hvert.color.g + f.fedges.next.hvert.color.g + f.fedges.next.next.hvert.color.g) / 3.0f;
                                        //voxelGrid[i][j][k].color.b = (f.fedges.hvert.color.b + f.fedges.next.hvert.color.b + f.fedges.next.next.hvert.color.b) / 3.0f;
                                        cross = true;
                                        break;
                                    }
                                }
                                if (cross) break;
                                //三角形とボクセルのyz平面の交線とボクセルの辺z方向が交差するかどうか調査
                                for (int k0 = 0; k0 < 2; ++k0)
                                {
                                    P34.z = voxelGrid[i][j][k].pos.z + (2 * k0 - 1) * (LengthXZ / 2.0f);
                                    t34 = (P34.z - P3.z) / (P4.z - P3.z);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    P34.y = (P4.y - P3.y) * t34 + P3.y;
                                    if ((voxelGrid[i][j][k].pos.y - (LengthY / 2.0) <= P34.y) && (P34.y <= voxelGrid[i][j][k].pos.y + (LengthY / 2.0)))
                                    {
                                        voxelGrid[i][j][k].cross = true;
                                        voxelGrid[i][j][k].surface = true;
                                        voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);
                                        //voxelGrid[i][j][k].color.r = (f.fedges.hvert.color.r + f.fedges.next.hvert.color.r + f.fedges.next.next.hvert.color.r) / 3.0f;
                                        //voxelGrid[i][j][k].color.g = (f.fedges.hvert.color.g + f.fedges.next.hvert.color.g + f.fedges.next.next.hvert.color.g) / 3.0f;
                                        //voxelGrid[i][j][k].color.b = (f.fedges.hvert.color.b + f.fedges.next.hvert.color.b + f.fedges.next.next.hvert.color.b) / 3.0f;
                                        cross = true;
                                        break;
                                    }
                                }
                                if (cross) break;
                            }
                        }
                        if (cross) continue;

                        //三角形の面とボクセルのxz平面2面との交差
                        for (int j0 = 0; j0 < 2; ++j0)
                        {
                            P3.y = voxelGrid[i][j][k].pos.y + (2 * j0 - 1) * (LengthY / 2.0f);

                            //三角形の面とボクセルのxz面が一致する場合
                            //if ((P0.y == P3.y) && (P1.y == P3.y) && (P2.y == P3.y))
                            if ((Math.Abs(P0.y - P3.y) < 1.0e-8) && (Math.Abs(P1.y - P3.y) < 1.0e-8) && (Math.Abs(P2.y - P3.y) < 1.0e-8))
                            {
                                area = Vector3.Cross((P0 - P1), (P0 - P2)).magnitude / 2.0f;
                                area012 = 0.0f;
                                P.y = P3.y;
                                for (int i1 = 0; i1 < 2; ++i1)
                                {
                                    P.x = voxelGrid[i][j][k].pos.x + (float)(2 * i1 - 1) * (LengthXZ / 2.0f);
                                    for (int k1 = 0; k1 < 2; ++k1)
                                    {
                                        P.z = voxelGrid[i][j][k].pos.z + (float)(2 * k1 - 1) * (LengthXZ / 2.0f);
                                        area012 += Vector3.Cross((P0 - P), (P1 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P1 - P), (P2 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P2 - P), (P0 - P)).magnitude / 2.0f;
                                        //if (area == area012)
                                        if (Math.Abs(area - area012) < 1.0e-8)
                                        {
                                            voxelGrid[i][j][k].cross = true;
                                            voxelGrid[i][j][k].surface = true;
                                            voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);
                                            //voxelGrid[i][j][k].color.r = (f.fedges.hvert.color.r + f.fedges.next.hvert.color.r + f.fedges.next.next.hvert.color.r) / 3.0f;
                                            //voxelGrid[i][j][k].color.g = (f.fedges.hvert.color.g + f.fedges.next.hvert.color.g + f.fedges.next.next.hvert.color.g) / 3.0f;
                                            //voxelGrid[i][j][k].color.b = (f.fedges.hvert.color.b + f.fedges.next.hvert.color.b + f.fedges.next.next.hvert.color.b) / 3.0f;
                                            cross = true;
                                            break;
                                        }
                                    }
                                    if (cross) break;
                                }
                                if (cross) break;
                            }

                            //三角形の面とボクセルのxz面が一致しない場合
                            else
                            {
                                seki01 = (P0.y - P3.y) * (P1.y - P3.y);
                                seki12 = (P1.y - P3.y) * (P2.y - P3.y);
                                seki20 = (P2.y - P3.y) * (P0.y - P3.y);
                                P4.y = P3.y;

                                //P2.yのみ異符号
                                if ((seki01 > 0.0) && (seki12 < 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.y - P0.y) / (P2.y - P0.y);
                                    t4 = (P4.y - P1.y) / (P2.y - P1.y);
                                    P3 = (P2 - P0) * t3 + P0;
                                    P4 = (P2 - P1) * t4 + P1;
                                }
                                //P0.yのみ異符号
                                else if ((seki01 < 0.0) && (seki12 > 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.y - P1.y) / (P0.y - P1.y);
                                    t4 = (P4.y - P2.y) / (P0.y - P2.y);
                                    P3 = (P0 - P1) * t3 + P1;
                                    P4 = (P0 - P2) * t4 + P2;
                                }
                                //P1.yのみ異符号
                                else if ((seki01 < 0.0) && (seki12 < 0.0) && (seki20 > 0.0))
                                {
                                    t3 = (P3.y - P0.y) / (P1.y - P0.y);
                                    t4 = (P4.y - P2.y) / (P1.y - P2.y);
                                    P3 = (P1 - P0) * t3 + P0;
                                    P4 = (P1 - P2) * t4 + P2;
                                }
                                else
                                {
                                    continue;
                                }
                                //t3は線分内(0.0<=t3<=1.0)
                                if ((t3 < 0.0) || (1.0 < t3)) continue;
                                //t4は線分内(0.0<=t4<=1.0)
                                if ((t4 < 0.0) || (1.0 < t4)) continue;
                                //三角形とボクセルのxz平面の交線とボクセルの辺x方向が交差するかどうか調査
                                for (int i0 = 0; i0 < 2; ++i0)
                                {
                                    P34.x = voxelGrid[i][j][k].pos.x + (2 * i0 - 1) * (LengthXZ / 2.0f);
                                    t34 = (P34.x - P3.x) / (P4.x - P3.x);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    P34 = (P4 - P3) * t34 + P3;
                                    if ((voxelGrid[i][j][k].pos.z - (LengthXZ / 2.0) <= P34.z) && (P34.z <= voxelGrid[i][j][k].pos.z + (LengthXZ / 2.0)))
                                    {
                                        voxelGrid[i][j][k].cross = true;
                                        voxelGrid[i][j][k].surface = true;
                                        voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);
                                        //voxelGrid[i][j][k].color.r = (f.fedges.hvert.color.r + f.fedges.next.hvert.color.r + f.fedges.next.next.hvert.color.r) / 3.0f;
                                        //voxelGrid[i][j][k].color.g = (f.fedges.hvert.color.g + f.fedges.next.hvert.color.g + f.fedges.next.next.hvert.color.g) / 3.0f;
                                        //voxelGrid[i][j][k].color.b = (f.fedges.hvert.color.b + f.fedges.next.hvert.color.b + f.fedges.next.next.hvert.color.b) / 3.0f;
                                        cross = true;
                                        break;
                                    }
                                }
                                if (cross) break;
                                //三角形とボクセルのxz平面の交線とボクセルの辺z方向が交差するかどうか調査
                                for (int k0 = 0; k0 < 2; ++k0)
                                {
                                    P34.z = voxelGrid[i][j][k].pos.z + (2 * k0 - 1) * (LengthXZ / 2.0f);
                                    t34 = (P34.z - P3.z) / (P4.z - P3.z);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    P34 = (P4 - P3) * t34 + P3;
                                    if ((voxelGrid[i][j][k].pos.x - (LengthXZ / 2.0) <= P34.x) && (P34.x <= voxelGrid[i][j][k].pos.x + (LengthXZ / 2.0)))
                                    {
                                        voxelGrid[i][j][k].cross = true;
                                        voxelGrid[i][j][k].surface = true;
                                        voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);
                                        //voxelGrid[i][j][k].color.r = (f.fedges.hvert.color.r + f.fedges.next.hvert.color.r + f.fedges.next.next.hvert.color.r) / 3.0f;
                                        //voxelGrid[i][j][k].color.g = (f.fedges.hvert.color.g + f.fedges.next.hvert.color.g + f.fedges.next.next.hvert.color.g) / 3.0f;
                                        //voxelGrid[i][j][k].color.b = (f.fedges.hvert.color.b + f.fedges.next.hvert.color.b + f.fedges.next.next.hvert.color.b) / 3.0f;
                                        cross = true;
                                        break;
                                    }
                                }
                                if (cross) break;
                            }
                        }
                        if (cross) continue;

                        //三角形の面とボクセルのxy平面2面との交差
                        for (int k0 = 0; k0 < 2; ++k0)
                        {
                            P3.z = voxelGrid[i][j][k].pos.z + (2 * k0 - 1) * (LengthXZ / 2.0f);

                            //三角形の面とボクセルのxy面が一致する場合
                            //if ((P0.z == P3.z) && (P1.z == P3.z) && (P2.z == P3.z))
                            if ((Math.Abs(P0.z - P3.z) < 1.0e-8) && (Math.Abs(P1.z - P3.z) < 1.0e-8) && (Math.Abs(P2.z - P3.z) < 1.0e-8))
                            {
                                area = Vector3.Cross((P0 - P1), (P0 - P2)).magnitude / 2.0f;
                                area012 = 0.0f;
                                P.z = P3.z;
                                for (int i1 = 0; i1 < 2; ++i1)
                                {
                                    P.x = voxelGrid[i][j][k].pos.x + (float)(2 * i1 - 1) * (LengthXZ / 2.0f);
                                    for (int j1 = 0; j1 < 2; ++j1)
                                    {
                                        P.y = voxelGrid[i][j][k].pos.y + (float)(2 * j1 - 1) * (LengthY / 2.0f);
                                        area012 += Vector3.Cross((P0 - P), (P1 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P1 - P), (P2 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P2 - P), (P0 - P)).magnitude / 2.0f;
                                        //if (area == area012)
                                        if (Math.Abs(area - area012) < 1.0e-8)
                                        {
                                            voxelGrid[i][j][k].cross = true;
                                            voxelGrid[i][j][k].surface = true;
                                            voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);
                                            //voxelGrid[i][j][k].color.r = (f.fedges.hvert.color.r + f.fedges.next.hvert.color.r + f.fedges.next.next.hvert.color.r) / 3.0f;
                                            //voxelGrid[i][j][k].color.g = (f.fedges.hvert.color.g + f.fedges.next.hvert.color.g + f.fedges.next.next.hvert.color.g) / 3.0f;
                                            //voxelGrid[i][j][k].color.b = (f.fedges.hvert.color.b + f.fedges.next.hvert.color.b + f.fedges.next.next.hvert.color.b) / 3.0f;
                                            cross = true;
                                            break;
                                        }
                                    }
                                    if (cross) break;
                                }
                                if (cross) break;
                            }

                            //三角形の面とボクセルのxy面が一致しない場合
                            else
                            {
                                seki01 = (P0.z - P3.z) * (P1.z - P3.z);
                                seki12 = (P1.z - P3.z) * (P2.z - P3.z);
                                seki20 = (P2.z - P3.z) * (P0.z - P3.z);
                                P4.z = P3.z;

                                //P2.zのみ異符号
                                if ((seki01 > 0.0) && (seki12 < 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.z - P0.z) / (P2.z - P0.z);
                                    t4 = (P4.z - P1.z) / (P2.z - P1.z);
                                    P3 = (P2 - P0) * t3 + P0;
                                    P4 = (P2 - P1) * t4 + P1;
                                }
                                //P0.zのみ異符号
                                else if ((seki01 < 0.0) && (seki12 > 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.z - P1.z) / (P0.z - P1.z);
                                    t4 = (P4.z - P2.z) / (P0.z - P2.z);
                                    P3 = (P0 - P1) * t3 + P1;
                                    P4 = (P0 - P2) * t4 + P2;
                                }
                                //P1.zのみ異符号
                                else if ((seki01 < 0.0) && (seki12 < 0.0) && (seki20 > 0.0))
                                {
                                    t3 = (P3.z - P0.z) / (P1.z - P0.z);
                                    t4 = (P4.z - P2.z) / (P1.z - P2.z);
                                    P3 = (P1 - P0) * t3 + P0;
                                    P4 = (P1 - P2) * t4 + P2;
                                }
                                else
                                {
                                    continue;
                                }
                                //t3は線分内(0.0<=t3<=1.0)
                                if ((t3 < 0.0) || (1.0 < t3)) continue;
                                //t4は線分内(0.0<=t4<=1.0)
                                if ((t4 < 0.0) || (1.0 < t4)) continue;
                                //三角形とボクセルのxy平面の交線とボクセルの辺x方向が交差するかどうか調査
                                for (int i0 = 0; i0 < 2; ++i0)
                                {
                                    P34.x = voxelGrid[i][j][k].pos.x + (2 * i0 - 1) * (LengthXZ / 2.0f);
                                    t34 = (P34.x - P3.x) / (P4.x - P3.x);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    P34 = (P4 - P3) * t34 + P3;
                                    if ((voxelGrid[i][j][k].pos.y - (LengthY / 2.0) <= P34.y) && (P34.y <= voxelGrid[i][j][k].pos.y + (LengthY / 2.0)))
                                    {
                                        voxelGrid[i][j][k].cross = true;
                                        voxelGrid[i][j][k].surface = true;
                                        voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);
                                        //voxelGrid[i][j][k].color.r = (f.fedges.hvert.color.r + f.fedges.next.hvert.color.r + f.fedges.next.next.hvert.color.r) / 3.0f;
                                        //voxelGrid[i][j][k].color.g = (f.fedges.hvert.color.g + f.fedges.next.hvert.color.g + f.fedges.next.next.hvert.color.g) / 3.0f;
                                        //voxelGrid[i][j][k].color.b = (f.fedges.hvert.color.b + f.fedges.next.hvert.color.b + f.fedges.next.next.hvert.color.b) / 3.0f;
                                        cross = true;
                                        break;
                                    }
                                }
                                if (cross) break;
                                //三角形とボクセルのxy平面の交線とボクセルの辺y方向が交差するかどうか調査
                                for (int j0 = 0; j0 < 2; ++j0)
                                {
                                    P34.y = voxelGrid[i][j][k].pos.y + (2 * j0 - 1) * (LengthY / 2.0f);
                                    t34 = (P34.y - P3.y) / (P4.y - P3.y);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    //P34.x = (P4.x - P3.x)*t34 + P3.x;
                                    P34 = (P4 - P3) * t34 + P3;
                                    if ((voxelGrid[i][j][k].pos.x - (LengthXZ / 2.0) <= P34.x) && (P34.x <= voxelGrid[i][j][k].pos.x + (LengthXZ / 2.0)))
                                    {
                                        voxelGrid[i][j][k].cross = true;
                                        voxelGrid[i][j][k].surface = true;
                                        voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);
                                        //voxelGrid[i][j][k].color.r = (f.fedges.hvert.color.r + f.fedges.next.hvert.color.r + f.fedges.next.next.hvert.color.r) / 3.0f;
                                        //voxelGrid[i][j][k].color.g = (f.fedges.hvert.color.g + f.fedges.next.hvert.color.g + f.fedges.next.next.hvert.color.g) / 3.0f;
                                        //voxelGrid[i][j][k].color.b = (f.fedges.hvert.color.b + f.fedges.next.hvert.color.b + f.fedges.next.next.hvert.color.b) / 3.0f;
                                        cross = true;
                                        break;
                                    }
                                }
                                if (cross) break;
                            }
                        }
                        if (cross) continue;

                    }
                }
            }
        }
    }

    //メッシュ面を含む全てのボクセルの判定,どの面に関係があるのかすべて取ってくる

    public void AllCrossFace(Solid m)
    {
        //計算中のHalfEdge
        HalfEdge temp;

        //各三角形のバウンディングボックス
        Vector3 small, large;

        //バウンディングボックスの余白
        Vector3 unitVox = new Vector3(LengthXZ / 2.0f, LengthY / 2.0f, LengthXZ / 2.0f);

        Vector3 P0, P1, P2;
        float seki01, seki12, seki20;

        Vector3 P;
        // float area01, area12, area20;
        float area, area012;

        bool cross = false;//このボクセルが交差しているなら

        float t3, t4;//三角形の辺の直線の式のパラメーター
        Vector3 P3, P4;//ボクセルの面と三角形の辺の直線との交点

        Vector3 P34;//三角形の面とボクセルのいずれかの面の交線上の点
        float t34;//三角形の面とボクセルのいずれかの面の交線の式のパラメーター

        int count = 0;
        //各三角形について調べる
        foreach (var f in m.sfaces)
        {
            f.colorNum = count;
            ++count;

            temp = f.fedges;
            small = temp.hvert.vr;
            large = temp.hvert.vr;

            //各三角形のバウンディングボックスを調べる
            for (int i = 0; i < f.sidednum - 1; ++i)
            {
                temp = temp.next;
                if (small.x > temp.hvert.vr.x) small.x = temp.hvert.vr.x;
                if (large.x < temp.hvert.vr.x) large.x = temp.hvert.vr.x;

                if (small.y > temp.hvert.vr.y) small.y = temp.hvert.vr.y;
                if (large.y < temp.hvert.vr.y) large.y = temp.hvert.vr.y;

                if (small.z > temp.hvert.vr.z) small.z = temp.hvert.vr.z;
                if (large.z < temp.hvert.vr.z) large.z = temp.hvert.vr.z;
            }

            small -= unitVox;
            large += unitVox;

            //各三角形のバウンディングボックスと各ボクセルとの交差判定
            for (int i = 0; i < boxSize[0]; ++i)
            {
                //各三角形のバウンディングボックスのx方向による判定
                if ((voxelGrid[i][0][0].pos.x < small.x) || (large.x < voxelGrid[i][0][0].pos.x)) continue;

                for (int j = 0; j < boxSize[1]; ++j)
                {
                    //各三角形のバウンディングボックスのy方向による判定
                    if ((voxelGrid[0][j][0].pos.y < small.y) || (large.y < voxelGrid[0][j][0].pos.y)) continue;

                    for (int k = 0; k < boxSize[2]; ++k)
                    {
                        //各三角形のバウンディングボックスのz方向による判定
                        if ((voxelGrid[0][0][k].pos.z < small.z) || (large.z < voxelGrid[0][0][k].pos.z)) continue;

                        // if (voxelGrid[i][j][k].cross) continue;

                        //三角形の面がボクセルのいずれかの面と交差
                        cross = false;

                        P0 = f.fedges.hvert.vr;
                        P1 = f.fedges.next.hvert.vr;
                        P2 = f.fedges.next.next.hvert.vr;

                        //三角形の面とボクセルのyz平面2面との交差
                        for (int i0 = 0; i0 < 2; ++i0)
                        {
                            P3.x = voxelGrid[i][j][k].pos.x + (2 * i0 - 1) * (LengthXZ / 2.0f);

                            //三角形の面とボクセルのyz面が一致する場合
                            //if ((P0.x == P3.x) && (P1.x == P3.x) && (P2.x == P3.x))
                            if ((Math.Abs(P0.x - P3.x) < 1.0e-8) && (Math.Abs(P1.x - P3.x) < 1.0e-8) && (Math.Abs(P2.x - P3.x) < 1.0e-8))
                            {
                                area = (Vector3.Cross((P0 - P1), (P0 - P2))).magnitude / 2.0f;
                                // float a=Vector3.(P0)
                                area012 = 0.0f;
                                P.x = P3.x;
                                for (int j1 = 0; j1 < 2; ++j1)
                                {
                                    P.y = voxelGrid[i][j][k].pos.y + (float)(2 * j1 - 1) * (LengthY / 2.0f);
                                    for (int k1 = 0; k1 < 2; ++k1)
                                    {
                                        P.z = voxelGrid[i][j][k].pos.z + (float)(2 * k1 - 1) * (LengthXZ / 2.0f);
                                        area012 += Vector3.Cross((P0 - P), (P1 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P1 - P), (P2 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P2 - P), (P0 - P)).magnitude / 2.0f;
                                        //if (area == area012)
                                        if (Math.Abs(area - area012) < 1.0e-8)
                                        {
                                            voxelGrid[i][j][k].cross = true;
                                            voxelGrid[i][j][k].surface = true;

                                            //if (voxelGrid[i][j][k].crossedFacesList == null)
                                            //{
                                            //    voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);

                                            //}
                                            //else 
                                            //{
                                            //    bool faceBool = false;
                                            //    foreach (var each in voxelGrid[i][j][k].crossedFacesList)
                                            //    {
                                            //        if (each != f.colorNum)
                                            //        {
                                            //            continue;
                                            //        }
                                            //        else 
                                            //        {
                                            //            faceBool = true;
                                            //            break;
                                            //        }
                                            //    }
                                            //    if (!faceBool)
                                            //    {
                                            //        voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);

                                            //    }
                                            //}


                                            voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);
                                            //voxelGrid[i][j][k].color.r = (f.fedges.hvert.color.r + f.fedges.next.hvert.color.r + f.fedges.next.next.hvert.color.r) / 3.0f;
                                            //voxelGrid[i][j][k].color.g = (f.fedges.hvert.color.g + f.fedges.next.hvert.color.g + f.fedges.next.next.hvert.color.g) / 3.0f;
                                            //voxelGrid[i][j][k].color.b = (f.fedges.hvert.color.b + f.fedges.next.hvert.color.b + f.fedges.next.next.hvert.color.b) / 3.0f;

                                            //cross = true;
                                            break;
                                        }
                                    }
                                    // if (cross) break;
                                }
                                //if (cross) break;
                            }

                            //三角形の面とボクセルのyz面が一致しない場合
                            else
                            {
                                seki01 = (P0.x - P3.x) * (P1.x - P3.x);
                                seki12 = (P1.x - P3.x) * (P2.x - P3.x);
                                seki20 = (P2.x - P3.x) * (P0.x - P3.x);
                                P4.x = P3.x;

                                //P2.xのみ異符号
                                if ((seki01 > 0.0) && (seki12 < 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.x - P0.x) / (P2.x - P0.x);
                                    t4 = (P4.x - P1.x) / (P2.x - P1.x);
                                    P3 = (P2 - P0) * t3 + P0;
                                    P4 = (P2 - P1) * t4 + P1;
                                }
                                //P0.xのみ異符号
                                else if ((seki01 < 0.0) && (seki12 > 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.x - P1.x) / (P0.x - P1.x);
                                    t4 = (P4.x - P2.x) / (P0.x - P2.x);
                                    P3 = (P0 - P1) * t3 + P1;
                                    P4 = (P0 - P2) * t4 + P2;
                                }
                                //P1.xのみ異符号
                                else if ((seki01 < 0.0) && (seki12 < 0.0) && (seki20 > 0.0))
                                {
                                    t3 = (P3.x - P0.x) / (P1.x - P0.x);
                                    t4 = (P4.x - P2.x) / (P1.x - P2.x);
                                    P3 = (P1 - P0) * t3 + P0;
                                    P4 = (P1 - P2) * t4 + P2;
                                }
                                else
                                {
                                    continue;
                                }
                                //t3は線分内(0.0<=t3<=1.0)
                                //if ((t3 < 0.0) || (1.0 < t3))
                                //{
                                //cout << "!" << endl;
                                //	continue;
                                //}
                                //t4は線分内(0.0<=t4<=1.0)
                                //if ((t4 < 0.0) || (1.0 < t4))
                                //{
                                //cout << "!!" << endl;
                                //	continue;
                                //}
                                //三角形とボクセルのyz平面の交線とボクセルの辺y方向が交差するかどうか調査
                                for (int j0 = 0; j0 < 2; ++j0)
                                {
                                    P34.y = voxelGrid[i][j][k].pos.y + (2 * j0 - 1) * (LengthY / 2.0f);
                                    t34 = (P34.y - P3.y) / (P4.y - P3.y);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34))
                                    {
                                        //cout << "!" << endl;
                                        continue;
                                    }
                                    P34.z = (P4.z - P3.z) * t34 + P3.z;
                                    if ((voxelGrid[i][j][k].pos.z - (LengthXZ / 2.0) <= P34.z) && (P34.z <= voxelGrid[i][j][k].pos.z + (LengthXZ / 2.0)))
                                    {
                                        voxelGrid[i][j][k].cross = true;
                                        voxelGrid[i][j][k].surface = true;
                                        voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);
                                        //voxelGrid[i][j][k].color.r = (f.fedges.hvert.color.r + f.fedges.next.hvert.color.r + f.fedges.next.next.hvert.color.r) / 3.0f;
                                        //voxelGrid[i][j][k].color.g = (f.fedges.hvert.color.g + f.fedges.next.hvert.color.g + f.fedges.next.next.hvert.color.g) / 3.0f;
                                        //voxelGrid[i][j][k].color.b = (f.fedges.hvert.color.b + f.fedges.next.hvert.color.b + f.fedges.next.next.hvert.color.b) / 3.0f;
                                        // cross = true;
                                        break;
                                    }
                                }
                                //if (cross) break;
                                //三角形とボクセルのyz平面の交線とボクセルの辺z方向が交差するかどうか調査
                                for (int k0 = 0; k0 < 2; ++k0)
                                {
                                    P34.z = voxelGrid[i][j][k].pos.z + (2 * k0 - 1) * (LengthXZ / 2.0f);
                                    t34 = (P34.z - P3.z) / (P4.z - P3.z);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    P34.y = (P4.y - P3.y) * t34 + P3.y;
                                    if ((voxelGrid[i][j][k].pos.y - (LengthY / 2.0) <= P34.y) && (P34.y <= voxelGrid[i][j][k].pos.y + (LengthY / 2.0)))
                                    {
                                        voxelGrid[i][j][k].cross = true;
                                        voxelGrid[i][j][k].surface = true;
                                        voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);
                                        //voxelGrid[i][j][k].color.r = (f.fedges.hvert.color.r + f.fedges.next.hvert.color.r + f.fedges.next.next.hvert.color.r) / 3.0f;
                                        //voxelGrid[i][j][k].color.g = (f.fedges.hvert.color.g + f.fedges.next.hvert.color.g + f.fedges.next.next.hvert.color.g) / 3.0f;
                                        //voxelGrid[i][j][k].color.b = (f.fedges.hvert.color.b + f.fedges.next.hvert.color.b + f.fedges.next.next.hvert.color.b) / 3.0f;
                                        // cross = true;
                                        break;
                                    }
                                }
                                //if (cross) break;
                            }
                        }
                        //if (cross) continue;

                        //三角形の面とボクセルのxz平面2面との交差
                        for (int j0 = 0; j0 < 2; ++j0)
                        {
                            P3.y = voxelGrid[i][j][k].pos.y + (2 * j0 - 1) * (LengthY / 2.0f);

                            //三角形の面とボクセルのxz面が一致する場合
                            //if ((P0.y == P3.y) && (P1.y == P3.y) && (P2.y == P3.y))
                            if ((Math.Abs(P0.y - P3.y) < 1.0e-8) && (Math.Abs(P1.y - P3.y) < 1.0e-8) && (Math.Abs(P2.y - P3.y) < 1.0e-8))
                            {
                                area = Vector3.Cross((P0 - P1), (P0 - P2)).magnitude / 2.0f;
                                area012 = 0.0f;
                                P.y = P3.y;
                                for (int i1 = 0; i1 < 2; ++i1)
                                {
                                    P.x = voxelGrid[i][j][k].pos.x + (float)(2 * i1 - 1) * (LengthXZ / 2.0f);
                                    for (int k1 = 0; k1 < 2; ++k1)
                                    {
                                        P.z = voxelGrid[i][j][k].pos.z + (float)(2 * k1 - 1) * (LengthXZ / 2.0f);
                                        area012 += Vector3.Cross((P0 - P), (P1 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P1 - P), (P2 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P2 - P), (P0 - P)).magnitude / 2.0f;
                                        //if (area == area012)
                                        if (Math.Abs(area - area012) < 1.0e-8)
                                        {
                                            voxelGrid[i][j][k].cross = true;
                                            voxelGrid[i][j][k].surface = true;
                                            voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);
                                            //voxelGrid[i][j][k].color.r = (f.fedges.hvert.color.r + f.fedges.next.hvert.color.r + f.fedges.next.next.hvert.color.r) / 3.0f;
                                            //voxelGrid[i][j][k].color.g = (f.fedges.hvert.color.g + f.fedges.next.hvert.color.g + f.fedges.next.next.hvert.color.g) / 3.0f;
                                            //voxelGrid[i][j][k].color.b = (f.fedges.hvert.color.b + f.fedges.next.hvert.color.b + f.fedges.next.next.hvert.color.b) / 3.0f;
                                            //cross = true;
                                            break;
                                        }
                                    }
                                    //if (cross) break;
                                }
                                //if (cross) break;
                            }

                            //三角形の面とボクセルのxz面が一致しない場合
                            else
                            {
                                seki01 = (P0.y - P3.y) * (P1.y - P3.y);
                                seki12 = (P1.y - P3.y) * (P2.y - P3.y);
                                seki20 = (P2.y - P3.y) * (P0.y - P3.y);
                                P4.y = P3.y;

                                //P2.yのみ異符号
                                if ((seki01 > 0.0) && (seki12 < 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.y - P0.y) / (P2.y - P0.y);
                                    t4 = (P4.y - P1.y) / (P2.y - P1.y);
                                    P3 = (P2 - P0) * t3 + P0;
                                    P4 = (P2 - P1) * t4 + P1;
                                }
                                //P0.yのみ異符号
                                else if ((seki01 < 0.0) && (seki12 > 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.y - P1.y) / (P0.y - P1.y);
                                    t4 = (P4.y - P2.y) / (P0.y - P2.y);
                                    P3 = (P0 - P1) * t3 + P1;
                                    P4 = (P0 - P2) * t4 + P2;
                                }
                                //P1.yのみ異符号
                                else if ((seki01 < 0.0) && (seki12 < 0.0) && (seki20 > 0.0))
                                {
                                    t3 = (P3.y - P0.y) / (P1.y - P0.y);
                                    t4 = (P4.y - P2.y) / (P1.y - P2.y);
                                    P3 = (P1 - P0) * t3 + P0;
                                    P4 = (P1 - P2) * t4 + P2;
                                }
                                else
                                {
                                    continue;
                                }
                                //t3は線分内(0.0<=t3<=1.0)
                                if ((t3 < 0.0) || (1.0 < t3)) continue;
                                //t4は線分内(0.0<=t4<=1.0)
                                if ((t4 < 0.0) || (1.0 < t4)) continue;
                                //三角形とボクセルのxz平面の交線とボクセルの辺x方向が交差するかどうか調査
                                for (int i0 = 0; i0 < 2; ++i0)
                                {
                                    P34.x = voxelGrid[i][j][k].pos.x + (2 * i0 - 1) * (LengthXZ / 2.0f);
                                    t34 = (P34.x - P3.x) / (P4.x - P3.x);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    P34 = (P4 - P3) * t34 + P3;
                                    if ((voxelGrid[i][j][k].pos.z - (LengthXZ / 2.0) <= P34.z) && (P34.z <= voxelGrid[i][j][k].pos.z + (LengthXZ / 2.0)))
                                    {
                                        voxelGrid[i][j][k].cross = true;
                                        voxelGrid[i][j][k].surface = true;
                                        voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);
                                        //voxelGrid[i][j][k].color.r = (f.fedges.hvert.color.r + f.fedges.next.hvert.color.r + f.fedges.next.next.hvert.color.r) / 3.0f;
                                        //voxelGrid[i][j][k].color.g = (f.fedges.hvert.color.g + f.fedges.next.hvert.color.g + f.fedges.next.next.hvert.color.g) / 3.0f;
                                        //voxelGrid[i][j][k].color.b = (f.fedges.hvert.color.b + f.fedges.next.hvert.color.b + f.fedges.next.next.hvert.color.b) / 3.0f;
                                        //cross = true;
                                        break;
                                    }
                                }
                                //if (cross) break;
                                //三角形とボクセルのxz平面の交線とボクセルの辺z方向が交差するかどうか調査
                                for (int k0 = 0; k0 < 2; ++k0)
                                {
                                    P34.z = voxelGrid[i][j][k].pos.z + (2 * k0 - 1) * (LengthXZ / 2.0f);
                                    t34 = (P34.z - P3.z) / (P4.z - P3.z);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    P34 = (P4 - P3) * t34 + P3;
                                    if ((voxelGrid[i][j][k].pos.x - (LengthXZ / 2.0) <= P34.x) && (P34.x <= voxelGrid[i][j][k].pos.x + (LengthXZ / 2.0)))
                                    {
                                        voxelGrid[i][j][k].cross = true;
                                        voxelGrid[i][j][k].surface = true;
                                        voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);
                                        //voxelGrid[i][j][k].color.r = (f.fedges.hvert.color.r + f.fedges.next.hvert.color.r + f.fedges.next.next.hvert.color.r) / 3.0f;
                                        //voxelGrid[i][j][k].color.g = (f.fedges.hvert.color.g + f.fedges.next.hvert.color.g + f.fedges.next.next.hvert.color.g) / 3.0f;
                                        //voxelGrid[i][j][k].color.b = (f.fedges.hvert.color.b + f.fedges.next.hvert.color.b + f.fedges.next.next.hvert.color.b) / 3.0f;
                                        // cross = true;
                                        break;
                                    }
                                }
                                //if (cross) break;
                            }
                        }
                        //if (cross) continue;

                        //三角形の面とボクセルのxy平面2面との交差
                        for (int k0 = 0; k0 < 2; ++k0)
                        {
                            P3.z = voxelGrid[i][j][k].pos.z + (2 * k0 - 1) * (LengthXZ / 2.0f);

                            //三角形の面とボクセルのxy面が一致する場合
                            //if ((P0.z == P3.z) && (P1.z == P3.z) && (P2.z == P3.z))
                            if ((Math.Abs(P0.z - P3.z) < 1.0e-8) && (Math.Abs(P1.z - P3.z) < 1.0e-8) && (Math.Abs(P2.z - P3.z) < 1.0e-8))
                            {
                                area = Vector3.Cross((P0 - P1), (P0 - P2)).magnitude / 2.0f;
                                area012 = 0.0f;
                                P.z = P3.z;
                                for (int i1 = 0; i1 < 2; ++i1)
                                {
                                    P.x = voxelGrid[i][j][k].pos.x + (float)(2 * i1 - 1) * (LengthXZ / 2.0f);
                                    for (int j1 = 0; j1 < 2; ++j1)
                                    {
                                        P.y = voxelGrid[i][j][k].pos.y + (float)(2 * j1 - 1) * (LengthY / 2.0f);
                                        area012 += Vector3.Cross((P0 - P), (P1 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P1 - P), (P2 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P2 - P), (P0 - P)).magnitude / 2.0f;
                                        //if (area == area012)
                                        if (Math.Abs(area - area012) < 1.0e-8)
                                        {
                                            voxelGrid[i][j][k].cross = true;
                                            voxelGrid[i][j][k].surface = true;
                                            voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);
                                            //voxelGrid[i][j][k].color.r = (f.fedges.hvert.color.r + f.fedges.next.hvert.color.r + f.fedges.next.next.hvert.color.r) / 3.0f;
                                            //voxelGrid[i][j][k].color.g = (f.fedges.hvert.color.g + f.fedges.next.hvert.color.g + f.fedges.next.next.hvert.color.g) / 3.0f;
                                            //voxelGrid[i][j][k].color.b = (f.fedges.hvert.color.b + f.fedges.next.hvert.color.b + f.fedges.next.next.hvert.color.b) / 3.0f;
                                            //cross = true;
                                            break;
                                        }
                                    }
                                    // if (cross) break;
                                }
                                //if (cross) break;
                            }

                            //三角形の面とボクセルのxy面が一致しない場合
                            else
                            {
                                seki01 = (P0.z - P3.z) * (P1.z - P3.z);
                                seki12 = (P1.z - P3.z) * (P2.z - P3.z);
                                seki20 = (P2.z - P3.z) * (P0.z - P3.z);
                                P4.z = P3.z;

                                //P2.zのみ異符号
                                if ((seki01 > 0.0) && (seki12 < 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.z - P0.z) / (P2.z - P0.z);
                                    t4 = (P4.z - P1.z) / (P2.z - P1.z);
                                    P3 = (P2 - P0) * t3 + P0;
                                    P4 = (P2 - P1) * t4 + P1;
                                }
                                //P0.zのみ異符号
                                else if ((seki01 < 0.0) && (seki12 > 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.z - P1.z) / (P0.z - P1.z);
                                    t4 = (P4.z - P2.z) / (P0.z - P2.z);
                                    P3 = (P0 - P1) * t3 + P1;
                                    P4 = (P0 - P2) * t4 + P2;
                                }
                                //P1.zのみ異符号
                                else if ((seki01 < 0.0) && (seki12 < 0.0) && (seki20 > 0.0))
                                {
                                    t3 = (P3.z - P0.z) / (P1.z - P0.z);
                                    t4 = (P4.z - P2.z) / (P1.z - P2.z);
                                    P3 = (P1 - P0) * t3 + P0;
                                    P4 = (P1 - P2) * t4 + P2;
                                }
                                else
                                {
                                    continue;
                                }
                                //t3は線分内(0.0<=t3<=1.0)
                                if ((t3 < 0.0) || (1.0 < t3)) continue;
                                //t4は線分内(0.0<=t4<=1.0)
                                if ((t4 < 0.0) || (1.0 < t4)) continue;
                                //三角形とボクセルのxy平面の交線とボクセルの辺x方向が交差するかどうか調査
                                for (int i0 = 0; i0 < 2; ++i0)
                                {
                                    P34.x = voxelGrid[i][j][k].pos.x + (2 * i0 - 1) * (LengthXZ / 2.0f);
                                    t34 = (P34.x - P3.x) / (P4.x - P3.x);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    P34 = (P4 - P3) * t34 + P3;
                                    if ((voxelGrid[i][j][k].pos.y - (LengthY / 2.0) <= P34.y) && (P34.y <= voxelGrid[i][j][k].pos.y + (LengthY / 2.0)))
                                    {
                                        voxelGrid[i][j][k].cross = true;
                                        voxelGrid[i][j][k].surface = true;
                                        voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);
                                        //voxelGrid[i][j][k].color.r = (f.fedges.hvert.color.r + f.fedges.next.hvert.color.r + f.fedges.next.next.hvert.color.r) / 3.0f;
                                        //voxelGrid[i][j][k].color.g = (f.fedges.hvert.color.g + f.fedges.next.hvert.color.g + f.fedges.next.next.hvert.color.g) / 3.0f;
                                        //voxelGrid[i][j][k].color.b = (f.fedges.hvert.color.b + f.fedges.next.hvert.color.b + f.fedges.next.next.hvert.color.b) / 3.0f;
                                        //cross = true;
                                        break;
                                    }
                                }
                                //if (cross) break;
                                //三角形とボクセルのxy平面の交線とボクセルの辺y方向が交差するかどうか調査
                                for (int j0 = 0; j0 < 2; ++j0)
                                {
                                    P34.y = voxelGrid[i][j][k].pos.y + (2 * j0 - 1) * (LengthY / 2.0f);
                                    t34 = (P34.y - P3.y) / (P4.y - P3.y);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    //P34.x = (P4.x - P3.x)*t34 + P3.x;
                                    P34 = (P4 - P3) * t34 + P3;
                                    if ((voxelGrid[i][j][k].pos.x - (LengthXZ / 2.0) <= P34.x) && (P34.x <= voxelGrid[i][j][k].pos.x + (LengthXZ / 2.0)))
                                    {
                                        voxelGrid[i][j][k].cross = true;
                                        voxelGrid[i][j][k].surface = true;
                                        voxelGrid[i][j][k].crossedFacesList.Add(f.colorNum);
                                        //voxelGrid[i][j][k].color.r = (f.fedges.hvert.color.r + f.fedges.next.hvert.color.r + f.fedges.next.next.hvert.color.r) / 3.0f;
                                        //voxelGrid[i][j][k].color.g = (f.fedges.hvert.color.g + f.fedges.next.hvert.color.g + f.fedges.next.next.hvert.color.g) / 3.0f;
                                        //voxelGrid[i][j][k].color.b = (f.fedges.hvert.color.b + f.fedges.next.hvert.color.b + f.fedges.next.next.hvert.color.b) / 3.0f;
                                        //cross = true;
                                        break;
                                    }
                                }
                                //if (cross) break;
                            }
                        }
                        //if (cross) continue;

                        if (voxelGrid[i][j][k].crossedFacesList != null)
                        {
                            var al = new List<int>();
                            //基になる配列の要素を列挙する
                            foreach (int each in voxelGrid[i][j][k].crossedFacesList)
                            {
                                //コレクション内に存在していなければ、追加する
                                if (!al.Contains(each))
                                {
                                    al.Add(each);
                                }
                            }
                            voxelGrid[i][j][k].crossedFacesList = al;
                        }

                    }
                }
            }
        }
    }

    //メッシュ頂点を含むsurveyボクセルの判定
    public void SurveyCrossVert(Solid m)
    {
        //計算中のHalfEdge
        // HalfEdge temp;

        //各三角形のバウンディングボックス
        Vector3 small, large;

        //バウンディングボックスの余白
        Vector3 unitVox = new Vector3(surveyLengXZ / 2.0f, surveyLengY / 2.0f, surveyLengXZ / 2.0f);


        //for(int v=0;v<m.numVertex;++v)
        foreach (var v in m.sverts)
        {
            small = v.vr;
            large = v.vr;
            // Debug.Log(m.sverts[v].vr);
            //small = m.sverts[v].vr;
            //large = m.sverts[v].vr;


            small -= unitVox;
            large += unitVox;
            //各頂点のバウンディングボックスと各ボクセルとの交差判定
            for (int i = 0; i < surveyBoxSize[0]; ++i)
            {
                //各頂点のバウンディングボックスのx方向による判定
                if ((surveyVoxel[i][0][0].pos.x < small.x) || (large.x < surveyVoxel[i][0][0].pos.x)) continue;

                for (int j = 0; j < surveyBoxSize[1]; ++j)
                {
                    //各頂点のバウンディングボックスのy方向による判定
                    if ((surveyVoxel[0][j][0].pos.y < small.y) || (large.y < surveyVoxel[0][j][0].pos.y)) continue;

                    for (int k = 0; k < surveyBoxSize[2]; ++k)
                    {
                        //各頂点のバウンディングボックスのz方向による判定
                        if ((surveyVoxel[0][0][k].pos.z < small.z) || (large.z < surveyVoxel[0][0][k].pos.z)) continue;

                        surveyVoxel[i][j][k].cross = true;
                        surveyVoxel[i][j][k].surface = true;
                        //cross = true;
                        //break;
                    }
                }
            }
        }


    }
    //メッシュ辺と交差するsurveyボクセルの判定
    public void SurveyCrossEdge(Solid m)
    {
        //各辺のバウンディングボックス
        Vector3 small, large;

        //バウンディングボックスの余白
        Vector3 unitVox = new Vector3(surveyLengXZ / 2.0f, surveyLengY / 2.0f, surveyLengXZ / 2.0f);

        Vector3 P0, P1;

        //このボクセルが交差しているなら
        bool cross = false;

        float t;//辺の直線の式のパラメーター
        Vector3 P;//ボクセルの面と辺の直線との交点

        //foreach (var e in m.sedges)
        //{
        //    Debug.Log(e.he1.hvert.vr);
        //}

        //各辺について調べる
        //int count = 0;
        foreach (var e in m.sedges)
        {
            //e.colorNum = count;
            //++count;
            //    for (auto e : m.sedges)
            //{
            //各辺のバウンディングボックスを調べる
            P0 = e.he1.hvert.vr;
            //Debug.Log(e.he1.hvert.vr);
            P1 = e.he2.hvert.vr;

            if (P0.x < P1.x)
            {
                small.x = P0.x;
                large.x = P1.x;
            }
            else
            {
                small.x = P1.x;
                large.x = P0.x;
            }

            if (P0.y < P1.y)
            {
                small.y = P0.y;
                large.y = P1.y;
            }
            else
            {
                small.y = P1.y;
                large.y = P0.y;
            }

            if (P0.z < P1.z)
            {
                small.z = P0.z;
                large.z = P1.z;
            }
            else
            {
                small.z = P1.z;
                large.z = P0.z;
            }

            small -= unitVox;
            large += unitVox;


            //各辺のバウンディングボックスと各ボクセルとの交差判定
            for (int i = 0; i < surveyBoxSize[0]; ++i)
            {
                //各辺のバウンディングボックスのx方向による判定
                if ((surveyVoxel[i][0][0].pos.x < small.x) || (large.x < surveyVoxel[i][0][0].pos.x)) continue;

                for (int j = 0; j < surveyBoxSize[1]; ++j)
                {
                    //各辺のバウンディングボックスのy方向による判定
                    if ((surveyVoxel[0][j][0].pos.y < small.y) || (large.y < surveyVoxel[0][j][0].pos.y)) continue;

                    for (int k = 0; k < surveyBoxSize[2]; ++k)
                    {
                        //各辺のバウンディングボックスのz方向による判定
                        if ((surveyVoxel[0][0][k].pos.z < small.z) || (large.z < surveyVoxel[0][0][k].pos.z)) continue;

                        if (surveyVoxel[i][j][k].cross) continue;

                        //辺がボクセルのいずれかの面と交差
                        cross = false;
                        // cross = true;//勝手に変えた


                        P0 = e.he1.hvert.vr;
                        P1 = e.he2.hvert.vr;

                        //辺とボクセルのyz平面2面との交差
                        for (int i0 = 0; i0 < 2; ++i0)
                        {
                            //ボクセルのyz平面のx座標
                            P.x = surveyVoxel[i][j][k].pos.x + (float)(2 * i0 - 1) * (surveyLengXZ / 2.0f);

                            //三角形の辺の直線のパラメーター
                            t = (P.x - P0.x) / (P1.x - P0.x);

                            //tは線分P0-P1内(0.0<=t<=1.0)
                            if ((t < 0.0) || (1.0 < t)) continue;

                            //辺の直線とボクセルのyz平面の交点
                            P = (P1 - P0) * t + P0;

                            //交点のy座標の確認
                            //if ((surveyVoxel[i][j][k].pos.y - (Length / 2.0) <= P.y) && (P.y <= surveyVoxel[i][j][k].pos.y + (Length / 2.0)))
                            if ((surveyVoxel[i][j][k].pos.y - (surveyLengY / 2.0) - P.y <= 1.0e-8) && (-1.0e-8 <= surveyVoxel[i][j][k].pos.y + (surveyLengY / 2.0) - P.y))
                            {
                                //交点のz座標の確認
                                //if ((surveyVoxel[i][j][k].pos.z - (Length / 2.0) <= P.z) && (P.z <= surveyVoxel[i][j][k].pos.z + (Length / 2.0)))
                                if ((surveyVoxel[i][j][k].pos.z - (surveyLengXZ / 2.0) - P.z <= 1.0e-8) && (-1.0e-8 <= surveyVoxel[i][j][k].pos.z + (surveyLengXZ / 2.0) - P.z))
                                {
                                    surveyVoxel[i][j][k].cross = true;
                                    surveyVoxel[i][j][k].surface = true;
                                    cross = true;
                                    break;
                                }
                            }
                        }
                        if (cross) continue;    //辺とボクセルのxz平面2面との交差
                        for (int j0 = 0; j0 < 2; ++j0)
                        {
                            //ボクセルのxz平面のy座標
                            P.y = surveyVoxel[i][j][k].pos.y + (float)(2 * j0 - 1) * (surveyLengY / 2.0f);

                            //辺の直線のパラメーター
                            t = (P.y - P0.y) / (P1.y - P0.y);

                            //tは線分P0-P1内(0.0<=t<=1.0)
                            if ((t < 0.0) || (1.0 < t)) continue;

                            //辺の直線とボクセルのxz平面の交点
                            P = (P1 - P0) * t + P0;

                            //交点のx座標の確認
                            //if ((surveyVoxel[i][j][k].pos.x - (Length / 2.0) <= P.x) && (P.x <= surveyVoxel[i][j][k].pos.x + (Length / 2.0)))
                            if ((surveyVoxel[i][j][k].pos.x - (surveyLengXZ / 2.0) - P.x <= 1.0e-8) && (-1.0e-8 <= surveyVoxel[i][j][k].pos.x + (surveyLengXZ / 2.0) - P.x))
                            {
                                //交点のz座標の確認
                                //if ((surveyVoxel[i][j][k].pos.z - (Length / 2.0) <= P.z) && (P.z <= surveyVoxel[i][j][k].pos.z + (Length / 2.0)))
                                if ((surveyVoxel[i][j][k].pos.z - (surveyLengXZ / 2.0) - P.z <= 1.0e-8) && (-1.0e-8 <= surveyVoxel[i][j][k].pos.z + (surveyLengXZ / 2.0) - P.z))
                                {
                                    surveyVoxel[i][j][k].cross = true;
                                    surveyVoxel[i][j][k].surface = true;
                                    cross = true;
                                    break;
                                }
                            }
                        }
                        if (cross) continue;

                        //辺とボクセルのxy平面2面との交差
                        for (int k0 = 0; k0 < 2; ++k0)
                        {
                            //ボクセルのxy平面のz座標
                            P.z = surveyVoxel[i][j][k].pos.z + (float)(2 * k0 - 1) * (surveyLengXZ / 2.0f);

                            //辺の直線のパラメーター
                            t = (P.z - P0.z) / (P1.z - P0.z);

                            //tは線分P0-P1内(0.0<=t<=1.0)
                            if ((t < 0.0) || (1.0 < t)) continue;

                            //辺の直線とボクセルのxy平面の交点
                            P = (P1 - P0) * t + P0;

                            //交点のx座標の確認
                            //if ((surveyVoxel[i][j][k].pos.x - (Length / 2.0) <= P.x) && (P.x <= surveyVoxel[i][j][k].pos.x + (Length / 2.0)))
                            if ((surveyVoxel[i][j][k].pos.x - (surveyLengXZ / 2.0) - P.x <= 1.0e-8) && (-1.0e-8 <= surveyVoxel[i][j][k].pos.x + (surveyLengXZ / 2.0) - P.x))
                            {
                                //交点のy座標の確認
                                //if ((surveyVoxel[i][j][k].pos.y - (Length / 2.0) <= P.y) && (P.y <= surveyVoxel[i][j][k].pos.y + (Length / 2.0)))
                                if ((surveyVoxel[i][j][k].pos.y - (surveyLengY / 2.0) - P.y <= 1.0e-8) && (-1.0e-8 <= surveyVoxel[i][j][k].pos.y + (surveyLengY / 2.0) - P.y))
                                {
                                    surveyVoxel[i][j][k].cross = true;
                                    surveyVoxel[i][j][k].surface = true;
                                    cross = true;
                                    break;
                                }
                            }
                        }
                        if (cross) continue;
                    }
                }
            }
        }

    }
    //メッシュ面を含むsurveyボクセルの判定
    public void SurveyCrossFace(Solid m)
    {
        //計算中のHalfEdge
        HalfEdge temp;

        //各三角形のバウンディングボックス
        Vector3 small, large;

        //バウンディングボックスの余白
        Vector3 unitVox = new Vector3(surveyLengXZ / 2.0f, surveyLengY / 2.0f, surveyLengXZ / 2.0f);

        Vector3 P0, P1, P2;
        float seki01, seki12, seki20;

        Vector3 P;
        // float area01, area12, area20;
        float area, area012;

        bool cross = false;//このボクセルが交差しているなら

        float t3, t4;//三角形の辺の直線の式のパラメーター
        Vector3 P3, P4;//ボクセルの面と三角形の辺の直線との交点

        Vector3 P34;//三角形の面とボクセルのいずれかの面の交線上の点
        float t34;//三角形の面とボクセルのいずれかの面の交線の式のパラメーター

        float halfDiagonal;

        float cosAlpha;
        Vector3 g0, g1, g2, v1, v2, v3;
        float g0g1Mag;
        float f1, f2, f3;

        float xzDia;
        xzDia = Mathf.Sqrt(surveyLengXZ * surveyLengXZ * 2f);
        halfDiagonal = Mathf.Sqrt(xzDia * xzDia + surveyLengY * surveyLengY) / 2f;

        int count = 0;
        //各三角形について調べる
        foreach (var f in m.sfaces)
        {
            f.colorNum = count;
            ++count;

            temp = f.fedges;
            small = temp.hvert.vr;
            large = temp.hvert.vr;

            //各三角形のバウンディングボックスを調べる
            for (int i = 0; i < f.sidednum - 1; ++i)
            {
                temp = temp.next;
                if (small.x > temp.hvert.vr.x) small.x = temp.hvert.vr.x;
                if (large.x < temp.hvert.vr.x) large.x = temp.hvert.vr.x;

                if (small.y > temp.hvert.vr.y) small.y = temp.hvert.vr.y;
                if (large.y < temp.hvert.vr.y) large.y = temp.hvert.vr.y;

                if (small.z > temp.hvert.vr.z) small.z = temp.hvert.vr.z;
                if (large.z < temp.hvert.vr.z) large.z = temp.hvert.vr.z;
            }

            small -= unitVox;
            large += unitVox;

            //各三角形のバウンディングボックスと各ボクセルとの交差判定
            for (int i = 0; i < surveyBoxSize[0]; ++i)
            {
                //各三角形のバウンディングボックスのx方向による判定
                if ((surveyVoxel[i][0][0].pos.x < small.x) || (large.x < surveyVoxel[i][0][0].pos.x)) continue;

                for (int j = 0; j < surveyBoxSize[1]; ++j)
                {
                    //各三角形のバウンディングボックスのy方向による判定
                    if ((surveyVoxel[0][j][0].pos.y < small.y) || (large.y < surveyVoxel[0][j][0].pos.y)) continue;

                    for (int k = 0; k < surveyBoxSize[2]; ++k)
                    {
                        //各三角形のバウンディングボックスのz方向による判定
                        if ((surveyVoxel[0][0][k].pos.z < small.z) || (large.z < surveyVoxel[0][0][k].pos.z)) continue;

                        if (surveyVoxel[i][j][k].cross) continue;

                        //三角形の面がボクセルのいずれかの面と交差
                        cross = false;

                        P0 = f.fedges.hvert.vr;
                        P1 = f.fedges.next.hvert.vr;
                        P2 = f.fedges.next.next.hvert.vr;

                        //==================================================================================================//
                        //Journal of Chemical and Pharmaceutical Research, 2013, 5(9):420-427 Reading and voxelization of 3D modelsより，この距離以下のボクセルは省く
                        //g0 = surveyVoxel[i][j][k].pos;
                        //f.CalNormal();
                        //cosAlpha = Vector3.Dot(g0 - P0, f.normal) / (g0 - P0).magnitude / f.normal.magnitude;
                        //g0g1Mag = (P0 - g0).magnitude * cosAlpha;
                        //g1 = g0 - g0g1Mag * Vector3.Normalize(f.normal);

                        //v1 = Vector3.Normalize(P0 - P1) + Vector3.Normalize(P0 - P2);
                        //v2 = Vector3.Normalize(P1 - P2) + Vector3.Normalize(P1 - P0);
                        //v3 = Vector3.Normalize(P0 - P1) + Vector3.Normalize(P0 - P2);

                        //f1 = Vector3.Dot(Vector3.Cross(v1, (g1 - P0)), f.normal);
                        //f2 = Vector3.Dot(Vector3.Cross(v2, (g1 - P1)), f.normal);
                        //f3 = Vector3.Dot(Vector3.Cross(v3, (g1 - P2)), f.normal);

                        //float distance = 0;//三角形メッシュの面からボクセルの中心の距離

                        //if (f1 > 0 && f2 < 0)//線上にあった場合どうすればいいのか
                        //{
                        //    float decideOut;//これが外側の時にマイナス
                        //    decideOut = Vector3.Dot(Vector3.Cross((P0 - g1), (P1 - g1)), f.normal);
                        //    if (decideOut > 0)
                        //    {
                        //        distance = g0g1Mag;
                        //        // voxelGrid[i][j][k].disTri=
                        //    }
                        //    else
                        //    {
                        //        Vector3 r = new Vector3();
                        //        r = Vector3.Cross(Vector3.Cross(P1 - g1, P0 - g1), P1 - P0);
                        //        float cosGanma = Vector3.Dot((P0 - g1).normalized, r.normalized);
                        //        float g1g2Mag = (P0 - g1).magnitude * cosGanma;
                        //        Vector3 g1g2 = new Vector3();
                        //        g1g2 = g1g2Mag * r.normalized;
                        //        g2 = g1 + g1g2;
                        //        float t;
                        //        //辺に近いのか頂点に近いのかの判定
                        //        if ((P1.x - P0.x) != 0)//これでいいのか
                        //        {
                        //            t = (g2.x - P0.x) / (P1.x - P0.x);

                        //        }
                        //        else
                        //        {
                        //            t = (g2.y - P0.y) / (P1.y - P0.y);

                        //        }
                        //        if (0 <= t && t <= 1)
                        //        {
                        //            distance = Mathf.Sqrt((g2 - g1).sqrMagnitude + (g1 - g0).sqrMagnitude);

                        //        }
                        //        else if (t < 0)
                        //        {
                        //            distance = (P0 - g0).magnitude;
                        //        }
                        //        else
                        //        {
                        //            distance = (P1 - g0).magnitude;
                        //        }
                        //    }

                        //}
                        //if (f2 > 0 && f3 < 0)//線上にあった場合どうすればいいのか
                        //{
                        //    float decideOut;//これが外側の時にマイナス
                        //    decideOut = Vector3.Dot(Vector3.Cross((P1 - g1), (P2 - g1)), f.normal);
                        //    if (decideOut > 0)
                        //    {
                        //        distance = g0g1Mag;
                        //        // voxelGrid[i][j][k].disTri=
                        //    }
                        //    else
                        //    {
                        //        Vector3 r = new Vector3();
                        //        r = Vector3.Cross(Vector3.Cross(P2 - g1, P1 - g1), P2 - P1);
                        //        float cosGanma = Vector3.Dot((P1 - g1).normalized, r.normalized);
                        //        float g1g2Mag = (P1 - g1).magnitude * cosGanma;
                        //        Vector3 g1g2 = new Vector3();
                        //        g1g2 = g1g2Mag * r.normalized;
                        //        g2 = g1 + g1g2;
                        //        float t;
                        //        //辺に近いのか頂点に近いのかの判定
                        //        if ((P2.x - P1.x) != 0)//これでいいのか
                        //        {
                        //            t = (g2.x - P1.x) / (P2.x - P1.x);

                        //        }
                        //        else
                        //        {
                        //            t = (g2.y - P1.y) / (P2.y - P1.y);

                        //        }
                        //        if (0 <= t && t <= 1)
                        //        {
                        //            distance = Mathf.Sqrt((g2 - g1).sqrMagnitude + (g1 - g0).sqrMagnitude);

                        //        }
                        //        else if (t < 0)
                        //        {
                        //            distance = (P1 - g0).magnitude;
                        //        }
                        //        else
                        //        {
                        //            distance = (P2 - g0).magnitude;
                        //        }
                        //    }

                        //}
                        //if (f3 > 0 && f1 < 0)//線上にあった場合どうすればいいのか
                        //{
                        //    float decideOut;//これが外側の時にマイナス
                        //    decideOut = Vector3.Dot(Vector3.Cross((P2 - g1), (P0 - g1)), f.normal);
                        //    if (decideOut > 0)
                        //    {
                        //        distance = g0g1Mag;
                        //        // voxelGrid[i][j][k].disTri=
                        //    }
                        //    else
                        //    {
                        //        Vector3 r = new Vector3();
                        //        r = Vector3.Cross(Vector3.Cross(P0 - g1, P2 - g1), P0 - P2);
                        //        float cosGanma = Vector3.Dot((P2 - g1).normalized, r.normalized);
                        //        float g1g2Mag = (P2 - g1).magnitude * cosGanma;
                        //        Vector3 g1g2 = new Vector3();
                        //        g1g2 = g1g2Mag * r.normalized;
                        //        g2 = g1 + g1g2;
                        //        float t;
                        //        //辺に近いのか頂点に近いのかの判定
                        //        if ((P0.x - P2.x) != 0)//これでいいのか
                        //        {
                        //            t = (g2.x - P2.x) / (P0.x - P2.x);

                        //        }
                        //        else
                        //        {
                        //            t = (g2.y - P2.y) / (P0.y - P2.y);

                        //        }
                        //        if (0 <= t && t <= 1)
                        //        {
                        //            distance = Mathf.Sqrt((g2 - g1).sqrMagnitude + (g1 - g0).sqrMagnitude);

                        //        }
                        //        else if (t < 0)
                        //        {
                        //            distance = (P2 - g0).magnitude;
                        //        }
                        //        else
                        //        {
                        //            distance = (P0 - g0).magnitude;
                        //        }
                        //    }

                        //}

                        //if (distance > halfDiagonal) continue;

                        //=====================================================================================================//


                        //三角形の面とボクセルのyz平面2面との交差
                        for (int i0 = 0; i0 < 2; ++i0)
                        {
                            P3.x = surveyVoxel[i][j][k].pos.x + (2 * i0 - 1) * (surveyLengXZ / 2.0f);

                            //三角形の面とボクセルのyz面が一致する場合
                            //if ((P0.x == P3.x) && (P1.x == P3.x) && (P2.x == P3.x))
                            if ((Math.Abs(P0.x - P3.x) < 1.0e-8) && (Math.Abs(P1.x - P3.x) < 1.0e-8) && (Math.Abs(P2.x - P3.x) < 1.0e-8))
                            {
                                area = (Vector3.Cross((P0 - P1), (P0 - P2))).magnitude / 2.0f;
                                // float a=Vector3.(P0)
                                area012 = 0.0f;
                                P.x = P3.x;
                                for (int j1 = 0; j1 < 2; ++j1)
                                {
                                    P.y = surveyVoxel[i][j][k].pos.y + (float)(2 * j1 - 1) * (surveyLengY / 2.0f);
                                    for (int k1 = 0; k1 < 2; ++k1)
                                    {
                                        P.z = surveyVoxel[i][j][k].pos.z + (float)(2 * k1 - 1) * (surveyLengXZ / 2.0f);
                                        area012 += Vector3.Cross((P0 - P), (P1 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P1 - P), (P2 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P2 - P), (P0 - P)).magnitude / 2.0f;
                                        //if (area == area012)
                                        if (Math.Abs(area - area012) < 1.0e-8)
                                        {
                                            surveyVoxel[i][j][k].cross = true;
                                            surveyVoxel[i][j][k].surface = true;
                                            cross = true;
                                            break;
                                        }
                                    }
                                    if (cross) break;
                                }
                                if (cross) break;
                            }

                            //三角形の面とボクセルのyz面が一致しない場合
                            else
                            {
                                seki01 = (P0.x - P3.x) * (P1.x - P3.x);
                                seki12 = (P1.x - P3.x) * (P2.x - P3.x);
                                seki20 = (P2.x - P3.x) * (P0.x - P3.x);
                                P4.x = P3.x;

                                //P2.xのみ異符号
                                if ((seki01 > 0.0) && (seki12 < 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.x - P0.x) / (P2.x - P0.x);
                                    t4 = (P4.x - P1.x) / (P2.x - P1.x);
                                    P3 = (P2 - P0) * t3 + P0;
                                    P4 = (P2 - P1) * t4 + P1;
                                }
                                //P0.xのみ異符号
                                else if ((seki01 < 0.0) && (seki12 > 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.x - P1.x) / (P0.x - P1.x);
                                    t4 = (P4.x - P2.x) / (P0.x - P2.x);
                                    P3 = (P0 - P1) * t3 + P1;
                                    P4 = (P0 - P2) * t4 + P2;
                                }
                                //P1.xのみ異符号
                                else if ((seki01 < 0.0) && (seki12 < 0.0) && (seki20 > 0.0))
                                {
                                    t3 = (P3.x - P0.x) / (P1.x - P0.x);
                                    t4 = (P4.x - P2.x) / (P1.x - P2.x);
                                    P3 = (P1 - P0) * t3 + P0;
                                    P4 = (P1 - P2) * t4 + P2;
                                }
                                else
                                {
                                    continue;
                                }
                                //t3は線分内(0.0<=t3<=1.0)
                                //if ((t3 < 0.0) || (1.0 < t3))
                                //{
                                //cout << "!" << endl;
                                //	continue;
                                //}
                                //t4は線分内(0.0<=t4<=1.0)
                                //if ((t4 < 0.0) || (1.0 < t4))
                                //{
                                //cout << "!!" << endl;
                                //	continue;
                                //}
                                //三角形とボクセルのyz平面の交線とボクセルの辺y方向が交差するかどうか調査
                                for (int j0 = 0; j0 < 2; ++j0)
                                {
                                    P34.y = surveyVoxel[i][j][k].pos.y + (2 * j0 - 1) * (surveyLengY / 2.0f);
                                    t34 = (P34.y - P3.y) / (P4.y - P3.y);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34))
                                    {
                                        //cout << "!" << endl;
                                        continue;
                                    }
                                    P34.z = (P4.z - P3.z) * t34 + P3.z;
                                    if ((surveyVoxel[i][j][k].pos.z - (surveyLengXZ / 2.0) <= P34.z) && (P34.z <= surveyVoxel[i][j][k].pos.z + (surveyLengXZ / 2.0)))
                                    {
                                        surveyVoxel[i][j][k].cross = true;
                                        surveyVoxel[i][j][k].surface = true;
                                        cross = true;
                                        break;
                                    }
                                }
                                if (cross) break;
                                //三角形とボクセルのyz平面の交線とボクセルの辺z方向が交差するかどうか調査
                                for (int k0 = 0; k0 < 2; ++k0)
                                {
                                    P34.z = surveyVoxel[i][j][k].pos.z + (2 * k0 - 1) * (surveyLengXZ / 2.0f);
                                    t34 = (P34.z - P3.z) / (P4.z - P3.z);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    P34.y = (P4.y - P3.y) * t34 + P3.y;
                                    if ((surveyVoxel[i][j][k].pos.y - (surveyLengY / 2.0) <= P34.y) && (P34.y <= surveyVoxel[i][j][k].pos.y + (surveyLengY / 2.0)))
                                    {
                                        surveyVoxel[i][j][k].cross = true;
                                        surveyVoxel[i][j][k].surface = true;
                                        cross = true;
                                        break;
                                    }
                                }
                                if (cross) break;
                            }
                        }
                        if (cross) continue;

                        //三角形の面とボクセルのxz平面2面との交差
                        for (int j0 = 0; j0 < 2; ++j0)
                        {
                            P3.y = surveyVoxel[i][j][k].pos.y + (2 * j0 - 1) * (surveyLengY / 2.0f);

                            //三角形の面とボクセルのxz面が一致する場合
                            //if ((P0.y == P3.y) && (P1.y == P3.y) && (P2.y == P3.y))
                            if ((Math.Abs(P0.y - P3.y) < 1.0e-8) && (Math.Abs(P1.y - P3.y) < 1.0e-8) && (Math.Abs(P2.y - P3.y) < 1.0e-8))
                            {
                                area = Vector3.Cross((P0 - P1), (P0 - P2)).magnitude / 2.0f;
                                area012 = 0.0f;
                                P.y = P3.y;
                                for (int i1 = 0; i1 < 2; ++i1)
                                {
                                    P.x = surveyVoxel[i][j][k].pos.x + (float)(2 * i1 - 1) * (surveyLengXZ / 2.0f);
                                    for (int k1 = 0; k1 < 2; ++k1)
                                    {
                                        P.z = surveyVoxel[i][j][k].pos.z + (float)(2 * k1 - 1) * (surveyLengXZ / 2.0f);
                                        area012 += Vector3.Cross((P0 - P), (P1 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P1 - P), (P2 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P2 - P), (P0 - P)).magnitude / 2.0f;
                                        //if (area == area012)
                                        if (Math.Abs(area - area012) < 1.0e-8)
                                        {
                                            surveyVoxel[i][j][k].cross = true;
                                            surveyVoxel[i][j][k].surface = true;
                                            cross = true;
                                            break;
                                        }
                                    }
                                    if (cross) break;
                                }
                                if (cross) break;
                            }

                            //三角形の面とボクセルのxz面が一致しない場合
                            else
                            {
                                seki01 = (P0.y - P3.y) * (P1.y - P3.y);
                                seki12 = (P1.y - P3.y) * (P2.y - P3.y);
                                seki20 = (P2.y - P3.y) * (P0.y - P3.y);
                                P4.y = P3.y;

                                //P2.yのみ異符号
                                if ((seki01 > 0.0) && (seki12 < 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.y - P0.y) / (P2.y - P0.y);
                                    t4 = (P4.y - P1.y) / (P2.y - P1.y);
                                    P3 = (P2 - P0) * t3 + P0;
                                    P4 = (P2 - P1) * t4 + P1;
                                }
                                //P0.yのみ異符号
                                else if ((seki01 < 0.0) && (seki12 > 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.y - P1.y) / (P0.y - P1.y);
                                    t4 = (P4.y - P2.y) / (P0.y - P2.y);
                                    P3 = (P0 - P1) * t3 + P1;
                                    P4 = (P0 - P2) * t4 + P2;
                                }
                                //P1.yのみ異符号
                                else if ((seki01 < 0.0) && (seki12 < 0.0) && (seki20 > 0.0))
                                {
                                    t3 = (P3.y - P0.y) / (P1.y - P0.y);
                                    t4 = (P4.y - P2.y) / (P1.y - P2.y);
                                    P3 = (P1 - P0) * t3 + P0;
                                    P4 = (P1 - P2) * t4 + P2;
                                }
                                else
                                {
                                    continue;
                                }
                                //t3は線分内(0.0<=t3<=1.0)
                                if ((t3 < 0.0) || (1.0 < t3)) continue;
                                //t4は線分内(0.0<=t4<=1.0)
                                if ((t4 < 0.0) || (1.0 < t4)) continue;
                                //三角形とボクセルのxz平面の交線とボクセルの辺x方向が交差するかどうか調査
                                for (int i0 = 0; i0 < 2; ++i0)
                                {
                                    P34.x = surveyVoxel[i][j][k].pos.x + (2 * i0 - 1) * (surveyLengXZ / 2.0f);
                                    t34 = (P34.x - P3.x) / (P4.x - P3.x);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    P34 = (P4 - P3) * t34 + P3;
                                    if ((surveyVoxel[i][j][k].pos.z - (surveyLengXZ / 2.0) <= P34.z) && (P34.z <= surveyVoxel[i][j][k].pos.z + (surveyLengXZ / 2.0)))
                                    {
                                        surveyVoxel[i][j][k].cross = true;
                                        surveyVoxel[i][j][k].surface = true;
                                        cross = true;
                                        break;
                                    }
                                }
                                if (cross) break;
                                //三角形とボクセルのxz平面の交線とボクセルの辺z方向が交差するかどうか調査
                                for (int k0 = 0; k0 < 2; ++k0)
                                {
                                    P34.z = surveyVoxel[i][j][k].pos.z + (2 * k0 - 1) * (surveyLengXZ / 2.0f);
                                    t34 = (P34.z - P3.z) / (P4.z - P3.z);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    P34 = (P4 - P3) * t34 + P3;
                                    if ((surveyVoxel[i][j][k].pos.x - (surveyLengXZ / 2.0) <= P34.x) && (P34.x <= surveyVoxel[i][j][k].pos.x + (surveyLengXZ / 2.0)))
                                    {
                                        surveyVoxel[i][j][k].cross = true;
                                        surveyVoxel[i][j][k].surface = true;
                                        cross = true;
                                        break;
                                    }
                                }
                                if (cross) break;
                            }
                        }
                        if (cross) continue;

                        //三角形の面とボクセルのxy平面2面との交差
                        for (int k0 = 0; k0 < 2; ++k0)
                        {
                            P3.z = surveyVoxel[i][j][k].pos.z + (2 * k0 - 1) * (surveyLengXZ / 2.0f);

                            //三角形の面とボクセルのxy面が一致する場合
                            //if ((P0.z == P3.z) && (P1.z == P3.z) && (P2.z == P3.z))
                            if ((Math.Abs(P0.z - P3.z) < 1.0e-8) && (Math.Abs(P1.z - P3.z) < 1.0e-8) && (Math.Abs(P2.z - P3.z) < 1.0e-8))
                            {
                                area = Vector3.Cross((P0 - P1), (P0 - P2)).magnitude / 2.0f;
                                area012 = 0.0f;
                                P.z = P3.z;
                                for (int i1 = 0; i1 < 2; ++i1)
                                {
                                    P.x = surveyVoxel[i][j][k].pos.x + (float)(2 * i1 - 1) * (surveyLengXZ / 2.0f);
                                    for (int j1 = 0; j1 < 2; ++j1)
                                    {
                                        P.y = surveyVoxel[i][j][k].pos.y + (float)(2 * j1 - 1) * (surveyLengY / 2.0f);
                                        area012 += Vector3.Cross((P0 - P), (P1 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P1 - P), (P2 - P)).magnitude / 2.0f;
                                        area012 += Vector3.Cross((P2 - P), (P0 - P)).magnitude / 2.0f;
                                        //if (area == area012)
                                        if (Math.Abs(area - area012) < 1.0e-8)
                                        {
                                            surveyVoxel[i][j][k].cross = true;
                                            surveyVoxel[i][j][k].surface = true;
                                            cross = true;
                                            break;
                                        }
                                    }
                                    if (cross) break;
                                }
                                if (cross) break;
                            }

                            //三角形の面とボクセルのxy面が一致しない場合
                            else
                            {
                                seki01 = (P0.z - P3.z) * (P1.z - P3.z);
                                seki12 = (P1.z - P3.z) * (P2.z - P3.z);
                                seki20 = (P2.z - P3.z) * (P0.z - P3.z);
                                P4.z = P3.z;

                                //P2.zのみ異符号
                                if ((seki01 > 0.0) && (seki12 < 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.z - P0.z) / (P2.z - P0.z);
                                    t4 = (P4.z - P1.z) / (P2.z - P1.z);
                                    P3 = (P2 - P0) * t3 + P0;
                                    P4 = (P2 - P1) * t4 + P1;
                                }
                                //P0.zのみ異符号
                                else if ((seki01 < 0.0) && (seki12 > 0.0) && (seki20 < 0.0))
                                {
                                    t3 = (P3.z - P1.z) / (P0.z - P1.z);
                                    t4 = (P4.z - P2.z) / (P0.z - P2.z);
                                    P3 = (P0 - P1) * t3 + P1;
                                    P4 = (P0 - P2) * t4 + P2;
                                }
                                //P1.zのみ異符号
                                else if ((seki01 < 0.0) && (seki12 < 0.0) && (seki20 > 0.0))
                                {
                                    t3 = (P3.z - P0.z) / (P1.z - P0.z);
                                    t4 = (P4.z - P2.z) / (P1.z - P2.z);
                                    P3 = (P1 - P0) * t3 + P0;
                                    P4 = (P1 - P2) * t4 + P2;
                                }
                                else
                                {
                                    continue;
                                }
                                //t3は線分内(0.0<=t3<=1.0)
                                if ((t3 < 0.0) || (1.0 < t3)) continue;
                                //t4は線分内(0.0<=t4<=1.0)
                                if ((t4 < 0.0) || (1.0 < t4)) continue;
                                //三角形とボクセルのxy平面の交線とボクセルの辺x方向が交差するかどうか調査
                                for (int i0 = 0; i0 < 2; ++i0)
                                {
                                    P34.x = surveyVoxel[i][j][k].pos.x + (2 * i0 - 1) * (surveyLengXZ / 2.0f);
                                    t34 = (P34.x - P3.x) / (P4.x - P3.x);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    P34 = (P4 - P3) * t34 + P3;
                                    if ((surveyVoxel[i][j][k].pos.y - (surveyLengY / 2.0) <= P34.y) && (P34.y <= surveyVoxel[i][j][k].pos.y + (surveyLengY / 2.0)))
                                    {
                                        surveyVoxel[i][j][k].cross = true;
                                        surveyVoxel[i][j][k].surface = true;
                                        cross = true;
                                        break;
                                    }
                                }
                                if (cross) break;
                                //三角形とボクセルのxy平面の交線とボクセルの辺y方向が交差するかどうか調査
                                for (int j0 = 0; j0 < 2; ++j0)
                                {
                                    P34.y = surveyVoxel[i][j][k].pos.y + (2 * j0 - 1) * (surveyLengY / 2.0f);
                                    t34 = (P34.y - P3.y) / (P4.y - P3.y);
                                    //t34は線分内(0.0<=t34<=1.0)
                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                    //P34.x = (P4.x - P3.x)*t34 + P3.x;
                                    P34 = (P4 - P3) * t34 + P3;
                                    if ((surveyVoxel[i][j][k].pos.x - (surveyLengXZ / 2.0) <= P34.x) && (P34.x <= surveyVoxel[i][j][k].pos.x + (surveyLengXZ / 2.0)))
                                    {
                                        surveyVoxel[i][j][k].cross = true;
                                        surveyVoxel[i][j][k].surface = true;
                                        cross = true;
                                        break;
                                    }
                                }
                                if (cross) break;
                            }
                        }
                        if (cross) continue;

                    }
                }
            }
        }
    }

    //メッシュ面を含むボクセルの判定
    public void SurveyCrossFaceDistance(Solid m)
    {
        //計算中のHalfEdge
        HalfEdge temp;

        //各三角形のバウンディングボックス
        Vector3 small, large;

        //バウンディングボックスの余白
        Vector3 unitVox = new Vector3(surveyLengXZ / 2.0f, surveyLengY / 2.0f, surveyLengXZ / 2.0f);

        Vector3 P0, P1, P2;
        float seki01, seki12, seki20;

        Vector3 P;
        // float area01, area12, area20;
        float area, area012;

        bool cross = false;//このボクセルが交差しているなら

        float t3, t4;//三角形の辺の直線の式のパラメーター
        Vector3 P3, P4;//ボクセルの面と三角形の辺の直線との交点

        Vector3 P34;//三角形の面とボクセルのいずれかの面の交線上の点
        float t34;//三角形の面とボクセルのいずれかの面の交線の式のパラメーター
        float halfDiagonal;

        float cosAlpha;
        //Vector3 g0, g1, g2, v1, v2, v3;
        float g0g1Mag;
        float f1, f2, f3;

        float xzDiaSquare;
        xzDiaSquare = LengthXZ * LengthXZ;
        //xzDia = Mathf.Sqrt(LengthXZ * LengthXZ * 2f);
        halfDiagonal = Mathf.Sqrt(xzDiaSquare + LengthY * LengthY) / 2f;



        int count = 0;
        //各三角形について調べる
        foreach (var f in m.sfaces)
        {
            f.colorNum = count;
            ++count;

            temp = f.fedges;
            small = temp.hvert.vr;
            large = temp.hvert.vr;

            //各三角形のバウンディングボックスを調べる
            for (int i = 0; i < f.sidednum - 1; ++i)
            {
                temp = temp.next;
                if (small.x > temp.hvert.vr.x) small.x = temp.hvert.vr.x;
                if (large.x < temp.hvert.vr.x) large.x = temp.hvert.vr.x;

                if (small.y > temp.hvert.vr.y) small.y = temp.hvert.vr.y;
                if (large.y < temp.hvert.vr.y) large.y = temp.hvert.vr.y;

                if (small.z > temp.hvert.vr.z) small.z = temp.hvert.vr.z;
                if (large.z < temp.hvert.vr.z) large.z = temp.hvert.vr.z;
            }

            small -= unitVox;
            large += unitVox;

            //各三角形のバウンディングボックスと各ボクセルとの交差判定
            for (int i = 0; i < surveyBoxSize[0]; ++i)
            {
                //各三角形のバウンディングボックスのx方向による判定
                if ((surveyVoxel[i][0][0].pos.x < small.x) || (large.x < surveyVoxel[i][0][0].pos.x)) continue;

                for (int j = 0; j < surveyBoxSize[1]; ++j)
                {
                    //各三角形のバウンディングボックスのy方向による判定
                    if ((surveyVoxel[0][j][0].pos.y < small.y) || (large.y < surveyVoxel[0][j][0].pos.y)) continue;

                    for (int k = 0; k < surveyBoxSize[2]; ++k)
                    {
                        //各三角形のバウンディングボックスのz方向による判定
                        if ((surveyVoxel[0][0][k].pos.z < small.z) || (large.z < surveyVoxel[0][0][k].pos.z)) continue;

                        if (surveyVoxel[i][j][k].cross) continue;

                        //三角形の面がボクセルのいずれかの面と交差
                        cross = false;
                        P0 = f.fedges.hvert.vr;
                        P1 = f.fedges.next.hvert.vr;
                        P2 = f.fedges.next.next.hvert.vr;

                        //Journal of Chemical and Pharmaceutical Research, 2013, 5(9):420-427 Reading and voxelization of 3D modelsより，この距離以下のボクセルは省く
                        Vector3 g0, g1, g2, v1, v2, v3;
                        g0 = new Vector3();
                        g1 = new Vector3();
                        g2 = new Vector3();
                        v1 = new Vector3();
                        v2 = new Vector3();
                        v3 = new Vector3();



                        g0 = surveyVoxel[i][j][k].pos;

                        f.CalNormal();
                        cosAlpha = Vector3.Dot((g0 - P0).normalized, f.normal.normalized);
                        //cosAlpha = Vector3.Dot(g0 - P0, f.normal) / (g0 - P0).magnitude / f.normal.magnitude;
                        g0g1Mag = (P0 - g0).magnitude * cosAlpha;//ここ違うのではないか
                        g1 = g0 - g0g1Mag * Vector3.Normalize(f.normal);//プラスマイナス逆にしても意味ないぞ？
                        //g1 = g0 - g0g1Mag * Vector3.Normalize(f.normal);

                        v1 = Vector3.Normalize(P0 - P1) + Vector3.Normalize(P0 - P2);
                        v2 = Vector3.Normalize(P1 - P2) + Vector3.Normalize(P1 - P0);
                        v3 = Vector3.Normalize(P2 - P0) + Vector3.Normalize(P2 - P1);
                        //これが正なら反時計回り
                        f1 = Vector3.Dot(Vector3.Cross(v1, (g1 - P0)), f.normal);
                        f2 = Vector3.Dot(Vector3.Cross(v2, (g1 - P1)), f.normal);
                        f3 = Vector3.Dot(Vector3.Cross(v3, (g1 - P2)), f.normal);

                        float distance = 0;//三角形メッシュの面からボクセルの中心の距離

                        if (f1 > 0 && f2 < 0)//線上にあった場合どうすればいいのか
                        {
                            float decideOut;//これが外側の時にマイナス
                            decideOut = Vector3.Dot(Vector3.Cross((P0 - g1), (P1 - g1)), f.normal);//マイナスにしても変わらない
                            if (decideOut > 0)
                            {
                                distance = g0g1Mag;
                                // surveyVoxel[i][j][k].disTri=
                            }
                            else
                            {
                                Vector3 r = new Vector3();
                                r = Vector3.Cross(Vector3.Cross(P1 - g1, P0 - g1), P1 - P0);
                                float cosGanma = Vector3.Dot((P0 - g1).normalized, r.normalized);
                                float g1g2Mag = (P0 - g1).magnitude * cosGanma;
                                Vector3 g1g2 = new Vector3();
                                g1g2 = g1g2Mag * r.normalized;
                                g2 = g1 + g1g2;
                                float t;
                                //辺に近いのか頂点に近いのかの判定

                                if ((P1.x - P0.x) != 0)//これでいいのか
                                {
                                    t = (g2.x - P0.x) / (P1.x - P0.x);

                                }
                                else if ((P1.y - P0.y) != 0)
                                {
                                    t = (g2.y - P0.y) / (P1.y - P0.y);

                                }
                                else
                                {
                                    t = (g2.z - P0.z) / (P1.z - P0.z);

                                }
                                if (0 <= t && t <= 1)
                                {
                                    distance = Mathf.Sqrt(Mathf.Pow((g2 - g1).magnitude, 2f) + Mathf.Pow((g1 - g0).magnitude, 2f));
                                    //distance = Mathf.Sqrt((g2 - g1).sqrMagnitude + (g1 - g0).sqrMagnitude);

                                }
                                else if (t < 0)
                                {
                                    //distance = (P1 - g0).magnitude;
                                    distance = (P0 - g0).magnitude;
                                }
                                else
                                {
                                    //distance = (P0 - g0).magnitude;
                                    distance = (P1 - g0).magnitude;
                                }
                            }
                            if (Mathf.Abs(distance) <= halfDiagonal)
                            {
                                surveyVoxel[i][j][k].cross = true;
                                surveyVoxel[i][j][k].surface = true;
                                // cross = true;
                                // break;

                            }

                        }
                        if (f2 > 0 && f3 < 0)//線上にあった場合どうすればいいのか
                        {
                            float decideOut;//これが外側の時にマイナス
                            decideOut = Vector3.Dot(Vector3.Cross((P1 - g1), (P2 - g1)), f.normal);
                            if (decideOut > 0)
                            {
                                distance = g0g1Mag;
                                // surveyVoxel[i][j][k].disTri=
                            }
                            else
                            {
                                Vector3 r = new Vector3();
                                r = Vector3.Cross(Vector3.Cross(P2 - g1, P1 - g1), P2 - P1);
                                float cosGanma = Vector3.Dot((P1 - g1).normalized, r.normalized);
                                float g1g2Mag = (P1 - g1).magnitude * cosGanma;
                                Vector3 g1g2 = new Vector3();
                                g1g2 = g1g2Mag * r.normalized;
                                g2 = g1 + g1g2;
                                float t = 0;
                                //辺に近いのか頂点に近いのかの判定
                                if ((P2.x - P1.x) != 0)//これでいいのか
                                {
                                    t = (g2.x - P1.x) / (P2.x - P1.x);

                                }
                                else if ((P2.y - P1.y) != 0)
                                {
                                    t = (g2.y - P1.y) / (P2.y - P1.y);

                                }
                                //else
                                else if ((P2.z - P1.z) != 0)
                                {
                                    t = (g2.z - P1.z) / (P2.z - P1.z);

                                }
                                if (0 <= t && t <= 1)
                                {
                                    distance = Mathf.Sqrt((g2 - g1).sqrMagnitude + (g1 - g0).sqrMagnitude);

                                }
                                else if (t < 0)
                                {
                                    //distance = (P2 - g0).magnitude;
                                    distance = (P1 - g0).magnitude;
                                }
                                else
                                {
                                    //distance = (P1 - g0).magnitude;
                                    distance = (P2 - g0).magnitude;
                                }
                            }
                            //if (distance <= halfDiagonal)
                            if (Mathf.Abs(distance) <= halfDiagonal)

                            {
                                surveyVoxel[i][j][k].cross = true;
                                surveyVoxel[i][j][k].surface = true;
                                // cross = true;
                                // break;

                            }
                        }
                        // else
                        if (f3 > 0 && f1 < 0)//線上にあった場合どうすればいいのか
                        {
                            float decideOut;//これが外側の時にマイナス
                            decideOut = Vector3.Dot(Vector3.Cross((P2 - g1), (P0 - g1)), f.normal);
                            if (decideOut > 0)
                            {
                                distance = g0g1Mag;
                                // surveyVoxel[i][j][k].disTri=
                            }
                            else
                            {
                                Vector3 r = new Vector3();
                                r = Vector3.Cross(Vector3.Cross(P0 - g1, P2 - g1), P0 - P2);
                                float cosGanma = Vector3.Dot((P2 - g1).normalized, r.normalized);
                                float g1g2Mag = (P2 - g1).magnitude * cosGanma;
                                Vector3 g1g2 = new Vector3();
                                g1g2 = g1g2Mag * r.normalized;
                                g2 = g1 + g1g2;
                                float t;
                                //辺に近いのか頂点に近いのかの判定
                                if ((P0.x - P2.x) != 0)//これでいいのか
                                {
                                    t = (g2.x - P2.x) / (P0.x - P2.x);

                                }
                                else if ((P0.y - P2.y) != 0)
                                {
                                    t = (g2.y - P2.y) / (P0.y - P2.y);

                                }
                                else
                                {
                                    t = (g2.z - P2.z) / (P0.z - P2.z);

                                }
                                if (0 <= t && t <= 1)
                                {
                                    distance = Mathf.Sqrt((g2 - g1).sqrMagnitude + (g1 - g0).sqrMagnitude);

                                }
                                else if (t < 0)
                                {
                                    //distance = (P0 - g0).magnitude;
                                    distance = (P2 - g0).magnitude;
                                }
                                else
                                {
                                    //distance = (P2 - g0).magnitude;
                                    distance = (P0 - g0).magnitude;
                                }
                            }
                            //if (distance <= halfDiagonal)
                            if (Mathf.Abs(distance) <= halfDiagonal)

                            {
                                surveyVoxel[i][j][k].cross = true;
                                surveyVoxel[i][j][k].surface = true;
                                //  cross = true;
                                // break;

                            }

                        }

                        // if (distance > halfDiagonal)
                        // {
                        // surveyVoxel[i][j][k].cross = true;
                        // surveyVoxel[i][j][k].surface = true;
                        // cross = true;
                        //// break;

                        // }






                    }
                }
            }
            // if (count >= 2) { break; }
        }
    }

    //メッシュ面を含むsurveyボクセルの判定//64分割にしたものに対して辺を取ってきて個別に判定
    public void SurveyCrossFaceEach(Solid m, float divisionNum)
    {
        //計算中のHalfEdge
        HalfEdge temp;

        //各三角形のバウンディングボックス
        Vector3 small, large;

        //バウンディングボックスの余白
        Vector3 unitVox = new Vector3(surveyLengXZ / 2.0f, surveyLengY / 2.0f, surveyLengXZ / 2.0f);

        Vector3 P0, P1, P2;
        float seki01, seki12, seki20;

        Vector3 P;
        // float area01, area12, area20;
        float area, area012;

        bool cross = false;//このボクセルが交差しているなら

        float t3, t4;//三角形の辺の直線の式のパラメーター
        Vector3 P3, P4;//ボクセルの面と三角形の辺の直線との交点

        Vector3 P34;//三角形の面とボクセルのいずれかの面の交線上の点
        float t34;//三角形の面とボクセルのいずれかの面の交線の式のパラメーター

        float division = Mathf.Log(divisionNum, 2);
        division = division / 1.5f;

        //int count = 0;
        //    /*==========================================================================================================*/
        //親交差ボクセルの判定
        for (int x = 0; x < boxSize[0]; ++x)
        {

            for (int y = 0; y < boxSize[1]; ++y)
            {

                for (int z = 0; z < boxSize[2]; ++z)
                {
                    if (voxelGrid[x][y][z].cross)
                    {
                        //各三角形について調べる
                        foreach (var faceNum in voxelGrid[x][y][z].crossedFacesList)
                        {
                            //f.colorNum = count;
                            //++count;

                            temp = m.sfaces[faceNum].fedges;
                            small = temp.hvert.vr;
                            large = temp.hvert.vr;

                            //各三角形のバウンディングボックスを調べる
                            for (int i = 0; i < m.sfaces[faceNum].sidednum - 1; ++i)
                            {
                                temp = temp.next;
                                if (small.x > temp.hvert.vr.x) small.x = temp.hvert.vr.x;
                                if (large.x < temp.hvert.vr.x) large.x = temp.hvert.vr.x;

                                if (small.y > temp.hvert.vr.y) small.y = temp.hvert.vr.y;
                                if (large.y < temp.hvert.vr.y) large.y = temp.hvert.vr.y;

                                if (small.z > temp.hvert.vr.z) small.z = temp.hvert.vr.z;
                                if (large.z < temp.hvert.vr.z) large.z = temp.hvert.vr.z;
                            }

                            small -= unitVox;
                            large += unitVox;

                            //各三角形のバウンディングボックスと各ボクセルとの交差判定
                            for (int i = (int)division * x; i < (int)division * x + (int)division; ++i)
                            {
                                ////各三角形のバウンディングボックスのx方向による判定
                                //if ((surveyVoxel[i][0][0].pos.x < small.x) || (large.x < surveyVoxel[i][0][0].pos.x)) continue;

                                for (int j = (int)division * y; j < (int)division * y + (int)division; ++j)
                                {
                                    ////各三角形のバウンディングボックスのy方向による判定
                                    //if ((surveyVoxel[0][j][0].pos.y < small.y) || (large.y < surveyVoxel[0][j][0].pos.y)) continue;

                                    for (int k = (int)division * z; k < (int)division * z + (int)division; ++k)
                                    {
                                        ////各三角形のバウンディングボックスのz方向による判定
                                        //if ((surveyVoxel[0][0][k].pos.z < small.z) || (large.z < surveyVoxel[0][0][k].pos.z)) continue;

                                        if (surveyVoxel[i][j][k].cross) continue;

                                        //三角形の面がボクセルのいずれかの面と交差
                                        cross = false;

                                        P0 = m.sfaces[faceNum].fedges.hvert.vr;
                                        P1 = m.sfaces[faceNum].fedges.next.hvert.vr;
                                        P2 = m.sfaces[faceNum].fedges.next.next.hvert.vr;

                                        //三角形の面とボクセルのyz平面2面との交差
                                        for (int i0 = 0; i0 < 2; ++i0)
                                        {
                                            P3.x = surveyVoxel[i][j][k].pos.x + (2 * i0 - 1) * (surveyLengXZ / 2.0f);

                                            //三角形の面とボクセルのyz面が一致する場合
                                            //if ((P0.x == P3.x) && (P1.x == P3.x) && (P2.x == P3.x))
                                            if ((Math.Abs(P0.x - P3.x) < 1.0e-8) && (Math.Abs(P1.x - P3.x) < 1.0e-8) && (Math.Abs(P2.x - P3.x) < 1.0e-8))
                                            {
                                                area = (Vector3.Cross((P0 - P1), (P0 - P2))).magnitude / 2.0f;
                                                // float a=Vector3.(P0)
                                                area012 = 0.0f;
                                                P.x = P3.x;
                                                for (int j1 = 0; j1 < 2; ++j1)
                                                {
                                                    P.y = surveyVoxel[i][j][k].pos.y + (float)(2 * j1 - 1) * (surveyLengY / 2.0f);
                                                    for (int k1 = 0; k1 < 2; ++k1)
                                                    {
                                                        P.z = surveyVoxel[i][j][k].pos.z + (float)(2 * k1 - 1) * (surveyLengXZ / 2.0f);
                                                        area012 += Vector3.Cross((P0 - P), (P1 - P)).magnitude / 2.0f;
                                                        area012 += Vector3.Cross((P1 - P), (P2 - P)).magnitude / 2.0f;
                                                        area012 += Vector3.Cross((P2 - P), (P0 - P)).magnitude / 2.0f;
                                                        //if (area == area012)
                                                        if (Math.Abs(area - area012) < 1.0e-8)
                                                        {
                                                            surveyVoxel[i][j][k].cross = true;
                                                            surveyVoxel[i][j][k].surface = true;
                                                            cross = true;
                                                            break;
                                                        }
                                                    }
                                                    if (cross) break;
                                                }
                                                if (cross) break;
                                            }

                                            //三角形の面とボクセルのyz面が一致しない場合
                                            else
                                            {
                                                seki01 = (P0.x - P3.x) * (P1.x - P3.x);
                                                seki12 = (P1.x - P3.x) * (P2.x - P3.x);
                                                seki20 = (P2.x - P3.x) * (P0.x - P3.x);
                                                P4.x = P3.x;

                                                //P2.xのみ異符号
                                                if ((seki01 > 0.0) && (seki12 < 0.0) && (seki20 < 0.0))
                                                {
                                                    t3 = (P3.x - P0.x) / (P2.x - P0.x);
                                                    t4 = (P4.x - P1.x) / (P2.x - P1.x);
                                                    P3 = (P2 - P0) * t3 + P0;
                                                    P4 = (P2 - P1) * t4 + P1;
                                                }
                                                //P0.xのみ異符号
                                                else if ((seki01 < 0.0) && (seki12 > 0.0) && (seki20 < 0.0))
                                                {
                                                    t3 = (P3.x - P1.x) / (P0.x - P1.x);
                                                    t4 = (P4.x - P2.x) / (P0.x - P2.x);
                                                    P3 = (P0 - P1) * t3 + P1;
                                                    P4 = (P0 - P2) * t4 + P2;
                                                }
                                                //P1.xのみ異符号
                                                else if ((seki01 < 0.0) && (seki12 < 0.0) && (seki20 > 0.0))
                                                {
                                                    t3 = (P3.x - P0.x) / (P1.x - P0.x);
                                                    t4 = (P4.x - P2.x) / (P1.x - P2.x);
                                                    P3 = (P1 - P0) * t3 + P0;
                                                    P4 = (P1 - P2) * t4 + P2;
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                                //t3は線分内(0.0<=t3<=1.0)
                                                //if ((t3 < 0.0) || (1.0 < t3))
                                                //{
                                                //cout << "!" << endl;
                                                //	continue;
                                                //}
                                                //t4は線分内(0.0<=t4<=1.0)
                                                //if ((t4 < 0.0) || (1.0 < t4))
                                                //{
                                                //cout << "!!" << endl;
                                                //	continue;
                                                //}
                                                //三角形とボクセルのyz平面の交線とボクセルの辺y方向が交差するかどうか調査
                                                for (int j0 = 0; j0 < 2; ++j0)
                                                {
                                                    P34.y = surveyVoxel[i][j][k].pos.y + (2 * j0 - 1) * (surveyLengY / 2.0f);
                                                    t34 = (P34.y - P3.y) / (P4.y - P3.y);
                                                    //t34は線分内(0.0<=t34<=1.0)
                                                    if ((t34 < 0.0) || (1.0 < t34))
                                                    {
                                                        //cout << "!" << endl;
                                                        continue;
                                                    }
                                                    P34.z = (P4.z - P3.z) * t34 + P3.z;
                                                    if ((surveyVoxel[i][j][k].pos.z - (surveyLengXZ / 2.0) <= P34.z) && (P34.z <= surveyVoxel[i][j][k].pos.z + (surveyLengXZ / 2.0)))
                                                    {
                                                        surveyVoxel[i][j][k].cross = true;
                                                        surveyVoxel[i][j][k].surface = true;
                                                        cross = true;
                                                        break;
                                                    }
                                                }
                                                if (cross) break;
                                                //三角形とボクセルのyz平面の交線とボクセルの辺z方向が交差するかどうか調査
                                                for (int k0 = 0; k0 < 2; ++k0)
                                                {
                                                    P34.z = surveyVoxel[i][j][k].pos.z + (2 * k0 - 1) * (surveyLengXZ / 2.0f);
                                                    t34 = (P34.z - P3.z) / (P4.z - P3.z);
                                                    //t34は線分内(0.0<=t34<=1.0)
                                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                                    P34.y = (P4.y - P3.y) * t34 + P3.y;
                                                    if ((surveyVoxel[i][j][k].pos.y - (surveyLengY / 2.0) <= P34.y) && (P34.y <= surveyVoxel[i][j][k].pos.y + (surveyLengY / 2.0)))
                                                    {
                                                        surveyVoxel[i][j][k].cross = true;
                                                        surveyVoxel[i][j][k].surface = true;
                                                        cross = true;
                                                        break;
                                                    }
                                                }
                                                if (cross) break;
                                            }
                                        }
                                        if (cross) continue;

                                        //三角形の面とボクセルのxz平面2面との交差
                                        for (int j0 = 0; j0 < 2; ++j0)
                                        {
                                            P3.y = surveyVoxel[i][j][k].pos.y + (2 * j0 - 1) * (surveyLengY / 2.0f);

                                            //三角形の面とボクセルのxz面が一致する場合
                                            //if ((P0.y == P3.y) && (P1.y == P3.y) && (P2.y == P3.y))
                                            if ((Math.Abs(P0.y - P3.y) < 1.0e-8) && (Math.Abs(P1.y - P3.y) < 1.0e-8) && (Math.Abs(P2.y - P3.y) < 1.0e-8))
                                            {
                                                area = Vector3.Cross((P0 - P1), (P0 - P2)).magnitude / 2.0f;
                                                area012 = 0.0f;
                                                P.y = P3.y;
                                                for (int i1 = 0; i1 < 2; ++i1)
                                                {
                                                    P.x = surveyVoxel[i][j][k].pos.x + (float)(2 * i1 - 1) * (surveyLengXZ / 2.0f);
                                                    for (int k1 = 0; k1 < 2; ++k1)
                                                    {
                                                        P.z = surveyVoxel[i][j][k].pos.z + (float)(2 * k1 - 1) * (surveyLengXZ / 2.0f);
                                                        area012 += Vector3.Cross((P0 - P), (P1 - P)).magnitude / 2.0f;
                                                        area012 += Vector3.Cross((P1 - P), (P2 - P)).magnitude / 2.0f;
                                                        area012 += Vector3.Cross((P2 - P), (P0 - P)).magnitude / 2.0f;
                                                        //if (area == area012)
                                                        if (Math.Abs(area - area012) < 1.0e-8)
                                                        {
                                                            surveyVoxel[i][j][k].cross = true;
                                                            surveyVoxel[i][j][k].surface = true;
                                                            cross = true;
                                                            break;
                                                        }
                                                    }
                                                    if (cross) break;
                                                }
                                                if (cross) break;
                                            }

                                            //三角形の面とボクセルのxz面が一致しない場合
                                            else
                                            {
                                                seki01 = (P0.y - P3.y) * (P1.y - P3.y);
                                                seki12 = (P1.y - P3.y) * (P2.y - P3.y);
                                                seki20 = (P2.y - P3.y) * (P0.y - P3.y);
                                                P4.y = P3.y;

                                                //P2.yのみ異符号
                                                if ((seki01 > 0.0) && (seki12 < 0.0) && (seki20 < 0.0))
                                                {
                                                    t3 = (P3.y - P0.y) / (P2.y - P0.y);
                                                    t4 = (P4.y - P1.y) / (P2.y - P1.y);
                                                    P3 = (P2 - P0) * t3 + P0;
                                                    P4 = (P2 - P1) * t4 + P1;
                                                }
                                                //P0.yのみ異符号
                                                else if ((seki01 < 0.0) && (seki12 > 0.0) && (seki20 < 0.0))
                                                {
                                                    t3 = (P3.y - P1.y) / (P0.y - P1.y);
                                                    t4 = (P4.y - P2.y) / (P0.y - P2.y);
                                                    P3 = (P0 - P1) * t3 + P1;
                                                    P4 = (P0 - P2) * t4 + P2;
                                                }
                                                //P1.yのみ異符号
                                                else if ((seki01 < 0.0) && (seki12 < 0.0) && (seki20 > 0.0))
                                                {
                                                    t3 = (P3.y - P0.y) / (P1.y - P0.y);
                                                    t4 = (P4.y - P2.y) / (P1.y - P2.y);
                                                    P3 = (P1 - P0) * t3 + P0;
                                                    P4 = (P1 - P2) * t4 + P2;
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                                //t3は線分内(0.0<=t3<=1.0)
                                                if ((t3 < 0.0) || (1.0 < t3)) continue;
                                                //t4は線分内(0.0<=t4<=1.0)
                                                if ((t4 < 0.0) || (1.0 < t4)) continue;
                                                //三角形とボクセルのxz平面の交線とボクセルの辺x方向が交差するかどうか調査
                                                for (int i0 = 0; i0 < 2; ++i0)
                                                {
                                                    P34.x = surveyVoxel[i][j][k].pos.x + (2 * i0 - 1) * (surveyLengXZ / 2.0f);
                                                    t34 = (P34.x - P3.x) / (P4.x - P3.x);
                                                    //t34は線分内(0.0<=t34<=1.0)
                                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                                    P34 = (P4 - P3) * t34 + P3;
                                                    if ((surveyVoxel[i][j][k].pos.z - (surveyLengXZ / 2.0) <= P34.z) && (P34.z <= surveyVoxel[i][j][k].pos.z + (surveyLengXZ / 2.0)))
                                                    {
                                                        surveyVoxel[i][j][k].cross = true;
                                                        surveyVoxel[i][j][k].surface = true;
                                                        cross = true;
                                                        break;
                                                    }
                                                }
                                                if (cross) break;
                                                //三角形とボクセルのxz平面の交線とボクセルの辺z方向が交差するかどうか調査
                                                for (int k0 = 0; k0 < 2; ++k0)
                                                {
                                                    P34.z = surveyVoxel[i][j][k].pos.z + (2 * k0 - 1) * (surveyLengXZ / 2.0f);
                                                    t34 = (P34.z - P3.z) / (P4.z - P3.z);
                                                    //t34は線分内(0.0<=t34<=1.0)
                                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                                    P34 = (P4 - P3) * t34 + P3;
                                                    if ((surveyVoxel[i][j][k].pos.x - (surveyLengXZ / 2.0) <= P34.x) && (P34.x <= surveyVoxel[i][j][k].pos.x + (surveyLengXZ / 2.0)))
                                                    {
                                                        surveyVoxel[i][j][k].cross = true;
                                                        surveyVoxel[i][j][k].surface = true;
                                                        cross = true;
                                                        break;
                                                    }
                                                }
                                                if (cross) break;
                                            }
                                        }
                                        if (cross) continue;

                                        //三角形の面とボクセルのxy平面2面との交差
                                        for (int k0 = 0; k0 < 2; ++k0)
                                        {
                                            P3.z = surveyVoxel[i][j][k].pos.z + (2 * k0 - 1) * (surveyLengXZ / 2.0f);

                                            //三角形の面とボクセルのxy面が一致する場合
                                            //if ((P0.z == P3.z) && (P1.z == P3.z) && (P2.z == P3.z))
                                            if ((Math.Abs(P0.z - P3.z) < 1.0e-8) && (Math.Abs(P1.z - P3.z) < 1.0e-8) && (Math.Abs(P2.z - P3.z) < 1.0e-8))
                                            {
                                                area = Vector3.Cross((P0 - P1), (P0 - P2)).magnitude / 2.0f;
                                                area012 = 0.0f;
                                                P.z = P3.z;
                                                for (int i1 = 0; i1 < 2; ++i1)
                                                {
                                                    P.x = surveyVoxel[i][j][k].pos.x + (float)(2 * i1 - 1) * (surveyLengXZ / 2.0f);
                                                    for (int j1 = 0; j1 < 2; ++j1)
                                                    {
                                                        P.y = surveyVoxel[i][j][k].pos.y + (float)(2 * j1 - 1) * (surveyLengY / 2.0f);
                                                        area012 += Vector3.Cross((P0 - P), (P1 - P)).magnitude / 2.0f;
                                                        area012 += Vector3.Cross((P1 - P), (P2 - P)).magnitude / 2.0f;
                                                        area012 += Vector3.Cross((P2 - P), (P0 - P)).magnitude / 2.0f;
                                                        //if (area == area012)
                                                        if (Math.Abs(area - area012) < 1.0e-8)
                                                        {
                                                            surveyVoxel[i][j][k].cross = true;
                                                            surveyVoxel[i][j][k].surface = true;
                                                            cross = true;
                                                            break;
                                                        }
                                                    }
                                                    if (cross) break;
                                                }
                                                if (cross) break;
                                            }

                                            //三角形の面とボクセルのxy面が一致しない場合
                                            else
                                            {
                                                seki01 = (P0.z - P3.z) * (P1.z - P3.z);
                                                seki12 = (P1.z - P3.z) * (P2.z - P3.z);
                                                seki20 = (P2.z - P3.z) * (P0.z - P3.z);
                                                P4.z = P3.z;

                                                //P2.zのみ異符号
                                                if ((seki01 > 0.0) && (seki12 < 0.0) && (seki20 < 0.0))
                                                {
                                                    t3 = (P3.z - P0.z) / (P2.z - P0.z);
                                                    t4 = (P4.z - P1.z) / (P2.z - P1.z);
                                                    P3 = (P2 - P0) * t3 + P0;
                                                    P4 = (P2 - P1) * t4 + P1;
                                                }
                                                //P0.zのみ異符号
                                                else if ((seki01 < 0.0) && (seki12 > 0.0) && (seki20 < 0.0))
                                                {
                                                    t3 = (P3.z - P1.z) / (P0.z - P1.z);
                                                    t4 = (P4.z - P2.z) / (P0.z - P2.z);
                                                    P3 = (P0 - P1) * t3 + P1;
                                                    P4 = (P0 - P2) * t4 + P2;
                                                }
                                                //P1.zのみ異符号
                                                else if ((seki01 < 0.0) && (seki12 < 0.0) && (seki20 > 0.0))
                                                {
                                                    t3 = (P3.z - P0.z) / (P1.z - P0.z);
                                                    t4 = (P4.z - P2.z) / (P1.z - P2.z);
                                                    P3 = (P1 - P0) * t3 + P0;
                                                    P4 = (P1 - P2) * t4 + P2;
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                                //t3は線分内(0.0<=t3<=1.0)
                                                if ((t3 < 0.0) || (1.0 < t3)) continue;
                                                //t4は線分内(0.0<=t4<=1.0)
                                                if ((t4 < 0.0) || (1.0 < t4)) continue;
                                                //三角形とボクセルのxy平面の交線とボクセルの辺x方向が交差するかどうか調査
                                                for (int i0 = 0; i0 < 2; ++i0)
                                                {
                                                    P34.x = surveyVoxel[i][j][k].pos.x + (2 * i0 - 1) * (surveyLengXZ / 2.0f);
                                                    t34 = (P34.x - P3.x) / (P4.x - P3.x);
                                                    //t34は線分内(0.0<=t34<=1.0)
                                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                                    P34 = (P4 - P3) * t34 + P3;
                                                    if ((surveyVoxel[i][j][k].pos.y - (surveyLengY / 2.0) <= P34.y) && (P34.y <= surveyVoxel[i][j][k].pos.y + (surveyLengY / 2.0)))
                                                    {
                                                        surveyVoxel[i][j][k].cross = true;
                                                        surveyVoxel[i][j][k].surface = true;
                                                        cross = true;
                                                        break;
                                                    }
                                                }
                                                if (cross) break;
                                                //三角形とボクセルのxy平面の交線とボクセルの辺y方向が交差するかどうか調査
                                                for (int j0 = 0; j0 < 2; ++j0)
                                                {
                                                    P34.y = surveyVoxel[i][j][k].pos.y + (2 * j0 - 1) * (surveyLengY / 2.0f);
                                                    t34 = (P34.y - P3.y) / (P4.y - P3.y);
                                                    //t34は線分内(0.0<=t34<=1.0)
                                                    if ((t34 < 0.0) || (1.0 < t34)) continue;
                                                    //P34.x = (P4.x - P3.x)*t34 + P3.x;
                                                    P34 = (P4 - P3) * t34 + P3;
                                                    if ((surveyVoxel[i][j][k].pos.x - (surveyLengXZ / 2.0) <= P34.x) && (P34.x <= surveyVoxel[i][j][k].pos.x + (surveyLengXZ / 2.0)))
                                                    {
                                                        surveyVoxel[i][j][k].cross = true;
                                                        surveyVoxel[i][j][k].surface = true;
                                                        cross = true;
                                                        break;
                                                    }
                                                }
                                                if (cross) break;
                                            }
                                        }
                                        if (cross) continue;

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }



        //    /*==========================================================================================================*/



    }


    //boxelGridとその中のbool型(cross)を元にボクセルを生成する
    public void CreateVoxel(GameObject voxelbase, GameObject parentVoxel)
    {
        var size = new Vector3(LengthXZ, LengthY, LengthXZ);

        for (int size_x = 0; size_x < boxSize[0]; size_x++)
        {

            for (int size_y = 0; size_y < boxSize[1]; size_y++)
            {

                for (int size_z = 0; size_z < boxSize[2]; size_z++)
                {
                    if (voxelGrid[size_x][size_y][size_z].cross)
                    {
                        GameObject cube = GameObject.Instantiate(voxelbase);// GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.position = voxelGrid[size_x][size_y][size_z].pos;
                        cube.transform.localScale = size;

                        // cube.transform.parent = parentVoxel.transform;
                        cube.transform.SetParent(parentVoxel.transform);
                        //BoxCollider collider = new BoxCollider();


                        // GameObject.Destroy(cube, 5f);
                        //GameObject.DestroyImmediate(cube);

                        // cube = null;
                        //   cube.transform.localScale.x = 2.0f;
                    }
                }

            }

        }
    }

    //boxelGridとその中のbool型(cross)を元にボクセルを生成する//colliderの大きさを変更させた
    //enableをfalseにした
    public void CreateVoxelSurf(GameObject voxelbase, GameObject parentVoxel)
    {
        var size = new Vector3(LengthXZ, LengthY, LengthXZ);

        for (int size_x = 0; size_x < boxSize[0]; size_x++)
        {

            for (int size_y = 0; size_y < boxSize[1]; size_y++)
            {

                for (int size_z = 0; size_z < boxSize[2]; size_z++)
                {
                    if (voxelGrid[size_x][size_y][size_z].surface)
                    {
                        GameObject cube = GameObject.Instantiate(voxelbase);// GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.position = voxelGrid[size_x][size_y][size_z].pos;
                        cube.transform.localScale = size;

                        // cube.transform.parent = parentVoxel.transform;
                        cube.transform.SetParent(parentVoxel.transform);
                        BoxCollider collider = cube.GetComponent<BoxCollider>();
                        collider.size = size;

                        cube.name = size_x.ToString() + " " + size_y.ToString() + " " + size_z.ToString();
                        Renderer rend;
                        rend = cube.GetComponent<Renderer>();
                        rend.enabled = false;                        //BoxCollider collider = new BoxCollider();


                        // GameObject.Destroy(cube, 5f);
                        //GameObject.DestroyImmediate(cube);

                        // cube = null;
                        //   cube.transform.localScale.x = 2.0f;
                    }
                }

            }

        }
    }

    //boxelGridとその中のbool型(surface)を元にボクセルを生成する
    public void CreateVoxelSurfNew(GameObject voxelbase, GameObject parentVoxel)
    {
        var size = new Vector3(LengthXZ, LengthY, LengthXZ);

        for (int size_x = 0; size_x < boxSize[0]; size_x++)
        {

            for (int size_y = 0; size_y < boxSize[1]; size_y++)
            {

                for (int size_z = 0; size_z < boxSize[2]; size_z++)
                {
                    if (voxelGrid[size_x][size_y][size_z].surface)

                        if (!voxelGrid[size_x][size_y][size_z].cross)
                        {
                            GameObject cube = GameObject.Instantiate(voxelbase);// GameObject.CreatePrimitive(PrimitiveType.Cube);
                            cube.transform.position = voxelGrid[size_x][size_y][size_z].pos;
                            cube.transform.localScale = size;

                            // cube.transform.parent = parentVoxel.transform;
                            cube.transform.SetParent(parentVoxel.transform);
                            //BoxCollider collider = new BoxCollider();


                            // GameObject.Destroy(cube, 5f);
                            //GameObject.DestroyImmediate(cube);

                            // cube = null;
                            //   cube.transform.localScale.x = 2.0f;
                        }
                }

            }

        }
    }

    //boxelGridとその中のbool型(cross)を元にボクセルを生成する,Plyデータで色を付ける
    public void PlyCreateVoxel(GameObject voxelbase, GameObject parentVoxel)
    {
        var size = new Vector3(LengthXZ, LengthY, LengthXZ);

        for (int size_x = 0; size_x < boxSize[0]; size_x++)
        {

            for (int size_y = 0; size_y < boxSize[1]; size_y++)
            {

                for (int size_z = 0; size_z < boxSize[2]; size_z++)
                {
                    if (voxelGrid[size_x][size_y][size_z].surface)
                    //if (voxelGrid[size_x][size_y][size_z].Fill)
                    //if (voxelGrid[size_x][size_y][size_z].cross)
                    {
                        GameObject cube = GameObject.Instantiate(voxelbase);
                        // GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.position = voxelGrid[size_x][size_y][size_z].pos;
                        cube.transform.localScale = size;
                        //Material mat = cube.GetComponent<Renderer>().material;

                        //     Material mat = cube.GetComponent<Renderer>().material;
                        float r = voxelGrid[size_x][size_y][size_z].color.r;
                        float g = voxelGrid[size_x][size_y][size_z].color.g;
                        float b = voxelGrid[size_x][size_y][size_z].color.b;

                        r = r / 255f;
                        g = g / 255f;
                        b = b / 255f;

                        //  Debug.Log(voxelGrid[size_x][size_y][size_z].voxelColor.r);
                        // mat.color = new Color(r,g,b);
                        cube.GetComponent<Renderer>().material.SetColor("_Color", new Color(r, g, b, 1));
                        //mat.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);

                        // cube.transform.parent = parentVoxel.transfosrm;
                        cube.transform.SetParent(parentVoxel.transform);
                        //BoxCollider collider = new BoxCollider();


                        // GameObject.Destroy(cube, 5f);
                        //GameObject.DestroyImmediate(cube);

                        // cube = null;
                        //   cube.transform.localScale.x = 2.0f;
                    }
                }

            }

        }
    }

    //boxelGridとその中のbool型(surface)を元にボクセルを生成した後の関数
    //Textureから取ってきた色を付ける
    public void TexCreateVoxel(GameObject voxelbase, GameObject parentVoxel)
    {
        var size = new Vector3(LengthXZ, LengthY, LengthXZ);

        for (int size_x = 0; size_x < boxSize[0]; size_x++)
        {

            for (int size_y = 0; size_y < boxSize[1]; size_y++)
            {

                for (int size_z = 0; size_z < boxSize[2]; size_z++)
                {
                    if (voxelGrid[size_x][size_y][size_z].surface)
                    //if (voxelGrid[size_x][size_y][size_z].Fill)
                    //if (voxelGrid[size_x][size_y][size_z].cross)
                    {
                        GameObject cube = GameObject.Instantiate(voxelbase);
                        //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.position = voxelGrid[size_x][size_y][size_z].pos;
                        cube.transform.localScale = size;
                        //Material mat = cube.GetComponent<Renderer>().material;

                        //     Material mat = cube.GetComponent<Renderer>().material;

                        //var num = voxelGrid[size_x][size_y][size_z].rayCount;


                        //float r = voxelGrid[size_x][size_y][size_z].color.r / num;
                        //float g = voxelGrid[size_x][size_y][size_z].color.g / num;
                        //float b = voxelGrid[size_x][size_y][size_z].color.b / num;

                        //float r = voxelGrid[size_x][size_y][size_z].color.r;
                        //float g = voxelGrid[size_x][size_y][size_z].color.g;
                        //float b = voxelGrid[size_x][size_y][size_z].color.b;

                        string name;
                        name = "voxel " + size_x.ToString() + " " + size_y.ToString() + " " + size_z.ToString();

                        cube.name = name;

                        //  Debug.Log(voxelGrid[size_x][size_y][size_z].voxelColor.r);
                        // mat.color = new Color(r,g,b);

                        //cube.GetComponent<Renderer>().material.color = new Color(r, g, b, 1);

                        /*--------------------------------------------------------------------------*/
                        //float min;//最小値
                        //int min_no = 0;//最小値が何番目なのか
                        //Color temp=new Color();
                        //float tempDeltaE;

                        //float r = voxelGrid[size_x][size_y][size_z].color.r;
                        //float g = voxelGrid[size_x][size_y][size_z].color.g;
                        //float b = voxelGrid[size_x][size_y][size_z].color.b;
                        ////Debug.Log(r + " " + g + " " + b);

                        //voxelGrid[size_x][size_y][size_z].color.r = 255f * r;
                        //voxelGrid[size_x][size_y][size_z].color.g = 255f * g;
                        //voxelGrid[size_x][size_y][size_z].color.b = 255f * b;
                        //Debug.Log(voxelGrid[size_x][size_y][size_z].color.r + " " + voxelGrid[size_x][size_y][size_z].color.g + " " + voxelGrid[size_x][size_y][size_z].color.b);


                        //temp = RGB_to_Lab(voxelGrid[size_x][size_y][size_z].color);

                        ////Debug.Log(temp.r + " " + temp.g + " " + temp.b);

                        //min = 3.402823466e+38f; // max value                  //最小値をfloat型の最大とおく
                        //min_no = 0;
                        //for (int i = 0; i < legoColor.Count(); ++i)
                        //{
                        //    //なぜLabにしているのか
                        //    tempDeltaE = DeltaE(legoColor_Lab[i], temp);
                        //    if (tempDeltaE < min)
                        //    {
                        //        min = tempDeltaE;           //最小値の更新
                        //        min_no = i;                     //最小値が何番目か
                        //    }
                        //}
                        //voxelGrid[size_x][size_y][size_z].color = legoColor[min_no];
                        //voxelGrid[size_x][size_y][size_z].colorID = min_no;
                        /*--------------------------------------------------------------------------*/
                        //voxelGrid[size_x][size_y][size_z].color.r=voxelGrid[size_x][size_y][size_z].color.r/255f;
                        //voxelGrid[size_x][size_y][size_z].color.g=voxelGrid[size_x][size_y][size_z].color.g/255f;
                        //voxelGrid[size_x][size_y][size_z].color.b= voxelGrid[size_x][size_y][size_z].color.b/255f;

                        voxelGrid[size_x][size_y][size_z].color.r = voxelGrid[size_x][size_y][size_z].color.r ;
                        voxelGrid[size_x][size_y][size_z].color.g = voxelGrid[size_x][size_y][size_z].color.g ;
                        voxelGrid[size_x][size_y][size_z].color.b = voxelGrid[size_x][size_y][size_z].color.b ;

                        cube.GetComponent<Renderer>().material.color = voxelGrid[size_x][size_y][size_z].color;
                        //cube.GetComponent<Renderer>().material.color = new Color(0,0,0,1);
                        //Debug.Log(r + ", " + g + ", " + b);

                        //BoxCollider collider = new BoxCollider();

                        //mat.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);

                        // cube.transform.parent = parentVoxel.transfosrm;
                        //cube.transform.SetParent(parentVoxel.transform);
                        //BoxCollider collider = new BoxCollider();


                        // GameObject.Destroy(cube, 5f);
                        //GameObject.DestroyImmediate(cube);

                        // cube = null;
                        //   cube.transform.localScale.x = 2.0f;


                    }
                }

            }

        }
    }

    //boxelGridとその中のbool型(surface)を元にボクセルを生成した後の関数
    //Textureから取ってきた色を付ける
    public void TexColorToVoxel()
    {
        var size = new Vector3(LengthXZ, LengthY, LengthXZ);

        for (int size_x = 0; size_x < boxSize[0]; size_x++)
        {

            for (int size_y = 0; size_y < boxSize[1]; size_y++)
            {

                for (int size_z = 0; size_z < boxSize[2]; size_z++)
                {
                    if (voxelGrid[size_x][size_y][size_z].surface)
                    //if (voxelGrid[size_x][size_y][size_z].Fill)
                    //if (voxelGrid[size_x][size_y][size_z].cross)
                    {
                        //if (voxelGrid[size_x][size_y][size_z].rayCount>0)
                        //{
                        string name;
                        name = size_x.ToString() + " " + size_y.ToString() + " " + size_z.ToString();
                        GameObject cube;
                        cube = GameObject.Find(name);
                        if (cube != null)
                        {
                            var num = voxelGrid[size_x][size_y][size_z].rayCount;


                            float r = voxelGrid[size_x][size_y][size_z].color.r / num;
                            float g = voxelGrid[size_x][size_y][size_z].color.g / num;
                            float b = voxelGrid[size_x][size_y][size_z].color.b / num;

                            //r = r / 255f;
                            //g = g / 255f;
                            //b = b / 255f;

                            //  Debug.Log(voxelGrid[size_x][size_y][size_z].voxelColor.r);
                            // mat.color = new Color(r,g,b);

                            cube.GetComponent<Renderer>().material.SetColor("_Color", new Color(r, g, b, 1));

                            Renderer rend;
                            rend = cube.GetComponent<Renderer>();
                            rend.enabled = true;                        //BoxCollider collider = new BoxCollider();

                            //mat.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);

                            // cube.transform.parent = parentVoxel.transfosrm;
                            //cube.transform.SetParent(parentVoxel.transform);
                            //BoxCollider collider = new BoxCollider();


                            // GameObject.Destroy(cube, 5f);
                            //GameObject.DestroyImmediate(cube);

                            // cube = null;
                            //   cube.transform.localScale.x = 2.0f;
                        }
                        //}
                    }
                }

            }

        }
    }


    //boxelGridとその中のbool型(Fill)を元にボクセルを生成する
    public void CreateVoxel_Fill(GameObject voxelbase, GameObject parentVoxel)
    {
        var size = new Vector3(LengthXZ, LengthY, LengthXZ);

        for (int size_x = 0; size_x < boxSize[0]; size_x++)
        {

            for (int size_y = 0; size_y < boxSize[1]; size_y++)
            {

                for (int size_z = 0; size_z < boxSize[2]; size_z++)
                {
                    if (voxelGrid[size_x][size_y][size_z].Fill)
                    {
                        GameObject cube = GameObject.Instantiate(voxelbase);// GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.position = voxelGrid[size_x][size_y][size_z].pos;
                        cube.transform.localScale = size;


                        /*-----------------------------------------------------------------*/
                        //Material mat = cube.GetComponent<Renderer>().material;



                        //mat.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                        /*-----------------------------------------------------------------*/








                        // cube.transform.parent = parentVoxel.transform;
                        cube.transform.SetParent(parentVoxel.transform);
                        //BoxCollider collider = new BoxCollider();


                        // GameObject.Destroy(cube, 5f);
                        //GameObject.DestroyImmediate(cube);

                        // cube = null;
                        //   cube.transform.localScale.x = 2.0f;
                    }
                }

            }

        }
    }

    //boxelGridとその中のbool型(Fill)を元にボクセルを生成するcolliderの大きさを変化させる
    public void CreateVoxelFillCol(GameObject voxelbase, GameObject parentVoxel)
    {
        var size = new Vector3(LengthXZ, LengthY, LengthXZ);

        for (int size_x = 0; size_x < boxSize[0]; size_x++)
        {

            for (int size_y = 0; size_y < boxSize[1]; size_y++)
            {

                for (int size_z = 0; size_z < boxSize[2]; size_z++)
                {
                    if (voxelGrid[size_x][size_y][size_z].Fill)
                    {
                        GameObject cube = GameObject.Instantiate(voxelbase);// GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.position = voxelGrid[size_x][size_y][size_z].pos;
                        cube.transform.localScale = size;


                        /*-----------------------------------------------------------------*/
                        //Material mat = cube.GetComponent<Renderer>().material;



                        //mat.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                        /*-----------------------------------------------------------------*/








                        // cube.transform.parent = parentVoxel.transform;
                        cube.transform.SetParent(parentVoxel.transform);
                        //BoxCollider collider = new BoxCollider();


                        // GameObject.Destroy(cube, 5f);
                        //GameObject.DestroyImmediate(cube);

                        // cube = null;
                        //   cube.transform.localScale.x = 2.0f;
                    }
                }

            }

        }
    }

    //boxelGridとその中のbool型(Fill)を元にボクセルを生成する
    public void CreateSurveyVoxel_Fill(GameObject voxelbase, GameObject parentVoxel)
    {
        var size = new Vector3(surveyLengXZ, surveyLengY, surveyLengXZ);

        for (int size_x = 0; size_x < surveyBoxSize[0]; size_x++)
        {

            for (int size_y = 0; size_y < surveyBoxSize[1]; size_y++)
            {

                for (int size_z = 0; size_z < surveyBoxSize[2]; size_z++)
                {
                    if (surveyVoxel[size_x][size_y][size_z].Fill)
                    {
                        GameObject cube = GameObject.Instantiate(voxelbase);// GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.position = surveyVoxel[size_x][size_y][size_z].pos;
                        cube.transform.localScale = size;


                        /*-----------------------------------------------------------------*/
                        //Material mat = cube.GetComponent<Renderer>().material;



                        //mat.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                        /*-----------------------------------------------------------------*/








                        // cube.transform.parent = parentVoxel.transform;
                        cube.transform.SetParent(parentVoxel.transform);
                        //BoxCollider collider = new BoxCollider();


                        // GameObject.Destroy(cube, 5f);
                        //GameObject.DestroyImmediate(cube);

                        // cube = null;
                        //   cube.transform.localScale.x = 2.0f;
                    }
                }

            }

        }
    }
    //各ボクセルの情報にcrossがtrueならFillに渡す
    //間違えた，使ってない
    public void CrossToFill()
    {

        for (int i = 0; i < boxSize[0]; i++)
            for (int j = 0; j < boxSize[1]; j++)
                for (int k = 0; k < boxSize[2]; k++)
                {
                    if (voxelGrid[i][j][k].cross)
                    {
                        voxelGrid[i][j][k].Fill = true;
                    }

                }
    }

    //最表面ボクセルの判定
    public void SurfaceVoxel()
    {
        for (int i = 1; i < boxSize[0] - 1; ++i)
        {
            for (int j = 1; j < boxSize[1] - 1; ++j)
            {
                for (int k = 1; k < boxSize[2] - 1; ++k)
                {
                    if (voxelGrid[i][j][k].surface)
                    {
                        if ((voxelGrid[i - 1][j][k].Fill) && (voxelGrid[i + 1][j][k].Fill) &&
                            (voxelGrid[i][j - 1][k].Fill) && (voxelGrid[i][j + 1][k].Fill) &&
                            (voxelGrid[i][j][k - 1].Fill) && (voxelGrid[i][j][k + 1].Fill))
                        {
                            voxelGrid[i][j][k].surface = false;
                        }
                    }
                }
            }
        }
    }

    //ボクセルの内部をボクセルで埋める
    public int InnerVoxelFill()
    {

        //ボクセル情報の初期化と最端部のボクセルの調査
        for (int i = 0; i < boxSize[0]; i++)
            for (int j = 0; j < boxSize[1]; j++)
                for (int k = 0; k < boxSize[2]; k++)
                {
                    voxelGrid[i][j][k].num = 0;
                    if ((i == 0) || (j == 0) || (k == 0) || (i == boxSize[0] - 1) || (j == boxSize[1] - 1) || (k == boxSize[2] - 1))
                    {
                        voxelGrid[i][j][k].edge = true;
                        voxelGrid[i][j][k].Fill = false;
                        voxelGrid[i][j][k].num = 1;
                        if (voxelGrid[i][j][k].surface)
                            voxelGrid[i][j][k].Fill = true;
                    }
                }

        //端部のボクセルの調査
        for (int i = 0; i < boxSize[0]; i++)
            for (int j = 0; j < boxSize[1]; j++)
            {
                if ((!voxelGrid[i][j][0].surface) && (!voxelGrid[i][j][1].surface))
                {
                    voxelGrid[i][j][1].Fill = false;
                    voxelGrid[i][j][1].num = 2;
                }
                if ((!voxelGrid[i][j][boxSize[2] - 1].surface) && (!voxelGrid[i][j][boxSize[2] - 2].surface))
                {
                    voxelGrid[i][j][boxSize[2] - 2].Fill = false;
                    voxelGrid[i][j][boxSize[2] - 2].num = 2;
                }
            }

        for (int i = 0; i < boxSize[0]; i++)
            for (int k = 0; k < boxSize[2]; k++)
            {
                if ((!voxelGrid[i][0][k].surface) && (!voxelGrid[i][1][k].surface))
                {
                    voxelGrid[i][1][k].Fill = false;
                    voxelGrid[i][1][k].num = 2;
                }
                if ((!voxelGrid[i][boxSize[1] - 1][k].surface) && (!voxelGrid[i][boxSize[1] - 2][k].surface))
                {
                    voxelGrid[i][boxSize[1] - 2][k].Fill = false;
                    voxelGrid[i][boxSize[1] - 2][k].num = 2;
                }
            }

        for (int k = 0; k < boxSize[2]; k++)
            for (int j = 0; j < boxSize[1]; j++)
            {
                if ((!voxelGrid[0][j][k].surface) && (!voxelGrid[1][j][k].surface))
                {
                    voxelGrid[1][j][k].Fill = false;
                    voxelGrid[1][j][k].num = 2;
                }
                if ((!voxelGrid[boxSize[0] - 1][j][k].surface) && (!voxelGrid[boxSize[0] - 2][j][k].surface))
                {
                    voxelGrid[boxSize[0] - 2][j][k].Fill = false;
                    voxelGrid[boxSize[0] - 2][j][k].num = 2;
                }
            }

        //端から順に隣接するボクセルを調査
        int count = 2;
        while (count < boxSize[0])
        {

            for (int i = 1; i < boxSize[0] - 1; i++)
                for (int j = 1; j < boxSize[1] - 1; j++)
                    for (int k = 1; k < boxSize[2] - 1; k++)
                    {
                        if (voxelGrid[i][j][k].surface) continue;
                        if (voxelGrid[i][j][k].num == count)
                        {

                            //x-1
                            if ((voxelGrid[i - 1][j][k].num == 0) && (!voxelGrid[i - 1][j][k].surface))
                            {
                                voxelGrid[i - 1][j][k].Fill = false;
                                voxelGrid[i - 1][j][k].num = count + 1;
                            }

                            //x+1
                            if ((voxelGrid[i + 1][j][k].num == 0) && (!voxelGrid[i + 1][j][k].surface))
                            {
                                voxelGrid[i + 1][j][k].Fill = false;
                                voxelGrid[i + 1][j][k].num = count + 1;
                            }

                            //y-1
                            if ((voxelGrid[i][j - 1][k].num == 0) && (!voxelGrid[i][j - 1][k].surface))
                            {
                                voxelGrid[i][j - 1][k].Fill = false;
                                voxelGrid[i][j - 1][k].num = count + 1;
                            }

                            //y+1
                            if ((voxelGrid[i][j + 1][k].num == 0) && (!voxelGrid[i][j + 1][k].surface))
                            {
                                voxelGrid[i][j + 1][k].Fill = false;
                                voxelGrid[i][j + 1][k].num = count + 1;
                            }

                            //z-1
                            if ((voxelGrid[i][j][k - 1].num == 0) && (!voxelGrid[i][j][k - 1].surface))
                            {
                                voxelGrid[i][j][k - 1].Fill = false;
                                voxelGrid[i][j][k - 1].num = count + 1;
                            }

                            //z+1
                            if ((voxelGrid[i][j][k + 1].num == 0) && (!voxelGrid[i][j][k + 1].surface))
                            {
                                voxelGrid[i][j][k + 1].Fill = false;
                                voxelGrid[i][j][k + 1].num = count + 1;
                            }
                        }
                    }
            count++;
        }

        fillVoxelNumber = 0;
        for (int i = 0; i < boxSize[0]; i++)
            for (int j = 0; j < boxSize[1]; j++)
                for (int k = 0; k < boxSize[2]; k++)
                    if (voxelGrid[i][j][k].Fill)
                        fillVoxelNumber++;

        //cout << "Fillボクセル数は" << fillVoxelNumber << "です" << endl;

        return fillVoxelNumber;

    }

    //surveyボクセルの内部をボクセルで埋める
    public int SurveyInnerVoxelFill()
    {

        //ボクセル情報の初期化と最端部のボクセルの調査
        for (int i = 0; i < surveyBoxSize[0]; i++)
            for (int j = 0; j < surveyBoxSize[1]; j++)
                for (int k = 0; k < surveyBoxSize[2]; k++)
                {
                    surveyVoxel[i][j][k].num = 0;
                    if ((i == 0) || (j == 0) || (k == 0) || (i == surveyBoxSize[0] - 1) || (j == surveyBoxSize[1] - 1) || (k == surveyBoxSize[2] - 1))
                    {
                        surveyVoxel[i][j][k].edge = true;
                        surveyVoxel[i][j][k].Fill = false;
                        surveyVoxel[i][j][k].num = 1;
                        if (surveyVoxel[i][j][k].surface)
                            surveyVoxel[i][j][k].Fill = true;
                    }
                }

        //端部のボクセルの調査
        for (int i = 0; i < surveyBoxSize[0]; i++)
            for (int j = 0; j < surveyBoxSize[1]; j++)
            {
                if ((!surveyVoxel[i][j][0].surface) && (!surveyVoxel[i][j][1].surface))
                {
                    surveyVoxel[i][j][1].Fill = false;
                    surveyVoxel[i][j][1].num = 2;
                }
                if ((!surveyVoxel[i][j][surveyBoxSize[2] - 1].surface) && (!surveyVoxel[i][j][surveyBoxSize[2] - 2].surface))
                {
                    surveyVoxel[i][j][surveyBoxSize[2] - 2].Fill = false;
                    surveyVoxel[i][j][surveyBoxSize[2] - 2].num = 2;
                }
            }

        for (int i = 0; i < surveyBoxSize[0]; i++)
            for (int k = 0; k < surveyBoxSize[2]; k++)
            {
                if ((!surveyVoxel[i][0][k].surface) && (!surveyVoxel[i][1][k].surface))
                {
                    surveyVoxel[i][1][k].Fill = false;
                    surveyVoxel[i][1][k].num = 2;
                }
                if ((!surveyVoxel[i][surveyBoxSize[1] - 1][k].surface) && (!surveyVoxel[i][surveyBoxSize[1] - 2][k].surface))
                {
                    surveyVoxel[i][surveyBoxSize[1] - 2][k].Fill = false;
                    surveyVoxel[i][surveyBoxSize[1] - 2][k].num = 2;
                }
            }

        for (int k = 0; k < surveyBoxSize[2]; k++)
            for (int j = 0; j < surveyBoxSize[1]; j++)
            {
                if ((!surveyVoxel[0][j][k].surface) && (!surveyVoxel[1][j][k].surface))
                {
                    surveyVoxel[1][j][k].Fill = false;
                    surveyVoxel[1][j][k].num = 2;
                }
                if ((!surveyVoxel[surveyBoxSize[0] - 1][j][k].surface) && (!surveyVoxel[surveyBoxSize[0] - 2][j][k].surface))
                {
                    surveyVoxel[surveyBoxSize[0] - 2][j][k].Fill = false;
                    surveyVoxel[surveyBoxSize[0] - 2][j][k].num = 2;
                }
            }

        //端から順に隣接するボクセルを調査
        int count = 2;
        while (count < surveyBoxSize[0])
        {

            for (int i = 1; i < surveyBoxSize[0] - 1; i++)
                for (int j = 1; j < surveyBoxSize[1] - 1; j++)
                    for (int k = 1; k < surveyBoxSize[2] - 1; k++)
                    {
                        if (surveyVoxel[i][j][k].surface) continue;
                        if (surveyVoxel[i][j][k].num == count)
                        {

                            //x-1
                            if ((surveyVoxel[i - 1][j][k].num == 0) && (!surveyVoxel[i - 1][j][k].surface))
                            {
                                surveyVoxel[i - 1][j][k].Fill = false;
                                surveyVoxel[i - 1][j][k].num = count + 1;
                            }

                            //x+1
                            if ((surveyVoxel[i + 1][j][k].num == 0) && (!surveyVoxel[i + 1][j][k].surface))
                            {
                                surveyVoxel[i + 1][j][k].Fill = false;
                                surveyVoxel[i + 1][j][k].num = count + 1;
                            }

                            //y-1
                            if ((surveyVoxel[i][j - 1][k].num == 0) && (!surveyVoxel[i][j - 1][k].surface))
                            {
                                surveyVoxel[i][j - 1][k].Fill = false;
                                surveyVoxel[i][j - 1][k].num = count + 1;
                            }

                            //y+1
                            if ((surveyVoxel[i][j + 1][k].num == 0) && (!surveyVoxel[i][j + 1][k].surface))
                            {
                                surveyVoxel[i][j + 1][k].Fill = false;
                                surveyVoxel[i][j + 1][k].num = count + 1;
                            }

                            //z-1
                            if ((surveyVoxel[i][j][k - 1].num == 0) && (!surveyVoxel[i][j][k - 1].surface))
                            {
                                surveyVoxel[i][j][k - 1].Fill = false;
                                surveyVoxel[i][j][k - 1].num = count + 1;
                            }

                            //z+1
                            if ((surveyVoxel[i][j][k + 1].num == 0) && (!surveyVoxel[i][j][k + 1].surface))
                            {
                                surveyVoxel[i][j][k + 1].Fill = false;
                                surveyVoxel[i][j][k + 1].num = count + 1;
                            }
                        }
                    }
            count++;
        }

        fillVoxelNumber = 0;
        for (int i = 0; i < surveyBoxSize[0]; i++)
            for (int j = 0; j < surveyBoxSize[1]; j++)
                for (int k = 0; k < surveyBoxSize[2]; k++)
                    if (surveyVoxel[i][j][k].Fill)
                        fillVoxelNumber++;

        //cout << "Fillボクセル数は" << fillVoxelNumber << "です" << endl;

        return fillVoxelNumber;

    }

    //ボクセルの数を数えて出力//体積も出力//角度も出力
    public void VoxelNumberVolumeDebug(float angle)
    {
        int voxelNumber = 0;
        float voxelVolume = 0.0f;
        float voxelGridVolume = 0.0f;

        for (int size_x = 0; size_x < boxSize[0]; size_x++)
        {

            for (int size_y = 0; size_y < boxSize[1]; size_y++)
            {

                for (int size_z = 0; size_z < boxSize[2]; size_z++)
                {
                    if (voxelGrid[size_x][size_y][size_z].Fill)
                    {
                        voxelNumber++;
                    }
                }

            }

        }

        voxelVolume = LengthXZ * LengthXZ * LengthY;
        voxelGridVolume = voxelVolume * voxelNumber;

        Debug.Log("Number" + voxelNumber + "Volume" + voxelGridVolume + "angle" + angle);
        // Debug.Log(voxelGridVolume);

    }
    //ボクセルの数を数えて出力//体積も出力//角度も出力
    public float VoxelVolumeReturn(float angle)
    {
        int voxelNumber = 0;
        float voxelVolume = 0.0f;
        float voxelGridVolume = 0.0f;

        for (int size_x = 0; size_x < boxSize[0]; size_x++)
        {

            for (int size_y = 0; size_y < boxSize[1]; size_y++)
            {

                for (int size_z = 0; size_z < boxSize[2]; size_z++)
                {
                    if (voxelGrid[size_x][size_y][size_z].Fill)
                    {
                        voxelNumber++;
                    }
                }

            }

        }

        voxelVolume = LengthXZ * LengthXZ * LengthY;
        voxelGridVolume = voxelVolume * voxelNumber;

        Debug.Log("Number" + voxelNumber + "Volume" + voxelGridVolume + "angle" + angle);
        return voxelGridVolume;
        // Debug.Log(voxelGridVolume);

    }

    //ボクセルの数を数える
    public int VoxelNumberMaxMin(float angle)
    {
        int voxelNumber = 0;
        // float voxelVolume = 0.0f;
        //float voxelGridVolume = 0.0f;

        for (int size_x = 0; size_x < boxSize[0]; size_x++)
        {

            for (int size_y = 0; size_y < boxSize[1]; size_y++)
            {

                for (int size_z = 0; size_z < boxSize[2]; size_z++)
                {
                    if (voxelGrid[size_x][size_y][size_z].Fill)
                    {
                        voxelNumber++;
                    }
                }

            }

        }

        return voxelNumber;

        //if (maxNumberVoxel.y == 0)
        //{
        //    maxNumberVoxel.y = voxelNumber;
        //}
        //if (minNumberVoxel.y == 0)
        //{
        //    minNumberVoxel.y = voxelNumber;
        //}

        //if (maxNumberVoxel.y < voxelNumber)
        //{
        //    maxNumberVoxel.y = voxelNumber;
        //}
        //if (minNumberVoxel.y > voxelNumber)
        //{
        //    minNumberVoxel.y = voxelNumber;
        //}




        //voxelVolume = LengthXZ * LengthXZ * LengthY;
        //voxelGridVolume = voxelVolume * voxelNumber;

        // Debug.Log("Number" + voxelNumber + "Volume" + voxelGridVolume + "angle" + angle);
        // Debug.Log(voxelGridVolume);

    }

    //各ボクセルを初期化する
    public void IniVoxelGrid()
    {
        for (int size_x = 0; size_x < boxSize[0]; size_x++)
        {

            for (int size_y = 0; size_y < boxSize[1]; size_y++)
            {

                for (int size_z = 0; size_z < boxSize[2]; size_z++)
                {
                    //voxelGrid[size_x][size_y][size_z].IniVoxel();
                    voxelGrid[size_x][size_y][size_z].surface = false;
                    voxelGrid[size_x][size_y][size_z].pos = Vector3.zero;
                    voxelGrid[size_x][size_y][size_z].edge = false;
                    voxelGrid[size_x][size_y][size_z].Fill = true;
                }

            }

        }


    }

    //ボクセルの配列のデータをtxtファイルに書き込む
    public void WriteVoxelTxt()//(string writeFileName)
    {
        var dataname = EditorUtility.SaveFilePanel("Save", Application.dataPath, "", "txt");
        FileInfo fi = new FileInfo(dataname);
        using (StreamWriter sw = fi.AppendText())
        {
            sw.WriteLine((boxSize[0] + 2) + "  " + (boxSize[1] + 2) + "  " + (boxSize[2] + 2));

            for (int size_z = 0; size_z < boxSize[2]; size_z++)
            {
                for (int size_y = 0; size_y < boxSize[1]; size_y++)
                {
                    for (int size_x = 0; size_x < boxSize[0]; size_x++)
                    {


                        if (voxelGrid[size_x][size_y][size_z].Fill)
                        {
                            sw.WriteLine((size_x + 1) + "  " + (size_y + 1) + "  " + (size_z + 1));
                        }
                    }

                }

            }

        }
    }



    public void VoxelcoloringAve(Solid model)
    {

        //float tempcolorRed;
        //float tempcolorGreen;
        //float tempcolorBlue;
        var tempColorVert = new Color();

        for (int x = 0; x < boxSize[0]; ++x)
        {
            for (int y = 0; y < boxSize[1]; ++y)
            {
                for (int z = 0; z < boxSize[2]; ++z)
                {
                    if (voxelGrid[x][y][z].surface)
                    {
                        if (voxelGrid[x][y][z].crossedVertsList.Count > 0)
                        {
                            if (voxelGrid[x][y][z].crossedVertsList.Count == 1)
                            {
                                voxelGrid[x][y][z].color = model.sverts[voxelGrid[x][y][z].crossedVertsList[0]].color;
                            }
                            else
                            {
                                tempColorVert.r = 0f;
                                tempColorVert.g = 0f;
                                tempColorVert.b = 0f;

                                foreach (var l in voxelGrid[x][y][z].crossedVertsList)
                                {
                                    tempColorVert.r += model.sverts[l].color.r;
                                    tempColorVert.g += model.sverts[l].color.g;
                                    tempColorVert.b += model.sverts[l].color.b;
                                }
                                voxelGrid[x][y][z].color.r = (tempColorVert.r / (voxelGrid[x][y][z].crossedVertsList.Count()));
                                voxelGrid[x][y][z].color.g = (tempColorVert.g / (voxelGrid[x][y][z].crossedVertsList.Count()));
                                voxelGrid[x][y][z].color.b = (tempColorVert.b / (voxelGrid[x][y][z].crossedVertsList.Count()));
                            }
                        }
                        else if (voxelGrid[x][y][z].crossedEdgesList.Count() > 0)
                        {
                            if (voxelGrid[x][y][z].crossedEdgesList.Count() == 1)
                            {
                                voxelGrid[x][y][z].color.r = (model.sedges[voxelGrid[x][y][z].crossedEdgesList[0]].he1.hvert.color.r + model.sedges[voxelGrid[x][y][z].crossedEdgesList[0]].he2.hvert.color.r) / 2.0f;
                                voxelGrid[x][y][z].color.g = (model.sedges[voxelGrid[x][y][z].crossedEdgesList[0]].he1.hvert.color.g + model.sedges[voxelGrid[x][y][z].crossedEdgesList[0]].he2.hvert.color.g) / 2.0f;
                                voxelGrid[x][y][z].color.b = (model.sedges[voxelGrid[x][y][z].crossedEdgesList[0]].he1.hvert.color.b + model.sedges[voxelGrid[x][y][z].crossedEdgesList[0]].he2.hvert.color.b) / 2.0f;
                            }
                            else
                            {
                                voxelGrid[x][y][z].color.r = 0.0f;
                                voxelGrid[x][y][z].color.g = 0.0f;
                                voxelGrid[x][y][z].color.b = 0.0f;
                                foreach (var l in voxelGrid[x][y][z].crossedEdgesList)
                                {
                                    voxelGrid[x][y][z].color.r += (model.sedges[l].he1.hvert.color.r + model.sedges[l].he2.hvert.color.r) / 2.0f;
                                    voxelGrid[x][y][z].color.g += (model.sedges[l].he1.hvert.color.g + model.sedges[l].he2.hvert.color.g) / 2.0f;
                                    voxelGrid[x][y][z].color.b += (model.sedges[l].he1.hvert.color.b + model.sedges[l].he2.hvert.color.b) / 2.0f;
                                }
                                voxelGrid[x][y][z].color.r = voxelGrid[x][y][z].color.r / (voxelGrid[x][y][z].crossedEdgesList.Count());
                                voxelGrid[x][y][z].color.g = voxelGrid[x][y][z].color.g / (voxelGrid[x][y][z].crossedEdgesList.Count());
                                voxelGrid[x][y][z].color.b = voxelGrid[x][y][z].color.b / (voxelGrid[x][y][z].crossedEdgesList.Count());
                            }
                        }
                        else if (voxelGrid[x][y][z].crossedFacesList.Count() > 0)
                        {
                            voxelGrid[x][y][z].color.r = 0.0f;
                            voxelGrid[x][y][z].color.g = 0.0f;
                            voxelGrid[x][y][z].color.b = 0.0f;
                            HalfEdge temp = new HalfEdge();//違うかも
                            if (voxelGrid[x][y][z].crossedFacesList.Count() == 1)
                            {
                                temp = model.sfaces[voxelGrid[x][y][z].crossedFacesList[0]].fedges;
                                for (int f = 0; f < model.sfaces[voxelGrid[x][y][z].crossedFacesList[0]].sidednum; ++f)
                                {
                                    voxelGrid[x][y][z].color.r += temp.hvert.color.r;
                                    voxelGrid[x][y][z].color.g += temp.hvert.color.g;
                                    voxelGrid[x][y][z].color.b += temp.hvert.color.b;
                                    temp = temp.next;
                                }
                                voxelGrid[x][y][z].color.r = voxelGrid[x][y][z].color.r / (model.sfaces[voxelGrid[x][y][z].crossedFacesList[0]].sidednum);
                                voxelGrid[x][y][z].color.g = voxelGrid[x][y][z].color.g / (model.sfaces[voxelGrid[x][y][z].crossedFacesList[0]].sidednum);
                                voxelGrid[x][y][z].color.b = voxelGrid[x][y][z].color.b / (model.sfaces[voxelGrid[x][y][z].crossedFacesList[0]].sidednum);
                            }
                            else
                            {
                                // List<Color> tempcolor(voxelGrid[x][y][z].crossedFacesList.Count());
                                List<Color> tempcolor = new List<Color>();
                                for (int i = 0; i < voxelGrid[x][y][z].crossedFacesList.Count(); ++i)
                                {
                                    tempcolor.Add(new Color());
                                }



                                int count = 0;
                                foreach (var l in voxelGrid[x][y][z].crossedFacesList)


                                {
                                    temp = model.sfaces[l].fedges;
                                    for (int f = 0; f < model.sfaces[l].sidednum; ++f)
                                    {
                                        // tempcolor[count].r = temp.hvert.color.r;
                                        //// tempcolor[].r = temp.hvert.color.r;
                                        // tempcolor[count].g = temp.hvert.color.g;
                                        // tempcolor[count].b = temp.hvert.color.b;
                                        tempcolor[count] = temp.hvert.color;

                                        temp = temp.next;
                                    }
                                    ++count;
                                }
                                for (int i = 0; i < voxelGrid[x][y][z].crossedFacesList.Count(); ++i)
                                {

                                    //合ってるか分からない
                                    voxelGrid[x][y][z].color.r += tempcolor[i].r;
                                    voxelGrid[x][y][z].color.g += tempcolor[i].g;
                                    voxelGrid[x][y][z].color.b += tempcolor[i].b;
                                }
                                voxelGrid[x][y][z].color.r = voxelGrid[x][y][z].color.r / (model.sfaces[voxelGrid[x][y][z].crossedFacesList[0]].sidednum);
                                voxelGrid[x][y][z].color.g = voxelGrid[x][y][z].color.g / (model.sfaces[voxelGrid[x][y][z].crossedFacesList[0]].sidednum);
                                voxelGrid[x][y][z].color.b = voxelGrid[x][y][z].color.b / (model.sfaces[voxelGrid[x][y][z].crossedFacesList[0]].sidednum);
                            }
                        }
                        else
                        {
                            voxelGrid[x][y][z].color.r = 0.0f;
                            voxelGrid[x][y][z].color.g = 0.0f;
                            voxelGrid[x][y][z].color.b = 0.0f;
                        }
                    }
                }


            }

        }
    }

    public void VoxelcoloringLin(Solid model)
    {

        //float tempcolorRed;
        //float tempcolorGreen;
        //float tempcolorBlue;
        var tempColorVert = new Color();

        for (int x = 0; x < boxSize[0]; ++x)
        {
            for (int y = 0; y < boxSize[1]; ++y)
            {
                for (int z = 0; z < boxSize[2]; ++z)
                {
                    if (voxelGrid[x][y][z].surface)
                    {
                        if (voxelGrid[x][y][z].crossedVertsList.Count > 0)
                        {
                            if (voxelGrid[x][y][z].crossedVertsList.Count == 1)
                            {
                                voxelGrid[x][y][z].color = model.sverts[voxelGrid[x][y][z].crossedVertsList[0]].color;
                            }
                            else
                            {
                                tempColorVert.r = 0f;
                                tempColorVert.g = 0f;
                                tempColorVert.b = 0f;

                                foreach (var l in voxelGrid[x][y][z].crossedVertsList)
                                {
                                    tempColorVert.r += model.sverts[l].color.r;
                                    tempColorVert.g += model.sverts[l].color.g;
                                    tempColorVert.b += model.sverts[l].color.b;
                                }
                                voxelGrid[x][y][z].color.r = (tempColorVert.r / (voxelGrid[x][y][z].crossedVertsList.Count()));
                                voxelGrid[x][y][z].color.g = (tempColorVert.g / (voxelGrid[x][y][z].crossedVertsList.Count()));
                                voxelGrid[x][y][z].color.b = (tempColorVert.b / (voxelGrid[x][y][z].crossedVertsList.Count()));
                            }
                        }
                        else if (voxelGrid[x][y][z].crossedEdgesList.Count() > 0)
                        {
                            if (voxelGrid[x][y][z].crossedEdgesList.Count() == 1)
                            {
                                var line01 = new Vector3();
                                line01 = model.sedges[voxelGrid[x][y][z].crossedEdgesList[0]].he1.hvert.vr - voxelGrid[x][y][z].pos;
                                var line02 = new Vector3();
                                line02 = model.sedges[voxelGrid[x][y][z].crossedEdgesList[0]].he2.hvert.vr - voxelGrid[x][y][z].pos;
                                float lineSize = line01.magnitude + line02.magnitude;

                                voxelGrid[x][y][z].color.r = (model.sedges[voxelGrid[x][y][z].crossedEdgesList[0]].he1.hvert.color.r * line02.magnitude
                                    + model.sedges[voxelGrid[x][y][z].crossedEdgesList[0]].he2.hvert.color.r * line01.magnitude) / lineSize;
                                voxelGrid[x][y][z].color.g = (model.sedges[voxelGrid[x][y][z].crossedEdgesList[0]].he1.hvert.color.g * line02.magnitude
                                    + model.sedges[voxelGrid[x][y][z].crossedEdgesList[0]].he2.hvert.color.g * line01.magnitude) / lineSize;
                                voxelGrid[x][y][z].color.b = (model.sedges[voxelGrid[x][y][z].crossedEdgesList[0]].he1.hvert.color.b * line02.magnitude
                                    + model.sedges[voxelGrid[x][y][z].crossedEdgesList[0]].he2.hvert.color.b * line01.magnitude) / lineSize;
                            }
                            else
                            {
                                voxelGrid[x][y][z].color.r = 0.0f;
                                voxelGrid[x][y][z].color.g = 0.0f;
                                voxelGrid[x][y][z].color.b = 0.0f;
                                foreach (var l in voxelGrid[x][y][z].crossedEdgesList)
                                {
                                    var line01 = new Vector3();
                                    line01 = model.sedges[voxelGrid[x][y][z].crossedEdgesList[0]].he1.hvert.vr - voxelGrid[x][y][z].pos;
                                    var line02 = new Vector3();
                                    line02 = model.sedges[voxelGrid[x][y][z].crossedEdgesList[0]].he2.hvert.vr - voxelGrid[x][y][z].pos;
                                    float lineSize = line01.magnitude + line02.magnitude;
                                    //ここでエラー
                                    //                                voxelGrid[x][y][z].color.r += (model.sedges[voxelGrid[x][y][z].crossedEdgesList[l]].he1.hvert.color.r * line02.magnitude
                                    //+ model.sedges[voxelGrid[x][y][z].crossedEdgesList[l]].he2.hvert.color.r * line01.magnitude) / lineSize;
                                    //                                voxelGrid[x][y][z].color.g += (model.sedges[voxelGrid[x][y][z].crossedEdgesList[l]].he1.hvert.color.g * line02.magnitude
                                    //                                    + model.sedges[voxelGrid[x][y][z].crossedEdgesList[l]].he2.hvert.color.g * line01.magnitude) / lineSize;
                                    //                                voxelGrid[x][y][z].color.b += (model.sedges[voxelGrid[x][y][z].crossedEdgesList[l]].he1.hvert.color.b * line02.magnitude
                                    //                                    + model.sedges[voxelGrid[x][y][z].crossedEdgesList[l]].he2.hvert.color.b * line01.magnitude) / lineSize;

                                    voxelGrid[x][y][z].color.r += (model.sedges[l].he1.hvert.color.r * line02.magnitude
       + model.sedges[l].he2.hvert.color.r * line01.magnitude) / lineSize;
                                    voxelGrid[x][y][z].color.g += (model.sedges[l].he1.hvert.color.g * line02.magnitude
                                        + model.sedges[l].he2.hvert.color.g * line01.magnitude) / lineSize;
                                    voxelGrid[x][y][z].color.b += (model.sedges[l].he1.hvert.color.b * line02.magnitude
                                        + model.sedges[l].he2.hvert.color.b * line01.magnitude) / lineSize;

                                    //voxelGrid[x][y][z].color.r += (model.sedges[l].he1.hvert.color.r + model.sedges[l].he2.hvert.color.r) / 2.0f;
                                    //voxelGrid[x][y][z].color.g += (model.sedges[l].he1.hvert.color.g + model.sedges[l].he2.hvert.color.g) / 2.0f;
                                    //voxelGrid[x][y][z].color.b += (model.sedges[l].he1.hvert.color.b + model.sedges[l].he2.hvert.color.b) / 2.0f;
                                }
                                voxelGrid[x][y][z].color.r = voxelGrid[x][y][z].color.r / (voxelGrid[x][y][z].crossedEdgesList.Count());
                                voxelGrid[x][y][z].color.g = voxelGrid[x][y][z].color.g / (voxelGrid[x][y][z].crossedEdgesList.Count());
                                voxelGrid[x][y][z].color.b = voxelGrid[x][y][z].color.b / (voxelGrid[x][y][z].crossedEdgesList.Count());
                            }
                        }
                        else if (voxelGrid[x][y][z].crossedFacesList.Count() > 0)
                        {
                            voxelGrid[x][y][z].color.r = 0.0f;
                            voxelGrid[x][y][z].color.g = 0.0f;
                            voxelGrid[x][y][z].color.b = 0.0f;
                            if (voxelGrid[x][y][z].crossedFacesList.Count() == 1)
                            {
                                HalfEdge temp = new HalfEdge();//違うかも
                                                               //
                                                               //var line01 = new Vector3();
                                                               //line01 = model.sedges[voxelGrid[x][y][z].crossedEdgesList[0]].he1.hvert.vr - voxelGrid[x][y][z].pos;
                                                               //var line02 = new Vector3();
                                                               //line02 = model.sedges[voxelGrid[x][y][z].crossedEdgesList[0]].he2.hvert.vr - voxelGrid[x][y][z].pos;
                                                               //float lineSize = line01.magnitude + line02.magnitude;

                                List<Vector3> fVert = new List<Vector3>();
                                List<Vector3> VertToCenter = new List<Vector3>();
                                List<float> ReVertToCenter = new List<float>();
                                List<Color> tempColor = new List<Color>();
                                float SumReVertToCenter = 0f;

                                //
                                temp = model.sfaces[voxelGrid[x][y][z].crossedFacesList[0]].fedges;
                                for (int f = 0; f < model.sfaces[voxelGrid[x][y][z].crossedFacesList[0]].sidednum; ++f)
                                {
                                    //voxelGrid[x][y][z].color.r += temp.hvert.color.r;
                                    //voxelGrid[x][y][z].color.g += temp.hvert.color.g;
                                    //voxelGrid[x][y][z].color.b += temp.hvert.color.b;

                                    tempColor.Add(temp.hvert.color);


                                    fVert.Add(temp.hvert.vr);
                                    temp = temp.next;
                                }
                                for (int i = 0; i < model.sfaces[voxelGrid[x][y][z].crossedFacesList[0]].sidednum; ++i)
                                {
                                    // VertToCenter[i] = fVert[i] - voxelGrid[x][y][z].pos;
                                    VertToCenter.Add(fVert[i] - voxelGrid[x][y][z].pos);
                                    //ReVertToCenter[i] = 1 / VertToCenter[i].magnitude;
                                    ReVertToCenter.Add(1 / VertToCenter[i].magnitude);
                                    SumReVertToCenter += ReVertToCenter[i];
                                }

                                for (int i = 0; i < model.sfaces[voxelGrid[x][y][z].crossedFacesList[0]].sidednum; ++i)

                                {

                                    voxelGrid[x][y][z].color.r += ReVertToCenter[i] * tempColor[i].r;
                                    voxelGrid[x][y][z].color.g += ReVertToCenter[i] * tempColor[i].g;
                                    voxelGrid[x][y][z].color.b += ReVertToCenter[i] * tempColor[i].b;


                                }



                                voxelGrid[x][y][z].color.r = voxelGrid[x][y][z].color.r / SumReVertToCenter;
                                voxelGrid[x][y][z].color.g = voxelGrid[x][y][z].color.g / SumReVertToCenter;
                                voxelGrid[x][y][z].color.b = voxelGrid[x][y][z].color.b / SumReVertToCenter;

                            }
                            else
                            {

                                List<Color> eachColor = new List<Color>();
                                int count = 0;
                                foreach (var l in voxelGrid[x][y][z].crossedFacesList)
                                {

                                    List<Color> tempcolor = new List<Color>();//0,1,2番目はそれぞれの色を格納
                                    List<Vector3> fVert = new List<Vector3>();
                                    List<Vector3> VertToCenter = new List<Vector3>();
                                    List<float> ReVertToCenter = new List<float>();
                                    float SumReVertToCenter = 0f;

                                    var tempSumColor = new Color();



                                    HalfEdge temp = new HalfEdge();//違うかも

                                    temp = model.sfaces[l].fedges;
                                    for (int f = 0; f < model.sfaces[l].sidednum; ++f)
                                    {

                                        tempcolor.Add(temp.hvert.color);
                                        fVert.Add(temp.hvert.vr);
                                        VertToCenter.Add(fVert[f] - voxelGrid[x][y][z].pos);
                                        ReVertToCenter.Add(1f / VertToCenter[f].magnitude);
                                        SumReVertToCenter += ReVertToCenter[f];
                                        temp = temp.next;
                                    }

                                    //eachColor.Add(new Color());


                                    for (int i = 0; i < model.sfaces[voxelGrid[x][y][z].crossedFacesList[0]].sidednum; ++i)

                                    {

                                        //eachColor[count].r += ReVertToCenter[i] * tempcolor[i].r;
                                        tempSumColor.r += ReVertToCenter[i] * tempcolor[i].r;


                                        tempSumColor.g += ReVertToCenter[i] * tempcolor[i].g;
                                        tempSumColor.b += ReVertToCenter[i] * tempcolor[i].b;


                                    }

                                    count++;


                                    tempSumColor.r = tempSumColor.r / SumReVertToCenter;
                                    tempSumColor.g = tempSumColor.g / SumReVertToCenter;
                                    tempSumColor.b = tempSumColor.b / SumReVertToCenter;

                                    eachColor.Add(tempSumColor);


                                }
                                for (int f = 0; f < voxelGrid[x][y][z].crossedFacesList.Count(); ++f)//
                                {

                                }
                                for (int i = 0; i < voxelGrid[x][y][z].crossedFacesList.Count(); ++i)
                                {


                                    voxelGrid[x][y][z].color.r += eachColor[i].r;
                                    voxelGrid[x][y][z].color.g += eachColor[i].g;
                                    voxelGrid[x][y][z].color.b += eachColor[i].b;
                                }
                                voxelGrid[x][y][z].color.r = voxelGrid[x][y][z].color.r / voxelGrid[x][y][z].crossedFacesList.Count();
                                voxelGrid[x][y][z].color.g = voxelGrid[x][y][z].color.g / voxelGrid[x][y][z].crossedFacesList.Count();
                                voxelGrid[x][y][z].color.b = voxelGrid[x][y][z].color.b / voxelGrid[x][y][z].crossedFacesList.Count();
                            }
                        }
                        else
                        {
                            voxelGrid[x][y][z].color.r = 0.0f;
                            voxelGrid[x][y][z].color.g = 0.0f;
                            voxelGrid[x][y][z].color.b = 0.0f;
                        }
                    }
                }


            }

        }
    }

    public float SurveyDeleteRate(float deleteNum, float divisionNum)
    {

        float totalInVoxelNum = 0f;
        float totalInVoxelEva = 0f;
        float InVoxelNumAve = 0f;
        float InVoxelNumEva = 0f;
        float division = Mathf.Log(divisionNum, 2);
        division = division / 1.5f;

        for (int x = 0; x < boxSize[0]; x++)
        {

            for (int y = 0; y < boxSize[1]; y++)
            {

                for (int z = 0; z < boxSize[2]; z++)
                {
                    if (voxelGrid[x][y][z].cross)
                    {
                        float countInNum = 0f;

                        for (int l = 0; l < division; l++)
                        {

                            for (int m = 0; m < division; m++)
                            {
                                for (int n = 0; n < division; n++)
                                {
                                    if (surveyVoxel[x * (int)division + l][y * (int)division + m][z * (int)division + n].Fill)
                                    {
                                        countInNum++;
                                    }


                                }

                            }

                        }

                        totalInVoxelNum += countInNum;
                        voxelGrid[x][y][z].inVoxelNum = countInNum;
                        crossVoxelNum++;
                    }
                }

            }

        }


        //



        float iniCrossVoxelNum = crossVoxelNum;
        totalInVoxelEva = totalInVoxelNum;
        //float InVoxelNumEach = 0f;

        for (int x = 0; x < boxSize[0]; x++)
        {

            for (int y = 0; y < boxSize[1]; y++)
            {

                for (int z = 0; z < boxSize[2]; z++)
                {
                    if (voxelGrid[x][y][z].cross)
                    {
                        // float countInNum = 0f;



                        if (voxelGrid[x][y][z].inVoxelNum <= deleteNum)
                        {
                            //voxelGrid[x][y][z].cross = false;
                            voxelGrid[x][y][z].surface = false;

                            //他のやつに表面を渡すやつ書かなきゃ，色も同様
                            voxelGrid[x][y][z].Fill = false;

                            if (voxelGrid[x - 1][y][z].Fill)
                            {
                                if (!voxelGrid[x - 1][y][z].surface)
                                {

                                    voxelGrid[x - 1][y][z].surface = true;
                                }
                                //if (!voxelGrid[x-1][y][z].cross )
                                //{
                                //    if (!voxelGrid[x - 1][y][z].surface)

                                //        // voxelGrid[x-1][y][z].cross = true;
                                //        voxelGrid[x-1][y][z].surface = true;

                                //}
                            }
                            if (voxelGrid[x + 1][y][z].Fill)
                            {
                                //if (!voxelGrid[x + 1][y][z].cross)
                                //{
                                //    if (!voxelGrid[x + 1][y][z].surface)

                                //        //voxelGrid[x + 1][y][z].cross = true;
                                //        voxelGrid[x + 1][y][z].surface = true;

                                //}
                                if (!voxelGrid[x + 1][y][z].surface)
                                {

                                    voxelGrid[x + 1][y][z].surface = true;
                                }

                            }
                            if (voxelGrid[x][y - 1][z].Fill)
                            {
                                //if (!voxelGrid[x ][y - 1][z].cross)
                                //{
                                //    if (!voxelGrid[x ][y-1][z].surface)

                                //        //voxelGrid[x ][y - 1][z].cross = true;
                                //        voxelGrid[x ][y - 1][z].surface = true;

                                //}
                                if (!voxelGrid[x][y - 1][z].surface)
                                {

                                    voxelGrid[x][y - 1][z].surface = true;
                                }

                            }
                            if (voxelGrid[x][y + 1][z].Fill)
                            {
                                //if (!voxelGrid[x][y + 1][z].cross)
                                //{
                                //    if (!voxelGrid[x][y+1][z].surface)

                                //        //voxelGrid[x][y + 1][z].cross = true;
                                //        voxelGrid[x][y + 1][z].surface = true;

                                //}
                                if (!voxelGrid[x][y + 1][z].surface)
                                {

                                    voxelGrid[x][y + 1][z].surface = true;
                                }

                            }
                            if (voxelGrid[x][y][z - 1].Fill)
                            {
                                //if (!voxelGrid[x][y][z - 1].cross)
                                //{
                                //    if (!voxelGrid[x][y][z-1].surface)

                                //        //voxelGrid[x][y][z - 1].cross = true;
                                //    voxelGrid[x][y][z - 1].surface = true;

                                //}
                                if (!voxelGrid[x][y][z - 1].surface)
                                {

                                    voxelGrid[x][y][z - 1].surface = true;
                                }

                            }
                            if (voxelGrid[x][y][z + 1].Fill)
                            {
                                //if (!voxelGrid[x][y][z + 1].cross)
                                //{
                                //    if (!voxelGrid[x][y][z+1].surface)

                                //        //voxelGrid[x][y][z + 1].cross = true;
                                //        voxelGrid[x][y][z + 1].surface = true;

                                //}
                                if (!voxelGrid[x][y][z + 1].surface)
                                {

                                    voxelGrid[x][y][z + 1].surface = true;
                                }

                            }

                            totalInVoxelNum -= voxelGrid[x][y][z].inVoxelNum;
                            totalInVoxelEva += divisionNum - voxelGrid[x][y][z].inVoxelNum * 2;

                            crossVoxelNum--;
                        }


                        // voxelGrid[x][y][z].inVoxelNum = countInNum;
                    }
                }

            }

        }




        InVoxelNumAve = totalInVoxelNum / crossVoxelNum;
        InVoxelNumEva = totalInVoxelEva / iniCrossVoxelNum;

        //return InVoxelNumAve;
        //return InVoxelNumEva;
        return InVoxelNumEva / divisionNum;


    }

    public float PlySurveyDeleteRate(float deleteNum, float divisionNum)
    {

        float totalInVoxelNum = 0f;
        float totalInVoxelEva = 0f;
        float InVoxelNumAve = 0f;
        float InVoxelNumEva = 0f;
        float division = Mathf.Log(divisionNum, 2);
        division = division / 1.5f;

        for (int x = 0; x < boxSize[0]; x++)
        {

            for (int y = 0; y < boxSize[1]; y++)
            {

                for (int z = 0; z < boxSize[2]; z++)
                {
                    if (voxelGrid[x][y][z].cross)
                    {
                        float countInNum = 0f;

                        for (int l = 0; l < division; l++)
                        {

                            for (int m = 0; m < division; m++)
                            {
                                for (int n = 0; n < division; n++)
                                {
                                    if (surveyVoxel[x * (int)division + l][y * (int)division + m][z * (int)division + n].Fill)
                                    {
                                        countInNum++;
                                    }


                                }

                            }

                        }

                        totalInVoxelNum += countInNum;
                        voxelGrid[x][y][z].inVoxelNum = countInNum;
                        crossVoxelNum++;
                    }
                }

            }

        }


        //



        float iniCrossVoxelNum = crossVoxelNum;
        totalInVoxelEva = totalInVoxelNum;
        //float InVoxelNumEach = 0f;

        for (int x = 0; x < boxSize[0]; x++)
        {

            for (int y = 0; y < boxSize[1]; y++)
            {

                for (int z = 0; z < boxSize[2]; z++)
                {
                    if (voxelGrid[x][y][z].cross)
                    {
                        // float countInNum = 0f;



                        if (voxelGrid[x][y][z].inVoxelNum <= deleteNum)
                        {
                            Color tempColor = new Color();
                            tempColor = voxelGrid[x][y][z].color;
                            //voxelGrid[x][y][z].cross = false;
                            voxelGrid[x][y][z].surface = false;

                            //他のやつに表面を渡すやつ書かなきゃ，色も同様
                            voxelGrid[x][y][z].Fill = false;

                            if (voxelGrid[x - 1][y][z].Fill)
                            {
                                if (!voxelGrid[x - 1][y][z].surface)
                                {

                                    voxelGrid[x - 1][y][z].surface = true;
                                    voxelGrid[x - 1][y][z].color = tempColor;
                                }
                                //if (!voxelGrid[x-1][y][z].cross )
                                //{
                                //    if (!voxelGrid[x - 1][y][z].surface)

                                //        // voxelGrid[x-1][y][z].cross = true;
                                //        voxelGrid[x-1][y][z].surface = true;

                                //}
                            }
                            if (voxelGrid[x + 1][y][z].Fill)
                            {

                                if (!voxelGrid[x + 1][y][z].surface)
                                {

                                    voxelGrid[x + 1][y][z].surface = true;
                                    voxelGrid[x + 1][y][z].color = tempColor;

                                }

                            }
                            if (voxelGrid[x][y - 1][z].Fill)
                            {

                                if (!voxelGrid[x][y - 1][z].surface)
                                {

                                    voxelGrid[x][y - 1][z].surface = true;
                                    voxelGrid[x][y - 1][z].color = tempColor;

                                }

                            }
                            if (voxelGrid[x][y + 1][z].Fill)
                            {

                                if (!voxelGrid[x][y + 1][z].surface)
                                {

                                    voxelGrid[x][y + 1][z].surface = true;
                                    voxelGrid[x][y + 1][z].color = tempColor;

                                }

                            }
                            if (voxelGrid[x][y][z - 1].Fill)
                            {

                                if (!voxelGrid[x][y][z - 1].surface)
                                {

                                    voxelGrid[x][y][z - 1].surface = true;
                                    voxelGrid[x][y][z - 1].color = tempColor;

                                }

                            }
                            if (voxelGrid[x][y][z + 1].Fill)
                            {

                                if (!voxelGrid[x][y][z + 1].surface)
                                {

                                    voxelGrid[x][y][z + 1].surface = true;
                                    voxelGrid[x][y][z + 1].color = tempColor;

                                }

                            }

                            totalInVoxelNum -= voxelGrid[x][y][z].inVoxelNum;
                            totalInVoxelEva += divisionNum - voxelGrid[x][y][z].inVoxelNum * 2;

                            crossVoxelNum--;
                        }


                        // voxelGrid[x][y][z].inVoxelNum = countInNum;
                    }
                }

            }

        }




        InVoxelNumAve = totalInVoxelNum / crossVoxelNum;
        InVoxelNumEva = totalInVoxelEva / iniCrossVoxelNum;

        //return InVoxelNumAve;
        //return InVoxelNumEva;
        return InVoxelNumEva / divisionNum;


    }

    public float SurveyInRate()
    {

        float totalInVoxelNum = 0f;
        float InVoxelNumAve = 0f;

        for (int x = 0; x < boxSize[0]; x++)
        {

            for (int y = 0; y < boxSize[1]; y++)
            {

                for (int z = 0; z < boxSize[2]; z++)
                {
                    if (voxelGrid[x][y][z].cross)
                    {
                        float countInNum = 0f;

                        for (int l = 0; l < 2; l++)
                        {

                            for (int m = 0; m < 2; m++)
                            {
                                for (int n = 0; n < 2; n++)
                                {
                                    if (surveyVoxel[x * 2 + l][y * 2 + m][z * 2 + n].Fill)
                                    {
                                        countInNum++;
                                    }


                                }

                            }

                        }

                        totalInVoxelNum += countInNum;
                        voxelGrid[x][y][z].inVoxelNum = countInNum;
                        crossVoxelNum++;
                    }
                }

            }

        }
        InVoxelNumAve = totalInVoxelNum / crossVoxelNum;

        return InVoxelNumAve;





    }

    public float OffInternalRate(Mesh mesh, float res, Solid model, float deleteNum, float divisionNum)//offから作ったボクセルが8等分したボクセルのうちどの程度内部に属しているのか判定
    {
        SurveySpaceDefine(mesh, res, divisionNum);

        SurveyCrossVert(model);
        //SurveyCrossEdge(model);
        SurveyCrossFace(model);
        //SurveyCrossFaceDistance(model);

        //SurveyCrossFaceEach(model, divisionNum);
        SurveyInnerVoxelFill();
        //外部入力で与えた数以下の子ボクセルを持っている表面ボクセルを削る

        return SurveyDeleteRate(deleteNum, divisionNum);


        //return SurveyInRate();

    }

    public float PlyInternalRate(Mesh mesh, float res, Solid model, float deleteNum, float divisionNum)//offから作ったボクセルが8等分したボクセルのうちどの程度内部に属しているのか判定
    {
        SurveySpaceDefine(mesh, res, divisionNum);

        SurveyCrossVert(model);
        SurveyCrossEdge(model);
        SurveyCrossFace(model);

        SurveyInnerVoxelFill();
        //外部入力で与えた数以下の子ボクセルを持っている表面ボクセルを削る
        return PlySurveyDeleteRate(deleteNum, divisionNum);
        // return SurveyDeleteRate(deleteNum, divisionNum);


        //return SurveyInRate();

    }

    public void LegoRGB_to_Lab(string fileNameRGB)
    {
        string path_name = Path.Combine(Application.dataPath, fileNameRGB);
        StreamReader reader = new System.IO.StreamReader(path_name, Encoding.UTF8);
        string line = "";
        string temps;
        List<string> iro = new List<string>();
        List<Color> tempLegoColor = new List<Color>();
        int backColorNum = 0;
        int i = 0;

        //###########################
        //ファイルを1行ずつ読み、かく色の情報を配列に格納
        //###########################
        // 読み込みできる文字がなくなるまで繰り返す,読み取り可能文字出ない場合-1を返す
        while (reader.Peek() >= 0)
        {
            Color temp = new Color();
            line = reader.ReadLine();
            string[] tempColor = line.Split(' ');
            temp.r = int.Parse(tempColor[0]);
            temp.g = int.Parse(tempColor[1]);
            temp.b = int.Parse(tempColor[2]);
            temps = tempColor[3];

            iro.Add(temps);
            tempLegoColor.Add(temp);

        }
        //###########################
        //背景の色を選ばせる//いらない
        //###########################
        for (i = 0; i < iro.Count(); i++)
        {

            Debug.Log(i + iro[i]);
        }


        // cout << "使いたい色の数を入力してください" << endl;
        int WantToUSE = 0;//使いたい色の数
                          //cin >> WantToUSE;
        WantToUSE = 3;
        int tempNum = 0;
        for (i = 0; i < WantToUSE; ++i)
        {
            legoColor.Add(tempLegoColor[tempNum]);
            ++tempNum;
        }
        //###########################
        //使用するレゴブロックの色を選ばれる
        //###########################
        //List<Color> rgbit = new List<Color>();
        //List<Color> labit = new List<Color>();
        //foreach (var each in legoColor)
        //{
        //    Color labEach = new Color();
        //    labEach = RGB_to_Lab(each);

        //}


        //List<Color> rgbit = new List<Color>();
        //List<Color> labit = new List<Color>();
        foreach (var each in legoColor)
        {
            Color labEach = new Color();
            labEach = RGB_to_Lab(each);
            legoColor_Lab.Add(labEach);

        }

    }

    public void TexLegoRGB_to_Lab(string fileNameRGB,int wantToUSE)
    {
        string path_name = Path.Combine(Application.dataPath, fileNameRGB);
        StreamReader reader = new System.IO.StreamReader(path_name, Encoding.UTF8);
        string line = "";
        string temps;
        List<string> iro = new List<string>();
        List<Color> tempLegoColor = new List<Color>();
        int backColorNum = 0;
        int i = 0;

        //###########################
        //ファイルを1行ずつ読み、かく色の情報を配列に格納
        //###########################
        // 読み込みできる文字がなくなるまで繰り返す,読み取り可能文字出ない場合-1を返す
        while (reader.Peek() >= 0)
        {
            Color temp = new Color();
            line = reader.ReadLine();
            string[] tempColor = line.Split(' ');
            temp.r = int.Parse(tempColor[0]);
            temp.g = int.Parse(tempColor[1]);
            temp.b = int.Parse(tempColor[2]);
            temps = tempColor[3];

            iro.Add(temps);
            tempLegoColor.Add(temp);

        }
        //###########################
        //背景の色を選ばせる//いらない
        //###########################
        for (i = 0; i < iro.Count(); i++)
        {

            Debug.Log(i + iro[i]);
        }


        // cout << "使いたい色の数を入力してください" << endl;
        //int WantToUSE = 0;//使いたい色の数
        //                  //cin >> WantToUSE;
        ////WantToUSE = 3;
        //WantToUSE = 9;
        int tempNum = 0;
        for (i = 0; i < wantToUSE; ++i)
        {
            legoColor.Add(tempLegoColor[tempNum]);
            //Debug.Log(legoColor[tempNum].r+" "+ legoColor[tempNum].g + " " + legoColor[tempNum].b);
            ++tempNum;
        }
        //###########################
        //使用するレゴブロックの色を選ばれる
        //###########################
        //List<Color> rgbit = new List<Color>();
        //List<Color> labit = new List<Color>();
        //foreach (var each in legoColor)
        //{
        //    Color labEach = new Color();
        //    labEach = RGB_to_Lab(each);

        //}


        //List<Color> rgbit = new List<Color>();
        //List<Color> labit = new List<Color>();
        foreach (var each in legoColor)
        {
            Color labEach = new Color();
            labEach = RGB_to_Lab(each);
            //Debug.Log(labEach.r + " " + labEach.g + " " + labEach.b);

            legoColor_Lab.Add(labEach);

        }
        int count=0;
        for(int j=0;j<legoColor.Count();j++)
        {
            float r = legoColor[j].r / 255f;
            float g = legoColor[j].g / 255f;
            float b = legoColor[j].b / 255f;

            Color temp = new Color();
            //legoColor[count].r=r;
            //count
            temp.r = r;
            temp.g = g;
            temp.b = b;
            legoColor[j] = temp;

        }

    }

    //RGB表色系からCIEL*a*b*表色系へ変換
    Color RGB_to_Lab(Color rgb)
    {
        //参考ＵＲＬ
        //http://www.easyrgb.com/index.php?X=MATH&H=02#text2
        //cout<<"RGB:"<<rgb.r<< " " << rgb.g << " "<<rgb.b<<endl;
        Color srgb = RGB_to_sRGB(rgb);
        //cout<<"sRGB:"<<srgb.r<< " " << srgb.g << " "<<srgb.b<<endl;
        Color xyz = sRGB_to_XYZ(srgb);
        //cout<<"XYZ:"<<xyz.r<< " " << xyz.g << " "<<xyz.b<<endl;
        Color lab = XYZ_to_Lab(xyz);
        //cout<<"LAB:"<<lab.r<< " " << lab.g << " "<<lab.b<<endl;
        return lab;
    }
    //RGB表色系からCIEL*a*b*表色系へ変換
    Color RGB_to_Lab2(Color rgb)
    {
        Color xyz = sRGB_to_XYZ(rgb);
        Color lab = XYZ_to_Lab(xyz);
        return lab;
    }
    //sRGB / Adobe RGB の RGB 値は，スクリーン上で自然な明るさに見えるように明るさの補正がなされている
    Color RGB_to_sRGB(Color rgb)
    {
        Color temp = new Color();
        //cout<<"rgb"<<rgb.r<<" "<<rgb.g<<" "<<rgb.b<<endl;
        float gamma = 2.4f;
        if (rgb.r > 0.040450)
            temp.r = Mathf.Pow((rgb.r + 0.055f) / 1.055f, gamma);
        else
            temp.r = rgb.r / 12.92f;
        if (rgb.g > 0.040450f)
            temp.g = Mathf.Pow((rgb.g + 0.055f) / 1.055f, gamma);
        else
            temp.g = rgb.g / 12.92f;
        if (rgb.b > 0.040450f)
            temp.b = Mathf.Pow((rgb.b + 0.055f) / 1.055f, gamma);
        else
            temp.b = rgb.b / 12.92f;
        //cout<<"srgb"<<temp.r<<" "<<temp.g<<" "<<temp.b<<endl;

        return temp;
    }

    //sRGB表色系をXYZ表色系へ変換
    Color sRGB_to_XYZ(Color rgb)
    {
        Color xyz = new Color();
        //observer → 2°(1931)
        //sRGB illuminant → D50
        //xyz.r = rgb.r * 0.4360747 + rgb.g * 0.3850649 + rgb.b * 0.1430804;
        //xyz.g = rgb.r * 0.2225045 + rgb.g * 0.7168786 + rgb.b * 0.0606169;
        //xyz.b = rgb.r * 0.0139322 + rgb.g * 0.0971045 + rgb.b * 0.7141733;

        rgb.r = 100.0f * rgb.r;
        rgb.g = 100.0f * rgb.g;
        rgb.b = 100.0f * rgb.b;

        // //sRGB illuminant → D65
        xyz.r = rgb.r * 0.4124f + rgb.g * 0.3576f + rgb.b * 0.1805f;
        xyz.g = rgb.r * 0.2126f + rgb.g * 0.7152f + rgb.b * 0.0722f;
        xyz.b = rgb.r * 0.0193f + rgb.g * 0.1192f + rgb.b * 0.9505f;

        //xyz.r = rgb.r * 0.4124564 + rgb.g * 0.3575761 + rgb.b *  0.1804375;
        //xyz.g = rgb.r * 0.2126729 + rgb.g * 0.7151522 + rgb.b *  0.0721750;
        //xyz.b = rgb.r * 0.0193339 + rgb.g * 0.1191920 + rgb.b *  0.9503041;

        //OpenCVの中で使用されている値
        //xyz.r = rgb.r * 0.412453 + rgb.g * 0.357580 + rgb.b *  0.180423;
        //xyz.g = rgb.r * 0.212671 + rgb.g * 0.715160 + rgb.b *  0.072169;
        //xyz.b = rgb.r * 0.019334 + rgb.g * 0.119193 + rgb.b *  0.950227;

        //cout<<"XYZ"<<xyz.r<<" "<<xyz.g<<" "<<xyz.b<<endl;
        return xyz;
    }

    //XYZ表色系をCIEL*a*b*表色系へ変換
    Color XYZ_to_Lab(Color xyz)
    {
        //observer → 2°(1931)	illuminant → D50
        //float Xn(0.9642);
        //float Yn(1.0);
        //float Zn(0.8249);
        //float Xn(0.96422);
        //float Yn(1.0000);
        //float Zn(0.82521);

        //observer → 2°(1931)	illuminant → D65
        //float Xn(0.95046);
        //float Yn(1.0000);
        //float Zn(1.08906);
        float Xn = 95.047f;
        float Yn = 100.000f;
        float Zn = 108.883f;

        ////OpenCVで使用している値	
        //float Xn(0.950456);
        //float Yn(1.0000);
        //float Zn(1.088754);

        float Ft_x = XYZLab_func(xyz.r / Xn);
        float Ft_y = XYZLab_func(xyz.g / Yn);
        float Ft_z = XYZLab_func(xyz.b / Zn);

        Color lab = new Color();
        lab.r = 116.0f * Ft_y - 16.0f;
        lab.g = 500.0f * (Ft_x - Ft_y);
        lab.b = 200.0f * (Ft_y - Ft_z);

        return lab;
    }

    //XYZ表色系をCIEL*a*b*表色系へ変換するときに使用する関数
    float XYZLab_func(float tt)
    {
        //float threshold = pow(6.0/29.0, 3);
        float threshold = 0.008856f;

        if (tt > threshold)
            return Mathf.Pow(tt, 1.0f / 3.0f);
        else
            return (7.787f * tt + 4.0f / 29.0f);

        //return (pow((29.0/6.0), 2) * tt /3.0 + 4.0/29.0);
    }

    //入力：２つの色の(Lab)　出力：色差を返す
    float DeltaE(Color aa, Color bb)
    {
        float color_difference = Mathf.Sqrt((aa.r - bb.r) * (aa.r - bb.r) + (aa.g - bb.g) * (aa.g - bb.g) + (aa.b - bb.b) * (aa.b - bb.b));
        return color_difference;
    }

    //入力：２つの色の(ab)　出力：色差を返す(明るさを考慮しない)
    float DeltaE_noL(Color aa, Color bb)
    {
        float color_difference = Mathf.Sqrt((aa.g - bb.g) * (aa.g - bb.g) + (aa.b - bb.b) * (aa.b - bb.b));
        return color_difference;
    }

    //・・・レゴカラーにIDをつける//全部に対して行う
    public void RealColor_to_LegoColor_lab()
    {
        float min;//最小値
        int min_no = 0;//最小値が何番目なのか
        Color temp;
        float tempDeltaE;

        for (int x = 0; x < boxSize[0]; ++x)
        {
            for (int y = 0; y < boxSize[1]; ++y)
            {
                for (int z = 0; z < boxSize[2]; ++z)
                {
                    if (voxelGrid[x][y][z].surface)
                    {

                        temp = RGB_to_Lab(voxelGrid[x][y][z].color);
                        min = 3.402823466e+38f; // max value                  //最小値をfloat型の最大とおく
                        min_no = 0;
                        for (int i = 0; i < legoColor.Count(); ++i)
                        {
                            //なぜLabにしているのか
                            tempDeltaE = DeltaE(legoColor_Lab[i], temp);
                            if (tempDeltaE < min)
                            {
                                min = tempDeltaE;           //最小値の更新
                                min_no = i;                     //最小値が何番目か
                            }
                        }
                        voxelGrid[x][y][z].color = legoColor[min_no];
                        voxelGrid[x][y][z].colorID = min_no;
                    }
                }
            }
        }
        //string OutputFileName("Colored_" + model + "_" + to_string(L) + ".txt");
        //ofstream fout(OutputFileName);
        //fout << boxSize[0] + 6 << " " << boxSize[1] + 6 << " " << boxSize[2] + 6 << endl;
        /*-------------------------------------------------------------------------------------*/

        //var dataname = EditorUtility.SaveFilePanel("Save", Application.dataPath, "", "txt");
        //FileInfo fi = new FileInfo(dataname);
        //using (StreamWriter sw = fi.AppendText())
        //{

        //    sw.WriteLine(((boxSize[0] + 6) + "  " + (boxSize[2] + 6) + "  " + (boxSize[1] + 6)));
        //    //sw.WriteLine(((boxSize[0] + 6) + "  " + (boxSize[1] + 6) + "  " + (boxSize[2] + 6)));


        //    int count = 0;
        //    //foreach (var each in writeVoxelNum)
        //    //{

        //    //    sw.WriteLine((count) + "  " + (each));
        //    //    count++;
        //    //}

        //    for (int x = 0; x < boxSize[0]; ++x)
        //    {
        //        for (int z = 0; z < boxSize[2]; ++z)
        //        {
        //            for (int y = 0; y < boxSize[1]; ++y)
        //            {
        //                if (voxelGrid[x][y][z].Fill)
        //                {
        //                    //オチビサン用
        //                    sw.Write((x + 3) + "  " + (boxSize[2] - 1 - z + 3) + "  " + (boxSize[1] - 1 - y + 3) + "  ");
        //                    //sw.Write((x + 3) + "  " + (boxSize[1] - 1 - y + 3) + "  " + (boxSize[2] - 1 - z + 3) + "  ");


        //                    if (voxelGrid[x][y][z].surface)
        //                    {
        //                        //fout << voxelGrid[x][y][z].colorID + 1 << endl;
        //                        if (voxelGrid[x][y][z].colorID == 0)
        //                        {
        //                            //fout << 1 << endl;
        //                            sw.WriteLine(1);
        //                            //sw.WriteLine(1);
        //                        }
        //                        else if (voxelGrid[x][y][z].colorID == 1)
        //                        {
        //                            //fout << 5 << endl;
        //                            sw.WriteLine(2);
        //                            //sw.WriteLine(5);

        //                        }
        //                        else if (voxelGrid[x][y][z].colorID == 2)
        //                        {
        //                            sw.WriteLine(3);
        //                            //sw.WriteLine(9);

        //                            //fout << 9 << endl;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        sw.WriteLine(0);
        //                        //sw.WriteLine("NULL");

        //                        //fout << NULL << endl;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //}



    }
    //・・・レゴカラーにIDをつける//全部に対して行う
    public void TexColor_to_LegoColor_lab()
    {
        //float min;//最小値
        //int min_no = 0;//最小値が何番目なのか
        //Color temp;
        //float tempDeltaE;
        foreach (var each in legoColor_Lab)
        {
            Debug.Log(each.r + " " + each.g + " " + each.b);
        }

        for (int x = 0; x < boxSize[0]; ++x)
        {
            for (int y = 0; y < boxSize[1]; ++y)
            {
                for (int z = 0; z < boxSize[2]; ++z)
                {
                    if (voxelGrid[x][y][z].surface)
                    {
                        float min;//最小値
                        int min_no = 0;//最小値が何番目なのか
                        Color temp;
                        float tempDeltaE;

                        float r = voxelGrid[x][y][z].color.r;
                        float g = voxelGrid[x][y][z].color.g;
                        float b = voxelGrid[x][y][z].color.b;
                        //Debug.Log(r + " " + g + " " + b);

                        voxelGrid[x][y][z].color.r = 255f * r;
                        voxelGrid[x][y][z].color.g = 255f * g;
                        voxelGrid[x][y][z].color.b = 255f * b;


                        //Debug.Log(voxelGrid[x][y][z].color.r + " " + voxelGrid[x][y][z].color.g + " " + voxelGrid[x][y][z].color.b);

                        temp = RGB_to_Lab(voxelGrid[x][y][z].color);

                        //Debug.Log(temp.r + " " + temp.g + " " + temp.b);

                        min = 3.402823466e+38f; // max value                  //最小値をfloat型の最大とおく
                        min_no = 0;
                        for (int i = 0; i < legoColor.Count(); ++i)
                        {
                            //なぜLabにしているのか
                            tempDeltaE = DeltaE(legoColor_Lab[i], temp);
                            if (x == 25 && y == 39 && z == 27)
                            {
                                Debug.Log(tempDeltaE);
                            }
                            if (tempDeltaE < min)
                            {
                                min = tempDeltaE;           //最小値の更新
                                min_no = i;                     //最小値が何番目か
                               
                            }
                        }
                        voxelGrid[x][y][z].color = legoColor[min_no];
                        voxelGrid[x][y][z].colorID = min_no;
                    }
                }
            }
        }


    }

    public void VoxelModelWrite()
    {
        var dataname = EditorUtility.SaveFilePanel("Save", Application.dataPath, "", "txt");
        FileInfo fi = new FileInfo(dataname);
        using (StreamWriter sw = fi.AppendText())
        {

            sw.WriteLine(((boxSize[0] + 6) + "  " + (boxSize[2] + 6) + "  " + (boxSize[1] + 6)));
            //sw.WriteLine(((boxSize[0] + 6) + "  " + (boxSize[1] + 6) + "  " + (boxSize[2] + 6)));


            int count = 0;
            //foreach (var each in writeVoxelNum)
            //{

            //    sw.WriteLine((count) + "  " + (each));
            //    count++;
            //}

            for (int x = 0; x < boxSize[0]; ++x)
            {
                for (int z = 0; z < boxSize[2]; ++z)
                {
                    for (int y = 0; y < boxSize[1]; ++y)
                    {
                        if (voxelGrid[x][y][z].Fill)
                        {
                            //オチビサン用
                            sw.Write((x + 3) + "  " + (z + 3) + "  " + (boxSize[1] - 1 - y + 3) + "  ");
                            //sw.Write((x + 3) + "  " + (boxSize[2] - 1 - z + 3) + "  " + ( y + 3) + "  ");
                            //sw.Write((x + 3) + "  " + (z + 3) + "  " + ( y + 3) + "  ");
                            //sw.Write((x + 3) + "  " + (boxSize[2] - 1 - z + 3) + "  " + (boxSize[1] - 1 - y + 3) + "  ");//元
                            //sw.Write((x + 3) + "  " + (boxSize[1] - 1 - y + 3) + "  " + (boxSize[2] - 1 - z + 3) + "  ");

                            //sw.WriteLine(1);

                            if (voxelGrid[x][y][z].surface)
                            {
                                sw.WriteLine(1);

                                //fout << voxelGrid[x][y][z].colorID + 1 << endl;
                                //if (voxelGrid[x][y][z].colorID == 0)
                                //{
                                //    //fout << 1 << endl;
                                //    sw.WriteLine(1);
                                //    //sw.WriteLine(1);
                                //}
                                //else if (voxelGrid[x][y][z].colorID == 1)
                                //{
                                //    //fout << 5 << endl;
                                //    sw.WriteLine(2);
                                //    //sw.WriteLine(5);

                                //}
                                //else if (voxelGrid[x][y][z].colorID == 2)
                                //{
                                //    sw.WriteLine(3);
                                //    //sw.WriteLine(9);

                                //    //fout << 9 << endl;
                                //}
                            }
                            else
                            {
                                sw.WriteLine(0);
                                //sw.WriteLine("NULL");

                                //fout << NULL << endl;
                            }
                        }
                    }
                }
            }

        }
    }
    //特徴点の色を検出する
    public void FeatureColor()
    {
        float min;//最小値
        int min_no = 0;//最小値が何番目なのか
        Color temp;
        float tempDeltaE;

        for (int x = 0; x < boxSize[0]; ++x)
        {
            for (int y = 0; y < boxSize[1]; ++y)
            {
                for (int z = 0; z < boxSize[2]; ++z)
                {
                    if (voxelGrid[x][y][z].surface)
                    {
                        voxelGrid[x][y][z].colorLab = LABColor.FromColor(voxelGrid[x][y][z].color);

                    }
                }
            }
        }
        for (int x = 0; x < boxSize[0]; ++x)
        {
            for (int y = 0; y < boxSize[1]; ++y)
            {
                for (int z = 0; z < boxSize[2]; ++z)
                {
                    if (voxelGrid[x][y][z].surface)
                    {

                        min = 3.402823466e+38f; // max value                  //最小値をfloat型の最大とおく

                        for (int xx = -1; xx < 2; ++xx)
                        {
                            for (int yy = -1; yy < 2; ++yy)
                            {
                                for (int zz = -1; zz < 2; ++zz)
                                {
                                    if (voxelGrid[x + xx][y + yy][z + zz].surface)
                                    {

                                    }

                                }
                            }

                        }

                        //min_no = 0;
                        //for (int i = 0; i < legoColor.Count(); ++i)
                        //{
                        //    tempDeltaE = DeltaE(legoColor_Lab[i], temp);
                        //    if (tempDeltaE < min)
                        //    {
                        //        min = tempDeltaE;           //最小値の更新
                        //        //min_no = i;                     //最小値が何番目か
                        //    }
                        //}
                        //voxelGrid[x][y][z].color = legoColor[min_no];
                        //voxelGrid[x][y][z].colorID = min_no;
                    }
                }
            }
        }
    }
    //入力：画素の配列		出力：配列を代表する色		（色相の最頻値）
    public void GetVoxelColor_hue_mode(Solid m)
    //public Color GetVoxelColor_hue_mode(List<Color> v)
    {
        for (int i = 0; i < m.sverts.Count(); i++)
        {
            Color.RGBToHSV(m.sverts[i].color, out m.sverts[i].hue, out m.sverts[i].saturation, out m.sverts[i].value);
        }
        //分割区間数に関するメモ
        //パンダの場合15で左右の目が出る、鼻は１ブロック
        //	24の場合左目がよく出るが、右目があまり出ない
        //  int partN(25);					//色相の分割区間数
        int partN = 15;                    //色相の分割区間数
        float minH = 0.0f;             //色相の最小値
        float maxH = 1.0f;       //色相の最大値
                                 //float maxH = 360.0f;       //色相の最大値
        float sec = maxH / partN;          //色相の分割区間幅
        for (int x = 0; x < boxSize[0]; ++x)
        {
            for (int y = 0; y < boxSize[1]; ++y)
            {
                for (int z = 0; z < boxSize[2]; ++z)
                {
                    if (voxelGrid[x][y][z].crossedVertsList.Count() > 1)
                    {
                        //voxelGrid[x][y][z].color.r=0f;
                        //voxelGrid[x][y][z].color.g=0f;
                        //voxelGrid[x][y][z].color.b=0f;



                        //分割区間数毎に入っている頻度をカウントする
                        //int* count;
                        //count = new int[partN];

                        List<int> count = new List<int>();
                        for (int i = 0; i < partN; i++)
                        {
                            int countEach = 0;
                            count.Add(countEach);
                        }
                        for (int i = 0; i < partN; i++)
                            count[i] = 0;

                        int maxN = 0;        //最大区間に含まれる色の数
                        int maxS = 0;        //最大区間の番号(３６：０～３５)

                        int kk;
                        int start = 0;
                        int end = 0;
                        Color temp = new Color(0.0f, 0.0f, 0.0f);



                        //###############################
                        //                          色相(hue)の昇順                          
                        //###############################

                        //sort(v.begin(), v.end(), Color_hue::Less_Hue);

                        ////各区間の頻度をカウント
                        //for (int i = 0; i < voxelGrid[x][y][z].crossedVertsList.Count(); i++)
                        //{
                        //    kk = (int)(v[i].hue / sec);

                        //    if (kk < 0 || kk > partN - 1)
                        //        Debug.Log("baku-----");
                        //    else
                        //        count[kk]++;
                        //}

                        //各区間の頻度をカウント
                        foreach (var num in voxelGrid[x][y][z].crossedVertsList)
                        {
                            kk = (int)(m.sverts[num].hue / sec);

                            if (kk < 0 || kk > partN - 1)
                                Debug.Log("baku-----");
                            else
                            {
                                count[kk]++;
                            }
                        }


                        //最頻値区間を算出
                        for (int i = 0; i < partN; i++)
                        {
                            if (count[i] > maxN)
                            {
                                maxN = count[i];
                                maxS = i;
                            }
                        }
                        //====================================================================================//


                        ////最頻区間は配列のstart～endである
                        //for (int i = 0; i < maxS; i++)
                        //{
                        //    start += count[i];
                        //}
                        //end = start + maxN;

                        ////最頻値区間のrgbを加算する
                        //for (int i = start; i < end; i++)
                        //{
                        //    temp.r += v[i].r;
                        //    temp.g += v[i].g;
                        //    temp.b += v[i].b;
                        //}

                        //最頻区間は配列のstart～endである
                        //最頻値区間のrgbを加算する
                        foreach (var num in voxelGrid[x][y][z].crossedVertsList)
                        {
                            kk = (int)(m.sverts[num].hue / sec);
                            if (kk == maxS)
                            {
                                temp.r += m.sverts[num].color.r;
                                temp.g += m.sverts[num].color.g;
                                temp.b += m.sverts[num].color.b;
                            }
                        }




                        //平均値を算出
                        temp.r = temp.r / maxN;
                        temp.g = temp.g / maxN;
                        temp.b = temp.b / maxN;

                        voxelGrid[x][y][z].color = temp;
                    }
                }
            }
        }
        //return temp;
    }

    //入力：画素の配列		出力：配列を代表する色		（色相の最頻値）
    public void TexVoxelColor_hue_mode(Solid m)
    //public Color GetVoxelColor_hue_mode(List<Color> v)
    {
        //for (int i = 0; i < m.sverts.Count(); i++)
        //{
        //    Color.RGBToHSV(m.sverts[i].color, out m.sverts[i].hue, out m.sverts[i].saturation, out m.sverts[i].value);
        //}
        for (int x = 0; x < boxSize[0]; ++x)
        {
            for (int y = 0; y < boxSize[1]; ++y)
            {
                for (int z = 0; z < boxSize[2]; ++z)
                {
                    for (int i = 0; i < voxelGrid[x][y][z].colorTex.Count(); i++)
                    {
                        float saturation;
                        float value;
                        float hue;

                        Color.RGBToHSV(voxelGrid[x][y][z].colorTex[i], out hue, out saturation, out value);
                        voxelGrid[x][y][z].hueTex.Add(new float());
                        voxelGrid[x][y][z].hueTex[i] = hue;
                    }
                }
            }
        }

        //分割区間数に関するメモ
        //パンダの場合15で左右の目が出る、鼻は１ブロック
        //	24の場合左目がよく出るが、右目があまり出ない
        //  int partN(25);					//色相の分割区間数
        int partN = 15;                    //色相の分割区間数
        float minH = 0.0f;             //色相の最小値
        float maxH = 1.0f;       //色相の最大値
                                 //float maxH = 360.0f;       //色相の最大値
        float sec = maxH / partN;          //色相の分割区間幅
        for (int x = 0; x < boxSize[0]; ++x)
        {
            for (int y = 0; y < boxSize[1]; ++y)
            {
                for (int z = 0; z < boxSize[2]; ++z)
                {
                    if (voxelGrid[x][y][z].hueTex.Count() > 1)
                    {
                        //voxelGrid[x][y][z].color.r=0f;
                        //voxelGrid[x][y][z].color.g=0f;
                        //voxelGrid[x][y][z].color.b=0f;



                        //分割区間数毎に入っている頻度をカウントする
                        //int* count;
                        //count = new int[partN];

                        List<int> count = new List<int>();
                        for (int i = 0; i < partN; i++)
                        {
                            int countEach = 0;
                            count.Add(countEach);
                        }
                        for (int i = 0; i < partN; i++)
                            count[i] = 0;

                        int maxN = 0;        //最大区間に含まれる色の数
                        int maxS = 0;        //最大区間の番号(３６：０～３５)

                        int kk;
                        int start = 0;
                        int end = 0;
                        Color temp = new Color(0.0f, 0.0f, 0.0f);



                        //###############################
                        //                          色相(hue)の昇順                          
                        //###############################

                        //sort(v.begin(), v.end(), Color_hue::Less_Hue);

                        ////各区間の頻度をカウント
                        //for (int i = 0; i < voxelGrid[x][y][z].crossedVertsList.Count(); i++)
                        //{
                        //    kk = (int)(v[i].hue / sec);

                        //    if (kk < 0 || kk > partN - 1)
                        //        Debug.Log("baku-----");
                        //    else
                        //        count[kk]++;
                        //}

                        //各区間の頻度をカウント
                        foreach (var hue in voxelGrid[x][y][z].hueTex)
                        {
                            kk = (int)(hue / sec);

                            if (kk < 0 || kk > partN - 1)
                                Debug.Log("baku-----");
                            else
                            {
                                count[kk]++;
                            }
                        }


                        //最頻値区間を算出
                        for (int i = 0; i < partN; i++)
                        {
                            if (count[i] > maxN)
                            {
                                maxN = count[i];
                                maxS = i;
                            }
                        }
                        //====================================================================================//


                        ////最頻区間は配列のstart～endである
                        //for (int i = 0; i < maxS; i++)
                        //{
                        //    start += count[i];
                        //}
                        //end = start + maxN;

                        ////最頻値区間のrgbを加算する
                        //for (int i = start; i < end; i++)
                        //{
                        //    temp.r += v[i].r;
                        //    temp.g += v[i].g;
                        //    temp.b += v[i].b;
                        //}

                        //最頻区間は配列のstart～endである
                        //最頻値区間のrgbを加算する
                        //foreach (var num in voxelGrid[x][y][z].hueTex)
                        //{
                        //    kk = (int)(hue / sec);
                        //    if (kk == maxS)
                        //    {
                        //        temp.r += m.sverts[num].color.r;
                        //        temp.g += m.sverts[num].color.g;
                        //        temp.b += m.sverts[num].color.b;
                        //    }
                        //}

                        for (int i = 0; i < voxelGrid[x][y][z].hueTex.Count(); i++)
                        {
                            kk = (int)(voxelGrid[x][y][z].hueTex[i] / sec);
                            if (kk == maxS)
                            {
                                temp.r += voxelGrid[x][y][z].colorTex[i].r;
                                temp.g += voxelGrid[x][y][z].colorTex[i].g;
                                temp.b += voxelGrid[x][y][z].colorTex[i].b;
                            }
                        }



                        //平均値を算出
                        temp.r = temp.r / maxN;
                        temp.g = temp.g / maxN;
                        temp.b = temp.b / maxN;

                        voxelGrid[x][y][z].color = temp;
                    }
                }
            }
        }
        //return temp;
    }

    //入力：画素の配列		出力：配列を代表する色		（輝度値の最頻値）Luminance
    public void GetVoxelColor_lumi_mode(Solid m)
    //public Color GetVoxelColor_hue_mode(List<Color> v)
    {
        for (int i = 0; i < m.sverts.Count(); i++)
        {
            //Color.RGBToHSV(m.sverts[i].color, out m.sverts[i].hue, out m.sverts[i].saturation, out m.sverts[i].value);
            m.sverts[i].luminance = (0.298912f * m.sverts[i].color.r + 0.586611f * m.sverts[i].color.g + 0.114478f * m.sverts[i].color.b);
        }
        //分割区間数に関するメモ
        //パンダの場合15で左右の目が出る、鼻は１ブロック
        //	24の場合左目がよく出るが、右目があまり出ない
        //  int partN(25);					//輝度値の分割区間数
        int partN = 2;                    //輝度値の分割区間数
        float minH = 0.0f;             //輝度値の最小値
        float maxH = 256f;       //輝度値の最大値
                                 //float maxH = 255f;       //輝度値の最大値
                                 //float maxH = 360.0f;       //輝度値の最大値
        float sec = maxH / partN;          //輝度値の分割区間幅
        for (int x = 0; x < boxSize[0]; ++x)
        {
            for (int y = 0; y < boxSize[1]; ++y)
            {
                for (int z = 0; z < boxSize[2]; ++z)
                {
                    if (voxelGrid[x][y][z].crossedVertsList.Count() > 1)
                    {
                        //voxelGrid[x][y][z].color.r=0f;
                        //voxelGrid[x][y][z].color.g=0f;
                        //voxelGrid[x][y][z].color.b=0f;



                        //分割区間数毎に入っている頻度をカウントする
                        //int* count;
                        //count = new int[partN];

                        List<int> count = new List<int>();
                        for (int i = 0; i < partN; i++)
                        {
                            int countEach = 0;
                            count.Add(countEach);
                        }
                        for (int i = 0; i < partN; i++)
                            count[i] = 0;

                        int maxN = 0;        //最大区間に含まれる色の数
                        int maxS = 0;        //最大区間の番号(３６：０～３５)

                        int kk;
                        int start = 0;
                        int end = 0;
                        Color temp = new Color(0.0f, 0.0f, 0.0f);



                        //###############################
                        //                          輝度値(hue)の昇順                          
                        //###############################

                        //sort(v.begin(), v.end(), Color_hue::Less_Hue);

                        ////各区間の頻度をカウント
                        //for (int i = 0; i < voxelGrid[x][y][z].crossedVertsList.Count(); i++)
                        //{
                        //    kk = (int)(v[i].hue / sec);

                        //    if (kk < 0 || kk > partN - 1)
                        //        Debug.Log("baku-----");
                        //    else
                        //        count[kk]++;
                        //}

                        //各区間の頻度をカウント
                        foreach (var num in voxelGrid[x][y][z].crossedVertsList)
                        {
                            kk = (int)(m.sverts[num].luminance / sec);

                            if (kk < 0 || kk > partN - 1)
                                Debug.Log("baku-----");
                            else
                            {
                                count[kk]++;
                            }
                        }


                        //最頻値区間を算出
                        for (int i = 0; i < partN; i++)
                        {
                            if (count[i] > maxN)
                            {
                                maxN = count[i];
                                maxS = i;
                            }
                        }
                        //====================================================================================//


                        ////最頻区間は配列のstart～endである
                        //for (int i = 0; i < maxS; i++)
                        //{
                        //    start += count[i];
                        //}
                        //end = start + maxN;

                        ////最頻値区間のrgbを加算する
                        //for (int i = start; i < end; i++)
                        //{
                        //    temp.r += v[i].r;
                        //    temp.g += v[i].g;
                        //    temp.b += v[i].b;
                        //}

                        //最頻区間は配列のstart～endである
                        //最頻値区間のrgbを加算する
                        foreach (var num in voxelGrid[x][y][z].crossedVertsList)
                        {
                            kk = (int)(m.sverts[num].luminance / sec);
                            if (kk == maxS)
                            {
                                temp.r += m.sverts[num].color.r;
                                temp.g += m.sverts[num].color.g;
                                temp.b += m.sverts[num].color.b;
                            }
                        }




                        //平均値を算出
                        temp.r = temp.r / maxN;
                        temp.g = temp.g / maxN;
                        temp.b = temp.b / maxN;

                        voxelGrid[x][y][z].color = temp;
                    }
                }
            }
        }
        //return temp;
    }
    //入力：画素の配列		出力：配列を代表する色		（輝度値の最頻値）Luminance
    public void TexVoxelColor_lumi_mode(Solid m)
    //public Color GetVoxelColor_hue_mode(List<Color> v)
    {
        for (int x = 0; x < boxSize[0]; ++x)
        {
            for (int y = 0; y < boxSize[1]; ++y)
            {
                for (int z = 0; z < boxSize[2]; ++z)
                {
                    for (int i = 0; i < voxelGrid[x][y][z].colorTex.Count(); i++)
                    {
                        float luminance;

                        luminance = (0.298912f * voxelGrid[x][y][z].colorTex[i].r + 0.586611f * voxelGrid[x][y][z].colorTex[i].g + 0.114478f * voxelGrid[x][y][z].colorTex[i].b);

                        voxelGrid[x][y][z].luminance.Add(new float());
                        voxelGrid[x][y][z].luminance[i] = luminance;


                    }
                }
            }
        }

        //分割区間数に関するメモ
        //パンダの場合15で左右の目が出る、鼻は１ブロック
        //	24の場合左目がよく出るが、右目があまり出ない
        //  int partN(25);					//輝度値の分割区間数
        int partN = 15;                    //輝度値の分割区間数
        float minH = 0.0f;             //輝度値の最小値
        float maxH = 256f;       //輝度値の最大値
                                 //float maxH = 255f;       //輝度値の最大値
                                 //float maxH = 360.0f;       //輝度値の最大値
        float sec = maxH / partN;          //輝度値の分割区間幅
        for (int x = 0; x < boxSize[0]; ++x)
        {
            for (int y = 0; y < boxSize[1]; ++y)
            {
                for (int z = 0; z < boxSize[2]; ++z)
                {
                    if (voxelGrid[x][y][z].luminance.Count() > 1)
                    {
                        //voxelGrid[x][y][z].color.r=0f;
                        //voxelGrid[x][y][z].color.g=0f;
                        //voxelGrid[x][y][z].color.b=0f;



                        //分割区間数毎に入っている頻度をカウントする
                        //int* count;
                        //count = new int[partN];

                        List<int> count = new List<int>();
                        for (int i = 0; i < partN; i++)
                        {
                            int countEach = 0;
                            count.Add(countEach);
                        }
                        for (int i = 0; i < partN; i++)
                            count[i] = 0;

                        int maxN = 0;        //最大区間に含まれる色の数
                        int maxS = 0;        //最大区間の番号(３６：０～３５)

                        int kk;
                        int start = 0;
                        int end = 0;
                        Color temp = new Color(0.0f, 0.0f, 0.0f);



                        //###############################
                        //                          輝度値(hue)の昇順                          
                        //###############################

                        //sort(v.begin(), v.end(), Color_hue::Less_Hue);

                        ////各区間の頻度をカウント
                        //for (int i = 0; i < voxelGrid[x][y][z].crossedVertsList.Count(); i++)
                        //{
                        //    kk = (int)(v[i].hue / sec);

                        //    if (kk < 0 || kk > partN - 1)
                        //        Debug.Log("baku-----");
                        //    else
                        //        count[kk]++;
                        //}

                        //各区間の頻度をカウント
                        foreach (var luminance in voxelGrid[x][y][z].luminance)
                        {
                            kk = (int)(luminance / sec);

                            if (kk < 0 || kk > partN - 1)
                                Debug.Log("baku-----");
                            else
                            {
                                count[kk]++;
                            }
                        }


                        //最頻値区間を算出
                        for (int i = 0; i < partN; i++)
                        {
                            if (count[i] > maxN)
                            {
                                maxN = count[i];
                                maxS = i;
                            }
                        }
                        //====================================================================================//


                        ////最頻区間は配列のstart～endである
                        //for (int i = 0; i < maxS; i++)
                        //{
                        //    start += count[i];
                        //}
                        //end = start + maxN;

                        ////最頻値区間のrgbを加算する
                        //for (int i = start; i < end; i++)
                        //{
                        //    temp.r += v[i].r;
                        //    temp.g += v[i].g;
                        //    temp.b += v[i].b;
                        //}

                        //最頻区間は配列のstart～endである
                        //最頻値区間のrgbを加算する
                        for(int i=0;i< voxelGrid[x][y][z].colorTex.Count(); i++)
                        {
                            kk = (int)(voxelGrid[x][y][z].luminance[i] / sec);
                            if (kk == maxS)
                            {
                                temp.r += voxelGrid[x][y][z].colorTex[i].r;
                                temp.g += voxelGrid[x][y][z].colorTex[i].g;
                                temp.b += voxelGrid[x][y][z].colorTex[i].b;
                            }
                        }
                       
                        //平均値を算出
                        temp.r = temp.r / maxN;
                        temp.g = temp.g / maxN;
                        temp.b = temp.b / maxN;

                        voxelGrid[x][y][z].color = temp;
                    }
                }
            }
        }
        //return temp;
    }

    public void CutInCrossSection(Solid solid)
    {
        float cutHeight = 5 * LengthY + voxelGrid[0][0][0].pos.y;//ここで見るのはvoxelGrid[x][5][y]のボクセル達
                                                                 // float cutHeight = boxSize[1] / 2f*LengthY+voxelGrid[0][0][0].pos.y;

        //計算中のHalfEdge
        HalfEdge temp;
        Vector3 P0, P1, P2; //三角形メッシュの頂点
        Vector3 I1, I2;//三角形メッシュとカットする平面の交点


        for (int x = 0; x < boxSize[0]; x++)
        {
            for (int z = 0; z < boxSize[2]; z++)
            {
                foreach (var faceNum in voxelGrid[x][5][z].crossedFacesList)
                {
                    P0 = solid.sfaces[faceNum].fedges.hvert.vr;
                    P1 = solid.sfaces[faceNum].fedges.next.hvert.vr;
                    P2 = solid.sfaces[faceNum].fedges.next.next.hvert.vr;

                }
            }
        }

    }

    static class VoxelFunctions
    {
        public static Vector3 DirectionToVector(int direction)
        {
            switch (direction)
            {
                case 0: return Vector3.right;
                case 1: return Vector3.left;
                case 2: return Vector3.up;
                case 3: return Vector3.down;
                case 4: return Vector3.forward;
                case 5: return Vector3.back;
            }
            return Vector3.zero;
        }

        public static int VectorToDirection(Vector3 dir)
        {
            if ((dir / dir.magnitude) == Vector3.right) return 0;
            else if ((dir / dir.magnitude) == Vector3.left) return 1;
            else if ((dir / dir.magnitude) == Vector3.up) return 2;
            else if ((dir / dir.magnitude) == Vector3.down) return 3;
            else if ((dir / dir.magnitude) == Vector3.forward) return 4;
            else if ((dir / dir.magnitude) == Vector3.back) return 5;
            else return 6;
        }
    }
    //public class DisplayCondition
    //{
    //    public bool inside = true;                     //内部ボクセルかどうか
    //    public bool hollow = true;                    //中空のボクセルかどうか
    //    public bool suport = true;
    //    public bool floot = false;
    //    public bool CheckVoxel(Voxel xyz)
    //    {
    //        var possible = false;
    //        if (inside) { if (xyz.fill) possible = true; }
    //        //if (possible && hollow) { if (xyz.lego != null && !xyz.lego.hollow) possible = true; else possible = false; }
    //        //if (possible && floot) { if (xyz.lego != null && xyz.lego.CheckFloot()) possible = true; else possible = false; }

    //        if (suport) { if (xyz.suport) possible = true; }
    //        return possible;
    //    }

    //    //public bool CheckLego(Lego xyz)
    //    //{
    //    //    var possible = false;
    //    //    if (hollow) { if (xyz != null && !xyz.hollow) possible = true; else possible = false; }
    //    //    if (possible && floot) { if (xyz != null && xyz.CheckOutFloot()) possible = true; else possible = false; }

    //    //    //if (suport) { if (xyz.suport) possible = true; }
    //    //    return possible;
    //    //}
    //}
}