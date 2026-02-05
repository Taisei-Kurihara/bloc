using Common;
using InGame;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public abstract class Shape_Model_abstract : ModelBase, Shape_interface
{
    protected Shape_Model_abstract(CharacterPresenterBase presenter) : base(presenter)
    {
        this.presenter = presenter;
    }

    protected ShapeCompStatus _shapeComp;
    public ShapeStatus shape => _shapeComp?.status;
    public ShapeGenerator_abstract shapeGenerator => _shapeComp?.generator;

    /// <summary>
    /// このモデルが使用しているShape情報の記録 (ShapeUnitCirclePolygonManagerで管理).
    /// </summary>
    public ShapeRecordInfo ShapeRecord => ShapeUnitCirclePolygonManager.Instance().GetModelShapeRecord(this);

    public virtual SpriteRenderer spriteRenderer { get; protected set; }
    protected Transform offsetobj => spriteRenderer.transform;
    public virtual EdgeCollider2D edgeCollider { get; protected set; }
    public virtual CircleCollider2D circleCollider { get; protected set; }

    /// <summary>
    /// ShapeStatusを非同期で取得または生成する処理.
    /// ShapeUnitCirclePolygonManagerで記録管理される.
    /// </summary>
    /// <param name="nAngular">角数.</param>
    /// <param name="generatorType">ShapeGenerator_abstractを継承したクラスの型 (default = ShapeGenerator_Polygon).</param>
    /// <param name="persistsAcrossScenes">sceneをまたいで保持される可能性があるか.</param>
    /// <returns>ShapeCompStatus.</returns>
    protected async UniTask<ShapeCompStatus> GetOrCreateShapeAsync(int nAngular, Type generatorType = null, bool persistsAcrossScenes = false)
    {
        // ShapeUnitCirclePolygonManagerで記録管理しつつ取得.
        return await ShapeUnitCirclePolygonManager.Instance().GetOrCreateShapeForModelAsync(this, nAngular, generatorType, persistsAcrossScenes);
    }

    public abstract UniTask SetShapeAsync(int nAngular);

    public override void Init()
    {
        // 初期化処理.
    }
}
