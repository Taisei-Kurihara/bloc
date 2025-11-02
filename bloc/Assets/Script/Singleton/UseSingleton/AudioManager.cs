using UnityEngine;
using Common;

public class AudioManager : Singleton_MonoBehaviourBase<AudioManager>
{
    private AudioSource audioSource;

    void Awake()
    {
        // 既に存在するインスタンスがある場合は破棄
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this as AudioManager;
        DontDestroyOnLoad(gameObject);

        // AudioSource を付与
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// 指定された AudioClip を一度だけ再生する
    /// </summary>
    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// 指定された AudioClip をループ再生する
    /// </summary>
    public void PlayLoop(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.Play();
    }

    /// <summary>
    /// 再生を停止する
    /// </summary>
    public void Stop()
    {
        audioSource.Stop();
    }
}

