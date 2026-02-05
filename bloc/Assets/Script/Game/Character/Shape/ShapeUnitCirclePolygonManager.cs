using UnityEngine;
using System.Collections.Generic;
using Common;
using Cysharp.Threading.Tasks;
using System;

public class ShapeUnitCirclePolygonManager : Singleton_MonoBehaviourBase<ShapeUnitCirclePolygonManager>
{

    /// <summary>
    /// ShapeGenerator_abstractの型ごとに初期生成する角数の範囲を管理する辞書.
    /// </summary>
    private Dictionary<Type, (int min, int max)> InitialGenerationRangeDict { get; } = new Dictionary<Type, (int min, int max)>
    {
        { typeof(ShapeGenerator_Polygon), (3, 9) }
    };

    /// <summary>
    /// InitialGenerationRangeDictに基づいて初期ShapeStatusを生成する処理.
    /// </summary>
    public async UniTask InitializeShapesAsync()
    {
        foreach (var kvp in InitialGenerationRangeDict)
        {
            Type generatorType = kvp.Key;
            int min = kvp.Value.min;
            int max = kvp.Value.max;

            for (int nAngular = min; nAngular <= max; nAngular++)
            {
                if (!ShapeStatusDict.ContainsKey(nAngular))
                {
                    await CreateShapeStatusAsync(generatorType, nAngular);
                }
            }
        }
    }

    private Dictionary<int, ShapeCompStatus> ShapeStatusDict = new Dictionary<int, ShapeCompStatus>();

    /// <summary>
    /// モデルごとのShape記録情報を管理する辞書.
    /// scene遷移時の保持や素早い形状返却に使用する.
    /// </summary>
    private Dictionary<Shape_Model_abstract, ShapeRecordInfo> ModelShapeRecordDict = new Dictionary<Shape_Model_abstract, ShapeRecordInfo>();

    /// <summary>
    /// プレイヤーが現在設定している形状の角数.
    /// </summary>
    public int PlayerCurrentNAngular { get; private set; } = 4;

    /// <summary>
    /// プレイヤーの形状が設定されたか.
    /// </summary>
    public bool IsPlayerShapeSet { get; private set; } = false;

    /// <summary>
    /// プレイヤーの形状を記録する処理.
    /// </summary>
    /// <param name="nAngular">角数.</param>
    public void SetPlayerShape(int nAngular)
    {
        PlayerCurrentNAngular = nAngular;
        IsPlayerShapeSet = true;
    }

    // ShapeGenerator_abstract型変数.
    public ShapeGenerator_abstract shapeGenerator;

    /// <summary>
    /// ShapeStatusを取得する処理. 存在しない場合は生成して登録する.
    /// </summary>
    /// <param name="generatorType">ShapeGenerator_abstractを継承したクラスの型 (default = ShapeGenerator_Polygon).</param>
    /// <param name="nAngular">角数 (default = 3).</param>
    /// <returns>ShapeStatus.</returns>
    public async UniTask<ShapeStatus> GetOrCreateShapeStatusAsync(Type generatorType = null, int nAngular = 3)
    {
        generatorType = generatorType ?? typeof(ShapeGenerator_Polygon);

        if (InitialGenerationRangeDict.TryGetValue(generatorType, out var range))
        {
            if (nAngular >= range.min && nAngular <= range.max)
            {
                // 初期生成の範囲内なので、初期生成が完了するまで待機.
                await UniTask.WaitUntil(() => ShapeStatusDict.ContainsKey(nAngular));
            }
        }

        if (ShapeStatusDict.TryGetValue(nAngular, out ShapeCompStatus compStatus))
        {
            return compStatus.status;
        }

        // 存在しない場合は生成処理を呼んで待機.
        ShapeStatus newStatus = await CreateShapeStatusAsync(generatorType, nAngular);
        return newStatus;
    }

    /// <summary>
    /// ShapeCompStatusを取得する処理.
    /// </summary>
    /// <param name="nAngular">角数.</param>
    /// <returns>ShapeCompStatus.</returns>
    public ShapeCompStatus GetShapeCompStatus(int nAngular)
    {
        if (ShapeStatusDict.TryGetValue(nAngular, out ShapeCompStatus compStatus))
        {
            return compStatus;
        }
        return null;
    }

