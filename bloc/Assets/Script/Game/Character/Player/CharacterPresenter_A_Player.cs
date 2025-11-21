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

        public Move_interface Move { get; private set; }
        public Shape_interface Shape { get; private set; }
        public Status_abstract<StatusInitialize_abstract> Status { get; private set; }

        InputSystem_Actions inputActions;

        [SerializeField]
        GameObject Point;

        CircleCollider2D circleCollider;

        async void Start()
        {
            inputActions = InputSystemActionsManager.Instance().GetInputSystem_Actions();
            Shape = new Shape_Model_Player(this, Point.GetComponent<SpriteRenderer>(), Point.GetComponent<EdgeCollider2D>(), GetComponent<CircleCollider2D>(), 3);
            Shape.Init();

            // Shape の非同期初期化を待機.
            await ((Shape_Model_Player)Shape).InitAsync();

            Move = new Move_Rotate_OnGrounded_PlayerInput(this, Shape);
            Move.Init();

            Status = gameObject.AddComponent<Status_BattleCharacter_default>();
            ((Status_BattleCharacter_default)Status).Initialize(new StatusInitialize_Default(this, ContactType.Player));
            Status.Init();

            Observable.EveryUpdate().Subscribe(_ => { UpdateController(); }).AddTo(this);

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