using Common;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CanvasManager : Singleton_MonoBehaviourBase<CanvasManager>
{

    private AsyncOperationHandle<GameObject> currentCanvasHandle;

    private void Start()
    {
        LoadCanvasAsync().Forget();
    }

    /// <summary> Addressableを使用してCanvasを非同期で読み込む. </summary>
    /// <param name="loadCanvasName">読み込むCanvasのAddressable名.</param>
    public async UniTask LoadCanvasAsync(string loadCanvasName = "Canvas_Blank")
    {
        // 既存のCanvasがあれば破棄する.
        if (currentCanvasHandle.IsValid())
        {
            AddressablesManager.Instance().ReleaseHandle(currentCanvasHandle);
        }

        // AddressablesManagerを使用してcanvasを読み込む.
        currentCanvasHandle = AddressablesManager.Instance().LoadAsset(loadCanvasName, AddressableTiming.Persistent, this.transform);

        await currentCanvasHandle.ToUniTask();

        if (currentCanvasHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log($"Canvas '{loadCanvasName}' を読み込みました.");
        }
        else
        {
            Debug.LogError($"Canvas '{loadCanvasName}' の読み込みに失敗しました.");
        }
    }
}
