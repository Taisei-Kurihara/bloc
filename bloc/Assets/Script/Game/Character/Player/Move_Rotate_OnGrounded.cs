using System;
using System.Linq;
using Common;
using R3;
using UnityEngine;
using InGame.Character;
using R3.Triggers;

public class Move_Rotate_OnGrounded : ModelBase, Move_interface, Camera_FollowTarget_interface
{
    public Move_Rotate_OnGrounded(PresenterBase presenter,Shape_interface shape) : base(presenter)
    {
        this.presenter = presenter;
        this.shape = shape;
    }

    Shape_interface shape{ get; set; }

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
    [SerializeField]
    int maxJumpCount = 1; // 最大ジャンプ回数
    ReactiveProperty<int> Count = new ReactiveProperty<int>(0);

    PhysicsMaterial2D pm2d = new PhysicsMaterial2D("DynamicMaterial") { friction = 0, bounciness = 0 };
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

    #region アクション

    #region 移動・回転処理
    /// <summary>
    /// Moveできるようにする
    /// </summary>
    private void OnMoveEvent(Rigidbody2D rigid)
    {
        rigid.gravityScale = 0;
        InputSystem_Actions action = InputSystemActionsManager.Instance().GetInputSystem_Actions();
        int lastX = 0;

        //移動処理
        Dismove = Observable.EveryUpdate().Subscribe(_ =>
        {
            // === 移動処理 ===
            var vec = action.Player.Move.ReadValue<Vector2>();
            var rotatevec = action.Player.Rotate.ReadValue<float>();
            int vecX = (int)(new Vector2(vec.x, 0).normalized).x;

            lastX = (vecX != 0) ? vecX : (RotateMoveIsGrounded.Value) ? 0 : lastX;

            ShouldCheckNormal.Value = GroundCheckRay(2);

            // 鋭角コーナーでの法線追従を防止
            bool isSharpCorner = false;
            float slopeAngle = 0f;

            if (ShouldCheckNormal.Value)
            {
                // groundNormals[0]=左, [1]=中央, [2]=右 を使用して角度を確認
                Vector2 left = groundNormals[0];
                Vector2 center = groundNormals[1];
                Vector2 right = groundNormals[2];

                float angleL = Vector2.Angle(center, left);
                float angleR = Vector2.Angle(center, right);

                if (angleL < 120f || angleR < 120f)
                {
                    isSharpCorner = true;
                }

                // 斜面の角度を取得（中央法線基準）
                slopeAngle = Vector2.Angle(Vector2.up, center);
            }
            
            
            // ===== 移動処理 =====
            if (vecX != 0)
            {
                pm2d.friction = 0f;

                if (slopeAngle > 40f)
                {
                    //Debug.Log($"Slope Angle: {slopeAngle}, Is Sharp Corner: {isSharpCorner}, slow");
                    move = new Vector3(vecX * (Speed / 10), rb.linearVelocity.y, 0);
                }
                if (ShouldCheckNormal.Value && !isSharpCorner) // 法線追従するか判定
                {
                    //Debug.Log($"Slope Angle: {slopeAngle}, Is Sharp Corner: {isSharpCorner}, normal");
                    // 法線追従
                    Vector2 normal = groundNormals[1];
                    Vector3 desiredMove = new Vector3(vecX, 0, 0) * Speed;
                    Vector3 projectedMove = Vector3.ProjectOnPlane(desiredMove, normal);
                    move = new Vector3(projectedMove.x, rb.linearVelocity.y, 0);
                }
                else
                {
                    //Debug.Log($"Slope Angle: {slopeAngle}, Is Sharp Corner: {isSharpCorner}, Aer");
                    // 通常移動（斜面45°超 or 鋭角コーナー or 空中）
                    move = new Vector3(vecX * Speed, rb.linearVelocity.y, 0);
                }
            }

            else if (RotateMoveIsGrounded.Value)
            {
                pm2d.friction = 1;
                move = new Vector3(rb.linearVelocityX * 0.99f, rb.linearVelocity.y, 0);
            }
            else if (!RotateMoveIsGrounded.Value)
            {
                move = new Vector3(rb.linearVelocityX, rb.linearVelocity.y, 0);
            }
            else
            {
                move = new Vector3(rb.linearVelocityX * 0.99f, rb.linearVelocity.y, 0);
            }

            shape.edgeCollider.sharedMaterial = pm2d;
            rigid.linearVelocity = move;


            // === 回転処理 ===
            // 回転移動開始時/終了時に挙動を安定させるために
            if (RotateMoveIsGrounded.Value && vecX != 0) { RotateMoveIsGrounded.Value = GroundCheckRay(shape.edgeCollider.points[0].magnitude); }
            

            if (RotateMoveIsGrounded.Value && vecX != 0)
            {
                // === 着地しているとき ===
                AlignToGroundNormal(lastX);
            }
            else if(!RotateMoveIsGrounded.Value)
            {
                // === 空中にいるとき ===
                RotateWhileAirborne(lastX);
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

    #endregion

    #region 回転補正

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
        //Debug.Log(angle);
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

    #endregion

    #region ジャンプ処理

    /// <summary>
    /// ジャンプ処理
    /// </summary>
    private void OnJumpEvent()
    {
        InputSystem_Actions action = InputSystemActionsManager.Instance().GetInputSystem_Actions();

        Observable.EveryUpdate()
            .Where(_ => action.Player.Jump.WasPressedThisFrame())//ジャンプを押したとき
            .Where(_ => Count.Value < maxJumpCount) //ジャンプ回数が最大値以下のとき
            .Subscribe(_ =>
            {
                Count.Value++;//カウント回数を増やす。
                move = Vector3.right * rb.linearVelocity;
                move += Vector3.up * JumpPower;
                rb.linearVelocity = move;
            }).AddTo(presenter);
    }

    #endregion

    #endregion

    #region

    ReactiveProperty<bool> RotateMoveIsGrounded = new ReactiveProperty<bool>(false); // 回転移動用
    ReactiveProperty<bool> ShouldCheckNormal = new ReactiveProperty<bool>(false); //法線確認するか
    private Vector2 groundNormal = Vector2.up;
    private float alignSpeed = 5f; // 補間速度
    /// <summary>
    /// 地面の判定
    /// </summary>
    public void CheckGround()
    {
        shape.edgeCollider
            .OnCollisionStay2DAsObservable()
            .Where(collider => collider.gameObject.layer == LayerMask.NameToLayer("Default")) // 地面のレイヤーを指定
            .Subscribe(collider =>
            {
                RotateMoveIsGrounded.Value = true;
            }).AddTo(presenter);

        Observable.EveryUpdate()
            .Where(_ => Count.Value!=0&&rb.linearVelocityY<=0)
            .Subscribe(_ =>
            {
                if (GroundCheckRay(1.1f)) { Count.Value = 0; }
            }).AddTo(presenter);

        // 落ちた時(debug)
        Observable.EveryUpdate()
            .Where(_ => presenter.transform.position.y<= -30)
            .Subscribe(_ =>
            {
                presenter.transform.position = new Vector3(0, 0, 0);
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = 0;
                Count.Value = 0;
            }).AddTo(presenter);
    }

    /// <summary>
    /// 地面判定 + 法線取得
    /// </summary>
    private Vector2[] groundNormals = new Vector2[4]; // 左・中央・右の3本分

    private bool GroundCheckRay(float length = 1.1f)
    {
        bool isHit = false;

        Vector3[] offsets = new Vector3[]
        {
        Vector3.left * 0.4f,
        Vector3.zero,
        Vector3.right * 0.4f
        };

        for (int i = 0; i < offsets.Length; i++)
        {
            Vector3 origin = presenter.transform.position + offsets[i];
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, length, LayerMask.GetMask("Default"));
            Debug.DrawRay(origin, Vector2.down * length, Color.red);

            if (hit.collider != null)
            {
                groundNormals[i] = hit.normal;
                isHit = true;
            }
            else
            {
                groundNormals[i] = Vector2.up;
            }
        }

        return isHit;
    }

    #endregion

    public void Dispose()
    {
        Disgravity?.Dispose();
        Dismove?.Dispose();
    }

    public void SetCameraFollowTarget(Transform target)
    {
        CameraManager.Instance().TrackingTargetTransform = target;
    }
}