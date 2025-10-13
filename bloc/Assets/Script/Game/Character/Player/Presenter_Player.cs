using UnityEngine;
using Common;
using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.ProbeAdjustmentVolume;
namespace InGame
{
    public class Presenter_Player : PresenterBase, Presenter_interface
    {
        object Presenter_interface.View => new View();

        Move_interface Move { get; set; }

        new Shape_interface Shape { get; set; }

        InputSystem_Actions inputActions;

        [SerializeField]
        GameObject Point;

        CircleCollider2D circleCollider;

        void Start()
        {
            inputActions = InputSystemActionsManager.Instance().GetInputSystem_Actions();
            circleCollider = GetComponent<CircleCollider2D>();
            Shape = new ShapeModel(this, Point.GetComponent<SpriteRenderer>(), Point.GetComponent<EdgeCollider2D>(),3);
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

        
    }
}