using UnityEngine;

public interface Shape_interface : IModelBase
{
    public Shape shape { get; }
    public SpriteRenderer spriteRenderer { get; }

    public EdgeCollider2D edgeCollider { get; }
}
