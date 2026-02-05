using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 攻撃描画システム（DrawMesh方式）.
/// </summary>
public class AttackInstancedRenderer : MonoBehaviour
{
    #region Singleton

    private static AttackInstancedRenderer _instance;

    public static AttackInstancedRenderer Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("AttackInstancedRenderer");
                _instance = go.AddComponent<AttackInstancedRenderer>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public static bool HasInstance => _instance != null;

    #endregion

    #region Settings

    [SerializeField]
    private int _maxEntities = 5000;

    [SerializeField]
    private bool _showDebugLines = false;

    // ShapeManagerで初期生成される角形の範囲.
    private const int PRECACHE_MIN_ANGULAR = 3;
    private const int PRECACHE_MAX_ANGULAR = 9;

    #endregion

    #region Rendering

    private Material _material;
    private MaterialPropertyBlock _propertyBlock;

    // 角数ごとのメッシュキャッシュ.
    private Dictionary<int, Mesh> _meshCache = new Dictionary<int, Mesh>();

    #endregion

    #region Entity Data

    private AttackEntityData[] _entities;
    private int _activeCount = 0;
    private Queue<int> _freeIndices;

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

        ReleaseResources();
    }

    private void Update()
    {
        UpdateEntities(Time.deltaTime);
        Render();
        if (_showDebugLines) DrawDebugLines();
    }

    #endregion

    #region Initialization

    private void Initialize()
    {
        _entities = new AttackEntityData[_maxEntities];
        _freeIndices = new Queue<int>(_maxEntities);

        for (int i = 0; i < _maxEntities; i++)
        {
            _freeIndices.Enqueue(i);
        }

        // マテリアル作成.
        _material = new Material(Shader.Find("Sprites/Default"));
        _propertyBlock = new MaterialPropertyBlock();

        // メッシュを事前キャッシュ.
        PreCacheMeshesAsync().Forget();
    }

    /// <summary>
    /// ShapeManagerのキャッシュ済み形状を事前にメッシュ化.
    /// </summary>
    private async UniTaskVoid PreCacheMeshesAsync()
    {
        await UniTask.Yield();

        var shapeManager = ShapeUnitCirclePolygonManager.Instance();

        for (int angular = PRECACHE_MIN_ANGULAR; angular <= PRECACHE_MAX_ANGULAR; angular++)
        {
            if (_meshCache.ContainsKey(angular)) continue;

            Mesh mesh = null;

            if (shapeManager != null)
            {
                var shapeComp = shapeManager.GetShapeCompStatus(angular);
                if (shapeComp != null && shapeComp.status != null)
                {
                    mesh = CreateMeshFromShapeStatus(shapeComp.status, angular);
                }
            }

            if (mesh == null)
            {
                mesh = CreatePolygonMesh(angular);
            }

            _meshCache[angular] = mesh;

            await UniTask.Yield();
        }
    }

    private void ReleaseResources()
    {
        if (_material != null)
        {
            Destroy(_material);
        }

        foreach (var mesh in _meshCache.Values)
        {
            if (mesh != null) Destroy(mesh);
        }
        _meshCache.Clear();
    }

    #endregion

    #region Public API

    public bool ShowDebugLines
    {
        get => _showDebugLines;
        set => _showDebugLines = value;
    }

    public int AddAttack(AttackEntityData data)
    {
        if (_freeIndices.Count == 0)
        {
            return -1;
        }

        int index = _freeIndices.Dequeue();
        data.IsActive = true;
        data.HitTargets = data.HitTargets ?? new HashSet<IStatus_base>();
        _entities[index] = data;
        _activeCount++;

        // 使用した角形をキャッシュ.
        EnsureMeshCached(data.ShapeAngular);

        return index;
    }

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

    public void RemoveAttack(int index)
    {
        if (index < 0 || index >= _maxEntities) return;
        if (!_entities[index].IsActive) return;

        _entities[index].IsActive = false;
        _entities[index].HitTargets = null;
        _freeIndices.Enqueue(index);
        _activeCount--;
    }

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

    public int ActiveCount => _activeCount;
    public int MaxEntities => _maxEntities;

    #endregion

    #region Mesh Caching

    private void EnsureMeshCached(int angular)
    {
        if (_meshCache.ContainsKey(angular)) return;

        Mesh mesh = null;

        var shapeManager = ShapeUnitCirclePolygonManager.Instance();
        if (shapeManager != null)
        {
            var shapeComp = shapeManager.GetShapeCompStatus(angular);
            if (shapeComp != null && shapeComp.status != null)
            {
                mesh = CreateMeshFromShapeStatus(shapeComp.status, angular);
            }
        }

        if (mesh == null)
        {
            mesh = CreatePolygonMesh(angular);
        }

        _meshCache[angular] = mesh;
    }

    private Mesh GetCachedMesh(int angular)
    {
        if (_meshCache.TryGetValue(angular, out Mesh cached))
        {
            return cached;
        }

        EnsureMeshCached(angular);
        return _meshCache.TryGetValue(angular, out Mesh mesh) ? mesh : null;
    }

    private Mesh CreateMeshFromShapeStatus(ShapeStatus status, int angular)
    {
        Mesh mesh = new Mesh();
        mesh.name = $"AttackMesh_{angular}";

        int vertexCount = status.vertices.Length;
        Vector3[] vertices3D = new Vector3[vertexCount + 1];
        Vector2[] uvs = new Vector2[vertexCount + 1];

        vertices3D[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);

        for (int i = 0; i < vertexCount; i++)
        {
            Vector2 v = status.vertices[i];
            vertices3D[i + 1] = new Vector3(v.x, v.y, 0f);
            uvs[i + 1] = new Vector2((v.x + 1f) * 0.5f, (v.y + 1f) * 0.5f);
        }

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
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private Mesh CreatePolygonMesh(int angular)
    {
        Mesh mesh = new Mesh();
        mesh.name = $"AttackMesh_Polygon_{angular}";

        Vector3[] vertices = new Vector3[angular + 1];
        Vector2[] uvs = new Vector2[angular + 1];

        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);

        float flatDownOffset = (angular % 2 != 0) ? Mathf.PI / 2f :
            ((angular - 2 > 3 && (angular - 2) % 4 == 0) ? 0f : Mathf.PI / angular);

        for (int i = 0; i < angular; i++)
        {
            float angle = ((i * Mathf.PI * 2f) / angular) + flatDownOffset;
            float x = Mathf.Cos(angle);
            float y = Mathf.Sin(angle);
            vertices[i + 1] = new Vector3(x, y, 0f);
            uvs[i + 1] = new Vector2((x + 1f) * 0.5f, (y + 1f) * 0.5f);
        }

        int[] triangles = new int[angular * 3];
        for (int i = 0; i < angular; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % angular + 1;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    #endregion

    #region Update Logic

    private void UpdateEntities(float deltaTime)
    {
        for (int i = 0; i < _maxEntities; i++)
        {
            if (!_entities[i].IsActive) continue;

            ref AttackEntityData entity = ref _entities[i];

            entity.PreviousPosition = entity.Position;
            entity.ElapsedTime += deltaTime;

            if (entity.ElapsedTime >= entity.LifeTime)
            {
                RemoveAttack(i);
                continue;
            }

            entity.Position = entity.CalculatePosition();

            var hitResult = AttackHitDetection2D.CheckHit(ref entity);

            if (hitResult.HasHit)
            {
                if (hitResult.HitStatus != null)
                {
                    OnHit?.Invoke(i, hitResult.HitStatus);
                }

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

    private void Render()
    {
        if (_material == null) return;
        if (_activeCount == 0) return;

        for (int i = 0; i < _maxEntities; i++)
        {
            if (!_entities[i].IsActive) continue;

            ref AttackEntityData entity = ref _entities[i];

            Mesh mesh = GetCachedMesh(entity.ShapeAngular);
            if (mesh == null) continue;

            Matrix4x4 matrix = Matrix4x4.TRS(
                new Vector3(entity.Position.x, entity.Position.y, 0f),
                Quaternion.Euler(0f, 0f, entity.Rotation * Mathf.Rad2Deg),
                new Vector3(entity.ShapeSize, entity.ShapeSize, 1f)
            );

            _propertyBlock.SetColor("_Color", entity.Color);

            Graphics.DrawMesh(mesh, matrix, _material, 0, null, 0, _propertyBlock);
        }
    }

    #endregion

    #region Debug

    private void DrawDebugLines()
    {
        for (int i = 0; i < _maxEntities; i++)
        {
            if (!_entities[i].IsActive) continue;

            ref AttackEntityData entity = ref _entities[i];

            Vector2 dirEnd = entity.Position + entity.Direction * entity.ShapeSize * 2f;
            Debug.DrawLine(entity.Position, dirEnd, Color.blue, 0f);
            Debug.DrawLine(entity.PreviousPosition, entity.Position, Color.green, 0f);
        }
    }

    #endregion
}
