using Common;
using InGame;
using UnityEngine;
using Cysharp.Threading.Tasks;

public abstract class Shape_Model_abstract : ModelBase, Shape_interface
{
    protected Shape_Model_abstract(CharacterPresenterBase presenter) : base(presenter)
    {
        this.presenter = presenter;
    }

    // 上:Shape_Model_abstractで生成処理を呼びだす関数はUniTask型で待機処理させるようにしてください

    protected ShapeCompStatus _shapeComp;
    public ShapeStatus shape => _shapeComp?.status;
    public ShapeGenerator_abstract shapeGenerator => _shapeComp?.generator;

    public virtual SpriteRenderer spriteRenderer { get; protected set; }
    protected Transform offsetobj => spriteRenderer.transform;
    public virtual EdgeCollider2D edgeCollider { get; protected set; }
    public virtual CircleCollider2D circleCollider { get; protected set; }

    /// <summary>
    /// ShapeStatusを非同期で取得または生成する処理.
    /// </summary>
    /// <param name="nAngular">角数.</param>
    /// <returns>ShapeCompStatus.</returns>
    protected async UniTask<ShapeCompStatus> GetOrCreateShapeAsync(int nAngular)
    {
        ShapeStatus status = await ShapeUnitCirclePolygonManager.Instance().GetOrCreateShapeStatusAsync(null, nAngular);
        // ShapeCompStatusを取得.
        return ShapeUnitCirclePolygonManager.Instance().GetShapeCompStatus(nAngular);
    }

    public abstract UniTask SetShapeAsync(int nAngular);

    public override void Init()
    {
        // 初期化処理.
    }
}
