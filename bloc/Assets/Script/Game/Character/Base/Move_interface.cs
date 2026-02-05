using UnityEngine;

public interface Move_interface : IModelBase
{    Vector2 input { get; set; }    void SetMoveInput(Vector2 input);
}
