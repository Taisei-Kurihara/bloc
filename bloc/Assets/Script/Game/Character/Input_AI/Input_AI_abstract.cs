using UnityEngine;

public abstract class Input_AI_abstract : ModelBase
{
    protected Input_AI_abstract(CharacterPresenterBase presenter) : base(presenter)
    {
    }

    protected virtual Move_interface move { get; set; }
}
