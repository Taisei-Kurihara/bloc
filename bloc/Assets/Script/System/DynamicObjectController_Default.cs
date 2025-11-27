// ==========================
// DynamicObjectController_Default.cs
// デフォルトコントローラー
// ==========================
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DynamicObjectController_Default : DynamicObjectController_interface
{
    public List<DynamicObjectController_interface> ChildControllers => new List<DynamicObjectController_interface>();
    public async UniTask OnBeforeFadeLoadAsync()
    {
        await UniTask.CompletedTask;
    }

    public async UniTask OnFadeCompletedAsync()
    {
        // デフォルトは何もしない
        await UniTask.CompletedTask;
    }

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