    /// <summary>
    /// ShapeStatusを生成する処理.
    /// </summary>
    /// <param name="generatorType">ShapeGenerator_abstractを継承したクラスの型 (default = ShapeGenerator_Polygon).</param>
    /// <param name="nAngular">角数 (default = 3).</param>
    /// <returns>生成されたShapeStatus.</returns>
    public async UniTask<ShapeStatus> CreateShapeStatusAsync(Type generatorType = null, int nAngular = 3)
    {
        generatorType = generatorType ?? typeof(ShapeGenerator_Polygon);

        await UniTask.Yield();

        // ShapeGenerator_abstractの生成処理を呼ぶ.
        ShapeGenerator_abstract generator = (ShapeGenerator_abstract)Activator.CreateInstance(generatorType, nAngular, 100f, true);

        // ShapeCompStatusを作成して辞書に登録.
        ShapeCompStatus compStatus = new ShapeCompStatus(generator);
        ShapeStatusDict[nAngular] = compStatus;

        return compStatus.status;
    }

    /// <summary>
    /// モデルに対してShapeCompStatusを取得または生成し、記録情報を更新する処理.
    /// 既に記録されている場合はそこから素早く取得する.
    /// </summary>
    /// <param name="model">対象のShape_Model_abstract.</param>
    /// <param name="nAngular">角数.</param>
    /// <param name="generatorType">ShapeGenerator_abstractを継承したクラスの型 (default = ShapeGenerator_Polygon).</param>
    /// <param name="persistsAcrossScenes">sceneをまたいで保持される可能性があるか.</param>
    /// <returns>ShapeCompStatus.</returns>
    public async UniTask<ShapeCompStatus> GetOrCreateShapeForModelAsync(Shape_Model_abstract model, int nAngular, Type generatorType = null, bool persistsAcrossScenes = false)
    {
        generatorType = generatorType ?? typeof(ShapeGenerator_Polygon);

        // 既に記録されている場合はそこから取得.
        if (ModelShapeRecordDict.TryGetValue(model, out ShapeRecordInfo recordInfo))
        {
            if (recordInfo.RecordedShapeComp != null &&
                recordInfo.NAngular == nAngular &&
                recordInfo.GeneratorType == generatorType)
            {
                return recordInfo.RecordedShapeComp;
            }
        }

        // ShapeStatusを取得または生成.
        await GetOrCreateShapeStatusAsync(generatorType, nAngular);
        ShapeCompStatus compStatus = GetShapeCompStatus(nAngular);

        // 記録情報を更新.
        if (recordInfo == null)
        {
            recordInfo = new ShapeRecordInfo(generatorType, nAngular, persistsAcrossScenes);
            ModelShapeRecordDict[model] = recordInfo;
        }
        recordInfo.UpdateRecord(generatorType, nAngular, persistsAcrossScenes, compStatus);

        return compStatus;
    }

    /// <summary>
    /// モデルのShape記録情報を取得する処理.
    /// </summary>
    /// <param name="model">対象のShape_Model_abstract.</param>
    /// <returns>ShapeRecordInfo (存在しない場合はnull).</returns>
    public ShapeRecordInfo GetModelShapeRecord(Shape_Model_abstract model)
    {
        if (ModelShapeRecordDict.TryGetValue(model, out ShapeRecordInfo recordInfo))
        {
            return recordInfo;
        }
        return null;
    }

    /// <summary>
    /// モデルのShape記録を削除する処理.
    /// scene遷移時にpersistsAcrossScenes=falseのモデルを削除する際に使用.
    /// </summary>
    /// <param name="model">対象のShape_Model_abstract.</param>
    public void RemoveModelShapeRecord(Shape_Model_abstract model)
    {
        if (ModelShapeRecordDict.ContainsKey(model))
        {
            ModelShapeRecordDict.Remove(model);
        }
    }

    /// <summary>
    /// scene遷移時にpersistsAcrossScenes=falseの記録を削除する処理.
    /// </summary>
    public void CleanupNonPersistentRecords()
    {
        List<Shape_Model_abstract> toRemove = new List<Shape_Model_abstract>();
        foreach (var kvp in ModelShapeRecordDict)
        {
            if (!kvp.Value.PersistsAcrossScenes)
            {
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var model in toRemove)
        {
            ModelShapeRecordDict.Remove(model);
        }
    }
}
