using System.Drawing;
using Common;
using UnityEngine;

public class CharacterPresenter_B_AttackEntity_default : CharacterPresenter_D_AttackEntity_abstract
{
    public void Initialize(
        ShapeCompStatus _shapeComp,
        Move_AttackEntity_abstract move = null,
        Shape_Model_AttackEntity_abstract shape = null,
        Status_AttackEntity_abstract status = null)
    {
        // Shapeの初期化（引数がnullならデフォルトを生成）.
        if (shape != null)
        {
            _shape = shape;
        }
        else
        {
            _shape = new Shape_Model_AttackEntity_Default(this, Point.GetComponent<SpriteRenderer>(), Point.GetComponent<EdgeCollider2D>(), GetComponent<CircleCollider2D>(), _shapeComp);
        }
        _shape.Init();

        // Moveの初期化（引数がnullならデフォルトを生成）.
        if (move != null)
        {
            _move = move;
        }
        else
        {
            _move = new Move_AttackEntity_Default(this, _shape);
        }
        _move.Init();

        // Statusの初期化（引数がnullならデフォルトを生成）.
        if (status != null)
        {
            _status = status;
        }
        else
        {
            _status = gameObject.AddComponent<Status_AttackEntity_Default>();
            ((Status_AttackEntity_Default)_status).Initialize(new StatusInitializeAttack_Default(this, ContactType.Attack));
        }
        _status.Init();
    }

    public AttackEntity_abstract Attack { get; private set; }

    void Start()
    {
    }

    private void OnDestroy()
    {
    }

    public async void ShapeSet(int shape)
    {

        await Shape.SetShapeAsync(shape);
        Debug.Log($"[Player_Presenter] Shape set to {shape}");
    }
}