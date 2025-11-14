using UnityEngine;
using System.Collections.Generic;
using R3.Triggers;
using R3;

public abstract class AttackEntity_abstract : ModelBase
{
    protected AttackEntity_abstract(CharacterPresenterBase presenter) : base(presenter)
    {
        this.presenter = presenter;
    }

    private System.IDisposable hitDetectionSubscription;


    private ContactType master;

    // CurrentLayerを取得・設定するプロパティ.
    public ContactType Master
    {
        get { return master; }
        set { master = value; }
    }

    private readonly ContactType[] hitTargets_ = new ContactType[0];

    private List<ContactType> hitTargets = new List<ContactType>();



    // Hit対象を取得・設定するプロパティ.
    public List<ContactType> HitTargets
    {
        get { return hitTargets; }
        set
        {
            OnSetHitTargets(value);
        }
    }


    // HitTargetsが設定された時に呼び出されるvirtual関数.
    protected virtual void OnSetHitTargets(List<ContactType> targets)
    {
        hitTargets = new List<ContactType>();

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


    public override void Init()
    {
        SetupHitDetection();
    }

    // 当たり判定を設定する関数.
    protected void SetupHitDetection()
    {
        // 既存のサブスクリプションがあればキャンセル.
        CancelHitDetection();

        hitDetectionSubscription = presenter
            .OnTriggerEnter2DAsObservable()
            .Where(collider => collider.gameObject.layer == LayerMask.NameToLayer("Default")) // 地面のレイヤーを指定
            .Subscribe(collider =>
            {
                Status_abstract status = collider.GetComponent<Status_abstract>();

                if (status != null && status.masterHitstatus != null)
                {
                    if (hitTargets.Contains(status.masterHitstatus))
                    {
                        OnHitTarget(status);
                    }
                    else
                    {

                    }
                }
                else
                {

                }
            });
    }

    // 当たり判定をキャンセルする関数.
    protected void CancelHitDetection()
    {
        if (hitDetectionSubscription != null)
        {
            hitDetectionSubscription.Dispose();
            hitDetectionSubscription = null;
        }
    }

    // Hit対象に当たった時に呼び出されるvirtual関数.
    protected virtual void OnHitTarget(Status_abstract target)
    {

    }



}
