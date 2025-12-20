using UnityEngine;
using Common;
using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.ProbeAdjustmentVolume;
namespace InGame
{
    public class CharacterPresenter_A_Player : CharacterPresenterBase, Presenter_interface
    {
        object Presenter_interface.View => new View();

        private Move_interface _move;
        private Shape_interface _shape;
        private Input_AI_PlayerInput _status_base;
        private Status_BattleCharacter_default _status;

        public override Move_interface Move { get => _move; protected set => _move = value; }
        public override Shape_interface Shape { get => _shape; protected set => _shape = value; }
        public override IStatus_base Status { get => _status; protected set => _status = (Status_BattleCharacter_default)value; }
        public override Input_AI_abstract Input { get => _status_base; protected set => _status_base = (Input_AI_PlayerInput)value; }
        public override CharacterPresenter_D_AttackEntity_abstract Attack { get => null; protected set { } }

        private AttackEntityAdvent_Player_Default _attackAdvent;
        public AttackEntityAdvent_Player_Default AttackAdvent => _attackAdvent;

        InputSystem_Actions inputActions;


        CircleCollider2D circleCollider;

        async void Start()
        {
            inputActions = InputSystemActionsManager.Instance().GetInputSystem_Actions();
            Shape = new Shape_Model_Player(this, Point.GetComponent<SpriteRenderer>(), Point.GetComponent<EdgeCollider2D>(), GetComponent<CircleCollider2D>(), 3);
            Shape.Init();

            // Shape の非同期初期化を待機.
            await ((Shape_Model_Player)Shape).InitAsync();

            Move = new Move_Rotate_OnGrounded_PlayerInput(this);
            Move.Init();

            _status = new Status_BattleCharacter_default();
            _status.Initialize(new StatusInitialize_Default(this, ContactType.Player));
            _status.Init();

            Observable.EveryUpdate().Subscribe(_ => { UpdateController(); }).AddTo(this);


            _attackAdvent = new AttackEntityAdvent_Player_Default(this);
            _attackAdvent.Init();


            Input = new Input_AI_PlayerInput(this);
            Input.Init();

        }

        /// <summary>
        ///　キー操作関係の関数
        /// </summary>
        public void UpdateController()
        {
        }

        private void OnDestroy()
        {
        }

        public async void ShapeSet(int shape)
        {

            await Shape.SetShapeAsync(shape);
            Debug.Log($"[Player_Presenter] Shape set to {shape}");
            _attackAdvent.Init();
        }
    }
}
