using System;
using Common;
using R3;
using UnityEngine;

public class Input_AI_PlayerInput : Input_AI_abstract
{
    public Input_AI_PlayerInput(CharacterPresenterBase presenter) : base(presenter)
    {
        this.presenter = presenter;
        this.move = presenter.Move;
    }

    private IDisposable Dismove;

    public override void Init()
    {

        InputSystem_Actions action = InputSystemActionsManager.Instance().GetInputSystem_Actions();

        Dismove = Observable.EveryUpdate().Subscribe(_ =>
        {
            // === ˆÚ“®ˆ— ===

            // “ü—Íæ“¾
            Vector2 vec = action.Player.Move.ReadValue<UnityEngine.Vector2>();
            move.SetMoveInput(vec);

        }).AddTo(presenter);
    }
}
