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
    
    protected RectTransform rectTransform { get; private set; }

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

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

    // アンカーポイントを設定し、anchoredPositionを(0,0)に移動するメソッド.
    public void SetAnchorWithResetPosition(Vector2 anchorMin, Vector2 anchorMax)
    {
        if (rectTransform != null)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    // アンカーポイントを変更前の相対位置を維持したまま設定するメソッド.
    public void SetAnchorPreservingPosition(Vector2 anchorMin, Vector2 anchorMax)
    {
        if (rectTransform != null)
        {
            Vector2 oldPosition = rectTransform.anchoredPosition;
            Vector2 oldAnchorMin = rectTransform.anchorMin;
            Vector2 oldAnchorMax = rectTransform.anchorMax;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;

            // アンカー変更による位置のずれを補正.
            Vector2 anchorDelta = new Vector2(
                (anchorMin.x - oldAnchorMin.x + anchorMax.x - oldAnchorMax.x) * rectTransform.rect.width * 0.5f,
                (anchorMin.y - oldAnchorMin.y + anchorMax.y - oldAnchorMax.y) * rectTransform.rect.height * 0.5f
            );
            rectTransform.anchoredPosition = oldPosition - anchorDelta;
        }
    }

    // アンカーポイントのみを変更するメソッド(anchoredPositionは自動調整される).
    public void SetAnchor(Vector2 anchorMin, Vector2 anchorMax)
    {
        if (rectTransform != null)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }
    }

    // RectTransformのpositionを取得/設定するプロパティ.
    public Vector3 Position
    {
        get
        {
            return rectTransform != null ? rectTransform.position : Vector3.zero;
        }
        set
        {
            if (rectTransform != null)
            {
                rectTransform.position = value;
            }
        }
    }

    // RectTransformのlocalPositionを取得/設定するプロパティ.
    public Vector3 LocalPosition
    {
        get
        {
            return rectTransform != null ? rectTransform.localPosition : Vector3.zero;
        }
        set
        {
            if (rectTransform != null)
            {
                rectTransform.localPosition = value;
            }
        }
    }

    // RectTransformのanchoredPositionを取得/設定するプロパティ.
    public Vector2 AnchoredPosition
    {
        get
        {
            return rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
        }
        set
        {
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = value;
            }
        }
    }
}
