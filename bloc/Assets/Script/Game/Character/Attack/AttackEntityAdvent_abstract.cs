using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class AttackEntityAdvent_abstract : ModelBase
{
    protected AttackEntityAdvent_abstract(CharacterPresenterBase presenter) : base(presenter)
    {
        this.presenter = presenter;
        this.shape = presenter.Shape;
    }

    protected Shape_interface shape { get; set; }

    // 発射速度（継承クラスで変更可能）.
    public virtual float FireSpeed => 10f;

    // 攻撃オブジェクトのShape角数（継承クラスで変更可能）.
    // デフォルトはプレイヤーの現在の形状と同じ.
    public virtual int AttackShapeAngular => presenter?.Shape?.shape?.length ?? 3;

    // 攻撃対象（継承クラスで変更可能）.
    public virtual List<ContactType> HitTargets => new List<ContactType> { ContactType.Enemy };

    // 攻撃の生存時間（継承クラスで変更可能）.
    public virtual float AttackLifeTime => 5f;

    // 攻撃のサイズ（継承クラスで変更可能）.
    public virtual float AttackSize => 1.0f;

    // 貫通数（継承クラスで変更可能）.
    public virtual int PenetrationCount => 0;

    // 地形貫通（継承クラスで変更可能）.
    public virtual bool IgnoreTerrain => false;

    // 攻撃の色（継承クラスで変更可能）.
    public virtual Color AttackColor => Color.white;

    // 追加移動関数（継承クラスで変更可能）.
    public virtual Func<Vector2, float, Vector2> AdditionalMovement => null;

    // キャッシュ用フィールド.
    protected bool _isInitialized = false;

    public override void Init()
    {
        // 形状変更時に最新のShapeを取得.
        shape = presenter.Shape;
        _isInitialized = true;
    }

    /// <summary>
    /// 攻撃発動（AttackInstancedRendererを使用）.
    /// </summary>
    public virtual async UniTask Advent()
    {
        if (!_isInitialized) return;

        // 最新のshapeを取得（形状変更に対応）.
        shape = presenter.Shape;
        if (shape == null || shape.shape == null) return;

        Vector2[] vertices = shape.shape.vertices;
        float versize = shape.shape.versize;

        if (vertices == null || vertices.Length == 0) return;

        // AttackInstancedRendererのインスタンスを取得.
        var renderer = AttackInstancedRenderer.Instance;

        // 各頂点方向に攻撃を発射.
        Vector3 presenterPos = presenter.transform.position;
        Quaternion presenterRot = presenter.transform.rotation;

        // プレイヤーの回転角度を取得（ラジアン）.
        float presenterRotationRad = presenterRot.eulerAngles.z * Mathf.Deg2Rad;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector2 vertexDir = vertices[i];

            // presenterの回転を適用した方向（verticesは単位円上なのでそのまま使用）.
            Vector2 rotatedDir = presenterRot * vertexDir;
            rotatedDir.Normalize();

            // 発射位置: presenter.position + 回転適用済みのオフセット.
            // versizeはピクセル単位（100）なのでUnity単位に変換.
            float unitSize = versize / 100f;
            Vector2 firePosition = (Vector2)presenterPos + (rotatedDir * unitSize);

            // AttackInstancedRendererに攻撃を追加.
            renderer.AddAttack(
                startPosition: firePosition,
                direction: rotatedDir,
                speed: FireSpeed,
                lifeTime: AttackLifeTime,
                owner: presenter,
                targetContactTypes: HitTargets,
                shapeAngular: AttackShapeAngular,
                shapeSize: AttackSize,
                penetrationCount: PenetrationCount,
                ignoreTerrain: IgnoreTerrain,
                additionalMovement: AdditionalMovement,
                color: AttackColor,
                initialRotation: presenterRotationRad
            );
        }

        await UniTask.CompletedTask;
    }
}
