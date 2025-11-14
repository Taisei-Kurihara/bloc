using Common;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class CharacterPresenter_D_Attack_abstract : CharacterPresenterBase, Presenter_interface
{
    object Presenter_interface.View => new View();

    public Move_interface Move { get; private set; }
    public Shape_interface Shape { get; private set; }
    public Status_abstract Status { get; private set; }

    [SerializeField]
    GameObject Point;

    void Start()
    {
        Shape = new Shape_Model(this, Point.GetComponent<SpriteRenderer>(), Point.GetComponent<EdgeCollider2D>(), GetComponent<CircleCollider2D>(), 3);
        Shape.Init();

        Move = new Move_Rotate_OnGrounded(this, Shape);
        Move.Init();

        Status = gameObject.AddComponent<Status_BattleCharacter_default>();
        ((Status_BattleCharacter_default)Status).Initialize(this, ContactType.Player);
        Status.Init();

    }
}