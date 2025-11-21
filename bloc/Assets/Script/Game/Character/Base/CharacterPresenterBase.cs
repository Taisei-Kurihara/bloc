using UnityEngine;

public abstract class CharacterPresenterBase : MonoBehaviour
{
    Move_interface Move { get; }

    Shape_interface Shape { get; }

    Status_abstract<StatusInitialize_abstract> Status { get; }
}
