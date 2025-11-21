using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class Status_AttackEntity_abstract : Status_abstract<StatusInitializeAttack_abstract>
{

    CharacterPresenterBase presenter;

    protected new Status_Current_AttackEntity status { get; set; }

}

public abstract class StatusInitializeAttack_abstract : StatusInitialize_abstract
{
    public StatusInitializeAttack_abstract(CharacterPresenterBase presenter, ContactType contactType) : base(presenter, contactType)
    {
        this.presenter = presenter;
        this.masterHitstatus = contactType;
    }

}

public class StatusInitializeAttack_Default : StatusInitializeAttack_abstract
{
    public StatusInitializeAttack_Default(CharacterPresenterBase presenter, ContactType contactType) : base(presenter, contactType)
    {
        this.presenter = presenter;
        this.masterHitstatus = contactType;
    }
}

public class Status_Current_AttackEntity : Status_Current_interface
{
    public float hp { get; set; }
    public float hpMax { get; set; }

    public float attack { get; set; }
}