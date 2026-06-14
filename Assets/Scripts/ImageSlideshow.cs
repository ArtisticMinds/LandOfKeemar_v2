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

    [Header("Mask / Container")]
    [Tooltip("RectTransform che contiene imageA/imageB e ha la Mask. Se nullo verrà usato il parent di imageA.")]
    public RectTransform maskRect;

    [Header("Sprites (Caricato da funzione)")]
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

    // Salvo le scale originali per poterle ripristinare / moltiplicare
    Vector3 originalScaleImageA = Vector3.one;
    Vector3 originalScaleImageB = Vector3.one;
    Vector3 originalScaleMask = Vector3.one;

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

        // se non è assegnata la mask, prova a usare il parent di imageA
        if (maskRect == null && imageA != null)
        {
            var parent = imageA.transform.parent as RectTransform;
            if (parent != null)
                maskRect = parent;
        }

        // inizializza al trasparente / visibile
        if (imageA) SetAlpha(imageA, 1f);
        if (imageB) SetAlpha(imageB, 0f);

        // salva scale originali (se presenti)
        if (imageA) originalScaleImageA = imageA.transform.localScale;
        if (imageB) originalScaleImageB = imageB.transform.localScale;
        if (maskRect) originalScaleMask = maskRect.localScale;
    }

    void Start()
    {
        if (playOnAwake)
            Play();
    }


    public void SetCollectionAndPlay(SpriteCollection _spriteCollection)
    {
        if (TappaMapMarker.openTappa != null)
        {
            spriteCollection = _spriteCollection;

            if (spriteCollection != null)
            {
                imageA.gameObject.SetActive(true); // mostra imageA se non abbiamo sprite da mostrare
                imageB.gameObject.SetActive(true); // mostra imageB se non abbiamo sprite da mostrare
                // quando cambio collection, parto dal primo elemento
                index = 0;
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

    // restituisce scala per indice (null-safe)
    float GetScaleForIndex(int idx)
    {
        if (spriteCollection == null || spriteCollection.spriteScale == null) return 1f;
        if (idx < 0 || idx >= spriteCollection.spriteScale.Count) return 1f;
        return spriteCollection.spriteScale[idx];
    }

    // applica la scala al contenitore/mask (preferito) o alla singola Image come fallback
    void ApplySpriteScale(Image img, int idx)
    {
        float factor = GetScaleForIndex(idx);

        if (maskRect != null)
        {
            maskRect.localScale = originalScaleMask * factor;
            return;
        }

        // fallback: scala l'immagine stessa (compatibilità)
        if (img == null) return;
        if (img == imageA)
            img.transform.localScale = originalScaleImageA * factor;
        else if (img == imageB)
            img.transform.localScale = originalScaleImageB * factor;
        else
            img.transform.localScale = Vector3.one * factor;
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

        // applica scala corretta alla prima immagine (scala alla mask)
        ApplySpriteScale(imageA, index);
        // reset imageB alla scala originale (sarà aggiornata in TransitionTo se necessario)
        if (imageB) imageB.transform.localScale = originalScaleImageB;

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

        // applica scala per il prossimo indice (scala la mask, così le immagini non vengono tagliate)
        ApplySpriteScale(imageB, nextIndex);

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
        // assicuriamoci che la mask abbia la scala corretta per l'indice appena impostato
        ApplySpriteScale(imageA, nextIndex);

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
        // ripristina scale originali della mask o delle immagini
        if (maskRect) maskRect.localScale = originalScaleMask;
        if (imageA) imageA.transform.localScale = originalScaleImageA;
        if (imageB) imageB.transform.localScale = originalScaleImageB;
        imageA.gameObject.SetActive(false); // nascondi imageA 
        imageB.gameObject.SetActive(false); // nascondi imageB 
    }


    private void OnDisable()
    {
        Clear();
    }
}