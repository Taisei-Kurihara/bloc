using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class Status_abstract : MonoBehaviour, IModelBase
{
    protected CharacterPresenterBase presenter;

    public ContactType masterHitstatus { get; protected set; }

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

    protected virtual ChangeHP defaultDamage { get; set; }
    protected virtual ChangeHP defaultHeal { get; set; }

    public abstract void Init();

    protected virtual async UniTask OnDamaged(float damage, ChangeHP aschangeHP = null)
    {
        aschangeHP ??= defaultDamage;
        aschangeHP.OnHPChange(this,damage);
    }

    protected virtual async UniTask OnHeal(float heal, ChangeHP aschangeHP = null)
    {
        aschangeHP ??= defaultHeal;
        aschangeHP.OnHPChange(this, heal);
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
    public UniTask OnHPChange(Status_abstract nowstatus,float hp);
}

public class ChangeHP_Damage_Default : ChangeHP
{
    public ChangeHPtype changeHPtype { get; private set; } = ChangeHPtype.Damage;

    public async UniTask OnHPChange(Status_abstract nowstatus, float damage)
    {
        if (damage <= 0) { return; } 
        nowstatus.hp = Mathf.Clamp(nowstatus.hp - damage,0, nowstatus.hpMax);
    }
}

public class ChangeHP_Heal_Default : ChangeHP
{
    public ChangeHPtype changeHPtype { get; private set; } = ChangeHPtype.Heal;

    public async UniTask OnHPChange(Status_abstract nowstatus, float heal)
    {
        if (heal <= 0) { return; }
        nowstatus.hp = Mathf.Clamp(nowstatus.hp + heal, 0, nowstatus.hpMax);
    }
}