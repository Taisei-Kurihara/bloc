using UnityEngine;

public abstract class ShapeGenerator_abstract
{
    public virtual ShapeStatus shapeStatus { get; protected set; } = new ShapeStatus();

    public virtual float flatDownOffset { get { return shapeStatus.flatDownOffset; } protected set { shapeStatus.flatDownOffset = value; } }
    public virtual int length { get { return shapeStatus.length; } protected set { shapeStatus.length = value; } }
    public virtual float versize { get { return shapeStatus.versize; } protected set { shapeStatus.versize = value; } }
    public virtual Vector2[] vertices { get { return shapeStatus.vertices; } protected set { shapeStatus.vertices = value; } }
    public virtual ushort[] triangles { get { return shapeStatus.triangles; } protected set { shapeStatus.triangles = value; } }
    public virtual Sprite sprite { get { return shapeStatus.sprite; } protected set { shapeStatus.sprite = value; } }
    public virtual Vector2[] ColliderPoints { get { return shapeStatus.ColliderPoints; } protected set { shapeStatus.ColliderPoints = value; } }

    public abstract void ShapeSetNAnglar(int N_Angular, bool flatDown = true);
    public abstract Vector3 ShapeCase(int N_Angular);

    public virtual Vector2[] OfsVertices(float size, TextAnchor CenterDir = TextAnchor.MiddleCenter)
    {
        versize = size;
        Vector2 ofs = AnchorCase(CenterDir) * size;

        return OfsVertices(ofs, size);
    }

    public virtual Vector2[] OfsVertices(TextAnchor CenterDir = TextAnchor.MiddleCenter)
    {
        Vector2 ofs = AnchorCase(CenterDir) * versize;
        return OfsVertices(ofs, versize);
    }

    protected virtual Vector2[] OfsVertices(Vector2 ofs, float size)
    {
        Vector2[] newOfsVertices = new Vector2[this.length];
        for (int i = 0; i < this.length; i++)
        {
            newOfsVertices[i] = (vertices[i] * size) + ofs;
        }

        return newOfsVertices;
    }

    protected virtual Vector2 AnchorCase(TextAnchor TA)
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

        return ofs - Vector2.one * 0.5f; // 中心原点にするためオフセットを調整.
    }

    public virtual void DebugeLine(Transform transform, Transform pre)
    {
        Vector3 A1 = ColliderPoints[0];
        Vector3 B2 = ColliderPoints[0];

        // ポリゴンの辺を描画.
        for (int i = 0; i < length; i++)
        {
            A1 = ColliderPoints[i];
            B2 = ColliderPoints[(i + 1) % length];

            Debug.DrawLine(
                transform.TransformPoint(A1), // ローカルからワールド.
                transform.TransformPoint(B2),
                Color.red,
                0f
            );

            A1 = ColliderPoints[i] + (Vector2)ShapeCase(length);
            Debug.DrawLine(
                transform.TransformPoint(-ShapeCase(length)),
                pre.TransformPoint(A1 * 1.3f), // 拡大してから回転後.
                Color.blue,
                0f
            );
        }
    }
}

public class ShapeStatus
{
    public float flatDownOffset;
    public int length;
    public float versize;
    public Vector2[] vertices;
    public ushort[] triangles;
    public Sprite sprite;
    public Vector2[] ColliderPoints;
}

public class ShapeCompStatus
{
    public ShapeGenerator_abstract generator;
    public ShapeStatus status;

    public ShapeCompStatus(ShapeGenerator_abstract generator)
    {
        this.generator = generator;
        this.status = generator.shapeStatus;
    }
}
