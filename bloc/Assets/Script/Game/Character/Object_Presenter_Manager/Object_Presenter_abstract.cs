using UnityEngine;

public abstract class Object_Presenter_abstract<T> : MonoBehaviour where T : CharacterPresenterBase
{
    protected virtual T presenter { get; set; }
    public virtual T Presenter { get { return presenter; } set { PresenterSet(value); } }

    [SerializeField]
    protected GameObject Point;

    protected virtual void PresenterSet(T valuepresenter)
    {
        var addedComponent = gameObject.AddComponent(valuepresenter.GetType());

        presenter = (T)addedComponent;
        presenter.AllSet(
            Point,
            valuepresenter.Move,
            valuepresenter.Shape,
            valuepresenter.Status,
            valuepresenter.Input,
            valuepresenter.Attack);
    }
}

// 非ジェネリック版（後方互換性のため）.
public abstract class Object_Presenter_abstract : Object_Presenter_abstract<CharacterPresenterBase>
{
}
