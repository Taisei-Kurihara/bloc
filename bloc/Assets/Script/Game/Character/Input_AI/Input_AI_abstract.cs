using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class Input_AI_abstract : ModelBase, IDisposable
{
    protected Input_AI_abstract(CharacterPresenterBase presenter) : base(presenter)
    {
    }

    protected virtual Move_interface move { get; set; }

    /// <summary>
    /// 移動可能フラグ（default: true）.
    /// </summary>
    public bool CanMove { get; set; } = true;

    /// <summary>
    /// MoveInputループ用CancellationTokenSource.
    /// </summary>
    protected CancellationTokenSource _moveInputCts;

    /// <summary>
    /// MoveInput繰り返しUniTask開始.
    /// </summary>
    public virtual void StartMoveInput()
    {
        StopMoveInput();
        _moveInputCts = CancellationTokenSource.CreateLinkedTokenSource(
            presenter.GetCancellationTokenOnDestroy()
        );
        MoveInputLoopAsync(_moveInputCts.Token).Forget();
    }

    /// <summary>
    /// MoveInput繰り返し破棄用関数.
    /// </summary>
    public virtual void StopMoveInput()
    {
        _moveInputCts?.Cancel();
        _moveInputCts?.Dispose();
        _moveInputCts = null;
    }

    /// <summary>
    /// 繰り返しUniTask関数.
    /// </summary>
    protected virtual async UniTaskVoid MoveInputLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (CanMove && move != null)
                ProcessMoveInput();
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
    }

    /// <summary>
    /// 移動入力処理（継承先でオーバーライド）.
    /// </summary>
    protected virtual void ProcessMoveInput() { }

    /// <summary>
    /// リソース解放.
    /// </summary>
    public virtual void Dispose()
    {
        StopMoveInput();
    }
}
