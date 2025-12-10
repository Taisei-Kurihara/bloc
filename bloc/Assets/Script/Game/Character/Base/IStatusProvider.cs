// IStatus_base を持つことを強制するインターフェース.
public interface IStatusProvider
{
    /// <summary> ステータスを取得. </summary>
    IStatus_base Status { get; }
}
