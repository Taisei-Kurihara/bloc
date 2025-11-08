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

public enum Hitstatus
{
    None,
    Player,
    NPC,
    Enemy,
    Attack,
    Support,
    Guard
}

public abstract class Attack_abstract : MonoBehaviour
{
    private Hitstatus master;

    // CurrentLayerを取得・設定するプロパティ.
    public Hitstatus Master
    {
        get { return master; }
        set { master = value; }
    }

    private List<Hitstatus> hitDefaultTargets = new List<Hitstatus>();

    // Hit対象を取得・設定するプロパティ.
    public List<Hitstatus> HitDefaultTargets
    {
        get { return hitDefaultTargets; }
        set { hitDefaultTargets = value; }
    }
}
