using Common;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public enum UseScene
{
    Title,
    Game_C_Lv1_Lesson1
}

public class SceneManagerSingleton : Singleton_MonoBehaviourBase<SceneManagerSingleton>
{
    public UseScene CurrentScene { get; private set; } = UseScene.Title; // デフォルトは Title
    private bool isLoading = false;
    private bool isInitialized = false;

    private async void Awake()
    {
        // Addressables 初期化
        try
        {
            await Addressables.InitializeAsync();
            isInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SceneManager] Addressables initialization failed: {e}");
        }

        // 起動時の現在シーンを CurrentScene にセット
        InitCurrentScene();
    }

    /// <summary>
    /// 起動時のシーン名から CurrentScene を初期化
    /// </summary>
    private void InitCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (System.Enum.TryParse<UseScene>(sceneName, out UseScene scene))
        {
            CurrentScene = scene;
        }
        else
        {
            Debug.LogWarning($"[SceneManager] CurrentScene 初期化失敗: シーン名 '{sceneName}' は UseScene に存在しません。デフォルト Title を使用");
            CurrentScene = UseScene.Title;
        }
    }

    /// <summary>
    /// シーン切り替え（強制再ロード可能）
    /// </summary>
    public async UniTask ChangeScene(UseScene newScene, bool forceReload = false)
    {
        Debug.Log($"[SceneManager] ChangeScene called: {newScene} (forceReload={forceReload})");

        if (isLoading) return;
        isLoading = true;

        await UniTask.WaitUntil(() => isInitialized);

        try
        {
            await LoadSceneAsync(newScene);
            CurrentScene = newScene;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SceneManager] ChangeScene failed: {e}");
        }

        isLoading = false;
    }

    private async UniTask LoadSceneAsync(UseScene scene)
    {

        // CanvasManager 初期化待機
        await UniTask.WaitUntil(() => CanvasManager.Instance() != null);
        await UniTask.WaitUntil(() => DynamicObjectManager.Instance() != null);

        await CanvasManager.Instance().LoadCanvasAsync();

        GameObject fadeObj = null;

        try
        {
            var fadeHandle = Addressables.InstantiateAsync("Load");
            await fadeHandle.Task;
            fadeObj = fadeHandle.Result;
            fadeObj.transform.SetParent(transform);
            fadeObj.transform.localPosition = Vector3.zero;
            await CanvasManager.Instance().AttachToCanvas(fadeObj, 100);

            // UI_OneSet_Normalコンポーネントを取得してサイズと位置を設定.
            var uiOneSet = fadeObj.GetComponent<UI_OneSet_Normal>();
            if (uiOneSet != null)
            {
                uiOneSet.SetAnchor(Vector2.one/2, Vector2.one/2);
                uiOneSet.AnchoredPosition = Vector3.zero;
                uiOneSet.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
            }

        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SceneManager] Failed to instantiate fade prefab: {e}");
        }

        var loadScene = fadeObj?.GetComponent<LoadScene_interface>();

        if (loadScene != null) await loadScene.StartFadeIn();

        // シーンロード
        try
        {
            var sceneHandle = Addressables.LoadSceneAsync(scene.ToString(), LoadSceneMode.Single);
            await sceneHandle.Task;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SceneManager] Scene load failed: {e}");
            throw;
        }

        // DynamicObjectManager に通知
        try
        {
            await DynamicObjectManager.Instance().SetScene(scene);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SceneManager] DynamicObjectManager.SetScene failed: {e.Message}");
        }

        // fade開始前に読み込みを開始.
        DynamicObjectManager.Instance().OnBeforeFadeLoadAsync().Forget();

        if (loadScene != null)
        {
            await loadScene.StartFadeOut();
            GameObject.Destroy(fadeObj);
        }

        DynamicObjectManager.Instance().OnFadeCompleted().Forget();
    }
}
