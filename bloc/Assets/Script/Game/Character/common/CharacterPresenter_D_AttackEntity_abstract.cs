using Common;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class CharacterPresenter_D_AttackEntity_abstract : CharacterPresenterBase, Presenter_interface
{
    object Presenter_interface.View => new View();


    public Move_AttackEntity_abstract Move { get; private set; }
    public Shape_interface Shape { get; private set; }
    public Status_AttackEntity_abstract Status { get; private set; }

    [SerializeField]
    GameObject Point;

    void Start()
    {
        Shape = new Shape_Model_Player(this, Point.GetComponent<SpriteRenderer>(), Point.GetComponent<EdgeCollider2D>(), GetComponent<CircleCollider2D>(), 3);
        Shape.Init();

        Move = new Move_AttackEntity_Default(this, Shape);
        Move.Init();

        Status = gameObject.AddComponent<Status_AttackEntity_Default>();
        ((Status_AttackEntity_Default)Status).Initialize(new StatusInitializeAttack_Default(this, ContactType.Attack));
        Status.Init();

    }
}