using UnityEngine;

public class CharacterPresenterBase : MonoBehaviour
{
    Move_interface Move { get; }

    Shape_interface Shape { get; }

    Status_abstract Status { get; }
}
