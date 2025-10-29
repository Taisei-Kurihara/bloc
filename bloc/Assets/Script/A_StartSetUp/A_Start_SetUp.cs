using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class A_Start_SetUp : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    //[InitializeOnLoadMethod]
    static void Init()
    {
        Debug.Log("実行開始時に呼ばれた (AfterSceneLoad)");

        // StartCoroutine/UniTaskで非同期呼び出し
        _ = InitAsync();
    }

    static async UniTaskVoid InitAsync()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (System.Enum.TryParse<UseScene>(currentSceneName, out UseScene currentScene))
        {
            Debug.Log($"現在のシーン名を設定中: {currentScene}");
            await DynamicObjectManager.Instance().SetScene(currentScene);
            Debug.Log($"シーン設定完了: {currentScene}");
        }
        else
        {
            Debug.LogWarning($"シーン名 '{currentSceneName}' をUseSceneに変換できませんでした");
        }
    }

}
