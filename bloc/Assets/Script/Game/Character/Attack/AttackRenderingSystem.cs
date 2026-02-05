using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 攻撃描画・更新システム（シングルトン）.
/// GameObjectなしでDrawMesh + Raycastによる攻撃処理を行う.
/// </summary>
public class AttackRenderingSystem : MonoBehaviour
{
    #region Singleton

    private static AttackRenderingSystem _instance;

    /// <summary>
    /// シングルトンインスタンス取得.
    /// </summary>
    public static AttackRenderingSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("AttackRenderingSystem");
                _instance = go.AddComponent<AttackRenderingSystem>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    /// <summary>
    /// インスタンスが存在するか.
    /// </summary>
    public static bool HasInstance => _instance != null;

    #endregion

    #region Settings

    /// <summary>
    /// 最大攻撃エンティティ数.
    /// </summary>
    [SerializeField]
    private int _maxEntities = 5000;

    /// <summary>
    /// デバッグライン表示フラグ.
    /// </summary>
    [SerializeField]
    private bool _showDebugLines = true;

    /// <summary>
    /// 攻撃用デフォルトマテリアル.
    /// </summary>
    private Material _defaultMaterial;

    /// <summary>
    /// MaterialPropertyBlock（色変更用）.
    /// </summary>
    private MaterialPropertyBlock _propertyBlock;

    /// <summary>
    /// 角数ごとのメッシュキャッシュ（ShapeManagerから生成）.
    /// </summary>
    private Dictionary<int, Mesh> _shapeMeshCache = new Dictionary<int, Mesh>();

    #endregion

    #region Data

    /// <summary>
    /// 攻撃エンティティ配列.
    /// </summary>
    private AttackEntityData[] _entities;

    /// <summary>
    /// アクティブなエンティティ数.
    /// </summary>
    private int _activeCount = 0;

    /// <summary>
    /// 空きインデックスキュー.
    /// </summary>
    private Queue<int> _freeIndices;

