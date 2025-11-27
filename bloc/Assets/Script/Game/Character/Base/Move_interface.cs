using UnityEngine;

public interface Move_interface : IModelBase
{
    // (既)修: 入力値を Vector2型を 受け取る public関数を継承先で追加するように指定するようにしてください.
    Vector2 input { get; set; }    void SetMoveInput(Vector2 input);
}
