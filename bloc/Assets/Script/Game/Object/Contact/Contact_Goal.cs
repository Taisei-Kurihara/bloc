using Cysharp.Threading.Tasks;
using UnityEngine;

public class Contact_Goal : Contact_abstract
{
    override protected void Contact(Collider2D collider)
    {
        // ゴールに到達したときの処理
        Debug.Log("Goal reached!");
        ChangeScene().Forget();
        // ここでゲームクリアの処理を追加できます
        // 例: シーンの切り替え、UIの表示など
    }

    private async UniTask ChangeScene()
    {
        Debug.Log("Goal ins!");
        // SceneManagerSingletonが存在しない場合は待機または生成
        var sceneManager = SceneManagerSingleton.Instance();
        await UniTask.WaitUntil(() => sceneManager != null);
        Debug.Log("Goal not null!");

        await sceneManager.ChangeScene(UseScene.Title);
        Debug.Log("Goal cs!");
    }
}
