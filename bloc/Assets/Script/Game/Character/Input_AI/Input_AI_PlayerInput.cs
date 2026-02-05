using System;
using Common;
using Cysharp.Threading.Tasks;
using InGame;
using R3;
using Unity.VisualScripting;
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

    private CharacterPresenter_A_Player _playerPresenter;
    private AttackEntityAdvent_Player_Default _advent;
    InputSystem_Actions action;
    public override void Init()
    {
        action = InputSystemActionsManager.Instance().GetInputSystem_Actions();
        StartMoveInput();
        
    }

    protected override void ProcessMoveInput()
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
    }
}
