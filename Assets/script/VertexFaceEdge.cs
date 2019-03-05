using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Linq;

public class Vertex
{

    public Vector3 vr;
    //= new Vector3();           //頂点の座標
    public Vector3 nr;
    //=new Vector3();           //頂点の法線ベクトル	//グーローシェーディングなどで使う
    public HalfEdge vedge;
    //= new HalfEdge();    //この頂点を根に持つHalfEdgeの一つ
    public bool Boundary;
    //= new bool();      //境界上の点かどうか。境界ならtrue

    //この頂点を根に持つHalfEdgeの配列
    public List<HalfEdge> vhedges = new List<HalfEdge>(); //CCWに並べてある
    //
    //public Color color;
    public Color color=new Color();
    public float hue;//色相 - 色の種類（例えば赤、青、黄色）。0 - 360の範囲（アプリケーションによっては0 - 100 % に正規化されることもある）。
    public float saturation;//彩度 - 色の鮮やかさ。0 - 100 % の範囲。刺激純度と colorimetric purity の色彩的な量と比較して「純度」などともいう。
    //色の彩度の低下につれて、灰色さが顕著になり、くすんだ色が現れる。また彩度の逆として「desaturation」を定義すると有益である。
    public float value;//明度 - 色の明るさ。0 - 100 % の範囲。
    public float luminance;//輝度値

    public int colorNum;
    //public Vertex()
    //{
    //    vr = new Vector3();
    //    nr = new Vector3();
    //    vedge = new HalfEdge();
    //    Boundary = new bool();
    //    vhedges = new List<HalfEdge>();
    //}

}

public class HalfEdge
{

    public Vertex hvert;
    // = new Vertex();  //根に持つ頂点
    public Edge hedge;
    //= new Edge();    //属する辺
    public Face hface;
    //= new Face();    //属する面
    public HalfEdge prev;
    //= new HalfEdge(); //前のHalfEdge
    public HalfEdge next;//= new HalfEdge(); //次のHalfEdge

    // HalfEdge HalfEdgeMate(void);   //同じEdgeに属する、もう一方のHalfEdgeを呼び出す関数
    //void HalfEdgeConstruct(Face* f, Vertex* v);     //面と頂点の関係からHalfEdgeを構築

    //面の情報を元にHalfEdgeを作る
    public void HalfEdgeConstruct(Face f, Vertex v)
    {
        hface = f;      //このHalfEdgeが属する面
        hvert = v;      //このHalfEdgeの根元の頂点
                        //v->vedge= this;		//ここだと、何度も代入が起きる	//別の場所でvedgeは定義する
        v.vhedges.Add(this); //vを根元に持つHalfEdgeを動的に格納
        //合ってるかわかんない
        f.AddHalfEdge(this);   //同一の面のHalfEdgeをループさせる。
    }

    //public HalfEdge()
    //{

    //    hvert = new Vertex();  //根に持つ頂点
    //    hedge = new Edge();    //属する辺
    //    hface = new Face();    //属する面
    //                           // prev = new HalfEdge(); //前のHalfEdge
    //                           //next = new HalfEdge(); //次のHalfEdge



    //    hvert = null;
    //    hedge = null;
    //    hface = null;
    //    // prev = next = this;     //初期値は自分自身
    //}

    //対となるHalfEdgeを返す
    public HalfEdge HalfEdgeMate()
    {
        if (hedge != null)
        {   //もし、Edgeが構築されていれば
            if (hedge.he1 == this) return hedge.he2;  //自分じゃない方がmate
            else return hedge.he1;
        }

        Vertex start, end;        //このHalfEdgeはstartからendを指している
        start = this.hvert;        //自分の根元の頂点	//thisってのは自分自身のこと
        end = this.next.hvert;    //どんなHalfEdgeでも必ずnextはあり、その根元は自信が指す頂点

        for (int i = 0; i < (int)end.vhedges.Count; ++i)
        {   //endを根に持つHalfEdgeを一つ一つめぐる
            if (end.vhedges[i].next.hvert == start)
            {   //もし、HalfEdgeがstartを指していたら
                return end.vhedges[i];             //mate発見
            }
        }

        return null;    //MateがないときはNULL。
    }


}

public class Edge
{

    public HalfEdge he1;//= new HalfEdge();
    //=null;  //このEdgeが持つHalfEdgeの一つ
    public HalfEdge he2;//= new HalfEdge();
                        //=null;  //he1じゃない方のHalfEdge
    public int num;   //= new int();                    //頂点番号
    public int colorNum;


    //void EdgeConstruct(HalfEdge Hedge, HalfEdge Hmate);   //２本のHalfEdgeからEdgeを構築

    //public Edge()
    //{
    //    he1 = new HalfEdge();
    //    he2 = new HalfEdge();
    //    num = new int();
    //}

    //２本のHalfEdgeから１本のEdgeを作る。
    //境界線かどうかは、he2がNULLかどうかで判定
    public void EdgeConstruct(HalfEdge Hedge, HalfEdge Hmate)
    {
        he1 = Hedge;
        he2 = Hmate;
        he1.hedge = this;  //he1とリンク



        if (he2 == null)
        {   //he2がNULLであれば、このEdgeは境界
            he1.hvert.Boundary = true;   //根元の頂点も境界上の点
        }
        else
        {
            he2.hedge = this;  //he2ともリンク
        }
    }
}

public class Face
{

    public int sidednum = 3;       //ｎ角形のｎのこと。５角形なら５になる
    public HalfEdge fedges = null;   //この面に属するHalfEdgeの一つ
                                     //void AddHalfEdge(HalfEdge he); //面にHalfEdgeを追加。既にあるHalfEdgeとループさせる。
    public bool fedges_bool = false;        
    public int colorNum;
    public Vector3 normal;

    public void CalNormal()
    {
        normal = new Vector3();
        normal = Vector3.Cross(fedges.next.hvert.vr- fedges.hvert.vr, fedges.next.next.hvert.vr- fedges.hvert.vr);
    }


    // void FaceDestruct(void);	//明示的に呼び出すデストラクタ。面に属するHalfEdgeを消していく
    //public Face()
    //{
    //    sidednum = new int();
    //    fedges = new HalfEdge();
    //}

    //同一の面のHalfEdgeをループさせる。ここがHalfEdgeの肝。
    //自信がなければ、ここは絶対にいじるな！
    //OFFの頂点番号順に依存している。番号順がめちゃくちゃだと、正しく動きません。
    //頂点番号の順番はモデル全体で一方向に統一して下さい。モデル外側に対してCCWとか。
    public void AddHalfEdge(HalfEdge he)
    {
        //fedges = new HalfEdge();

        if (fedges == null)
        {
            fedges = he;        //最初のHalfEdgeはfedgesになる。以後、fedgesは動かない
            fedges.next = fedges.prev = he;   //HalfEdgeが一つでもループする。
            fedges_bool = true;
        }
        else
        {
            he.prev = fedges.prev;        //末尾のHalfEdgeを、新しいheのprevにする
            he.next = fedges;              //最初のHalfEdgeを、新しいheのnextにする
            fedges.prev = he.prev.next = he; //末尾のHalfEdgeは、新しいheになる。新しいheの前の後は、新しいheになる。
        }
    }
}
