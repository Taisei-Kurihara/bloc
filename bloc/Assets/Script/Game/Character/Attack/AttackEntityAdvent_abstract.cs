using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class AttackEntityAdvent_abstract : ModelBase
{
    protected AttackEntityAdvent_abstract(CharacterPresenterBase presenter) : base(presenter)
    {
        this.presenter = presenter;
        this.shape = presenter.Shape;
    }

    protected Shape_interface shape { get; set; }

    // 初期生成数（継承クラスで変更可能）.
    public virtual int InitialPoolCount => 100;

    // 最大プール数（継承クラスで変更可能）.
    public virtual int MaxPoolCount => 1000;

    // 発射速度（継承クラスで変更可能）.
    public virtual float FireSpeed => 10f;

    // 攻撃オブジェクトのShape角数（継承クラスで変更可能）.
    public virtual int AttackShapeAngular => 3;

    // 攻撃対象（継承クラスで変更可能）.
    public virtual List<ContactType> HitTargets => new List<ContactType> { ContactType.Enemy };

    // キャッシュ用フィールド.
    protected DynamicObjectController_Game_Default _controller;
    protected bool _isInitialized = false;

    public override void Init()
    {
        _controller = DynamicObjectController_Game_Default.Instance();
        if (_controller == null) return;

        // 攻撃プールを登録.
        Type adventType = this.GetType();

        // presenterのShapeから攻撃用のShapeCompStatusを取得.
        ShapeCompStatus shapeComp = ShapeUnitCirclePolygonManager.Instance()?.GetShapeCompStatus(presenter.Shape.shape.length);

        var settings = new AttackPoolSettings
        {
            InitialCount = InitialPoolCount,
            MaxCount = MaxPoolCount,
            ShapeComp = shapeComp,
            HitTargets = HitTargets,
            Move = null,
            Shape = null,
            Status = null,
            Attack = null,
            AdventFactory = (p) => (AttackEntityAdvent_abstract)Activator.CreateInstance(adventType, new object[] { p })
        };
        _controller.RegisterAttackPool(adventType, settings);

        _isInitialized = true;
    }

    public virtual async UniTask Advent()
    {
        if (!_isInitialized || _controller == null) return;

        // shapeのverticesとversizeを取得.
        if (shape == null || shape.shape == null) return;

        Vector2[] vertices = shape.shape.vertices;
        float versize = shape.shape.versize;

        if (vertices == null || vertices.Length == 0) return;

        // 頂点数分の攻撃オブジェクトをプールから取得.
        Type adventType = this.GetType();
        var attackEntities = _controller.RequestAttackObjects(adventType, vertices.Length);

        // 各攻撃オブジェクトに位置と方向を設定.
        Vector3 presenterPos = presenter.transform.position;
        Quaternion presenterRot = presenter.transform.rotation;

        for (int i = 0; i < attackEntities.Count && i < vertices.Length; i++)
        {
            var entity = attackEntities[i];
            Vector2 vertexDir = vertices[i];

            // presenterの回転を適用した方向.
            Vector2 rotatedDir = presenterRot * (vertexDir / 100);

            // 発射位置: presenter.position + 回転適用済みのオフセット.
            Vector3 firePosition = presenterPos + (Vector3)(rotatedDir * versize);

            // Move_AttackEntity_abstractを使用して位置と発射方向を設定.
            if (entity.MoveAttack != null)
            {
                entity.MoveAttack.SetPosition(firePosition);
                entity.MoveAttack.Fire(rotatedDir, FireSpeed);
            }
        }

        await UniTask.CompletedTask;
    }
}
