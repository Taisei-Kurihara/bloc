using System.Collections.Generic;
using R3;
using UnityEngine;

public class Shape_Model : ModelBase , Shape_interface
{
    public Shape_Model(CharacterPresenterBase presenter, SpriteRenderer spriteRenderer, EdgeCollider2D edgeCollider,CircleCollider2D circleCollider, int An = 4) : base(presenter)
    {
        this.presenter = presenter;
        N_Angular = An;

        this.spriteRenderer = spriteRenderer;
        this.edgeCollider = edgeCollider;

        offsetobj = edgeCollider.transform;

        this.circleCollider = circleCollider;
        // コライダーのisTrigger設定
        if (this.circleCollider != null) this.circleCollider.isTrigger = true;
    }

    [SerializeField]
    int N_Angular = 4;

    Transform offsetobj;

    public SpriteRenderer spriteRenderer { get; private set; }

    public EdgeCollider2D edgeCollider { get; private set; }

    public Shape_PolygonGenerator shape { get; private set; }

    public CircleCollider2D circleCollider { get; private set; }

    public override void Init()
    {
        this.shape = new Shape_PolygonGenerator(N_Angular);
        SetAll();
        DebugeLineUpdate();
    }

    public void SetShape(int shape)
    {
        this.shape.ShapeSetNAnglar(shape);
        SetAll();
        DebugeLineUpdate();
    }

    void SetAll()
    {
        // 表示・当たり判定を更新
        SetSprite();
        SetColl();
    }


    void SetSprite()
    {
        spriteRenderer.transform.localPosition = shape.ShapeCase(shape.length);

        spriteRenderer.sprite = shape.sprite;
        spriteRenderer.size = new Vector2(100, 100);

    }

    void SetColl()
    {
        circleCollider.radius = shape.versize / 100f;

        if (shape.length >= 24)
        {
            edgeCollider.points = new Vector2[2] { Vector2.zero,Vector2.zero };
            circleCollider.isTrigger = false;
        }
        else
        {
            edgeCollider.points = shape.ColliderPoints;
        }
    }

    #region debug
    public void DebugeN_AngularUpdate()
    {
        Observable.EveryUpdate().Subscribe(_ =>
        {
            Debug.Log("N_Angular:" + N_Angular + "/ofs:" + shape.ShapeCase(N_Angular).y);
            presenter.transform.position = Vector2.zero;
            SetShape(N_Angular);
            N_Angular++;
        }).AddTo(presenter);
    }

    public void DebugeLineUpdate()
    {
        Observable.EveryUpdate().Subscribe(_ =>
        {
            // デバッグ用のラインを描画
            shape.DebugeLine(spriteRenderer.transform, presenter.transform);
        }).AddTo(presenter);
    }

    #endregion

}





public class Shape_PolygonGenerator
{
    #region Setフィールド

    #region メソッド

    public Shape_PolygonGenerator(int N_Angular = 3,float size = 100 , bool flatDown = true)
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

    // 特定の形状だけ位置をずらす用のオフセット
    public Vector3 ShapeCase(int N_Angular)
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

    public void DebugeLine(Transform transform,Transform pre)
    {
        Vector3 A1 = ColliderPoints[0];
        Vector3 B2 = ColliderPoints[0];

        // ポリゴンの辺を描画
        for (int i = 0; i < length; i++)
        {
            A1 = ColliderPoints[i];
            B2 = ColliderPoints[(i + 1) % length];

            Debug.DrawLine(
                transform.TransformPoint(A1), // ローカル→ワールド
                transform.TransformPoint(B2),
                Color.red,
                0f
            );

            A1 = ColliderPoints[i] + (Vector2)ShapeCase(length);
            Debug.DrawLine(
                transform.TransformPoint(-ShapeCase(length)),
                pre.TransformPoint(A1 * 1.3f), // 拡大方向も回転反映
                Color.blue,
                0f
            );
        }
    }


    #endregion


}
