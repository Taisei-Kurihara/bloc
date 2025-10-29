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
    private readonly List<Object> loadedAssets = new();

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

        // 前回ロード済みアセットを解放
        ReleaseAllAssets();

        // デフォルトで初期化
        UseDO = new DynamicObjectController_Default();

        // シーンに応じてコントローラー切り替え
        switch (scene)
        {
            case UseScene.Game_C_Lv1_Lesson1:
                UseDO = new DynamicObjectController_Lv1_Lesson1();
                break;
        }

        // シーンロード後処理を実行
        await UseDO.OnSceneLoadedAsync();
    }

    /// <summary>
    /// Addressable を非同期ロード
    /// </summary>
    public async UniTask<T> LoadAssetAsync<T>(string address) where T : Object
    {
        Debug.Log($"[DynamicObjectManager] LoadAssetAsync 開始: {address}");
        var handle = Addressables.LoadAssetAsync<T>(address);
        await handle.ToUniTask();

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            loadedAssets.Add(handle.Result);
            Debug.Log($"[DynamicObjectManager] LoadAssetAsync 成功: {address}");
            return handle.Result;
        }
        else
        {
            Debug.LogError($"[DynamicObjectManager] LoadAssetAsync 失敗: {address}");
            return null;
        }
    }

    /// <summary>
    /// すべてのロード済みアセットを解放
    /// </summary>
    public void ReleaseAllAssets()
    {
        foreach (var asset in loadedAssets)
        {
            if (asset != null)
            {
                Addressables.Release(asset);
                Debug.Log($"[DynamicObjectManager] Released: {asset.name}");
            }
        }
        loadedAssets.Clear();
    }
}
