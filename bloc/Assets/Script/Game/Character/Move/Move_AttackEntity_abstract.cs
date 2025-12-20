using Common;
using InGame;
using UnityEngine;

public abstract class Move_AttackEntity_abstract : ModelBase, Move_interface
{
    protected Move_AttackEntity_abstract(CharacterPresenterBase presenter, Shape_interface shape) : base(presenter)
    {
        this.presenter = presenter;
        this.shape = shape;
    }

    protected Shape_interface shape { get; set; }

    protected Rigidbody2D rb;

    protected UnityEngine.Vector2 input { get; set; } = UnityEngine.Vector2.zero;
    UnityEngine.Vector2 Move_interface.input { get => input; set => input = value; }

    // 発射速度.
    public float FireSpeed { get; set; } = 10f;

    public override void Init()
    {
        rb = presenter.GetComponent<Rigidbody2D>();
    }

    public void SetMoveInput(Vector2 input)
    {
        this.input = input;
    }

    /// <summary>
    /// 発射方向と速度を設定してオブジェクトを飛ばす.
    /// </summary>
    /// <param name="direction">発射方向（正規化される）.</param>
    /// <param name="speed">発射速度（省略時はFireSpeed使用）.</param>
    public virtual void Fire(Vector2 direction, float? speed = null)
    {
        if (rb == null) return;
        float fireSpeed = speed ?? FireSpeed;
        rb.linearVelocity = direction.normalized * fireSpeed;
    }

    /// <summary>
    /// 位置を設定する.
    /// </summary>
    /// <param name="position">設定位置.</param>
    public virtual void SetPosition(Vector3 position)
    {
        if (presenter != null)
        {
            presenter.transform.position = position;
        }
    }
}