    /// <summary>
    /// ヒット時コールバック.
    /// </summary>
    public event Action<int, IStatus_base> OnHit;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        Initialize();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }

        // マテリアル解放.
        if (_defaultMaterial != null)
        {
            Destroy(_defaultMaterial);
        }

        // メッシュキャッシュクリア.
        foreach (var mesh in _shapeMeshCache.Values)
        {
            if (mesh != null) Destroy(mesh);
        }
        _shapeMeshCache.Clear();
        AttackMesh2DGenerator.ClearCache();
    }

    private void Update()
    {
        UpdateEntities(Time.deltaTime);
        RenderEntities();
        if (_showDebugLines) DrawDebugLines();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// 初期化.
    /// </summary>
    private void Initialize()
    {
        _entities = new AttackEntityData[_maxEntities];
        _freeIndices = new Queue<int>(_maxEntities);

        for (int i = 0; i < _maxEntities; i++)
        {
            _freeIndices.Enqueue(i);
        }

        // デフォルトマテリアル作成（Sprites/Default使用）.
        _defaultMaterial = new Material(Shader.Find("Sprites/Default"));
        _propertyBlock = new MaterialPropertyBlock();
    }

    #endregion

    #region Public API

    /// <summary>
    /// デバッグライン表示の有効/無効を設定.
    /// </summary>
    public bool ShowDebugLines
    {
        get => _showDebugLines;
        set => _showDebugLines = value;
    }

    /// <summary>
    /// 攻撃エンティティを追加.
    /// </summary>
    /// <returns>追加されたエンティティのインデックス（-1の場合は追加失敗）.</returns>
    public int AddAttack(AttackEntityData data)
    {
        if (_freeIndices.Count == 0)
        {
            Debug.LogWarning("[AttackRenderingSystem] 最大エンティティ数に達しました.");
            return -1;
        }

        int index = _freeIndices.Dequeue();
        data.IsActive = true;
        data.HitTargets = data.HitTargets ?? new HashSet<IStatus_base>();
        _entities[index] = data;
        _activeCount++;

        return index;
    }

    /// <summary>
    /// 攻撃エンティティを追加（パラメータ指定版）.
    /// </summary>
    public int AddAttack(
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
        var data = AttackEntityData.Create(
            startPosition,
            direction,
            speed,
            lifeTime,
            owner,
            targetContactTypes,
            shapeAngular,
            shapeSize,
            penetrationCount,
            ignoreTerrain,
            additionalMovement,
            color,
            initialRotation
        );

        return AddAttack(data);
    }

    /// <summary>
    /// 攻撃エンティティを削除.
    /// </summary>
    public void RemoveAttack(int index)
    {
        if (index < 0 || index >= _maxEntities) return;
        if (!_entities[index].IsActive) return;

        _entities[index].IsActive = false;
        _entities[index].HitTargets = null;
        _freeIndices.Enqueue(index);
        _activeCount--;
    }

    /// <summary>
    /// 全攻撃エンティティを削除.
    /// </summary>
    public void ClearAllAttacks()
    {
        for (int i = 0; i < _maxEntities; i++)
        {
            if (_entities[i].IsActive)
            {
                _entities[i].IsActive = false;
                _entities[i].HitTargets = null;
                _freeIndices.Enqueue(i);
            }
        }
        _activeCount = 0;
    }

    /// <summary>
    /// アクティブなエンティティ数を取得.
    /// </summary>
    public int ActiveCount => _activeCount;

    /// <summary>
    /// 最大エンティティ数を取得.
    /// </summary>
    public int MaxEntities => _maxEntities;

    #endregion

    #region Update Logic

    /// <summary>
    /// エンティティ更新処理.
    /// </summary>
    private void UpdateEntities(float deltaTime)
    {
        for (int i = 0; i < _maxEntities; i++)
        {
            if (!_entities[i].IsActive) continue;

            ref AttackEntityData entity = ref _entities[i];

            // 前フレーム位置を保存.
            entity.PreviousPosition = entity.Position;

            // 時間更新.
            entity.ElapsedTime += deltaTime;

            // 寿命チェック.
            if (entity.ElapsedTime >= entity.LifeTime)
            {
                RemoveAttack(i);
                continue;
            }

            // 位置更新.
            entity.Position = entity.CalculatePosition();

            // 当たり判定.
            var hitResult = AttackHitDetection2D.CheckHit(ref entity);

            if (hitResult.HasHit)
            {
                // ヒットコールバック.
                if (hitResult.HitStatus != null)
                {
                    OnHit?.Invoke(i, hitResult.HitStatus);
                }

                // 貫通処理.
                bool shouldDestroy = AttackHitDetection2D.ProcessPenetration(ref entity, hitResult);
                if (shouldDestroy)
                {
                    RemoveAttack(i);
                }
            }
        }
    }

    #endregion

    #region Rendering

    /// <summary>
    /// ShapeManagerから形状メッシュを取得（キャッシュあり）.
    /// </summary>
    private Mesh GetShapeMesh(int angular)
    {
        if (_shapeMeshCache.TryGetValue(angular, out Mesh cachedMesh))
        {
            return cachedMesh;
        }

        // ShapeUnitCirclePolygonManagerから形状を取得.
        var shapeManager = ShapeUnitCirclePolygonManager.Instance();
        if (shapeManager != null)
        {
            var shapeComp = shapeManager.GetShapeCompStatus(angular);
            if (shapeComp != null && shapeComp.status != null)
            {
                Mesh mesh = CreateMeshFromShapeStatus(shapeComp.status);
                _shapeMeshCache[angular] = mesh;
                return mesh;
            }
        }

        // ShapeManagerが利用できない場合はAttackMesh2DGeneratorを使用.
        return AttackMesh2DGenerator.GetMesh(angular);
    }

    /// <summary>
    /// ShapeStatusからメッシュを生成.
    /// </summary>
    private Mesh CreateMeshFromShapeStatus(ShapeStatus status)
    {
        Mesh mesh = new Mesh();
        mesh.name = $"AttackMesh_Shape_{status.length}";

        int vertexCount = status.vertices.Length;

        // 頂点変換（Vector2[] → Vector3[]）.
        Vector3[] vertices3D = new Vector3[vertexCount + 1];
        Vector2[] uvs = new Vector2[vertexCount + 1];
        Color[] colors = new Color[vertexCount + 1];

        // 中心点.
        vertices3D[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);
        colors[0] = Color.white;

        // 外周頂点.
        for (int i = 0; i < vertexCount; i++)
        {
            Vector2 v = status.vertices[i];
            vertices3D[i + 1] = new Vector3(v.x, v.y, 0f);
            uvs[i + 1] = new Vector2((v.x + 1f) * 0.5f, (v.y + 1f) * 0.5f);
            colors[i + 1] = Color.white;
        }

        // 三角形インデックス生成（ファン形式）.
        int[] triangles = new int[vertexCount * 3];
        for (int i = 0; i < vertexCount; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i % vertexCount) + 2;
            if (triangles[i * 3 + 2] > vertexCount)
                triangles[i * 3 + 2] = 1;
        }

        mesh.vertices = vertices3D;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    /// <summary>
    /// エンティティ描画処理.
    /// </summary>
    private void RenderEntities()
    {
        if (_defaultMaterial == null) return;

        for (int i = 0; i < _maxEntities; i++)
        {
            if (!_entities[i].IsActive) continue;

            ref AttackEntityData entity = ref _entities[i];

            // ShapeManagerからメッシュを取得.
            Mesh mesh = GetShapeMesh(entity.ShapeAngular);

            // 変換行列作成.
            Matrix4x4 matrix = Matrix4x4.TRS(
                new Vector3(entity.Position.x, entity.Position.y, 0f),
                Quaternion.Euler(0f, 0f, entity.Rotation * Mathf.Rad2Deg),
                new Vector3(entity.ShapeSize, entity.ShapeSize, 1f)
            );

            // 色設定.
            _propertyBlock.SetColor("_Color", entity.Color);

            // 描画.
            Graphics.DrawMesh(mesh, matrix, _defaultMaterial, 0, null, 0, _propertyBlock);
        }
    }

    #endregion

    #region Debug

    /// <summary>
    /// Gizmos描画（Sceneビュー用）.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (_entities == null) return;

        for (int i = 0; i < _maxEntities; i++)
        {
            if (!_entities[i].IsActive) continue;

            ref AttackEntityData entity = ref _entities[i];

            // 中心点を描画（黄色の球）.
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(new Vector3(entity.Position.x, entity.Position.y, 0f), entity.ShapeSize * 0.5f);

            // 移動方向を描画（青い線）.
            Gizmos.color = Color.blue;
            Vector3 pos = new Vector3(entity.Position.x, entity.Position.y, 0f);
            Vector3 dirEnd = pos + new Vector3(entity.Direction.x, entity.Direction.y, 0f) * entity.ShapeSize * 2f;
            Gizmos.DrawLine(pos, dirEnd);
        }
    }

    /// <summary>
    /// デバッグライン描画.
    /// </summary>
    private void DrawDebugLines()
    {
        for (int i = 0; i < _maxEntities; i++)
        {
            if (!_entities[i].IsActive) continue;

            ref AttackEntityData entity = ref _entities[i];

            // 頂点位置を取得.
            Vector2[] worldVertices = GetWorldVertices(ref entity);

            // ポリゴンの辺を描画（赤）.
            for (int j = 0; j < worldVertices.Length; j++)
            {
                Vector2 a = worldVertices[j];
                Vector2 b = worldVertices[(j + 1) % worldVertices.Length];
                Debug.DrawLine(a, b, Color.red, 0f);
            }

            // 移動方向を描画（青）.
            Vector2 dirEnd = entity.Position + entity.Direction * entity.ShapeSize * 2f;
            Debug.DrawLine(entity.Position, dirEnd, Color.blue, 0f);

            // 前フレームからの移動線を描画（緑）.
            Debug.DrawLine(entity.PreviousPosition, entity.Position, Color.green, 0f);

            // 中心点を描画（黄）.
            float crossSize = entity.ShapeSize * 0.2f;
            Debug.DrawLine(
                entity.Position + new Vector2(-crossSize, -crossSize),
                entity.Position + new Vector2(crossSize, crossSize),
                Color.yellow, 0f
            );
            Debug.DrawLine(
                entity.Position + new Vector2(-crossSize, crossSize),
                entity.Position + new Vector2(crossSize, -crossSize),
                Color.yellow, 0f
            );
        }
    }

    /// <summary>
    /// ワールド座標での頂点位置を取得.
    /// </summary>
    private Vector2[] GetWorldVertices(ref AttackEntityData entity)
    {
        // ShapeManagerから頂点データを取得.
        Vector2[] localVertices = null;

        var shapeManager = ShapeUnitCirclePolygonManager.Instance();
        if (shapeManager != null)
        {
            var shapeComp = shapeManager.GetShapeCompStatus(entity.ShapeAngular);
            if (shapeComp != null && shapeComp.status != null)
            {
                localVertices = shapeComp.status.vertices;
            }
        }

        // ShapeManagerがない場合はAttackMesh2DGeneratorを使用.
        if (localVertices == null)
        {
            localVertices = AttackMesh2DGenerator.GetVertices(entity.ShapeAngular);
        }

        // ワールド座標に変換.
        Vector2[] worldVertices = new Vector2[localVertices.Length];
        float cos = Mathf.Cos(entity.Rotation);
        float sin = Mathf.Sin(entity.Rotation);

        for (int i = 0; i < localVertices.Length; i++)
        {
            Vector2 scaled = localVertices[i] * entity.ShapeSize;
            float rotatedX = scaled.x * cos - scaled.y * sin;
            float rotatedY = scaled.x * sin + scaled.y * cos;
            worldVertices[i] = entity.Position + new Vector2(rotatedX, rotatedY);
        }

        return worldVertices;
    }

    #endregion
}
