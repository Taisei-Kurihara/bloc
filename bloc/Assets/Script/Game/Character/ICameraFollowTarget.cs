using UnityEngine;

namespace InGame.Character
{
    public interface ICameraFollowTarget
    {
        /// <summary>
        /// カメラのターゲットを設定する
        /// </summary>
        /// <param name="target">カメラのターゲット</param>
        public void SetCameraFollowTarget(UnityEngine.Transform target);
    }
}