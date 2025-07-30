using System;
using System.Linq;
using Common;
using R3;
using UnityEngine;

public class RotateMove : ModelBase, IMove
{
    public RotateMove(PresenterBase presenter,IShape shape) : base(presenter)
    {
        this.presenter = presenter;
        this.shape = shape;
    }

    IShape shape{ get; set; }

    Rigidbody2D rb;
    private IDisposable Dismove;
    private IDisposable Disgravity;


    //------ステータス---------
    [SerializeField]
    private float Speed = 10;
    [SerializeField]
    private float GravityPower = -9.8f;
    private Vector3 move;
    private int Count = 0;


    public override void Init()
    {
        rb = presenter.GetComponent<Rigidbody2D>();

        InputSystemActionsManager manager = InputSystemActionsManager.Instance();
        InputSystem_Actions action = manager.GetInputSystem_Actions();
        manager.PlayerEnable();
        OnMoveEvent(rb);
        OnJumpEvent();
        CheckGround();
    }

    /// <summary>
    /// Moveできるようにする
    /// </summary>
    private void OnMoveEvent(Rigidbody2D rigid)
    {
        rigid.gravityScale = 0;
        InputSystem_Actions action = InputSystemActionsManager.Instance().GetInputSystem_Actions();

        //移動処理
        Dismove = Observable.EveryUpdate().Subscribe(_ =>
        {
            var a = action.Player.Move.ReadValue<Vector2>();
            int vecX = (int)(new Vector2(a.x, 0).normalized).x; 



            DebugeLine();

            if (vecX != 0)
            {
                int x = (shape.shape.length % 2 == 0) ? shape.shape.length / 2 : (int)((shape.shape.length / 2) + 0.5f);
                x += (int)(((shape.shape.length % 2 == 0) ? (shape.shape.length > 4) ? (1 * (int)((shape.shape.length - 2) / 4)) : 0 : 0) + 0.5f + (vecX * 0.5f));

                int x2 = x + vecX;

                Vector3 A1 = shape.shape.ColliderPoints[x];
                Vector3 B2 = shape.shape.ColliderPoints[x2];

                float angle = Vector3.SignedAngle(A1, B2, Vector3.forward);

                Debug.Log(angle);

                rb.MoveRotation(rb.rotation + (-angle * 0.05f)); //回転処理


                move = new Vector3(a.x * Speed, 0, 0);

                move += new Vector3(0, rb.linearVelocity.y, 0);
                rigid.linearVelocity = move;
            }



        }).AddTo(presenter);

        //重力の設定
        Disgravity = Observable.EveryUpdate(UnityFrameProvider.FixedUpdate)
            .Index()
            .Subscribe(_ =>
            {
                rigid.AddForce(Vector3.up * GravityPower, ForceMode2D.Force);
            }).AddTo(presenter);
    }
    /// <summary>
    /// ジャンプ処理
    /// </summary>
    private void OnJumpEvent()
    {
        InputSystem_Actions action = InputSystemActionsManager.Instance().GetInputSystem_Actions();

        Observable.EveryUpdate()
            .Where(_ => action.Player.Jump.WasPressedThisFrame())//ジャンプを押したとき
            .Where(_ => Count < 2)
            .Subscribe(_ =>
            {
                Count++;//カウント回数を増やす。
                move = Vector3.zero;
                move += Vector3.up * 6.0f;
                rb.linearVelocity = move;
            }).AddTo(presenter);
    }
    /// <summary>
    /// 地面の判定
    /// </summary>
    public void CheckGround()
    {
        //Observable.EveryUpdate()
    }

    public void Dispose()
    {
        Disgravity?.Dispose();
        Dismove?.Dispose();
    }


    private void DebugeLine()
    {
        Vector3 A1 = shape.shape.ColliderPoints[0];
        Vector3 B2 = shape.shape.ColliderPoints[0];


        for (int i = 0; i < shape.shape.length; i++)
        {
            A1 = shape.shape.ColliderPoints[i];
            B2 = shape.shape.ColliderPoints[(i + 1) % shape.shape.length];

            Debug.DrawLine(
                presenter.transform.position + A1,
                presenter.transform.position + B2,
                Color.red,
                0f
            );
        }

        int x = (shape.shape.length % 2 == 0) ? shape.shape.length / 2 : (int)((shape.shape.length / 2) + 0.5f);
        x += (shape.shape.length % 2 == 0) ? (shape.shape.length > 4) ? (1 * (int)((shape.shape.length - 2) / 4)) : 0 : 0;
        A1 = shape.shape.ColliderPoints[x];

        Debug.DrawLine(
                presenter.transform.position + A1,
                presenter.transform.position + (A1 * 1.3f),
                Color.red,
                0f
            );

        // 右側
        x = x + 1;

        A1 = shape.shape.ColliderPoints[x];

        Debug.DrawLine(
                presenter.transform.position + A1,
                presenter.transform.position + (A1 * 1.3f),
                Color.red,
                0f
            );
    }
}
