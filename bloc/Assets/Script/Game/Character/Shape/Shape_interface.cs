using UnityEngine;
using Cysharp.Threading.Tasks;

public interface Shape_interface : IModelBase
{
    public ShapeStatus shape { get; }
    public SpriteRenderer spriteRenderer { get; }

    public EdgeCollider2D edgeCollider { get; }

    public CircleCollider2D circleCollider { get; }

    public UniTask SetShapeAsync(int nAngular);
}
