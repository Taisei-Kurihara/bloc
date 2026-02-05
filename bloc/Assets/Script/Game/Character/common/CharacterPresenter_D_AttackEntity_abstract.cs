using System;
using System.Collections.Generic;
using Common;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class CharacterPresenter_D_AttackEntity_abstract : CharacterPresenterBase, Presenter_interface
{
    object Presenter_interface.View => new View();

    // 使用中かどうかのフラグ（初期値:未使用）.
    public bool IsInUse { get; set; } = false;

    // 初期化済みかどうかのフラグ.
    public bool IsInitialized { get; private set; } = false;

    /// <summary>
    /// 初期化状態をリセットする（形状変更時などに再初期化を可能にする）.
    /// </summary>
    public void ResetInitializationState()
    {
        IsInitialized = false;
    }

    // プールで使用するAttackEntityAdvent型.
    public Type PoolAdventType { get; set; }

    // x秒後に未使用に戻るための秒数（継承先で変更可能、0.001～120でクランプ）.
    [SerializeField]
    private float _returnToPoolSeconds = 5f;
    protected float ReturnToPoolSeconds
    {
        get => _returnToPoolSeconds;
        set => _returnToPoolSeconds = Mathf.Clamp(value, 0.001f, 120f);
    }


    // 基底クラスのabstractプロパティをoverrideで実装.
    protected Move_AttackEntity_abstract _move;
    protected Shape_Model_AttackEntity_abstract _shape;
    protected Input_AI_Attack_abstract _input;
    protected Hit_AttackEntity_abstract _attack;
    protected Status_AttackEntity_abstract _status;
    protected AttackEntityAdvent_abstract _advent;

    public override Move_interface Move { get => _move; protected set => _move = (Move_AttackEntity_abstract)value; }
    public override Shape_interface Shape { get => _shape; protected set => _shape = (Shape_Model_AttackEntity_abstract)value; }
    public override IStatus_base Status { get => _status; protected set => _status = (Status_AttackEntity_abstract)value; }
    public override Input_AI_abstract Input { get => _input; protected set => _input = (Input_AI_Attack_abstract)value; }
    public override CharacterPresenter_D_AttackEntity_abstract Attack { get => null; protected set { } }
    public Hit_AttackEntity_abstract AttackEntity { get => _attack; protected set => _attack = (Hit_AttackEntity_abstract)value; }

    // 派生型でアクセスしやすくするプロパティ.
    public Move_AttackEntity_abstract MoveAttack => _move;
    public Shape_Model_AttackEntity_abstract ShapeAttack => _shape;
    public Status_AttackEntity_abstract StatusAttack => _status;
    public AttackEntityAdvent_abstract Advent => _advent;

    public virtual void Init()
    {
    }

    public virtual void Initialize(
        ShapeCompStatus shapeComp,
        List<ContactType> HitTargets,
        Move_AttackEntity_abstract move = null,
        Shape_Model_AttackEntity_abstract shape = null,
        Status_AttackEntity_abstract status = null,
        Hit_AttackEntity_abstract attack = null,
        Func<CharacterPresenterBase, AttackEntityAdvent_abstract> adventFactory = null)
    {
        _shape = shape ?? new Shape_Model_AttackEntity_Default(this, Point.GetComponent<SpriteRenderer>(), Point.GetComponent<EdgeCollider2D>(), GetComponent<CircleCollider2D>(), shapeComp);
        _shape.Init();

        _move = move ?? new Move_AttackEntity_Default(this, _shape);
        _move.Init();

        _status = status ?? new Status_AttackEntity_Default();
        _status.Initialize(new StatusInitializeAttack_Default(this, ContactType.Attack));
        _status.Init();

        _attack = attack ?? new Hit_AttackEntity_Default(this);
        _attack.HitTargets = HitTargets;
        _attack.Init();

        // AttackEntityAdventを生成.
        if (adventFactory != null)
        {
            _advent = adventFactory(this);
            _advent.Init();
        }

        IsInitialized = true;
    }

    /// <summary>
    /// 発射用関数（継承先で変更不可）.
    /// 状態にかかわらずx秒後に未使用に戻る処理を行い、virtualの発射用関数を呼び出す.
    /// </summary>
    public void Fire()
    {
        FireAsync().Forget();
    }

    /// <summary>
    /// 発射用非同期関数.
    /// </summary>
    private async UniTaskVoid FireAsync()
    {
        // Shapeを設定.
        if (_shape != null && _advent != null)
        {
            await _shape.SetShapeAsync(_advent.AttackShapeAngular);
        }

        // x秒後に未使用に戻る処理を開始.
        ReturnToPoolAfterDelayAsync().Forget();

        // virtualの発射用関数呼び出し.
        OnFire();
    }

    /// <summary>
    /// x秒後に未使用プールに戻す非同期処理.
    /// </summary>
    private async UniTaskVoid ReturnToPoolAfterDelayAsync()
    {
        await UniTask.Delay((int)(ReturnToPoolSeconds * 1000));
        
        // 未使用プールに戻す.
        var instance = DynamicObjectController_Game_Default.Instance();
        if (instance != null)
        {
            instance.ReturnAttackObject(this);
        }
    }

    /// <summary>
    /// virtualの発射用関数（継承先でオーバーライド可能）.
    /// </summary>
    protected virtual void OnFire()
    {
        // 継承先で実装.
    }
}
