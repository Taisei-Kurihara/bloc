using Common;
using InGame;
using UnityEngine;

public abstract class ModelBase : IModelBase
{
    protected PresenterBase presenter;

    protected ModelBase(PresenterBase presenter)
    {
        this.presenter = presenter;
    }
    public abstract void Init();
}

public interface IModelBase
{
    public abstract void Init();
}