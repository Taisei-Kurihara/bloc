using Common;
using InGame;
using UnityEngine;

public abstract class ModelBase : IModelBase
{
    protected CharacterPresenterBase presenter;

    protected ModelBase(CharacterPresenterBase presenter)
    {
        this.presenter = presenter;
    }
    public abstract void Init();
}

public interface IModelBase
{
    public abstract void Init();
}