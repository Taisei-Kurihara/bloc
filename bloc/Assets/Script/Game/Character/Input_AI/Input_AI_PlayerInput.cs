using UnityEngine;

public class Input_AI_PlayerInput : Input_AI_abstract
{
    public Input_AI_PlayerInput(CharacterPresenterBase presenter) : base(presenter)
    {
        this.presenter = presenter;
        this.move = presenter.Move;
    }


    public override void Init()
    {
    }
}
