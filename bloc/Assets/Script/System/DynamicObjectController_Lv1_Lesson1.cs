// ==========================
// DynamicObjectController_Lv1_Lesson1.cs
// Lv1_Lesson1 専用コントローラー
// ==========================
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DynamicObjectController_Lv1_Lesson1 : DynamicObjectController_interface
{
    GameObject Player;
    GameObject UI_worldPos_Text;

    Vector2 Text_firstPos = Vector2.zero;
    Vector2 Text_secondPos_jump = new Vector2(20, 0);
    Vector2 Text_thirdPos_goal = new Vector2(50, 5);

    GameObject UI_worldPos_Text_firstPos;
    GameObject UI_worldPos_Text_secondPos_jump;
    GameObject UI_worldPos_Text_thirdPos_goal;

    public async UniTask OnSceneLoadedAsync()
    {
        Debug.Log("[Lv1_Lesson1] OnSceneLoadedAsync 開始");

        // UI プレハブロード
        UI_worldPos_Text = await DynamicObjectManager.Instance().LoadAssetAsync<GameObject>("UI_worldPos_Text");
        if (UI_worldPos_Text == null)
        {
            Debug.LogError("[Lv1_Lesson1] UI_worldPos_Text がロードできません");
            return;
        }

        await WorldPos_TextSet();

        // Player プレハブロード
        Player = await DynamicObjectManager.Instance().LoadAssetAsync<GameObject>("Player");
        if (Player == null)
        {
            Debug.LogError("[Lv1_Lesson1] Player がロードできません");
            return;
        }

        // Player をシーンに生成
        GameObject.Instantiate(Player, new Vector3(0f, -2.5f, 0f), Quaternion.identity);

        Debug.Log("[Lv1_Lesson1] OnSceneLoadedAsync 完了");
    }

    private async UniTask WorldPos_TextSet()
    {
        Debug.Log("[Lv1_Lesson1] WorldPos_TextSet 開始");

        // 最初の位置にテキスト生成
        UI_worldPos_Text_firstPos = GameObject.Instantiate(UI_worldPos_Text,Vector3.zero,Quaternion.identity);
        var textComponent1 = UI_worldPos_Text_firstPos.GetComponent<UI_OneSet_worldPosition_Text>();
        textComponent1?.Initialize("HarrowWorld", 100f);
        if (textComponent1 != null) textComponent1.OfSet = Text_firstPos;
        textComponent1.UpdateStart();

        // 2番目の位置
        UI_worldPos_Text_secondPos_jump = GameObject.Instantiate(UI_worldPos_Text, Vector3.zero, Quaternion.identity);
        var textComponent2 = UI_worldPos_Text_secondPos_jump.GetComponent<UI_OneSet_worldPosition_Text>();
        textComponent2?.Initialize("ここでジャンプ", 100f);
        if (textComponent2 != null) textComponent2.OfSet = Text_secondPos_jump;
        textComponent2.UpdateStart();

        // 3番目の位置
        UI_worldPos_Text_thirdPos_goal = GameObject.Instantiate(UI_worldPos_Text, Vector3.zero, Quaternion.identity);
        var textComponent3 = UI_worldPos_Text_thirdPos_goal.GetComponent<UI_OneSet_worldPosition_Text>();
        textComponent3?.Initialize("プレイしていただき\nありがとうございました!!", 100f);
        if (textComponent3 != null) textComponent3.OfSet = Text_thirdPos_goal;
        textComponent3.UpdateStart();

        // Canvas にアタッチ
        var canvas = CanvasManager.Instance();
        if (canvas == null)
        {
            Debug.LogError("[Lv1_Lesson1] CanvasManager.Instance() が null");
        }
        else
        {
            await canvas.AttachToCanvas(UI_worldPos_Text_firstPos);
            await canvas.AttachToCanvas(UI_worldPos_Text_secondPos_jump);
            await canvas.AttachToCanvas(UI_worldPos_Text_thirdPos_goal);
        }



        Debug.Log("[Lv1_Lesson1] WorldPos_TextSet 完了");
    }

    public async UniTask OnSceneUnloadedAsync()
    {
        await UniTask.CompletedTask;
    }
}
