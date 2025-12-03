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
        private IStatus_base _status;
        private Input_AI_PlayerInput _status_base;

        public override Move_interface Move { get => _move; protected set => _move = value; }
        public override Shape_interface Shape { get => _shape; protected set => _shape = value; }
        public override IStatus_base Status { get => _status; protected set => _status = value; }
        public override Input_AI_abstract Input { get => _status_base; protected set => _status_base = (Input_AI_PlayerInput)value; }
        public override CharacterPresenter_D_AttackEntity_abstract Attack { get => null; protected set { } }

        InputSystem_Actions inputActions;

        //[SerializeField]
        //GameObject Point;

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

            Status = gameObject.AddComponent<Status_BattleCharacter_default>();
            ((Status_BattleCharacter_default)Status).Initialize(new StatusInitialize_Default(this, ContactType.Player));
            Status.Init();

            Observable.EveryUpdate().Subscribe(_ => { UpdateController(); }).AddTo(this);

            Input = new Input_AI_PlayerInput(this);
            Input.Init();

            Attack = new CharacterPresenter_B_AttackEntity_default();
            // ここでAttack.Initialize();
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
        }
    }
}