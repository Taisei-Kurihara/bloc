using R3.Triggers;
using R3;
using UnityEngine;

public enum ContactType
{
    None,
    Player,
    Enemy,
    Bullet,
    Item,
    Wall,
    Ground,
    Water,
    Lava,
    Spike,
    Goal,
    Checkpoint,
    SavePoint,
    Door,
    Key,
    Switch,
    MovingPlatform,
    Elevator,
    ConveyorBelt,
    Ladder,
    Rope,
    Platform,
    TriggerZone,
    HazardZone,
    SafeZone
}

public abstract class Contact_abstract : MonoBehaviour
{
    [SerializeField]
    AudioClip contactSound;

    [SerializeField]
    protected ContactType[] ResponseTargets;  // ï°êîå`Ç…ïœçX
    
    protected Rigidbody2D rb;
    public Rigidbody2D RB { get { if (rb == null) rb = GetComponent<Rigidbody2D>(); return rb; } }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.OnTriggerEnter2DAsObservable()
            .Subscribe(collider =>
            {
                // ëäéËÇÃ Layer ñºèÃÇéÊìæ
                string otherLayerName = LayerMask.LayerToName(collider.gameObject.layer);

                // ResponseTargets ÇÃ enum ñºèÃÇ∆î‰är
                foreach (var target in ResponseTargets)
                {
                    if (target.ToString() == otherLayerName)
                    {
                        Debug.Log($"Contact with {otherLayerName}");
                        Contact(collider);
                        return;
                    }
                }

                Debug.Log($"No Contact with {otherLayerName}");
            })
            .AddTo(this);
    }

    protected virtual void Contact(Collider2D collider)
    {
        Destroy(gameObject);
    }
}
