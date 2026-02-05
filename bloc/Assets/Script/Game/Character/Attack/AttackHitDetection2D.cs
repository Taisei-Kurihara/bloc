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
    /// 頂点配列プール（角数別）- 前フレーム用.
    /// </summary>
    private static readonly Dictionary<int, Vector2[]> _prevVerticesPool = new Dictionary<int, Vector2[]>();

    /// <summary>
    /// 頂点配列プール（角数別）- 現フレーム用.
    /// </summary>
    private static readonly Dictionary<int, Vector2[]> _currVerticesPool = new Dictionary<int, Vector2[]>();

    /// <summary>
    /// GetComponent結果キャッシュ.
    /// </summary>
    private static readonly Dictionary<Collider2D, IStatusProvider> _statusProviderCache = new Dictionary<Collider2D, IStatusProvider>();

    /// <summary>
    /// キャッシュクリア用フレームカウント.
    /// </summary>
    private static int _lastCacheClearFrame = -1;

    /// <summary>
    /// キャッシュクリア間隔（フレーム数）.
    /// </summary>
    private const int CACHE_CLEAR_INTERVAL = 300;

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
    /// 代表点（中心＋対角頂点）のみRaycastを行う（軽量化）.
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

        // 移動距離計算.
        Vector2 centerDir = data.Position - data.PreviousPosition;
        float centerDist = centerDir.magnitude;

        // 移動がほぼない場合はスキップ.
        if (centerDist < 0.001f) return result;

        centerDir /= centerDist;

        // 1. 中心点のRaycast（最優先）.
        RaycastHit2D centerHit = Physics2D.Raycast(data.PreviousPosition, centerDir, centerDist);
        if (centerHit.collider != null)
        {
            HitResult hitResult = ProcessHit(ref data, centerHit);
            if (hitResult.HasHit) return hitResult;
        }

        // 2. 代表頂点のRaycast（対角2点のみ）.
        Vector2[] prevVertices = GetWorldVerticesForHit(
            data.ShapeAngular, data.PreviousPosition, data.ShapeSize, data.Rotation, true);
        Vector2[] currVertices = GetWorldVerticesForHit(
            data.ShapeAngular, data.Position, data.ShapeSize, data.Rotation, false);

        int vertexCount = prevVertices.Length;
        if (vertexCount > 0)
        {
            // 先頭頂点.
            HitResult vertexResult = CheckVertexHit(ref data, prevVertices[0], currVertices[0]);
            if (vertexResult.HasHit) return vertexResult;

            // 対角頂点（頂点数の半分位置）.
            if (vertexCount >= 3)
            {
                int oppositeIdx = vertexCount / 2;
                vertexResult = CheckVertexHit(ref data, prevVertices[oppositeIdx], currVertices[oppositeIdx]);
                if (vertexResult.HasHit) return vertexResult;
            }
        }

        return result;
    }

    /// <summary>
    /// 頂点間のRaycastを実行.
    /// </summary>
    private static HitResult CheckVertexHit(ref AttackEntityData data, Vector2 prevVertex, Vector2 currVertex)
    {
        Vector2 direction = currVertex - prevVertex;
        float distance = direction.magnitude;

        if (distance < 0.001f) return new HitResult();

        direction /= distance;

        RaycastHit2D hit = Physics2D.Raycast(prevVertex, direction, distance);
        if (hit.collider != null)
        {
            return ProcessHit(ref data, hit);
        }

        return new HitResult();
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

        // キャラクター判定（キャッシュ使用）.
        IStatusProvider statusProvider = GetCachedStatusProvider(hit.collider);

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
    /// ワールド座標での頂点位置を取得（ShapeManagerから取得、配列プール使用）.
    /// </summary>
    private static Vector2[] GetWorldVerticesForHit(int angular, Vector2 position, float size, float rotation, bool isPrev)
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
            localVertices = AttackMesh2DGenerator.GetVertices(angular);
        }

        // 配列プールから取得または作成.
        var pool = isPrev ? _prevVerticesPool : _currVerticesPool;
        if (!pool.TryGetValue(angular, out Vector2[] worldVertices) || worldVertices.Length != localVertices.Length)
        {
            worldVertices = new Vector2[localVertices.Length];
            pool[angular] = worldVertices;
        }

        // ワールド座標に変換（配列再利用）.
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

    /// <summary>
    /// StatusProviderをキャッシュから取得または検索.
    /// </summary>
    private static IStatusProvider GetCachedStatusProvider(Collider2D collider)
    {
        if (collider == null) return null;

        // 定期的にキャッシュをクリア（破棄されたオブジェクト対策）.
        int currentFrame = Time.frameCount;
        if (currentFrame - _lastCacheClearFrame > CACHE_CLEAR_INTERVAL)
        {
            _statusProviderCache.Clear();
            _lastCacheClearFrame = currentFrame;
        }

        // キャッシュから取得.
        if (_statusProviderCache.TryGetValue(collider, out IStatusProvider cached))
        {
            return cached;
        }

        // 検索してキャッシュに保存.
        IStatusProvider provider = collider.GetComponent<IStatusProvider>();
        if (provider == null)
        {
            provider = collider.GetComponentInParent<IStatusProvider>();
        }

        _statusProviderCache[collider] = provider;
        return provider;
    }

    /// <summary>
    /// キャッシュをクリア（シーン切替時などに呼び出し）.
    /// </summary>
    public static void ClearCache()
    {
        _statusProviderCache.Clear();
        _prevVerticesPool.Clear();
        _currVerticesPool.Clear();
        _lastCacheClearFrame = -1;
    }
}
