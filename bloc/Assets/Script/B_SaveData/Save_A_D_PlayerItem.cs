using System.Collections.Generic;
using UnityEngine;

// -----------------------------
// SaveData構造（セーブ内容を定義）
// -----------------------------
[System.Serializable]
public class SaveData_PlayerItem : SaveData_A_A_interface
{
    public void SetDefaultData()
    {
    }
}

// -----------------------------
// セーブクラス本体
// -----------------------------
public class Save_A_D_PlayerItem : Save_A_B_abstract<SaveData_PlayerItem>
{
    // ファイル名（拡張子除く）
    public override string SaveDataName => "PlayerItem";

    // 実際にセーブするデータ
    public override SaveData_PlayerItem SaveData { get; set; }



}
