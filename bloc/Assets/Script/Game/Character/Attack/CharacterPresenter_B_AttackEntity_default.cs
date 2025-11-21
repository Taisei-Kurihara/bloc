using Common;
using UnityEngine;

public class CharacterPresenter_B_AttackEntity_default : CharacterPresenterBase, Presenter_interface
{
    object Presenter_interface.View => new View();

    public Move_interface Move { get; private set; }
    public Shape_interface Shape { get; private set; }
    public Status_abstract<StatusInitialize_abstract> Status { get; private set; }

    public AttackEntity_abstract Attack { get; private set; }

    void Start()
    {
    }

    private void OnDestroy()
    {
    }

    public async void ShapeSet(int shape)
    {

        await Shape.SetShapeAsync(shape);
        Debug.Log($"[Player_Presenter] Shape set to {shape}");
    }
}