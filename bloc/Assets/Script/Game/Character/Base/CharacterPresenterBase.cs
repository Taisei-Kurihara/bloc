using UnityEngine;

public abstract class CharacterPresenterBase : MonoBehaviour, IStatusProvider
{
    public virtual void AllSet(
        GameObject point,
        Move_interface move = null,
        Shape_interface shape = null,
        IStatus_base status = null,
        Input_AI_abstract input = null,
        CharacterPresenter_D_AttackEntity_abstract attack = null)
    {
        Point = point;
        if (move != null) Move = move;
        if (shape != null) Shape = shape;
        if (status != null) Status = status;
        if (input != null) Input = input;
        if (attack != null) Attack = attack;
    }
    public abstract Move_interface Move { get; protected set; }
    　
    public abstract Shape_interface Shape { get; protected set; }

    public abstract IStatus_base Status { get; protected set; }

    // IStatusProvider の明示的実装.
    IStatus_base IStatusProvider.Status => Status;

    public abstract Input_AI_abstract Input { get; protected set; }

    public abstract CharacterPresenter_D_AttackEntity_abstract Attack { get; protected set; }

    [SerializeField]
    protected GameObject Point;
}
