using UnityEngine;

public interface IShape : IModelBase
{
    public Shape shape { get; }
    public SpriteRenderer spriteRenderer { get; }

    public EdgeCollider2D edgeCollider { get; }
}
