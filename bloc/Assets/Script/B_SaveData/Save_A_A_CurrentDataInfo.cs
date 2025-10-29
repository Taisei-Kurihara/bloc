using System.Collections.Generic;

public class Save_A_A_CurrentDataInfo : Save_A_B_abstract<Savedata_A_B_CurrentData>
{
    public override string SaveDataName => "CurrentDataInfo";

    public Savedata_A_B_CurrentData SaveData { get; set; } = new Savedata_A_B_CurrentData();

    public List<SaveDataEntry<Savedata_A_B_CurrentData>> SaveDataEntries { get; private set; } = new();
}

public class Savedata_A_B_CurrentData : SaveData_A_A_interface
{
    public void SetDefaultData()
    {
    }
}

// Save_A_B_abstract型、int、破棄タイミングを収納するクラス.
public class SaveDataEntry<T> where T : SaveData_A_A_interface, new()
{
    public Save_A_B_abstract<T> SaveData { get; set; }
    public int Value { get; set; }
    public DisposeTimingType DisposeTiming { get; set; }
}


// 破棄タイミングの列挙型.
public enum DisposeTimingType
{
    OnSceneUnload,
    OnApplicationQuit,
    Manual
}