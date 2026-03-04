using Firebase;
using Firebase.Messaging;
using UnityEngine;
using System.Threading.Tasks; // Aggiungi questo per Task

public class FirebaseMessagingManager : MonoBehaviour
{
    private const string TAG = "FirebaseFCM"; // Tag per i log
    private const string PENDING_URL_KEY = "pending_notification_url";

    void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebaseMessaging();
            }
            else
            {
                Debug.LogError(
                    $"[{TAG}] Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    void InitializeFirebaseMessaging()
    {
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;

        // Richiedi il token di registrazione FCM
        GetFCMToken();
    }

    async void GetFCMToken()
    {
        Task<string> tokenTask = FirebaseMessaging.GetTokenAsync();
        await tokenTask;

        if (tokenTask.IsCompleted)
        {
            string token = tokenTask.Result;
            Debug.LogFormat("[{0}] FCM registration token: {1}", TAG, token);
        }
        else if (tokenTask.IsFaulted)
        {
            Debug.LogErrorFormat("[{0}] Failed to get FCM registration token: {1}", TAG, tokenTask.Exception);
        }
    }

    public void OnTokenReceived(object sender, TokenReceivedEventArgs tokenInfo)
    {
        // tokenInfo.Token disponibile qui se vuoi inviarlo al server
    }

    public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log($"[{TAG}] Received a new message!");
        // leggi data payload
        if (e.Message.Data != null && e.Message.Data.Count > 0)
        {
            foreach (System.Collections.Generic.KeyValuePair<string, string> iter in e.Message.Data)
            {
                Debug.Log($"[{TAG}]   {iter.Key}: {iter.Value}");
            }

            // se c'è l'url nella data payload, gestiscila
            string url = null;
            if (e.Message.Data.ContainsKey("url"))
                url = e.Message.Data["url"];
            else if (e.Message.Data.ContainsKey("link"))
                url = e.Message.Data["link"];

            if (!string.IsNullOrEmpty(url))
            {
                Debug.Log($"[{TAG}] Received URL: {url}");
                if (Application.isFocused)
                {
                    // apri subito se siamo in foreground
                    Application.OpenURL(url);
                }
                else
                {
                    // salva per il prossimo avvio o per essere gestito quando l'utente apre l'app
                    PlayerPrefs.SetString(PENDING_URL_KEY, url);
                    PlayerPrefs.Save();
                    Debug.Log($"[{TAG}] URL salvato in PlayerPrefs per l'apertura successiva");
                }
            }
        }

        if (e.Message.Notification != null)
        {
            Debug.Log($"[{TAG}] Notification Title: {e.Message.Notification.Title}");
            Debug.Log($"[{TAG}] Notification Body: {e.Message.Notification.Body}");
        }
    }

    void OnDestroy()
    {
        FirebaseMessaging.TokenReceived -= OnTokenReceived;
        FirebaseMessaging.MessageReceived -= OnMessageReceived;
    }
}
