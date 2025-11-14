using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class Status_AttackEntity_abstract : Status_abstract
{
    protected Status_AttackEntity_abstract(CharacterPresenterBase presenter)
    {
        this.presenter = presenter;
    }

    CharacterPresenterBase presenter;

    protected new Status_Current_AttackEntity status { get; set; }

}

public class Status_Current_AttackEntity : Status_Current_interface
{
    public float hp { get; set; }
    public float hpMax { get; set; }

    public float attack { get; set; }
}