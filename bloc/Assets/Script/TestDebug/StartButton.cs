using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


public class StartButton : MonoBehaviour
{
    // 遷移先のシーン
    [SerializeField]
    UseScene useScene = UseScene.Game_C_Lv1_Lesson1;

    /// <summary>
    /// 初期化処理
    /// ボタンコンポーネントにクリックイベントを登録する
    /// </summary>
    private void Awake()
    {
        var button = GetComponent<UnityEngine.UI.Button>();

        button.onClick.AddListener(Onclick);
        Debug.Log("sss");
    }

    /// <summary>
    /// ボタンクリック時の処理
    /// 指定されたシーンへの遷移を実行する
    /// </summary>
    void Onclick()
    {
        SceneManagerSingleton.Instance().ChangeScene(useScene).Forget();
    }
}
