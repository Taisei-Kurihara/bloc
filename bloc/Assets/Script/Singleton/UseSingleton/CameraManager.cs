using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class CameraManager : Singleton_DestroyAvailableMonoSingleton<CameraManager>
{

    Transform trackingTargetTransform;
    public Transform TrackingTargetTransform
    {
        get => trackingTargetTransform;
        set
        {
            trackingTargetTransform = value;
        }
    }

    // (既)修: 以下のCancellationTokenSourceの変数名をUpdateCameraPositionの専用cancel用に適切な名称に変更してください.
    private CancellationTokenSource updateCameraPositionCts;

    private void Start()
    {
        UpdateCameraPosition().Forget();
    }

    async UniTask UpdateCameraPosition()
    {
        updateCameraPositionCts = new CancellationTokenSource();
        var destroyToken = this.GetCancellationTokenOnDestroy();
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(updateCameraPositionCts.Token, destroyToken);

        if (trackingTargetTransform != null)
        {
            while (true)
            {
                Vector3 cameraPosition = trackingTargetTransform.position;
                cameraPosition.z = Camera.main.transform.position.z; // Maintain the current z position.
                SetUpdateCameraPosition(cameraPosition);

                await UniTask.DelayFrame(1 / 60, cancellationToken: linkedCts.Token);
            }

        }
        else
        {
            linkedCts?.Dispose();
        }
    }

    #region 単発処理

    /// <summary> カメラ追従を停止する. </summary>
    public void StopCameraTracking()
    {
        if (updateCameraPositionCts != null && !updateCameraPositionCts.IsCancellationRequested)
        {
            updateCameraPositionCts.Cancel();
            updateCameraPositionCts.Dispose();
            updateCameraPositionCts = null;
        }
    }

    /// <summary></summary>
    /// <param name="position">new�J�����ʒu</param>
    public void SetUpdateCameraPosition(Vector3 position)
    {
        Camera.main.transform.position = position;
    }

    /// <summary></summary>
    /// <param name="zoom">new�Y�[�����x��</param>
    public void UpdateCameraZoom(float zoom)
    {
        Camera.main.orthographicSize = zoom;
    }

    #endregion
}
