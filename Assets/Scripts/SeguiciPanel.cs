using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class SeguiciPanel : MonoBehaviour
{
    public Button invioButton;
    public Button cancellaIscrizioneButton;
    public TMP_InputField emailInputField;
    public TMP_InputField citt‡InputField;
    public GameObject emailNonValidaMessage;
    public GameObject problemaFirewallMessage;
    public GameObject nessunaConnessioneMessage;
    public GameObject emailIscrizioneEffettuataMessage;
    public GameObject cancellazioneEffettuataMessage;
    public GameObject giaIscrittoMessage;
    public TMP_Text giaIscrittoTextUI;
    public bool iscritto;

    private string encodedEmail;
    private string encodedCity;

    // evita invii ripetuti in una singola sessione
    private bool attemptedServerSync = false;

    private void Awake()
    {
        invioButton.onClick.AddListener(OnInvioButtonClicked);
        cancellaIscrizioneButton.onClick.AddListener(CancellaIscrizione);
        emailNonValidaMessage.SetActive(false);

        nessunaConnessioneMessage.SetActive(false);
        problemaFirewallMessage.SetActive(false);
        emailIscrizioneEffettuataMessage.SetActive(false);
        cancellazioneEffettuataMessage.SetActive(false);
        giaIscrittoMessage.SetActive(false);
    }

    private void OnEnable()
    {
        iscritto = PlayerPrefs.HasKey("IscrizioneNewsletter");
        CheckIscrizione();

        // Se l'utente risulta gi‡ iscritto localmente, tentiamo silenziosamente la sincronizzazione col DB
        if (iscritto && !attemptedServerSync)
        {
            AttemptServerSubscribeFromPrefs();
        }
    }

    public void CheckIscrizione(bool showUnSubscrive= true,bool showGiaIscritto= true)
    {
        if (iscritto)
        {
            emailInputField.gameObject.SetActive(false);
            citt‡InputField.gameObject.SetActive(false);
            emailInputField.text = "";
            citt‡InputField.text = "";
            invioButton.gameObject.SetActive(false);
            if (showGiaIscritto)
            {
                giaIscrittoMessage.SetActive(true);
                giaIscrittoTextUI.text="SEI GI¿ ISCRITTO CON L'EMAIL: " + UnityWebRequest.UnEscapeURL(PlayerPrefs.GetString("IscrizioneNewsletter", ""));
            }
            if(showUnSubscrive)
            cancellaIscrizioneButton.gameObject.SetActive(true);
        }
        else
        {
            emailInputField.gameObject.SetActive(true);
            citt‡InputField.gameObject.SetActive(true);
            invioButton.gameObject.SetActive(true);
            giaIscrittoMessage.SetActive(false);
            cancellaIscrizioneButton.gameObject.SetActive(false);
        }
    }

    void OnInvioButtonClicked()
    {
        // nascondi messaggi precedenti
        emailNonValidaMessage.SetActive(false);
        nessunaConnessioneMessage.SetActive(false);
        problemaFirewallMessage.SetActive(false);
        emailIscrizioneEffettuataMessage.SetActive(false);

        string email = emailInputField.text?.Trim() ?? "";
        string citt‡ = citt‡InputField.text?.Trim() ?? "";

        // semplice validazione minima email
        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
        {
            emailNonValidaMessage.SetActive(true);
            Debug.LogError("Email non valida");
            return;
        }

        // validazione minima citt‡
        // consente lettere Unicode, spazi, apostrofi, trattini e punti, lunghezza 1-20
        var cityPattern = @"^[\p{L}\s'\-\.]{1,20}$";
        if (string.IsNullOrEmpty(citt‡) || !Regex.IsMatch(citt‡, cityPattern))
        {
            Debug.LogError("Citt‡ non valida");
            return;
        }

        // verifica connettivit‡ di rete
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            nessunaConnessioneMessage.SetActive(true);
            Debug.LogError("Nessuna connessione di rete disponibile");
            return;
        }

        encodedEmail = UnityWebRequest.EscapeURL(email);
        encodedCity = UnityWebRequest.EscapeURL(citt‡);
        // invia sia email che citt‡ al server (server deve gestire il parametro city)
        string url = "https://wilez.it/KeemarApp/getEmails.php?subject=" + encodedEmail + "&city=" + encodedCity;

        // disabilita bottone per evitare doppie richieste
        invioButton.gameObject.SetActive(false);
        emailInputField.gameObject.SetActive(false);
        citt‡InputField.gameObject.SetActive(false);
        StartCoroutine(GetRequest(url));
    }

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // timeout in secondi
            webRequest.timeout = 15;

            yield return webRequest.SendWebRequest();

            // riabilita bottone
            invioButton.gameObject.SetActive(true);

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    ProblemaFirewallMessage();
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error + " (code " + webRequest.responseCode + ")");
                    break;
                case UnityWebRequest.Result.Success:
                    IscrizioneEffettuata(encodedEmail);
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }
        }
    }

    // versione "silenziosa" per sincronizzare con il server senza modificare UI
    IEnumerator SilentGetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            webRequest.timeout = 15;
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[SeguiciPanel] Silent subscribe success: {webRequest.downloadHandler.text}");
            }
            else
            {
                Debug.LogWarning($"[SeguiciPanel] Silent subscribe failed ({pages[page]}): {webRequest.error}");
                // non mostriamo messaggi all'utente: log sufficiente
            }
        }
    }

    // tenta l'iscrizione lato server usando l'email salvata in PlayerPrefs (se presente)
    void AttemptServerSubscribeFromPrefs()
    {
        attemptedServerSync = true;

        string stored = PlayerPrefs.GetString("IscrizioneNewsletter", "");
        if (string.IsNullOrEmpty(stored))
            return;

        // stored potrebbe essere l'email codificata o non; normalizziamo
        string decoded = UnityWebRequest.UnEscapeURL(stored);
        string reEncoded = UnityWebRequest.EscapeURL(decoded);

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("[SeguiciPanel] Skip silent subscribe: no network");
            return;
        }

        string url = "https://wilez.it/KeemarApp/getEmails.php?subject=" + reEncoded;
        StartCoroutine(SilentGetRequest(url));
    }

    public void IscrizioneEffettuata(string encodedEmail)
    {
        PlayerPrefs.SetString("IscrizioneNewsletter", encodedEmail); //Salva l'email codificata come prova di iscrizione
        iscritto = true;
        emailIscrizioneEffettuataMessage.SetActive(true);

       CheckIscrizione(false,false);
    }
    public void ProblemaFirewallMessage()
    {
        emailInputField.gameObject.SetActive(true);
        citt‡InputField.gameObject.SetActive(true);
        invioButton.gameObject.SetActive(true);
        problemaFirewallMessage.SetActive(true);
    }

    // --- nuova funzionalit‡: cancellazione iscrizione ---
    public void CancellaIscrizione()
    {
            // usa PlayerPrefs per ottenere l'email salvata (non usare il campo input)
        string stored = PlayerPrefs.GetString("IscrizioneNewsletter", "");
        if (string.IsNullOrEmpty(stored))
        {
            Debug.LogWarning("[SeguiciPanel] Nessuna email disponibile per cancellazione.");
            return;
        }

        string email = UnityWebRequest.UnEscapeURL(stored).Trim();

        // validazione minima
        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
        {
            Debug.LogWarning("[SeguiciPanel] Email non valida per cancellazione."+ email);
            // rimuovi comunque la chiave locale
            if (PlayerPrefs.HasKey("IscrizioneNewsletter"))
                PlayerPrefs.DeleteKey("IscrizioneNewsletter");

            return;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            nessunaConnessioneMessage.SetActive(true);
            Debug.LogError("Nessuna connessione di rete disponibile");
            return;
        }

        // prepara POST form
        WWWForm form = new WWWForm();
        form.AddField("email", email);

        // disabilita UI
        cancellaIscrizioneButton.gameObject.SetActive(false);
        cancellazioneEffettuataMessage.SetActive(true);

        StartCoroutine(UnsubscribeRequest("https://wilez.it/KeemarApp/unsubscribe.php", form));
    }

    IEnumerator UnsubscribeRequest(string url, WWWForm form)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.timeout = 15;
            yield return www.SendWebRequest();

            cancellaIscrizioneButton.gameObject.SetActive(true);

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[SeguiciPanel] Unsubscribe response: " + www.downloadHandler.text);
                // rimuovi la chiave locale
                if (PlayerPrefs.HasKey("IscrizioneNewsletter"))
                    PlayerPrefs.DeleteKey("IscrizioneNewsletter");

                iscritto = false;
                CheckIscrizione();
                // mostra conferma semplice
                emailIscrizioneEffettuataMessage.SetActive(false);
                giaIscrittoMessage.SetActive(false);
                // puoi riusare un messaggio: usa emailIscrizioneEffettuataMessage o crearne uno per la cancellazione
                Debug.Log("[SeguiciPanel] Iscrizione cancellata localmente.");
            }
            else
            {
                Debug.LogError("[SeguiciPanel] Unsubscribe failed: " + www.error);
                // mostra messaggio firewall / errore
                ProblemaFirewallMessage();
            }
        }
    }
}
