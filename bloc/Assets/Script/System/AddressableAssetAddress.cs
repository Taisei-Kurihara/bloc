// ==========================
// AddressableAssetAddress.cs
// Addressablesで使用するアセットアドレスのenum定義.
// ==========================

/// <summary>
/// Addressablesでロードするアセットのアドレス定義.
/// </summary>
public enum AddressableAssetAddress
{
    // Player関連.
    Player,

    // UI関連.
    UI_worldPos_Text,
    UI_mono_Button,

    // Attack関連.
    Attack,
}

/// <summary>
/// AddressableAssetAddress の拡張メソッド.
/// </summary>
public static class AddressableAssetAddressExtensions
{
    /// <summary>
    /// enumを文字列アドレスに変換.
    /// </summary>
    public static string ToAddress(this AddressableAssetAddress address)
    {
        return address.ToString();
    }
}
