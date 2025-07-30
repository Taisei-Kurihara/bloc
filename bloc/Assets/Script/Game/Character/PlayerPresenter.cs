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
        object IPresenter.View => new ShapeModel(this);

        IMove Move { get; set; }

        new IShape Shape { get; set; }

        InputSystem_Actions inputActions;

        void Start()
        {
            inputActions = InputSystemActionsManager.Instance().GetInputSystem_Actions();
            Shape = new ShapeModel(this,3);
            Shape.Init();

            Move = new RotateMove(this, Shape);
            Move.Init();
            Observable.EveryUpdate().Subscribe(_ => { UpdateController(); }).AddTo(this);

        }

        /// <summary>
        ///@ƒL[‘€ìŠÖŒW‚ÌŠÖ”
        /// </summary>
        public void UpdateController()
        {
        }

        private void OnDestroy()
        {
        }

        
    }
}