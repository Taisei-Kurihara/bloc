using Common;
using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum AddressableTiming
{
    Persistent = 0,  // 永続的に保持.
    SceneTransition = 1,  // シーン遷移時に破棄.
    Manual = 2,  // 手動で破棄.
    Immediate = 3,  // 使用後すぐに破棄.
}

public class AddressablesManager : Singleton_MonoBehaviourBase<AddressablesManager>
{
    private Dictionary<AddressableTiming, List<AsyncOperationHandle<GameObject>>> handleDictionary = new Dictionary<AddressableTiming, List<AsyncOperationHandle<GameObject>>>();

    // Immediate用の破棄トリガー.
    private ReactiveProperty<bool> immediateDisposeTrigger = new ReactiveProperty<bool>(false);

    private void Awake()
    {
        // Dictionaryの初期化.
        foreach (AddressableTiming timing in System.Enum.GetValues(typeof(AddressableTiming)))
        {
            handleDictionary[timing] = new List<AsyncOperationHandle<GameObject>>();
        }

        // Immediateタイミングの破棄を監視.
        immediateDisposeTrigger
            .Subscribe(_ =>
            {
                ReleaseHandlesByTiming(AddressableTiming.Immediate);
            });
    }


    /// <summary> Addressableを使用してGameObjectを非同期で読み込み、GameObjectを返す. </summary>
    /// <param name="key">読み込むAddressableのキー.</param>
    /// <param name="timing">破棄タイミング.</param>
    /// <param name="parent">親Transform (nullの場合は親なし).</param>
    public async Cysharp.Threading.Tasks.UniTask<GameObject> LoadAssetAsync(string key, AddressableTiming timing = AddressableTiming.SceneTransition)
    {
        // Immediateの場合は、破棄用コールバックを登録.
        if (timing == AddressableTiming.Immediate)
        {
            immediateDisposeTrigger.Value = true;
        }

        AsyncOperationHandle<GameObject> handle;

        handle = Addressables.LoadAssetAsync<GameObject>(key);

        // handleをDictionaryに追加.
        handleDictionary[timing].Add(handle);

        // 完了を待つ.
        await handle;

        if (timing == AddressableTiming.Immediate)
        {
        }

        return handle.Result;
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

    /// <summary> Immediate破棄トリガーをリセットする. </summary>
    public void ResetImmediateDisposeTrigger()
    {
        immediateDisposeTrigger.Value = false;
    }
}
