using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class AttackEntityAdvent_abstract : ModelBase
{
    protected AttackEntityAdvent_abstract(CharacterPresenterBase presenter) : base(presenter)
    {
        this.presenter = presenter;
        this.shape = presenter.Shape;
    }

    protected Shape_interface shape { get; set; }

    public override void Init()
    {
    }

    public virtual async UniTask Advent()
    {

    }


}
