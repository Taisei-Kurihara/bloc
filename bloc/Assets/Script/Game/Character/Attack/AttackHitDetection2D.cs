using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Raycast当たり判定クラス.
/// 頂点間のRaycastで高速な当たり判定を実現.
/// </summary>
public static class AttackHitDetection2D
{
    /// <summary>
    /// 地形レイヤーマスク（Wall, Ground等）.
    /// </summary>
    private static int _terrainLayerMask = -1;

    /// <summary>
    /// デフォルトレイヤーマスク.
    /// </summary>
    private static int _defaultLayerMask = -1;

    /// <summary>
    /// レイヤーマスク初期化.
    /// </summary>
    private static void InitializeLayerMasks()
    {
        if (_terrainLayerMask == -1)
        {
            _terrainLayerMask = LayerMask.GetMask("Wall", "Ground", "Terrain");
            if (_terrainLayerMask == 0)
            {
                // レイヤーが存在しない場合はDefaultを使用.
                _terrainLayerMask = LayerMask.GetMask("Default");
            }
        }

        if (_defaultLayerMask == -1)
        {
            _defaultLayerMask = LayerMask.GetMask("Default");
        }
    }

    /// <summary>
    /// 当たり判定結果.
    /// </summary>
    public struct HitResult
    {
        /// <summary>
        /// 何かにヒットしたか.
        /// </summary>
        public bool HasHit;

        /// <summary>
        /// 地形にヒットしたか.
        /// </summary>
        public bool HitTerrain;

        /// <summary>
        /// ヒットしたStatus（キャラクター）.
        /// </summary>
        public IStatus_base HitStatus;

        /// <summary>
        /// ヒット位置.
        /// </summary>
        public Vector2 HitPosition;

        /// <summary>
        /// ヒットしたコライダー.
        /// </summary>
        public Collider2D HitCollider;
    }

