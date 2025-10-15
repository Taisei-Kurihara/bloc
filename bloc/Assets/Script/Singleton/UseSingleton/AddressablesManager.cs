using Common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum AddressableTiming
{
    Persistent = 0,  // 永続的に保持.
    SceneTransition = 1,  // シーン遷移時に破棄.
    Manual = 2,  // 手動で破棄.
}

public class AddressablesManager : Singleton_MonoBehaviourBase<AddressablesManager>
{
    private Dictionary<AddressableTiming, List<AsyncOperationHandle<GameObject>>> handleDictionary = new Dictionary<AddressableTiming, List<AsyncOperationHandle<GameObject>>>();

    private void Awake()
    {
        // Dictionaryの初期化.
        foreach (AddressableTiming timing in System.Enum.GetValues(typeof(AddressableTiming)))
        {
            handleDictionary[timing] = new List<AsyncOperationHandle<GameObject>>();
        }
    }

    /// <summary> Addressableを使用してGameObjectを非同期で読み込み、handleを返す. </summary>
    /// <param name="key">読み込むAddressableのキー.</param>
    /// <param name="timing">破棄タイミング.</param>
    /// <param name="parent">親Transform (nullの場合は親なし).</param>
    public AsyncOperationHandle<GameObject> LoadAsset(string key, AddressableTiming timing = AddressableTiming.SceneTransition, Transform parent = null)
    {
        AsyncOperationHandle<GameObject> handle;

        if (parent != null)
        {
            handle = Addressables.InstantiateAsync(key, parent);
        }
        else
        {
            handle = Addressables.InstantiateAsync(key);
        }

        // handleをDictionaryに追加.
        handleDictionary[timing].Add(handle);

        return handle;
    }

    /// <summary> 指定したタイミングのhandleを一括で削除する. </summary>
    /// <param name="timing">削除するタイミング.</param>
    public void ReleaseHandlesByTiming(AddressableTiming timing)
    {
        if (handleDictionary.ContainsKey(timing))
        {
            foreach (var handle in handleDictionary[timing])
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            handleDictionary[timing].Clear();
        }
    }

    /// <summary> 特定のhandleを手動で削除する (注意: 外部に渡したhandleを削除する場合は、外部での使用状況を確認してください). </summary>
    /// <param name="handle">削除するhandle.</param>
    public void ReleaseHandle(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.IsValid())
        {
            Addressables.Release(handle);

            // Dictionaryから削除.
            foreach (var list in handleDictionary.Values)
            {
                if (list.Contains(handle))
                {
                    list.Remove(handle);
                    break;
                }
            }
        }
    }
}
