using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;



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

    // riferimenti alle coroutine di fade attive (se presenti)
    private Coroutine musicFadeCoroutine;
    private Coroutine soundsFadeCoroutine;

    [Header("Fade")]
    [Tooltip("Velocità del fade in unità di volume al secondo (maggiore = fade più veloce)")]
    public float fadeSpeed = 1f;

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
            if (instance != this)
            {
                Destroy(transform.root.gameObject);
                return;
            }
        }

        if (audioListener == null)
            audioListener = FindFirstObjectByType<AudioListener>();
        

            #endregion


        //Estrapoliamo i gruppi dal Mixer, senza bisogno di impostarli da inspector
        masterGroup = mainMixer.FindMatchingGroups(string.Empty)[0];
        musicGroup = mainMixer.FindMatchingGroups(string.Empty)[2];
        soundsGroup = mainMixer.FindMatchingGroups(string.Empty)[1];


        Initialize();
        
    }


    public void Initialize()
    {
        Button[] butts = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button butt in butts)
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



    #region FadeIN/OUT Musica durante la lettura del testo
    public void FadeOutMusic () //All'apertura del pannello di lettura del testo, faccio partire la Coroutine per diminuire il volume della musica
    {
        // ferma eventuale fade in/out in corso prima di iniziare un nuovo fade
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }
        musicFadeCoroutine = StartCoroutine(FadeOutMusicCoroutine());
    }

    IEnumerator FadeOutMusicCoroutine()
    {
        float startVolume = musicSource.volume;
        float endVolume = startVolume * 0.2F;
        while (musicSource.volume > endVolume)
        {
            // uso MoveTowards con fadeSpeed pubblico
            musicSource.volume = Mathf.MoveTowards(musicSource.volume, endVolume, fadeSpeed * Time.deltaTime);
            yield return null;
        }

        // assicurati di segnare la coroutine come terminata
        musicFadeCoroutine = null;
   
    }

    public void FadeInMusic() //alla chiusura del pannello di lettura del testo, faccio partire la Coroutine per aumentare il volume della musica
    {
        // ferma eventuale fade in/out in corso prima di iniziare un nuovo fade
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }
        musicFadeCoroutine = StartCoroutine(FadeInMusicCoroutine());
    }
    IEnumerator FadeInMusicCoroutine()
    {
        float target = musicValue;
        while (musicSource.volume < target)
        {
            // uso MoveTowards con fadeSpeed pubblico
            musicSource.volume = Mathf.MoveTowards(musicSource.volume, target, fadeSpeed * Time.deltaTime);
            yield return null;
        }

        // segnalo che la coroutine è terminata
        musicFadeCoroutine = null;
    }

    #endregion




    //Metodo per far partire un suono, dato un AudioClip
    public void PlayAudioClip(AudioClip audioClip)
    {
        soundsSource.Stop();
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
        // ferma eventuale fade della musica in corso
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }
        musicSource.Stop();
    }




    //Metodo per stoppare audio con fade (utilie per il parlato delle descrizioni)
    public void StopAudioClipWithFade()
    {
        // ferma eventuale fade dei suoni in corso
        if (soundsFadeCoroutine != null)
        {
            StopCoroutine(soundsFadeCoroutine);
            soundsFadeCoroutine = null;
        }
        soundsFadeCoroutine = StartCoroutine(FadeOutAudioClipCoroutine());
        
    }
    IEnumerator FadeOutAudioClipCoroutine()
    {
        float startVolume = soundsSource.volume;
        while (soundsSource.volume > 0.2F)
        {
            soundsSource.volume = Mathf.MoveTowards(soundsSource.volume, 0.2f, fadeSpeed * Time.deltaTime);
            yield return null;
        }

        soundsSource.Stop();
        soundsSource.volume = startVolume;

        // segnalo termine coroutine
        soundsFadeCoroutine = null;
    }
}
