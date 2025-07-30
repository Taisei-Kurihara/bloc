using System.Collections.Generic;
using R3;
using R3.Triggers;
using UnityEngine;

public class ShapeModel : ModelBase , IShape
{
    public ShapeModel(PresenterBase presenter,int An = 4) : base(presenter)
    {
        this.presenter = presenter;
        N_Angular = An;
    }

    [SerializeField]
    int N_Angular = 4;

    SpriteRenderer spriteRenderer;

    EdgeCollider2D edgeCollider;

    public Shape shape { get; private set; }

    public override void Init()
    {
        spriteRenderer = presenter.GetComponent<SpriteRenderer>();
        edgeCollider = presenter.GetComponent<EdgeCollider2D>();
        shape = new Shape(N_Angular);
        SetAll();

        presenter.GetComponent<Collider2D>().OnCollisionEnter2DAsObservable().Subscribe(collision =>
        {
            Debug.Log("N_Angular:" + N_Angular);
            presenter.transform.position = Vector2.zero;
            shape = new Shape(N_Angular);
            SetAll();
            N_Angular++;
        }).AddTo(presenter);
    }

    void SetAll(int An)
    {
        shape.ShapeSetNAnglar(An, true);
        SetAll();
    }
    void SetAll()
    {
        SetSprite();
        SetColl();
    }

    void SetSprite(int An)
    {
        shape.ShapeSetNAnglar(An, true);
        SetSprite();
    }

    void SetSprite()
    {
        spriteRenderer.sprite = shape.sprite;
        spriteRenderer.size = new Vector2(100, 100);
    }


    void SetColl(int An)
    {
        shape.ShapeSetNAnglar(An, true);
        SetColl();
    }

    void SetColl()
    {
        edgeCollider.points = shape.ColliderPoints;
    }


}





public class Shape
{
    #region Setフィールド

    #region メソッド

    public Shape(int N_Angular = 3,float size = 100 , bool flatDown = true)
    {
        versize = size;
        ShapeSetNAnglar(N_Angular, flatDown);
    }

    public void ShapeSetNAnglar(int N_Angular, bool flatDown = true)
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



    #region Getフィールド

    public float flatDownOffset { get; private set; } = 0f; // 初期角度

    public int length { get; private set; }
    public float versize { get; private set; }

    public Vector2[] vertices { get; private set; } 

    public ushort[] triangles { get; private set; }


    public Sprite sprite { get; private set; }

    public Vector2[] ColliderPoints { get; private set; }
    #endregion



    #region 位置調整

    public Vector2[] OfsVertices(float size, TextAnchor CenterDir = TextAnchor.MiddleCenter)
    {
        versize = size;
        Vector2 ofs = AnchorCase(CenterDir) * size;

        return OfsVertices(ofs , size);
    }

    public Vector2[] OfsVertices(TextAnchor CenterDir = TextAnchor.MiddleCenter)
    {
        Vector2 ofs = AnchorCase(CenterDir) * versize;
        return OfsVertices(ofs, versize);
    }

    private Vector2[] OfsVertices(Vector2 ofs , float size)
    {
        Vector2[] newOfsVertices = new Vector2[this.length];
        for (int i = 0; i < this.length; i++)
        {
            newOfsVertices[i] = (vertices[i] * size) + ofs;
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

    #endregion


}
