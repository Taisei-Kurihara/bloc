using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public abstract class UI_OneSet_alignment : UI_OneSet_abstract
{
    protected abstract List<UI_OneSet_abstract> UIList { get; }

    // 次へ移動する条件を返す関数.
    protected abstract bool CanMoveNext();

    // 前に戻る条件を返す関数.
    protected abstract bool CanMovePrevious();

    protected virtual void Highlightchanged(int currentIndex, int nextIndex)
    {

    }

    // 現在指定されているList番号と次の指定されている番号を受け取って処理する関数.
    protected abstract void ProcessIndexChange(int currentIndex, int nextIndex);
}