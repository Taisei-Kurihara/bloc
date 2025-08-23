using System;
using System.Linq;
using Common;
using R3;
using UnityEngine;
using UnityEngine.Rendering;
using InGame.Character;

public class RotateMove : ModelBase, IMove, ICameraFollowTarget
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
    private float JumpPower = 15.0f;
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

        SetCameraFollowTarget(presenter.transform);
    }

    /// <summary>
    /// Moveできるようにする
    /// </summary>
    private void OnMoveEvent(Rigidbody2D rigid)
    {
        rigid.gravityScale = 0;
        InputSystem_Actions action = InputSystemActionsManager.Instance().GetInputSystem_Actions();
        int beforeX = 0;
        int lastX = 0;

        //移動処理
        Dismove = Observable.EveryUpdate().Subscribe(_ =>
        {
            var vec = action.Player.Move.ReadValue<Vector2>();
            int vecX = (int)(new Vector2(vec.x, 0).normalized).x;

            
            if (vecX != 0)
            {
                move = new Vector3(vec.x * Speed, 0, 0);

                move += new Vector3(0, rb.linearVelocity.y, 0);
                lastX = vecX; //現在のX軸の値を保存
            }
            else if (beforeX != 0)
            {
                move = new Vector3(rb.linearVelocity.x * 0.99f, rb.linearVelocity.y, 0);
            }
            else
            {
                move = new Vector3(rb.linearVelocity.x * 0.99f, rb.linearVelocity.y, 0);
            }

            rigid.linearVelocity = move;


            // === 回転処理 ===
            GroundCheck();

            if (isGrounded.Value && vecX != 0)
            {
                // === 着地しているとき ===
                AlignToGroundNormal(lastX);
            }
            else if(!isGrounded.Value)
            {
                // === 空中にいるとき ===
                RotateWhileAirborne(lastX);
            }

            beforeX = vecX; //前回のX軸の値を保存

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
            .Where(_ => Count < 1)
            .Subscribe(_ =>
            {
                Count++;//カウント回数を増やす。
                move = Vector3.zero;
                move += Vector3.up * JumpPower;
                rb.linearVelocity = move;
            }).AddTo(presenter);
    }
    /// <summary>
    /// 地面の判定
    /// </summary>
    public void CheckGround()
    {
        isGrounded
            .Where(isGrounded => isGrounded) // 地面にいるときのみジャンプ可能
            .Subscribe(_ =>
            {
                Count = 0; // ジャンプしたらカウントをリセット
            }).AddTo(presenter);
    }

    public void Dispose()
    {
        Disgravity?.Dispose();
        Dismove?.Dispose();
    }



    public void SetCameraFollowTarget(Transform target)
    {
        CameraManager.Instance().TrackingTargetTransform = target;
    }


    ReactiveProperty<bool> isGrounded = new ReactiveProperty<bool>(false);
    private Vector2 groundNormal = Vector2.up;
    private float alignSpeed = 5f; // 補間速度


    /// <summary>
    /// 地面判定 + 法線取得
    /// </summary>
    private void GroundCheck()
    {
        float rayLength = 1.1f; // コライダーの大きさに応じて調整
        RaycastHit2D hit = Physics2D.Raycast(
            presenter.transform.position,
            Vector2.down,
            rayLength,
            LayerMask.GetMask("Default")
        );

        // デバッグ用のRayを描画
        Debug.DrawRay(presenter.transform.position, Vector2.down * rayLength, Color.red);

        if (hit.collider != null)
        {
            Debug.Log("Ground hit: " + hit.collider.name);
            isGrounded.Value = true;
            groundNormal = hit.normal; // 地面の法線を取得
        }
        else
        {
            isGrounded.Value = false;
        }
    }

    /// <summary>
    /// 地面と平行になるように回転を補正（補間版）
    /// </summary>
    private void AlignToGroundNormal(int lastX)
    {
        int x = (shape.shape.length % 2 == 0) ? shape.shape.length / 2 : (int)((shape.shape.length / 2) + 0.5f);
        x += (int)(((shape.shape.length % 2 == 0) ? (shape.shape.length > 4) ? (1 * (int)((shape.shape.length - 2) / 4)) : 0 : 0) + 0.5f + (lastX * 0.5f));

        int x2 = x + lastX;

        Vector3 A1 = shape.shape.ColliderPoints[x];
        Vector3 B2 = shape.shape.ColliderPoints[x2];

        float angle = Vector3.SignedAngle(A1, B2, Vector3.forward) * ((rb.linearVelocity.x / Speed) * ((rb.linearVelocity.x > 1) ? 1f : -1f));
        Debug.Log(angle);
        rb.MoveRotation(rb.rotation + (-angle * 0.05f)); //回転処理
    }

    /// <summary>
    /// 空中で回転し続ける処理
    /// </summary>
    private void RotateWhileAirborne(int lastX)
    {
        if (lastX != 0)
        {
            // 空中は lastX の方向に合わせて継続回転（補間付き）
            float targetAngle = rb.rotation + (lastX * -90f); // 1回転で90度回すイメージ
            float newRotation = Mathf.LerpAngle(rb.rotation, targetAngle, Time.fixedDeltaTime * (alignSpeed * 0.5f));
            rb.MoveRotation(newRotation);
        }
    }

    /// <summary>
    /// lastXの方向に近い辺のインデックスを探す
    /// </summary>
    private int FindClosestEdgeIndex(int direction)
    {
        int bestIndex = 0;
        float bestDot = -999f;

        for (int i = 0; i < shape.shape.length; i++)
        {
            Vector2 A = shape.shape.ColliderPoints[i];
            Vector2 B = shape.shape.ColliderPoints[(i + 1) % shape.shape.length];
            Vector2 edgeDir = (B - A).normalized;

            float dot = Vector2.Dot(edgeDir, new Vector2(direction, 0));
            if (dot > bestDot)
            {
                bestDot = dot;
                bestIndex = i;
            }
        }
        return bestIndex;
    }

}


