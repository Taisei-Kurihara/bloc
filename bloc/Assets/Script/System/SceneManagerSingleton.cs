using System.Diagnostics;
using Common;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using R3;
using R3.Collections;
using UnityEngine;

/// <summary>
/// 使用可能なシーンを定義する列挙型
/// </summary>
public enum UseScene
{
    Title,
    Game_C_Lv1_Lesson1
}

/// <summary>
/// シーン管理シングルトンクラス
/// シーンの切り替えと現在のシーン状態を管理する
/// </summary>
public class SceneManagerSingleton : Singleton_MonoBehaviourBase<SceneManagerSingleton>
{

    // 現在のシーン
    public UseScene CurrentScene { get; private set; }

    // シーンローディング中かどうかを示すフラグ
    private bool isLoading = false;

    /// <summary>
    /// 指定されたシーンに変更する
    /// 既にローディング中または同じシーンの場合は処理をスキップする
    /// </summary>
    /// <param name="newScene">変更先のシーン</param>
    public async UniTask ChangeScene(UseScene newScene)
    {
        var token = this.GetCancellationTokenOnDestroy();
        if (isLoading || CurrentScene == newScene) return;

        isLoading = true;
        await LoadSceneAsync(newScene);
        CurrentScene = newScene;
        isLoading = false;
    }

    /// <summary>
    /// シーンを非同期でロードする
    /// フェードイン・アウト効果と共にシーンを切り替える
    /// </summary>
    /// <param name="scene">ロードするシーン</param>
    async UniTask LoadSceneAsync(UseScene scene)
    {
        // (既)修: Loadの表示の優先順位を1e5にする CanvasManagerのAttachToCanvas()に 表示の優先順位を設定する項目が追加された同名の関数を追加する.
        // フェード用キャンバスを呼び出し.
        var fadeHandle = Addressables.InstantiateAsync("Load");
        await fadeHandle.Task;
        var fadeObj = fadeHandle.Result;
        fadeObj.transform.SetParent(transform);

        // UIManagerのCanvas親子付け関数で親子付け（表示優先順位100000）.
        await CanvasManager.Instance().AttachToCanvas(fadeObj, 100000);

        var loadScene = fadeObj.GetComponent<LoadScene_interface>();
        if (loadScene != null)
        {
            await loadScene.StartFadeIn();
        }
        else
        {
            UnityEngine.Debug.LogWarning("LoadScene_interfaceがアタッチされていません");
        }

        // シーンをロード（非常に重要）
        var sceneHandle = Addressables.LoadSceneAsync(scene.ToString(), LoadSceneMode.Single);
        await sceneHandle.Task;

        // DynamicObjectManagerに通知
        await DynamicObjectManager.Instance().SetScene(scene);

        if (loadScene != null)
        {
            await loadScene.StartFadeOut();
        }
    }
}