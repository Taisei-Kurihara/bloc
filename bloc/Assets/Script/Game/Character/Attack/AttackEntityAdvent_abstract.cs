using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class AttackEntityAdvent_abstract : ModelBase
{
    protected AttackEntityAdvent_abstract(CharacterPresenterBase presenter, Shape_interface shape) : base(presenter)
    {
        this.presenter = presenter;
        this.shape = shape;
    }

    protected Shape_interface shape { get; set; }

    public override void Init()
    {
    }

    public virtual async UniTask Advent()
    {

    }


}
