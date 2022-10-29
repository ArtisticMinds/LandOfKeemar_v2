
using UnityEngine;

public class PlayClip : MonoBehaviour
{
    public void PlayAudioClip(AudioClip audioClip)
    { 
        if (AudioManager.instance)
        AudioManager.instance.soundsSource.PlayOneShot(audioClip);
    }

    public void PlayMusicClip(AudioClip audioClip)
    {
        if (AudioManager.instance)
            AudioManager.instance.musicSource.PlayOneShot(audioClip);
    }

}
