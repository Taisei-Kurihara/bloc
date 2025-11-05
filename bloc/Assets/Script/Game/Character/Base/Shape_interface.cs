using UnityEngine;

public interface Shape_interface : IModelBase
{
    public Shape_PolygonGenerator shape { get; }
    public SpriteRenderer spriteRenderer { get; }

    public EdgeCollider2D edgeCollider { get; }

    public CircleCollider2D circleCollider { get; }

    public void SetShape(int shape);
}
