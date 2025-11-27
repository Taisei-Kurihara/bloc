using Common;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class CharacterPresenter_D_AttackEntity_abstract : CharacterPresenterBase, Presenter_interface
{
    object Presenter_interface.View => new View();


    // 基底クラスのabstractプロパティをoverrideで実装.
    protected Move_AttackEntity_abstract _move;
    protected Shape_Model_AttackEntity_abstract _shape;
    protected Status_AttackEntity_abstract _status;

    public override Move_interface Move { get => _move; protected set => _move = (Move_AttackEntity_abstract)value; }
    public override Shape_interface Shape { get => _shape; protected set => _shape = (Shape_Model_AttackEntity_abstract)value; }
    public override IStatus_base Status { get => _status; protected set => _status = (Status_AttackEntity_abstract)value; }

    // 派生型でアクセスしやすくするプロパティ.
    public Move_AttackEntity_abstract MoveAttack => _move;
    public Shape_Model_AttackEntity_abstract ShapeAttack => _shape;
    public Status_AttackEntity_abstract StatusAttack => _status;

    [SerializeField]
    protected GameObject Point;

    void Init()
    {
    }
}