using Cysharp.Threading.Tasks;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public enum Audios
    {
        None = -1,
        Walk = 0,
        Scream = 1,
        Music = 2,
        Grab = 3,
        Damage = 4,
    }
    
    [SerializeField] private float volumeSpeed;
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private AudioSource bgAudio;
    [SerializeField] private AudioSource source;

    public void PlayClip(Audios clip)
    {
        AudioUp();
        source.PlayOneShot(clips[(int)clip]);
    }

    public void PlayLoop(Audios clip)
    {
        source.loop = true;
        source.clip = clips[(int)clip];
        source.Play();
    }

    public void StopLoop()
    {
        source.loop = false;
        source.Stop();
    }

    public void ToggleBGPlay(Audios clip = Audios.None)
    {
        if (bgAudio.isPlaying)
        {
            return;
        }
        if ((int)clip >= 0)
        {
            source.clip =  clips[(int)clip];
        }

        bgAudio.Play();
    }

    public UniTask FadeIn(float volume)
    {
        while (bgAudio.volume < volume)
        {
            bgAudio.volume += volumeSpeed * Time.deltaTime;
        }
        return UniTask.CompletedTask;
    }

    public UniTask FadeOut(float volume)
    {
        while (bgAudio.volume > volume)
        {
            bgAudio.volume -= volumeSpeed * Time.deltaTime;
        }
        return UniTask.CompletedTask;
    }

    public void PlayBGClip(Audios clip)
    {
        bgAudio.clip = clips[(int)clip];
        bgAudio.Play();
    }

    public void AudioUp()
    {
        source.volume = 0.75f;
    }

    public void BGAudioUp()
    {
        bgAudio.volume = 0.75f;
    }
    
    public void Mute()
    {
        source.volume = 0;
    }

    public void BGMute()
    {
        bgAudio.volume = 0;
    }

    public bool IsPlaying()
    {
        return source.isPlaying;
    }
}
