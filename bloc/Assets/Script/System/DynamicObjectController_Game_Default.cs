using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DynamicObjectController_Game_Default : Singleton_DestroyAvailableMonoSingleton<DynamicObjectController_Game_Default>, DynamicObjectController_interface
{
    GameObject Player_Prefab;
    GameObject Player_Instance;

    GameObject Attack_Prefab;
    public GameObject GetPlayer_Instance => Player_Instance;

    public List<DynamicObjectController_interface> ChildControllers => new List<DynamicObjectController_interface>();

    public async UniTask OnBeforeFadeLoadAsync()
    {

    }

    public async UniTask OnSceneLoadedAsync()
    {
        Debug.Log("[Game_Default] OnBeforeFadeLoadAsync: Player プレハブロード開始");
        // Player プレハブロード.
        Player_Prefab = await DynamicObjectManager.Instance().LoadAssetAsync<GameObject>("Player");
        if (Player_Prefab == null)
        {
            Debug.LogError("[Game_Default] Player がロードできません");
            return;
        }
        else
        {
            Debug.Log("[Game_Default] OnBeforeFadeLoadAsync: Player プレハブロード完了");
        }

        Attack_Prefab = await DynamicObjectManager.Instance().LoadAssetAsync<GameObject>("Attack");
        if (Attack_Prefab == null)
        {
            Debug.LogError("[Game_Default] Attack がロードできません");
        }
        else
        {
            Debug.Log("[Game_Default] OnSceneLoadedAsync: Attack プレハブロード完了");
        }
    }

    public async UniTask OnFadeCompletedAsync()
    {
        Debug.Log("[Game_Default] OnFadeCompletedAsync: Player_Prefab待機開始");
        await UniTask.WaitUntil(() => Player_Prefab != null);
        Debug.Log("[Game_Default] OnFadeCompletedAsync: Player_Prefab待機完了");
        // Player をシーンに生成.
        Player_Instance = GameObject.Instantiate(Player_Prefab, new Vector3(0f, -2.5f, 0f), Quaternion.identity);
        Debug.Log("[Game_Default] OnFadeCompletedAsync: Player生成完了");
    }


    public async UniTask OnSceneUnloadedAsync()
    {
        await UniTask.CompletedTask;
    }
}