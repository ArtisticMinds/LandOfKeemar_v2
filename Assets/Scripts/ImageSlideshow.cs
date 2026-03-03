using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class ImageSlideshow : MonoBehaviour
{
    public static ImageSlideshow Instance { get; private set; } 
    [Header("UI Images (overlayed)")]
    public Image imageA; // immagine visibile corrente
    public Image imageB; // immagine che viene sfumata sopra

    [Header("Sprites (use ScriptableObject)")]
    public SpriteCollection spriteCollection;

    [Header("Timing")]
    [Tooltip("Tempo in secondi che ogni immagine resta visibile (escluse transizioni)")]
    public float displayDuration = 3f;
    [Tooltip("Durata in secondi della transizione fade")]
    public float fadeDuration = 0.6f;

    [Header("Behaviour")]
    public bool playOnAwake = true;
    public bool loop = true;

    int index = 0;
    bool running = false;
    Coroutine slideshowCoroutine;

    void Awake()
    {
        Instance=this; // singleton access

        // sicurezza: se manca una Image, prova a recuperarla
        if (imageA == null || imageB == null)
        {
            var imgs = GetComponentsInChildren<Image>();
            if (imgs.Length >= 2)
            {
                if (imageA == null) imageA = imgs[0];
                if (imageB == null) imageB = imgs[1];
            }
        }

        // inizializza al trasparente / visibile
        if (imageA) SetAlpha(imageA, 1f);
        if (imageB) SetAlpha(imageB, 0f);
    }

    void Start()
    {
        if (playOnAwake)
            Play();
    }


    public void SetCollectionAndPlay()
    {
        if (TappaMapMarker.openTappa != null)
        {
            spriteCollection = TappaMapMarker.openTappa.slideSpriteCollection;

            if (spriteCollection != null)
            {
                imageA.gameObject.SetActive(true); // mostra imageA se non abbiamo sprite da mostrare
                imageB.gameObject.SetActive(true); // mostra imageB se non abbiamo sprite da mostrare
                Play();
                Debug.Log("Play: " + spriteCollection);
            }
            else
            {
                Clear();
                Debug.Log("spriteCollection è null, non posso settare la SpriteCollection");
            }
        }
        else
        {
            Clear();
            Debug.Log("TappaMapMarker.openTappa è null, non posso settare la SpriteCollection");
        }
    }

    // Ottiene la lista di sprite dalla collection (null-safe)
    List<Sprite> GetSprites()
    {
        return spriteCollection != null ? spriteCollection.sprites : null;
    }

    // API: avvia lo slideshow
    public void Play()
    {
        var sprites = GetSprites();
        if (running) return;
        if (sprites == null || sprites.Count == 0) return;

        running = true;
        // inizializza immagini con primo sprite
        index = Mathf.Clamp(index, 0, sprites.Count - 1);
        imageA.sprite = sprites[index];
        SetAlpha(imageA, 1f);
        SetAlpha(imageB, 0f);

        slideshowCoroutine = StartCoroutine(SlideshowLoop());
    }

    // API: ferma lo slideshow
    public void Stop()
    {
        if (!running) return;
        running = false;
        if (slideshowCoroutine != null) StopCoroutine(slideshowCoroutine);
        slideshowCoroutine = null;
    }



    // Vai all'immagine successiva (transizione immediata)
    public void Next()
    {
        var sprites = GetSprites();
        if (sprites == null || sprites.Count == 0) return;
        int nextIndex = (index + 1) % sprites.Count;
        if (!loop && nextIndex <= index) return;
        if (slideshowCoroutine != null) StopCoroutine(slideshowCoroutine);
        StartCoroutine(TransitionTo(nextIndex));
    }

    // Vai all'immagine precedente
    public void Previous()
    {
        var sprites = GetSprites();
        if (sprites == null || sprites.Count == 0) return;
        int prevIndex = index - 1;
        if (prevIndex < 0)
        {
            if (!loop) return;
            prevIndex = sprites.Count - 1;
        }
        if (slideshowCoroutine != null) StopCoroutine(slideshowCoroutine);
        StartCoroutine(TransitionTo(prevIndex));
    }

    IEnumerator SlideshowLoop()
    {
        var sprites = GetSprites();
        while (running)
        {
            yield return new WaitForSeconds(displayDuration);
            sprites = GetSprites();
            if (sprites == null || sprites.Count == 0) break;

            int nextIndex = index + 1;
            if (nextIndex >= sprites.Count)
            {
                if (loop) nextIndex = 0;
                else break;
            }
            yield return StartCoroutine(TransitionTo(nextIndex));
        }
        running = false;
    }

    // Transition: carica sprite su imageB e cross-fade
    IEnumerator TransitionTo(int nextIndex)
    {
        var sprites = GetSprites();
        if (imageA == null || imageB == null || sprites == null || sprites.Count == 0) yield break;

        // prepara imageB come sopra (manteniamo dimensioni correnti -> non chiamare SetNativeSize se causa salti)
        imageB.sprite = sprites[nextIndex];

        // porta imageB sopra per la transizione
        imageB.transform.SetAsLastSibling();

        float t = 0f;
        // assicuriamoci che imageA parta da alpha 1 e imageB da 0
        SetAlpha(imageA, 1f);
        SetAlpha(imageB, 0f);

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / fadeDuration);
            SetAlpha(imageB, alpha);
            SetAlpha(imageA, 1f - alpha);
            yield return null;
        }

        // termine: imageB piena
        SetAlpha(imageB, 1f);
        SetAlpha(imageA, 0f);

        // copia il nuovo sprite su imageA (manteniamo imageA come immagine "principale")
        imageA.sprite = imageB.sprite;

        // assicuriamo l'ordine: imageA sotto e imageB sopra (imageB rimane pronta per la prossima transizione)
        imageA.transform.SetSiblingIndex(0);
        imageB.transform.SetAsLastSibling();

        index = nextIndex;
    }

    // utility per settare alpha dell'Image
    void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        var c = img.color;
        c.a = a;
        img.color = c;
    }

    public void Clear()
    {
        Stop();
        if (imageA) imageA.sprite = null;
        if (imageB) imageB.sprite = null;
        imageA.gameObject.SetActive(false); // nascondi imageA 
        imageB.gameObject.SetActive(false); // nascondi imageB 
    }


    private void OnDisable()
    {
        Clear();
    }
}