// ==========================
// DynamicObjectController_Lv1_Lesson1.cs
// Lv1_Lesson1 専用コントローラー
// ==========================
using Cysharp.Threading.Tasks;
using InGame;
using R3;
using UnityEngine;
using static UnityEngine.Rendering.ProbeAdjustmentVolume;

public class DynamicObjectController_Lv1_Lesson1 : DynamicObjectController_interface
{
    GameObject UI_worldPos_Text;
    GameObject UI_mono_Button;

    Vector2 Text_firstPos = Vector2.zero;
    Vector2 Text_secondPos_jump = new Vector2(43f, 1f);
    Vector2 Text_thirdPos_goal = new Vector2(80, 5);

    GameObject UI_worldPos_Text_firstPos;
    GameObject UI_worldPos_Text_secondPos_jump;
    GameObject UI_worldPos_Text_thirdPos_goal;

    GameObject UI_mono_Button_triangle;
    GameObject UI_mono_Button_rectangle;
    GameObject UI_mono_Button_pentagon;
    GameObject UI_mono_Button_minus;
    GameObject UI_mono_Button_plus;

    

    public async UniTask OnSceneLoadedAsync()
    {
        Debug.Log("[Lv1_Lesson1] OnSceneLoadedAsync 開始");
        await DynamicObjectController_Game_Default.Instance().OnSceneLoadedAsync();

        // UI プレハブロード
        UI_worldPos_Text = await DynamicObjectManager.Instance().LoadAssetAsync<GameObject>("UI_worldPos_Text");
        if (UI_worldPos_Text == null)
        {
            Debug.LogError("[Lv1_Lesson1] UI_worldPos_Text がロードできません");
            return;
        }

        // UI_mono_Button プレハブロード
        UI_mono_Button = await DynamicObjectManager.Instance().LoadAssetAsync<GameObject>("UI_mono_Button");
        if (UI_mono_Button == null)
        {
            Debug.LogError("[Lv1_Lesson1] UI_mono_Button がロードできません");
            return;
        }

        await WorldPos_TextSet();
        await CreateButtons();

        Debug.Log("[Lv1_Lesson1] OnSceneLoadedAsync 完了");
    }

