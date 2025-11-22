using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class Status_abstract<TIni> : MonoBehaviour, IModelBase where TIni : StatusInitialize_abstract
{

    TIni StatusIni;

    protected CharacterPresenterBase presenter { get { return StatusIni.presenter; } set { StatusIni.presenter = value; } }

    public ContactType masterHitstatus { get { return StatusIni.masterHitstatus; } private set { StatusIni.masterHitstatus = value; } }

    protected Status_Current_interface status { get; set; }

    public float hp
    {
        get => status.hp;
        set => status.hp = value;
    }

    public float hpMax
    {
        get => status.hpMax;
        set => status.hpMax = value;
    }

    protected virtual ChangeHP defaultDamage { get; set; } = new ChangeHP_Damage_Default();
    protected virtual ChangeHP defaultHeal { get; set; } = new ChangeHP_Heal_Default();

    public abstract void Init();

    public virtual void Initialize(TIni ini)
    {
        StatusIni = ini;
    }

    public virtual async UniTask OnDamaged(float damage, ChangeHP aschangeHP = null)
    {
        aschangeHP ??= defaultDamage;
        aschangeHP.OnHPChange(this, damage);
    }

    public virtual async UniTask OnHeal(float heal, ChangeHP aschangeHP = null)
    {
        aschangeHP ??= defaultHeal;
        aschangeHP.OnHPChange(this , heal);
    }
}

public abstract class StatusInitialize_abstract
{
    public StatusInitialize_abstract(CharacterPresenterBase presenter, ContactType contactType)
    {
        this.presenter = presenter;
        masterHitstatus = contactType;
    }

    public CharacterPresenterBase presenter;

    public ContactType masterHitstatus;
}

public class StatusInitialize_Default : StatusInitialize_abstract
{
    public StatusInitialize_Default(CharacterPresenterBase presenter, ContactType contactType) : base(presenter, contactType)
    {
        this.presenter = presenter;
        masterHitstatus = contactType;
    }
}


public interface Status_Current_interface
{
    public float hp { get; set; }
    public float hpMax { get; set; }
}

public enum ChangeHPtype
{
    None,
    Damage,
    Heal
}

public interface ChangeHP
{
    public ChangeHPtype changeHPtype { get; }
    public UniTask OnHPChange<TIni>(Status_abstract<TIni> nowstatus,float hp) where TIni : StatusInitialize_abstract;
}

public class ChangeHP_Damage_Default : ChangeHP
{
    public ChangeHPtype changeHPtype { get; private set; } = ChangeHPtype.Damage;

    public async UniTask OnHPChange<TIni>(Status_abstract<TIni> nowstatus, float damage) where TIni : StatusInitialize_abstract
    {
        if (damage <= 0) { return; }
        nowstatus.hp = Mathf.Clamp(nowstatus.hp - damage,0, nowstatus.hpMax);
    }
}

public class ChangeHP_Heal_Default : ChangeHP
{
    public ChangeHPtype changeHPtype { get; private set; } = ChangeHPtype.Heal;

    public async UniTask OnHPChange<TIni>(Status_abstract<TIni> nowstatus, float heal) where TIni : StatusInitialize_abstract
    {
        if (heal <= 0) { return; }
        nowstatus.hp = Mathf.Clamp(nowstatus.hp + heal, 0, nowstatus.hpMax);
    }
}