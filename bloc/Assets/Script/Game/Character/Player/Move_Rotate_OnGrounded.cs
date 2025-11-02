using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using Common;
using InGame.Character;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public enum GroundStatus
{
    flat = 0,   // 平地.
    uphill = 1, //  上り坂状態.
    downhill = 2,   // 下り坂状態.
    steepSlope = 3,  // 急坂状態.
    junpRamp = 4,   // ジャンプ台状態.
    air = 5, // 空.
    wall = 6 // 壁.
}

public class Move_Rotate_OnGrounded : ModelBase, Move_interface, Camera_FollowTarget_interface
{
    public Move_Rotate_OnGrounded(PresenterBase presenter,Shape_interface shape, CircleCollider2D circleCollider) : base(presenter)
    {
        this.presenter = presenter;
        this.shape = shape;
        this.circleCollider = circleCollider;
    }

    #region Fields and Properties
    
    // コアコンポーネント
    private Shape_interface shape { get; set; }
    private CircleCollider2D circleCollider;
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
    private ReactiveProperty<GroundStatus> _checkRayGroundStatus = new ReactiveProperty<GroundStatus>(GroundStatus.air);
    public ReadOnlyReactiveProperty<GroundStatus> CheckRayGroundStatus => _checkRayGroundStatus;
    private GroundStatus checkRayGroundStatus
    {
        get => _checkRayGroundStatus.Value;
        set => _checkRayGroundStatus.Value = value;
    }

    // 坂の方向と急坂判定.
    private int slopeDirection = 0; // -1: 左向き上り, 1: 右向き上り, 0: 平地または空中.
    float slopeMoveAngle = 0;
    float lastgroundAngle = 3.14f;

    // 実行時変数
    private UnityEngine.Vector3 move;
    private ReactiveProperty<int> JunpCount = new ReactiveProperty<int>(0);
    private float spinSpeed = 6f;
    
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

        CanvasManager.Instance();
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
        int beforeX = 0;

        //移動処理
        Dismove = Observable.EveryUpdate().Subscribe(_ =>
        {
            // === 移動処理 ===

            // 入力取得
            var vec = action.Player.Move.ReadValue<UnityEngine.Vector2>();
            var rotatevec = action.Player.Rotate.ReadValue<float>();
            int vecX = (int)(new UnityEngine.Vector2(vec.x, 0).normalized).x;

            CheckOnGroundStatus(vecX);

            lastX = (vecX != 0) ? vecX : (checkRayGroundStatus != GroundStatus.air) ? 0 : lastX;


            // コライダーのisTrigger設定
            if (circleCollider != null)
            {
                // 移動入力時はfalse、それ以外はtrue
                //circleCollider.isTrigger = DetermineColliderBehavior(vecX, lastX);
            }

            // 物理マテリアルの摩擦設定
            pm2d.friction = vecX == 0 ? 1f : 0f;

            shape.edgeCollider.sharedMaterial = pm2d;

            // 入力と地面条件に基づく移動処理.
            ProcessMovementInput(vecX, lastX, beforeX);

            //// === 回転処理 ===
            ProcessRotationByGroundStatus(vecX, lastX);

            beforeX = vecX;

        }).AddTo(presenter);