    private async UniTask WorldPos_TextSet()
    {
        Debug.Log("[Lv1_Lesson1] WorldPos_TextSet 開始");

        // 最初の位置にテキスト生成
        UI_worldPos_Text_firstPos = GameObject.Instantiate(UI_worldPos_Text,Vector3.zero,Quaternion.identity);
        var textworldComponent1 = UI_worldPos_Text_firstPos.GetComponent<UI_OneSet_worldPosition>();
        if (textworldComponent1 != null) textworldComponent1.OfSet = Text_firstPos;
        textworldComponent1.UpdateStart();

        // 2番目の位置
        UI_worldPos_Text_secondPos_jump = GameObject.Instantiate(UI_worldPos_Text, Vector3.zero, Quaternion.identity);
        var textworldComponent2 = UI_worldPos_Text_secondPos_jump.GetComponent<UI_OneSet_worldPosition>();
        if (textworldComponent2 != null) textworldComponent2.OfSet = Text_secondPos_jump;
        textworldComponent2.UpdateStart();

        // 3番目の位置
        UI_worldPos_Text_thirdPos_goal = GameObject.Instantiate(UI_worldPos_Text, Vector3.zero, Quaternion.identity);
        var textworldComponent3 = UI_worldPos_Text_thirdPos_goal.GetComponent<UI_OneSet_worldPosition>();
        if (textworldComponent3 != null) textworldComponent3.OfSet = Text_thirdPos_goal;
        textworldComponent3.UpdateStart();

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

    private async UniTask TextUpdate()
    {
        Debug.Log("[Lv1_Lesson1] TextUpdate 開始");
        DynamicObjectController_Game_Default GD = DynamicObjectController_Game_Default.Instance();
        Debug.Log("[Lv1_Lesson1] Player_Instance 取得待機中");
        
        await UniTask.WaitUntil(() => DynamicObjectController_Game_Default.Instance().GetPlayer_Instance != null);
        GameObject Player_Instance = DynamicObjectController_Game_Default.Instance().GetPlayer_Instance;
        Debug.Log("[Lv1_Lesson1] Player_Instance 取得完了");

        var presenter = Player_Instance.GetComponent<Player_Presenter>();
        Debug.Log("[Lv1_Lesson1] Player_Presenter 取得待機中");
        await UniTask.WaitUntil(() => presenter != null && presenter.Move != null);
        Debug.Log("[Lv1_Lesson1] Player_Presenter 取得完了");

        var moveRotateOnGrounded = presenter.Move as Move_Rotate_OnGrounded;
        if (moveRotateOnGrounded == null)
        {
            Debug.LogError("[Lv1_Lesson1] Move is not Move_Rotate_OnGrounded");
            return;
        }
        Debug.Log("[Lv1_Lesson1] Move_Rotate_OnGrounded キャスト成功");

        var textComponent1 = UI_worldPos_Text_firstPos.GetComponent<UI_Mono_Text>();
        textComponent1?.Initialize("HarrowWorld");

        var textComponent2 = UI_worldPos_Text_secondPos_jump.GetComponent<UI_Mono_Text>();
        textComponent2?.Initialize("順調ですね!");

        var textComponent3 = UI_worldPos_Text_thirdPos_goal.GetComponent<UI_Mono_Text>();
        textComponent3?.Initialize("プレイしていただき\nありがとうございました!!");

        moveRotateOnGrounded.CheckRayGroundStatus
            .Where(status => status != GroundStatus.air)
            .Take(1)
            .Subscribe(async _ =>
            {
                Debug.Log("[Lv1_Lesson1] Player landed (not Air)");
                await UniTask.Delay(System.TimeSpan.FromSeconds(0.75f));
                Debug.Log("[Lv1_Lesson1] 2秒待機完了");
                // ここに接地時に一回だけ実行したい処理を追加.
                var textComponent1 = UI_worldPos_Text_firstPos.GetComponent<UI_Mono_Text>();
                textComponent1?.Initialize("コントローラ では 左のスティック\nキーボードでは A D で移動できます");
            })
            .AddTo(Player_Instance);

        // (既)修:急坂になった時一度だけ実行するを追加してください.
        moveRotateOnGrounded.CheckRayGroundStatus
            .Where(status => status == GroundStatus.steepSlope)
            .Take(1)
            .Subscribe(async _ =>
            {
                Debug.Log("[Lv1_Lesson1] Player on steep slope");
                await UniTask.Delay(System.TimeSpan.FromSeconds(0.75f));
                Debug.Log("[Lv1_Lesson1] 急坂待機完了");
                // 急坂に到達した時に一回だけ実行したい処理を追加.
                var textComponent2 = UI_worldPos_Text_secondPos_jump.GetComponent<UI_Mono_Text>();
                textComponent2?.Initialize("急坂は滑って登れません");
                await UniTask.Delay(System.TimeSpan.FromSeconds(1.25f));
                textComponent2?.Initialize("コントローラ では Aボタン\nキーボードでは スペース で\nジャンプできます");
            })
            .AddTo(Player_Instance);
    }

    private async UniTask CreateButtons()
    {
        Debug.Log("[Lv1_Lesson1] CreateButtons 開始");

        var canvas = CanvasManager.Instance();
        if (canvas == null)
        {
            Debug.LogError("[Lv1_Lesson1] CanvasManager.Instance() が null");
            return;
        }

        // 三角形ボタン生成
        UI_mono_Button_triangle = GameObject.Instantiate(UI_mono_Button, Vector3.zero, Quaternion.identity);
        var positionComponent1 = UI_mono_Button_triangle.GetComponent<UI_OneSet_Normal>();
        if (positionComponent1 != null)
        {
            await canvas.AttachToCanvas(UI_mono_Button_triangle);
            positionComponent1.SetAnchorPreservingPosition(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            positionComponent1.AnchoredPosition = new Vector2(-250f, -50f);
        }

        var buttonComponent1 = UI_mono_Button_triangle.GetComponent<UI_Mono_Button>();
        buttonComponent1.Initialize(text: "三角形");


        // 四角形ボタン生成
        UI_mono_Button_rectangle = GameObject.Instantiate(UI_mono_Button, Vector3.zero, Quaternion.identity);
        var positionComponent2 = UI_mono_Button_rectangle.GetComponent<UI_OneSet_Normal>();
        if (positionComponent2 != null)
        {
            await canvas.AttachToCanvas(UI_mono_Button_rectangle);
            positionComponent2.SetAnchorPreservingPosition(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            positionComponent2.AnchoredPosition = new Vector2(0f, -50f);
        }

        var buttonComponent2 = UI_mono_Button_rectangle.GetComponent<UI_Mono_Button>();
        buttonComponent2.Initialize(text: "四角形");


        // 五角形ボタン生成
        UI_mono_Button_pentagon = GameObject.Instantiate(UI_mono_Button, Vector3.zero, Quaternion.identity);
        var positionComponent3 = UI_mono_Button_pentagon.GetComponent<UI_OneSet_Normal>();
        if (positionComponent3 != null)
        {
            await canvas.AttachToCanvas(UI_mono_Button_pentagon);
            positionComponent3.SetAnchorPreservingPosition(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            positionComponent3.AnchoredPosition = new Vector2(250f, -50f);
        }


        var buttonComponent3 = UI_mono_Button_pentagon.GetComponent<UI_Mono_Button>();
        buttonComponent3.Initialize(width: 110f, text: "五角形");

        // マイナスボタン生成
        UI_mono_Button_minus = GameObject.Instantiate(UI_mono_Button, Vector3.zero, Quaternion.identity);
        var positionComponent4 = UI_mono_Button_minus.GetComponent<UI_OneSet_Normal>();
        if (positionComponent4 != null)
        {
            await canvas.AttachToCanvas(UI_mono_Button_minus);
            positionComponent4.SetAnchorPreservingPosition(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            positionComponent4.AnchoredPosition = new Vector2(160f, -50f);
        }

        var buttonComponent4 = UI_mono_Button_minus.GetComponent<UI_Mono_Button>();
        buttonComponent4.Initialize(width: 60f, text: "-");

        // プラスボタン生成
        UI_mono_Button_plus = GameObject.Instantiate(UI_mono_Button, Vector3.zero, Quaternion.identity);
        var positionComponent5 = UI_mono_Button_plus.GetComponent<UI_OneSet_Normal>();
        if (positionComponent5 != null)
        {
            await canvas.AttachToCanvas(UI_mono_Button_plus);
            positionComponent5.SetAnchorPreservingPosition(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            positionComponent5.AnchoredPosition = new Vector2(340f, -50f);
        }

        var buttonComponent5 = UI_mono_Button_plus.GetComponent<UI_Mono_Button>();
        buttonComponent5.Initialize(width: 60f, text: "+");

        Debug.Log("[Lv1_Lesson1] CreateButtons 完了");
    }

    private void ButtonSet()
    {
        UI_mono_Button_triangle.GetComponent<UI_Mono_Button>().Button.onClick.AddListener(() => SetShape(3));
        UI_mono_Button_rectangle.GetComponent<UI_Mono_Button>().Button.onClick.AddListener(() => SetShape(4));
        UI_mono_Button_pentagon.GetComponent<UI_Mono_Button>().Button.onClick.AddListener(() => SetShape(5));

        ChangeShape();
    }

    private void ChangeShape()
    {
        int shape = 5;
        
        UI_mono_Button_minus.GetComponent<UI_Mono_Button>().Button.onClick.AddListener(() => shape = ChangeShapenum(shape,-1));
        UI_mono_Button_plus.GetComponent<UI_Mono_Button>().Button.onClick.AddListener(() => shape = ChangeShapenum(shape,1));
    }
    private int ChangeShapenum(int nowsape,int add)
    {
        nowsape = Mathf.Clamp(nowsape+add,3, (int)Mathf.Pow(2, 6));
        UI_mono_Button_pentagon.GetComponent<UI_Mono_Button>().Button.onClick.RemoveAllListeners();
        UI_mono_Button_pentagon.GetComponent<UI_Mono_Button>().Button.onClick.AddListener(() => SetShape(nowsape));
        string fullText = ConvertToKanjiNumber(nowsape) + "角形";
        string displayText = fullText.Length > 3 ? fullText.Substring(0, 3) : fullText;
        UI_mono_Button_pentagon.GetComponent<UI_Mono_Button>().ButtonText.Text = displayText;
        return nowsape;
    }

    // 数字を漢数字に変換するメソッド.
    private string ConvertToKanjiNumber(int number)
    {
        if (number < 0 || number > 99) return number.ToString();

        string[] digits = { "〇", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
        string[] tens = { "", "十", "二十", "三十", "四十", "五十", "六十", "七十", "八十", "九十" };

        if (number < 10)
        {
            return digits[number];
        }
        else if (number < 20)
        {
            return "十" + (number % 10 == 0 ? "" : digits[number % 10]);
        }
        else
        {
            int ten = number / 10;
            int one = number % 10;
            return tens[ten] + (one == 0 ? "" : digits[one]);
        }
    }

    private void SetShape(int shape)
    {
        GameObject Player_Instance = DynamicObjectController_Game_Default.Instance().GetPlayer_Instance;
        if (Player_Instance != null)
        {
            var presenter = Player_Instance.GetComponent<Player_Presenter>();
            if (presenter != null)
            {
                presenter.ShapeSet(shape);
            }
            else
            {
                Debug.LogError("[Lv1_Lesson1] Player_Presenter component not found on Player_Instance");
            }
        }
        else
        {
            Debug.LogError("[Lv1_Lesson1] Player_Instance is null");
        }
    }

    public async UniTask OnBeforeFadeLoadAsync()
    {
        Debug.Log("[Lv1_Lesson1] OnBeforeFadeLoadAsync 開始");
        TextUpdate().Forget();
        ButtonSet();
        Debug.Log("[Lv1_Lesson1] OnBeforeFadeLoadAsync 完了");
    }

    public async UniTask OnFadeCompletedAsync()
    {
        await DynamicObjectController_Game_Default.Instance().OnFadeCompletedAsync();
    }


    public async UniTask OnSceneUnloadedAsync()
    {
    }

}
