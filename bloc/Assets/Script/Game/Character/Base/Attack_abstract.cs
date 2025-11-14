using System.Collections.Generic;
using UnityEngine;
public enum CurrentLayer
{
    None,
    Player,
    Enemy,
    PlayerAttack,
    EnemyAttack
}

public abstract class Attack_abstract : MonoBehaviour
{
    private ContactType master;

    // CurrentLayerを取得・設定するプロパティ.
    public ContactType Master
    {
        get { return master; }
        set { master = value; }
    }

    private List<ContactType> hitDefaultTargets = new List<ContactType>();

    // Hit対象を取得・設定するプロパティ.
    public List<ContactType> HitDefaultTargets
    {
        get { return hitDefaultTargets; }
        set { hitDefaultTargets = value; }
    }
}
