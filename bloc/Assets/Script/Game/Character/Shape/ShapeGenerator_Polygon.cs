using System.Collections.Generic;
using UnityEngine;

public class ShapeGenerator_Polygon : ShapeGenerator_abstract
{
    #region Setフィールド

    #region メソッド

    public ShapeGenerator_Polygon(int N_Angular = 3,float size = 100 , bool flatDown = true)
    {
        versize = size;
        ShapeSetNAnglar(N_Angular, flatDown);
    }

    public override void ShapeSetNAnglar(int N_Angular, bool flatDown = true)
    {
        GenerateVertices(N_Angular, flatDown);
        GenerateTriangles(N_Angular);
        GenerateSpritePoints();
    }

    #region Generate

    private void GenerateVertices(int N_Angular, bool flatDown)
    {
        Vector2[] vector2s = new Vector2[N_Angular];

        flatDownOffset = 0f;
        if (flatDown)
        {
            if (N_Angular % 2 != 0)
            {
                flatDownOffset = Mathf.PI / 2f; // 奇数：頂点が下向き
            }
            else
            {
                flatDownOffset = (N_Angular - 2 > 3 && (N_Angular - 2) % 4 == 0) ? 0f : Mathf.PI / N_Angular;
            }
        }


        for (int i = 0; i < N_Angular; i++)
        {
            float angle = ((i * Mathf.PI * 2f) / N_Angular) + flatDownOffset;
            vector2s[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        SetVertices = vector2s;
    }

    private void GenerateTriangles(int vertexCount)
    {
        List<ushort> indices = new List<ushort>();
        for (int i = 1; i < vertexCount-1; i++)
        {
            indices.Add(0);
            indices.Add((ushort)i);
            indices.Add((ushort)(i + 1));
        }
        triangles = indices.ToArray();
    }

    private void GenerateSpritePoints()
    {
        Vector2[] vertices = OfsVertices(TextAnchor.UpperRight);

        Vector2 min = vertices[0];
        Vector2 max = vertices[0];
        foreach (var v in vertices)
        {
            min = Vector2.Min(min, v);
            max = Vector2.Max(max, v);
        }

        Vector2 size = max - min;
        Vector2 pivotOffset = size * 0.5f;


        Texture2D tex = new Texture2D(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y));

        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                tex.SetPixel(x, y, Color.white); // Alpha = 1.0 の白で塗りつぶし
            }
        }

        tex.Apply();

        Vector2[] adjustedVertices = new Vector2[length];
        Vector2[] points = new Vector2[length+1];

        for (int i = 0; i < length; i++)
        {
            adjustedVertices[i] = vertices[i] - min;

            points[i] = (adjustedVertices[i] - pivotOffset) / 100f;
        }

        points[length] = points[0];

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size.x, size.y), new Vector2(0.5f, 0.5f), versize);
        sprite.OverrideGeometry(adjustedVertices, triangles);

        this.sprite = sprite;
        ColliderPoints = points;

    }
    #endregion

    #endregion

    #region プロパティ
    public Vector2[] SetVertices
    {
        set
        {
            vertices = value;
            length = vertices.Length;
        }
    }

    #endregion

    #endregion


    #region 位置調整

    // 特定の形状だけ位置をずらす用のオフセット
    public override Vector3 ShapeCase(int N_Angular)
    {
        Vector2 ofs = Vector2.zero;
        if (N_Angular % 2 == 1)
        {
            ofs.y = OffsetYQuick(N_Angular);
        }


        return ofs;
    }

    float OffsetYQuick(int n)
    {
        // 手軽な早見式
        return 2.25f / (n * n);
    }

    float OffsetYFitted(int n)
    {
        // 近似フィット版（提示データに合わせて調整）
        float y = -0.00670f + 0.05742f / n + 2.1377f / (n * n);
        return Mathf.Max(0f, y);
    }
    #endregion


}
