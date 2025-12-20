using Cysharp.Threading.Tasks;
using UnityEngine;

public class AttackEntityAdvent_Player_Default : AttackEntityAdvent_abstract
{
    public AttackEntityAdvent_Player_Default(CharacterPresenterBase presenter) : base(presenter)
    {
        this.presenter = presenter;
        this.shape = presenter.Shape;
    }
}
