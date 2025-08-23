using UnityEngine;
using Common;
using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.ProbeAdjustmentVolume;
namespace InGame
{
    public class PlayerPresenter : PresenterBase, IPresenter
    {
        object IPresenter.View => new View();

        IMove Move { get; set; }

        new IShape Shape { get; set; }

        InputSystem_Actions inputActions;

        [SerializeField]
        GameObject Point;

        void Start()
        {
            inputActions = InputSystemActionsManager.Instance().GetInputSystem_Actions();
            Shape = new ShapeModel(this, Point.GetComponent<SpriteRenderer>(), Point.GetComponent<EdgeCollider2D>(),3);
            Shape.Init();

            Move = new RotateMove(this, Shape);
            Move.Init();
            Observable.EveryUpdate().Subscribe(_ => { UpdateController(); }).AddTo(this);

        }

        /// <summary>
        ///Å@ÉLÅ[ëÄçÏä÷åWÇÃä÷êî
        /// </summary>
        public void UpdateController()
        {
        }

        private void OnDestroy()
        {
        }

        
    }
}