using System.Collections.Generic;
using Common;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class CharacterPresenter_D_AttackEntity_abstract : CharacterPresenterBase, Presenter_interface
{
    object Presenter_interface.View => new View();


    // 基底クラスのabstractプロパティをoverrideで実装.
    protected Move_AttackEntity_abstract _move;
    protected Shape_Model_AttackEntity_abstract _shape;
    protected Input_AI_Attack_abstract _input;
    protected Hit_AttackEntity_abstract _attack;
    protected Status_AttackEntity_abstract _status;
    
    public override Move_interface Move { get => _move; protected set => _move = (Move_AttackEntity_abstract)value; }
    public override Shape_interface Shape { get => _shape; protected set => _shape = (Shape_Model_AttackEntity_abstract)value; }
    public override IStatus_base Status { get => _status; protected set => _status = (Status_AttackEntity_abstract)value; }
    public override Input_AI_abstract Input { get => _input; protected set => _input = (Input_AI_Attack_abstract)value; }
    public override CharacterPresenter_D_AttackEntity_abstract Attack { get => null; protected set { } }
    public Hit_AttackEntity_abstract AttackEntity { get => _attack; protected set => _attack = (Hit_AttackEntity_abstract)value; }

    // 派生型でアクセスしやすくするプロパティ.
    public Move_AttackEntity_abstract MoveAttack => _move;
    public Shape_Model_AttackEntity_abstract ShapeAttack => _shape;
    public Status_AttackEntity_abstract StatusAttack => _status;

    public virtual void Init()
    {
    }

    public virtual void Initialize(
        ShapeCompStatus shapeComp,
        List<ContactType> HitTargets,
        Move_AttackEntity_abstract move = null,
        Shape_Model_AttackEntity_abstract shape = null,
        Status_AttackEntity_abstract status = null,
        Hit_AttackEntity_abstract attack = null)
    {
        // Shapeの初期化（引数がnullならデフォルトを生成）.
        if (shape != null)
        {
            _shape = shape;
        }
        else
        {
            _shape = new Shape_Model_AttackEntity_Default(this, Point.GetComponent<SpriteRenderer>(), Point.GetComponent<EdgeCollider2D>(), GetComponent<CircleCollider2D>(), shapeComp);
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

        // Statusの初期化（引数があれば使用、なければnewで生成）.
        if (status != null)
        {
            _status = status;
        }
        else
        {
            _status = new Status_AttackEntity_Default();
        }
        _status.Initialize(new StatusInitializeAttack_Default(this, ContactType.Attack));
        _status.Init();

        if (attack != null)
        {
            _attack = attack;
        }
        else
        {
            _attack = new Hit_AttackEntity_Default(this);
        }

        _attack.HitTargets = HitTargets;
        _attack.Init();

    }
}