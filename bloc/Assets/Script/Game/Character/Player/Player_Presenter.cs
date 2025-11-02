using UnityEngine;
using Common;
using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.ProbeAdjustmentVolume;
namespace InGame
{
    public class Player_Presenter : PresenterBase, Presenter_interface
    {
        object Presenter_interface.View => new View();

        public Move_interface Move { get; private set; }
        public Shape_interface Shape { get; private set; }

        InputSystem_Actions inputActions;

        [SerializeField]
        GameObject Point;

        CircleCollider2D circleCollider;

        void Start()
        {
            inputActions = InputSystemActionsManager.Instance().GetInputSystem_Actions();
            //circleCollider = GetComponent<CircleCollider2D>();
            Shape = new Shape_Model(this, Point.GetComponent<SpriteRenderer>(), Point.GetComponent<EdgeCollider2D>(),3);
            Shape.Init();

            Move = new Move_Rotate_OnGrounded(this, Shape, circleCollider);
            Move.Init();
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

        public void ShapeSet(int shape)
        {

            Shape.SetShape(shape);
            Debug.Log($"[Player_Presenter] Shape set to {shape}");
        }
    }
}