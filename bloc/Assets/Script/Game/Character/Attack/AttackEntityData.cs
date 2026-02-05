using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 攻撃エンティティデータ構造体（GameObjectなし）.
/// DrawMesh + Raycast方式で使用.
/// </summary>
public struct AttackEntityData
{
    /// <summary>
    /// アクティブ状態.
    /// </summary>
    public bool IsActive;

    /// <summary>
    /// 開始地点.
    /// </summary>
    public Vector2 StartPosition;

    /// <summary>
    /// 現在位置.
    /// </summary>
    public Vector2 Position;

    /// <summary>
    /// 前フレーム位置（Ray用）.
    /// </summary>
    public Vector2 PreviousPosition;

    /// <summary>
    /// 発射方向（正規化済み）.
    /// </summary>
    public Vector2 Direction;

    /// <summary>
    /// 速度.
    /// </summary>
    public float Speed;

    /// <summary>
    /// 経過時間.
    /// </summary>
    public float ElapsedTime;

    /// <summary>
    /// 生存時間.
    /// </summary>
    public float LifeTime;

    /// <summary>
    /// 貫通残数（default: 0 = 貫通なし）.
    /// </summary>
    public int PenetrationCount;

    /// <summary>
    /// 地形貫通（default: false = 地形で消滅）.
    /// </summary>
    public bool IgnoreTerrain;

    /// <summary>
    /// 発射元Presenter（自身を無視用）.
    /// </summary>
    public CharacterPresenterBase OwnerPresenter;

    /// <summary>
    /// 既にヒットした対象（重複ヒット防止）.
    /// </summary>
    public HashSet<IStatus_base> HitTargets;

    /// <summary>
    /// 追加移動計算関数（default: null → Vector2.zero）.
    /// 引数: (Direction, ElapsedTime) → 追加移動量.
    /// </summary>
    public Func<Vector2, float, Vector2> AdditionalMovement;

    /// <summary>
    /// 攻撃対象ContactTypeリスト.
    /// </summary>
    public List<ContactType> TargetContactTypes;

    /// <summary>
    /// 形状の角数.
    /// </summary>
    public int ShapeAngular;

    /// <summary>
    /// 形状のサイズ.
    /// </summary>
    public float ShapeSize;

    /// <summary>
    /// 回転角度（ラジアン）.
    /// </summary>
    public float Rotation;

    /// <summary>
    /// 色.
    /// </summary>
    public Color Color;

    /// <summary>
    /// 作成時のフレーム番号（切替追跡用）.
    /// </summary>
    public int CreatedFrame;

    /// <summary>
    /// フォールバック描画を使用するか（形状変更時の一時フラグ）.
    /// </summary>
    public bool UseFallback;

    /// <summary>
    /// 現在位置を計算して返す.
    /// Position = StartPosition + (Direction * (Speed * ElapsedTime)) + AdditionalMovement(Direction, ElapsedTime)
    /// </summary>
    public Vector2 CalculatePosition()
    {
        Vector2 baseMovement = Direction * (Speed * ElapsedTime);
        Vector2 additionalMovement = AdditionalMovement != null ? AdditionalMovement(Direction, ElapsedTime) : Vector2.zero;
        return StartPosition + baseMovement + additionalMovement;
    }

    /// <summary>
    /// デフォルト値で初期化した新しいAttackEntityDataを作成.
    /// </summary>
    public static AttackEntityData Create(
        Vector2 startPosition,
        Vector2 direction,
        float speed,
        float lifeTime,
        CharacterPresenterBase owner,
        List<ContactType> targetContactTypes,
        int shapeAngular = 3,
        float shapeSize = 0.5f,
        int penetrationCount = 0,
        bool ignoreTerrain = false,
        Func<Vector2, float, Vector2> additionalMovement = null,
        Color? color = null,
        float initialRotation = 0f)
    {
        return new AttackEntityData
        {
            IsActive = true,
            StartPosition = startPosition,
            Position = startPosition,
            PreviousPosition = startPosition,
            Direction = direction.normalized,
            Speed = speed,
            ElapsedTime = 0f,
            LifeTime = lifeTime,
            PenetrationCount = penetrationCount,
            IgnoreTerrain = ignoreTerrain,
            OwnerPresenter = owner,
            HitTargets = new HashSet<IStatus_base>(),
            AdditionalMovement = additionalMovement,
            TargetContactTypes = targetContactTypes ?? new List<ContactType>(),
            ShapeAngular = shapeAngular,
            ShapeSize = shapeSize,
            Rotation = initialRotation,
            Color = color ?? Color.white,
            CreatedFrame = Time.frameCount,
            UseFallback = false
        };
    }
}
