using UnityEngine;

// 攻撃AI用の抽象クラス.
public abstract class Input_AI_Attack_abstract : Input_AI_abstract
{
    protected Input_AI_Attack_abstract(CharacterPresenterBase presenter) : base(presenter)
    {
    }
}
