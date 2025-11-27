using Cysharp.Threading.Tasks;
using UnityEngine;

public class Shape_Model_AttackEntity_Default : Shape_Model_AttackEntity_abstract
{
    public Shape_Model_AttackEntity_Default(CharacterPresenterBase presenter, SpriteRenderer spriteRenderer, EdgeCollider2D edgeCollider, CircleCollider2D circleCollider, ShapeCompStatus _shapeComp) : base(presenter)
    {
        this.presenter = presenter;

        this.spriteRenderer = spriteRenderer;
        this.edgeCollider = edgeCollider;

        this.circleCollider = circleCollider;

        if (this.circleCollider != null)
        {
            this.circleCollider.isTrigger = true;
        }

        this._shapeComp = _shapeComp;
    }

    public override async UniTask SetShapeAsync(int nAngular)
    {
        _shapeComp = await GetOrCreateShapeAsync(nAngular);
        SetAll();
    }

    void SetAll()
    {
        // 表示・当たり判定を更新
        SetSprite();
        SetColl();
    }


    void SetSprite()
    {
        if (shapeGenerator == null) return;

        offsetobj.localPosition = shapeGenerator.ShapeCase(shape.length);

        spriteRenderer.sprite = shape.sprite;
        spriteRenderer.size = new Vector2(100, 100);

    }

    void SetColl()
    {
        if (shape == null) return;

        circleCollider.radius = shape.versize / 100f;

        if (shape.length >= 24)
        {
            edgeCollider.points = new Vector2[2] { Vector2.zero, Vector2.zero };
            circleCollider.isTrigger = false;
        }
        else
        {
            edgeCollider.points = shape.ColliderPoints;
        }
    }
}
