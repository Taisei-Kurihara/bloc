using Common;
using InGame;
using UnityEngine;

public class Move_AttackEntity_Default : Move_AttackEntity_abstract
{
    public Move_AttackEntity_Default(CharacterPresenterBase presenter, Shape_interface shape) : base(presenter, shape)
    {
        this.presenter = presenter;
        this.shape = shape;
    }

    public override void Init()
    {
        base.Init();
        // デフォルトの初期化処理.
    }
}
