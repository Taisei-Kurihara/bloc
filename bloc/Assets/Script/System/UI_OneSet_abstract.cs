using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public abstract class UI_OneSet_abstract : MonoBehaviour
{
    [SerializeField]
    protected Image highlightImage { get; set; }
    private CancellationTokenSource _highlightCts;

    // 初期化処理を行う抽象メソッド.
    protected abstract void Initialize();

    protected bool isHighlight
    {
        get
        {
            isHighlight = (highlightImage != null) ? highlightImage.gameObject.activeInHierarchy : false;
            return isHighlight;
        }
        set
        {
            isHighlight = value;
            _highlightCts?.Cancel();
            _highlightCts?.Dispose();
            _highlightCts = new CancellationTokenSource();
            SetHighlightState(value, _highlightCts.Token).Forget();
        }
    }

    protected async virtual UniTask SetHighlightState(bool isActive, CancellationToken cancellationToken = default)
    {
        highlightImage?.gameObject.SetActive(isActive);
    }

    protected virtual void OnDestroy()
    {
        _highlightCts?.Cancel();
        _highlightCts?.Dispose();
    }
}
