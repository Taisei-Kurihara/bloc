using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Start_SetUp : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        Debug.Log("実行開始時に呼ばれた (AfterSceneLoad)");

        // StartCoroutine/UniTaskで非同期呼び出し
        _ = InitAsync();
    }

    static async UniTaskVoid InitAsync()
    {

        ShapeUnitCirclePolygonManager.Instance().InitializeShapesAsync().Forget();

        string currentSceneName = SceneManager.GetActiveScene().name;
        if (System.Enum.TryParse<UseScene>(currentSceneName, out UseScene currentScene))
        {
            Debug.Log($"現在のシーン名を設定中: {currentScene}");
            await DynamicObjectManager.Instance().SetScene(currentScene);
            // fade開始前に読み込みを開始.
            DynamicObjectManager.Instance().OnBeforeFadeLoadAsync().Forget();
            DynamicObjectManager.Instance().OnFadeCompleted().Forget();
            Debug.Log($"シーン設定完了: {currentScene}");
        }
        else
        {
            Debug.LogWarning($"シーン名 '{currentSceneName}' をUseSceneに変換できませんでした");
        }
    }

}
