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
    protected AudioClip contactSound;

    [SerializeField]
    protected ContactType[] ResponseTargets;  // 複数形に変更
    
    protected Rigidbody2D rb;
    public Rigidbody2D RB { get { if (rb == null) rb = GetComponent<Rigidbody2D>(); return rb; } }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.OnTriggerEnter2DAsObservable()
            .Subscribe(collider =>
            {
                // 相手の Layer 名称を取得
                string otherLayerName = LayerMask.LayerToName(collider.gameObject.layer);

                // ResponseTargets の enum 名称と比較
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
