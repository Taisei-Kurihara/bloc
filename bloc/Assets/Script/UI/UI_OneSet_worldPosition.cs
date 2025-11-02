using Cysharp.Threading.Tasks;
using UnityEngine;

// UI配置の揃え方向を定義する列挙型.
public enum UI_Alignment
{
    Center, // 中央揃え.
    Left,   // 左揃え.
    Right   // 右揃え.
}

public class UI_OneSet_worldPosition : UI_OneSet_abstract
{
    // カメラ (nullの場合はメインカメラを使用).
    protected Camera TargetCamera { get; set; }

    // UIのオフセット位置.
    protected Vector3 worldPos { get; set; }
    public Vector3 OfSet { set { worldPos = value; } }
    // UI配置の揃え方向 (デフォルトは中央揃え).
    protected UI_Alignment Alignment => UI_Alignment.Center;

    // 配置されたタイミングを記録する変数.
    private float startTime;

    public void UpdateStart()
    {
        startTime = Time.time;

        PosUpdate().Forget();

    }

    // 自動削除までの時間 (0以下の場合は自動削除しない).
    protected float AutoDestroyTime => -1f;

    // ターゲットが削除された場合に自身も削除するかどうか.
    protected bool DestroyWithTarget => false;

    protected async UniTask PosUpdate()
    {
        while (true)
        {
            // 自動削除時間のチェック.
            if (AutoDestroyTime > 0f && Time.time - startTime >= AutoDestroyTime)
            {
                Destroy(gameObject);
                return;
            }

            UpdatePosition();
            await UniTask.Delay(1);
        }
    }


    // ワールド座標からスクリーン座標に変換してUIを配置する関数.
    protected void UpdatePosition()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        rectTransform.position = screenPos;
    }

    protected override void Initialize()
    {
    }
}
