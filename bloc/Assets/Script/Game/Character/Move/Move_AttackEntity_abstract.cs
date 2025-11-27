using Common;
using InGame;
using UnityEngine;

public abstract class Move_AttackEntity_abstract : ModelBase, Move_interface
{
    protected Move_AttackEntity_abstract(CharacterPresenterBase presenter, Shape_interface shape) : base(presenter)
    {
        this.presenter = presenter;
        this.shape = shape;
    }

    protected Shape_interface shape { get; set; }

    protected Rigidbody2D rb;

    protected UnityEngine.Vector2 input { get; set; } = UnityEngine.Vector2.zero;
    UnityEngine.Vector2 Move_interface.input { get => input; set => input = value; }



    public override void Init()
    {
        // 初期化処理.
    }

    public void SetMoveInput(Vector2 input)
    {
        throw new System.NotImplementedException();
    }
}