        //重力の設定
        Disgravity = Observable.EveryUpdate(UnityFrameProvider.FixedUpdate)
            .Index()
            .Subscribe(_ =>
            {
                rigid.AddForce(UnityEngine.Vector3.up * GravityPower, ForceMode2D.Force);
            }).AddTo(presenter);
    }


    /// <summary>
    /// 地面の状態を判定する.
    /// </summary>
    private void CheckOnGroundStatus(int vecX)
    {
        float slopeAngle = 0;

        lastgroundAngle = -(3.14f/2);
        slopeDirection = 0;

        float groundDistance = 0;

        bool onground = false;
        bool onflat = false;
        bool steepSlope = false;
        bool uphill = false;

        UnityEngine.Vector2 origin = presenter.transform.position;

        // vecX!=0のときは横方向チェックを先に行う.
        if (vecX != 0)
        {
            UnityEngine.Vector2 horizontalDirection = new UnityEngine.Vector2(vecX, 0);
            RaycastHit2D horizontalHit = RaycastWithDebug(origin, horizontalDirection, 1.3f, Color.cyan);

            if (horizontalHit.collider != null)
            {
                // 角度を調べて急坂かどうか確認.
                float horizontalSlopeAngle = Mathf.Atan2(horizontalHit.normal.y, horizontalHit.normal.x) * Mathf.Rad2Deg;
                horizontalSlopeAngle -= 90f; // 地面の傾きに変換.

                const float EPSILON = 0.01f; // 誤差許容値.
                bool isHorizontalSteep = (Mathf.Abs(horizontalSlopeAngle) + EPSILON >= MAX_SLOPE_ANGLE);

                if (isHorizontalSteep)
                {
                    SetGroundStatus(GroundStatus.wall, "壁 : 横方向急坂");
                    return;
                }
            }
        }


        UnityEngine.Vector2 diagonalDirection = new UnityEngine.Vector2(vecX, -1).normalized;
        RaycastHit2D hit = RaycastWithDebug(origin, diagonalDirection, 2, Color.yellow);

        onground = (hit.collider != null);
        //if (!onground)
        //{
        //    diagonalDirection = new UnityEngine.Vector2(-vecX, -1).normalized;
        //    hit = RaycastWithDebug(origin, diagonalDirection, 2, Color.yellow);

        //    onground = (hit.collider != null);
        //}
        if (onground)
        {
            // 法線ベクトルを角度に変換
            slopeAngle = Mathf.Atan2(hit.normal.y, hit.normal.x) * Mathf.Rad2Deg;

            lastgroundAngle = (slopeAngle + 180f) * Mathf.Deg2Rad;

            // 地面方向に向かってrayを飛ばす.
            diagonalDirection = new UnityEngine.Vector2(Mathf.Cos(lastgroundAngle), Mathf.Sin(lastgroundAngle));
            RaycastHit2D groundHit = RaycastWithDebug(origin, diagonalDirection, 1.3f, Color.green);

            onground = (groundHit.collider != null);

            if (onground)
            {
                groundDistance = groundHit.distance;

                // 法線ベクトルを角度に変換
                slopeAngle = Mathf.Atan2(groundHit.normal.y, groundHit.normal.x) * Mathf.Rad2Deg;

                slopeAngle -= 90f; // 地面の傾きに変換

                slopeMoveAngle = (slopeAngle + 90) * Mathf.Deg2Rad;

                const float EPSILON = 0.01f; // 誤差許容値
                steepSlope = (Mathf.Abs(slopeAngle) + EPSILON >= MAX_SLOPE_ANGLE);

                bool input = (vecX > 0);
                bool slope = (slopeAngle > 0);

                uphill = (input == slope);

                // 下り方向確認.
                slopeDirection = ((slopeAngle > 0) ? 1 : -1) * ((uphill) ? 1 : -1);

                onflat = ((vecX == 0 && !steepSlope) || Mathf.Abs(slopeAngle) < 0.5f);
            }
        }

        switch (true)
        {
            case bool _ when !onground:
                SetGroundStatus(GroundStatus.air, "空中 : !onground");
                break;
            case bool _ when onflat:
                SetGroundStatus(GroundStatus.flat, "平地 : onflat");
                break;
            case bool _ when steepSlope && uphill:
                SetGroundStatus(GroundStatus.steepSlope, "急坂 : steepSlope && uphill");
                break;
            case bool _ when uphill:
                SetGroundStatus(GroundStatus.uphill, "上り坂 : uphill");
                break;
            case bool _ when !uphill:
                SetGroundStatus(GroundStatus.downhill, "下り坂 : !uphill");
                break;
            default:
                SetGroundStatus(GroundStatus.air, "空中 : else");
                break;
        }
    }

    void SetGroundStatus(GroundStatus value , string setText = "")
    {
        if (checkRayGroundStatus != value && setText != "") Debug.Log(setText);
        checkRayGroundStatus = value;
    }

    #endregion

    #region CircleCollider System

    /// <summary>
    /// GroundStatusに基づいてCircleColliderの挙動を決定する.
    /// </summary>
    private bool DetermineColliderBehavior(int vecX, int lastX)
    {
        // 〇コライダーの透過/非透過設定.
        if (vecX == 0)
        {
            return (checkRayGroundStatus != GroundStatus.steepSlope);
        }

        return false;
    }

    #endregion

    #region Movement System

    /// <summary>
    /// 移動入力を処理し、適切な移動ベクトルを返す.
    /// </summary>
    private void ProcessMovementInput(int vecX, int lastX, int beforeX)
    {
        if (vecX != 0)
        {
            switch (checkRayGroundStatus)
            {
                case GroundStatus.wall:
                    return;
                case GroundStatus.steepSlope:
                    SteepSlopeInertia();
                    return;
                case GroundStatus.downhill:
                    HandleDownhillMovement(vecX);
                    //if (beforeX == 0) SnapToGround(vecX);
                    return;
                case GroundStatus.junpRamp:
                case GroundStatus.uphill:
                case GroundStatus.flat:
                case GroundStatus.air:
                default:
                    HandleNormalGroundMovement(vecX);
                    return;
            }
        }
        else
        {
            switch (checkRayGroundStatus)
            {
                case GroundStatus.wall:
                    return;
                case GroundStatus.steepSlope:
                    return;
                case GroundStatus.downhill:
                case GroundStatus.junpRamp:
                case GroundStatus.uphill:
                case GroundStatus.flat:
                    if(beforeX!=0) SnapToGround(beforeX);
                    return;
                case GroundStatus.air:
                default:
                    // 空中移動.
                    AirInertia();
                    return;
            }
        }
    }



    private void HandleNormalGroundMovement(int vecX)
    {
        if (vecX == 0)
        {
            rb.linearVelocity = new UnityEngine.Vector2(0,rb.linearVelocityY);
        }

        float angle = lastgroundAngle + ((3.14f / 2) * vecX); // 角度に基づく速度調整
        // 通常の地面移動.
        UnityEngine.Vector2 MoveDirection = new UnityEngine.Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        //Debug.Log($"angle:{angle * Mathf.Rad2Deg},MoveDirection:{MoveDirection},checkRayGroundStatus:{checkRayGroundStatus}");
        
        MoveDirection *= new UnityEngine.Vector2(Speed, 0);
        MoveDirection += new UnityEngine.Vector2(0, rb.linearVelocityY);

        rb.linearVelocity = MoveDirection;
    }

    private void HandleDownhillMovement(int vecX)
    {
        if (vecX == 0)
        {
            rb.linearVelocity = new UnityEngine.Vector2(0, rb.linearVelocityY);
        }

        // 斜面を下る移動.
        float downhillSpeed = Speed * 1.2f; // 下り坂では少し速くなる

        float angle = lastgroundAngle + ((3.14f/2) * vecX); // 角度に基づく速度調整
        // 通常の地面移動.
        UnityEngine.Vector2 MoveDirection = new UnityEngine.Vector2(Mathf.Cos(angle),Mathf.Sin(angle));

        //Debug.Log($"angle:{angle * Mathf.Rad2Deg},MoveDirection:{MoveDirection},checkRayGroundStatus:{checkRayGroundStatus}");

        MoveDirection *= new UnityEngine.Vector2(downhillSpeed, 0);
        MoveDirection += new UnityEngine.Vector2(0, rb.linearVelocityY);

        rb.linearVelocity = MoveDirection;
    }


    private void AirInertia()
    {
        rb.linearVelocity = new UnityEngine.Vector3(rb.linearVelocity.x * (1f - 1e-4f), rb.linearVelocity.y, 0);
    }

    private void SteepSlopeInertia()
    {
        rb.linearVelocity = new UnityEngine.Vector3(rb.linearVelocity.x * (1f - 1e-9f), rb.linearVelocity.y, 0);
    }

    // 修:SnapToGround
    private void SnapToGround(int vecX)
    {
        rb.linearVelocity = new UnityEngine.Vector2(0, rb.linearVelocityY*0.99f);
        //if (checkRayGroundStatus == GroundStatus.air)
        //{
        //    rb.linearVelocity = new UnityEngine.Vector2(0, rb.linearVelocityY);
        //}

        //float angle = lastgroundAngle; // 角度に基づく速度調整
        //// 通常の地面移動.
        //UnityEngine.Vector2 MoveDirection = new UnityEngine.Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        //Debug.Log($"angle:{angle * Mathf.Rad2Deg},MoveDirection:{MoveDirection},checkRayGroundStatus:{checkRayGroundStatus}");


        //MoveDirection *= new UnityEngine.Vector2(0,10);
        //MoveDirection += new UnityEngine.Vector2(0, rb.linearVelocityY);

        //rb.linearVelocity = MoveDirection;
    }

    #endregion



    #region Rotation System

    /// <summary>
    /// GroundStatusに基づいて回転処理を実行する.
    /// </summary>
    private void ProcessRotationByGroundStatus(int vecX, int lastX)
    {
        if (vecX != 0)
        {
            if (checkRayGroundStatus == GroundStatus.steepSlope)
            {
                RotateWhileAirborneFast(vecX);
            }
            else
            {
                RotateWhileAirborne(vecX);
            }
            return;
        }
        else
        {
            if(checkRayGroundStatus == GroundStatus.air) RotateWhileAirborne(lastX);
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

        UnityEngine.Vector3 A1 = shape.shape.ColliderPoints[x];
        UnityEngine.Vector3 B2 = shape.shape.ColliderPoints[x2];

        float angle = UnityEngine.Vector3.SignedAngle(A1, B2, UnityEngine.Vector3.forward) * ((rb.linearVelocity.x / Speed) * ((rb.linearVelocity.x > 1) ? 1f : -1f));
        //Debug.Log(angle);
        rb.MoveRotation(rb.rotation + (-angle * 0.05f)); //回転処理
    }

    // 
    /// <summary>
    /// 回転し続ける処理
    /// </summary>
    private void RotateWhileAirborne(int lastX)
    {
        if (lastX != 0)
        {
            // 空中は lastX の方向に合わせて継続回転（補間付き）
            float targetAngle = rb.rotation + (lastX * -90f); // 1回転で90度回すイメージ
            float newRotation = Mathf.LerpAngle(rb.rotation, targetAngle, Time.fixedDeltaTime * (spinSpeed * 0.5f));
            rb.MoveRotation(newRotation);
        }
    }

    /// <summary>
    /// 急坂で急速回転する関数
    /// </summary>
    private void RotateWhileAirborneFast(int lastX)
    {
        if (lastX != 0)
        {
            float targetAngle = rb.rotation + (lastX * -90f); // 1回転で90度回すイメージ
            float newRotation = Mathf.LerpAngle(rb.rotation, targetAngle, Time.fixedDeltaTime * (spinSpeed * 0.5f * 1.5f));
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
            .Where(_ => JunpCount.Value < maxJumpCount) //ジャンプ回数が最大値以下のとき
            .Subscribe(_ =>
            {
                JunpCount.Value++;//カウント回数を増やす。
                move = UnityEngine.Vector3.right * rb.linearVelocity;
                move += UnityEngine.Vector3.up * JumpPower;
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
        // (既)修: jump回数がリセットされた時何処でリセットされたかをログに出す.
        // 地面に接地しているかの判定. 厳密にEdgeColliderの衝突判定で行う.
        shape.edgeCollider
            .OnCollisionStay2DAsObservable()
            .Where(collider => collider.gameObject.layer == LayerMask.NameToLayer("Default")) // 地面のレイヤーを指定
            .Subscribe(collider =>
            {

                if (JunpCount.Value != 0 && rb.linearVelocityY <= 0.5f)
                {
                    // 衝突点の法線方向を取得して急坂判定を行う.
                    ContactPoint2D[] contacts = new ContactPoint2D[collider.contactCount];
                    collider.GetContacts(contacts);

                    bool isSteepSlope = false;
                    foreach (var contact in contacts)
                    {
                        float slopeAngle = Mathf.Atan2(contact.normal.y, contact.normal.x) * Mathf.Rad2Deg;
                        slopeAngle -= 90f; // 地面の傾きに変換.

                        const float EPSILON = 0.01f; // 誤差許容値.
                        if (Mathf.Abs(slopeAngle) + EPSILON >= MAX_SLOPE_ANGLE)
                        {
                            isSteepSlope = true;
                            break;
                        }
                    }

                    // 急坂でない場合のみjump回数をリセット.
                    if (!isSteepSlope)
                    {
                        Debug.Log("Jump回数リセット: EdgeCollider接地判定 (急坂でない)");
                        JunpCount.Value = 0;
                    }
                }
            }).AddTo(presenter);

        // jump回数リセット. ( 既jump時 && 非上昇時 ) 地面に接地していたらリセット.
        Observable.EveryUpdate()
            .Where(_ => JunpCount.Value != 0)
            .Subscribe(_ =>
            {
                // 上昇中でない、& は地面に十分近い場合にリセット.
                if (rb.linearVelocityY <= 0.5f)
                {
                    if (GroundCheckRay(1.3f))
                    {
                        Debug.Log("Jump回数リセット: GroundCheckRay判定 (下降中)");
                        JunpCount.Value = 0;
                    }
                }
            }).AddTo(presenter);

        // 落ちた時リスポーン(debug).
        Observable.EveryUpdate()
            .Where(_ => presenter.transform.position.y<= -30)
            .Subscribe(_ =>
            {
                presenter.transform.position = new UnityEngine.Vector3(0, 0, 0);
                rb.linearVelocity = UnityEngine.Vector3.zero;
                rb.angularVelocity = 0;
                JunpCount.Value = 0;
            }).AddTo(presenter);
    }

    private bool GroundCheckRay(float length = 1.1f)
    {
        bool isHit = false;

        // 3本のレイキャストで地面を検出
        UnityEngine.Vector3[] offsets = new UnityEngine.Vector3[]
        {
        UnityEngine.Vector3.left * 0.4f,
        UnityEngine.Vector3.zero,
        UnityEngine.Vector3.right * 0.4f
        };

        for (int i = 0; i < offsets.Length; i++)
        {
            UnityEngine.Vector3 origin = presenter.transform.position + offsets[i];
            RaycastHit2D hit = RaycastWithDebug(origin, UnityEngine.Vector2.down, length, Color.red);

            if (hit.collider != null)
            {
                // 法線ベクトルから地面の傾きを計算.
                float slopeAngle = Mathf.Atan2(hit.normal.y, hit.normal.x) * Mathf.Rad2Deg;
                slopeAngle -= 90f; // 地面の傾きに変換.

                // 急坂判定(MAX_SLOPE_ANGLE以上の場合はfalseを返す).
                const float EPSILON = 0.01f; // 誤差許容値.
                if (Mathf.Abs(slopeAngle) + EPSILON >= MAX_SLOPE_ANGLE)
                {
                    return false;
                }
                else
                {
                    isHit = true;
                }

            }
        }

        return isHit;
    }


    // (既)修: RaycastHit2D とDebug.DrawRayを出してRaycastHit2Dをreturnする関数を追加 引数は Vec2 origin , Vec2 direction, float length, int layerMask = LayerMask.GetMask("Default") + 現在RaycastHit2D とDebug.DrawRayを同時に出している部分を全てこの実装した関数に置換.
    private RaycastHit2D RaycastWithDebug(UnityEngine.Vector2 origin, UnityEngine.Vector2 direction, float length, Color color, int layerMask = -1)
    {
        if (layerMask == -1)
        {
            layerMask = LayerMask.GetMask("Default");
        }
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, length, layerMask);
        Debug.DrawRay(origin, direction * length, color);
        return hit;
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