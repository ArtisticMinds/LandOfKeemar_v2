using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;



public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;
    public Slider SoundsSlider;
    public float soundsValue;
    public float musicValue;
    public Slider MusicSlider;
    public AudioSource musicSource;
    public AudioSource soundsSource;
    public static AudioListener audioListener;

    public AudioMixer mainMixer; //Il mixer 
    AudioMixerGroup masterGroup; //Gruppo per il suono globale
    AudioMixerGroup soundsGroup; //Gruppo per i suoni
    AudioMixerGroup musicGroup; //Gruppo per le musiche


    public List <AudioClip> Tracks = new List<AudioClip>();
    public List<AudioClip> sounds = new List<AudioClip>();

    void Awake()
    {
        

        #region Singleton
     
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.root.gameObject); //Con questa istruzione rendo "permanente" questo GameObject
        }
        else
        {
            Destroy(transform.root.gameObject);
            return;
        }

        if (audioListener == null)
            audioListener = FindObjectOfType<AudioListener>();
        

            #endregion


        //Estrapoliamo i gruppi dal Mixer, senza bisogno di impostarli da inspector
        masterGroup = mainMixer.FindMatchingGroups(string.Empty)[0];
        musicGroup = mainMixer.FindMatchingGroups(string.Empty)[2];
        soundsGroup = mainMixer.FindMatchingGroups(string.Empty)[1];


        Initialize();
        
    }


    public void Initialize()
    {
        foreach (Button butt in FindObjectsOfType<Button>(true))
        {
            EventTrigger triggerDown = GetComponent<EventTrigger>();

        if (!triggerDown)
            triggerDown = butt.gameObject.AddComponent<EventTrigger>();

        var pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        if (butt.name.Contains("Zoom"))
            pointerDown.callback.AddListener((e) => soundsSource.PlayOneShot(sounds[1]));
        else
            pointerDown.callback.AddListener((e) => soundsSource.PlayOneShot(sounds[0]));

        triggerDown.triggers.Add(pointerDown);
        }
    }

    void Start()
    {
        //Aggiungiamo i Listener che "intercettino" un cambio di valore dei tre slaiders
       if(SoundsSlider) SoundsSlider.onValueChanged.AddListener(SetSoundsVolume);
        if (MusicSlider) MusicSlider.onValueChanged.AddListener(SetMusicVolume);


    }

    public void PlayMenuMusic()
    {
        musicSource.Stop();
        musicSource.clip = Tracks[0];
        musicSource.Play();
    }
    //Metodo che si esegue nel momento in cui lo slider SoundsSlider cambia valore
    public void SetSoundsVolume(float value)
    {
        mainMixer.SetFloat("SoundsVolume", Mathf.Log10(value + 0.0001f) * 20);
        soundsValue = value ;
    }


    //Metodo che si esegue nel momento in cui lo slider MusicSlider cambia valore
    public void SetMusicVolume(float value)
    {
        mainMixer.SetFloat("MusicsVolume", Mathf.Log10(value + 0.0001f) * 20);
        musicValue =value ;
    }

    public void PlayAudioClip(AudioClip audioClip)
    {
        soundsSource.PlayOneShot(audioClip);
    }

    public void PlayMusicClip(AudioClip audioClip)
    {
        musicSource.Stop();
        musicSource.clip = audioClip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

}
