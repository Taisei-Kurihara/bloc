using System.Collections.Generic;
using Common;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;



public abstract class Save_A_B_abstract<TSaveData>
    : Singleton_MonoBehaviourBase<Save_A_B_abstract<TSaveData>>
    where TSaveData : SaveData_A_A_interface, new()
{
    public abstract string SaveDataName { get; }
    public virtual TSaveData SaveData { get; set; } = new TSaveData();

    public virtual SaveData_A_A_Config Config { get; set; }

    public virtual async UniTask Save(int addnumber = -1)
    {
        string fileName = addnumber < 0 ? SaveDataName : $"{SaveDataName}_{addnumber}";
        string json = JsonUtility.ToJson(SaveData, true);
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, $"{fileName}.json");
        Debug.Log($"Save: {filePath}");

        await UniTask.Run(() => System.IO.File.WriteAllText(filePath, json));
    }

    // (既)修: 絶対に再読み込みするLoadトTSaveDataがnull でなければ読み込むLoad関数で分けてください.
    // 絶対に再読み込みするLoad関数.
    public virtual async UniTask<SaveData_A_A_interface> LoadForce(int addnumber = -1)
    {
        string fileName = addnumber < 0 ? SaveDataName : $"{SaveDataName}_{addnumber}";
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, $"{fileName}.json");

        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogWarning($"File not found: {filePath}");
            return SaveData;
        }

        string json = await UniTask.Run(() => System.IO.File.ReadAllText(filePath));
        SaveData = JsonUtility.FromJson<TSaveData>(json);
        return SaveData;
    }

    // TSaveDataがnullでなければ読み込むLoad関数.
    public virtual async UniTask<SaveData_A_A_interface> Load(int addnumber = -1)
    {
        if (SaveData != null)
        {
            Debug.Log("SaveData is already loaded. Skipping load.");
            return SaveData;
        }

        return await LoadForce(addnumber);
    }

    // SaveData_A_A_Configを保存する関数.
    public virtual async UniTask SaveConfig()
    {
        string fileName = $"{SaveDataName}_Config";
        string json = JsonUtility.ToJson(Config, true);
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, $"{fileName}.json");
        Debug.Log($"Save Config: {filePath}");

        await UniTask.Run(() => System.IO.File.WriteAllText(filePath, json));
    }

    // SaveData_A_A_Configを読み込む関数.
    public virtual async UniTask<SaveData_A_A_Config> LoadConfig()
    {
        string fileName = $"{SaveDataName}_Config";
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, $"{fileName}.json");

        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogWarning($"Config file not found: {filePath}");
            return Config;
        }

        string json = await UniTask.Run(() => System.IO.File.ReadAllText(filePath));
        Config = JsonUtility.FromJson<SaveData_A_A_Config>(json);
        return Config;
    }
}

public interface SaveData_A_A_interface
{
    public void SetDefaultData();
}


public class SaveData_A_A_Config
{
    SaveData_A_A_Config(int maxdata)
    {
        this.maxDataSlot = maxdata;
        nowSaveDataSlot = 0 << maxdata;
    }
    int maxDataSlot = 3;
    int nowSaveDataSlot = 0 << 3;

    // nowSaveDataSlot のn番目のビットが1かどうかを判定する関数.
    public bool IsSlotActive(int n)
    {
        if (n < 0 || n >= maxDataSlot)
        {
            Debug.LogWarning($"Invalid slot index: {n}. Must be between 0 and {maxDataSlot - 1}.");
            return false;
        }
        return (nowSaveDataSlot & (1 << n)) != 0;
    }
}