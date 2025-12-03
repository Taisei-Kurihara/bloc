using System.Drawing;
using Common;
using UnityEngine;

public class CharacterPresenter_B_AttackEntity_default : CharacterPresenter_D_AttackEntity_abstract
{

    

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