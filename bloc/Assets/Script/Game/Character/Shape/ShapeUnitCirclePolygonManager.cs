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
}
