using System;
using Common;
using Cysharp.Threading.Tasks;
using InGame;
using R3;
using UnityEngine;

public class Input_AI_PlayerInput : Input_AI_abstract
{
    public Input_AI_PlayerInput(CharacterPresenterBase presenter) : base(presenter)
    {
        this.presenter = presenter;
        this._playerPresenter = presenter as CharacterPresenter_A_Player;
        this._advent = _playerPresenter?.AttackAdvent;
        this.move = presenter.Move;
    }

    private IDisposable Dismove;
    private CharacterPresenter_A_Player _playerPresenter;
    private AttackEntityAdvent_Player_Default _advent;

    public override void Init()
    {

        InputSystem_Actions action = InputSystemActionsManager.Instance().GetInputSystem_Actions();

        Dismove = Observable.EveryUpdate().Subscribe(_ =>
        {
            // === 移動処理 ===

            // 入力取得.
            Vector2 vec = action.Player.Move.ReadValue<UnityEngine.Vector2>();
            move.SetMoveInput(vec);

            if (action.Player.Attack.WasPressedThisFrame())
            {
                if (_advent != null)
                {
                    _advent.Advent().Forget();
                }
            }

        }).AddTo(presenter);
    }
}
