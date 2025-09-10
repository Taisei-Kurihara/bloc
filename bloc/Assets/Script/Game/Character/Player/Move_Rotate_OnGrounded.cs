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

    #region Fields and Properties
    
    // コアコンポーネント
    private Shape_interface shape { get; set; }
    private Rigidbody2D rb;
    private IDisposable Dismove;
    private IDisposable Disgravity;

    // 移動パラメータ
    [SerializeField] private float Speed = 10f;
    [SerializeField] private float JumpPower = 15.0f;
    [SerializeField] private float GravityPower = -9.8f;
    [SerializeField] private int maxJumpCount = 1;
    
    // 斜面と地面の定数
    private const float MAX_SLOPE_ANGLE = 45f;
    private const float GROUND_DISTANCE = 1f;
    private const float CIRCLE_RADIUS = 0.5f;
    
    // 実行時変数
    private Vector3 move;
    private ReactiveProperty<int> Count = new ReactiveProperty<int>(0);
    private ReactiveProperty<bool> RotateMoveIsGrounded = new ReactiveProperty<bool>(false);
    private ReactiveProperty<bool> ShouldCheckNormal = new ReactiveProperty<bool>(false);
    private Vector2[] groundNormals = new Vector2[4];
    private float alignSpeed = 5f;
    
    // 物理マテリアル
    private PhysicsMaterial2D pm2d = new PhysicsMaterial2D("DynamicMaterial") { friction = 0, bounciness = 0 };

    #endregion

    #region Initialization

    public override void Init()
    {
        rb = presenter.GetComponent<Rigidbody2D>();

        InputSystemActionsManager manager = InputSystemActionsManager.Instance();
        InputSystem_Actions action = manager.GetInputSystem_Actions();
        manager.PlayerEnable();
        
        InitializeMovement(rb);
        InitializeJump();
        InitializeGroundDetection();
        SetCameraFollowTarget(presenter.transform);
    }

    #endregion

    #region Movement System

    /// <summary>
    /// 移動と回転システムを初期化
    /// </summary>
    private void InitializeMovement(Rigidbody2D rigid)
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
            bool isJumpRamp = false; // ジャンプ台判定
            float slopeAngle = 0f;
            bool canClimb = true; // 斜面を登れるかどうか

            if (ShouldCheckNormal.Value)
            {
                // groundNormals[0]=左, [1]=中央, [2]=右 を使用して角度を確認
                Vector2 left = groundNormals[0];
                Vector2 center = groundNormals[1];
                Vector2 right = groundNormals[2];

                float angleL = Vector2.Angle(center, left);
                float angleR = Vector2.Angle(center, right);

                // 鋭角コーナーの判定
                if (angleL < 120f || angleR < 120f)
                {
                    isSharpCorner = true;
                    
                    // ジャンプ台の判定（鋭角で十分な横方向の勢いがある場合のみ）
                    // 移動開始時の誤判定を防ぐため、X方向の速度を確認
                    if (Mathf.Abs(rb.linearVelocity.x) > Speed * 0.9f && slopeAngle > 15f)
                    {
                        isJumpRamp = true;
                    }
                }

                // 斜面の角度を取得（中央法線基準）
                slopeAngle = Vector2.Angle(Vector2.up, center);
                
                // 45度以上の斜面は登れない
                if (slopeAngle >= MAX_SLOPE_ANGLE)
                {
                    canClimb = false;
                }
            }
            
            
            // 入力と地面条件に基づく移動処理
            if (vecX != 0)
            {
                pm2d.friction = 0f;
                move = ProcessMovementInput(vecX, isJumpRamp, isSharpCorner, canClimb, slopeAngle);
            }

            else if (RotateMoveIsGrounded.Value)
            {
                // 45度以上の斜面で入力がない場合は下り方向に滑らせる
                if (ShouldCheckNormal.Value && slopeAngle >= MAX_SLOPE_ANGLE)
                {
                    // 斜面を下る方向を計算
                    Vector2 normal = groundNormals[1];
                    Vector2 slopeDirection = Vector2.Perpendicular(normal);
                    
                    // 下り方向にする（Y成分が負になるように）
                    if (slopeDirection.y > 0) slopeDirection = -slopeDirection;
                    
                    // 滑り落ちる速度
                    float slideSpeed = Speed * 0.3f;
                    
                    // 地面との距離維持（下り時補正を適用）
                    RaycastHit2D groundHit = Physics2D.Raycast(presenter.transform.position, Vector2.down, 1.5f, LayerMask.GetMask("Default"));
                    float currentVelocityY = rb.linearVelocity.y;
                    
                    if (groundHit.collider != null && groundHit.distance < GROUND_DISTANCE - 0.2f)
                    {
                        float pushForce = (GROUND_DISTANCE - groundHit.distance) * 0.5f;
                        currentVelocityY = Mathf.Max(currentVelocityY, pushForce);
                    }
                    
                    // 下り補正を強化
                    float downwardCorrection = slopeDirection.y * slideSpeed * 1.2f;
                    
                    pm2d.friction = 0f;
                    move = new Vector3(slopeDirection.x * slideSpeed, downwardCorrection + currentVelocityY * 0.3f, 0);
                }
                else
                {
                    // 通常の摩擦による減速
                    pm2d.friction = 1;
                    move = new Vector3(rb.linearVelocityX * 0.99f, rb.linearVelocity.y, 0);
                }
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
            
            // 空回り中の特別な回転処理（より厳密な条件）
            bool isSpinning = !canClimb && IsMovingUpSlope(vecX, groundNormals[1]) && vecX != 0 && slopeAngle >= MAX_SLOPE_ANGLE;
            
            // 自動滑落中の回転処理（入力なし時の45度以上斜面）
            bool isAutoSliding = vecX == 0 && RotateMoveIsGrounded.Value && ShouldCheckNormal.Value && slopeAngle >= MAX_SLOPE_ANGLE;

            if (isSpinning)
            {
                // 空回り時は非常に速く回転（タイヤが空転しているような効果）
                float spinSpeed = vecX * -300f * Time.fixedDeltaTime;
                rb.MoveRotation(rb.rotation + spinSpeed);
            }
            else if (isAutoSliding)
            {
                // 自動滑落時の回転（下り時の入力と同じように）
                Vector2 normal = groundNormals[1];
                Vector2 slopeDirection = Vector2.Perpendicular(normal);
                if (slopeDirection.y > 0) slopeDirection = -slopeDirection;
                
                // 滑り方向に基づいた回転
                float slideDirection = slopeDirection.x > 0 ? 1 : -1;
                AlignToGroundNormal((int)slideDirection);
            }
            else if (RotateMoveIsGrounded.Value && vecX != 0)
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

    /// <summary>
    /// 移動入力を処理し、適切な移動ベクトルを返す
    /// </summary>
    private Vector3 ProcessMovementInput(int vecX, bool isJumpRamp, bool isSharpCorner, bool canClimb, float slopeAngle)
    {
        // ジャンプ台の発射
        if (isJumpRamp && rb.linearVelocity.y > -1f)
        {
            return HandleJumpRampLaunch();
        }
        
        // 下り移動（最優先）
        if (IsMovingDownSlope(vecX, groundNormals[1]) && ShouldCheckNormal.Value && !isSharpCorner)
        {
            return HandleDownhillMovement(vecX);
        }
        
        // 急な上り坂での空回り（45度以上）
        if (!canClimb && IsMovingUpSlope(vecX, groundNormals[1]) && slopeAngle >= MAX_SLOPE_ANGLE)
        {
            return HandleSteepUphillSpinning();
        }
        
        // 中程度の斜面移動（40-45度）
        if (slopeAngle > 40f && slopeAngle < MAX_SLOPE_ANGLE && !IsMovingDownSlope(vecX, groundNormals[1]))
        {
            return HandleMediumSlopeMovement(vecX);
        }
        
        // 通常の地面移動
        if (ShouldCheckNormal.Value && !isSharpCorner)
        {
            return HandleNormalGroundMovement(vecX);
        }
        
        // 空中移動またはフォールバック
        return new Vector3(vecX * Speed, rb.linearVelocity.y, 0);
    }

    /// <summary>
    /// ジャンプ台の発射動作を処理
    /// </summary>
    private Vector3 HandleJumpRampLaunch()
    {
        Vector2 normal = groundNormals[1];
        float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);
        float verticalBoost = Mathf.Max(0, normal.y * horizontalSpeed * 0.2f);
        
        RotateMoveIsGrounded.Value = false;
        return new Vector3(rb.linearVelocity.x, rb.linearVelocity.y + verticalBoost, 0);
    }

    /// <summary>
    /// 強化された地面追従で下り移動を処理
    /// </summary>
    private Vector3 HandleDownhillMovement(int vecX)
    {
        Vector2 normal = groundNormals[1];
        float currentVelocityY = ApplyCircleColliderBehavior(0.2f, 0.3f);
        
        Vector3 desiredMove = new Vector3(vecX, 0, 0) * Speed;
        Vector3 projectedMove = Vector3.ProjectOnPlane(desiredMove, normal);
        
        float downwardCorrection = projectedMove.y * 1.2f;
        return new Vector3(projectedMove.x, downwardCorrection + currentVelocityY * 0.3f, 0);
    }

    /// <summary>
    /// 急な上り坂での空回りを処理（入力キャンセルだが物理は維持）
    /// </summary>
    private Vector3 HandleSteepUphillSpinning()
    {
        RaycastHit2D groundHit = Physics2D.Raycast(presenter.transform.position, Vector2.down, 1.5f, LayerMask.GetMask("Default"));
        float pushAwayForce = 0f;
        
        if (groundHit.collider != null && groundHit.distance < GROUND_DISTANCE)
        {
            pushAwayForce = (GROUND_DISTANCE - groundHit.distance) * 3f;
        }
        
        float verticalComponent = rb.linearVelocity.y + Mathf.Max(pushAwayForce - 1f, 0f);
        float existingXVelocity = rb.linearVelocity.x * 0.95f;
        
        return new Vector3(existingXVelocity, verticalComponent, 0);
    }

    /// <summary>
    /// 中程度の斜面移動を処理（40-45度）
    /// </summary>
    private Vector3 HandleMediumSlopeMovement(int vecX)
    {
        Vector2 normal = groundNormals[1];
        float currentVelocityY = ApplyCircleColliderBehavior(2f, 0.1f);
        
        Vector3 desiredMove = new Vector3(vecX, 0, 0) * (Speed / 10);
        Vector3 projectedMove = Vector3.ProjectOnPlane(desiredMove, normal);
        
        return new Vector3(projectedMove.x, currentVelocityY, 0);
    }

    /// <summary>
    /// 表面投影で通常の地面移動を処理
    /// </summary>
    private Vector3 HandleNormalGroundMovement(int vecX)
    {
        Vector2 normal = groundNormals[1];
        float currentVelocityY = ApplyCircleColliderBehavior(2f, 0.1f);
        
        Vector3 desiredMove = new Vector3(vecX, 0, 0) * Speed;
        Vector3 projectedMove = Vector3.ProjectOnPlane(desiredMove, normal);
        
        return new Vector3(projectedMove.x, currentVelocityY, 0);
    }

    /// <summary>
    /// 地面距離維持のためCircleCollider2Dのような動作を適用
    /// </summary>
    private float ApplyCircleColliderBehavior(float pushForceMultiplier, float distanceThreshold)
    {
        RaycastHit2D groundHit = Physics2D.Raycast(presenter.transform.position, Vector2.down, 1.5f, LayerMask.GetMask("Default"));
        float currentVelocityY = rb.linearVelocity.y;
        
        if (groundHit.collider != null && groundHit.distance < GROUND_DISTANCE - distanceThreshold)
        {
            float pushForce = (GROUND_DISTANCE - groundHit.distance) * pushForceMultiplier;
            currentVelocityY = Mathf.Max(currentVelocityY, pushForce);
        }
        
        return currentVelocityY;
    }

    #endregion

    #region Rotation System

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

    #region Jump System

    /// <summary>
    /// ジャンプシステムを初期化
    /// </summary>
    private void InitializeJump()
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

    #region Ground Detection System

    /// <summary>
    /// 地面検出システムを初期化
    /// </summary>
    public void InitializeGroundDetection()
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

    private bool GroundCheckRay(float length = 1.1f)
    {
        bool isHit = false;

        // 3本のレイキャストで地面を検出
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
    
    /// <summary>
    /// 斜面を登ろうとしているかチェック（法線ベースの判定）
    /// </summary>
    private bool IsMovingUpSlope(int moveDirection, Vector2 slopeNormal)
    {
        if (moveDirection == 0) return false;
        
        // 法線に基づいて移動した場合のベクトルを計算
        Vector3 desiredMove = new Vector3(moveDirection, 0, 0);
        Vector3 projectedMove = Vector3.ProjectOnPlane(desiredMove, slopeNormal);
        
        // 投影された移動ベクトルのY成分で上り下りを判定
        // Y成分が正の場合は上り、負の場合は下り
        return projectedMove.y > 0.1f; // 明確に上向きの場合のみtrue
    }
    
    /// <summary>
    /// 斜面を下ろうとしているかチェック（法線ベースの判定）
    /// </summary>
    private bool IsMovingDownSlope(int moveDirection, Vector2 slopeNormal)
    {
        if (moveDirection == 0) return false;
        
        // 法線に基づいて移動した場合のベクトルを計算
        Vector3 desiredMove = new Vector3(moveDirection, 0, 0);
        Vector3 projectedMove = Vector3.ProjectOnPlane(desiredMove, slopeNormal);
        
        // 投影された移動ベクトルのY成分で上り下りを判定
        // Y成分が負の場合は下り
        return projectedMove.y < -0.1f; // 明確に下向きの場合のみtrue
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