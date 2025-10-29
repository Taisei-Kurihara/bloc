// ==========================
// DynamicObjectController_Default.cs
// デフォルトコントローラー
// ==========================
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DynamicObjectController_Default : DynamicObjectController_interface
{
    public async UniTask OnSceneLoadedAsync()
    {
        // デフォルトは何もしない
        await UniTask.CompletedTask;
    }

    public async UniTask OnSceneUnloadedAsync()
    {
        await UniTask.CompletedTask;
    }
}
