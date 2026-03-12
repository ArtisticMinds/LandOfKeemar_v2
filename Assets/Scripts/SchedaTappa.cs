using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

using UnityEngine.UI;

public class SchedaTappa : MonoBehaviour
{
    public static SchedaTappa instance;
    public TMP_Text titoloUI;
    public TMP_Text descrizioneUI;
    [Header("Pulsanti")]
    public Button playButton;
    public Button playButtonChiusa;
    public Button infosButton;
    public Button googleMapButton;
    public Button videoButton;
    public GameObject tappaChiusaMessage;

    // valore di riferimento: 120s corrisponde a anim.speed == 1
    private const float referenceDuration = 120f;
    private const float minAnimSpeed = 0.1f;
    private const float maxAnimSpeed = 10f;
    private float originalAnimSpeed = 1f;
    private Animator anim;

    private void Awake()
    {
        if (anim == null)
        {
            anim = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
        }
        if (anim != null)
            originalAnimSpeed = anim.speed;

        playButton.onClick.AddListener(CLickOnPlayeGame);
        playButtonChiusa.onClick.AddListener(ShowTappaChiusaMessage); 

        tappaChiusaMessage.SetActive(false);
    }

    public void SetReal()
    {
        titoloUI.text = TappaMapMarker.openTappa.nomeReale;
        descrizioneUI.text = TappaMapMarker.openTappa.storiaReale;

        if (TappaMapMarker.openTappa.descrizioneAudioReal != null)
        {
            AudioManager.instance.FadeOutMusic();
            PlayDescrizioneAudio(TappaMapMarker.openTappa.descrizioneAudioReal);
        }

        ImageSlideshow.Instance.SetCollectionAndPlay();
    }



    public void SetRomanzi()
    {
        titoloUI.text = TappaMapMarker.openTappa.nomeNeiRomanzi;
        descrizioneUI.text = TappaMapMarker.openTappa.storiaRomanzi;

        if (TappaMapMarker.openTappa.descrizioneAudioRomanzi != null)
        {
            AudioManager.instance.FadeOutMusic();
            PlayDescrizioneAudio(TappaMapMarker.openTappa.descrizioneAudioRomanzi);
        }
    }


    public void PlayDescrizioneAudio(AudioClip audioDescrizione)
    {
        if (audioDescrizione == null)
            return;

        AudioManager.instance.PlayAudioClip(audioDescrizione);

        // Sincronizza lo scorrimento del testo con la durata dell'audio
        float durata = audioDescrizione.length; // durata in secondi
        if (durata <= 0f || anim == null)
            return;

        // se referenceDuration (120s) corrisponde a anim.speed == 1:
        float calculatedSpeed = referenceDuration / durata;

        // clamp per sicurezza
        calculatedSpeed = Mathf.Clamp(calculatedSpeed, minAnimSpeed, maxAnimSpeed);

        anim.speed = calculatedSpeed;
    }

    public void GotVideoURL()
    {
        Application.OpenURL(TappaMapMarker.openTappa.videoLink);
    }

    public void GotMapURL()
    {
        Application.OpenURL(TappaMapMarker.openTappa.googleMapLink);
    }

    public void ShowTappaChiusaMessage()
    {
        tappaChiusaMessage.SetActive(true);
    }

    public void CLickOnPlayeGame()
    {
        MainMenu.instance.CloseMainMenu();
        SceneLoader.instance.LoadTappaScene(TappaMapMarker.openTappa);
        Debug.Log("PLAY TAPPA: "+TappaMapMarker.openTappa.tappaName);
    }

    private void OnDisable()
    {
        AudioManager.instance.StopAudioClipWithFade();
        AudioManager.instance.FadeInMusic();

        // ripristina speed originale
        if (anim != null)
            anim.speed = originalAnimSpeed;
    }
}
