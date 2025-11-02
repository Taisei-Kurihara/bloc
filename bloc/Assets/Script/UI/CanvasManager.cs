using Common;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CanvasManager : Singleton_MonoBehaviourBase<CanvasManager>
{

    private GameObject currentCanvas;

    private void Start()
    {
        LoadCanvasAsync().Forget();
    }

    /// <summary> Addressableを使用してCanvasを非同期で読み込む. </summary>
    /// <param name="loadCanvasName">読み込むCanvasのAddressable名.</param>
    public async UniTask LoadCanvasAsync(string loadCanvasName = "Canvas_Blank")
    {
        // 既存のCanvasがあれば破棄する.
        if (currentCanvas != null)
        {
            // 自身の子オブジェクトをすべて削除.
            foreach (Transform child in this.transform)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
            UnityEngine.Object.Destroy(currentCanvas);
            currentCanvas = null;
        }

        // AddressablesManagerを使用してcanvasを読み込む.
        currentCanvas = Instantiate(await AddressablesManager.Instance().LoadAssetAsync(loadCanvasName, AddressableTiming.Persistent));

        currentCanvas.transform.SetParent(this.transform);
        if (currentCanvas != null)
        {
            Debug.Log($"Canvas '{loadCanvasName}' を読み込みました.");
        }
        else
        {
            Debug.LogError($"Canvas '{loadCanvasName}' の読み込みに失敗しました.");
        }
    }

    /// <summary>
    /// 外部からCanvas objectを親子付けできる関数
    /// SetUpCanvasがtrueになるまで待機してから実行
    /// </summary>
    /// <param name="childObject">親子付けするオブジェクト</param>
    public async UniTask AttachToCanvas(GameObject childObject)
    {
        // SetUpCanvasがtrueになるまで待機
        await UniTask.WaitUntil(() => currentCanvas);

        if (currentCanvas != null && childObject != null)
        {
            childObject.transform.SetParent(currentCanvas.transform, false);
        }
    }

    /// <summary>
    /// 外部からCanvas objectを親子付けできる関数（表示優先順位付き）.
    /// SetUpCanvasがtrueになるまで待機してから実行.
    /// </summary>
    /// <param name="childObject">親子付けするオブジェクト.</param>
    /// <param name="sortingOrder">表示優先順位（大きいほど前面に表示）.</param>
    public async UniTask AttachToCanvas(GameObject childObject, int sortingOrder)
    {
        // SetUpCanvasがtrueになるまで待機.
        await UniTask.WaitUntil(() => currentCanvas);

        if (currentCanvas != null && childObject != null)
        {
            childObject.transform.SetParent(currentCanvas.transform);

            // CanvasコンポーネントがあればsortingOrderを設定.
            Canvas canvas = childObject.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = sortingOrder;
            }
        }
    }
}
