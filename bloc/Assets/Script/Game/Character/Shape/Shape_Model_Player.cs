using System.Collections.Generic;
using R3;
using Unity.Mathematics;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Shape_Model_Player : Shape_Model_abstract
{
    public Shape_Model_Player(CharacterPresenterBase presenter, SpriteRenderer spriteRenderer, EdgeCollider2D edgeCollider,CircleCollider2D circleCollider, int An = 4) : base(presenter)
    {
        this.presenter = presenter;
        N_Angular = An;

        this.spriteRenderer = spriteRenderer;
        this.edgeCollider = edgeCollider;

        this.circleCollider = circleCollider;

        if (this.circleCollider != null)
        {
            this.circleCollider.isTrigger = true;
        }
    }

    [SerializeField]
    int N_Angular = 4;

    public override void Init()
    {
        // 非同期初期化はInitAsyncで行う.
        DebugeLineUpdate();
    }

    /// <summary>
    /// 非同期初期化処理.
    /// </summary>
    public async UniTask InitAsync()
    {
        _shapeComp = await GetOrCreateShapeAsync(N_Angular);
        SetAll();
    }

    public override async UniTask SetShapeAsync(int nAngular)
    {
        _shapeComp = await GetOrCreateShapeAsync(nAngular);
        N_Angular = nAngular;
        SetAll();
        DebugeLineUpdate();
    }

    void SetAll()
    {
        // 表示・当たり判定を更新.
        //(既)修:C:\Users\hukaw\Desktop\DF\private\2D_bloc\bloc\Assets\Script\Game\Character\Shape\ShapeUnitCirclePolygonManager.csにplayerが度の形状を設定したか記録されるようにして下さし
        ShapeUnitCirclePolygonManager.Instance().SetPlayerShape(N_Angular);
        SetSprite();
        SetColl();
    }


    void SetSprite()
    {
        if (shapeGenerator == null) return;

        offsetobj.localPosition = shapeGenerator.ShapeCase(shape.length);

        spriteRenderer.sprite = shape.sprite;
        spriteRenderer.size = new Vector2(100, 100);

    }

    void SetColl()
    {
        if (shape == null) return;

        circleCollider.radius = shape.versize / 100f;

        if (shape.length >= 24)
        {
            edgeCollider.points = new Vector2[2] { Vector2.zero,Vector2.zero };
            circleCollider.isTrigger = false;
        }
        else
        {
            edgeCollider.points = shape.ColliderPoints;
        }
    }

    #region debug
    public void DebugeN_AngularUpdate()
    {
        Observable.EveryUpdate().Subscribe(async _ =>
        {
            if (shapeGenerator == null) return;
            Debug.Log("N_Angular:" + N_Angular + "/ofs:" + shapeGenerator.ShapeCase(N_Angular).y);
            presenter.transform.position = Vector2.zero;
            await SetShapeAsync(N_Angular);
            N_Angular++;
        }).AddTo(presenter);
    }

    public void DebugeLineUpdate()
    {
        Observable.EveryUpdate().Subscribe(_ =>
        {
            if (shapeGenerator == null) return;
            // デバッグ用のラインを描画
            shapeGenerator.DebugeLine(spriteRenderer.transform, presenter.transform);
        }).AddTo(presenter);
    }

    #endregion

}
