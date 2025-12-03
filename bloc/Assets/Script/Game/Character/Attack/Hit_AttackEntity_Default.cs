using UnityEngine;
using System.Collections.Generic;

// Hit攻撃エンティティのデフォルト実装.
public class Hit_AttackEntity_Default : Hit_AttackEntity_abstract
{
    public Hit_AttackEntity_Default(CharacterPresenterBase presenter) : base(presenter)
    {
        this.presenter = presenter;
    }
}
