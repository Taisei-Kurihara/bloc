using UnityEngine;
using System.Collections.Generic;

public abstract class AttackEntity_abstract : MonoBehaviour
{
    private Hitstatus master;

    // CurrentLayerを取得・設定するプロパティ.
    public Hitstatus Master
    {
        get { return master; }
        set { master = value; }
    }

    private readonly Hitstatus[] hitTargets_ = new Hitstatus[0];

    private List<Hitstatus> hitTargets = new List<Hitstatus>();

    // Hit対象を取得・設定するプロパティ.
    public List<Hitstatus> HitTargets
    {
        get { return hitTargets; }
        set
        {
            OnSetHitTargets(value);
        }
    }


    // HitTargetsが設定された時に呼び出されるvirtual関数.
    protected virtual void OnSetHitTargets(List<Hitstatus> targets)
    {
        hitTargets = new List<Hitstatus>();

        // targetsの内容を追加.
        if (targets != null)
        {
            hitTargets.AddRange(targets);
        }

        // hitTargets_の内容を追加.
        if (hitTargets_ != null)
        {
            hitTargets.AddRange(hitTargets_);
        }
    }
}