    /// <summary>
    /// 攻撃エンティティの当たり判定を実行.
    /// 各頂点について前フレーム→現在フレームのRaycastを行う.
    /// </summary>
    /// <param name="data">攻撃エンティティデータ.</param>
    /// <returns>ヒット結果.</returns>
    public static HitResult CheckHit(ref AttackEntityData data)
    {
        InitializeLayerMasks();

        HitResult result = new HitResult
        {
            HasHit = false,
            HitTerrain = false,
            HitStatus = null,
            HitPosition = Vector2.zero,
            HitCollider = null
        };

        // 前フレームと現在フレームの頂点位置を計算.
        Vector2[] prevVertices = GetWorldVerticesForHit(
            data.ShapeAngular, data.PreviousPosition, data.ShapeSize, data.Rotation);
        Vector2[] currVertices = GetWorldVerticesForHit(
            data.ShapeAngular, data.Position, data.ShapeSize, data.Rotation);

        // 各頂点についてRaycast.
        for (int i = 0; i < prevVertices.Length; i++)
        {
            Vector2 prevVertex = prevVertices[i];
            Vector2 currVertex = currVertices[i];
            Vector2 direction = currVertex - prevVertex;
            float distance = direction.magnitude;

            if (distance < 0.001f) continue;

            direction.Normalize();

            // Raycast実行（全レイヤー対象）.
            RaycastHit2D hit = Physics2D.Raycast(prevVertex, direction, distance);

            if (hit.collider != null)
            {
                // ヒット判定処理.
                HitResult hitResult = ProcessHit(ref data, hit);
                if (hitResult.HasHit)
                {
                    return hitResult;
                }
            }
        }

        // 中心点のRaycastも実行（小さいオブジェクトのすり抜け防止）.
        {
            Vector2 direction = data.Position - data.PreviousPosition;
            float distance = direction.magnitude;

            if (distance >= 0.001f)
            {
                direction.Normalize();
                RaycastHit2D hit = Physics2D.Raycast(data.PreviousPosition, direction, distance);

                if (hit.collider != null)
                {
                    HitResult hitResult = ProcessHit(ref data, hit);
                    if (hitResult.HasHit)
                    {
                        return hitResult;
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// ヒット処理.
    /// </summary>
    private static HitResult ProcessHit(ref AttackEntityData data, RaycastHit2D hit)
    {
        HitResult result = new HitResult
        {
            HasHit = false,
            HitTerrain = false,
            HitStatus = null,
            HitPosition = hit.point,
            HitCollider = hit.collider
        };

        // 地形判定.
        int hitLayer = hit.collider.gameObject.layer;
        if (IsTerrainLayer(hitLayer))
        {
            if (!data.IgnoreTerrain)
            {
                result.HasHit = true;
                result.HitTerrain = true;
                return result;
            }
            // 地形貫通の場合はスキップ.
            return result;
        }

        // キャラクター判定.
        IStatusProvider statusProvider = hit.collider.GetComponent<IStatusProvider>();
        if (statusProvider == null)
        {
            statusProvider = hit.collider.GetComponentInParent<IStatusProvider>();
        }

        if (statusProvider != null)
        {
            IStatus_base status = statusProvider.Status;

            // 発射元を無視.
            if (data.OwnerPresenter != null && statusProvider is CharacterPresenterBase presenter)
            {
                if (presenter == data.OwnerPresenter)
                {
                    return result;
                }
            }

            // 既にヒットした対象を無視.
            if (data.HitTargets != null && data.HitTargets.Contains(status))
            {
                return result;
            }

            // ターゲットタイプ判定.
            if (status != null && data.TargetContactTypes != null &&
                data.TargetContactTypes.Contains(status.masterHitstatus))
            {
                result.HasHit = true;
                result.HitStatus = status;
                return result;
            }
        }

        return result;
    }

    /// <summary>
    /// 地形レイヤー判定.
    /// </summary>
    private static bool IsTerrainLayer(int layer)
    {
        InitializeLayerMasks();
        return (_terrainLayerMask & (1 << layer)) != 0;
    }

    /// <summary>
    /// ワールド座標での頂点位置を取得（ShapeManagerから取得）.
    /// </summary>
    private static Vector2[] GetWorldVerticesForHit(int angular, Vector2 position, float size, float rotation)
    {
        // ShapeManagerから頂点データを取得.
        Vector2[] localVertices = null;

        var shapeManager = ShapeUnitCirclePolygonManager.Instance();
        if (shapeManager != null)
        {
            var shapeComp = shapeManager.GetShapeCompStatus(angular);
            if (shapeComp != null && shapeComp.status != null)
            {
                localVertices = shapeComp.status.vertices;
            }
        }

        // ShapeManagerがない場合はAttackMesh2DGeneratorを使用.
        if (localVertices == null)
        {
            return AttackMesh2DGenerator.GetWorldVertices(angular, position, size, rotation);
        }

        // ワールド座標に変換.
        Vector2[] worldVertices = new Vector2[localVertices.Length];
        float cos = Mathf.Cos(rotation);
        float sin = Mathf.Sin(rotation);

        for (int i = 0; i < localVertices.Length; i++)
        {
            Vector2 scaled = localVertices[i] * size;
            float rotatedX = scaled.x * cos - scaled.y * sin;
            float rotatedY = scaled.x * sin + scaled.y * cos;
            worldVertices[i] = position + new Vector2(rotatedX, rotatedY);
        }

        return worldVertices;
    }

    /// <summary>
    /// 貫通処理（ヒット後の処理）.
    /// </summary>
    /// <param name="data">攻撃エンティティデータ.</param>
    /// <param name="hitResult">ヒット結果.</param>
    /// <returns>攻撃を消滅させるべきか.</returns>
    public static bool ProcessPenetration(ref AttackEntityData data, HitResult hitResult)
    {
        if (!hitResult.HasHit) return false;

        // 地形ヒット時は消滅.
        if (hitResult.HitTerrain) return true;

        // キャラクターヒット時.
        if (hitResult.HitStatus != null)
        {
            // ヒット対象を記録.
            data.HitTargets?.Add(hitResult.HitStatus);

            // 貫通残数がある場合.
            if (data.PenetrationCount > 0)
            {
                data.PenetrationCount--;
                return false; // 消滅しない.
            }

            // 貫通なしの場合は消滅.
            return true;
        }

        return false;
    }
}
