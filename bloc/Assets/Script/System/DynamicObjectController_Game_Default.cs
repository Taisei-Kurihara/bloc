using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DynamicObjectController_Game_Default : Singleton_DestroyAvailableMonoSingleton<DynamicObjectController_Game_Default>, DynamicObjectController_interface
{
    GameObject Player_Prefab;
    GameObject Player_Instance;

    public GameObject Attack_Prefab { get; private set; }
    public GameObject GetPlayer_Instance => Player_Instance;

    // AttackEntityAdvent型ごとの攻撃オブジェプール（未使用/使用中）.
    private Dictionary<Type, Queue<CharacterPresenter_D_AttackEntity_abstract>> _attackPoolsByType_Unused = new Dictionary<Type, Queue<CharacterPresenter_D_AttackEntity_abstract>>();
    private Dictionary<Type, List<CharacterPresenter_D_AttackEntity_abstract>> _attackPoolsByType_InUse = new Dictionary<Type, List<CharacterPresenter_D_AttackEntity_abstract>>();

    // プール設定情報を保持.
    private Dictionary<Type, AttackPoolSettings> _poolSettings = new Dictionary<Type, AttackPoolSettings>();

    // 攻撃オブジェ用Listのlength取得用.
    public int GetAttackPool_UnusedCount(Type adventType) => _attackPoolsByType_Unused.TryGetValue(adventType, out var queue) ? queue.Count : 0;
    public int GetAttackPool_InUseCount(Type adventType) => _attackPoolsByType_InUse.TryGetValue(adventType, out var list) ? list.Count : 0;
    public int GetAttackPool_TotalCount(Type adventType) => GetAttackPool_UnusedCount(adventType) + GetAttackPool_InUseCount(adventType);

    // 全体のカウント取得用.
    public int AttackPool_UnusedCount
    {
        get
        {
            int count = 0;
            foreach (var queue in _attackPoolsByType_Unused.Values)
                count += queue.Count;
            return count;
        }
    }
    public int AttackPool_InUseCount
    {
        get
        {
            int count = 0;
            foreach (var list in _attackPoolsByType_InUse.Values)
                count += list.Count;
            return count;
        }
    }
    public int AttackPool_TotalCount => AttackPool_UnusedCount + AttackPool_InUseCount;

    // 非同期読み込み設定.
    private const int LOAD_PER_SECOND = 16;
    private float _loadIntervalSeconds;
    private HashSet<Type> _isLoadingAttackPool = new HashSet<Type>();

    public SceneControllerType ControllerType => SceneControllerType.InGame;
    public List<DynamicObjectController_interface> ChildControllers => new List<DynamicObjectController_interface>();
    public List<AddressableAssetAddress> LoadedAssetAddresses => new List<AddressableAssetAddress> { AddressableAssetAddress.Player };

    public async UniTask OnBeforeFadeLoadAsync()
    {

    }

    public async UniTask OnSceneLoadedAsync()
    {
        Debug.Log("[Game_Default] OnBeforeFadeLoadAsync: Player プレハブロード開始");
        // Player プレハブロード.
        Player_Prefab = await DynamicObjectManager.Instance().LoadAssetAsync<GameObject>("Player");
        if (Player_Prefab == null)
        {
            Debug.LogError("[Game_Default] Player がロードできません");
            return;
        }
        else
        {
            Debug.Log("[Game_Default] OnBeforeFadeLoadAsync: Player プレハブロード完了");
        }

        try
        {
            Attack_Prefab = await DynamicObjectManager.Instance().LoadAssetAsync<GameObject>("Attack");
            if (Attack_Prefab == null)
            {
                Debug.LogWarning("[Game_Default] Attack がロードできません（Addressablesに登録されていない可能性があります）");
            }
            else
            {
                Debug.Log("[Game_Default] OnSceneLoadedAsync: Attack プレハブロード完了");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Game_Default] Attack ロード失敗（未登録の可能性）: {e.Message}");
        }
    }

    public async UniTask OnFadeCompletedAsync()
    {
        Debug.Log("[Game_Default] OnFadeCompletedAsync: Player_Prefab待機開始");
        await UniTask.WaitUntil(() => Player_Prefab != null);
        Debug.Log("[Game_Default] OnFadeCompletedAsync: Player_Prefab待機完了");
        // Player をシーンに生成.
        Player_Instance = GameObject.Instantiate(Player_Prefab, new Vector3(0f, -2.5f, 0f), Quaternion.identity);
        Debug.Log("[Game_Default] OnFadeCompletedAsync: Player生成完了");
    }

    /// <summary>
    /// 攻撃プールを登録する.
    /// AttackEntityAdvent型 + Initialize設定を渡してプールを作成.
    /// </summary>
    /// <param name="adventType">AttackEntityAdvent_abstractの継承型.</param>
    /// <param name="settings">プール設定.</param>
    public void RegisterAttackPool(Type adventType, AttackPoolSettings settings)
    {
        if (adventType == null || settings == null)
        {
            Debug.LogWarning("[Game_Default] RegisterAttackPool: adventType または settings がnull.");
            return;
        }

        if (!typeof(AttackEntityAdvent_abstract).IsAssignableFrom(adventType))
        {
            Debug.LogWarning($"[Game_Default] RegisterAttackPool: {adventType.Name} は AttackEntityAdvent_abstract を継承していません.");
            return;
        }

        // 既に登録済みの場合は設定を更新.
        _poolSettings[adventType] = settings;

        // プールがない場合は作成.
        if (!_attackPoolsByType_Unused.ContainsKey(adventType))
        {
            _attackPoolsByType_Unused[adventType] = new Queue<CharacterPresenter_D_AttackEntity_abstract>();
            _attackPoolsByType_InUse[adventType] = new List<CharacterPresenter_D_AttackEntity_abstract>();
        }

        Debug.Log($"[Game_Default] 攻撃プール登録: {adventType.Name}, 初期生成数: {settings.InitialCount}, 最大数: {settings.MaxCount}");

        // 非同期で初期生成開始.
        StartAttackPoolLoadingAsync(adventType).Forget();
    }

    public async UniTask OnSceneUnloadedAsync()
    {
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 攻撃オブジェ非同期読み込み繰り返し関数（型指定版）.
    /// </summary>
    /// <param name="adventType">AttackEntityAdvent_abstractの継承型.</param>
    /// <param name="loadPerSecond">秒間読み込み数.</param>
    public async UniTask StartAttackPoolLoadingAsync(Type adventType, int loadPerSecond = LOAD_PER_SECOND)
    {
        if (_isLoadingAttackPool.Contains(adventType)) return;
        if (Attack_Prefab == null)
        {
            Debug.LogWarning("[Game_Default] Attack_Prefab がnullのため読み込みをスキップ.");
            return;
        }

        if (!_poolSettings.TryGetValue(adventType, out var settings))
        {
            Debug.LogWarning($"[Game_Default] {adventType.Name} のプール設定が見つかりません.");
            return;
        }

        _isLoadingAttackPool.Add(adventType);
        _loadIntervalSeconds = 1f / loadPerSecond;

        int targetCount = settings.InitialCount;
        int maxCount = settings.MaxCount;
        int currentCount = GetAttackPool_TotalCount(adventType);

        while (currentCount < targetCount && currentCount < maxCount)
        {
            LoadAttackObject(adventType, settings);
            currentCount = GetAttackPool_TotalCount(adventType);
            await UniTask.Delay((int)(_loadIntervalSeconds * 1000));
        }

        _isLoadingAttackPool.Remove(adventType);
        Debug.Log($"[Game_Default] 攻撃オブジェクトプール読み込み完了. 型: {adventType.Name}, 総数: {GetAttackPool_TotalCount(adventType)}");
    }

    /// <summary>
    /// 読み込み用関数（内部用、型指定版）.
    /// </summary>
    /// <param name="adventType">AttackEntityAdvent_abstractの継承型.</param>
    /// <param name="settings">プール設定.</param>
    private void LoadAttackObject(Type adventType, AttackPoolSettings settings)
    {
        GameObject attackObj = GameObject.Instantiate(Attack_Prefab);
        attackObj.SetActive(false);
        var attackEntity = attackObj.GetComponent<CharacterPresenter_D_AttackEntity_abstract>();
        if (attackEntity != null)
        {
            attackEntity.IsInUse = false;
            attackEntity.PoolAdventType = adventType;

            // プールに追加.
            if (!_attackPoolsByType_Unused.ContainsKey(adventType))
            {
                _attackPoolsByType_Unused[adventType] = new Queue<CharacterPresenter_D_AttackEntity_abstract>();
                _attackPoolsByType_InUse[adventType] = new List<CharacterPresenter_D_AttackEntity_abstract>();
            }
            _attackPoolsByType_Unused[adventType].Enqueue(attackEntity);
        }
        else
        {
            Debug.LogWarning("[Game_Default] Attack_Prefab に CharacterPresenter_D_AttackEntity_abstract がアタッチされていません.");
            GameObject.Destroy(attackObj);
        }
    }

    /// <summary>
    /// 一括でプールのCharacterPresenter_D_AttackEntity_abstract.Initializeを実行する.
    /// </summary>
    /// <param name="adventType">AttackEntityAdvent_abstractの継承型.</param>
    public void InitializePoolAttackEntities(Type adventType)
    {
        if (!_poolSettings.TryGetValue(adventType, out var settings))
        {
            Debug.LogWarning($"[Game_Default] {adventType.Name} のプール設定が見つかりません.");
            return;
        }

        if (!_attackPoolsByType_Unused.TryGetValue(adventType, out var unusedPool))
        {
            Debug.LogWarning($"[Game_Default] {adventType.Name} の未使用プールが見つかりません.");
            return;
        }

        int initializedCount = 0;
        foreach (var entity in unusedPool)
        {
            if (!entity.IsInitialized)
            {
                entity.Initialize(
                    settings.ShapeComp,
                    settings.HitTargets,
                    settings.Move,
                    settings.Shape,
                    settings.Status,
                    settings.Attack,
                    settings.AdventFactory
                );
                initializedCount++;
            }
        }

        Debug.Log($"[Game_Default] プール一括初期化完了. 型: {adventType.Name}, 初期化数: {initializedCount}");
    }

    /// <summary>
    /// 攻撃を行うための関数（型指定版）.
    /// </summary>
    /// <param name="adventType">AttackEntityAdvent_abstractの継承型.</param>
    /// <param name="count">必要な攻撃オブジェクト数.</param>
    /// <returns>取得した攻撃オブジェクトのリスト.</returns>
    public List<CharacterPresenter_D_AttackEntity_abstract> RequestAttackObjects(Type adventType, int count)
    {
        List<CharacterPresenter_D_AttackEntity_abstract> result = new List<CharacterPresenter_D_AttackEntity_abstract>();

        if (!_attackPoolsByType_Unused.TryGetValue(adventType, out var unusedPool))
        {
            Debug.LogWarning($"[Game_Default] {adventType.Name} の未使用プールが見つかりません.");
            return result;
        }

        if (!_attackPoolsByType_InUse.TryGetValue(adventType, out var inUseList))
        {
            _attackPoolsByType_InUse[adventType] = new List<CharacterPresenter_D_AttackEntity_abstract>();
            inUseList = _attackPoolsByType_InUse[adventType];
        }

        if (!_poolSettings.TryGetValue(adventType, out var settings))
        {
            Debug.LogWarning($"[Game_Default] {adventType.Name} のプール設定が見つかりません.");
            return result;
        }

        for (int i = 0; i < count; i++)
        {
            // プールが足りない場合は生成処理を呼ぶ.
            if (unusedPool.Count == 0)
            {
                if (Attack_Prefab == null)
                {
                    Debug.LogWarning("[Game_Default] Attack_Prefab がnullのため生成できません.");
                    break;
                }
                LoadAttackObject(adventType, settings);
            }

            if (unusedPool.Count == 0)
            {
                Debug.LogWarning($"[Game_Default] {adventType.Name} のプールが空です.");
                break;
            }

            var attackEntity = unusedPool.Dequeue();
            attackEntity.IsInUse = true;
            inUseList.Add(attackEntity);
            attackEntity.gameObject.SetActive(true);
            result.Add(attackEntity);
        }

        // 各攻撃オブジェクトの初期化と発射処理を呼び出す.
        foreach (var entity in result)
        {
            // 未初期化の場合は初期化を実行.
            if (!entity.IsInitialized)
            {
                entity.Initialize(
                    settings.ShapeComp,
                    settings.HitTargets,
                    settings.Move,
                    settings.Shape,
                    settings.Status,
                    settings.Attack,
                    settings.AdventFactory
                );
            }
            entity.Fire();
        }

        return result;
    }

    /// <summary>
    /// 攻撃オブジェクトを未使用プールに戻す.
    /// </summary>
    public void ReturnAttackObject(CharacterPresenter_D_AttackEntity_abstract attackEntity)
    {
        if (attackEntity == null) return;

        Type adventType = attackEntity.PoolAdventType;
        if (adventType == null)
        {
            Debug.LogWarning("[Game_Default] ReturnAttackObject: PoolAdventType がnullです.");
            return;
        }

        attackEntity.IsInUse = false;
        attackEntity.gameObject.SetActive(false);

        if (_attackPoolsByType_InUse.TryGetValue(adventType, out var inUseList))
        {
            inUseList.Remove(attackEntity);
        }

        if (!_attackPoolsByType_Unused.TryGetValue(adventType, out var unusedPool))
        {
            _attackPoolsByType_Unused[adventType] = new Queue<CharacterPresenter_D_AttackEntity_abstract>();
            unusedPool = _attackPoolsByType_Unused[adventType];
        }

        unusedPool.Enqueue(attackEntity);
    }
}

/// <summary>
/// 攻撃プール設定クラス.
/// </summary>
public class AttackPoolSettings
{
    // 初期生成数.
    public int InitialCount { get; set; } = 100;
    // 最大数.
    public int MaxCount { get; set; } = 1000;

    // CharacterPresenter_D_AttackEntity_abstract.Initialize用の設定.
    public ShapeCompStatus ShapeComp { get; set; }
    public List<ContactType> HitTargets { get; set; }
    public Move_AttackEntity_abstract Move { get; set; }
    public Shape_Model_AttackEntity_abstract Shape { get; set; }
    public Status_AttackEntity_abstract Status { get; set; }
    public Hit_AttackEntity_abstract Attack { get; set; }
    public Func<CharacterPresenterBase, AttackEntityAdvent_abstract> AdventFactory { get; set; }
}
