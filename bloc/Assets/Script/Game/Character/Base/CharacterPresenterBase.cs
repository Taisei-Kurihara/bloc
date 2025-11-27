using UnityEngine;

public abstract class CharacterPresenterBase : MonoBehaviour
{
    public abstract Move_interface Move { get; protected set; }

    public abstract Shape_interface Shape { get; protected set; }

    public abstract IStatus_base Status { get; protected set; }
}
