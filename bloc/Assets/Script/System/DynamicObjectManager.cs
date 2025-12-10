// ==========================
// DynamicObjectManager.cs
// Addressables と動的オブジェクト管理
// ==========================
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DynamicObjectManager : Singleton_DestroyAvailableMonoSingleton<DynamicObjectManager>
{
    DynamicObjectController_interface UseDO = new DynamicObjectController_Default();
    public DynamicObjectController_interface USEDO { get { return UseDO; } }
    private bool initialized = false;
    private readonly Dictionary<AddressableAssetAddress, Object> loadedAssets = new();

    // アセットがどのSceneControllerTypeでロードされたかを記録.
    private readonly Dictionary<AddressableAssetAddress, SceneControllerType> assetControllerTypes = new();

    /// <summary>
    /// シーンに応じたコントローラー設定
    /// </summary>
    public async UniTask SetScene(UseScene scene)
    {
        // Addressables 初期化 (一度だけ)
        if (!initialized)
        {
            Debug.Log("[DynamicObjectManager] Addressables 初期化開始");
            await Addressables.InitializeAsync().ToUniTask();
            initialized = true;
            Debug.Log("[DynamicObjectManager] Addressables 初期化完了");
        }

        // 新しいコントローラーを決定.
        DynamicObjectController_interface newController;
        switch (scene)
        {
            case UseScene.Game_C_Lv1_Lesson1:
                newController = new DynamicObjectController_Lv1_Lesson1();
                break;
            default:
                newController = new DynamicObjectController_Default();
                break;
        }

        // 新しいコントローラーで使用するアセットアドレスを収集.
        HashSet<AddressableAssetAddress> newAssetAddresses = CollectAllAssetAddresses(newController);

        // 前回ロード済みの内、新しいコントローラーで使わないアセットのみ解放.
        ReleaseUnusedAssets(newAssetAddresses);

        // SceneControllerTypeに基づくリリース処理.
        SceneControllerType newControllerType = newController.ControllerType;
        ReleaseAssetsByControllerType(newControllerType);

        // コントローラーを切り替え.
        UseDO = newController;

        // シーンロード後処理を実行.
        await ExecuteOnChildControllersAsync(UseDO, async controller => await controller.OnSceneLoadedAsync());
    }

    /// <summary>
    /// fade開始前に読み込みを開始する処理を実行.
    /// </summary>
    public async UniTask OnBeforeFadeLoadAsync()
    {
        // (既)修:UseDO の持つ ChildControllers を全てawaitでOnBeforeFadeLoadAsyncを実行するようにしてください (UseDOと同じクラスでなければ).
        await ExecuteOnChildControllersAsync(UseDO, async controller => await controller.OnBeforeFadeLoadAsync());
    }

    /// <summary>
    /// fade完了後の処理を実行.
    /// </summary>
    public async UniTask OnFadeCompleted()
    {
        // (既)修:UseDO の持つ ChildControllers を全てawaitでOnFadeCompletedAsyncを実行するようにしてください (UseDOと同じクラスでなければ).
        await ExecuteOnChildControllersAsync(UseDO, async controller => await controller.OnFadeCompletedAsync());
    }

    /// <summary>
    /// シーンのアンロード時の処理を実行.
    /// </summary>
    public async UniTask OnSceneUnloadedAsync()
    {
        // (既)修:UseDO の持つ ChildControllers を全てawaitでOnSceneUnloadedAsyncを実行するようにしてください (UseDOと同じクラスでなければ).
        await ExecuteOnChildControllersAsync(UseDO, async controller => await controller.OnSceneUnloadedAsync());
    }

    /// <summary>
    /// Addressable を非同期ロード（enum版）.
    /// </summary>
    public async UniTask<T> LoadAssetAsync<T>(AddressableAssetAddress address) where T : Object
    {
        // 既にロード済みの場合はキャッシュから返す.
        if (loadedAssets.TryGetValue(address, out Object cachedAsset))
        {
            Debug.Log($"[DynamicObjectManager] LoadAssetAsync キャッシュ使用: {address}");
            return cachedAsset as T;
        }

        string addressString = address.ToAddress();
        Debug.Log($"[DynamicObjectManager] LoadAssetAsync 開始: {addressString}");
        try
        {
            var handle = Addressables.LoadAssetAsync<T>(addressString);
            await handle.ToUniTask();

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedAssets[address] = handle.Result;
                // アセットがどのControllerTypeでロードされたかを記録.
                assetControllerTypes[address] = UseDO.ControllerType;
                Debug.Log($"[DynamicObjectManager] LoadAssetAsync 成功: {addressString} (Type: {UseDO.ControllerType})");
                return handle.Result;
            }
            else
            {
                Debug.LogWarning($"[DynamicObjectManager] LoadAssetAsync 失敗: {addressString}");
                return null;
            }
        }
        catch (UnityEngine.AddressableAssets.InvalidKeyException)
        {
            Debug.LogWarning($"[DynamicObjectManager] アセットが見つかりません（Addressablesに未登録）: {addressString}");
            return null;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[DynamicObjectManager] LoadAssetAsync 例外: {addressString} - {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Addressable を非同期ロード（文字列版 - 後方互換性用）.
    /// </summary>
    public async UniTask<T> LoadAssetAsync<T>(string address) where T : Object
    {
        // 文字列をenumに変換を試みる.
        if (System.Enum.TryParse<AddressableAssetAddress>(address, out var enumAddress))
        {
            return await LoadAssetAsync<T>(enumAddress);
        }

        // enumに変換できない場合は直接ロード（キャッシュなし）.
        Debug.Log($"[DynamicObjectManager] LoadAssetAsync 開始（非enum）: {address}");
        try
        {
            var handle = Addressables.LoadAssetAsync<T>(address);
            await handle.ToUniTask();

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"[DynamicObjectManager] LoadAssetAsync 成功（非enum）: {address}");
                return handle.Result;
            }
            else
            {
                Debug.LogWarning($"[DynamicObjectManager] LoadAssetAsync 失敗: {address}");
                return null;
            }
        }
        catch (UnityEngine.AddressableAssets.InvalidKeyException)
        {
            Debug.LogWarning($"[DynamicObjectManager] アセットが見つかりません（Addressablesに未登録）: {address}");
            return null;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[DynamicObjectManager] LoadAssetAsync 例外: {address} - {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// ChildControllersを再帰的に処理するヘルパーメソッド.
    /// </summary>
    private async UniTask ExecuteOnChildControllersAsync(DynamicObjectController_interface controller, System.Func<DynamicObjectController_interface, UniTask> action)
    {
        // 自身を実行.
        await action(controller);

        // ChildControllersがあれば再帰的に実行.
        if (controller.ChildControllers != null)
        {
            foreach (var child in controller.ChildControllers)
            {
                if (child != null && child.GetType() != controller.GetType())
                {
                    await ExecuteOnChildControllersAsync(child, action);
                }
            }
        }
    }

    /// <summary>
    /// コントローラーとその子コントローラーの全アセットアドレスを収集.
    /// </summary>
    private HashSet<AddressableAssetAddress> CollectAllAssetAddresses(DynamicObjectController_interface controller)
    {
        HashSet<AddressableAssetAddress> addresses = new HashSet<AddressableAssetAddress>();
        CollectAssetAddressesRecursive(controller, addresses);
        return addresses;
    }

    /// <summary>
    /// 再帰的にアセットアドレスを収集.
    /// </summary>
    private void CollectAssetAddressesRecursive(DynamicObjectController_interface controller, HashSet<AddressableAssetAddress> addresses)
    {
        // このコントローラーのアドレスを追加.
        if (controller.LoadedAssetAddresses != null)
        {
            foreach (var address in controller.LoadedAssetAddresses)
            {
                addresses.Add(address);
            }
        }

        // 子コントローラーを再帰的に処理.
        if (controller.ChildControllers != null)
        {
            foreach (var child in controller.ChildControllers)
            {
                if (child != null && child.GetType() != controller.GetType())
                {
                    CollectAssetAddressesRecursive(child, addresses);
                }
            }
        }
    }

    /// <summary>
    /// 指定されたアドレス以外のアセットを解放.
    /// </summary>
    private void ReleaseUnusedAssets(HashSet<AddressableAssetAddress> keepAddresses)
    {
        List<AddressableAssetAddress> addressesToRelease = new List<AddressableAssetAddress>();

        foreach (var kvp in loadedAssets)
        {
            if (!keepAddresses.Contains(kvp.Key))
            {
                addressesToRelease.Add(kvp.Key);
            }
        }

        foreach (var address in addressesToRelease)
        {
            if (loadedAssets.TryGetValue(address, out Object asset) && asset != null)
            {
                Addressables.Release(asset);
                Debug.Log($"[DynamicObjectManager] Released: {address}");
            }
            loadedAssets.Remove(address);
            assetControllerTypes.Remove(address);
        }
    }

    /// <summary>
    /// SceneControllerTypeに基づいてアセットを解放.
    /// InGame -> OutGame: InGameでロードされたアセットを解放.
    /// OutGame -> InGame: OutGameでロードされたアセットを解放.
    /// シーン切り替え時: CurrentSceneOnlyでロードされたアセットを解放.
    /// </summary>
    private void ReleaseAssetsByControllerType(SceneControllerType newControllerType)
    {
        List<AddressableAssetAddress> addressesToRelease = new List<AddressableAssetAddress>();

        foreach (var kvp in assetControllerTypes)
        {
            SceneControllerType assetType = kvp.Value;
            bool shouldRelease = false;

            // CurrentSceneOnlyは常にシーン切り替え時に解放.
            if (assetType == SceneControllerType.CurrentSceneOnly)
            {
                shouldRelease = true;
            }
            // InGameでロードされたものはOutGame時に解放.
            else if (assetType == SceneControllerType.InGame && newControllerType == SceneControllerType.OutGame)
            {
                shouldRelease = true;
            }
            // OutGameでロードされたものはInGame時に解放.
            else if (assetType == SceneControllerType.OutGame && newControllerType == SceneControllerType.InGame)
            {
                shouldRelease = true;
            }

            if (shouldRelease)
            {
                addressesToRelease.Add(kvp.Key);
            }
        }

        foreach (var address in addressesToRelease)
        {
            if (loadedAssets.TryGetValue(address, out Object asset) && asset != null)
            {
                Addressables.Release(asset);
                Debug.Log($"[DynamicObjectManager] Released by ControllerType: {address} (was: {assetControllerTypes[address]}, new: {newControllerType})");
            }
            loadedAssets.Remove(address);
            assetControllerTypes.Remove(address);
        }
    }

    /// <summary>
    /// すべてのロード済みアセットを解放.
    /// </summary>
    public void ReleaseAllAssets()
    {
        foreach (var kvp in loadedAssets)
        {
            if (kvp.Value != null)
            {
                Addressables.Release(kvp.Value);
                Debug.Log($"[DynamicObjectManager] Released: {kvp.Key}");
            }
        }
        loadedAssets.Clear();
        assetControllerTypes.Clear();
    }
}
