using UnityEngine;

namespace InGame.Character
{
    public interface Camera_FollowTarget_interface
    {
        /// <summary>
        /// カメラのターゲットを設定する
        /// </summary>
        /// <param name="target">カメラのターゲット</param>
        public void SetCameraFollowTarget(UnityEngine.Transform target);
    }
}