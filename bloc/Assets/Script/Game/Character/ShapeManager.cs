using System.Collections.Generic;
using R3;
using R3.Triggers;
using UnityEngine;

public class ShapeManager : MonoBehaviour
{
    int N_Angular = 3; // 頂点の数（3角形）
    void Start()
    {
        SetSprite();
        GetComponent<EdgeCollider2D>().OnCollisionEnter2DAsObservable()
            .Subscribe(collision =>
            {
                SetSprite();
            })
            .AddTo(this);
    }

    void SetSprite()
    {
        Debug.Log("nowAngular: " + N_Angular);

        transform.position = Vector3.zero;

        Shape shape = new Shape();
        shape.ShapeSetNAnglar(N_Angular, true);
        N_Angular++;

        Vector2[] vertices = shape.OfsVertices(100, TextAnchor.UpperRight);

        Vector2 min = vertices[0];
        Vector2 max = vertices[0];
        foreach (var v in vertices)
        {
            min = Vector2.Min(min, v);
            max = Vector2.Max(max, v);
        }

        Vector2 size = max - min;
        Vector2 pivotOffset = size * 0.5f;

        // ② テクスチャ作成
        Texture2D tex = new Texture2D(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y));
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        // ③ 頂点を min でオフセット
        Vector2[] adjustedVertices = new Vector2[vertices.Length];
        Vector2[] points = new Vector2[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            adjustedVertices[i] = vertices[i] - min;

            // ピボット中央に合わせて調整し、pixelsPerUnitでワールド単位に変換
            points[i] = (adjustedVertices[i] - pivotOffset) / 100f;
        }

        // ④ Sprite作成（pivot = 中央）
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size.x, size.y), new Vector2(0.5f, 0.5f), 100f);
        sprite.OverrideGeometry(adjustedVertices, shape.triangles);

        // ⑤ SpriteRenderer設定
        var sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = Color.white;
        sr.material = new Material(Shader.Find("Sprites/Default"));
        sr.drawMode = SpriteDrawMode.Simple;
        sr.size = new Vector2(100, 100);

        // ⑥ Collider設定
        GetComponent<EdgeCollider2D>().points = points;
    }
}

public class Shape
{
    public void ShapeSetNAnglar(int N_Angular, bool flatDown = true)
    {
        Vector2[] vector2s = new Vector2[N_Angular];

        float flatDownOffset = 0f;
        if (flatDown)
        {
            int diff = N_Angular - 2;

            bool treatAsOdd = (N_Angular % 2 == 0);

            flatDownOffset = treatAsOdd
                ? ((diff > 3 && diff % 4 == 0)) ? 0 : Mathf.PI / N_Angular      // 偶数扱い → 辺が下向き
                : Mathf.PI / 2f;              // 奇数扱い → 頂点が下向き
        }

        for (int i = 0; i < N_Angular; i++)
        {
            float angle = ((i * Mathf.PI * 2f) / N_Angular) + flatDownOffset;
            vector2s[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        SetOfsVertices = vector2s;
        triangles = GenerateFanTriangles(N_Angular);
    }



    private ushort[] GenerateFanTriangles(int vertexCount)
    {
        List<ushort> indices = new List<ushort>();
        for (int i = 1; i < vertexCount-1; i++)
        {
            indices.Add(0);
            indices.Add((ushort)i);
            indices.Add((ushort)(i + 1));
        }
        return indices.ToArray();
    }

    protected int natlength { get; private set; }
    protected int inclength { get { return natlength + 1; } }

    //public Vector2[] vertices { get; private set; }

    public Vector2[] vertices { get; private set; }

    public Vector2[] circleFittedVer { get; private set; }

    public Vector2[] SetOfsVertices {
        set {
            vertices = value;
            circleFittedVer = GetUnitCircleFittedVertices();

            natlength = vertices.Length;
        }
    }

    public ushort[] triangles { get; private set; }

    public Vector2[] OfsVertices(float length, TextAnchor CenterDir = TextAnchor.MiddleCenter)
    {
        Vector2 ofs = AnchorCase(CenterDir) * length;
        return OfsVertices(ofs , length);
    }


    public Vector2[] OfsVertices(Vector2 ofs , float length)
    {
        Vector2[] newOfsVertices = new Vector2[natlength];
        for (int i = 0; i < natlength; i++)
        {
            newOfsVertices[i] = (vertices[i] * length) + ofs;
        }

        return newOfsVertices;
    }

    Vector2 AnchorCase(TextAnchor TA)
    {
        Vector2 ofs;
        switch (TA)
        {
            case TextAnchor.UpperLeft:
                ofs = new Vector2(0, 1);
                break;
            case TextAnchor.UpperCenter:
                ofs = new Vector2(0.5f, 1);
                break;
            case TextAnchor.UpperRight:
                ofs = new Vector2(1, 1);
                break;
            case TextAnchor.MiddleLeft:
                ofs = new Vector2(0, 0.5f);
                break;
            case TextAnchor.MiddleCenter:
                ofs = new Vector2(0.5f, 0.5f);
                break;
            case TextAnchor.MiddleRight:
                ofs = new Vector2(1, 0.5f);
                break;
            case TextAnchor.LowerLeft:
                ofs = new Vector2(0, 0);
                break;
            case TextAnchor.LowerCenter:
                ofs = new Vector2(0.5f, 0);
                break;
            case TextAnchor.LowerRight:
                ofs = new Vector2(1, 0);
                break;
            default:
                ofs = Vector2.zero;
                break;
        }

        return ofs - Vector2.one * 0.5f; // 中央を基準にするためオフセットを調整
    }


    public Vector2[] GetUnitCircleFittedVertices()
    {
        float maxRadius = 0f;

        // 中心からの最大距離を取得
        foreach (var v in vertices)
        {
            float dist = v.magnitude;
            if (dist > maxRadius)
                maxRadius = dist;
        }

        // 全ての点を最大半径でスケーリング → 単位円内に収まるように
        Vector2[] scaled = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            scaled[i] = vertices[i] / maxRadius;
        }

        return scaled;
    }
}
