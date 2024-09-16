using System;
using UnityEngine;
using Utils;

public enum SoundType
{
    PullWeapon = 0,
    Jump = 1,
    Shoot = 2,
    Hit = 3,
}

[Serializable]
public class Sound
{
    public SoundType Type;
    public AudioClip Clip;
}

public class AudioController : Singleton<AudioController>
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Sound[] _sounds;

    public void PlaySound(SoundType type)
    {
        var sound = Array.Find(_sounds, s => s.Type == type);
        if (sound == null)
        {
            Debug.LogWarning("Sound not found");
            return;
        }
        _audioSource.PlayOneShot(sound.Clip);
    }
}