using UnityEngine;

public class Status_BattleCharacter_default : Status_BattleCharacter_abstract
{
    void Awake()
    {
        defaultDamage = new ChangeHP_Damage_Default();
        defaultHeal = new ChangeHP_Heal_Default();
    }

    public override void Init()
    {
        // (既)修: log追加this.obj.name + クラス名 +  masterHitstatus.
        Debug.Log($"{gameObject.name} + {this.GetType().Name} + {masterHitstatus}");
    }
}
