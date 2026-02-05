using UnityEngine;

// 敵AI用の抽象クラス.
public abstract class Input_AI_Enemy_abstract : Input_AI_abstract
{
    protected Input_AI_Enemy_abstract(CharacterPresenterBase presenter) : base(presenter)
    {
    }
}
