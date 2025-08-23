using Cysharp.Threading.Tasks;
using UnityEngine;

public class CameraManager : DestroyAvailableMonoSingleton<CameraManager>
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

    private void Start()
    {
        UpdateUpdateCameraPosition().Forget();
    }

    async UniTask UpdateUpdateCameraPosition()
    {
        var token = this.GetCancellationTokenOnDestroy();
        while (true)
        {
            if (trackingTargetTransform != null)
            {
                Vector3 cameraPosition = trackingTargetTransform.position;
                cameraPosition.z = Camera.main.transform.position.z; // Maintain the current z position
                SetUpdateCameraPosition(cameraPosition);
            }
            await UniTask.DelayFrame(1/60, cancellationToken: token);
        }
    }

    /// <summary> カメラ位置更新</summary>
    /// <param name="position">newカメラ位置</param>
    public void SetUpdateCameraPosition(Vector3 position)
    {
        Camera.main.transform.position = position;
    }

    /// <summary> カメラズーム更新 </summary>
    /// <param name="zoom">newズームレベル</param>
    public void UpdateCameraZoom(float zoom)
    {
        Camera.main.orthographicSize = zoom;
    }

}
