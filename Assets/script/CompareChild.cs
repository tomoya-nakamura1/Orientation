using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CompareChild : MonoBehaviour
{

    public List<float> minList = new List<float>();
    public List<float> maxList = new List<float>();
    public List<float> newContoursList = new List<float>();//画像ごとのピクセル当たりの色差の平均
    public List<float> oldContoursList = new List<float>();//画像ごとのピクセル当たりの色差の平均
    public List<float> meshNumList = new List<float>();//画像ごとのメッシュモデルのピクセル数
    public List<float> newDiffRateList = new List<float>();//画像ごとのメッシュモデルのピクセル数
    public List<float> oldDiffRateList = new List<float>();//画像ごとのメッシュモデルのピクセル数
    public List<float> newVoxelList = new List<float>();//画像ごとのメッシュモデルのピクセル数
    public List<float> oldVoxelList = new List<float>();//画像ごとのメッシュモデルのピクセル数
    public float newContours = 0;
    public float oldContours = 0;
    public float diffTexMin = 900000000000000000f;
    public float diffPlyMin = 900000000000000000f;
    public float diffTexMax = 0;
    public float diffPlyMax = 0;
    public float diffMax = 0;
    // Use this for initialization
    void Start()
    {
        //float totalDis=0;
        string fileNameMesh = "monoMesh";
        string fileNameNew = "monoNew";
        string fileNameOld = "monoOld";
        //string texName = "tex";
        //string plyName = "ply";
        string newContoursFileName = fileNameNew + "Contours";
        string oldContoursFileName = fileNameOld + "Contours";
        //Debug.Log(texCBarFileName);
        //Debug.Log(plyCBarFileName);

        for (int i = 0; i < 20; i++)
        {
            string passMesh = pathMake(i + 1, fileNameMesh);
            string passNew = pathMake(i + 1, fileNameNew);
            string passOld = pathMake(i + 1, fileNameOld);
            string passNewContours = pathMake(i + 1, newContoursFileName);
            string passOldContours = pathMake(i + 1, oldContoursFileName);
            Texture2D meshTexture = ReadTexture(passMesh);
            Texture2D newTexture = ReadTexture(passNew);
            Texture2D oldTexture = ReadTexture(passOld);
            //Debug.Log(passTex);
            //Debug.Log(passPly);
            //Debug.Log(passTexCBar);
            //Debug.Log(passPlyCBar);
            //totalDis += CompareTex(plyTex, meshTex);
            //totalDis += CompareLab(meshTexutre, texTexutre, plyTexutre);
            CompareContours(meshTexture, newTexture, oldTexture, newContoursFileName, oldContoursFileName, i);
            //OutTexture(meshTexture, newTexture, oldTexture, newContoursFileName, oldContoursFileName, i);

        }
        foreach (var each in newContoursList)
        {
            //Debug.Log(each);
            //if (diffTexMin>each)
            //{
            //    diffTexMin = each;
            //}
            //if (diffTexMax < each)
            //{
            //    diffTexMax = each;
            //}

            newContours += each;
        }
        foreach (var each in oldContoursList)
        {
            //Debug.Log(each);
            //if (diffPlyMin > each)
            //{
            //    diffPlyMin = each;
            //}
            //if (diffPlyMax < each)
            //{
            //    diffPlyMax = each;
            //}

            oldContours += each;
        }

        for (int i = 0; i < 20; i++)
        {
            float newDiffRate = 0;
            float oldDiffRate = 0;
            newDiffRate = newContoursList[i] / newVoxelList[i];
            oldDiffRate = oldContoursList[i] / oldVoxelList[i];

            newDiffRateList.Add(newDiffRate);
            oldDiffRateList.Add(oldDiffRate);
        }

        for (int i = 0; i < 20; i++)
        {
            float diff;
            diff = oldContoursList[i] - newContoursList[i];
            Debug.Log(i + " :" + diff);
            if (diffMax < diff)
            {
                diffMax = diff;
            }
            Debug.Log("max" + maxList[i] + " " + "min" + minList[i]);

        }
       float newDiffRateAve=0;
       float oldDiffRateAve=0;

        for (int i = 0; i < 20; i++)
        {

            newDiffRateAve +=  newDiffRateList[i];
            oldDiffRateAve +=  oldDiffRateList[i];
        }
        newDiffRateAve = newDiffRateAve / 20f;
        oldDiffRateAve = oldDiffRateAve / 20f;

        newContours = newContours / 20f;
        oldContours = oldContours / 20f;
        Debug.Log("newContours:" + newContours + "oldContours: " + oldContours);
        Debug.Log("newDiffRateAve:" + newDiffRateAve);
        Debug.Log("oldDiffRateAve:" + oldDiffRateAve);
        //Debug.Log(fileName +": " + totalDis);

    }
    void OutTexture(Texture2D meshTexture, Texture2D texTexture, Texture2D plyTexture
        , string texCBarFileName, string plyCBarFileName, int num)
    {
        List<List<Color>> meshColors = new List<List<Color>>();
        List<List<Color>> texColors = new List<List<Color>>();
        List<List<Color>> plyColors = new List<List<Color>>();
        meshColors = Substitute(meshTexture);
        texColors = Substitute(texTexture);
        plyColors = Substitute(plyTexture);
        List<List<Color>> texCBarColors = new List<List<Color>>();
        List<List<Color>> plyCBarColors = new List<List<Color>>();

        //Debug.Log(texColors1[0][0].b)
        //画像のサイズは一緒なこと前提
        int width = meshTexture.width;
        int height = meshTexture.height;
        //Debug.Log("width: " + width + " height: " + height);
        float texTotalDis = 0;
        float plyTotalDis = 0;
        int texCount = 0;
        int plyCount = 0;
        float min = 0;
        float max = 0;

        for (int i = 0; i < width; i++)
        {
            List<Color> rowTexPixel = new List<Color>();
            List<Color> rowPlyPixel = new List<Color>();
            for (int j = 0; j < height; j++)
            {

                rowTexPixel.Add(new Color());
                rowPlyPixel.Add(new Color());

            }
            texCBarColors.Add(rowTexPixel);
            plyCBarColors.Add(rowPlyPixel);



            for (int j = 0; j < height; j++)
            {
                if ((meshColors[i][j].b > 250 && meshColors[i][j].r < 5 && meshColors[i][j].g < 5)
                    || (texColors[i][j].b > 250 && texColors[i][j].r < 5 && texColors[i][j].g < 5))
                {
                    texCBarColors[i][j] = Color.black;
                }
                else
                {
                    Color colors1 = RGB_to_Lab(meshColors[i][j]);
                    Color colors2 = RGB_to_Lab(texColors[i][j]);
                    float delta = DeltaE(colors1, colors2);
                    //texCBarColors[i][j] = ColorBar(delta, maxList[num]/5f, 3f*minList[num], 0, true);
                    texCBarColors[i][j] = ColorBar(delta, 1600f, 3f * minList[num], 0, true);



                }

                //if (meshColors[i][j].b > 250 || plyColors[i][j].b > 250)
                if ((meshColors[i][j].b > 250 && meshColors[i][j].r < 5 && meshColors[i][j].g < 5)
                 || (plyColors[i][j].b > 250 && plyColors[i][j].r < 5 && plyColors[i][j].g < 5))
                {
                    plyCBarColors[i][j] = Color.black;

                }
                else
                {
                    Color colors1 = RGB_to_Lab(meshColors[i][j]);
                    Color colors2 = RGB_to_Lab(plyColors[i][j]);
                    float delta = DeltaE(colors1, colors2);
                    //plyCBarColors[i][j] = ColorBar(delta, maxList[num]/5f, 3f*minList[num], 0, true);
                    plyCBarColors[i][j] = ColorBar(delta, 1600f, 3f * minList[num], 0, true);


                }

            }
        }
        Texture2D texCBarTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        Texture2D plyCBarTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        for (int i = 0; i < width; i++)
        {

            for (int j = 0; j < height; j++)
            {
                texCBarTexture.SetPixel(i, j, texCBarColors[i][j]);
                plyCBarTexture.SetPixel(i, j, plyCBarColors[i][j]);

            }

        }
        texCBarTexture.Apply();
        plyCBarTexture.Apply();

        byte[] texBytes = texCBarTexture.EncodeToPNG();
        byte[] plyBytes = plyCBarTexture.EncodeToPNG();

        //File.WriteAllBytes(texCBarFileName, texBytes);
        File.WriteAllBytes(Application.dataPath + "/" + "compare" + "/" + "texCBar" + "/" + (num + 1).ToString() + ".png", texBytes);
        File.WriteAllBytes(Application.dataPath + "/" + "compare" + "/" + "plyCBar" + "/" + (num + 1).ToString() + ".png", plyBytes);
        //File.WriteAllBytes(plyCBarFileName, plyBytes);

    }


    void CompareContours(Texture2D meshTexture, Texture2D newTexutre, Texture2D oldTexutre,
        string texCBarFileName, string plyCBarFileName, int num)
    {
        List<List<Color>> meshColors = new List<List<Color>>();
        List<List<Color>> newColors = new List<List<Color>>();
        List<List<Color>> oldColors = new List<List<Color>>();
        List<List<Color>> newContours = new List<List<Color>>();
        List<List<Color>> oldContours = new List<List<Color>>();
        meshColors = Substitute(meshTexture);
        newColors = Substitute(newTexutre);
        oldColors = Substitute(oldTexutre);
        newContours = SubstituteWhite(newTexutre);
        oldContours = SubstituteWhite(oldTexutre);

        //Debug.Log(texColors1[0][0].b)
        //画像のサイズは一緒なこと前提
        int width = meshTexture.width;
        int height = meshTexture.height;
        //Debug.Log("width: " + width + " height: " + height);
        float texTotalDis = 0;
        float plyTotalDis = 0;
        float newCount = 0;
        float oldCount = 0;
        float newVoxelCount = 0;
        float oldVoxelCount = 0;
        float min = 0;
        float max = 0;

        float meshNum = 0;



        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //if (meshColors[i][j].r > 250 ^ newColors[i][j].r > 250)//排他的論理和
                //{
                //    newContours[i][j] = Color.red;
                //    newCount++;
                //}
                //else
                //{

                //}
                if (meshColors[i][j].r > 250 )//白
                {
                    meshNum++;
                    if (newColors[i][j].r > 250)//白
                    {

                    }
                    else  //青
                    {
                     　　newContours[i][j] = Color.blue;
                    newCount++;

                    }


                }
                else //青
                {
                    if (newColors[i][j].r > 250)//白
                    {
                        newContours[i][j] = Color.red;
                        newCount++;
                    }
                    else  //青
                    {

                    }

                }

                if (meshColors[i][j].r > 250 ^ oldColors[i][j].r > 250)
                {
                    oldContours[i][j] = Color.red;

                    oldCount++;
                }
                else
                {
                    //Color colors1 = RGB_to_Lab(meshColors[i][j]);
                    //Color colors2 = RGB_to_Lab(oldColors[i][j]);
                    //float delta = DeltaE(colors1, colors2);
                    //if (delta < min)
                    //{
                    //    min = delta;
                    //}
                    //if (delta > max)
                    //{
                    //    max = delta;
                    //}
                    //plyTotalDis += delta;
                    ////texTotalDis += DeltaE(texColors1[i][j], texColors2[i][j]);

                }
                if ( oldColors[i][j].r > 250)
                {
                    
                    oldVoxelCount++;
                }
                if (newColors[i][j].r > 250)
                {
                    
                    newVoxelCount++;
                }


            }
        }
        minList.Add(min);
        maxList.Add(max);
        newContoursList.Add(newCount);
        oldContoursList.Add(oldCount);
        newVoxelList.Add(newVoxelCount);
        oldVoxelList.Add(oldVoxelCount);
        meshNumList.Add(meshNum);
        //Debug.Log(texTotalDis / texCount);
        //Debug.Log(plyTotalDis / plyCount);
        //Debug.Log(texCount);
        //Debug.Log(plyCount);
        //return texTotalDis;

        Texture2D texCBarTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        Texture2D plyCBarTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        for (int i = 0; i < width; i++)
        {

            for (int j = 0; j < height; j++)
            {
                texCBarTexture.SetPixel(i, j, newContours[i][j]);
                plyCBarTexture.SetPixel(i, j, oldContours[i][j]);

            }

        }
        texCBarTexture.Apply();
        plyCBarTexture.Apply();

        byte[] texBytes = texCBarTexture.EncodeToPNG();
        byte[] plyBytes = plyCBarTexture.EncodeToPNG();

        //File.WriteAllBytes(texCBarFileName, texBytes);
        File.WriteAllBytes(Application.dataPath + "/" + "compare" + "/" + "monoNewContours" + "/" + (num + 1).ToString() + ".png", texBytes);
        File.WriteAllBytes(Application.dataPath + "/" + "compare" + "/" + "monoOldContours" + "/" + (num + 1).ToString() + ".png", plyBytes);
        //File.WriteAllBytes(plyCBarFileName, plyBytes);

    }
    float CompareTex(Texture2D tex1, Texture2D tex2)
    {
        List<List<Color>> texColors1 = new List<List<Color>>();
        List<List<Color>> texColors2 = new List<List<Color>>();
        texColors1 = Substitute(tex1);
        texColors2 = Substitute(tex2);

        //Debug.Log(texColors1[0][0].b)

        int width1 = tex1.width;
        int height1 = tex1.height;
        //int width2 = tex2.width;
        //int height2 = tex2.height;
        float texTotalDis = 0;
        int count = 0;
        for (int i = 0; i < width1; i++)
        {
            for (int j = 0; j < height1; j++)
            {
                //if(texColors1[i][j].b==1 || texColors2[i][j].b == 1)
                if (texColors1[i][j].b > 250 || texColors2[i][j].b > 250)
                {
                    count++;
                }
                else
                {
                    Color colors1 = RGB_to_Lab(texColors1[i][j]);
                    Color colors2 = RGB_to_Lab(texColors2[i][j]);
                    texTotalDis += DeltaE(colors1, colors2);
                    //texTotalDis += DeltaE(texColors1[i][j], texColors2[i][j]);

                }

            }
        }
        Debug.Log(count);

        return texTotalDis;

    }
    public List<List<Color>> Substitute(Texture2D texture)//RGBまで
    {
        int width = texture.width;
        int height = texture.height;
        List<List<Color>> texColor = new List<List<Color>>();


        for (int i = 0; i < width; i++)
        {
            List<Color> rowPixel = new List<Color>();
            for (int j = 0; j < height; j++)
            {
                Color tempRGB = texture.GetPixel(i, j);
                tempRGB.r *= 255f;
                tempRGB.g *= 255f;
                tempRGB.b *= 255f;
                //rowPixel.Add(new Color());
                rowPixel.Add(tempRGB);

            }
            texColor.Add(rowPixel);
        }
        return texColor;
    }

    public List<List<Color>> SubstituteWhite(Texture2D texture)//RGBまで//0～1
    {
        int width = texture.width;
        int height = texture.height;
        List<List<Color>> texColor = new List<List<Color>>();


        for (int i = 0; i < width; i++)
        {
            List<Color> rowPixel = new List<Color>();
            for (int j = 0; j < height; j++)
            {
                //Color tempRGB = texture.GetPixel(i, j);
                Color tempRGB = new Color();
                tempRGB = Color.white;
                //tempRGB.r *= 255f;
                //tempRGB.g *= 255f;
                //tempRGB.b *= 255f;
                //rowPixel.Add(new Color());
                rowPixel.Add(tempRGB);

            }
            texColor.Add(rowPixel);
        }
        return texColor;
    }

    public string pathMake(int num, string file)
    {

        //int asdf=0;
        string fileName = "compare/" + file + "/" + num.ToString() + ".png";
        string path = Path.Combine(Application.dataPath, fileName);
        return path;

    }
    public byte[] ReadPngFile(string path)
    {
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        BinaryReader bin = new BinaryReader(fileStream);
        byte[] values = bin.ReadBytes((int)bin.BaseStream.Length);

        bin.Close();

        return values;
    }
    Texture2D ReadTexture(string path)//, int width, int height)
    {
        byte[] readBinary = ReadPngFile(path);
        int pos = 16; // 16バイトから開始

        int width = 0;
        for (int i = 0; i < 4; i++)
        {
            width = width * 256 + readBinary[pos++];
        }
        int height = 0;
        for (int i = 0; i < 4; i++)
        {
            height = height * 256 + readBinary[pos++];
        }

        Texture2D texture = new Texture2D(width, height);
        texture.LoadImage(readBinary);

        return texture;
    }
    //public static Texture2D ReadPng(string path)
    //{
    //    byte[] readBinary = ReadPngFile(path);

    //    //int width = 0;
    //    //int height = 0;
    //    int pos = 16; // 16バイトから開始

    //    int width = 0;
    //    for (int i = 0; i < 4; i++)
    //    {
    //        width = width * 256 + readBinary[pos++];
    //    }

    //    int height = 0;
    //    for (int i = 0; i < 4; i++)
    //    {
    //        height = height * 256 + readBinary[pos++];
    //    }

    //    Texture2D texture = new Texture2D(width, height);
    //    texture.LoadImage(readBinary);

    //    return texture;
    //}
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
    //【7】カラーバー用関数
    public Color ColorBar(float value, float max, float min, int PalettesCaseNumber, bool LinearFlag)
    {
        Color rgb;
        if (value > max) value = max;
        if (value < min) value = min;

        float H, S, V;
        float startH, endH;

        S = 1.0f;    //彩度 1.0で固定
        V = 1.0f;    //明度 1.0で固定

        switch (PalettesCaseNumber)
        {
            case 0: //B->G->R
                startH = 240.0f; endH = 0.0f;
                break;
            case 1: //G->R
                startH = 120.0f; endH = 0.0f;
                break;
            case 2://B->G
                startH = 240.0f; endH = 120.0f;
                break;
            default://R->M->B->C->G->Y->R
                startH = 359.9999f; endH = 0.0f;
                break;
        }

        H = startH - (startH - endH) * (value - min) / (max - min);
        H = H / 360f;
        rgb = Color.HSVToRGB(H, S, V, LinearFlag);
        return rgb;
        //r = rgb.r;
        //g = rgb.g;
        //b = rgb.b;
    }

}
